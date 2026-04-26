using System.Collections.Generic;
using MagicFramework.Conditions;

namespace MagicFramework.Definitions;

/// <summary>
/// Base authored node for conditional spell branching.
/// </summary>
public abstract class SpellConditionDef
{
    public virtual IEnumerable<SpellConditionDef> GetChildConditions()
    {
        yield break;
    }

    public abstract SpellConditionWorker CreateWorker();
}

public enum SpellConditionTargetSource
{
    CurrentTarget,
    InitialTarget,
    Caster
}

public enum SpellConditionCellSource
{
    CurrentCell,
    CurrentTargetCell,
    InitialTargetCell,
    CasterCell
}
