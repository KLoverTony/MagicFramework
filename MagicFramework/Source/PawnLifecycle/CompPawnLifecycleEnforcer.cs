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
    private Pawn master;
    private string masterThingId;
    private bool followMasterWhileDrafted = true;
    private bool followMasterWhileFieldwork = true;

    public Pawn Master => master;
    public bool FollowMasterWhileDrafted => followMasterWhileDrafted;
    public bool FollowMasterWhileFieldwork => followMasterWhileFieldwork;

    public void AssignMaster(Pawn newMaster, bool followDrafted = true, bool followFieldwork = true)
    {
        master = newMaster;
        masterThingId = newMaster?.ThingID;
        followMasterWhileDrafted = followDrafted;
        followMasterWhileFieldwork = followFieldwork;
        PawnLifecycleEnforcementUtility.EnforceControlPolicy(Pawn);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        PawnLifecycleEnforcementUtility.EnforceAll(Pawn);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref master, "lifecycleMaster");
        Scribe_Values.Look(ref masterThingId, "lifecycleMasterThingId");
        Scribe_Values.Look(ref followMasterWhileDrafted, "followMasterWhileDrafted", true);
        Scribe_Values.Look(ref followMasterWhileFieldwork, "followMasterWhileFieldwork", true);
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
