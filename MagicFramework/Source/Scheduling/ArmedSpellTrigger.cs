using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Represents an armed persistent trigger waiting for a runtime condition before executing spell actions.
/// </summary>
public sealed class ArmedSpellTrigger : IExposable
{
    private List<int> actionPath = new();
    private Thing caster;
    private SpellDef spellDef;
    private LocalTargetInfo initialTarget;
    private LocalTargetInfo currentTarget;
    private List<LocalTargetInfo> currentTargets = new();
    private IntVec3 currentCell = IntVec3.Invalid;
    private int randomSeed;
    private SpellVariableStore variables = new();
    private float triggerRadius;
    private SpellPawnAffinity pawnAffinity = SpellPawnAffinity.Foe;
    private bool includeCaster;
    private int checkIntervalTicks = 15;
    private int nextCheckTick;

    public ArmedSpellTrigger()
    {
    }

    public ArmedSpellTrigger(
        SpellDef spellDef,
        Thing caster,
        LocalTargetInfo initialTarget,
        LocalTargetInfo currentTarget,
        IEnumerable<LocalTargetInfo> currentTargets,
        IntVec3 currentCell,
        int randomSeed,
        SpellVariableStore variables,
        IEnumerable<int> actionPath,
        float triggerRadius,
        SpellPawnAffinity pawnAffinity,
        bool includeCaster,
        int checkIntervalTicks)
    {
        this.spellDef = spellDef;
        this.caster = caster;
        this.initialTarget = initialTarget;
        this.currentTarget = currentTarget;
        this.currentTargets = currentTargets != null ? new List<LocalTargetInfo>(currentTargets) : new List<LocalTargetInfo>();
        this.currentCell = currentCell;
        this.randomSeed = randomSeed;
        this.variables = variables?.Clone() ?? new SpellVariableStore();
        this.actionPath = actionPath != null ? new List<int>(actionPath) : new List<int>();
        this.triggerRadius = triggerRadius;
        this.pawnAffinity = pawnAffinity;
        this.includeCaster = includeCaster;
        this.checkIntervalTicks = checkIntervalTicks > 0 ? checkIntervalTicks : 15;
        nextCheckTick = Find.TickManager?.TicksGame ?? 0;
    }

    public Thing Caster => caster;

    public SpellDef SpellDef => spellDef;

    public IntVec3 ArmedCell => currentCell;

    public float TriggerRadius => triggerRadius;

    public SpellPawnAffinity PawnAffinity => pawnAffinity;

    public bool IncludeCaster => includeCaster;

    public int NextCheckTick => nextCheckTick;

    public string DebugLabel => TryResolveActionDef(out ProximityTriggerActionDef actionDef)
        ? actionDef.debugLabel ?? actionDef.GetType().Name
        : "<unresolved armed trigger>";

    public bool TryResolveActionDef(out ProximityTriggerActionDef actionDef)
    {
        actionDef = SpellActionPathUtility.ResolveAction(spellDef, actionPath) as ProximityTriggerActionDef;
        return actionDef != null;
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
            initialTarget = initialTarget,
            currentTarget = triggeringPawn != null ? new LocalTargetInfo(triggeringPawn) : currentTarget,
            currentCell = currentCell,
            randomSeed = randomSeed
        };

        context.executionState.costsApplied = true;
        context.executionState.variables = variables?.Clone() ?? new SpellVariableStore();

        if (currentTargets != null && currentTargets.Count > 0)
        {
            context.currentTargets.AddRange(currentTargets);
        }

        if (triggeringPawn != null)
        {
            context.currentTarget = new LocalTargetInfo(triggeringPawn);
            if (!context.currentTargets.Contains(context.currentTarget))
            {
                context.currentTargets.Add(context.currentTarget);
            }

            // Keep the trap anchored to the armed cell even though a pawn tripped it.
            context.currentCell = currentCell;
        }

        return true;
    }

    public void ScheduleNextCheck()
    {
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        nextCheckTick = currentTick + (checkIntervalTicks > 0 ? checkIntervalTicks : 15);
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_References.Look(ref caster, "caster");
        Scribe_TargetInfo.Look(ref initialTarget, "initialTarget");
        Scribe_TargetInfo.Look(ref currentTarget, "currentTarget");
        Scribe_Collections.Look(ref currentTargets, "currentTargets", LookMode.TargetInfo);
        Scribe_Values.Look(ref currentCell, "currentCell", IntVec3.Invalid);
        Scribe_Values.Look(ref randomSeed, "randomSeed");
        Scribe_Deep.Look(ref variables, "variables");
        Scribe_Collections.Look(ref actionPath, "actionPath", LookMode.Value);
        Scribe_Values.Look(ref triggerRadius, "triggerRadius");
        Scribe_Values.Look(ref pawnAffinity, "pawnAffinity", SpellPawnAffinity.Foe);
        Scribe_Values.Look(ref includeCaster, "includeCaster");
        Scribe_Values.Look(ref checkIntervalTicks, "checkIntervalTicks", 15);
        Scribe_Values.Look(ref nextCheckTick, "nextCheckTick");

        if (Scribe.mode == LoadSaveMode.PostLoadInit && currentTargets == null)
        {
            currentTargets = new List<LocalTargetInfo>();
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit && actionPath == null)
        {
            actionPath = new List<int>();
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit && variables == null)
        {
            variables = new SpellVariableStore();
        }
    }
}
