using MagicFramework.Context;
using MagicFramework.Definitions;

namespace MagicFramework.Conditions;

/// <summary>
/// Base executable worker for authored spell conditions.
/// </summary>
public abstract class SpellConditionWorker
{
    public abstract bool Evaluate(SpellContext context, SpellConditionDef conditionDef);
}
