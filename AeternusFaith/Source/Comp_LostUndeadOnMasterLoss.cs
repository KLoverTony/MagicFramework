using System;
using MagicFramework.PawnLifecycle;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class CompProperties_LostUndeadOnMasterLoss : CompProperties
    {
        public string lostLabel = "lost undead";
        public int graceTicks = 7500;
        public float rampageRadius = 40f;

        public CompProperties_LostUndeadOnMasterLoss()
        {
            compClass = typeof(Comp_LostUndeadOnMasterLoss);
        }
    }

    public class Comp_LostUndeadOnMasterLoss : ThingComp
    {
        private CompProperties_LostUndeadOnMasterLoss Props => (CompProperties_LostUndeadOnMasterLoss)props;
        private Pawn Pawn => parent as Pawn;
        private int masterLossStartedTick = -1;
        private bool lost;

        public bool IsLost => lost;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref masterLossStartedTick, "masterLossStartedTick", -1);
            Scribe_Values.Look(ref lost, "lost");
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            Pawn pawn = Pawn;
            if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned)
                return;

            if (lost)
            {
                TryRampage(pawn);
                return;
            }

            CompPawnLifecycleEnforcer lifecycleComp = pawn.GetComp<CompPawnLifecycleEnforcer>();
            Pawn master = lifecycleComp?.Master;
            if (master == null)
                return;

            if (!master.Destroyed && !master.Dead)
            {
                masterLossStartedTick = -1;
                return;
            }

            int currentTick = Find.TickManager?.TicksGame ?? 0;
            if (masterLossStartedTick < 0)
            {
                masterLossStartedTick = currentTick;
                return;
            }

            if (currentTick - masterLossStartedTick >= Math.Max(0, Props.graceTicks))
                BecomeLost(pawn, lifecycleComp);
        }

        public override string CompInspectStringExtra()
        {
            string baseText = base.CompInspectStringExtra();
            if (lost)
                return AppendInspectLine(baseText, "Lost undead: " + Props.lostLabel.CapitalizeFirst());

            if (masterLossStartedTick < 0)
                return baseText;

            int currentTick = Find.TickManager?.TicksGame ?? 0;
            int remainingTicks = Math.Max(0, Props.graceTicks - (currentTick - masterLossStartedTick));
            return AppendInspectLine(baseText, "Binding failing: " + remainingTicks.ToStringTicksToPeriod());
        }

        private void BecomeLost(Pawn pawn, CompPawnLifecycleEnforcer lifecycleComp)
        {
            lost = true;
            masterLossStartedTick = -1;
            string oldLabel = pawn.LabelShort;
            pawn.Name = new NameSingle(ResolveLostName(oldLabel));
            lifecycleComp?.AssignMaster(null);
            pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            TryAssignHostileFaction(pawn);
            InterruptCurrentJob(pawn);
            Messages.Message(oldLabel + " has become " + pawn.LabelShortCap + ".", pawn, MessageTypeDefOf.ThreatBig, historical: true);
            TryRampage(pawn);
        }

        private string ResolveLostName(string oldLabel)
        {
            string sourceName = oldLabel;
            int ofIndex = oldLabel?.IndexOf(" of ", StringComparison.OrdinalIgnoreCase) ?? -1;
            if (ofIndex >= 0 && ofIndex + 4 < oldLabel.Length)
                sourceName = oldLabel.Substring(ofIndex + 4);

            return Props.lostLabel.CapitalizeFirst() + " of " + (sourceName.NullOrEmpty() ? "the dead" : sourceName);
        }

        private static void TryAssignHostileFaction(Pawn pawn)
        {
            FactionDef hostileDef = DefDatabase<FactionDef>.GetNamedSilentFail("AncientsHostile");
            Faction hostileFaction = hostileDef != null ? Find.FactionManager.FirstFactionOfDef(hostileDef) : null;
            if (hostileFaction != null && pawn.Faction != hostileFaction)
                pawn.SetFaction(hostileFaction);
        }

        private void TryRampage(Pawn pawn)
        {
            Pawn target = FindRampageTarget(pawn);
            if (target == null)
                return;

            LocalTargetInfo currentTarget = pawn.CurJob?.GetTarget(TargetIndex.A) ?? LocalTargetInfo.Invalid;
            if (pawn.CurJobDef == JobDefOf.AttackMelee && currentTarget.Thing == target)
                return;

            InterruptCurrentJob(pawn);
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
            pawn.jobs?.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true);
        }

        private Pawn FindRampageTarget(Pawn pawn)
        {
            if (pawn?.Map?.mapPawns?.AllPawnsSpawned == null)
                return null;

            Pawn bestTarget = null;
            float bestDistanceSquared = Props.rampageRadius * Props.rampageRadius;
            foreach (Pawn candidate in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (!IsValidRampageTarget(pawn, candidate))
                    continue;

                float distanceSquared = pawn.Position.DistanceToSquared(candidate.Position);
                if (distanceSquared < bestDistanceSquared)
                {
                    bestTarget = candidate;
                    bestDistanceSquared = distanceSquared;
                }
            }

            return bestTarget;
        }

        private static bool IsValidRampageTarget(Pawn pawn, Pawn candidate)
        {
            return candidate != null &&
                   candidate != pawn &&
                   !candidate.Dead &&
                   candidate.Spawned &&
                   candidate.Map == pawn.Map &&
                   !PawnLifecycleUtility.IsUndead(candidate) &&
                   candidate.RaceProps?.FleshType != null &&
                   pawn.CanReach(candidate, PathEndMode.Touch, Danger.Deadly);
        }

        private static void InterruptCurrentJob(Pawn pawn)
        {
            if (pawn?.jobs == null)
                return;

            if (pawn.carryTracker?.CarriedThing != null)
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);

            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            pawn.pather?.StopDead();
        }

        private static string AppendInspectLine(string baseText, string line)
        {
            return baseText.NullOrEmpty() ? line : baseText + "\n" + line;
        }
    }
}
