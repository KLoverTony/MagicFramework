using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Spawns wall markers and registers periodic wall hazard zones.
/// </summary>
public sealed class PersistentWallZoneService
{
    public void CreateWallZone(SpellContext context, PersistentWallZoneActionDef actionDef)
    {
        if (context?.map == null || context.spellDef == null || actionDef == null || !context.currentCell.IsValid)
        {
            return;
        }

        ThingDef markerDef = ResolveMarkerThingDef(actionDef);
        if (markerDef == null)
        {
            Log.Warning($"[MagicFramework] Failed to create wall zone because marker def '{actionDef.markerThingDef ?? "<null>"}' could not be resolved.");
            return;
        }

        List<IntVec3> wallCells = SpellWallUtility.BuildWallCells(context, context.currentCell, actionDef.wallLength);
        if (wallCells.Count == 0)
        {
            Log.Warning("[MagicFramework] Failed to create wall zone because no valid wall cells were resolved.");
            return;
        }

        List<Thing> markerThings = new();
        foreach (IntVec3 wallCell in wallCells)
        {
            Thing markerThing = ThingMaker.MakeThing(markerDef);
            if (markerThing == null)
            {
                continue;
            }

            GenSpawn.Spawn(markerThing, wallCell, context.map);
            markerThings.Add(markerThing);
        }

        if (markerThings.Count == 0)
        {
            Log.Warning($"[MagicFramework] Failed to spawn wall zone markers for {markerDef.defName}.");
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        int expireAtTick = ResolveExpireTick(context, currentTick, actionDef);
        if (!SpellActionPathUtility.TryCreatePath(context.spellDef, actionDef, out var actionPath))
        {
            foreach (Thing markerThing in markerThings)
            {
                if (markerThing != null && !markerThing.Destroyed)
                {
                    markerThing.Destroy();
                }
            }

            Log.Warning($"[MagicFramework] Failed to create wall zone because its action path in {context.spellDef.defName} could not be resolved.");
            return;
        }

        PersistentWallZone wallZone = new(
            context.caster,
            context.spellDef,
            context.currentCell,
            wallCells,
            markerThings,
            context.randomSeed,
            context.executionState?.variables,
            actionPath,
            actionDef.pawnAffinity,
            actionDef.includeCaster,
            actionDef.pulseRadius,
            actionDef.pulseIntervalTicks,
            expireAtTick);

        PersistentWallZoneMapComponent runtime = context.map.GetComponent<PersistentWallZoneMapComponent>();
        if (runtime == null || !runtime.Register(wallZone, actionDef.replaceExistingForCaster))
        {
            wallZone.DestroyMarkers();
            Log.Warning($"[MagicFramework] Failed to register wall zone {context.spellDef.defName} because the map runtime was unavailable.");
            return;
        }

        Log.Message($"[MagicFramework] Created wall zone for {context.spellDef.defName} with {wallCells.Count} cells.");
    }

    private static ThingDef ResolveMarkerThingDef(PersistentWallZoneActionDef actionDef)
    {
        if (string.IsNullOrWhiteSpace(actionDef.markerThingDef))
        {
            return null;
        }

        return DefDatabase<ThingDef>.GetNamedSilentFail(actionDef.markerThingDef);
    }

    private static int ResolveExpireTick(SpellContext context, int currentTick, PersistentWallZoneActionDef actionDef)
    {
        int durationTicks = SpellPowerUtility.ResolveScalableInt(context, actionDef.durationTicks, actionDef.scalableDurationTicks);
        int failsafeDurationTicks = SpellPowerUtility.ResolveScalableInt(context, actionDef.failsafeDurationTicks, actionDef.scalableFailsafeDurationTicks);
        int durationTick = durationTicks > 0 ? currentTick + durationTicks : -1;
        int failsafeTick = failsafeDurationTicks > 0 ? currentTick + failsafeDurationTicks : -1;

        if (durationTick >= 0 && failsafeTick >= 0)
        {
            return durationTick < failsafeTick ? durationTick : failsafeTick;
        }

        return durationTick >= 0 ? durationTick : failsafeTick;
    }
}
