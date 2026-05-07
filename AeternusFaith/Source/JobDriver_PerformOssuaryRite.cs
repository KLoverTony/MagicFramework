using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class JobDriver_PerformOssuaryRite : JobDriver
    {
        private const TargetIndex CorpseInd = TargetIndex.A;
        private const TargetIndex LecternInd = TargetIndex.B;
        private const TargetIndex OssuaryInd = TargetIndex.C;
        private const int DefaultRitualTicks = 900;

        private Corpse Corpse => job.GetTarget(CorpseInd).Thing as Corpse;
        private Thing Lectern => job.GetTarget(LecternInd).Thing;
        private Thing Ossuary => job.GetTarget(OssuaryInd).Thing;
        private IntVec3 OssuaryCell => job.GetTarget(OssuaryInd).Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(CorpseInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(LecternInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(OssuaryInd), job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CorpseInd);
            this.FailOnDestroyedOrNull(LecternInd);
            this.FailOnDestroyedOrNull(OssuaryInd);

            yield return Toils_Reserve.Reserve(CorpseInd);
            yield return Toils_Reserve.Reserve(LecternInd);
            yield return Toils_Reserve.Reserve(OssuaryInd);

            Toil postPickup = Toils_General.Label();
            yield return Toils_Jump.JumpIf(postPickup, () => pawn.carryTracker.CarriedThing == Corpse);

            yield return Toils_Goto.GotoThing(CorpseInd, PathEndMode.ClosestTouch, canGotoSpawnedParent: true)
                .FailOnSomeonePhysicallyInteracting(CorpseInd)
                .FailOnSelfAndParentsDespawnedOrNull(CorpseInd);

            yield return Toils_General.DoAtomic(PickUpCorpse);
            yield return postPickup;

            yield return Toils_Goto.GotoThing(OssuaryInd, PathEndMode.Touch);
            yield return Toils_General.DoAtomic(DropCorpseNearOssuary);

            yield return Toils_Goto.GotoThing(LecternInd, PathEndMode.InteractionCell);

            Toil rite = Toils_General.WaitWith(LecternInd, DefaultRitualTicks, useProgressBar: true, face: OssuaryInd);
            rite.WithEffect(EffecterDefOf.Research, LecternInd);
            yield return rite;

            yield return Toils_General.DoAtomic(FinishRite);
        }

        private void PickUpCorpse()
        {
            Corpse corpse = Corpse;
            if (corpse == null || corpse.Destroyed)
            {
                Messages.Message("The ossuary rite could not find the selected corpse.", Lectern ?? Ossuary, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            if (pawn.carryTracker.CarriedThing == corpse)
                return;

            int carriedCount = pawn.carryTracker.TryStartCarry(corpse, 1, reserve: false);
            if (carriedCount <= 0)
            {
                Messages.Message(pawn.LabelShortCap + " could not carry " + corpse.LabelShortCap + " to the ossuary.", corpse, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
            }
        }

        private void DropCorpseNearOssuary()
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing == null)
            {
                Messages.Message(pawn.LabelShortCap + " reached the ossuary without the selected corpse.", Ossuary ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            if (!pawn.carryTracker.TryDropCarriedThing(OssuaryCell, ThingPlaceMode.Near, out _))
            {
                Messages.Message(pawn.LabelShortCap + " could not place the corpse near the ossuary.", Ossuary ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
            }
        }

        private void FinishRite()
        {
            Corpse corpse = Corpse ?? FindPlacedCorpseNearOssuary();
            if (corpse == null || corpse.Destroyed)
                return;

            Comp_OssuaryObituary ossuaryContents = (Ossuary as ThingWithComps)?.GetComp<Comp_OssuaryObituary>();
            if (ossuaryContents?.HasRemains == true)
            {
                Messages.Message("The ossuary bone box is already filled.", Ossuary ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Pawn deceased = corpse.InnerPawn;
            ossuaryContents?.Record(corpse, pawn);
            string corpseLabel = corpse.LabelShortCap;
            MarkFuneralObligationsCompleted(deceased);
            corpse.Destroy(DestroyMode.Vanish);
            MarkFuneralObligationsCompleted(deceased);
            ReleaseAttendees();
            Messages.Message(corpseLabel + " has been sealed by the ossuary rite.", Lectern ?? Ossuary, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        private void ReleaseAttendees()
        {
            JobDef attendJobDef = DefDatabase<JobDef>.GetNamedSilentFail("AF_AttendOssuaryRite");
            if (attendJobDef == null)
                return;

            foreach (Pawn attendee in Map.mapPawns.FreeColonistsSpawned)
            {
                if (attendee == pawn || attendee.CurJobDef != attendJobDef)
                    continue;

                LocalTargetInfo focus = attendee.CurJob.GetTarget(TargetIndex.B);
                if (focus.Thing == Ossuary)
                    attendee.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        }

        private void MarkFuneralObligationsCompleted(Pawn deceased)
        {
            if (!ModsConfig.IdeologyActive || deceased?.Ideo == null)
                return;

            CompleteFuneralObligations(deceased.Ideo.GetPrecept(PreceptDefOf.Funeral) as Precept_Ritual, deceased);
            CompleteFuneralObligations(deceased.Ideo.GetPrecept(PreceptDefOf.FuneralNoCorpse) as Precept_Ritual, deceased);
        }

        private void CompleteFuneralObligations(Precept_Ritual ritual, Pawn deceased)
        {
            if (ritual?.activeObligations == null)
                return;

            List<RitualObligation> obligations = new List<RitualObligation>(ritual.activeObligations);
            foreach (RitualObligation obligation in obligations)
            {
                if (ObligationMatchesPawn(obligation, deceased))
                    ritual.RemoveObligation(obligation, completed: true);
            }
        }

        private bool ObligationMatchesPawn(RitualObligation obligation, Pawn deceased)
        {
            if (obligation == null || deceased == null)
                return false;

            if (obligation.onlyForPawns != null && obligation.onlyForPawns.Contains(deceased))
                return true;

            return TargetContainsPawn(obligation.targetA, deceased) ||
                   TargetContainsPawn(obligation.targetB, deceased) ||
                   TargetContainsPawn(obligation.targetC, deceased);
        }

        private bool TargetContainsPawn(TargetInfo target, Pawn deceased)
        {
            if (!target.IsValid)
                return false;

            Thing thing = target.Thing;
            if (thing == deceased)
                return true;

            return thing is Corpse targetCorpse && targetCorpse.InnerPawn == deceased;
        }

        private Corpse FindPlacedCorpseNearOssuary()
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(OssuaryCell, 2f, true))
            {
                if (!cell.InBounds(Map))
                    continue;

                Corpse corpse = cell.GetFirstThing<Corpse>(Map);
                if (corpse != null && !corpse.Destroyed)
                    return corpse;
            }

            return null;
        }
    }
}
