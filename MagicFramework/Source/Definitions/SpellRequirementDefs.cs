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

/// <summary>
/// Requires the caster pawn to carry the framework's arcane gift metadata.
/// </summary>
public sealed class ArcaneGiftRequirementDef : SpellRequirementDef
{
    public override SpellRequirementWorker CreateWorker() => new ArcaneGiftRequirementWorker();
}

/// <summary>
/// Requires the caster pawn to have reached an authored caster level.
/// </summary>
public sealed class CasterLevelRequirementDef : SpellRequirementDef
{
    public int minimumLevel = 1;

    public override SpellRequirementWorker CreateWorker() => new CasterLevelRequirementWorker();
}
