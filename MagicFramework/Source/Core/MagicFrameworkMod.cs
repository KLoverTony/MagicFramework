using MagicFramework.Debug;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Mod entry point reserved for future settings and diagnostics.
/// </summary>
public sealed class MagicFrameworkMod : Mod
{
    public MagicFrameworkMod(ModContentPack content)
        : base(content)
    {
        Log.Message("[MagicFramework] Mod instance created.");
    }

    public override string SettingsCategory()
    {
        return "Magic Framework";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Rect buttonRect = new(inRect.x, inRect.y, Mathf.Min(260f, inRect.width), 35f);
        if (Widgets.ButtonText(buttonRect, "Log Delayed Runtime"))
        {
            SpellDebugUtility.LogDelayedSpellRuntime();
        }

        Rect triggerButtonRect = new(inRect.x, buttonRect.yMax + 8f, Mathf.Min(260f, inRect.width), 35f);
        if (Widgets.ButtonText(triggerButtonRect, "Log Armed Triggers"))
        {
            SpellDebugUtility.LogArmedSpellTriggers();
        }

        Rect persistentButtonRect = new(inRect.x, triggerButtonRect.yMax + 8f, Mathf.Min(260f, inRect.width), 35f);
        if (Widgets.ButtonText(persistentButtonRect, "Log Persistent Effects"))
        {
            SpellDebugUtility.LogPersistentSpellEffects();
        }

        Rect wallZoneButtonRect = new(inRect.x, persistentButtonRect.yMax + 8f, Mathf.Min(260f, inRect.width), 35f);
        if (Widgets.ButtonText(wallZoneButtonRect, "Log Wall Zones"))
        {
            SpellDebugUtility.LogWallZones();
        }

        Rect areaZoneButtonRect = new(inRect.x, wallZoneButtonRect.yMax + 8f, Mathf.Min(260f, inRect.width), 35f);
        if (Widgets.ButtonText(areaZoneButtonRect, "Log Area Zones"))
        {
            SpellDebugUtility.LogAreaZones();
        }

        Rect labelRect = new(inRect.x, areaZoneButtonRect.yMax + 12f, inRect.width, 160f);
        Widgets.Label(labelRect, "Writes delayed spell runtime, armed trigger details, persistent spell marker details, wall zone details, and area zone details for every loaded map to the RimWorld log.");
    }
}
