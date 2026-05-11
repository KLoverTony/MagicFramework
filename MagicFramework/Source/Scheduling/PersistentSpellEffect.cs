using MagicFramework.Definitions;
using MagicFramework.Context;
using System.Collections.Generic;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Represents a spawned persistent spell marker that can expire or be cleaned up by linked runtime events.
/// </summary>
public sealed class PersistentSpellEffect : IExposable
{
    private List<int> actionPath = new();
    private Thing caster;
    private SpellDef spellDef;
    private Thing markerThing;
    private IntVec3 cell = IntVec3.Invalid;
    private int randomSeed;
    private SpellVariableStore variables = new();
    private int expireAtTick = -1;

    public PersistentSpellEffect()
    {
    }

    public PersistentSpellEffect(
        Thing caster,
        SpellDef spellDef,
        Thing markerThing,
        IntVec3 cell,
        int randomSeed,
        SpellVariableStore variables,
        IEnumerable<int> actionPath,
        int expireAtTick)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.markerThing = markerThing;
        this.cell = cell;
        this.randomSeed = randomSeed;
        this.variables = variables?.Clone() ?? new SpellVariableStore();
        this.actionPath = actionPath != null ? new List<int>(actionPath) : new List<int>();
        this.expireAtTick = expireAtTick;
    }

    public Thing Caster => caster;

    public SpellDef SpellDef => spellDef;

    public Thing MarkerThing => markerThing;

    public IntVec3 Cell => cell;

    public int ExpireAtTick => expireAtTick;

    public bool TryResolveActionDef(out PersistentEffectActionDef actionDef)
    {
        actionDef = SpellActionPathUtility.ResolveAction(spellDef, actionPath) as PersistentEffectActionDef;
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
            initialTarget = new LocalTargetInfo(cell),
            currentTarget = markerThing != null && !markerThing.Destroyed ? new LocalTargetInfo(markerThing) : new LocalTargetInfo(cell),
            currentCell = cell,
            randomSeed = randomSeed
        };
        context.executionState.costsApplied = true;
        context.executionState.variables = variables?.Clone() ?? new SpellVariableStore();
        context.currentTargets.Add(new LocalTargetInfo(cell));
        if (markerThing != null && !markerThing.Destroyed)
        {
            context.currentTargets.Add(new LocalTargetInfo(markerThing));
        }

        return true;
    }

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_References.Look(ref markerThing, "markerThing");
        Scribe_Values.Look(ref cell, "cell", IntVec3.Invalid);
        Scribe_Values.Look(ref randomSeed, "randomSeed");
        Scribe_Deep.Look(ref variables, "variables");
        Scribe_Collections.Look(ref actionPath, "actionPath", LookMode.Value);
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            variables ??= new SpellVariableStore();
            actionPath ??= new List<int>();
        }
    }
}
