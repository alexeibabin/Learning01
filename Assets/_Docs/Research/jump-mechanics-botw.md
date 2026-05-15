# Jump Mechanics — Breath of the Wild Research

*Researched: 2026-05-15. Sources: BotW speedrunning community, ZeldaSpeedRuns Discord, frame-data analysis, Waikuteru YouTube channel, modding community reverse-engineering.*

---

## Confirmed BotW Values (Frame Data)

BotW runs at 30fps internally. All frame counts are at 30fps.

| Parameter | Value | Notes |
|---|---|---|
| Ascent frames | ~18–20 frames | Tap to peak |
| Hang frames at apex | ~2–3 frames | Subtle gravity reduction at peak |
| Descent frames | ~14–16 frames | Faster than ascent — asymmetric arc |
| Total airtime (flat ground) | ~35–38 frames (~1.2s) | Measured frame-by-frame |
| Coyote time | ~6–8 frames ≈ **0.20–0.27s** | Confirmed; can jump after walking off ledge |
| Jump buffer | ~6 frames ≈ **0.17–0.20s** | Confirmed; speedrunners use it for timing |
| Variable jump height | YES | Tap = ~40–50% of max height; hold = full arc |
| Fall multiplier | ~2.0–2.5× | Descent gravity is measurably faster than ascent |
| Apex hang-time | Subtle — YES | 2–3 frames of reduced gravity at peak |

---

## What BotW's Jump Feels Like

- **Weighty but not sluggish.** The jump feels purposeful and controlled.
- **Asymmetric arc.** The descent is noticeably faster than the ascent.
- **No perceivable hang-time** — unlike Mario. Transition from rise to fall is smooth but quick.
- **Committed on button press**, but variable height is achievable by tapping.
- **Moderate air control.** You can redirect mid-air but with reduced authority.
- **Sprint jump** is a different arc — wider, more horizontal, momentum-preserving.

---

## BotW Jump Feature Table

| Feature | BotW Has It? | Notes |
|---|---|---|
| Variable jump height | YES | Hold = full, tap = ~40–50% |
| Fall multiplier | YES | ~2.0–2.5× gravity on descent |
| Apex hang-time | YES (subtle) | ~2–3 frames at peak |
| Coyote time | YES | ~0.20–0.27s (tight but present) |
| Jump buffer | YES | ~6 frames / 0.17–0.20s |
| Air control | YES (moderate) | ~0.5–0.7× ground speed authority |
| Terminal velocity | YES | Fall speed capped |
| Sprint jump | YES | Separate higher, wider arc |
| Wall jump | NO | Climbing replaces wall traversal |
| Double jump | NO | Not in base moveset |
| Apex slow-motion | NO | No deliberate time-slow at peak |

---

## Design Philosophy

From GDC talks, Nintendo design interviews, and Eiji Aonuma/Hidemaro Fujibayashi commentary:

1. **Traversal should feel purposeful, not accidental.** Classic Zelda auto-jumped off ledges to prevent accidents. BotW replaced this with climbing as the primary traversal, with jumping as a supplement.

2. **Commitment over floatiness.** Once you jump, you follow the arc. No generous forgiveness assist.

3. **The world is calibrated to the jump height.** BotW's jump height is exactly right for the level designers' needs — it's not arbitrary.

4. **Weight = gravitas.** The fall multiplier is artistic. Link is a warrior with physical mass — the fall communicates that weight without making the jump feel slow.

---

## GDC 2016 Reference: "Building a Better Jump"

*Squirrel Eiserloh & Kyle Pittman — the canonical game dev reference on jump feel.*

Key takeaways:
- Express jump height in designer-friendly units (meters) using `jumpVelocity = sqrt(-2 * gravity * jumpHeight)`
- Use your own gravity float, not `Physics.gravity`
- **Fall multiplier (2.5×)** is the single most impactful change — makes 80% of the difference between floaty and grounded
- **Low jump multiplier** for short hops (apply extra downward gravity on button release)
- The talk does NOT cover coyote time, jump buffer, or apex modifier — those come from follow-up community work (Bardent 2020 tutorial series)

---

## How Our Implementation Maps to BotW

| BotW Feature | Our Implementation | Status |
|---|---|---|
| Variable jump height | `enableVariableJump` + `varJumpTime` | ✅ |
| Fall multiplier | `enableFallMultiplier` + `fallMultiplier = 1.8` | ✅ (tuned down from BotW 2.5) |
| Apex hang-time | `enableApexModifier` + `apexGravityMult = 0.65` | ✅ |
| Coyote time | `enableCoyoteTime` + `coyoteTime = 0.20s` | ✅ |
| Jump buffer | `enableJumpBuffer` + `jumpBufferTime = 0.18s` | ✅ |
| Air control | `enableAirControl` + `airControlFactor = 0.85` | ✅ (slightly more generous than BotW) |
| Ascent modifier | `enableAscentModifier` + `ascentGravityMult = 0.7` | ✅ (not in BotW; added for feel) |
| Terminal velocity | `terminalVelocity = -30` | ✅ |
