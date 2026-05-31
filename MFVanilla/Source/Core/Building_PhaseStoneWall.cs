using System.Collections.Generic;
using MagicFramework.Core;
using RimWorld;
using Verse;

namespace MFVanilla.Core;

public sealed class Building_PhaseStoneWall : Building_Door
{
    public override bool FreePassage => false;

    public override bool PawnCanOpen(Pawn p)
    {
        return p != null
            && p.CanOpenDoors
            && SpellRuntimeGameComponent.Instance?.HasArcaneGift(p) == true;
    }

    public override bool BlocksPawn(Pawn p)
    {
        return !PawnCanOpen(p);
    }

    protected override void DoorOpen(int ticksToClose = 110)
    {
        base.DoorOpen(ticksToClose);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        string holdOpenLabel = "CommandToggleDoorHoldOpen".Translate();
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            if (gizmo is Command_Toggle toggle && toggle.defaultLabel == holdOpenLabel)
            {
                continue;
            }

            yield return gizmo;
        }
    }

    public override string GetInspectString()
    {
        string text = base.GetInspectString();
        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        return text + "Arcane Gift pawns can phase through this wall.";
    }
}
