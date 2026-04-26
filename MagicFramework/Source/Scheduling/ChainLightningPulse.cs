using System.Collections.Generic;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

public sealed class ChainLightningPulse : IExposable
{
    private Thing caster;
    private SpellDef spellDef;
    private Thing sourceThing;
    private Thing targetThing;
    private IntVec3 sourceCell = IntVec3.Invalid;
    private IntVec3 targetCell = IntVec3.Invalid;
    private int executeAtTick;
    private int hopIndex;
    private List<int> actionPath = new();

    public ChainLightningPulse()
    {
    }

    public ChainLightningPulse(
        Thing caster,
        SpellDef spellDef,
        Thing sourceThing,
        Thing targetThing,
        IntVec3 sourceCell,
        IntVec3 targetCell,
        int executeAtTick,
        int hopIndex,
        IEnumerable<int> actionPath)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.sourceThing = sourceThing;
        this.targetThing = targetThing;
        this.sourceCell = sourceCell;
        this.targetCell = targetCell;
        this.executeAtTick = executeAtTick;
        this.hopIndex = hopIndex;
        this.actionPath = actionPath != null ? new List<int>(actionPath) : new List<int>();
    }

    public Thing Caster => caster;

    public SpellDef SpellDef => spellDef;

    public Thing SourceThing => sourceThing;

    public Thing TargetThing => targetThing;

    public IntVec3 SourceCell => sourceCell;

    public IntVec3 TargetCell => targetCell;

    public int ExecuteAtTick => executeAtTick;

    public int HopIndex => hopIndex;

    public IReadOnlyList<int> ActionPath => actionPath;

    public void ExposeData()
    {
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_References.Look(ref sourceThing, "sourceThing");
        Scribe_References.Look(ref targetThing, "targetThing");
        Scribe_Values.Look(ref sourceCell, "sourceCell", IntVec3.Invalid);
        Scribe_Values.Look(ref targetCell, "targetCell", IntVec3.Invalid);
        Scribe_Values.Look(ref executeAtTick, "executeAtTick");
        Scribe_Values.Look(ref hopIndex, "hopIndex");
        Scribe_Collections.Look(ref actionPath, "actionPath", LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && actionPath == null)
        {
            actionPath = new List<int>();
        }
    }
}
