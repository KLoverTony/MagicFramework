using System.Collections.Generic;
using MagicFramework.PawnMemory;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class JobDriver_PerformChoralumWardenRite : JobDriver
    {
        private const TargetIndex CorpseInd = TargetIndex.A;
        private const TargetIndex LecternInd = TargetIndex.B;
        private const TargetIndex CircleInd = TargetIndex.C;
        private const int DefaultRitualTicks = 1000;

        private Corpse Corpse => job.GetTarget(CorpseInd).Thing as Corpse;
        private Thing Lectern => job.GetTarget(LecternInd).Thing;
        private Thing Circle => job.GetTarget(CircleInd).Thing;
        private Thing Armor => job.targetQueueB != null && job.targetQueueB.Count > 0 ? job.targetQueueB[0].Thing : null;
        private IntVec3 CircleCell => job.GetTarget(CircleInd).Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(CorpseInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(LecternInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(CircleInd), job, errorOnFailed: errorOnFailed) &&
                   (Armor == null || pawn.Reserve(Armor, job, errorOnFailed: errorOnFailed));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CorpseInd);
            this.FailOnDestroyedOrNull(LecternInd);
            this.FailOnDestroyedOrNull(CircleInd);
            this.FailOn(() => Armor == null || Armor.Destroyed);
            this.FailOn(() => !BonewrightUtility.IsBonewright(pawn));

            yield return Toils_Reserve.Reserve(CorpseInd);
            yield return Toils_Reserve.Reserve(LecternInd);
            yield return Toils_Reserve.Reserve(CircleInd);
            Toil postPickup = Toils_General.Label();
            yield return Toils_Jump.JumpIf(postPickup, () => pawn.carryTracker.CarriedThing == Corpse);

            yield return Toils_Goto.GotoThing(CorpseInd, PathEndMode.ClosestTouch, canGotoSpawnedParent: true)
                .FailOnSomeonePhysicallyInteracting(CorpseInd)
                .FailOnSelfAndParentsDespawnedOrNull(CorpseInd);

            yield return Toils_General.DoAtomic(PickUpCorpse);
            yield return postPickup;

            yield return Toils_Goto.GotoThing(CircleInd, PathEndMode.Touch);
            yield return Toils_General.DoAtomic(DropCorpseAtCircle);

            yield return Toils_Goto.GotoThing(LecternInd, PathEndMode.InteractionCell);

            Toil rite = Toils_General.WaitWith(LecternInd, DefaultRitualTicks, useProgressBar: true, face: CircleInd);
            rite.WithEffect(EffecterDefOf.Research, LecternInd);
            yield return rite;

            yield return Toils_General.DoAtomic(FinishRite);
        }

        private void PickUpCorpse()
        {
            Corpse corpse = Corpse;
            if (!RitualCorpseEligibilityUtility.IsValidHumanlikeMortalCorpse(corpse, Map))
            {
                Messages.Message("The Choralum animation rite requires a humanlike mortal corpse.", Lectern ?? Circle, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            if (pawn.carryTracker.CarriedThing == corpse)
                return;

            int carriedCount = pawn.carryTracker.TryStartCarry(corpse, 1, reserve: false);
            if (carriedCount <= 0)
            {
                Messages.Message(pawn.LabelShortCap + " could not carry " + corpse.LabelShortCap + " to the circle.", corpse, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
            }
        }

        private void DropCorpseAtCircle()
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing == null)
            {
                Messages.Message(pawn.LabelShortCap + " reached the circle without the selected corpse.", Circle ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            if (!pawn.carryTracker.TryDropCarriedThing(CircleCell, ThingPlaceMode.Near, out _))
            {
                Messages.Message(pawn.LabelShortCap + " could not place the corpse at the circle.", Circle ?? Lectern, MessageTypeDefOf.RejectInput, historical: false);
                EndJobWith(JobCondition.Incompletable);
            }
        }

        private void FinishRite()
        {
            if (!BonewrightUtility.IsBonewright(pawn))
            {
                Messages.Message("Only a Bonewright can complete the animation rite.", pawn, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Corpse corpse = Corpse ?? FindPlacedCorpseNearCircle();
            if (!RitualCorpseEligibilityUtility.IsValidHumanlikeMortalCorpse(corpse, Map))
            {
                Messages.Message("The Choralum animation rite requires a humanlike mortal corpse.", Lectern ?? Circle, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Thing armor = Armor;
            if (armor == null || armor.Destroyed)
            {
                Messages.Message("The Choralum animation rite requires intact plate or flak armor.", Lectern ?? Circle, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            string corpseLabel = corpse.LabelShortCap;
            Pawn sourcePawn = corpse.InnerPawn;
            string sourceName = ResolveSourceName(corpse);
            Gender sourceGender = ResolveGenerationGender(corpse);
            Ideo sourceIdeo = ModsConfig.IdeologyActive ? sourcePawn?.ideo?.Ideo : null;
            IntVec3 spawnCell = ResolveSpawnCell(corpse.Position);

            PawnSoulRiteUtility.NotifyCorpseConsumedWithoutBindingSoul(sourcePawn, corpse, pawn);
            Pawn warden = CreateWardenPawn(corpse, sourcePawn, sourceName, sourceGender, sourceIdeo);
            if (!armor.Destroyed)
                armor.Destroy(DestroyMode.Vanish);

            if (warden == null)
            {
                Messages.Message("The Choralum rite consumed " + corpseLabel + ", but no Reliquary Warden could be animated.", Lectern ?? Circle, MessageTypeDefOf.NegativeEvent, historical: false);
                ReleaseAttendees();
                return;
            }

            if (!warden.Spawned)
                GenSpawn.Spawn(warden, spawnCell, Map);
            if (warden.Faction != Faction.OfPlayer)
                warden.SetFaction(Faction.OfPlayer);

            ReleaseAttendees();
            Messages.Message(corpseLabel + " rises as a Reliquary Warden.", warden, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        private Pawn CreateWardenPawn(Corpse corpse, Pawn sourcePawn, string sourceName, Gender sourceGender, Ideo sourceIdeo = null)
        {
            PawnKindDef pawnKindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail("AF_ReliquaryWarden");
            if (pawnKindDef == null)
            {
                Log.Error("AeternusFaith Choralum rite could not find PawnKindDef AF_ReliquaryWarden.");
                return null;
            }

            if (corpse != null && !corpse.Destroyed)
                corpse.Destroy(DestroyMode.Vanish);

            Pawn warden = UndeadPawnFactory.GeneratePawn(pawnKindDef, new UndeadPawnCreationOptions
            {
                faction = Faction.OfPlayer,
                context = PawnGenerationContext.NonPlayer,
                tile = Map.Tile,
                fixedGender = sourceGender,
                label = "Reliquary Warden of " + sourceName,
                sourcePawn = sourcePawn,
                sourceIdeo = sourceIdeo,
                resetSkills = true,
                forceNoBackstory = false
            });
            if (warden == null)
                return null;

            warden.Name = new NameTriple("", "Reliquary Warden of " + sourceName, "");
            Log.Message("[AeternusFaith] Reliquary Warden conversion result: def=" + warden.def?.defName +
                        ", kindDef=" + warden.kindDef?.defName +
                        ", xenotype=" + (ModsConfig.BiotechActive ? warden.genes?.Xenotype?.defName : "BiotechInactive") +
                        ", undead=" + SkeletonUndeadUtility.IsUndead(warden) +
                        ", skeletal=" + SkeletonUndeadUtility.IsSkeletonUndead(warden));
            return warden;
        }

        private string ResolveSourceName(Corpse corpse)
        {
            string name = corpse?.InnerPawn?.Name?.ToStringShort;
            if (name.NullOrEmpty())
                name = corpse?.InnerPawn?.LabelShort;
            if (name.NullOrEmpty())
                name = "the dead";

            return name;
        }

        private Gender ResolveGenerationGender(Corpse corpse)
        {
            Gender gender = corpse?.InnerPawn?.gender ?? Gender.None;
            return gender == Gender.Female ? Gender.Female : Gender.Male;
        }

        private IntVec3 ResolveSpawnCell(IntVec3 preferredCell)
        {
            if (IsValidSpawnCell(preferredCell))
                return preferredCell;

            foreach (IntVec3 candidate in GenRadial.RadialCellsAround(CircleCell, 2.9f, true))
            {
                if (IsValidSpawnCell(candidate))
                    return candidate;
            }

            return CircleCell;
        }

        private bool IsValidSpawnCell(IntVec3 cell)
        {
            return cell.IsValid &&
                   cell.InBounds(Map) &&
                   cell.Walkable(Map) &&
                   cell.Standable(Map) &&
                   cell.GetFirstPawn(Map) == null;
        }

        private void ReleaseAttendees()
        {
            JobDef attendJobDef = DefDatabase<JobDef>.GetNamedSilentFail("AF_AttendSkeletonRite");
            if (attendJobDef == null)
                return;

            foreach (Pawn attendee in Map.mapPawns.FreeColonistsSpawned)
            {
                if (attendee == pawn || attendee.CurJobDef != attendJobDef)
                    continue;

                LocalTargetInfo focus = attendee.CurJob.GetTarget(TargetIndex.B);
                if (focus.Thing == Circle)
                    attendee.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        }

        private Corpse FindPlacedCorpseNearCircle()
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(CircleCell, 2f, true))
            {
                if (!cell.InBounds(Map))
                    continue;

                Corpse corpse = cell.GetFirstThing<Corpse>(Map);
                if (RitualCorpseEligibilityUtility.IsValidHumanlikeMortalCorpse(corpse, Map))
                    return corpse;
            }

            return null;
        }
    }
}
