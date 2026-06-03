using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using AeternusFaith.Undead.Spectral;

namespace AeternusFaith
{
    [StaticConstructorOnStartup]
    public static class AeternusFaithHarmony
    {
        static AeternusFaithHarmony()
        {
            Harmony harmony = new Harmony("oracle.aeternuscore");
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

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), typeof(PawnGenerationRequest))]
    public static class PawnGenerator_GeneratePawn_AFSkeletonPatch
    {
        public static bool Prefix(ref PawnGenerationRequest request, ref Pawn __result)
        {
            if (!UndeadPawnFactory.CanHandleKind(request.KindDef))
                return true;

            __result = UndeadPawnFactory.GeneratePawn(request.KindDef, new UndeadPawnCreationOptions
            {
                faction = request.Faction ?? Faction.OfPlayer,
                context = request.Context,
                tile = request.Tile,
                fixedGender = Gender.Male,
                label = request.KindDef.label,
                resetSkills = true,
                forceNoBackstory = true
            });
            return false;
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

    [HarmonyPatch(typeof(ResurrectionUtility), nameof(ResurrectionUtility.TryResurrect), typeof(Pawn), typeof(ResurrectionParams))]
    public static class ResurrectionUtility_TryResurrect_SpectralPatch
    {
        public static void Postfix(Pawn pawn, bool __result)
        {
            if (!__result || pawn == null)
                return;

            MapComponent_SpectralEntities.RemoveSpiritsForSourcePawn(pawn);
        }
    }
}
