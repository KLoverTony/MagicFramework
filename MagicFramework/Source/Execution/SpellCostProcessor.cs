using MagicFramework.Context;
using MagicFramework.Definitions;
using System.Collections.Generic;

namespace MagicFramework.Execution;

/// <summary>
/// Applies all configured cast costs.
/// </summary>
public sealed class SpellCostProcessor
{
    public void ApplyCosts(SpellContext context)
    {
        List<SpellCostDef> costs = ResolveCosts(context?.spellDef);
        if (costs == null)
        {
            return;
        }

        foreach (SpellCostDef costDef in costs)
        {
            if (costDef == null)
            {
                continue;
            }

            costDef.CreateWorker().ApplyCost(context, costDef);
        }

        context.executionState.costsApplied = true;
    }

    private static List<SpellCostDef> ResolveCosts(SpellDef spellDef)
    {
        if (spellDef?.casting?.costs != null && spellDef.casting.costs.Count > 0)
        {
            return spellDef.casting.costs;
        }

        return spellDef?.costs;
    }
}
