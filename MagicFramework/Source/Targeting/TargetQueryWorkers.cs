using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using UnityEngine;
using Verse;

namespace MagicFramework.Targeting;

public sealed class CurrentTargetQueryWorker : TargetQueryWorker
{
    public override IReadOnlyList<LocalTargetInfo> ResolveTargets(SpellContext context, TargetQueryDef queryDef)
    {
        if (context == null || !context.currentTarget.IsValid)
        {
            return new List<LocalTargetInfo>();
        }

        return new List<LocalTargetInfo> { context.currentTarget };
    }
}

public sealed class TargetsInRadiusQueryWorker : TargetQueryWorker
{
    public override IReadOnlyList<LocalTargetInfo> ResolveTargets(SpellContext context, TargetQueryDef queryDef)
    {
        TargetsInRadiusQueryDef radiusDef = queryDef as TargetsInRadiusQueryDef;
        if (context?.map == null || radiusDef == null)
        {
            return new List<LocalTargetInfo>();
        }

        IntVec3 center = TargetQueryUtility.ResolvePoint(context, radiusDef.centerSource);
        if (!center.IsValid)
        {
            return new List<LocalTargetInfo>();
        }

        float radius = SpellEnhancementUtility.ResolveScalableRadius(context, radiusDef.radius, radiusDef.scalableRadius);
        return TargetQueryUtility.CollectOrderedTargets(
            context,
            radiusDef,
            radiusDef.includePawns,
            radiusDef.includeBuildings,
            radiusDef.includeItems,
            radiusDef.includeCaster,
            radiusDef.pawnAffinity,
            thing => thing.Position.DistanceTo(center) <= radius);
    }
}

public sealed class NearestValidTargetQueryWorker : TargetQueryWorker
{
    public override IReadOnlyList<LocalTargetInfo> ResolveTargets(SpellContext context, TargetQueryDef queryDef)
    {
        NearestValidTargetQueryDef nearestDef = queryDef as NearestValidTargetQueryDef;
        if (context?.map == null || nearestDef == null || !context.currentCell.IsValid)
        {
            return new List<LocalTargetInfo>();
        }

        return TargetQueryUtility.CollectOrderedTargets(
            context,
            nearestDef,
            nearestDef.includePawns,
            nearestDef.includeBuildings,
            nearestDef.includeItems,
            nearestDef.includeCaster,
            nearestDef.pawnAffinity,
            thing => thing.Position.DistanceTo(context.currentCell) <= nearestDef.maxRadius);
    }
}

public sealed class ShapeTargetsQueryWorker : TargetQueryWorker
{
    public override IReadOnlyList<LocalTargetInfo> ResolveTargets(SpellContext context, TargetQueryDef queryDef)
    {
        ShapeTargetsQueryDef shapeDef = queryDef as ShapeTargetsQueryDef;
        if (context?.map == null || shapeDef == null)
        {
            return new List<LocalTargetInfo>();
        }

        return shapeDef.shape switch
        {
            SpellTargetShape.Single => ResolveSingle(context, shapeDef),
            SpellTargetShape.Radius => ResolveRadius(context, shapeDef),
            SpellTargetShape.Line => ResolveLine(context, shapeDef),
            SpellTargetShape.Cone => ResolveCone(context, shapeDef),
            SpellTargetShape.Wall => ResolveWall(context, shapeDef),
            SpellTargetShape.Chain => ResolveChain(context, shapeDef),
            _ => new List<LocalTargetInfo>()
        };
    }

    private static IReadOnlyList<LocalTargetInfo> ResolveSingle(SpellContext context, ShapeTargetsQueryDef shapeDef)
    {
        if (!context.currentTarget.IsValid || context.currentTarget.Thing == null)
        {
            return new List<LocalTargetInfo>();
        }

        return TargetQueryUtility.MatchesThingFilter(
                context,
                context.currentTarget.Thing,
                shapeDef.includePawns,
                shapeDef.includeBuildings,
                shapeDef.includeItems,
                shapeDef.includeCaster,
                shapeDef.pawnAffinity)
            ? new List<LocalTargetInfo> { context.currentTarget }
            : new List<LocalTargetInfo>();
    }

    private static IReadOnlyList<LocalTargetInfo> ResolveRadius(SpellContext context, ShapeTargetsQueryDef shapeDef)
    {
        IntVec3 center = TargetQueryUtility.ResolvePoint(context, shapeDef.centerSource);
        if (!center.IsValid)
        {
            return new List<LocalTargetInfo>();
        }

        float radius = SpellEnhancementUtility.ResolveRadius(context, shapeDef.radius);
        return TargetQueryUtility.CollectOrderedTargets(
            context,
            shapeDef,
            shapeDef.includePawns,
            shapeDef.includeBuildings,
            shapeDef.includeItems,
            shapeDef.includeCaster,
            shapeDef.pawnAffinity,
            thing => thing.Position.DistanceTo(center) <= radius);
    }

    private static IReadOnlyList<LocalTargetInfo> ResolveLine(SpellContext context, ShapeTargetsQueryDef shapeDef)
    {
        IntVec3 origin = TargetQueryUtility.ResolvePoint(context, shapeDef.originSource);
        IntVec3 center = TargetQueryUtility.ResolvePoint(context, shapeDef.centerSource);
        if (!origin.IsValid || !center.IsValid)
        {
            return new List<LocalTargetInfo>();
        }

        IntVec3 lineEnd = center;
        if (shapeDef.lineLength > 0f && origin.DistanceTo(center) > shapeDef.lineLength)
        {
            Vector2 originVector = TargetQueryUtility.ToVector2(origin);
            Vector2 centerVector = TargetQueryUtility.ToVector2(center);
            Vector2 direction = (centerVector - originVector).normalized;
            Vector2 endVector = originVector + (direction * shapeDef.lineLength);
            lineEnd = new IntVec3(Mathf.RoundToInt(endVector.x - 0.5f), 0, Mathf.RoundToInt(endVector.y - 0.5f));
        }

        return TargetQueryUtility.CollectOrderedTargets(
            context,
            shapeDef,
            shapeDef.includePawns,
            shapeDef.includeBuildings,
            shapeDef.includeItems,
            shapeDef.includeCaster,
            shapeDef.pawnAffinity,
            thing => TargetQueryUtility.DistanceToSegment(thing.Position, origin, lineEnd) <= 0.75f);
    }

    private static IReadOnlyList<LocalTargetInfo> ResolveCone(SpellContext context, ShapeTargetsQueryDef shapeDef)
    {
        IntVec3 origin = TargetQueryUtility.ResolvePoint(context, shapeDef.originSource);
        IntVec3 center = TargetQueryUtility.ResolvePoint(context, shapeDef.centerSource);
        if (!origin.IsValid || !center.IsValid)
        {
            return new List<LocalTargetInfo>();
        }

        Vector2 originVector = TargetQueryUtility.ToVector2(origin);
        Vector2 aimVector = TargetQueryUtility.ToVector2(center) - originVector;
        if (aimVector.sqrMagnitude < 0.001f)
        {
            return new List<LocalTargetInfo>();
        }

        float length = shapeDef.lineLength > 0f ? shapeDef.lineLength : shapeDef.radius;
        length = SpellEnhancementUtility.ResolveRadius(context, length);
        if (length <= 0f)
        {
            return new List<LocalTargetInfo>();
        }

        Vector2 forward = aimVector.normalized;
        float halfAngle = Mathf.Clamp(shapeDef.coneAngleDegrees <= 0f ? 60f : shapeDef.coneAngleDegrees, 1f, 360f) * 0.5f;
        float minDot = Mathf.Cos(halfAngle * Mathf.Deg2Rad);

        return TargetQueryUtility.CollectOrderedTargets(
            context,
            shapeDef,
            shapeDef.includePawns,
            shapeDef.includeBuildings,
            shapeDef.includeItems,
            shapeDef.includeCaster,
            shapeDef.pawnAffinity,
            thing =>
            {
                Vector2 offset = TargetQueryUtility.ToVector2(thing.Position) - originVector;
                float distance = offset.magnitude;
                if (distance > length || distance < 0.001f)
                {
                    return false;
                }

                return Vector2.Dot(offset.normalized, forward) >= minDot;
            });
    }

    private static IReadOnlyList<LocalTargetInfo> ResolveWall(SpellContext context, ShapeTargetsQueryDef shapeDef)
    {
        IntVec3 origin = TargetQueryUtility.ResolvePoint(context, shapeDef.originSource);
        IntVec3 center = TargetQueryUtility.ResolvePoint(context, shapeDef.centerSource);
        if (!origin.IsValid || !center.IsValid)
        {
            return new List<LocalTargetInfo>();
        }

        Vector2 direction = TargetQueryUtility.ToVector2(center) - TargetQueryUtility.ToVector2(origin);
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.right;
        }

        direction.Normalize();
        Vector2 perpendicular = new(-direction.y, direction.x);
        Vector2 centerVector = TargetQueryUtility.ToVector2(center);
        float halfLength = Mathf.Max(0.5f, shapeDef.wallLength / 2f);

        return TargetQueryUtility.CollectOrderedTargets(
            context,
            shapeDef,
            shapeDef.includePawns,
            shapeDef.includeBuildings,
            shapeDef.includeItems,
            shapeDef.includeCaster,
            shapeDef.pawnAffinity,
            thing =>
            {
                Vector2 thingVector = TargetQueryUtility.ToVector2(thing.Position);
                Vector2 offset = thingVector - centerVector;
                float alongWall = Mathf.Abs(Vector2.Dot(offset, perpendicular));
                float wallDepth = Mathf.Abs(Vector2.Dot(offset, direction));
                return alongWall <= halfLength && wallDepth <= 0.75f;
            });
    }

    private static IReadOnlyList<LocalTargetInfo> ResolveChain(SpellContext context, ShapeTargetsQueryDef shapeDef)
    {
        if (context.currentTarget.Thing == null)
        {
            return new List<LocalTargetInfo>();
        }

        float chainRadius = shapeDef.lineLength > 0f ? shapeDef.lineLength : shapeDef.radius;
        chainRadius = SpellEnhancementUtility.ResolveRadius(context, chainRadius);
        if (chainRadius <= 0f)
        {
            chainRadius = 8f;
        }

        int maxChains = Mathf.Max(1, shapeDef.maxChains);
        List<LocalTargetInfo> chainTargets = new();
        HashSet<Thing> visited = new();
        Thing currentThing = context.currentTarget.Thing;

        while (currentThing != null && visited.Count < maxChains)
        {
            if (!visited.Add(currentThing))
            {
                break;
            }

            if (TargetQueryUtility.MatchesThingFilter(
                    context,
                    currentThing,
                    shapeDef.includePawns,
                    shapeDef.includeBuildings,
                    shapeDef.includeItems,
                    shapeDef.includeCaster,
                    shapeDef.pawnAffinity))
            {
                chainTargets.Add(new LocalTargetInfo(currentThing));
            }

            currentThing = FindNearestUnvisited(context, currentThing, chainRadius, visited, shapeDef);
        }

        return chainTargets;
    }

    private static Thing FindNearestUnvisited(
        SpellContext context,
        Thing sourceThing,
        float maxRadius,
        HashSet<Thing> visited,
        ShapeTargetsQueryDef shapeDef)
    {
        Thing nearestThing = null;
        float nearestDistance = float.MaxValue;

        foreach (Thing thing in context.map.listerThings?.AllThings ?? new List<Thing>())
        {
            if (visited.Contains(thing))
            {
                continue;
            }

            if (!TargetQueryUtility.MatchesThingFilter(
                    context,
                    thing,
                    shapeDef.includePawns,
                    shapeDef.includeBuildings,
                    shapeDef.includeItems,
                    shapeDef.includeCaster,
                    shapeDef.pawnAffinity))
            {
                continue;
            }

            float distance = thing.Position.DistanceTo(sourceThing.Position);
            if (distance > maxRadius || distance >= nearestDistance)
            {
                continue;
            }

            nearestThing = thing;
            nearestDistance = distance;
        }

        return nearestThing;
    }
}

public sealed class DirectionalChainQueryWorker : TargetQueryWorker
{
    private sealed class ChainBranch
    {
        public Thing currentThing;
        public Vector2 forward;
    }

    public override IReadOnlyList<LocalTargetInfo> ResolveTargets(SpellContext context, TargetQueryDef queryDef)
    {
        DirectionalChainQueryDef chainDef = queryDef as DirectionalChainQueryDef;
        if (context?.map == null || chainDef == null || context.currentTarget.Thing == null)
        {
            return new List<LocalTargetInfo>();
        }

        int maxJumps = Mathf.Max(1, chainDef.maxJumps);
        int maxBranches = Mathf.Max(1, chainDef.maxBranches);
        int splitCount = Mathf.Max(1, chainDef.splitCount);

        List<LocalTargetInfo> resolvedTargets = new();
        HashSet<Thing> visited = new();
        Queue<ChainBranch> frontier = new();

        Thing initialThing = context.currentTarget.Thing;
        visited.Add(initialThing);
        resolvedTargets.Add(new LocalTargetInfo(initialThing));
        frontier.Enqueue(new ChainBranch
        {
            currentThing = initialThing,
            forward = ResolveInitialForward(context, initialThing)
        });

        while (frontier.Count > 0 && resolvedTargets.Count < maxJumps)
        {
            ChainBranch branch = frontier.Dequeue();
            List<Thing> nextTargets = FindNextTargets(context, branch, chainDef, visited, splitCount);
            if (nextTargets.Count == 0)
            {
                continue;
            }

            int branchAdds = 0;
            foreach (Thing nextThing in nextTargets)
            {
                if (resolvedTargets.Count >= maxJumps)
                {
                    break;
                }

                if (chainDef.excludeVisitedTargets && !visited.Add(nextThing))
                {
                    continue;
                }

                if (!chainDef.excludeVisitedTargets)
                {
                    visited.Add(nextThing);
                }

                resolvedTargets.Add(new LocalTargetInfo(nextThing));
                if (frontier.Count + branchAdds < maxBranches)
                {
                    frontier.Enqueue(new ChainBranch
                    {
                        currentThing = nextThing,
                        forward = TargetQueryUtility.ToVector2(nextThing.Position) - TargetQueryUtility.ToVector2(branch.currentThing.Position)
                    });
                    branchAdds++;
                }
            }
        }

        return resolvedTargets;
    }

    private static Vector2 ResolveInitialForward(SpellContext context, Thing initialThing)
    {
        if (context.caster == null)
        {
            return Vector2.zero;
        }

        return TargetQueryUtility.ToVector2(initialThing.Position) - TargetQueryUtility.ToVector2(context.caster.Position);
    }

    private static List<Thing> FindNextTargets(
        SpellContext context,
        ChainBranch branch,
        DirectionalChainQueryDef chainDef,
        HashSet<Thing> visited,
        int desiredCount)
    {
        List<(Thing thing, float distance, float forwardScore)> candidates = new();
        foreach (Thing thing in context.map.listerThings?.AllThings ?? new List<Thing>())
        {
            if (thing == branch.currentThing)
            {
                continue;
            }

            if (chainDef.excludeVisitedTargets && visited.Contains(thing))
            {
                continue;
            }

            if (!TargetQueryUtility.MatchesThingFilter(
                    context,
                    thing,
                    chainDef.includePawns,
                    chainDef.includeBuildings,
                    chainDef.includeItems,
                    chainDef.includeCaster,
                    chainDef.pawnAffinity))
            {
                continue;
            }

            float distance = thing.Position.DistanceTo(branch.currentThing.Position);
            float jumpRadius = SpellEnhancementUtility.ResolveRadius(context, chainDef.jumpRadius);
            if (distance > jumpRadius)
            {
                continue;
            }

            float forwardScore = chainDef.preferForwardDirection
                ? TargetQueryUtility.ForwardScore(branch.currentThing.Position, thing.Position, branch.forward)
                : 0f;
            candidates.Add((thing, distance, forwardScore));
        }

        candidates.Sort((left, right) =>
        {
            if (chainDef.preferForwardDirection)
            {
                int scoreCompare = right.forwardScore.CompareTo(left.forwardScore);
                if (scoreCompare != 0)
                {
                    return scoreCompare;
                }
            }

            return left.distance.CompareTo(right.distance);
        });

        int takeCount = chainDef.allowSplit ? Mathf.Min(desiredCount, candidates.Count) : Mathf.Min(1, candidates.Count);
        List<Thing> results = new();
        for (int i = 0; i < takeCount; i++)
        {
            results.Add(candidates[i].thing);
        }

        return results;
    }
}
