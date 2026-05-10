using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MagicFramework.Definitions;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Builds plain-language player-facing descriptions from authored spell defs.
/// </summary>
public static class SpellDescriptionUtility
{
    private static readonly Dictionary<SpellDef, string> DetailCache = new();

    public static string GetDetails(SpellDef spellDef)
    {
        if (spellDef == null)
        {
            return string.Empty;
        }

        if (DetailCache.TryGetValue(spellDef, out string cached))
        {
            return cached;
        }

        string generated = GenerateDetails(spellDef);
        DetailCache[spellDef] = generated;
        return generated;
    }

    public static string GetGizmoDescription(SpellDef spellDef)
    {
        if (spellDef == null)
        {
            return string.Empty;
        }

        string authored = string.IsNullOrWhiteSpace(spellDef.description)
            ? $"Cast {spellDef.LabelCap}."
            : spellDef.description.Trim();
        string details = GetDetails(spellDef);
        return string.IsNullOrWhiteSpace(details)
            ? authored
            : authored + "\n\n" + details;
    }

    private static string GenerateDetails(SpellDef spellDef)
    {
        List<string> lines = new();
        AddTargetingLine(lines, spellDef);
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
        List<SpellCostDef> costs = spellDef?.casting?.costs != null && spellDef.casting.costs.Count > 0
            ? spellDef.casting.costs
            : spellDef?.costs;
        if (costs == null)
        {
            return;
        }

        for (int i = 0; i < costs.Count; i++)
        {
            switch (costs[i])
            {
                case ManaCostDef manaCost:
                    lines.Add("Costs " + FormatNumber(manaCost.amount) + " mana.");
                    break;
                case CooldownCostDef cooldownCost:
                    lines.Add("Starts a " + FormatTicks(cooldownCost.cooldownTicks) + " cooldown.");
                    break;
            }
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
                lines.Add("Deals " + FormatNumber(damage.amount) + " " + ResolveDefLabel<DamageDef>(damage.damageDef, "damage") + " damage.");
                break;
            case HealActionDef heal:
                lines.Add("Heals up to " + FormatNumber(heal.amount) + " total injury damage.");
                break;
            case ExplosionActionDef explosion:
                lines.Add("Creates a " + FormatNumber(explosion.radius) + "-cell " + ResolveDefLabel<DamageDef>(explosion.damageDef, "damage") + " explosion for " + FormatNumber(explosion.damageAmount) + " damage.");
                break;
            case ApplyHediffActionDef hediff:
                lines.Add("Applies " + ResolveDefLabel<HediffDef>(hediff.hediffDef, "a status effect") + DescribeDuration(hediff.durationTicks, hediff.removeAfterDuration) + ".");
                break;
            case ApplyStatModifierActionDef statModifier:
                lines.Add("Applies " + DescribeStatModifiers(statModifier.modifiers) + " for " + FormatTicks(statModifier.durationTicks) + ".");
                break;
            case SustainedStatModifierActionDef sustained:
                lines.Add("Maintains " + DescribeStatModifiers(sustained.modifiers) + DescribeOptionalMaxDuration(sustained.maxDurationTicks) + ".");
                break;
            case ApplyForceFieldActionDef forceField:
                string upkeepText = forceField.sustainedManaCost > 0f ? " Costs " + FormatNumber(forceField.sustainedManaCost) + " mana every " + FormatTicks(forceField.sustainedManaCostIntervalTicks) + " while maintained." : string.Empty;
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
                string impactText = knockback.impactDamageAmount > 0f ? " Collisions deal " + FormatNumber(knockback.impactDamageAmount) + " " + ResolveDefLabel<DamageDef>(knockback.impactDamageDef, "blunt") + " damage." : string.Empty;
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
            parts.Add("freezes water-like terrain");
        }

        if (terrain.meltIce)
        {
            parts.Add("melts ice");
        }

        if (terrain.addSnow)
        {
            parts.Add("adds snow");
        }

        if (terrain.removeSnow)
        {
            parts.Add("removes snow");
        }

        if (!string.IsNullOrWhiteSpace(terrain.replacementTerrainDef))
        {
            parts.Add("changes terrain to " + ResolveDefLabel<TerrainDef>(terrain.replacementTerrainDef, terrain.replacementTerrainDef));
        }

        return (parts.Count == 0 ? "Changes terrain" : Capitalize(string.Join(", ", parts))) + " in a " + FormatNumber(terrain.radius) + "-cell radius.";
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

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }
}
