using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class JobDriver_PerformSkeletonRite : JobDriver
    {
        private const TargetIndex CorpseInd = TargetIndex.A;
        private const TargetIndex LecternInd = TargetIndex.B;
        private const TargetIndex CircleInd = TargetIndex.C;
        private const int DefaultRitualTicks = 900;

        private Corpse Corpse => job.GetTarget(CorpseInd).Thing as Corpse;
        private Thing Lectern => job.GetTarget(LecternInd).Thing;
        private Thing Circle => job.GetTarget(CircleInd).Thing;
        private IntVec3 CircleCell => job.GetTarget(CircleInd).Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(CorpseInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(LecternInd), job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(CircleInd), job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CorpseInd);
            this.FailOnDestroyedOrNull(LecternInd);
            this.FailOnDestroyedOrNull(CircleInd);

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
                Messages.Message("The skeleton rite requires a humanlike mortal corpse.", Lectern ?? Circle, MessageTypeDefOf.RejectInput, historical: false);
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
            Corpse corpse = Corpse ?? FindPlacedCorpseNearCircle();
            if (!RitualCorpseEligibilityUtility.IsValidHumanlikeMortalCorpse(corpse, Map))
            {
                Messages.Message("The skeleton rite requires a humanlike mortal corpse.", Lectern ?? Circle, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            string corpseLabel = corpse.LabelShortCap;
            Pawn sourcePawn = corpse.InnerPawn;
            string sourceName = ResolveSourceName(corpse);
            Gender sourceGender = ResolveGenerationGender(corpse);
            Ideo sourceIdeo = ModsConfig.IdeologyActive ? sourcePawn?.ideo?.Ideo : null;
            IntVec3 spawnCell = ResolveSpawnCell(corpse.Position);

            Pawn skeleton = CreateSkeletonPawn(corpse, sourcePawn, sourceName, sourceGender, sourceIdeo);
            if (skeleton == null)
            {
                Messages.Message("The skeleton rite consumed " + corpseLabel + ", but no skeleton could be raised.", Lectern ?? Circle, MessageTypeDefOf.NegativeEvent, historical: false);
                ReleaseAttendees();
                return;
            }

            if (!skeleton.Spawned)
                GenSpawn.Spawn(skeleton, spawnCell, Map);
            if (skeleton.Faction != Faction.OfPlayer)
                skeleton.SetFaction(Faction.OfPlayer);

            ReleaseAttendees();
            Messages.Message(corpseLabel + " rises as a skeleton.", skeleton, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        private Pawn CreateSkeletonPawn(Corpse corpse, Pawn sourcePawn, string sourceName, Gender sourceGender, Ideo sourceIdeo = null)
        {
            PawnKindDef pawnKindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail("AF_Skeleton");
            if (pawnKindDef == null)
            {
                Log.Error("AeternusFaith skeleton rite could not find PawnKindDef AF_Skeleton.");
                return null;
            }

            if (corpse != null && !corpse.Destroyed)
                corpse.Destroy(DestroyMode.Vanish);

            PawnGenerationRequest request = new PawnGenerationRequest(
                kind: PawnKindDefOf.Colonist,
                faction: Faction.OfPlayer,
                context: PawnGenerationContext.NonPlayer,
                tile: Map.Tile,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 0f,
                allowPregnant: false,
                allowFood: false,
                allowAddictions: false,
                fixedGender: sourceGender,
                forceNoIdeo: true,
                forceNoBackstory: false,
                developmentalStages: DevelopmentalStage.Adult,
                dontGiveWeapon: true,
                maximumAgeTraits: 0,
                minimumAgeTraits: 0,
                forceNoGear: true);
            Pawn skeleton = PawnGenerator.GeneratePawn(request);
            if (skeleton == null)
                return null;

            ConvertPawnToSkeleton(skeleton, pawnKindDef, sourcePawn, sourceName, sourceIdeo);
            return skeleton;
        }

        private void ConvertPawnToSkeleton(Pawn skeleton, PawnKindDef pawnKindDef, Pawn sourcePawn, string sourceName, Ideo sourceIdeo)
        {
            if (skeleton == null || pawnKindDef == null)
                return;

            if (ModsConfig.IdeologyActive && skeleton.ideo != null)
            {
                Ideo ideoToApply = sourceIdeo ?? Faction.OfPlayer?.ideos?.PrimaryIdeo;
                if (ideoToApply != null)
                    skeleton.ideo.SetIdeo(ideoToApply);
            }

            skeleton.def = pawnKindDef.race;
            skeleton.kindDef = pawnKindDef;
            SkeletonUndeadUtility.NormalizeSkeletonLifeStage(skeleton);
            AttachSkeletonCleanupComp(skeleton);
            ApplySkeletonAppearance(skeleton);
            SkeletonUndeadUtility.RemoveLivingResurrectionHediffs(skeleton);
            SkeletonUndeadUtility.EnforceUndeadState(skeleton, resetSkills: true);
            SkeletonUndeadUtility.CopyBackstoriesFromSource(sourcePawn, skeleton);
            SkeletonUndeadUtility.CopySkillsFromSource(sourcePawn, skeleton);
            SkeletonUndeadUtility.RemoveNonUndeadHediffs(skeleton);
            skeleton.def = pawnKindDef.race;
            skeleton.kindDef = pawnKindDef;
            SkeletonUndeadUtility.NormalizeSkeletonLifeStage(skeleton);
            SkeletonUndeadUtility.ApplyUndeadHediffs(skeleton, "AF_SkeletalBody");
            SkeletonUndeadUtility.ApplyRaceBasedUndeadHediffs(skeleton);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadXenotype(skeleton);
            SkeletonUndeadUtility.SuppressUndeadSocialInteractions(skeleton);
            skeleton.Name = new NameTriple("", "Skeleton of " + sourceName, "");
            skeleton.Drawer?.renderer?.SetAllGraphicsDirty();
            Log.Message("[AeternusFaith] Raised skeleton conversion result: def=" + skeleton.def?.defName +
                        ", kindDef=" + skeleton.kindDef?.defName +
                        ", xenotype=" + (ModsConfig.BiotechActive ? skeleton.genes?.Xenotype?.defName : "BiotechInactive") +
                        ", undead=" + SkeletonUndeadUtility.IsUndead(skeleton) +
                        ", skeletal=" + SkeletonUndeadUtility.IsSkeletonUndead(skeleton));
        }

        private void AttachSkeletonCleanupComp(Pawn skeleton)
        {
            if (skeleton.GetComp<Comp_SkeletonUndeadCleanup>() != null)
                return;

            CompProperties_SkeletonUndeadCleanup compProperties = skeleton.def.comps?
                .OfType<CompProperties_SkeletonUndeadCleanup>()
                .FirstOrDefault();
            if (compProperties == null)
                return;

            Comp_SkeletonUndeadCleanup comp = new Comp_SkeletonUndeadCleanup
            {
                parent = skeleton
            };
            comp.Initialize(compProperties);
            skeleton.AllComps.Add(comp);
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

        private void ApplySkeletonAppearance(Pawn skeleton)
        {
            if (skeleton.story == null)
                return;

            BodyTypeDef bodyTypeDef = DefDatabase<BodyTypeDef>.GetNamedSilentFail("AF_SkeletonThin") ??
                                      DefDatabase<BodyTypeDef>.GetNamedSilentFail("Thin");
            HeadTypeDef headTypeDef = DefDatabase<HeadTypeDef>.GetNamedSilentFail("AF_SkeletonHead") ??
                                      DefDatabase<HeadTypeDef>.GetNamedSilentFail("Skull");
            HairDef hairDef = DefDatabase<HairDef>.GetNamedSilentFail("Bald");
            if (bodyTypeDef != null)
                skeleton.story.bodyType = bodyTypeDef;
            if (headTypeDef != null)
                skeleton.story.headType = headTypeDef;
            if (hairDef != null)
                skeleton.story.hairDef = hairDef;

            BeardDef beardDef = DefDatabase<BeardDef>.GetNamedSilentFail("NoBeard");
            if (beardDef != null && skeleton.style != null)
                skeleton.style.beardDef = beardDef;
            skeleton.Drawer?.renderer?.SetAllGraphicsDirty();
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
