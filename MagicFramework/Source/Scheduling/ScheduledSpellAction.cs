using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Represents delayed work scheduled by the spell framework.
/// </summary>
public sealed class ScheduledSpellAction : IExposable
{
    private List<int> actionPath = new();
    private Thing caster;
    private SpellDef spellDef;
    private int executeAtTick;
    private LocalTargetInfo initialTarget;
    private LocalTargetInfo currentTarget;
    private List<LocalTargetInfo> currentTargets = new();
    private IntVec3 currentCell = IntVec3.Invalid;
    private float powerValue;
    private int powerTier;
    private int randomSeed;
    private SpellVariableStore variables = new();

    public ScheduledSpellAction()
    {
    }

    public ScheduledSpellAction(
        int executeAtTick,
        SpellDef spellDef,
        Thing caster,
        LocalTargetInfo initialTarget,
        LocalTargetInfo currentTarget,
        IEnumerable<LocalTargetInfo> currentTargets,
        IntVec3 currentCell,
        float powerValue,
        int powerTier,
        int randomSeed,
        SpellVariableStore variables,
        IEnumerable<int> actionPath)
    {
        this.executeAtTick = executeAtTick;
        this.spellDef = spellDef;
        this.caster = caster;
        this.initialTarget = initialTarget;
        this.currentTarget = currentTarget;
        this.currentTargets = currentTargets != null ? new List<LocalTargetInfo>(currentTargets) : new List<LocalTargetInfo>();
        this.currentCell = currentCell;
        this.powerValue = powerValue;
        this.powerTier = powerTier;
        this.randomSeed = randomSeed;
        this.variables = variables?.Clone() ?? new SpellVariableStore();
        this.actionPath = actionPath != null ? new List<int>(actionPath) : new List<int>();
    }

    public int ExecuteAtTick => executeAtTick;

    public Thing Caster => caster;

    public SpellDef SpellDef => spellDef;

    public string DebugLabel => TryResolveActionDef(out SpellActionDef actionDef)
        ? actionDef.debugLabel ?? actionDef.GetType().Name
        : "<unresolved delayed action>";

    public bool TryResolveActionDef(out SpellActionDef actionDef)
    {
        actionDef = SpellActionPathUtility.ResolveAction(spellDef, actionPath);
        return actionDef != null;
    }

    public bool TryCreateExecutionContext(Map map, out SpellContext context)
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
            currentTarget = currentTarget,
            currentCell = currentCell,
            power = new SpellPowerContext
            {
                value = powerValue,
                tier = powerTier
            },
            randomSeed = randomSeed
        };
        context.executionState.costsApplied = true;
        context.executionState.variables = variables?.Clone() ?? new SpellVariableStore();

        if (currentTargets != null && currentTargets.Count > 0)
        {
            context.currentTargets.AddRange(currentTargets);
        }
        else
        {
            if (initialTarget.IsValid)
            {
                context.currentTargets.Add(initialTarget);
            }

            if (currentTarget.IsValid && !currentTarget.Equals(initialTarget))
            {
                context.currentTargets.Add(currentTarget);
            }
        }

        return true;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref executeAtTick, "executeAtTick");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_References.Look(ref caster, "caster");
        Scribe_TargetInfo.Look(ref initialTarget, "initialTarget");
        Scribe_TargetInfo.Look(ref currentTarget, "currentTarget");
        Scribe_Collections.Look(ref currentTargets, "currentTargets", LookMode.TargetInfo);
        Scribe_Values.Look(ref currentCell, "currentCell", IntVec3.Invalid);
        Scribe_Values.Look(ref powerValue, "powerValue");
        Scribe_Values.Look(ref powerTier, "powerTier");
        Scribe_Values.Look(ref randomSeed, "randomSeed");
        Scribe_Deep.Look(ref variables, "variables");
        Scribe_Collections.Look(ref actionPath, "actionPath", LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && actionPath == null)
        {
            actionPath = new List<int>();
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit && currentTargets == null)
        {
            currentTargets = new List<LocalTargetInfo>();
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit && variables == null)
        {
            variables = new SpellVariableStore();
        }
    }
}
