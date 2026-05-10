using System.Collections.Generic;

namespace MagicFramework.Definitions;

/// <summary>
/// Authored rules for computing the spell power available to a cast.
/// </summary>
public sealed class SpellPowerDef
{
    public float baseValue;
    public float casterLevelFactor = 1f;
    public string casterSkillDef;
    public float casterSkillFactor = 1f;
    public SpellPowerScalarDef damageScalar;
    public SpellPowerScalarDef healingScalar;
    public SpellPowerScalarDef radiusScalar;
    public SpellPowerScalarDef durationScalar;
    public SpellPowerScalarDef manaCostScalar;
    public SpellPowerScalarDef cooldownScalar;
    public int defaultTier;
    public List<SpellPowerTierDef> tiers = new();
}

/// <summary>
/// Maps a minimum power value to an authored tier.
/// </summary>
public sealed class SpellPowerTierDef
{
    public float minPower;
    public int tier;
}

/// <summary>
/// Numeric value that can scale from the current cast's spell power.
/// </summary>
public sealed class ScalableFloatDef
{
    public float baseValue;
    public float perPower;
    public float min = float.MinValue;
    public float max = float.MaxValue;
}

/// <summary>
/// Multiplicative scalar that can grow or shrink with the current cast's spell power.
/// </summary>
public sealed class SpellPowerScalarDef
{
    public float baseValue = 1f;
    public float perPower;
    public float min = 0f;
    public float max = float.MaxValue;
}

/// <summary>
/// Selects a ThingDef name when the current cast reaches the authored power tier.
/// </summary>
public sealed class TieredThingDefName
{
    public int minTier;
    public string thingDef;
}
