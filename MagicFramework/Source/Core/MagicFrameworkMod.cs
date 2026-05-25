using MagicFramework.Debug;
using RimWorld;
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
        return "MF_SettingsCategory".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing = new();
        listing.Begin(inRect);
        listing.Label("MF_SettingsInterface".Translate());
        listing.GapLine();
        listing.CheckboxLabeled("MF_UseColoredSpellText".Translate(), ref settings.useColoredSpellText, "MF_UseColoredSpellTextTooltip".Translate());
        if (listing.ButtonText("MF_ShowLatestMagicFrameworkNotes".Translate()))
        {
            MagicFrameworkSplashUtility.ShowLatest();
        }

        listing.GapLine();
        listing.Label("MF_CasterLevelScaling".Translate());
        listing.GapLine();
        DrawPercentSlider(listing, "MF_DamagePerPower".Translate(), ref settings.damageScalingPerPower, 0f, 0.12f);
        DrawPercentSlider(listing, "MF_HealingPerPower".Translate(), ref settings.healingScalingPerPower, 0f, 0.12f);
        DrawPercentSlider(listing, "MF_RadiusRangePerPower".Translate(), ref settings.radiusScalingPerPower, 0f, 0.08f);
        DrawPercentSlider(listing, "MF_DurationPerPower".Translate(), ref settings.durationScalingPerPower, 0f, 0.12f);
        DrawPercentSlider(listing, "MF_ManaCostReductionPerPower".Translate(), ref settings.manaCostReductionPerPower, 0f, 0.08f);
        DrawPercentSlider(listing, "MF_CooldownReductionPerPower".Translate(), ref settings.cooldownReductionPerPower, 0f, 0.08f);
        listing.GapLine();
        listing.Label("MF_SoulHaunting".Translate());
        listing.GapLine();
        DrawIntSlider(listing, "MF_MaxScheduledHauntingsPerMap".Translate(), ref settings.maxHauntingsPerMap, 0, 50);
        DrawIntSlider(listing, "MF_MinimumHauntingRiskScore".Translate(), ref settings.hauntingMinimumRiskScore, 0, 100);
        DrawDaysSlider(listing, "MF_MinimumHauntingDelay".Translate(), ref settings.hauntingMinDelayTicks, 0, 5);
        DrawDaysSlider(listing, "MF_MaximumHauntingDelay".Translate(), ref settings.hauntingMaxDelayTicks, 0, 10);
        if (settings.hauntingMaxDelayTicks < settings.hauntingMinDelayTicks)
            settings.hauntingMaxDelayTicks = settings.hauntingMinDelayTicks;
        listing.GapLine();
        listing.Label("MF_DiagnosticLogging".Translate());
        listing.GapLine();
        listing.CheckboxLabeled("MF_LogExecution".Translate(), ref settings.logExecution, "MF_LogExecutionTooltip".Translate());
        listing.CheckboxLabeled("MF_LogCosts".Translate(), ref settings.logCosts, "MF_LogCostsTooltip".Translate());
        listing.CheckboxLabeled("MF_LogRequirements".Translate(), ref settings.logRequirements, "MF_LogRequirementsTooltip".Translate());
        listing.CheckboxLabeled("MF_LogTargeting".Translate(), ref settings.logTargeting, "MF_LogTargetingTooltip".Translate());
        listing.CheckboxLabeled("MF_LogTriggers".Translate(), ref settings.logTriggers, "MF_LogTriggersTooltip".Translate());
        listing.CheckboxLabeled("MF_LogPersistentEffects".Translate(), ref settings.logPersistentEffects, "MF_LogPersistentEffectsTooltip".Translate());
        listing.CheckboxLabeled("MF_LogWallZones".Translate(), ref settings.logWallZones, "MF_LogWallZonesTooltip".Translate());
        listing.CheckboxLabeled("MF_LogAreaZones".Translate(), ref settings.logAreaZones, "MF_LogAreaZonesTooltip".Translate());
        listing.CheckboxLabeled("MF_LogStatModifiers".Translate(), ref settings.logStatModifiers, "MF_LogStatModifiersTooltip".Translate());
        listing.CheckboxLabeled("MF_LogDisplacement".Translate(), ref settings.logDisplacement, "MF_LogDisplacementTooltip".Translate());
        listing.CheckboxLabeled("MF_LogProjectiles".Translate(), ref settings.logProjectiles, "MF_LogProjectilesTooltip".Translate());
        listing.CheckboxLabeled("MF_LogForceFields".Translate(), ref settings.logForceFields, "MF_LogForceFieldsTooltip".Translate());
        listing.CheckboxLabeled("MF_LogEnhancements".Translate(), ref settings.logEnhancements, "MF_LogEnhancementsTooltip".Translate());
        listing.CheckboxLabeled("MF_LogVisuals".Translate(), ref settings.logVisuals, "MF_LogVisualsTooltip".Translate());
        listing.CheckboxLabeled("MF_LogSummons".Translate(), ref settings.logSummons, "MF_LogSummonsTooltip".Translate());
        listing.GapLine();

        if (listing.ButtonText("MF_DisableRoutineLogging".Translate()))
        {
            SetAllLogging(false);
        }

        if (listing.ButtonText("MF_EnableAllRoutineLogging".Translate()))
        {
            SetAllLogging(true);
        }

        listing.GapLine();
        listing.Label("MF_DebugLogSnapshots".Translate());

        Rect buttonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(buttonRect, "MF_LogDelayedRuntime".Translate()))
        {
            SpellDebugUtility.LogDelayedSpellRuntime();
        }

        Rect triggerButtonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(triggerButtonRect, "MF_LogArmedTriggers".Translate()))
        {
            SpellDebugUtility.LogArmedSpellTriggers();
        }

        Rect persistentButtonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(persistentButtonRect, "MF_LogPersistentEffectsSnapshot".Translate()))
        {
            SpellDebugUtility.LogPersistentSpellEffects();
        }

        Rect wallZoneButtonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(wallZoneButtonRect, "MF_LogWallZonesSnapshot".Translate()))
        {
            SpellDebugUtility.LogWallZones();
        }

        Rect areaZoneButtonRect = listing.GetRect(35f);
        if (Widgets.ButtonText(areaZoneButtonRect, "MF_LogAreaZonesSnapshot".Translate()))
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
        listing.Label("MF_SettingValue".Translate(label, value.ToStringPercent("F0")));
        value = listing.Slider(value, min, max);
    }

    private static void DrawIntSlider(Listing_Standard listing, string label, ref int value, int min, int max)
    {
        value = Mathf.RoundToInt(Mathf.Clamp(value, min, max));
        listing.Label("MF_SettingValue".Translate(label, value));
        value = Mathf.RoundToInt(listing.Slider(value, min, max));
    }

    private static void DrawDaysSlider(Listing_Standard listing, string label, ref int value, int minDays, int maxDays)
    {
        int days = Mathf.RoundToInt(value / (float)GenDate.TicksPerDay);
        days = Mathf.Clamp(days, minDays, maxDays);
        listing.Label("MF_SettingDaysValue".Translate(label, days));
        days = Mathf.RoundToInt(listing.Slider(days, minDays, maxDays));
        value = days * GenDate.TicksPerDay;
    }
}
