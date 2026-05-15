# Parkour Momentum Mechanics — Research

*Researched: 2026-05-15. Sources: 80.lv, Game Developer Magazine, GDC talks, GitHub (Matthew-J-Spencer, adammyhre, mixandjam), Dying Light modding docs, Mirror's Edge community, Ghostrunner speedrun guides.*

---

## The Core Problem

Standard character controllers recalculate horizontal velocity fresh every frame:
```
velocity = inputDirection * speed * airFactor
```
This produces "planted" movement — no real momentum, no carry-over between states. The character snaps to whatever speed the code dictates regardless of what just happened.

Parkour games solve this with a **persistent velocity vector** that:
- Carries sprint speed into the jump
- Blends with input mid-air
- Transitions smoothly on landing instead of snapping

---

## The Three Momentum Patterns

### Pattern 1: Momentum Carry-Over (Mirror's Edge, Ghostrunner, Dying Light)

Sprint momentum (70–100%) is preserved when entering a jump. The velocity vector persists.

```
OnJump:
    airVelocity = groundVelocity * carryOverFactor   // 0.7–1.0
    
InAir:
    airVelocity = Lerp(airVelocity, inputDir * speed * airControl, steeringRate * dt)
    
OnLanding:
    groundVelocity = airVelocity * landingCarryFactor  // 0.8–1.0
```

**Feel:** Fluid, rewards planning the sprint approach. Speed preserved through jumps feels natural.

### Pattern 2: Momentum Building (Reward-Based)

Speed is earned through rewards (landing boosts, chaining moves, descent acceleration). Individual moves amplify rather than preserve.

```
OnDescent (second half of fall):
    airControlFactor *= descentBoost   // 1.2–1.5

OnLanding:
    speed *= landingBoostMultiplier    // 1.1–1.3 for 0.3–0.8s
```

**Feel:** Skill-based, noticeable reward per successful action.

### Pattern 3: Momentum as Resource (Mirror's Edge Catalyst)

Momentum is a separate stat that regenerates while moving and gates abilities.

```
Update:
    if isMoving: momentum += buildRate * dt
    else:        momentum -= decayRate * dt
    momentum = Clamp(0, maxMomentum)
    
CombatDamageMultiplier = 1.0 + (momentum / maxMomentum) * 0.5
```

**Feel:** High-skill mastery loop, combat and traversal are deeply linked.

---

## Game-by-Game Reference

### Mirror's Edge Catalyst
- Momentum = the primary resource; Focus Shield depletes on stop
- "Flow state" — sustained speed is the game's core currency
- Momentum resets entirely on hard stops; design penalises stopping
- No explicit landing boost — momentum naturally carries through

### Dying Light 1 & 2
- DL1: Sprint momentum preserved only if sprint is held through the jump
- DL2: No sprint button; acceleration is continuous forward hold
- **Landing friction window:** 100ms of air friction on landing instead of full ground friction — allows chaining jumps

### Ghostrunner
- No speed cap — momentum accumulates through chaining
- Slide → jump → slide chain accelerates infinitely with skill
- Velocity preservation is absolute during chains
- Landing from a jump: if chained into next move, no momentum loss

### Titanfall 2
- Slide + jump = speed preserved + direction change
- Bunny hopping (air strafe + jump timing) accumulates velocity
- Double-jump + wall-run chains momentum
- Landing: implicit carry-over via velocity preservation

---

## Landing: The Critical Moment

Landing is where momentum design choices are most visible.

| Approach | Behaviour | Games | Feel |
|---|---|---|---|
| Momentum Reset | Full stop on landing | Classic Assassin's Creed | Grounded, cautious |
| Carry-Over (70–100%) | Velocity persists through landing | Ghostrunner, Mirror's Edge | Fluid, rewards momentum |
| Landing Boost (110–130%) | Temporary speed multiplier | Many action games | Rewarding, weight on landing |
| Friction Window | 50–200ms reduced ground friction | Dying Light, Parkour Reborn | Technical, enables chaining |

**Key insight:** Carry-over keeps the speed; the friction window gives it time to settle before ground control takes over. They are complementary.

---

## Parameter Ranges Across Games

| Parameter | Conservative | Moderate | Aggressive |
|---|---|---|---|
| Sprint → jump carry-over | 0.60–0.75 | 0.75–0.90 | 0.90–1.0 |
| Air control factor | 0.60–0.75 | 0.80–0.90 | 0.95–1.0 |
| Landing speed boost | 1.00–1.05 | 1.05–1.15 | 1.15–1.30 |
| Descent momentum boost | 1.05–1.10 | 1.10–1.30 | 1.30–1.50 |
| Landing friction duration | — | 0.10–0.20s | 0.20–0.30s |

---

## Common Pitfalls

1. **Landing boost too short** — window expires before the player can use it. Keep ≥ 0.3s.
2. **Carry-over too low** — sprint momentum feels useless. Don't go below 0.70×.
3. **Air control too high** — jump feels like ground movement, no air challenge.
4. **Descent momentum triggering too early** — unearned boost. Trigger at ~50% of peak velocity.
5. **No momentum stacking cap** — Ghostrunner is intentionally uncapped. Most games cap at 1.25–1.5×.

---

## Design Principles (from research consensus)

1. **Responsiveness is non-negotiable.** Momentum must feel natural, not like it's fighting the controls.
2. **Momentum should feel like a reward.** Maintain speed for skillful play; fumble it and pay the cost.
3. **Landing matters more than jumping.** Jump = launch (vertical control). Landing = momentum (horizontal consequence).
4. **Absolute values matter more than ratios.** `gravity -27, speed 4` is "medium weight" feel. `-40, speed 6` is violent and aggressive.
