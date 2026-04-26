using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Stub scheduler for delayed spell work.
/// </summary>
public sealed class SpellScheduler
{
    public void Schedule(SpellContext context, int executeAtTick, SpellActionDef actionDef)
    {
        if (context?.spellDef == null || actionDef == null || context.map == null)
        {
            return;
        }

        if (!SpellActionPathUtility.TryCreatePath(context.spellDef, actionDef, out var actionPath))
        {
            Log.Warning($"[MagicFramework] Failed to schedule delayed action {actionDef.GetType().Name} because its path in {context.spellDef.defName} could not be resolved.");
            return;
        }

        ScheduledSpellAction scheduledAction = new(
            executeAtTick,
            context.spellDef,
            context.caster,
            context.initialTarget,
            context.currentTarget,
            context.currentTargets,
            context.currentCell,
            context.power?.value ?? 0f,
            context.power?.tier ?? 0,
            context.randomSeed,
            context.executionState?.variables,
            actionPath);

        DelayedSpellRuntimeMapComponent runtime = context.map.GetComponent<DelayedSpellRuntimeMapComponent>();
        if (runtime == null || !runtime.Enqueue(scheduledAction))
        {
            Log.Warning($"[MagicFramework] Failed to enqueue delayed action {actionDef.GetType().Name} because the map runtime was unavailable.");
            return;
        }

        context.executionState.scheduledActions.Add(scheduledAction);
        Log.Message($"[MagicFramework] Scheduled {scheduledAction.DebugLabel} for tick {executeAtTick}.");
    }

    public void FlushDebugSchedule(SpellContext context)
    {
        if (context?.executionState?.scheduledActions == null || context.executionState.scheduledActions.Count == 0)
        {
            return;
        }

        Log.Message($"[MagicFramework] {context.executionState.scheduledActions.Count} delayed action(s) queued.");
    }
}
