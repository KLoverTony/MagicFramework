using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class CompProperties_ChoralumWardenRitual : CompProperties
    {
        public ThingDef circleDef;
        public List<ThingDef> armorDefs;
        public string commandLabel = "Animate Reliquary Warden";
        public string commandDescription = "Begin a Choralum rite to animate a corpse and suit of armor as a Reliquary Warden.";
        public string commandIconPath;

        public CompProperties_ChoralumWardenRitual()
        {
            compClass = typeof(Comp_ChoralumWardenRitual);
        }
    }

    public class Comp_ChoralumWardenRitual : ThingComp
    {
        private CompProperties_ChoralumWardenRitual Props => (CompProperties_ChoralumWardenRitual)props;

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

            if (!TryFindRitualSetup(out _, out _, out string failReason))
                command.Disable(failReason);

            yield return command;
        }

        private bool TryFindRitualSetup(out Thing circle, out Thing armor, out string failReason)
        {
            circle = null;
            armor = null;
            failReason = null;

            if (Props.circleDef == null)
            {
                failReason = "The animation rite is missing its ritual circle definition.";
                return false;
            }

            if (!RitualAdjacencyUtility.TryFindOrthogonallyAdjacentThing(parent, Props.circleDef, out circle))
            {
                failReason = "Requires an orthogonally adjacent Choralum circle.";
                return false;
            }

            armor = FindUsableArmor(circle, null);
            if (armor == null)
            {
                failReason = "Requires plate armor or flak armor near the Choralum circle.";
                return false;
            }

            return true;
        }

        private void OpenRitualDialog()
        {
            if (!TryFindRitualSetup(out Thing circle, out _, out string failReason))
            {
                Messages.Message(failReason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Find.WindowStack.Add(new Dialog_SkeletonRitual(parent, circle, TryStartRitualJobs, ValidateCorpseTarget, ValidateConductorForDialog, ValidateAudience, Props.commandLabel, Props.commandLabel));
        }

        internal AcceptanceReport ValidateCorpseTarget(Corpse corpse)
        {
            return RitualCorpseEligibilityUtility.ValidateHumanlikeMortalCorpse(corpse, parent.Map);
        }

        private void TryStartRitualJobs(Pawn conductor, List<Pawn> audience, Corpse corpse, Thing circle)
        {
            AcceptanceReport corpseReport = ValidateCorpseTarget(corpse);
            if (!corpseReport.Accepted)
            {
                Messages.Message(corpseReport.Reason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Thing armor = FindUsableArmor(circle, conductor);
            if (armor == null)
            {
                Messages.Message("The Choralum rite requires plate armor or flak armor near the ritual circle.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            AcceptanceReport conductorReport = ValidateConductor(conductor, corpse, circle, armor);
            if (!conductorReport.Accepted)
            {
                Messages.Message(conductorReport.Reason, corpse, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_PerformChoralumWardenRite"), corpse, parent, circle);
            job.count = 1;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            job.ignoreForbidden = true;
            job.playerForced = true;
            job.targetQueueB = new List<LocalTargetInfo> { armor };

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

        internal AcceptanceReport ValidateConductor(Pawn pawn, Corpse corpse, Thing circle, Thing armor)
        {
            AcceptanceReport report = RitualPawnEligibilityUtility.ValidateBonewrightConductor(pawn);
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, corpse, PathEndMode.ClosestTouch, "corpse");
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, armor, PathEndMode.ClosestTouch, "armor");
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, parent, PathEndMode.InteractionCell, "lectern");
            if (!report.Accepted)
                return report;

            return RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, circle, PathEndMode.Touch, "ritual circle");
        }

        internal AcceptanceReport ValidateConductorForDialog(Pawn pawn, Corpse corpse, Thing circle)
        {
            Thing armor = FindUsableArmor(circle, pawn);
            if (armor == null)
                return "Requires plate armor or flak armor near the Choralum circle.";

            return ValidateConductor(pawn, corpse, circle, armor);
        }

        internal bool IsEligibleAudience(Pawn pawn)
        {
            return ValidateAudience(pawn).Accepted;
        }

        internal AcceptanceReport ValidateAudience(Pawn pawn)
        {
            return RitualPawnEligibilityUtility.ValidateAudiencePawn(pawn);
        }

        private Thing FindUsableArmor(Thing circle, Pawn reserver)
        {
            if (parent?.Map == null || circle == null)
                return null;

            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(circle.Position, parent.Map, 3f, true)
                         .OrderBy(thing => thing.Position.DistanceToSquared(circle.Position)))
            {
                if (!IsAcceptedArmor(thing) || thing.IsForbidden(Faction.OfPlayer))
                    continue;

                if (reserver != null && !reserver.CanReserveAndReach(thing, PathEndMode.ClosestTouch, Danger.Deadly))
                    continue;

                return thing;
            }

            return null;
        }

        private bool IsAcceptedArmor(Thing thing)
        {
            if (thing == null || thing.Destroyed || thing.Spawned == false)
                return false;

            if (Props.armorDefs != null && Props.armorDefs.Count > 0)
                return Props.armorDefs.Contains(thing.def);

            string defName = thing.def?.defName;
            return defName == "Apparel_PlateArmor" ||
                   defName == "Apparel_FlakVest" ||
                   defName == "Apparel_FlakJacket";
        }
    }
}
