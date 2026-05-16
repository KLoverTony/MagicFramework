# MagicFramework Roadmap

This file tracks active, deferred, and exploratory work for MagicFramework and its first-party content mods. Completed implementation notes live in [ProjectCompleted.md](ProjectCompleted.md).

Complexity key:
- `XS`: XML/content tweak, docs, or narrow configuration.
- `S`: contained implementation in one subsystem.
- `M`: several files or a new authored primitive with validation content.
- `L`: cross-system feature with save/load, UI, and testing risk.
- `XL`: major framework or content pillar.

Priority key:
- `P0`: immediate stability or correctness.
- `P1`: near-term release, content, or framework work.
- `P2`: content/runtime polish.
- `P3`: nice-to-have, experiments, or later expansion.

## Priority Index

## Post-Release Direction

MagicFramework and MFVanilla now have a first public edition. The next phase should treat the framework as released software: keep the published surface stable, round out MFVanilla into a satisfying first-party content mod, and move AeternusFaith from promising prototype to first-edition release candidate.

Near-term priority order:
1. Protect the released mods: fix startup, save/load, cleanup, bad XML, broken research/recipes, and first-party spell usability issues immediately.
2. Finish MFVanilla's production and progression loop: make Arcane Ink, scrolls, research benches, gemstones, herbs, parchment, scribing, and spell learning feel coherent from early colony through advanced magic.
3. Add only the framework features that directly improve MFVanilla or unblock AeternusFaith first-edition content.
4. Prepare AeternusFaith for first release with focused ritual UX, undead cleanup, faith content gates, and release packaging.
5. Keep larger framework expansions, AI casting, equipment systems, custom wall atlas work, and deep event systems behind the first-edition content goals.

Current release hygiene checklist:
- Keep MagicFramework and MFVanilla Workshop metadata, dependency metadata, and local deployed assemblies synchronized after changes.
- Run clean builds for changed assemblies before copying to the local RimWorld mod folders.
- Smoke test the first-party validation spells that cover the stable authoring surface: projectile, explosion, heal, regeneration, chain, teleport/displacement, maintained shield, sustained stat buff, area aura, terrain patch, summon, rune/trap, and generated spell details.
- Smoke test MFVanilla progression after production-tree changes: research gates, workbenches, Arcane Ink, parchment/papyrus, gemstones, generated scroll recipes, scroll learning, spell details, and settings.
- Smoke test AeternusFaith before release: ideology load, ritual circles/rooms, lecterns, skeleton rite, ossuary rite, spectre rite, undead cleanup, save/load, and small-screen ritual dialogs.
- Capture any remaining known issues as post-release notes unless they affect startup, save/load, cleanup, or first-party usability.

| ID | Priority | Complexity | Area | Task |
| --- | --- | --- | --- | --- |
| MF-037 | P1 | M | MFVanilla | Round out production trees, resource flow, and research pacing. |
| MF-038 | P1 | M | MFVanilla | Add a few high-value content features that make production and progression feel complete. |
| MF-039 | P1 | M | AeternusFaith | Prepare the first-edition release candidate and packaging checklist. |
| MF-019 | P1 | M | AeternusFaith | Improve ritual dialogs with pawn avatars and clearer invalid-state feedback. |
| MF-033 | P1 | S | AeternusFaith | Shroudhymn summoned spectres should despawn cleanly. |
| MF-034 | P1 | S | AeternusFaith | Add corresponding lecterns as placeable objects in ritual circle action gizmos. |
| MF-035 | P1 | S | AeternusFaith | Ritual summons should only be performable by Bonewrights (ideology role). |
| MF-020 | P2 | M | AeternusFaith | Tie psychic sensitivity into haunting effects. |
| MF-009 | P2 | M | Spell power | Continue typed spell power and scaling support where content needs it. |
| MF-010 | P2 | M | Buffs | Add richer buff/debuff primitives where content proves repeated authoring pain. |
| MF-014 | P2 | M | Summons | Extend summon/spawn primitives beyond temporary trained creatures. |
| MF-016 | P2 | S | UI | Polish generated spell details and active modifier presentation. |
| MF-031 | P2 | L | Docs | Write a full MagicFramework spell design guide from the released surface. |
| MF-021 | P3 | L | AeternusFaith | Add pseudo-relationship memory for raised undead. |
| MF-012 | P3 | L | Chains | Generalize delayed branching chain support. |
| MF-023 | P3 | L | AeternusFaith | Build custom wall atlas and auto-joining support. |
| MF-024 | P3 | L | Content | Start magic tools and weapons framework/content. |
| MF-025 | P3 | M | Events | Add celestial event enhancement rules and gameplay hooks. |
| MF-026 | P3 | M | Visuals | Continue persistent visual support. |
| MF-027 | P3 | S | Content | Add future validation spell gizmo icons as spells are added. |
| MF-029 | P3 | M | Fire | Investigate real RimWorld fire integration for `Wall of Fire`. |
| MF-030 | P3 | M | World state | Consider persistent world-object representations for more spells. |
| MF-032 | P3 | S | Compatibility | Gate mechanisms that would not be supported in multiplayer mods. |
| MF-036 | P3 | L | AI | Review spells and evaluate if the hostile pawn AI can be empowered to use magic spells they have available. |
| MF-040 | P1 | S | Content | Introduce a first launch splash screen and include important notes |
| MF-041 | P3 | M | Content | Introduce an Arcane Forge production item |

## P1 Release And Content Priorities

### MF-037 MFVanilla Production And Progression

Goal: make MFVanilla feel like a complete first-party expansion instead of a spell validation pack.

Current state:
- Initial production-chain review completed.
- Papyrus is now the early plant-fiber sheet recipe: wood -> papyrus at the papyrus press.
- Parchment is now the later durable animal-skin sheet recipe: leather -> parchment at the parchmentery bench.
- Runic Inscription has been removed from the active MFVanilla tree for now; rune trap scroll access now sits under Enchantment, and scribing is gated by Spellcraft.
- Gemstone dust now exists as a generic tradeable raw resource.
- Cutting raw gemstone pieces now produces the cut gemstone plus gemstone dust, with more dust from lower-quality cuts: common gives 3, fine gives 2, exquisite gives 1.
- Arcane ink now requires exotic herbs plus gemstone dust, making lapidary part of the core scroll-production loop while keeping a single ink type.
- Shaman merchants now stock MFVanilla arcane reagents, gemstone materials, rough gemstone chunks, and occasional spell scrolls; they buy MFVanilla reagents, gemstones, and scrolls.
- Bulk goods traders now stock the practical production inputs: exotic herbs, gemstone dust, papyrus, parchment, and small amounts of rough gemstone chunks; they buy MFVanilla reagents and gemstones.
- The papyrus press now has final multi-directional building textures at `Things/Building/Production/PapyrusPress`.
- Research sequencing intentionally leaves Lapidary and Alchemy disconnected: colonies can buy gemstone dust for early Arcane Ink, then research Lapidary later when they want self-sufficient gemstone production.
- Papyrus can now be pressed from wood logs or plant matter such as hay, keeping wood useful while allowing a more literal plant-fiber route.
- MFVanilla builds successfully after the production-chain and research-tree changes.

Priority:
- review the full production chain from raw inputs through finished scrolls and magic utility items
- make sure each bench has a clear role, research position, recipe set, work type, texture, cost, and power/facility story
- tune Arcane Ink, parchment/papyrus, gemstones, herbs, and generated spell scroll costs so the early loop is useful and the advanced loop has meaningful investment
- check that research gates unlock recipes, benches, utility buildings, and spell access in a sensible order
- add missing descriptions, inspect strings, category placement, tradeability, stack sizes, market values, and bulk/storage behavior where content feels placeholder
- smoke test a new colony path: discover Arcane gift, unlock early research, craft inputs, make scrolls, learn spells, and progress into advanced production

Next steps:
1. Smoke test trader stock and buy behavior for shaman, neolithic bulk, outlander bulk, and orbital bulk traders, with special attention to whether purchasable gemstone dust supports early Arcane Ink before Lapidary.
2. Review gemstone vein availability, mining output, market values, and stack sizes so gemstone dust does not become either invisible or too abundant.
3. Decide the first typed-focus model for magic utility items: generic arcane foci for now, gemstone-family foci, or domain foci such as fire/water/earth/air/life/spirit.
4. Update magic heaters, coolers, torches, and future utility buildings once the focus model is chosen.
5. Add specific gemstone requirements to major scrolls only after the basic dust-and-ink loop feels stable.
6. Run an in-game smoke test of the full loop: buy or craft gemstone dust, grow herbs, make ink, make papyrus/parchment, scribe scroll, read scroll.

Success criteria:
- a player can understand what to build next without dev knowledge
- every production building earns its footprint in the colony
- scroll learning and generated scroll recipes feel like part of the same economy as the spells
- the content works as a stable example for third-party spell authors

### MF-038 MFVanilla Feature Completion

Goal: add a small number of features that deepen the released content without opening a broad framework expansion.

Good candidates:
- more utility recipes or buildings that consume magic production outputs
- one or two additional spell families only if they validate already-supported primitives or a narrowly needed framework hook
- stronger integration between spell metadata, scroll recipes, research, and generated descriptions
- clearer player feedback for spell scaling, active enhancement rules, and unlock paths
- balance pass for mana, cooldowns, costs, work amounts, and resource scarcity

Deferral rule:
- defer anything that primarily exists to prove a speculative framework system rather than make MFVanilla better now
- defer hostile AI casting, magic weapons/tools, real fire integration, and celestial event depth until after MFVanilla and AeternusFaith first-edition goals are in hand

### MF-039 AeternusFaith First Edition

Goal: get AeternusFaith ready for its first public release as a focused faith/ritual content mod.

Release-candidate checklist:
- verify `About.xml`, dependency metadata, preview/icon assets, assembly output, and Workshop/GitHub packaging expectations
- decide the first-edition content boundary: which memes, precepts, roles, rituals, apparel, buildings, undead, and spectral systems are included
- smoke test ideology generation, role assignment, ritual action gizmos, ritual jobs, room/lectern placement, corpse selection, reservation/reachability, save/load, and cleanup
- make ritual invalid states readable enough that players can recover without opening logs
- ensure undead/spectre lifecycle behavior is predictable: spawn, ownership/faction, control expectations, expiration/despawn, death/downing, and map removal
- keep pseudo-relationship memory and custom wall auto-joining as post-first-edition unless release testing shows they are essential to the core fantasy

First-edition emphasis:
- Ossanith skeleton and ossuary loops should be reliable and understandable.
- Shroudhymn spectre content should be stable enough that it does not leave stale pawns, spectral state, or player confusion.
- Bonewright role requirements should make ritual access feel intentional rather than arbitrary.

## P2 Framework Polish

### MF-009 Spell Power And Scaling

Goal: keep adding typed scaling support where there is a real use case.

Current state:
- `SpellDef.power` can define base power and caster-level/skill contributions.
- Power tiers resolve from minimum thresholds.
- Delayed and projectile impact actions preserve power value and tier.
- `ScalableFloatDef` supports authored `Flat`, `Linear`, and `Tiered` numeric modes with clamps.
- `SpellPowerScalarDef` supports spell-level multiplicative scalars for damage, healing, radius/range, duration, mana cost, and cooldown.
- `SpellPowerScalarDef` supports the same `Flat`, `Linear`, and `Tiered` numeric modes.
- `SpellPowerDef.scaledAttributes` supports lightweight authored scaling lists for damage, healing, radius/range, duration, mana cost, and cooldown.
- Magic Framework mod settings expose global per-power scaling factors for lightweight scaled attributes.
- Explicit `SpellPowerScalarDef` entries remain available for spell-specific tuning and take precedence over lightweight global scaling.
- `PowerTierConditionDef` and `SpellPowerConditionDef` support structural spell changes through conditionals.
- Scalable support exists for damage, healing, explosion radius/damage, targeting range, spawned thing count, durations, repeat/pulse count, displacement distance, and several validation spells.
- `MF_Firebolt` now validates lightweight damage/cooldown scaling.
- `MF_Fireball` now validates lightweight damage/radius scaling.
- MFVanilla healing, lightning, rune, trap, and fire-field spells now use lightweight `scaledAttributes` where caster level should affect damage, healing, radius, or duration.
- `MF_Regeneration` now validates scalable repeat/pulse count.
- `MF_ForcePush` and `MF_ForcePull` now validate scalable displacement distance.
- `MF_Regeneration` now has a reusable visible regeneration status cue while retaining its repeated healing pulses.

Candidate additions:
- finish auditing remaining MFVanilla spells that need new scalar targets rather than the current damage/healing/radius/duration/cost/cooldown list
- add player-facing display of the active global scaling factors where useful
- scalable summon duration, summon count, or tiered summon selection
- shield-specific scaling for sustained mana upkeep, absorption efficiency, and force-field strength
- scalable target count for chains, bursts, and multi-target spells
- tiered projectile/effect selection
- scalable field/wall shapes where authored shape variants become useful

Open design questions:
- What should count as caster level or spell power in non-debug progression?
- Should power come from traits, equipment, mana invested, ritual quality, research, or a future magic skill?
- Should lightweight scaled attributes support per-spell dampening/amplification without requiring full explicit scalar blocks?
- Should randomized numeric scaling be supported, and if so where should deterministic values be captured for delayed or persistent effects?
- Which context-sensitive spell-family modifiers should remain enhancement rules versus authored spell-local conditionals?

### MF-010 Buff/Debuff Primitives

Goal: support common status design without overloading raw hediff application.

Current state:
- Stat modifier buffs can display authored or generic `statusCue` hediff indicators.
- Clear actions can remove framework stat/status effects.
- `TimedStatusEffectActionDef` can apply a visible timed status wrapper, run `onApplyActions`, schedule `onExpireActions`, replace prior caster/spell instances, and clean up its status cue.
- `SpellStatusEffectDef` plus `ApplyStatusEffectActionDef` supports reusable premade status bundles with default duration, status cue, stat modifiers, and immediate `onApplyActions`.
- `SpellStatusEffectDef.categories` supports lightweight reusable status metadata such as `buff`, `debuff`, `control`, `healing`, `movement`, and elemental/family tags.
- `SpellStatusEffectDef.refreshPolicy` supports `RefreshDuration`, `IgnoreIfActive`, `StackDuration`, and `Replace` behavior for reusable statuses.
- Sustained stat modifiers can reference `SpellStatusEffectDef` payloads while preserving maintenance and break behavior.
- Generated spell summaries include reusable status categories and non-default refresh policies when authored.
- `MF_Haste`, `MF_Might`, Might backlash, `MF_BlessingOfVigor`, `MF_Freeze`, and `MF_WatersEmbrace` now use reusable premade status defs in MFVanilla where the effect is a simple timed stat/status bundle.
- MFVanilla reusable status defs now classify their buff/debuff/control/healing/movement families with `categories` and validate replace/ignore refresh behavior.
- Dedicated common status review is complete for the current authoring surface: `StunActionDef` remains the only dedicated control primitive for now; root/immobilize, silence, charm, and ignite should stay as reusable status/hediff/action compositions until content proves repeated authoring or cleanup complexity.

Candidates:
- named parameters/scalars for reusable status defs
- reusable status expiry actions once scheduled actions can target def-owned action trees
- broader conversion pass for existing authored stat/status spells where reuse makes XML clearer
- capacity modifiers
- accuracy, dodge, armor, casting-speed modifiers
- status cleanup groups, immunity checks, and visible player-facing explanations

### MF-012 Branching Chains

Goal: generalize `ChainLightningActionDef` into reusable delayed chain state.

Current state:
- `ChainLightningActionDef` supports delayed branching chain pulses with authored per-hit actions.
- Chain pulses preserve the originating spell seed through save/load.
- Gameplay random decisions now use `SpellDeterministicRandom`, a stable hash-based utility, instead of ambient `Rand`.
- Chain branch count, target shuffling, fallback stun chance, random-chance conditions, random teleport cells, and explosion spawn rolls/cells are deterministic from explicit spell/gameplay state.
- `ChainLightningActionDef.visitedTargetPolicy` supports allowing repeats or globally excluding previously hit targets, with legacy `allowRepeatTargets=false` mapping to global exclusion.
- `MF_ChainLightning` validates globally excluding previously hit chain targets.

Future needs:
- authored per-hop action lists
- configurable branching count, forward bias, falloff, and target caps
- richer beam/arc visuals between targets

### MF-014 Summon/Spawn Expansion

Goal: support more temporary spell-created entities than trained animals.

Remaining capabilities:
- spawn temporary objects
- spawn hazards, wards, totems, beacons, and helper constructs
- support non-animal or untrainable summons with a different control model
- optional arrival/expiry lifecycle actions
- clearer UI/status indication for temporary summons

### MF-016 Spell Details UI

Goal: let players inspect spell metadata and active modifiers without dev logs.

Current state:
- `SpellDescriptionUtility` generates cached baseline effect summaries from loaded `SpellDef` action trees.
- Known-spell gizmo tooltips append generated plain-language effect summaries.
- MFVanilla spell scroll inspect text shows generated effect summaries through the learn-spell comp.
- Known-spell grouped menus expose a dedicated spell details window with classification, learning, casting, targeting, active enhancement rules, and generated effect summaries.
- MagicFramework settings can enable colored generated spell text for selected costs, scaling, healing, cooldowns, and elemental/damage terms.
- Authored spell descriptions can opt into generated detail insertion with `{MF:...}` tokens such as `{MF:SpellSummary}`, `{MF:Effects}`, `{MF:ManaCost}`, `{MF:Cooldown}`, `{MF:Range}`, `{MF:Radius}`, `{MF:CastTime}`, `{MF:PowerScaling}`, `{MF:Requirements}`, `{MF:Targeting}`, `{MF:Classification}`, and `{MF:ActiveModifiers}`.

Possible first pass:
- show element/domain/discipline/tags
- show learning requirements and unmet prerequisites
- show mana/cooldown costs
- show active enhancement modifiers
- show range, cast time, and target policy

Remaining work:
- add richer summary providers for conditions, target queries, scaling values, and contextual enhancement modifiers
- decide how generated summaries should react to language changes or mod settings

## P2 Content And Runtime Polish

### MF-019 Ritual Dialog Improvements

Goal: make AeternusFaith ritual setup clearer and more polished.

Current state:
- MagicFramework provides `Dialog_ParticipantSelection` as a reusable participant-selection shell.
- The reusable dialog supports corpse selection plus pawn buckets for conductor, audience, and available pawns.
- Bucket rows use pawn/corpse icons, disabled-row reasons, and a validation summary before accept.
- AeternusFaith skeleton, ossuary, and spectre rite dialogs now use thin adapters over the shared participant dialog.

Possible first pass:
- Replace plain checkbox/radio lists with pawn rows that include portraits.
- Show why a corpse or conductor is unavailable.
- Surface reachability/reservation failure reasons where practical.
- Keep the UI compact enough for small screens.

Remaining work:
- Replace the generic disabled reasons with more specific reachability, reservation, role, and corpse-state reasons where the ritual comps can expose them.
- Consider dedicated slot labels and optional min/max participant counts if future rites need them.
- Smoke test the skeleton, ossuary, and spectre ritual dialogs at small resolutions.

### MF-020 Psychic Sensitivity In Haunting Effects

Goal: make haunting and spectral systems react to pawn psychic sensitivity.

Possible behavior:
- psychic sensitivity scales chance to notice, suffer, resist, or amplify haunt effects
- psychically deaf pawns are less affected or immune to some haunt cues
- highly sensitive pawns can become preferred targets or stronger conduits

Implementation notes:
- Keep early changes as tuning factors, not a new progression system.
- Add clear log/debug output while tuning.

### MF-021 Undead Pseudo-Relationship Memory

Goal: preserve family-memory flavor without allowing undead to resume normal social relationships.

Problem:
- Normal RimWorld relationships can cause strange or inappropriate behavior if an undead skeleton tries to resume spouse/parent/child social roles.

Target behavior:
- Store a separate memory list for important source-corpse relationships.
- Apply readable effects to the undead and related living pawns.
- Avoid normal relationship graph entries unless explicitly intended.

Complexity notes:
- Needs save/load.
- Needs relationship-copy policy.
- Needs UI/inspect or thought/status feedback.

## P3 Later And Exploratory

### MF-023 Custom Wall Atlas And Auto-Joining

Goal: improve AeternusFaith wall visuals with custom joining.

Notes:
- Likely needs atlas art, neighbor detection, and careful testing around blueprints, frames, minified things, corners, and save/load.

### MF-024 Magic Tools And Weapons

Goal: explore a framework/content layer for magic equipment.

Ideas:
- wands: cast or grant spells
- staves: boost caster level, spell power, or grant abilities
- swords: martial spell delivery, enchantments, or element riders
- armor: mana capacity, protection fields, ritual bonuses, domain affinities

Open design questions:
- Are these plain apparel/equipment comps, spell containers, requirement modifiers, or a separate enchantment system?
- Should equipment grant spells, modify known spells, or only improve casting?
- How should NPCs use them?

### MF-025 Celestial And Weather Event Hooks

Goal: make solar eclipses, auroras, solar flares, rain, wind, and unusual weather matter to magic.

Near-term content ideas:
- eclipse empowers death/shadow
- aurora empowers arcane/spirit
- rain weakens fire and empowers aquamancy
- wind empowers aeromancy

Implementation notes:
- Add after scalar factors are wired broadly enough to make rules visible in play.
- Consider event-specific incidents only after enhancement rules feel good.

### MF-026 Persistent Visual Support

Goal: keep visuals as a first-class framework feature rather than repeated fleck hacks.

Future needs:
- loadable tight personal shield texture for maintained overlays
- cloned hidden magic shield-belt apparel experiment, if vanilla visuals are worth borrowing
- temporary-equip spell action for spawned apparel and cleanup on expire/break/death/drop
- calmer sustained ambient visuals beyond repeated flecks
- optional multi-point patterns, persistent sounds, and stateful visuals
- validate draw order and scale against vanilla shield belt visuals

### MF-027 Validation Spell Icons

Goal: add custom gizmo icons for future validation spells as they are added.

Current wired icons include all current MFVanilla spell defs:
- `MF_ArcSeeker`, `MF_BlessingOfVigor`, `MF_BlinkStep`, `MF_ChainLightning`, `MF_CreateFood`, `MF_DelayedBlastRune`, `MF_Disintegrate`, `MF_EarthCall`, `MF_Fireball`, `MF_Firebolt`, `MF_FlameField`, `MF_ForceField`, `MF_ForcePull`, `MF_ForcePush`, `MF_Freeze`, `MF_Haste`, `MF_Heal`, `MF_ManaShield`, `MF_Might`, `MF_Regeneration`, `MF_RepulsionWard`, `MF_RescueRecall`, `MF_Resurrection`, `MF_RuneTrap`, `MF_SummonDog`, `MF_ThreatSpike`, `MF_Tornado`, `MF_Transposition`, `MF_TriagePulse`, `MF_WallOfFire`, and `MF_WatersEmbrace`.
- Runtime gizmo icon loading first uses authored `gizmoIconPath`, then falls back to `UI/Gizmos/Spells/{defName}` before the generic DesirePower icon.

### MF-029 Real Fire Integration For Wall Of Fire

Goal: decide whether `Wall of Fire` should use real RimWorld fire objects or remain a custom magical hazard.

Risks:
- real fire can spread, interact with rain/snow/fuel, and create cleanup surprises
- custom hazards are more predictable but may feel less integrated

### MF-030 Persistent World Objects

Goal: represent some long-lived spell effects as world/map objects rather than only runtime state.

Possible uses:
- wards
- beacons
- ritual anchors
- lingering curse/blessing sites
- map hazards created by major spells

### MF-031 Spell Design Guide

Goal: write a complete authoring guide for MagicFramework spells.

Sequencing note:
- Start the version 1 guide now against the stable authoring surface. Later exploratory work should add appendices or follow-up sections instead of blocking the first published guide.

Working document:
- [SpellDesignGuide.md](SpellDesignGuide.md)

Target coverage:
- top-level `SpellDef` fields
- targeting and pawn affinity rules
- requirements, learning requirements, costs, and cooldowns
- action trees and common action defs
- persistent state replacement/lifecycle policy
- scaling and spell power authoring
- procedural FX metadata and explicit visual/sound actions
- reusable resources: status effect defs, status cue hediffs, ordinary hediffs, spell metadata defs, magic FX profiles, projectiles, marker things, and generated scroll/research hooks
- built-in resource discovery: where authors can find usable `HediffDef`, `ThingDef`, `EffecterDef`, `FleckDef`, `SoundDef`, `DamageDef`, `ResearchProjectDef`, and texture path names
- common spell patterns: projectile, delayed rune, trap, wall, aura, displacement, buff, debuff, summon
- validation and regression expectations

Documentation pass order:
1. Establish the guide structure, terminology, and minimum viable spell example.
2. Document `SpellDef` anatomy, targeting, requirements, costs, cooldowns, and learning.
3. Document action tree execution context, target/cell sources, queries, conditions, and deterministic random expectations.
4. Document persistent lifecycle hooks for zones, walls, force fields, persistent effects, triggers, summons, and spawned things.
5. Document scaling, power tiers, enhancement rules, generated descriptions, details UI, and settings.
6. Document reusable authoring resources: `SpellStatusEffectDef`, status cues, hediffs, metadata defs, FX profiles, marker defs, projectile defs, and scroll/research integration points.
7. Document how to discover built-in RimWorld and MagicFramework resource names, including where to search in local defs/source and how to validate missing names in logs.
8. Add pattern recipes for first-party MFVanilla examples and a final validation checklist.

MF-031 documentation backlog:
- Tutorial 1, direct spell: expand `Minor Heal` into a full page covering file placement, load order/dependencies, allied targeting, requirement/cost pairing, Arcane gift, research gates, generated description tokens, healing scaling, and variants such as self-only, ally-only, and heal-over-time.
- Tutorial 2, projectile spell: expand `Ember Bolt` into a full page covering cast sequence, projectile launch, impact context, hit thing versus landing cell, secondary hediffs, explosions, projectile `ThingDef` authoring, scaling, and friendly-fire/targeting cautions.
- Tutorial 3, reusable timed status: add a Haste-style tutorial covering `SpellStatusEffectDef`, `ApplyStatusEffectActionDef`, `statusCue`, categories, refresh policies, stat modifiers, default duration, visible hediff indicators, and when to use reusable statuses instead of raw `ApplyHediffActionDef`.
- Tutorial 4, raw hediff/progressive status: add a Held Under / Burn-style tutorial covering `ApplyHediffActionDef`, severity, duration removal, body-part and add/remove policies where supported, preserving higher severity, max severity, and cleanup expectations.
- Tutorial 5, persistent aura: add a Flame Field / Water's Embrace-style tutorial covering marker things, pulse actions, target queries, concentration, create/pulse/expire/remove/break hooks, save/load, and overlap/replacement policy.
- Tutorial 6, maintained spell: add a Mana Shield / Might-style tutorial covering maintained state, manual release, break rules, sustained mana/upkeep, status cues, force-field impacts, and player-facing cancel behavior.
- Tutorial 7, delayed or triggered spell: add a Rune Trap / Delayed Blast Rune-style tutorial covering delayed actions, proximity triggers, persistent trigger state, trigger cleanup, and deterministic execution after save/load.
- Tutorial 8, movement spell: add Blink Step / Force Push / Force Pull coverage for teleport, displacement distance, collision handling, post-teleport stun, cell validation, and self/ally/foe safety.
- Tutorial 9, chain spell: add Chain Lightning coverage for delayed branch pulses, visited target policy, deterministic random helpers, target selection, stun/damage payloads, and save/load.
- Tutorial 10, summon/spawn spell: add Summon Dog coverage for temporary pawn lifecycle, owner/faction policy, trainables, expiry hooks, and future spawned thing/ward authoring.
- Reference, action catalog: produce a table of version 1 action defs with purpose, key fields, child action lists, common examples, lifecycle/save-load notes, and first-party spell references.
- Reference, targeting/query catalog: document target shapes, primary target types, pawn affinity, caster-as-target, cell restrictions, water/resurrectable requirements, query defs, ordering, max targets, and stable tie-breaking.
- Reference, reusable resources: catalog first-party MFVanilla `SpellStatusEffectDef` entries, status cue hediffs, notable hediffs, spell metadata defs, MagicFramework FX profiles, marker things, and validation spell icons.
- Reference, built-in name discovery: explain how authors can find RimWorld/vanilla defs locally by searching `Defs`, the RimWorld install `Data` folders, and source/XML examples; include common categories such as `EffecterDef`, `FleckDef`, `SoundDef`, `DamageDef`, `ThingDef`, `HediffDef`, `ResearchProjectDef`, `PawnKindDef`, and texture paths.
- Reference, generated presentation: document generated description tokens, spell details UI fields, colored generated text settings, scroll inspect text, and how to keep authored descriptions readable.
- Reference, scaling and enhancement: document `SpellPowerDef`, `ScalableFloatDef`, lightweight `scaledAttributes`, explicit scalar defs, power tiers, structural conditionals, enhancement rules, and settings multipliers.
- Reference, validation checklist: provide copyable smoke-test checklists for XML load, cast success/failure, targeting edges, cost/cooldown, generated descriptions, save/load, cleanup, replacement, cancel, caster down/death, target invalidation, and multiplayer-sensitive deterministic behavior.
- Web presentation: keep the Markdown guide as the canonical source, add browser tutorial pages/cards as sections mature, and keep SpellForge links anchored to the tutorial pages through `sync-docs.ps1`.

### MF-032 Compatibility

Goal: Gate any mechanisms that would not be supported in multiplayer mods

Current state:
 - `SpellDeterministicRandom` provides stable hash-derived values for gameplay decisions that need random-looking behavior.
 - Current MagicFramework gameplay calls no longer use ambient `Rand`; visual-only or vanilla-internal randomness may still occur outside framework-owned decisions.
 - New framework-owned gameplay code should prefer `SpellDeterministicRandom` whenever it needs chance rolls, random ranges, random selection, or shuffling.

Target coverage:
 - using `Verse.Rand`, `System.Random`, `UnityEngine.Random`, random collection helpers, or time-based randomness without Multiplayer-safe syncing.
 - depending on real time, frame rate, local UI timing, thread timing, or machine-specific order.
 - changing game state from UI code.
 - running logic only on one client.
 - using dictionaries or unordered collections where iteration order could affect gameplay results.
 - async/network/API calls that affect gameplay state.
 - Harmony patches that alter core ticking, job assignment, combat, map generation, or pawn behavior in nondeterministic ways.
 - visual-only effects that accidentally touch gameplay state.


### MF-036 AI

Goal: Review spells and evaluate if the hostile pawn AI can be empowered to use magic spells they have available.

Target coverage:
 - Evaluate whether hostile pawns can use known MagicFramework spells through a shared non-player casting path rather than the current gizmo/targeter flow.
 - Evaluate random generation for hostile humanlike pawns that happen to have the Arcane gift trait and an authored spell loadout.
 - Define enough spell metadata or heuristics for AI to choose targets, avoid friendly fire, and avoid wasting mana/cooldowns.
 - Keep the first implementation scoped to hostile combat pawns; do not broaden to animals, mechanoids, guests, prisoners, traders, or friendly colonist automation until the hostile case is stable.

Initial observations:
 - The framework already has most of the runtime pieces an AI caster would need: Arcane gift, known spell storage, caster level, mana, cooldowns, targeting validation, cast warmup, and execution.
 - The current player path is UI-oriented (`SpellGizmoUtility` plus RimWorld targeter), so hostile AI likely needs a separate service that can build candidate targets/cells and call `SpellCastWarmupUtility.StartOrExecute` or a lower-level executor without messages/UI assumptions.
 - `SpellRequirementUtility.CanCastSpell(..., requireKnownSpell: true)` should probably be part of the AI gate so generated pawns cannot cast arbitrary defs just because an AI scorer found them.
 - Spell targeting already exposes affinity, target type, line of sight, range, walkable/standable/water checks, and self-targeting. The missing piece is intent: whether a spell is offensive, defensive, ally-support, escape, terrain/control, summon, utility, or too dangerous for AI.
 - A first pass should probably whitelist AI-usable spells rather than infer behavior from arbitrary action trees. Many spells have delayed, persistent, area, wall, teleport, summon, or resurrection behavior that can be valid for players but awkward or abusive when selected generically by a raid pawn.
 - Friendly-fire and self-harm risk need explicit treatment for radius, line, wall, chain, persistent field, explosion, knockback/pull, teleport, and trap/rune spells. The vanilla hostile AI will not naturally understand MagicFramework action side effects.
 - Maintained/sustained spells need separate handling from instant casts. AI should avoid repeatedly recasting an already active maintained spell, and may need release/break policy if the caster flees, downs, changes target, or runs out of mana.
 - Random gifted hostile generation should be rare and content-controlled. It needs faction/pawn-kind filters, storyteller/difficulty tuning, optional incident or raid-point scaling, and a cap so a raid does not accidentally become mostly casters.
 - Humanlike-only generation is a good starting constraint, but biotech/xenotype, ideology role, backstory, trait, faction tech level, and modded pawn kinds may all be better eligibility signals than race alone.
 - Save/load is already covered for caster runtime state, but hostile pawns created and destroyed in large numbers could leave stale caster state if cleanup is not already sufficient.
 - Multiplayer compatibility matters here: spell selection should use deterministic game-state inputs or existing deterministic helpers, not ambient `Rand`, UI timing, or local-only decisions.
 - Debugging support will be important. Add log categories or dev-mode inspection for why an AI pawn did or did not cast: no Arcane gift, no known spell, cooldown, mana, no valid target, friendly-fire risk, or scorer chose weapon attack.

Design questions:
 - Should AI spell behavior be authored on `SpellDef` directly, through a separate `SpellAIDef`, or through tags/categories in `SpellMetadataProperties`?
 - Should spell use be inserted as a `JobGiver`/think-tree behavior, a verb-like combat option, a Harmony hook around ranged attack selection, or a periodic map-level caster brain?
 - Should AI casters spend the same mana and cooldowns as players, or should some hostile-only tuning exist for encounter balance?
 - Should random hostile casters learn from the same research-gated spell list as players, or from faction/pawn-kind loadout tables that ignore player research?
 - How visible should enemy casting intent be to the player during warmup: stance only, mote/sound cue, inspect string, combat log, or letter/message for dangerous spells?
 - Should downed, fleeing, mental-state, drafted-equivalent, or lord-controlled pawns continue casting, cancel pending warmups, or revert to normal combat?

Candidate first pass:
 - Add authored AI metadata with conservative defaults: `aiUsable`, use category, target preference, minimum/maximum range, friendly-fire policy, score weights, and optional raid generation weight.
 - Create a reusable `SpellAIUtility` that lists known castable spells, checks mana/cooldown/requirements, enumerates valid targets using the existing targeting rules, and scores spell-target pairs.
 - Start with self/ally buffs, direct hostile single-target damage/control, and simple summons. Defer walls, traps/runes, resurrection, long-lived fields, teleport swaps, displacement, chain spells, and large radius attacks until the scorer can reason about collateral risk.
 - Add a small hostile humanlike caster generation hook with settings and debug logging, then validate with one or two MFVanilla spells before opening the whole content set.


### MF-040 Splash screen

Goal: Introduce a first launch splash screen and include important notes

Target coverage: 
 - Inform players about mod settings to enable/disable tech themed vanilla research.
 - Consider other important details (to be determined)

### MF-040 Arcane forge

Goal: Introduce an Arcane Forge production item

Target coverage: 
 - develop requirements to build this mid - late game item
 - develop recipes for transforming mundane weapons (ie. swords) of high quality into magic versions (eg. flame sword, runic blade, etc), 
