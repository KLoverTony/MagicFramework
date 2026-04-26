using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Creates authored spell-spawned things such as conjured food, wards, or temporary objects.
/// </summary>
public sealed class SpawnedThingService
{
    public void CreateSpawnedThing(SpellContext context, SpawnThingActionDef actionDef)
    {
        if (context?.map == null || actionDef == null)
        {
            return;
        }

        ThingDef thingDef = ResolveThingDef(context, actionDef);
        if (thingDef == null)
        {
            Log.Warning($"[MagicFramework] SpawnThingActionWorker could not resolve thing def '{actionDef.thingDef ?? "<null>"}'.");
            return;
        }

        Thing spawnedThing = ThingMaker.MakeThing(thingDef);
        spawnedThing.stackCount = ResolveStackCount(context, actionDef);
        spawnedThing.SetForbidden(actionDef.forbidden, false);

        if (!GenPlace.TryPlaceThing(spawnedThing, context.currentCell, context.map, ThingPlaceMode.Near, out Thing placedThing))
        {
            Log.Warning($"[MagicFramework] SpawnThingActionWorker could not place '{thingDef.defName}' near {context.currentCell}.");
            return;
        }

        int durationTicks = ResolveDurationTicks(context, actionDef);
        if (durationTicks > 0)
        {
            SpawnedThingMapComponent component = context.map.GetComponent<SpawnedThingMapComponent>();
            component?.Register(
                new SpawnedThingRecord(
                    context.caster,
                    context.spellDef,
                    placedThing,
                    (Find.TickManager?.TicksGame ?? 0) + durationTicks),
                actionDef.replaceExistingForCasterSpell);
        }

        context.SetCurrentTarget(new LocalTargetInfo(placedThing));
        Log.Message($"[MagicFramework] Spawned {placedThing.LabelCap} at {placedThing.Position}.");
    }

    private static ThingDef ResolveThingDef(SpellContext context, SpawnThingActionDef actionDef)
    {
        string thingDefName = ResolveThingDefName(context, actionDef);
        if (string.IsNullOrWhiteSpace(thingDefName))
        {
            return null;
        }

        return DefDatabase<ThingDef>.GetNamedSilentFail(thingDefName);
    }

    private static string ResolveThingDefName(SpellContext context, SpawnThingActionDef actionDef)
    {
        string resolvedThingDef = actionDef?.thingDef;
        int powerTier = context?.power?.tier ?? 0;
        if (actionDef?.tieredThingDefs != null)
        {
            for (int i = 0; i < actionDef.tieredThingDefs.Count; i++)
            {
                TieredThingDefName tieredThingDef = actionDef.tieredThingDefs[i];
                if (tieredThingDef != null && powerTier >= tieredThingDef.minTier && !string.IsNullOrWhiteSpace(tieredThingDef.thingDef))
                {
                    resolvedThingDef = tieredThingDef.thingDef;
                }
            }
        }

        return resolvedThingDef;
    }

    private static int ResolveStackCount(SpellContext context, SpawnThingActionDef actionDef)
    {
        float resolvedStackCount = SpellPowerUtility.ResolveScalableFloat(context, actionDef.stackCount, actionDef.scalableStackCount);
        return Mathf.Max(1, Mathf.RoundToInt(resolvedStackCount));
    }

    private static int ResolveDurationTicks(SpellContext context, SpawnThingActionDef actionDef)
    {
        return SpellPowerUtility.ResolveScalableInt(context, actionDef.durationTicks, actionDef.scalableDurationTicks);
    }
}
