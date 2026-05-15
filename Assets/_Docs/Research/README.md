# Research Notes

Game development research conducted during the character movement system work.

All research is automatically indexed in `tools/research-index.json` for efficient querying and deduplication. Query the index before searching the web:

```bash
python tools/research.py index list
python tools/research.py index search "jump mechanics"
```

---

## Files

| File | Contents |
|---|---|
| [jump-mechanics-botw.md](jump-mechanics-botw.md) | Confirmed BotW jump values (frame data), design philosophy, GDC 2016 reference, how our implementation maps to BotW |
| [parkour-momentum.md](parkour-momentum.md) | The three momentum patterns, game-by-game breakdown (Mirror's Edge, Dying Light, Ghostrunner, Titanfall 2), landing mechanics, parameter ranges |
| [tachyon-flow-reference.md](tachyon-flow-reference.md) | Detailed breakdown of JankyAnims' Tachyon Flow controller — architecture, animation philosophy, stopping/jumping/landing implementation, key takeaways |
| [jump-system-current-state.md](jump-system-current-state.md) | Current JumpSettings values, how the controller uses them, known issues, full tuning history |

---

## Key Findings Summary

1. **BotW has all the features** we initially considered removing — variable jump, coyote time, buffer, apex modifier. The tuning was wrong; the mechanics were right.

2. **The "planted" feel** comes from recalculating horizontal velocity fresh each frame. The fix is a persistent `_horizontalVelocity` vector with carry-over and a landing friction window.

3. **Tachyon Flow's smoothness is 90% animation work**, not code. The code insight is: lerp-to-zero on stop, explicit forward speed carry-over on jump, momentum-based speed build-up.

4. **Landing is the most important moment** for momentum design — it's where carry-over vs. planted becomes visible to the player.
