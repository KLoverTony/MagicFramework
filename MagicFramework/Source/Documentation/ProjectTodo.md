# Project To-Do

This file tracks framework work that is incomplete, rough, or intentionally deferred.

Quick notes (if you see these while reviewing, please update, reorganize appropriately, and expand accordingly. Feel free to add interesting ideas here if you have any)
  1. Summon skeleton ritual should not allow targeting of non-humanlike corpses which excludes undead and animals
  2. The Ossanith bone box ritual should not allow targeting of non-humanlike corpses though pets might be acceptable if possible.
  3. Celestial events such as solar eclipse and auroras are opportunities for interesting in game events and effects...
  4. Some summon undead rituals should preserve a sort of pseudo relationship list for family members in particular. However, they can't be ordinary relationships because it would cause strange and inappropriate behavior if an undead skeleton tries to resume a relationship with their husband/wife for example. This effect also needs to apply to the related pawns as well.
  5. Need to build custom wall atlas and auto-joining function
  6. Need to build torch sconces as well as arcane torches to replace electric lighting.
  7. Area effect fire spells should melt snow / ice
  8. The freeze spell produces snow / ice but should have a thaw effect when the spell ends.
  9. Rituals dialog box could be prettier. Can we include an avatar selection instead of checkbox selector? Perhaps something similar to wedding dialog.


End of quick notes...


## Implemented Recently

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
  - the debug fallback Freeze spell combines procedural burst FX, terrain patching, and a lingering frost area marker
  Validation spells:
  - `MF_Freeze` debug fallback

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

## Current Priority

- Refine `TeleportActionDef` / `Blink Step` behavior.
  Current state:
  - teleport is positionally stable, but blinking can still interrupt pawn behavior awkwardly after relocation
  - `TeleportActionDef` can teleport the caster, current target pawn, or initial target pawn
  - authored destination modes include current cell, current target cell, initial target cell, caster cell, caster-adjacent cell, random near subject, random near caster, random near current cell, and random near initial target
  - `swapWithCaster` supports swapping the caster and target/subject positions
  - random blink controls include radius, minimum radius, and search attempts
  - destination validation can require standable, walkable, unoccupied, non-caster, and non-same-cell destinations
  Authoring coverage:
  - teleport ally: `subjectSource=CurrentTarget`, `destinationSource=CurrentCell`
  - forced enemy blink: `subjectSource=CurrentTarget`, `destinationSource=RandomCellNearSubject`
  - swap positions: `subjectSource=CurrentTarget`, `swapWithCaster=true`
  - random blink: `subjectSource=Caster`, `destinationSource=RandomCellNearCaster`
  - rescue teleport: `subjectSource=CurrentTarget`, `destinationSource=CasterAdjacentCell`
  Known symptoms:
  - drafted pawns can lose clean movement continuity
  - moving pawns can feel briefly staggered or path-confused after arrival
  Remaining gaps:
  - preserve pathing/job continuity without reintroducing snap-back to the original cell
  - add authored validation spells for the new teleport modes
  - richer safe-arrival scoring for random/caster-adjacent destinations

- Add channeling / sustained spell primitives.
  Target capabilities:
  - maintained stat buffs
  - concentration-based beams
  - maintained shields
  - caster-tethered walls or zones
  - sustained drain or support effects
  - cancellation when caster is interrupted, downed, or loses line of effect
  Current state:
  - maintained stat buffs have a first-pass primitive
  - maintained stat buffs support a first-pass `onBreak` lifecycle hook
  - maintained force fields have a first-pass primitive with damage reduction, mana absorption, status cues, and break hooks
  - debug gizmos can cleanly cancel selected maintained spells and show cooldown-disabled cast buttons
  Remaining gaps:
  - explicit interruption detection beyond downed/dead/invalid/range/line-of-sight breaks
  - sustained resource drain while maintained
  - action pulses while maintained
  - non-debug / player-facing cancel/toggle UX

- Expand target filters and target-query expressiveness.
  Useful next queries/filters:
  - nearest valid foe
  - nearest valid ally
  - lowest-health target
  - highest-threat target
  - all pawns in radius with optional exclusions
  - line-intersection and crossing checks
  - exclude already-hit or already-chained targets
  - target count limits and deterministic ordering
  Current state:
  - `ChainLightningActionDef` has purpose-built delayed forward-biased chain targeting

- Formalize cleanup / lifecycle hooks for persistent spell state.
  Useful hooks:
  - `onCreate`
  - `onPulse`
  - `onTrigger`
  - `onExpire`
  - `onRemove`
  - `onBreak`
  Goal: make persistent markers, traps, walls, zones, and future summons behave consistently

## Framework Follow-Ups

- Add structured spell metadata, learning requirements, and enhancement synergies.
  Goal:
  - expand `SpellDef` with additive, grouped metadata and learning/casting configuration
  - keep existing spell behavior loading unchanged when metadata is absent
  - preserve the current split between cast validation requirements and post-validation costs
  - support future systems such as spell books, research unlocks, AI spell choice, equipment bonuses, and weather/celestial spell modifiers
  Current state:
  - Batch A foundation is implemented in code
  - `SpellDef` now has additive `meta`, `learning`, and `casting` grouped properties
  - `SpellMetaProperties`, `SpellLearningProperties`, and `SpellCastingProperties` define the first-pass grouped data shape
  - `SpellElementDef`, `SpellDomainDef`, `SpellDisciplineDef`, and `SpellTagDef` provide moddable taxonomy defs
  - `SpellMetadataUtility` provides null-safe metadata query helpers by def reference and defName
  - MFVanilla defines initial taxonomy XML in `Defs/SpellMetadataDefs/MFV_SpellMetadataDefs.xml`
  - validation spell metadata has been added to `MF_Firebolt`, `MF_Heal`, and `MF_BlinkStep`
  - `dotnet build Source.sln --no-restore -p:DeployToModAssemblies=true` passes
  Guardrails:
  - do not remove existing top-level `SpellDef.requirements` or `SpellDef.costs` until compatibility migration is deliberate
  - do not replace legacy procedural FX fields (`element`, `delivery`, `effectShape`) in the first pass
  - use moddable `Def` references rather than enums for elements, domains, disciplines, and tags
  - keep metadata separate from requirements
  - keep learning requirements separate from casting requirements and casting costs
  - keep enhancement rules separate from spell definitions and avoid hardcoded spell-specific logic
  - treat missing metadata and null lists as valid empty data
  - update only a small number of validation spells first
  Batch A, safe metadata foundation:
  - completed first pass
  - add `SpellMetaProperties` with `tier`, `elements`, `domains`, `disciplines`, and `tags`
  - add `SpellLearningProperties` with `canBeLearned`, visibility flags, `researchPrerequisites`, and learning `requirements`
  - add `SpellCastingProperties` with casting `requirements` and casting `costs`
  - add grouped `meta`, `learning`, and `casting` fields to `SpellDef` with null protection in a lifecycle hook and/or utility methods
  - add taxonomy defs: `SpellElementDef`, `SpellDomainDef`, `SpellDisciplineDef`, and `SpellTagDef`
  - prefer clear defName prefixes such as `MF_Element_Fire`, `MF_Domain_Pyromancy`, `MF_Discipline_Combat`, and `MF_Tag_Projectile`
  - add MFVanilla taxonomy XML under a metadata-focused folder such as `Defs/SpellMetadataDefs/MFV_SpellMetadataDefs.xml`
  - add query helpers such as `HasElement`, `HasDomain`, `HasDiscipline`, and `HasTag`
  Acceptance for Batch A:
  - project compiles
  - existing spells still load without grouped metadata
  - MFVanilla can define new spell elements, domains, disciplines, and tags through XML
  - metadata helper calls are null-safe
  Batch B, learning and casting requirements:
  - extend `SpellRequirementWorker` with quiet default `CanLearn` and `CanCast` methods
  - keep `ArcaneGiftRequirementWorker` and `CasterLevelRequirementWorker` applicable to both learning and casting
  - keep `ManaRequirementWorker` and `CooldownRequirementWorker` casting-only by default
  - add or expand `SpellRequirementUtility` with `CanLearnSpell(Pawn pawn, SpellDef spell, out string reason)` and `CanCastSpell(SpellContext context, SpellDef spell, out string reason)`
  - have learning checks cover `learning.canBeLearned`, completed `learning.researchPrerequisites`, and `learning.requirements`
  - have casting checks cover existing known-spell runtime state, legacy top-level requirements, and new `casting.requirements`
  - have casting cost application support both legacy top-level costs and new `casting.costs`, with duplicate spending avoided during migration
  - use existing `SpellRuntimeGameComponent.KnowsSpell`, `LearnSpell`, `ForgetSpell`, and `GetKnownSpells`
  - update only 1-3 validation spells first, such as `MF_Firebolt`, `MF_Heal`, and `MF_BlinkStep`
  Acceptance for Batch B:
  - a spell can require Arcane Gift to learn
  - a spell can require completed research to learn
  - a spell can require mana and cooldown to cast
  - casting still spends mana and starts cooldown after successful validation only
  - failure reasons are returned cleanly for unmet learning and casting requirements
  Batch C, enhancement synergies:
  - add `SpellEnhancementRuleDef` targeting affected elements, domains, disciplines, required tags, and active game conditions
  - add `SpellModifierSet` with factors for damage, radius, duration, mana cost, and cooldown
  - add `SpellEnhancementUtility.GetActiveRules(SpellDef spell, Map map)` and `GetModifiers(SpellContext context)`
  - document matching semantics: empty affected lists match any spell, affected element/domain/discipline lists match any listed def, required tags must all be present, and active conditions match any listed condition
  - start with centralized mana/cooldown modifier support if straightforward, then add damage/radius/duration integration only after identifying the safest central calculation points
  - avoid scattering solar flare, fire, or pyromancy checks into individual spell workers
  Acceptance for Batch C:
  - a `SpellEnhancementRuleDef` can target fire spells by metadata
  - the rule can activate during a map condition such as `SolarFlare`
  - the framework can calculate an aggregated modifier set for a spell context
  - at least one centralized spell value, preferably mana cost or cooldown first, can be modified through the enhancement system

- Improve displacement destination resolution around obstacles.
  Current push/pull logic is intentionally simple and may need smarter fallback cell selection for diagonal or blocked paths.

- Extend summon/spawn primitives beyond temporary trained creatures.
  Remaining capabilities:
  - spawn temporary objects
  - spawn hazards, wards, totems, beacons
  - support non-animal or untrainable summons with a different control model
  - optional summon arrival/expiry lifecycle actions
  - clearer UI/status indication for temporary summons if needed

- Design spell scaling / spell power primitives.
  Core idea:
  - compute a runtime `SpellPower` value from the caster and cast context
  - let authored actions opt into typed scaling rules
  - avoid one generic catch-all scalar array until the actual use cases are clearer
  Current state:
  - `SpellDef.power` can define an authored base power value
  - `casterLevelFactor` can add the caster's debug caster level to spell power
  - optional `casterSkillDef` and `casterSkillFactor` can add a pawn skill contribution to spell power
  - authored power tiers are resolved from minimum power thresholds into `SpellContext.power.tier`
  - delayed actions and projectile impact actions preserve the computed power value and tier
  - `ScalableFloatDef` can compute `baseValue + power * perPower`, with optional min/max clamps
  - `DamageActionDef` supports scalable damage amount and armor penetration
  - `HealActionDef` supports scalable healing amount
  - `ExplosionActionDef` supports scalable radius and damage amount
  - `SpellTargetingDef` supports scalable targeting range
  - `SpawnThingActionDef` supports scalable stack count and tiered thing-def selection
  - duration-like fields can scale on repeat actions, persistent effects, wall zones, area zones, summons, spawned things, timed stat buffs, sustained stat buffs, and force fields
  - `PowerTierConditionDef` and `SpellPowerConditionDef` allow conditional branching on computed power
  - dev-mode pawns get a `Debug: Caster Level` gizmo that cycles levels `0 -> 1 -> 3 -> 5 -> 10 -> 20 -> 0`
  - dev-mode pawns get a built-in `Debug: Cast Scaling Bolt` spell that always uses scalable damage for testing, even when authored XML spells are loaded
  Validation spells:
  - `MF_Firebolt` scales range and damage from debug caster level
  - `MF_CreateFood` scales meal quality by power tier, meal quantity by power value, and conjured-food lifetime by power value
  - `MF_Haste` scales timed buff duration by power value
  Example scalable damage field:
  - `<scalableAmount><baseValue>10</baseValue><perPower>1.5</perPower><max>40</max></scalableAmount>`
  Example power definition:
  - `<power><baseValue>2</baseValue><casterLevelFactor>1</casterLevelFactor><casterSkillDef>Intellectual</casterSkillDef><casterSkillFactor>0.5</casterSkillFactor></power>`
  Open design questions:
  - what counts as caster level or spell power
  - whether additional power should come from traits, equipment, mana invested, ritual quality, or a future magic progression system
  - whether scaling should be linear, tiered, capped, randomized, or context-sensitive
  Candidate typed primitives:
  - extend `ScalableFloatDef` to cooldown, mana cost, and target count
  - tiered projectile/effect selection for upgraded visual or mechanical outcomes
  - scalable target count for chains, bursts, and multi-target spells
  - scalable area shape/radius for fields, walls, and explosions
  Example tiered progression:
  - low power `Create Food` creates `MealSimple`
  - medium power creates `MealFine`
  - high power creates `MealLavish`
  Example continuous progression:
  - `Firebolt` damage increases by a bounded amount per spell power
  - `Fireball` radius or damage increases up to an authored maximum
  Implementation note:
  - continue adding explicit typed scaling support on individual action defs as use cases appear
  - keep `SpellContext` as the place where computed spell power eventually lives
  - add debug/test hooks before tying scaling to a real progression system

- Add richer buff/debuff primitives beyond direct stat modifiers.
  Candidates:
  - maintained stat buffs
  - first-class visible status cues
  - clear/remove active framework status effects
  - generic timed status effects
  - stat offsets and factors across multiple stats
  - capacity modifiers
  - accuracy, dodge, armor, casting-speed modifiers
  Current state:
  - stat modifier buffs can display authored or generic `statusCue` hediff indicators

- Decide whether some common status effects should remain generic hediff applications or become dedicated primitives.
  Candidates:
  - ignite
  - stun
  - charm
  - silence
  - root / immobilize

- Improve projectile support.
  Current projectile action now launches real RimWorld projectiles and delays impact actions until projectile resolution. Remaining useful improvements:
  - exact hit-thing context for misses, cover interception, and shield blocking
  - richer authored launch origins and arcing/overhead policy
  - optional custom projectile classes for spell-only visuals or special impact callbacks

- Generalize delayed branching chain support.
  Current chain lightning support is intentionally purpose-built. Future chain spells may want:
  - reusable delayed chain state
  - authored per-hop action lists
  - visited-target policies shared with target queries
  - deterministic seeded random branching
  - richer beam/arc visuals between targets

- Continue persistent visual support as a first-class framework feature.
  Future needs:
  - find or ship a loadable tight personal shield texture for maintained shield overlays
  - prototype a cloned, hidden magic shield-belt apparel item to borrow vanilla personal shield visuals
  - evaluate a temporary-equip spell action for spawned apparel and cleanup on expire/break/death/drop
  - keep vanilla shield-belt projectile mechanics separate from framework mana/damage-reduction shields unless explicitly authored
  - validate sustained overlay draw order and scale against vanilla shield belt visuals
  - calmer sustained ambient visuals beyond repeated flecks
  - optional multi-point visual patterns
  - persistent sounds
  - visual states that change on arm/trigger/expire

## Content / Runtime Polish

- Add explicit Harmony dependency metadata in `About/About.xml` so load order is enforced by mod metadata.
  Current state:
  - MagicFramework uses Harmony patches internally and references Harmony in source, but `About/About.xml` does not yet declare a Harmony package dependency.

- Keep debug fallback spells lightweight. Authored validation spell XML has moved to MFVanilla content.

- Add custom gizmo icons for validation spells.
  Current state:
  - authored validation spells and matching PNGs now live in MFVanilla under `Defs/SpellDefs` and `Textures/UI/Gizmos/Spells`
  - currently wired: `MF_BlinkStep`, `MF_ChainLightning`, `MF_CreateFood`, `MF_DelayedBlastRune`, `MF_Disintegrate`, `MF_Fireball`, `MF_Firebolt`, `MF_FlameField`, `MF_ForceField`, `MF_ForcePull`, `MF_ForcePush`, `MF_Haste`, `MF_Heal`, `MF_ManaShield`, `MF_Might`, `MF_Regeneration`, `MF_RuneTrap`, `MF_SummonDog`, and `MF_WallOfFire`
  Remaining useful icons:
  - future validation spells as they are added

- Add a small suite of validation spells specifically for framework features:
  - conditional-branch spell
  - persistent zone spell
  - sustained/channel spell (`MF_Might` covers maintained stat buff behavior)
  - maintained shield spell (`MF_ForceField` and `MF_ManaShield` cover first-pass protective field behavior)
  - delayed branching chain spell (`MF_ChainLightning` covers purpose-built chain behavior)
  - direct heal / healing-over-time spells (`MF_Heal` and `MF_Regeneration` cover first-pass wound recovery behavior)
  - teleport / displacement regression spell (`MF_BlinkStep` covers the current basic teleport path; add dedicated swap/rescue/enemy-blink validation spells)

- Review caster-self-affect policy on persistent effects.
  Current behavior is authored per spell, but it is worth documenting clear conventions for:
  - self-safe zones
  - self-damaging zones
  - ally-safe vs neutral hazards

- Add better logging toggles so verbose debug output can be enabled selectively by subsystem.
  Good candidates:
  - execution
  - triggers
  - persistent effects
  - wall zones
  - area zones
  - stat modifiers
  - displacement

## Nice-to-Have

- Investigate whether `Wall of Fire` should eventually integrate with real RimWorld fire objects instead of remaining a custom magical hazard.

- Consider persistent world-object representations for more spells, not just traps and walls.

- Write a full MagicFramework spell design guide.
  Target coverage:
  - top-level `SpellDef` fields, including label, description, icon, range, cast time, targeting, requirements, costs, power, and action tree structure
  - targeting options, pawn-affinity rules, self-target policy, line-of-sight behavior, and category filters
  - requirement and cost authoring, including mana and cooldown conventions
  - action options and required fields for damage, healing, hediffs, explosions, projectiles, delays, repeats, triggers, persistent effects, zones, summons, spawned things, terrain patches, teleport/displacement, stat modifiers, sustained effects, force fields, conditionals, and target queries
  - replacement/lifecycle policy, including default `replaceExistingForCaster` behavior and when to opt into stacking
  - scaling/power authoring with `ScalableFloatDef`, power tiers, and validation expectations
  - procedural FX metadata and explicit visual/sound action options
  - common spell patterns:
  - projectile spell
  - delayed rune
  - triggered trap
  - wall
  - aura / area field
  - displacement spell
  - buff / debuff spell
  - design requirements for safe validation spells, including target safety, cleanup behavior, debug logging expectations, and regression coverage
