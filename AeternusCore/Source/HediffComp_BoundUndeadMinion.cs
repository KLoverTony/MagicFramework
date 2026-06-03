using System;
using MagicFramework.PawnLifecycle;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class HediffCompProperties_BoundUndeadMinion : HediffCompProperties
    {
        public int graceTicks = 7500;
        public float rampageRadius = 40f;

        public HediffCompProperties_BoundUndeadMinion()
        {
            compClass = typeof(HediffComp_BoundUndeadMinion);
        }
    }

    public class HediffComp_BoundUndeadMinion : HediffComp
    {
        private HediffCompProperties_BoundUndeadMinion Props => (HediffCompProperties_BoundUndeadMinion)props;
        private Pawn master;
        private string masterThingId;
        private int masterLossStartedTick = -1;
        private int nextCheckTick;
        private bool lost;

        public Pawn Master => master;
        public bool IsLost => lost;

        public void AssignMaster(Pawn newMaster)
        {
            if (newMaster != null && master == newMaster && !lost)
                return;

            master = newMaster;
            masterThingId = newMaster?.ThingID;
            masterLossStartedTick = -1;
            lost = false;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref master, "boundMaster");
            Scribe_Values.Look(ref masterThingId, "boundMasterThingId");
            Scribe_Values.Look(ref masterLossStartedTick, "masterLossStartedTick", -1);
            Scribe_Values.Look(ref nextCheckTick, "nextCheckTick");
            Scribe_Values.Look(ref lost, "lost");
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            Pawn pawn = Pawn;
            if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned)
                return;

            int currentTick = Find.TickManager?.TicksGame ?? 0;
            if (currentTick < nextCheckTick)
                return;

            nextCheckTick = currentTick + 60;
            if (lost)
            {
                TryRampage(pawn);
                return;
            }

            if (master == null)
            {
                masterLossStartedTick = -1;
                return;
            }

            if (!master.Destroyed && !master.Dead)
            {
                masterLossStartedTick = -1;
                return;
            }

            if (masterLossStartedTick < 0)
            {
                masterLossStartedTick = currentTick;
                return;
            }

            if (currentTick - masterLossStartedTick >= Math.Max(0, Props.graceTicks))
                BecomeLost(pawn);
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (lost)
                    return LostLabelFor(Pawn).CapitalizeFirst();

                if (master != null && !master.Destroyed && !master.Dead)
                    return "bound to " + master.LabelShortCap;

                if (masterLossStartedTick >= 0)
                {
                    int currentTick = Find.TickManager?.TicksGame ?? 0;
                    int remainingTicks = Math.Max(0, Props.graceTicks - (currentTick - masterLossStartedTick));
                    return "binding failing: " + remainingTicks.ToStringTicksToPeriod();
                }

                return null;
            }
        }

        private void BecomeLost(Pawn pawn)
        {
            lost = true;
            masterLossStartedTick = -1;
            string oldLabel = pawn.LabelShort;
            string lostName = LostLabelFor(pawn).CapitalizeFirst();
            pawn.Name = new NameSingle(lostName + " of " + SourceNameFrom(oldLabel));
            pawn.GetComp<CompPawnLifecycleEnforcer>()?.AssignMaster(null);
            master = null;
            masterThingId = null;
            if (pawn.playerSettings != null)
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            TryAssignHostileFaction(pawn);
            InterruptCurrentJob(pawn);
            Messages.Message(oldLabel + " has become " + pawn.LabelShortCap + ".", pawn, MessageTypeDefOf.ThreatBig, historical: true);
            TryRampage(pawn);
        }

        private static string LostLabelFor(Pawn pawn)
        {
            string defName = pawn?.def?.defName;
            return defName switch
            {
                "AF_SkeletonRace" => "hollowborn",
                "AF_EchoboundRevenantRace" => "fractured echobound",
                "AF_ReliquaryWardenRace" => "wailwright",
                "AF_SpectreRace" => "errant soul",
                _ => "lost undead"
            };
        }

        private static string SourceNameFrom(string oldLabel)
        {
            int ofIndex = oldLabel?.IndexOf(" of ", StringComparison.OrdinalIgnoreCase) ?? -1;
            if (ofIndex >= 0 && ofIndex + 4 < oldLabel.Length)
                return oldLabel.Substring(ofIndex + 4);

            return oldLabel.NullOrEmpty() ? "the dead" : oldLabel;
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
    }

    public static class BoundUndeadMinionUtility
    {
        private const string BoundMinionHediffDefName = "AF_BoundUndeadMinion";

        public static void AssignMaster(Pawn minion, Pawn master)
        {
            if (minion?.health == null || master == null)
                return;

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(BoundMinionHediffDefName);
            if (hediffDef == null)
            {
                Log.ErrorOnce("[AeternusFaith] Could not find AF_BoundUndeadMinion HediffDef.", 1964217501);
                return;
            }

            Hediff hediff = minion.health.hediffSet?.GetFirstHediffOfDef(hediffDef);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(hediffDef, minion);
                hediff.Severity = 1f;
                minion.health.AddHediff(hediff);
            }
            else if (hediff.Severity <= 0f)
            {
                hediff.Severity = 1f;
            }

            GetComp(hediff)?.AssignMaster(master);
        }

        public static void EnsureBoundMarkerFromLifecycle(Pawn minion)
        {
            Pawn master = minion?.GetComp<CompPawnLifecycleEnforcer>()?.Master;
            if (master == null)
                return;

            HediffComp_BoundUndeadMinion existingComp = GetComp(minion);
            if (existingComp?.Master == master && !existingComp.IsLost)
                return;

            AssignMaster(minion, master);
        }

        public static HediffComp_BoundUndeadMinion GetComp(Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null)
                return null;

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                HediffComp_BoundUndeadMinion comp = GetComp(hediff);
                if (comp != null)
                    return comp;
            }

            return null;
        }

        private static HediffComp_BoundUndeadMinion GetComp(Hediff hediff)
        {
            return hediff is HediffWithComps hediffWithComps
                ? hediffWithComps.TryGetComp<HediffComp_BoundUndeadMinion>()
                : null;
        }
    }
}
