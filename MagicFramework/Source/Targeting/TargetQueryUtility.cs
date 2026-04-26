using System;
using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Targeting;

internal static class TargetQueryUtility
{
    public static IntVec3 ResolvePoint(SpellContext context, TargetQueryCenterSource source)
    {
        switch (source)
        {
            case TargetQueryCenterSource.CurrentTarget:
                return context.currentTarget.IsValid ? context.currentTarget.Cell : context.currentCell;
            case TargetQueryCenterSource.InitialTarget:
                return context.initialTarget.IsValid ? context.initialTarget.Cell : IntVec3.Invalid;
            case TargetQueryCenterSource.Caster:
                return context.caster?.Position ?? IntVec3.Invalid;
            case TargetQueryCenterSource.CurrentCell:
            default:
                return context.currentCell;
        }
    }

    public static List<LocalTargetInfo> CollectTargets(
        SpellContext context,
        bool includePawns,
        bool includeBuildings,
        bool includeItems,
        bool includeCaster,
        SpellPawnAffinity pawnAffinity,
        Func<Thing, bool> predicate)
    {
        List<LocalTargetInfo> targets = new();
        List<Thing> things = context?.map?.listerThings?.AllThings;
        if (things == null)
        {
            return targets;
        }

        foreach (Thing thing in things)
        {
            if (!MatchesThingFilter(context, thing, includePawns, includeBuildings, includeItems, includeCaster, pawnAffinity))
            {
                continue;
            }

            if (!predicate(thing))
            {
                continue;
            }

            targets.Add(new LocalTargetInfo(thing));
        }

        return targets;
    }

    public static bool MatchesThingFilter(
        SpellContext context,
        Thing thing,
        bool includePawns,
        bool includeBuildings,
        bool includeItems,
        bool includeCaster,
        SpellPawnAffinity pawnAffinity)
    {
        if (thing == null || thing.Destroyed)
        {
            return false;
        }

        if (!includeCaster && thing == context?.caster)
        {
            return false;
        }

        if (thing is Pawn pawn)
        {
            return includePawns && MatchesPawnAffinity(context?.caster, pawn, pawnAffinity);
        }

        return thing.def.category switch
        {
            ThingCategory.Building => includeBuildings,
            ThingCategory.Item => includeItems,
            _ => false
        };
    }

    public static bool MatchesPawnAffinity(Thing caster, Pawn pawn, SpellPawnAffinity pawnAffinity)
    {
        if (pawnAffinity == SpellPawnAffinity.All)
        {
            return true;
        }

        if (caster == null || pawn == null)
        {
            return false;
        }

        Faction casterFaction = caster.Faction;
        Faction targetFaction = pawn.Faction;
        bool sameFaction = casterFaction != null && targetFaction != null && casterFaction == targetFaction;
        bool hostile = casterFaction != null && targetFaction != null && casterFaction.HostileTo(targetFaction);

        return pawnAffinity switch
        {
            SpellPawnAffinity.Ally => pawn == caster || sameFaction,
            SpellPawnAffinity.Foe => hostile,
            _ => true
        };
    }

    public static float DistanceToSegment(IntVec3 point, IntVec3 segmentStart, IntVec3 segmentEnd)
    {
        Vector2 pointVector = ToVector2(point);
        Vector2 startVector = ToVector2(segmentStart);
        Vector2 endVector = ToVector2(segmentEnd);
        Vector2 segment = endVector - startVector;
        if (segment.sqrMagnitude < 0.001f)
        {
            return Vector2.Distance(pointVector, startVector);
        }

        float t = Mathf.Clamp01(Vector2.Dot(pointVector - startVector, segment) / segment.sqrMagnitude);
        Vector2 closest = startVector + (segment * t);
        return Vector2.Distance(pointVector, closest);
    }

    public static Vector2 ToVector2(IntVec3 cell)
    {
        return new Vector2(cell.x + 0.5f, cell.z + 0.5f);
    }

    public static float ForwardScore(IntVec3 origin, IntVec3 candidate, Vector2 forward)
    {
        if (forward.sqrMagnitude < 0.001f)
        {
            return 0f;
        }

        Vector2 direction = ToVector2(candidate) - ToVector2(origin);
        if (direction.sqrMagnitude < 0.001f)
        {
            return 0f;
        }

        return Vector2.Dot(direction.normalized, forward.normalized);
    }
}
