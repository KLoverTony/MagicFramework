using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    [StaticConstructorOnStartup]
    public static class AeternusFaithHarmony
    {
        static AeternusFaithHarmony()
        {
            Harmony harmony = new Harmony("oracle.aeternusfaith");
            harmony.PatchAll();
            Log.Message("[AeternusFaith] Harmony patches applied.");
        }
    }

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
    public static class PawnInteractionsTracker_TryInteractWith_Patch
    {
        public static bool Prefix(Pawn ___pawn, Pawn recipient, ref bool __result)
        {
            if (SkeletonUndeadUtility.ShouldSuppressSocialInteraction(___pawn, recipient))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(JobGiver_Nuzzle), "TryGiveJob")]
    public static class JobGiverNuzzle_TryGiveJob_Patch
    {
        public static void Postfix(ref Job __result)
        {
            if (__result?.targetA.Thing is Pawn recipient && SkeletonUndeadUtility.IsUndeadRace(recipient))
                __result = null;
        }
    }
}
