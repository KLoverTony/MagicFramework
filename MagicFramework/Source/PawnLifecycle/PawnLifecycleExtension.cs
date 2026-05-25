using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MagicFramework.PawnLifecycle;

public enum PawnLifecycleBodyForm
{
    Unspecified,
    Living,
    Skeletal,
    Flesh,
    Spectral,
    Construct,
    CorpseHosted,
    PhylacteryReformed,
    Custom
}

public enum PawnLifecycleIntelligence
{
    Unspecified,
    Mindless,
    Instinctive,
    TaskBound,
    PartialIdentity,
    FullSapience,
    Custom
}

public enum PawnLifecycleNeedsPolicy
{
    Unspecified,
    Ordinary,
    None,
    NoFoodNoRest,
    ManaUpkeep,
    CorpseOrFleshConsumption,
    EssenceDrain,
    DormancyRecharge,
    Custom
}

public enum PawnLifecycleSocialPolicy
{
    Unspecified,
    Ordinary,
    None,
    SuppressedBothWays,
    AuraOnly,
    LimitedRecognition,
    PseudoRelationshipMemory,
    FullRelationships,
    Custom
}

public enum PawnLifecycleGearPolicy
{
    Unspecified,
    Ordinary,
    None,
    StripAll,
    WeaponsOnly,
    ApparelOnly,
    FullGear,
    RestrictedLoadout,
    RitualGearOnly,
    Custom
}

public enum PawnLifecycleControlPolicy
{
    Unspecified,
    Ordinary,
    HostileOnly,
    AutonomousGuest,
    AutonomousServant,
    AlliedNonControllable,
    DraftedFollower,
    MasterBoundMinion,
    FullPlayerControl,
    TemporarySummon,
    EventControlled,
    Custom
}

public enum PawnLifecycleWorkPolicy
{
    Unspecified,
    Ordinary,
    None,
    CombatOnly,
    HaulingCleaningOnly,
    MundaneLabor,
    LimitedLabor,
    FullWork,
    RitualOnly,
    Custom
}

public enum PawnLifecycleRecoveryPolicy
{
    Unspecified,
    OrdinaryMedicine,
    None,
    Repair,
    Regeneration,
    Reassembly,
    CorpseReplacement,
    AnchorReform,
    PhylacteryReform,
    Custom
}

public enum PawnLifecycleDeathPolicy
{
    Unspecified,
    OrdinaryCorpse,
    NoCorpse,
    Vanish,
    LeaveRemains,
    ReturnToAnchor,
    ReleaseSoul,
    CorruptSoul,
    CreateHauntingRisk,
    DropConstructMaterials,
    CustomActions,
    Custom
}

public enum PawnLifecycleSoulPolicy
{
    Unspecified,
    OrdinaryLivingSoul,
    None,
    CorpseOnlyHusk,
    ReleasedSourceSoul,
    BoundSourceSoul,
    ActiveSpirit,
    CopiedEcho,
    SplitEcho,
    ConsumedSoul,
    CorruptedSoul,
    PhylacteryAnchored,
    ConstructCore,
    Custom
}

public enum PawnLifecycleDurationPolicy
{
    Unspecified,
    Ordinary,
    Permanent,
    TemporaryTimer,
    MaintainedSpell,
    MasterUpkeep,
    AnchorUpkeep,
    MapBound,
    SiteBound,
    Custom
}

/// <summary>
/// XML-authored lifecycle policy for undead, spirits, constructs, and other nonstandard pawns.
/// Attach to a pawn race ThingDef or PawnKindDef. PawnKindDef policy overrides race policy.
/// </summary>
public class PawnLifecycleExtension : DefModExtension
{
    public PawnLifecycleBodyForm bodyForm = PawnLifecycleBodyForm.Unspecified;
    public PawnLifecycleIntelligence intelligence = PawnLifecycleIntelligence.Unspecified;
    public PawnLifecycleNeedsPolicy needsPolicy = PawnLifecycleNeedsPolicy.Unspecified;
    public PawnLifecycleSocialPolicy socialPolicy = PawnLifecycleSocialPolicy.Unspecified;
    public PawnLifecycleGearPolicy gearPolicy = PawnLifecycleGearPolicy.Unspecified;
    public PawnLifecycleControlPolicy controlPolicy = PawnLifecycleControlPolicy.Unspecified;
    public PawnLifecycleWorkPolicy workPolicy = PawnLifecycleWorkPolicy.Unspecified;
    public PawnLifecycleRecoveryPolicy recoveryPolicy = PawnLifecycleRecoveryPolicy.Unspecified;
    public PawnLifecycleDeathPolicy deathPolicy = PawnLifecycleDeathPolicy.Unspecified;
    public PawnLifecycleSoulPolicy soulPolicy = PawnLifecycleSoulPolicy.Unspecified;
    public PawnLifecycleDurationPolicy durationPolicy = PawnLifecycleDurationPolicy.Unspecified;

    public bool isUndead;
    public bool isSpirit;
    public bool isConstruct;
    public bool enforceNeeds;
    public bool enforceSocialPolicy;
    public bool enforceGearPolicy;
    public bool enforceControlPolicy;
    public bool enforceWorkPolicy;
    public bool enforceIdentityPolicy;
    public bool clearGeneratedHealthState;
    public bool enforceLifeStage;
    public bool enforceMarkers;

    public List<HediffDef> markerHediffs;
    public List<TraitDef> lifecycleTraits;
    public List<string> lifecycleTags;
}
