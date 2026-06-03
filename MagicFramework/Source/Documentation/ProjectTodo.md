# MagicFramework Active Todo

This file is the short-term command center for MagicFramework, MFVanilla, and Aeternus Core. Detailed task notes live in the linked backlog files so this page stays small enough for quick release reviews.

Current emphasis: finish the Aeternus Core RC1 packaging slice now that MFVanilla 0.10.0 / MagicFramework 1.5.0 is released. Keep MagicFramework support work close to Aeternus Core lifecycle and ritual needs, and treat MFVanilla as a watch/polish surface unless live play exposes a concrete blocker.

Complexity key: `XS` docs/XML tweak, `S` contained implementation, `M` multi-file feature, `L` cross-system feature, `XL` major pillar.
Priority key: `P0` immediate stability, `P1` near-term release, `P2` polish/content depth, `P3` later or exploratory.

## Backlog Files

- [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md): MFVanilla production, world missions, constructs, leylines, elemental faction/spells, enchanted weapons.
- [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md): framework polish, scaling, UI, docs, compatibility, AI.
- [ProjectAeternusCoreTodo.md](ProjectAeternusCoreTodo.md): Aeternus Core release candidate and faith/Bonewright follow-up work.
- [ProjectLongTermBacklog.md](ProjectLongTermBacklog.md): long-range school identity, planar/lichdom content, and exploratory systems.
- [ProjectCompleted.md](ProjectCompleted.md): completed implementation history.

## Last Uploaded Band

Uploaded June 1, 2026: MFVanilla 0.10.0 / MagicFramework 1.5.0 feature release. Chronomancy now has Temporal Resurrection and Borrowed Season; Illusion gains Phantom Reinforcements; Planar Magic gains clearer material purpose through phase stone walls, void glass walls, named destinations, and Cinderdeep harvestables; arcane ink can use healroot as an alternative organic input; and the planar gate/research/documentation loop received release polish.

Previous uploaded band May 25, 2026: MFVanilla 0.9.0 / MagicFramework 1.4.0 feature release. Planar Magic now has a playable gate/pocket-dimension foundation with save/load-tested return flow; Forbidden Lore gains Dominate Will and Forbidden Plague; Vitalism gains Cure Disease; Geomancy gains Dig and Earth Wall; spell and scroll text are fixed; and the first Arcane Forge weapon set has validated activated powers, passives, protections, and melee triggers.

Previous uploaded band May 22, 2026: MFVanilla 0.8.2 / MagicFramework 1.3.1 quality-of-life progression patch. Spell-unlocking research now grants a matching mystery scroll, scrolls are more available through Elementalist traders and arcane rewards, scroll prices scale with research depth, known-spell casting and scroll learning grant caster XP, and Arcane Gift pawns can apprentice under higher-level mentors during arcane work.

Previous major band uploaded May 19, 2026 at 11 PM: MFVanilla 0.8 / MagicFramework 1.3 world-layer mission release with Arcane Cache, Ruined Sanctum, and Sealed Vault opportunities; Leyline Sensitivity gameplay; Arcane Discipline rituals; elemental spell expansion; deterministic site generation; construct defenders; and updated splash notes.

Current local development after the uploaded band:
- Start the next iteration from a stable MFVanilla 0.10.0 / MagicFramework 1.5.0 baseline.
- Make Aeternus Core RC1 the active slice: Bonewright anointment validation, ritual-dialog smoke tests, Shroudhymn Veilbound Shade lifecycle validation, cathedra presentation, ideology/package review, and one conservative ritual/content decision at a time.
- Keep MagicFramework lifecycle work tightly scoped to AF release needs: shade manifestation, non-player guest control, cleanup, save/load, and inspect/debug clarity.
- Keep MFVanilla in watch mode. Desiccate animal/undead behavior, Ruined Sanctum/Sealed Vault observation, planar pocket balance, Chronomancy/Illusion icon and balance polish, and late-node research honesty remain visible but are not the primary slice.
- Deeper MFVanilla endgame design around Transcendence, relics, lichdom, Grand Sorcery, Fleshcraft, and Infernal Pact should stay as notes until the AF RC1 surface is steadier.

Playtest status:
- The research mystery-scroll drop was tested in game and works well.
- Arcane apprenticeship now works in game after replacing the brittle global-tick XP check with accumulated apprenticeship time; multiple apprentices can learn from the same mentor.
- Scroll availability and scaled scroll prices are rolled out as a QoL correction to spell acquisition pacing.
- Aeternus Core manifested spirits now smoke test correctly as non-player guest pawns: they remain unmanageable, non-hostile, save/load safely, and clean up through the tested lifecycle paths.
- Aeternus Core ritual dialogs now surface specific invalid-state messages for corpse validity, Bonewright requirements, reachability, reservations, missing ritual targets, missing armor, and already-bound shades.
- The universal Bonewright lectern now reads adjacent ritual circles, filters out irrelevant ritual gizmos, and shows one disabled fallback gizmo when no supported circle is adjacent.
- Choralum's `Animate Reliquary Warden` rite is implemented and smoke tested: it consumes a corpse plus nearby plate/flak armor and creates a tougher armored skeletal guardian through the shared undead factory. Reliquary Wardens now use the limited-labor intelligence tier and inherit a reduced echo of simple source skills.
- Aeternus Core bound undead now use a one-minion-per-Bonewright rule across the current animation rites. Bound minions follow drafted masters through the lifecycle escort loop, attack hostiles near their drafted master, and can be dismissed through an Ossanith burial rite that fills an ossuary bone box.
- Animara's `Animate Echobound Revenant` rite now has a real first-pass pawn kind/race/job path instead of borrowing the Ossanith skeleton result. It creates a skeletal limited-labor revenant through the shared undead factory, blocks complex work such as crafting/art/medicine/research, and copies a reduced practical skill echo from the source pawn.
- Master death/destruction now has a validated lost-undead behavior for current bound undead: `AF_BoundUndeadMinion` stores the master binding, shows the bound/failing state as a hediff marker, starts a short grace timer when the master dies or is destroyed, then renames the minion to its cathedra lost form, clears the binding, and begins attacking nearby living pawns.
- Aeternus Core undead now project a standardized `Unnerving aura` onto nearby living non-Bonewright pawns. The debuff lingers for several hours, excludes undead and Bonewrights, and has a stronger tier for Veilbound Shades and the future Voressai Hungering Husk.
- Aeternus Core RC1 validation pass reported June 3, 2026: Bonewright anointment, ritual invalid-state messaging, universal lectern filtering, Echobound Revenant, Reliquary Warden, Veilbound Shade lifecycle, bound-minion rules, master-loss/lost-undead behavior, dismissal/bone-box outcome, and undead aura behavior all passed. The dismissal timing quirk is now fixed and regression tested: the conductor waits for the minion to reach the ossuary, then proceeds properly.
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
- Phantom Reinforcements smoke testing confirms the Illusion MVP works as intended after cleanup: phantasms appear as disposable reinforcements, draw attacks, vanish on damage, avoid corpse/death/mourning events, and no longer emit normal-play render warnings.
- Ruined Sanctum and Sealed Vault remain observation items, but the shared mission loop is provisionally trusted unless variant-specific issues appear.

## Release Gate

- Clean builds for changed assemblies.
- Deployed local mod folders match workspace content.
- Version and splash/update notes are current.
- XML load check passes.
- Focused in-game smoke tests pass for changed systems.
- Known issues are moved into post-release notes unless they affect startup, save/load, cleanup, or basic usability.

## Current Focus Band

Aeternus Core RC1 packaging slice: make the first public faith/Bonewright ritual surface reliable, understandable, and packageable without broadening the doctrine into a large new expansion pass.

Primary target: MF-039 Aeternus Core first-edition release candidate.

- Bonewright anointment: validation passed for circle command, popup selection, Soulwarden bootstrap, membership cap behavior, save/load, and rite gating.
- Ritual dialogs: validation passed for universal lectern filtering, wrong corpse, missing target, missing armor, non-Bonewright conductor, reachability, reservation, existing bound minion, and dismissal/bone-box messages.
- Veilbound Shade lifecycle: validation passed for Shroudhymn rite-bound shades and debug manifestation with the shared lifecycle profile: guest/non-draftable control, manifest/unmanifest, save/load while manifested, death/downing, map removal, and cleanup.
- Cathedra surface: Ossanith Skeleton, Echobound Revenant, Reliquary Warden, and Veilbound Shade have working player-facing rites through the universal Bonewright lectern. Ossanith also has a dismissal path for a Bonewright's current minion. Bound-undead master binding and master-death/lost-undead behavior are implemented and smoke tested.
- Dismissal presentation: validation passed after the synchronization fix; the conductor waits for the minion to reach the ossuary before the rite timer completes.
- Ideology and package review: check memes, precepts, roles, apparel, research gates, starting ideology/preset, build availability, metadata, preview/icon assets, dependencies, assembly output, XML load, local deployment, and release notes.
- Scope guard: defer pseudo-relationship memory, custom ritual-room tiles, decorative building sets, custom wall auto-joining, full Choralum aura/guardian suites, and full Voressai hostile ecosystems unless they become necessary for RC1 coherence.

## MFVanilla Watch Track

MFVanilla remains the mature first-party content mod after the 0.10.0 release. Keep watching it during normal play, but avoid opening another major pillar until Aeternus Core RC1 pressure eases or a concrete MFVanilla issue appears.

- Desiccate: animal targets, undead immunity/targeting feedback, mana, cooldown, and debuff duration remain balance watch items.
- Sites: Ruined Sanctum and Sealed Vault remain observation items; tune only mission frequency, threat, reward tier, timeout, cleanup, and letter text unless a concrete blocker appears.
- Planar Magic: watch alignment timing, pocket duration, return capacity, material value, Cinderdeep harvestables, and whether pocket-map reward beats are needed before the next MFVanilla content push.
- Chronomancy and Illusion: keep Temporal Resurrection, Borrowed Season, and Phantom Reinforcements in icon/art/text/save-load/balance watch status.
- Late nodes: Grand Sorcery, Fleshcraft, Infernal Pact, Soulcraft lichdom, Transcendence, and map-scale arcane relics stay in design scaffolding until a future MFVanilla-focused slice.

## Active Task Index

| ID | Priority | Complexity | Area | Status | Detail |
| --- | --- | --- | --- | --- | --- |
| MF-039 | P1 | M | Aeternus Core | Release target | Prepare the Aeternus Core first-edition release candidate: Bonewright anointment, ritual-dialog validation, spectre lifecycle smoke tests, cathedra surface decisions, ideology/package review, and RC1 release hygiene. See [ProjectAeternusCoreTodo.md](ProjectAeternusCoreTodo.md#mf-039-aeternus-core-first-edition). |
| MF-019 | P1 | S | Aeternus Core/UI | Complete | Ritual participant dialogs are implemented for the current RC1 rites, including invalid-state reasons and widened action buttons. Remaining checks are ordinary MF-039 RC1 smoke testing. See [ProjectAeternusCoreTodo.md](ProjectAeternusCoreTodo.md#mf-019-ritual-dialog-improvements). |
| MF-063 | P1 | M | Framework/Lifecycle | Support | Support the AF RC1 lifecycle surface: spectre manifestation, guest/non-draftable control, cleanup, save/load, debug inspection, and reusable lifecycle readouts. See [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md#mf-063-shared-undead-and-construct-pawn-foundations). |
| MF-038 | P2 | M | MFVanilla | Watch | MFVanilla 0.10.0 shipped the completion/polish band; keep research/content honesty and late-node design notes visible for the next MFVanilla-focused slice. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-038-mfvanilla-feature-completion). |
| MF-055 | P2 | M | MFVanilla/Planar | Watch | Post-0.10 planar pocket, material, Cinderdeep, and reward-beat tuning remain watch items rather than the active release target. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-055-planar-magic-foundation-and-validation). |
| MF-052 | P2 | M | MFVanilla/Illusion | Watch | Phantom Reinforcements gives Illusion a first playable decoy spell; watch save/load, enemy targeting, art/icon polish, and whether true mirror images are worth a later pass. See [ProjectLongTermBacklog.md](ProjectLongTermBacklog.md#mf-052-illusionary-pawns). |
| MF-046B | P2 | M | MFVanilla/Spells | Watch | Desiccate passed first hostile-human smoke testing; verify animals, undead immunity, scroll coverage, and final balance. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-046b-geomancy-and-aquamancy-follow-up). |
| MF-043 | P2 | M | MFVanilla/Sites | Watch | Continue observing Ruined Sanctum, Sealed Vault, cleanup, save/load, and repeat generation during normal play. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-043-mfvanilla-next-release-content-pillars). |
| MF-047 | P2 | S | MFVanilla/Items | Watch | First enchanted weapon set passed 0.9 smoke testing; keep art/balance/inspect polish visible. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-047-enchanted-weapon-special-features). |
| MF-051 | P2 | M | MFVanilla/Forbidden | Follow-up | Dominate Will and Forbidden Plague passed 0.9 smoke testing; tune and expand only after more live balance observation. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-051-forbidden-lore-first-spells). |
| MF-062 | P2 | M | MFVanilla/Vitalism | Watch | Cure Disease passed smoke testing; watch balance against ordinary medicine and disease pressure. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-062-cure-disease-spell-concept). |
| MF-037 | P2 | S | MFVanilla/Economy | Watch | Production loop is provisionally accepted; only reopen for concrete issues in pacing, player clarity, scroll access, apprenticeship, or gemstone/reward economy. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-037-mfvanilla-production-and-progression). |
| MF-031 | P2 | M | Framework/Docs | Support | Continue the Spell Design Guide where it helps stabilize the MFVanilla authoring surface and future content packs. See [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md#mf-031-spell-design-guide). |
| MF-032 | P2 | M | Framework/Compatibility | Support | Keep deterministic/save-load/multiplayer risks visible, especially around planar transfer, missions, item abilities, AI casting, and random production outcomes. See [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md#mf-032-compatibility). |

## Completed Previous Slice

- MFVanilla 0.10.0 / MagicFramework 1.5.0: Chronomancy first identity through Temporal Resurrection and Borrowed Season, Illusion first identity through Phantom Reinforcements, planar materials and named destinations, Cinderdeep harvestables, healroot-compatible arcane ink, and post-0.9 release polish.
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
- MF-063 Shared undead and construct pawn foundations before deeper Necromancy, Fleshcraft, and Soulcraft creature work.
- MF-053 Necromancy undead pawns.
- MF-054 Fleshcraft golems.
- MF-055 Planar exploration expansion beyond the current gate/pocket foundation.
- MF-056 Grand Sorcery legendary weapons and buff ritual.
- MF-052 Illusionary pawns first pass is implemented in MFVanilla; keep future work to art/icon polish, save/load validation, richer mirror images, and lifecycle reuse.
- MF-057 Chronometric resurrection first pass is implemented in MFVanilla; keep future work to balance, icon polish, and deeper temporal-memory infrastructure.
- MF-058 Soulcraft lichdom.
- MF-064 Magical Transcendence endgame and map-scale arcane relics.

Aeternus Core follow-up:
- MF-020, MF-021, MF-023, MF-033, MF-034, MF-035, and MF-059 remain side-track or post-first-edition polish unless they become release blockers.

Consideration backlog:
- MF-060: evaluate paintings as decoration and platinum as a trade good only when they have a clear economy or presentation role.
- MF-049B: defer full connected-room arcane ruin generation to a future MFVanilla site release; ship MF-043 on the current authored/profile-driven mission sites first.

## Recommended Next Work

1. Review Aeternus Core ideology/package readiness: memes, precepts, roles, apparel, research gates, preset, build availability, `About.xml`, preview/icon assets, dependencies, build/deploy output, XML load, and release notes.
2. Decide the RC1 content boundary: keep RC1 in validation/package mode unless one small presentation improvement clearly improves first-edition coherence.
3. Prepare the first-edition release notes from the validated feature surface: soul tracking, Bonewright anointment, four player-facing rites, dismissal/rest handling, lost-undead consequences, and undead aura behavior.
