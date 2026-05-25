using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AeternusFaith.Undead.Spectral
{
    public class JobGiver_SpectreManifested : ThinkNode_JobGiver
    {
        private const float BoundSummonerDriftChance = 0.55f;
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

            if (spirit.persistentManifestation &&
                spirit.boundSummoner?.Spawned == true &&
                spirit.boundSummoner.Map == pawn.Map &&
                Rand.Chance(BoundSummonerDriftChance))
            {
                return DriftToCell(pawn, spirit.boundSummoner.Position, 5f, spirit, "Following summoner.");
            }

            if (Rand.Chance(AnchorDriftChance))
                return DriftToCell(pawn, spirit.anchorPosition, 9f, spirit, "Haunting anchor.");

            return RestJob(pawn, spirit);
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

        private static bool IsValidDriftCell(Pawn pawn, IntVec3 cell)
        {
            return cell.IsValid &&
                   cell.InBounds(pawn.Map) &&
                   cell.Standable(pawn.Map) &&
                   pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly);
        }
    }
}
