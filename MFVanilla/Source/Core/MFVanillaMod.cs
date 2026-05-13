using HarmonyLib;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

/// <summary>
/// Main mod class for MFVanilla - handles settings and research suppression.
/// </summary>
public sealed class MFVanillaMod : Mod
{
    /// <summary>
    /// Harmony instance for this mod.
    /// </summary>
    private Harmony _harmony;

    /// <summary>
    /// Settings container for MFVanilla mod settings.
    /// </summary>
    public class MFVanillaSettings : ModSettings
    {
        /// <summary>
        /// Whether to hide vanilla electricity and downstream tech research.
        /// </summary>
        public bool DisableTechResearch = true;

        /// <summary>
        /// Whether to show a warning on game start when tech is disabled.
        /// </summary>
        public bool ShowTechDisabledWarning = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DisableTechResearch, "disableTechResearch", true);
            Scribe_Values.Look(ref ShowTechDisabledWarning, "showTechDisabledWarning", true);
        }
    }

    /// <summary>
    /// Static accessor for settings from anywhere in the mod.
    /// </summary>
    public static MFVanillaSettings Settings { get; private set; }
    public static ModContentPack ContentPack { get; private set; }

    public MFVanillaMod(ModContentPack content)
        : base(content)
    {
        ContentPack = content;
        Settings = GetSettings<MFVanillaSettings>();
        
        // Initialize Harmony
        _harmony = new Harmony("oracle.mfvanilla");
        MFVanillaPatcher.Patch(_harmony);
        
        Log.Message("[MFVanilla] Mod instance created.");
    }

    public override string SettingsCategory()
    {
        return "MF Vanilla";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing = new();
        listing.Begin(inRect);

        listing.Label("MFV_SettingsTitle".Translate());
        listing.GapLine();

        bool disableTechResearch = Settings.DisableTechResearch;
        listing.CheckboxLabeled(
            "MFV_SuppressVanillaTechResearch".Translate(),
            ref disableTechResearch,
            "MFV_SuppressVanillaTechResearchTooltip".Translate());

        if (disableTechResearch != Settings.DisableTechResearch)
        {
            Settings.DisableTechResearch = disableTechResearch;
            MFVanillaPatcher.NotifySettingsChanged();
            WriteSettings();
        }

        listing.Label("MFV_SuppressVanillaTechResearchDescription".Translate());
        listing.Gap();

        if (listing.ButtonText("MFV_RestoreVanillaTech".Translate()))
        {
            Settings.DisableTechResearch = false;
            MFVanillaPatcher.NotifySettingsChanged();
            WriteSettings();
        }

        listing.GapLine();

        bool showWarning = Settings.ShowTechDisabledWarning;
        listing.CheckboxLabeled(
            "MFV_ShowTechDisabledWarning".Translate(),
            ref showWarning,
            "MFV_ShowTechDisabledWarningTooltip".Translate());

        if (showWarning != Settings.ShowTechDisabledWarning)
        {
            Settings.ShowTechDisabledWarning = showWarning;
            WriteSettings();
        }

        listing.Label("MFV_ShowTechDisabledWarningDescription".Translate());
        listing.Gap();

        if (listing.ButtonText("MFV_ResetSettingsToDefaults".Translate()))
        {
            Settings.DisableTechResearch = true;
            Settings.ShowTechDisabledWarning = true;
            MFVanillaPatcher.NotifySettingsChanged();
            WriteSettings();
        }

        listing.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        MFVanillaPatcher.NotifySettingsChanged();
    }
}
