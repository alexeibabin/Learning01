# Tachyon Flow (JankyAnims) — Character Controller Reference

*Researched: 2026-05-15. Sources: Reddit r/Unity3D posts by u/JankyAnims, 80.lv articles, Steam page, direct developer comments in threads.*

Steam page: https://store.steampowered.com/app/4413210/Tachyon_Flow/
Discord: https://discord.gg/gGQBRbhX

---

## Why This Is a Reference

The movement in Tachyon Flow is the target feel for this project — not the game's speed or aesthetic, but the **smoothness, character weight, and momentum continuity** of the controller. It was called out specifically as having the exact controller feel to aim for.

---

## What the Movement Looks Like

- Third-person parkour: wall runs, ledge grabs, slides, tucks, rolls
- Slow motion at the peak of jumps
- Seamless animation blending between all states
- No auto-correction on any move — everything is manually timed and skill-gated
- Described by observers as: "feels weighty with carried momentum"
- Compared to Mirror's Edge, Cloudbuilt

---

## Technical Implementation (from developer's own words)

### Foundation
- **Unity's built-in `CharacterController` component** — not Rigidbody, not Kinematic Rigidbody
  > "All character controller" — JankyAnims, direct reply
- Custom scripts on top; no third-party movement packages
- Code was partially written with ChatGPT assistance; developer described it as "spaghetti AI code that runs on hopes and dreams" — **not recommended as a direct code reference**

### Architecture
- **One script per move type** to keep it manageable
  > "I made separate scripts for everything to keep it modular and organised, things like horizontal wall run, vertical wall run, wall jumps etc were all separated so I didn't get lost in the sauce"
- State machine driven by **bools + speed checks**
  > "State entry and exit, just a lot of bools and speed checks lol, it was quite complicated by the time I got it to this state"

### What Makes It Look Smooth — Animation, Not Code
This is the most important finding. The visual fluidity is almost entirely animation work:
> "There's no IK, no layer blending, nothing procedural, etc. The transition smoothness is all in the actual animation work. I care very much about things like weight/balance and foot placement, all animations blending into the next etc, so I have a lot of animations dedicated just to going between states."

- No IK of any kind
- No root motion — **explicitly avoided**
  > "I specifically wanted to avoid root motion because I don't like GTA-style movement. I wanted this to look really nice, accurate, and weighty, but also play very responsively."
- No motion matching
- Standard Unity Animator (Mecanim), animations made in Blender
- Dedicated animations for every transition (run start → loop → end, move-to-move blends)

### How Stopping Works
- **Regular run stop:** lerp speed to 0 over a few frames after input releases
  > "just transitions from run start > run loop > run end depending on speed. If the player is doing a regular run, she'll come to a stop in a couple of steps, I did this by having a little lerp to 0 speed after player releases controls."
- **Sprint stop:** plays a slide-to-stop animation
  > "If she's sprinting, she'll do a little slide to come to a stop instead of steps."
- The "not planted" stop feel comes from the lerp and the slide animation — not from momentum carry-over code

### How Jumping Works
- **Forward speed is preserved on jump** — intentional carry-over
  > "keeping the forward speed on jumps was intentional to keep momentum"
- Jump anticipation animations were tried but dropped for responsiveness
  > "I did try to add some anticipation, but it delays input because you have to wait several frames. I went for responsiveness instead."
- Slow motion at jump apex (visual effect)

### Landing System
- Timing-gated roll on landing: **press Space just before hitting the ground** to roll and preserve flow
  > "There is [a roll], you can see it at the start - I just have the height requirement a little higher so it doesn't feel like you have to do it all the time"
  > "Have to press Space to roll right before you hit the ground, or you get stuck in a small landing pose."
- No auto-roll — fully skill-based
- Miss the window = brief planted landing pose that interrupts momentum

### Ledge Detection
- Raycasts to check if a spot can be climbed — nothing fancy
  > "Just using raycasts to find if the spot can be climbed, nothing fancy just animations made to flow into each other/transition fluidly"

### Wall Running
- Custom math for momentum and angle preservation
  > "Movement vector was more than I really understand myself, all I knew was the overall logic of how I wanted it to function, I left all the math/tangent angles and all that magic to the AI"
- **Wall-to-wall jumps lose power per consecutive jump** — capped at ~3–4 effective jumps
  > "I have it set up so that you continuously lose a bit more power with every consecutive wall jump, so you can't just keep going forever"
- Wall run ledge grabs: "continue your initial speed and slide to a halt, so it feels weighty with the carried momentum"

### Momentum System (Update #3)
- **Switched from fixed run/sprint to momentum-based movement** — this was a major overhaul
  > "Improved overall ground movement/smoothness and switched to momentum based instead of generic run/sprint"
- Speed builds as you run; no instant sprint toggle

### Manual Traversal (Skill-Gated)
All special moves require manual input at the right moment — nothing is automatic:
- Tuck over obstacles: **Ctrl mid-air** (timing window)
- Slide under obstacles: **Ctrl on ground** (timing window)
- Landing roll: **Space just before landing** (timing window)
> "Right now, nothing chooses for you, I wanted to make it more skill based."

---

## Key Takeaways for This Project

| Insight | Application |
|---|---|
| Smoothness = animation transitions, not code | Need dedicated blend animations between movement states |
| Stop feel = lerp to zero | Replace snap-stop with lerp over a few frames |
| Sprint stop = slide animation | Add a slide-deceleration when stopping from high speed |
| Jump forward speed = explicit carry-over | `_horizontalVelocity` carry-over on jump launch |
| Momentum-based speed | Build speed over time rather than fixed sprint toggle |
| Landed "planted" = animation pose | The planted feel is partly a missing landing animation |
| Root motion = avoided | Keep physics-driven movement, not animation-driven |

---

## What This Project Should NOT Copy

- The spaghetti AI-assisted code architecture
- The skill-gated roll system (not right for this game's feel)
- The speed/scale of Tachyon Flow (too fast for this project's goals)
