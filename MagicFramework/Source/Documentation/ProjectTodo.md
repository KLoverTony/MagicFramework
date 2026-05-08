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


## Completed Work

Completed implementation notes have been moved to [ProjectCompleted.md](ProjectCompleted.md).

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

- Structured spell metadata, learning requirements, and enhancement synergies.
  Current state:
  - `SpellDef` now has additive `meta`, `learning`, and `casting` grouped properties
  - `SpellElementDef`, `SpellDomainDef`, `SpellDisciplineDef`, and `SpellTagDef` provide moddable XML taxonomy
  - `SpellMetadataUtility` provides null-safe metadata query helpers by def reference and defName
  - MFVanilla defines initial taxonomy XML in `Defs/SpellMetadataDefs/MFV_SpellMetadataDefs.xml`
  - MFVanilla validation spells now have metadata and first-pass learning blocks where their element/domain/research fit is clear
  - `SpellRequirementWorker` supports quiet default `CanLearn` and `CanCast` checks
  - `ArcaneGiftRequirementWorker` and `CasterLevelRequirementWorker` apply to learning and casting
  - `SpellRequirementUtility` centralizes learning checks, research prerequisites, known-spell checks, and casting requirement evaluation
  - `SpellCastValidator` and player-facing known-spell gizmos now use the shared casting requirement utility
  - `SpellCostProcessor` supports grouped `casting.costs` while falling back to legacy top-level `costs`
  - MFVanilla scroll learning now honors spell-level `learning` requirements in addition to scroll-specific requirements
  - `SpellEnhancementRuleDef` defines metadata and game-condition based spell modifier rules
  - `SpellEnhancementUtility` matches active rules and aggregates `SpellModifierSet` factors
  - mana requirements, mana costs, and cooldown costs now consume enhancement modifiers centrally
  - MFVanilla defines `MFV_SolarFlareEmpowersFireMagic`, which targets fire spells during `SolarFlare`
  - enhancement rules can also match active weather and map hilliness
  - MFVanilla defines `MFV_RainEmpowersAquamancy` and `MFV_HillsEmpowerGeomancy`
  - selected controllable pawns with Arcane Gift now show a mana gizmo with current/max mana and percent
  - dev-mode pawns now get a `Debug: Spell Enhancements` gizmo that logs active enhancement rules and final modifier factors for a selected spell
  Remaining work:
  - wire enhancement `damageFactor` into `DamageActionWorker` and `ExplosionActionWorker`
  - wire `radiusFactor` and `durationFactor` into the safest central calculation points for scalable spell values
  - add more sample enhancement rules once diagnostics are comfortable, such as rain weakening fire, eclipse empowering death/shadow, aurora empowering arcane/spirit, and wind empowering Aeromancy after scalar support exists
  - smoke test `MF_WatersEmbrace` so rain enhancement rules have direct in-game test coverage
  - add a dedicated consciousness/drowning hediff primitive once the Waterbound aura is stable
  - decide whether domain `defaultResearchPrerequisite` should ever be used as a fallback when spell-level prerequisites are empty
  - eventually migrate authored spells from top-level `requirements`/`costs` to grouped `casting.requirements`/`casting.costs`, with compatibility retained until intentionally removed
  - consider a player-facing spell details UI that can display metadata, learning requirements, cost, cooldown, and active modifiers without relying on dev logs
  Compatibility notes:
  - do not remove existing top-level `SpellDef.requirements` or `SpellDef.costs` until compatibility migration is deliberate
  - do not replace legacy procedural FX fields (`element`, `delivery`, `effectShape`) until FX fallback behavior is designed
  - use moddable `Def` references rather than enums for elements, domains, disciplines, and tags
  - keep metadata separate from requirements
  - keep learning requirements separate from casting requirements and casting costs
  - keep enhancement rules separate from spell definitions and avoid hardcoded spell-specific logic
  - treat missing metadata and null lists as valid empty data

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

- Plan MFVanilla arcane ink production chain for scrollmaking.
  Goal:
  - Scroll recipes should eventually require a produced ink resource in addition to papyrus/parchment.
  - Early/common scrolls can use a baseline `MFV_ArcaneInk`.
  - Later powerful scrolls, forbidden/infernal motifs, or domain-specific scrolls can branch into specialized inks with different ingredients.
  Proposed content flow:
  - Add a growable exotic herb crop, likely labeled `exotic herbs` for generic fantasy usability, with optional flavor name `belladonna` if we want a darker/poisonous identity.
  - Add harvested item `MFV_ExoticHerbs` or `MFV_Belladonna`, used as the organic base for ink.
  - Add an ink-making station, a production table that takes harvested herbs plus a small amount of silver and produces `MFV_ArcaneInk`.
  - Add a work giver for the ink station, like the parchmentery and scribing table work givers.
  - Update generated scroll recipes so each scroll requires one writing material plus arcane ink.
  - Keep current paper rule: tier 1-2 scrolls use papyrus or parchment, tier 3+ scrolls use parchment.
  - Add future generator hooks for motif/domain ink rules, for example infernal spells requiring infernal ink instead of generic arcane ink.
  Defs to add or update:
  - `ThingDef` plant crop, probably under `Defs/ThingDefs/Plants/`.
  - harvested herb `ThingDef`, probably under `Defs/ThingDefs/Items/`.
  - arcane ink `ThingDef`, probably under `Defs/ThingDefs/Items/`.
  - ink station `ThingDef`, probably in `MFV_ArcaneResearchBenches.xml` unless production tables are split into a separate file.
  - ink recipe `RecipeDef`, probably under `Defs/RecipeDefs/`.
  - ink station `WorkGiverDef` in `MFV_WorkGivers.xml`.
  - `GenerateSpellScrollDefs.ps1` recipe generation to add ink ingredients.
  Balancing placeholders:
  - `MFV_MakeArcaneInk`: 8-12 exotic herbs plus 3-5 silver -> 1 arcane ink.
  - Scroll recipes: 1 papyrus/parchment plus 1 arcane ink -> 1 scroll.
  - Ink station research prerequisite: `MFV_Alchemy` or `MFV_RunicInscription`; choose based on whether ink feels more reagent/alchemy or scrollcraft.
  - Crop research prerequisite: likely `MFV_Papyrus` or `MFV_Alchemy`.
  Texture requests:
  - Crop plant textures, no directional variants:
    - `ExoticHerbs_Immature.png`
    - `ExoticHerbs_Growing.png`
    - `ExoticHerbs_Mature.png`
    - optional `ExoticHerbs_Harvestable.png` if we want a distinct final visible stage
  - Harvested herb item texture, no directional variants:
    - `Exotic herbs.png` or `Belladonna.png`
  - Arcane ink item texture, no directional variants:
    - `Arcane ink.png`
  - Ink-making station, directional variants required if `Graphic_Multi`:
    - `Ink making station.png`
    - `Ink making station_north.png`
    - `Ink making station_south.png`
    - `Ink making station_east.png`
    - optional `Ink making station_west.png` only if the east texture is not acceptable mirrored
  - If the station is visually symmetric, we can use a single texture with `Graphic_Single`, but production tables usually look better with north/south/east variants.
  Open naming decision:
  - Choose `exotic herbs` for broader use across inks, potions, and reagents, or `belladonna` for a stronger occult/poison identity.

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

