# Research Skill for Claude Code

## Availability

This skill provides comprehensive, multi-source research capabilities for Claude Code. Use it whenever you need to gather information, design patterns, code examples, or reference material for the Learning01 project.

## When to Use This Skill

Automatically invoke for queries like:
- "Research how X works in Valheim/Genshin/other games"
- "Find best practices for [Unity feature]"
- "Look up [architecture pattern] examples"
- "Get a GDC talk about [game design topic]"
- "Find Unity code examples for [system]"
- "What's the recommended approach for [problem]?"

---

## Library-First Protocol

Always check the local research library before doing any web search.

**Step 1: Query the index** (cheap — reads JSON metadata only)
```bash
python tools/research.py index search "<topic>"
```

**Step 2: Interpret the score**
- **Score ≥ 3:** Confident match. Read the file instead of web searching.
- **Score 1–2:** Partial match. Read the doc AND run a targeted web search for gaps.
- **No results:** Proceed to web search.

**Step 3: After any web search**
The result auto-saves to `Assets/_Docs/Research/` and the index updates automatically. No manual step needed.

**See everything researched:**
```bash
python tools/research.py index list
```

---

## Commands

### `search <query>` — Multi-source web search

Fast web search across multiple free sources. Returns a markdown report with titles, URLs, and excerpts.

**Usage:**
```bash
python tools/research.py search "Unity 6 character controller IK" --depth 3
python tools/research.py search "Valheim building mechanics" --category game-design --depth 4
```

**Options:**
- `--sources N` — number of results (default 5)
- `--depth 1-5` — research intensity (1=quick, 5=exhaustive)
- `--category CATEGORY` — bias results (see Categories below)

**Output:** Markdown report saved to `tools/reports/` + printed to stdout.

---

### `scrape <url>` — Extract specific URL

Get clean, readable text from any webpage. Good for reading documentation or articles directly.

**Usage:**
```bash
python tools/research.py scrape https://docs.unity3d.com/Manual/InputSystem.html
python tools/research.py scrape https://www.gamasutra.com/article/...
```

**Options:**
- `--format md` — return as markdown (default)
- `--format json` — return as JSON

**Output:** Clean text printed to stdout.

---

### `youtube <url>` — Extract YouTube transcript

Get full transcript (with timestamps) from YouTube videos, tutorials, and GDC talks. No API key needed.

**Usage:**
```bash
python tools/research.py youtube "https://www.youtube.com/watch?v=..."
python tools/research.py youtube "GDC 2023 third person camera design" --top 3
```

**Options:**
- `--full` — include full transcript (default truncated)
- `--top N` — for search queries, fetch top N video transcripts (default 1)

**Output:** Timestamped transcript printed to stdout.

---

### `github <query>` — Search GitHub for code examples

Find open-source C# code examples and architectural patterns on GitHub. No authentication needed.

**Usage:**
```bash
python tools/research.py github "event bus observer pattern csharp" --language csharp
python tools/research.py github "third person camera unity" --language csharp
```

**Options:**
- `--language LANG` — filter by language (default: all)

**Output:** Top 5 results with file paths and snippets.

---

### `research <topic>` — Full research pipeline

Deep multi-source research orchestrating all backends: web search → scrape results → YouTube → GitHub code examples. Best for broad topics requiring thorough understanding.

**Usage:**
```bash
python tools/research.py research "third person camera rig Unity 6" --depth 4 --category unity
python tools/research.py research "Valheim combat feel design" --depth 3 --category game-design
```

**Options:**
- `--depth 1-5` — controls breadth and content depth (see Depth Levels below)
- `--category CATEGORY` — domain bias (see Categories below)

**Output:** Comprehensive markdown report saved to `Assets/_Docs/Research/` + printed to stdout.

---

### `index <list|search|rebuild>` — Manage research library

Query and maintain the persistent research library.

**Usage:**
```bash
python tools/research.py index list
python tools/research.py index search "jump mechanics"
python tools/research.py index rebuild
```

**Subcommands:**
- `list` — Display all researched docs with title, category, date, topics
- `search "<query>"` — Query the index by keyword; returns matching docs with score
- `rebuild` — Rescan `Assets/_Docs/Research/` and rebuild the index from scratch

**Output:** Formatted table or search results printed to stdout.

---

## Categories

Apply category bias to searches for more relevant results:

| Category | Best for | Bias |
|----------|----------|------|
| `unity` | Unity docs, APIs, packages | Filters to docs.unity3d.com, Unity blogs |
| `game-design` | Game mechanics, design patterns | Adds "GDC", "game design", "mechanics" |
| `comparable` | Valheim, Genshin, MMBN research | Searches for specific game names |
| `shaders` | URP, Shader Graph, rendering | Adds shader-specific keywords |
| `multiplayer` | Netcode, authority, state sync | Adds "Netcode", "NGO", "authority" |
| `procgen` | Procedural generation techniques | Adds "procedural", "noise", "biome" |
| `performance` | Optimization, profiling | Adds "profiling", "optimization", GPU terms |
| `animation` | Animator, IK, blend trees | Adds animation-specific keywords |
| `architecture` | Event bus, patterns, observer | Adds "event bus", "reactive", "pattern" |

**Example:**
```bash
python tools/research.py search "character controller" --category unity
python tools/research.py research "building system" --category game-design
```

---

## Depth Levels

`--depth 1-5` controls how thorough the research is:

| Level | Use Case | Sources | Content/Source | Includes YouTube | Includes GitHub |
|-------|----------|---------|-----------------|------------------|-----------------|
| 1 | Quick reference | 3 | 500 chars | No | No |
| 2 | Standard search | 5 | 1,500 chars | No | No |
| 3 | Normal (default) | 7 | 3,000 chars | If found | No |
| 4 | Thorough research | 10 | Full content | Yes (3 videos) | Yes |
| 5 | Exhaustive | 10+ | Full content | Yes (5 videos) | Yes (deep) |

**Examples:**
- `--depth 1` — Quick answer to simple question
- `--depth 3` — Standard research report
- `--depth 4` — Comprehensive research with code
- `--depth 5` — Exhaustive multi-source synthesis

---

## Output

All research results are saved to `tools/reports/` as markdown files with:
- Query, category, depth, timestamp
- Source sections with titles, URLs, full/excerpted content
- YouTube transcripts (if any)
- GitHub code examples (if any)
- Citations list

Reports are also printed to stdout so you can read them immediately.

---

## Examples

### Research Game Design
```bash
python tools/research.py research "stamina system design" --depth 3 --category game-design
```
→ Searches for GDC talks, articles, comparable game designs about stamina mechanics.

### Find Unity API Usage
```bash
python tools/research.py search "Animator.SetTrigger best practices" --category unity --depth 3
```
→ Finds official Unity docs, blog posts, and code examples for Animator API.

### Get GDC Talk Transcript
```bash
python tools/research.py youtube "GDC 2022 third person camera design"
```
→ Fetches and transcribes the talk directly (requires exact video title or direct URL).

### Find Architecture Patterns
```bash
python tools/research.py github "reactive event bus observer csharp" --language csharp
```
→ Finds open-source implementations of reactive patterns in C#.

### Full Deep Dive
```bash
python tools/research.py research "URP shader rendering pipeline" --depth 5 --category shaders
```
→ Exhaustive research: docs + articles + YouTube tutorials + GitHub implementations.

---

## How to Interpret Results

1. **Read the summary section** — contains the most relevant findings.
2. **Check sources** — verify claims by reading cited articles.
3. **Cross-reference** — compare approaches from different sources.
4. **Use code examples** — GitHub sections provide runnable patterns.
5. **Watch transcripts** — YouTube sections often contain expert insights.

---

## Integration with Claude Code

When you ask a research question, Claude Code automatically:
1. Determines the appropriate research depth and category
2. Invokes the research tool via `python tools/research.py`
3. Reads the generated report
4. Synthesizes findings into a coherent answer

You don't need to invoke these commands manually — they're here for reference. Claude Code uses this skill autonomously.

---

## Fallback Behavior

If a primary backend fails:
- **Jina Search fails** → Fallback to DuckDuckGo HTML scraping
- **Jina Reader fails** → Fallback to Beautiful Soup parsing
- **YouTube transcript fails** → Fallback to yt-dlp with auto-generated captions
- **GitHub API fails** → Returns empty results

The skill continues gracefully — no failures, just degraded extraction quality.

---

## No API Keys Required

All backends are free and require no authentication:
- **Jina Search** — Free tier
- **Jina Reader** — Free tier
- **YouTube Transcript API** — YouTube's public CC system
- **yt-dlp** — No auth
- **GitHub API** — Public repos, no auth required
- **DuckDuckGo** — No auth

Just run `pip install -r tools/requirements.txt` once.

---

## Performance Notes

- Search: ~2-5 seconds
- Scrape: ~3-10 seconds (depends on page size)
- YouTube transcript: ~5-15 seconds (depends on video length)
- GitHub search: ~2-3 seconds
- Full research pipeline: ~30-60 seconds for depth 4-5

Reports are cached in `tools/reports/` for later reference.
