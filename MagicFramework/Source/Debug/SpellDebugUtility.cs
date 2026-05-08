using System.Collections.Generic;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Scheduling;
using Verse;

namespace MagicFramework.Debug;

/// <summary>
/// Debug helpers for inspecting spell framework state in-game.
/// </summary>
public static class SpellDebugUtility
{
    public static void LogSpellEnhancementReport(Pawn pawn, SpellDef spellDef)
    {
        if (spellDef == null)
        {
            Log.Message("[MagicFramework] Cannot inspect spell enhancements because the spell was null.");
            return;
        }

        SpellContext context = SpellRequirementUtility.CreatePawnContext(pawn, spellDef);
        Map map = context.map ?? pawn?.Map;
        SpellModifierSet modifiers = SpellEnhancementUtility.GetModifiers(context);
        List<SpellEnhancementRuleDef> activeRules = new(SpellEnhancementUtility.GetActiveRules(spellDef, map));

        StringBuilder builder = new();
        builder.AppendLine($"[MagicFramework] Enhancement report for {spellDef.LabelCap} ({spellDef.defName})");
        builder.AppendLine($"  Caster: {pawn?.LabelShortCap ?? "<none>"}");
        builder.AppendLine($"  Map: {(map == null ? "<none>" : map.Index.ToString())}");
        builder.AppendLine($"  Elements: {FormatDefNames(spellDef.meta?.elements)}");
        builder.AppendLine($"  Domains: {FormatDefNames(spellDef.meta?.domains)}");
        builder.AppendLine($"  Disciplines: {FormatDefNames(spellDef.meta?.disciplines)}");
        builder.AppendLine($"  Tags: {FormatDefNames(spellDef.meta?.tags)}");
        builder.AppendLine("  Final modifiers:");
        builder.AppendLine($"    damageFactor: {modifiers.damageFactor:0.###}");
        builder.AppendLine($"    radiusFactor: {modifiers.radiusFactor:0.###}");
        builder.AppendLine($"    durationFactor: {modifiers.durationFactor:0.###}");
        builder.AppendLine($"    manaCostFactor: {modifiers.manaCostFactor:0.###}");
        builder.AppendLine($"    cooldownFactor: {modifiers.cooldownFactor:0.###}");
        builder.AppendLine("  Active rules:");

        if (activeRules.Count == 0)
        {
            builder.AppendLine("    <none>");
        }
        else
        {
            for (int i = 0; i < activeRules.Count; i++)
            {
                SpellEnhancementRuleDef rule = activeRules[i];
                builder.AppendLine($"    {rule.defName}: {rule.LabelCap}");
                builder.AppendLine($"      affectedElements: {FormatDefNames(rule.affectedElements)}");
                builder.AppendLine($"      affectedDomains: {FormatDefNames(rule.affectedDomains)}");
                builder.AppendLine($"      affectedDisciplines: {FormatDefNames(rule.affectedDisciplines)}");
                builder.AppendLine($"      requiredTags: {FormatDefNames(rule.requiredTags)}");
                builder.AppendLine($"      activeDuringConditions: {FormatDefNames(rule.activeDuringConditions)}");
                builder.AppendLine($"      activeDuringWeather: {FormatDefNames(rule.activeDuringWeather)}");
                builder.AppendLine($"      activeOnHilliness: {FormatValues(rule.activeOnHilliness)}");
                builder.AppendLine($"      factors: damage {rule.damageFactor:0.###}, radius {rule.radiusFactor:0.###}, duration {rule.durationFactor:0.###}, mana {rule.manaCostFactor:0.###}, cooldown {rule.cooldownFactor:0.###}");
            }
        }

        Log.Message(builder.ToString().TrimEnd());
    }

    public static void LogDelayedSpellRuntime()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting delayed spell runtime.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            DelayedSpellRuntimeMapComponent runtime = map?.GetComponent<DelayedSpellRuntimeMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have a delayed spell runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    public static void LogArmedSpellTriggers()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting armed spell triggers.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            SpellTriggerMapComponent runtime = map?.GetComponent<SpellTriggerMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have a spell trigger runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    public static void LogPersistentSpellEffects()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting persistent spell effects.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            PersistentSpellEffectMapComponent runtime = map?.GetComponent<PersistentSpellEffectMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have a persistent effect runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    public static void LogWallZones()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting wall zones.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            PersistentWallZoneMapComponent runtime = map?.GetComponent<PersistentWallZoneMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have a wall zone runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    public static void LogAreaZones()
    {
        if (Find.Maps == null || Find.Maps.Count == 0)
        {
            Log.Message("[MagicFramework] No maps were available while inspecting area zones.");
            return;
        }

        foreach (Map map in Find.Maps)
        {
            PersistentAreaZoneMapComponent runtime = map?.GetComponent<PersistentAreaZoneMapComponent>();
            if (runtime == null)
            {
                Log.Message($"[MagicFramework] Map {map?.Index ?? -1} does not have an area zone runtime component.");
                continue;
            }

            Log.Message(runtime.GetDebugSummary());
        }
    }

    private static string FormatDefNames<TDef>(List<TDef> defs)
        where TDef : Def
    {
        if (defs == null || defs.Count == 0)
        {
            return "<none>";
        }

        StringBuilder builder = new();
        for (int i = 0; i < defs.Count; i++)
        {
            if (defs[i] == null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(defs[i].defName);
        }

        return builder.Length == 0 ? "<none>" : builder.ToString();
    }

    private static string FormatValues<TValue>(List<TValue> values)
    {
        if (values == null || values.Count == 0)
        {
            return "<none>";
        }

        StringBuilder builder = new();
        for (int i = 0; i < values.Count; i++)
        {
            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(values[i]);
        }

        return builder.ToString();
    }
}
