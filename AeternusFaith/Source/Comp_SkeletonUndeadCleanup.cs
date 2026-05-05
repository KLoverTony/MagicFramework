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

            ClearIdeoWithoutCallbacks(pawn);

            if (pawn.story == null)
                return;

            ChildhoodField?.SetValue(pawn.story, null);
            AdulthoodField?.SetValue(pawn.story, null);
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
            if (pawn?.ageTracker == null || pawn.def?.defName != "AF_SkeletonRace")
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
    }
}
