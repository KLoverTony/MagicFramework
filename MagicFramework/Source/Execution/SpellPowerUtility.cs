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

        float value = scalableFloat.baseValue + ((context?.power?.value ?? 0f) * scalableFloat.perPower);
        if (value < scalableFloat.min)
        {
            return scalableFloat.min;
        }

        if (value > scalableFloat.max)
        {
            return scalableFloat.max;
        }

        return value;
    }

    public static int ResolveScalableInt(SpellContext context, int fallbackValue, ScalableFloatDef scalableFloat)
    {
        if (scalableFloat == null)
        {
            return fallbackValue;
        }

        return Mathf.RoundToInt(ResolveScalableFloat(context, fallbackValue, scalableFloat));
    }

    public static float ResolveDamageScalar(SpellContext context) => ResolveScalar(context, context?.spellDef?.power?.damageScalar);

    public static float ResolveHealingScalar(SpellContext context) => ResolveScalar(context, context?.spellDef?.power?.healingScalar);

    public static float ResolveRadiusScalar(SpellContext context) => ResolveScalar(context, context?.spellDef?.power?.radiusScalar);

    public static float ResolveDurationScalar(SpellContext context) => ResolveScalar(context, context?.spellDef?.power?.durationScalar);

    public static float ResolveManaCostScalar(SpellContext context) => ResolveScalar(context, context?.spellDef?.power?.manaCostScalar);

    public static float ResolveCooldownScalar(SpellContext context) => ResolveScalar(context, context?.spellDef?.power?.cooldownScalar);

    private static float ResolveScalar(SpellContext context, SpellPowerScalarDef scalarDef)
    {
        if (scalarDef == null)
        {
            return 1f;
        }

        float value = scalarDef.baseValue + ((context?.power?.value ?? 0f) * scalarDef.perPower);
        if (value < scalarDef.min)
        {
            return scalarDef.min;
        }

        if (value > scalarDef.max)
        {
            return scalarDef.max;
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
