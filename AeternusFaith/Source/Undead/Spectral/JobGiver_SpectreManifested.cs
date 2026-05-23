using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith.Undead.Spectral
{
    public class JobGiver_SpectreManifested : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn?.Map == null)
                return null;

            if (!MapComponent_SpectralEntities.TryGetSpiritForManifestedPawn(pawn, out SpectralEntity spirit))
                return RestJob(pawn);

            if (spirit.persistentManifestation && spirit.boundSummoner?.Spawned == true && spirit.boundSummoner.Map == pawn.Map && Rand.Chance(0.18f))
                return DriftToCell(pawn, spirit.boundSummoner.Position, 4f);

            if (Rand.Chance(0.18f))
                return DriftToCell(pawn, spirit.anchorPosition, 7f);

            return RestJob(pawn);
        }

        private static Job RestJob(Pawn pawn)
        {
            JobDef jobDef = DefDatabase<JobDef>.GetNamedSilentFail("AF_SpectreRest") ?? JobDefOf.Wait_Wander;
            Job job = JobMaker.MakeJob(jobDef, pawn.Position);
            job.expiryInterval = Rand.RangeInclusive(900, 2500);
            return job;
        }

        private static Job DriftToCell(Pawn pawn, IntVec3 center, float radius)
        {
            if (!center.IsValid || !center.InBounds(pawn.Map))
                return RestJob(pawn);

            IntVec3 destination = ResolveDriftCell(pawn, center, radius);
            if (!destination.IsValid)
                return RestJob(pawn);

            Job job = JobMaker.MakeJob(JobDefOf.GotoWander, destination);
            job.locomotionUrgency = LocomotionUrgency.Amble;
            job.expiryInterval = Rand.RangeInclusive(800, 1800);
            return job;
        }

        private static IntVec3 ResolveDriftCell(Pawn pawn, IntVec3 center, float radius)
        {
            if (IsValidDriftCell(pawn, center))
                return center;

            IntVec3 best = IntVec3.Invalid;
            float bestDistance = float.MaxValue;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!IsValidDriftCell(pawn, cell))
                    continue;

                float distance = cell.DistanceToSquared(pawn.Position);
                if (distance < bestDistance)
                {
                    best = cell;
                    bestDistance = distance;
                }
            }

            return best;
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
