using MagicFramework.Debug;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Mod entry point reserved for future settings and diagnostics.
/// </summary>
public sealed class MagicFrameworkMod : Mod
{
    private readonly MagicFrameworkSettings settings;

    public MagicFrameworkMod(ModContentPack content)
        : base(content)
    {
        settings = GetSettings<MagicFrameworkSettings>();
        MagicFrameworkSettings.SetCurrent(settings);
        MagicFrameworkSplashUtility.QueueShowIfNew();
    }

    public override string SettingsCategory()
    {
        return "Magic Framework";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing = new();
        listing.Begin(inRect);
        listing.Label("Interface");
        listing.GapLine();
        listing.CheckboxLabeled("Use colored spell text", ref settings.useColoredSpellText, "Adds color tags to generated spell summaries where RimWorld rich text supports them.");
        if (listing.ButtonText("Show latest Magic Framework notes"))
        {
            MagicFrameworkSplashUtility.ShowLatest();
        }

        listing.GapLine();
        listing.Label("Caster level scaling");
        listing.GapLine();
        DrawPercentSlider(listing, "Damage per power", ref settings.damageScalingPerPower, 0f, 0.12f);
        DrawPercentSlider(listing, "Healing per power", ref settings.healingScalingPerPower, 0f, 0.12f);
        DrawPercentSlider(listing, "Radius/range per power", ref settings.radiusScalingPerPower, 0f, 0.08f);
        DrawPercentSlider(listing, "Duration per power", ref settings.durationScalingPerPower, 0f, 0.12f);
        DrawPercentSlider(listing, "Mana cost reduction per power", ref settings.manaCostReductionPerPower, 0f, 0.08f);
        DrawPercentSlider(listing, "Cooldown reduction per power", ref settings.cooldownReductionPerPower, 0f, 0.08f);
        listing.GapLine();
        listing.Label("Diagnostic logging");
        listing.GapLine();
        listing.CheckboxLabeled("Execution", ref settings.logExecution, "Logs action execution and general spell runtime flow.");
        listing.CheckboxLabeled("Costs", ref settings.logCosts, "Logs mana spending and cooldown starts.");
        listing.CheckboxLabeled("Requirements", ref settings.logRequirements, "Logs requirement and cast validation diagnostics.");
        listing.CheckboxLabeled("Targeting", ref settings.logTargeting, "Logs target query and target resolution diagnostics.");
        listing.CheckboxLabeled("Triggers", ref settings.logTriggers, "Logs armed spell trigger scheduling and firing.");
        listing.CheckboxLabeled("Persistent effects", ref settings.logPersistentEffects, "Logs persistent markers and generic persistent effects.");
        listing.CheckboxLabeled("Wall zones", ref settings.logWallZones, "Logs persistent wall zone creation and runtime diagnostics.");
        listing.CheckboxLabeled("Area zones", ref settings.logAreaZones, "Logs persistent area zone creation and runtime diagnostics.");
        listing.CheckboxLabeled("Stat modifiers", ref settings.logStatModifiers, "Logs timed and sustained stat modifier activity.");
        listing.CheckboxLabeled("Displacement", ref settings.logDisplacement, "Logs knockback, pull, and teleport activity.");
        listing.CheckboxLabeled("Projectiles", ref settings.logProjectiles, "Logs projectile launch and impact runtime activity.");
        listing.CheckboxLabeled("Force fields", ref settings.logForceFields, "Logs maintained force-field absorption, reduction, upkeep, and breaks.");
        listing.CheckboxLabeled("Enhancements", ref settings.logEnhancements, "Logs enhancement rule diagnostics.");
        listing.CheckboxLabeled("Visuals", ref settings.logVisuals, "Logs procedural visual effect playback.");
        listing.CheckboxLabeled("Summons", ref settings.logSummons, "Logs summoned pawns and temporary spawned things.");
        listing.GapLine();

        if (listing.ButtonText("Disable Routine Logging"))
        {
            SetAllLogging(false);
        }

        if (listing.ButtonText("Enable All Routine Logging"))
        {
            SetAllLogging(true);
        }

        listing.GapLine();
        listing.Label("Debug log snapshots");

        Rect buttonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(buttonRect, "Log Delayed Runtime"))
        {
            SpellDebugUtility.LogDelayedSpellRuntime();
        }

        Rect triggerButtonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(triggerButtonRect, "Log Armed Triggers"))
        {
            SpellDebugUtility.LogArmedSpellTriggers();
        }

        Rect persistentButtonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(persistentButtonRect, "Log Persistent Effects"))
        {
            SpellDebugUtility.LogPersistentSpellEffects();
        }

        Rect wallZoneButtonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(wallZoneButtonRect, "Log Wall Zones"))
        {
            SpellDebugUtility.LogWallZones();
        }

        Rect areaZoneButtonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(areaZoneButtonRect, "Log Area Zones"))
        {
            SpellDebugUtility.LogAreaZones();
        }

        listing.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        MagicFrameworkSettings.SetCurrent(settings);
    }

    private void SetAllLogging(bool enabled)
    {
        settings.logExecution = enabled;
        settings.logCosts = enabled;
        settings.logRequirements = enabled;
        settings.logTargeting = enabled;
        settings.logTriggers = enabled;
        settings.logPersistentEffects = enabled;
        settings.logWallZones = enabled;
        settings.logAreaZones = enabled;
        settings.logStatModifiers = enabled;
        settings.logDisplacement = enabled;
        settings.logProjectiles = enabled;
        settings.logForceFields = enabled;
        settings.logEnhancements = enabled;
        settings.logVisuals = enabled;
        settings.logSummons = enabled;
    }

    private static void DrawPercentSlider(Listing_Standard listing, string label, ref float value, float min, float max)
    {
        value = Mathf.Clamp(value, min, max);
        listing.Label($"{label}: {value.ToStringPercent("F0")}");
        value = listing.Slider(value, min, max);
    }
}
