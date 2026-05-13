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
| MF-005 | P1 | M | Lifecycle | Formalize lifecycle hooks for persistent spell state. |
| MF-007 | P1 | M | Validation | Add focused validation spells for under-tested framework modes. |
| MF-009 | P2 | M | Spell power | Continue typed spell power and scaling support. |
| MF-010 | P2 | M | Buffs | Add richer buff/debuff primitives beyond direct stat modifiers. |
| MF-011 | P2 | M | Projectiles | Improve projectile impact context and launch authoring. |
| MF-012 | P2 | L | Chains | Generalize delayed branching chain support. |
| MF-014 | P2 | M | Summons | Extend summon/spawn primitives beyond temporary trained creatures. |
| MF-016 | P2 | S | UI | Add a player-facing spell details UI concept or first pass. |
| MF-017 | P2 | M | Settings | Add RimWorld mod settings tabs for framework and first-party content options. |
| MF-018 | P2 | M | MFVanilla | Build the arcane ink production chain for scrollmaking. |
| MF-019 | P2 | M | AeternusFaith | Improve ritual dialogs with pawn avatars and clearer invalid-state feedback. |
| MF-020 | P2 | M | AeternusFaith | Tie psychic sensitivity into haunting effects. |
| MF-021 | P2 | L | AeternusFaith | Add pseudo-relationship memory for raised undead. |
| MF-022 | P2 | M | AeternusFaith | Add torch sconces and arcane torch lighting content. |
| MF-023 | P3 | L | AeternusFaith | Build custom wall atlas and auto-joining support. |
| MF-024 | P3 | L | Content | Start magic tools and weapons framework/content. |
| MF-025 | P3 | M | Events | Add celestial event enhancement rules and gameplay hooks. |
| MF-026 | P3 | M | Visuals | Continue persistent visual support. |
| MF-027 | P3 | S | Content | Add future validation spell gizmo icons as spells are added. |
| MF-028 | P3 | S | Design | Review which common statuses deserve dedicated primitives after reusable status adoption. |
| MF-029 | P3 | M | Fire | Investigate real RimWorld fire integration for `Wall of Fire`. |
| MF-030 | P3 | M | World state | Consider persistent world-object representations for more spells. |
| MF-031 | P3 | L | Docs | Write a full MagicFramework spell design guide. |
| MF-032 | P3 | S | Compatibility | Gate mechanisms that would not be supported in multiplayer mods. |
| MF-033 | P3 | S | Content | Shroudhymn summoned spectres should despawn cleanly. |
| MF-034 | P3 | S | Content | Add corresponding lecterns as placeable objects in ritual circle action gizmos. |
| MF-035 | P3 | S | Content | Ritual summons should only be performable by Bonewrights (ideology role). |
| MF-036 | P3 | S | Content | Roles for Custom Aeternus Faith should be fixed if possible. |
| MF-037 | P3 | S | Content | Ritual circles should be research dependent and gain new research fields: Traditions of the Ossanith and Sacred Rituals of the Bonewrights. |


## P1 Framework Capabilities

### MF-005 Lifecycle Hooks For Persistent State

Goal: make markers, traps, walls, zones, summons, spawned things, and maintained effects predictable.

Current state:
- Some systems have purpose-built hooks such as sustained `onBreakActions` and area-zone `onEndActions`.
- `LifecycleHooks.md` defines shared hook semantics for create, pulse, trigger, expire, remove, break, and legacy end behavior.
- Persistent area zones support explicit `onCreateActions`, `onPulseActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`, while preserving `onEndActions` as a legacy terminal catch-all.
- Maintained force fields support explicit `onCreateActions`, `onPulseActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`.
- Persistent wall zones support explicit `onCreateActions`, `onPulseActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`.
- Persistent effects support explicit `onCreateActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`.
- Proximity triggers support explicit `onCreateActions`, `onTriggerActions`, `onRemoveActions`, and `onBreakActions`, while preserving existing child `actions` as the trigger body.

Target hooks:
- `onCreate`
- `onPulse`
- `onTrigger`
- `onExpire`
- `onRemove`
- `onBreak`

Remaining work:
- Backfill summons, spawned things, and fuller stat modifier lifecycle hooks as appropriate.
- Ensure hooks survive save/load where runtime state persists.

### MF-007 Validation Spell Suite

Goal: keep feature coverage easy to test in-game without relying only on debug fallbacks.

Needed coverage:
- dedicated persistent zone spell
- maintained/channel spell beyond `MF_Might`
- teleport/displacement regression spells for swap, rescue, ally teleport, and enemy blink
- lifecycle hook validation spell once hooks are generalized

Already covered:
- target-query validation: `MF_ArcSeeker`
- ally-radius query with caster exclusion: `MF_BlessingOfVigor`
- lowest-health query ordering and target limit: `MF_TriagePulse`
- highest-threat query ordering and target limit: `MF_ThreatSpike`
- conditional branch spell: `MF_Disintegrate`
- sustained stat buff: `MF_Might`
- maintained shields: `MF_ForceField`, `MF_ManaShield`
- delayed branching chain: `MF_ChainLightning`
- direct heal / heal-over-time: `MF_Heal`, `MF_Regeneration`
- basic teleport: `MF_BlinkStep`
- terrain patch and thaw/melt behavior: `MF_Freeze`, `MF_FlameField`

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
- Scalable support exists for damage, healing, explosion radius/damage, targeting range, spawned thing count, durations, and several validation spells.
- `MF_Firebolt` now validates lightweight damage/cooldown scaling.
- `MF_Fireball` now validates lightweight damage/radius scaling.

Candidate additions:
- audit MFVanilla spells and add lightweight `scaledAttributes` where level progression should be visible
- add player-facing display of the active global scaling factors where useful
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
- Sustained stat modifiers can reference `SpellStatusEffectDef` payloads while preserving maintenance and break behavior.
- `MF_Haste`, `MF_Might`, Might backlash, `MF_BlessingOfVigor`, `MF_Freeze`, and `MF_WatersEmbrace` now use reusable premade status defs in MFVanilla where the effect is a simple timed stat/status bundle.

Candidates:
- decide whether reusable statuses need their own category/tag model (`Buff`, `Debuff`, `Control`, `Elemental`, `Mental`, etc.)
- stacking and refresh policies for reusable statuses
- named parameters/scalars for reusable status defs
- reusable status expiry actions once scheduled actions can target def-owned action trees
- broader conversion pass for existing authored stat/status spells where reuse makes XML clearer
- capacity modifiers
- accuracy, dodge, armor, casting-speed modifiers
- root/immobilize, silence, charm, stun, ignite as dedicated primitives if authoring proves repetitive
- status cleanup groups, immunity checks, and visible player-facing explanations

### MF-011 Projectile Support

Goal: make projectile spells feel like real RimWorld combat objects without losing spell context.

Current state:
- `LaunchProjectileActionDef` launches vanilla projectiles.
- Impact actions run after projectile impact/destruction/timeout.
- Impact context resolves to the projectile's last known cell.

Remaining work:
- Capture exact hit thing where RimWorld exposes it.
- Account for misses, cover interception, and shield blocking.
- Add richer authored launch origins and arcing/overhead policy.
- Consider custom spell projectile classes only when vanilla projectiles cannot expose enough context.

### MF-012 Branching Chains

Goal: generalize `ChainLightningActionDef` into reusable delayed chain state.

Future needs:
- authored per-hop action lists
- visited-target policies shared with target queries
- deterministic seeded random branching
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

Possible first pass:
- show element/domain/discipline/tags
- show learning requirements and unmet prerequisites
- show mana/cooldown costs
- show active enhancement modifiers
- show range, cast time, and target policy

Remaining work:
- add a dedicated spell details window or tab instead of relying only on tooltips/inspect strings
- add richer summary providers for conditions, target queries, scaling values, and contextual enhancement modifiers
- decide how generated summaries should react to language changes or mod settings

### MF-017 Mod Settings Tabs

Goal: add RimWorld mod settings tabs for MagicFramework and first-party content so players can tune behavior without XML edits.

Initial tabs:
- `Framework`: logging level, subsystem log toggles, debug gizmo visibility, and compatibility toggles.
- `MFVanilla`: research behavior, scroll generation/content toggles, and balance multipliers where appropriate.
- `AeternusFaith`: ritual UX/debug options, haunting tuning, and undead behavior toggles once those systems mature.

Initial settings:
- global log level: off, errors, warnings, info, verbose
- per-subsystem logging: execution, costs, requirements, targeting, projectiles, persistent effects, area zones, wall zones, stat modifiers, force fields, enhancements
- show/hide debug spell gizmos outside dev mode if desired
- MFVanilla research policy: arcane research suppresses standard tech research, coexists with standard tech, or only gates magic content
- optional first-party content balance multipliers, such as mana cost, cooldown, scroll learning availability, and enhancement strength

Implementation notes:
- Use RimWorld `ModSettings` with version-tolerant defaults.
- Keep framework settings independent from first-party content settings where possible.
- Settings that affect generated defs may need restart/reload notices.
- Avoid settings that change save-critical semantics without clear warnings.

## P2 Content And Runtime Polish

### MF-018 MFVanilla Arcane Ink Production Chain

Goal: make scrollmaking depend on a produced ink resource, not only papyrus/parchment.

Content flow:
- Add growable exotic herb crop. Default label should probably be `exotic herbs`; `belladonna` can remain a darker flavor variant later.
- Add harvested herb item.
- Add `MFV_ArcaneInk`.
- Add an ink-making station or reuse/split existing arcane research production bench content.
- Add ink recipe and work giver.
- Update `GenerateSpellScrollDefs.ps1` so scroll recipes require one writing material plus one arcane ink.

Balancing placeholders:
- `MFV_MakeArcaneInk`: 8-12 exotic herbs plus 3-5 silver -> 1 arcane ink.
- Scroll recipe: 1 papyrus/parchment plus 1 arcane ink -> 1 scroll.
- Ink station research: likely `MFV_Alchemy` or `MFV_RunicInscription`.
- Crop research: likely `MFV_Papyrus` or `MFV_Alchemy`.

Texture needs:
- crop stages: `ExoticHerbs_Immature.png`, `ExoticHerbs_Growing.png`, `ExoticHerbs_Mature.png`
- harvested herb item
- arcane ink item
- ink station, preferably north/south/east variants if `Graphic_Multi`

### MF-019 Ritual Dialog Improvements

Goal: make AeternusFaith ritual setup clearer and more polished.

Possible first pass:
- Replace plain checkbox/radio lists with pawn rows that include portraits.
- Show why a corpse or conductor is unavailable.
- Surface reachability/reservation failure reasons where practical.
- Keep the UI compact enough for small screens.

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

### MF-022 Torches And Arcane Lighting

Goal: provide low-tech magical lighting content for fantasy colonies.

Content ideas:
- wall torch sconces
- arcane torches
- occult shrine/ritual lighting variants
- research or material gates if needed

Implementation notes:
- This is content-light if textures exist.
- It becomes larger if custom fuel, glow colors, refuel jobs, or magic-power mechanics are added.

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

### MF-028 Dedicated Common Status Primitive Review

Goal: review which common effects still deserve first-class action defs after the reusable status-def layer matures.

Candidates:
- ignite
- stun
- charm
- silence
- root / immobilize

Decision rule:
- Prefer `SpellStatusEffectDef` for reusable stat/status bundles.
- Add a dedicated primitive when authoring repeats, cleanup is subtle, or RimWorld behavior needs a wrapper.

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

Target coverage:
 - using System.Random, UnityEngine.Random, or time-based randomness without Multiplayer-safe syncing.
 - depending on real time, frame rate, local UI timing, thread timing, or machine-specific order.
 - changing game state from UI code.
 - running logic only on one client.
 - using dictionaries or unordered collections where iteration order could affect gameplay results.
 - async/network/API calls that affect gameplay state.
 - Harmony patches that alter core ticking, job assignment, combat, map generation, or pawn behavior in nondeterministic ways.
 - visual-only effects that accidentally touch gameplay state.
