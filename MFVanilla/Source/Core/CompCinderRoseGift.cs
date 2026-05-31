using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class CompProperties_CinderRoseGift : CompProperties
{
    public ThoughtDef moodThought;
    public ThoughtDef socialThought;

    public CompProperties_CinderRoseGift()
    {
        compClass = typeof(CompCinderRoseGift);
    }
}

public sealed class CompCinderRoseGift : ThingComp
{
    private CompProperties_CinderRoseGift Props => (CompProperties_CinderRoseGift)props;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        if (!parent.Spawned && GetHolderPawn() == null)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "Give cinder rose",
            defaultDesc = "Give this rose to a pawn, granting a short mood lift and a social opinion bonus toward the giver.",
            icon = parent.def.uiIcon,
            action = BeginTargeting
        };
    }

    private void BeginTargeting()
    {
        TargetingParameters parameters = new TargetingParameters
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            canTargetItems = false,
            canTargetLocations = false,
            validator = target => target.Thing is Pawn pawn && CanReceiveGift(pawn).Accepted
        };

        Find.Targeter.BeginTargeting(parameters, target => GiveTo(target.Thing as Pawn));
    }

    private void GiveTo(Pawn recipient)
    {
        AcceptanceReport report = CanReceiveGift(recipient);
        if (!report.Accepted)
        {
            Messages.Message(report.Reason, recipient, MessageTypeDefOf.RejectInput, false);
            return;
        }

        Pawn giver = GetHolderPawn();
        recipient.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.moodThought);
        if (giver != null && giver != recipient)
        {
            recipient.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.socialThought, giver);
        }

        string giverText = giver == null ? "Someone" : giver.LabelShortCap;
        Messages.Message($"{giverText} gave {recipient.LabelShortCap} a cinder rose.", recipient, MessageTypeDefOf.PositiveEvent, false);
        parent.SplitOff(1).Destroy();
    }

    private static AcceptanceReport CanReceiveGift(Pawn pawn)
    {
        if (pawn == null || pawn.Destroyed || pawn.Dead)
        {
            return false;
        }

        if (!pawn.RaceProps.Humanlike)
        {
            return "Only humanlike pawns can appreciate a cinder rose.";
        }

        if (pawn.needs?.mood == null)
        {
            return $"{pawn.LabelShortCap} cannot receive this gift.";
        }

        return true;
    }

    private Pawn GetHolderPawn()
    {
        IThingHolder holder = parent.ParentHolder;
        while (holder != null)
        {
            switch (holder)
            {
                case Pawn_InventoryTracker inventory:
                    return inventory.pawn;
                case Pawn_CarryTracker carryTracker:
                    return carryTracker.pawn;
                case Pawn_ApparelTracker apparelTracker:
                    return apparelTracker.pawn;
                case Pawn_EquipmentTracker equipmentTracker:
                    return equipmentTracker.pawn;
            }

            holder = holder.ParentHolder;
        }

        return null;
    }
}
