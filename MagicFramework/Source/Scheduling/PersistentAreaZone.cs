using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Represents a persistent circular hazardous or beneficial zone that can pulse spell actions over time.
/// </summary>
public sealed class PersistentAreaZone : IExposable
{
    private List<int> actionPath = new();
    private Thing caster;
    private SpellDef spellDef;
    private IntVec3 centerCell = IntVec3.Invalid;
    private List<Thing> markerThings = new();
    private int randomSeed;
    private float powerValue;
    private int powerTier;
    private SpellVariableStore variables = new();
    private SpellPawnAffinity pawnAffinity = SpellPawnAffinity.All;
    private bool includeCaster;
    private float zoneRadius = 3f;
    private int pulseIntervalTicks = 60;
    private int nextPulseTick;
    private string ambientEffectDef;
    private string ambientSoundDef;
    private int visualPulseIntervalTicks = 30;
    private bool emitVisualFromMarkers = true;
    private int maxVisualMarkersPerPulse = -1;
    private bool pulseAtCenter;
    private bool requiresConcentration;
    private bool breakWhenCasterDowned = true;
    private bool breakWhenCasterStunned = true;
    private bool breakWhenCasterMentalState = true;
    private SpellMaintenanceDef maintenance;
    private int nextVisualTick;
    private int expireAtTick = -1;

    public PersistentAreaZone()
    {
    }

    public PersistentAreaZone(
        Thing caster,
        SpellDef spellDef,
        IntVec3 centerCell,
        IEnumerable<Thing> markerThings,
        int randomSeed,
        SpellPowerContext power,
        SpellVariableStore variables,
        IEnumerable<int> actionPath,
        SpellPawnAffinity pawnAffinity,
        bool includeCaster,
        float zoneRadius,
        int pulseIntervalTicks,
        string ambientEffectDef,
        string ambientSoundDef,
        int visualPulseIntervalTicks,
        bool emitVisualFromMarkers,
        int maxVisualMarkersPerPulse,
        bool pulseAtCenter,
        bool requiresConcentration,
        bool breakWhenCasterDowned,
        bool breakWhenCasterStunned,
        bool breakWhenCasterMentalState,
        SpellMaintenanceDef maintenance,
        int expireAtTick)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.centerCell = centerCell;
        this.markerThings = markerThings != null ? new List<Thing>(markerThings) : new List<Thing>();
        this.randomSeed = randomSeed;
        powerValue = power?.value ?? 0f;
        powerTier = power?.tier ?? 0;
        this.variables = variables?.Clone() ?? new SpellVariableStore();
        this.actionPath = actionPath != null ? new List<int>(actionPath) : new List<int>();
        this.pawnAffinity = pawnAffinity;
        this.includeCaster = includeCaster;
        this.zoneRadius = zoneRadius;
        this.pulseIntervalTicks = pulseIntervalTicks > 0 ? pulseIntervalTicks : 60;
        this.ambientEffectDef = ambientEffectDef;
        this.ambientSoundDef = ambientSoundDef;
        this.visualPulseIntervalTicks = visualPulseIntervalTicks > 0 ? visualPulseIntervalTicks : 30;
        this.emitVisualFromMarkers = emitVisualFromMarkers;
        this.maxVisualMarkersPerPulse = maxVisualMarkersPerPulse;
        this.pulseAtCenter = pulseAtCenter;
        this.requiresConcentration = requiresConcentration;
        this.breakWhenCasterDowned = breakWhenCasterDowned;
        this.breakWhenCasterStunned = breakWhenCasterStunned;
        this.breakWhenCasterMentalState = breakWhenCasterMentalState;
        this.maintenance = maintenance;
        nextPulseTick = Find.TickManager?.TicksGame ?? 0;
        nextVisualTick = Find.TickManager?.TicksGame ?? 0;
        this.expireAtTick = expireAtTick;
    }

    public Thing Caster => caster;
    public SpellDef SpellDef => spellDef;
    public IntVec3 CenterCell => centerCell;
    public IReadOnlyList<Thing> MarkerThings => markerThings;
    public SpellPawnAffinity PawnAffinity => pawnAffinity;
    public bool IncludeCaster => includeCaster;
    public float ZoneRadius => zoneRadius;
    public int NextPulseTick => nextPulseTick;
    public string AmbientEffectDef => ambientEffectDef;
    public string AmbientSoundDef => ambientSoundDef;
    public int NextVisualTick => nextVisualTick;
    public bool EmitVisualFromMarkers => emitVisualFromMarkers;
    public int MaxVisualMarkersPerPulse => maxVisualMarkersPerPulse;
    public bool PulseAtCenter => pulseAtCenter;
    public bool RequiresConcentration => requiresConcentration;
    public int ExpireAtTick => expireAtTick;

    public string DebugLabel => TryResolveActionDef(out PersistentAreaZoneActionDef actionDef)
        ? actionDef.debugLabel ?? actionDef.GetType().Name
        : "<unresolved area zone>";

    public bool TryResolveActionDef(out PersistentAreaZoneActionDef actionDef)
    {
        actionDef = SpellActionPathUtility.ResolveAction(spellDef, actionPath) as PersistentAreaZoneActionDef;
        return actionDef != null;
    }

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public bool IsConcentrationBroken(out string reason)
    {
        reason = null;
        if (maintenance?.profiles != null && maintenance.profiles.Count > 0)
        {
            return SpellMaintenanceUtility.IsMaintenanceBroken(maintenance, caster, null, caster?.MapHeld, centerCell, out reason);
        }

        if (!requiresConcentration)
        {
            return false;
        }

        if (caster == null || caster.Destroyed)
        {
            reason = "caster invalid";
            return true;
        }

        if (caster is not Pawn casterPawn)
        {
            return false;
        }

        if (casterPawn.Dead)
        {
            reason = "caster dead";
            return true;
        }

        if (breakWhenCasterDowned && casterPawn.Downed)
        {
            reason = "caster downed";
            return true;
        }

        if (breakWhenCasterStunned && casterPawn.stances?.stunner?.Stunned == true)
        {
            reason = "caster stunned";
            return true;
        }

        if (breakWhenCasterMentalState && casterPawn.MentalState != null)
        {
            reason = "caster mental state";
            return true;
        }

        return false;
    }

    public bool TryCreateExecutionContext(Map map, Pawn triggeringPawn, out SpellContext context)
    {
        context = null;
        if (spellDef == null || map == null)
        {
            return false;
        }

        context = new SpellContext
        {
            caster = caster,
            map = map,
            spellDef = spellDef,
            initialTarget = new LocalTargetInfo(centerCell),
            currentTarget = triggeringPawn != null ? new LocalTargetInfo(triggeringPawn) : new LocalTargetInfo(centerCell),
            currentCell = triggeringPawn?.Position ?? centerCell,
            power = new SpellPowerContext
            {
                value = powerValue,
                tier = powerTier
            },
            randomSeed = randomSeed
        };
        context.executionState.costsApplied = true;
        context.executionState.variables = variables?.Clone() ?? new SpellVariableStore();
        context.currentTargets.Add(new LocalTargetInfo(centerCell));
        if (triggeringPawn != null)
        {
            context.currentTargets.Add(new LocalTargetInfo(triggeringPawn));
        }

        return true;
    }

    public bool TryCreateCenterExecutionContext(Map map, out SpellContext context)
    {
        context = null;
        if (spellDef == null || map == null || !centerCell.IsValid)
        {
            return false;
        }

        context = new SpellContext
        {
            caster = caster,
            map = map,
            spellDef = spellDef,
            initialTarget = new LocalTargetInfo(centerCell),
            currentTarget = new LocalTargetInfo(centerCell),
            currentCell = centerCell,
            power = new SpellPowerContext
            {
                value = powerValue,
                tier = powerTier
            },
            randomSeed = randomSeed
        };
        context.executionState.costsApplied = true;
        context.executionState.variables = variables?.Clone() ?? new SpellVariableStore();
        context.currentTargets.Add(new LocalTargetInfo(centerCell));
        return true;
    }

    public void ScheduleNextPulse()
    {
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        nextPulseTick = currentTick + (pulseIntervalTicks > 0 ? pulseIntervalTicks : 60);
    }

    public void ScheduleNextVisualPulse()
    {
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        nextVisualTick = currentTick + (visualPulseIntervalTicks > 0 ? visualPulseIntervalTicks : 30);
    }

    public void DestroyMarkers()
    {
        if (markerThings == null)
        {
            return;
        }

        foreach (Thing markerThing in markerThings)
        {
            if (markerThing != null && !markerThing.Destroyed)
            {
                markerThing.Destroy();
            }
        }
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_Values.Look(ref centerCell, "centerCell", IntVec3.Invalid);
        Scribe_Collections.Look(ref markerThings, "markerThings", LookMode.Reference);
        Scribe_Values.Look(ref randomSeed, "randomSeed");
        Scribe_Values.Look(ref powerValue, "powerValue");
        Scribe_Values.Look(ref powerTier, "powerTier");
        Scribe_Deep.Look(ref variables, "variables");
        Scribe_Collections.Look(ref actionPath, "actionPath", LookMode.Value);
        Scribe_Values.Look(ref pawnAffinity, "pawnAffinity", SpellPawnAffinity.All);
        Scribe_Values.Look(ref includeCaster, "includeCaster");
        Scribe_Values.Look(ref zoneRadius, "zoneRadius", 3f);
        Scribe_Values.Look(ref pulseIntervalTicks, "pulseIntervalTicks", 60);
        Scribe_Values.Look(ref nextPulseTick, "nextPulseTick");
        Scribe_Values.Look(ref ambientEffectDef, "ambientEffectDef");
        Scribe_Values.Look(ref ambientSoundDef, "ambientSoundDef");
        Scribe_Values.Look(ref visualPulseIntervalTicks, "visualPulseIntervalTicks", 30);
        Scribe_Values.Look(ref emitVisualFromMarkers, "emitVisualFromMarkers", true);
        Scribe_Values.Look(ref maxVisualMarkersPerPulse, "maxVisualMarkersPerPulse", -1);
        Scribe_Values.Look(ref pulseAtCenter, "pulseAtCenter");
        Scribe_Values.Look(ref requiresConcentration, "requiresConcentration");
        Scribe_Values.Look(ref breakWhenCasterDowned, "breakWhenCasterDowned", true);
        Scribe_Values.Look(ref breakWhenCasterStunned, "breakWhenCasterStunned", true);
        Scribe_Values.Look(ref breakWhenCasterMentalState, "breakWhenCasterMentalState", true);
        Scribe_Deep.Look(ref maintenance, "maintenance");
        Scribe_Values.Look(ref nextVisualTick, "nextVisualTick");
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            markerThings ??= new List<Thing>();
            actionPath ??= new List<int>();
            variables ??= new SpellVariableStore();
        }
    }
}
