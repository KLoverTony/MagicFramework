using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AeternusFaith.Undead.Spectral
{
    public class JobGiver_SpectreManifested : ThinkNode_JobGiver
    {
        private const float VisitationChance = 0.32f;
        private const float BoundSummonerDriftChance = 0.55f;
        private const float LovedOneDriftChance = 0.28f;
        private const float RivalDriftChance = 0.18f;
        private const float AnchorDriftChance = 0.35f;
        private const float AmbientDriftChance = 0.65f;
        private const float MinimumMoveDistanceSquared = 9f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn?.Map == null)
                return null;

            if (!MapComponent_SpectralEntities.TryGetSpiritForManifestedPawn(pawn, out SpectralEntity spirit))
            {
                if (Rand.Chance(AmbientDriftChance))
                    return DriftToCell(pawn, pawn.Position, 8f, null, "Drifting.");

                return RestJob(pawn, null);
            }

            if (spirit.IsRestless)
                return DriftToCell(pawn, pawn.Position, 11f, spirit, "Wandering restlessly.");

            if (Rand.Chance(VisitationChance) && TryCreateVisitationJob(pawn, spirit, out Job visitationJob))
                return visitationJob;

            if (spirit.persistentManifestation &&
                spirit.boundSummoner?.Spawned == true &&
                spirit.boundSummoner.Map == pawn.Map &&
                Rand.Chance(BoundSummonerDriftChance))
            {
                return DriftToCell(pawn, spirit.boundSummoner.Position, 5f, spirit, "Following summoner.");
            }

            if (Rand.Chance(LovedOneDriftChance) &&
                TryResolveEmotionalTarget(pawn, spirit, SpectralEmotionalAnchorKind.LovedOne, out Pawn lovedOne))
            {
                return DriftToCell(pawn, lovedOne.Position, 5f, spirit, "Remembering " + lovedOne.LabelShort + ".");
            }

            if (Rand.Chance(LovedOneDriftChance) &&
                TryResolveEmotionalTarget(pawn, spirit, SpectralEmotionalAnchorKind.Family, out Pawn familyPawn))
            {
                return DriftToCell(pawn, familyPawn.Position, 5f, spirit, "Remembering " + familyPawn.LabelShort + ".");
            }

            if (Rand.Chance(RivalDriftChance) &&
                TryResolveEmotionalTarget(pawn, spirit, SpectralEmotionalAnchorKind.Rival, out Pawn rival))
            {
                return DriftToCell(pawn, rival.Position, 4f, spirit, "Haunting " + rival.LabelShort + ".");
            }

            if (Rand.Chance(AnchorDriftChance))
                return DriftToCell(pawn, spirit.anchorPosition, 9f, spirit, "Haunting anchor.");

            return RestJob(pawn, spirit);
        }

        private static bool TryCreateVisitationJob(Pawn pawn, SpectralEntity spirit, out Job job)
        {
            job = null;
            if (pawn?.Map == null || spirit == null)
                return false;

            if (TryResolveEmotionalTarget(pawn, spirit, SpectralEmotionalAnchorKind.LovedOne, out Pawn lovedOne) &&
                TryMakeVisitJob(pawn, "AF_SpectralVisitLovedOne", lovedOne.Position, lovedOne, spirit, "Seeking remembered love.", out job))
            {
                return true;
            }

            if (TryResolveEmotionalTarget(pawn, spirit, SpectralEmotionalAnchorKind.Family, out Pawn familyPawn) &&
                TryMakeVisitJob(pawn, "AF_SpectralVisitFamily", familyPawn.Position, familyPawn, spirit, "Seeking remembered family.", out job))
            {
                return true;
            }

            return TryMakeVisitJob(pawn, "AF_SpectralVisitAnchor", spirit.anchorPosition, null, spirit, "Returning to anchor.", out job);
        }

        private static bool TryMakeVisitJob(Pawn pawn, string jobDefName, IntVec3 center, Pawn targetPawn, SpectralEntity spirit, string summary, out Job job)
        {
            job = null;
            JobDef jobDef = DefDatabase<JobDef>.GetNamedSilentFail(jobDefName);
            if (jobDef == null || !center.IsValid || !center.InBounds(pawn.Map))
                return false;

            IntVec3 destination = ResolveDriftCell(pawn, center, targetPawn == null ? 4f : 3f);
            if (!destination.IsValid)
                return false;

            job = targetPawn == null
                ? JobMaker.MakeJob(jobDef, destination)
                : JobMaker.MakeJob(jobDef, destination, targetPawn);
            job.locomotionUrgency = LocomotionUrgency.Amble;
            job.expiryInterval = Rand.RangeInclusive(1600, 2600);
            if (spirit != null)
                spirit.lastActionSummary = summary;
            return true;
        }

        private static Job RestJob(Pawn pawn, SpectralEntity spirit)
        {
            JobDef jobDef = DefDatabase<JobDef>.GetNamedSilentFail("AF_SpectreRest") ?? JobDefOf.Wait_Wander;
            Job job = JobMaker.MakeJob(jobDef, pawn.Position);
            job.expiryInterval = Rand.RangeInclusive(180, 520);
            if (spirit != null)
                spirit.lastActionSummary = "Lingering.";
            return job;
        }

        private static Job DriftToCell(Pawn pawn, IntVec3 center, float radius, SpectralEntity spirit, string summary)
        {
            if (!center.IsValid || !center.InBounds(pawn.Map))
                return RestJob(pawn, spirit);

            IntVec3 destination = ResolveDriftCell(pawn, center, radius);
            if (!destination.IsValid)
                return RestJob(pawn, spirit);

            Job job = JobMaker.MakeJob(JobDefOf.GotoWander, destination);
            job.locomotionUrgency = LocomotionUrgency.Amble;
            job.expiryInterval = Rand.RangeInclusive(450, 1200);
            if (spirit != null)
                spirit.lastActionSummary = summary;
            return job;
        }

        private static IntVec3 ResolveDriftCell(Pawn pawn, IntVec3 center, float radius)
        {
            List<IntVec3> preferredCells = new List<IntVec3>();
            List<IntVec3> fallbackCells = new List<IntVec3>();

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!IsValidDriftCell(pawn, cell))
                    continue;

                float distance = cell.DistanceToSquared(pawn.Position);
                if (distance >= MinimumMoveDistanceSquared)
                    preferredCells.Add(cell);
                else if (cell != pawn.Position)
                    fallbackCells.Add(cell);
            }

            if (preferredCells.Count > 0)
                return preferredCells.RandomElement();

            if (fallbackCells.Count > 0)
                return fallbackCells.RandomElement();

            return IntVec3.Invalid;
        }

        private static bool TryResolveEmotionalTarget(Pawn spectre, SpectralEntity spirit, SpectralEmotionalAnchorKind kind, out Pawn target)
        {
            target = null;
            if (spectre?.Map == null || spirit == null)
                return false;

            if (!spirit.TryGetEmotionalAnchor(spectre.Map, kind, out _, out Pawn resolvedPawn))
                return false;

            if (resolvedPawn == spectre || resolvedPawn.Dead || !resolvedPawn.Spawned || resolvedPawn.Map != spectre.Map)
                return false;

            target = resolvedPawn;
            return true;
        }

        private static bool IsValidDriftCell(Pawn pawn, IntVec3 cell)
        {
            return cell.IsValid &&
                   cell.InBounds(pawn.Map) &&
                   cell.Standable(pawn.Map) &&
                   pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly);
        }
    }
}
