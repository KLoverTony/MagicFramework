using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Registers persistent proximity triggers with the per-map runtime.
/// </summary>
public sealed class SpellTriggerService
{
    public void ArmProximityTrigger(SpellContext context, ProximityTriggerActionDef actionDef)
    {
        if (context?.spellDef == null || actionDef == null || context.map == null)
        {
            return;
        }

        if (!SpellActionPathUtility.TryCreatePath(context.spellDef, actionDef, out var actionPath))
        {
            Log.Warning($"[MagicFramework] Failed to arm proximity trigger {actionDef.GetType().Name} because its path in {context.spellDef.defName} could not be resolved.");
            return;
        }

        ArmedSpellTrigger trigger = new(
            context.spellDef,
            context.caster,
            context.initialTarget,
            context.currentTarget,
            context.currentTargets,
            context.currentCell,
            context.randomSeed,
            context.executionState?.variables,
            actionPath,
            actionDef.triggerRadius,
            actionDef.pawnAffinity,
            actionDef.includeCaster,
            actionDef.checkIntervalTicks);

        SpellTriggerMapComponent runtime = context.map.GetComponent<SpellTriggerMapComponent>();
        if (runtime == null || !runtime.Register(trigger, actionDef.replaceExistingForCaster))
        {
            Log.Warning($"[MagicFramework] Failed to arm proximity trigger {actionDef.GetType().Name} because the map trigger runtime was unavailable.");
            return;
        }

        Log.Message($"[MagicFramework] Armed proximity trigger {trigger.DebugLabel} at {context.currentCell} with radius {actionDef.triggerRadius}.");
    }
}
