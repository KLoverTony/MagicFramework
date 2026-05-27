using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith.Undead.Spectral
{
    public class JobDriver_SpectralVisit : JobDriver
    {
        private const int LingerTicks = 650;

        private static readonly TargetIndex VisitCellIndex = TargetIndex.A;
        private static readonly TargetIndex VisitPawnIndex = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => pawn?.Map == null);
            this.FailOn(() => job.GetTarget(VisitCellIndex).Cell.IsValid == false);

            Toil gotoCell = Toils_Goto.GotoCell(VisitCellIndex, PathEndMode.OnCell);
            gotoCell.socialMode = RandomSocialMode.Off;
            yield return gotoCell;

            Toil linger = Toils_General.Wait(LingerTicks);
            linger.socialMode = RandomSocialMode.Off;
            linger.handlingFacing = true;
            linger.tickAction = () =>
            {
                LocalTargetInfo visitPawn = job.GetTarget(VisitPawnIndex);
                if (visitPawn.HasThing)
                {
                    pawn.rotationTracker.FaceTarget(visitPawn);
                }
            };
            yield return linger;

            Toil complete = ToilMaker.MakeToil("CompleteSpectralVisit");
            complete.initAction = ApplyCompletionMemory;
            complete.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return complete;
        }

        private void ApplyCompletionMemory()
        {
            ThoughtDef thoughtDef = ResolveCompletionThought();
            if (thoughtDef != null)
            {
                pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtDef);
            }

            if (MapComponent_SpectralEntities.TryGetSpiritForManifestedPawn(pawn, out SpectralEntity spirit))
            {
                spirit.lastActionSummary = ResolveCompletionSummary();
            }
        }

        private ThoughtDef ResolveCompletionThought()
        {
            string thoughtDefName = job.def.defName switch
            {
                "AF_SpectralVisitLovedOne" => "AF_SpectreVisitedLovedOne",
                "AF_SpectralVisitFamily" => "AF_SpectreVisitedFamily",
                "AF_SpectralVisitAnchor" => "AF_SpectreVisitedAnchor",
                _ => null
            };

            return thoughtDefName.NullOrEmpty()
                ? null
                : DefDatabase<ThoughtDef>.GetNamedSilentFail(thoughtDefName);
        }

        private string ResolveCompletionSummary()
        {
            return job.def.defName switch
            {
                "AF_SpectralVisitLovedOne" => "Visited remembered love.",
                "AF_SpectralVisitFamily" => "Visited remembered family.",
                "AF_SpectralVisitAnchor" => "Returned to its anchor.",
                _ => "Completed visitation."
            };
        }
    }
}
