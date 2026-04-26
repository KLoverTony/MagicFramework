using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Targeting;

/// <summary>
/// Base worker for resolving runtime targets during spell execution.
/// </summary>
public abstract class TargetQueryWorker
{
    public virtual IReadOnlyList<LocalTargetInfo> ResolveTargets(SpellContext context, TargetQueryDef queryDef)
    {
        return new List<LocalTargetInfo>();
    }
}
