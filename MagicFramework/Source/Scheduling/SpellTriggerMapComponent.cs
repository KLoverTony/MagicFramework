using System.Collections.Generic;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Targeting;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Owns armed proximity triggers for a single map and fires them when valid pawns enter range.
/// </summary>
public sealed class SpellTriggerMapComponent : MapComponent
{
    private readonly SpellActionRunner actionRunner = new();
    private readonly PersistentSpellEffectService persistentEffectService = new();
    private List<ArmedSpellTrigger> armedTriggers = new();

    public SpellTriggerMapComponent(Map map)
        : base(map)
    {
    }

    public bool Register(ArmedSpellTrigger trigger, bool replaceExistingForCaster)
    {
        if (trigger == null)
        {
            return false;
        }

        armedTriggers ??= new List<ArmedSpellTrigger>();

        if (replaceExistingForCaster)
        {
            for (int i = armedTriggers.Count - 1; i >= 0; i--)
            {
                ArmedSpellTrigger existingTrigger = armedTriggers[i];
                if (existingTrigger?.Caster == trigger.Caster
                    && existingTrigger.SpellDef == trigger.SpellDef)
                {
                    RunTriggerLifecycleActions(existingTrigger, TriggerLifecycleEvent.Remove);
                    armedTriggers.RemoveAt(i);
                }
            }
        }

        armedTriggers.Add(trigger);
        RunTriggerLifecycleActions(trigger, TriggerLifecycleEvent.Create);
        return true;
    }

    public override void MapComponentTick()
    {
        if (armedTriggers == null || armedTriggers.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = armedTriggers.Count - 1; i >= 0; i--)
        {
            ArmedSpellTrigger trigger = armedTriggers[i];
            if (trigger == null || trigger.ArmedCell == IntVec3.Invalid)
            {
                RunTriggerLifecycleActions(trigger, TriggerLifecycleEvent.Break);
                armedTriggers.RemoveAt(i);
                continue;
            }

            if (trigger.Caster != null && trigger.Caster.Destroyed)
            {
                RunTriggerLifecycleActions(trigger, TriggerLifecycleEvent.Break);
                persistentEffectService.RemovePersistentEffectsForCasterSpell(map, trigger.Caster, trigger.SpellDef);
                armedTriggers.RemoveAt(i);
                continue;
            }

            if (trigger.NextCheckTick > currentTick)
            {
                continue;
            }

            Pawn triggeringPawn = FindTriggeringPawn(trigger);
            if (triggeringPawn == null)
            {
                trigger.ScheduleNextCheck();
                continue;
            }

            armedTriggers.RemoveAt(i);
            ExecuteTrigger(trigger, triggeringPawn);
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref armedTriggers, "armedTriggers", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && armedTriggers == null)
        {
            armedTriggers = new List<ArmedSpellTrigger>();
        }
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new();
        builder.Append("[MagicFramework] Trigger runtime for map ");
        builder.Append(map?.Index ?? -1);
        builder.Append(": ");
        builder.Append(armedTriggers?.Count ?? 0);
        builder.Append(" armed trigger(s).");

        if (armedTriggers == null || armedTriggers.Count == 0)
        {
            return builder.ToString();
        }

        for (int i = 0; i < armedTriggers.Count; i++)
        {
            ArmedSpellTrigger trigger = armedTriggers[i];
            builder.AppendLine();
            builder.Append("  [");
            builder.Append(i);
            builder.Append("] cell=");
            builder.Append(trigger?.ArmedCell ?? IntVec3.Invalid);
            builder.Append(" radius=");
            builder.Append(trigger?.TriggerRadius ?? 0f);
            builder.Append(" spell=");
            builder.Append(trigger?.SpellDef?.defName ?? "<null>");
            builder.Append(" action=");
            builder.Append(trigger?.DebugLabel ?? "<null>");
        }

        return builder.ToString();
    }

    private Pawn FindTriggeringPawn(ArmedSpellTrigger trigger)
    {
        List<Thing> things = map?.listerThings?.AllThings;
        if (things == null)
        {
            return null;
        }

        Thing caster = trigger.Caster;
        for (int i = 0; i < things.Count; i++)
        {
            if (things[i] is not Pawn pawn || pawn.Destroyed)
            {
                continue;
            }

            if (!trigger.IncludeCaster && pawn == caster)
            {
                continue;
            }

            if (!TargetQueryUtility.MatchesPawnAffinity(caster, pawn, trigger.PawnAffinity))
            {
                continue;
            }

            if (pawn.Position.DistanceTo(trigger.ArmedCell) <= trigger.TriggerRadius)
            {
                return pawn;
            }
        }

        return null;
    }

    private void ExecuteTrigger(ArmedSpellTrigger trigger, Pawn triggeringPawn)
    {
        if (!trigger.TryResolveActionDef(out ProximityTriggerActionDef actionDef))
        {
            Log.Warning("[MagicFramework] Dropped armed trigger because its authored node could not be resolved.");
            return;
        }

        if (!trigger.TryCreateExecutionContext(map, triggeringPawn, out SpellContext context))
        {
            Log.Warning($"[MagicFramework] Dropped armed trigger {trigger.DebugLabel} because its execution context could not be rebuilt.");
            return;
        }

        if (actionDef.removePersistentEffectsForCasterSpell)
        {
            persistentEffectService.RemovePersistentEffectsForCasterSpell(map, trigger.Caster, trigger.SpellDef);
        }

        MagicLog.Message(MagicLogSubsystem.Triggers, $"[MagicFramework] Triggered armed spell {trigger.DebugLabel} at {trigger.ArmedCell} from {trigger.SpellDef?.defName ?? "<null>"}.");
        RunTriggerLifecycleActions(trigger, TriggerLifecycleEvent.Trigger, actionDef, triggeringPawn);
        actionRunner.RunActions(context, actionDef.actions);
    }

    private void RunTriggerLifecycleActions(
        ArmedSpellTrigger trigger,
        TriggerLifecycleEvent lifecycleEvent,
        ProximityTriggerActionDef resolvedActionDef = null,
        Pawn triggeringPawn = null)
    {
        if (trigger == null ||
            (resolvedActionDef == null && !trigger.TryResolveActionDef(out resolvedActionDef)) ||
            !trigger.TryCreateExecutionContext(map, triggeringPawn, out SpellContext context))
        {
            return;
        }

        List<SpellActionDef> specificActions = lifecycleEvent switch
        {
            TriggerLifecycleEvent.Create => resolvedActionDef.onCreateActions,
            TriggerLifecycleEvent.Trigger => resolvedActionDef.onTriggerActions,
            TriggerLifecycleEvent.Remove => resolvedActionDef.onRemoveActions,
            TriggerLifecycleEvent.Break => resolvedActionDef.onBreakActions,
            _ => null
        };

        if (specificActions != null && specificActions.Count > 0)
        {
            actionRunner.RunActions(context, specificActions);
        }
    }

    private enum TriggerLifecycleEvent
    {
        Create,
        Trigger,
        Remove,
        Break
    }
}
