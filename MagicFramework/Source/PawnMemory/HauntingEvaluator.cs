using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework.Core;
using RimWorld;
using Verse;

namespace MagicFramework.PawnMemory;

public static class HauntingEvaluator
{
    public const int DefaultMaxHauntingsPerMap = 10;
    public const int DefaultMinimumRiskScore = 25;
    public const int DefaultMinDelayTicks = GenDate.TicksPerDay * 2;
    public const int DefaultMaxDelayTicks = GenDate.TicksPerDay * 4;

    public static void EvaluateAfterDeath(PawnMemoryRecord record, IEnumerable<PawnMemoryRecord> allRecords)
    {
        if (record == null || record.hauntingEvaluated)
            return;

        record.hauntingEvaluated = true;
        record.hauntingDecisionTick = Find.TickManager.TicksGame;
        record.hauntingMapId = record.deathMapId ?? record.lastKnownMapId;
        record.hauntingChance = CalculateHauntingChance(record.hauntingRiskScore);
        record.hauntingRoll = Rand.Value;

        if (record.state == PawnMemoryState.Released || record.rituallyReleased)
        {
            Suppress(record, "Soul has been released.");
            return;
        }

        int minimumRisk = Math.Max(0, MagicFrameworkSettings.Current?.hauntingMinimumRiskScore ?? DefaultMinimumRiskScore);
        if (record.hauntingRiskScore < minimumRisk)
        {
            Suppress(record, "Risk score below threshold.");
            return;
        }

        if (record.hauntingRoll > record.hauntingChance)
        {
            Suppress(record, "Haunting chance roll failed.");
            return;
        }

        int mapId = record.hauntingMapId ?? -1;
        if (mapId >= 0 && CountScheduledOrActiveHauntings(mapId, allRecords) >= MaxHauntingsForMap(mapId))
        {
            Suppress(record, "Map haunting limit reached.");
            return;
        }

        int minDelay = Math.Max(0, MagicFrameworkSettings.Current?.hauntingMinDelayTicks ?? DefaultMinDelayTicks);
        int maxDelay = Math.Max(minDelay, MagicFrameworkSettings.Current?.hauntingMaxDelayTicks ?? DefaultMaxDelayTicks);
        record.hauntingEarliestTick = Find.TickManager.TicksGame + Rand.RangeInclusive(minDelay, maxDelay);
        record.hauntingEligible = true;
        record.hauntingScheduled = true;
        record.hauntingSuppressed = false;
        record.hauntingSuppressionReason = null;
    }

    public static bool IsReadyToHaunt(PawnMemoryRecord record)
    {
        if (record == null || !record.hauntingScheduled || record.hauntingSuppressed)
            return false;

        if (record.state == PawnMemoryState.Released || record.rituallyReleased)
            return false;

        return record.hauntingEarliestTick.HasValue && Find.TickManager.TicksGame >= record.hauntingEarliestTick.Value;
    }

    public static int CountScheduledOrActiveHauntings(int mapId, IEnumerable<PawnMemoryRecord> allRecords)
    {
        if (allRecords == null)
            return 0;

        return allRecords.Count(record =>
            record != null &&
            record.hauntingMapId == mapId &&
            !record.hauntingSuppressed &&
            (record.hauntingScheduled || record.state == PawnMemoryState.SpiritActive));
    }

    public static int MaxHauntingsForMap(int mapId)
    {
        int configured = MagicFrameworkSettings.Current?.maxHauntingsPerMap ?? DefaultMaxHauntingsPerMap;
        return Math.Max(0, configured);
    }

    public static float CalculateHauntingChance(int riskScore)
    {
        if (riskScore < 25) return 0f;
        if (riskScore < 50) return 0.2f;
        if (riskScore < 75) return 0.5f;
        return 0.8f;
    }

    public static void ForceReevaluate(PawnMemoryRecord record, IEnumerable<PawnMemoryRecord> allRecords)
    {
        if (record == null)
            return;

        record.hauntingEvaluated = false;
        record.hauntingEligible = false;
        record.hauntingScheduled = false;
        record.hauntingSuppressed = false;
        record.hauntingSuppressionReason = null;
        record.hauntingDecisionTick = null;
        record.hauntingEarliestTick = null;
        record.hauntingMapId = null;
        EvaluateAfterDeath(record, allRecords);
    }

    private static void Suppress(PawnMemoryRecord record, string reason)
    {
        record.hauntingEligible = false;
        record.hauntingScheduled = false;
        record.hauntingSuppressed = true;
        record.hauntingSuppressionReason = reason;
    }
}
