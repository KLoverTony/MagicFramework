using System.Text;
using MagicFramework.Scheduling;
using RimWorld;
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

    public override string CompInspectStringExtra()
    {
        string baseText = base.CompInspectStringExtra();
        Pawn pawn = Pawn;
        PawnLifecycleExtension extension = PawnLifecycleUtility.GetLifecycle(pawn);
        if (pawn == null || extension == null)
        {
            return baseText;
        }

        StringBuilder builder = new();
        if (!baseText.NullOrEmpty())
        {
            builder.AppendLine(baseText);
        }

        builder.Append("Lifecycle: ");
        builder.Append(FormatEnumName(extension.controlPolicy));
        if (extension.workPolicy != PawnLifecycleWorkPolicy.Unspecified)
        {
            builder.Append(", ");
            builder.Append(FormatEnumName(extension.workPolicy));
        }

        if (master != null && !master.Destroyed)
        {
            builder.AppendLine();
            builder.Append("Master: ");
            builder.Append(master.LabelShortCap);
        }

        if (TryGetRemainingSummonTicks(pawn, out int remainingTicks))
        {
            builder.AppendLine();
            builder.Append("Expires in: ");
            builder.Append(remainingTicks.ToStringTicksToPeriod());
        }

        return builder.ToString().TrimEnd();
    }

    private static bool TryGetRemainingSummonTicks(Pawn pawn, out int remainingTicks)
    {
        remainingTicks = 0;
        SummonedPawnMapComponent component = pawn?.MapHeld?.GetComponent<SummonedPawnMapComponent>()
            ?? pawn?.Map?.GetComponent<SummonedPawnMapComponent>();
        if (component == null || !component.TryGetRecord(pawn, out SummonedPawnRecord record) || record.ExpireAtTick < 0)
        {
            return false;
        }

        remainingTicks = System.Math.Max(0, record.ExpireAtTick - (Find.TickManager?.TicksGame ?? 0));
        return true;
    }

    private static string FormatEnumName<TEnum>(TEnum value)
        where TEnum : struct
    {
        string text = value.ToString();
        if (text == "Unspecified")
        {
            return "unspecified";
        }

        StringBuilder builder = new();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (i > 0 && char.IsUpper(c))
            {
                builder.Append(' ');
            }

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }
}
