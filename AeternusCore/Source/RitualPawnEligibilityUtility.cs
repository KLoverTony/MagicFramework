using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public static class RitualPawnEligibilityUtility
    {
        public static AcceptanceReport ValidateAudiencePawn(Pawn pawn)
        {
            if (pawn == null)
                return "Select a pawn.";

            if (pawn.Dead)
                return pawn.LabelShortCap + " is dead.";

            if (pawn.Downed)
                return pawn.LabelShortCap + " is downed.";

            if (!pawn.Spawned)
                return pawn.LabelShortCap + " must be spawned on this map.";

            if (pawn.InMentalState)
                return pawn.LabelShortCap + " is in a mental state.";

            if (pawn.Faction != Faction.OfPlayer)
                return pawn.LabelShortCap + " is not a colony pawn.";

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                return pawn.LabelShortCap + " cannot move.";

            return true;
        }

        public static AcceptanceReport ValidateBonewrightConductor(Pawn pawn)
        {
            AcceptanceReport audience = ValidateAudiencePawn(pawn);
            if (!audience.Accepted)
                return audience;

            if (!BonewrightUtility.IsBonewright(pawn))
                return pawn.LabelShortCap + " must be an anointed Bonewright.";

            if (pawn.WorkTagIsDisabled(WorkTags.ManualDumb))
                return pawn.LabelShortCap + " cannot perform manual ritual work.";

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return pawn.LabelShortCap + " cannot manipulate ritual materials.";

            return true;
        }

        public static AcceptanceReport ValidateReachAndReserve(Pawn pawn, Thing target, PathEndMode pathEndMode, string targetLabel)
        {
            if (pawn == null)
                return "Select a conductor.";

            if (target == null || target.Destroyed)
                return "The " + targetLabel + " is missing.";

            if (!target.Spawned || target.Map != pawn.Map)
                return "The " + targetLabel + " must be on the same map as " + pawn.LabelShortCap + ".";

            if (!pawn.CanReach(target, pathEndMode, Danger.Deadly))
                return pawn.LabelShortCap + " cannot reach the " + targetLabel + ".";

            if (!pawn.CanReserve(target))
                return pawn.LabelShortCap + " cannot reserve the " + targetLabel + ".";

            return true;
        }
    }
}
