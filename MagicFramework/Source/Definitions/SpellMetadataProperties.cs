using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MagicFramework.Definitions;

/// <summary>
/// Additive classification metadata for spell discovery, filtering, and future modifiers.
/// </summary>
public sealed class SpellMetaProperties
{
    public int tier = 1;
    public List<SpellElementDef> elements;
    public List<SpellDomainDef> domains;
    public List<SpellDisciplineDef> disciplines;
    public List<SpellTagDef> tags;
}

/// <summary>
/// Authored rules that decide whether a pawn can learn a spell.
/// </summary>
public sealed class SpellLearningProperties
{
    public bool canBeLearned = true;
    public bool hiddenUntilResearchUnlocked;
    public bool hiddenUntilRequirementsMet;
    public List<ResearchProjectDef> researchPrerequisites;
    public List<SpellRequirementDef> requirements;
}

/// <summary>
/// Grouped casting checks and costs. Legacy top-level fields remain supported.
/// </summary>
public sealed class SpellCastingProperties
{
    public List<SpellRequirementDef> requirements;
    public List<SpellCostDef> costs;
}
