# Jump System — Current Implementation State

*Last updated: 2026-05-15*

---

## Architecture

The jump system is split across two files:

- **`Assets/_Scripts/Settings/JumpSettings.cs`** — ScriptableObject holding all designer-tunable parameters
- **`Assets/_Scripts/CheapCharController.cs`** — MonoBehaviour that drives the character; reads from JumpSettings

The asset instance lives at: `Assets/_Scripts/Settings/JumpSettings.asset`

---

## JumpSettings Parameters (current values in asset)

### Core Physics
| Field | Value | Notes |
|---|---|---|
| `gravity` | -27 | In range for BotW feel (-25 to -30) |
| `maxJumpHeight` | 1.25 | Cut in half from original 2.5 |
| `terminalVelocity` | -30 | Fall speed cap |

### Variable Jump Height
| Field | Value | Notes |
|---|---|---|
| `enableVariableJump` | true | BotW has this |
| `minJumpHeight` | 1.0 | BotW tap ≈ 40–50% of max |
| `varJumpTime` | 0.2 | Window to hold for full height |

### Ascent Feel
| Field | Value | Notes |
|---|---|---|
| `enableAscentModifier` | true | Not in BotW; added for feel |
| `ascentGravityMult` | 0.7 | Faster ascent (less gravity going up) |

### Fall Feel
| Field | Value | Notes |
|---|---|---|
| `enableFallMultiplier` | true | BotW has this (~2.0–2.5×) |
| `fallMultiplier` | 1.4 | Tuned down from BotW; less punishing |

### Apex Modifier
| Field | Value | Notes |
|---|---|---|
| `enableApexModifier` | true | BotW has this (subtle) |
| `apexThreshold` | 25 | Velocity below which apex kicks in |
| `apexGravityMult` | 0.65 | Higher = less hang, lower = more hover |

### Coyote Time
| Field | Value | Notes |
|---|---|---|
| `enableCoyoteTime` | true | BotW confirmed ~0.20–0.27s |
| `coyoteTime` | 0.20 | Matches BotW |

### Jump Buffer
| Field | Value | Notes |
|---|---|---|
| `enableJumpBuffer` | true | BotW confirmed ~0.17–0.20s |
| `jumpBufferTime` | 0.18 | Matches BotW |

### Air Movement
| Field | Value | Notes |
|---|---|---|
| `enableAirControl` | true | BotW ~0.5–0.7× |
| `airControlFactor` | 0.85 | Slightly more generous than BotW |

### Descent Momentum
| Field | Value | Notes |
|---|---|---|
| `enableDescentMomentum` | true | Activates in second half of descent |
| `descentMomentumMultiplier` | 1.3 | Air control boost during descent |

### Landing Speed Boost
| Field | Value | Notes |
|---|---|---|
| `enableLandingSpeedBoost` | true | Brief speed reward on landing |
| `landingSpeedBoostMultiplier` | 1.15 | 15% speed boost |
| `landingSpeedBoostDuration` | 0.5 | Boost lasts 0.5s |

### Debug
| Field | Value | Notes |
|---|---|---|
| `logJumpEvents` | false | Toggle to true to debug |

---

## How the Controller Uses These Values

```
CheapCharController.Update():
    ApplyGravity()       ← handles landing detection, coyote time, fall/apex/ascent gravity
    HandleJump()         ← handles buffer, coyote gate, jump execution, variable height cut
    [horizontal speed]   ← air control factor + descent momentum multiplier
    _cc.Move()           ← applies combined horizontal + vertical velocity
```

### Key Derived Values (properties on JumpSettings, not serialized)
- `JumpVelocity = sqrt(-2 * gravity * maxJumpHeight)` — computed each time, never stale
- `MinJumpVelocity = sqrt(-2 * gravity * minJumpHeight)` — same

### Landing Boost Anti-Stack
`_landingBoostTimer` is only set on the frame of landing (`!_wasGroundedLastFrame && isGrounded`). Jumping again before the timer expires does NOT reset it — it counts down regardless.

---

## Known Issues / Next Steps

### "Planted" Landing Feel
The current horizontal movement is still recalculated fresh each frame:
```csharp
Vector3 movement = moveDir * hSpeed + Vector3.up * _verticalVelocity;
```
There is no persistent `_horizontalVelocity` vector. This means:
- No true momentum carry-over from sprint into jump
- Landing snaps to ground control immediately
- The landing boost adds speed but doesn't carry any momentum

**Planned fix:** Introduce a `_horizontalVelocity` vector that:
1. Carries 90% of ground speed into the jump (momentum carry-over)
2. Blends toward input during airtime (air steering)
3. Carries air velocity through landing with a friction window (~0.18s) before settling to ground speed

### Stopping Feel
No lerp-to-zero on stop. Character snaps from moving to not-moving immediately.
Tachyon Flow uses a `lerp to 0 speed over a few frames` on regular stop and a slide-to-stop animation on sprint stop.

---

## Tuning History

| Date | Change | Reason |
|---|---|---|
| 2026-05-15 | `gravity` -20 → -27 | Too floaty |
| 2026-05-15 | `apexGravityMult` 0.5 → 0.35 | Too much hover |
| 2026-05-15 | `maxJumpHeight` 2.5 → 1.25 | "Absurdly high" |
| 2026-05-15 | `fallMultiplier` 2.5 → 1.8 → 1.4 | Drop too fast |
| 2026-05-15 | `apexGravityMult` 0.35 → 0.65 | Apex transition too aggressive |
| 2026-05-15 | Added `enableAscentModifier` + `ascentGravityMult = 0.7` | Ascent ("climb") felt too slow |
| 2026-05-15 | `airControlFactor` 0.6 → 0.85 | Losing forward momentum mid-air |
| 2026-05-15 | Added `enableDescentMomentum` + `descentMomentumMultiplier` | Momentum builds in second half |
| 2026-05-15 | Added `enableLandingSpeedBoost` + multiplier + duration | Reward landing with brief speed |
| 2026-05-15 | `minJumpHeight` 0.4 → 1.0 | BotW tap is ~40–50% of max |
| 2026-05-15 | `coyoteTime` 0.15 → 0.20 | Matches BotW confirmed value |
| 2026-05-15 | `jumpBufferTime` 0.15 → 0.18 | Matches BotW confirmed value |
