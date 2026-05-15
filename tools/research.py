#!/usr/bin/env python3
"""
Research Tool for Claude Code — Multi-source web research CLI
No API keys. Runs locally against free endpoints (Jina, YouTube, GitHub, DuckDuckGo).
"""

import sys
import json
import os
from datetime import datetime
from pathlib import Path
import requests
import subprocess
from urllib.parse import quote, urlencode
from bs4 import BeautifulSoup
import re

# Report output directory
REPORT_DIR = Path(__file__).parent / "reports"
REPORT_DIR.mkdir(exist_ok=True)

# Research categories with query augmentation
CATEGORIES = {
    "unity": ["site:docs.unity3d.com", "site:unity.com/blog", "Unity"],
    "game-design": ["game design", "GDC", "mechanics", "player experience"],
    "comparable": ["Valheim", "Genshin Impact", "MMBN", "solarpunk"],
    "shaders": ["URP shader", "shader graph", "HLSL"],
    "multiplayer": ["Unity Netcode", "NGO", "authority", "state sync"],
    "procgen": ["procedural generation", "noise", "biome"],
    "performance": ["profiling", "optimization", "GPU", "CPU"],
    "animation": ["animator", "blend tree", "IK", "root motion"],
    "architecture": ["event bus", "reactive", "pattern", "observer"],
}

# HTTP headers to avoid bot detection
HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
}

TIMEOUT = 10


class ResearchError(Exception):
    """Research operation error."""
    pass


# ============================================================================
# SEARCH BACKENDS
# ============================================================================

def search_jina(query: str, num_results: int = 5) -> list[dict]:
    """
    Search using Jina Search API (free, no key).
    Returns: [{"title": str, "url": str, "content": str}, ...]
    """
    try:
        url = f"https://s.jina.ai/{quote(query)}"
        resp = requests.get(url, headers=HEADERS, timeout=TIMEOUT)
        resp.raise_for_status()

        # Jina returns markdown — parse it for structure
        lines = resp.text.split('\n')
        results = []
        current = {}

        for line in lines:
            if line.startswith("## "):
                if current:
                    results.append(current)
                current = {"title": line.replace("## ", "").strip(), "content": ""}
            elif line.startswith("["):
                # Extract URL from markdown link [title](url)
                match = re.search(r'\]\((.*?)\)', line)
                if match:
                    current["url"] = match.group(1)
            elif current:
                current["content"] += line + "\n"

        if current:
            results.append(current)

        return results[:num_results]
    except Exception as e:
        raise ResearchError(f"Jina search failed: {e}")


def search_duckduckgo(query: str, num_results: int = 5) -> list[dict]:
    """
    Fallback: scrape DuckDuckGo results.
    Returns: [{"title": str, "url": str, "content": str}, ...]
    """
    try:
        url = "https://html.duckduckgo.com/"
        params = {"q": query}
        resp = requests.get(url, params=params, headers=HEADERS, timeout=TIMEOUT)
        resp.raise_for_status()

        soup = BeautifulSoup(resp.content, 'html.parser')
        results = []

        for result in soup.find_all('div', class_='result'):
            link_elem = result.find('a', class_='result__url')
            title_elem = result.find('a', class_='result__link')
            snippet_elem = result.find('a', class_='result__snippet')

            if link_elem and title_elem:
                results.append({
                    "title": title_elem.get_text(strip=True),
                    "url": link_elem.get_text(strip=True),
                    "content": snippet_elem.get_text(strip=True) if snippet_elem else ""
                })

        return results[:num_results]
    except Exception as e:
        raise ResearchError(f"DuckDuckGo fallback failed: {e}")


def scrape_jina(url: str) -> str:
    """
    Extract URL content using Jina Reader (free, no key).
    Returns: clean markdown text
    """
    try:
        reader_url = f"https://r.jina.ai/{url}"
        resp = requests.get(reader_url, headers=HEADERS, timeout=TIMEOUT)
        resp.raise_for_status()
        return resp.text
    except Exception as e:
        raise ResearchError(f"Jina scrape failed: {e}")


def scrape_beautifulsoup(url: str) -> str:
    """
    Fallback: scrape using Beautiful Soup.
    Returns: plain text content
    """
    try:
        resp = requests.get(url, headers=HEADERS, timeout=TIMEOUT)
        resp.raise_for_status()
        soup = BeautifulSoup(resp.content, 'html.parser')

        # Remove boilerplate
        for tag in soup(['script', 'style', 'nav', 'footer', 'aside']):
            tag.decompose()

        return soup.get_text(separator='\n', strip=True)
    except Exception as e:
        raise ResearchError(f"BeautifulSoup scrape failed: {e}")


# ============================================================================
# YOUTUBE
# ============================================================================

def get_youtube_transcript(url: str, max_chars: int = None) -> dict:
    """
    Get YouTube transcript from URL.
    Returns: {"transcript": str, "title": str, "duration": str}
    """
    video_id = None
    if "youtube.com/watch?v=" in url:
        video_id = url.split("v=")[1].split("&")[0]
    elif "youtu.be/" in url:
        video_id = url.split("youtu.be/")[1].split("?")[0]

    if not video_id:
        raise ResearchError(f"Could not extract video ID from {url}")

    try:
        from youtube_transcript_api import YouTubeTranscriptApi
        transcript = YouTubeTranscriptApi.get_transcript(video_id)

        # Convert to timestamped text
        text_lines = [f"[{int(item['start'])}s] {item['text']}" for item in transcript]
        full_text = "\n".join(text_lines)

        if max_chars:
            full_text = full_text[:max_chars]

        return {
            "transcript": full_text,
            "video_id": video_id,
            "source": "YouTube Transcript API"
        }
    except Exception as e:
        # Fallback to yt-dlp
        try:
            result = subprocess.run(
                ["yt-dlp", "--write-auto-sub", "--sub-format=vtt", "--quiet", "--output", "-", url],
                capture_output=True,
                text=True,
                timeout=30
            )
            if result.stdout:
                return {
                    "transcript": result.stdout[:max_chars] if max_chars else result.stdout,
                    "video_id": video_id,
                    "source": "yt-dlp (auto-generated)"
                }
        except:
            pass

        raise ResearchError(f"Could not fetch YouTube transcript: {e}")


# ============================================================================
# GITHUB
# ============================================================================

def search_github(query: str, language: str = None, min_stars: int = 0) -> list[dict]:
    """
    Search GitHub code (public API, no auth).
    Returns: [{"repo": str, "file": str, "snippet": str, "url": str}, ...]
    """
    try:
        params = {"q": f"{query} language:{language}" if language else query}
        resp = requests.get(
            "https://api.github.com/search/code",
            params=params,
            headers=HEADERS,
            timeout=TIMEOUT
        )
        resp.raise_for_status()

        results = []
        for item in resp.json().get("items", [])[:5]:
            results.append({
                "repo": item.get("repository", {}).get("full_name", "unknown"),
                "file": item.get("path", "unknown"),
                "snippet": item.get("text_matches", [{}])[0].get("fragment", "")[:500],
                "url": item.get("html_url", ""),
                "language": item.get("language", "")
            })

        return results
    except Exception as e:
        raise ResearchError(f"GitHub search failed: {e}")


# ============================================================================
# LIBRARY INDEX MANAGEMENT
# ============================================================================

def load_index() -> dict:
    """Load research index from JSON. Returns empty structure if file missing."""
    try:
        index_path = Path(__file__).parent / "research-index.json"
        if index_path.exists():
            return json.loads(index_path.read_text())
        return {"version": 1, "updated": "", "entries": [], "query_log": []}
    except Exception as e:
        raise ResearchError(f"Failed to load index: {e}")


def save_index(index: dict):
    """Write index atomically."""
    try:
        index_path = Path(__file__).parent / "research-index.json"
        index["updated"] = datetime.now().isoformat()
        index_path.write_text(json.dumps(index, indent=2))
    except Exception as e:
        raise ResearchError(f"Failed to save index: {e}")


def index_search(query: str, index: dict) -> list[dict]:
    """
    Tokenize query, score each entry by keyword hits in title+topics+summary.
    Returns sorted list with score attached.
    """
    query_tokens = set(query.lower().split())
    results = []

    for entry in index.get("entries", []):
        score = 0
        searchable = (
            entry.get("title", "").lower() + " " +
            " ".join(entry.get("topics", [])).lower() + " " +
            entry.get("summary", "").lower() + " " +
            entry.get("query", "").lower()
        )

        for token in query_tokens:
            if token in searchable:
                score += searchable.count(token)

        if score > 0:
            results.append({**entry, "score": score})

    return sorted(results, key=lambda x: x["score"], reverse=True)


def save_to_library(topic: str, content: str, category: str, sources: list, query: str, depth: int) -> str:
    """
    Save research findings to Assets/_Docs/Research/.
    Auto-extract topics from content keywords.
    Update and save index.
    Returns file path.
    """
    slug = slugify(topic)
    timestamp = datetime.now().strftime('%Y%m%d')
    filename = f"{slug}-{timestamp}.md"

    docs_dir = Path(__file__).parent.parent / "Assets" / "_Docs" / "Research"
    docs_dir.mkdir(parents=True, exist_ok=True)
    filepath = docs_dir / filename

    # Auto-extract topics (top keywords from content)
    words = re.findall(r'\b[a-z]{3,}\b', content.lower())
    word_freq = {}
    for word in words:
        word_freq[word] = word_freq.get(word, 0) + 1
    topics = [w for w, _ in sorted(word_freq.items(), key=lambda x: -x[1])[:8]]

    # Build doc — content is the raw research, not formatted report
    doc = f"""# Research: {topic}

*Date: {datetime.now().strftime('%Y-%m-%d')} | Category: {category} | Depth: {depth}*
*Sources: {', '.join(sources)}*
*Query: "{query}"*

---

{content}

## Citations

Sources listed above.
"""

    filepath.write_text(doc)

    # Update index
    index = load_index()
    index["entries"].append({
        "file": f"Assets/_Docs/Research/{filename}",
        "title": f"Research: {topic}",
        "date": datetime.now().strftime('%Y-%m-%d'),
        "category": category,
        "topics": topics,
        "summary": content[:200] + ("..." if len(content) > 200 else ""),
        "sources": sources,
        "query": query
    })
    index["query_log"].append({
        "query": query,
        "date": datetime.now().isoformat(),
        "category": category,
        "result_file": f"Assets/_Docs/Research/{filename}"
    })
    save_index(index)

    return str(filepath)


# ============================================================================
# REPORT FORMATTING
# ============================================================================

def slugify(text: str) -> str:
    """Convert text to filename-safe slug."""
    slug = re.sub(r'\W+', '-', text.lower())
    return slug.strip('-')[:50]


def format_report(query: str, results: dict, category: str = None, depth: int = 3) -> str:
    """Format research results as markdown."""
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    report = f"""# Research: {query}
**Date:** {timestamp} | **Category:** {category or 'general'} | **Depth:** {depth}

## Summary

*Awaiting Claude synthesis*

## Sources

"""

    # Web results
    if "web" in results and results["web"]:
        for i, source in enumerate(results["web"], 1):
            report += f"""
### {i}. {source.get('title', 'Untitled')}
**URL:** {source.get('url', 'unknown')}

> {source.get('content', '')[:500]}

---

"""

    # YouTube
    if "youtube" in results and results["youtube"]:
        for item in results["youtube"]:
            report += f"""
### YouTube: {item.get('title', 'Video Transcript')}
**Source:** {item.get('source', 'unknown')}

{item.get('transcript', '')[:2000]}

---

"""

    # GitHub
    if "github" in results and results["github"]:
        report += "\n## Code Examples\n\n"
        for item in results["github"]:
            report += f"""
### {item.get('repo', 'unknown')} / {item.get('file', 'unknown')}
**Language:** {item.get('language', 'unknown')}

```
{item.get('snippet', '')}
```

**Link:** {item.get('url', 'unknown')}

---

"""

    report += "\n## Citations\n*Sources listed above.*"
    return report


# ============================================================================
# MAIN COMMANDS
# ============================================================================

def cmd_search(query: str, sources: int = 5, depth: int = 3, category: str = None):
    """Search command."""
    # Check library first
    index = load_index()
    matches = index_search(query, index)
    if matches:
        print(f"[LIBRARY] Found {len(matches)} related doc(s):")
        for m in matches[:3]:
            print(f"  [score:{m['score']}] {m['title']}  ->  {m['file']}")
        print()

    # Augment query with category bias
    augmented_query = query
    if category and category in CATEGORIES:
        augmented_query += " " + " ".join(CATEGORIES[category])

    try:
        print(f"Searching: {augmented_query}")
        web_results = search_jina(augmented_query, num_results=sources)
    except:
        print("Jina failed, trying DuckDuckGo...")
        web_results = search_duckduckgo(augmented_query, num_results=sources)

    results = {"web": web_results}
    report = format_report(augmented_query, results, category, depth)

    # Save to library instead of reports
    sources_list = [s.get("url", "unknown") for s in web_results]
    save_to_library(query, report, category or "general", sources_list, query, depth)

    # Also print to stdout
    print(f"\n[OK] Research saved to library\n")
    print(report)


def cmd_scrape(url: str, format: str = "md"):
    """Scrape command."""
    print(f"Scraping: {url}")
    try:
        content = scrape_jina(url)
    except:
        print("Jina failed, trying BeautifulSoup...")
        content = scrape_beautifulsoup(url)

    if format == "json":
        output = json.dumps({"url": url, "content": content}, indent=2)
    else:
        output = f"# {url}\n\n{content}"

    print(output)


def cmd_youtube(query_or_url: str, full: bool = False, top: int = 1):
    """YouTube command."""
    print(f"Fetching YouTube content: {query_or_url}")

    # If it's a URL, fetch transcript directly
    if query_or_url.startswith("http"):
        result = get_youtube_transcript(query_or_url)
        print(result["transcript"])
    else:
        # Search YouTube (limited support without API)
        print(f"Search: {query_or_url} (requires direct URL for full transcript)")


def cmd_github(query: str, language: str = None):
    """GitHub search command."""
    print(f"Searching GitHub: {query}")
    results = search_github(query, language=language)

    for item in results:
        print(f"\n{item['repo']} / {item['file']}")
        print(f"  {item['url']}")
        print(f"  {item['snippet']}\n")


def cmd_index(subcommand: str, args: list = None):
    """Index management command."""
    args = args or []
    index = load_index()

    if subcommand == "list":
        if not index.get("entries"):
            print("No research docs in library yet.")
            return
        print(f"Research Library ({len(index['entries'])} docs)\n")
        for entry in index["entries"]:
            topics = ", ".join(entry.get("topics", [])[:3])
            print(f"{entry['title']}")
            print(f"  Category: {entry.get('category')} | Date: {entry.get('date')}")
            print(f"  Topics: {topics}")
            print(f"  File: {entry['file']}\n")

    elif subcommand == "search":
        if not args:
            print("Usage: index search \"<query>\"")
            return
        query = " ".join(args)
        matches = index_search(query, index)
        if not matches:
            print(f"No matches for: {query}")
            return
        print(f"Matches for '{query}':\n")
        for m in matches[:10]:
            print(f"[score:{m['score']}] {m['title']}")
            print(f"  File: {m['file']}")
            print(f"  Summary: {m['summary'][:100]}...\n")

    elif subcommand == "rebuild":
        print("Rebuilding index from Assets/_Docs/Research/...")
        docs_dir = Path(__file__).parent.parent / "Assets" / "_Docs" / "Research"
        if not docs_dir.exists():
            print("Research directory not found.")
            return

        entries = []
        for md_file in docs_dir.glob("*.md"):
            if md_file.name == "README.md":
                continue
            try:
                content = md_file.read_text(encoding='utf-8', errors='replace')
                lines = content.split('\n')
                title = lines[0].replace("# ", "") if lines else "Unknown"

                # Extract metadata from italic lines
                meta_line = lines[1] if len(lines) > 1 else ""
                date_match = re.search(r'\*Date: (\d{4}-\d{2}-\d{2})', meta_line)
                date = date_match.group(1) if date_match else "unknown"

                cat_match = re.search(r'Category: (\w+)', meta_line)
                category = cat_match.group(1) if cat_match else "general"

                # Auto-extract topics
                words = re.findall(r'\b[a-z]{3,}\b', content.lower())
                word_freq = {}
                for word in words:
                    word_freq[word] = word_freq.get(word, 0) + 1
                topics = [w for w, _ in sorted(word_freq.items(), key=lambda x: -x[1])[:8]]

                # Get summary (first 200 chars after metadata)
                summary_start = content.find("## Summary") + len("## Summary")
                summary_end = content.find("\n##", summary_start)
                summary = content[summary_start:summary_end].strip()[:200]

                entries.append({
                    "file": f"Assets/_Docs/Research/{md_file.name}",
                    "title": title,
                    "date": date,
                    "category": category,
                    "topics": topics,
                    "summary": summary,
                    "sources": [],
                    "query": title.lower()
                })
            except Exception as e:
                print(f"  Error processing {md_file.name}: {e}")

        index["entries"] = entries
        save_index(index)
        print(f"[OK] Rebuilt index with {len(entries)} docs")

    else:
        print(f"Unknown subcommand: {subcommand}")


def cmd_research(topic: str, depth: int = 3, category: str = None):
    """Full research pipeline."""
    # Check library first
    index = load_index()
    matches = index_search(topic, index)
    if matches:
        print(f"[LIBRARY] Found {len(matches)} related doc(s):")
        for m in matches[:3]:
            print(f"  [score:{m['score']}] {m['title']}  ->  {m['file']}")
        print()

    print(f"Starting research: {topic} (depth={depth})")

    results = {}

    # Web search
    print("  → Web search...")
    if category and category in CATEGORIES:
        query = topic + " " + " ".join(CATEGORIES[category])
    else:
        query = topic

    try:
        results["web"] = search_jina(query, num_results=5 if depth >= 3 else 3)
    except:
        results["web"] = search_duckduckgo(query, num_results=5 if depth >= 3 else 3)

    # Scrape top results
    if depth >= 3:
        print("  → Scraping top results...")
        for src in results["web"][:3]:
            if "url" in src:
                try:
                    src["full_content"] = scrape_jina(src["url"])[:3000]
                except:
                    src["full_content"] = src.get("content", "")

    # GitHub code search if depth >= 4
    if depth >= 4:
        print("  → GitHub code search...")
        try:
            results["github"] = search_github(topic, language="csharp")
        except:
            results["github"] = []

    # Generate report
    report = format_report(topic, results, category, depth)

    # Save to library instead of reports
    sources_list = [s.get("url", "unknown") for s in results.get("web", [])]
    save_to_library(topic, report, category or "general", sources_list, query, depth)

    print(f"\n[OK] Research saved to library\n")
    print(report)


# ============================================================================
# CLI
# ============================================================================

def main():
    if len(sys.argv) < 2:
        print("""
Claude Code Research Tool

Usage:
  python research.py search <query> [--sources N] [--depth 1-5] [--category CATEGORY]
  python research.py scrape <url> [--format md|json]
  python research.py youtube <url|query> [--full] [--top N]
  python research.py github <query> [--language LANG]
  python research.py research <topic> [--depth 1-5] [--category CATEGORY]
  python research.py index <list|search|rebuild> [args...]

Categories: unity, game-design, comparable, shaders, multiplayer, procgen, performance, animation, architecture

Examples:
  python research.py search "Unity 6 character controller" --depth 3
  python research.py youtube "https://youtube.com/watch?v=..." --full
  python research.py research "Valheim combat design" --depth 4 --category game-design
  python research.py index list
  python research.py index search "jump mechanics"
        """.strip())
        sys.exit(0)

    cmd = sys.argv[1]

    try:
        if cmd == "search":
            query = sys.argv[2]
            sources = int(next((sys.argv[i+1] for i, x in enumerate(sys.argv) if x == "--sources"), 5))
            depth = int(next((sys.argv[i+1] for i, x in enumerate(sys.argv) if x == "--depth"), 3))
            category = next((sys.argv[i+1] for i, x in enumerate(sys.argv) if x == "--category"), None)
            cmd_search(query, sources, depth, category)

        elif cmd == "scrape":
            url = sys.argv[2]
            format_type = next((sys.argv[i+1] for i, x in enumerate(sys.argv) if x == "--format"), "md")
            cmd_scrape(url, format_type)

        elif cmd == "youtube":
            query = sys.argv[2]
            full = "--full" in sys.argv
            top = int(next((sys.argv[i+1] for i, x in enumerate(sys.argv) if x == "--top"), 1))
            cmd_youtube(query, full, top)

        elif cmd == "github":
            query = sys.argv[2]
            language = next((sys.argv[i+1] for i, x in enumerate(sys.argv) if x == "--language"), None)
            cmd_github(query, language)

        elif cmd == "research":
            topic = sys.argv[2]
            depth = int(next((sys.argv[i+1] for i, x in enumerate(sys.argv) if x == "--depth"), 3))
            category = next((sys.argv[i+1] for i, x in enumerate(sys.argv) if x == "--category"), None)
            cmd_research(topic, depth, category)

        elif cmd == "index":
            subcommand = sys.argv[2] if len(sys.argv) > 2 else "list"
            args = sys.argv[3:] if len(sys.argv) > 3 else []
            cmd_index(subcommand, args)

        else:
            print(f"Unknown command: {cmd}")
            sys.exit(1)

    except IndexError:
        print("Error: missing required arguments")
        sys.exit(1)
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
