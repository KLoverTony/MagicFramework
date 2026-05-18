using System.Collections.Generic;
using MagicFramework.Core;
using MagicFramework.Definitions;
using RimWorld;
using Verse;

namespace MagicFramework.Execution;

public static class ArcaneDisciplineUtility
{
    public static bool TryGetDisciplineUnlockedBy(ResearchProjectDef research, out ArcaneDisciplineDef discipline)
    {
        discipline = research?.GetModExtension<ArcaneDisciplineUnlockExtension>()?.discipline;
        return discipline != null;
    }

    public static ResearchProjectDef GetUnlockResearch(ArcaneDisciplineDef discipline)
    {
        if (discipline == null)
        {
            return null;
        }

        foreach (ResearchProjectDef research in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
        {
            ArcaneDisciplineDef unlockedDiscipline = research.GetModExtension<ArcaneDisciplineUnlockExtension>()?.discipline;
            if (unlockedDiscipline == discipline)
            {
                return research;
            }
        }

        return null;
    }

    public static bool IsSpellAtOrBelowDiscipline(SpellDef spell, ArcaneDisciplineDef discipline)
    {
        ResearchProjectDef unlockResearch = GetUnlockResearch(discipline);
        if (spell?.learning?.researchPrerequisites == null || unlockResearch == null)
        {
            return false;
        }

        List<ResearchProjectDef> spellResearch = spell.learning.researchPrerequisites;
        for (int i = 0; i < spellResearch.Count; i++)
        {
            if (IsResearchAtOrBelow(spellResearch[i], unlockResearch))
            {
                return true;
            }
        }

        return false;
    }

    public static bool CanPawnDisciplineLearnSpell(Pawn pawn, SpellDef spell, out string reason)
    {
        reason = null;
        if (pawn == null)
        {
            reason = "No pawn selected.";
            return false;
        }

        ArcaneDisciplineDef discipline = SpellRuntimeGameComponent.Instance?.GetArcaneDiscipline(pawn);
        if (discipline == null)
        {
            reason = $"{pawn.LabelShortCap} has not embraced an Arcane Discipline.";
            return false;
        }

        if (IsSpellAtOrBelowDiscipline(spell, discipline))
        {
            return true;
        }

        reason = $"{spell?.LabelCap ?? "This spell"} is outside {discipline.LabelCap}.";
        return false;
    }

    public static bool CanPawnEmbraceDiscipline(Pawn pawn, ArcaneDisciplineDef candidate, out string reason)
    {
        reason = null;
        if (pawn == null)
        {
            reason = "No pawn selected.";
            return false;
        }

        if (candidate == null)
        {
            reason = "No Arcane Discipline selected.";
            return false;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime == null)
        {
            reason = "Spell runtime component was unavailable.";
            return false;
        }

        if (!runtime.HasArcaneGift(pawn))
        {
            reason = $"{pawn.LabelShortCap} does not have the Arcane gift.";
            return false;
        }

        ResearchProjectDef candidateResearch = GetUnlockResearch(candidate);
        if (candidateResearch == null)
        {
            reason = $"{candidate.LabelCap} is not linked to a research project.";
            return false;
        }

        if (!candidateResearch.IsFinished)
        {
            reason = $"Requires completed research: {candidateResearch.LabelCap}.";
            return false;
        }

        ArcaneDisciplineDef current = runtime.GetArcaneDiscipline(pawn);
        if (current == null)
        {
            return true;
        }

        if (current == candidate)
        {
            reason = $"{pawn.LabelShortCap} has already embraced {candidate.LabelCap}.";
            return false;
        }

        ResearchProjectDef currentResearch = GetUnlockResearch(current);
        if (currentResearch == null)
        {
            reason = $"{current.LabelCap} is not linked to a research project.";
            return false;
        }

        if (!IsResearchAtOrBelow(candidateResearch, currentResearch))
        {
            reason = $"{pawn.LabelShortCap} can only advance from {current.LabelCap}, not move sideways or down.";
            return false;
        }

        return true;
    }

    public static bool IsResearchAtOrBelow(ResearchProjectDef candidate, ResearchProjectDef ancestor)
    {
        if (candidate == null || ancestor == null)
        {
            return false;
        }

        HashSet<ResearchProjectDef> visited = new();
        return IsResearchAtOrBelowRecursive(candidate, ancestor, visited);
    }

    private static bool IsResearchAtOrBelowRecursive(
        ResearchProjectDef candidate,
        ResearchProjectDef ancestor,
        HashSet<ResearchProjectDef> visited)
    {
        if (candidate == null || !visited.Add(candidate))
        {
            return false;
        }

        if (candidate == ancestor)
        {
            return true;
        }

        if (ListContainsAncestor(candidate.prerequisites, ancestor, visited))
        {
            return true;
        }

        return ListContainsAncestor(candidate.hiddenPrerequisites, ancestor, visited);
    }

    private static bool ListContainsAncestor(
        List<ResearchProjectDef> prerequisites,
        ResearchProjectDef ancestor,
        HashSet<ResearchProjectDef> visited)
    {
        if (prerequisites == null)
        {
            return false;
        }

        for (int i = 0; i < prerequisites.Count; i++)
        {
            if (IsResearchAtOrBelowRecursive(prerequisites[i], ancestor, visited))
            {
                return true;
            }
        }

        return false;
    }
}
