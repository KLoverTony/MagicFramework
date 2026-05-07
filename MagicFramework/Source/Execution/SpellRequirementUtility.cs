using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using RimWorld;
using Verse;

namespace MagicFramework.Execution;

public static class SpellRequirementUtility
{
    public static bool CanLearnSpell(Pawn pawn, SpellDef spell, out string reason)
    {
        reason = null;
        if (pawn == null)
        {
            reason = "No pawn selected.";
            return false;
        }

        if (spell == null)
        {
            reason = "No spell selected.";
            return false;
        }

        SpellLearningProperties learning = spell.learning ?? new SpellLearningProperties();
        if (!learning.canBeLearned)
        {
            reason = $"{spell.LabelCap} cannot be learned.";
            return false;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime == null)
        {
            reason = "Spell runtime component was unavailable.";
            return false;
        }

        if (runtime.KnowsSpell(pawn, spell))
        {
            reason = $"{pawn.LabelShortCap} already knows {spell.LabelCap}.";
            return false;
        }

        ResearchProjectDef missingResearch = FirstMissingResearch(spell);
        if (missingResearch != null)
        {
            reason = $"Requires completed research: {missingResearch.LabelCap}.";
            return false;
        }

        SpellContext context = CreatePawnContext(pawn, spell);
        return RequirementsPass(context, learning.requirements, true, out reason);
    }

    public static bool CanCastSpell(SpellContext context, SpellDef spell, out string reason, bool requireKnownSpell = false)
    {
        reason = null;
        if (context == null)
        {
            reason = "Spell context was unavailable.";
            return false;
        }

        spell ??= context.spellDef;
        if (spell == null)
        {
            reason = "No spell selected.";
            return false;
        }

        if (requireKnownSpell && context.caster is Pawn pawn)
        {
            SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
            if (runtime == null)
            {
                reason = "Spell runtime component was unavailable.";
                return false;
            }

            if (!runtime.KnowsSpell(pawn, spell))
            {
                reason = $"{pawn.LabelShortCap} does not know {spell.LabelCap}.";
                return false;
            }
        }

        if (!RequirementsPass(context, spell.requirements, false, out reason))
        {
            return false;
        }

        return RequirementsPass(context, spell.casting?.requirements, false, out reason);
    }

    public static bool ResearchPrerequisitesMet(SpellDef spell)
    {
        return FirstMissingResearch(spell) == null;
    }

    public static ResearchProjectDef FirstMissingResearch(SpellDef spell)
    {
        List<ResearchProjectDef> projects = spell?.learning?.researchPrerequisites;
        if (projects == null)
        {
            return null;
        }

        for (int i = 0; i < projects.Count; i++)
        {
            ResearchProjectDef project = projects[i];
            if (project != null && !project.IsFinished)
            {
                return project;
            }
        }

        return null;
    }

    public static SpellContext CreatePawnContext(Pawn pawn, SpellDef spell)
    {
        return new SpellContext
        {
            caster = pawn,
            map = pawn?.Map,
            spellDef = spell,
            power = SpellPowerUtility.ComputePower(spell, pawn),
            randomSeed = Find.TickManager?.TicksGame ?? 0
        };
    }

    private static bool RequirementsPass(
        SpellContext context,
        List<SpellRequirementDef> requirements,
        bool learningCheck,
        out string reason)
    {
        reason = null;
        if (requirements == null)
        {
            return true;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            SpellRequirementDef requirementDef = requirements[i];
            if (requirementDef == null)
            {
                continue;
            }

            bool passed = learningCheck
                ? requirementDef.CreateWorker().CanLearn(context, requirementDef, out reason)
                : requirementDef.CreateWorker().CanCast(context, requirementDef, out reason);
            if (!passed)
            {
                return false;
            }
        }

        reason = null;
        return true;
    }
}
