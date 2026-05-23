using System.Collections.Generic;
using System.Linq;
using MagicFramework.Core;
using MagicFramework.Definitions;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MFVanilla.Core;

public static class ArcanePracticeUtility
{
    private const float BasicResearchExperiencePerTick = 0.04f;
    private const float AdvancedResearchExperiencePerTick = 0.06f;
    private const float ProductionExperiencePerWork = 0.08f;
    private const float MinimumProductionExperience = 25f;
    private const float MaximumProductionExperience = 500f;
    private const float LearnSpellBaseExperience = 100f;
    private const float LearnSpellExperiencePerTier = 75f;
    private const float ApprenticeExperiencePerTick = 0.018f;

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

        float xp = bench?.def?.defName == ArcaneGiftUtility.AdvancedBenchDefName
            ? AdvancedResearchExperiencePerTick
            : BasicResearchExperiencePerTick;

        if (!HasArcaneGift(pawn))
        {
            ArcaneGiftStudyGameComponent.Instance?.NotifyArcanePracticeExposure(pawn, xp);
            return;
        }

        SpellRuntimeGameComponent.Instance?.GainCasterExperience(pawn, xp);
    }

    public static void NotifyArcaneProductionCompleted(Pawn pawn, RecipeDef recipeDef)
    {
        if (pawn == null || recipeDef == null || !IsArcanePracticeRecipe(recipeDef))
        {
            return;
        }

        float workAmount = recipeDef.WorkAmountTotal(null);
        float xp = Mathf.Clamp(workAmount * ProductionExperiencePerWork, MinimumProductionExperience, MaximumProductionExperience);
        if (!HasArcaneGift(pawn))
        {
            ArcaneGiftStudyGameComponent.Instance?.NotifyArcanePracticeExposure(pawn, xp);
            return;
        }

        SpellRuntimeGameComponent.Instance?.GainCasterExperience(pawn, xp);
    }

    public static void NotifySpellLearnedFromScroll(Pawn pawn, SpellDef spellDef)
    {
        if (pawn == null || spellDef == null || !HasArcaneGift(pawn))
        {
            return;
        }

        float xp = LearnSpellBaseExperience + (LearnSpellExperiencePerTier * SpellTier(spellDef));
        SpellRuntimeGameComponent.Instance?.GainCasterExperience(pawn, xp);
    }

    public static void NotifyArcaneApprenticeshipObserved(Pawn apprentice, Pawn mentor, int tickInterval)
    {
        if (!CanApprenticeLearnFrom(apprentice, mentor) || !IsPawnDoingArcanePractice(mentor))
        {
            return;
        }

        float xp = ApprenticeExperiencePerTick * Mathf.Max(1, tickInterval);
        SpellRuntimeGameComponent.Instance?.GainCasterExperience(apprentice, xp);
    }

    public static bool CanApprenticeLearnFrom(Pawn apprentice, Pawn mentor)
    {
        return CanApprenticeLearnFrom(apprentice, mentor, out _);
    }

    public static bool CanApprenticeLearnFrom(Pawn apprentice, Pawn mentor, out string reason)
    {
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        reason = null;

        if (apprentice == null || mentor == null)
        {
            reason = "Missing apprentice or mentor.";
            return false;
        }

        if (apprentice == mentor)
        {
            reason = "A pawn cannot apprentice to themself.";
            return false;
        }

        if (!apprentice.Spawned || !mentor.Spawned || apprentice.Map != mentor.Map)
        {
            reason = "Apprentice and mentor must be spawned on the same map.";
            return false;
        }

        if (apprentice.Faction != Faction.OfPlayer || mentor.Faction != Faction.OfPlayer)
        {
            reason = "Both pawns must be player-controlled.";
            return false;
        }

        if (runtime?.HasArcaneGift(apprentice) != true)
        {
            reason = $"{apprentice.LabelShortCap} does not have the Arcane Gift.";
            return false;
        }

        if (!runtime.HasArcaneGift(mentor))
        {
            reason = $"{mentor.LabelShortCap} does not have the Arcane Gift.";
            return false;
        }

        if (runtime.GetCasterLevel(mentor) <= runtime.GetCasterLevel(apprentice))
        {
            reason = $"{mentor.LabelShortCap} must have a higher caster level than {apprentice.LabelShortCap}.";
            return false;
        }

        return true;
    }

    public static bool IsPawnDoingArcanePractice(Pawn pawn)
    {
        Job job = pawn?.CurJob;
        if (job == null)
        {
            return false;
        }

        if (job.def?.defName == "MFV_ArcaneApprenticeship")
        {
            return false;
        }

        Thing targetThing = job.GetTarget(TargetIndex.A).Thing;
        if (ArcaneGiftUtility.IsArcaneResearchBench(targetThing))
        {
            return true;
        }

        return IsArcanePracticeRecipe(job.bill?.recipe);
    }

    private static int SpellTier(SpellDef spellDef)
    {
        return Mathf.Max(1, spellDef?.meta?.tier ?? 1);
    }

    private static bool HasArcaneGift(Pawn pawn)
    {
        return SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) == true;
    }

    public static bool IsArcanePracticeRecipe(RecipeDef recipeDef)
    {
        return recipeDef?.recipeUsers != null
            && recipeDef.recipeUsers.Any(user => user != null && ArcaneProductionBenchDefNames.Contains(user.defName));
    }
}
