using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class CompProperties_OssanithSkeletonRitual : CompProperties
    {
        public ThingDef circleDef;
        public string commandLabel = "Animate Ossanith Skeleton";
        public string commandDescription = "Begin an Ossanith rite to animate a corpse as an Ossanith Skeleton.";
        public string commandIconPath;
        public bool hideWhenCircleAbsent;

        public CompProperties_OssanithSkeletonRitual()
        {
            compClass = typeof(Comp_OssanithSkeletonRitual);
        }
    }

    public class Comp_OssanithSkeletonRitual : ThingComp
    {
        private CompProperties_OssanithSkeletonRitual Props => (CompProperties_OssanithSkeletonRitual)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            Command_Action command = new Command_Action
            {
                defaultLabel = Props.commandLabel,
                defaultDesc = Props.commandDescription,
                icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true),
                action = OpenRitualDialog
            };

            if (!TryFindRitualSetup(out _, out string failReason))
            {
                if (Props.hideWhenCircleAbsent && !HasAdjacentCircle())
                    yield break;

                command.Disable(failReason);
            }

            yield return command;
        }

        private bool HasAdjacentCircle()
        {
            return Props.circleDef != null &&
                   RitualAdjacencyUtility.TryFindOrthogonallyAdjacentThing(parent, Props.circleDef, out _);
        }

        private bool TryFindRitualSetup(out Thing circle, out string failReason)
        {
            circle = null;
            failReason = null;

            if (Props.circleDef == null)
            {
                failReason = "The animation rite is missing its ritual circle definition.";
                return false;
            }

            if (RitualAdjacencyUtility.TryFindOrthogonallyAdjacentThing(parent, Props.circleDef, out circle))
                return true;

            failReason = "Requires an orthogonally adjacent Ossanith circle.";
            return false;
        }

        private void OpenRitualDialog()
        {
            if (!TryFindRitualSetup(out Thing circle, out string failReason))
            {
                Messages.Message(failReason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Find.WindowStack.Add(new Dialog_SkeletonRitual(parent, circle, TryStartRitualJobs, ValidateCorpseTarget, ValidateConductor, ValidateAudience, Props.commandLabel, Props.commandLabel));
        }

        internal AcceptanceReport ValidateCorpseTarget(Corpse corpse)
        {
            return RitualCorpseEligibilityUtility.ValidateHumanlikeMortalCorpse(corpse, parent.Map);
        }

        internal bool IsValidCorpseTarget(Corpse corpse)
        {
            return ValidateCorpseTarget(corpse).Accepted;
        }

        private void TryStartRitualJobs(Pawn conductor, List<Pawn> audience, Corpse corpse, Thing circle)
        {
            AcceptanceReport corpseReport = ValidateCorpseTarget(corpse);
            if (!corpseReport.Accepted)
            {
                Messages.Message(corpseReport.Reason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            AcceptanceReport conductorReport = ValidateConductor(conductor, corpse, circle);
            if (!conductorReport.Accepted)
            {
                Messages.Message(conductorReport.Reason, corpse, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_PerformSkeletonRite"), corpse, parent, circle);
            job.count = 1;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            job.ignoreForbidden = true;
            job.playerForced = true;

            if (!conductor.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                Messages.Message(conductor.LabelShortCap + " could not start " + Props.commandLabel + ".", conductor, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            HashSet<IntVec3> reservedAudienceCells = new HashSet<IntVec3>();
            foreach (Pawn attendee in audience.Where(pawn => pawn != null && pawn != conductor && IsEligibleAudience(pawn)))
            {
                IntVec3 audienceCell = FindAudienceCell(attendee, circle, reservedAudienceCells);
                reservedAudienceCells.Add(audienceCell);
                Job attendJob = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_AttendSkeletonRite"), audienceCell, circle, parent);
                attendJob.locomotionUrgency = LocomotionUrgency.Walk;
                attendee.jobs.TryTakeOrderedJob(attendJob, JobTag.Misc);
            }

            Messages.Message(conductor.LabelShortCap + " begins " + Props.commandLabel + ".", parent, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        private IntVec3 FindAudienceCell(Pawn attendee, Thing circle, HashSet<IntVec3> reservedAudienceCells)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(circle.Position, 4.5f, true).OrderBy(cell => cell.DistanceToSquared(circle.Position)))
            {
                if (!reservedAudienceCells.Contains(cell) && cell.InBounds(parent.Map) && cell.Standable(parent.Map) && attendee.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly))
                    return cell;
            }

            return parent.InteractionCell;
        }

        internal bool IsEligibleConductor(Pawn pawn, Corpse corpse, Thing circle)
        {
            return ValidateConductor(pawn, corpse, circle).Accepted;
        }

        internal AcceptanceReport ValidateConductor(Pawn pawn, Corpse corpse, Thing circle)
        {
            AcceptanceReport report = RitualPawnEligibilityUtility.ValidateBonewrightConductor(pawn);
            if (!report.Accepted)
                return report;

            report = BonewrightMinionUtility.ValidateCanBindMinion(pawn);
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, corpse, PathEndMode.ClosestTouch, "corpse");
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, parent, PathEndMode.InteractionCell, "lectern");
            if (!report.Accepted)
                return report;

            return RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, circle, PathEndMode.Touch, "ritual circle");
        }

        internal bool IsEligibleAudience(Pawn pawn)
        {
            return ValidateAudience(pawn).Accepted;
        }

        internal AcceptanceReport ValidateAudience(Pawn pawn)
        {
            return RitualPawnEligibilityUtility.ValidateAudiencePawn(pawn);
        }
    }
}
