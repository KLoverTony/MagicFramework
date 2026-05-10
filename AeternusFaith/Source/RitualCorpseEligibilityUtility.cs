using Verse;

namespace AeternusFaith
{
    public static class RitualCorpseEligibilityUtility
    {
        public static bool IsValidHumanlikeMortalCorpse(Corpse corpse, Map map)
        {
            if (corpse == null || corpse.Destroyed || !corpse.Spawned || corpse.Map != map)
                return false;

            Pawn pawn = corpse.InnerPawn;
            if (pawn?.RaceProps?.Humanlike != true)
                return false;

            return !SkeletonUndeadUtility.IsUndeadRace(pawn) && !SkeletonUndeadUtility.IsUndead(pawn);
        }
    }
}
