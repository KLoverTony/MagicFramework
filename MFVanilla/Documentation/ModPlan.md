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

MFVanilla uses a standalone `Arcane` research tab. When vanilla technology suppression is enabled, this tree becomes the colony's main non-industrial progression path. When suppression is disabled, it still works as a parallel magic progression path.

Design goals:
- Keep the first tier practical and colony-focused, so magic feels like an alternative infrastructure path instead of only combat power.
- Use `MFV_Spellcraft` as the main spell-school entry point.
- Split elemental practice through `MFV_Elementalism`, then branch into individual elemental disciplines.
- Keep forbidden research visually and mechanically separate, with higher costs, darker consequences, or story hooks.
- Avoid requiring suppressed vanilla research such as `Electricity`; arcane infrastructure should stand on its own.
- Use stable RimWorld 1.6 `ResearchProjectDef` fields. Current advanced projects also use `requiredResearchBuilding` to require the advanced arcane research bench.

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
| `MFV_LeylineSensitivity` | Leyline Sensitivity | 700 | Arcane Theory | Mana detection, leyline map events, passive mana recovery concepts. |
| `MFV_Alchemy` | Alchemy | 800 | Arcane Theory | Potions, reagents, transmutation recipes, herbal magical medicine. |
| `MFV_RunicInscription` | Runic Inscription | 800 | Arcane Theory | Stable runes, warding marks, and the carved grammar needed for enchantment and infrastructure. |
| `MFV_Enchantment` | Enchantment | 1000 | Arcane Theory, Runic Inscription | Wands, staves, apparel enchantments, sustained item effects. |
| `MFV_Spellcraft` | Spellcraft | 900 | Arcane Theory | Entry point for teachable magical schools. |

Tier 2 - Spell Schools:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_Elementalism` | Elementalism | 1000 | Spellcraft | Shared foundation for fire, water, air, and earth magic. |
| `MFV_Pyromancy` | Pyromancy | 1000 | Elementalism | Firebolt, fireball variants, wall of fire, heat hazards. |
| `MFV_Aquamancy` | Aquamancy | 1000 | Elementalism | Water shaping, protection, control effects, and hostile water magic. |
| `MFV_Aeromancy` | Aeromancy | 1000 | Elementalism | Push/pull, blink-step support, wind pressure, lightning-adjacent mobility. |
| `MFV_Geomancy` | Geomancy | 1000 | Elementalism | Stone barriers, terrain shaping, golems, defensive fields. |
| `MFV_Vitalism` | Vitalism | 1100 | Spellcraft | Healing, regeneration, disease resistance, growth magic. |
| `MFV_Transformation` | Transformation | 1200 | Spellcraft | Magical alteration, transmutation, and body/object conversion concepts. |
| `MFV_Illusion` | Illusion | 1200 | Spellcraft | Deceptive images, sensory manipulation, concealment, and misdirection. |
| `MFV_Summoning` | Summoning | 1200 | Spellcraft | Calling and commanding otherworldly entities. |
| `MFV_Necromancy` | Necromancy | 1400 | Vitalism | Corpse use, spirit binding, undead summons; morally grey but not yet forbidden. |

Tier 3 - Arcane Secrets:

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_ArcaneSecrets` | Arcane Secrets | 1800 | Elementalism, Transformation, Vitalism, Illusion, Summoning | Gate node for deeper colony-scale magic. |
| `MFV_Infrastructure` | Infrastructure | 1800 | Advanced Schools, Leyline Sensitivity, Runic Inscription | Arcane power replacements, wards, teleport anchors, mana wells. Requires advanced arcane research bench. |
| `MFV_Soulcraft` | Soulcraft | 2400 | Advanced Schools, Necromancy, Vitalism | Soul gems, resurrection-adjacent effects, sustained buffs with backlash. Requires advanced arcane research bench. |
| `MFV_PlanarMagic` | Planar Magic | 2600 | Advanced Schools, Leyline Sensitivity | Summons, portals, banishment, planar hazards. Requires advanced arcane research bench. |
| `MFV_GrandSorcery` | Grand Sorcery | 3200 | Infrastructure, Planar Magic | Endgame spells, large rituals, colony-scale effects. Requires advanced arcane research bench. |

Review note: `MFV_ArcaneSecrets` has replaced the old `MFV_AdvancedSchools` gate. Live XML should not reference `MFV_AdvancedSchools`.

Tier 4 - Forbidden Lore:

Forbidden research should be optional and expensive. It should not block the main arcane tree, but it can provide powerful shortcuts or unique effects with social, health, storyteller, or faction consequences.

| Def | Label | Cost | Prerequisites | Unlock intent |
|-----|-------|------|---------------|---------------|
| `MFV_ForbiddenLore` | Forbidden Lore | 2000 | Arcane Theory; hidden prerequisite: Necromancy | Records magical principles most traditions refuse to teach openly. Requires advanced arcane research bench. |
| `MFV_Fleshcraft` | Fleshcraft | 2600 | Forbidden Lore, Vitalism, Necromancy | Body mutation, grafts, flesh constructs, risky healing. Requires advanced arcane research bench. |
| `MFV_InfernalPact` | Infernal Pact | 2800 | Forbidden Lore, Soulcraft | Pact boons, sacrifice mechanics, infernal summons. Requires advanced arcane research bench. |
| `MFV_Chronomancy` | Chronomancy | 3600 | Forbidden Lore, Grand Sorcery | Time dilation, cooldown manipulation, stasis, aging effects. Requires advanced arcane research bench. |

Current layout coordinates:

| Def | X | Y |
|-----|---|---|
| `MFV_ArcaneTheory` | 0 | 2.5 |
| `MFV_LeylineSensitivity` | 2 | 0.5 |
| `MFV_Alchemy` | 2 | 2.5 |
| `MFV_RunicInscription` | 2 | 3.5 |
| `MFV_Enchantment` | 2 | 4.5 |
| `MFV_Spellcraft` | 2 | 5.5 |
| `MFV_Elementalism` | 4 | 0 |
| `MFV_Pyromancy` | 6 | 0 |
| `MFV_Aquamancy` | 6 | 0.75 |
| `MFV_Aeromancy` | 6 | 1.5 |
| `MFV_Geomancy` | 6 | 2.25 |
| `MFV_Vitalism` | 4 | 3 |
| `MFV_Transformation` | 4 | 3.75 |
| `MFV_Illusion` | 4 | 4.5 |
| `MFV_Summoning` | 4 | 5.25 |
| `MFV_Necromancy` | 6 | 4.5 |
| `MFV_ArcaneSecrets` | 8 | 2.25 |
| `MFV_Infrastructure` | 8 | 0.75 |
| `MFV_Soulcraft` | 10 | 2.25 |
| `MFV_PlanarMagic` | 10 | 3 |
| `MFV_GrandSorcery` | 10 | 2.25 |
| `MFV_ForbiddenLore` | 4 | 7 |
| `MFV_Fleshcraft` | 8 | 4.5 |
| `MFV_InfernalPact` | 10 | 3.75 |
| `MFV_Chronomancy` | 12 | 4.35 |

Implementation notes:
- Use `MFV_` prefixes for MFVanilla content to avoid colliding with framework validation defs.
- The tab and projects live in `Defs/ResearchProjectDefs/MFV_ArcaneResearch.xml`.
- Do not use `<researchIcon>`; RimWorld 1.6 `ResearchProjectDef` does not have that field.
- Do not make magic projects require `Electricity` while tech suppression is enabled, or the tree will depend on hidden research.
- The standard arcane research bench is available without research.
- The advanced arcane research bench is unlocked by `MFV_ArcaneSecrets`.
- `MFV_RunicInscription` is the practical bridge into enchantment and infrastructure.
- Consider a setting later for whether forbidden lore is visible from game start or revealed after prerequisite research.

### 2. Items & Equipment

| Category | Examples | Purpose |
|----------|----------|---------|
| Wands | Fire Wand, Ice Wand, Lightning Wand | Spell delivery devices |
| Staves | Archmage Staff, Battle Staff | Enhanced spell power |
| Consumables | Mana Potion, Scrolls, Runes | One-time use items |
| Production | Alchemy Table | Early magical crafting station unlocked by Alchemy. |
| Apparel | Mage Robes, Enchanted Cloaks | Stat bonuses |
| Artifacts | Ancient Relics, Legendary Items | Unique effects |

Implementation notes:
- Prefer cloning known-valid vanilla parents or author full `ThingDef`s from working vanilla examples.
- Avoid placeholder fields from older RimWorld versions; validate every field against RimWorld 1.6.
- Do not add texture paths until the matching files exist under `Textures`.
- The standard arcane research bench is available without research. The advanced arcane research bench should be unlocked by the current advanced gate, likely `MFV_ArcaneSecrets`.
- `MFV_AlchemyTable` is a production work table unlocked by `MFV_Alchemy` and uses `Things/Building/Production/AlchemyTable`.

### 3. Spell Extensions

| Category | Spells | Framework primitive |
|----------|--------|---------------------|
| Elemental | Frost Nova, Thunder Strike, Acid Splash | Damage, hediff, explosion, chain, displacement |
| Nature/Vitalism | Entangle, Heal, Regrow | Heal, repeat, hediff, area effects |
| Arcane | Mana Burn, Arcane Bolt, Dispel | Direct damage, clear effects, conditionals |
| Summoning/Planar | Summon Wolf, Summon Golem, Summon Elemental | `SummonPawnActionDef` |

Implementation notes:
- Use MagicFramework's actual XML type names and existing validation spells as templates.
- Authored validation spells now live in MFVanilla as starter content. MagicFramework should keep only framework code and in-memory debug fallbacks.

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

2. Phase 2: Spell-school content
   - Add spell-school research.
   - Expand the migrated validation spells into balanced content per school using existing MagicFramework primitives.
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

See MFVanilla's `Defs/SpellDefs/` for working examples before authoring new spell XML.
