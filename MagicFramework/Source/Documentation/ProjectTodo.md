# MagicFramework Active Todo

This file is the short-term command center for MagicFramework, MFVanilla, and AeternusFaith. Detailed task notes live in the linked backlog files so this page stays small enough for quick release reviews.

Current emphasis: push MFVanilla toward a complete first-party content mod, keep MagicFramework support work close to the content needs that expose it, and move AeternusFaith into a side-track cadence until the MFVanilla completion pass is in better shape.

Complexity key: `XS` docs/XML tweak, `S` contained implementation, `M` multi-file feature, `L` cross-system feature, `XL` major pillar.
Priority key: `P0` immediate stability, `P1` near-term release, `P2` polish/content depth, `P3` later or exploratory.

## Backlog Files

- [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md): MFVanilla production, world missions, constructs, leylines, elemental faction/spells, enchanted weapons.
- [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md): framework polish, scaling, UI, docs, compatibility, AI.
- [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md): AeternusFaith release candidate and faith-specific follow-up work.
- [ProjectLongTermBacklog.md](ProjectLongTermBacklog.md): long-range school identity, planar/lichdom content, and exploratory systems.
- [ProjectCompleted.md](ProjectCompleted.md): completed implementation history.

## Last Uploaded Band

Uploaded May 25, 2026: MFVanilla 0.9.0 / MagicFramework 1.4.0 feature release. Planar Magic now has a playable gate/pocket-dimension foundation with save/load-tested return flow; Forbidden Lore gains Dominate Will and Forbidden Plague; Vitalism gains Cure Disease; Geomancy gains Dig and Earth Wall; spell and scroll text are fixed; and the first Arcane Forge weapon set has validated activated powers, passives, protections, and melee triggers.

Previous uploaded band May 22, 2026: MFVanilla 0.8.2 / MagicFramework 1.3.1 quality-of-life progression patch. Spell-unlocking research now grants a matching mystery scroll, scrolls are more available through Elementalist traders and arcane rewards, scroll prices scale with research depth, known-spell casting and scroll learning grant caster XP, and Arcane Gift pawns can apprentice under higher-level mentors during arcane work.

Previous major band uploaded May 19, 2026 at 11 PM: MFVanilla 0.8 / MagicFramework 1.3 world-layer mission release with Arcane Cache, Ruined Sanctum, and Sealed Vault opportunities; Leyline Sensitivity gameplay; Arcane Discipline rituals; elemental spell expansion; deterministic site generation; construct defenders; and updated splash notes.

Current local development after the uploaded band:
- Start the next iteration from a stable MFVanilla 0.9.0 baseline rather than opening another major pillar immediately.
- Aquamancy is less exposed after `Desiccate` smoke tested well against hostile humans as an effective hostile-target spell; keep animals, undead immunity, and final balance as watch items.
- Planar Magic should move from foundation to purpose: alignment timing, pocket duration, return capacity, material economy, player-facing text, optional small hazards/reward beats, and at least one reason for planar dimensions to matter beyond novelty. Void glass walls and phase stone walls now give the first planar materials concrete construction roles.
- Endgame design should start forming around magical Transcendence, arcane relics with map-wide effects, and eventual Soulcraft lichdom, but those should guide prerequisites and reward hooks before becoming implementation targets.
- The research/content audit remains important: identify underused or misleading research nodes now that the live tree has moved past the original roadmap.
- Shared lifecycle undead work now has two verified skeleton baselines: AF skeletons are selectable autonomous servants with natural RimWorld work restrictions and no drafting, while MFV raised skeletons remain temporary master-bound combat minions.

Playtest status:
- The research mystery-scroll drop was tested in game and works well.
- Arcane apprenticeship now works in game after replacing the brittle global-tick XP check with accumulated apprenticeship time; multiple apprentices can learn from the same mentor.
- Scroll availability and scaled scroll prices are rolled out as a QoL correction to spell acquisition pacing.
- AeternusFaith manifested spirits now smoke test correctly as non-player guest pawns: they remain unmanageable, non-hostile, save/load safely, and clean up through the tested lifecycle paths.
- AeternusFaith skeleton, ossuary, and spectre ritual dialogs now surface specific invalid-state messages for corpse validity, Bonewright requirements, reachability, reservations, missing ritual targets, and already-bound spectres.
- Arcane Cache has been confirmed in normal gameplay around day 45, including event arrival and expected completion.
- Deep Iron Golem has been dev tested and produced a strong boss fight.
- Automata are working fairly well in current testing.
- Planar Magic first-pass smoke testing looks functional and has clear expansion opportunities; save/reload inside the planar dimension appears to work as expected.
- Desiccate smoke testing against hostile humans works well and appears effective; animal targets, undead behavior, and final mana/cooldown tuning remain watch items.
- Dominate Will and Forbidden Plague build and deploy with generated scroll/recipe coverage; Forbidden Lore scrolls are excluded from research-completion mystery aid drops so initial acquisition must come from finding or trade. Dominate Will now smoke tests correctly after fixing the temporary compelled faction name-generation path, and Forbidden Plague spreads through clustered hostile pawns at a readable pace.
- Cure Disease smoke tests correctly against disease cleanup expectations.
- Dig and Earth Wall smoke test correctly.
- Spell text and spell scroll inspect text are verified fixed after copying the MagicFramework generated-description language file into the installed mod folder.
- Magic weapons smoke test correctly, including the remaining first-set enchanted weapon behaviors after the earlier Zephyr Spear pass.
- AF_Skeleton and MFV_Skeleton smoke tests confirm the composable lifecycle split: AF servants are selectable but not draftable, MFV skeletons are manageable combat minions, lifecycle traits drive disabled work through vanilla RimWorld behavior, and lifecycle inspect strings now surface control/work/expiry state.
- Ruined Sanctum and Sealed Vault remain observation items, but the shared mission loop is provisionally trusted unless variant-specific issues appear.

## Release Gate

- Clean builds for changed assemblies.
- Deployed local mod folders match workspace content.
- Version and splash/update notes are current.
- XML load check passes.
- Focused in-game smoke tests pass for changed systems.
- Known issues are moved into post-release notes unless they affect startup, save/load, cleanup, or basic usability.

## Current Focus Band

MFVanilla completion pass: move the first-party content mod from "broadly functional" toward "near-complete, shippable, and easy to explain." The current priority is to stabilize the implemented systems, close misleading research/content gaps, and update presentation rather than open another large pillar.

Primary target: MF-038 MFVanilla feature completion and post-0.9 cleanup.

- Planar Magic: polish the new planar gate and pocket-map loop after a successful first release, including alignment timing, pocket duration, return capacity, material economy, optional hazards/rewards, player-facing text, and practical purpose for exotic materials.
- World missions: continue normal-play validation for Arcane Cache, Ruined Sanctum, and Sealed Vault; tune only mission frequency, threat, reward tier, timeout, cleanup, and letter text unless a concrete blocker appears.
- Production/progression: watch the now mostly accepted production loop, scroll acquisition, caster XP, and apprenticeship pacing during normal play; reopen only for clear balance or clarity issues.
- Enchanted weapons: first-weapon-set smoke testing passed for 0.9.0; keep only balance/art polish watch items unless a concrete issue appears.
- Spell balance: Desiccate has passed first hostile-human smoke testing. Keep Aquamancy tuning focused on animals, undead immunity, mana, cooldown, and debuff duration before deciding whether the school needs another spell.
- Endgame scaffolding: begin capturing design notes for magical Transcendence, map-scale arcane relics, lichdom/phylactery prerequisites, and whether additional planes belong in MFVanilla or separate expansion mods.
- Research/content audit: identify underused or misleading research nodes now that leylines, elemental content, disciplines, missions, and planar magic exist; attach real content, rename expectations, or defer/hide stale promises.
- Release hygiene: update MFVanilla docs, splash notes, package metadata, XML load/build/deploy checks, and a tight release smoke checklist.

## AeternusFaith Side Track

AeternusFaith remains important, but it is no longer the main active release band. Work it in small slices while MFVanilla is the major focus.

- Keep the first-edition scope intact: neutral soul ecology first, cathedra-shaped necromancy second.
- Preserve the tested manifested-spirit and ritual-dialog work as the current baseline.
- Prefer small validation or presentation passes: Bonewright anointment smoke tests, ritual invalid-state readability, package metadata, ideology presentation, and one conservative ritual loop at a time.
- Do not expand pseudo-relationship memory, custom wall auto-joining, or broad faith content until MFVanilla completion pressure eases or a blocker proves they are necessary.

## Active Task Index

| ID | Priority | Complexity | Area | Status | Detail |
| --- | --- | --- | --- | --- | --- |
| MF-038 | P1 | M | MFVanilla | Release target | Drive the MFVanilla completion pass: prune stale roadmap promises, audit research/content gaps, update docs, and define the remaining shippable surface. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-038-mfvanilla-feature-completion). |
| MF-055 | P1 | M | MFVanilla/Planar | Polish | Post-0.9 polish for the now-functional planar gate, pocket-map, return, cleanup, material, and documentation loop. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-055-planar-magic-foundation-and-validation). |
| MF-046B | P1 | M | MFVanilla/Spells | Watch | Desiccate passed first hostile-human smoke testing; verify animals, undead immunity, scroll coverage, and final balance. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-046b-geomancy-and-aquamancy-follow-up). |
| MF-043 | P1 | M | MFVanilla/Sites | Watch | Continue observing Ruined Sanctum, Sealed Vault, cleanup, save/load, and repeat generation during normal play. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-043-mfvanilla-next-release-content-pillars). |
| MF-047 | P2 | S | MFVanilla/Items | Watch | First enchanted weapon set passed 0.9 smoke testing; keep art/balance/inspect polish visible. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-047-enchanted-weapon-special-features). |
| MF-051 | P2 | M | MFVanilla/Forbidden | Follow-up | Dominate Will and Forbidden Plague passed 0.9 smoke testing; tune and expand only after more live balance observation. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-051-forbidden-lore-first-spells). |
| MF-062 | P2 | M | MFVanilla/Vitalism | Watch | Cure Disease passed smoke testing; watch balance against ordinary medicine and disease pressure. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-062-cure-disease-spell-concept). |
| MF-037 | P1 | S | MFVanilla/Economy | Watch | Production loop is provisionally accepted; only reopen for concrete issues in pacing, player clarity, scroll access, apprenticeship, or gemstone/reward economy. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-037-mfvanilla-production-and-progression). |
| MF-031 | P2 | M | Framework/Docs | Support | Continue the Spell Design Guide where it helps stabilize the MFVanilla authoring surface and future content packs. See [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md#mf-031-spell-design-guide). |
| MF-032 | P2 | M | Framework/Compatibility | Support | Keep deterministic/save-load/multiplayer risks visible, especially around planar transfer, missions, item abilities, AI casting, and random production outcomes. See [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md#mf-032-compatibility). |
| MF-063 | P2 | M | Framework/Lifecycle | Active slice | Shared undead lifecycle foundations now have verified AF servant and MFV master-bound skeleton profiles; AF spectres have a first lifecycle profile and need Shroudhymn/debug manifestation smoke testing. See [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md#mf-063-shared-undead-and-construct-pawn-foundations). |
| MF-039 | P2 | M | AeternusFaith | Side track | Prepare the AeternusFaith first-edition release candidate in small slices while MFVanilla remains the main focus. See [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md#mf-039-aeternusfaith-first-edition). |
| MF-019 | P2 | S | AeternusFaith/UI | Side track | Polish ritual dialogs and smoke test small-resolution participant selection when working an Aeternus slice. See [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md#mf-019-ritual-dialog-improvements). |

## Completed Previous Slice

- MFVanilla 0.8.2 / MagicFramework 1.3.1: progression QoL patch covering research mystery-scroll drops, improved scroll trader/reward availability, research-depth scroll pricing, XP from scroll learning and known-spell casting, and Arcane Gift apprenticeship under higher-level mentors.
- MF-044: Leyline Sensitivity reveals a stable hidden leyline map, supports optional numeric inspection, boosts Arcane Gift pawn mana recovery near strong currents, and gives Arcane Forges a leyline resonance chance to improve enchanted weapon quality.
- MF-061: Arcane Discipline specialization gives research projects reward labels, lets Arcane Gift pawns embrace/advance disciplines through a marker ritual, shows discipline in the mana gizmo, optionally enforces discipline spell learning, and requires scroll scribes to know the spell being copied.
- MF-046: Elemental spell expansion covers Air Blast, Stoneskin, Extinguish, Deluge, Warmth, and sustained room-warming Heat, with scroll generation and research gates in place.
- MF-045: Elementalist tribe first pass adds one broad faction, an Elementalist caravan trader, mixed elemental trade stock, faction flavor, rare hostile spell-capable pawns, and visually readable themed caster garb.
- MF-055 first implementation: Planar Magic has moved out of pure long-term planning into local MFVanilla validation with planar gates, pocket maps, planar terrain/plants/materials, return handling, and debug support.
- MF-051 first implementations: Dominate Will adds maintained mind control and temporary non-player allied allegiance; Forbidden Plague adds a contagious treated disease that periodically creates lesion/blister wounds.
- MF-046B Geomancy follow-up: Earth Wall adds a real temporary wall-line spawn primitive; Dig adds a real mining primitive and a level-scaling Geomancy utility spell gated by Geomancy research.

## Following Release Bands

School identity and advanced magic after the MFVanilla completion pass:
- MF-051 Forbidden Lore expansion beyond Dominate Will and Forbidden Plague.
- MF-052 Illusionary pawns.
- MF-063 Shared undead and construct pawn foundations before deeper Necromancy, Fleshcraft, and Soulcraft creature work.
- MF-053 Necromancy undead pawns.
- MF-054 Fleshcraft golems.
- MF-055 Planar exploration expansion beyond the current gate/pocket foundation.
- MF-056 Grand Sorcery legendary weapons and buff ritual.
- MF-057 Chronometric resurrection.
- MF-058 Soulcraft lichdom.
- MF-064 Magical Transcendence endgame and map-scale arcane relics.

AeternusFaith follow-up:
- MF-020, MF-021, MF-023, MF-033, MF-034, MF-035, and MF-059 remain side-track or post-first-edition polish unless they become release blockers.

Consideration backlog:
- MF-060: evaluate paintings as decoration and platinum as a trade good only when they have a clear economy or presentation role.
- MF-049B: defer full connected-room arcane ruin generation to a future MFVanilla site release; ship MF-043 on the current authored/profile-driven mission sites first.

## Recommended Next Work

1. Finish Desiccate validation against animals and undead, then tune damage, cooldown, mana, and debuff duration only if normal play asks for it.
2. Continue practical-purpose Planar Magic work after void glass and phase stone walls: add small pocket-map reward beats or one more planar-material recipe, and outline whether future planes are MFVanilla content or separate expansion mods.
3. Start endgame notes for magical Transcendence and map-scale arcane relics; keep them as design scaffolding until the planar/material/relic prerequisites are clearer.
4. Do a research/content audit pass: underused nodes, stale promises in `Mods/MFVanilla/Documentation/ModPlan.md`, and misleading unlock expectations.
5. Work a small AeternusFaith side slice only if it stays bounded: AF spectre lifecycle manifestation smoke test, Bonewright anointment validation, package metadata, or ritual presentation.
