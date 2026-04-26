using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Actions;

/// <summary>
/// Base executable worker for spell actions.
/// </summary>
public abstract class SpellActionWorker
{
    public virtual void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        Log.Message($"[MagicFramework] Stub action worker {GetType().Name} executed.");
    }
}
