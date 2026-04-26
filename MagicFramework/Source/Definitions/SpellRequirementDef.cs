using MagicFramework.Requirements;

namespace MagicFramework.Definitions;

/// <summary>
/// Base authored requirement used during cast validation.
/// </summary>
public abstract class SpellRequirementDef
{
    public string debugLabel;

    public abstract SpellRequirementWorker CreateWorker();
}
