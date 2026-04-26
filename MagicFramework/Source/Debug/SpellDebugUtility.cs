using MagicFramework.Scheduling;
using Verse;

namespace MagicFramework.Debug;

/// <summary>
/// Debug helpers for inspecting spell framework state in-game.
/// </summary>
public static class SpellDebugUtility
{
    public static void LogDelayedSpellRuntime()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting delayed spell runtime.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            DelayedSpellRuntimeMapComponent runtime = map?.GetComponent<DelayedSpellRuntimeMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have a delayed spell runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    public static void LogArmedSpellTriggers()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting armed spell triggers.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            SpellTriggerMapComponent runtime = map?.GetComponent<SpellTriggerMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have a spell trigger runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    public static void LogPersistentSpellEffects()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting persistent spell effects.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            PersistentSpellEffectMapComponent runtime = map?.GetComponent<PersistentSpellEffectMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have a persistent effect runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    public static void LogWallZones()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting wall zones.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            PersistentWallZoneMapComponent runtime = map?.GetComponent<PersistentWallZoneMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have a wall zone runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    public static void LogAreaZones()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting area zones.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            PersistentAreaZoneMapComponent runtime = map?.GetComponent<PersistentAreaZoneMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have an area zone runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }
}
