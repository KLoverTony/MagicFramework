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
- `P1`: high-value framework capability.
- `P2`: content/runtime polish.
- `P3`: nice-to-have, experiments, or later expansion.

## Priority Index

| ID | Priority | Complexity | Area | Task |
| --- | --- | --- | --- | --- |
| MF-009 | P2 | M | Spell power | Continue typed spell power and scaling support. |
| MF-010 | P2 | M | Buffs | Add richer buff/debuff primitives beyond direct stat modifiers. |
| MF-012 | P2 | L | Chains | Generalize delayed branching chain support. |
| MF-014 | P2 | M | Summons | Extend summon/spawn primitives beyond temporary trained creatures. |
| MF-016 | P2 | S | UI | Add a player-facing spell details UI concept or first pass. |
| MF-019 | P2 | M | AeternusFaith | Improve ritual dialogs with pawn avatars and clearer invalid-state feedback. |
| MF-020 | P2 | M | AeternusFaith | Tie psychic sensitivity into haunting effects. |
| MF-021 | P2 | L | AeternusFaith | Add pseudo-relationship memory for raised undead. |
| MF-023 | P3 | L | AeternusFaith | Build custom wall atlas and auto-joining support. |
| MF-024 | P3 | L | Content | Start magic tools and weapons framework/content. |
| MF-025 | P3 | M | Events | Add celestial event enhancement rules and gameplay hooks. |
| MF-026 | P3 | M | Visuals | Continue persistent visual support. |
| MF-027 | P3 | S | Content | Add future validation spell gizmo icons as spells are added. |
| MF-029 | P3 | M | Fire | Investigate real RimWorld fire integration for `Wall of Fire`. |
| MF-030 | P3 | M | World state | Consider persistent world-object representations for more spells. |
| MF-031 | P3 | L | Docs | Write a full MagicFramework spell design guide. |
| MF-032 | P3 | S | Compatibility | Gate mechanisms that would not be supported in multiplayer mods. |
| MF-033 | P3 | S | Content | Shroudhymn summoned spectres should despawn cleanly. |
| MF-034 | P3 | S | Content | Add corresponding lecterns as placeable objects in ritual circle action gizmos. |
| MF-035 | P3 | S | Content | Ritual summons should only be performable by Bonewrights (ideology role). |
| MF-036 | P3 | L | AI | Review spells and evaluate if the hostile pawn AI can be empowered to use magic spells they have available. |

## P1 Framework Capabilities

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

Current wired icons include:
- `MF_ArcSeeker`, `MF_BlessingOfVigor`, `MF_BlinkStep`, `MF_ChainLightning`, `MF_CreateFood`, `MF_DelayedBlastRune`, `MF_Disintegrate`, `MF_Fireball`, `MF_Firebolt`, `MF_FlameField`, `MF_ForceField`, `MF_ForcePull`, `MF_ForcePush`, `MF_Haste`, `MF_Heal`, `MF_ManaShield`, `MF_Might`, `MF_Regeneration`, `MF_RuneTrap`, `MF_SummonDog`, and `MF_WallOfFire`.

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
- Do not start this until there are no further active MagicFramework framework to-dos, so the guide documents a stable authoring surface instead of chasing moving targets.

Target coverage:
- top-level `SpellDef` fields
- targeting and pawn affinity rules
- requirements, learning requirements, costs, and cooldowns
- action trees and common action defs
- persistent state replacement/lifecycle policy
- scaling and spell power authoring
- procedural FX metadata and explicit visual/sound actions
- common spell patterns: projectile, delayed rune, trap, wall, aura, displacement, buff, debuff, summon
- validation and regression expectations

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
 
