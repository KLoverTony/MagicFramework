using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MagicFramework.Execution;

public static class SpellEnhancementUtility
{
    public static IEnumerable<SpellEnhancementRuleDef> GetActiveRules(SpellDef spell, Map map)
    {
        List<SpellEnhancementRuleDef> rules = DefDatabase<SpellEnhancementRuleDef>.AllDefsListForReading;
        if (rules == null)
        {
            yield break;
        }

        for (int i = 0; i < rules.Count; i++)
        {
            SpellEnhancementRuleDef rule = rules[i];
            if (RuleMatches(rule, spell, map))
            {
                yield return rule;
            }
        }
    }

    public static SpellModifierSet GetModifiers(SpellContext context)
    {
        SpellModifierSet modifiers = new();
        SpellDef spell = context?.spellDef;
        Map map = context?.map ?? context?.caster?.Map;
        foreach (SpellEnhancementRuleDef rule in GetActiveRules(spell, map))
        {
            modifiers.Apply(rule);
        }

        return modifiers;
    }

    public static float ResolveManaCost(SpellContext context, float baseAmount)
    {
        float factor = GetModifiers(context).manaCostFactor * SpellPowerUtility.ResolveManaCostScalar(context);
        return Mathf.Max(0f, baseAmount * factor);
    }

    public static float ResolveDamageAmount(SpellContext context, float baseAmount)
    {
        float factor = GetModifiers(context).damageFactor * SpellPowerUtility.ResolveDamageScalar(context);
        return Mathf.Max(0f, baseAmount * factor);
    }

    public static float ResolveScalableDamageAmount(SpellContext context, float fallbackValue, ScalableFloatDef scalableFloat)
    {
        return ResolveDamageAmount(context, SpellPowerUtility.ResolveScalableFloat(context, fallbackValue, scalableFloat));
    }

    public static float ResolveHealingAmount(SpellContext context, float baseAmount)
    {
        float factor = SpellPowerUtility.ResolveHealingScalar(context);
        return Mathf.Max(0f, baseAmount * factor);
    }

    public static float ResolveScalableHealingAmount(SpellContext context, float fallbackValue, ScalableFloatDef scalableFloat)
    {
        return ResolveHealingAmount(context, SpellPowerUtility.ResolveScalableFloat(context, fallbackValue, scalableFloat));
    }

    public static float ResolveRadius(SpellContext context, float baseRadius)
    {
        float factor = GetModifiers(context).radiusFactor * SpellPowerUtility.ResolveRadiusScalar(context);
        return Mathf.Max(0f, baseRadius * factor);
    }

    public static float ResolveScalableRadius(SpellContext context, float fallbackValue, ScalableFloatDef scalableFloat)
    {
        return ResolveRadius(context, SpellPowerUtility.ResolveScalableFloat(context, fallbackValue, scalableFloat));
    }

    public static int ResolveDurationTicks(SpellContext context, int baseTicks)
    {
        if (baseTicks <= 0)
        {
            return baseTicks;
        }

        float factor = GetModifiers(context).durationFactor * SpellPowerUtility.ResolveDurationScalar(context);
        return Mathf.Max(1, Mathf.RoundToInt(baseTicks * factor));
    }

    public static int ResolveScalableDurationTicks(SpellContext context, int fallbackValue, ScalableFloatDef scalableFloat)
    {
        return ResolveDurationTicks(context, SpellPowerUtility.ResolveScalableInt(context, fallbackValue, scalableFloat));
    }

    public static int ResolveCooldownTicks(SpellContext context, int baseTicks)
    {
        float factor = GetModifiers(context).cooldownFactor * SpellPowerUtility.ResolveCooldownScalar(context);
        return Mathf.Max(0, Mathf.RoundToInt(baseTicks * factor));
    }

    private static bool RuleMatches(SpellEnhancementRuleDef rule, SpellDef spell, Map map)
    {
        if (rule == null || spell == null)
        {
            return false;
        }

        return MatchesAny(rule.affectedElements, spell.meta?.elements)
            && MatchesAny(rule.affectedDomains, spell.meta?.domains)
            && MatchesAny(rule.affectedDisciplines, spell.meta?.disciplines)
            && MatchesAll(rule.requiredTags, spell.meta?.tags)
            && MatchesAnyActiveCondition(rule.activeDuringConditions, map)
            && MatchesAnyActiveWeather(rule.activeDuringWeather, map)
            && MatchesAnyHilliness(rule.activeOnHilliness, map);
    }

    private static bool MatchesAny<TDef>(List<TDef> filterDefs, List<TDef> spellDefs)
        where TDef : Def
    {
        if (filterDefs == null || filterDefs.Count == 0)
        {
            return true;
        }

        if (spellDefs == null || spellDefs.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < filterDefs.Count; i++)
        {
            TDef filterDef = filterDefs[i];
            if (filterDef != null && spellDefs.Contains(filterDef))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesAll<TDef>(List<TDef> requiredDefs, List<TDef> spellDefs)
        where TDef : Def
    {
        if (requiredDefs == null || requiredDefs.Count == 0)
        {
            return true;
        }

        if (spellDefs == null || spellDefs.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < requiredDefs.Count; i++)
        {
            TDef requiredDef = requiredDefs[i];
            if (requiredDef != null && !spellDefs.Contains(requiredDef))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesAnyActiveCondition(List<GameConditionDef> conditionDefs, Map map)
    {
        if (conditionDefs == null || conditionDefs.Count == 0)
        {
            return true;
        }

        if (map?.gameConditionManager == null)
        {
            return false;
        }

        for (int i = 0; i < conditionDefs.Count; i++)
        {
            GameConditionDef conditionDef = conditionDefs[i];
            if (conditionDef != null && map.gameConditionManager.ConditionIsActive(conditionDef))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesAnyActiveWeather(List<WeatherDef> weatherDefs, Map map)
    {
        if (weatherDefs == null || weatherDefs.Count == 0)
        {
            return true;
        }

        WeatherDef currentWeather = map?.weatherManager?.curWeather;
        if (currentWeather == null)
        {
            return false;
        }

        for (int i = 0; i < weatherDefs.Count; i++)
        {
            if (weatherDefs[i] != null && weatherDefs[i] == currentWeather)
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesAnyHilliness(List<Hilliness> hillinessValues, Map map)
    {
        if (hillinessValues == null || hillinessValues.Count == 0)
        {
            return true;
        }

        if (map?.TileInfo == null)
        {
            return false;
        }

        Hilliness mapHilliness = map.TileInfo.hilliness;
        for (int i = 0; i < hillinessValues.Count; i++)
        {
            if (hillinessValues[i] == mapHilliness)
            {
                return true;
            }
        }

        return false;
    }
}
