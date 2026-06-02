using System.Text;
using MagicFramework.Scheduling;
using RimWorld;
using Verse;
using Verse.AI;

namespace MagicFramework.PawnLifecycle;

public class CompProperties_PawnLifecycleEnforcer : CompProperties
{
    public CompProperties_PawnLifecycleEnforcer()
    {
        compClass = typeof(CompPawnLifecycleEnforcer);
    }
}

public class CompPawnLifecycleEnforcer : ThingComp
{
    private const int EscortCheckIntervalTicks = 30;
    private const float DraftedFollowRadius = 6f;
    private const float DraftedFollowTargetRadius = 3f;
    private const float MasterDefenseRadius = 18f;

    private Pawn Pawn => parent as Pawn;
    private Pawn master;
    private string masterThingId;
    private bool followMasterWhileDrafted = true;
    private bool followMasterWhileFieldwork = true;
    private int nextEscortCheckTick;

    public Pawn Master => master;
    public bool FollowMasterWhileDrafted => followMasterWhileDrafted;
    public bool FollowMasterWhileFieldwork => followMasterWhileFieldwork;

    public void AssignMaster(Pawn newMaster, bool followDrafted = true, bool followFieldwork = true)
    {
        master = newMaster;
        masterThingId = newMaster?.ThingID;
        followMasterWhileDrafted = followDrafted;
        followMasterWhileFieldwork = followFieldwork;
        PawnLifecycleEnforcementUtility.EnforceControlPolicy(Pawn);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        PawnLifecycleEnforcementUtility.EnforceAll(Pawn);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref master, "lifecycleMaster");
        Scribe_Values.Look(ref masterThingId, "lifecycleMasterThingId");
        Scribe_Values.Look(ref followMasterWhileDrafted, "followMasterWhileDrafted", true);
        Scribe_Values.Look(ref followMasterWhileFieldwork, "followMasterWhileFieldwork", true);
        Scribe_Values.Look(ref nextEscortCheckTick, "nextEscortCheckTick");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            PawnLifecycleEnforcementUtility.EnforceAll(Pawn);
        }
    }

    public override void CompTick()
    {
        base.CompTick();
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        if (currentTick < nextEscortCheckTick)
        {
            return;
        }

        nextEscortCheckTick = currentTick + EscortCheckIntervalTicks;
        TickMasterEscort();
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
        PawnLifecycleEnforcementUtility.EnforceRecurring(Pawn);
    }

    private void TickMasterEscort()
    {
        if (TryDefendMaster())
        {
            return;
        }

        TryFollowDraftedMaster();
    }

    private bool TryDefendMaster()
    {
        Pawn pawn = Pawn;
        if (!CanRespondToDraftedMaster(pawn))
        {
            return false;
        }

        Pawn target = FindMasterDefenseTarget(pawn, master);
        if (target == null)
        {
            return false;
        }

        LocalTargetInfo currentTarget = pawn.CurJob?.GetTarget(TargetIndex.A) ?? LocalTargetInfo.Invalid;
        if (pawn.CurJobDef == JobDefOf.AttackMelee && currentTarget.Thing == target)
        {
            return true;
        }

        Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
        pawn.jobs?.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true);
        return true;
    }

    private void TryFollowDraftedMaster()
    {
        Pawn pawn = Pawn;
        if (!CanRespondToDraftedMaster(pawn))
        {
            return;
        }

        if (pawn.Position.DistanceToSquared(master.Position) <= DraftedFollowRadius * DraftedFollowRadius)
        {
            return;
        }

        if (pawn.CurJobDef == JobDefOf.AttackMelee ||
            pawn.CurJobDef == JobDefOf.Wait_Combat ||
            pawn.mindState?.enemyTarget != null)
        {
            return;
        }

        IntVec3 destination = ResolveFollowCell(pawn, master);
        if (!destination.IsValid || !pawn.CanReach(destination, PathEndMode.OnCell, Danger.Deadly))
        {
            return;
        }

        LocalTargetInfo currentTarget = pawn.CurJob?.GetTarget(TargetIndex.A) ?? LocalTargetInfo.Invalid;
        if (pawn.CurJobDef == JobDefOf.Goto && currentTarget.Cell.IsValid && currentTarget.Cell.DistanceToSquared(destination) <= 4f)
        {
            return;
        }

        Job job = JobMaker.MakeJob(JobDefOf.Goto, destination);
        job.expiryInterval = 240;
        job.checkOverrideOnExpire = true;
        InterruptCurrentNonCombatJob(pawn);
        pawn.jobs?.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true);
    }

    private static void InterruptCurrentNonCombatJob(Pawn pawn)
    {
        if (pawn == null ||
            pawn.CurJobDef == null ||
            pawn.CurJobDef == JobDefOf.AttackMelee ||
            pawn.CurJobDef == JobDefOf.Wait_Combat)
        {
            return;
        }

        if (pawn.carryTracker?.CarriedThing != null)
        {
            pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
        }

        pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
    }

    private bool CanRespondToDraftedMaster(Pawn pawn)
    {
        return pawn != null &&
               !pawn.Dead &&
               !pawn.Downed &&
               pawn.Spawned &&
               master != null &&
               !master.Destroyed &&
               master.Spawned &&
               master.Map == pawn.Map &&
               master.drafter?.Drafted == true &&
               followMasterWhileDrafted;
    }

    private static Pawn FindMasterDefenseTarget(Pawn pawn, Pawn master)
    {
        if (pawn?.Map?.mapPawns?.AllPawnsSpawned == null || master == null)
        {
            return null;
        }

        Pawn directTarget = master.mindState?.enemyTarget as Pawn;
        if (IsValidDefenseTarget(pawn, master, directTarget))
        {
            return directTarget;
        }

        Pawn bestTarget = null;
        float bestDistanceSquared = MasterDefenseRadius * MasterDefenseRadius;
        foreach (Pawn candidate in pawn.Map.mapPawns.AllPawnsSpawned)
        {
            if (!IsValidDefenseTarget(pawn, master, candidate))
            {
                continue;
            }

            float distanceSquared = candidate.Position.DistanceToSquared(master.Position);
            if (distanceSquared < bestDistanceSquared)
            {
                bestTarget = candidate;
                bestDistanceSquared = distanceSquared;
            }
        }

        return bestTarget;
    }

    private static bool IsValidDefenseTarget(Pawn pawn, Pawn master, Pawn candidate)
    {
        return candidate != null &&
               candidate != pawn &&
               candidate != master &&
               !candidate.Dead &&
               !candidate.Downed &&
               candidate.Spawned &&
               candidate.Map == pawn.Map &&
               candidate.Position.DistanceToSquared(master.Position) <= MasterDefenseRadius * MasterDefenseRadius &&
               master.HostileTo(candidate) &&
               pawn.HostileTo(candidate) &&
               pawn.CanReach(candidate, PathEndMode.Touch, Danger.Deadly);
    }

    private static IntVec3 ResolveFollowCell(Pawn pawn, Pawn master)
    {
        if (CellFinder.TryFindRandomCellNear(master.Position, master.Map, (int)DraftedFollowTargetRadius,
                cell => cell.Walkable(master.Map) &&
                        cell.Standable(master.Map) &&
                        cell.GetFirstPawn(master.Map) == null &&
                        pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly),
                out IntVec3 destination))
        {
            return destination;
        }

        return master.Position;
    }

    public override string CompInspectStringExtra()
    {
        string baseText = base.CompInspectStringExtra();
        Pawn pawn = Pawn;
        PawnLifecycleExtension extension = PawnLifecycleUtility.GetLifecycle(pawn);
        if (pawn == null || extension == null)
        {
            return baseText;
        }

        StringBuilder builder = new();
        if (!baseText.NullOrEmpty())
        {
            builder.AppendLine(baseText);
        }

        builder.Append("Lifecycle: ");
        builder.Append(FormatEnumName(extension.controlPolicy));
        if (extension.workPolicy != PawnLifecycleWorkPolicy.Unspecified)
        {
            builder.Append(", ");
            builder.Append(FormatEnumName(extension.workPolicy));
        }

        if (master != null && !master.Destroyed)
        {
            builder.AppendLine();
            builder.Append("Master: ");
            builder.Append(master.LabelShortCap);
        }

        if (TryGetRemainingSummonTicks(pawn, out int remainingTicks))
        {
            builder.AppendLine();
            builder.Append("Expires in: ");
            builder.Append(remainingTicks.ToStringTicksToPeriod());
        }

        return builder.ToString().TrimEnd();
    }

    private static bool TryGetRemainingSummonTicks(Pawn pawn, out int remainingTicks)
    {
        remainingTicks = 0;
        SummonedPawnMapComponent component = pawn?.MapHeld?.GetComponent<SummonedPawnMapComponent>()
            ?? pawn?.Map?.GetComponent<SummonedPawnMapComponent>();
        if (component == null || !component.TryGetRecord(pawn, out SummonedPawnRecord record) || record.ExpireAtTick < 0)
        {
            return false;
        }

        remainingTicks = System.Math.Max(0, record.ExpireAtTick - (Find.TickManager?.TicksGame ?? 0));
        return true;
    }

    private static string FormatEnumName<TEnum>(TEnum value)
        where TEnum : struct
    {
        string text = value.ToString();
        if (text == "Unspecified")
        {
            return "unspecified";
        }

        StringBuilder builder = new();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (i > 0 && char.IsUpper(c))
            {
                builder.Append(' ');
            }

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }
}
