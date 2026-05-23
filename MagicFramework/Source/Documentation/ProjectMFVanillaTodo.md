# MFVanilla Todo And Release Backlog

Detailed MFVanilla planning extracted from ProjectTodo.md. Keep the compact release dashboard in ProjectTodo.md authoritative for what is next; use this file for task context and design notes.

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
- Arcane foci are now split by the first four elemental production paths: fire foci from ruby, water foci from sapphire, earth foci from emerald, and air foci from topaz. The flaming longsword recipe now consumes a fire arcane focus.
- Magic heaters and torches now consume fire foci, magic passive coolers consume water foci, and arcane spires consume air foci.
- The Arcane Forge is now an active enchantment bench gated by linked arcane spires. Its first weapon set includes Flaming Longsword, Zephyr Spear, Tidebreaker Mace, and Stonefall Mace, all transformed from good-or-better mundane weapons while preserving final quality.
- Arcane Forge setup costs have been tuned down for release readiness while keeping the four-spire requirement as the advanced enchantment gate.
- Arcane Forge smoke testing confirmed build requirements, linked-spire gating, bill availability, and the first enchanted weapon set are working well.
- Enchanted weapon products now assign the enchanter as the art author when quality-generated art is present, avoiding unknown authors on high-quality outputs.
- Trader stock smoke testing confirmed relevant MFVanilla production stocks are appearing in tested traders. Shaman trader direct dev-spawn testing was blocked by dev UI availability, so shaman stock remains a watch item during normal playtesting rather than a release blocker.
- Gemstone vein availability, mining output, market values, and stack sizes are provisionally accepted for the current release pass. Keep watching gemstone dust and elemental focus abundance during broader progression and treasure testing.
- Playtest reward watch item: an unrelated mission offered about 17 fine topaz worth roughly 366 total value. Initial "few and valuable" tuning raised cut gemstone values, raised rough/raw/focus values to keep the production chain coherent, reduced arcane treasure gemstone rolls, and lowered gemstone dust cache counts. Continue watching external quest reward stacks.
- Elemental foci should remain player-crafted only for now, preserving them as an industrial requirement and gate for powerful magical items.
- Enchanted weapons can appear as expensive, rare commodities in world rewards, loot, or limited trader contexts; tune availability so colony crafting remains the reliable path.
- Major scroll gemstone requirements should follow elemental association and the elemental research that exposes the spell; for example, pyromancy spells use ruby-derived requirements. Non-elemental major spells can accept any elemental focus as a flexible advanced reagent.
- Full-loop progression smoke testing is provisionally accepted for this pass: dust, herbs, ink, papyrus/parchment, scroll scribing, scroll learning, gemstone cutting, focus crafting, and weapon enchanting remain watch items during normal balance testing.
- MFVanilla 0.8.2 corrected the most painful spell acquisition bottlenecks: finishing spell-unlocking research drops one matching mystery scroll, Elementalist traders carry more scrolls, arcane treasure rewards are more scroll-friendly, and generated scroll market values now scale from the full research prerequisite chain.
- Caster progression has more natural paths after 0.8.2: learning a spell from a scroll grants a breakthrough XP bump, successful known-spell casting grants small XP, and lower-level Arcane Gift pawns can apprentice under higher-level gifted mentors while the mentor performs arcane research, scribing, alchemy, or enchantment work.
- Apprenticeship smoke testing confirmed the player-facing job works after fixing its XP timing. Multiple apprentices can learn from the same mentor; pawns incapable of study/research are not expected to apprentice for now.

Priority:
- review the full production chain from raw inputs through finished scrolls and magic utility items
- make sure each bench has a clear role, research position, recipe set, work type, texture, cost, and power/facility story
- tune Arcane Ink, parchment/papyrus, gemstones, herbs, and generated spell scroll costs so the early loop is useful and the advanced loop has meaningful investment
- check that research gates unlock recipes, benches, utility buildings, and spell access in a sensible order
- add missing descriptions, inspect strings, category placement, tradeability, stack sizes, market values, and bulk/storage behavior where content feels placeholder
- smoke test a new colony path: discover Arcane gift, unlock early research, craft inputs, make scrolls, learn spells, and progress into advanced production

Next steps:
- Watch production pacing, reagent abundance, and player clarity during normal playtesting; reopen MF-037 only for concrete release-blocking issues.
- Watch the new scroll and apprenticeship pacing during normal play. Reopen only if spell access becomes too fast, apprenticeship crowds out normal colony work, or non-research pawns needing an alternate training path becomes a concrete problem.

Success criteria:
- a player can understand what to build next without dev knowledge
- every production building earns its footprint in the colony
- scroll learning and generated scroll recipes feel like part of the same economy as the spells
- the content works as a stable example for third-party spell authors


### MF-038 MFVanilla Feature Completion

Goal: keep the near-term MFVanilla content roadmap current after completed feature pillars, and identify which remaining release ideas should become concrete tasks.

Status:
- Partially superseded by follow-up tasks. Arcane treasure chests were promoted into MF-042 and are complete for the current release pass.
- Arcane Forge first-edition weapon work is tracked by MF-041.
- Encounter maps and construct enemies are tracked by MF-049 and MF-050.
- Current cleanup pass removed completed MF-045, MF-046, and MF-061 work from the active index; their remaining concerns are now release hygiene or future follow-up tasks.
- This item remains open as a planning and cleanup bucket, especially for research nodes and stale documentation that no longer match live content.

Next-release content direction:
- Favor content definition before final economy tuning. Balance should happen after the intended release content set is clearer.
- Treat completed elemental tribes, leyline maps, and school expansion as release-polish surfaces unless testing exposes concrete follow-up tasks.
- Treat special enchanted weapon behavior as mostly implemented first-pass content; remaining work should focus on inspect text, smoke testing, art details, and balance.
- Audit research fields with no current application. Either attach real content to them or remove/hide them until they have a purpose.
- Reconsider plasteel/component costs on arcane benches and infrastructure after the content set is known. Prefer magical, pre-industrial, or trade-economy materials where they preserve meaningful scarcity.
- Bring `Mods/MFVanilla/Documentation/ModPlan.md` back in sync with live XML before treating it as authoritative; it still references removed or renamed research nodes such as old runic/infrastructure planning.

Good candidates:
- more utility recipes or buildings that consume magic production outputs
- targeted Arcane Forge expansions that reuse the elemental focus model without becoming a broad equipment framework
- one or two additional spell families only if they validate already-supported primitives or a narrowly needed framework hook
- stronger integration between spell metadata, scroll recipes, research, and generated descriptions
- clearer player feedback for spell scaling, active enhancement rules, and unlock paths
- balance pass for mana, cooldowns, costs, work amounts, and resource scarcity
- concrete follow-up tasks for underused research nodes, especially Leyline Sensitivity, Illusion, Fleshcraft, Planar Magic, Infernal Pact, Grand Sorcery, and Chronomancy

Deferral rule:
- defer anything that primarily exists to prove a speculative framework system rather than make MFVanilla better now
- defer hostile AI casting, magic weapons/tools, real fire integration, and celestial event depth until after MFVanilla and AeternusFaith first-edition goals are in hand


### MF-042 Arcane Treasure Chests

Goal: add an arcane treasure chest system that makes MFVanilla content appear as mission rewards, rare loot, and occasional world/map discoveries.

Status: complete for the current release pass; future cursed/trapped variants remain deferred expansion work.

Priority:
- this is the favored next MFVanilla content system before broad tuning
- use treasure chests to make scrolls, gemstones, enchanted weapons, silver, gold, and future magic items feel discoverable outside colony production
- scale rewards with mission value and game advancement so early chests are useful without trivializing late rewards

Target behavior:
- a chest opens into a pseudo-random reward bundle with approximately one major item, two minor items, a few gemstones, silver, and gold
- major rewards can include rare scrolls, enchanted weapons, future staves/wands/apparel, or artifacts
- minor rewards can include arcane ink, papyrus/parchment, exotic herbs, gemstone dust, cut gemstones, or common scrolls
- reward generation should account for game stage, quest/reward value, map wealth, storyteller points, or an authored chest tier
- generation must be deterministic and multiplayer-friendly once the chest exists, so save/load or multiple clients cannot reroll the same chest unexpectedly
- every spawned chest should store a unique stable ID/hash plus tier/value metadata; the reward generator should derive all pseudo-random choices from that stored data rather than ambient `Rand`
- chests should be usable by quests, ancient/ruin-style placement, traders/loot sources if appropriate, and possible dev/test spawning
- future trapped/cursed chest variants should use the same stable ID/hash path so curse outcomes are deterministic and multiplayer-friendly

Implementation questions:
- should the chest be a minifiable thing, haulable item, building-like container, or use-effect item?
- should rewards be generated when the chest spawns, when it is opened, or generated from a stored seed/tier at spawn and realized on open? - preferred: store seed/id at spawn, realize deterministic loot on open
- should there be separate chest defs for common, fine, excellent, masterwork, and legendary arcane caches, or a single def with comp properties?
- should hostile maps, ancient dangers, quests, and traders use the same loot table with different tier/weight inputs?
- should the chest ever be trapped, cursed, faction-owned, or locked behind research/Arcane Gift?
- can existing trader stock generator/ThingSetMaker mechanisms provide enough weighted selection, or is a small custom loot-table def simpler and more transparent?
- should curse traps be authored as separate trap tables, mixed into treasure tables, or implemented as an optional chest comp that runs MagicFramework-style effects on open?

First implementation pass:
1. Define the chest thing and opening UX. - initial `MFV_ArcaneTreasureChest` and `MFV_GreaterArcaneTreasureChest` item defs added
2. Add a comp that assigns and saves a stable chest ID/hash on spawn, plus tier/value metadata. - initial saved stable ID and tier support added
3. Build a deterministic generator that uses the saved chest ID/hash as its pseudo-random seed and never depends on ambient `Rand` during reward selection. - initial local deterministic hash/random path added
4. Compare reuse of trader/stock generator tables against a custom MFVanilla loot-table def; choose the simpler path that keeps results deterministic and author-friendly. - chose custom XML `ArcaneTreasureTableDef` for first pass
5. Build reward tables for major, minor, gemstone, silver, and gold buckets. - roll-band table added with XML-editable buckets, 1d100-style roll ranges, chest/bucket roll bonuses, counts, weights, and optional quality
6. Add dev/debug spawning and logging for generated contents. - sufficient validation coverage exists through direct testing and spawned-site generation for this pass
7. Smoke test XML load, chest use job, stable inspect ID, spawned reward placement, save/load repeatability, and opened chest destruction. - confirmed chest generation works, treasure release is stable and constant across saves, and spawned rewards behave properly
8. Wire into one safe acquisition path first, then broaden to quests/loot. - initial rare quest reward eligibility and ancient temple/ancient complex loot patches added; spawned-site generation confirmed; no trader inventory integration
9. Hide raw deterministic chest IDs from normal player-facing inspect text while retaining dev-mode diagnostics. - done; chests now show authored cache magnitude labels
10. Add a third/highest chest tier for rarer, higher-magnitude rewards. - initial `MFV_GrandArcaneTreasureChest` added
11. Add broader high-band rewards: quality armor/weapons, jade, devilstrand cloth, high-quality small art, and rare materials. - initial table entries added
12. Later: add cursed/trapped chest support, preferably as deterministic XML-authored curse tables or MagicFramework action lists triggered on open. - deferred

Success criteria:
- opening a chest feels like a magical reward, not a plain resource bundle
- rewards are useful at multiple colony stages without breaking the production loop
- generated output is stable across save/load
- generated output is deterministic from the stored chest ID/hash and tier/value data, supporting multiplayer-friendly behavior
- loot tables are easy to extend as MFVanilla gains staves, robes, wands, familiars, artifacts, or more spell schools


### MF-043 MFVanilla Next-Release Content Pillars

Goal: make the next MFVanilla release a small, playable mission set that turns existing arcane rewards, encounter maps, and construct defenders into normal in-game opportunities before final economy tuning.

Release posture: implementation-complete enough for the next MFVanilla release if focused smoke tests remain clean. Do not expand this slice into the larger connected-room generator, hazard ecosystem, faction sender system, or elemental tribe work before release.

Playtest status:
- Arcane Cache normal-play smoke test passed around day 45: the event appeared, the site was completable, and the reward/completion flow behaved as expected.
- Ruined Sanctum and Sealed Vault are provisionally trusted through their shared implementation path, but should remain watch items during ongoing play.

Current band target:
- Prove the implemented natural incidents create usable world sites at appropriate campaign timings.
- Smoke test Arcane Cache, Ruined Sanctum, and Sealed Vault end to end: incident letter, world site creation, caravan travel, map generation, construct combat, chest reward, leaving the map, expiry/site cleanup, save/load, and repeat generation.
- Tune only release-facing values: base chance, earliest day, refire days, distance bands, threat points, defender mix, reward tier, mission text, and splash/update notes.
- Treat new site families, connected-room generation, active hazards, faction senders, leyline sites, elemental shrines, and cursed archives as later MF-049/MF-049B work.

Selected pillar:
- Arcane missions: complete a small set of world-map mission opportunities built around `MFV_ArcaneCache` first, then add two lightweight variants that reuse the same deterministic site generation, reward chest, and defender infrastructure.

Mission set:
- Arcane Cache: the first normal mission path for `MFV_ArcaneCache`; a compact treasure-cache site with construct defenders and an arcane treasure chest. - implemented with normal incident generation, timeout, letter, site/gen-step path, debug spawn/offer actions, threat-gated profile selection, and construct defenders
- Sealed Vault: a higher-threat cache variant that favors `MFV_ArcaneCache_Sealed` or `MFV_ArcaneCache_DeepIronVault`, uses greater/grand chest rewards, and appears later or at higher storyteller points. - initial `MFV_SealedVault` mission now locks to `MFV_ArcaneCache_DeepIronVault`, uses the grand chest profile, and showcases the Deep Iron Golem as the vault guardian
- Ruined Sanctum: a lower-to-mid threat exploration variant using the ruined/tower module pieces, lighter defenders, and side-room dressing or minor loot. - initial `MFV_RuinedSanctum` mission now uses a larger dedicated profile with side rooms, heavy exterior ruins, broken wall gaps, stone golem-focused defenders, its own incident, and debug spawn/offer actions

Out of scope for this band:
- Arcane loot/reward system expansion; the current arcane treasure chest pass should be validated through missions rather than reopened broadly.
- New elemental school spells; MF-046 is complete for this release and future ideas such as Fly, Summon Golem, and Earthy Grave should become later school-identity tasks.
- New leyline mission sites or leyline incident hooks; MF-044 should only be smoke tested as a support feature.
- New Elementalist faction behavior; MF-045 only needs trader/caravan smoke testing unless testing exposes a release blocker.
- New enchanted weapon mechanics; MF-047 should stay to inspect text, art details, weapon-special validation, and balance.
- Broad research-tree redesign; only fix misleading or broken unlocks that affect this release.

Effort plan:
1. Confirm vanilla-friendly mission entry point: choose quest script, incident, or storyteller-driven site opportunity for spawning `MFV_ArcaneCache` in normal gameplay. - done through natural `IncidentDef`/`IncidentWorker` mission entries for Arcane Cache, Ruined Sanctum, and Sealed Vault
2. Implement the first player-facing Arcane Cache mission path using the existing `SiteMaker`/`SitePartDef`/`GenStepDef` flow; include timeout, threat points, arrival text, and reward extraction expectations. - initial storyteller incident worker now creates `MFV_ArcaneCache` sites, sends a player-facing letter, registers an 18-day expiry, and exposes a debug action for the same mission path
3. Add mission tuning knobs: minimum days, threat-point bands, distance from colony, repeat cooldown, optional research/progression gate, and dev-mode spawn/log actions for the same path. - initial incident tuning uses earliest day 8, base chance 0.35, 18-day refire, 4-18 tile distance, 300+ threat points, and one-active-cache gating
4. Split authored mission variants only where needed: normal cache, ruined sanctum, and sealed/deep vault can start as profile-driven variants before receiving separate site parts.
   - Sealed Vault now has its own site part, linked gen step, incident worker, incident def, 28-day expiry, 8-24 tile distance, earliest day 45, 45-day refire, and debug spawn/offer actions.
   - Arcane site profiles now support weighted, threat-gated defender entries with per-kind caps, so automata count and type vary with game progress while keeping each generated site deterministic.
   - Ruined Sanctum now has its own site part, linked gen step, incident worker, incident def, 22-day expiry, 5-20 tile distance, earliest day 22, 28-day refire, and reusable broken-wall generation for ruined profiles.
5. Smoke test end to end: incident generation, world site creation, caravan arrival, map generation, construct combat, chest opening, leaving the map, site cleanup, save/load, and repeat generation. - Arcane Cache confirmed in normal play around day 45; continue observing Ruined Sanctum, Sealed Vault, cleanup, save/load, and repeat generation.
6. Tune mission frequency and reward tier after several natural-generation tests, then update splash notes and completed-work notes.

Estimated effort:
- Arcane Cache natural mission path: implemented; remaining effort is smoke testing, frequency tuning, reward/threat tuning, and cleanup validation.
- Small three-mission set: current release target. The map generation and incident entries exist; player-facing scheduling, variant identity, reward/threat tuning, and smoke testing across multiple site profiles are the remaining risk.
- Broader mission ecosystem with unique hazards, leyline sites, elemental shrines, cursed archives, and faction senders: `XL`; explicitly deferred to MF-049/MF-049B after the small set ships.

Sequencing note:
- avoid deep balance work until natural mission frequency, reward tiering, repeat frequency, and site cleanup behavior are visible in normal play.
- remaining MF-043 work should be polish and validation only: mission text, defender/reward tuning, site cleanup, save/load checks, and update notes for the release.


### MF-049 Arcane Encounter Maps And Mission Sites

Goal: build pseudo-random arcane-themed mission maps as a high-quality MFVanilla content pillar, not just generic item stashes with magic loot.

Status: the current three-site release pillar is tracked by MF-043. Keep MF-049 as the broader follow-up bucket for new site families, hazards, elemental shrines, leyline sites, cursed archives, and richer authored profiles after the Arcane Cache/Ruined Sanctum/Sealed Vault release is stable.

Feature vision:
- create world sites and mission maps that feel like magical places: arcane caches, ruined sanctums, sealed vaults, leyline ruptures, elemental shrines, cursed archives, and later faction/tribe-themed ritual sites
- use deterministic site/map seeds so layout, loot, curse/trap outcomes, and major encounter choices are stable across save/load and multiplayer-friendly
- make treasure chests one reward anchor inside the larger encounter, rather than the entire encounter
- keep early vanilla-mechanoid validation as a historical fallback only; current release-facing sites should present MFVanilla construct identities
- support authored XML profiles for layout themes, loot tables, hazard/trap tables, prop sets, and future defender sets

Vertical slice: Arcane Cache Site:
1. Add a basic `SitePartDef` / quest opportunity that creates a small arcane cache site on the world map. - initial `MFV_ArcaneCache` site part and linked gen step added
2. Generate one compact encounter map with a clear goal: reach and extract an arcane treasure chest. - initial cache chamber generator added
3. Place deterministic rewards using the existing arcane chest system. - initial standard chest placement added
4. Add a minimal threat or obstacle using existing RimWorld site threats first, with mechanoids as the temporary default defender family. - initial scyther/lancer/pikeman defenders added
5. Smoke test quest creation, map generation, chest opening, site cleanup, save/load, and reward extraction. - dev-spawned arcane cache site confirmed working: one room, mechanoid defenders, decorations, and centered chest

Map generation architecture:
- start with a small custom `GenStep` or symbol-resolver path rather than loose item scatter
- keep generation deterministic from site tile, quest/site ID, and authored profile data
- place a readable structure: entrance, approach space, vault/chamber, loot anchor, optional side room, debris/ruin dressing, and threat/hazard positions
- use existing walls/floors/props first, then add arcane-specific props as needed
- prefer XML-authored profiles so future mods can add new encounter themes without changing code

Content layers:
- rewards: arcane treasure chests, scrolls, gemstones, enchanted weapons, silver/gold, and future artifacts
- props: broken arcane spires, inert forge fragments, glyph floors, arcane torches, ritual stones, ruined benches, leyline markers, sealed containers
- hazards: curse traps, fire/ice/lightning fields, dormant runes, unstable mana nodes, trapped doors/chests
- defenders: current release sites use MFVanilla automata and the Deep Iron Golem; later site families can add elemental guardians, summoned creatures, or magic-capable hostile pawns when those systems have enough support
- deferred themes: leftover magic misfires and malfunctioning arcane spires that jolt nearby pawns fit a later war-torn/unstable magic site theme, not the first treasure-cache encounter

Implementation phases:
1. Design arcane site profile defs: theme, layout size, loot tier, chest def, hazard chance, defender/threat tags, and prop tables. - initial `ArcaneSiteProfileDef` supports room size, floor, wall stuff, chest def, defender pawn kinds/count, and fixed dressing placements
2. Implement the Arcane Cache Site vertical slice with a single profile and one chest. - current cache generator now reads `MFV_ArcaneCache_Default` while preserving the tested single-room cache behavior
3. Add deterministic generation diagnostics in dev mode so a site can report its seed/profile/chest ID. - dev-mode generation log now reports profile, tile, threat points, room, chest def/location, chest ID, and spawned defenders
4. Add reliable dev/test spawning for arcane cache sites. - `MFVanilla - Arcane Sites` debug actions now spawn a proper `SiteMaker` arcane cache near the current map and can remove bare empty sites left by generic dev spawning
5. Expand into Arcane Ruin Generator: multiple rooms, themed dressing, side loot, and optional hazards. - initial profile-driven circular tower layout support added; default arcane cache now uses a round wizard-tower chamber footprint, deterministic stone wall variation, a real door, and less crowded torch/spire dressing; smoke test confirmed clear torches and marble wall generation
6. Add curse/trap integration using XML-authored trap tables or MagicFramework action lists. - deferred beyond the current three-site release
7. Add themed variants: fire shrine, flooded vault, earth-buried sanctum, wind-swept ruin, leyline node, cursed archive. - deferred beyond the current three-site release
8. Integrate higher-tier sites with quest points/game stage and future elemental tribes or magic AI when those systems exist. - keep out of the current release band unless needed for bug fixing

#### MF-049A Arcane Site Generation Utility

Goal: turn the current arcane cache generator into a reusable, deterministic, profile-driven site layout utility for MFVanilla encounter maps.

Reason:
- the first cache site now works, but entry paths, exterior ruins, side rooms, reward anchors, defenders, and future hazards should not become one-off hardcoded branches
- reusable modules let arcane caches, ruined sanctums, sealed vaults, elemental shrines, cursed archives, leyline nodes, and later war-torn magic sites share generation infrastructure
- XML-authored profiles should let future site variants be added or patched without C# changes unless a genuinely new module behavior is needed

Utility scope:
- main-room modules: circular tower chamber first, with rectangular vaults and other footprints later
- optional layout modules: entry path, exterior ruin dressing, side room or annex, secondary loot alcove, interior dressing, reward anchor, and defender placement
- deterministic module selection and material selection from stored site identity/profile/quest inputs, not ambient `Rand`; separate generated sites should vary while each individual site remains stable after creation
- profile fields for module weights, fixed module inclusion, room dimensions, material pools, prop tables, defender tables, and reward chest tier
- dev diagnostics that report selected profile, modules, material choices, reward anchors, and defender placement

First utility pass:
1. Define `ArcaneSiteModule` metadata and profile fields for fixed modules plus optional weighted modules. - initial fixed room-module definitions added for axis, room kind, width, depth, and tower distance
2. Refactor the current tower generation into a `MainRoom` module while preserving the tested arcane cache output. - current circular tower remains the main room and can use separate tower wall material options
3. Add `EntryPath` as the first optional module: short stone path or paved landing leading to the door. - first deterministic entry path module added; generator now derives a stable per-site seed from site ID, creation tick, tile, site part, and profile for variation between spawned sites
4. Add `ExteriorRuin` as the second optional module: rubble, stone chunks, broken wall stubs, and light terrain cleanup around the tower. - first deterministic exterior ruin scatter added
5. Add `SideRoom` only after entry/exterior modules smoke test cleanly. - showcase profile now exercises axis rooms: south antechamber, east bedroom, west servants quarters, and north storage placeholder; polish passes route entry path through the antechamber, add an exterior antechamber door, push beds toward walls, clear natural/mineable rock from generated rooms, and add shelves/dresser/end-table plus low-value household storage dressing
6. Keep hazards, magical misfires, malfunctioning spires, curse traps, and unstable mana nodes deferred until a site theme specifically asks for them.

Current first consumer:
- `MFV_ArcaneCache_Default` remains the normal first profile and should stay a compact treasure-cache encounter: readable tower, chest, modest defenders, light exterior dressing, no active magical hazards.
- Normal `MFV_ArcaneCache` generation now uses seeded, weighted, threat-gated profile entries for player-facing variants: `MFV_ArcaneCache_Default` is common from the start, `MFV_ArcaneCache_Ruined` appears from mid threat, and `MFV_ArcaneCache_Sealed` appears rarely at higher threat; the showcase profile remains excluded from normal selection.
- `MFV_ArcaneCache_Showcase` is the oversized debug/test profile for validating all current modules at once: axis rooms, antechamber, bedroom, servants quarters, storage, entry path, exterior ruin, reward anchor, and defenders.
- Debug actions can spawn either the normal arcane cache or the showcase cache through the safe `SiteMaker` path.

Enemy bridge:
- historical bridge used vanilla mechanoid defenders for early Arcane Cache validation so combat, pathing, down/death behavior, threat scaling, and site integration were known-good
- current release sites should use the MF-050 automata/construct family instead of vanilla mechanoid identities
- keep any remaining vanilla-mech fallback local to arcane mission construction; do not globally replace vanilla mechanoids

Success criteria:
- the first arcane site feels like a real mission location, not a chest spawned in a field
- generation is stable enough for save/load and multiplayer-friendly play
- content authors can add or patch encounter profiles through XML
- rewards, threats, and hazards scale with mission tier without replacing the normal production loop
- the system can grow into elemental sites, cursed vaults, and faction/tribe encounters without a rewrite

#### MF-049B Connected Arcane Ruin Generator

Goal: future-release work to build a deterministic pseudo-random site generator that creates connected room networks for larger arcane ruins, sanctums, vaults, archives, shrines, and later mission families.

Deferral note:
- keep MF-043 scoped to the current authored/profile-driven mission set: Arcane Cache, Sealed Vault, and Ruined Sanctum
- do not block the next MFVanilla mission release on a full room-graph generator
- treat this as the larger successor to the current module/profile utility once the small mission set is stable in normal play

Generator scope:
- generate a room graph first, then place rooms and corridors with guaranteed connectivity, bounds checks, no accidental overlaps, and reachable entrances/reward rooms
- support room roles such as entry hall, collapsed hall, side loot room, sealed vault, ritual chamber, archive, storage room, defender post, and boss chamber
- populate by room role: floors, walls, doors, rubble, chunks, props, treasure anchors, sealed containers, beds/shelves/workbenches, ritual dressing, and defender positions
- allocate defenders from a threat budget across the room graph so early sites have a few readable guardians and late sites can mix lesser automata with elite sentinels or boss constructs
- derive all layout, material, population, loot-anchor, and defender variation from stable site identity/profile inputs rather than ambient `Rand`
- expose XML/profile hooks for room count, room-size bands, role weights, corridor style, sealed-room chance, ruin damage, rubble density, loot tiers, defender tables, and boss-room eligibility
- include dev diagnostics that report the seed, room graph, role assignment, selected profile, population passes, and unreachable-cell checks

Implementation phases:
1. Build the connected room-graph and placement pass with empty rooms/corridors only.
2. Add role assignment and role-driven population for floors, doors, props, reward anchors, and sealed side rooms.
3. Add ruin damage: broken walls, collapsed halls, rubble, chunks, exposed floors, and partially sealed rooms.
4. Add threat-budgeted defender allocation by room role and site tier.
5. Add profile hooks for multiple future site families without new C# for each variant.

Out of first pass:
- active magical hazards, trap tables, unstable leyline rooms, elemental room effects, and puzzle-like locked-room chains can layer onto this generator later
- faction integration, magic-capable hostile pawns, and narrative quest chains should wait until the basic connected-site generator is proven


### MF-050 Arcane Constructs, Golems, And Automata

Goal: create a fantasy-facing enemy family for MFVanilla by reusing proven mechanoid combat roles as golems, automata, sentinels, and arcane constructs.

Status: first release set is implemented enough to support MF-043. Automata are working fairly well in current testing, and the Deep Iron Golem has been dev tested as a strong boss fight. Current-band work is texture/art verification, site combat smoke testing, drops, tuning, and readability; elemental variants and deeper MagicFramework interactions should wait.

Rationale:
- mechanoid mechanics already provide hostile AI, combat behavior, site/raid integration, threat scaling, pathing, death handling, and save/load stability
- rebranding selected mechanoid roles removes tech-themed enemies from fantasy arcane missions without requiring custom magic AI immediately
- constructs give MFVanilla a recurring enemy identity and a story-facing guardian/ancient-defense faction for arcane sites

Design direction:
- do not globally replace vanilla mechanoids
- create MFVanilla-specific construct pawn kinds/races or wrappers that borrow mechanoid role patterns where practical
- use constructs mainly in arcane cache sites, ruined sanctums, sealed vaults, cursed archives, elemental shrines, and later elemental tribe/AI content
- start mechanically conservative, then add custom textures, drops, resistances, vulnerabilities, and magic interactions after the mission loop is stable

Role mapping candidates:
- militor-style enemy -> lesser clay automaton, bronze servitor, or shardling
- scyther-style enemy -> blade automaton, wind-cutter, or rune-slasher
- lancer-style enemy -> crystal sentinel or arcane beam warden
- pikeman-style enemy -> rune-ballista construct
- centipede/boss-style enemy -> Deep Iron Golem
- termite-style enemy -> breach golem or stonebreaker automaton

Deep Iron Golem capstone direction:
- replace the generic Iron Colossus idea with a boss-tier Deep Iron Golem inspired by the Gravendark/Duergar campaign-setting construct
- presentation: towering dark-metal golem, glowing force rune in the chest, slow movement, extreme durability, heavy melee threat, and anti-magic identity
- mechanical translation for RimWorld should favor a readable boss encounter over a direct tabletop stat conversion
- baseline role: very slow vault guardian or high-tier site boss, not a common cache defender
- expected weaknesses: slow speed, large body, limited pursuit, possible vulnerability to being kited or controlled by terrain
- expected strengths: high armor, high health, powerful blunt melee, resistance to heat/cold-like damage where practical, immunity or high resistance to poison/toxic/psychic-style effects where vanilla stats support it
- force rune concept: the golem gains stored force when damaged by MagicFramework spells or future magic weapons; after enough charge it can trigger one major force behavior
- possible first implementation of force rune behavior: charge counter hediff or comp, dev-visible inspect string, and one deterministic ability selected by health/context - initial `CompDeepIronForceRune` added with saved charges, inspect string, shield state, and context-triggered abilities
- candidate force abilities: temporary damage-reduction shield at low health, radial force blast when surrounded, or empowered slam/knockback while near full health - initial shield and radial force blast added; empowered slam remains deferred
- defer legendary-action complexity; RimWorld version should use cooldowns, tick logic, or triggered comps rather than tabletop turn reactions
- defer until the four baseline automata are textured, site-tested, and tuned

Implementation phases:
1. Use vanilla mechanoid defenders temporarily in MF-049 Arcane Cache Site maps. - done for the first site-generator validation pass
2. Add one or two first construct pawn kinds with fantasy labels/descriptions and borrowed mechanoid behavior/stats. - expanded release minimum to four first-pass automata: Clay Automaton, Rune-Slasher Automaton, Crystal Sentinel, and Rune-Ballista Construct
3. Add initial textures or recolors so constructs are visually distinct from mechs. - initial automata and golem textures are present; remaining work is in-game art verification, scale/readability checks, and release tuning
4. Use constructs as arcane site defenders once smoke tested. - initial arcane cache profiles now use MFVanilla automata instead of vanilla scythers/lancers/pikemen
5. Add construct-specific drops, such as gemstone dust, arcane fragments, jade, plasteel replacement candidates, or future construct cores. - initial gemstone dust and jade butcher products added for the first pass
6. Tune automata around slow construct identity: clay should be kiteable, rune-slasher should be the only semi-mobile pressure unit, and ranged constructs should feel like position-holding sentries. - initial speed reduction pass added
7. Add Deep Iron Golem as the boss/capstone construct once baseline automata pass texture and site testing. - initial `MFV_DeepIronGolem` added and integrated into a rare high-threat `MFV_ArcaneCache_DeepIronVault` profile with a grand arcane treasure chest
8. Add elemental variants and resistances/vulnerabilities after fire/water/earth/air content is broader.
9. Consider later MagicFramework interactions: spells that disrupt constructs, bind golems, repair automata, or animate inert guardians.

Success criteria:
- arcane sites no longer feel like tech/mechanoid encounters once construct skins and defs are in place
- the first implementation keeps the reliability of mechanoid combat without requiring custom hostile magic AI
- construct roles are readable to players and scale naturally from low-tier cache guardians to high-tier vault defenders
- the system supports future story identity: ancient arcane defense systems, cursed guardians, elemental constructs, and automata recovered or studied by players


### MF-044 Leyline Map And Sensitivity Gameplay

Goal: make Leyline Sensitivity reveal a stable, useful magical geography layer for each map.

Status: complete for the current release slice. Future leyline effects should be opened as separate follow-up tasks once the overlay, mana recovery, and Arcane Forge resonance have been playtested.

Design direction:
- generate a hidden static leyline map per RimWorld map, conceptually similar to the deep-drill resource overlay but expressing magical flow rather than mineable resources
- store the leyline data on a map component so it survives save/load and does not reroll unexpectedly
- start with debug/reveal tools before adding strong gameplay dependencies
- use Leyline Sensitivity as the first player-facing unlock for seeing, sensing, or exploiting the leyline map

Target behavior:
- each map receives pseudo-random leyline paths and possible leyline nodes during or shortly after map creation
- generated layout should be deterministic from map/game state where practical
- leylines should remain static thereafter unless a future explicit event or spell changes them
- research, buildings, rituals, or spells can later query local leyline strength or proximity to a node

Implementation questions:
- should leyline data be a cell grid, sparse path list, node list, or a combination?
- should the overlay reveal exact strength, only approximate bands, or only nodes until deeper research?
- should leylines affect mana regeneration, spell power, ritual quality, building efficiency, treasure placement, incidents, or all of these over time?
- should leyline nodes be rare enough to drive colony placement decisions, or common enough to influence local base layout?

First implementation pass:
1. Add a map component that generates and saves leyline paths/nodes. - initial `LeylineMapComponent` added with a saved byte-strength grid and optional saved segment records
2. Add dev-mode debug drawing and logging for generated leyline data. - initial dev actions added for overlay toggle, regeneration, and summary logging
3. Add a simple overlay or inspection mode gated by Leyline Sensitivity. - initial map overlay button and cell-strength rendering added; normal access requires `MFV_LeylineSensitivity`, dev mode can preview before research; optional zoom-gated numeric strength labels added through MFVanilla settings
4. Add one low-risk gameplay hook, such as improved mana recovery or arcane research speed near a node, only after the map layer is stable. - initial pawn mana recovery hook added: Arcane Gift pawns periodically sample peak leyline strength in a tiny radius and receive a capped mana recovery bonus
5. Add reusable area-reading helpers for future buildings, rituals, and site placement. - initial `LeylineAreaReading` and `LeylineUtility` helpers added for cell, radius, rect, thing footprint, and capped peak-strength bonus calculations
6. Wire leyline readings into one building as a placement reward. - Arcane Forge now reads its footprint leyline sum and gains a capped chance to improve enchanted weapon output quality by one tier; inspect text shows current resonance chance

Success criteria:
- Leyline Sensitivity has an immediately understandable use
- leyline data is stable across save/load
- future systems can query leyline strength without knowing generation internals
- the overlay provides enough information to feel magical without overwhelming normal map play


### MF-045 Elementalist Tribe And Themed Traders

Goal: add one broad Elementalist tribe that makes elemental magic feel present in the world, first through traders and later through hostile magic pawns.

Status: first implementation pass complete. Keep only caravan/trader buy-sell smoke testing in release hygiene; broader role-specific trader variants, bespoke pawn kinds, and richer allied/neutral behavior should become new follow-up tasks if they prove worthwhile.

Design direction:
- start with faction/trader/content identity before hostile caster AI
- prefer one coherent Elementalist faction/tribe over four separate elemental factions for the first pass
- express fire, earth, air, and water as internal roles, trader stock themes, apparel/visual accents, pawn kinds, settlement flavor, and later combat loadouts
- four separate elemental tribes should remain a later expansion only if each can justify distinct behavior, economy, diplomacy, visuals, and magic identity
- hostile spellcasting should use the deliberately narrow MF-036 first pass: rare hostile Elementalist casters, single-target spells only, curated loadouts, and normal validator/runtime execution

Target behavior:
- Elementalist caravans and traders carry a mixed but weighted stock of elemental scrolls, gemstones, exotic herbs, arcane reagents, and occasional magic utility goods
- fire-themed roles favor pyromancy scrolls, rubies, heat/light infrastructure, and aggressive magic goods
- earth-themed roles favor geomancy scrolls, emeralds, stone/gem materials, defensive goods, and construction-adjacent items
- air-themed roles favor aeromancy scrolls, topaz, mobility/control goods, and arcane spire supplies
- water-themed roles favor aquamancy scrolls, sapphires, cooling/protection goods, medicine-adjacent items, and extinguishing tools
- the faction can produce rare hostile spell-capable pawns with appropriate curated magic loadouts; broader allied/neutral spell behavior remains deferred

Implementation questions:
- should the Elementalist tribe be a full faction, trader kinds attached to existing factions, or rare world pawns/caravans?
- should they be neutral by default, mixed relations, or scenario/storyteller controlled?
- should their identity be tribal, monastic, guild-like, cultic, or a loose confederation of elemental circles?
- should they sell finished enchanted gear, only inputs, or occasional rare major items through treasure chests/rewards?

First implementation pass:
1. Define the single Elementalist faction/trader scope without AI casting. - initial `MFV_ElementalistTribe` faction added as a neutral tribal world presence using reliable vanilla tribal pawn groups
2. Add stock generators for mixed elemental goods, scrolls, gemstones, and production inputs. - initial `MFV_Caravan_ElementalistCircle` trader added with elemental scrolls, gemstones, exotic herbs, arcane ink, papyrus, parchment, herbal medicine, and MFVanilla buy tags
3. Add names/descriptions/backstory flavor sufficient for one recognizable world presence. - done through Elementalist faction labels, leader title, settlement flavor, four-element description, and color spectrum
4. Smoke test caravan/trader generation and buy/sell behavior. - remaining release hygiene item
5. Defer broad hostile caster behavior to MF-036, but allow the narrow first-pass Elementalist caster hook to prove the runtime path. - initial hostile Elementalist pawn generation now gives about one in five hostile humanlike pawns Arcane Gift, caster level 3, and 1-3 curated single-target spells from Firebolt, Force Push, Heal, Stoneskin, and Might
6. Make hostile casters visually readable. - initial caster package now gives Elementalist AI pawns the broad Elementalist discipline, adds/dyes a robe or visible apparel, and colors it by the selected spell theme: fire, water, earth, or air

Success criteria:
- elemental magic appears in the world economy, not only player crafting
- the Elementalist tribe has a recognizable trade identity without requiring four separate factions
- fire, earth, air, and water are visible as internal variety rather than separate thin factions
- hostile spell-capable pawns remain rare, curated, and limited to the first safe single-target AI casting path
- future AI caster loadouts have clear faction/theme homes


### MF-046 Elemental Spell Expansion

Goal: fill thin elemental schools and add high-value utility spells that make MFVanilla feel less like a validation pack.

Status: complete for the current release slice. Remaining large ideas such as Fly, Summon Golem, and Earthy Grave should be opened as separate school-identity tasks rather than keeping MF-046 active indefinitely.

Priority spell candidates:
- Air Blast: implemented in wave 1 as blunt damage plus scalable knockback; upgraded in phase 2 to a cone spell backed by reusable cone target queries.
- Fly: mobility/terrain bypass fantasy; likely needs careful framework and pathing design before implementation.
- Heat: implemented as a sustained Pyromancy utility spell that warms an area like a magical heater.
- Warmth: implemented in phase 2 as a maintained Pyromancy aura that keeps the target and nearby allies comfortable in cold conditions without improving heat tolerance or warming rooms.
- Summon Golem: earth summon, likely requiring summon/spawn expansion and a pawn kind.
- Earthy Grave: earth control spell; possible immobilize, bury, down, slow, or terrain hazard.
- Stoneskin: implemented in wave 1 as a defensive earth status with armor offsets and movement penalty.
- Deluge: first draft implemented as a water area pulse that extinguishes fires, drenches pawns, and pushes them outward from the target center; terrain mud, crop destruction, and deeper wet/frost synergy remain future polish.
- Extinguish: implemented in wave 1 using the Tidebreaker fire-clearing primitive, with a new scalable radius hook.

Design direction:
- prioritize spells that strengthen underused schools and can be authored with current primitives
- use new framework hooks only when they clearly support multiple future spells or item features
- prefer utility and colony-support spells alongside combat spells so each school has a play identity

First implementation pass:
1. Completed: classified the first candidates against existing support.
   - Air Blast: XML-only composition of `DamageActionDef` and `KnockbackActionDef`.
   - Stoneskin: XML-only spell plus reusable `SpellStatusEffectDef`.
   - Extinguish: existing `ExtinguishFireActionDef` from Tidebreaker, plus a small `scalableRadius` framework hook.
   - Deluge, Heat, Warmth, Fly, Summon Golem, and Earthy Grave remain design/implementation candidates.
2. Completed: selected Air Blast, Stoneskin, and Extinguish for wave 1.
3. Completed: added spell defs, scroll generation coverage, research gates, and provisional reused icons.
4. Smoke test targeting, mana/cooldown, scroll learning, and save/load for any persistent effects.

Second implementation pass:
1. Completed: added `MF_Deluge` as a learnable tier 2 Aquamancy spell.
2. Completed: generalized `KnockbackActionDef` with an authored `originSource`, preserving caster-origin behavior by default and allowing Deluge to push pawns away from the spell center.
3. Completed: added scalable radius support to `TargetsInRadiusQueryDef` so Deluge can keep targeting, extinguish, and pawn-effect radii aligned.
4. Completed: added `MFV_Status_Drenched` as a short water-control status with movement slow and heat armor support.
5. Remaining Deluge polish: temporary mud terrain, crop/small-plant destruction, richer water visuals, and possible explicit wet/frost interaction.

Cone targeting follow-up:
1. Completed: added `Cone` to `SpellTargetShape`.
2. Completed: added `coneAngleDegrees` to spell targeting and `ShapeTargetsQueryDef`.
3. Completed: implemented cone resolution in `ShapeTargetsQueryWorker` using caster/origin, aim cell, line length, and angle.
4. Completed: converted `MF_AirBlast` from single-target to a 60-degree, 7-cell hostile-pawn cone.
5. Future polish: targeting preview/overlay for cone cells and optional scalable cone length/angle authoring.

Warmth follow-up:
1. Completed: added `MF_Warmth` as a learnable Pyromancy utility spell.
2. Completed: authored maintained `MFV_Status_Warmth` comfort protection for the target using only `ComfyTemperatureMin`.
3. Completed: authored pulsed `MFV_Status_WarmthAura` with matching comfort protection for nearby allies in a 4-cell radius.
4. Completed: generated scroll and scribing recipe coverage.
5. Future polish: dedicated Warmth icon/visuals and possible shared temperature-aura tuning after in-game testing.

Heat follow-up:
1. Completed: added `TemperaturePushActionDef` as a reusable framework action that pushes heat into a room at an authored spell location.
2. Completed: redesigned `MF_Heat` as a learnable sustained Pyromancy utility spell gated behind Pyromancy research.
3. Completed: authored Heat as a maintained area warming field with upkeep mana, concentration break handling, and pulsed heat output.
4. Completed: generated scroll and scribing recipe coverage.
5. Future polish: dedicated Heat icon/visuals and balance tuning after in-game room-temperature testing.

Success criteria:
- water and earth no longer feel obviously thin
- new spells have clear research and scroll paths
- each added spell either improves player gameplay or validates a reusable framework primitive
- large ideas such as Fly and Golem are scoped before implementation rather than squeezed into a small pass


### MF-047 Enchanted Weapon Special Features

Goal: give the first Arcane Forge weapons unique identities backed by MagicFramework mechanics where practical.

Status: mostly implemented for the first weapon set. Active work is polish and validation: inspect strings/tooltips, in-game smoke tests, art details, damage-resistance checks, and balance.

Current weapon set:
- Flaming Longsword
- Zephyr Spear
- Tidebreaker Mace
- Stonefall Mace

Candidate features:
- Flaming Longsword: ignite chance, small flame burst, heat status, or fire vulnerability interaction on hit.
- Zephyr Spear: movement speed bonus while equipped, air-blast proc, short reach/control identity, or dodge/evasion bonus.
- Tidebreaker Mace: chill, slow, wet/frostbite synergy, extinguish splash, or defensive water effect on hit.
- Stonefall Mace: knockback, stun, armor break, stagger, or extra blunt impact on heavy hits.

Framework direction:
- consider a reusable equipment comp that triggers MagicFramework action lists on melee hit, equip, unequip, tick, or damage taken
- keep the first implementation narrow enough for the four weapons, but avoid hardcoding weapon-specific behavior in generic combat patches
- ensure triggered effects are deterministic enough for save/load and multiplayer-sensitive behavior

Current framework support:
- `CompProperties_MagicItemAbilities` / `CompMagicItemAbilities` can be attached to ThingDefs to grant activated spell-like powers from equipped, worn, or carried items.
- Item abilities reference existing `SpellDef` entries and execute through the normal MagicFramework targeting, validation, warmup, FX, cost, and action pipeline without adding the spell to a pawn's known-spell list.
- Activated item powers expose pawn gizmos, can require the item to be equipped/worn, can optionally require Arcane Gift, can consume the item on use, and use item-local cooldowns saved on the comp.
- Passive item statuses can reference `SpellStatusEffectDef` entries and are refreshed by a saved game component while the source item remains equipped or worn.
- `SpellContext.sourceItem` records the item that supplied the ability so future action workers, logs, triggers, or requirements can distinguish item-sourced magic from ordinary pawn spells.
- Zephyr Spear now validates both paths with an equipped-only `Zephyr Gust` activated power and a `Zephyr Stride` passive movement status.
- Item melee triggers can run authored MagicFramework action lists on successful melee damage, with item-local cooldowns and deterministic chance checks.
- Item damage resistance can scale incoming damage by `DamageDef` and prevent authored direct hediff applications, so equipment protection can cover both vanilla damage and MagicFramework spell actions.
- Flaming Longsword now provides flame ward fire protection, Flame damage resistance, Burn prevention, and retains its existing flame damage rider.
- Tidebreaker Mace now provides water ward fire suppression, partial Flame damage resistance, an activated `Quenching Wave` extinguish power, and a stagger trigger.
- Stonefall Mace now provides a `Stoneguard` armor passive and a stagger/knockback trigger.

Implementation questions:
- should weapon specials be authored directly on ThingDefs through a comp, through a separate MagicWeaponEffectDef, or through existing SpellActionDef lists?
- should procs spend mana, have cooldowns, require Arcane Gift, or function for any wielder?
- should passive bonuses appear in stats, inspect strings, or generated descriptions?
- how should friendly fire and forced movement be handled for on-hit effects?

First implementation pass:
1. Design the smallest reusable equipment-trigger bridge that can call MagicFramework-style effects. - initial activated-power bridge done
2. Author one item ability on an MFVanilla weapon or test item and smoke test targeting, cooldown, save/load, and gizmo display. - Zephyr Spear example added
3. Add passive/status support for while-equipped effects without making them learned spells. - initial support done
4. Add inspect strings/tooltips so players understand each special feature.
5. Add on-hit/on-damage trigger support only after activated and passive item abilities are stable. - initial melee trigger support done
6. Expand to the remaining three weapons only after the trigger path is stable. - initial expansion done
7. Smoke test all four enchanted weapons in game: gizmos, passives, fire behavior, melee triggers, cooldowns, save/load, and art details.
8. Smoke test Flame spell damage against Flaming Longsword and Tidebreaker Mace wielders; the former should heavily resist Flame and block direct Burn hediff application, while the latter should partially resist Flame and keep extinguishing attached fire.

Success criteria:
- each enchanted weapon has a memorable mechanical identity
- special features reuse or inform MagicFramework primitives rather than isolated one-off code
- behavior remains understandable, deterministic, and safe in ordinary melee combat


### MF-041 Arcane forge

Goal: Introduce an Arcane Forge production item

Target coverage: 
 - smoke test requirements to build this mid-late game item - done
 - validate spire-link gating, inspect strings, and bill availability - done
 - balance recipes for transforming good-or-better mundane weapons into magic versions - initial pass done
 - decide whether the first weapon set needs custom textures before release

