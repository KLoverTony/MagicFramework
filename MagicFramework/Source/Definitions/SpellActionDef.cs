using System.Collections.Generic;
using MagicFramework.Actions;

namespace MagicFramework.Definitions;

/// <summary>
/// Base authored node for spell action execution.
/// </summary>
public abstract class SpellActionDef
{
    public string debugLabel;

    public virtual IEnumerable<SpellActionDef> GetChildActions()
    {
        yield break;
    }

    public abstract SpellActionWorker CreateWorker();
}
