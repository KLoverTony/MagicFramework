using System.Collections.Generic;
using Verse;

namespace MagicFramework.Scheduling;

public sealed class ScheduledHediffRemoval : IExposable
{
    private int executeAtTick;
    private Pawn pawn;
    private HediffDef hediffDef;
    private string bodyPartDef;

    public ScheduledHediffRemoval()
    {
    }

    public ScheduledHediffRemoval(int executeAtTick, Pawn pawn, HediffDef hediffDef, string bodyPartDef)
    {
        this.executeAtTick = executeAtTick;
        this.pawn = pawn;
        this.hediffDef = hediffDef;
        this.bodyPartDef = bodyPartDef;
    }

    public int ExecuteAtTick => executeAtTick;

    public string DebugLabel => hediffDef?.defName ?? "<null hediff>";

    public bool TryExecute()
    {
        if (pawn == null || pawn.Destroyed || pawn.health?.hediffSet == null || hediffDef == null)
        {
            return false;
        }

        BodyPartRecord bodyPart = ResolveBodyPart(pawn, bodyPartDef);
        Hediff hediff = null;
        foreach (Hediff candidate in pawn.health.hediffSet.hediffs)
        {
            if (candidate.def == hediffDef && (bodyPart == null || candidate.Part == bodyPart))
            {
                hediff = candidate;
                break;
            }
        }

        if (hediff == null)
        {
            return false;
        }

        pawn.health.RemoveHediff(hediff);
        return true;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref executeAtTick, "executeAtTick");
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Defs.Look(ref hediffDef, "hediffDef");
        Scribe_Values.Look(ref bodyPartDef, "bodyPartDef");
    }

    private static BodyPartRecord ResolveBodyPart(Pawn pawn, string bodyPartDef)
    {
        if (pawn?.RaceProps?.body == null || string.IsNullOrWhiteSpace(bodyPartDef))
        {
            return null;
        }

        foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
        {
            if (part.def.defName == bodyPartDef)
            {
                return part;
            }
        }

        return null;
    }
}

public sealed class HediffRemovalMapComponent : MapComponent
{
    private List<ScheduledHediffRemoval> scheduledRemovals = new();

    public HediffRemovalMapComponent(Map map)
        : base(map)
    {
    }

    public bool Enqueue(ScheduledHediffRemoval scheduledRemoval)
    {
        if (scheduledRemoval == null)
        {
            return false;
        }

        scheduledRemovals ??= new List<ScheduledHediffRemoval>();
        int insertIndex = scheduledRemovals.Count;
        for (int i = 0; i < scheduledRemovals.Count; i++)
        {
            ScheduledHediffRemoval existingRemoval = scheduledRemovals[i];
            if (existingRemoval == null || existingRemoval.ExecuteAtTick > scheduledRemoval.ExecuteAtTick)
            {
                insertIndex = i;
                break;
            }
        }

        scheduledRemovals.Insert(insertIndex, scheduledRemoval);
        return true;
    }

    public override void MapComponentTick()
    {
        if (scheduledRemovals == null || scheduledRemovals.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        while (scheduledRemovals.Count > 0)
        {
            ScheduledHediffRemoval scheduledRemoval = scheduledRemovals[0];
            if (scheduledRemoval == null)
            {
                scheduledRemovals.RemoveAt(0);
                continue;
            }

            if (scheduledRemoval.ExecuteAtTick > currentTick)
            {
                return;
            }

            scheduledRemovals.RemoveAt(0);
            if (scheduledRemoval.TryExecute())
            {
                Log.Message($"[MagicFramework] Removed scheduled hediff {scheduledRemoval.DebugLabel}.");
            }
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref scheduledRemovals, "scheduledHediffRemovals", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && scheduledRemovals == null)
        {
            scheduledRemovals = new List<ScheduledHediffRemoval>();
        }
    }
}
