using System.Collections.Generic;
using Verse;

namespace MagicFramework.Definitions;

public sealed class SpellMaintenanceDef : IExposable
{
    public List<SpellMaintenanceProfile> profiles = new();
    public float maxRange = -1f;
    public bool useInitialTargetCell;

    public void ExposeData()
    {
        Scribe_Collections.Look(ref profiles, "profiles", LookMode.Value);
        Scribe_Values.Look(ref maxRange, "maxRange", -1f);
        Scribe_Values.Look(ref useInitialTargetCell, "useInitialTargetCell");

        if (Scribe.mode == LoadSaveMode.PostLoadInit && profiles == null)
        {
            profiles = new List<SpellMaintenanceProfile>();
        }
    }
}

public enum SpellMaintenanceProfile
{
    CasterValid,
    CasterConscious,
    CasterFocused,
    TargetValid,
    TargetConscious,
    Tethered,
    LineOfSight,
    Anchored
}
