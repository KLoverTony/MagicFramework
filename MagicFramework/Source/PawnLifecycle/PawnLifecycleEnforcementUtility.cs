using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace MagicFramework.PawnLifecycle;

public static class PawnLifecycleEnforcementUtility
{
    private static readonly string[] NonFoodRestNeedDefNames =
    {
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

    private static readonly FieldInfo InteractionLastInteractTimeField = typeof(Pawn_InteractionsTracker).GetField(
        "lastInteractTime",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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

    public static void EnforceAll(Pawn pawn)
    {
        if (!PawnLifecycleUtility.TryGetLifecycle(pawn, out PawnLifecycleExtension extension, out _))
        {
            return;
        }

        if (extension.enforceNeeds)
        {
            EnforceNeeds(pawn, extension);
        }

        EnsureRenderableHumanlikeState(pawn, extension);

        if (extension.enforceSocialPolicy)
        {
            EnforceSocialPolicy(pawn, extension);
        }

        if (extension.enforceIdentityPolicy)
        {
            EnforceIdentityPolicy(pawn, extension);
        }

        if (extension.clearGeneratedHealthState)
        {
            ClearGeneratedHealthState(pawn, extension);
        }

        if (extension.enforceGearPolicy)
        {
            EnforceGearPolicy(pawn, extension);
        }

        if (extension.enforceWorkPolicy)
        {
            EnforceWorkPolicy(pawn, extension);
        }

        if (extension.enforceLifeStage)
        {
            NormalizeLifeStage(pawn);
        }

        if (extension.enforceMarkers)
        {
            EnforceMarkerHediffs(pawn, extension);
        }
    }

    public static void EnforceRecurring(Pawn pawn)
    {
        if (!PawnLifecycleUtility.TryGetLifecycle(pawn, out PawnLifecycleExtension extension, out _))
        {
            return;
        }

        if (extension.enforceNeeds)
        {
            EnforceNeeds(pawn, extension);
        }

        EnsureRenderableHumanlikeState(pawn, extension);

        if (extension.enforceSocialPolicy)
        {
            EnforceSocialPolicy(pawn, extension);
        }

        if (extension.enforceIdentityPolicy)
        {
            EnforceIdentityPolicy(pawn, extension);
        }

        if (extension.enforceGearPolicy)
        {
            EnforceGearPolicy(pawn, extension);
        }

        if (extension.enforceMarkers)
        {
            EnforceMarkerHediffs(pawn, extension);
        }
    }

    private static void EnsureRenderableHumanlikeState(Pawn pawn, PawnLifecycleExtension extension)
    {
        if (pawn?.RaceProps?.Humanlike == true
            && extension?.bodyForm == PawnLifecycleBodyForm.Skeletal
            && pawn.gender == Gender.None)
        {
            pawn.gender = Gender.Male;
            ResetRenderer(pawn);
        }
    }

    private static void ResetRenderer(Pawn pawn)
    {
        if (pawn?.Drawer?.renderer == null)
        {
            return;
        }

        pawn.Drawer.renderer = new PawnRenderer(pawn);
        pawn.Drawer.renderer.renderTree = new PawnRenderTree(pawn);
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    public static void EnforceNeeds(Pawn pawn, PawnLifecycleExtension extension = null)
    {
        if (pawn?.needs == null)
        {
            return;
        }

        extension ??= PawnLifecycleUtility.GetLifecycle(pawn);
        if (extension == null)
        {
            return;
        }

        switch (extension.needsPolicy)
        {
            case PawnLifecycleNeedsPolicy.None:
                RemoveNeed(pawn, "Food");
                RemoveNeed(pawn, "Rest");
                RemoveNonFoodRestNeeds(pawn);
                RemoveChemicalNeeds(pawn);
                break;
            case PawnLifecycleNeedsPolicy.NoFoodNoRest:
            case PawnLifecycleNeedsPolicy.ManaUpkeep:
            case PawnLifecycleNeedsPolicy.CorpseOrFleshConsumption:
            case PawnLifecycleNeedsPolicy.EssenceDrain:
            case PawnLifecycleNeedsPolicy.DormancyRecharge:
                RemoveNeed(pawn, "Food");
                RemoveNeed(pawn, "Rest");
                break;
        }
    }

    public static void EnforceSocialPolicy(Pawn pawn, PawnLifecycleExtension extension = null)
    {
        if (pawn?.interactions == null)
        {
            return;
        }

        extension ??= PawnLifecycleUtility.GetLifecycle(pawn);
        if (extension == null)
        {
            return;
        }

        if (extension.socialPolicy is PawnLifecycleSocialPolicy.None or PawnLifecycleSocialPolicy.SuppressedBothWays)
        {
            InteractionLastInteractTimeField?.SetValue(pawn.interactions, Find.TickManager.TicksGame + 9999999);
        }
    }

    public static void EnforceIdentityPolicy(Pawn pawn, PawnLifecycleExtension extension = null)
    {
        if (pawn == null)
        {
            return;
        }

        extension ??= PawnLifecycleUtility.GetLifecycle(pawn);
        if (extension == null || extension.intelligence != PawnLifecycleIntelligence.Mindless)
        {
            return;
        }

        pawn.relations?.ClearAllRelations();

        if (pawn.story != null)
        {
            ChildhoodField?.SetValue(pawn.story, null);
            AdulthoodField?.SetValue(pawn.story, null);
            TitleField?.SetValue(pawn.story, null);
            BirthLastNameField?.SetValue(pawn.story, null);

            if (pawn.story.traits != null)
            {
                foreach (Trait trait in pawn.story.traits.TraitsSorted.ToList())
                {
                    pawn.story.traits.RemoveTrait(trait, false);
                }
            }
        }

        pawn.Notify_DisabledWorkTypesChanged();
        pawn.workSettings?.Notify_DisabledWorkTypesChanged();

        if (pawn.skills?.skills == null)
        {
            return;
        }

        foreach (SkillRecord skill in pawn.skills.skills)
        {
            skill.Level = 0;
            skill.passion = Passion.None;
            skill.xpSinceLastLevel = 0f;
            skill.xpSinceMidnight = 0f;
        }
    }

    public static void EnforceGearPolicy(Pawn pawn, PawnLifecycleExtension extension = null)
    {
        if (pawn == null)
        {
            return;
        }

        extension ??= PawnLifecycleUtility.GetLifecycle(pawn);
        if (extension == null)
        {
            return;
        }

        switch (extension.gearPolicy)
        {
            case PawnLifecycleGearPolicy.None:
            case PawnLifecycleGearPolicy.StripAll:
                pawn.apparel?.DestroyAll(DestroyMode.Vanish);
                pawn.equipment?.DestroyAllEquipment(DestroyMode.Vanish);
                pawn.inventory?.DestroyAll(DestroyMode.Vanish);
                break;
            case PawnLifecycleGearPolicy.WeaponsOnly:
                pawn.apparel?.DestroyAll(DestroyMode.Vanish);
                break;
            case PawnLifecycleGearPolicy.ApparelOnly:
                pawn.equipment?.DestroyAllEquipment(DestroyMode.Vanish);
                break;
        }
    }

    public static void EnforceWorkPolicy(Pawn pawn, PawnLifecycleExtension extension = null)
    {
        if (pawn?.workSettings == null)
        {
            return;
        }

        extension ??= PawnLifecycleUtility.GetLifecycle(pawn);
        if (extension == null)
        {
            return;
        }

        switch (extension.workPolicy)
        {
            case PawnLifecycleWorkPolicy.None:
            case PawnLifecycleWorkPolicy.CombatOnly:
                pawn.workSettings.DisableAll();
                break;
            case PawnLifecycleWorkPolicy.HaulingCleaningOnly:
                SetAllowedWork(pawn, ("Hauling", 1), ("Cleaning", 2));
                break;
            case PawnLifecycleWorkPolicy.MundaneLabor:
                SetAllowedWork(pawn, ("Hauling", 1), ("Cleaning", 2), ("BasicWorker", 2), ("Firefighter", 3));
                break;
        }
    }

    public static void NormalizeLifeStage(Pawn pawn)
    {
        if (pawn?.ageTracker == null)
        {
            return;
        }

        pawn.ageTracker.AgeBiologicalTicks = 0L;
        pawn.ageTracker.AgeChronologicalTicks = 0L;
        LockedLifeStageIndexField?.SetValue(pawn.ageTracker, 0);
        CachedLifeStageIndexField?.SetValue(pawn.ageTracker, 0);
    }

    public static void ClearGeneratedHealthState(Pawn pawn, PawnLifecycleExtension extension = null)
    {
        if (pawn?.health?.hediffSet?.hediffs == null)
        {
            return;
        }

        extension ??= PawnLifecycleUtility.GetLifecycle(pawn);
        HashSet<HediffDef> preservedHediffs = extension?.markerHediffs != null
            ? new HashSet<HediffDef>(extension.markerHediffs)
            : new HashSet<HediffDef>();

        foreach (Hediff hediff in pawn.health.hediffSet.hediffs.ToList())
        {
            if (hediff?.def == null || preservedHediffs.Contains(hediff.def))
            {
                continue;
            }

            pawn.health.RemoveHediff(hediff);
        }
    }

    public static void EnforceMarkerHediffs(Pawn pawn, PawnLifecycleExtension extension = null)
    {
        if (pawn?.health?.hediffSet == null)
        {
            return;
        }

        extension ??= PawnLifecycleUtility.GetLifecycle(pawn);
        if (extension?.markerHediffs == null)
        {
            return;
        }

        foreach (HediffDef hediffDef in extension.markerHediffs)
        {
            AddMarkerIfMissing(pawn, hediffDef);
        }
    }

    private static void RemoveNonFoodRestNeeds(Pawn pawn)
    {
        foreach (string defName in NonFoodRestNeedDefNames)
        {
            RemoveNeed(pawn, defName);
        }
    }

    private static void RemoveChemicalNeeds(Pawn pawn)
    {
        if (pawn?.needs?.AllNeeds == null)
        {
            return;
        }

        foreach (Need need in pawn.needs.AllNeeds.ToList())
        {
            if (need?.def?.defName?.StartsWith("Chemical_") == true)
            {
                RemoveNeed(pawn, need.def);
            }
        }
    }

    private static void RemoveNeed(Pawn pawn, string needDefName)
    {
        NeedDef needDef = DefDatabase<NeedDef>.GetNamedSilentFail(needDefName);
        if (needDef != null)
        {
            RemoveNeed(pawn, needDef);
        }
    }

    private static void RemoveNeed(Pawn pawn, NeedDef needDef)
    {
        if (pawn?.needs == null || needDef == null || pawn.needs.TryGetNeed(needDef) == null)
        {
            return;
        }

        RemoveNeedMethod?.Invoke(pawn.needs, new object[] { needDef });
    }

    private static void SetAllowedWork(Pawn pawn, params (string defName, int priority)[] allowedWork)
    {
        if (pawn?.workSettings == null)
        {
            return;
        }

        pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
        pawn.workSettings.DisableAll();

        foreach ((string defName, int priority) in allowedWork)
        {
            WorkTypeDef workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName);
            if (workTypeDef != null && !pawn.WorkTypeIsDisabled(workTypeDef))
            {
                pawn.workSettings.SetPriority(workTypeDef, priority);
            }
        }
    }

    private static void AddMarkerIfMissing(Pawn pawn, HediffDef hediffDef)
    {
        if (pawn?.health?.hediffSet == null || hediffDef == null || pawn.health.hediffSet.HasHediff(hediffDef))
        {
            return;
        }

        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.Severity = 1f;
        pawn.health.AddHediff(hediff);
    }
}
