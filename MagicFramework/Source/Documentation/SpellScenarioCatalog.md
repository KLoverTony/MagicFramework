# Spell Scenario Catalog

This file is a design resource for two related jobs:

1. Template new sample spells in a consistent format.
2. Stress test whether the current spell model can express and execute the kinds of spells we care about.

The intent is not to define final XML yet. The intent is to keep a grounded scenario catalog that references the constructs we already have in code:

- `SpellTargetingDef`
- `SequenceActionDef`
- `DelayActionDef`
- `DamageActionDef`
- `ApplyHediffActionDef`
- `ExplosionActionDef`
- `LaunchProjectileActionDef`
- `ApplyToTargetsActionDef`
- `ConditionalActionDef`
- `CurrentTargetQueryDef`
- `TargetsInRadiusQueryDef`
- `NearestValidTargetQueryDef`

Targeting concerns that repeatedly show up across these scenarios include:

- area shape and size such as single target, radius, line, wall, or chained cells
- target categories such as pawns, items, buildings, or cells
- pawn affiliation filters such as all pawns, allies, or foes
- contextual validation such as line of sight, self-target rules, standability, hostility, or species restrictions

## Proposed Targeting Framework

The current `SpellTargetingDef` is intentionally lightweight, but the scenarios in this document suggest we should treat targeting as an explicit framework surface rather than a loose string plus a few booleans.

One workable direction would be to evolve targeting into a small authored model with separable concerns:

```text
SpellTargetingDef
- shape: Single | Radius | Line | Wall | Chain
- primaryTargetType: Cell | Pawn | Thing | PawnOrThing | PawnOrCell
- pawnAffinity: All | Ally | Foe
- includePawns: bool
- includeBuildings: bool
- includeItems: bool
- allowSelfTarget: bool
- requireLineOfSight: bool
- requireStandableCell: bool
- requireWalkableCell: bool
- range: float
- radius: float
- lineLength: float
- wallLength: int
- maxChains: int
- factionFilter: optional future extension
- speciesFilter: optional future extension
```

The important design idea is that these are orthogonal:

- shape answers "what spatial pattern is the player authoring?"
- primary target type answers "what kind of thing or location can be clicked?"
- pawn affinity answers "if a pawn is involved, do we mean all pawns, allies, or foes?"
- validation flags answer "what world constraints must be true?"

That separation matters because many spells mix these concerns differently:

- `Healing Touch` wants `shape = Single`, `primaryTargetType = Pawn`, `pawnAffinity = Ally`
- `Hold Person` wants `shape = Single`, `primaryTargetType = Pawn`, `pawnAffinity = Foe`
- `Fireball` likely wants `shape = Radius`, `primaryTargetType = PawnOrCell`, `pawnAffinity = All`
- `Wall of Fire` likely wants `shape = Wall`, `primaryTargetType = Cell`, with later runtime queries applying `pawnAffinity = All` or `Foe` depending on design
- `Teleport` may want `primaryTargetType = Cell`, but also a separate ally-gather rule for secondary affected pawns

This also suggests a useful distinction between:

- cast targeting: what the player selects to begin the spell
- runtime queries: what the spell affects later during execution

Those two layers should probably share concepts like pawn affinity, target category, and spatial shape, even if they are authored on different defs.

## Scenario Template

Use this shape when adding new spells:

```text
Name:
Intent:
Player Targeting:
Execution Steps:
Expected Constructs:
Current Model Fit:
Current Runtime Fit:
Open Gaps:
Notes:
```

Meaning of the fit labels:

- `Strong`: the current framework shape covers this cleanly.
- `Partial`: the idea mostly fits, but one or more important pieces are missing or awkward.
- `Missing`: the current framework does not model this well yet.

## Baseline Scenarios

### Firebolt

Name:
`Firebolt`

Intent:
The simplest offensive spell. Pick one pawn or thing, show an energy impact effect, then apply fire damage.

Player Targeting:
Single target.
Usually `targetType = pawnOrThing`, `allowSelfTarget = false`, `requireLineOfSight = true`.

Execution Steps:
1. Resolve the selected target as the current target.
2. Produce a visible bolt or impact effect.
3. Apply direct fire damage to that target.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- `LaunchProjectileActionDef` or a future dedicated visual effect action
- `DamageActionDef`
- `CurrentTargetQueryDef` only if we want to re-apply child actions through a query wrapper

Current Model Fit:
`Strong`

Current Runtime Fit:
`Strong`

Open Gaps:
- Player targeting rules like `pawnOrThing`, line of sight, and self-target restrictions are not yet enforced by the framework runtime.
- `LaunchProjectileActionWorker` now launches a real RimWorld projectile and defers authored impact actions until projectile resolution.
- `EffectActionWorker` now attempts to resolve authored `EffecterDef` and `SoundDef` names, but complex attachment behavior and richer VFX authoring are still future work.

Notes:
This is the best baseline spell for validating the minimum viable cast pipeline.

### Fireball

Name:
`Fireball`

Intent:
Target a pawn or location, show a traveling fire effect, then deal fire damage in an area and optionally ignite affected targets.

Player Targeting:
Single pawn or cell target.
The design likely wants `targetType = pawnOrCell` or an equivalent concept.

Execution Steps:
1. Accept either a pawn target or a ground target.
2. Launch a visible fireball toward the chosen target point.
3. On impact, affect all pawns and things in a radius.
4. Apply fire damage in that radius.
5. Optionally ignite or add a burning hediff to affected targets.

Expected Constructs:
- `SpellTargetingDef`
- `LaunchProjectileActionDef`
- `ExplosionActionDef`
- `ApplyToTargetsActionDef`
- `TargetsInRadiusQueryDef`
- `DamageActionDef`
- `ApplyHediffActionDef`

Current Model Fit:
`Partial`

Current Runtime Fit:
`Partial`

Open Gaps:
- `targetType` is a raw string today; mixed pawn-or-cell targeting is not strongly modeled.
- `ExplosionActionWorker` has basic flame explosion behavior, but the damage type, fire chance, falloff, and visual/audio details are not fully authorable yet.
- `ApplyHediffActionWorker` has basic severity adjustment behavior, but body-part targeting and richer hediff policies are not modeled yet.
- `LaunchProjectileActionWorker` now models projectile travel and delayed impact timing. Impact actions receive a named impact result for hit, shield block, no-hit impact, destruction, or timeout; cover interception is treated as part of vanilla hit/no-hit handling unless a future spell needs deeper custom projectile context.
- Player targeting rules like `pawnOrCell`, line of sight, and range semantics are not yet enforced by the framework runtime.
- Ignition may want something more specific than a generic hediff, depending on RimWorld fire semantics.
- Impact actions receive the projectile's last known cell plus first-class impact variables such as `ProjectileImpactResult`, `ProjectileBlockedByShield`, and `ProjectileHitThing`.

Notes:
`Fireball` is an excellent first "stress" spell because it forces us to handle location targeting, impact points, area queries, and secondary effects together.

### Delayed Blast Rune

Name:
`Delayed Blast Rune`

Intent:
Place a magical rune at a location, wait briefly, then explode.

Player Targeting:
Single cell target.

Execution Steps:
1. Target a location.
2. Spawn or display a rune effect at that cell.
3. Delay for a fixed number of ticks.
4. Explode at that same cell.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- `DelayActionDef`
- `ExplosionActionDef`

Current Model Fit:
`Partial`

Current Runtime Fit:
`Partial`

Open Gaps:
- There is no action for placing a persistent world visual or object.
- The delayed runtime now schedules and executes child actions later, and `ExplosionActionWorker` provides a basic flame explosion follow-up. Richer explosion authoring is still future work.
- Single-cell player targeting is still only descriptive metadata; the framework does not yet enforce it at cast entry.
- Delayed execution restores spell, caster, targets, current cell, seed, and variable state across the delay.

Notes:
This scenario is useful because it exercises scheduled execution without needing complicated branching.

### Healing Touch

Name:
`Healing Touch`

Intent:
Target one allied pawn, show a restorative effect, then heal injuries or remove a negative condition.

Player Targeting:
Single allied pawn target.

Execution Steps:
1. Select a friendly pawn.
2. Play a healing effect.
3. Restore health or remove a harmful condition.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- A future heal action or condition-removal action
- Possibly `ApplyHediffActionDef` if healing is represented via authored hediff changes

Current Model Fit:
`Missing`

Current Runtime Fit:
`Missing`

Open Gaps:
- There is no heal action.
- There is no ally/foe or faction filtering in targeting or query definitions.
- `ConditionalActionDef` can evaluate authored conditions and branch into `thenActions` or `elseActions`, including target, pawn state, distance, line-of-sight, and random chance checks.

Notes:
This is a good reminder that offensive and restorative spells often need different domain primitives.

### Chain Lightning

Name:
`Chain Lightning`

Intent:
Hit one target, then jump to the nearest additional valid targets a limited number of times.

Player Targeting:
Single pawn target.

Execution Steps:
1. Strike the primary target.
2. Resolve the next valid target within a jump radius, preferring a forward direction from the prior jump.
3. Optionally split into multiple branches.
4. Repeat for a limited number of jumps or branches.
5. Avoid hitting the same target twice across the chain.

Expected Constructs:
- `SpellTargetingDef`
- `DamageActionDef`
- A future directional chain-target query or chain-targeting action
- `ConditionalActionDef`
- `SpellVariableStore` for jump count, visited targets, and branch state

Current Model Fit:
`Missing`

Current Runtime Fit:
`Missing`

Open Gaps:
- No loop or repeat construct.
- No authored chain or branch execution model.
- No variable-driven query filtering for visited targets or per-branch state.
- No authored way to express forward direction bias between jumps.
- No authored way to split into multiple branches and carry distinct chain state.
- `ConditionalActionWorker` evaluates authored conditions and supports nested branches. Remaining control-spell gaps are mostly dedicated status semantics and richer ongoing maintenance logic.

Notes:
Current implementation note:

- `MF_ChainLightning` now validates a purpose-built delayed branching chain primitive.
- It supports forward-biased hops, random branch count, repeated target hits, authored per-hit actions, and queued visual arcs from the previous node.

Remaining framework pressure:

- The chain support is still named and tuned as `ChainLightningActionDef`, but per-hit behavior is now authored through `onHitActions`.
- Future work should decide whether to rename/generalize the chain runtime for other spells.

### Blink

Name:
`Blink`

Intent:
Teleport the caster to a target cell with a short cast effect.

Player Targeting:
Single cell target, often within range and line of sight.

Execution Steps:
1. Select a destination cell.
2. Show departure and arrival effects.
3. Move the caster instantly to the destination.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- `TeleportActionDef`

Current Model Fit:
`Strong`

Current Runtime Fit:
`Partial`

Open Gaps:
- `TeleportActionDef` supports caster blink, target teleport, caster-adjacent rescue, random blink, and caster/subject position swaps.
- Destination validation can require standable, walkable, unoccupied, non-caster, and non-same-cell destinations.
- Blink still needs pathing/job continuity polish after relocation.
- Authored before/after action context is basic; there is no first-class "previous cell" value yet.

Notes:
Movement spells are a useful test for whether the framework is too damage-centric.

## D&D-Inspired Scenarios

### Hold Person

Name:
`Hold Person`

Intent:
Target a humanoid enemy and immobilize them, potentially for a duration, while they remain unable to act.

Player Targeting:
Single hostile humanoid pawn target.
Usually wants faction filtering, species filtering, and line of sight.

Execution Steps:
1. Select a valid humanoid enemy.
2. Play a binding or psychic restraint effect.
3. Apply a disabling status that prevents movement or actions.
4. Potentially maintain the effect for a duration or until another condition breaks it.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- `EffectActionDef`
- `ApplyHediffActionDef` or a future dedicated disable/stun action
- Possibly `DelayActionDef` or a future sustained-effect model

Current Model Fit:
`Partial`

Current Runtime Fit:
`Partial`

Open Gaps:
- There is no targeting support for humanoid-only, hostile-only, ally-only, foe-only, or faction-filtered selection.
- `ApplyHediffActionWorker` can adjust hediff severity on a pawn, but dedicated disable/control semantics are not modeled yet.
- The framework does not yet model sustained or concentration-style spell maintenance.
- There is no built-in way to express "target cannot act" beyond whatever a future hediff or disable action would do.

Notes:
`Hold Person` is a strong test of single-target control magic and whether status effects should be first-class actions instead of incidental hediff applications.

### Charm Person

Name:
`Charm Person`

Intent:
Target a humanoid and temporarily change their disposition so they become friendly or at least non-hostile.

Player Targeting:
Single humanoid pawn target, often hostile or neutral.
Usually wants social/faction validity rules and line of sight.

Execution Steps:
1. Select a valid humanoid target.
2. Play a subtle mental or enchantment effect.
3. Apply a charm state that changes the target's faction behavior, hostility, or social response.
4. Potentially expire the effect after a duration or under break conditions.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- `EffectActionDef`
- A future charm, mental-state, or faction-attitude action
- Possibly `DelayActionDef` or a future sustained-effect model

Current Model Fit:
`Missing`

Current Runtime Fit:
`Missing`

Open Gaps:
- There is no action for changing faction allegiance, hostility, or social state.
- There is no targeting support for humanoid-only, ally/foe subdivision, or attitude-based filtering.
- The framework has no explicit model for duration-based social control effects or break conditions.
- `ConditionalActionDef` can evaluate one-shot branch conditions, but charm still needs dedicated social-control semantics and ongoing break checks.

Notes:
`Charm Person` pushes the framework beyond combat effects into social and AI-state manipulation, which is an important design boundary to acknowledge early.

### Teleport

Name:
`Teleport`

Intent:
Move the caster or a chosen group instantly from one location to another, potentially across long range.

Player Targeting:
Usually a cell target, a destination marker, or a pair of source/destination selections.
May optionally target allies near the caster.

Execution Steps:
1. Choose a valid destination.
2. Optionally gather the caster and nearby companions.
3. Play departure effects.
4. Instantly relocate the affected pawns to the destination.
5. Play arrival effects.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- `EffectActionDef`
- `TeleportActionDef`
- Possibly `ApplyToTargetsActionDef` if teleporting multiple allied pawns

Current Model Fit:
`Partial`

Current Runtime Fit:
`Partial`
Current single-subject teleport modes cover ally relocation, rescue-to-caster-adjacent, forced enemy blink, random blink, and caster/subject swaps.

Open Gaps:
- The framework does not support multi-step targeting like source plus destination selection.
- Multi-pawn group teleport still needs target queries plus destination distribution.
- Safe-arrival validation is basic; random and caster-adjacent destinations may need richer scoring.
- Group teleport still needs ally/foe filters and placement rules for gathered pawns.

Notes:
`Teleport` is a bigger version of `Blink` and is useful for testing whether the framework can support spatial utility spells, not just combat spells.

### Wall of Stone

Name:
`Wall of Stone`

Intent:
Create a persistent line or barrier of solid wall segments that reshape the battlefield and block movement.

Player Targeting:
Usually a line, corridor, or a sequence of adjacent cells.
Often wants cell validity, obstruction rules, and max length constraints.

Execution Steps:
1. Choose a valid placement line or chain of cells.
2. Play a formation effect along the wall path.
3. Spawn persistent wall objects along the selected cells.
4. Leave those obstacles in the world for a duration or permanently.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- `EffectActionDef`
- A future spawn/place-structure action
- A future line or path targeting/query model

Current Model Fit:
`Missing`

Current Runtime Fit:
`Missing`

Open Gaps:
- There is no action for spawning persistent wall or structure objects.
- The framework does not model line, chain, or multi-cell placement targets.
- There is no validation for blocked cells, existing buildings, or terrain compatibility.
- Persistent created objects are outside the current action/runtime model.

Notes:
`Wall of Stone` is a great test for whether battlefield-construction spells belong in this framework or need a more world-object-oriented layer.

### Wall of Fire

Name:
`Wall of Fire`

Intent:
Create a persistent wall of flame that blocks space visually and damages creatures crossing or standing near it.

Player Targeting:
Usually a line or arc of cells.
Often wants orientation, length, and placement validation.

Execution Steps:
1. Choose a valid wall path.
2. Play a fire-formation effect along that path.
3. Spawn or represent a persistent burning wall zone.
4. Repeatedly damage or ignite pawns near or crossing that wall over time.

Expected Constructs:
- `SpellTargetingDef`
- `SequenceActionDef`
- `EffectActionDef`
- `ApplyToTargetsActionDef`
- `TargetsInRadiusQueryDef` or a future line/zone query model
- `DelayActionDef` or a future periodic/sustained area effect model
- A future spawn/place persistent effect action

Current Model Fit:
`Partial`

Current Runtime Fit:
`Missing`

Open Gaps:
- There is no action for placing a persistent fire wall or zone object in the world.
- The framework does not model line-shaped or wall-shaped targeting and area queries.
- Periodic area damage over time would need stronger delayed or repeating-effect support than a one-shot delay.
- `DamageActionWorker` and `ApplyHediffActionWorker` have basic direct target behavior, but wall-style periodic application still needs persistent zone/query support.
- The framework does not yet distinguish between "standing in the wall", "crossing the wall", and "near the wall" as authored query concepts.

Notes:
`Wall of Fire` fits the current model slightly better than `Wall of Stone` because it conceptually combines existing pieces like effects, delayed execution, and target application, but it still needs major new primitives for persistent zones and line geometry.

## Directional Chaining Proposal

`Chain Lightning` suggests a targeting pattern that is different from simple area queries or one-shot nearest-target selection.

Instead of treating chaining as a radius effect, a cleaner framework direction would be a dedicated sequential chain model with:

```text
ChainTargeting
- jumpRadius: float
- maxJumps: int
- excludeVisitedTargets: bool
- preferForwardDirection: bool
- allowSplit: bool
- maxBranches: int
- splitCount: int
```

The important design idea is:

- radius constrains the search space for each jump
- direction biases the next target choice so the chain propagates forward
- visited-target exclusion prevents bounce-back or repeated hits
- split support allows lightning to fork into multiple simultaneous branches

That means `Chain Lightning` is best understood as:

- a sequential targeting system
- using radius-limited next-target queries
- with directional propagation and optional branching

This is a stronger fit for lightning-style spells than trying to force them into ordinary area-of-effect targeting.

## Design Pressure Summary

The current design already has a promising backbone for authored spell graphs:

- Root spell targeting
- Ordered action sequences
- Delayed execution
- Per-target action application
- Radius-based target queries
- A place for execution state and variables

The scenarios above suggest the next missing primitives are:

1. Real worker implementations for damage, explosion, projectile launch, hediff application, and other core spell effects.
2. Stronger targeting semantics than raw `targetType` strings.
3. Target filters that distinguish pawn affiliation, such as all, ally, and foe, alongside species or faction restrictions.
4. A dedicated visual effect action so VFX is not forced through projectile or damage nodes.
5. Query filtering and condition evaluation that can use execution state.
6. A small set of domain actions beyond damage: heal, ignite, stun/disable, charm, teleport, and spawn/place effect objects.
7. Persistent world constructs such as spawned walls, rune objects, and hazardous area zones.
8. Richer spatial authoring for lines, paths, chained cells, and oriented wall-like areas.
9. A sustained or periodic-effect model for concentration-style spells and repeating hazards.
10. A directional chaining model for effects like `Chain Lightning`, including forward propagation, visited-target exclusion, and optional branching.

## Recommended Next Step

If we want to turn this document into an executable authoring target, `Firebolt` and `Fireball` are the best first pair:

- `Firebolt` validates the minimum single-target pipeline.
- `Fireball` validates point targeting, impact handling, area queries, and secondary effects.

Once those two work cleanly, we can revisit whether the current action/query model still feels natural before adding more specialized primitives.

## Concrete Examples

Concrete authored examples for the first two baseline spells live in [SpellDefExamples.xml](/d:/RimWorld/Mods/MagicFramework/Source/Documentation/SpellDefExamples.xml).

Those examples are intentionally written against the current `SpellDef` object model, including:

- nested action graphs
- dedicated spell effect actions
- requirements and costs
- projectile impact actions
- area target queries with explicit center selection and target filters for `Fireball`

They should be treated as design-reference defs for now, not yet as proof that the runtime can execute every node they describe.
