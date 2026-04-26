using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Requirements;

/// <summary>
/// Base validation worker for spell casting requirements.
/// </summary>
public abstract class SpellRequirementWorker
{
    public virtual bool CanCast(SpellContext context, SpellRequirementDef requirementDef, out string reason)
    {
        reason = $"{GetType().Name} allows the cast by default.";
        Log.Message($"[MagicFramework] Requirement check {GetType().Name} passed.");
        return true;
    }
}
