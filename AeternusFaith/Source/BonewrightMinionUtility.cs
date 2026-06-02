using System.Collections.Generic;
using MagicFramework.PawnLifecycle;
using Verse;

namespace AeternusFaith
{
    public static class BonewrightMinionUtility
    {
        public static AcceptanceReport ValidateCanBindMinion(Pawn bonewright)
        {
            if (bonewright == null)
                return "No Bonewright selected.";

            return TryFindBoundMinion(bonewright, out Pawn minion)
                ? bonewright.LabelShortCap + " already has a bound undead minion: " + minion.LabelShortCap + "."
                : true;
        }

        public static bool TryFindBoundMinion(Pawn bonewright, out Pawn minion)
        {
            minion = null;
            if (bonewright?.Map?.mapPawns?.AllPawnsSpawned == null)
                return false;

            foreach (Pawn candidate in bonewright.Map.mapPawns.AllPawnsSpawned)
            {
                if (IsBoundMinionOf(candidate, bonewright))
                {
                    minion = candidate;
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<Pawn> BoundMinionsOf(Pawn bonewright)
        {
            if (bonewright?.Map?.mapPawns?.AllPawnsSpawned == null)
                yield break;

            foreach (Pawn candidate in bonewright.Map.mapPawns.AllPawnsSpawned)
            {
                if (IsBoundMinionOf(candidate, bonewright))
                    yield return candidate;
            }
        }

        public static bool IsBoundMinionOf(Pawn candidate, Pawn bonewright)
        {
            if (candidate == null || bonewright == null || candidate == bonewright || candidate.Destroyed || candidate.Dead)
                return false;

            HediffComp_BoundUndeadMinion boundComp = BoundUndeadMinionUtility.GetComp(candidate);
            if (boundComp != null)
                return !boundComp.IsLost && boundComp.Master == bonewright;

            CompPawnLifecycleEnforcer lifecycleComp = candidate.GetComp<CompPawnLifecycleEnforcer>();
            return lifecycleComp?.Master == bonewright;
        }
    }
}
