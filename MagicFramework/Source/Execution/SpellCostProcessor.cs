using MagicFramework.Context;
using MagicFramework.Definitions;

namespace MagicFramework.Execution;

/// <summary>
/// Applies all configured cast costs.
/// </summary>
public sealed class SpellCostProcessor
{
    public void ApplyCosts(SpellContext context)
    {
        if (context?.spellDef?.costs == null)
        {
            return;
        }

        foreach (SpellCostDef costDef in context.spellDef.costs)
        {
            if (costDef == null)
            {
                continue;
            }

            costDef.CreateWorker().ApplyCost(context, costDef);
        }

        context.executionState.costsApplied = true;
    }
}
