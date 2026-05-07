using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class Building_FilledAlcoveWall : Building
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn))
                yield return option;

            if (!CanBeVisited)
                yield break;

            if (selPawn == null || selPawn.Dead || selPawn.Downed)
                yield break;

            if (!selPawn.CanReserveAndReach(this, PathEndMode.Touch, Danger.Deadly))
            {
                yield return new FloatMenuOption("Cannot visit filled alcove: no path", null);
                yield break;
            }

            yield return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption("Visit filled alcove", () =>
                {
                    Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("AF_VisitFilledAlcove"), this);
                    job.playerForced = true;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }),
                selPawn,
                this);
        }

        public bool CanBeVisited => GetComp<Comp_OssuaryObituary>()?.HasRemains == true;
    }

    public class JobDriver_VisitFilledAlcove : JobDriver
    {
        private const TargetIndex AlcoveInd = TargetIndex.A;
        private const int VisitTicks = 4000;

        private Thing Alcove => job.GetTarget(AlcoveInd).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(AlcoveInd), job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(AlcoveInd);

            yield return Toils_Reserve.Reserve(AlcoveInd);
            yield return Toils_Goto.GotoThing(AlcoveInd, PathEndMode.Touch);

            Toil visit = Toils_General.WaitWith(AlcoveInd, VisitTicks, useProgressBar: true, face: AlcoveInd);
            visit.tickAction = () =>
            {
                if (pawn.needs?.joy != null)
                    pawn.needs.joy.GainJoy(0.000144f * job.def.joyGainRate, job.def.joyKind);
            };
            yield return visit;
        }
    }

    public class JoyGiver_VisitFilledAlcove : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn?.Map == null)
                return null;

            foreach (Thing alcove in FilledAlcoves(pawn.Map).InRandomOrder())
            {
                if (alcove is not Building_FilledAlcoveWall filledAlcove || !filledAlcove.CanBeVisited)
                    continue;

                if (!pawn.CanReserveAndReach(alcove, PathEndMode.Touch, Danger.Deadly))
                    continue;

                return JobMaker.MakeJob(def.jobDef, alcove);
            }

            return null;
        }

        private static IEnumerable<Thing> FilledAlcoves(Map map)
        {
            ThingDef granite = DefDatabase<ThingDef>.GetNamedSilentFail("AF_AlcoveWallFilledGranite");
            ThingDef slate = DefDatabase<ThingDef>.GetNamedSilentFail("AF_AlcoveWallFilledSlate");

            if (granite != null)
            {
                foreach (Thing thing in map.listerThings.ThingsOfDef(granite))
                    yield return thing;
            }

            if (slate != null)
            {
                foreach (Thing thing in map.listerThings.ThingsOfDef(slate))
                    yield return thing;
            }
        }
    }
}
