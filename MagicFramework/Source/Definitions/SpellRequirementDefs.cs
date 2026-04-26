using MagicFramework.Requirements;

namespace MagicFramework.Definitions;

/// <summary>
/// Stub requirement for resource-based casting.
/// </summary>
public sealed class ManaRequirementDef : SpellRequirementDef
{
    public float amount;

    public override SpellRequirementWorker CreateWorker() => new ManaRequirementWorker();
}

/// <summary>
/// Stub requirement for cooldown readiness.
/// </summary>
public sealed class CooldownRequirementDef : SpellRequirementDef
{
    public int cooldownTicks;

    public override SpellRequirementWorker CreateWorker() => new CooldownRequirementWorker();
}
