using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Execution;

public static class SpellPowerUtility
{
    public static SpellPowerContext ComputePower(SpellDef spellDef, Thing caster)
    {
        SpellPowerDef powerDef = spellDef?.power;
        if (powerDef == null)
        {
            return new SpellPowerContext();
        }

        float value = powerDef.baseValue
            + ComputeCasterLevelContribution(powerDef, caster)
            + ComputeCasterSkillContribution(powerDef, caster);
        return new SpellPowerContext
        {
            value = value,
            tier = ResolveTier(powerDef, value)
        };
    }

    public static float ResolveScalableFloat(SpellContext context, float fallbackValue, ScalableFloatDef scalableFloat)
    {
        if (scalableFloat == null)
        {
            return fallbackValue;
        }

        float value = ResolveScaledValue(
            context,
            scalableFloat.mode,
            scalableFloat.baseValue,
            scalableFloat.perPower,
            scalableFloat.perTier);
        return Clamp(value, scalableFloat.min, scalableFloat.max);
    }

    public static int ResolveScalableInt(SpellContext context, int fallbackValue, ScalableFloatDef scalableFloat)
    {
        if (scalableFloat == null)
        {
            return fallbackValue;
        }

        return Mathf.RoundToInt(ResolveScalableFloat(context, fallbackValue, scalableFloat));
    }

    public static float ResolveDamageScalar(SpellContext context) => ResolveScalar(context, SpellScaledAttribute.Damage, context?.spellDef?.power?.damageScalar);

    public static float ResolveHealingScalar(SpellContext context) => ResolveScalar(context, SpellScaledAttribute.Healing, context?.spellDef?.power?.healingScalar);

    public static float ResolveRadiusScalar(SpellContext context) => ResolveScalar(context, SpellScaledAttribute.Radius, context?.spellDef?.power?.radiusScalar);

    public static float ResolveDurationScalar(SpellContext context) => ResolveScalar(context, SpellScaledAttribute.Duration, context?.spellDef?.power?.durationScalar);

    public static float ResolveManaCostScalar(SpellContext context) => ResolveScalar(context, SpellScaledAttribute.ManaCost, context?.spellDef?.power?.manaCostScalar);

    public static float ResolveCooldownScalar(SpellContext context) => ResolveScalar(context, SpellScaledAttribute.Cooldown, context?.spellDef?.power?.cooldownScalar);

    private static float ResolveScalar(SpellContext context, SpellScaledAttribute attribute, SpellPowerScalarDef scalarDef)
    {
        if (scalarDef != null)
        {
            float value = ResolveScaledValue(
                context,
                scalarDef.mode,
                scalarDef.baseValue,
                scalarDef.perPower,
                scalarDef.perTier);
            return Clamp(value, scalarDef.min, scalarDef.max);
        }

        return ResolveGlobalScalar(context, attribute);
    }

    private static float ResolveGlobalScalar(SpellContext context, SpellScaledAttribute attribute)
    {
        SpellPowerDef powerDef = context?.spellDef?.power;
        if (powerDef?.scaledAttributes == null || !powerDef.scaledAttributes.Contains(attribute))
        {
            return 1f;
        }

        float power = Mathf.Max(0f, context?.power?.value ?? 0f);
        MagicFrameworkSettings settings = MagicFrameworkSettings.Current;
        return attribute switch
        {
            SpellScaledAttribute.Damage => 1f + (power * Mathf.Max(0f, settings?.damageScalingPerPower ?? MagicFrameworkSettings.DefaultDamageScalingPerPower)),
            SpellScaledAttribute.Healing => 1f + (power * Mathf.Max(0f, settings?.healingScalingPerPower ?? MagicFrameworkSettings.DefaultHealingScalingPerPower)),
            SpellScaledAttribute.Radius => 1f + (power * Mathf.Max(0f, settings?.radiusScalingPerPower ?? MagicFrameworkSettings.DefaultRadiusScalingPerPower)),
            SpellScaledAttribute.Duration => 1f + (power * Mathf.Max(0f, settings?.durationScalingPerPower ?? MagicFrameworkSettings.DefaultDurationScalingPerPower)),
            SpellScaledAttribute.ManaCost => Mathf.Max(0.1f, 1f - (power * Mathf.Max(0f, settings?.manaCostReductionPerPower ?? MagicFrameworkSettings.DefaultManaCostReductionPerPower))),
            SpellScaledAttribute.Cooldown => Mathf.Max(0.1f, 1f - (power * Mathf.Max(0f, settings?.cooldownReductionPerPower ?? MagicFrameworkSettings.DefaultCooldownReductionPerPower))),
            _ => 1f
        };
    }

    private static float ResolveScaledValue(
        SpellContext context,
        SpellPowerScalingMode mode,
        float baseValue,
        float perPower,
        float perTier)
    {
        return mode switch
        {
            SpellPowerScalingMode.Flat => baseValue,
            SpellPowerScalingMode.Tiered => baseValue + ((context?.power?.tier ?? 0) * perTier),
            _ => baseValue + ((context?.power?.value ?? 0f) * perPower)
        };
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    private static float ComputeCasterSkillContribution(SpellPowerDef powerDef, Thing caster)
    {
        if (string.IsNullOrWhiteSpace(powerDef.casterSkillDef) || caster is not Pawn pawn || pawn.skills == null)
        {
            return 0f;
        }

        SkillDef skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(powerDef.casterSkillDef);
        if (skillDef == null)
        {
            Log.Warning($"[MagicFramework] Spell power skill def '{powerDef.casterSkillDef}' could not be resolved.");
            return 0f;
        }

        return pawn.skills.GetSkill(skillDef).Level * powerDef.casterSkillFactor;
    }

    private static float ComputeCasterLevelContribution(SpellPowerDef powerDef, Thing caster)
    {
        if (caster == null || powerDef.casterLevelFactor == 0f)
        {
            return 0f;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        int casterLevel = caster is Pawn pawn
            ? runtime?.GetCasterLevel(pawn) ?? 0
            : runtime?.GetDebugCasterLevel(caster) ?? 0;
        return casterLevel * powerDef.casterLevelFactor;
    }

    private static int ResolveTier(SpellPowerDef powerDef, float value)
    {
        int tier = powerDef.defaultTier;
        List<SpellPowerTierDef> tiers = powerDef.tiers;
        if (tiers == null)
        {
            return tier;
        }

        for (int i = 0; i < tiers.Count; i++)
        {
            SpellPowerTierDef tierDef = tiers[i];
            if (tierDef != null && value >= tierDef.minPower)
            {
                tier = tierDef.tier;
            }
        }

        return tier;
    }
}
