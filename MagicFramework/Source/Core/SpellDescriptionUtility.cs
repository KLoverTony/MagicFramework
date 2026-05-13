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
            ? $"Cast {spellDef.LabelCap}."
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
            ? $"Cast {spellDef.LabelCap}."
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
        builder.AppendLine("Effects:");
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
            parts.Add("tier " + spellDef.meta.tier);
        }

        AddDefList(parts, spellDef?.meta?.elements);
        AddDefList(parts, spellDef?.meta?.domains);
        AddDefList(parts, spellDef?.meta?.disciplines);
        AddDefList(parts, spellDef?.meta?.tags);

        return parts.Count == 0 ? string.Empty : "Classification: " + string.Join(", ", parts) + ".";
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
                    parts.Add("research " + project.LabelCap + (project.IsFinished ? " met" : " unmet"));
                }
            }
        }

        if (caster != null)
        {
            bool canCast = SpellRequirementUtility.CanCastSpell(SpellRequirementUtility.CreatePawnContext(caster, spellDef), spellDef, out string reason, true);
            if (canCast)
            {
                parts.Add("ready for " + caster.LabelShortCap);
            }
            else if (!string.IsNullOrWhiteSpace(reason))
            {
                parts.Add(reason);
            }
        }

        return parts.Count == 0 ? "Requirements: none authored." : "Requirements: " + string.Join("; ", parts) + ".";
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
            parts.Add("line of sight");
        }

        if (targeting.requireWaterCell)
        {
            parts.Add("water-like terrain");
        }

        return "Targeting: " + string.Join(", ", parts) + ".";
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

        return rules.Count == 0 ? "Active modifiers: none." : "Active modifiers: " + string.Join(", ", rules) + ".";
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

        return "0";
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

        return "none";
    }

    private static string BuildRangeText(SpellDef spellDef, Pawn caster)
    {
        SpellTargetingDef targeting = spellDef?.targeting;
        if (targeting == null || targeting.range <= 0f)
        {
            return string.Empty;
        }

        SpellContext context = SpellRequirementUtility.CreatePawnContext(caster, spellDef);
        return "range " + FormatNumber(SpellEnhancementUtility.ResolveScalableRadius(context, targeting.range, targeting.scalableRange));
    }

    private static string BuildRadiusText(SpellDef spellDef)
    {
        SpellTargetingDef targeting = spellDef?.targeting;
        return targeting != null && targeting.radius > 0f
            ? FormatNumber(targeting.radius) + "-cell radius"
            : string.Empty;
    }

    private static string BuildCastTimeText(SpellDef spellDef)
    {
        return spellDef?.castTimeTicks > 0 ? spellDef.castTimeTicks.ToStringTicksToPeriod() : "instant";
    }

    private static string BuildPowerScalingText(SpellDef spellDef)
    {
        List<SpellScaledAttribute> scaledAttributes = spellDef?.power?.scaledAttributes;
        if (scaledAttributes == null || scaledAttributes.Count == 0)
        {
            return "none";
        }

        List<string> parts = new();
        for (int i = 0; i < scaledAttributes.Count; i++)
        {
            string label = scaledAttributes[i] switch
            {
                SpellScaledAttribute.Damage => "damage",
                SpellScaledAttribute.Healing => "healing",
                SpellScaledAttribute.Radius => "radius/range",
                SpellScaledAttribute.Duration => "duration",
                SpellScaledAttribute.ManaCost => "mana cost",
                SpellScaledAttribute.Cooldown => "cooldown",
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(label) && !parts.Contains(label))
            {
                parts.Add(label);
            }
        }

        return parts.Count == 0 ? "none" : JoinList(parts);
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
            parts.Add("range " + FormatNumber(targeting.range));
        }

        if (targeting.shape == SpellTargetShape.Radius && targeting.radius > 0f)
        {
            parts.Add(FormatNumber(targeting.radius) + "-cell radius");
        }

        if (targeting.requireLineOfSight)
        {
            parts.Add("requires line of sight");
        }

        if (targeting.requireWaterCell)
        {
            parts.Add("requires water-like terrain");
        }

        if (parts.Count > 0)
        {
            lines.Add("Targets " + DescribePrimaryTarget(targeting) + " (" + string.Join(", ", parts) + ").");
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
                    lines.Add("Costs " + FormatNumber(manaCost.amount) + " " + Colorize("mana", "#62b8ff") + ".");
                    break;
                case CooldownCostDef cooldownCost:
                    lines.Add("Starts a " + FormatTicks(cooldownCost.cooldownTicks) + " " + Colorize("cooldown", "#ffd166") + ".");
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
                SpellScaledAttribute.Damage => "damage",
                SpellScaledAttribute.Healing => "healing",
                SpellScaledAttribute.Radius => "radius/range",
                SpellScaledAttribute.Duration => "duration",
                SpellScaledAttribute.ManaCost => "mana cost",
                SpellScaledAttribute.Cooldown => "cooldown",
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(label) && !parts.Contains(label))
            {
                parts.Add(label);
            }
        }

        if (parts.Count > 0)
        {
            lines.Add("Scales " + JoinList(parts) + " with " + Colorize("spell power", "#c69cff") + ".");
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
                lines.Add("Deals " + FormatNumber(damage.amount) + " " + ColorizeDamageLabel(ResolveDefLabel<DamageDef>(damage.damageDef, "damage")) + " damage.");
                break;
            case HealActionDef heal:
                lines.Add(Colorize("Heals", "#7fdc8a") + " up to " + FormatNumber(heal.amount) + " total injury damage.");
                break;
            case ExplosionActionDef explosion:
                lines.Add("Creates a " + FormatNumber(explosion.radius) + "-cell " + ColorizeDamageLabel(ResolveDefLabel<DamageDef>(explosion.damageDef, "damage")) + " explosion for " + FormatNumber(explosion.damageAmount) + " damage.");
                break;
            case ApplyHediffActionDef hediff:
                lines.Add("Applies " + ResolveDefLabel<HediffDef>(hediff.hediffDef, "a status effect") + DescribeDuration(hediff.durationTicks, hediff.removeAfterDuration) + ".");
                break;
            case ApplyStatModifierActionDef statModifier:
                lines.Add("Applies " + DescribeStatModifiers(statModifier.modifiers) + " for " + FormatTicks(statModifier.durationTicks) + ".");
                break;
            case SustainedStatModifierActionDef sustained:
                string sustainedEffect = !string.IsNullOrWhiteSpace(sustained.statusEffectDef)
                    ? ResolveDefLabel<SpellStatusEffectDef>(sustained.statusEffectDef, "a reusable status effect")
                    : DescribeStatModifiers(sustained.modifiers);
                lines.Add("Maintains " + sustainedEffect + DescribeOptionalMaxDuration(sustained.maxDurationTicks) + ".");
                break;
            case TimedStatusEffectActionDef status:
                lines.Add("Applies a timed status effect for " + FormatTicks(status.durationTicks) + ".");
                AddActionLines(lines, status.onApplyActions, depth + 1);
                break;
            case ApplyStatusEffectActionDef reusableStatus:
                lines.Add("Applies " + ResolveDefLabel<SpellStatusEffectDef>(reusableStatus.statusEffectDef, "a reusable status effect") + DescribeOptionalDuration(reusableStatus.durationTicks) + ".");
                break;
            case ApplyForceFieldActionDef forceField:
                string upkeepText = forceField.sustainedManaCost > 0f ? " Costs " + FormatNumber(forceField.sustainedManaCost) + " " + Colorize("mana", "#62b8ff") + " every " + FormatTicks(forceField.sustainedManaCostIntervalTicks) + " while maintained." : string.Empty;
                lines.Add("Maintains a protective force field" + DescribeOptionalMaxDuration(forceField.maxDurationTicks) + "." + upkeepText);
                break;
            case PersistentAreaZoneActionDef area:
                lines.Add("Creates a lingering " + FormatNumber(area.zoneRadius) + "-cell area for " + FormatTicks(area.durationTicks) + ".");
                AddActionLines(lines, area.actions, depth + 1);
                break;
            case PersistentWallZoneActionDef wall:
                lines.Add("Creates a " + wall.wallLength + "-cell wall effect for " + FormatTicks(wall.durationTicks) + ".");
                AddActionLines(lines, wall.actions, depth + 1);
                break;
            case TerrainPatchActionDef terrain:
                lines.Add(DescribeTerrainPatch(terrain));
                break;
            case SummonPawnActionDef summon:
                lines.Add("Summons " + ResolveDefLabel<PawnKindDef>(summon.pawnKindDef, "a creature") + " for " + FormatTicks(summon.durationTicks) + ".");
                break;
            case SpawnThingActionDef spawn:
                lines.Add("Creates " + spawn.stackCount + " " + ResolveDefLabel<ThingDef>(spawn.thingDef, "item") + DescribeOptionalDuration(spawn.durationTicks) + ".");
                break;
            case DelayActionDef delay:
                lines.Add("After " + FormatTicks(delay.delayTicks) + ":");
                AddActionLines(lines, delay.actions, depth + 1);
                break;
            case RepeatActionDef repeat:
                lines.Add("Repeats " + repeat.repeatCount + " time(s), every " + FormatTicks(repeat.intervalTicks) + ".");
                AddActionLines(lines, repeat.actions, depth + 1);
                break;
            case LaunchProjectileActionDef projectile:
                lines.Add("Launches " + ResolveDefLabel<ThingDef>(projectile.projectileDef, "a projectile") + ".");
                AddActionLines(lines, projectile.onImpactActions, depth + 1);
                break;
            case TeleportActionDef teleport:
                string stunText = teleport.postTeleportStunTicks > 0 ? " Causes brief disorientation for " + FormatTicks(teleport.postTeleportStunTicks) + "." : string.Empty;
                lines.Add("Teleports the chosen subject to an authored destination." + stunText);
                break;
            case KnockbackActionDef knockback:
                string impactText = knockback.impactDamageAmount > 0f ? " Collisions deal " + FormatNumber(knockback.impactDamageAmount) + " " + ColorizeDamageLabel(ResolveDefLabel<DamageDef>(knockback.impactDamageDef, "blunt")) + " damage." : string.Empty;
                lines.Add("Pushes the target back up to " + knockback.distance + " cell(s)." + impactText);
                break;
            case PullActionDef pull:
                lines.Add("Pulls the target up to " + pull.distance + " cell(s) toward the caster.");
                break;
            case MovePawnTowardPointActionDef movePawn:
                lines.Add("Moves the target up to " + movePawn.distance + " cell(s) toward the spell point.");
                break;
            case MoveStoneChunksActionDef chunks:
                lines.Add("Draws nearby stone chunks toward the spell point.");
                break;
            case ConditionalActionDef conditional:
                lines.Add("Has a conditional effect" + (string.IsNullOrWhiteSpace(conditional.conditionLabel) ? "." : ": " + conditional.conditionLabel + "."));
                break;
            case ApplyToTargetsActionDef applyToTargets:
                lines.Add("Applies effects to queried targets.");
                AddActionLines(lines, applyToTargets.actions, depth + 1);
                break;
            case ApplyChainTargetsActionDef chainTargets:
                lines.Add("Chains effects through nearby targets.");
                AddActionLines(lines, chainTargets.actions, depth + 1);
                break;
            case ChainLightningActionDef chain:
                lines.Add("Chains lightning through nearby targets for up to " + chain.maxHops + " hop(s).");
                break;
            case StunActionDef stun:
                lines.Add("Has a " + FormatPercent(stun.chance) + " chance to stun for " + FormatTicks(stun.stunTicks) + ".");
                break;
            case DestroyThingActionDef:
                lines.Add("Destroys the target.");
                break;
        }
    }

    private static string DescribePrimaryTarget(SpellTargetingDef targeting)
    {
        string target = targeting.primaryTargetType switch
        {
            SpellPrimaryTargetType.Cell => "a cell",
            SpellPrimaryTargetType.Pawn => "a pawn",
            SpellPrimaryTargetType.Thing => "a thing",
            SpellPrimaryTargetType.PawnOrCell => "a pawn or cell",
            SpellPrimaryTargetType.PawnOrThing => "a pawn or thing",
            _ => "a target"
        };

        return targeting.pawnAffinity switch
        {
            SpellPawnAffinity.Ally => "an allied " + target,
            SpellPawnAffinity.Foe => "a hostile " + target,
            _ => target
        };
    }

    private static string DescribeStatModifiers(List<SpellStatModifierDef> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return "stat changes";
        }

        List<string> parts = new();
        for (int i = 0; i < modifiers.Count; i++)
        {
            SpellStatModifierDef modifier = modifiers[i];
            if (modifier == null)
            {
                continue;
            }

            string stat = ResolveDefLabel<StatDef>(modifier.statDef, modifier.statDef ?? "a stat");
            if (Math.Abs(modifier.factor - 1f) > 0.001f)
            {
                parts.Add(FormatPercentDelta(modifier.factor) + " " + stat);
            }

            if (Math.Abs(modifier.offset) > 0.001f)
            {
                parts.Add((modifier.offset > 0f ? "+" : "") + FormatNumber(modifier.offset) + " " + stat);
            }
        }

        return parts.Count == 0 ? "stat changes" : string.Join(", ", parts);
    }

    private static string DescribeTerrainPatch(TerrainPatchActionDef terrain)
    {
        List<string> parts = new();
        if (terrain.replaceWater)
        {
            parts.Add(Colorize("freezes", "#9fdcff") + " water-like terrain");
        }

        if (terrain.meltIce)
        {
            parts.Add("melts " + Colorize("ice", "#9fdcff"));
        }

        if (terrain.addSnow)
        {
            parts.Add("adds " + Colorize("snow", "#d7f2ff"));
        }

        if (terrain.removeSnow)
        {
            parts.Add("removes snow");
        }

        if (!string.IsNullOrWhiteSpace(terrain.replacementTerrainDef))
        {
            parts.Add("changes terrain to " + ColorizeElementalTerm(ResolveDefLabel<TerrainDef>(terrain.replacementTerrainDef, terrain.replacementTerrainDef)));
        }

        return (parts.Count == 0 ? "Changes terrain" : Capitalize(string.Join(", ", parts))) + " in a " + FormatNumber(terrain.radius) + "-cell radius.";
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
            ManaRequirementDef mana => "at least " + FormatNumber(mana.amount) + " mana",
            CooldownRequirementDef => "cooldown ready",
            ArcaneGiftRequirementDef => "Arcane gift",
            CasterLevelRequirementDef casterLevel => "caster level " + casterLevel.minimumLevel + "+",
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

        const string header = "Effects:\n";
        return details.StartsWith(header) ? details.Substring(header.Length) : details;
    }

    private static string DescribeDuration(int ticks, bool active)
    {
        return active && ticks > 0 ? " for " + FormatTicks(ticks) : string.Empty;
    }

    private static string DescribeOptionalDuration(int ticks)
    {
        return ticks > 0 ? " for " + FormatTicks(ticks) : string.Empty;
    }

    private static string DescribeOptionalMaxDuration(int ticks)
    {
        return ticks > 0 ? " for up to " + FormatTicks(ticks) : string.Empty;
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
        return ticks > 0 ? ticks.ToStringTicksToPeriod() : "briefly";
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
            return parts[0] + " and " + parts[1];
        }

        return string.Join(", ", parts.GetRange(0, parts.Count - 1)) + ", and " + parts[parts.Count - 1];
    }

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
