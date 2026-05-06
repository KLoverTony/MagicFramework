using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class ArcaneGiftStudyGameComponent : GameComponent
{
    private Dictionary<int, int> studyTicksByPawnId = new();
    private Dictionary<int, int> ticksSinceLastRollByPawnId = new();

    public ArcaneGiftStudyGameComponent(Game game)
    {
    }

    public static ArcaneGiftStudyGameComponent Instance => Current.Game?.GetComponent<ArcaneGiftStudyGameComponent>();

    public void NotifyResearchPerformed(Pawn pawn, Thing bench)
    {
        if (pawn == null
            || !ArcaneGiftUtility.IsArcaneResearchBench(bench)
            || ArcaneGiftUtility.HasArcaneGiftTrait(pawn))
        {
            return;
        }

        int pawnId = pawn.thingIDNumber;
        studyTicksByPawnId.TryGetValue(pawnId, out int studyTicks);
        studyTicks++;
        studyTicksByPawnId[pawnId] = studyTicks;

        if (studyTicks < ArcaneGiftUtility.StudyThresholdTicks)
        {
            return;
        }

        ticksSinceLastRollByPawnId.TryGetValue(pawnId, out int ticksSinceLastRoll);
        ticksSinceLastRoll++;

        if (ticksSinceLastRoll < ArcaneGiftUtility.RollIntervalTicks)
        {
            ticksSinceLastRollByPawnId[pawnId] = ticksSinceLastRoll;
            return;
        }

        ticksSinceLastRollByPawnId[pawnId] = 0;
        if (Rand.Chance(ArcaneGiftUtility.GiftChanceForBench(bench)))
        {
            ArcaneGiftUtility.TryGiveArcaneGiftTrait(pawn, true);
            studyTicksByPawnId.Remove(pawnId);
            ticksSinceLastRollByPawnId.Remove(pawnId);
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref studyTicksByPawnId, "studyTicksByPawnId", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref ticksSinceLastRollByPawnId, "ticksSinceLastRollByPawnId", LookMode.Value, LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            studyTicksByPawnId ??= new Dictionary<int, int>();
            ticksSinceLastRollByPawnId ??= new Dictionary<int, int>();
        }
    }
}
