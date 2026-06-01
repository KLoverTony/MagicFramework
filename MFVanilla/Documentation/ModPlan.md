# MFVanilla - Magic Framework Vanilla Extensions

This mod adds vanilla content extensions for Magic Framework, including an arcane research tree, items, spells, creatures, and optional suppression of vanilla technology research.

## Current Status

- The `Arcane` research tab and the current foundation, school, advanced, and forbidden research tree are implemented in XML.
- The standard arcane research bench is available without research; the advanced bench is unlocked by `MFV_ArcaneSecrets` and required by late research.
- Vanilla tech suppression is implemented as Harmony patches over research visibility/startability rather than by deleting defs.
- Pawns can gain the `MFV_ArcaneGift` trait after sustained research work at arcane benches, with higher awakening odds at the advanced bench.
- Generated spell scroll items and recipes cover the current MFVanilla spell library, with learning gated by Arcane Gift and each spell's research prerequisites.
- Gemstone chunks, raw pieces, cut gems, dust byproducts, lapidary recipes, and related trader inventory are implemented.
- Arcane treasure chests, automata defenders, and the current Arcane Cache / Ruined Sanctum / Sealed Vault mission site set are implemented for release smoke testing.
- Leyline Sensitivity reveals a saved leyline overlay, improves mana recovery near strong currents, and gives Arcane Forges a leyline resonance chance to improve enchanted weapon quality.
- Arcane Discipline specialization is implemented through research reward labels, discipline rituals, mana-gizmo display, optional discipline learning restrictions, and scroll-scribe knowledge checks.
- The first Arcane Forge enchanted weapon set is implemented: Flaming Longsword, Zephyr Spear, Tidebreaker Mace, and Stonefall Mace.
- Planar Magic has a functional first pass: buildable planar gates, alignment windows, selected-pawn traversal into temporary planar pocket maps, return selection, off-map transport blocking, pocket cleanup, planar terrain/plants/materials, and debug support.
- Validation spell content and matching gizmo icons currently live in this mod under `Defs/SpellDefs` and `Textures/UI/Gizmos/Spells`.

## Folder Structure

```
MFVanilla/
|-- About/
|   |-- About.xml          # Mod metadata with dependencies on Harmony and MagicFramework
|   |-- Preview.png        # Mod preview image
|   `-- ModIcon.png        # Mod icon
|-- Defs/
|   |-- IncidentDefs/         # Arcane mission incidents
|   |-- RecipeDefs/           # Arcane crafting, gemstones, scroll recipes
|   |-- ResearchProjectDefs/   # Custom research projects
|   |-- Sites/                # Arcane and planar site parts, profiles, dimensions, and gen steps
|   |-- ThingDefs/
|   |   |-- Items/             # Magic items and equipment
|   |   `-- Buildings/         # Magic structures and stations
|   |-- PawnKindDefs/          # Creature definitions
|   |-- RaceDefs/              # Custom races
|   `-- SpellDefs/             # Additional spells
|-- Source/                    # C# source code
|-- Textures/                  # Texture assets
`-- Assemblies/                # Compiled DLLs
```

## Feature Plans

### 1. Research System

MFVanilla uses a standalone `Arcane` research tab. When vanilla technology suppression is enabled, this tree becomes the colony's main non-industrial progression path. When suppression is disabled, it still works as a parallel magic progression path.

Design goals:
- Keep the first tier practical and colony-focused, so magic feels like an alternative infrastructure path instead of only combat power.
- Use `MFV_Spellcraft` as the main spell-school entry point.
- Split elemental practice through `MFV_Elementalism`, then branch into individual elemental disciplines.
- Keep forbidden research visually and mechanically separate, with higher costs, darker consequences, or story hooks.
- Avoid requiring suppressed vanilla research such as `Electricity`; arcane infrastructure should stand on its own.
- Use stable RimWorld 1.6 `ResearchProjectDef` fields. Current advanced projects also use `requiredResearchBuilding` to require the advanced arcane research bench.
- Keep research promises honest: if a node has only a foundation, its description should describe the working foundation and leave expansion hooks for later.

Research tab:

| Def | Label | Notes |
|-----|-------|-------|
| `MFV_Arcane` | Arcane | Dedicated tab for all MFVanilla research projects. |

Tier 0 - Foundation:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_ArcaneTheory` | Arcane Theory | 500 | None | Basic mana awareness and entry into practical arcana. |

Tier 1 - Practical Arcana:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_LeylineSensitivity` | Leyline Sensitivity | 700 | Arcane Theory | Reveals leyline overlay, supports mana recovery near strong currents, and powers Arcane Forge resonance. |
| `MFV_Lapidary` | Lapidary | 700 | Arcane Theory | Gemstone cutting, gemstone dust byproducts, and later focus/resource self-sufficiency. |
| `MFV_Alchemy` | Alchemy | 800 | Arcane Theory | Exotic herbs, arcane ink, and practical magical reagent crafting. |
| `MFV_Papyrus` | Papyrus | 500 | Arcane Theory | Early writing material production from wood or plant matter. |
| `MFV_Spellcraft` | Spellcraft | 900 | Arcane Theory | Entry point for teachable magical schools. |

Tier 2 - Spell Schools:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_Elementalism` | Elementalism | 1000 | Spellcraft | Shared foundation for fire, water, air, and earth magic. |
| `MFV_Enchantment` | Enchantment | 1000 | Elementalism | Arcane Forge, elemental foci, arcane spires, enchanted weapons, and item magic. |
| `MFV_Summoning` | Summoning | 1200 | Spellcraft | Calling and commanding otherworldly or temporary entities. |
| `MFV_Transformation` | Transformation | 1200 | Spellcraft | Magical alteration, transmutation, and body/object conversion concepts. |
| `MFV_Illusion` | Illusion | 1200 | Spellcraft | Deceptive images, sensory manipulation, concealment, and misdirection. Current payoff: Phantom Reinforcements, a decoy spell that creates short-lived illusory allies. |
| `MFV_Vitalism` | Vitalism | 1100 | Spellcraft | Healing, regeneration, disease resistance, and growth magic. |
| `MFV_Parchmentry` | Parchmentry | 900 | Papyrus | Durable animal-skin writing material production. |
| `MFV_Pyromancy` | Pyromancy | 1000 | Elementalism | Firebolt, fireball variants, wall of fire, heat hazards. |
| `MFV_Aquamancy` | Aquamancy | 1000 | Elementalism | Water shaping, protection, control effects, and hostile water magic. |
| `MFV_Aeromancy` | Aeromancy | 1000 | Elementalism | Push/pull, blink-step support, wind pressure, lightning-adjacent mobility. |
| `MFV_Geomancy` | Geomancy | 1000 | Elementalism | Stone barriers, terrain shaping, golems, defensive fields. |
| `MFV_Necromancy` | Necromancy | 1400 | Vitalism | Corpse use, spirit binding, undead summons; morally grey but not yet forbidden. |

Tier 3 - Arcane Secrets:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_ArcaneSecrets` | Arcane Secrets | 1800 | Pyromancy, Aquamancy, Aeromancy, Geomancy, Vitalism, Transformation, Illusion, Summoning | Gate node for deeper colony-scale magic and the advanced arcane research bench. |
| `MFV_Soulcraft` | Soulcraft | 2400 | Arcane Secrets, Necromancy, Vitalism | Soul gems, resurrection-adjacent effects, sustained buffs with backlash. Requires advanced arcane research bench. |
| `MFV_PlanarMagic` | Planar Magic | 2600 | Arcane Secrets, Leyline Sensitivity | Planar gates, temporary pocket maps, planar materials, and future planar hazards/sites. Requires advanced arcane research bench. |
| `MFV_GrandSorcery` | Grand Sorcery | 3200 | Arcane Secrets, Planar Magic | Currently valuable mainly for the Archmage discipline, which can learn nearly every spell outside the most focused disciplines. Future target: endgame rituals, colony-scale effects, and planar/artifact rewards. Requires advanced arcane research bench. |

Review note: `MFV_ArcaneSecrets` has replaced the old `MFV_AdvancedSchools` gate. Live XML should not reference `MFV_AdvancedSchools`.

Tier 4 - Forbidden Lore:

Forbidden research should be optional and expensive. It should not block the main arcane tree, but it can provide powerful shortcuts or unique effects with social, health, storyteller, or faction consequences.

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_ForbiddenLore` | Forbidden Lore | 2000 | Arcane Theory; hidden prerequisite: Necromancy | Records magical principles most traditions refuse to teach openly. Requires advanced arcane research bench. |
| `MFV_Fleshcraft` | Fleshcraft | 2600 | Forbidden Lore, Vitalism, Necromancy | Future golem and flesh-construct work, building from shared undead/construct lifecycle support. Requires advanced arcane research bench. |
| `MFV_InfernalPact` | Infernal Pact | 2800 | Forbidden Lore, Soulcraft | Undeveloped future path; core concept still open. Requires advanced arcane research bench. |
| `MFV_Chronomancy` | Chronomancy | 3600 | Forbidden Lore, Grand Sorcery | Temporal manipulation, including Temporal Resurrection with proportional skill-memory loss and Borrowed Season crop-growth acceleration. Requires advanced arcane research bench. |

Current layout coordinates:

| Def | X | Y |
|-----|---|---|
| `MFV_ArcaneTheory` | 0 | 2.5 |
| `MFV_LeylineSensitivity` | 1.5 | 0.5 |
| `MFV_Lapidary` | 1.5 | 1.5 |
| `MFV_Alchemy` | 1.5 | 2.5 |
| `MFV_Papyrus` | 1.5 | 4.5 |
| `MFV_Spellcraft` | 2.5 | 2.5 |
| `MFV_ForbiddenLore` | 1.5 | 6 |
| `MFV_Elementalism` | 4 | 1.25 |
| `MFV_Enchantment` | 4 | 1.75 |
| `MFV_Summoning` | 4 | 2.25 |
| `MFV_Transformation` | 4 | 2.75 |
| `MFV_Illusion` | 4 | 3.25 |
| `MFV_Vitalism` | 4 | 3.75 |
| `MFV_Parchmentry` | 4 | 4.5 |
| `MFV_Pyromancy` | 6 | 1.5 |
| `MFV_Aquamancy` | 6 | 2 |
| `MFV_Aeromancy` | 6 | 2.5 |
| `MFV_Geomancy` | 6 | 3 |
| `MFV_ArcaneSecrets` | 6 | 3.5 |
| `MFV_Necromancy` | 6 | 4 |
| `MFV_Fleshcraft` | 8 | 4.5 |
| `MFV_Soulcraft` | 9 | 3.5 |
| `MFV_PlanarMagic` | 9 | 1.5 |
| `MFV_InfernalPact` | 11 | 4.5 |
| `MFV_GrandSorcery` | 11 | 2.5 |
| `MFV_Chronomancy` | 12 | 5.5 |

Implementation notes:
- Use `MFV_` prefixes for MFVanilla content to avoid colliding with framework validation defs.
- The tab and projects live in `Defs/ResearchProjectDefs/MFV_ArcaneResearch.xml`.
- Do not use `<researchIcon>`; RimWorld 1.6 `ResearchProjectDef` does not have that field.
- Do not make magic projects require `Electricity` while tech suppression is enabled, or the tree will depend on hidden research.
- The standard arcane research bench is available without research.
- The advanced arcane research bench is unlocked by `MFV_ArcaneSecrets`.
- `MFV_RunicInscription` and `MFV_Infrastructure` are not active research nodes in the current live tree.
- Consider a setting later for whether forbidden lore is visible from game start or revealed after prerequisite research.

### 2. Items & Equipment

| Category | Examples | Purpose |
|----------|----------|---------|
| Wands | Fire Wand, Ice Wand, Lightning Wand | Spell delivery devices |
| Staves | Archmage Staff, Battle Staff | Enhanced spell power |
| Consumables | Mana Potion, Scrolls, Runes | One-time use items |
| Production | Alchemy Table | Early magical crafting station unlocked by Alchemy. |
| Enchantment | Arcane Forge, Arcane Spire, elemental foci | Advanced magical item production and facility support. |
| Planar | Planar Gate, phase stone walls, void glass walls, planar plants | Advanced pocket-map exploration, extraction, special construction, and future planar systems. |
| Apparel | Mage Robes, Enchanted Cloaks | Stat bonuses |
| Artifacts | Ancient Relics, Legendary Items | Unique effects |

Implementation notes:
- Prefer cloning known-valid vanilla parents or author full `ThingDef`s from working vanilla examples.
- Avoid placeholder fields from older RimWorld versions; validate every field against RimWorld 1.6.
- Do not add texture paths until the matching files exist under `Textures`.
- The standard arcane research bench is available without research. The advanced arcane research bench is unlocked by `MFV_ArcaneSecrets`.
- `MFV_AlchemyTable` is a production work table unlocked by `MFV_Alchemy` and uses `Things/Building/Production/AlchemyTable`.
- Generated spell scrolls reuse `MFV_SpellScrollBase`, teach MagicFramework spells through `CompUseEffect_LearnSpell`, and are generated from current spell learning metadata.
- Spell acquisition now has research mystery-scroll drops, generated recipes, trader hooks, treasure rewards, XP from scroll learning, known-spell casting XP, and apprenticeship support. Future work should tune economy and pacing rather than reopen the basic acquisition path.
- Gemstone cutting creates common, fine, or exquisite cut gems plus dust byproduct. Lower-quality cuts produce more dust, giving failed precision some crafting value.
- Arcane treasure chests are now the shared mission reward container for cache-style sites.
- Arcane Forge recipes transform good-or-better mundane weapons into the first four enchanted weapons while preserving final quality.
- Planar gates are supported by nearby arcane spires and currently open into functional temporary planar pocket maps; the first release should polish this loop before broadening planar rewards or hazards.
- Phase stone blocks now build phase stone walls that behave as sealed walls for ordinary pawns while Arcane Gift pawns can pass through them.

### 3. Spell Extensions

| Category | Spells | Framework primitive |
|----------|--------|---------------------|
| Elemental | Firebolt, Fireball, Flame Field, Heat, Warmth, Extinguish, Deluge, Air Blast, Stoneskin, Earth Call | Damage, hediff/status, explosion, chain, terrain, temperature, cone, displacement |
| Vitalism | Heal, Regeneration, Triage Pulse, Blessing of Vigor | Heal, repeat, reusable status, target queries |
| Arcane/control | Blink Step, Rescue Recall, Transposition, Force Push/Pull, Force Field, Mana Shield, Repulsion Ward, Chain Lightning | Teleport, displacement, maintained effects, force fields, delayed chains |
| Summoning/Planar | Summon Dog, planar gate/pocket-map support | `SummonPawnActionDef`, planar gate and pocket-map infrastructure |

Implementation notes:
- Use MagicFramework's actual XML type names and existing validation spells as templates.
- Authored validation spells now live in MFVanilla as starter content. MagicFramework should keep only framework code and in-memory debug fallbacks.
- New school content should either improve MFVanilla play directly or validate a reusable MagicFramework primitive that the content actually needs.

### 4. Races & Creatures

| Category | Examples | Use case |
|----------|----------|----------|
| Familiars | Tiny Fire Spirit, Owl Companion | Companion/utility |
| Elementals | Fire Elemental, Ice Elemental | Combat summons |
| Golems | Stone Golem, Iron Golem | Combat summons |
| Beasts | Magic Wolf, Arcane Bear | Combat summons |
| Humanoids | Mage, Battle Mage | Faction/raids |

Implementation notes:
- Use existing vanilla race `ThingDef` names, not generic labels like `Wolf` or `Bird`.
- For early content, prefer existing vanilla pawn kinds or simple animal summons before custom races.
- Humanlike summoned pawn kinds require humanlike-specific fields such as initial will/resistance ranges.
- Current construct defenders use custom MFVanilla identities while leaning on reliable RimWorld combat behavior.
- Hostile Elementalist casters are intentionally rare and curated; broad hostile spell AI remains a framework/content follow-up, not a requirement for the current MFVanilla completion pass.

### 5. Arcane Mission Sites

Current implementation:
- Three player-facing mission incidents exist: `MFV_ArcaneCacheMission`, `MFV_RuinedSanctumMission`, and `MFV_SealedVaultMission`.
- Each mission creates a world `Site` through `SiteMaker`, sends a player letter, registers an expiry timer, and blocks duplicate active sites of the same type.
- `GenStep_ArcaneCache` is now a reusable profile-driven generator for arcane cache, ruined sanctum, and sealed vault layouts.
- Site profiles live in `Defs/Sites/Parts/MFV_ArcaneCacheSite.xml` and control room shape, wall materials, chest tier, defenders, dressing, side rooms, entry paths, exterior ruins, and broken walls.
- Current defenders use MFVanilla construct identities: clay automata, rune slashers, rune ballistae, crystal sentinels, flesh golems, and the Deep Iron Golem.

Current mission tuning:

| Mission | Earliest day | Refire | Distance | Timeout | Threat floor |
|---------|--------------|--------|----------|---------|--------------|
| Arcane Cache | 8 | 18 days | 4-18 tiles | 18 days | 250 |
| Ruined Sanctum | 22 | 28 days | 5-20 tiles | 22 days | 500 |
| Sealed Vault | 45 | 45 days | 8-24 tiles | 28 days | 1200 |

Design rules:
- Keep the first release to Arcane Cache, Ruined Sanctum, and Sealed Vault.
- Prefer deterministic, profile-authored variation over ad hoc generation branches.
- Keep active hazards, leyline sites, elemental shrines, cursed archives, and connected-room ruin generation in the MF-049/MF-049B follow-up bucket.
- Validate the real incident/mission path, not only direct debug-spawned sites.

See [ArcaneSites.md](ArcaneSites.md) for the current site profile reference and validation checklist.

### 6. Planar Magic

Current implementation:
- `MFV_PlanarGate` is a buildable advanced structure tied to Planar Magic and supported by nearby `MFV_ArcaneSpire` buildings.
- The gate exposes player-facing alignment status and can send selected player-controlled pawns within its activation radius into a planar pocket when the window is open.
- Planar pockets use a dedicated world object/site part, `PlanarPocketParent`, `MFV_PlanarPocketMap`, and `GenStep_PlanarPocket`.
- Pocket maps generate planar terrain, plants, mineables, stone chunks, a return gate, and a forced-return lifecycle.
- Return uses a selection dialog so the player chooses travelers and supplies to bring back within carrying-capacity limits.
- MFVanilla blocks ordinary off-map transport from planar pocket maps so the gate/return loop remains coherent.
- Smoke testing has shown the first pass is functional and fun; it is now a release-polish surface rather than speculative planning.

Design rules:
- Keep the first release focused on a reliable gate, pocket, extraction, return, and cleanup loop.
- Treat hazards, defenders, treasure beats, expedition chains, planar events, and Grand Sorcery hooks as follow-up opportunities.
- Use the existing planar dimension/site XML so future pockets can be authored without duplicating the travel lifecycle.
- Preserve clear failure messages for alignment, pawn eligibility, missing gates, carrying capacity, and cleanup edge cases.

Near-term opportunities:
- Add small pocket-map points of interest: rare resource clusters, unstable mana flows, strange ruins, or guarded cache fragments.
- Give planar materials clear production or trade roles without bypassing gemstone, scroll, and arcane treasure progression.
- Add optional hazards or time pressure once the safe traversal loop is settled.
- Let Grand Sorcery, legendary weapons, or late quests reference planar pockets as reward sources after the base loop is stable.

### 7. Research Suppression

Current implementation:
- MFVanilla has a mod setting to suppress vanilla technology research.
- The suppression roots are `Electricity`, `MicroelectronicsBasics`, and `MultiAnalyzer`.
- The patch hides those core projects and downstream core projects that depend on them.
- The mod settings menu can restore vanilla technology research.
- The same patch layer synchronizes the `MFV_ArcaneGift` trait with MagicFramework's learned-spell runtime and tracks research-bench study for awakening Arcane Gift.

Design rules:
- Suppression must not filter `DefDatabase<ThingDef>` globally.
- Suppression should not delete or unload vanilla defs.
- MFVanilla magic research should not require suppressed vanilla research.

## Implementation Priority

1. Phase 1: Research foundation - implemented
   - Add `MFV_Arcane` research tab.
   - Add the Tier 0 and Tier 1 research projects.
   - Verify no XML errors in RimWorld.

2. Phase 2: Spell-school content - mostly implemented for the current completion pass
   - Add spell-school research.
   - Expand the migrated validation spells into balanced content per school using existing MagicFramework primitives.
   - Add simple valid item unlocks only after matching textures or safe vanilla graphics are chosen.
   - Generated scroll defs and recipes now cover current spell learning metadata; remaining work is balance, visuals, and school-identity follow-up where the live tree still feels thin.

3. Phase 3: Advanced and forbidden content - partially implemented
   - Add advanced research gates.
   - Planar Magic has a functional gate and pocket-map foundation.
   - Soulcraft has resurrection-adjacent spell content; Chronomancy has Temporal Resurrection and Borrowed Season as its first verified spell pair; Fleshcraft, Infernal Pact, and deeper forbidden consequences remain later work.
   - Balance costs, unlock pacing, and storyteller impact.

4. Phase 4: World-layer mission release - implemented, still in normal-play validation
   - Smoke test Arcane Cache, Ruined Sanctum, and Sealed Vault from natural incidents and debug offer actions.
   - Tune threat, reward, expiry, frequency, and mission text.
   - Validate automata and Deep Iron Golem site combat across save/load.
   - Keep broader connected-room ruins, active hazards, and new site families deferred until the current mission set is stable.

5. Phase 5: MFVanilla completion polish - active focus
   - Validate planar gates/pockets through save/load and ordinary play.
   - Finish enchanted weapon inspect text, art details, and balance smoke tests.
   - Update splash notes, mod metadata, release notes, and player-facing docs.
   - Audit research nodes whose descriptions promise future content more strongly than the current implementation supports.

6. Phase 6: Future expansion
   - Add textures and icons.
   - Add localization.
   - Add authoring notes for future MFVanilla content packs.
   - Expand planar pockets, Grand Sorcery rewards, forbidden lore consequences, deeper Illusion, Fleshcraft, deeper Chronomancy, and richer site families only after the completion pass is clean.

## Integration with MagicFramework

MFVanilla uses MagicFramework's spell system primitives:
- `SpellDef` for spell definitions
- `SpellActionDef` for spell effects
- `SpellCostDef` for resource costs
- `SpellRequirementDef` for usage conditions

See MFVanilla's `Defs/SpellDefs/` for working examples before authoring new spell XML.
