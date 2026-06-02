# MagicFramework Framework Backlog

Framework-level polish, compatibility, documentation, UI, and runtime expansion tasks extracted from ProjectTodo.md.

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


### MF-063 Shared Undead And Construct Pawn Foundations

Goal: move reusable undead, spirit, and magical construct lifecycle behavior into MagicFramework so AeternusFaith, MFVanilla, and future content packs can define custom creature types without copying brittle race-specific cleanup code.

Design direction:
- keep MagicFramework responsible for neutral infrastructure: lifecycle policy, needs control, social/interaction policy, apparel/equipment policy, player control policy, corpse/soul hooks, save/load cleanup, and readable status markers
- keep content mods responsible for doctrine, art, ritual flavor, research gates, named hediffs/xenotypes, pawn kinds, and balance
- distinguish undead, spirits, and constructs even when they share implementation hooks
- prefer data-driven profiles or mod extensions over hardcoded race def names such as AeternusFaith's current skeleton/spectre checks
- build the foundation around explicit behavioral qualities instead of assuming all undead behave like skeletons

Current source examples:
- AeternusFaith has the richest undead lifecycle today: Ossanith skeletons, Shroudhymn/spectral pawns, undead hediff markers, xenotype application, need removal, social suppression, corpse consumption, and soul-state interaction.
- MFVanilla has spell/content creatures that should eventually consume the same framework surface: temporary necromancy skeletons, flesh golems, arcane automata, stone golems, and future lich or revenant content.
- MFVanilla's current constructs intentionally lean on reliable mechanoid-style bases, which is acceptable for hostile site defenders but should not become the only foundation for soul-aware undead.

Current state:
- Initial framework-only XML/API surface exists under `MagicFramework.PawnLifecycle`.
- `PawnLifecycleExtension` can be attached to a pawn race `ThingDef` or `PawnKindDef`; pawn-kind policy overrides race policy.
- The first policy axes are body form, intelligence, needs, social behavior, gear, control, work, recovery, death cleanup, soul contract, and duration/upkeep.
- `PawnLifecycleUtility` provides read-only query helpers for lifecycle detection, undead/spirit/construct classification, lifecycle tags, marker hediffs, and debug summaries.
- `CompPawnLifecycleEnforcer` and `PawnLifecycleEnforcementUtility` provide the first opt-in runtime enforcement layer for needs, social suppression, gear stripping, life-stage normalization, and marker hediffs.
- Dev actions can log a selected pawn's lifecycle profile or all profiled pawns on the current map.
- XML examples live in [PawnLifecycleProfiles.md](PawnLifecycleProfiles.md).
- Lifecycle traits can now be injected from XML profiles so RimWorld's native disabled-work system governs simple worker/combat limitations instead of framework priority forcing.
- Autonomous lifecycle pawns can be made selectable but non-draftable, with direct draft attempts blocked while master-bound minions remain manageable.
- Lifecycle pawns now expose a compact inspect-string readout for control/work profile, master binding, and temporary summon expiry when tracked by the summon runtime.
- MFVanilla's temporary `MFV_Skeleton` has a verified master-bound combat-minion profile.
- AeternusFaith's `AF_SkeletonRace` has a verified autonomous servant profile.
- AeternusFaith's `AF_SpectreRace` now has a spectral lifecycle profile layered around its existing Shroudhymn manifestation system: no needs, no gear, no work, guest/non-draftable control, active-spirit soul policy, and AF spectral marker hediffs.
- AeternusFaith now has a reusable `UndeadPawnFactory` that generates AF lifecycle-authored undead pawn kinds from a clean humanlike template and applies lifecycle enforcement, source ideology, optional source backstories/skills, race markers, xenotypes, and appearance in one path. Existing `AF_Skeleton` and `AF_Spectre` generation now route through this factory, while their old conversion helpers remain compatibility wrappers.
- Recovery, death cleanup, soul contracts, deeper duration/upkeep policies, and advanced interaction policies are still early or definition/query-only.

Behavioral qualities to model:
- hunger/eating: none, ordinary food, corpse/flesh consumption, mana upkeep, essence drain, or content-defined feeding
- sleep/rest: none, ordinary rest, periodic dormancy, phylactery/anchor recharge, daylight dormancy, or content-defined rest
- mood/joy/comfort: removed, suppressed, ordinary, anchor-driven, master-driven, or special spirit-emotion behavior
- social interaction: none, suppressed both ways, limited non-social presence, eerie/aura-only impact, ordinary conversation, pseudo-relationship memory, or full living-style relationships
- apparel and equipment: stripped, no apparel, equipment-only, apparel-only, full clothing/weapon use, restricted loadout, or content-defined ritual gear
- player control: hostile only, autonomous guest, allied non-controllable, drafted follower, full colonist-like pawn, master-bound minion, temporary spell summon, or event-controlled entity
- work behavior: no work, hauling/cleaning only, combat only, limited labor set, full work tab, ritual-only duties, or content-defined work settings
- medical/repair: cannot heal, ordinary medicine, repair job, regeneration, reassembly, corpse replacement, phylactery reform, or content-defined recovery
- death/despawn cleanup: corpse remains, ash/bone pile, vanishes, returns to anchor, releases soul, corrupts soul, creates haunting risk, drops construct materials, or triggers content actions
- soul/corpse contract: no soul, corpse-only husk, released source soul, bound source soul, active spirit, split echo, consumed soul, corrupted soul, phylactery-anchored soul, or constructed non-soul core
- duration/upkeep: permanent, temporary timer, maintained spell, master/conductor upkeep, anchor upkeep, map/site bound, or content-defined expiry

Type policy examples:
- skeletons: no hunger, no sleep, no ordinary social interaction, usually stripped/no apparel unless explicitly authored, limited work/combat roles, usually corpse-only husks with released souls
- spectres/shades: no hunger or sleep, limited or aura-style interaction, normally no apparel/equipment, often autonomous guest or duty-bound rather than player-managed, can be active spirits rather than corpse husks
- revenants: may retain partial identity, may interact socially in limited ways, may use gear, often bound to a memory/soul contract, likely not ordinary full colonists by default
- liches: no ordinary hunger/sleep, full intelligence, likely full apparel/equipment and strong player control, explicit phylactery identity, death/reform lifecycle, and careful relationship/magic-state preservation
- flesh golems: no soul or unstable body-soul contract depending on content, may have no hunger/sleep, usually no social interaction, repair/regeneration rather than medicine, limited control
- arcane constructs: no soul, no hunger/sleep/mood/social, no apparel, equipment only if built into the pawn kind, repair/material drops, command-core or faction control rather than undead soul behavior

First implementation pass:
1. Audit current AeternusFaith and MFVanilla creature behavior and write a small matrix of existing values for hunger, sleep, mood, social, apparel, equipment, control, work, healing, death cleanup, soul state, and duration.
2. Define a framework data shape, likely `UndeadLifecycleExtension`, `ConstructLifecycleExtension`, or one shared `PawnLifecycleExtension`, with conservative defaults and explicit opt-ins for risky behavior.
3. Add framework helpers for detecting lifecycle categories and querying policy by pawn/race/kind instead of checking content def names directly.
4. Extract the generic parts of AeternusFaith's undead cleanup into MagicFramework: need removal, life-stage normalization, social suppression, gear stripping, marker application, and save/load re-enforcement.
5. Keep AeternusFaith-specific marker defs, xenotype defs, Bonewright rules, cathedra rules, and ritual messages in AeternusFaith, but make them call framework helpers.
6. Add XML parent defs or example profiles for base skeletal undead, spectral undead, revenant-like undead, flesh construct, and arcane construct.
7. Migrate AeternusFaith skeletons and spectres to use the framework lifecycle profile while preserving current gameplay and save/load behavior.
8. Decide whether MFVanilla's spell skeleton should remain a simple temporary mechanoid-style pawn or move to the shared skeletal undead profile.
9. Decide whether MFVanilla automata stay on construct-only profiles and whether the flesh golem needs a hybrid flesh-construct profile.
10. Add debug/readout support that explains a pawn's lifecycle profile: hunger, sleep, interaction, apparel/equipment, control, work, death cleanup, soul contract, and duration/upkeep.

Iteration slices:
1. Documentation and audit only: no code, just the behavior matrix and target profiles. - started through this plan
2. Read-only framework query helpers and lifecycle profile definitions. - initial `PawnLifecycleExtension` and `PawnLifecycleUtility` added
3. Generic cleanup comp/helper implementation with no content migration. - initial opt-in enforcer added for needs, social, gear, life-stage, and marker hediffs
4. AeternusFaith skeleton migration and smoke test. - verified as selectable autonomous servant, not draftable
5. AeternusFaith spectre migration and smoke test. - lifecycle profile added; needs in-game Shroudhymn rite/debug manifestation smoke test
6. MFVanilla skeleton/necromancy decision and migration if approved. - `MFV_Skeleton` converted and verified as temporary master-bound combat minion
7. Generic AF undead pawn factory for skeleton/spectre generation and future close variants. - initial `UndeadPawnFactory` added; current skeleton and spectre paths route through it
8. Construct/flesh-golem profile pass after undead migration is stable.
9. Advanced identity pass for revenants, liches, pseudo-relationship memory, and phylactery-style reform.

Success criteria:
- content mods can author new undead or constructs by choosing explicit lifecycle policies instead of copying cleanup code
- skeletons, spectres, revenants, liches, flesh golems, and constructs can differ meaningfully in hunger, sleep, social behavior, gear use, controllability, work, healing, and death cleanup
- AeternusFaith keeps its doctrine-specific identity while MagicFramework owns the reusable lifecycle surface
- MFVanilla can add Necromancy, Fleshcraft, and Soulcraft creatures without turning each one into a one-off pawn hack
- save/load, despawn, death, map removal, and soul/corpse state are testable per profile


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
- Web presentation: keep the Markdown guide as the canonical source, add browser tutorial pages/cards as sections mature, and maintain SpellForge directly under `Mods/docs/spell-def-builder`.


### MF-032 Compatibility

Goal: Gate any mechanisms that would not be supported in multiplayer mods

Current state:
 - `SpellDeterministicRandom` provides stable hash-derived values for gameplay decisions that need random-looking behavior.
 - Current MagicFramework gameplay calls no longer use ambient `Rand`; visual-only or vanilla-internal randomness may still occur outside framework-owned decisions.
 - New framework-owned gameplay code should prefer `SpellDeterministicRandom` whenever it needs chance rolls, random ranges, random selection, or shuffling.
 - Source review suggests MagicFramework is partially multiplayer-friendly because delayed/persistent spell state is mostly scribed and many gameplay-random decisions use saved deterministic seeds.
 - Source review suggests MagicFramework and MFVanilla should not yet be advertised as confirmed Multiplayer-compatible because there is no visible Multiplayer/MPCompat sync layer for player-triggered spell and item ability actions.
 - MFVanilla still has several gameplay-affecting `Rand` calls, including Elementalist caster assignment, gemstone mining/cutting, enchantment quality upgrades, and Arcane gift practice rolls.
 - Framework hostile spell AI still uses ambient randomness for assessment timing, per-pawn cast bias, and available spell shuffling.
 - Spell warmup pending casts are held in a game component but are not currently scribed, which is a save/load and multiplayer-stability risk.

Target coverage:
 - using `Verse.Rand`, `System.Random`, `UnityEngine.Random`, random collection helpers, or time-based randomness without Multiplayer-safe syncing.
 - depending on real time, frame rate, local UI timing, thread timing, or machine-specific order.
 - changing game state from UI code.
 - running logic only on one client.
 - using dictionaries or unordered collections where iteration order could affect gameplay results.
 - async/network/API calls that affect gameplay state.
 - Harmony patches that alter core ticking, job assignment, combat, map generation, or pawn behavior in nondeterministic ways.
 - visual-only effects that accidentally touch gameplay state.

Next steps:
 - Add or document a Multiplayer/MPCompat integration layer for synced spell casting, maintained-spell release, item ability use, and any UI command that mutates game state.
 - Replace remaining gameplay `Rand`/`Shuffle()` calls with deterministic helpers or Multiplayer-safe synced calls:
   - `MFVanillaPatcher` Elementalist caster selection and curated spell package choice.
   - `GemstoneUtility` mining yields, family selection, dense chunk selection, and cut quality.
   - `EnchantmentUtility` leyline quality upgrade roll.
   - `ArcaneGiftStudyGameComponent` practice-to-gift roll.
   - `SpellAIManagerGameComponent` assessment jitter, cast bias, and available-entry ordering.
 - Decide whether visual-only randomness such as `CompArcaneForge` lightning flecks should be deterministic, local-only presentation, or left as harmless unsynced visual noise.
 - Add `ExposeData` support or a different recovery policy for pending spell warmups.
 - Run a two-client smoke test before claiming compatibility: player spell cast/release, magic item ability use, delayed rune/trap, maintained shield/aura, chain spell, gemstone mining/cutting, enchantment, Arcane gift practice gain, Elementalist hostile caster generation, and hostile AI casting.



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
 - Add authored AI metadata with conservative defaults: `aiUsable`, use category, target preference, minimum/maximum range, friendly-fire policy, score weights, and optional raid generation weight. - deferred in favor of generation-owned curated spell packages for the first pass
 - Create a reusable `SpellAIUtility` that lists known castable spells, checks mana/cooldown/requirements, enumerates valid targets using the existing targeting rules, and scores spell-target pairs. - initial `SpellAIManagerGameComponent` added: registered pawns keep curated spell/intent entries, check known available spells first, gather small priority-ordered target lists by intent, validate spell-target pairs with the existing validator, score validated options against intent-specific cast thresholds with per-pawn hesitation bias, gate support spells behind actual combat engagement, and cast through `SpellCastWarmupUtility.StartOrExecute`
 - Start with self/ally buffs, direct hostile single-target damage/control, and simple summons. Defer walls, traps/runes, resurrection, long-lived fields, teleport swaps, displacement, chain spells, and large radius attacks until the scorer can reason about collateral risk. - first pass includes hostile, heal-ally, and buff-ally intents only; summons and all area/cell/multi-target behavior remain deferred
 - Add a small hostile humanlike caster generation hook with settings and debug logging, then validate with one or two MFVanilla spells before opening the whole content set. - initial MFVanilla hook assigns about 20% of hostile `MFV_ElementalistTribe` humanlike pawns 1-3 curated spells from `MF_Firebolt`, `MF_ForcePush`, `MF_Heal`, `MF_Stoneskin`, and `MF_Might`


