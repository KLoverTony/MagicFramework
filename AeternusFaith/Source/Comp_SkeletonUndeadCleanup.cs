using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

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
            SkeletonUndeadUtility.EnforceUndeadState(Pawn, resetSkills: false);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadHediffs(Pawn);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadXenotype(Pawn);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                SkeletonUndeadUtility.EnforceUndeadState(Pawn, resetSkills: false);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            SkeletonUndeadUtility.EnforceUndeadNeeds(Pawn);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadHediffs(Pawn);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadXenotype(Pawn);
        }
    }

    public static class SkeletonUndeadUtility
    {
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

        public static void EnforceUndeadState(Pawn pawn, bool resetSkills)
        {
            if (pawn == null)
                return;

            NormalizeSkeletonLifeStage(pawn);
            EnforceUndeadNeeds(pawn);
            StripGear(pawn);
            ClearHumanIdentity(pawn);
            if (resetSkills)
                ResetSkills(pawn);
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
                ApplyUndeadHediffs(pawn, "AF_SkeletalBody");
            else if (pawn?.def?.defName == "AF_SpectreRace")
                ApplyUndeadHediffs(pawn, "AF_SpectralForm");
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

        public static void EnforceUndeadNeeds(Pawn pawn)
        {
            if (pawn?.needs == null)
                return;

            foreach (string defName in RemovedNeedDefNames)
            {
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

        public static void StripGear(Pawn pawn)
        {
            pawn?.apparel?.DestroyAll(DestroyMode.Vanish);
            pawn?.equipment?.DestroyAllEquipment(DestroyMode.Vanish);
            pawn?.inventory?.DestroyAll(DestroyMode.Vanish);
        }

        public static void ClearHumanIdentity(Pawn pawn)
        {
            if (pawn == null)
                return;

            pawn.gender = Gender.None;
            pawn.relations?.ClearAllRelations();

            if (pawn.story == null)
                return;

            TitleField?.SetValue(pawn.story, null);
            BirthLastNameField?.SetValue(pawn.story, null);

            if (pawn.story.traits == null)
                return;

            List<Trait> traits = new List<Trait>(pawn.story.traits.TraitsSorted);
            foreach (Trait trait in traits)
                pawn.story.traits.RemoveTrait(trait, false);
        }

        public static void NormalizeSkeletonLifeStage(Pawn pawn)
        {
            if (pawn?.ageTracker == null ||
                (pawn.def?.defName != "AF_SkeletonRace" && pawn.def?.defName != "AF_SpectreRace"))
                return;

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
            if (xenotypeDef == null || pawn.genes.Xenotype == xenotypeDef)
                return;

            pawn.genes.SetXenotypeDirect(xenotypeDef);
        }
    }
}
