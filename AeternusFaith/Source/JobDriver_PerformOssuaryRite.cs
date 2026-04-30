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
            this.FailOnCannotTouch(CorpseInd, PathEndMode.ClosestTouch);
            this.FailOnCannotTouch(LecternInd, PathEndMode.InteractionCell);

            yield return Toils_Goto.GotoThing(CorpseInd, PathEndMode.ClosestTouch)
                .FailOnSomeonePhysicallyInteracting(CorpseInd);

            yield return Toils_Haul.StartCarryThing(CorpseInd, subtractNumTakenFromJobCount: true);

            Toil carryToOssuary = Toils_Haul.CarryHauledThingToCell(OssuaryInd);
            yield return carryToOssuary;

            yield return Toils_Haul.PlaceHauledThingInCell(OssuaryInd, carryToOssuary, storageMode: false);

            yield return Toils_Goto.GotoThing(LecternInd, PathEndMode.InteractionCell);

            Toil rite = Toils_General.WaitWith(LecternInd, DefaultRitualTicks, useProgressBar: true, face: OssuaryInd);
            rite.WithEffect(EffecterDefOf.Research, LecternInd);
            yield return rite;

            yield return Toils_General.DoAtomic(FinishRite);
        }

        private void FinishRite()
        {
            Corpse corpse = Corpse ?? OssuaryCell.GetFirstThing<Corpse>(Map);
            if (corpse == null || corpse.Destroyed)
                return;

            (Ossuary as ThingWithComps)?.GetComp<Comp_OssuaryObituary>()?.Record(corpse, pawn);
            string corpseLabel = corpse.LabelShortCap;
            corpse.Destroy(DestroyMode.Vanish);
            Messages.Message(corpseLabel + " has been sealed by the ossuary rite.", Lectern ?? Ossuary, MessageTypeDefOf.PositiveEvent, historical: false);
        }
    }
}
