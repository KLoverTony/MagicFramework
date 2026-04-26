using System.Collections.Generic;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Context;

/// <summary>
/// Shared execution context for a single spell cast.
/// </summary>
public sealed class SpellContext
{
    public Thing caster;
    public Map map;
    public SpellDef spellDef;
    public LocalTargetInfo initialTarget;
    public LocalTargetInfo currentTarget;
    public IntVec3 currentCell = IntVec3.Invalid;
    public List<LocalTargetInfo> currentTargets = new();
    public SpellExecutionState executionState = new();
    public SpellPowerContext power = new();
    public int randomSeed;

    public void SetCurrentTarget(LocalTargetInfo target)
    {
        currentTarget = target;

        if (target.IsValid)
        {
            currentCell = target.Cell;
        }
    }
}

/// <summary>
/// Computed spell power values for this cast.
/// </summary>
public sealed class SpellPowerContext
{
    public float value;
    public int tier;
}
