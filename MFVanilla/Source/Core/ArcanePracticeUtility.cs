using System.Collections.Generic;
using System.Linq;
using MagicFramework.Core;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public static class ArcanePracticeUtility
{
    private const float BasicResearchExperiencePerTick = 0.12f;
    private const float AdvancedResearchExperiencePerTick = 0.16f;
    private const float ProductionExperiencePerWork = 0.08f;
    private const float MinimumProductionExperience = 25f;
    private const float MaximumProductionExperience = 500f;

    private static readonly HashSet<string> ArcaneProductionBenchDefNames = new()
    {
        "MFV_AlchemyTable",
        "MFV_ScribingTable",
        "MFV_ArcaneForge"
    };

    public static void NotifyArcaneResearchPerformed(Pawn pawn, Thing bench)
    {
        if (pawn == null || !ArcaneGiftUtility.IsArcaneResearchBench(bench))
        {
            return;
        }

        ArcaneGiftStudyGameComponent.Instance?.NotifyResearchPerformed(pawn, bench);
        if (!HasArcaneGift(pawn))
        {
            return;
        }

        float xp = bench?.def?.defName == ArcaneGiftUtility.AdvancedBenchDefName
            ? AdvancedResearchExperiencePerTick
            : BasicResearchExperiencePerTick;
        SpellRuntimeGameComponent.Instance?.GainCasterExperience(pawn, xp);
    }

    public static void NotifyArcaneProductionCompleted(Pawn pawn, RecipeDef recipeDef)
    {
        if (pawn == null || recipeDef == null || !HasArcaneGift(pawn) || !IsArcanePracticeRecipe(recipeDef))
        {
            return;
        }

        float workAmount = recipeDef.WorkAmountTotal(null);
        float xp = Mathf.Clamp(workAmount * ProductionExperiencePerWork, MinimumProductionExperience, MaximumProductionExperience);
        SpellRuntimeGameComponent.Instance?.GainCasterExperience(pawn, xp);
    }

    private static bool HasArcaneGift(Pawn pawn)
    {
        return SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) == true;
    }

    private static bool IsArcanePracticeRecipe(RecipeDef recipeDef)
    {
        return recipeDef?.recipeUsers != null
            && recipeDef.recipeUsers.Any(user => user != null && ArcaneProductionBenchDefNames.Contains(user.defName));
    }
}
