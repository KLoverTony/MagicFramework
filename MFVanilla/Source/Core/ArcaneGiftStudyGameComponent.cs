using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class ArcaneGiftStudyGameComponent : GameComponent
{
    private Dictionary<int, float> exposureByPawnId = new();
    private Dictionary<int, int> studyTicksByPawnId = new();
    private Dictionary<int, int> ticksSinceLastRollByPawnId = new();

    public ArcaneGiftStudyGameComponent(Game game)
    {
    }

    public static ArcaneGiftStudyGameComponent Instance => Current.Game?.GetComponent<ArcaneGiftStudyGameComponent>();

    public void NotifyResearchPerformed(Pawn pawn, Thing bench)
    {
        if (pawn == null || !ArcaneGiftUtility.IsArcaneResearchBench(bench))
        {
            return;
        }

        NotifyArcanePracticeExposure(pawn, bench?.def?.defName == ArcaneGiftUtility.AdvancedBenchDefName ? 0.16f : 0.12f);
    }

    public void NotifyArcanePracticeExposure(Pawn pawn, float amount)
    {
        if (pawn == null || amount <= 0f || ArcaneGiftUtility.HasArcaneGiftTrait(pawn))
        {
            return;
        }

        int pawnId = pawn.thingIDNumber;
        exposureByPawnId.TryGetValue(pawnId, out float exposure);
        exposure += amount;

        while (exposure >= ArcaneGiftUtility.ArcanePracticeExposureThreshold)
        {
            exposure -= ArcaneGiftUtility.ArcanePracticeExposureThreshold;
            if (Rand.Chance(ArcaneGiftUtility.ArcanePracticeGiftChance))
            {
                ArcaneGiftUtility.TryGiveArcaneGiftTrait(pawn, true);
                exposureByPawnId.Remove(pawnId);
                return;
            }
        }

        exposureByPawnId[pawnId] = exposure;
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref exposureByPawnId, "exposureByPawnId", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref studyTicksByPawnId, "studyTicksByPawnId", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref ticksSinceLastRollByPawnId, "ticksSinceLastRollByPawnId", LookMode.Value, LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            exposureByPawnId ??= new Dictionary<int, float>();
            studyTicksByPawnId ??= new Dictionary<int, int>();
            ticksSinceLastRollByPawnId ??= new Dictionary<int, int>();

            foreach (KeyValuePair<int, int> legacyStudy in studyTicksByPawnId)
            {
                if (!exposureByPawnId.ContainsKey(legacyStudy.Key))
                {
                    exposureByPawnId[legacyStudy.Key] = Mathf.Min(legacyStudy.Value, ArcaneGiftUtility.ArcanePracticeExposureThreshold - 1f);
                }
            }
        }
    }
}
