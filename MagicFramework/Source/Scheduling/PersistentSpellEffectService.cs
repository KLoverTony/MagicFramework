using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Spawns and tracks persistent visible spell markers anchored to the map.
/// </summary>
public sealed class PersistentSpellEffectService
{
    public void CreatePersistentEffect(SpellContext context, PersistentEffectActionDef actionDef)
    {
        if (context?.map == null || context.spellDef == null || actionDef == null || !context.currentCell.IsValid)
        {
            return;
        }

        ThingDef markerDef = ResolveMarkerThingDef(actionDef);
        if (markerDef == null)
        {
            Log.Warning($"[MagicFramework] Failed to create persistent effect because marker def '{actionDef.markerThingDef ?? "<null>"}' could not be resolved.");
            return;
        }

        Thing markerThing = ThingMaker.MakeThing(markerDef);
        if (markerThing == null)
        {
            Log.Warning($"[MagicFramework] Failed to create persistent effect marker {markerDef.defName}.");
            return;
        }

        GenSpawn.Spawn(markerThing, context.currentCell, context.map);

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        int expireAtTick = ResolveExpireTick(context, currentTick, actionDef);
        if (!SpellActionPathUtility.TryCreatePath(context.spellDef, actionDef, out var actionPath))
        {
            markerThing.Destroy();
            Log.Warning($"[MagicFramework] Failed to create persistent effect because its action path in {context.spellDef.defName} could not be resolved.");
            return;
        }

        PersistentSpellEffect persistentEffect = new(
            context.caster,
            context.spellDef,
            markerThing,
            context.currentCell,
            context.randomSeed,
            context.executionState?.variables,
            actionPath,
            expireAtTick);

        PersistentSpellEffectMapComponent runtime = context.map.GetComponent<PersistentSpellEffectMapComponent>();
        if (runtime == null || !runtime.Register(persistentEffect, actionDef.replaceExistingForCaster))
        {
            markerThing.Destroy();
            Log.Warning($"[MagicFramework] Failed to register persistent effect {markerDef.defName} because the map runtime was unavailable.");
            return;
        }

        MagicLog.Message(MagicLogSubsystem.PersistentEffects, $"[MagicFramework] Created persistent effect {markerDef.defName} at {context.currentCell} for {context.spellDef.defName}.");
    }

    public void RemovePersistentEffectsForCasterSpell(Map map, Thing caster, SpellDef spellDef)
    {
        map?.GetComponent<PersistentSpellEffectMapComponent>()?.RemoveForCasterSpell(caster, spellDef);
    }

    private static ThingDef ResolveMarkerThingDef(PersistentEffectActionDef actionDef)
    {
        if (string.IsNullOrWhiteSpace(actionDef.markerThingDef))
        {
            return null;
        }

        return DefDatabase<ThingDef>.GetNamedSilentFail(actionDef.markerThingDef);
    }

    private static int ResolveExpireTick(SpellContext context, int currentTick, PersistentEffectActionDef actionDef)
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
}
