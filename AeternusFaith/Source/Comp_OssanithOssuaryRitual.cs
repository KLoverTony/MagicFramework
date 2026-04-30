using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class CompProperties_OssanithOssuaryRitual : CompProperties
    {
        public ThingDef circleDef;
        public ThingDef ossuaryDef;
        public string commandLabel = "Begin rite";
        public string commandDescription = "Begin an Ossanith ossuary rite";
        public string commandIconPath;

        public CompProperties_OssanithOssuaryRitual()
        {
            this.compClass = typeof(Comp_OssanithOssuaryRitual);
        }
    }

    public class Comp_OssanithOssuaryRitual : ThingComp
    {
        private CompProperties_OssanithOssuaryRitual Props => (CompProperties_OssanithOssuaryRitual)this.props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            Command_Action command = new Command_Action
            {
                defaultLabel = Props.commandLabel,
                defaultDesc = Props.commandDescription,
                icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true),
                action = OpenRitualDialog
            };

            if (!TryFindRitualSetup(out _, out _, out string failReason))
                command.Disable(failReason);

            yield return command;
        }

        private bool TryFindRitualSetup(out Thing circle, out Thing ossuary, out string failReason)
        {
            circle = null;
            ossuary = null;
            failReason = null;

            if (Props.circleDef == null || Props.ossuaryDef == null)
            {
                failReason = "The ossuary rite is missing its ritual definitions.";
                return false;
            }

            IntVec3[] adjacentCells = GenAdj.CardinalDirections.Select(offset => parent.Position + offset).ToArray();
            foreach (IntVec3 cell in adjacentCells)
            {
                if (!cell.InBounds(parent.Map))
                    continue;

                circle = cell.GetThingList(parent.Map).FirstOrDefault(t => t.def == Props.circleDef);
                if (circle == null)
                    continue;

                ossuary = circle.Position.GetThingList(parent.Map).FirstOrDefault(t => t.def == Props.ossuaryDef);
                if (ossuary != null)
                    return true;
            }

            failReason = "Requires an orthogonally adjacent Ossanith circle with a completed ossuary bone box in its center.";
            return false;
        }

        private void OpenRitualDialog()
        {
            if (!TryFindRitualSetup(out Thing circle, out Thing ossuary, out string failReason))
            {
                Messages.Message(failReason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Find.WindowStack.Add(new Dialog_OssuaryRitual(parent, circle, ossuary, TryStartRitualJobs, IsValidCorpseTarget, IsEligibleConductor, IsEligibleAudience));
        }

        internal bool IsValidCorpseTarget(Corpse corpse)
        {
            return corpse != null && !corpse.Destroyed && corpse.Spawned && corpse.Map == parent.Map;
        }

        private void TryStartRitualJobs(Pawn conductor, List<Pawn> audience, Corpse corpse, Thing circle, Thing ossuary)
        {
            if (!IsValidCorpseTarget(corpse))
            {
                Messages.Message("Select a reachable corpse on this map.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (!IsEligibleConductor(conductor, corpse, ossuary))
            {
                Messages.Message("The selected conductor cannot reach and reserve the corpse, lectern, and ossuary.", corpse, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            JobDef jobDef = DefDatabase<JobDef>.GetNamed("AF_PerformOssuaryRite");
            Job job = JobMaker.MakeJob(jobDef, corpse, parent, ossuary);
            job.count = 1;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            job.ignoreForbidden = true;
            job.playerForced = true;

            if (!conductor.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                Messages.Message(conductor.LabelShortCap + " could not start the ossuary rite.", conductor, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            HashSet<IntVec3> reservedAudienceCells = new HashSet<IntVec3>();
            foreach (Pawn attendee in audience.Where(pawn => pawn != null && pawn != conductor && IsEligibleAudience(pawn)))
            {
                IntVec3 audienceCell = FindAudienceCell(attendee, circle, ossuary, reservedAudienceCells);
                reservedAudienceCells.Add(audienceCell);
                Job attendJob = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_AttendOssuaryRite"), audienceCell, ossuary, parent);
                attendJob.locomotionUrgency = LocomotionUrgency.Walk;
                attendee.jobs.TryTakeOrderedJob(attendJob, JobTag.Misc);
            }

            Messages.Message(conductor.LabelShortCap + " begins the ossuary rite.", parent, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        private IntVec3 FindAudienceCell(Pawn attendee, Thing circle, Thing ossuary, HashSet<IntVec3> reservedAudienceCells)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(circle.Position, 4.5f, true).OrderBy(cell => cell.DistanceToSquared(ossuary.Position)))
            {
                if (!reservedAudienceCells.Contains(cell) && cell.InBounds(parent.Map) && cell.Standable(parent.Map) && attendee.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly))
                    return cell;
            }

            return parent.InteractionCell;
        }

        internal bool IsEligibleConductor(Pawn pawn, Corpse corpse, Thing ossuary)
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
                   pawn.CanReserveAndReach(ossuary, PathEndMode.Touch, Danger.Deadly);
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
