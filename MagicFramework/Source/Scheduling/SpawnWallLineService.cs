using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Creates temporary spell walls as actual buildings on the map.
/// </summary>
public sealed class SpawnWallLineService
{
    public void CreateWallLine(SpellContext context, SpawnWallLineActionDef actionDef)
    {
        if (context?.map == null || actionDef == null)
        {
            return;
        }

        ThingDef wallDef = ResolveThingDef(actionDef);
        if (wallDef == null)
        {
            Log.Warning($"[MagicFramework] SpawnWallLineActionWorker could not resolve thing def '{actionDef.thingDef ?? "<null>"}'.");
            return;
        }

        ThingDef stuffDef = ResolveStuffDef(actionDef);
        if (!string.IsNullOrWhiteSpace(actionDef.stuffDef) && stuffDef == null)
        {
            Log.Warning($"[MagicFramework] SpawnWallLineActionWorker could not resolve stuff def '{actionDef.stuffDef}'.");
            return;
        }

        List<IntVec3> wallCells = SpellWallUtility.BuildWallCells(context, context.currentCell, actionDef.wallLength);
        if (wallCells.Count == 0)
        {
            return;
        }

        int durationTicks = SpellEnhancementUtility.ResolveScalableDurationTicks(context, actionDef.durationTicks, actionDef.scalableDurationTicks);
        int expireAtTick = durationTicks > 0 ? (Find.TickManager?.TicksGame ?? 0) + durationTicks : -1;
        SpellActionPathUtility.TryCreatePath(context.spellDef, actionDef, out List<int> actionPath);

        SpawnedThingMapComponent component = context.map.GetComponent<SpawnedThingMapComponent>();
        if (actionDef.replaceExistingForCasterSpell)
        {
            component?.RemoveForCasterSpell(context.caster, context.spellDef);
        }

        int spawnedCount = 0;
        for (int i = 0; i < wallCells.Count; i++)
        {
            IntVec3 cell = wallCells[i];
            if (!CanPlaceWallAt(cell, context.map, actionDef))
            {
                continue;
            }

            Thing wall = ThingMaker.MakeThing(wallDef, stuffDef);
            wall.SetForbidden(actionDef.forbidden, false);
            if (actionDef.setFactionToCaster && context.caster?.Faction != null && wall.def.CanHaveFaction)
            {
                wall.SetFaction(context.caster.Faction);
            }

            GenSpawn.Spawn(wall, cell, context.map);
            spawnedCount++;

            SpawnedThingRecord record = new(context.caster, context.spellDef, wall, expireAtTick, actionPath);
            if (durationTicks > 0)
            {
                component?.Register(record, false);
            }

            context.SetCurrentTarget(new LocalTargetInfo(wall));
            component?.RunLifecycleActions(record, SpawnedThingLifecycleEvent.Create);
        }

        if (spawnedCount == 0)
        {
            Log.Warning($"[MagicFramework] SpawnWallLineActionWorker could not place any '{wallDef.defName}' cells near {context.currentCell}.");
            return;
        }

        MagicLog.Message(MagicLogSubsystem.Summons, $"[MagicFramework] Spawned {spawnedCount} wall cells at {context.currentCell}.");
    }

    private static bool CanPlaceWallAt(IntVec3 cell, Map map, SpawnWallLineActionDef actionDef)
    {
        if (!cell.IsValid || !cell.InBounds(map))
        {
            return false;
        }

        if (actionDef.requireWalkableCell && !cell.Walkable(map))
        {
            return false;
        }

        if (actionDef.requireStandableCell && !cell.Standable(map))
        {
            return false;
        }

        if (actionDef.requireNoEdifice && cell.GetEdifice(map) != null)
        {
            return false;
        }

        return true;
    }

    private static ThingDef ResolveThingDef(SpawnWallLineActionDef actionDef)
    {
        if (string.IsNullOrWhiteSpace(actionDef?.thingDef))
        {
            return null;
        }

        return DefDatabase<ThingDef>.GetNamedSilentFail(actionDef.thingDef);
    }

    private static ThingDef ResolveStuffDef(SpawnWallLineActionDef actionDef)
    {
        if (string.IsNullOrWhiteSpace(actionDef?.stuffDef))
        {
            return null;
        }

        return DefDatabase<ThingDef>.GetNamedSilentFail(actionDef.stuffDef);
    }
}
