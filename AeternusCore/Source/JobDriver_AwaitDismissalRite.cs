using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class JobDriver_AwaitDismissalRite : JobDriver
    {
        private const TargetIndex OssuaryInd = TargetIndex.A;
        private const TargetIndex MasterInd = TargetIndex.B;
        private const int DefaultWaitTicks = 1200;

        private Pawn Master => job.GetTarget(MasterInd).Thing as Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(OssuaryInd);
            this.FailOn(() => !BonewrightMinionUtility.IsBoundMinionOf(pawn, Master));

            yield return Toils_Goto.GotoThing(OssuaryInd, PathEndMode.OnCell);

            Toil wait = Toils_General.Wait(DefaultWaitTicks, TargetIndex.None);
            wait.WithProgressBarToilDelay(OssuaryInd);
            wait.handlingFacing = true;
            wait.tickAction = () =>
            {
                Thing masterThing = job.GetTarget(MasterInd).Thing;
                if (masterThing != null && masterThing.Spawned)
                    pawn.rotationTracker.FaceCell(masterThing.Position);
            };
            yield return wait;
        }
    }
}
