# Project Completed

This file tracks implemented framework and content milestones moved out of the active to-do list.

## Implemented Recently

- Dynamic MFVanilla spell scroll generation.
  Current state:
  - `SpellScrollDefGenerator` creates a `ThingDef` scroll for each learnable MFVanilla `SpellDef`
  - generated scrolls reuse `MFV_SpellScrollBase`, learn via `CompUseEffect_LearnSpell`, and inherit spell learning research prerequisites
  - static hand-authored scroll defs were removed from XML, leaving the abstract base as the single authoring point

- Basic summon/spawn primitive.
  Current state:
  - `SummonPawnActionDef` can spawn temporary pawns from authored `PawnKindDef` names
  - summoned pawns can be player-aligned
  - authored trainables are applied when supported
  - summoned animals can assign the caster as master after learning `Obedience`
  - follow-master settings can be authored for drafted and fieldwork behavior
  - summons are tracked by map runtime state and expire automatically
  Validation spell:
  - `MF_SummonDog`

- Sustained stat buff primitive.
  Current state:
  - `SustainedStatModifierActionDef` can maintain stat offsets/factors on a target
  - sustained buffs can break when the caster is downed/dead, target is invalid, range is lost, or line of sight is lost
  - break reasons are logged when maintained buffs end early
  - `onBreakActions` can run authored effects when maintenance breaks
  - optional max duration acts as a failsafe for maintained buffs
  - `statusCue` authoring can attach a visible hediff status cue
  Validation spell:
  - `MF_Might`, including `MF_Weakened` backlash on abrupt break

- First-class spell status cue primitive.
  Current state:
  - stat modifier actions can author a `statusCue` block with hediff, severity, and cleanup behavior
  - missing cues fall back to generic runtime text like "Affected by <spell name>" with a warning
  - legacy indicator fields still resolve for older authored spells
  Validation spells:
  - `MF_Haste`
  - `MF_Might`

- Clear stat/status effect primitive.
  Current state:
  - `ClearStatModifiersActionDef` can remove active framework stat modifiers and their status cues from an authored target
  - clear scope can target all framework effects, current caster, current spell, a specific spell, or a specific status hediff
  - sustained `onBreakActions` can be optionally triggered or suppressed when clearing

- Maintained force field primitive.
  Current state:
  - `ApplyForceFieldActionDef` can maintain a protective field on an ally or the caster
  - force fields can reduce incoming damage by an authored factor
  - force fields can fully absorb damage by spending caster mana per incoming damage point
  - shield-belt fleck and sound defs can be authored for impact feedback
  - optional ambient shield flecks can be authored with interval, scale, and color tint
  - sustained shield-bubble overlays can be authored with texture path, scale, and color tint, but the sample spells currently leave this disabled because the vanilla shield-belt bubble texture is not exposed as a normal content path
  - maintained shield status cues are shown through hediff indicators
  - force fields break when the caster is downed/dead, target is invalid, range is lost, or line of sight is lost
  - `onBreakActions` can run authored effects when maintenance breaks
  - debug gizmos can cleanly cancel maintained force fields without running break actions
  Validation spells:
  - `MF_ForceField`, reducing incoming damage without impact mana drain
  - `MF_ManaShield`, fully absorbing hits while the caster can pay mana

- Chain lightning primitive.
  Current state:
  - `ChainLightningActionDef` starts a delayed branching lightning chain from the current target
  - each pulse can run authored `onHitActions`, and queues 1-2 later jumps by default
  - target selection is radius-limited and forward-biased from the previous arc direction
  - queued pulses persist source and target cells so arcs continue from the previous node even if references change
  - repeated hits on the same target are allowed across the chain
  - vanilla electrical spark flecks provide a jagged trail, impact flash, and stun cue
  Validation spell:
  - `MF_ChainLightning`, using authored damage and random stun hit actions

- Custom spell gizmo icon support.
  Current state:
  - `SpellDef.gizmoIconPath` can point to a texture under `Textures`
  - debug spell gizmos use authored icons when available and fall back to vanilla command icons
  - project deployment includes `Textures/**`
  Validation spell:
  - `MF_Haste`

- Evenly distributed healing primitive.
  Current state:
  - `HealActionDef` heals pawn injuries by sharing a healing pool across current wounds
  - unused healing from closed wounds is redistributed to the remaining injuries
  - `RepeatActionDef` can reuse the same heal action over time through scheduled pulses
  Validation spells:
  - `MF_Heal`
  - `MF_Regeneration`

- Basic damage, hediff, and explosion primitives.
  Current state:
  - `DamageActionDef` applies authored damage amount, damage def, and armor penetration to the current target thing
  - `DamageActionDef` now supports extra damage entries, hit part selection, guilt policy, and combat log behavior
  - `ApplyHediffActionDef` adjusts an authored hediff severity on the current pawn target
  - `ApplyHediffActionDef` now supports body-part targeting, add/remove modes, duration policies, and checkIfAlreadyHas
  - `ExplosionActionDef` triggers a radial flame explosion at the current cell with authored radius and damage
  - `ExplosionActionDef` now supports damage type, fire chance, falloff, explosion sound/effect, spawned filth/things, and RimWorld explosion gas type
  - gas duration authoring is retained for compatibility, but RimWorld 1.6 explosion gas exposes type/amount/radius rather than lifetime control
  - `RemoveHediffActionDef` and worker for scheduled hediff removal
  Validation spells:
  - `MF_Firebolt` (now uses extra damage, guilt policy, combat log)
  - `MF_Fireball` (now uses full explosion options, hediff add modes)

- Modular procedural magic FX system.
  Current state:
  - `SpellDef` supports semantic FX metadata (`element`, `delivery`, `effectShape`) plus spell-specific override knobs
  - `MagicElementDef` and `MagicFXDef` provide Def-driven visual profiles that external mods can extend
  - `MagicFXResolver` composes a playable FX package from spell metadata, power tier, overrides, and fallback profiles
  - `MagicFXSpawner` plays resolved effecters, flecks, sounds, color overrides, and tier-scaled fleck intensity
  - `SpellExecutor` plays a metadata-driven cast-start FX automatically when procedural FX is enabled
  - `ProceduralFXActionDef` lets authored action trees request cast, impact, explosion, area pulse, and sustain FX without naming concrete RimWorld effect defs
  Remaining gaps:
  - delivery-specific projectile/trail generation is not yet automatic
  - existing damage, explosion, projectile, chain, force-field, and persistent-effect workers do not yet consume procedural FX as missing-field fallbacks
  - no custom beam renderer, decal placement, light pulse, or shader overlay layer yet
  Validation spells:
  - `MF_Firebolt`
  - `MF_Fireball`

- Terrain and weather-buildup action primitives.
  Current state:
  - `TerrainPatchActionDef` mutates cells in a radius around an authored center source
  - terrain patches can convert water-like terrain to authored replacement terrain such as `Ice`
  - terrain patches can add snow depth to non-water cells, with optional roof skipping
  - `MF_Freeze` combines procedural burst FX, terrain patching, and a lingering frost area marker
  Validation spells:
  - `MF_Freeze`

- Real projectile launch primitive.
  Current state:
  - `LaunchProjectileActionDef` spawns and launches vanilla RimWorld projectiles
  - projectile launch supports authored hit flags and friendly-fire prevention
  - `onImpactActions` are tracked by map runtime state and run after the projectile impacts, explodes, is destroyed, or times out
  - impact action context resolves to the projectile's last known cell so location-based effects can use the landing point
  Validation spells:
  - `MF_Firebolt`
  - `MF_Fireball`

- Conditional branching primitive.
  Current state:
  - `ConditionalActionDef` evaluates an authored `SpellConditionDef` and runs either `thenActions` or `elseActions`
  - conditionals can be nested because both branches are normal action lists
  - boolean composition supports `AllOfConditionDef`, `AnyOfConditionDef`, and `NotConditionDef`
  - target checks include target existence, pawn target, pawn affinity, thing category, downed/dead pawn state, hediff presence, and health below threshold
  - cell/spatial checks include occupied cell, distance bounds between authored cell sources, and line of sight between authored cell sources
  - `RandomChanceConditionDef` supports simple probabilistic branches
  Current cell sources:
  - current cell
  - current target cell
  - initial target cell
  - caster cell
  Current target sources:
  - current target
  - initial target
  - caster
  Remaining gaps:
  - `conditionLabel` is descriptive only
  - conditions do not yet read arbitrary execution variables
  - no query-count, mana/cooldown, weather/time, reachability/pathing, or line-crossing conditions yet

- Stone chunk movement primitive.
  Current state:
  - `MoveStoneChunksActionDef` moves nearby stone chunks one cell per pulse toward a resolved center point
  - `PersistentAreaZoneActionDef.pulseAtCenter` lets persistent area zones run child actions at their anchor point even when no pawn is inside
  Validation spell:
  - `MF_EarthCall`, a Geomancy spell that calls chunks toward a selected cell with level-scaled radius and duration
  - Earth Call now uses a wider radius and slower movement cadence for clearer testing: base radius 9, scalable pull radius up to 16, pulse interval 60 ticks
  - dev-mode pawns have a `Debug: Cast Earth Call` gizmo, with an authored-spell lookup and debug fallback
  Next validation:
  - smoke test with loose stone chunks around the target point on flat and hilly/mountainous maps
  - verify chunks step inward every pulse, respect occupied/invalid destination cells, and receive the Geomancy hilliness enhancement where applicable

- Water terrain targeting and Water's Embrace MVP.
  Current state:
  - `SpellTerrainUtility` centralizes water terrain detection for authored targeting and terrain actions
  - `SpellTargetingDef.requireWaterCell` blocks casts unless the selected cell is water-like terrain
  - player-facing and debug targeting validators both respect `requireWaterCell`
  - `MF_WatersEmbraceMarker` anchors the persistent water aura
  - `MF_Waterbound` provides the first visible hostile water restraint status
  - `MF_WatersEmbrace` is authored as an Aquamancy validation spell: target a water cell, create a hostile aura, and repeatedly slow foes with a timed Waterbound status cue
  - dev-mode pawns have a `Debug: Cast Water's Embrace` gizmo, with authored-spell lookup and debug fallback
  - repeated timed stat modifiers now refresh matching non-sustained caster/spell/target effects in place instead of removing and recreating duplicate records every pulse
  - status-cued timed stat modifiers are idempotent by status hediff: a second caster applying the same status refreshes/replaces the active effect rather than stacking it
  - routine timed stat modifier application logging is quieted so aura spells do not bury real errors in the log
  - scheduled hediff removals now refresh matching pawn/hediff/body-part removals, allowing aura pulses to keep timed hediffs alive cleanly
  - `ApplyHediffActionDef` supports coalesced progressive statuses with `preserveHigherSeverity`, `maxSeverity`, `scalableSeverity`, and `scalableMaxSeverity`
  - `MF_HeldUnder` adds a first nonlethal consciousness-pressure status with staged consciousness offsets as severity rises
  - Water's Embrace now applies both Waterbound movement restraint and progressive Held Under consciousness pressure while hostile pawns remain in the aura
  - overlapping Held Under applications refresh duration and can increase severity up to the cap, but do not reset stronger suffocation progress downward
  - `PersistentAreaZoneActionDef` supports hard-interruption concentration breaks through `requiresConcentration`, `breakWhenCasterDowned`, `breakWhenCasterStunned`, and `breakWhenCasterMentalState`
  - Water's Embrace now requires concentration and collapses if the caster is dead/invalid, downed, stunned, or in a mental state
  - Water's Embrace intentionally has no caster-distance or line-of-sight leash yet; those need UI/feedback before use
  - concentration-based area zones are treated as maintained spells by known-spell and debug gizmos, so Water's Embrace can be toggled off by the caster
  - `MovePawnTowardPointActionDef` pulls the current pawn target toward an authored point while respecting walkability/standability options
  - Water's Embrace now applies an undertow pull toward the enchanted water each pulse so restrained pawns do not simply walk out of the aura
  - `SpellDrowningHediff` adds lethal drowning behavior only at the highest Held Under severity, with a 600-tick downed grace period
  - persistent area zones now preserve the original cast's spell power value and tier across pulses, so level-scaled aura actions resolve correctly
  - Water's Embrace Held Under severity gain and cap now scale with caster power; the retuned curve starts gently but lets level-20 casters reach the heavy 75% consciousness-pressure stage after sustained exposure
  - Held Under severity 0.65 applies heavy -75% consciousness pressure, while lethal drowning/grace starts at severity 0.9
  - if Water's Embrace ends during the grace period and Held Under expires, the pawn survives
  Next validation:
  - smoke test that Water's Embrace can only target water, marsh, mud, or other waterBodyType terrain
  - confirm hostile pawns in the aura receive and then cleanly lose Waterbound after the aura ends or they leave pulse range
  - confirm hostile pawns receive Held Under, show escalating nonlethal consciousness reduction while exposed, and recover after leaving the aura
  - confirm Water's Embrace collapses when the caster is downed, stunned, or mentally broken
  - confirm the Water's Embrace gizmo switches to cancel while active and cleanly removes the aura when toggled off
  - confirm undertow pull keeps hostile pawns near the water without pushing them into invalid/deep-water cells
  - confirm high-level Water's Embrace can escalate Held Under to the 0.65 heavy-pressure stage and, with continued exposure, into the 0.9 lethal grace stage
  - confirm cancelling or breaking concentration during the grace period lets the pawn survive once Held Under expires
  - confirm rain activates `MFV_RainEmpowersAquamancy` for the spell through the enhancement diagnostics gizmo
  Next implementation:
  - defer manual release/capture, rescue/escape jobs, water-source scaling, and richer feedback/visuals for later reconsideration

