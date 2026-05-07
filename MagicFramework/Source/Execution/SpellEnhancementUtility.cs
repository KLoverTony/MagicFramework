using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using RimWorld;
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
        float factor = GetModifiers(context).manaCostFactor;
        return Mathf.Max(0f, baseAmount * factor);
    }

    public static int ResolveCooldownTicks(SpellContext context, int baseTicks)
    {
        float factor = GetModifiers(context).cooldownFactor;
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
            && MatchesAnyActiveCondition(rule.activeDuringConditions, map);
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
}
