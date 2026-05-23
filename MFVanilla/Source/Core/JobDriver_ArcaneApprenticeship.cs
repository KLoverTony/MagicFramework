using System.Collections.Generic;
using MagicFramework.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace MFVanilla.Core;

public sealed class JobDriver_ArcaneApprenticeship : JobDriver
{
    private const int LearnIntervalTicks = 60;
    private const float DesiredDistance = 3f;
    private const float MaximumLearningDistance = 6f;

    private int ticksSinceLearning;

    private Pawn Mentor => job.GetTarget(TargetIndex.A).Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOn(() => !IsValidMentor(Mentor));

        Toil observe = new()
        {
            defaultCompleteMode = ToilCompleteMode.Never,
            socialMode = RandomSocialMode.Off,
            tickIntervalAction = tickInterval =>
            {
                Pawn mentor = Mentor;
                if (!IsValidMentor(mentor))
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                FollowMentor(mentor);

                ticksSinceLearning += tickInterval;
                if (ticksSinceLearning >= LearnIntervalTicks
                    && pawn.Position.DistanceTo(mentor.Position) <= MaximumLearningDistance)
                {
                    int observedTicks = ticksSinceLearning;
                    ticksSinceLearning = 0;
                    ArcanePracticeUtility.NotifyArcaneApprenticeshipObserved(pawn, mentor, observedTicks);
                }
            }
        };

        yield return observe;
    }

    private void FollowMentor(Pawn mentor)
    {
        float distance = pawn.Position.DistanceTo(mentor.Position);
        if (distance <= DesiredDistance)
        {
            if (pawn.pather.Moving)
            {
                pawn.pather.StopDead();
            }

            return;
        }

        if (!pawn.pather.Moving || pawn.pather.Destination.Cell != mentor.Position)
        {
            pawn.pather.StartPath(mentor, PathEndMode.Touch);
        }
    }

    private static bool IsValidMentor(Pawn mentor)
    {
        return mentor != null
            && mentor.Spawned
            && !mentor.Dead
            && !mentor.Downed
            && !mentor.Drafted
            && ArcanePracticeUtility.IsPawnDoingArcanePractice(mentor);
    }
}

public sealed class WorkGiver_ArcaneApprenticeship : WorkGiver_Scanner
{
    private static JobDef apprenticeshipJobDef;

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn?.Map?.mapPawns?.FreeColonistsSpawned ?? new List<Pawn>();
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return t is Pawn mentor
            && IsEligibleApprentice(pawn)
            && IsEligibleMentor(pawn, mentor)
            && pawn.CanReach(mentor, PathEndMode.Touch, Danger.Some);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return HasJobOnThing(pawn, t, forced)
            ? JobMaker.MakeJob(ApprenticeshipJobDef, t)
            : null;
    }

    private static bool IsEligibleApprentice(Pawn pawn)
    {
        return pawn != null
            && pawn.Spawned
            && !pawn.Dead
            && !pawn.Downed
            && !pawn.Drafted;
    }

    private static bool IsEligibleMentor(Pawn apprentice, Pawn mentor)
    {
        return ArcanePracticeUtility.CanApprenticeLearnFrom(apprentice, mentor)
            && mentor.CurJobDef?.defName != "MFV_ArcaneApprenticeship"
            && ArcanePracticeUtility.IsPawnDoingArcanePractice(mentor);
    }

    private static JobDef ApprenticeshipJobDef
    {
        get
        {
            apprenticeshipJobDef ??= DefDatabase<JobDef>.GetNamed("MFV_ArcaneApprenticeship");
            return apprenticeshipJobDef;
        }
    }
}
