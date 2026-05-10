using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;

namespace MagicFramework.Actions;

/// <summary>
/// Base executable worker for spell actions.
/// </summary>
public abstract class SpellActionWorker
{
    public virtual void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        MagicLog.Message(MagicLogSubsystem.Execution, $"[MagicFramework] Stub action worker {GetType().Name} executed.");
    }
}
