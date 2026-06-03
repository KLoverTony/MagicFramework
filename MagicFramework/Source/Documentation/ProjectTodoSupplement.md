# MagicFramework Roadmap Supplement

This supplement preserves an older implementation sequencing plan for roadmap context. The active short-term dashboard now lives in [ProjectTodo.md](ProjectTodo.md), while detailed task definitions live in [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md), [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md), [ProjectAeternusCoreTodo.md](ProjectAeternusCoreTodo.md), and [ProjectLongTermBacklog.md](ProjectLongTermBacklog.md).

## Planning Principles

- Finish framework foundations before adding more content that depends on them.
- Prefer validation spells and smoke tests that exercise real authored XML, not only debug fallbacks.
- Keep player-facing clarity close behind framework capability, especially for spell descriptions, settings, and cancel/toggle behavior.
- Defer large exploratory systems until lifecycle, targeting, scaling, and persistent cleanup rules are stable.

## Milestone 1: Stabilize Core Framework Behavior

Highest priority because later content depends on persistent spell state being predictable.

| Task | Priority | Complexity | Reason |
| --- | --- | --- | --- |
| MF-008 Logging toggles | P1 | M | Complete first pass; routine diagnostics now have subsystem toggles. |
| MF-005 Lifecycle hooks | P1 | M/L | Persistent spell state needs consistent semantics before more systems use it. |
| MF-004 Sustained/channeling cleanup | P1 | L | Complete first pass; maintained UX, interruption profiles, pulses, and area-zone maintained detection are in place. |
| MF-015 Persistent effect self/ally policy | P2 | S | Complete first pass; policy documented and first-party examples audited. |

Implementation order:
1. Add framework settings/logging foundation for subsystem log toggles. Complete.
2. Define lifecycle hook semantics in documentation before code: create, pulse, trigger, expire, remove, and break. Complete.
3. Backfill lifecycle hooks into one or two systems first, likely area zones and force fields. Area-zone and force-field first passes complete.
4. Finish sustained spell UX: player-facing release/toggle, shared maintenance profile interruption rules, authored maintained pulses, and maintained-only area-zone release detection are complete for the first sustained spell systems.
5. Document self/ally defaults and identify XML cleanup opportunities.

Smoke tests:
- Cast `MF_ManaShield`, cancel it, run out of mana, down/stun the caster, and confirm each exit path behaves distinctly.
- Cast `MF_WatersEmbrace`, cancel manually, break concentration, save/load mid-aura, and confirm cleanup.
- Cast `MF_ForceField`, verify break hooks still work, and verify logs appear only when enabled.
- Confirm routine spellcasting does not spam logs with default settings.

## Milestone 2: Complete Targeting And Validation Coverage

This milestone improves confidence that authored spells can express more targeting shapes without one-off workers.

| Task | Priority | Complexity | Reason |
| --- | --- | --- | --- |
| MF-006 Target queries | P1 | M | Already partly validated by `MF_ArcSeeker` and `MF_BlessingOfVigor`. |
| MF-007 Validation spell suite | P1 | M | Keeps framework regressions visible in-game. |
| MF-009 Continue scaling | P2 | M | Best expanded through real validation spells. |

Implementation order:
1. Add missing high-value target query features: target count limits, deterministic ordering, exclude already-hit, lowest-health, and highest-threat.
2. Add a conditional branch validation spell.
3. Add teleport/displacement regression spells: swap, rescue ally, and enemy blink.
4. Wire spell-level scalars into more MFVanilla spells conservatively:
   - `MF_Heal` and `MF_Regeneration`: healing and duration.
   - `MF_ChainLightning`: damage and jump radius.
   - `MF_Haste` and `MF_BlessingOfVigor`: duration only.
5. Defer exotic scaling such as tiered projectile/effect selection until the basic mechanics feel solid.

Smoke tests:
- `MF_ArcSeeker` picks the nearest hostile from the caster position.
- `MF_BlessingOfVigor` affects allies in radius and excludes the caster.
- New query tests produce stable ordering when multiple pawns qualify.
- Heal scaling produces sensible healing values without errors.
- Chain lightning scaling affects damage/radius without runaway targeting.

## Milestone 3: Player-Facing UX And Settings

This milestone makes the framework understandable and configurable for players.

| Task | Priority | Complexity | Reason |
| --- | --- | --- | --- |
| MF-016 Spell details UI | P2 | S/M | Builds on generated descriptions and scalars. |
| MF-027 Validation spell icons | P3 | S | Low-risk polish as new spells are added. |

Implementation order:
1. Add a first-pass spell details window or tab.
2. Show metadata, costs, cooldowns, targeting policy, and generated effect summaries.
3. Show active enhancement/scaling display where practical.
4. Add missing gizmo icons opportunistically.

Smoke tests:
- Open spell details for `MF_Firebolt`, `MF_Fireball`, `MF_ManaShield`, `MF_WatersEmbrace`, and `MF_BlessingOfVigor`.
- Confirm costs/cooldowns match actual cast behavior.
- Confirm no UI overflow at small resolutions.

## Milestone 4: Content Systems With Strong Payoff

This milestone adds gameplay depth once framework rules are stable.

| Task | Priority | Complexity | Reason |
| --- | --- | --- | --- |
| MF-010 Buff/debuff primitives | P2 | M | Reusable status defs are in place; remaining work is policy, scalars, and conversion cleanup. |
| MF-014 Summon/spawn expansion | P2 | M | Opens wards, constructs, hazards, and beacons. |
| MF-011 Projectile support | P2 | M | Complete first pass; impact context, launch authoring, and vanilla-bound policy decisions are in place. |

Implementation order:
1. Finish the MF-010 follow-through: status categories, stacking/refresh policy, named scalars, and a small conversion pass for spells that benefit from premade statuses.
2. Expand summon/spawn support for temporary objects, hazards, wards, and constructs.
3. Projectile impact context is complete for the current vanilla projectile surface; reopen only for future content that needs custom projectile classes or per-launch arcing overrides.
4. Waters Embrace SpellDrowningHediff is owned by the spell and fades immediately after the spell fades which is not the desired effect. Consider  giving SpellDrowningHediff its own decay behavior, and changing Water’s Embrace so it applies/increases/refreshes the hediff without scheduling spell-owned removal in the same way. Then the spell ending stops adding pressure, but the pawn still has to recover from what already happened.

Smoke tests:
- Spawn a temporary object/ward and confirm expiry/save/load cleanup.
- Test projectile spells against pawn hits, misses, walls, shielded pawns, and cover-like interceptions during normal smoke passes.

## Milestone 5: Chain, Visual, And Event Expansion

These are powerful systems, but they are safer after lifecycle, targeting, and projectiles are stable.

| Task | Priority | Complexity | Reason |
| --- | --- | --- | --- |
| MF-012 Branching chains | P2 | L | Crosses targeting, scheduling, visuals, and save/load. |
| MF-025 Celestial/weather hooks | P3 | M | Depends on enhancement/scalar visibility. |
| MF-026 Persistent visuals | P3 | M | Polish-heavy and can expand quickly. |
| MF-029 Real fire integration | P3 | M | Risky gameplay side effects. |
| MF-030 Persistent world objects | P3 | M | Needs mature lifecycle policy. |

Smoke tests:
- Chain spells survive save/load mid-chain.
- No repeated-target bugs occur unless explicitly allowed.
- Weather enhancement visibly affects relevant spells.
- Persistent visuals clean up on expire, cancel, death, and reload.
- `MF_WallOfFire` real-fire experiments do not spread beyond intended bounds unless designed to.

## Milestone 6: Aeternus Core Track

Run this after core MagicFramework stabilizes, unless a content break is desirable.

| Task | Priority | Complexity | Suggested Order |
| --- | --- | --- | --- |
| MF-019 Ritual dialog improvements | P2 | M | 1 |
| MF-020 Psychic sensitivity haunting | P2 | M | 2 |
| MF-022 Torches and arcane lighting | P2 | M | 3 |
| MF-021 Undead pseudo-relationship memory | P2 | L | 4 |
| MF-023 Custom wall atlas | P3 | L | 5 |

Smoke tests:
- Ritual dialog explains every invalid pawn/corpse choice.
- Psychic sensitivity changes haunt outcomes in predictable debug scenarios.
- Undead memory saves/loads without normal relationship graph side effects.
- Torch/sconce placement, rotation, glow, and fuel/refuel behavior work if fuel is added.

## Milestone 7: Late Expansion And Documentation

The version 1 framework surface is steady enough to begin author-facing documentation. Large expansion systems should remain future work, but the spell design guide should now document the current supported surface and call out deferred capabilities explicitly.

| Task | Priority | Complexity | Reason |
| --- | --- | --- | --- |
| MF-028 Dedicated common status review | P3 | S | Revisit after reusable status defs prove which effects still need dedicated primitives. |
| MF-024 Magic tools/weapons | P3 | L | May depend on common status and spell-container decisions. |
| MF-031 Spell design guide | P3 | L | Version 1 documentation pass can begin from the stable authoring surface. |

Recommended order:
1. Write the version 1 spell design guide from the current lifecycle, targeting, scaling, settings, UI, and validation spell surface.
2. Review which statuses still deserve dedicated primitives after guide gaps reveal repeated authoring friction.
3. Explore magic tools and weapons as next-version design work.

## Recommended Next Step

Finish version 1 release hygiene and start MF-031:
- confirm published Workshop dependency/download metadata in both `About.xml` files
- run clean builds and local deployment for MagicFramework and MFVanilla
- smoke test the first-party validation spell suite plus MFVanilla research, scroll, and settings loops
- draft [SpellDesignGuide.md](SpellDesignGuide.md) with `SpellDef` anatomy, targeting, action trees, lifecycle hooks, scaling, FX, generated descriptions, and validation recipes
