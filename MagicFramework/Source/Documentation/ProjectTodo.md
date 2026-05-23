# MagicFramework Active Todo

This file is the short-term command center for MagicFramework, MFVanilla, and AeternusFaith. Detailed task notes live in the linked backlog files so this page stays small enough for quick release reviews.

Complexity key: `XS` docs/XML tweak, `S` contained implementation, `M` multi-file feature, `L` cross-system feature, `XL` major pillar.
Priority key: `P0` immediate stability, `P1` near-term release, `P2` polish/content depth, `P3` later or exploratory.

## Backlog Files

- [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md): MFVanilla production, world missions, constructs, leylines, elemental faction/spells, enchanted weapons.
- [ProjectFrameworkBacklog.md](ProjectFrameworkBacklog.md): framework polish, scaling, UI, docs, compatibility, AI.
- [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md): AeternusFaith release candidate and faith-specific follow-up work.
- [ProjectLongTermBacklog.md](ProjectLongTermBacklog.md): long-range school identity, planar/lichdom content, and exploratory systems.
- [ProjectCompleted.md](ProjectCompleted.md): completed implementation history.

## Last Uploaded Band

Uploaded May 22, 2026: MFVanilla 0.8.2 / MagicFramework 1.3.1 quality-of-life progression patch. Spell-unlocking research now grants a matching mystery scroll, scrolls are more available through Elementalist traders and arcane rewards, scroll prices scale with research depth, known-spell casting and scroll learning grant caster XP, and Arcane Gift pawns can apprentice under higher-level mentors during arcane work.

Previous major band uploaded May 19, 2026 at 11 PM: MFVanilla 0.8 / MagicFramework 1.3 world-layer mission release with Arcane Cache, Ruined Sanctum, and Sealed Vault opportunities; Leyline Sensitivity gameplay; Arcane Discipline rituals; elemental spell expansion; deterministic site generation; construct defenders; and updated splash notes.

Playtest status:
- The research mystery-scroll drop was tested in game and works well.
- Arcane apprenticeship now works in game after replacing the brittle global-tick XP check with accumulated apprenticeship time; multiple apprentices can learn from the same mentor.
- Scroll availability and scaled scroll prices are rolled out as a QoL correction to spell acquisition pacing.
- AeternusFaith manifested spirits now smoke test correctly as non-player guest pawns: they remain unmanageable, non-hostile, save/load safely, and clean up through the tested lifecycle paths.
- AeternusFaith skeleton, ossuary, and spectre ritual dialogs now surface specific invalid-state messages for corpse validity, Bonewright requirements, reachability, reservations, missing ritual targets, and already-bound spectres.
- Arcane Cache has been confirmed in normal gameplay around day 45, including event arrival and expected completion.
- Deep Iron Golem has been dev tested and produced a strong boss fight.
- Automata are working fairly well in current testing.
- Ruined Sanctum and Sealed Vault remain observation items, but the shared mission loop is provisionally trusted unless variant-specific issues appear.

## Release Gate

- Clean builds for changed assemblies.
- Deployed local mod folders match workspace content.
- Version and splash/update notes are current.
- XML load check passes.
- Focused in-game smoke tests pass for changed systems.
- Known issues are moved into post-release notes unless they affect startup, save/load, cleanup, or basic usability.

## Current Release Candidate

AeternusFaith first edition: focused faith/ritual content release centered on soul tracking, general haunting risk, Bonewright cathedra ritual circles, Ossanith skeleton/ossuary/rest rites, Shroudhymn temporary spirit behavior, ideology completion, package metadata, and first-public-release presentation.

Primary target: MF-039 AeternusFaith first-edition release candidate.

- Package readiness: verify `About.xml`, dependencies, preview/icon assets, assembly output, and Workshop/GitHub packaging expectations.
- Content boundary: follow the RC1 spine in [AeternusFaithRC1Plan.md](../../../AeternusFaith/Documentation/AeternusFaithRC1Plan.md): neutral soul ecology first, cathedra-shaped necromancy second.
- Soul ecology: stabilize player-relevant soul records, death/corpse context, release/rest state, haunting suppression, and dev-mode inspection.
- Ritual circles: provide Ossanith, Animara, Choralum, Shroudhymn, and Voressai circles with at least one working ritual each; Ossanith should be the deepest first-release loop.
- Ritual smoke testing: test ideology generation, role assignment, ritual action gizmos, ritual jobs, room/lectern placement, corpse selection, reservation/reachability, save/load, and cleanup.
- Player readability: skeleton, ossuary, and spectre ritual setup now reports specific invalid states; continue smoke testing at small resolutions and through normal play.
- Undead/spectre lifecycle: guest ownership, control expectations, save/load, death/downing, map removal, and stale-reference cleanup have passed smoke testing; keep watching only for normal-play edge cases.
- First-edition deferrals: keep pseudo-relationship memory and custom wall auto-joining as post-first-edition unless release testing shows they are essential.

## Active Task Index

| ID | Priority | Complexity | Area | Status | Detail |
| --- | --- | --- | --- | --- | --- |
| MF-039 | P1 | M | AeternusFaith | Release target | Prepare the AeternusFaith first-edition release candidate. See [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md#mf-039-aeternusfaith-first-edition). |
| MF-019 | P1 | S | AeternusFaith/UI | Release support | Polish ritual dialogs and smoke test small-resolution participant selection. See [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md#mf-019-ritual-dialog-improvements). |
| MF-033 | P1 | M | AeternusFaith | Release support | Validate first-edition faith content boundary and presentation. See [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md#mf-039-aeternusfaith-first-edition). |
| MF-034 | P1 | M | AeternusFaith | Release support | Validate first-edition ritual, undead, and spectre behavior. See [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md#mf-039-aeternusfaith-first-edition). |
| MF-035 | P1 | M | AeternusFaith | Release support | Validate first-edition packaging, metadata, and release presentation. See [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md#mf-039-aeternusfaith-first-edition). |
| MF-059 | P2 | S | AeternusFaith | Presentation polish | Add decorative and religious statues if the first edition needs a stronger ritual-space presentation pass. See [ProjectAeternusFaithTodo.md](ProjectAeternusFaithTodo.md#mf-059-aeternusfaith-decorative-and-religious-statues). |
| MF-043 | P2 | L | MFVanilla | Post-upload watch | Continue observing Ruined Sanctum, Sealed Vault, cleanup, save/load, and repeat generation during normal play. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-043-mfvanilla-next-release-content-pillars). |
| MF-037 | P2 | M | MFVanilla | Post-upload watch | Production loop is provisionally accepted; only reopen for concrete issues or gemstone/reward economy tuning. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-037-mfvanilla-production-and-progression). |
| MF-038 | P2 | M | MFVanilla | Planning | Keep MFVanilla roadmap current and prune stale notes after the uploaded band settles. See [ProjectMFVanillaTodo.md](ProjectMFVanillaTodo.md#mf-038-mfvanilla-feature-completion). |

## Completed Previous Slice

- MFVanilla 0.8.2 / MagicFramework 1.3.1: progression QoL patch covering research mystery-scroll drops, improved scroll trader/reward availability, research-depth scroll pricing, XP from scroll learning and known-spell casting, and Arcane Gift apprenticeship under higher-level mentors.
- MF-044: Leyline Sensitivity reveals a stable hidden leyline map, supports optional numeric inspection, boosts Arcane Gift pawn mana recovery near strong currents, and gives Arcane Forges a leyline resonance chance to improve enchanted weapon quality.
- MF-061: Arcane Discipline specialization gives research projects reward labels, lets Arcane Gift pawns embrace/advance disciplines through a marker ritual, shows discipline in the mana gizmo, optionally enforces discipline spell learning, and requires scroll scribes to know the spell being copied.
- MF-046: Elemental spell expansion covers Air Blast, Stoneskin, Extinguish, Deluge, Warmth, and sustained room-warming Heat, with scroll generation and research gates in place.
- MF-045: Elementalist tribe first pass adds one broad faction, an Elementalist caravan trader, mixed elemental trade stock, faction flavor, rare hostile spell-capable pawns, and visually readable themed caster garb.

## Following Release Bands

School identity and advanced magic:
- MF-051 Forbidden Lore mind control.
- MF-052 Illusionary pawns.
- MF-053 Necromancy undead pawns.
- MF-054 Fleshcraft golems.
- MF-055 Planar exploration.
- MF-056 Grand Sorcery legendary weapons and buff ritual.
- MF-057 Chronometric resurrection.
- MF-058 Soulcraft lichdom.

AeternusFaith follow-up:
- MF-020, MF-021, and MF-023 remain post-first-edition polish unless they become release blockers.

Consideration backlog:
- MF-060: evaluate paintings as decoration and platinum as a trade good only when they have a clear economy or presentation role.
- MF-049B: defer full connected-room arcane ruin generation to a future MFVanilla site release; ship MF-043 on the current authored/profile-driven mission sites first.

## Recommended Next Work

1. Smoke-test the Bonewright anointment loop: circle command, popup selection, Soulwarden bootstrap, cap behavior, save/load, and rite gating.
2. Smoke-test the improved skeleton, ossuary, and spectre ritual invalid-state messages in the dialog and start-job rejection paths.
3. Lock first rituals for Animara, Choralum, and Voressai, keeping scope conservative.
4. Audit AeternusFaith ideology, package metadata, dependencies, assets, and assembly output.
5. Run final XML load, build/deploy, and first-edition release hygiene checks.
