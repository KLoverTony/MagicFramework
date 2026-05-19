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
- Update modversion label in about.xml files commensurate with developmental progress.

## Internal Release Schedule

Use this schedule as the internal release train. Each release should have a small theme, a clear content boundary, and a publish gate. When most items in a release band are implemented, built, deployed, and smoke tested, the release can be pushed; unfinished stretch items should move to a later band instead of delaying indefinitely.

Release gate for every band:
- clean builds for changed assemblies
- deployed local mod folders match workspace content
- version and splash notes updated
- XML load check and focused in-game smoke tests for changed systems
- known issues moved into post-release notes unless they affect startup, save/load, cleanup, or basic usability

Recent public release:
- MFVanilla 0.7 / MagicFramework 1.2: production loop, Arcane Forge first weapon set, enchanted weapon identities, arcane treasure chests, arcane cache sites, automata/construct defenders, and updated splash notes.

Completed current slice:
- MF-044: Leyline Sensitivity now reveals a stable hidden leyline map, supports optional numeric inspection, boosts Arcane Gift pawn mana recovery near strong currents, and gives Arcane Forges a leyline resonance chance to improve enchanted weapon quality.
- MF-061: Arcane Discipline specialization now gives research projects reward labels, lets Arcane Gift pawns embrace/advance disciplines through a marker ritual, shows discipline in the mana gizmo, optionally enforces discipline spell learning, and requires scroll scribes to know the spell being copied.
- MF-046: Elemental spell expansion now covers Air Blast, Stoneskin, Extinguish, Deluge, Warmth, and sustained room-warming Heat, with scroll generation and research gates in place.

Next release band - MFVanilla world layer:
- MF-043: complete a small MFVanilla mission set, starting with normal in-game Arcane Cache generation, and keep research cleanup decisions current
- MF-045: elemental cultures and themed traders if the world layer needs a stronger trade/faction identity

Following release band - school identity and advanced magic:
- MF-051: mind control under Forbidden Lore
- MF-052: illusionary pawns under Illusion
- MF-053: undead pawns under Necromancy
- MF-054: golems under Fleshcraft
- MF-055: planar exploration under Planar Magic
- MF-056: legendary weapons and magic buff rituals under Grand Sorcery
- MF-057: chronometric resurrection under Chronomancy
- MF-058: lichdom under Soulcraft

AeternusFaith release band:
- MF-039, MF-019, MF-033, MF-034, and MF-035 remain the first-edition release core
- MF-059 adds decorative and religious statues as a content/presentation pass
- MF-020, MF-021, and MF-023 remain follow-up polish unless they become release blockers

Consideration backlog:
- MF-060: evaluate paintings as decoration and platinum as a trade good; do not implement until they have a clear economy or presentation role

| ID | Priority | Complexity | Area | Task |
| --- | --- | --- | --- | --- |
| MF-038 | P1 | M | MFVanilla | Keep the MFVanilla content roadmap current after completed feature pillars and prune stale planning notes. |
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
| MF-041 | P1 | M | MFVanilla | Finish and release-tune the Arcane Forge production item. |
| MF-043 | P1 | L | MFVanilla | Complete a small next-release MFVanilla mission set, starting with normal in-game Arcane Cache generation. |
| MF-049 | P1 | XL | MFVanilla | Build arcane encounter maps and mission sites as a high-quality content pillar. |
| MF-050 | P1 | L | MFVanilla | Create arcane construct enemies by rebranding mechanoid combat roles as golems and automata. |
| MF-045 | P2 | L | MFVanilla | Add elemental tribes, themed traders, and later magic-capable hostile pawns. |
| MF-047 | P2 | M | Equipment | Give first enchanted weapons unique MagicFramework-backed features. |
| MF-051 | P2 | L | Forbidden Lore | Add mind-control magic as a dangerous forbidden school feature. |
| MF-052 | P2 | L | Illusion | Add illusionary pawns under Illusion research. |
| MF-053 | P2 | L | Necromancy | Add undead pawns under Necromancy research. |
| MF-054 | P2 | L | Fleshcraft | Add golems under Fleshcraft research. |
| MF-055 | P2 | XL | Planar Magic | Add planar exploration as an advanced magic feature. |
| MF-056 | P2 | L | Grand Sorcery | Add legendary weapons and a magic buff ritual under Grand Sorcery. |
| MF-057 | P2 | L | Chronomancy | Add chronometric resurrection under Chronomancy research. |
| MF-058 | P2 | XL | Soulcraft | Add lichdom under Soulcraft research. |
| MF-059 | P2 | M | AeternusFaith | Add decorative and religious statues. |
| MF-060 | P3 | S | Content | Consider paintings as decoration and platinum as a trade good. |
| MF-061 | P2 | L | MFVanilla | Add optional Arcane Discipline specialization for Arcane Gift pawns, including research-tree unlock labels, an embrace ritual, spell eligibility gating, settings support, and pawn mana-bar display text. |

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
- Arcane foci are now split by the first four elemental production paths: fire foci from ruby, water foci from sapphire, earth foci from emerald, and air foci from topaz. The flaming longsword recipe now consumes a fire arcane focus.
- Magic heaters and torches now consume fire foci, magic passive coolers consume water foci, and arcane spires consume air foci.
- The Arcane Forge is now an active enchantment bench gated by linked arcane spires. Its first weapon set includes Flaming Longsword, Zephyr Spear, Tidebreaker Mace, and Stonefall Mace, all transformed from good-or-better mundane weapons while preserving final quality.
- Arcane Forge setup costs have been tuned down for release readiness while keeping the four-spire requirement as the advanced enchantment gate.
- Arcane Forge smoke testing confirmed build requirements, linked-spire gating, bill availability, and the first enchanted weapon set are working well.
- Enchanted weapon products now assign the enchanter as the art author when quality-generated art is present, avoiding unknown authors on high-quality outputs.
- Trader stock smoke testing confirmed relevant MFVanilla production stocks are appearing in tested traders. Shaman trader direct dev-spawn testing was blocked by dev UI availability, so shaman stock remains a watch item during normal playtesting rather than a release blocker.
- Gemstone vein availability, mining output, market values, and stack sizes are provisionally accepted for the current release pass. Keep watching gemstone dust and elemental focus abundance during broader progression and treasure testing.
- Elemental foci should remain player-crafted only for now, preserving them as an industrial requirement and gate for powerful magical items.
- Enchanted weapons can appear as expensive, rare commodities in world rewards, loot, or limited trader contexts; tune availability so colony crafting remains the reliable path.
- Major scroll gemstone requirements should follow elemental association and the elemental research that exposes the spell; for example, pyromancy spells use ruby-derived requirements. Non-elemental major spells can accept any elemental focus as a flexible advanced reagent.
- Full-loop progression smoke testing is provisionally accepted for this pass: dust, herbs, ink, papyrus/parchment, scroll scribing, scroll learning, gemstone cutting, focus crafting, and weapon enchanting remain watch items during normal balance testing.

Priority:
- review the full production chain from raw inputs through finished scrolls and magic utility items
- make sure each bench has a clear role, research position, recipe set, work type, texture, cost, and power/facility story
- tune Arcane Ink, parchment/papyrus, gemstones, herbs, and generated spell scroll costs so the early loop is useful and the advanced loop has meaningful investment
- check that research gates unlock recipes, benches, utility buildings, and spell access in a sensible order
- add missing descriptions, inspect strings, category placement, tradeability, stack sizes, market values, and bulk/storage behavior where content feels placeholder
- smoke test a new colony path: discover Arcane gift, unlock early research, craft inputs, make scrolls, learn spells, and progress into advanced production

Next steps:
- Watch production pacing, reagent abundance, and player clarity during normal playtesting; reopen MF-037 only for concrete release-blocking issues.

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
- This item remains open as a planning and cleanup bucket, especially for research nodes and stale documentation that no longer match live content.

Next-release content direction:
- Favor content definition before final economy tuning. Balance should happen after the intended release content set is clearer.
- Treat elemental tribes, leyline maps, school expansion, and special enchanted weapon behavior as major candidate pillars for upcoming releases, depending on implementation scope.
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

Selected pillar:
- Arcane missions: complete a small set of world-map mission opportunities built around `MFV_ArcaneCache` first, then add two lightweight variants that reuse the same deterministic site generation, reward chest, and defender infrastructure.

Mission set:
- Arcane Cache: the first normal mission path for `MFV_ArcaneCache`; a compact treasure-cache site with construct defenders and an arcane treasure chest. This is currently implemented as a site/gen-step/debug-spawn slice, but still needs a real player-facing quest or incident path.
- Sealed Vault: a higher-threat cache variant that favors `MFV_ArcaneCache_Sealed` or `MFV_ArcaneCache_DeepIronVault`, uses greater/grand chest rewards, and appears later or at higher storyteller points. - initial `MFV_SealedVault` mission now locks to `MFV_ArcaneCache_DeepIronVault`, uses the grand chest profile, and showcases the Deep Iron Golem as the vault guardian
- Ruined Sanctum: a lower-to-mid threat exploration variant using the ruined/tower module pieces, lighter defenders, and side-room dressing or minor loot. It can initially share the `MFV_ArcaneCache` site part/profile machinery if separate site identity would add too much first-pass cost.

Non-pillar candidates for this release:
- Arcane loot and rewards: treasure chests, rare finds, quest rewards, and loot table integration. - first treasure chest pass shipped; this release should use them through missions rather than reopening the reward system broadly
- Elemental school completion: mostly complete for the current pass; add only mission-adjacent spell/content fixes if testing exposes a gap
- Leyline gameplay: keep current Leyline Sensitivity work as a support feature; defer leyline mission sites until the first mission generator is player-facing
- Elemental cultures: defer tribes/traders unless mission generation needs a source faction or narrative sender
- Enchanted weapon identity: defer unique weapon mechanics unless mission rewards need a small tuning pass
- Arcane material economy: defer broad material-cost changes until mission reward pacing is visible
- Advanced school identity: keep as separate release-band tasks after the world-layer mission loop is proven

Candidate spells:
- Air: Air Blast, Fly.
- Fire/heat: Heat, Warmth.
- Earth: Summon Golem, Earthy Grave, Stoneskin.
- Water: Deluge, Extinguish.

Research audit:
- identify research projects with no current unlocks or gameplay effect
- decide whether each should receive content in this release, move to a later release, or be removed/hidden until useful
- ensure research names imply real player-facing outcomes rather than only future intent

Effort plan:
1. Confirm vanilla-friendly mission entry point: choose quest script, incident, or storyteller-driven site opportunity for spawning `MFV_ArcaneCache` in normal gameplay.
2. Implement the first player-facing Arcane Cache mission path using the existing `SiteMaker`/`SitePartDef`/`GenStepDef` flow; include timeout, threat points, arrival text, and reward extraction expectations. - initial storyteller incident worker now creates `MFV_ArcaneCache` sites, sends a player-facing letter, registers an 18-day expiry, and exposes a debug action for the same mission path
3. Add mission tuning knobs: minimum days, threat-point bands, distance from colony, repeat cooldown, optional research/progression gate, and dev-mode spawn/log actions for the same path. - initial incident tuning uses earliest day 8, base chance 0.35, 18-day refire, 4-18 tile distance, 300+ threat points, and one-active-cache gating
4. Split authored mission variants only where needed: normal cache, ruined sanctum, and sealed/deep vault can start as profile-driven variants before receiving separate site parts.
   - Sealed Vault now has its own site part, linked gen step, incident worker, incident def, 28-day expiry, 8-24 tile distance, earliest day 45, 45-day refire, and debug spawn/offer actions.
5. Smoke test end to end: quest/incident generation, world site creation, caravan arrival, map generation, construct combat, chest opening, leaving the map, site cleanup, save/load, and repeat generation.
6. Tune mission frequency and reward tier after several natural-generation tests, then update splash notes and completed-work notes.

Estimated effort:
- Arcane Cache normal mission generation: `M`. The site already exists, but the natural quest/incident path, tuning gates, and cleanup validation need implementation and in-game testing.
- Small three-mission set: `L`. Most map generation can be reused, but player-facing scheduling, variant identity, reward/threat tuning, and smoke testing across multiple site profiles raise the effort.
- Broader mission ecosystem with unique hazards, leyline sites, elemental shrines, cursed archives, and faction senders: `XL`; explicitly deferred until the small set works.

Sequencing note:
- avoid deep balance work until the first natural mission path, reward tiering, repeat frequency, and site cleanup behavior are visible in normal play.

### MF-049 Arcane Encounter Maps And Mission Sites

Goal: build pseudo-random arcane-themed mission maps as a high-quality MFVanilla content pillar, not just generic item stashes with magic loot.

Feature vision:
- create world sites and mission maps that feel like magical places: arcane caches, ruined sanctums, sealed vaults, leyline ruptures, elemental shrines, cursed archives, and later faction/tribe-themed ritual sites
- use deterministic site/map seeds so layout, loot, curse/trap outcomes, and major encounter choices are stable across save/load and multiplayer-friendly
- make treasure chests one reward anchor inside the larger encounter, rather than the entire encounter
- use vanilla mechanoids as the temporary standard site enemy so the first mission slice can rely on proven RimWorld threat behavior
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
- defenders: vanilla mechanoid/site threats first; later rebranded arcane constructs, elemental guardians, golems, summoned creatures, or magic-capable hostile pawns once AI casting exists
- deferred themes: leftover magic misfires and malfunctioning arcane spires that jolt nearby pawns fit a later war-torn/unstable magic site theme, not the first treasure-cache encounter

Implementation phases:
1. Design arcane site profile defs: theme, layout size, loot tier, chest def, hazard chance, defender/threat tags, and prop tables. - initial `ArcaneSiteProfileDef` supports room size, floor, wall stuff, chest def, defender pawn kinds/count, and fixed dressing placements
2. Implement the Arcane Cache Site vertical slice with a single profile and one chest. - current cache generator now reads `MFV_ArcaneCache_Default` while preserving the tested single-room cache behavior
3. Add deterministic generation diagnostics in dev mode so a site can report its seed/profile/chest ID. - dev-mode generation log now reports profile, tile, threat points, room, chest def/location, chest ID, and spawned defenders
4. Add reliable dev/test spawning for arcane cache sites. - `MFVanilla - Arcane Sites` debug actions now spawn a proper `SiteMaker` arcane cache near the current map and can remove bare empty sites left by generic dev spawning
5. Expand into Arcane Ruin Generator: multiple rooms, themed dressing, side loot, and optional hazards. - initial profile-driven circular tower layout support added; default arcane cache now uses a round wizard-tower chamber footprint, deterministic stone wall variation, a real door, and less crowded torch/spire dressing; smoke test confirmed clear torches and marble wall generation
6. Add curse/trap integration using XML-authored trap tables or MagicFramework action lists.
7. Add themed variants: fire shrine, flooded vault, earth-buried sanctum, wind-swept ruin, leyline node, cursed archive.
8. Integrate higher-tier sites with quest points/game stage and future elemental tribes or magic AI when those systems exist.

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
- use existing mechanoid defenders for the first Arcane Cache Site implementation so combat, pathing, down/death behavior, threat scaling, and site integration are known-good
- avoid globally replacing vanilla mechanoids; the temporary use should be local to arcane mission construction
- migrate the fantasy-facing enemy identity to MF-050 Arcane Constructs once the mission loop works

Success criteria:
- the first arcane site feels like a real mission location, not a chest spawned in a field
- generation is stable enough for save/load and multiplayer-friendly play
- content authors can add or patch encounter profiles through XML
- rewards, threats, and hazards scale with mission tier without replacing the normal production loop
- the system can grow into elemental sites, cursed vaults, and faction/tribe encounters without a rewrite

### MF-050 Arcane Constructs, Golems, And Automata

Goal: create a fantasy-facing enemy family for MFVanilla by reusing proven mechanoid combat roles as golems, automata, sentinels, and arcane constructs.

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
3. Add initial textures or recolors so constructs are visually distinct from mechs. - pending art pass; current defs expect `Things/Pawn/Automata/ClayAutomaton`, `RuneSlasherAutomaton`, `CrystalSentinel`, and `RuneBallistaConstruct`
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

### MF-045 Elemental Tribes And Themed Traders

Goal: add fire, earth, air, and water themed cultures that make elemental magic feel present in the world, first through traders and later through hostile magic pawns.

Design direction:
- start with faction/trader/content identity before hostile caster AI
- each elemental tribe should have a distinct trade profile, visual flavor, likely goods, and preferred magic school
- hostile spellcasting should wait for authored AI spell metadata and a safe AI casting path

Target behavior:
- fire tribe traders favor pyromancy scrolls, rubies, heat/light infrastructure, and aggressive magic goods
- earth tribe traders favor geomancy scrolls, emeralds, stone/gem materials, defensive goods, and construction-adjacent items
- air tribe traders favor aeromancy scrolls, topaz, mobility/control goods, and arcane spire supplies
- water tribe traders favor aquamancy scrolls, sapphires, cooling/protection goods, medicine-adjacent items, and extinguishing tools
- factions can later produce hostile pawns with appropriate magic loadouts once AI casting exists

Implementation questions:
- should these be four full factions, trader kinds attached to existing factions, or rare world pawns/caravans?
- should they be neutral by default, mixed relations, or scenario/storyteller controlled?
- should their identities be tribal, monastic, guild-like, cultic, or mixed by element?
- should they sell finished enchanted gear, only inputs, or occasional rare major items through treasure chests/rewards?

First implementation pass:
1. Define the faction/trader scope without AI casting.
2. Add themed stock generators for elemental goods, scrolls, and gemstones.
3. Add names/descriptions/backstory flavor sufficient for world presence.
4. Smoke test caravan/orbital/trader generation and buy/sell behavior.
5. Defer hostile caster behavior to the AI casting task unless a very narrow scripted encounter is safer.

Success criteria:
- elemental magic appears in the world economy, not only player crafting
- each tribe/trader has a recognizable trade identity
- no hostile pawn depends on unimplemented AI spellcasting
- future AI caster loadouts have clear faction/theme homes

### MF-046 Elemental Spell Expansion

Goal: fill thin elemental schools and add high-value utility spells that make MFVanilla feel less like a validation pack.

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

### MF-051 Forbidden Lore Mind Control

Goal: add mind-control magic as a dangerous Forbidden Lore feature without making ordinary hostile AI or player control unstable.

Design direction:
- treat mind control as forbidden, costly, and narratively risky rather than a routine crowd-control spell
- prefer short, readable effects first: compel movement, interrupt work, force a target to flee, temporary mental break influence, or brief ally/hostile confusion
- avoid permanent faction conversion, pawn ownership rewrites, or deep job-driver takeover until the smaller effects are reliable
- require clear player feedback, strong cooldown/cost pressure, and save/load-safe state cleanup

First implementation pass:
1. Identify which effects can be authored with existing mental-state, hediff, job, or forced-target primitives.
2. Add one narrow validation spell under `MFV_ForbiddenLore`.
3. Smoke test target validity, downed/dead cleanup, save/load, prisoner/colonist/hostile behavior, and faction edge cases.

### MF-052 Illusionary Pawns

Goal: add illusionary pawns under Illusion research so the school has a distinct battlefield and deception identity.

Design direction:
- start with temporary decoy pawns or mirage pawns that distract enemies without becoming full colonists
- illusion pawns should have strict lifetime, cleanup, and save/load behavior
- avoid inventory, needs, training, medical, romance, ideology, and caravan complexity for the first pass
- make the player-facing difference between summoned creatures, undead, constructs, and illusions obvious

First implementation pass:
1. Define the minimal illusion pawn kind and lifetime behavior.
2. Add one spell or item source under `MFV_Illusion`.
3. Smoke test spawning, targeting, combat distraction, expiry, map transition, save/load, and cleanup.

### MF-053 Necromancy Undead Pawns

Goal: add undead pawns under Necromancy research as the first MFVanilla necromantic creature feature.

Design direction:
- distinguish MFVanilla undead from AeternusFaith ritual undead; MFVanilla should be spell/research driven, while AeternusFaith remains ideology/rite driven
- begin with temporary or limited-control undead before persistent colony members
- require corpse selection, reachability, reservation, faction assignment, expiry or upkeep, and cleanup to be explicit
- avoid pseudo-relationship memory until MF-021 is ready unless the first pass stays temporary

First implementation pass:
1. Choose the first undead type: skeleton, shambling corpse, bound wraith, or similar low-scope pawn.
2. Add a Necromancy-gated spell and scroll path.
3. Smoke test corpse targeting, map cleanup, save/load, death/despawn behavior, and drafted/undrafted control expectations.

### MF-054 Fleshcraft Golems

Goal: add golems under Fleshcraft research as risky created servants or guardians rather than ordinary summons.

Design direction:
- use Fleshcraft for body-made or stitched constructs; keep earth/stone golems under elemental or construct content if that distinction matters later
- first pass should be a controlled creature creation path, not a broad pawn crafting framework
- avoid permanent colonist-equivalent behavior until control, upkeep, and balance are clear
- connect costs to forbidden/advanced materials so Fleshcraft feels distinct from Arcane Forge automata

First implementation pass:
1. Decide whether the first Fleshcraft golem is temporary, bonded, or persistent with upkeep.
2. Prototype one pawn kind and creation spell/recipe under `MFV_Fleshcraft`.
3. Smoke test creation, commandability, expiry/upkeep, death drops, save/load, and cleanup.

### MF-055 Planar Exploration

Goal: add a Planar Magic feature that lets advanced colonies interact with other planes through events, expeditions, or special sites.

Design direction:
- start as a controlled exploration loop rather than full new-world simulation
- possible forms: planar rift site, expedition incident, temporary pocket map, encounter chain, or costly scan that reveals a planar destination
- rewards should include rare scrolls, gemstones, artifacts, legendary weapon inputs, exotic threats, and future planar materials
- risks should be explicit: dangerous defenders, unstable exits, curses, pawn injury, or time pressure

First implementation pass:
1. Choose the minimum viable loop: world site, temporary map, or event chain.
2. Reuse arcane site profile/generation infrastructure where possible.
3. Smoke test generation, travel, map closure, reward extraction, save/load, and abandonment.

### MF-056 Grand Sorcery Legendary Weapons And Buff Ritual

Goal: make Grand Sorcery feel like colony-scale magic by adding legendary weapons and a high-impact magic buff ritual.

Design direction:
- legendary weapons should be rare capstone items, not an infinite ordinary production tier
- buff rituals should affect a colony, party, room, or limited group with strong cost/cooldown/story constraints
- prefer reusing MagicFramework item abilities, passive statuses, and ritual/action-list infrastructure before inventing a separate system
- avoid power creep that makes Arcane Forge weapons obsolete too quickly

First implementation pass:
1. Define one legendary weapon acquisition path: quest reward, grand forge recipe, planar reward, or ritual outcome.
2. Define one Grand Sorcery buff ritual with a clear target and duration.
3. Smoke test save/load, cooldowns, stacking, removal, generated descriptions, and UI clarity.

### MF-057 Chronometric Resurrection

Goal: add Chronomancy resurrection that restores a pawn through time magic rather than ordinary healing or necromancy.

Design direction:
- distinguish this from Soulcraft resurrection, Necromancy undead, and AeternusFaith rites
- possible identity: rewind a recent death, restore from a temporal echo, reverse corpse decay, or resurrect with age/time side effects
- require strict limits so it cannot trivialize death: time window, rare materials, severe cooldown, memory/age injury, or pawn-specific anchor
- save/load and corpse/reference cleanup are release blockers

First implementation pass:
1. Decide whether the effect targets a fresh corpse, grave, pawn record, or pre-made temporal anchor.
2. Implement one narrow Chronomancy-gated spell or ritual.
3. Smoke test corpse state, missing body cases, world pawn references, save/load, failure feedback, and side effects.

### MF-058 Soulcraft Lichdom

Goal: add lichdom under Soulcraft as a major advanced transformation with strong costs, risks, and identity.

Design direction:
- treat lichdom as a capstone character transformation, not just a buff
- likely needs a soul anchor, phylactery-like object, altered needs/health, social consequences, and special death/reform rules
- avoid implementing until undead, resurrection, and persistent pawn state patterns are stable enough to support it
- player feedback, save/load, death cleanup, and exploit prevention are all release-critical

First implementation pass:
1. Scope the minimum viable lich: transformation status only, phylactery behavior, or full death/reform loop.
2. Identify required framework hooks before content authoring.
3. Prototype behind dev/test access before exposing through Soulcraft research.

### MF-059 AeternusFaith Decorative And Religious Statues

Goal: add decorative and religious statues that strengthen AeternusFaith presentation and ritual spaces.

Design direction:
- start with placeable art/building defs that fit ossuary, bonewright, grave, shrine, and ritual-room themes
- support beauty, room impressiveness, ritual flavor, and ideology presentation without needing new ritual code
- prefer a small coherent set over many nearly identical objects

First implementation pass:
1. Define the first statue set and research/ideology availability rules.
2. Add textures, thing defs, costs, categories, descriptions, and room/stat effects.
3. Smoke test placement, minification, beauty/room stats, ritual room compatibility, and save/load.

### MF-060 Paintings And Platinum Consideration

Goal: evaluate whether paintings as decoration and platinum as a trade good belong in the first-party content set.

Consideration notes:
- paintings could add visual variety for colonies, ritual rooms, and arcane/religious spaces, but need a clear art/category role beyond duplicating vanilla sculpture
- platinum could be a high-value trade good, advanced magic reagent, legendary weapon input, or planar reward, but should not dilute silver/gold/gemstone economies
- do not implement either item until a release band needs them for presentation, economy, or reward structure

Decision checklist:
1. Identify which mod owns the content: MFVanilla, AeternusFaith, or a shared decorative pack.
2. Decide whether the content adds distinct gameplay or only visual variety.
3. If approved, split implementation into concrete follow-up tasks.

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

Current state:
 - MagicFramework now provides `SplashNoteDef` as a small XML-authored note surface for framework and dependent mod notes.
 - Splash notes can point at a mod package ID and display the current `About/About.xml` `modVersion` next to the mod name.
 - MagicFramework now shows a combined notes dialog from both startup and saved-game lifecycle checks when the active note set has not been seen.
 - The seen state is saved in `MagicFrameworkSettings` as a combined note key, so new or changed dependent-mod notes can trigger the dialog without repeatedly showing old notes.
 - MagicFramework and MFVanilla both provide player-facing splash notes with important settings notes, recent accomplishments, and planned-feature teasers.
 - MagicFramework and MFVanilla settings both include a button to re-show the latest magic notes.
 - MagicFramework and MFVanilla build successfully after the first pass.

Target coverage: 
 - Inform players about mod settings to enable/disable tech themed vanilla research. - done for MFVanilla
 - Consider other important details (to be determined). - initial framework, settings, and production-chain notes added
 - host in Magic Framework to provide a utility for dependent mods. - done
 - if possible, allow the utility to absorb details from any dependent mod and present it in one splashscreen instead of multiple. - done through `SplashNoteDef`
 - place a mod settings button in each dependent mod to re-show the latest notes. - done for MagicFramework and MFVanilla
 - this should be used for major changes and version info. - initial version-key support added

### MF-041 Arcane forge

Goal: Introduce an Arcane Forge production item

Target coverage: 
 - smoke test requirements to build this mid-late game item - done
 - validate spire-link gating, inspect strings, and bill availability - done
 - balance recipes for transforming good-or-better mundane weapons into magic versions - initial pass done
 - decide whether the first weapon set needs custom textures before release
