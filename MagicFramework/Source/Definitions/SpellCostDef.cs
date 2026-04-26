using MagicFramework.Costs;

namespace MagicFramework.Definitions;

/// <summary>
/// Base authored cost applied after validation succeeds.
/// </summary>
public abstract class SpellCostDef
{
    public string debugLabel;

    public abstract SpellCostWorker CreateWorker();
}
