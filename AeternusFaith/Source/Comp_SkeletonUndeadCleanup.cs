using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MagicFramework.PawnLifecycle;
using RimWorld;
using Verse;
using Verse.AI;

namespace AeternusFaith
{
    public class CompProperties_SkeletonUndeadCleanup : CompProperties
    {
        public CompProperties_SkeletonUndeadCleanup()
        {
            compClass = typeof(Comp_SkeletonUndeadCleanup);
        }
    }

    public class Comp_SkeletonUndeadCleanup : ThingComp
    {
        private Pawn Pawn => parent as Pawn;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            SkeletonUndeadUtility.RepairUndeadRenderingState(Pawn);
            SkeletonUndeadUtility.EnforceUndeadState(Pawn, resetSkills: false);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadHediffs(Pawn);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadXenotype(Pawn);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                SkeletonUndeadUtility.RepairUndeadRenderingState(Pawn);
                SkeletonUndeadUtility.EnforceUndeadState(Pawn, resetSkills: false);
                SkeletonUndeadUtility.ApplyRaceBasedUndeadHediffs(Pawn);
                SkeletonUndeadUtility.ApplyRaceBasedUndeadXenotype(Pawn);
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            SkeletonUndeadUtility.RepairUndeadRenderingState(Pawn);
            SkeletonUndeadUtility.EnforceUndeadNeeds(Pawn);
            SkeletonUndeadUtility.SuppressUndeadSocialInteractions(Pawn);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadHediffs(Pawn);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadXenotype(Pawn);
        }
    }

    public static class SkeletonUndeadUtility
    {
        private const int DefaultUndeadBiologicalAgeYears = 30;

        private static readonly string[] RemovedNeedDefNames =
        {
            "Food",
            "Rest",
            "Joy",
            "Comfort",
            "Mood",
            "Beauty",
            "Outdoors",
            "Indoors",
            "DrugDesire",
            "RoomSize"
        };

        private static readonly MethodInfo RemoveNeedMethod = typeof(Pawn_NeedsTracker).GetMethod(
            "RemoveNeed",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CachedLifeStageIndexField = typeof(Pawn_AgeTracker).GetField(
            "cachedLifeStageIndex",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo LockedLifeStageIndexField = typeof(Pawn_AgeTracker).GetField(
            "lockedLifeStageIndex",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo IdeoField = typeof(Pawn_IdeoTracker).GetField(
            "ideo",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo PreviousIdeosField = typeof(Pawn_IdeoTracker).GetField(
            "previousIdeos",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CertaintyField = typeof(Pawn_IdeoTracker).GetField(
            "certaintyInt",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ChildhoodField = typeof(Pawn_StoryTracker).GetField(
            "childhood",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo AdulthoodField = typeof(Pawn_StoryTracker).GetField(
            "adulthood",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo TitleField = typeof(Pawn_StoryTracker).GetField(
            "title",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo BirthLastNameField = typeof(Pawn_StoryTracker).GetField(
            "birthLastName",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo InteractionLastInteractTimeField = typeof(Pawn_InteractionsTracker).GetField(
            "lastInteractTime",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static void EnforceUndeadState(Pawn pawn, bool resetSkills)
        {
            if (pawn == null)
                return;

            EnsureMindState(pawn);
            NormalizeSkeletonLifeStage(pawn);
            EnforceUndeadNeeds(pawn);
            SuppressUndeadSocialInteractions(pawn);
            StripGear(pawn);
            ClearHumanIdentity(pawn);
            if (resetSkills)
                ResetSkills(pawn);
        }

        public static void ConvertPawnToSkeleton(Pawn pawn, PawnKindDef skeletonKindDef, string name = "skeleton", bool resetSkills = true)
        {
            if (pawn == null || skeletonKindDef?.race == null)
                return;

            pawn.def = skeletonKindDef.race;
            pawn.kindDef = skeletonKindDef;
            pawn.gender = Gender.Male;
            if (!name.NullOrEmpty())
                pawn.Name = new NameSingle(name);

            ResetPawnRenderer(pawn);
            NormalizeSkeletonLifeStage(pawn);
            EnsureUndeadCleanupComp(pawn);
            ApplySkeletonAppearance(pawn);
            RemoveLivingResurrectionHediffs(pawn);
            EnforceUndeadState(pawn, resetSkills);
            EnsureFrameworkLifecycleComp(pawn);
            RemoveNonUndeadHediffs(pawn);
            ApplyRaceBasedUndeadHediffs(pawn);
            ApplyRaceBasedUndeadXenotype(pawn);
            SuppressUndeadSocialInteractions(pawn);
            ResetPawnRenderer(pawn);
            TryInitializeRenderer(pawn);
        }

        public static void ConvertPawnToSpectre(Pawn pawn, PawnKindDef spectreKindDef, string name = "spectre", bool resetSkills = true)
        {
            if (pawn == null || spectreKindDef?.race == null)
                return;

            pawn.def = spectreKindDef.race;
            pawn.kindDef = spectreKindDef;
            pawn.gender = Gender.Male;
            if (!name.NullOrEmpty())
                pawn.Name = new NameSingle(name);

            ResetPawnRenderer(pawn);
            NormalizeSkeletonLifeStage(pawn);
            EnsureUndeadCleanupComp(pawn);
            ApplySpectreAppearance(pawn);
            RemoveLivingResurrectionHediffs(pawn);
            EnforceUndeadState(pawn, resetSkills);
            EnsureFrameworkLifecycleComp(pawn);
            RemoveNonUndeadHediffs(pawn);
            ApplyRaceBasedUndeadHediffs(pawn);
            ApplyRaceBasedUndeadXenotype(pawn);
            SuppressUndeadSocialInteractions(pawn);
            ResetPawnRenderer(pawn);
            TryInitializeRenderer(pawn);
        }

        public static void RepairUndeadRenderingState(Pawn pawn)
        {
            if (!IsUndeadRace(pawn))
                return;

            pawn.gender = Gender.Male;

            if (pawn.def?.defName == "AF_SkeletonRace")
            {
                ApplySkeletonAppearance(pawn);
            }

            ResetPawnRenderer(pawn);
        }

        public static void ApplyUndeadHediffs(Pawn pawn, string specializedHediffDefName)
        {
            if (pawn?.health?.hediffSet == null)
                return;

            AddHediffIfMissing(pawn, "AF_UndeadNature");
            AddHediffIfMissing(pawn, specializedHediffDefName);
        }

        public static void ApplyRaceBasedUndeadHediffs(Pawn pawn)
        {
            if (pawn?.def?.defName == "AF_SkeletonRace")
            {
                ApplyUndeadHediffs(pawn, "AF_SkeletalBody");
                AddHediffIfMissing(pawn, "AF_SkeletalLimitations");
            }
            else if (pawn?.def?.defName == "AF_SpectreRace")
            {
                ApplyUndeadHediffs(pawn, "AF_SpectralForm");
                AddHediffIfMissing(pawn, "AF_SpectralLimitations");
            }
        }

        public static void ApplyRaceBasedUndeadXenotype(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || pawn?.genes == null)
                return;

            if (pawn.def?.defName == "AF_SkeletonRace")
                ApplyXenotype(pawn, "AF_SkeletonXenotype");
            else if (pawn.def?.defName == "AF_SpectreRace")
                ApplyXenotype(pawn, "AF_SpectreXenotype");
        }

        public static void RemoveLivingResurrectionHediffs(Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null)
                return;

            string[] removedDefNames =
            {
                "CryptosleepSickness",
                "ResurrectionSickness",
                "ResurrectionPsychosis"
            };

            foreach (string defName in removedDefNames)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(defName);
                if (hediffDef == null)
                    continue;

                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                while (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                }
            }
        }

        public static void RemoveNonUndeadHediffs(Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null)
                return;

            HashSet<string> preservedHediffDefNames = new HashSet<string>
            {
                "AF_UndeadNature",
                "AF_SkeletalBody",
                "AF_SkeletalLimitations",
                "AF_SpectralForm",
                "AF_SpectralLimitations"
            };

            List<Hediff> hediffs = new List<Hediff>(pawn.health.hediffSet.hediffs);
            foreach (Hediff hediff in hediffs)
            {
                if (hediff?.def == null || preservedHediffDefNames.Contains(hediff.def.defName))
                    continue;

                pawn.health.RemoveHediff(hediff);
            }
        }

        public static bool IsUndead(Pawn pawn)
        {
            return HasHediff(pawn, "AF_UndeadNature");
        }

        public static bool IsSkeletonUndead(Pawn pawn)
        {
            return HasHediff(pawn, "AF_SkeletalBody");
        }

        public static bool IsSpectralUndead(Pawn pawn)
        {
            return HasHediff(pawn, "AF_SpectralForm");
        }

        public static bool IsUndeadRace(Pawn pawn)
        {
            string defName = pawn?.def?.defName;
            return defName == "AF_SkeletonRace" || defName == "AF_SpectreRace";
        }

        public static bool ShouldSuppressSocialInteraction(Pawn initiator, Pawn recipient)
        {
            return IsUndeadRace(initiator) || IsUndeadRace(recipient);
        }

        public static void EnforceUndeadNeeds(Pawn pawn)
        {
            if (pawn?.needs == null)
                return;

            if (pawn.def?.defName == "AF_SpectreRace" &&
                pawn.needs.mood == null)
            {
                pawn.needs.AddOrRemoveNeedsAsAppropriate();
            }

            foreach (string defName in RemovedNeedDefNames)
            {
                if (pawn.def?.defName == "AF_SpectreRace" && defName == "Mood")
                    continue;

                NeedDef needDef = DefDatabase<NeedDef>.GetNamedSilentFail(defName);
                if (needDef != null && pawn.needs.TryGetNeed(needDef) != null)
                    RemoveNeedMethod?.Invoke(pawn.needs, new object[] { needDef });
            }

            foreach (Need need in new List<Need>(pawn.needs.AllNeeds))
            {
                if (need?.def?.defName?.StartsWith("Chemical_") == true)
                    RemoveNeedMethod?.Invoke(pawn.needs, new object[] { need.def });
            }
        }

        public static void SuppressUndeadSocialInteractions(Pawn pawn)
        {
            if (pawn?.interactions == null || !IsUndeadRace(pawn))
            {
                return;
            }

            InteractionLastInteractTimeField?.SetValue(pawn.interactions, Find.TickManager.TicksGame + 9999999);
        }

        public static void StripGear(Pawn pawn)
        {
            pawn?.apparel?.DestroyAll(DestroyMode.Vanish);
            pawn?.equipment?.DestroyAllEquipment(DestroyMode.Vanish);
            pawn?.inventory?.DestroyAll(DestroyMode.Vanish);
        }

        public static void EnsureUndeadCleanupComp(Pawn pawn)
        {
            if (pawn == null || pawn.GetComp<Comp_SkeletonUndeadCleanup>() != null)
                return;

            CompProperties_SkeletonUndeadCleanup compProperties = pawn.def?.comps?
                .OfType<CompProperties_SkeletonUndeadCleanup>()
                .FirstOrDefault();
            if (compProperties == null)
                return;

            Comp_SkeletonUndeadCleanup comp = new Comp_SkeletonUndeadCleanup
            {
                parent = pawn
            };
            comp.Initialize(compProperties);
            pawn.AllComps.Add(comp);
        }

        public static void EnsureFrameworkLifecycleComp(Pawn pawn)
        {
            if (pawn == null || pawn.GetComp<CompPawnLifecycleEnforcer>() != null)
                return;

            CompProperties_PawnLifecycleEnforcer compProperties = pawn.def?.comps?
                .OfType<CompProperties_PawnLifecycleEnforcer>()
                .FirstOrDefault();
            if (compProperties == null)
                return;

            CompPawnLifecycleEnforcer comp = new CompPawnLifecycleEnforcer
            {
                parent = pawn
            };
            comp.Initialize(compProperties);
            pawn.AllComps.Add(comp);
            PawnLifecycleEnforcementUtility.EnforceAll(pawn);
        }

        public static void EnsureMindState(Pawn pawn)
        {
            if (pawn == null)
                return;

            pawn.mindState ??= new Pawn_MindState(pawn);
            pawn.mindState.mentalStateHandler ??= new MentalStateHandler(pawn);
        }

        public static void ClearHumanIdentity(Pawn pawn)
        {
            if (pawn == null)
                return;

            pawn.gender = Gender.Male;
            pawn.relations?.ClearAllRelations();

            if (pawn.story == null)
                return;

            ChildhoodField?.SetValue(pawn.story, null);
            AdulthoodField?.SetValue(pawn.story, null);
            TitleField?.SetValue(pawn.story, null);
            BirthLastNameField?.SetValue(pawn.story, null);

            if (pawn.story.traits == null)
                return;

            PawnLifecycleExtension extension = PawnLifecycleUtility.GetLifecycle(pawn);
            List<Trait> traits = new List<Trait>(pawn.story.traits.TraitsSorted);
            foreach (Trait trait in traits)
            {
                if (trait?.def == null || extension?.lifecycleTraits?.Contains(trait.def) != true)
                    pawn.story.traits.RemoveTrait(trait, false);
            }
        }

        public static void CopyBackstoriesFromSource(Pawn sourcePawn, Pawn targetPawn)
        {
            if (sourcePawn?.story == null || targetPawn?.story == null)
                return;

            ChildhoodField?.SetValue(targetPawn.story, ChildhoodField?.GetValue(sourcePawn.story));
            AdulthoodField?.SetValue(targetPawn.story, AdulthoodField?.GetValue(sourcePawn.story));
        }

        public static void CopySkillsFromSource(Pawn sourcePawn, Pawn targetPawn)
        {
            if (sourcePawn?.skills?.skills == null || targetPawn?.skills == null)
                return;

            foreach (SkillRecord sourceSkill in sourcePawn.skills.skills)
            {
                SkillRecord targetSkill = targetPawn.skills.GetSkill(sourceSkill.def);
                if (targetSkill == null)
                    continue;

                targetSkill.Level = sourceSkill.Level;
                targetSkill.passion = sourceSkill.passion;
                targetSkill.xpSinceLastLevel = sourceSkill.xpSinceLastLevel;
                targetSkill.xpSinceMidnight = sourceSkill.xpSinceMidnight;
            }
        }

        public static void NormalizeSkeletonLifeStage(Pawn pawn)
        {
            if (pawn?.ageTracker == null ||
                (pawn.def?.defName != "AF_SkeletonRace" && pawn.def?.defName != "AF_SpectreRace"))
                return;

            pawn.ageTracker.AgeBiologicalTicks = (long)DefaultUndeadBiologicalAgeYears * GenDate.TicksPerYear;
            pawn.ageTracker.AgeChronologicalTicks = 0L;
            LockedLifeStageIndexField?.SetValue(pawn.ageTracker, 0);
            CachedLifeStageIndexField?.SetValue(pawn.ageTracker, 0);
        }

        private static void ClearIdeoWithoutCallbacks(Pawn pawn)
        {
            if (pawn?.ideo == null)
                return;

            IdeoField?.SetValue(pawn.ideo, null);
            if (PreviousIdeosField?.GetValue(pawn.ideo) is List<Ideo> previousIdeos)
                previousIdeos.Clear();
            CertaintyField?.SetValue(pawn.ideo, 0f);
        }

        public static void ResetSkills(Pawn pawn)
        {
            if (pawn?.skills?.skills == null)
                return;

            foreach (SkillRecord skill in pawn.skills.skills)
            {
                skill.Level = 0;
                skill.passion = Passion.None;
                skill.xpSinceLastLevel = 0f;
                skill.xpSinceMidnight = 0f;
            }
        }

        public static void ApplySkeletonAppearance(Pawn skeleton)
        {
            if (skeleton?.story == null)
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
        }

        public static void ApplySpectreAppearance(Pawn spectre)
        {
            if (spectre?.story == null)
                return;

            BodyTypeDef bodyTypeDef = DefDatabase<BodyTypeDef>.GetNamedSilentFail("AF_SpectreBody");
            HeadTypeDef headTypeDef = DefDatabase<HeadTypeDef>.GetNamedSilentFail("AF_SpectreHead");
            HairDef hairDef = DefDatabase<HairDef>.GetNamedSilentFail("Bald");

            if (bodyTypeDef != null)
                spectre.story.bodyType = bodyTypeDef;
            if (headTypeDef != null)
                spectre.story.headType = headTypeDef;
            if (hairDef != null)
                spectre.story.hairDef = hairDef;

            BeardDef beardDef = DefDatabase<BeardDef>.GetNamedSilentFail("NoBeard");
            if (beardDef != null && spectre.style != null)
                spectre.style.beardDef = beardDef;
        }

        public static void ResetPawnRenderer(Pawn pawn)
        {
            if (pawn?.Drawer?.renderer == null)
                return;

            pawn.Drawer.renderer = new PawnRenderer(pawn);
            pawn.Drawer.renderer.renderTree = new PawnRenderTree(pawn);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public static void TryInitializeRenderer(Pawn pawn)
        {
            try
            {
                pawn?.Drawer?.renderer?.EnsureGraphicsInitialized();
            }
            catch (System.Exception ex)
            {
                Log.Warning("[AeternusFaith] Could not initialize undead renderer for " + (pawn?.LabelShort ?? "<null>") + ": " + ex);
            }
        }

        private static void AddHediffIfMissing(Pawn pawn, string hediffDefName)
        {
            if (pawn == null || hediffDefName.NullOrEmpty())
                return;

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffDefName);
            if (hediffDef == null)
            {
                Log.Warning("[AeternusFaith] Missing undead HediffDef " + hediffDefName + "; marker could not be applied to " + pawn.LabelShort);
                return;
            }

            if (pawn.health.hediffSet.HasHediff(hediffDef))
                return;

            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            hediff.Severity = 1f;
            pawn.health.AddHediff(hediff);
        }

        private static bool HasHediff(Pawn pawn, string hediffDefName)
        {
            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffDefName);
            return hediffDef != null && pawn?.health?.hediffSet?.HasHediff(hediffDef) == true;
        }

        private static void ApplyXenotype(Pawn pawn, string xenotypeDefName)
        {
            XenotypeDef xenotypeDef = DefDatabase<XenotypeDef>.GetNamedSilentFail(xenotypeDefName);
            if (xenotypeDef == null)
                return;

            StripGeneratedGenes(pawn);
            if (pawn.genes.Xenotype == xenotypeDef)
                return;

            pawn.genes.SetXenotypeDirect(xenotypeDef);
            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }

        private static void StripGeneratedGenes(Pawn pawn)
        {
            if (pawn?.genes?.GenesListForReading == null)
                return;

            foreach (Gene gene in pawn.genes.GenesListForReading.ToList())
            {
                pawn.genes.RemoveGene(gene);
            }

            pawn.genes.ClearXenogenes();
        }
    }
}
