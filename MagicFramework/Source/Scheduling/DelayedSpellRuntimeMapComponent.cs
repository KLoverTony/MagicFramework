using System.Collections.Generic;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Owns delayed spell work for a single map and executes due actions on tick.
/// </summary>
public sealed class DelayedSpellRuntimeMapComponent : MapComponent
{
    private readonly SpellActionRunner actionRunner = new();
    private List<ScheduledSpellAction> scheduledActions = new();

    public DelayedSpellRuntimeMapComponent(Map map)
        : base(map)
    {
    }

    public int ScheduledActionCount => scheduledActions?.Count ?? 0;

    public IReadOnlyList<ScheduledSpellAction> ScheduledActions => scheduledActions;

    public bool Enqueue(ScheduledSpellAction scheduledAction)
    {
        if (scheduledAction == null)
        {
            return false;
        }

        scheduledActions ??= new List<ScheduledSpellAction>();
        int insertIndex = scheduledActions.Count;
        for (int i = 0; i < scheduledActions.Count; i++)
        {
            ScheduledSpellAction existingAction = scheduledActions[i];
            if (existingAction == null || existingAction.ExecuteAtTick > scheduledAction.ExecuteAtTick)
            {
                insertIndex = i;
                break;
            }
        }

        scheduledActions.Insert(insertIndex, scheduledAction);
        return true;
    }

    public override void MapComponentTick()
    {
        if (scheduledActions == null || scheduledActions.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        while (scheduledActions.Count > 0)
        {
            ScheduledSpellAction scheduledAction = scheduledActions[0];
            if (scheduledAction == null)
            {
                scheduledActions.RemoveAt(0);
                continue;
            }

            if (scheduledAction.ExecuteAtTick > currentTick)
            {
                return;
            }

            scheduledActions.RemoveAt(0);
            ExecuteScheduledAction(scheduledAction);
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref scheduledActions, "scheduledActions", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && scheduledActions == null)
        {
            scheduledActions = new List<ScheduledSpellAction>();
        }
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new();
        builder.Append("[MagicFramework] Delayed runtime for map ");
        builder.Append(map?.Index ?? -1);
        builder.Append(": ");
        builder.Append(ScheduledActionCount);
        builder.Append(" queued action(s).");

        if (scheduledActions == null || scheduledActions.Count == 0)
        {
            return builder.ToString();
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = 0; i < scheduledActions.Count; i++)
        {
            ScheduledSpellAction scheduledAction = scheduledActions[i];
            if (scheduledAction == null)
            {
                builder.AppendLine();
                builder.Append("  [");
                builder.Append(i);
                builder.Append("] <null>");
                continue;
            }

            int ticksRemaining = scheduledAction.ExecuteAtTick - currentTick;
            builder.AppendLine();
            builder.Append("  [");
            builder.Append(i);
            builder.Append("] tick=");
            builder.Append(scheduledAction.ExecuteAtTick);
            builder.Append(" remaining=");
            builder.Append(ticksRemaining);
            builder.Append(" spell=");
            builder.Append(scheduledAction.SpellDef?.defName ?? "<null>");
            builder.Append(" caster=");
            builder.Append(scheduledAction.Caster?.LabelCap ?? "<null>");
            builder.Append(" action=");
            builder.Append(scheduledAction.DebugLabel);
        }

        return builder.ToString();
    }

    private void ExecuteScheduledAction(ScheduledSpellAction scheduledAction)
    {
        if (!scheduledAction.TryResolveActionDef(out var actionDef))
        {
            Log.Warning("[MagicFramework] Dropped delayed spell action because its authored node could not be resolved.");
            return;
        }

        if (!scheduledAction.TryCreateExecutionContext(map, out SpellContext context))
        {
            Log.Warning($"[MagicFramework] Dropped delayed action {scheduledAction.DebugLabel} because its execution context could not be rebuilt.");
            return;
        }

        if (context.caster != null && context.caster.Destroyed)
        {
            Log.Message($"[MagicFramework] Skipped delayed action {scheduledAction.DebugLabel} because the caster no longer exists.");
            return;
        }

        Log.Message($"[MagicFramework] Executing delayed action {scheduledAction.DebugLabel}.");
        actionRunner.RunAction(context, actionDef);
    }
}
