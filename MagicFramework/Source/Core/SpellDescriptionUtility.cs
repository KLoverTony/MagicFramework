using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Builds plain-language player-facing descriptions from authored spell defs.
/// </summary>
public static class SpellDescriptionUtility
{
    private static readonly Dictionary<SpellDetailCacheKey, string> DetailCache = new();
    private const string SpellSummaryToken = "{MF:SpellSummary}";
    private const string EffectsToken = "{MF:Effects}";
    private const string ManaCostToken = "{MF:ManaCost}";
    private const string CooldownToken = "{MF:Cooldown}";
    private const string RangeToken = "{MF:Range}";
    private const string RadiusToken = "{MF:Radius}";
    private const string CastTimeToken = "{MF:CastTime}";
    private const string PowerScalingToken = "{MF:PowerScaling}";
    private const string RequirementsToken = "{MF:Requirements}";
    private const string TargetingToken = "{MF:Targeting}";
    private const string ClassificationToken = "{MF:Classification}";
    private const string ActiveModifiersToken = "{MF:ActiveModifiers}";

    public static string GetDetails(SpellDef spellDef)
    {
        if (spellDef == null)
        {
            return string.Empty;
        }

        SpellDetailCacheKey cacheKey = new(spellDef, UseColoredText);
        if (DetailCache.TryGetValue(cacheKey, out string cached))
        {
            return cached;
        }

        string generated = GenerateDetails(spellDef);
        DetailCache[cacheKey] = generated;
        return generated;
    }

    public static string GetGizmoDescription(SpellDef spellDef)
    {
        return GetGizmoDescription(spellDef, null);
    }

    public static string GetGizmoDescription(SpellDef spellDef, Pawn caster)
    {
        if (spellDef == null)
        {
            return string.Empty;
        }

        string authored = string.IsNullOrWhiteSpace(spellDef.description)
            ? T("MF_SpellDescriptionCastFallback", spellDef.LabelCap)
            : spellDef.description.Trim();
        if (ContainsToken(authored))
        {
            return ResolveDescriptionTokens(authored, spellDef, caster);
        }

        string details = GetDetails(spellDef);
        return string.IsNullOrWhiteSpace(details)
            ? authored
            : authored + "\n\n" + details;
    }

    public static string GetResolvedDescription(SpellDef spellDef, Pawn caster = null)
    {
        if (spellDef == null)
        {
            return string.Empty;
        }

        string authored = string.IsNullOrWhiteSpace(spellDef.description)
            ? T("MF_SpellDescriptionCastFallback", spellDef.LabelCap)
            : spellDef.description.Trim();
        return ContainsToken(authored)
            ? ResolveDescriptionTokens(authored, spellDef, caster)
            : authored;
    }

    public static bool HasDescriptionTokens(SpellDef spellDef)
    {
        return ContainsToken(spellDef?.description);
    }

    private static bool ContainsToken(string text)
    {
        return !string.IsNullOrWhiteSpace(text) && text.Contains("{MF:");
    }

    private static string ResolveDescriptionTokens(string text, SpellDef spellDef, Pawn caster)
    {
        if (string.IsNullOrWhiteSpace(text) || spellDef == null)
        {
            return text ?? string.Empty;
        }

        return text
            .Replace(SpellSummaryToken, BuildSpellSummary(spellDef, caster))
            .Replace(EffectsToken, StripEffectsHeader(GetDetails(spellDef)))
            .Replace(ManaCostToken, BuildManaCostText(spellDef, caster))
            .Replace(CooldownToken, BuildCooldownText(spellDef, caster))
            .Replace(RangeToken, BuildRangeText(spellDef, caster))
            .Replace(RadiusToken, BuildRadiusText(spellDef))
            .Replace(CastTimeToken, BuildCastTimeText(spellDef))
            .Replace(PowerScalingToken, BuildPowerScalingText(spellDef))
            .Replace(RequirementsToken, BuildRequirementsText(spellDef, caster))
            .Replace(TargetingToken, BuildTargetingText(spellDef, caster))
            .Replace(ClassificationToken, BuildClassificationText(spellDef))
            .Replace(ActiveModifiersToken, BuildActiveModifiersText(spellDef, caster));
    }

    private static string GenerateDetails(SpellDef spellDef)
    {
        List<string> lines = new();
        AddTargetingLine(lines, spellDef);
        AddScalingLine(lines, spellDef);
        AddCostLines(lines, spellDef);
        AddActionLines(lines, spellDef.actions, 0);

        if (lines.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        builder.AppendLine(T("MF_SpellDescriptionEffectsHeader"));
        for (int i = 0; i < lines.Count; i++)
        {
            builder.Append("- ");
            builder.AppendLine(lines[i]);
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildSpellSummary(SpellDef spellDef, Pawn caster)
    {
        List<string> sections = new();
        AddNonEmpty(sections, BuildClassificationText(spellDef));
        AddNonEmpty(sections, BuildRequirementsText(spellDef, caster));
        AddNonEmpty(sections, BuildTargetingText(spellDef, caster));
        AddNonEmpty(sections, BuildActiveModifiersText(spellDef, caster));
        AddNonEmpty(sections, StripEffectsHeader(GetDetails(spellDef)));
        return sections.Count == 0 ? string.Empty : string.Join("\n\n", sections);
    }

    private static string BuildClassificationText(SpellDef spellDef)
    {
        List<string> parts = new();
        if (spellDef?.meta?.tier > 0)
        {
            parts.Add(T("MF_SpellDescriptionTier", spellDef.meta.tier));
        }

        AddDefList(parts, spellDef?.meta?.elements);
        AddDefList(parts, spellDef?.meta?.domains);
        AddDefList(parts, spellDef?.meta?.disciplines);
        AddDefList(parts, spellDef?.meta?.tags);

        return parts.Count == 0 ? string.Empty : T("MF_SpellDescriptionClassification", string.Join(", ", parts));
    }

    private static string BuildRequirementsText(SpellDef spellDef, Pawn caster)
    {
        List<string> parts = new();
        AddRequirementDescriptions(parts, spellDef?.learning?.requirements);
        AddRequirementDescriptions(parts, spellDef?.casting?.requirements);
        AddRequirementDescriptions(parts, spellDef?.requirements);

        List<ResearchProjectDef> research = spellDef?.learning?.researchPrerequisites;
        if (research != null)
        {
            for (int i = 0; i < research.Count; i++)
            {
                ResearchProjectDef project = research[i];
                if (project != null)
                {
                    parts.Add(T(project.IsFinished ? "MF_SpellDescriptionResearchMet" : "MF_SpellDescriptionResearchUnmet", project.LabelCap));
                }
            }
        }

        if (caster != null)
        {
            bool canCast = SpellRequirementUtility.CanCastSpell(SpellRequirementUtility.CreatePawnContext(caster, spellDef), spellDef, out string reason, true);
            if (canCast)
            {
                parts.Add(T("MF_SpellDescriptionReadyForCaster", caster.LabelShortCap));
            }
            else if (!string.IsNullOrWhiteSpace(reason))
            {
                parts.Add(reason);
            }
        }

        return parts.Count == 0 ? T("MF_SpellDescriptionRequirementsNone") : T("MF_SpellDescriptionRequirements", string.Join("; ", parts));
    }

    private static string BuildTargetingText(SpellDef spellDef, Pawn caster)
    {
        SpellTargetingDef targeting = spellDef?.targeting;
        if (targeting == null)
        {
            return string.Empty;
        }

        List<string> parts = new()
        {
            DescribePrimaryTarget(targeting)
        };

        AddNonEmpty(parts, BuildRangeText(spellDef, caster));
        AddNonEmpty(parts, BuildRadiusText(spellDef));
        if (targeting.requireLineOfSight)
        {
            parts.Add(T("MF_SpellDescriptionLineOfSight"));
        }

        if (targeting.requireWaterCell)
        {
            parts.Add(T("MF_SpellDescriptionWaterLikeTerrain"));
        }

        return T("MF_SpellDescriptionTargeting", string.Join(", ", parts));
    }

    private static string BuildActiveModifiersText(SpellDef spellDef, Pawn caster)
    {
        List<string> rules = new();
        foreach (SpellEnhancementRuleDef rule in SpellEnhancementUtility.GetActiveRules(spellDef, caster?.Map))
        {
            if (rule != null)
            {
                rules.Add(rule.LabelCap);
            }
        }

        return rules.Count == 0 ? T("MF_SpellDescriptionActiveModifiersNone") : T("MF_SpellDescriptionActiveModifiers", string.Join(", ", rules));
    }

    private static string BuildManaCostText(SpellDef spellDef, Pawn caster)
    {
        SpellContext context = SpellRequirementUtility.CreatePawnContext(caster, spellDef);
        foreach (SpellCostDef cost in GetCosts(spellDef))
        {
            if (cost is ManaCostDef mana)
            {
                return FormatNumber(SpellEnhancementUtility.ResolveManaCost(context, mana.amount));
            }
        }

        return T("MF_SpellDescriptionZero");
    }

    private static string BuildCooldownText(SpellDef spellDef, Pawn caster)
    {
        SpellContext context = SpellRequirementUtility.CreatePawnContext(caster, spellDef);
        foreach (SpellCostDef cost in GetCosts(spellDef))
        {
            if (cost is CooldownCostDef cooldown)
            {
                return SpellEnhancementUtility.ResolveCooldownTicks(context, cooldown.cooldownTicks).ToStringTicksToPeriod();
            }
        }

        return T("MF_SpellDescriptionNone");
    }

    private static string BuildRangeText(SpellDef spellDef, Pawn caster)
    {
        SpellTargetingDef targeting = spellDef?.targeting;
        if (targeting == null || targeting.range <= 0f)
        {
            return string.Empty;
        }

        SpellContext context = SpellRequirementUtility.CreatePawnContext(caster, spellDef);
        return T("MF_SpellDescriptionRange", FormatNumber(SpellEnhancementUtility.ResolveScalableRadius(context, targeting.range, targeting.scalableRange)));
    }

    private static string BuildRadiusText(SpellDef spellDef)
    {
        SpellTargetingDef targeting = spellDef?.targeting;
        return targeting != null && targeting.radius > 0f
            ? T("MF_SpellDescriptionCellRadius", FormatNumber(targeting.radius))
            : string.Empty;
    }

    private static string BuildCastTimeText(SpellDef spellDef)
    {
        return spellDef?.castTimeTicks > 0 ? spellDef.castTimeTicks.ToStringTicksToPeriod() : T("MF_SpellDescriptionInstant");
    }

    private static string BuildPowerScalingText(SpellDef spellDef)
    {
        List<SpellScaledAttribute> scaledAttributes = spellDef?.power?.scaledAttributes;
        if (scaledAttributes == null || scaledAttributes.Count == 0)
        {
            return T("MF_SpellDescriptionNone");
        }

        List<string> parts = new();
        for (int i = 0; i < scaledAttributes.Count; i++)
        {
            string label = scaledAttributes[i] switch
            {
                SpellScaledAttribute.Damage => T("MF_SpellDescriptionScaledDamage"),
                SpellScaledAttribute.Healing => T("MF_SpellDescriptionScaledHealing"),
                SpellScaledAttribute.Radius => T("MF_SpellDescriptionScaledRadiusRange"),
                SpellScaledAttribute.Duration => T("MF_SpellDescriptionScaledDuration"),
                SpellScaledAttribute.ManaCost => T("MF_SpellDescriptionScaledManaCost"),
                SpellScaledAttribute.Cooldown => T("MF_SpellDescriptionScaledCooldown"),
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(label) && !parts.Contains(label))
            {
                parts.Add(label);
            }
        }

        return parts.Count == 0 ? T("MF_SpellDescriptionNone") : JoinList(parts);
    }

    private static void AddTargetingLine(List<string> lines, SpellDef spellDef)
    {
        SpellTargetingDef targeting = spellDef?.targeting;
        if (targeting == null)
        {
            return;
        }

        List<string> parts = new();
        if (targeting.range > 0f)
        {
            parts.Add(T("MF_SpellDescriptionRange", FormatNumber(targeting.range)));
        }

        if (targeting.shape == SpellTargetShape.Radius && targeting.radius > 0f)
        {
            parts.Add(T("MF_SpellDescriptionCellRadius", FormatNumber(targeting.radius)));
        }
        else if (targeting.shape == SpellTargetShape.Line && targeting.lineLength > 0f)
        {
            parts.Add(T("MF_SpellDescriptionCellLine", FormatNumber(targeting.lineLength)));
        }
        else if (targeting.shape == SpellTargetShape.Cone && targeting.lineLength > 0f)
        {
            parts.Add(T("MF_SpellDescriptionCellCone", FormatNumber(targeting.lineLength)));
        }
        else if (targeting.shape == SpellTargetShape.Wall && targeting.wallLength > 0)
        {
            parts.Add(T("MF_SpellDescriptionCellWall", targeting.wallLength));
        }

        if (targeting.requireLineOfSight)
        {
            parts.Add(T("MF_SpellDescriptionRequiresLineOfSight"));
        }

        if (targeting.requireWaterCell)
        {
            parts.Add(T("MF_SpellDescriptionRequiresWaterLikeTerrain"));
        }

        if (parts.Count > 0)
        {
            lines.Add(T("MF_SpellDescriptionTargets", DescribePrimaryTarget(targeting), string.Join(", ", parts)));
        }
    }

    private static void AddCostLines(List<string> lines, SpellDef spellDef)
    {
        List<SpellCostDef> costs = GetCosts(spellDef);
        if (costs.Count == 0)
        {
            return;
        }

        for (int i = 0; i < costs.Count; i++)
        {
            switch (costs[i])
            {
                case ManaCostDef manaCost:
                    lines.Add(T("MF_SpellDescriptionCostsMana", FormatNumber(manaCost.amount), Colorize(T("MF_SpellDescriptionMana"), "#62b8ff")));
                    break;
                case CooldownCostDef cooldownCost:
                    lines.Add(T("MF_SpellDescriptionStartsCooldown", FormatTicks(cooldownCost.cooldownTicks), Colorize(T("MF_SpellDescriptionCooldown"), "#ffd166")));
                    break;
            }
        }
    }

    private static void AddScalingLine(List<string> lines, SpellDef spellDef)
    {
        List<SpellScaledAttribute> scaledAttributes = spellDef?.power?.scaledAttributes;
        if (scaledAttributes == null || scaledAttributes.Count == 0)
        {
            return;
        }

        List<string> parts = new();
        for (int i = 0; i < scaledAttributes.Count; i++)
        {
            string label = scaledAttributes[i] switch
            {
                SpellScaledAttribute.Damage => T("MF_SpellDescriptionScaledDamage"),
                SpellScaledAttribute.Healing => T("MF_SpellDescriptionScaledHealing"),
                SpellScaledAttribute.Radius => T("MF_SpellDescriptionScaledRadiusRange"),
                SpellScaledAttribute.Duration => T("MF_SpellDescriptionScaledDuration"),
                SpellScaledAttribute.ManaCost => T("MF_SpellDescriptionScaledManaCost"),
                SpellScaledAttribute.Cooldown => T("MF_SpellDescriptionScaledCooldown"),
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(label) && !parts.Contains(label))
            {
                parts.Add(label);
            }
        }

        if (parts.Count > 0)
        {
            lines.Add(T("MF_SpellDescriptionScalesWithSpellPower", JoinList(parts), Colorize(T("MF_SpellDescriptionSpellPower"), "#c69cff")));
        }
    }

    private static void AddActionLines(List<string> lines, IEnumerable<SpellActionDef> actions, int depth)
    {
        if (actions == null || depth > 4)
        {
            return;
        }

        foreach (SpellActionDef action in actions)
        {
            AddActionLine(lines, action, depth);
        }
    }

    private static void AddActionLine(List<string> lines, SpellActionDef action, int depth)
    {
        switch (action)
        {
            case null:
                return;
            case SequenceActionDef sequence:
                AddActionLines(lines, sequence.actions, depth + 1);
                break;
            case DamageActionDef damage:
                lines.Add(T("MF_SpellDescriptionDealsDamage", FormatNumber(damage.amount), ColorizeDamageLabel(ResolveDefLabel<DamageDef>(damage.damageDef, T("MF_SpellDescriptionDamage")))));
                break;
            case HealActionDef heal:
                lines.Add(T("MF_SpellDescriptionHealsInjury", Colorize(T("MF_SpellDescriptionHeals"), "#7fdc8a"), FormatNumber(heal.amount)));
                break;
            case ExplosionActionDef explosion:
                lines.Add(T("MF_SpellDescriptionCreatesExplosion", FormatNumber(explosion.radius), ColorizeDamageLabel(ResolveDefLabel<DamageDef>(explosion.damageDef, T("MF_SpellDescriptionDamage"))), FormatNumber(explosion.damageAmount)));
                break;
            case TemperaturePushActionDef temperature:
                lines.Add(T("MF_SpellDescriptionPushesHeat", FormatNumber(temperature.heatEnergy)));
                break;
            case ApplyHediffActionDef hediff:
                lines.Add(T("MF_SpellDescriptionAppliesEffect", ResolveDefLabel<HediffDef>(hediff.hediffDef, T("MF_SpellDescriptionAStatusEffect")), DescribeDuration(hediff.durationTicks, hediff.removeAfterDuration)));
                break;
            case ApplyStatModifierActionDef statModifier:
                lines.Add(T("MF_SpellDescriptionAppliesStatModifiers", DescribeStatModifiers(statModifier.modifiers), FormatTicks(statModifier.durationTicks)));
                break;
            case SustainedStatModifierActionDef sustained:
                string sustainedEffect = !string.IsNullOrWhiteSpace(sustained.statusEffectDef)
                    ? DescribeStatusEffect(sustained.statusEffectDef)
                    : DescribeStatModifiers(sustained.modifiers);
                lines.Add(T("MF_SpellDescriptionMaintainsEffect", sustainedEffect, DescribeOptionalMaxDuration(sustained.maxDurationTicks)));
                break;
            case TimedStatusEffectActionDef status:
                lines.Add(T("MF_SpellDescriptionAppliesTimedStatusEffect", FormatTicks(status.durationTicks)));
                AddActionLines(lines, status.onApplyActions, depth + 1);
                break;
            case ApplyStatusEffectActionDef reusableStatus:
                lines.Add(T("MF_SpellDescriptionAppliesEffect", DescribeStatusEffect(reusableStatus.statusEffectDef), DescribeOptionalDuration(reusableStatus.durationTicks)));
                break;
            case ApplyForceFieldActionDef forceField:
                string upkeepText = forceField.sustainedManaCost > 0f ? T("MF_SpellDescriptionCostsManaEveryWhileMaintained", FormatNumber(forceField.sustainedManaCost), Colorize(T("MF_SpellDescriptionMana"), "#62b8ff"), FormatTicks(forceField.sustainedManaCostIntervalTicks)) : string.Empty;
                lines.Add(T("MF_SpellDescriptionMaintainsProtectiveForceField", DescribeOptionalMaxDuration(forceField.maxDurationTicks), upkeepText));
                break;
            case PersistentAreaZoneActionDef area:
                    string areaUpkeepText = area.sustainedManaCost > 0f ? T("MF_SpellDescriptionCostsManaEveryWhileMaintained", FormatNumber(area.sustainedManaCost), Colorize(T("MF_SpellDescriptionMana"), "#62b8ff"), FormatTicks(area.sustainedManaCostIntervalTicks)) : string.Empty;
                    string areaTargetCostText = area.manaCostPerAffectedPawn > 0f ? T("MF_SpellDescriptionCostsManaPerAffectedPawn", FormatNumber(area.manaCostPerAffectedPawn), Colorize(T("MF_SpellDescriptionMana"), "#62b8ff")) : string.Empty;
                    lines.Add(T("MF_SpellDescriptionCreatesLingeringArea", FormatNumber(area.zoneRadius), FormatTicks(area.durationTicks), areaUpkeepText, areaTargetCostText));
                    AddActionLines(lines, area.actions, depth + 1);
                    break;
            case PersistentWallZoneActionDef wall:
                lines.Add(T("MF_SpellDescriptionCreatesWallEffect", wall.wallLength, FormatTicks(wall.durationTicks)));
                AddActionLines(lines, wall.actions, depth + 1);
                break;
            case TerrainPatchActionDef terrain:
                lines.Add(DescribeTerrainPatch(terrain));
                break;
            case SummonPawnActionDef summon:
                lines.Add(T("MF_SpellDescriptionSummonsPawn", ResolveDefLabel<PawnKindDef>(summon.pawnKindDef, T("MF_SpellDescriptionACreature")), FormatTicks(summon.durationTicks)));
                break;
            case SpawnThingActionDef spawn:
                lines.Add(T("MF_SpellDescriptionCreatesThing", spawn.stackCount, ResolveDefLabel<ThingDef>(spawn.thingDef, T("MF_SpellDescriptionItem")), DescribeOptionalDuration(spawn.durationTicks)));
                break;
            case DelayActionDef delay:
                lines.Add(T("MF_SpellDescriptionAfterDelay", FormatTicks(delay.delayTicks)));
                AddActionLines(lines, delay.actions, depth + 1);
                break;
            case RepeatActionDef repeat:
                lines.Add(T("MF_SpellDescriptionRepeatsEvery", DescribeScalableCount(repeat.repeatCount, repeat.scalableRepeatCount), DescribeScalableInterval(repeat.intervalTicks, repeat.scalableIntervalTicks)));
                AddActionLines(lines, repeat.actions, depth + 1);
                break;
            case LaunchProjectileActionDef projectile:
                lines.Add(T("MF_SpellDescriptionLaunchesProjectile", ResolveDefLabel<ThingDef>(projectile.projectileDef, T("MF_SpellDescriptionAProjectile")), DescribeProjectileLaunch(projectile)));
                AddActionLines(lines, projectile.onImpactActions, depth + 1);
                break;
            case TeleportActionDef teleport:
                string stunText = teleport.postTeleportStunTicks > 0 ? T("MF_SpellDescriptionCausesBriefDisorientation", FormatTicks(teleport.postTeleportStunTicks)) : string.Empty;
                lines.Add(T("MF_SpellDescriptionTeleportsSubject", stunText));
                break;
            case KnockbackActionDef knockback:
                string impactText = knockback.impactDamageAmount > 0f ? T("MF_SpellDescriptionCollisionsDealDamage", FormatNumber(knockback.impactDamageAmount), ColorizeDamageLabel(ResolveDefLabel<DamageDef>(knockback.impactDamageDef, T("MF_SpellDescriptionBlunt")))) : string.Empty;
                lines.Add(T("MF_SpellDescriptionPushesTargetBack", DescribeScalableDistance(knockback.distance, knockback.scalableDistance), impactText));
                break;
            case PullActionDef pull:
                lines.Add(T("MF_SpellDescriptionPullsTarget", DescribeScalableDistance(pull.distance, pull.scalableDistance)));
                break;
            case MovePawnTowardPointActionDef movePawn:
                lines.Add(T("MF_SpellDescriptionMovesTarget", DescribeScalableDistance(movePawn.distance, movePawn.scalableDistance)));
                break;
            case MoveStoneChunksActionDef chunks:
                lines.Add(T("MF_SpellDescriptionDrawsStoneChunks"));
                break;
            case ConditionalActionDef conditional:
                lines.Add(string.IsNullOrWhiteSpace(conditional.conditionLabel) ? T("MF_SpellDescriptionHasConditionalEffect") : T("MF_SpellDescriptionHasConditionalEffectLabel", conditional.conditionLabel));
                break;
            case ApplyToTargetsActionDef applyToTargets:
                lines.Add(T("MF_SpellDescriptionAppliesToQueriedTargets"));
                AddActionLines(lines, applyToTargets.actions, depth + 1);
                break;
            case ApplyChainTargetsActionDef chainTargets:
                lines.Add(T("MF_SpellDescriptionChainsEffects"));
                AddActionLines(lines, chainTargets.actions, depth + 1);
                break;
            case ChainLightningActionDef chain:
                lines.Add(DescribeChainLightning(chain));
                break;
            case StunActionDef stun:
                lines.Add(T("MF_SpellDescriptionChanceToStun", FormatPercent(stun.chance), FormatTicks(stun.stunTicks)));
                break;
            case ExtinguishFireActionDef extinguish:
                lines.Add(T("MF_SpellDescriptionExtinguishesFires", DescribeScalableRadius(extinguish.radius, extinguish.scalableRadius)));
                break;
            case DestroyThingActionDef:
                lines.Add(T("MF_SpellDescriptionDestroysTarget"));
                break;
            case MineThingsActionDef mine:
                lines.Add(T("MF_SpellDescriptionMinesRockCells", DescribeScalableCount(mine.count, mine.scalableCount)));
                break;
            case TemporaryAllegianceActionDef allegiance:
                string allegianceUpkeepText = allegiance.sustainedManaCost > 0f ? T("MF_SpellDescriptionCostsManaEveryWhileMaintained", FormatNumber(allegiance.sustainedManaCost), Colorize(T("MF_SpellDescriptionMana"), "#62b8ff"), FormatTicks(allegiance.sustainedManaCostIntervalTicks)) : string.Empty;
                lines.Add(T("MF_SpellDescriptionTemporarilyCompelsTarget", DescribeOptionalMaxDuration(allegiance.maxDurationTicks), allegianceUpkeepText));
                break;
        }
    }

    private static string DescribePrimaryTarget(SpellTargetingDef targeting)
    {
        string target = targeting.primaryTargetType switch
        {
            SpellPrimaryTargetType.Cell => T("MF_SpellDescriptionACell"),
            SpellPrimaryTargetType.Pawn => T("MF_SpellDescriptionAPawn"),
            SpellPrimaryTargetType.Thing => T("MF_SpellDescriptionAThing"),
            SpellPrimaryTargetType.PawnOrCell => T("MF_SpellDescriptionAPawnOrCell"),
            SpellPrimaryTargetType.PawnOrThing => T("MF_SpellDescriptionAPawnOrThing"),
            _ => T("MF_SpellDescriptionATarget")
        };

        return targeting.pawnAffinity switch
        {
            SpellPawnAffinity.Ally => T("MF_SpellDescriptionAlliedTarget", target),
            SpellPawnAffinity.Foe => T("MF_SpellDescriptionHostileTarget", target),
            _ => target
        };
    }

    private static string DescribeStatModifiers(List<SpellStatModifierDef> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return T("MF_SpellDescriptionStatChanges");
        }

        List<string> parts = new();
        for (int i = 0; i < modifiers.Count; i++)
        {
            SpellStatModifierDef modifier = modifiers[i];
            if (modifier == null)
            {
                continue;
            }

            string stat = ResolveDefLabel<StatDef>(modifier.statDef, modifier.statDef ?? T("MF_SpellDescriptionAStat"));
            if (Math.Abs(modifier.factor - 1f) > 0.001f)
            {
                parts.Add(FormatPercentDelta(modifier.factor) + " " + stat);
            }

            if (Math.Abs(modifier.offset) > 0.001f)
            {
                parts.Add((modifier.offset > 0f ? "+" : "") + FormatNumber(modifier.offset) + " " + stat);
            }
        }

        return parts.Count == 0 ? T("MF_SpellDescriptionStatChanges") : string.Join(", ", parts);
    }

    private static string DescribeScalableDistance(int distance, ScalableFloatDef scalableDistance)
    {
        string text = T("MF_SpellDescriptionCells", distance > 0 ? distance : 1);
        return scalableDistance == null ? text : T("MF_SpellDescriptionScalingWithSpellPower", text);
    }

    private static string DescribeScalableRadius(float radius, ScalableFloatDef scalableRadius)
    {
        string text = T("MF_SpellDescriptionCellMeasure", FormatNumber(radius > 0f ? radius : 1f));
        return scalableRadius == null ? text : T("MF_SpellDescriptionScalingWithSpellPower", text);
    }

    private static string DescribeScalableCount(int count, ScalableFloatDef scalableCount)
    {
        string text = T("MF_SpellDescriptionTimes", count);
        return scalableCount == null ? text : T("MF_SpellDescriptionScalingWithSpellPower", text);
    }

    private static string DescribeScalableInterval(int intervalTicks, ScalableFloatDef scalableIntervalTicks)
    {
        string text = FormatTicks(intervalTicks);
        return scalableIntervalTicks == null ? text : T("MF_SpellDescriptionScalingWithSpellPower", text);
    }

    private static string DescribeStatusEffect(string defName)
    {
        if (string.IsNullOrWhiteSpace(defName))
        {
            return T("MF_SpellDescriptionReusableStatusEffect");
        }

        SpellStatusEffectDef statusEffectDef = DefDatabase<SpellStatusEffectDef>.GetNamedSilentFail(defName);
        if (statusEffectDef == null)
        {
            return T("MF_SpellDescriptionReusableStatusEffect");
        }

        string label = string.IsNullOrWhiteSpace(statusEffectDef.label) ? T("MF_SpellDescriptionReusableStatusEffect") : statusEffectDef.label;
        if (statusEffectDef.categories == null || statusEffectDef.categories.Count == 0)
        {
            return label + DescribeStatusRefreshPolicy(statusEffectDef.refreshPolicy);
        }

        List<string> categories = new();
        for (int i = 0; i < statusEffectDef.categories.Count; i++)
        {
            string category = statusEffectDef.categories[i];
            if (!string.IsNullOrWhiteSpace(category) && !categories.Contains(category))
            {
                categories.Add(category);
            }
        }

        string categoryText = categories.Count == 0 ? string.Empty : " (" + string.Join(", ", categories) + ")";
        return label + categoryText + DescribeStatusRefreshPolicy(statusEffectDef.refreshPolicy);
    }

    private static string DescribeChainLightning(ChainLightningActionDef chain)
    {
        string branchText = chain.maxBranches > 1
            ? T("MF_SpellDescriptionChainLightningBranching", Mathf.Max(1, chain.minBranches), Mathf.Max(Mathf.Max(1, chain.minBranches), chain.maxBranches))
            : string.Empty;
        string repeatText = chain.allowRepeatTargets && chain.visitedTargetPolicy == ChainVisitedTargetPolicy.AllowRepeats
            ? T("MF_SpellDescriptionMayRevisitTargets")
            : T("MF_SpellDescriptionAvoidsPreviouslyHitTargets");
        return T("MF_SpellDescriptionChainsLightning", chain.maxHops, FormatNumber(chain.jumpRadius), branchText, repeatText);
    }

    private static string DescribeProjectileLaunch(LaunchProjectileActionDef projectile)
    {
        if (projectile == null)
        {
            return string.Empty;
        }

        List<string> parts = new();
        if (projectile.launchOrigin != ProjectileLaunchOriginSource.Caster)
        {
            parts.Add(T("MF_SpellDescriptionFromOrigin", DescribeProjectileLaunchOrigin(projectile.launchOrigin)));
        }

        if (projectile.targetSource != ProjectileTargetSource.CurrentTarget)
        {
            parts.Add(T("MF_SpellDescriptionTowardTargetSource", DescribeProjectileTargetSource(projectile.targetSource)));
        }

        return parts.Count == 0 ? string.Empty : " " + string.Join(" ", parts);
    }

    private static string DescribeProjectileLaunchOrigin(ProjectileLaunchOriginSource launchOrigin)
    {
        return launchOrigin switch
        {
            ProjectileLaunchOriginSource.CurrentTarget => T("MF_SpellDescriptionCurrentTarget"),
            ProjectileLaunchOriginSource.CurrentCell => T("MF_SpellDescriptionSpellPoint"),
            _ => T("MF_SpellDescriptionCaster")
        };
    }

    private static string DescribeProjectileTargetSource(ProjectileTargetSource targetSource)
    {
        return targetSource switch
        {
            ProjectileTargetSource.CurrentCell => T("MF_SpellDescriptionSpellPoint"),
            ProjectileTargetSource.Caster => T("MF_SpellDescriptionCaster"),
            _ => T("MF_SpellDescriptionCurrentTarget")
        };
    }

    private static string DescribeStatusRefreshPolicy(SpellStatusRefreshPolicy refreshPolicy)
    {
        return refreshPolicy switch
        {
            SpellStatusRefreshPolicy.IgnoreIfActive => T("MF_SpellDescriptionStatusIgnoredIfActive"),
            SpellStatusRefreshPolicy.StackDuration => T("MF_SpellDescriptionStatusDurationStacks"),
            SpellStatusRefreshPolicy.Replace => T("MF_SpellDescriptionStatusReplacesExisting"),
            _ => string.Empty
        };
    }

    private static string DescribeTerrainPatch(TerrainPatchActionDef terrain)
    {
        List<string> parts = new();
        if (terrain.replaceWater)
        {
            parts.Add(T("MF_SpellDescriptionFreezesWaterTerrain", Colorize(T("MF_SpellDescriptionFreezes"), "#9fdcff")));
        }

        if (terrain.meltIce)
        {
            parts.Add(T("MF_SpellDescriptionMeltsIce", Colorize(T("MF_SpellDescriptionIce"), "#9fdcff")));
        }

        if (terrain.addSnow)
        {
            parts.Add(T("MF_SpellDescriptionAddsSnow", Colorize(T("MF_SpellDescriptionSnow"), "#d7f2ff")));
        }

        if (terrain.removeSnow)
        {
            parts.Add(T("MF_SpellDescriptionRemovesSnow"));
        }

        if (!string.IsNullOrWhiteSpace(terrain.replacementTerrainDef))
        {
            parts.Add(T("MF_SpellDescriptionChangesTerrainTo", ColorizeElementalTerm(ResolveDefLabel<TerrainDef>(terrain.replacementTerrainDef, terrain.replacementTerrainDef))));
        }

        return T("MF_SpellDescriptionTerrainPatch", parts.Count == 0 ? T("MF_SpellDescriptionChangesTerrain") : Capitalize(string.Join(", ", parts)), FormatNumber(terrain.radius));
    }

    private static List<SpellCostDef> GetCosts(SpellDef spellDef)
    {
        if (spellDef?.casting?.costs != null && spellDef.casting.costs.Count > 0)
        {
            return spellDef.casting.costs;
        }

        return spellDef?.costs ?? new List<SpellCostDef>();
    }

    private static void AddRequirementDescriptions(List<string> parts, List<SpellRequirementDef> requirements)
    {
        if (requirements == null)
        {
            return;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            string description = DescribeRequirement(requirements[i]);
            if (!string.IsNullOrWhiteSpace(description) && !parts.Contains(description))
            {
                parts.Add(description);
            }
        }
    }

    private static string DescribeRequirement(SpellRequirementDef requirement)
    {
        return requirement switch
        {
            ManaRequirementDef mana => T("MF_SpellDescriptionRequirementMana", FormatNumber(mana.amount)),
            CooldownRequirementDef => T("MF_SpellDescriptionRequirementCooldownReady"),
            ArcaneGiftRequirementDef => T("MF_SpellDescriptionRequirementArcaneGift"),
            CasterLevelRequirementDef casterLevel => T("MF_SpellDescriptionRequirementCasterLevel", casterLevel.minimumLevel),
            null => null,
            _ => requirement.GetType().Name
        };
    }

    private static void AddDefList<TDef>(List<string> parts, List<TDef> defs)
        where TDef : Def
    {
        if (defs == null)
        {
            return;
        }

        for (int i = 0; i < defs.Count; i++)
        {
            if (defs[i] != null)
            {
                parts.Add(defs[i].LabelCap);
            }
        }
    }

    private static void AddNonEmpty(List<string> parts, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add(value);
        }
    }

    private static string StripEffectsHeader(string details)
    {
        if (string.IsNullOrWhiteSpace(details))
        {
            return string.Empty;
        }

        string header = T("MF_SpellDescriptionEffectsHeader") + "\n";
        return details.StartsWith(header) ? details.Substring(header.Length) : details;
    }

    private static string DescribeDuration(int ticks, bool active)
    {
        return active && ticks > 0 ? T("MF_SpellDescriptionForDuration", FormatTicks(ticks)) : string.Empty;
    }

    private static string DescribeOptionalDuration(int ticks)
    {
        return ticks > 0 ? T("MF_SpellDescriptionForDuration", FormatTicks(ticks)) : string.Empty;
    }

    private static string DescribeOptionalMaxDuration(int ticks)
    {
        return ticks > 0 ? T("MF_SpellDescriptionForUpToDuration", FormatTicks(ticks)) : string.Empty;
    }

    private static string ResolveDefLabel<TDef>(string defName, string fallback)
        where TDef : Def
    {
        if (string.IsNullOrWhiteSpace(defName))
        {
            return fallback;
        }

        TDef def = DefDatabase<TDef>.GetNamedSilentFail(defName);
        return def?.label ?? fallback;
    }

    private static string ColorizeDamageLabel(string label)
    {
        return ColorizeElementalTerm(label);
    }

    private static string ColorizeElementalTerm(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || !UseColoredText)
        {
            return text;
        }

        string lower = text.ToLowerInvariant();
        if (lower.Contains("flame") || lower.Contains("fire") || lower.Contains("burn"))
        {
            return Colorize(text, "#ff6b2a");
        }

        if (lower.Contains("frost") || lower.Contains("ice") || lower.Contains("cold"))
        {
            return Colorize(text, "#9fdcff");
        }

        if (lower.Contains("lightning") || lower.Contains("shock") || lower.Contains("electric"))
        {
            return Colorize(text, "#f4dd5f");
        }

        if (lower.Contains("water"))
        {
            return Colorize(text, "#62b8ff");
        }

        return text;
    }

    private static string Colorize(string text, string color)
    {
        return UseColoredText && !string.IsNullOrWhiteSpace(text)
            ? "<color=" + color + ">" + text + "</color>"
            : text;
    }

    private static bool UseColoredText => MagicFrameworkSettings.Current?.useColoredSpellText == true;

    private static string FormatTicks(int ticks)
    {
        return ticks > 0 ? ticks.ToStringTicksToPeriod() : T("MF_SpellDescriptionBriefly");
    }

    private static string FormatNumber(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatPercent(float chance)
    {
        return Mathf.RoundToInt(Mathf.Clamp01(chance) * 100f) + "%";
    }

    private static string FormatPercentDelta(float factor)
    {
        int percent = Mathf.RoundToInt((factor - 1f) * 100f);
        return percent >= 0 ? "+" + percent + "%" : percent + "%";
    }

    private static string JoinList(List<string> parts)
    {
        if (parts == null || parts.Count == 0)
        {
            return string.Empty;
        }

        if (parts.Count == 1)
        {
            return parts[0];
        }

        if (parts.Count == 2)
        {
            return T("MF_SpellDescriptionListTwo", parts[0], parts[1]);
        }

        return T("MF_SpellDescriptionListMany", string.Join(", ", parts.GetRange(0, parts.Count - 1)), parts[parts.Count - 1]);
    }

#pragma warning disable CS0618
    private static string T(string key)
    {
        return key.Translate().ToString();
    }

    private static string T(string key, object arg0)
    {
        return key.Translate(arg0).ToString();
    }

    private static string T(string key, object arg0, object arg1)
    {
        return key.Translate(arg0, arg1).ToString();
    }

    private static string T(string key, object arg0, object arg1, object arg2)
    {
        return key.Translate(arg0, arg1, arg2).ToString();
    }

    private static string T(string key, object arg0, object arg1, object arg2, object arg3)
    {
        return key.Translate(arg0, arg1, arg2, arg3).ToString();
    }
#pragma warning restore CS0618

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    private readonly struct SpellDetailCacheKey : IEquatable<SpellDetailCacheKey>
    {
        private readonly SpellDef spellDef;
        private readonly bool colored;

        public SpellDetailCacheKey(SpellDef spellDef, bool colored)
        {
            this.spellDef = spellDef;
            this.colored = colored;
        }

        public bool Equals(SpellDetailCacheKey other)
        {
            return ReferenceEquals(spellDef, other.spellDef) && colored == other.colored;
        }

        public override bool Equals(object obj)
        {
            return obj is SpellDetailCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((spellDef != null ? spellDef.GetHashCode() : 0) * 397) ^ colored.GetHashCode();
            }
        }
    }
}
