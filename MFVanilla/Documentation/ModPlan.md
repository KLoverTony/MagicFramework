# MFVanilla - Magic Framework Vanilla Extensions

This mod adds vanilla content extensions for Magic Framework, including an arcane research tree, items, spells, creatures, and optional suppression of vanilla technology research.

## Folder Structure

```
MFVanilla/
|-- About/
|   |-- About.xml          # Mod metadata with dependencies on Harmony and MagicFramework
|   |-- Preview.png        # Mod preview image
|   `-- ModIcon.png        # Mod icon
|-- Defs/
|   |-- ResearchProjectDefs/   # Custom research projects
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

MFVanilla should use a standalone `Arcane` research tab. When vanilla technology suppression is enabled, this tree becomes the colony's main non-industrial progression path. When suppression is disabled, it still works as a parallel magic progression path.

Design goals:
- Keep the first tier practical and colony-focused, so magic feels like an alternative infrastructure path instead of only combat power.
- Let elemental schools branch early for player expression.
- Keep forbidden research visually and mechanically separate, with higher costs, darker consequences, or story hooks.
- Avoid requiring suppressed vanilla research such as `Electricity`; arcane infrastructure should stand on its own.
- Use stable RimWorld 1.6 `ResearchProjectDef` fields only: `defName`, `label`, `description`, `baseCost`, `techLevel`, `tab`, `prerequisites`, `hiddenPrerequisites`, `researchViewX`, and `researchViewY`.

Research tab:

| Def | Label | Notes |
|-----|-------|-------|
| `MFV_Arcane` | Arcane | Dedicated tab for all MFVanilla research projects. |

Tier 0 - Foundation:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_ArcaneTheory` | Arcane Theory | 500 | None | Basic mana awareness, starter arcane workbench later, and entry into all practical arcana. |

Tier 1 - Practical Arcana:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_LeylineSensitivity` | Leyline Sensitivity | 700 | Arcane Theory | Mana detection, leyline map events, passive mana recovery concepts. |
| `MFV_RunicInscription` | Runic Inscription | 800 | Arcane Theory | Rune traps, ward markers, scroll authoring, inscription bench. |
| `MFV_Alchemy` | Alchemy | 800 | Arcane Theory | Potions, reagents, transmutation recipes, herbal magical medicine. |
| `MFV_Enchantment` | Enchantment | 1000 | Arcane Theory, Runic Inscription | Wands, staves, apparel enchantments, sustained item effects. |
| `MFV_ApprenticeSchools` | Apprentice Schools | 900 | Arcane Theory | Entry point for the first spell schools. |

Tier 2 - Apprentice Schools:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_Pyromancy` | Pyromancy | 1000 | Apprentice Schools | Firebolt, fireball variants, wall of fire, heat hazards. |
| `MFV_Cryomancy` | Cryomancy | 1000 | Apprentice Schools | Frost bolt, slows, cold zones, preservation magic. |
| `MFV_Aeromancy` | Aeromancy | 1000 | Apprentice Schools | Push/pull, blink-step support, lightning-adjacent mobility. |
| `MFV_Geomancy` | Geomancy | 1000 | Apprentice Schools | Stone barriers, terrain shaping, golems, defensive fields. |
| `MFV_Vitalism` | Vitalism | 1100 | Apprentice Schools, Alchemy | Healing, regeneration, disease resistance, growth magic. |
| `MFV_Umbramancy` | Umbramancy | 1200 | Apprentice Schools, Leyline Sensitivity | Concealment, fear, silence, darkness, psychic disruption. |
| `MFV_Necromancy` | Necromancy | 1400 | Apprentice Schools, Vitalism | Corpse use, spirit binding, undead summons; morally grey but not yet forbidden. |

Tier 3 - Advanced Schools:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_AdvancedSchools` | Advanced Schools | 1800 | Any two Apprentice Schools | Gate node for midgame arcane specialization. |
| `MFV_Infrastructure` | Infrastructure | 1800 | Advanced Schools, Leyline Sensitivity, Runic Inscription | Arcane power replacements, wards, teleport anchors, mana wells. |
| `MFV_ArtifactForging` | Artifact Forging | 2200 | Advanced Schools, Enchantment | Named artifacts, high-tier staves, bound equipment, rare components. |
| `MFV_Soulcraft` | Soulcraft | 2400 | Advanced Schools, Necromancy, Vitalism | Soul gems, resurrection-adjacent effects, sustained buffs with backlash. |
| `MFV_PlanarMagic` | Planar Magic | 2600 | Advanced Schools, Leyline Sensitivity, Geomancy | Summons, portals, banishment, planar hazards. |
| `MFV_GrandSorcery` | Grand Sorcery | 3200 | Infrastructure, Artifact Forging, Planar Magic | Endgame spells, large rituals, colony-scale effects. |

Implementation note: RimWorld research does not support "any two prerequisites" directly. Implement `MFV_AdvancedSchools` either as a normal prerequisite gate after a chosen minimum path, or add a small custom requirement later if flexible prerequisite counts become important.

Tier 4 - Forbidden Branches:

Forbidden research should be optional and expensive. It should not block the main arcane tree, but it can provide powerful shortcuts or unique effects with social, health, storyteller, or faction consequences.

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_ForbiddenBranches` | Forbidden Branches | 2000 | Arcane Theory, Umbramancy or Necromancy | Reveals the forbidden branch cluster. |
| `MFV_Fleshcraft` | Fleshcraft | 2600 | Forbidden Branches, Vitalism, Necromancy | Body mutation, grafts, flesh constructs, risky healing. |
| `MFV_InfernalPact` | Infernal Pact | 2800 | Forbidden Branches, Pyromancy, Soulcraft | Pact boons, sacrifice mechanics, infernal summons. |
| `MFV_VoidStudies` | Void Studies | 3000 | Forbidden Branches, Umbramancy, Planar Magic | Void damage, madness, null zones, anti-magic effects. |
| `MFV_Chronomancy` | Chronomancy | 3600 | Forbidden Branches, Grand Sorcery | Time dilation, cooldown manipulation, stasis, aging effects. |

Implementation note: "Umbramancy or Necromancy" also needs either a normal single prerequisite choice or a custom flexible requirement.

Suggested layout coordinates:

| Def | X | Y |
|-----|---|---|
| `MFV_ArcaneTheory` | 0 | 2.5 |
| `MFV_LeylineSensitivity` | 2 | 0.5 |
| `MFV_RunicInscription` | 2 | 1.5 |
| `MFV_Alchemy` | 2 | 2.5 |
| `MFV_Enchantment` | 2 | 3.5 |
| `MFV_ApprenticeSchools` | 2 | 4.5 |
| `MFV_Pyromancy` | 4 | 0 |
| `MFV_Cryomancy` | 4 | 0.75 |
| `MFV_Aeromancy` | 4 | 1.5 |
| `MFV_Geomancy` | 4 | 2.25 |
| `MFV_Vitalism` | 4 | 3 |
| `MFV_Umbramancy` | 4 | 3.75 |
| `MFV_Necromancy` | 4 | 4.5 |
| `MFV_AdvancedSchools` | 6 | 2.25 |
| `MFV_Infrastructure` | 8 | 0.75 |
| `MFV_ArtifactForging` | 8 | 1.5 |
| `MFV_Soulcraft` | 8 | 2.25 |
| `MFV_PlanarMagic` | 8 | 3 |
| `MFV_GrandSorcery` | 10 | 2.25 |
| `MFV_ForbiddenBranches` | 6 | 4.95 |
| `MFV_Fleshcraft` | 8 | 4.5 |
| `MFV_InfernalPact` | 10 | 3.75 |
| `MFV_VoidStudies` | 10 | 4.95 |
| `MFV_Chronomancy` | 12 | 4.35 |

Implementation notes:
- Use `MFV_` prefixes for MFVanilla content to avoid colliding with framework validation defs.
- Put the tab and projects in `Defs/ResearchProjectDefs/MFV_ArcaneResearch.xml` when implementation starts.
- Do not use `<researchIcon>`; RimWorld 1.6 `ResearchProjectDef` does not have that field.
- Do not make magic projects require `Electricity` while tech suppression is enabled, or the tree will depend on hidden research.
- Advanced and forbidden projects should require `MFV_AdvancedArcaneResearchBench`; `MFV_AdvancedSchools` unlocks that bench and must not require it, to avoid a circular dependency.
- Consider a setting later for whether forbidden branches are visible from game start or revealed after `MFV_ForbiddenBranches`.

### 2. Items & Equipment

| Category | Examples | Purpose |
|----------|----------|---------|
| Wands | Fire Wand, Ice Wand, Lightning Wand | Spell delivery devices |
| Staves | Archmage Staff, Battle Staff | Enhanced spell power |
| Consumables | Mana Potion, Scrolls, Runes | One-time use items |
| Apparel | Mage Robes, Enchanted Cloaks | Stat bonuses |
| Artifacts | Ancient Relics, Legendary Items | Unique effects |

Implementation notes:
- Prefer cloning known-valid vanilla parents or author full `ThingDef`s from working vanilla examples.
- Avoid placeholder fields from older RimWorld versions; validate every field against RimWorld 1.6.
- Do not add texture paths until the matching files exist under `Textures`.
- The standard arcane research bench is available without research. The advanced arcane research bench is unlocked by `MFV_AdvancedSchools`.

### 3. Spell Extensions

| Category | Spells | Framework primitive |
|----------|--------|---------------------|
| Elemental | Frost Nova, Thunder Strike, Acid Splash | Damage, hediff, explosion, chain, displacement |
| Nature/Vitalism | Entangle, Heal, Regrow | Heal, repeat, hediff, area effects |
| Arcane | Mana Burn, Arcane Bolt, Dispel | Direct damage, clear effects, conditionals |
| Summoning/Planar | Summon Wolf, Summon Golem, Summon Elemental | `SummonPawnActionDef` |

Implementation notes:
- Use MagicFramework's actual XML type names and existing validation spells as templates.
- Keep validation spells in MagicFramework; MFVanilla should become content, not framework test coverage.

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

### 5. Research Suppression

Current implementation:
- MFVanilla has a mod setting to suppress vanilla technology research.
- The suppression roots are `Electricity`, `MicroelectronicsBasics`, and `MultiAnalyzer`.
- The patch hides those core projects and downstream core projects that depend on them.
- The mod settings menu can restore vanilla technology research.

Design rules:
- Suppression must not filter `DefDatabase<ThingDef>` globally.
- Suppression should not delete or unload vanilla defs.
- MFVanilla magic research should not require suppressed vanilla research.

## Implementation Priority

1. Phase 1: Research foundation
   - Add `MFV_Arcane` research tab.
   - Add the Tier 0 and Tier 1 research projects.
   - Verify no XML errors in RimWorld.

2. Phase 2: Apprentice content
   - Add apprentice school research.
   - Add one validation content spell per school using existing MagicFramework primitives.
   - Add simple valid item unlocks only after matching textures or safe vanilla graphics are chosen.

3. Phase 3: Advanced and forbidden content
   - Add advanced research gates.
   - Prototype forbidden branch consequences.
   - Balance costs, unlock pacing, and storyteller impact.

4. Phase 4: Polish
   - Add textures and icons.
   - Add localization.
   - Add authoring notes for future MFVanilla content packs.

## Integration with MagicFramework

MFVanilla uses MagicFramework's spell system primitives:
- `SpellDef` for spell definitions
- `SpellActionDef` for spell effects
- `SpellCostDef` for resource costs
- `SpellRequirementDef` for usage conditions

See MagicFramework's `Defs/SpellDefs/` for working examples before authoring new MFVanilla spell XML.
