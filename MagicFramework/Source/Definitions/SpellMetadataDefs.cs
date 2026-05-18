using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MagicFramework.Definitions;

/// <summary>
/// Moddable elemental classification for spells. This is separate from legacy FX element strings.
/// </summary>
public class SpellElementDef : Def
{
    public string labelShort;
    public List<GameConditionDef> empoweredByConditions;
    public List<GameConditionDef> weakenedByConditions;
    public float defaultEmpowermentFactor = 1.15f;
    public float defaultWeakeningFactor = 0.85f;
}

/// <summary>
/// Broad school or tradition of magic, optionally tied to a default research concept later.
/// </summary>
public class SpellDomainDef : Def
{
    public SpellElementDef primaryElement;
    public ResearchProjectDef defaultResearchPrerequisite;
    public int displayOrder;
}

/// <summary>
/// Functional grouping for how a spell is used.
/// </summary>
public class SpellDisciplineDef : Def
{
    public int displayOrder;
}

/// <summary>
/// A caster specialization unlocked by a research node.
/// </summary>
public class ArcaneDisciplineDef : Def
{
    public int displayOrder;
}

/// <summary>
/// Marks a research project as unlocking an arcane discipline for gifted pawns.
/// </summary>
public class ArcaneDisciplineUnlockExtension : DefModExtension
{
    public ArcaneDisciplineDef discipline;
}

/// <summary>
/// Flexible free-form spell marker for filtering and rule matching.
/// </summary>
public class SpellTagDef : Def
{
}
