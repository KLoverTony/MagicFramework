using System.Linq;
using RimWorld;
using Verse;

namespace AeternusFaith
{
    public static class BonewrightUtility
    {
        public const int DefaultMaxBonewrightsPerMap = 5;
        public const string OssanithInitiateHediffDefName = "AF_BonewrightOssanithInitiate";
        private const string SoulwardenRoleDefName = "AET_Role_Soulwarden";

        public static bool IsBonewright(Pawn pawn)
        {
            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(OssanithInitiateHediffDefName);
            return hediffDef != null && pawn?.health?.hediffSet?.HasHediff(hediffDef) == true;
        }

        public static bool CanOfficiateAnointment(Pawn pawn)
        {
            if (!IsAvailableColonist(pawn))
                return false;

            if (IsBonewright(pawn))
                return true;

            if (!ModsConfig.IdeologyActive)
                return true;

            Precept_Role role = pawn.Ideo?.GetRole(pawn);
            return role?.def?.defName == SoulwardenRoleDefName;
        }

        public static bool CanBeAnointed(Pawn pawn, Map map, out string failReason)
        {
            failReason = null;

            if (!IsAvailableColonist(pawn))
            {
                failReason = "Not available for anointment.";
                return false;
            }

            if (pawn.RaceProps?.Humanlike != true)
            {
                failReason = "Only humanlike pawns can be anointed.";
                return false;
            }

            if (SkeletonUndeadUtility.IsUndead(pawn))
            {
                failReason = "Undead pawns cannot be anointed into the order.";
                return false;
            }

            if (IsBonewright(pawn))
            {
                failReason = pawn.LabelShortCap + " is already a Bonewright.";
                return false;
            }

            if (CountBonewrights(map) >= DefaultMaxBonewrightsPerMap)
            {
                failReason = "This map already has the maximum number of Bonewrights.";
                return false;
            }

            return true;
        }

        public static void AnointOssanithInitiate(Pawn pawn)
        {
            if (pawn?.health == null || IsBonewright(pawn))
                return;

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(OssanithInitiateHediffDefName);
            if (hediffDef == null)
            {
                Log.Error("[AeternusFaith] Missing HediffDef " + OssanithInitiateHediffDefName + "; Bonewright anointment cannot be completed.");
                return;
            }

            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            hediff.Severity = 1f;
            pawn.health.AddHediff(hediff);
        }

        public static int CountBonewrights(Map map)
        {
            return map?.mapPawns?.FreeColonistsSpawned?.Count(IsBonewright) ?? 0;
        }

        public static bool IsAvailableColonist(Pawn pawn)
        {
            return pawn != null &&
                   !pawn.Dead &&
                   !pawn.Downed &&
                   pawn.Spawned &&
                   !pawn.InMentalState &&
                   pawn.Faction == Faction.OfPlayer &&
                   pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Moving) == true;
        }
    }
}
