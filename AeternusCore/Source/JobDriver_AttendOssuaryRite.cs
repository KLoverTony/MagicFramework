using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class JobDriver_AttendOssuaryRite : JobDriver
    {
        private const TargetIndex StandCellInd = TargetIndex.A;
        private const TargetIndex FocusInd = TargetIndex.B;
        private const int AttendTicks = 5000;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(StandCellInd), job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(StandCellInd);
            yield return Toils_Goto.GotoCell(StandCellInd, PathEndMode.OnCell);
            yield return Toils_General.WaitWith(FocusInd, AttendTicks, useProgressBar: false, face: FocusInd);
        }
    }
}
