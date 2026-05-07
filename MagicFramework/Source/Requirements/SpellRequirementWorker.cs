using MagicFramework.Context;
using MagicFramework.Definitions;

namespace MagicFramework.Requirements;

/// <summary>
/// Base validation worker for spell learning and casting requirements.
/// </summary>
public abstract class SpellRequirementWorker
{
    public virtual bool CanLearn(SpellContext context, SpellRequirementDef requirementDef, out string reason)
    {
        reason = null;
        return true;
    }

    public virtual bool CanCast(SpellContext context, SpellRequirementDef requirementDef, out string reason)
    {
        reason = null;
        return true;
    }
}
