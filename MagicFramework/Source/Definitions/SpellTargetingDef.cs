namespace MagicFramework.Definitions;

/// <summary>
/// Describes what the player is allowed to target when initiating a cast.
/// </summary>
public class SpellTargetingDef
{
    public SpellTargetShape shape = SpellTargetShape.Single;
    public SpellPrimaryTargetType primaryTargetType = SpellPrimaryTargetType.PawnOrThing;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.All;
    public bool includePawns = true;
    public bool includeBuildings = true;
    public bool includeItems = true;
    public bool allowSelfTarget = true;
    public bool useCasterAsTarget;
    public bool requireLineOfSight;
    public bool requireStandableCell;
    public bool requireWalkableCell;
    public bool requireWaterCell;
    public float range;
    public ScalableFloatDef scalableRange;
    public float radius;
    public float lineLength;
    public int wallLength;
    public int maxChains;
}

public enum SpellTargetShape
{
    Single,
    Radius,
    Line,
    Wall,
    Chain
}

public enum SpellPrimaryTargetType
{
    Cell,
    Pawn,
    Thing,
    PawnOrThing,
    PawnOrCell
}

public enum SpellPawnAffinity
{
    All,
    Ally,
    Foe
}
