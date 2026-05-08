using MagicFramework.Context;
using MagicFramework.Definitions;
using RimWorld;
using Verse;

namespace MagicFramework.Execution;

/// <summary>
/// Runs all cast requirements for a spell.
/// </summary>
public sealed class SpellCastValidator
{
    public bool TryValidate(SpellContext context)
    {
        if (context?.spellDef == null)
        {
            Log.Warning("[MagicFramework] Spell validation failed: context or spellDef was null.");
            return false;
        }

        if (!TryValidateTargeting(context, out string targetingReason))
        {
            context.executionState.failed = true;
            context.executionState.failureReason = targetingReason;
            Log.Message($"[MagicFramework] Cast blocked for {context.spellDef.defName}: {targetingReason}");
            return false;
        }

        if (SpellRequirementUtility.CanCastSpell(context, context.spellDef, out string reason))
        {
            return true;
        }

        context.executionState.failed = true;
        context.executionState.failureReason = reason;
        Log.Message($"[MagicFramework] Cast blocked for {context.spellDef.defName}: {reason}");
        return false;
    }

    private static bool TryValidateTargeting(SpellContext context, out string reason)
    {
        SpellTargetingDef targeting = context.spellDef.targeting;
        if (targeting == null)
        {
            reason = "Spell targeting definition was missing.";
            return false;
        }

        LocalTargetInfo target = context.initialTarget;
        if (!target.IsValid)
        {
            reason = "Target was invalid.";
            return false;
        }

        Thing targetThing = target.Thing;
        bool hasThing = targetThing != null;
        bool isPawn = targetThing is Pawn;

        if (!MatchesPrimaryTargetType(targeting.primaryTargetType, hasThing, isPawn))
        {
            reason = $"Target did not match required target type {targeting.primaryTargetType}.";
            return false;
        }

        if (!MatchesCategoryFilters(targeting, targetThing, hasThing, isPawn))
        {
            reason = "Target did not match the configured target category filters.";
            return false;
        }

        if (!targeting.allowSelfTarget && hasThing && context.caster != null && targetThing == context.caster)
        {
            reason = "Self-targeting is not allowed.";
            return false;
        }

        if (isPawn && !MatchesPawnAffinity(context, targetThing, targeting.pawnAffinity))
        {
            reason = $"Target pawn did not match required affinity {targeting.pawnAffinity}.";
            return false;
        }

        IntVec3 targetCell = target.Cell;
        if (!targetCell.IsValid)
        {
            reason = "Target cell was invalid.";
            return false;
        }

        float range = SpellPowerUtility.ResolveScalableFloat(context, targeting.range, targeting.scalableRange);
        if (context.caster != null && range > 0f && context.caster.Position.DistanceTo(targetCell) > range)
        {
            reason = $"Target was out of range {range}.";
            return false;
        }

        if (targeting.requireLineOfSight && context.map != null && context.caster != null
            && !GenSight.LineOfSight(context.caster.Position, targetCell, context.map))
        {
            reason = "Target was not in line of sight.";
            return false;
        }

        if (RequiresCellValidation(targeting.primaryTargetType))
        {
            if (context.map == null)
            {
                reason = "Map was not available for cell validation.";
                return false;
            }

            if (targeting.requireStandableCell && !targetCell.Standable(context.map))
            {
                reason = "Target cell was not standable.";
                return false;
            }

            if (targeting.requireWalkableCell && !targetCell.Walkable(context.map))
            {
                reason = "Target cell was not walkable.";
                return false;
            }

            if (targeting.requireWaterCell && !SpellTerrainUtility.IsWaterCell(context.map, targetCell))
            {
                reason = "Target cell was not water terrain.";
                return false;
            }
        }

        reason = null;
        return true;
    }

    private static bool MatchesPrimaryTargetType(SpellPrimaryTargetType primaryTargetType, bool hasThing, bool isPawn)
    {
        switch (primaryTargetType)
        {
            case SpellPrimaryTargetType.Cell:
                return !hasThing;
            case SpellPrimaryTargetType.Pawn:
                return isPawn;
            case SpellPrimaryTargetType.Thing:
                return hasThing;
            case SpellPrimaryTargetType.PawnOrThing:
                return hasThing;
            case SpellPrimaryTargetType.PawnOrCell:
                return !hasThing || isPawn;
            default:
                return false;
        }
    }

    private static bool MatchesCategoryFilters(SpellTargetingDef targeting, Thing targetThing, bool hasThing, bool isPawn)
    {
        if (!hasThing)
        {
            return true;
        }

        if (isPawn)
        {
            return targeting.includePawns;
        }

        return targetThing.def.category switch
        {
            ThingCategory.Building => targeting.includeBuildings,
            ThingCategory.Item => targeting.includeItems,
            _ => true
        };
    }

    private static bool MatchesPawnAffinity(SpellContext context, Thing targetThing, SpellPawnAffinity pawnAffinity)
    {
        if (pawnAffinity == SpellPawnAffinity.All)
        {
            return true;
        }

        if (context.caster == null)
        {
            return false;
        }

        Faction casterFaction = context.caster.Faction;
        Faction targetFaction = targetThing.Faction;
        bool sameFaction = casterFaction != null && targetFaction != null && casterFaction == targetFaction;
        bool hostile = casterFaction != null && targetFaction != null && casterFaction.HostileTo(targetFaction);

        switch (pawnAffinity)
        {
            case SpellPawnAffinity.Ally:
                return targetThing == context.caster || sameFaction;
            case SpellPawnAffinity.Foe:
                return hostile;
            default:
                return true;
        }
    }

    private static bool RequiresCellValidation(SpellPrimaryTargetType primaryTargetType)
    {
        switch (primaryTargetType)
        {
            case SpellPrimaryTargetType.Cell:
            case SpellPrimaryTargetType.PawnOrCell:
                return true;
            default:
                return false;
        }
    }
}
