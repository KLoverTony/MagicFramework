using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MFVanilla.Core;

/// <summary>
/// Harmony patches for suppressing vanilla technology research.
/// </summary>
public static class MFVanillaPatcher
{
    private static readonly HashSet<string> SuppressionRoots = new()
    {
        "Electricity",
        "MicroelectronicsBasics",
        "MultiAnalyzer",
    };

    private static readonly HashSet<string> SuppressedResearchDefNames = new();
    private static bool _isPatched;
    private static bool _suppressionCacheBuilt;

    public static void Patch(Harmony harmony)
    {
        if (_isPatched) return;

        harmony.Patch(
            AccessTools.PropertyGetter(typeof(ResearchProjectDef), nameof(ResearchProjectDef.IsHidden)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(ResearchProjectDef_IsHidden_Postfix))
        );

        harmony.Patch(
            AccessTools.PropertyGetter(typeof(ResearchProjectDef), nameof(ResearchProjectDef.CanStartNow)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(ResearchProjectDef_CanStartNow_Postfix))
        );

        harmony.Patch(
            AccessTools.PropertyGetter(typeof(MainTabWindow_Research), nameof(MainTabWindow_Research.VisibleResearchProjects)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(MainTabWindow_Research_VisibleResearchProjects_Postfix))
        );

        _isPatched = true;
        Log.Message("[MFVanilla] Vanilla tech research suppression patches applied.");
    }

    public static void ResetSuppressionCache()
    {
        SuppressedResearchDefNames.Clear();
        _suppressionCacheBuilt = false;
    }

    public static void NotifySettingsChanged()
    {
        ResetSuppressionCache();

        foreach (ResearchProjectDef project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
        {
            project.ClearCachedData();
        }

        RefreshOpenResearchWindows();
    }

    public static bool IsResearchSuppressed(ResearchProjectDef project)
    {
        if (MFVanillaMod.Settings == null || !MFVanillaMod.Settings.DisableTechResearch || project == null) return false;

        EnsureSuppressionCache();
        return SuppressedResearchDefNames.Contains(project.defName);
    }

    private static void EnsureSuppressionCache()
    {
        if (_suppressionCacheBuilt) return;

        SuppressedResearchDefNames.Clear();

        List<ResearchProjectDef> coreProjects = DefDatabase<ResearchProjectDef>.AllDefsListForReading
            .Where(project => project?.modContentPack?.IsCoreMod == true)
            .ToList();

        foreach (ResearchProjectDef root in coreProjects.Where(project => SuppressionRoots.Contains(project.defName)))
        {
            SuppressedResearchDefNames.Add(root.defName);
        }

        bool changed;
        do
        {
            changed = false;
            foreach (ResearchProjectDef project in coreProjects)
            {
                if (SuppressedResearchDefNames.Contains(project.defName)) continue;
                if (!DependsOnSuppressedResearch(project)) continue;

                SuppressedResearchDefNames.Add(project.defName);
                changed = true;
            }
        }
        while (changed);

        _suppressionCacheBuilt = true;
        Log.Message($"[MFVanilla] Suppressing {SuppressedResearchDefNames.Count} vanilla tech research projects.");
    }

    private static bool DependsOnSuppressedResearch(ResearchProjectDef project)
    {
        return HasSuppressedPrerequisite(project.prerequisites)
            || HasSuppressedPrerequisite(project.hiddenPrerequisites);
    }

    private static bool HasSuppressedPrerequisite(List<ResearchProjectDef> prerequisites)
    {
        return prerequisites != null
            && prerequisites.Any(prerequisite => prerequisite != null && SuppressedResearchDefNames.Contains(prerequisite.defName));
    }

    private static void ResearchProjectDef_IsHidden_Postfix(ResearchProjectDef __instance, ref bool __result)
    {
        if (IsResearchSuppressed(__instance))
        {
            __result = true;
        }
    }

    private static void ResearchProjectDef_CanStartNow_Postfix(ResearchProjectDef __instance, ref bool __result)
    {
        if (IsResearchSuppressed(__instance))
        {
            __result = false;
        }
    }

    private static void MainTabWindow_Research_VisibleResearchProjects_Postfix(ref List<ResearchProjectDef> __result)
    {
        if (MFVanillaMod.Settings == null || !MFVanillaMod.Settings.DisableTechResearch || __result == null) return;

        __result = __result
            .Where(project => !IsResearchSuppressed(project))
            .ToList();
    }

    private static void RefreshOpenResearchWindows()
    {
        if (Find.WindowStack == null) return;

        foreach (MainTabWindow_Research window in Find.WindowStack.Windows.OfType<MainTabWindow_Research>())
        {
            AccessTools.Field(typeof(MainTabWindow_Research), "cachedVisibleResearchProjects")?.SetValue(window, null);
            AccessTools.Field(typeof(MainTabWindow_Research), "cachedUnlockedDefsGroupedByPrerequisites")?.SetValue(window, null);
            AccessTools.Field(typeof(MainTabWindow_Research), "matchingProjects")?.SetValue(window, null);

            ResearchProjectDef selectedProject = AccessTools.Field(typeof(MainTabWindow_Research), "selectedProject")?.GetValue(window) as ResearchProjectDef;
            if (IsResearchSuppressed(selectedProject))
            {
                AccessTools.Field(typeof(MainTabWindow_Research), "selectedProject")?.SetValue(window, null);
            }
        }
    }
}
