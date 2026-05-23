using Verse;

namespace AeternusFaith
{
    public static class RitualCorpseEligibilityUtility
    {
        public static AcceptanceReport ValidateHumanlikeMortalCorpse(Corpse corpse, Map map)
        {
            AcceptanceReport basic = ValidateAnyCorpseOnMap(corpse, map);
            if (!basic.Accepted)
                return basic;

            Pawn pawn = corpse.InnerPawn;
            if (pawn?.RaceProps?.Humanlike != true)
                return "The selected corpse must be humanlike.";

            if (SkeletonUndeadUtility.IsUndeadRace(pawn) || SkeletonUndeadUtility.IsUndead(pawn))
                return "The selected corpse is already undead.";

            return true;
        }

        public static AcceptanceReport ValidateAnyCorpseOnMap(Corpse corpse, Map map)
        {
            if (corpse == null)
                return "Select a corpse.";

            if (corpse.Destroyed)
                return "The selected corpse is no longer available.";

            if (!corpse.Spawned || corpse.Map != map)
                return "The selected corpse must be spawned on this map.";

            if (corpse.InnerPawn == null)
                return "The selected corpse has no recoverable body.";

            return true;
        }

        public static bool IsValidHumanlikeMortalCorpse(Corpse corpse, Map map)
        {
            return ValidateHumanlikeMortalCorpse(corpse, map).Accepted;
        }
    }
}
