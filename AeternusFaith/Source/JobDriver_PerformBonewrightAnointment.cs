using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class JobDriver_PerformBonewrightAnointment : JobDriver
    {
        private const TargetIndex InitiateInd = TargetIndex.A;
        private const TargetIndex CircleInd = TargetIndex.B;
        private const int AnointmentTicks = 900;

        private Pawn Initiate => job.GetTarget(InitiateInd).Pawn;
        private Thing Circle => job.GetTarget(CircleInd).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(CircleInd), job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CircleInd);
            this.FailOn(() => !BonewrightUtility.CanOfficiateAnointment(pawn));
            this.FailOn(() => !BonewrightUtility.CanBeAnointed(Initiate, Map, out _));

            yield return Toils_Reserve.Reserve(CircleInd);
            yield return Toils_Goto.GotoThing(CircleInd, PathEndMode.Touch);

            Toil rite = Toils_General.WaitWith(CircleInd, AnointmentTicks, useProgressBar: true, face: InitiateInd);
            rite.WithEffect(EffecterDefOf.Research, CircleInd);
            yield return rite;

            yield return Toils_General.DoAtomic(FinishAnointment);
        }

        private void FinishAnointment()
        {
            Pawn initiate = Initiate;
            if (!BonewrightUtility.CanBeAnointed(initiate, Map, out string failReason))
            {
                Messages.Message(failReason, initiate ?? Circle, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            BonewrightUtility.AnointOssanithInitiate(initiate);
            FleckMaker.ThrowMetaIcon(initiate.Position, Map, FleckDefOf.IncapIcon);
            Messages.Message(initiate.LabelShortCap + " has been anointed as an Ossanith Bonewright initiate.", initiate, MessageTypeDefOf.PositiveEvent, historical: false);
        }
    }
}
