using Verse;

namespace MagicFramework.PawnLifecycle;

public class CompProperties_PawnLifecycleEnforcer : CompProperties
{
    public CompProperties_PawnLifecycleEnforcer()
    {
        compClass = typeof(CompPawnLifecycleEnforcer);
    }
}

public class CompPawnLifecycleEnforcer : ThingComp
{
    private Pawn Pawn => parent as Pawn;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        PawnLifecycleEnforcementUtility.EnforceAll(Pawn);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            PawnLifecycleEnforcementUtility.EnforceAll(Pawn);
        }
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
        PawnLifecycleEnforcementUtility.EnforceRecurring(Pawn);
    }
}
