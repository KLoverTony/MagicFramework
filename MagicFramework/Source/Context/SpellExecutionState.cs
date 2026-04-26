using System.Collections.Generic;
using MagicFramework.Debug;
using MagicFramework.Scheduling;

namespace MagicFramework.Context;

/// <summary>
/// Mutable state accumulated while a spell cast executes.
/// </summary>
public sealed class SpellExecutionState
{
    public bool failed;
    public bool cancelled;
    public bool costsApplied;
    public SpellVariableStore variables = new();
    public List<ScheduledSpellAction> scheduledActions = new();
    public List<SpellDebugEntry> debugHistory = new();
}
