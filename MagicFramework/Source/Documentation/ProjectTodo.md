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
| MF-041 | P1 | M | MFVanilla | Finish and release-tune the Arcane Forge production item. |
| MF-042 | P1 | M | MFVanilla | Add scalable arcane treasure chests as quest/loot/world rewards. |
| MF-043 | P1 | M | MFVanilla | Plan the next-release MFVanilla content pillar set before final tuning. |
| MF-049 | P1 | XL | MFVanilla | Build arcane encounter maps and mission sites as a high-quality content pillar. |
| MF-050 | P1 | L | MFVanilla | Create arcane construct enemies by rebranding mechanoid combat roles as golems and automata. |
| MF-044 | P2 | L | MFVanilla | Add static per-map leyline maps and Leyline Sensitivity gameplay. |
| MF-045 | P2 | L | MFVanilla | Add elemental tribes, themed traders, and later magic-capable hostile pawns. |
| MF-046 | P2 | M | MFVanilla | Expand thin elemental schools with new spells and utility effects. |
| MF-047 | P2 | M | Equipment | Give first enchanted weapons unique MagicFramework-backed features. |

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

Priority:
- review the full production chain from raw inputs through finished scrolls and magic utility items
- make sure each bench has a clear role, research position, recipe set, work type, texture, cost, and power/facility story
- tune Arcane Ink, parchment/papyrus, gemstones, herbs, and generated spell scroll costs so the early loop is useful and the advanced loop has meaningful investment
- check that research gates unlock recipes, benches, utility buildings, and spell access in a sensible order
- add missing descriptions, inspect strings, category placement, tradeability, stack sizes, market values, and bulk/storage behavior where content feels placeholder
- smoke test a new colony path: discover Arcane gift, unlock early research, craft inputs, make scrolls, learn spells, and progress into advanced production

Next steps:
1. Smoke test trader stock and buy behavior for shaman, neolithic bulk, outlander bulk, and orbital bulk traders, with special attention to whether purchasable gemstone dust supports early Arcane Ink before Lapidary.
2. Review gemstone vein availability, mining output, market values, and stack sizes so gemstone dust and elemental focus production do not become either invisible or too abundant.
3. Decide whether elemental foci or enchanted weapons should appear in trader stock, quest rewards, or remain colony-crafted for the first release.
4. Add specific gemstone requirements to major scrolls only after the basic dust-and-ink loop and elemental focus loop feel stable.
5. Run an in-game smoke test of the full loop: buy or craft gemstone dust, grow herbs, make ink, make papyrus/parchment, scribe scroll, read scroll, cut gemstones, make foci, and enchant a weapon.

Success criteria:
- a player can understand what to build next without dev knowledge
- every production building earns its footprint in the colony
- scroll learning and generated scroll recipes feel like part of the same economy as the spells
- the content works as a stable example for third-party spell authors

### MF-038 MFVanilla Feature Completion

Goal: add a small number of features that deepen the released content without opening a broad framework expansion.

Next-release content direction:
- Favor content definition before final economy tuning. Balance should happen after the intended release content set is clearer.
- Make arcane treasure chests the next priority content system because they connect MFVanilla to quests, map finds, loot, traders, gemstones, scrolls, foci, enchanted gear, silver, and gold.
- Treat elemental tribes, leyline maps, school expansion, and special enchanted weapon behavior as major candidate pillars for the same or following release, depending on implementation scope.
- Audit research fields with no current application. Either attach real content to them or remove/hide them until they have a purpose.
- Reconsider plasteel/component costs on arcane benches and infrastructure after the content set is known. Prefer magical, pre-industrial, or trade-economy materials where they preserve meaningful scarcity.

Good candidates:
- scalable arcane treasure chests as quest rewards, rare loot, and random discoveries
- more utility recipes or buildings that consume magic production outputs
- targeted Arcane Forge expansions that reuse the elemental focus model without becoming a broad equipment framework
- one or two additional spell families only if they validate already-supported primitives or a narrowly needed framework hook
- stronger integration between spell metadata, scroll recipes, research, and generated descriptions
- clearer player feedback for spell scaling, active enhancement rules, and unlock paths
- balance pass for mana, cooldowns, costs, work amounts, and resource scarcity

Deferral rule:
- defer anything that primarily exists to prove a speculative framework system rather than make MFVanilla better now
- defer hostile AI casting, magic weapons/tools, real fire integration, and celestial event depth until after MFVanilla and AeternusFaith first-edition goals are in hand

### MF-042 Arcane Treasure Chests

Goal: add an arcane treasure chest system that makes MFVanilla content appear as mission rewards, rare loot, and occasional world/map discoveries.

Priority:
- this is the favored next MFVanilla content system before broad tuning
- use treasure chests to make scrolls, gemstones, foci, enchanted weapons, silver, gold, and future magic items feel discoverable outside colony production
- scale rewards with mission value and game advancement so early chests are useful without trivializing late rewards

Target behavior:
- a chest opens into a pseudo-random reward bundle with approximately one major item, two minor items, a few gemstones, silver, and gold
- major rewards can include rare scrolls, elemental foci, enchanted weapons, future staves/wands/apparel, or artifacts
- minor rewards can include arcane ink, papyrus/parchment, exotic herbs, gemstone dust, cut gemstones, common scrolls, or small foci
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
6. Add dev/debug spawning and logging for generated contents.
7. Smoke test XML load, chest use job, stable inspect ID, spawned reward placement, save/load repeatability, and opened chest destruction.
8. Wire into one safe acquisition path first, then broaden to quests/loot. - initial rare quest reward eligibility and ancient temple/ancient complex loot patches added; no trader inventory integration
9. Hide raw deterministic chest IDs from normal player-facing inspect text while retaining dev-mode diagnostics. - done; chests now show authored cache magnitude labels
10. Add a third/highest chest tier for rarer, higher-magnitude rewards. - initial `MFV_GrandArcaneTreasureChest` added
11. Add broader high-band rewards: quality armor/weapons, jade, devilstrand cloth, high-quality small art, and rare materials. - initial table entries added
12. Later: add cursed/trapped chest support, preferably as deterministic XML-authored curse tables or MagicFramework action lists triggered on open.

Success criteria:
- opening a chest feels like a magical reward, not a plain resource bundle
- rewards are useful at multiple colony stages without breaking the production loop
- generated output is stable across save/load
- generated output is deterministic from the stored chest ID/hash and tier/value data, supporting multiplayer-friendly behavior
- loot tables are easy to extend as MFVanilla gains staves, robes, wands, familiars, artifacts, or more spell schools

### MF-043 MFVanilla Next-Release Content Pillars

Goal: define the full content shape for the next MFVanilla release before final tuning.

Candidate pillars:
- Arcane loot and rewards: treasure chests, rare finds, quest rewards, and loot table integration.
- Arcane encounter maps: high-quality mission sites such as arcane caches, ruined sanctums, sealed vaults, leyline ruptures, elemental shrines, and cursed archives.
- Elemental school completion: fill thin schools, especially water, while adding spells that use existing primitives where possible.
- Leyline gameplay: make Leyline Sensitivity reveal or use a static per-map leyline map, then later connect nodes to mana, buildings, rituals, or incidents.
- Elemental cultures: fire, earth, air, and water tribes with themed traders first, and hostile magic pawns later once AI casting exists.
- Enchanted weapon identity: give each current Arcane Forge weapon a unique special feature rather than only adjusted damage.
- Arcane material economy: reduce or replace plasteel/component dependency on magical production where a pre-industrial magic economy makes more sense.

Candidate spells:
- Air: Air Blast, Fly.
- Fire/heat: Heat, Warmth.
- Earth: Golem, Earthy Grave, Stoneskin.
- Water: Deluge, Extinguish.

Research audit:
- identify research projects with no current unlocks or gameplay effect
- decide whether each should receive content in this release, move to a later release, or be removed/hidden until useful
- ensure research names imply real player-facing outcomes rather than only future intent

Task plan:
1. Inventory current MFVanilla unlocks by research project: spells, benches, recipes, buildings, items, traders, and settings-visible behavior.
2. Mark empty or weak projects, especially Leyline Sensitivity and advanced school nodes, with a proposed content role or removal/defer decision.
3. Pick the next-release pillar set, with treasure chests as the default first pillar.
4. Identify which pillar items need new MagicFramework code versus XML/content-only work.
5. Reorder tuning tasks after content scope is known.

Sequencing note:
- avoid deep balance work until treasure chests, spell additions, and any material-cost philosophy changes have settled enough to see the full economy.

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
5. Smoke test quest creation, map generation, chest opening, site cleanup, save/load, and reward extraction.

Map generation architecture:
- start with a small custom `GenStep` or symbol-resolver path rather than loose item scatter
- keep generation deterministic from site tile, quest/site ID, and authored profile data
- place a readable structure: entrance, approach space, vault/chamber, loot anchor, optional side room, debris/ruin dressing, and threat/hazard positions
- use existing walls/floors/props first, then add arcane-specific props as needed
- prefer XML-authored profiles so future mods can add new encounter themes without changing code

Content layers:
- rewards: arcane treasure chests, scrolls, foci, gemstones, enchanted weapons, silver/gold, and future artifacts
- props: broken arcane spires, inert forge fragments, glyph floors, arcane torches, ritual stones, ruined benches, leyline markers, sealed containers
- hazards: curse traps, fire/ice/lightning fields, dormant runes, unstable mana nodes, trapped doors/chests
- defenders: vanilla mechanoid/site threats first; later rebranded arcane constructs, elemental guardians, golems, summoned creatures, or magic-capable hostile pawns once AI casting exists

Implementation phases:
1. Design arcane site profile defs: theme, layout size, loot tier, chest def, hazard chance, defender/threat tags, and prop tables.
2. Implement the Arcane Cache Site vertical slice with a single profile and one chest.
3. Add deterministic generation diagnostics in dev mode so a site can report its seed/profile/chest ID.
4. Expand into Arcane Ruin Generator: multiple rooms, themed dressing, side loot, and optional hazards.
5. Add curse/trap integration using XML-authored trap tables or MagicFramework action lists.
6. Add themed variants: fire shrine, flooded vault, earth-buried sanctum, wind-swept ruin, leyline node, cursed archive.
7. Integrate higher-tier sites with quest points/game stage and future elemental tribes or magic AI when those systems exist.

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
- centipede-style enemy -> siege golem or iron colossus
- termite-style enemy -> breach golem or stonebreaker automaton

Implementation phases:
1. Use vanilla mechanoid defenders temporarily in MF-049 Arcane Cache Site maps.
2. Add one or two first construct pawn kinds with fantasy labels/descriptions and borrowed mechanoid behavior/stats.
3. Add initial textures or recolors so constructs are visually distinct from mechs.
4. Use constructs as arcane site defenders once smoke tested.
5. Add construct-specific drops, such as gemstone dust, arcane fragments, foci, jade, plasteel replacement candidates, or future construct cores.
6. Add elemental variants and resistances/vulnerabilities after fire/water/earth/air content is broader.
7. Consider later MagicFramework interactions: spells that disrupt constructs, bind golems, repair automata, or animate inert guardians.

Success criteria:
- arcane sites no longer feel like tech/mechanoid encounters once construct skins and defs are in place
- the first implementation keeps the reliability of mechanoid combat without requiring custom hostile magic AI
- construct roles are readable to players and scale naturally from low-tier cache guardians to high-tier vault defenders
- the system supports future story identity: ancient arcane defense systems, cursed guardians, elemental constructs, and automata recovered or studied by players

### MF-044 Leyline Map And Sensitivity Gameplay

Goal: make Leyline Sensitivity reveal a stable, useful magical geography layer for each map.

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
1. Add a map component that generates and saves leyline paths/nodes.
2. Add dev-mode debug drawing and logging for generated leyline data.
3. Add a simple overlay or inspection mode gated by Leyline Sensitivity.
4. Add one low-risk gameplay hook, such as improved mana recovery or arcane research speed near a node, only after the map layer is stable.

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
- fire tribe traders favor pyromancy scrolls, fire foci, rubies, heat/light infrastructure, and aggressive magic goods
- earth tribe traders favor geomancy scrolls, earth foci, emeralds, stone/gem materials, defensive goods, and construction-adjacent items
- air tribe traders favor aeromancy scrolls, air foci, topaz, mobility/control goods, and arcane spire/focus supplies
- water tribe traders favor aquamancy scrolls, water foci, sapphires, cooling/protection goods, medicine-adjacent items, and extinguishing tools
- factions can later produce hostile pawns with appropriate magic loadouts once AI casting exists

Implementation questions:
- should these be four full factions, trader kinds attached to existing factions, or rare world pawns/caravans?
- should they be neutral by default, mixed relations, or scenario/storyteller controlled?
- should their identities be tribal, monastic, guild-like, cultic, or mixed by element?
- should they sell finished enchanted gear, only inputs, or occasional rare major items through treasure chests/rewards?

First implementation pass:
1. Define the faction/trader scope without AI casting.
2. Add themed stock generators for elemental goods, scrolls, gemstones, and foci.
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
- Air Blast: push, stagger, damage, or knockback using displacement primitives.
- Fly: mobility/terrain bypass fantasy; likely needs careful framework and pathing design before implementation.
- Heat: targeted warming, heat damage, or room-temperature utility depending on design.
- Warmth: safer colony utility, probably a pawn/room comfort or hypothermia-protection effect.
- Golem: earth summon, likely requiring summon/spawn expansion and a pawn kind.
- Earthy Grave: earth control spell; possible immobilize, bury, down, slow, or terrain hazard.
- Stoneskin: defensive earth status using armor/stat modifiers and movement penalties.
- Deluge: water area effect, extinguish support, wet/frost synergy, or terrain/filth effect.
- Extinguish: fire-clearing utility, likely a small framework action if no existing primitive covers it cleanly.

Design direction:
- prioritize spells that strengthen underused schools and can be authored with current primitives
- use new framework hooks only when they clearly support multiple future spells or item features
- prefer utility and colony-support spells alongside combat spells so each school has a play identity

First implementation pass:
1. Classify each candidate as XML-only, small framework hook, or large framework feature.
2. Select two or three low-risk spells for the next content slice, likely Air Blast, Stoneskin, and Extinguish/Warmth.
3. Add spell defs, scroll generation coverage, research gates, icons, and generated summaries.
4. Smoke test targeting, mana/cooldown, scroll learning, and save/load for any persistent effects.

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
 - host in Magic Framework to provide a utility for dependent mods
 - if possible, allow the utility to absorb details from any dependent mod and present it in one splashscreen instead of multiple
 - place a mod settings button in each dependent mod to re-show the latest notes 
 - this should be used for major changes and version info

### MF-041 Arcane forge

Goal: Introduce an Arcane Forge production item

Target coverage: 
 - smoke test requirements to build this mid-late game item - done
 - validate spire-link gating, inspect strings, and bill availability - done
 - balance recipes for transforming good-or-better mundane weapons into magic versions - initial pass done
 - decide whether the first weapon set needs custom textures before release
