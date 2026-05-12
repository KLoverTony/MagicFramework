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
