using System.Collections.Generic;
using MagicFramework.Conditions;
using MagicFramework.Targeting;
using Verse;

namespace MagicFramework.Definitions;

/// <summary>
/// Returns true when every child condition passes.
/// </summary>
public sealed class AllOfConditionDef : SpellConditionDef
{
    public List<SpellConditionDef> conditions = new();

    public override IEnumerable<SpellConditionDef> GetChildConditions() => conditions;

    public override SpellConditionWorker CreateWorker() => new AllOfConditionWorker();
}

/// <summary>
/// Returns true when any child condition passes.
/// </summary>
public sealed class AnyOfConditionDef : SpellConditionDef
{
    public List<SpellConditionDef> conditions = new();

    public override IEnumerable<SpellConditionDef> GetChildConditions() => conditions;

    public override SpellConditionWorker CreateWorker() => new AnyOfConditionWorker();
}

/// <summary>
/// Inverts the result of a child condition.
/// </summary>
public sealed class NotConditionDef : SpellConditionDef
{
    public SpellConditionDef condition;

    public override IEnumerable<SpellConditionDef> GetChildConditions()
    {
        if (condition != null)
        {
            yield return condition;
        }
    }

    public override SpellConditionWorker CreateWorker() => new NotConditionWorker();
}

/// <summary>
/// Returns true when the requested target source resolves to a valid target.
/// </summary>
public sealed class TargetExistsConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;

    public override SpellConditionWorker CreateWorker() => new TargetExistsConditionWorker();
}

/// <summary>
/// Returns true when the requested target source resolves to a pawn.
/// </summary>
public sealed class TargetIsPawnConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;

    public override SpellConditionWorker CreateWorker() => new TargetIsPawnConditionWorker();
}

/// <summary>
/// Returns true when the requested pawn target matches the authored affinity.
/// </summary>
public sealed class PawnAffinityConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.Foe;

    public override SpellConditionWorker CreateWorker() => new PawnAffinityConditionWorker();
}

/// <summary>
/// Returns true when the requested pawn target has the authored hediff.
/// </summary>
public sealed class HasHediffConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;
    public string hediffDef;

    public override SpellConditionWorker CreateWorker() => new HasHediffConditionWorker();
}

/// <summary>
/// Returns true when the requested pawn target is below the configured health percentage threshold.
/// </summary>
public sealed class HealthBelowConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;
    public float thresholdPercent = 0.5f;

    public override SpellConditionWorker CreateWorker() => new HealthBelowConditionWorker();
}

/// <summary>
/// Returns true when the requested pawn target is downed.
/// </summary>
public sealed class TargetDownedConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;

    public override SpellConditionWorker CreateWorker() => new TargetDownedConditionWorker();
}

/// <summary>
/// Returns true when the requested pawn target is dead.
/// </summary>
public sealed class TargetDeadConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;

    public override SpellConditionWorker CreateWorker() => new TargetDeadConditionWorker();
}

/// <summary>
/// Returns true when the requested pawn target is authored as undead.
/// </summary>
public sealed class TargetUndeadConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;

    public override SpellConditionWorker CreateWorker() => new TargetUndeadConditionWorker();
}

/// <summary>
/// Returns true when the requested cell contains at least one thing.
/// </summary>
public sealed class CellOccupiedConditionDef : SpellConditionDef
{
    public SpellConditionCellSource cellSource = SpellConditionCellSource.CurrentCell;

    public override SpellConditionWorker CreateWorker() => new CellOccupiedConditionWorker();
}

/// <summary>
/// Returns true when two authored cell sources are within the configured distance bounds.
/// </summary>
public sealed class DistanceConditionDef : SpellConditionDef
{
    public SpellConditionCellSource fromCellSource = SpellConditionCellSource.CasterCell;
    public SpellConditionCellSource toCellSource = SpellConditionCellSource.CurrentCell;
    public float minDistance = -1f;
    public float maxDistance = -1f;

    public override SpellConditionWorker CreateWorker() => new DistanceConditionWorker();
}

/// <summary>
/// Returns true when two authored cell sources have direct line of sight.
/// </summary>
public sealed class LineOfSightConditionDef : SpellConditionDef
{
    public SpellConditionCellSource fromCellSource = SpellConditionCellSource.CasterCell;
    public SpellConditionCellSource toCellSource = SpellConditionCellSource.CurrentCell;

    public override SpellConditionWorker CreateWorker() => new LineOfSightConditionWorker();
}

/// <summary>
/// Returns true when the requested target thing matches the authored thing category.
/// </summary>
public sealed class ThingCategoryConditionDef : SpellConditionDef
{
    public SpellConditionTargetSource targetSource = SpellConditionTargetSource.CurrentTarget;
    public ThingCategory category = ThingCategory.Item;

    public override SpellConditionWorker CreateWorker() => new ThingCategoryConditionWorker();
}

/// <summary>
/// Returns true with the authored random chance.
/// </summary>
public sealed class RandomChanceConditionDef : SpellConditionDef
{
    public float chance = 1f;

    public override SpellConditionWorker CreateWorker() => new RandomChanceConditionWorker();
}

/// <summary>
/// Returns true when the current cast's spell power tier is within the configured bounds.
/// </summary>
public sealed class PowerTierConditionDef : SpellConditionDef
{
    public int minTier = int.MinValue;
    public int maxTier = int.MaxValue;

    public override SpellConditionWorker CreateWorker() => new PowerTierConditionWorker();
}

/// <summary>
/// Returns true when the current cast's spell power value is within the configured bounds.
/// </summary>
public sealed class SpellPowerConditionDef : SpellConditionDef
{
    public float minPower = float.MinValue;
    public float maxPower = float.MaxValue;

    public override SpellConditionWorker CreateWorker() => new SpellPowerConditionWorker();
}
