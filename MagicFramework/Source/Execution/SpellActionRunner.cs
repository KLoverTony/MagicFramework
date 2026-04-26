using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Execution;

/// <summary>
/// Dispatches action defs to their worker implementations.
/// </summary>
public sealed class SpellActionRunner
{
    public void RunRootActions(SpellContext context)
    {
        RunActions(context, context?.spellDef?.actions);
    }

    public void RunActions(SpellContext context, IEnumerable<SpellActionDef> actionDefs)
    {
        if (context == null || actionDefs == null)
        {
            return;
        }

        foreach (SpellActionDef actionDef in actionDefs)
        {
            if (context.executionState.cancelled || context.executionState.failed)
            {
                return;
            }

            if (actionDef == null)
            {
                continue;
            }

            RunAction(context, actionDef);
        }
    }

    public void RunAction(SpellContext context, SpellActionDef actionDef)
    {
        string label = actionDef.debugLabel ?? actionDef.GetType().Name;
        Log.Message($"[MagicFramework] Running action {label}.");
        context.executionState.debugHistory.Add(new(label));
        actionDef.CreateWorker().Execute(context, actionDef, this);
    }
}
