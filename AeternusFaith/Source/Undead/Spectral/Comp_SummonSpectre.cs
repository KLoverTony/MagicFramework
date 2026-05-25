using System.Collections.Generic;
using System.Linq;
using AeternusFaith;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AeternusFaith.Undead.Spectral
{
    public class CompProperties_SummonSpectre : CompProperties
    {
        public ThingDef circleDef;
        public string commandLabel = "Begin spectre rite";
        public string commandDescription = "Begin a Shroudhymn rite to manifest a spectre.";
        public string commandIconPath;

        public CompProperties_SummonSpectre()
        {
            compClass = typeof(Comp_SummonSpectre);
        }
    }

    public class Comp_SummonSpectre : ThingComp
    {
        private CompProperties_SummonSpectre Props => (CompProperties_SummonSpectre)props;

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

            if (!TryFindRitualCenter(out _, out string failReason))
                command.Disable(failReason);

            yield return command;
        }

        private bool TryFindRitualCenter(out Thing circle, out string failReason)
        {
            circle = null;
            failReason = null;

            if (Props.circleDef == null)
            {
                failReason = "The spectre rite is missing its ritual center definition.";
                return false;
            }

            if (RitualAdjacencyUtility.TryFindOrthogonallyAdjacentThing(parent, Props.circleDef, out circle))
                return true;

            failReason = "Requires an orthogonally adjacent Shroudhymn ritual center.";
            return false;
        }

        private void OpenRitualDialog()
        {
            if (!TryFindRitualCenter(out Thing circle, out string failReason))
            {
                Messages.Message(failReason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Find.WindowStack.Add(new Dialog_SpectreRitual(parent, circle, TryStartRitualJobs, ValidateCorpseTarget, ValidateConductor, ValidateAudience));
        }

        internal AcceptanceReport ValidateCorpseTarget(Corpse corpse)
        {
            return RitualCorpseEligibilityUtility.ValidateAnyCorpseOnMap(corpse, parent.Map);
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
                Messages.Message(conductorReport.Reason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (MapComponent_SpectralEntities.HasActiveRiteBoundSpectre(conductor, out SpectralEntity existingSpectre))
            {
                Messages.Message(conductor.LabelShortCap + " is already bound to " + existingSpectre.label + ".", conductor, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_PerformSpectreRite"), corpse, parent, circle);
            job.count = 1;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            job.ignoreForbidden = true;
            job.playerForced = true;

            if (!conductor.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                Messages.Message(conductor.LabelShortCap + " could not start the spectre rite.", conductor, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            HashSet<IntVec3> reservedAudienceCells = new HashSet<IntVec3>();
            foreach (Pawn attendee in audience.Where(pawn => pawn != null && pawn != conductor && IsEligibleAudience(pawn)))
            {
                IntVec3 audienceCell = FindAudienceCell(attendee, circle, reservedAudienceCells);
                reservedAudienceCells.Add(audienceCell);
                Job attendJob = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_AttendSpectreRite"), audienceCell, circle, parent);
                attendJob.locomotionUrgency = LocomotionUrgency.Walk;
                attendee.jobs.TryTakeOrderedJob(attendJob, JobTag.Misc);
            }

            Messages.Message(conductor.LabelShortCap + " begins the spectre rite.", parent, MessageTypeDefOf.PositiveEvent, historical: false);
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

            if (MapComponent_SpectralEntities.HasActiveRiteBoundSpectre(pawn, out SpectralEntity existingSpectre))
                return pawn.LabelShortCap + " is already bound to " + existingSpectre.label + ".";

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, corpse, PathEndMode.ClosestTouch, "corpse");
            if (!report.Accepted)
                return report;

            report = RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, parent, PathEndMode.InteractionCell, "lectern");
            if (!report.Accepted)
                return report;

            return RitualPawnEligibilityUtility.ValidateReachAndReserve(pawn, circle, PathEndMode.Touch, "ritual center");
        }

        internal bool IsEligibleAudience(Pawn pawn)
        {
            return ValidateAudience(pawn).Accepted;
        }

        internal AcceptanceReport ValidateAudience(Pawn pawn)
        {
            return RitualPawnEligibilityUtility.ValidateAudiencePawn(pawn);
        }

        public static SpectralEntity ManifestSpectre(Map map, Thing source, Thing circle, Pawn sourcePawn = null, string sourceName = null, Ideo sourceIdeo = null, Pawn summoner = null)
        {
            if (map == null || circle == null)
                return null;

            MapComponent_SpectralEntities comp = map.GetComponent<MapComponent_SpectralEntities>();
            if (comp == null)
            {
                Messages.Message("Could not find the spectral entity tracker for this map.", source ?? circle, MessageTypeDefOf.RejectInput, historical: false);
                return null;
            }

            SpectralEntity spirit = new SpectralEntity(map)
            {
                label = "Spectre of " + (sourceName.NullOrEmpty() ? "the dead" : sourceName),
                state = SpectralState.WanderingUnseen,
                anchorPosition = circle.Position,
                lastKnownPosition = ResolveManifestCell(map, circle, source),
                pawnKind = PawnKindDefOf.Colonist,
                faction = null,
                persistentPawn = true,
                persistentManifestation = true,
                intermittentManifestation = false,
                riteBoundSpectre = true,
                boundSummoner = summoner,
                boundSummonerThingId = summoner?.ThingID,
                sourcePawn = sourcePawn,
                sourcePawnThingId = sourcePawn?.ThingID,
                sourceMemoryId = sourcePawn?.ThingID,
                sourceIdeo = sourceIdeo
            };

            comp.AddSpirit(spirit);
            spirit.ManifestPersistent();
            return spirit;
        }

        private static IntVec3 ResolveManifestCell(Map map, Thing circle, Thing fallback)
        {
            if (IsValidManifestCell(map, circle.Position))
                return circle.Position;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(circle.Position, 3f, true).OrderBy(cell => cell.DistanceToSquared(circle.Position)))
            {
                if (IsValidManifestCell(map, cell))
                    return cell;
            }

            return fallback?.InteractionCell ?? circle.Position;
        }

        private static bool IsValidManifestCell(Map map, IntVec3 cell)
        {
            return cell.IsValid &&
                   cell.InBounds(map) &&
                   cell.Standable(map) &&
                   cell.GetFirstPawn(map) == null;
        }
    }
}
