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

    public MFVanillaMod(ModContentPack content)
        : base(content)
    {
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
        float y = inRect.y;
        float width = inRect.width - 200f;

        // Title
        Widgets.Label(new Rect(inRect.x, y, width, 30f), "Magic Framework Vanilla Settings");
        y += 40f;

        bool disableTechResearch = Settings.DisableTechResearch;
        Widgets.CheckboxLabeled(new Rect(inRect.x, y, width, 30f), "Suppress Vanilla Tech Research", ref disableTechResearch);
        if (disableTechResearch != Settings.DisableTechResearch)
        {
            Settings.DisableTechResearch = disableTechResearch;
            MFVanillaPatcher.NotifySettingsChanged();
        }
        y += 30f;

        Widgets.Label(new Rect(inRect.x + 20f, y, width - 20f, 24f), "Hides Electricity, Microelectronics, Multi-Analyzer, and vanilla research that depends on them.");
        y += 34f;

        if (Widgets.ButtonText(new Rect(inRect.x + 20f, y, 180f, 30f), "Restore Vanilla Tech"))
        {
            Settings.DisableTechResearch = false;
            MFVanillaPatcher.NotifySettingsChanged();
            WriteSettings();
        }
        y += 42f;

        Widgets.CheckboxLabeled(new Rect(inRect.x, y, width, 30f), "Show Tech Disabled Warning", ref Settings.ShowTechDisabledWarning);
        y += 30f;

        Widgets.Label(new Rect(inRect.x + 20f, y, width - 20f, 24f), "Show a warning message when tech is disabled on game start.");
        y += 40f;

        // Reset button
        if (Widgets.ButtonText(new Rect(inRect.x, y, 200f, 30f), "Reset to Defaults"))
        {
            Settings.DisableTechResearch = true;
            Settings.ShowTechDisabledWarning = true;
            MFVanillaPatcher.NotifySettingsChanged();
            WriteSettings();
        }
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        MFVanillaPatcher.NotifySettingsChanged();
    }
}
