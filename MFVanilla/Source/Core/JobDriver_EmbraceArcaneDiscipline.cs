using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MFVanilla.Core;

public sealed class JobDriver_EmbraceArcaneDiscipline : JobDriver
{
    private const TargetIndex MarkerInd = TargetIndex.A;

    private Thing Marker => job.GetTarget(MarkerInd).Thing;
    private CompArcaneDisciplineRitualMarker MarkerComp => Marker?.TryGetComp<CompArcaneDisciplineRitualMarker>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.GetTarget(MarkerInd), job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(MarkerInd);
        this.FailOn(() => MarkerComp == null);
        this.FailOn(() => MarkerComp.ActivePawn != pawn);
        this.FailOn(() => MarkerComp.ActiveDiscipline == null);

        yield return Toils_Reserve.Reserve(MarkerInd);
        yield return Toils_Goto.GotoThing(MarkerInd, PathEndMode.InteractionCell);

        Toil rite = Toils_General.WaitWith(MarkerInd, MarkerComp?.RitualTicksForJob() ?? 600, useProgressBar: true, face: MarkerInd);
        rite.WithEffect(EffecterDefOf.Research, MarkerInd);
        yield return rite;

        yield return Toils_General.DoAtomic(() => MarkerComp?.FinishRitual(pawn));
    }
}
