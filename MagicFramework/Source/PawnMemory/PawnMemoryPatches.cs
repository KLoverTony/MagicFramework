using HarmonyLib;
using RimWorld;
using Verse;

namespace MagicFramework.PawnMemory;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
public static class Pawn_SpawnSetup_MemoryPatch
{
    public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
    {
        if (respawningAfterLoad || __instance == null || !__instance.RaceProps.Humanlike) return;

        // Note: If this causes stutter during large raids spawning, we can refactor this
        // to queue the pawn for processing over multiple ticks in WorldComponent_PawnMemories.
        // For MVP, immediate processing is used as it is mostly simple assignment.
        WorldComponent_PawnMemories.Instance?.GetOrCreateMemory(__instance);
    }
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class Pawn_Kill_MemoryPatch
{
    public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
    {
        if (__instance == null || !__instance.RaceProps.Humanlike) return;

        // Note: Data collection on death is done here to capture the final state
        // before the corpse potentially decays or gets modified.
        WorldComponent_PawnMemories.Instance?.NotifyPawnKilled(__instance, dinfo, exactCulprit);
    }
}

[HarmonyPatch(typeof(Corpse), nameof(Corpse.SpawnSetup))]
public static class Corpse_SpawnSetup_MemoryPatch
{
    public static void Postfix(Corpse __instance, Map map, bool respawningAfterLoad)
    {
        if (__instance?.InnerPawn == null || !__instance.InnerPawn.RaceProps.Humanlike) return;

        WorldComponent_PawnMemories.Instance?.RecordCorpseAnchor(__instance);
    }
}

[HarmonyPatch(typeof(Corpse), nameof(Corpse.DeSpawn))]
public static class Corpse_DeSpawn_MemoryPatch
{
    public static void Prefix(Corpse __instance, DestroyMode mode = DestroyMode.Vanish)
    {
        if (__instance?.InnerPawn == null || !__instance.InnerPawn.RaceProps.Humanlike) return;

        WorldComponent_PawnMemories.Instance?.RecordCorpseAnchor(__instance);
    }
}

[HarmonyPatch(typeof(Corpse), nameof(Corpse.Destroy))]
public static class Corpse_Destroy_MemoryPatch
{
    public static void Prefix(Corpse __instance, DestroyMode mode = DestroyMode.Vanish)
    {
        if (__instance?.InnerPawn == null || !__instance.InnerPawn.RaceProps.Humanlike) return;

        WorldComponent_PawnMemories.Instance?.NotifyCorpseDestroyed(__instance);
    }
}

[HarmonyPatch(typeof(ResurrectionUtility), nameof(ResurrectionUtility.TryResurrect), typeof(Pawn), typeof(ResurrectionParams))]
public static class ResurrectionUtility_TryResurrect_MemoryPatch
{
    public static void Postfix(Pawn pawn, bool __result)
    {
        if (!__result || pawn == null || pawn.RaceProps?.Humanlike != true)
            return;

        PawnSoulRiteUtility.NotifyPawnResurrected(pawn);
    }
}
