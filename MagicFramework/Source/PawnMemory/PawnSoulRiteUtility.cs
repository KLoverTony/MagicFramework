using Verse;

namespace MagicFramework.PawnMemory;

public static class PawnSoulRiteUtility
{
    public static PawnMemoryRecord NotifySoulReleased(Pawn deceased, Corpse corpse = null, Pawn conductor = null)
    {
        PawnMemoryRecord record = PrepareRiteRecord(deceased, corpse);
        if (record == null)
            return null;

        record.properRitesPerformed = true;
        record.rituallyReleased = true;
        record.resurrectionAllowed = false;
        record.spiritActive = false;
        record.activeSpiritThingId = null;
        record.corrupted = false;
        record.state = PawnMemoryState.Released;
        ClearHauntingSchedule(record, "Soul released.");
        record.lastUpdatedTick = Find.TickManager.TicksGame;

        MarkCorpseConsumed(record, corpse);
        return record;
    }

    public static PawnMemoryRecord NotifyCorpseConsumedWithoutBindingSoul(Pawn deceased, Corpse corpse = null, Pawn conductor = null)
    {
        PawnMemoryRecord record = NotifySoulReleased(deceased, corpse, conductor);
        if (record == null)
            return null;

        record.bodyDestroyed = true;
        record.corpseAnchorKnown = false;
        return record;
    }

    public static PawnMemoryRecord NotifySoulBound(Pawn deceased, Corpse corpse = null, Pawn conductor = null, string boundThingId = null)
    {
        PawnMemoryRecord record = PrepareRiteRecord(deceased, corpse);
        if (record == null)
            return null;

        record.properRitesPerformed = true;
        record.rituallyReleased = false;
        record.resurrectionAllowed = false;
        record.spiritActive = false;
        record.activeSpiritThingId = boundThingId;
        record.state = PawnMemoryState.Bound;
        record.lastUpdatedTick = Find.TickManager.TicksGame;

        MarkCorpseConsumed(record, corpse);
        return record;
    }

    public static PawnMemoryRecord NotifySpiritManifested(Pawn deceased, string spiritThingId, bool permanent = false)
    {
        PawnMemoryRecord record = PrepareRiteRecord(deceased);
        if (record == null || record.state == PawnMemoryState.Released || record.rituallyReleased)
            return record;

        return NotifySpiritManifested(record, spiritThingId, permanent);
    }

    public static PawnMemoryRecord NotifySpiritManifested(PawnMemoryRecord record, string spiritThingId, bool permanent = false)
    {
        if (record == null || record.state == PawnMemoryState.Released || record.rituallyReleased)
            return record;

        record.spiritActive = true;
        record.activeSpiritThingId = spiritThingId;
        record.resurrectionAllowed = !permanent;
        record.state = permanent ? PawnMemoryState.Bound : PawnMemoryState.SpiritActive;
        record.hauntingEligible = false;
        record.hauntingScheduled = false;
        record.hauntingSuppressed = false;
        record.hauntingSuppressionReason = null;
        record.hauntingEarliestTick = null;
        record.lastUpdatedTick = Find.TickManager.TicksGame;
        return record;
    }

    public static PawnMemoryRecord NotifySpiritDeparted(Pawn deceased, SpiritDepartureReason reason)
    {
        PawnMemoryRecord record = PrepareRiteRecord(deceased);
        if (record == null)
            return null;

        record.spiritActive = false;
        record.activeSpiritThingId = null;
        record.lastUpdatedTick = Find.TickManager.TicksGame;

        if (reason == SpiritDepartureReason.Released)
        {
            record.rituallyReleased = true;
            record.resurrectionAllowed = false;
            record.state = PawnMemoryState.Released;
            ClearHauntingSchedule(record, "Spirit released.");
        }
        else if (record.state == PawnMemoryState.SpiritActive)
        {
            record.state = PawnMemoryState.DeadPendingRites;
        }

        return record;
    }

    public static PawnMemoryRecord NotifyCorpseConsumed(Pawn deceased, Corpse corpse = null, CorpseDispositionReason reason = CorpseDispositionReason.Other)
    {
        PawnMemoryRecord record = PrepareRiteRecord(deceased, corpse);
        if (record == null)
            return null;

        MarkCorpseConsumed(record, corpse);
        record.lastUpdatedTick = Find.TickManager.TicksGame;
        return record;
    }

    public static PawnMemoryRecord NotifyPawnResurrected(Pawn pawn)
    {
        if (pawn == null || !pawn.RaceProps.Humanlike)
            return null;

        WorldComponent_PawnMemories registry = WorldComponent_PawnMemories.Instance;
        PawnMemoryRecord record = registry?.GetOrCreateMemory(pawn);
        if (record == null)
            return null;

        record.spiritActive = false;
        record.activeSpiritThingId = null;
        record.state = PawnMemoryState.Active;
        record.resurrectionAllowed = true;
        record.bodyDestroyed = false;
        record.corpseAnchorKnown = false;
        record.corpseThingId = null;
        record.corpseMapId = null;
        record.corpseCell = null;
        ClearHauntingSchedule(record, "Pawn resurrected.");

        registry.UpdateMemory(pawn, PawnMemoryUpdateReason.Resurrection);
        record.state = PawnMemoryState.Active;
        record.lastUpdatedTick = Find.TickManager.TicksGame;
        return record;
    }

    public static void ClearHauntingSchedule(PawnMemoryRecord record, string reason = null)
    {
        if (record == null)
            return;

        record.hauntingEligible = false;
        record.hauntingScheduled = false;
        record.hauntingSuppressed = true;
        record.hauntingSuppressionReason = reason ?? "Cancelled.";
        record.hauntingEarliestTick = null;
    }

    private static PawnMemoryRecord PrepareRiteRecord(Pawn deceased, Corpse corpse = null)
    {
        deceased ??= corpse?.InnerPawn;
        if (deceased == null || !deceased.RaceProps.Humanlike)
            return null;

        WorldComponent_PawnMemories registry = WorldComponent_PawnMemories.Instance;
        PawnMemoryRecord record = registry?.GetOrCreateMemory(deceased);
        if (record == null)
            return null;

        registry.UpdateMemory(deceased, PawnMemoryUpdateReason.Ritual);
        if (record.deathTick == null && deceased.Dead)
            record.deathTick = Find.TickManager.TicksGame;

        if (corpse?.Map != null)
        {
            registry.RecordCorpseAnchor(corpse);
            record.deathMapId ??= corpse.Map.uniqueID;
            record.deathCell ??= corpse.PositionHeld;
        }

        return record;
    }

    private static void MarkCorpseConsumed(PawnMemoryRecord record, Corpse corpse)
    {
        if (record == null)
            return;

        if (corpse != null)
        {
            record.bodyDestroyed = true;
            record.corpseAnchorKnown = false;
            record.corpseThingId = null;
            record.corpseMapId = null;
            record.corpseCell = null;
        }
    }
}
