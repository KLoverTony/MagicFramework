using System.Linq;
using RimWorld;
using Verse;

namespace MFVanilla.Core;

public sealed class Hediff_IllusoryReinforcement : HediffWithComps
{
    public override void Tick()
    {
        base.Tick();

        if (pawn == null || pawn.Destroyed || !pawn.Spawned)
        {
            return;
        }

        if (Find.TickManager.TicksGame % 10 != 0)
        {
            return;
        }

        if (pawn.health?.hediffSet?.hediffs?.Any(hediff => hediff is Hediff_Injury) == true)
        {
            FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastAreaEffect, 0.8f);
            pawn.DeSpawn(DestroyMode.Vanish);
            pawn.Destroy(DestroyMode.Vanish);
        }
    }
}
