using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Costs;

/// <summary>
/// Base worker for applying spell casting costs.
/// </summary>
public abstract class SpellCostWorker
{
    public virtual void ApplyCost(SpellContext context, SpellCostDef costDef)
    {
        Log.Message($"[MagicFramework] Cost worker {GetType().Name} applied a stub cost.");
    }
}
