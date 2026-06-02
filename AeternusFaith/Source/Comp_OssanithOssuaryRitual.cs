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
        public bool hideWhenCircleAbsent;

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
            {
                if (Props.hideWhenCircleAbsent && !HasAdjacentCircle())
                    yield break;

                command.Disable(failReason);
            }

            yield return command;

            if (Props.circleDef?.defName == "AF_OssanithCircle")
                yield return MakeDismissMinionCommand();
        }

        private Gizmo MakeDismissMinionCommand()
        {
            Command_Action command = new Command_Action
            {
                defaultLabel = "Dismiss bound undead",
                defaultDesc = "Begin an Ossanith burial rite to dismiss and destroy a Bonewright's bound undead minion.",
                icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true),
                action = OpenDismissMinionMenu
            };

            if (!TryFindDismissalSetup(out _, out _, out string failReason))
            {
                command.Disable(failReason);
            }
            else if (!PotentialDismissalConductors().Any())
            {
                command.Disable("Requires a Bonewright with a bound undead minion.");
            }

            return command;
        }

        private bool HasAdjacentCircle()
        {
            return Props.circleDef != null &&
                   RitualAdjacencyUtility.TryFindOrthogonallyAdjacentThing(parent, Props.circleDef, out _);
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

            foreach (Thing candidateCircle in AdjacentRitualTargets())
            {
                circle = candidateCircle;
                if (circle == null)
                    continue;

                ossuary = circle.Position.GetThingList(parent.Map).FirstOrDefault(t => t.def == Props.ossuaryDef);
                if (ossuary != null && !IsFilledOssuary(ossuary))
                    return true;
            }

            failReason = "Requires an orthogonally adjacent Ossanith circle with an empty ossuary bone box in its center.";
            return false;
        }

        private bool TryFindDismissalSetup(out Thing circle, out Thing ossuary, out string failReason)
        {
            circle = null;
            ossuary = null;
            failReason = null;

            if (Props.circleDef == null || Props.ossuaryDef == null)
            {
                failReason = "The dismissal rite is missing its ritual definitions.";
                return false;
            }

            foreach (Thing candidateCircle in AdjacentRitualTargets())
            {
                circle = candidateCircle;
                if (circle == null)
                    continue;

                ossuary = circle.Position.GetThingList(parent.Map).FirstOrDefault(t => t.def == Props.ossuaryDef);
                if (ossuary != null)
                    return true;
            }

            failReason = "Requires an orthogonally adjacent Ossanith circle with an ossuary bone box in its center.";
            return false;
        }

        private IEnumerable<Thing> AdjacentRitualTargets()
        {
            if (RitualAdjacencyUtility.TryFindOrthogonallyAdjacentThing(parent, Props.circleDef, out Thing circle))
            {
                yield return circle;
            }
        }

        private void OpenRitualDialog()
        {
            if (!TryFindRitualSetup(out Thing circle, out Thing ossuary, out string failReason))
            {
                Messages.Message(failReason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Find.WindowStack.Add(new Dialog_OssuaryRitual(parent, circle, ossuary, TryStartRitualJobs, ValidateCorpseTarget, ValidateConductor, ValidateAudience));
        }

        private void OpenDismissMinionMenu()
        {
            if (!TryFindDismissalSetup(out _, out Thing ossuary, out string failReason))
            {
                Messages.Message(failReason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Pawn conductor in PotentialDismissalConductors())
            {
                foreach (Pawn minion in BonewrightMinionUtility.BoundMinionsOf(conductor))
                {
                    AcceptanceReport report = ValidateDismissalConductor(conductor, minion, ossuary);
                    string label = conductor.LabelShortCap + ": dismiss " + minion.LabelShortCap;
                    options.Add(report.Accepted
                        ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, () => TryStartDismissalJob(conductor, minion, ossuary)), conductor, ossuary)
                        : new FloatMenuOption(label + ": " + report.Reason, null));
                }
            }

            if (options.Count == 0)
                options.Add(new FloatMenuOption("No Bonewright has a bound undead minion.", null));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        internal AcceptanceReport ValidateCorpseTarget(Corpse corpse)
        {
            return RitualCorpseEligibilityUtility.ValidateHumanlikeMortalCorpse(corpse, parent.Map);
        }

        internal bool IsValidCorpseTarget(Corpse corpse)
        {
            return ValidateCorpseTarget(corpse).Accepted;
        }

        private void TryStartRitualJobs(Pawn conductor, List<Pawn> audience, Corpse corpse, Thing circle, Thing ossuary)
        {
            AcceptanceReport corpseReport = ValidateCorpseTarget(corpse);
            if (!corpseReport.Accepted)
            {
                Messages.Message(corpseReport.Reason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            AcceptanceReport conductorReport = ValidateConductor(conductor, corpse, ossuary);
            if (!conductorReport.Accepted)
            {
                Messages.Message(conductorReport.Reason, corpse, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (IsFilledOssuary(ossuary))
            {
                Messages.Message("The selected ossuary bone box is already filled.", ossuary, MessageTypeDefOf.RejectInput, historical: false);
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
            return ValidateConductor(pawn, corpse, ossuary).Accepted;
        }

        internal AcceptanceReport ValidateConductor(Pawn pawn, Corpse corpse, Thing ossuary)
        {
            AcceptanceReport report = RitualPawnEligibilityUtility.ValidateBonewrightConductor(pawn);
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, corpse, PathEndMode.ClosestTouch, "corpse");
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, parent, PathEndMode.InteractionCell, "lectern");
            if (!report.Accepted)
                return report;

            return RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, ossuary, PathEndMode.Touch, "ossuary bone box");
        }

        private IEnumerable<Pawn> PotentialDismissalConductors()
        {
            return parent?.Map?.mapPawns?.FreeColonistsSpawned
                       .Where(pawn => BonewrightUtility.IsBonewright(pawn) && BonewrightMinionUtility.TryFindBoundMinion(pawn, out _))
                   ?? Enumerable.Empty<Pawn>();
        }

        private AcceptanceReport ValidateDismissalConductor(Pawn pawn, Pawn minion, Thing ossuary)
        {
            AcceptanceReport report = RitualPawnEligibilityUtility.ValidateBonewrightConductor(pawn);
            if (!report.Accepted)
                return report;

            if (!BonewrightMinionUtility.IsBoundMinionOf(minion, pawn))
                return pawn.LabelShortCap + " is not bound to " + (minion?.LabelShortCap ?? "that undead") + ".";

            if (IsFilledOssuary(ossuary))
                return "The selected ossuary bone box is already filled.";

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, minion, PathEndMode.Touch, "bound undead");
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, parent, PathEndMode.InteractionCell, "lectern");
            if (!report.Accepted)
                return report;

            return RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, ossuary, PathEndMode.Touch, "ossuary bone box");
        }

        private void TryStartDismissalJob(Pawn conductor, Pawn minion, Thing ossuary)
        {
            AcceptanceReport report = ValidateDismissalConductor(conductor, minion, ossuary);
            if (!report.Accepted)
            {
                Messages.Message(report.Reason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_DismissBoundUndeadRite"), minion, parent, ossuary);
            job.locomotionUrgency = LocomotionUrgency.Jog;
            job.playerForced = true;

            if (!conductor.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                Messages.Message(conductor.LabelShortCap + " could not start the dismissal rite.", conductor, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Messages.Message(conductor.LabelShortCap + " begins the dismissal rite for " + minion.LabelShortCap + ".", parent, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        private static bool IsFilledOssuary(Thing ossuary)
        {
            return (ossuary as ThingWithComps)?.GetComp<Comp_OssuaryObituary>()?.HasRemains == true;
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
