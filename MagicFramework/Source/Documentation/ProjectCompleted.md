# Project Completed

This file tracks implemented framework and content milestones moved out of the active to-do list.

## Implemented Recently

- MFVanilla 0.8.2 / MagicFramework 1.3.1 progression QoL patch.
  Uploaded:
  - May 22, 2026.
  Current state:
  - Completing spell-unlocking arcane research drops one matching mystery scroll with a mysterious-aid message, giving each newly unlocked spell pool an immediate payoff without directly granting spells.
  - Generated spell scroll market values now scale by the full research prerequisite chain, roughly `200 x required research count`, making deeper scrolls significant purchases.
  - Elementalist trader stock and arcane treasure tables now make spell scrolls more reliably available without making them cheap.
  - Learning a spell from a scroll grants caster XP, and successful known-spell casts grant small caster XP through MagicFramework runtime support.
  - Arcane Gift pawns can apprentice under higher-level gifted mentors while the mentor performs qualifying arcane work: arcane research, scribing, alchemy, or enchantment.
  - Apprenticeship is exposed through normal RimWorld work/right-click behavior as "Prioritize learning from X"; multiple apprentices can learn from the same mentor.
  Validation:
  - Research scroll drop was tested and worked well in game.
  - Apprenticeship was tested in game and works after fixing the XP award from a brittle global-tick modulo check to accumulated apprenticeship time.
  Follow-up posture:
  - Watch whether combined scroll drops, trader stock, treasure rewards, casting XP, and apprenticeship make spell acquisition or caster leveling too fast.
  - Pawns incapable of study/research do not apprentice for now; leave this as an accepted identity boundary unless it becomes a concrete progression problem.

- MFVanilla 0.8 / MagicFramework 1.3 uploaded release band.
  Uploaded:
  - May 19, 2026 at 11 PM.
  Current state:
  - MFVanilla world-layer mission release is the last uploaded band.
  - Arcane Cache, Ruined Sanctum, and Sealed Vault opportunities are implemented through deterministic site generation, arcane treasure chest rewards, construct defenders, incident entries, and debug spawn/offer support.
  - Arcane Cache passed normal-play smoke testing around day 45, including event arrival and expected completion.
  - Deep Iron Golem was dev tested and produced a strong boss fight; automata were working fairly well in current testing.
  - Leyline Sensitivity, Arcane Discipline rituals, the elemental spell expansion, Elementalist tribe first pass, and updated splash notes shipped as part of the band.
  Follow-up posture:
  - Ruined Sanctum and Sealed Vault remain normal-play observation items for cleanup, reward extraction, save/load, repeat generation, Deep Iron Golem readability/drops, and tuning.
  - Gemstone reward economy remains a watch item after the first "few and valuable" tuning pass.

- MFVanilla caster-level growth hotfix.
  Current state:
  - MagicFramework caster runtime state now stores earned caster experience in addition to caster level.
  - `SpellRuntimeGameComponent.GainCasterExperience` advances caster level up to level 20 and saves the earned experience.
  - MFVanilla grants caster experience from arcane research performed at arcane research benches.
  - MFVanilla grants caster experience from completed recipes at the alchemy table, scribing table, and Arcane Forge.
  - Papyrus, parchment, and lapidary work intentionally do not grant caster experience.
  - Gift-awakening research still works for non-gifted pawns; caster experience begins once a pawn has the Arcane gift.

- MF-040 launch splash notes.
  Current state:
  - MagicFramework provides `SplashNoteDef` so framework and dependent mods can author important launch/version notes in XML.
  - Splash notes can reference a package ID and display that active mod's `About/About.xml` `modVersion`.
  - MagicFramework collects all active splash note defs into one dialog and shows it from startup and saved-game lifecycle checks when the combined active note key has not been seen.
  - Seen state is stored in `MagicFrameworkSettings.lastSeenSplashKey`, allowing new dependent-mod notes to trigger the dialog without repeatedly showing unchanged notes.
  - MagicFramework and MFVanilla both provide player-facing notes with important settings reminders, recent accomplishments, and planned-feature teasers.
  - MagicFramework and MFVanilla settings pages both include a button to re-show the latest notes.
  - MFVanilla notes call out the vanilla tech research suppression setting and where to restore standard tech research.

- Deterministic spell random utility.
  Current state:
  - `SpellDeterministicRandom` derives stable random-looking values from explicit gameplay salt such as spell seed, spell def, caster ID, map ID, targets, cells, and authored channels.
  - The utility supports hash values, float `0..1`, chance checks, inclusive integer ranges, float ranges, and deterministic shuffling.
  - Random-chance conditions, stun actions, random teleport destinations, explosion spawn rolls/cells, and chain-lightning branch/stun decisions now use deterministic framework-owned randomness instead of ambient `Rand`.
  - Chain-lightning pulses persist the original spell random seed so queued branches remain deterministic across save/load.
  - Framework-owned gameplay code should prefer this utility for future chance rolls, random ranges, random selection, and shuffling.
  Notes:
  - The utility is intended for gameplay decisions. Pure presentation jitter should remain harmless, but framework-owned chain visual jitter is also deterministic now.
  - Vanilla systems invoked by framework actions may still perform their own internal randomness.

- MF-011 projectile support.
  Current state:
  - `LaunchProjectileActionDef` launches vanilla RimWorld projectiles and supports authored hit flags plus friendly-fire prevention.
  - Projectile launch authoring can choose launch origin (`Caster`, `CurrentTarget`, or `CurrentCell`) and target source (`CurrentTarget`, `CurrentCell`, or `Caster`).
  - `onImpactActions` are tracked by map runtime state and run after projectile impact, explosion landing/detonation, projectile destruction, or timeout.
  - Projectile impact hooks capture exact hit thing and shield-block state where vanilla `Projectile.Impact(Thing hitThing, bool blockedByShield)` exposes them.
  - Impact context resolves to the hit thing when available, otherwise the projectile's last known cell.
  - Projectile impact execution variables include `ProjectileImpactCaptured`, `ProjectileImpactResult`, `ProjectileBlockedByShield`, and `ProjectileHitThing`.
  - `ProjectileImpactResult` distinguishes `HitThing`, `ShieldBlocked`, `ImpactNoThing`, `Destroyed`, and `Timeout`.
  - `MF_Firebolt` and `MF_Fireball` validate explicit caster-to-target projectile launch authoring.
  Completion note:
  - Cover interception is not kept as an active MF-011 blocker because the current vanilla hook surface exposes the impacted thing and shield-block state, while cover details are folded into vanilla combat logging/cover internals.
  - Arcing and overhead projectile behavior should remain projectile-def authoring through vanilla projectile fields such as `arcHeightFactor` and `flyOverhead`; add per-launch override support only if future content proves it needs runtime mutation.
  - Custom spell projectile classes are not needed for the current authoring surface; reopen as a new task only if vanilla projectiles cannot expose a future spell's required context.

- MF-005 lifecycle hook backfill.
  Current state:
  - `LifecycleHooks.md` defines shared create, pulse, trigger, expire, remove, break, and legacy end semantics.
  - Persistent area zones, wall zones, maintained force fields, persistent effects, and proximity triggers expose their explicit lifecycle hooks.
  - Summoned pawns now support `onCreateActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`.
  - Spawned things now support `onCreateActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`.
  - Summon and spawned-thing runtime records persist source action paths so lifecycle hooks still resolve after save/load.
  - Sustained stat modifiers retain their established `onPulseActions` and `onBreakActions`; create/expire/remove distinctions remain a future extension only if authoring needs them.

- MF-007 validation spell suite.
  Current state:
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
  - rescue teleport for downed allies: `MF_RescueRecall`
  - caster/target swap teleport: `MF_Transposition`
  - maintained enemy repulsion teleport with sustained and per-target mana costs: `MF_RepulsionWard`
  - terrain patch and thaw/melt behavior: `MF_Freeze`, `MF_FlameField`
  Completion note:
  - lifecycle-hook validation is tracked under MF-005 because it depends on the remaining hook surfaces being generalized.

- MF-006 target-query expressiveness.
  Current state:
  - reusable target query defs cover current target, radius, nearest valid target, shape targets, and directional chains
  - shared query handling supports deterministic ordering by nearest, farthest, lowest health, highest health, highest threat, and lowest threat
  - query defs support target limits through `maxTargets`
  - authored validation spells cover nearest hostile selection, allied radius selection with caster exclusion, lowest-health ordering, highest-threat ordering, and target limiting
  - future already-hit/visited target policy is covered by MF-012 branching-chain work rather than keeping MF-006 open

- MF-015 targeting and self-affect policy.
  Current state:
  - `TargetingPolicy.md` documents first-party conventions for beneficial pawn spells, hostile pawn spells, cell placement spells, mixed pawn/cell spells, persistent hazards, beneficial auras, and terrain-only pulses
  - the policy keeps XML explicit for now and defers inference from metadata tags until defaults are safer
  - audited `MF_Firebolt`, `MF_Haste`, `MF_Heal`, `MF_Freeze`, `MF_FlameField`, `MF_WatersEmbrace`, and `MF_SummonDog`
  - no behavior changes were needed from this audit

- MF-005 lifecycle hook semantics and area-zone first pass.
  Current state:
  - `LifecycleHooks.md` defines create, pulse, trigger, expire, remove, break, and legacy end semantics
  - persistent area zones support `onCreateActions`, `onPulseActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`
  - area-zone `onEndActions` remains supported as a legacy catch-all after expire, remove, or break terminal hooks
  - area-zone replacement/cancel is categorized as remove, natural duration end as expire, and invalid/concentration loss/marker loss as break
  - maintained force fields support `onCreateActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`
  - force-field replacement/cancel is categorized as remove, natural duration end as expire, and invalid/range/line-of-sight/mana maintenance failures as break
  - persistent wall zones support `onCreateActions`, `onPulseActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`
  - wall-zone replacement/cancel is categorized as remove, natural duration end as expire, and invalid caster or marker loss as break
  - persistent effects support `onCreateActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`
  - persistent-effect replacement/cancel is categorized as remove, natural duration end as expire, and marker loss as break
  - proximity triggers support `onCreateActions`, `onTriggerActions`, `onRemoveActions`, and `onBreakActions`
  - trigger replacement is categorized as remove, invalid runtime state as break, and `onTriggerActions` runs before the existing trigger body actions

- MF-004 sustained spell release UX.
  Current state:
  - player-known spells appear as full individual pawn gizmos, matching the debug spell presentation
  - known-spell gizmos and the known-spell menu toggle active maintained spells into `Release <spell>` commands
  - debug maintained-spell gizmos use the same release helper
  - clean release avoids break hooks while still ending the maintained state and cleaning status/visual runtime state
  - force-field release now routes through remove lifecycle hooks rather than being treated as a maintenance break
  - `SpellMaintenanceDef` supports composable interruption profiles with compatibility fallback when omitted
  - sustained stat modifiers, maintained force fields, and persistent area zones use shared maintenance profiles when authored
  - `MF_ForceField`, `MF_ManaShield`, `MF_Might`, and `MF_WatersEmbrace` now author explicit maintenance profiles
  - sustained stat modifiers and maintained force fields support `pulseIntervalTicks` plus `onPulseActions`
  - `MF_ManaShield` validates maintained force-field pulses with a harmless periodic visual pulse
  - area-zone release detection now only treats concentration or maintenance-profile area zones as maintained, so ordinary placed zones remain normal cast buttons while active
  Completion note:
  - future maintained wall zones, beams, or additional channeling spell families should be tracked as new work instead of reopening MF-004

- MF-008 subsystem logging toggles.
  Current state:
  - `MagicFrameworkSettings` saves per-subsystem routine logging toggles through RimWorld `ModSettings`
  - `MagicLog` routes routine diagnostics through named subsystems while keeping warnings/errors available through normal RimWorld logging
  - settings UI exposes toggles for execution, costs, requirements, targeting, triggers, persistent effects, wall zones, area zones, stat modifiers, displacement, projectiles, force fields, enhancements, visuals, and summons
  - the noisiest routine logs in execution, costs, scheduling, sustained effects, force fields, displacement, projectiles, visuals, summons, and action workers now honor the toggles
  - debug snapshot buttons remain available in the Magic Framework settings window

- MF-009 spell-level power scalars.
  Current state:
  - `SpellPowerDef` supports optional `SpellPowerScalarDef` entries for damage, healing, radius/range, duration, mana cost, and cooldown
  - omitted scalar defs resolve to a neutral factor of 1.0
  - scalar values default to `Linear` mode, using `baseValue + power * perPower` with min/max clamps
  - `ScalableFloatDef` and `SpellPowerScalarDef` can opt into `Flat`, `Linear`, or `Tiered` numeric scaling
  - `Tiered` numeric scaling uses `baseValue + tier * perTier`
  - structural power changes are supported through `PowerTierConditionDef` and `SpellPowerConditionDef` conditionals
  - central mechanics now honor spell-level scalars for damage, healing, radius/range, duration, mana costs, and cooldown costs
  - `MF_Firebolt` now validates the model with base damage/range/cooldown values plus spell-level damage, radius, and cooldown scalars
  - `MF_Fireball` now validates the same model across explosion damage, secondary damage, targeting radius, explosion radius, radius target queries, and cooldown

- MF-006 Blessing of Vigor radius-query validation spell.
  Current state:
  - `MF_BlessingOfVigor` is a learnable MFVanilla warding spell that fires from the caster without a target prompt
  - the spell uses `TargetsInRadiusQueryDef` centered on the caster with `pawnAffinity` set to `Ally`
  - the query sets `includeCaster` to `false`, validating caster exclusion for ally-radius spells
  - affected allies receive `MF_BlessedVigor`, movement speed, and general labor speed buffs
  - debug casting includes a Blessing of Vigor gizmo and fallback spell definition
  - generated MFVanilla spell scroll and recipe defs include Blessing of Vigor

- MF-007 Triage Pulse query-ordering validation spell.
  Current state:
  - `MF_TriagePulse` is a learnable MFVanilla vitalism spell that fires from the caster without a target prompt
  - the spell uses `TargetsInRadiusQueryDef` centered on the caster with `ordering` set to `LowestHealth` and `maxTargets` set to `1`
  - the selected ally receives a targeted healing pulse, validating ordered candidate selection and target limiting in normal authored XML
  - generated MFVanilla spell scroll and recipe defs include Triage Pulse

- MF-007 Threat Spike query-ordering validation spell.
  Current state:
  - `MF_ThreatSpike` is a learnable MFVanilla combat/control spell that fires from the caster without a target prompt
  - the spell uses `TargetsInRadiusQueryDef` centered on the caster with `ordering` set to `HighestThreat` and `maxTargets` set to `1`
  - the selected foe receives light damage and a brief stun chance, validating hostile ordered candidate selection and target limiting in normal authored XML
  - generated MFVanilla spell scroll and recipe defs include Threat Spike

- MF-006 shared target-query ordering and limits.
  Current state:
  - base `TargetQueryDef` supports optional `ordering`, `orderingCenterSource`, and `maxTargets`
  - radius, nearest-valid, and shape queries use a shared collect/filter/order/limit pipeline
  - supported ordering modes are `Nearest`, `Farthest`, `LowestHealth`, `HighestHealth`, `HighestThreat`, and `LowestThreat`
  - query ordering uses stable tie-breakers so equal candidates resolve deterministically
  - `NearestValidTargetQueryDef` defaults to nearest ordering from the current cell with one target, preserving existing behavior

- MF-006 Arc Seeker target-query validation spell.
  Current state:
  - `MF_ArcSeeker` is a learnable MFVanilla aeromancy spell that fires from the caster without a target prompt
  - `SpellTargetingDef.useCasterAsTarget` lets authored spells execute immediately with the caster as the initial target
  - the spell uses `NearestValidTargetQueryDef` to select the nearest hostile pawn within 7 cells of the caster
  - selected targets are struck through `ChainLightningActionDef` with zero hops, reusing chain lightning visuals
  - selected targets receive burn-type shock damage and a brief stun chance
  - debug casting includes an Arc Seeker gizmo and fallback spell definition
  - generated MFVanilla spell scroll and recipe defs include Arc Seeker

- MF-004 sustained force-field mana upkeep.
  Current state:
  - `ApplyForceFieldActionDef` supports optional `sustainedManaCost` and `sustainedManaCostIntervalTicks`
  - maintained force fields spend upkeep mana on interval ticks and break with cleanup/actions when the caster cannot pay
  - mana-backed force fields also break immediately when they cannot afford an incoming damage absorption/reduction cost, preventing lingering maintained visuals after the shield fails
  - upkeep state is saved on active force fields through `nextSustainedManaCostTick`
  - `MF_ManaShield` now costs 1 mana every 60 ticks while maintained, in addition to mana spent absorbing damage
  - generated spell summaries mention sustained upkeep costs when configured

- MF-001 teleport drafted-state continuity.
  Current state:
  - `TeleportActionDef` preserves drafted state by default after teleporting or swapping pawns
  - teleport still clears current pathing and busy stance instead of restoring volatile job/path state
  - `postTeleportStunTicks` lets authored teleports explain job interruption as disorientation
  - `MF_BlinkStep` preserves drafted state and applies a brief 30-tick post-blink stun
  - debug Blink Step fallback mirrors the authored behavior

- MF-013 collision-aware knockback.
  Current state:
  - `KnockbackActionDef` traces push movement until the target reaches full distance or collides with a blocked cell
  - collision-aware push lands the target on the last valid destination and can apply authored impact damage
  - impact damage supports authored damage def, armor penetration, and guilt policy
  - `MF_ForcePush` now deals light blunt impact damage when the shove ends against an obstacle
  - generated spell summaries mention collision damage when configured

- Dynamic spell effect summaries, first pass.
  Current state:
  - `SpellDescriptionUtility` generates cached plain-language summaries from loaded `SpellDef` action trees
  - supported summaries include targeting, mana/cooldown costs, damage, healing, explosions, hediffs, stat modifiers, sustained effects, force fields, persistent zones, terrain patches, summons, spawned things, delays, repeats, projectiles, movement, teleport, chain, stun, and destroy actions
  - known-spell gizmos append generated effect summaries below authored spell descriptions
  - MFVanilla spell scroll inspect text shows the generated effect summaries for the spell the scroll teaches
  Remaining gaps:
  - no dedicated spell details window yet
  - contextual enhancement modifiers are not displayed yet
  - some complex conditions and target queries still fall back to broad common-language summaries

- Enhancement scalar integration.
  Current state:
  - `SpellEnhancementUtility` now exposes shared resolver helpers for damage, radius, and duration factors
  - `damageFactor` applies to direct `DamageActionDef`, `ExplosionActionDef`, and fallback chain-lightning damage
  - `radiusFactor` applies to spell targeting range, target-query radii, explosions, persistent area zone radius, persistent wall pulse radius, terrain patch radius, stone-chunk movement radius, and chain jump radius
  - `durationFactor` applies to repeat intervals, persistent effects, wall zones, area zones, summons, spawned things, timed stat modifiers, sustained stat modifiers, maintained force fields, and scheduled hediff removal durations
  - mana and cooldown factors continue to use the existing enhancement resolution path
  Validation notes:
  - `MFV_SolarFlareEmpowersFireMagic` now affects fire damage/radius/duration where matching actions expose those values
  - `MFV_RainEmpowersAquamancy` and `MFV_HillsEmpowerGeomancy` now affect matching aura/terrain/radius/duration behavior where authored values exist

- Explicit Harmony dependency metadata.
  Current state:
  - MagicFramework `About/About.xml` declares `brrainz.harmony` in `modDependencies`
  - MagicFramework also lists `brrainz.harmony` in `loadAfter` so RimWorld orders Harmony before the framework
  - package ID was verified against the locally installed Harmony metadata

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
  - visited target policy can either allow repeated hits or globally exclude previously hit targets across queued branches
  - branch count, candidate shuffling, and fallback stun rolls use deterministic spell random state
  - vanilla electrical spark flecks provide a jagged trail, impact flash, and stun cue
  Validation spell:
  - `MF_ChainLightning`, using authored damage and deterministic stun hit actions while globally excluding previously hit targets

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

- MF-022 Torches and arcane lighting MVP.
  Current state:
  - `MFV_MagicTorchLamp` provides a freestanding arcane torch analogue with blue glow, fire overlay, modest heat, meditation flame focus support, and `MFV_Enchantment` research gating
  - `MFV_MagicTorchWallLamp` provides the wall-mounted sconce analogue using wall-attachment placement, blue glow, fire overlay, modest heat, meditation flame focus support, and `MFV_Enchantment` research gating
  - both use existing torch/wall-torch art as crude first-pass visuals and arcane focus material costs
  Remaining polish:
  - no bespoke AeternusFaith torch/sconce textures yet
  - no occult shrine or ritual-room lighting variants yet
  - no custom fuel, refuel work, glow color variants, or magic-power mechanics yet

- MF-036 Fixed Custom Aeternus Faith role names.
  Current state:
  - Aeternus role namer rule packs now contain a single `r_roleName` option per role
  - generated titles are fixed as high theomarch, voice of balance, soulwarden, verdant keeper, and forgewarden
  Note:
  - existing saved ideologies may keep already-generated role names until regenerated or edited

- MF-037 Aeternus ritual-circle research gates.
  Current state:
  - the Aeternus Faith research tab includes `AF_FaithRituals`, `AF_OssanithTraditions`, and the cathedra follow-up projects
  - Ossanith circle, ossuary bone box, and lectern require `AF_OssanithTraditions`
  - Animara ritual center and lectern require `AF_AnimaraSoulbinding`
  - Shroudhymn ritual center and lectern require `AF_ShroudhymnOaths`
  - Animara and Shroudhymn edge/corner pieces are hidden from build menus and research unlock lists; they are placed through the center gizmo
  Naming note:
  - the implemented research def is `AF_OssanithTraditions`; the broader Bonewright cathedra are represented by separate follow-up research projects rather than a single `Sacred Rituals of the Bonewrights` project

- MF-017 Mod settings MVP.
  Current state:
  - MagicFramework exposes a RimWorld settings category with colored generated spell text, caster-power scaling sliders, per-subsystem routine logging toggles, bulk logging enable/disable buttons, and debug snapshot buttons for delayed runtime, armed triggers, persistent effects, wall zones, and area zones
  - `MagicFrameworkSettings` persists those options through RimWorld `ModSettings` with version-tolerant defaults and a runtime `Current` accessor
  - MFVanilla exposes a separate RimWorld settings category for vanilla tech research suppression and warning behavior, with live patch notification when settings change
  Remaining polish:
  - settings are currently separate RimWorld mod categories rather than one literal multi-tab settings window
  - AeternusFaith does not have its own settings category yet
  - debug gizmo visibility, compatibility toggles, scroll/content toggles, and broader MFVanilla balance multipliers remain future polish

- MF-018 MFVanilla arcane ink production chain MVP.
  Current state:
  - `Plant_MFV_ExoticHerbs` provides a growable exotic herb crop gated by `MFV_ArcaneTheory`
  - `MFV_ExoticHerbs` provides the harvested herb item used by alchemical recipes
  - `MFV_ArcaneInk` provides the scrollmaking ink resource with item art
  - `MFV_AlchemyTable` provides the production bench for alchemical reagents and is gated by `MFV_Alchemy`
  - `MFV_MakeArcaneInk` turns 10 exotic herbs into 1 arcane ink at the alchemy table
  - `GenerateSpellScrollDefs.ps1` now writes scroll recipes that require 1 writing material plus 1 `MFV_ArcaneInk`
  - generated spell scroll recipes were regenerated after the generator update
  Remaining validation:
  - smoke test the full in-game loop: research, grow/harvest exotic herbs, make arcane ink, and scribe a scroll
  - tune balance after playtesting if herb yield, work amount, or ink cost feels too strict or too loose

