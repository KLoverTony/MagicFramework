using MagicFramework.Costs;

namespace MagicFramework.Definitions;

/// <summary>
/// Stub cost for spending a resource pool.
/// </summary>
public sealed class ManaCostDef : SpellCostDef
{
    public float amount;

    public override SpellCostWorker CreateWorker() => new ManaCostWorker();
}

/// <summary>
/// Stub cost for starting a cooldown.
/// </summary>
public sealed class CooldownCostDef : SpellCostDef
{
    public int cooldownTicks;

    public override SpellCostWorker CreateWorker() => new CooldownCostWorker();
}
