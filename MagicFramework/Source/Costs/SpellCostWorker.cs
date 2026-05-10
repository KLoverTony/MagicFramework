using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;

namespace MagicFramework.Costs;

/// <summary>
/// Base worker for applying spell casting costs.
/// </summary>
public abstract class SpellCostWorker
{
    public virtual void ApplyCost(SpellContext context, SpellCostDef costDef)
    {
        MagicLog.Message(MagicLogSubsystem.Costs, $"[MagicFramework] Cost worker {GetType().Name} applied a stub cost.");
    }
}
