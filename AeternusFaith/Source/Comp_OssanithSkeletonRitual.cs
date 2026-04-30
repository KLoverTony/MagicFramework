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
        public string commandLabel = "Raise skeleton";
        public string commandDescription = "Begin an Ossanith rite to raise a corpse as a skeleton.";
        public string commandIconPath;

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
                command.Disable(failReason);

            yield return command;
        }

        private bool TryFindRitualSetup(out Thing circle, out string failReason)
        {
            circle = null;
            failReason = null;

            if (Props.circleDef == null)
            {
                failReason = "The skeleton rite is missing its ritual circle definition.";
                return false;
            }

            foreach (IntVec3 cell in GenAdj.CardinalDirections.Select(offset => parent.Position + offset))
            {
                if (!cell.InBounds(parent.Map))
                    continue;

                circle = cell.GetThingList(parent.Map).FirstOrDefault(t => t.def == Props.circleDef);
                if (circle != null)
                    return true;
            }

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

            Find.WindowStack.Add(new Dialog_SkeletonRitual(parent, circle, TryStartRitualJobs, IsValidCorpseTarget, IsEligibleConductor, IsEligibleAudience));
        }

        internal bool IsValidCorpseTarget(Corpse corpse)
        {
            return corpse != null && !corpse.Destroyed && corpse.Spawned && corpse.Map == parent.Map;
        }

        private void TryStartRitualJobs(Pawn conductor, List<Pawn> audience, Corpse corpse, Thing circle)
        {
            if (!IsValidCorpseTarget(corpse))
            {
                Messages.Message("Select a reachable corpse on this map.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (!IsEligibleConductor(conductor, corpse, circle))
            {
                Messages.Message("The selected conductor cannot reach and reserve the corpse, lectern, and circle.", corpse, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_PerformSkeletonRite"), corpse, parent, circle);
            job.count = 1;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            job.ignoreForbidden = true;
            job.playerForced = true;

            if (!conductor.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                Messages.Message(conductor.LabelShortCap + " could not start the skeleton rite.", conductor, MessageTypeDefOf.RejectInput, historical: false);
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

            Messages.Message(conductor.LabelShortCap + " begins the skeleton rite.", parent, MessageTypeDefOf.PositiveEvent, historical: false);
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
            if (!IsEligibleAudience(pawn))
                return false;

            if (pawn.WorkTagIsDisabled(WorkTags.ManualDumb))
                return false;

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving) ||
                !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return false;

            return pawn.CanReserveAndReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly) &&
                   pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly) &&
                   pawn.CanReserveAndReach(circle, PathEndMode.Touch, Danger.Deadly);
        }

        internal bool IsEligibleAudience(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.Downed || !pawn.Spawned || pawn.InMentalState)
                return false;

            if (pawn.Faction != Faction.OfPlayer)
                return false;

            return pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving);
        }
    }
}
