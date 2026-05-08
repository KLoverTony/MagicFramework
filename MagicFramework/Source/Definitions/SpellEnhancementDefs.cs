using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MagicFramework.Definitions;

/// <summary>
/// Context-sensitive spell modifier rule matched by metadata and active map conditions.
/// </summary>
public class SpellEnhancementRuleDef : Def
{
    public List<SpellElementDef> affectedElements;
    public List<SpellDomainDef> affectedDomains;
    public List<SpellDisciplineDef> affectedDisciplines;
    public List<SpellTagDef> requiredTags;
    public List<GameConditionDef> activeDuringConditions;
    public List<WeatherDef> activeDuringWeather;
    public List<Hilliness> activeOnHilliness;
    public float damageFactor = 1f;
    public float radiusFactor = 1f;
    public float durationFactor = 1f;
    public float manaCostFactor = 1f;
    public float cooldownFactor = 1f;
}

public sealed class SpellModifierSet
{
    public float damageFactor = 1f;
    public float radiusFactor = 1f;
    public float durationFactor = 1f;
    public float manaCostFactor = 1f;
    public float cooldownFactor = 1f;

    public void Apply(SpellEnhancementRuleDef ruleDef)
    {
        if (ruleDef == null)
        {
            return;
        }

        damageFactor *= PositiveFactor(ruleDef.damageFactor);
        radiusFactor *= PositiveFactor(ruleDef.radiusFactor);
        durationFactor *= PositiveFactor(ruleDef.durationFactor);
        manaCostFactor *= PositiveFactor(ruleDef.manaCostFactor);
        cooldownFactor *= PositiveFactor(ruleDef.cooldownFactor);
    }

    private static float PositiveFactor(float factor)
    {
        return factor > 0f ? factor : 1f;
    }
}
