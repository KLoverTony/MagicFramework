using System.Collections.Generic;
using AeternusFaith;
using MagicFramework.PawnMemory;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith.Undead.Spectral
{
    public class JobDriver_PerformSpectreRite : JobDriver
    {
        private const TargetIndex CorpseInd = TargetIndex.A;
        private const TargetIndex LecternInd = TargetIndex.B;
        private const TargetIndex CircleInd = TargetIndex.C;
        private const int DefaultRitualTicks = 900;

        private Corpse Corpse => job.GetTarget(CorpseInd).Thing as Corpse;
        private Thing Lectern => job.GetTarget(LecternInd).Thing;
        private Thing Circle => job.GetTarget(CircleInd).Thing;
        private IntVec3 CircleCell => job.GetTarget(CircleInd).Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(CorpseInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(LecternInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(CircleInd), job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CorpseInd);
            this.FailOnDestroyedOrNull(LecternInd);
            this.FailOnDestroyedOrNull(CircleInd);
            this.FailOn(() => !BonewrightUtility.IsBonewright(pawn));

            yield return Toils_Reserve.Reserve(CorpseInd);
            yield return Toils_Reserve.Reserve(LecternInd);
            yield return Toils_Reserve.Reserve(CircleInd);

            Toil postPickup = Toils_General.Label();
            yield return Toils_Jump.JumpIf(postPickup, () => pawn.carryTracker.CarriedThing == Corpse);

            yield return Toils_Goto.GotoThing(CorpseInd, PathEndMode.ClosestTouch, canGotoSpawnedParent: true)
                .FailOnSomeonePhysicallyInteracting(CorpseInd)
                .FailOnSelfAndParentsDespawnedOrNull(CorpseInd);

            yield return Toils_General.DoAtomic(PickUpCorpse);
            yield return postPickup;

            yield return Toils_Goto.GotoThing(CircleInd, PathEndMode.Touch);
            yield return Toils_General.DoAtomic(DropCorpseAtCircle);

            yield return Toils_Goto.GotoThing(LecternInd, PathEndMode.InteractionCell);

            Toil rite = Toils_General.WaitWith(LecternInd, DefaultRitualTicks, useProgressBar: true, face: CircleInd);
            rite.WithEffect(EffecterDefOf.Research, LecternInd);
            yield return rite;

            yield return Toils_General.DoAtomic(FinishRite);
        }

        private void PickUpCorpse()
        {
            Corpse corpse = Corpse;
            if (corpse == null || corpse.Destroyed)
            {
                Messages.Message("The animation rite could not find the selected corpse.", Lectern ?? Circle, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            if (pawn.carryTracker.CarriedThing == corpse)
                return;

            int carriedCount = pawn.carryTracker.TryStartCarry(corpse, 1, reserve: false);
            if (carriedCount <= 0)
            {
                Messages.Message(pawn.LabelShortCap + " could not carry " + corpse.LabelShortCap + " to the ritual center.", corpse, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
            }
        }

        private void DropCorpseAtCircle()
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing == null)
            {
                Messages.Message(pawn.LabelShortCap + " reached the ritual center without the selected corpse.", Circle ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            if (!pawn.carryTracker.TryDropCarriedThing(CircleCell, ThingPlaceMode.Near, out _))
            {
                Messages.Message(pawn.LabelShortCap + " could not place the corpse at the ritual center.", Circle ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
            }
        }

        private void FinishRite()
        {
            if (!BonewrightUtility.IsBonewright(pawn))
            {
                Messages.Message("Only a Bonewright can complete the animation rite.", pawn, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (Map == null || Lectern == null || Circle == null || Circle.Destroyed)
                return;

            Corpse corpse = Corpse ?? FindPlacedCorpseNearCircle();
            if (corpse == null || corpse.Destroyed)
                return;

            if (MapComponent_SpectralEntities.HasActiveRiteBoundSpectre(pawn, out SpectralEntity existingSpectre))
            {
                Messages.Message(pawn.LabelShortCap + " is already bound to " + existingSpectre.label + ".", pawn, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            string corpseLabel = corpse.LabelShortCap;
            Pawn sourcePawn = corpse.InnerPawn;
            string sourceName = ResolveSourceName(corpse);
            Ideo sourceIdeo = ModsConfig.IdeologyActive ? sourcePawn?.ideo?.Ideo : null;
            PawnSoulRiteUtility.NotifyCorpseConsumed(sourcePawn, corpse);
            if (!corpse.Destroyed)
                corpse.Destroy(DestroyMode.Vanish);

            SpectralEntity spirit = Comp_SummonSpectre.ManifestSpectre(Map, Lectern, Circle, sourcePawn, sourceName, sourceIdeo, pawn);
            PawnSoulRiteUtility.NotifySpiritManifested(sourcePawn, spirit?.id, permanent: true);
            ReleaseAttendees();
            Messages.Message(corpseLabel + " manifests as a veilbound shade.", Circle, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        private void ReleaseAttendees()
        {
            JobDef attendJobDef = DefDatabase<JobDef>.GetNamedSilentFail("AF_AttendSpectreRite");
            if (attendJobDef == null)
                return;

            foreach (Pawn attendee in Map.mapPawns.FreeColonistsSpawned)
            {
                if (attendee == pawn || attendee.CurJobDef != attendJobDef)
                    continue;

                LocalTargetInfo focus = attendee.CurJob.GetTarget(TargetIndex.B);
                if (focus.Thing == Circle)
                    attendee.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        }

        private string ResolveSourceName(Corpse corpse)
        {
            string name = corpse?.InnerPawn?.Name?.ToStringShort;
            if (name.NullOrEmpty())
                name = corpse?.InnerPawn?.LabelShort;
            if (name.NullOrEmpty())
                name = "the dead";

            return name;
        }

        private Corpse FindPlacedCorpseNearCircle()
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(CircleCell, 2f, true))
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
