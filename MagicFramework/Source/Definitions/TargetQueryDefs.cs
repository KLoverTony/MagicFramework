using MagicFramework.Targeting;

namespace MagicFramework.Definitions;

/// <summary>
/// Base authored query used to resolve targets during spell execution.
/// </summary>
public abstract class TargetQueryDef
{
    public string debugLabel;
    public TargetQueryOrdering ordering = TargetQueryOrdering.None;
    public TargetQueryCenterSource orderingCenterSource = TargetQueryCenterSource.CurrentCell;
    public int maxTargets = -1;

    public abstract TargetQueryWorker CreateWorker();
}

/// <summary>
/// Uses the current execution target as the result set.
/// </summary>
public sealed class CurrentTargetQueryDef : TargetQueryDef
{
    public override TargetQueryWorker CreateWorker() => new CurrentTargetQueryWorker();
}

/// <summary>
/// Stub query for collecting targets around a point.
/// </summary>
public sealed class TargetsInRadiusQueryDef : TargetQueryDef
{
    public float radius;
    public ScalableFloatDef scalableRadius;
    public TargetQueryCenterSource centerSource;
    public bool includePawns = true;
    public bool includeBuildings = true;
    public bool includeItems = true;
    public bool includeCaster;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.All;

    public override TargetQueryWorker CreateWorker() => new TargetsInRadiusQueryWorker();
}

/// <summary>
/// Stub query for selecting the nearest valid target.
/// </summary>
public sealed class NearestValidTargetQueryDef : TargetQueryDef
{
    public NearestValidTargetQueryDef()
    {
        ordering = TargetQueryOrdering.Nearest;
        orderingCenterSource = TargetQueryCenterSource.CurrentCell;
        maxTargets = 1;
    }

    public float maxRadius;
    public bool includePawns = true;
    public bool includeBuildings = true;
    public bool includeItems = true;
    public bool includeCaster;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.All;

    public override TargetQueryWorker CreateWorker() => new NearestValidTargetQueryWorker();
}

/// <summary>
/// Resolves targets using a shape-oriented query that mirrors authored spell targeting concepts.
/// </summary>
public sealed class ShapeTargetsQueryDef : TargetQueryDef
{
    public SpellTargetShape shape = SpellTargetShape.Single;
    public TargetQueryCenterSource originSource = TargetQueryCenterSource.Caster;
    public TargetQueryCenterSource centerSource = TargetQueryCenterSource.CurrentTarget;
    public float radius;
    public float lineLength;
    public float coneAngleDegrees = 60f;
    public int wallLength;
    public int maxChains;
    public bool includePawns = true;
    public bool includeBuildings = true;
    public bool includeItems = true;
    public bool includeCaster;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.All;

    public override TargetQueryWorker CreateWorker() => new ShapeTargetsQueryWorker();
}

/// <summary>
/// Resolves targets as a directional sequential chain with optional branching.
/// </summary>
public sealed class DirectionalChainQueryDef : TargetQueryDef
{
    public float jumpRadius = 8f;
    public int maxJumps = 5;
    public bool excludeVisitedTargets = true;
    public bool preferForwardDirection = true;
    public bool allowSplit;
    public int maxBranches = 1;
    public int splitCount = 2;
    public bool includePawns = true;
    public bool includeBuildings;
    public bool includeItems;
    public bool includeCaster;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.Foe;

    public override TargetQueryWorker CreateWorker() => new DirectionalChainQueryWorker();
}

public enum TargetQueryCenterSource
{
    CurrentCell,
    CurrentTarget,
    InitialTarget,
    Caster
}

public enum TargetQueryOrdering
{
    None,
    Nearest,
    Farthest,
    LowestHealth,
    HighestHealth,
    HighestThreat,
    LowestThreat
}
