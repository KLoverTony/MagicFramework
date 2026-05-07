using System.Collections.Generic;
using MagicFramework.Actions;
using MagicFramework.Targeting;
using Verse;

namespace MagicFramework.Definitions;

/// <summary>
/// Runs child actions in author-defined order.
/// </summary>
public sealed class SequenceActionDef : SpellActionDef
{
    public List<SpellActionDef> actions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => actions;

    public override SpellActionWorker CreateWorker() => new SequenceActionWorker();
}

/// <summary>
/// Logs a debug message during execution.
/// </summary>
public sealed class LogMessageActionDef : SpellActionDef
{
    public string message;

    public override SpellActionWorker CreateWorker() => new LogMessageActionWorker();
}

/// <summary>
/// Stub for playing authored spell visuals and sounds at a chosen runtime location.
/// </summary>
public sealed class EffectActionDef : SpellActionDef
{
    public string effectDef;
    public string soundDef;
    public SpellEffectLocationSource locationSource;
    public bool attachToTarget;

    public override SpellActionWorker CreateWorker() => new EffectActionWorker();
}

/// <summary>
/// Plays a metadata-resolved visual/sound event without requiring concrete effect defs on the spell.
/// </summary>
public sealed class ProceduralFXActionDef : SpellActionDef
{
    public MagicFXEvent fxEvent = MagicFXEvent.Auto;
    public SpellEffectLocationSource locationSource = SpellEffectLocationSource.CurrentTarget;

    public override SpellActionWorker CreateWorker() => new ProceduralFXActionWorker();
}

/// <summary>
/// Schedules child actions to occur after a delay.
/// </summary>
public sealed class DelayActionDef : SpellActionDef
{
    public int delayTicks;
    public bool replaceExistingForCaster = true;
    public List<SpellActionDef> actions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => actions;

    public override SpellActionWorker CreateWorker() => new DelayActionWorker();
}

/// <summary>
/// Runs child actions repeatedly on a fixed tick interval.
/// </summary>
public sealed class RepeatActionDef : SpellActionDef
{
    public int intervalTicks = 60;
    public ScalableFloatDef scalableIntervalTicks;
    public int repeatCount = 1;
    public ScalableFloatDef scalableRepeatCount;
    public bool includeImmediate = true;
    public bool replaceExistingForCaster = true;
    public List<SpellActionDef> actions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => actions;

    public override SpellActionWorker CreateWorker() => new RepeatActionWorker();
}

/// <summary>
/// Arms a persistent proximity trigger at the current cell and runs child actions when it fires.
/// </summary>
public sealed class ProximityTriggerActionDef : SpellActionDef
{
    public float triggerRadius = 1.9f;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.Foe;
    public bool includeCaster;
    public bool replaceExistingForCaster = true;
    public bool removePersistentEffectsForCasterSpell = true;
    public int checkIntervalTicks = 15;
    public List<SpellActionDef> actions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => actions;

    public override SpellActionWorker CreateWorker() => new ProximityTriggerActionWorker();
}

/// <summary>
/// Spawns a persistent visible marker/effect at the current cell until removed or expired.
/// </summary>
public sealed class PersistentEffectActionDef : SpellActionDef
{
    public string markerThingDef;
    public int durationTicks = -1;
    public ScalableFloatDef scalableDurationTicks;
    public int failsafeDurationTicks = -1;
    public ScalableFloatDef scalableFailsafeDurationTicks;
    public bool replaceExistingForCaster = true;

    public override SpellActionWorker CreateWorker() => new PersistentEffectActionWorker();
}

/// <summary>
/// Creates a persistent wall-shaped zone anchored to the cast and pulses child actions while active.
/// </summary>
public sealed class PersistentWallZoneActionDef : SpellActionDef
{
    public string markerThingDef;
    public int wallLength = 5;
    public float pulseRadius = 0.9f;
    public int pulseIntervalTicks = 60;
    public int durationTicks = 600;
    public ScalableFloatDef scalableDurationTicks;
    public int failsafeDurationTicks = -1;
    public ScalableFloatDef scalableFailsafeDurationTicks;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.All;
    public bool includeCaster;
    public bool replaceExistingForCaster = true;
    public List<SpellActionDef> actions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => actions;

    public override SpellActionWorker CreateWorker() => new PersistentWallZoneActionWorker();
}

/// <summary>
/// Creates a persistent circular area zone anchored at the current cell and pulses child actions while active.
/// </summary>
public sealed class PersistentAreaZoneActionDef : SpellActionDef
{
    public string markerThingDef;
    public float zoneRadius = 3f;
    public int pulseIntervalTicks = 60;
    public string ambientEffectDef;
    public string ambientSoundDef;
    public int visualPulseIntervalTicks = 30;
    public bool emitVisualFromMarkers = true;
    public int maxVisualMarkersPerPulse = -1;
    public int durationTicks = 600;
    public ScalableFloatDef scalableDurationTicks;
    public int failsafeDurationTicks = -1;
    public ScalableFloatDef scalableFailsafeDurationTicks;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.All;
    public bool includeCaster;
    public bool replaceExistingForCaster = true;
    public List<SpellActionDef> actions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => actions;

    public override SpellActionWorker CreateWorker() => new PersistentAreaZoneActionWorker();
}

/// <summary>
/// Spawns a temporary pawn allied to the caster and cleans it up after a duration.
/// </summary>
public sealed class SummonPawnActionDef : SpellActionDef
{
    public string pawnKindDef;
    public int durationTicks = 600;
    public ScalableFloatDef scalableDurationTicks;
    public bool replaceExistingForCaster = true;
    public bool setFactionToPlayer = true;
    public bool joinLord = true;
    public bool followMasterWhileDrafted = true;
    public bool followMasterWhileFieldwork = true;
    public List<string> trainableDefs = new();

    public override SpellActionWorker CreateWorker() => new SummonPawnActionWorker();
}

/// <summary>
/// Spawns an authored thing at the current cell with optional timed cleanup.
/// </summary>
public sealed class SpawnThingActionDef : SpellActionDef
{
    public string thingDef;
    public List<TieredThingDefName> tieredThingDefs = new();
    public int stackCount = 1;
    public ScalableFloatDef scalableStackCount;
    public int durationTicks = -1;
    public ScalableFloatDef scalableDurationTicks;
    public bool forbidden;
    public bool replaceExistingForCasterSpell = true;

    public override SpellActionWorker CreateWorker() => new SpawnThingActionWorker();
}

/// <summary>
/// Mutates terrain and weather buildup around a resolved point.
/// </summary>
public sealed class TerrainPatchActionDef : SpellActionDef
{
    public TargetQueryCenterSource centerSource = TargetQueryCenterSource.CurrentCell;
    public float radius = 1f;
    public List<string> replaceTerrainDefs = new();
    public string replacementTerrainDef;
    public bool replaceWater;
    public string waterReplacementTerrainDef = "Ice";
    public bool addSnow;
    public float snowDepth = 0.35f;
    public bool skipRoofedCells = true;
    public bool onlyAffectNaturalTerrain;

    public override SpellActionWorker CreateWorker() => new TerrainPatchActionWorker();
}

/// <summary>
/// Pushes the current target away from the caster by a configured number of cells.
/// </summary>
public sealed class KnockbackActionDef : SpellActionDef
{
    public int distance = 3;
    public bool requireStandableDestination = true;
    public bool requireWalkableDestination = true;
    public bool allowHitCasterCell;

    public override SpellActionWorker CreateWorker() => new KnockbackActionWorker();
}

/// <summary>
/// Pulls the current target toward the caster by a configured number of cells.
/// </summary>
public sealed class PullActionDef : SpellActionDef
{
    public int distance = 3;
    public bool requireStandableDestination = true;
    public bool requireWalkableDestination = true;
    public int minDistanceFromCaster = 1;

    public override SpellActionWorker CreateWorker() => new PullActionWorker();
}

/// <summary>
/// Teleports the authored subject to a resolved destination cell.
/// </summary>
public sealed class TeleportActionDef : SpellActionDef
{
    public TeleportSubjectSource subjectSource = TeleportSubjectSource.Caster;
    public TeleportDestinationSource destinationSource = TeleportDestinationSource.CurrentCell;
    public int randomRadius = 6;
    public int randomMinRadius;
    public int randomCellSearchAttempts = 40;
    public bool swapWithCaster;
    public bool requireStandableDestination = true;
    public bool requireWalkableDestination = true;
    public bool requireUnoccupiedDestination;
    public bool allowTeleportOntoCaster;
    public bool allowSameCell;

    public override SpellActionWorker CreateWorker() => new TeleportActionWorker();
}

/// <summary>
/// Applies timed stat modifiers to an authored subject.
/// </summary>
public sealed class ApplyStatModifierActionDef : SpellActionDef
{
    public StatModifierTargetSource targetSource = StatModifierTargetSource.CurrentTarget;
    public int durationTicks = 300;
    public ScalableFloatDef scalableDurationTicks;
    public SpellStatusCueDef statusCue;
    public bool replaceExistingFromCasterSpell = true;
    public string indicatorHediffDef;
    public float indicatorSeverity = 0.01f;
    public bool removeIndicatorOnExpire = true;
    public List<SpellStatModifierDef> modifiers = new();

    public override SpellActionWorker CreateWorker() => new ApplyStatModifierActionWorker();
}

/// <summary>
/// Applies stat modifiers while the caster can maintain the effect.
/// </summary>
public sealed class SustainedStatModifierActionDef : SpellActionDef
{
    public StatModifierTargetSource targetSource = StatModifierTargetSource.CurrentTarget;
    public int maxDurationTicks = -1;
    public ScalableFloatDef scalableMaxDurationTicks;
    public SpellStatusCueDef statusCue;
    public bool replaceExistingFromCasterSpell = true;
    public string indicatorHediffDef;
    public float indicatorSeverity = 0.01f;
    public bool removeIndicatorOnExpire = true;
    public float maxRange = -1f;
    public bool breakWhenCasterDowned = true;
    public bool breakWhenTargetDowned;
    public bool breakWhenTargetOutOfRange = true;
    public bool breakWhenLineOfSightLost = true;
    public List<SpellStatModifierDef> modifiers = new();
    public List<SpellActionDef> onBreakActions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => onBreakActions;

    public override SpellActionWorker CreateWorker() => new SustainedStatModifierActionWorker();
}

/// <summary>
/// Maintains a defensive force field on a pawn, reducing or absorbing incoming damage.
/// </summary>
public sealed class ApplyForceFieldActionDef : SpellActionDef
{
    public StatModifierTargetSource targetSource = StatModifierTargetSource.CurrentTarget;
    public int maxDurationTicks = -1;
    public ScalableFloatDef scalableMaxDurationTicks;
    public SpellStatusCueDef statusCue;
    public float damageFactor = 0.5f;
    public bool absorbFullyWithMana;
    public float manaCostPerDamageAbsorbed = 1f;
    public float maxRange = -1f;
    public bool breakWhenCasterDowned = true;
    public bool breakWhenTargetDowned;
    public bool breakWhenTargetOutOfRange = true;
    public bool breakWhenLineOfSightLost = true;
    public string impactFleckDef = "BulletShieldAreaEffect";
    public string impactSoundDef = "EnergyShield_AbsorbDamage";
    public string ambientFleckDef = "BulletShieldAreaEffect";
    public int ambientFleckIntervalTicks = 90;
    public float ambientFleckScale = 1f;
    public string ambientColorHex;
    public string sustainedOverlayTexturePath = "Things/Mote/ShieldBubble";
    public float sustainedOverlayScale = 1.2f;
    public string sustainedOverlayColorHex;
    public List<SpellActionDef> onBreakActions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => onBreakActions;

    public override SpellActionWorker CreateWorker() => new ApplyForceFieldActionWorker();
}

/// <summary>
/// Removes active framework stat modifiers and their status cues from an authored subject.
/// </summary>
public sealed class ClearStatModifiersActionDef : SpellActionDef
{
    public StatModifierTargetSource targetSource = StatModifierTargetSource.CurrentTarget;
    public ClearStatModifierScope scope = ClearStatModifierScope.AllFromFramework;
    public string spellDef;
    public string statusHediffDef;
    public bool runBreakActions;

    public override SpellActionWorker CreateWorker() => new ClearStatModifiersActionWorker();
}

/// <summary>
/// Executes a delayed branching chain of lightning strikes through nearby targets.
/// </summary>
public sealed class ChainLightningActionDef : SpellActionDef
{
    public float damageAmount = 12f;
    public string damageDef = "Burn";
    public float armorPenetration;
    public float stunChance = 0.25f;
    public int stunTicks = 90;
    public int jumpDelayTicks = 12;
    public float jumpRadius = 8f;
    public int maxHops = 8;
    public int minBranches = 1;
    public int maxBranches = 2;
    public float minForwardScore = -0.15f;
    public bool allowRepeatTargets = true;
    public bool includeCaster;
    public SpellPawnAffinity pawnAffinity = SpellPawnAffinity.Foe;
    public string impactFleckDef = "SparkFlash";
    public string lineFleckDef = "ElectricalSpark";
    public string stunFleckDef = "Mote_Stun";
    public string soundDef;
    public List<SpellActionDef> onHitActions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => onHitActions;

    public override SpellActionWorker CreateWorker() => new ChainLightningActionWorker();
}

/// <summary>
/// Has a random chance to stun the current pawn target.
/// </summary>
public sealed class StunActionDef : SpellActionDef
{
    public float chance = 1f;
    public int stunTicks = 90;
    public string fleckDef = "Mote_Stun";

    public override SpellActionWorker CreateWorker() => new StunActionWorker();
}

/// <summary>
/// Heals injuries on the current pawn target, sharing healing evenly across wounds.
/// </summary>
public sealed class HealActionDef : SpellActionDef
{
    public float amount;
    public ScalableFloatDef scalableAmount;

    public override SpellActionWorker CreateWorker() => new HealActionWorker();
}

/// <summary>
/// Applies direct damage to the current target thing.
/// </summary>
public sealed class DamageActionDef : SpellActionDef
{
    public float amount;
    public ScalableFloatDef scalableAmount;
    public string damageDef;
    public float armorPenetration;
    public ScalableFloatDef scalableArmorPenetration;
    public List<ExtraDamageEntry> extraDamages;
    public string hitBodyPartDef;
    public GuiltPolicy guiltPolicy = GuiltPolicy.None;
    public bool useCombatLog;
    public string combatLogSignature;

    public override SpellActionWorker CreateWorker() => new DamageActionWorker();
}

/// <summary>
/// Controls how a damage action marks its instigator for RimWorld's guilt handling.
/// </summary>
public enum GuiltPolicy
{
    None,
    Damage
}

/// <summary>
/// Represents an additional damage entry for a damage action.
/// </summary>
public sealed class ExtraDamageEntry
{
    public string damageDef;
    public float amount;
    public float armorPenetration;
    public bool toHead;
}

/// <summary>
/// Immediately destroys the current target thing.
/// </summary>
public sealed class DestroyThingActionDef : SpellActionDef
{
    public bool allowPawns;

    public override SpellActionWorker CreateWorker() => new DestroyThingActionWorker();
}

/// <summary>
/// Adjusts hediff severity on the current pawn target.
/// </summary>
public sealed class ApplyHediffActionDef : SpellActionDef
{
    public string hediffDef;
    public float severity;
    public string bodyPartDef;
    public HediffAddMode addMode = HediffAddMode.Default;
    public bool removeAfterDuration;
    public int durationTicks;
    public ScalableFloatDef scalableDurationTicks;
    public bool checkIfAlreadyHas;

    public override SpellActionWorker CreateWorker() => new ApplyHediffActionWorker();
}

/// <summary>
/// Hediff add mode options.
/// </summary>
public enum HediffAddMode
{
    Default,
    Replace,
    TryAdd,
    SoftReplace
}

/// <summary>
/// Removes a hediff from the current pawn target.
/// </summary>
public sealed class RemoveHediffActionDef : SpellActionDef
{
    public string hediffDef;
    public string bodyPartDef;

    public override SpellActionWorker CreateWorker() => new RemoveHediffActionWorker();
}

/// <summary>
/// Triggers a radial flame explosion at the current cell.
/// </summary>
public sealed class ExplosionActionDef : SpellActionDef
{
    public float radius;
    public ScalableFloatDef scalableRadius;
    public float damageAmount;
    public ScalableFloatDef scalableDamageAmount;
    public string damageDef = "Flame";
    public float fireChance = 0.35f;
    public bool damageFalloff;
    public string explosionSoundDef;
    public string explosionEffectDef;
    public List<SpawnedThingEntry> spawnedThings;
    public List<SpawnedFilthEntry> spawnedFilth;
    public string gasDef;
    public float gasDurationTicks = -1f;

    public override SpellActionWorker CreateWorker() => new ExplosionActionWorker();
}

/// <summary>
/// Represents a thing to spawn after an explosion.
/// </summary>
public sealed class SpawnedThingEntry
{
    public string thingDef;
    public int stackCount = 1;
    public float chance = 1f;
}

/// <summary>
/// Represents filth to spawn after an explosion.
/// </summary>
public sealed class SpawnedFilthEntry
{
    public string filthDef;
    public float chance = 1f;
}

/// <summary>
/// Launches a real RimWorld projectile and runs child actions when it impacts or expires.
/// </summary>
public sealed class LaunchProjectileActionDef : SpellActionDef
{
    public string projectileDef;
    public ProjectileHitFlags hitFlags = ProjectileHitFlags.All;
    public bool preventFriendlyFire;
    public int impactTimeoutPaddingTicks = 60;
    public List<SpellActionDef> onImpactActions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => onImpactActions;

    public override SpellActionWorker CreateWorker() => new LaunchProjectileActionWorker();
}

/// <summary>
/// Applies child actions to targets resolved by a query definition.
/// </summary>
public sealed class ApplyToTargetsActionDef : SpellActionDef
{
    public TargetQueryDef targetQuery;
    public List<SpellActionDef> actions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => actions;

    public override SpellActionWorker CreateWorker() => new ApplyToTargetsActionWorker();
}

/// <summary>
/// Applies child actions across a directional chain of resolved targets.
/// </summary>
public sealed class ApplyChainTargetsActionDef : SpellActionDef
{
    public DirectionalChainQueryDef chainQuery = new();
    public List<SpellActionDef> actions = new();

    public override IEnumerable<SpellActionDef> GetChildActions() => actions;

    public override SpellActionWorker CreateWorker() => new ApplyChainTargetsActionWorker();
}

/// <summary>
/// Runs one of two child action lists based on an authored condition.
/// </summary>
public sealed class ConditionalActionDef : SpellActionDef
{
    public string conditionLabel;
    public SpellConditionDef condition;
    public List<SpellActionDef> thenActions = new();
    public List<SpellActionDef> elseActions = new();

    public override IEnumerable<SpellActionDef> GetChildActions()
    {
        foreach (SpellActionDef action in thenActions)
        {
            yield return action;
        }

        foreach (SpellActionDef action in elseActions)
        {
            yield return action;
        }
    }

    public override SpellActionWorker CreateWorker() => new ConditionalActionWorker();
}

public enum SpellEffectLocationSource
{
    CurrentCell,
    CurrentTarget,
    InitialTarget,
    Caster
}

public enum MagicFXEvent
{
    Auto,
    CastStart,
    ProjectileLaunch,
    ProjectileImpact,
    Impact,
    AreaPulse,
    Explosion,
    SustainStart,
    SustainTick,
    SustainEnd
}

public enum TeleportSubjectSource
{
    Caster,
    CurrentTarget,
    InitialTarget
}

public enum TeleportDestinationSource
{
    CurrentCell,
    InitialTargetCell,
    CurrentTargetCell,
    CasterCell,
    CasterAdjacentCell,
    RandomCellNearSubject,
    RandomCellNearCaster,
    RandomCellNearCurrentCell,
    RandomCellNearInitialTarget
}

public enum StatModifierTargetSource
{
    Caster,
    CurrentTarget
}

public enum ClearStatModifierScope
{
    AllFromFramework,
    FromCurrentCaster,
    FromCurrentSpell,
    FromSpecificSpell,
    WithStatusHediff
}

public sealed class SpellStatModifierDef
{
    public string statDef;
    public float offset;
    public float factor = 1f;
}

public sealed class SpellStatusCueDef
{
    public bool enabled = true;
    public string hediffDef;
    public string label;
    public string description;
    public float severity = 0.01f;
    public bool removeOnExpire = true;
}
