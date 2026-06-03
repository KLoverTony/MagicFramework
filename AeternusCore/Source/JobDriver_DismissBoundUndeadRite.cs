using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class JobDriver_DismissBoundUndeadRite : JobDriver
    {
        private const TargetIndex MinionInd = TargetIndex.A;
        private const TargetIndex LecternInd = TargetIndex.B;
        private const TargetIndex OssuaryInd = TargetIndex.C;
        private const int DefaultRitualTicks = 900;
        private const int MaxMinionArrivalWaitTicks = 2500;

        private int minionArrivalWaitTicks;

        private Pawn Minion => job.GetTarget(MinionInd).Thing as Pawn;
        private Thing Lectern => job.GetTarget(LecternInd).Thing;
        private Thing Ossuary => job.GetTarget(OssuaryInd).Thing;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref minionArrivalWaitTicks, "minionArrivalWaitTicks");
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(MinionInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(LecternInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(OssuaryInd), job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(MinionInd);
            this.FailOnDestroyedOrNull(LecternInd);
            this.FailOnDestroyedOrNull(OssuaryInd);
            this.FailOn(() => !BonewrightUtility.IsBonewright(pawn));
            this.FailOn(() => !BonewrightMinionUtility.IsBoundMinionOf(Minion, pawn));

            yield return Toils_Reserve.Reserve(MinionInd);
            yield return Toils_Reserve.Reserve(LecternInd);
            yield return Toils_Reserve.Reserve(OssuaryInd);

            yield return Toils_General.DoAtomic(StartMinionDismissalWait);

            yield return Toils_Goto.GotoThing(LecternInd, PathEndMode.InteractionCell);
            yield return WaitForMinionArrival();

            Toil rite = Toils_General.WaitWith(LecternInd, DefaultRitualTicks, useProgressBar: true, face: MinionInd);
            rite.WithEffect(EffecterDefOf.Research, LecternInd);
            yield return rite;

            yield return Toils_General.DoAtomic(FinishRite);
        }

        private void StartMinionDismissalWait()
        {
            Pawn minion = Minion;
            if (!BonewrightMinionUtility.IsBoundMinionOf(minion, pawn))
                return;

            if (minion.carryTracker?.CarriedThing != null)
                minion.carryTracker.TryDropCarriedThing(minion.Position, ThingPlaceMode.Near, out _);

            JobDef waitJobDef = DefDatabase<JobDef>.GetNamedSilentFail("AF_AwaitDismissalRite");
            if (waitJobDef == null)
                return;

            Job waitJob = JobMaker.MakeJob(waitJobDef, Ossuary, pawn);
            waitJob.playerForced = true;
            minion.jobs?.StartJob(waitJob, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true);
        }

        private Toil WaitForMinionArrival()
        {
            Toil wait = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never,
                handlingFacing = true,
                initAction = () =>
                {
                    minionArrivalWaitTicks = 0;
                },
                tickAction = () =>
                {
                    Pawn minion = Minion;
                    Thing ossuary = Ossuary;
                    if (minion == null || ossuary == null)
                    {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    pawn.rotationTracker.FaceCell(minion.Position);

                    if (IsMinionAtDismissalPoint(minion, ossuary))
                    {
                        ReadyForNextToil();
                        return;
                    }

                    minionArrivalWaitTicks++;
                    if (minionArrivalWaitTicks >= MaxMinionArrivalWaitTicks)
                    {
                        Messages.Message(minion.LabelShortCap + " could not reach the ossuary for dismissal.", ossuary, MessageTypeDefOf.RejectInput, historical: false);
                        EndJobWith(JobCondition.Incompletable);
                    }
                }
            };
            wait.WithProgressBar(LecternInd, () => MaxMinionArrivalWaitTicks <= 0 ? 1f : (float)minionArrivalWaitTicks / MaxMinionArrivalWaitTicks);
            return wait;
        }

        private static bool IsMinionAtDismissalPoint(Pawn minion, Thing ossuary)
        {
            return minion.Spawned
                && ossuary.Spawned
                && (minion.Position == ossuary.Position || minion.Position.AdjacentTo8Way(ossuary.Position));
        }

        private void FinishRite()
        {
            Pawn minion = Minion;
            if (!BonewrightUtility.IsBonewright(pawn))
            {
                Messages.Message("Only a Bonewright can complete the dismissal rite.", pawn, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (!BonewrightMinionUtility.IsBoundMinionOf(minion, pawn))
            {
                Messages.Message(pawn.LabelShortCap + " is not bound to the selected undead.", Lectern ?? Ossuary, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Comp_OssuaryObituary ossuaryContents = (Ossuary as ThingWithComps)?.GetComp<Comp_OssuaryObituary>();
            if (ossuaryContents == null)
            {
                Messages.Message("The ossuary bone box cannot receive remains.", Ossuary ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (ossuaryContents.HasRemains)
            {
                Messages.Message("The ossuary bone box is already filled.", Ossuary ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            string minionLabel = minion.LabelShortCap;
            ossuaryContents.RecordDismissedUndead(minion, pawn);
            minion.jobs?.EndCurrentJob(JobCondition.InterruptForced);
            minion.Destroy(DestroyMode.Vanish);
            Messages.Message(minionLabel + " has been dismissed by the ossuary rite.", Lectern ?? Ossuary, MessageTypeDefOf.PositiveEvent, historical: false);
        }
    }
}
