using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Spawns area markers and registers periodic circular area zones.
/// </summary>
public sealed class PersistentAreaZoneService
{
    public void CreateAreaZone(SpellContext context, PersistentAreaZoneActionDef actionDef)
    {
        if (context?.map == null || context.spellDef == null || actionDef == null || !context.currentCell.IsValid)
        {
            return;
        }

        ThingDef markerDef = ResolveMarkerThingDef(actionDef);
        if (markerDef == null)
        {
            Log.Warning($"[MagicFramework] Failed to create area zone because marker def '{actionDef.markerThingDef ?? "<null>"}' could not be resolved.");
            return;
        }

        float zoneRadius = SpellEnhancementUtility.ResolveRadius(context, actionDef.zoneRadius);
        List<Thing> markerThings = SpawnMarkers(context, actionDef, markerDef, zoneRadius);
        if (markerThings.Count == 0)
        {
            Log.Warning($"[MagicFramework] Failed to create area zone because marker thing '{markerDef.defName}' could not be spawned.");
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

            Log.Warning($"[MagicFramework] Failed to create area zone because its action path in {context.spellDef.defName} could not be resolved.");
            return;
        }

        PersistentAreaZone areaZone = new(
            context.caster,
            context.spellDef,
            context.currentCell,
            markerThings,
            context.randomSeed,
            context.power,
            context.executionState?.variables,
            actionPath,
            actionDef.pawnAffinity,
            actionDef.includeCaster,
            zoneRadius,
            actionDef.pulseIntervalTicks,
            actionDef.ambientEffectDef,
            actionDef.ambientSoundDef,
            actionDef.visualPulseIntervalTicks,
            actionDef.emitVisualFromMarkers,
            actionDef.maxVisualMarkersPerPulse,
            actionDef.pulseAtCenter,
            actionDef.requiresConcentration,
            actionDef.breakWhenCasterDowned,
            actionDef.breakWhenCasterStunned,
            actionDef.breakWhenCasterMentalState,
            actionDef.maintenance,
            SpellEnhancementUtility.ResolveManaCost(context, actionDef.sustainedManaCost),
            actionDef.sustainedManaCostIntervalTicks,
            SpellEnhancementUtility.ResolveManaCost(context, actionDef.manaCostPerAffectedPawn),
            expireAtTick);

        PersistentAreaZoneMapComponent runtime = context.map.GetComponent<PersistentAreaZoneMapComponent>();
        if (runtime == null || !runtime.Register(areaZone, actionDef.replaceExistingForCaster))
        {
            areaZone.DestroyMarkers();
            Log.Warning($"[MagicFramework] Failed to register area zone {context.spellDef.defName} because the map runtime was unavailable.");
            return;
        }

        MagicLog.Message(MagicLogSubsystem.AreaZones, $"[MagicFramework] Created area zone for {context.spellDef.defName} at {context.currentCell} with radius {zoneRadius} and {markerThings.Count} marker(s).");
    }

    private static ThingDef ResolveMarkerThingDef(PersistentAreaZoneActionDef actionDef)
    {
        if (string.IsNullOrWhiteSpace(actionDef.markerThingDef))
        {
            return null;
        }

        return DefDatabase<ThingDef>.GetNamedSilentFail(actionDef.markerThingDef);
    }

    private static int ResolveExpireTick(SpellContext context, int currentTick, PersistentAreaZoneActionDef actionDef)
    {
        int durationTicks = SpellEnhancementUtility.ResolveScalableDurationTicks(context, actionDef.durationTicks, actionDef.scalableDurationTicks);
        int failsafeDurationTicks = SpellEnhancementUtility.ResolveScalableDurationTicks(context, actionDef.failsafeDurationTicks, actionDef.scalableFailsafeDurationTicks);
        int durationTick = durationTicks > 0 ? currentTick + durationTicks : -1;
        int failsafeTick = failsafeDurationTicks > 0 ? currentTick + failsafeDurationTicks : -1;

        if (durationTick >= 0 && failsafeTick >= 0)
        {
            return durationTick < failsafeTick ? durationTick : failsafeTick;
        }

        return durationTick >= 0 ? durationTick : failsafeTick;
    }

    private static List<Thing> SpawnMarkers(SpellContext context, PersistentAreaZoneActionDef actionDef, ThingDef markerDef, float zoneRadius)
    {
        List<Thing> markerThings = new();
        foreach (IntVec3 markerCell in BuildMarkerCells(context.currentCell, context.map, zoneRadius))
        {
            Thing markerThing = ThingMaker.MakeThing(markerDef);
            if (markerThing == null)
            {
                continue;
            }

            GenSpawn.Spawn(markerThing, markerCell, context.map);
            markerThings.Add(markerThing);
        }

        return markerThings;
    }

    private static IEnumerable<IntVec3> BuildMarkerCells(IntVec3 centerCell, Map map, float zoneRadius)
    {
        HashSet<IntVec3> cells = new();
        if (map == null || !centerCell.IsValid)
        {
            return cells;
        }

        float innerRadius = zoneRadius - 0.8f;
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(centerCell, zoneRadius + 0.25f, true))
        {
            if (!cell.InBounds(map))
            {
                continue;
            }

            float distance = cell.DistanceTo(centerCell);
            if (distance >= innerRadius && distance <= zoneRadius + 0.35f)
            {
                cells.Add(cell);
            }
        }

        cells.Add(centerCell);
        return cells;
    }
}
