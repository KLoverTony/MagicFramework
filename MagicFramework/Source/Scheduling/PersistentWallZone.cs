using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Represents a persistent wall-shaped hazardous zone that can pulse spell actions over time.
/// </summary>
public sealed class PersistentWallZone : IExposable
{
    private List<int> actionPath = new();
    private Thing caster;
    private SpellDef spellDef;
    private IntVec3 anchorCell = IntVec3.Invalid;
    private List<IntVec3> wallCells = new();
    private List<Thing> markerThings = new();
    private int randomSeed;
    private SpellVariableStore variables = new();
    private SpellPawnAffinity pawnAffinity = SpellPawnAffinity.All;
    private bool includeCaster;
    private float pulseRadius = 0.9f;
    private int pulseIntervalTicks = 60;
    private int nextPulseTick;
    private int expireAtTick = -1;

    public PersistentWallZone()
    {
    }

    public PersistentWallZone(
        Thing caster,
        SpellDef spellDef,
        IntVec3 anchorCell,
        IEnumerable<IntVec3> wallCells,
        IEnumerable<Thing> markerThings,
        int randomSeed,
        SpellVariableStore variables,
        IEnumerable<int> actionPath,
        SpellPawnAffinity pawnAffinity,
        bool includeCaster,
        float pulseRadius,
        int pulseIntervalTicks,
        int expireAtTick)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.anchorCell = anchorCell;
        this.wallCells = wallCells != null ? new List<IntVec3>(wallCells) : new List<IntVec3>();
        this.markerThings = markerThings != null ? new List<Thing>(markerThings) : new List<Thing>();
        this.randomSeed = randomSeed;
        this.variables = variables?.Clone() ?? new SpellVariableStore();
        this.actionPath = actionPath != null ? new List<int>(actionPath) : new List<int>();
        this.pawnAffinity = pawnAffinity;
        this.includeCaster = includeCaster;
        this.pulseRadius = pulseRadius;
        this.pulseIntervalTicks = pulseIntervalTicks > 0 ? pulseIntervalTicks : 60;
        nextPulseTick = Find.TickManager?.TicksGame ?? 0;
        this.expireAtTick = expireAtTick;
    }

    public Thing Caster => caster;
    public SpellDef SpellDef => spellDef;
    public IntVec3 AnchorCell => anchorCell;
    public IReadOnlyList<IntVec3> WallCells => wallCells;
    public IReadOnlyList<Thing> MarkerThings => markerThings;
    public SpellPawnAffinity PawnAffinity => pawnAffinity;
    public bool IncludeCaster => includeCaster;
    public float PulseRadius => pulseRadius;
    public int NextPulseTick => nextPulseTick;
    public int ExpireAtTick => expireAtTick;

    public string DebugLabel => TryResolveActionDef(out PersistentWallZoneActionDef actionDef)
        ? actionDef.debugLabel ?? actionDef.GetType().Name
        : "<unresolved wall zone>";

    public bool TryResolveActionDef(out PersistentWallZoneActionDef actionDef)
    {
        actionDef = SpellActionPathUtility.ResolveAction(spellDef, actionPath) as PersistentWallZoneActionDef;
        return actionDef != null;
    }

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public bool TryCreateExecutionContext(Map map, Pawn triggeringPawn, IntVec3 sourceCell, out SpellContext context)
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
            initialTarget = new LocalTargetInfo(anchorCell),
            currentTarget = triggeringPawn != null ? new LocalTargetInfo(triggeringPawn) : new LocalTargetInfo(sourceCell),
            currentCell = sourceCell,
            randomSeed = randomSeed
        };
        context.executionState.costsApplied = true;
        context.executionState.variables = variables?.Clone() ?? new SpellVariableStore();
        context.currentTargets.Add(new LocalTargetInfo(sourceCell));
        if (triggeringPawn != null)
        {
            context.currentTargets.Add(new LocalTargetInfo(triggeringPawn));
        }

        return true;
    }

    public bool TryCreateCenterExecutionContext(Map map, out SpellContext context)
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
            initialTarget = new LocalTargetInfo(anchorCell),
            currentTarget = new LocalTargetInfo(anchorCell),
            currentCell = anchorCell,
            randomSeed = randomSeed
        };
        context.executionState.costsApplied = true;
        context.executionState.variables = variables?.Clone() ?? new SpellVariableStore();
        context.currentTargets.Add(new LocalTargetInfo(anchorCell));
        return true;
    }

    public void ScheduleNextPulse()
    {
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        nextPulseTick = currentTick + (pulseIntervalTicks > 0 ? pulseIntervalTicks : 60);
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
        Scribe_Values.Look(ref anchorCell, "anchorCell", IntVec3.Invalid);
        Scribe_Collections.Look(ref wallCells, "wallCells", LookMode.Value);
        Scribe_Collections.Look(ref markerThings, "markerThings", LookMode.Reference);
        Scribe_Values.Look(ref randomSeed, "randomSeed");
        Scribe_Deep.Look(ref variables, "variables");
        Scribe_Collections.Look(ref actionPath, "actionPath", LookMode.Value);
        Scribe_Values.Look(ref pawnAffinity, "pawnAffinity", SpellPawnAffinity.All);
        Scribe_Values.Look(ref includeCaster, "includeCaster");
        Scribe_Values.Look(ref pulseRadius, "pulseRadius", 0.9f);
        Scribe_Values.Look(ref pulseIntervalTicks, "pulseIntervalTicks", 60);
        Scribe_Values.Look(ref nextPulseTick, "nextPulseTick");
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            wallCells ??= new List<IntVec3>();
            markerThings ??= new List<Thing>();
            actionPath ??= new List<int>();
            variables ??= new SpellVariableStore();
        }
    }
}
