# Learning01 — CLAUDE.md

A permanent brief for Claude Code. Every session reads this before starting work.

---

## Project Identity

**Learning01** — A 3D action-adventure multiplayer game.

**Goal:** A learning project with a concrete deliverable. The dual objective is (1) learning to work effectively with AI to produce functional, readable code and (2) building genuine competence across all dimensions of 3D game development — design, code, art, animation, shaders, architecture, and multiplayer.

**Inspirations:**
- **Valheim** — feel, core loop, co-op survival/exploration
- **Genshin Impact** — aesthetic language, world presentation, character design quality
- **Mega Man Battle Network (MMBN)** — art direction reference, stylised visual identity
- **Solarpunk** — thematic and environmental aesthetic (lush, optimistic, nature + tech)

**World Design:**
- Hand-crafted key cities and landmark areas (authored, curated)
- Procedurally generated regions filling the rest of the map
- Fixed + procedural hybrid

**⚠️ Important — Current Assets:**  
All current art assets (`Blink/`, `PolygonPrototype/`) are **temporary placeholders** with no relation to the final art direction. They will be discarded. Do not treat them as design decisions.

---

## Architecture (Forming)

The architecture is EventHub-based and is still in early formation. It will expand significantly over time. Do not treat the current state as final.

### Core Patterns (Current)

| System | Pattern | Key Files |
|--------|---------|-----------|
| `Game` | Static service locator — single global access point for all managers | `Game.cs` |
| `AbstractProvider<TType,TProduct>` | Generic registry backed by `ReactiveDictionary` | `Utils/AbstractProvider.cs` |
| `ProviderView<TType,TProduct>` | MonoBehaviour auto-registers itself with its provider on `Awake` | `Utils/ProviderView.cs` |
| `EventHub` | Typed pub/sub bus: struct events implementing `IEvent`, UniRx `ReactiveProperty<T>` per type | `Utils/EventHub.cs` |
| `WindowsManager` | Layered UI with priority arbitration, lazy load from `Resources/Windows/` | `Managers/WindowsManager.cs` |

### Layer Separation Rules — HARD (Never Break)

| Layer | Reads game data? | Writes game data? | Owns state? | View operations? |
|-------|-----------------|-------------------|-------------|-----------------|
| **Views** | YES | NO (view-only data only) | — | YES |
| **Controllers** | YES | YES | NO | NO |
| **Utils** | via injection | via injection | NO | YES (any logic) |

- **Views**: Read game/model data freely. May only write data that is strictly view-related (e.g., UI state). Never mutate game state.
- **Controllers**: Subscribe to `EventHub` events. Read and modify game/model data. Contain no state of their own. Perform no view-related operations — only game logic.
- **Utils**: Static classes with static methods. All inputs arrive via parameter injection. May contain any type of logic — view, controller, calculations, transformations.

### Other Hard Rules

- **Never use `FindObjectOfType` or `FindWithTag`** — go through the provider system instead
- **Never use `Singleton<T>`** — use the `Game` service locator
- **Never use `async/await`, coroutines, or `UnityEvent` where a UniRx reactive stream fits**

---

## Codebase Map

```
Assets/
  _Scripts/           ← all custom code (underscore = ours)
    Game.cs           ← service locator root (WIP: Start not wired yet)
    BaseWindow.cs     ← base class for all UI windows
    WindowEvents.cs   ← WindowOpenedEvent / WindowClosedEvent structs
    CharacterActions.cs   ← InControl PlayerActionSet (WASD + arrows)
    CheapCharController.cs  ← temp test character Animator driver
    Managers/
      AssetManager.cs     ← Resources.Load<T> wrapper (GetAsync is a stub)
      CanvasManager.cs    ← AbstractProvider<WindowCanvasType, Canvas>
      WindowsManager.cs   ← layered window system with priority
    Utils/
      AbstractProvider.cs ← generic ReactiveDictionary registry
      ProviderView.cs     ← MonoBehaviour auto-registration base
      EventHub.cs         ← typed pub/sub event bus
      ManagerEnums.cs     ← WindowCanvasType, WindowLayerType, WindowPriorityType
      Consts.cs           ← shared constants (currently empty)
    Views/
      LoadingWindow.cs    ← empty BaseWindow subclass
      MainMenuWindow.cs   ← empty BaseWindow subclass
  _Scenes/
    TestScene.unity             ← UI/window system scene
    MovementTestingScene.unity  ← character movement scene
  _Prefabs/
    TestCharacter.prefab   ← temp placeholder character
```

### Third-Party Libraries (VOLATILE — subject to change)

- `InControl/` — cross-platform input (may be replaced)
- `Plugins/UniRx/` — reactive extensions (core to architecture, but specific version/library may change)
- `Blink/`, `PolygonPrototype/` — temp placeholder art, will be removed

When a library changes, update relevant sections of CLAUDE.md.

---

## Coding Conventions

**Naming:**
- Private fields: `_camelCase`
- Constants: `UPPER_SNAKE_CASE` in `Consts.cs`
- Event structs: implement `IEvent`, named `<Noun><Verb>Event` (e.g., `WindowOpenedEvent`)
- Classes follow standard PascalCase

**Adding new windows:**
1. Subclass `BaseWindow`
2. Add `WindowProperties` entry to `WindowsManager._windowProperties`
3. Place prefab at `Resources/Windows/<ClassName>`

**Adding new managers:**
1. Extend `AbstractProvider<TType, TProduct>` or use `ProviderView`
2. Register the static property in `Game.cs`

**Comments:** Only when the WHY is non-obvious. No docblocks. No method narration.

---

## Skill Areas

Claude must draw on all of the following domains when contributing to this project:

- **Game Design** — mechanics, systems, player experience, progression, loop design
- **Unity 6 Development** — MonoBehaviour, ScriptableObjects, Editor scripting, prefabs, scenes
- **Third-Person Character Control** — camera rigs, movement feel, combat, IK, root motion
- **3D Game Development** — scene management, physics, colliders, triggers, raycasts
- **3D Models, Textures & Integration** — FBX import settings, LODs, UV mapping, texture atlasing, material setup
- **Character & Object Animation** — Animator controller, blend trees, humanoid rigs, animation events, IK, state machines
- **Game Architecture** — service locator, event bus, state machines, object pooling, ECS concepts
- **Shaders & Render Pipeline** — URP (full familiarity), Shader Graph, VFX Graph, post-processing, lighting, performance implications
- **Multiplayer** — Unity Netcode for GameObjects (or equivalent), server architecture, state sync, authority models
- **Procedural Generation** — terrain, world regions, dungeon/POI generation
- **Performance Optimization** — profiling, GPU instancing, draw call batching, occlusion culling, memory management
- **Comparable Game Insights** — best practices and technical solutions from Valheim, Genshin Impact, MMBN, and Solarpunk-genre games

---

## Workflow

### Standard Process for Non-Trivial Work

1. Receive task
2. Research best practices online before designing a solution
3. Write a plan and present it for approval
4. Revise plan based on feedback — iterate until approved
5. Execute only after explicit approval

**Trivial work** (typos, single-line fixes, obvious null checks) may be done inline with a brief explanation.

### Research Mandate

This is a learning project. Always check for best practices, compare approaches, and proactively suggest solutions the user may not have considered. **Research comes before implementation.**

### Git Operations

**Auto-approved (no consent needed):**
- All read-only git operations: `git status`, `git log`, `git diff`, `git show`, `git branch` (list), `git remote`
- Any search/find/locate command in any form (terminal, CLI, scripts)

**Requires consent:**
- Any git operation that creates or modifies history: commits, branches, merges, resets, rebases, checkouts that modify files, push, pull

### General Operations

- All search, find, locate, and context-gathering commands (in any tool or form) are auto-approved
- Any creative (file write/edit) or destructive (delete, overwrite) operation on non-trivial tasks requires consent

---

## UnityCtl Integration

UnityCtl (via the `unity-editor` skill) is the **primary and mandatory interface for all non-code Unity queries**. Direct file reading is reserved for code only.

### The Code vs. Non-Code Split

| Query Type | Tool | Why |
|------------|------|-----|
| C# scripts, source code | Read / Grep / Glob (direct file access) | Files are current, syntax is clear |
| Scene hierarchy, GameObjects | `unityctl snapshot` | Runtime state, not file state |
| Component values, properties | `unityctl snapshot --components` | Actual values, not defaults |
| Prefab structure | `unityctl snapshot --prefab <path>` | Prefab overrides matter |
| Compilation errors | `unityctl asset refresh` + `logs` | Actual compile state |
| Console logs, warnings, errors | `unityctl logs` | Real-time feedback |
| Runtime state (play mode) | `unityctl script eval` | Can't read from files |
| Asset queries, scene list | `unityctl scene list` or `script eval` | Current editor state |
| Visual verification | `unityctl screenshot` | Only when visuals are the point |
| Anything else non-code | `unityctl script eval` — write ad-hoc C# | Flexible, current, avoids guessing |

### Bridge Availability Rule

**If the bridge is unavailable** (check with `unityctl status`):
- Do NOT silently fall back to file reading
- Stop, notify the user, and wait for them to start the bridge or open the editor
- This ensures I'm always using current, authoritative state, not cached/static files

### Verification Preference Hierarchy

When verifying changes, prefer (in order):
1. **`unityctl snapshot`** — cheapest, structured, current
2. **`unityctl logs`** — catches errors, warnings, real feedback
3. **`unityctl script eval`** — flexible, query exactly what you need
4. **`unityctl screenshot`** — expensive in context, hard to diff, only for visual output (art, layout, polish)

---

## Memory System

A persistent memory system lives at:  
`C:\Users\Supreme\.claude\projects\c--Users-Supreme-Documents-Unity-Projects-Learning01\memory\`

Read `MEMORY.md` at the start of each session to restore cross-session context: user preferences, prior decisions, recurring corrections, and architectural choices.

When the user corrects a plan, notes a preference, or makes a recurring choice — save it to memory immediately.

---

*Last updated: 2026-05-15*
