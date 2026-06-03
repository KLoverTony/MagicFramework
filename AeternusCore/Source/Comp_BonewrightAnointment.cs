using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class CompProperties_BonewrightAnointment : CompProperties
    {
        public string commandLabel = "Anoint Bonewright";
        public string commandDescription = "Anoint a pawn into the Bonewright order. All initiates begin under the Ossanith foundation.";
        public string commandIconPath;

        public CompProperties_BonewrightAnointment()
        {
            compClass = typeof(Comp_BonewrightAnointment);
        }
    }

    public class Comp_BonewrightAnointment : ThingComp
    {
        private CompProperties_BonewrightAnointment Props => (CompProperties_BonewrightAnointment)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            Command_Action command = new Command_Action
            {
                defaultLabel = Props.commandLabel,
                defaultDesc = Props.commandDescription,
                action = OpenOfficiantMenu
            };

            if (!Props.commandIconPath.NullOrEmpty())
                command.icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true);

            if (!HasEligibleOfficiant(out string failReason))
                command.Disable(failReason);

            yield return command;
        }

        private bool HasEligibleOfficiant(out string failReason)
        {
            failReason = null;

            if (parent?.Map == null)
            {
                failReason = "Requires a spawned ritual circle.";
                return false;
            }

            if (!parent.Map.mapPawns.FreeColonistsSpawned.Any(BonewrightUtility.CanOfficiateAnointment))
            {
                failReason = "Requires a Soulwarden or existing Bonewright.";
                return false;
            }

            if (!parent.Map.mapPawns.FreeColonistsSpawned.Any(pawn => BonewrightUtility.CanBeAnointed(pawn, parent.Map, out _)))
            {
                failReason = "No eligible pawn can be anointed.";
                return false;
            }

            return true;
        }

        private void OpenOfficiantMenu()
        {
            Find.WindowStack.Add(new Dialog_BonewrightAnointment(
                parent,
                parent.Map.mapPawns.FreeColonistsSpawned,
                TryStartAnointment));
        }

        private void TryStartAnointment(Pawn officiant, Pawn initiate)
        {
            if (!BonewrightUtility.CanOfficiateAnointment(officiant))
            {
                Messages.Message("The selected officiant cannot perform the anointment.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (!BonewrightUtility.CanBeAnointed(initiate, parent.Map, out string failReason))
            {
                Messages.Message(failReason, initiate, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (!officiant.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly))
            {
                Messages.Message(officiant.LabelShortCap + " cannot reach the ritual circle.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_PerformBonewrightAnointment"), initiate, parent);
            job.locomotionUrgency = LocomotionUrgency.Jog;
            job.playerForced = true;

            if (!officiant.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                Messages.Message(officiant.LabelShortCap + " could not begin the anointment.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Messages.Message(officiant.LabelShortCap + " begins the Bonewright anointment for " + initiate.LabelShortCap + ".", parent, MessageTypeDefOf.PositiveEvent, historical: false);
        }
    }
}
