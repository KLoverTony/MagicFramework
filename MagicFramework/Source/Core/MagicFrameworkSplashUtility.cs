using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

public static class MagicFrameworkSplashUtility
{
    private static bool queuedStartupCheck;

    public static void QueueShowIfNew()
    {
        if (queuedStartupCheck)
        {
            return;
        }

        queuedStartupCheck = true;
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            queuedStartupCheck = false;
            ShowIfNew();
        });
    }

    public static void ShowIfNew()
    {
        List<SplashNoteDef> notes = CurrentNotes();
        if (notes.Count == 0)
        {
            return;
        }

        string latestKey = LatestSeenKey(notes);
        MagicFrameworkSettings settings = MagicFrameworkSettings.Current;
        if (settings != null && settings.lastSeenSplashKey == latestKey)
        {
            return;
        }

        Show(notes, latestKey);
    }

    public static void ShowLatest()
    {
        List<SplashNoteDef> notes = CurrentNotes();
        if (notes.Count == 0)
        {
            return;
        }

        Show(notes, LatestSeenKey(notes));
    }

    private static void Show(List<SplashNoteDef> notes, string latestKey)
    {
        if (Find.WindowStack == null)
        {
            return;
        }

        Find.WindowStack.Add(new Dialog_MagicFrameworkSplash(notes, latestKey));
    }

    private static List<SplashNoteDef> CurrentNotes()
    {
        return DefDatabase<SplashNoteDef>.AllDefsListForReading
            .Where(def => def != null && def.notes != null && def.notes.Count > 0)
            .OrderBy(def => def.order)
            .ThenBy(def => def.DisplayModName)
            .ThenBy(def => def.defName)
            .ToList();
    }

    private static string LatestSeenKey(List<SplashNoteDef> notes)
    {
        return string.Join("|", notes.Select(note => note.SeenKey).OrderBy(key => key).ToArray());
    }
}

public sealed class Dialog_MagicFrameworkSplash : Window
{
    private readonly List<SplashNoteDef> notes;
    private readonly string latestKey;
    private Vector2 scrollPosition;
    private Texture2D bannerTexture;

    public Dialog_MagicFrameworkSplash(List<SplashNoteDef> notes, string latestKey)
    {
        this.notes = notes ?? new List<SplashNoteDef>();
        this.latestKey = latestKey;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        doCloseX = true;
    }

    public override Vector2 InitialSize => new(760f, 700f);

    public override void DoWindowContents(Rect inRect)
    {
        bannerTexture ??= LoadBannerTexture();

        Text.Font = GameFont.Medium;
        Color originalColor = GUI.color;
        GUI.color = new Color(0.72f, 0.9f, 1f);
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), "Magic Framework notes");
        GUI.color = originalColor;
        Text.Font = GameFont.Small;

        float yTop = inRect.y + 40f;
        if (bannerTexture != null)
        {
            Rect bannerRect = new(inRect.x, yTop, inRect.width, 120f);
            GUI.DrawTexture(bannerRect, bannerTexture, ScaleMode.ScaleAndCrop);
            Widgets.DrawBoxSolid(new Rect(bannerRect.x, bannerRect.yMax - 26f, bannerRect.width, 26f), new Color(0f, 0f, 0f, 0.45f));
            GUI.color = new Color(1f, 0.86f, 0.42f);
            Widgets.Label(new Rect(bannerRect.x + 12f, bannerRect.yMax - 25f, bannerRect.width - 24f, 24f), "A brighter doorway into the arcane colony path.");
            GUI.color = originalColor;
            yTop += 132f;
        }

        Rect bodyRect = new(inRect.x, yTop, inRect.width, inRect.yMax - yTop - 50f);
        float viewHeight = Math.Max(bodyRect.height + 1f, EstimateViewHeight(bodyRect.width));
        Rect viewRect = new(0f, 0f, bodyRect.width - 18f, viewHeight);
        Widgets.BeginScrollView(bodyRect, ref scrollPosition, viewRect);

        float y = 0f;
        GUI.color = new Color(0.78f, 1f, 0.82f);
        Widgets.Label(new Rect(0f, y, viewRect.width, 44f), "A few important details from MagicFramework and active first-party magic mods.");
        GUI.color = originalColor;
        y += 52f;

        Color[] noteColors =
        {
            new(0.98f, 0.72f, 0.42f),
            new(0.62f, 0.88f, 1f),
            new(0.8f, 0.72f, 1f),
            new(0.68f, 1f, 0.76f)
        };

        foreach (SplashNoteDef note in notes)
        {
            Text.Font = GameFont.Medium;
            string heading = note.DisplayModName;
            string version = ResolveVersion(note);
            if (!string.IsNullOrWhiteSpace(version))
            {
                heading += " " + version;
            }

            GUI.color = noteColors[Math.Abs(note.defName.GetHashCode()) % noteColors.Length];
            Widgets.Label(new Rect(0f, y, viewRect.width, 30f), heading);
            GUI.color = originalColor;
            y += 32f;
            Text.Font = GameFont.Small;

            for (int i = 0; i < note.notes.Count; i++)
            {
                string noteText = note.notes[i];
                string line = "- " + noteText;
                float height = Text.CalcHeight(line, viewRect.width - 12f);
                GUI.color = noteColors[i % noteColors.Length];
                Widgets.Label(new Rect(12f, y, viewRect.width - 12f, height), line);
                GUI.color = originalColor;
                y += height + 8f;
            }

            y += 14f;
        }

        if (MFVanillaSplashSettings.Available)
        {
            y = DrawMFVanillaToggle(new Rect(0f, y, viewRect.width, 118f), y);
        }

        Widgets.EndScrollView();

        Rect closeRect = new(inRect.xMax - 160f, inRect.yMax - 40f, 160f, 36f);
        if (Widgets.ButtonText(closeRect, "Close"))
        {
            Close();
        }
    }

    public override void PostClose()
    {
        base.PostClose();
        MagicFrameworkSettings settings = MagicFrameworkSettings.Current;
        if (settings != null)
        {
            settings.lastSeenSplashKey = latestKey;
            LoadedModManager.GetMod<MagicFrameworkMod>()?.WriteSettings();
        }
    }

    private float EstimateViewHeight(float width)
    {
        float y = 52f;
        float labelWidth = width - 30f;
        foreach (SplashNoteDef note in notes)
        {
            y += 32f;
            if (note.notes != null)
            {
                foreach (string noteText in note.notes)
                {
                    y += Text.CalcHeight("- " + noteText, labelWidth) + 8f;
                }
            }

            y += 14f;
        }

        if (MFVanillaSplashSettings.Available)
        {
            y += 128f;
        }

        return y + 12f;
    }

    private static float DrawMFVanillaToggle(Rect rect, float y)
    {
        Color originalColor = GUI.color;
        Widgets.DrawBoxSolid(rect, new Color(0.08f, 0.1f, 0.12f, 0.72f));
        Widgets.DrawBox(rect);

        GUI.color = new Color(0.9f, 0.72f, 1f);
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, 28f), "MF Vanilla research path");
        Text.Font = GameFont.Small;

        GUI.color = Color.white;
        bool disabled = MFVanillaSplashSettings.DisableTechResearch;
        bool enabled = !disabled;
        Widgets.CheckboxLabeled(
            new Rect(rect.x + 12f, rect.y + 42f, rect.width - 24f, 28f),
            "Enable vanilla technology research",
            ref enabled);

        if (enabled == disabled)
        {
            MFVanillaSplashSettings.DisableTechResearch = !enabled;
        }

        GUI.color = new Color(0.78f, 1f, 0.82f);
        Widgets.Label(
            new Rect(rect.x + 12f, rect.y + 72f, rect.width - 24f, 42f),
            "Leave it off to embrace immersion: colonies can grow through spellcraft, reagents, enchanted workbenches, and arcane discoveries instead of the standard industrial ladder.");
        GUI.color = originalColor;
        return y + rect.height + 12f;
    }

    private static Texture2D LoadBannerTexture()
    {
        ModContentPack mod = LoadedModManager.RunningModsListForReading
            .FirstOrDefault(pack => string.Equals(pack.PackageId, "oracle.magicframework", StringComparison.OrdinalIgnoreCase));
        string path = mod == null ? null : Path.Combine(mod.RootDir, "About", "Banner.png");
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            Texture2D texture = new(2, 2);
            return ImageConversion.LoadImage(texture, File.ReadAllBytes(path)) ? texture : null;
        }
        catch (Exception ex)
        {
            Log.Warning("[MagicFramework] Could not load splash banner: " + ex.Message);
            return null;
        }
    }

    private static string ResolveVersion(SplashNoteDef note)
    {
        if (note == null || string.IsNullOrWhiteSpace(note.packageId))
        {
            return null;
        }

        ModContentPack mod = LoadedModManager.RunningModsListForReading
            .FirstOrDefault(pack => string.Equals(pack.PackageId, note.packageId, StringComparison.OrdinalIgnoreCase));
        if (mod == null || string.IsNullOrWhiteSpace(mod.RootDir))
        {
            return null;
        }

        try
        {
            string aboutPath = Path.Combine(mod.RootDir, "About", "About.xml");
            if (!File.Exists(aboutPath))
            {
                return null;
            }

            string version = XDocument.Load(aboutPath).Root?.Element("modVersion")?.Value;
            return string.IsNullOrWhiteSpace(version) ? null : "v" + version.Trim();
        }
        catch (Exception ex)
        {
            Log.Warning("[MagicFramework] Could not read splash note version for " + note.defName + ": " + ex.Message);
            return null;
        }
    }
}

internal static class MFVanillaSplashSettings
{
    private static readonly Type ModType = Type.GetType("MFVanilla.Core.MFVanillaMod, MFVanilla");
    private static readonly Type PatcherType = Type.GetType("MFVanilla.Core.MFVanillaPatcher, MFVanilla");
    private static readonly PropertyInfo SettingsProperty = ModType?.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static);
    private static readonly MethodInfo NotifySettingsChangedMethod = PatcherType?.GetMethod("NotifySettingsChanged", BindingFlags.Public | BindingFlags.Static);

    public static bool Available => Settings != null;

    public static bool DisableTechResearch
    {
        get
        {
            object settings = Settings;
            FieldInfo settingField = settings?.GetType().GetField("DisableTechResearch", BindingFlags.Public | BindingFlags.Instance);
            return settingField != null && (bool)settingField.GetValue(settings);
        }
        set
        {
            object settings = Settings;
            FieldInfo settingField = settings?.GetType().GetField("DisableTechResearch", BindingFlags.Public | BindingFlags.Instance);
            if (settingField == null || (bool)settingField.GetValue(settings) == value)
            {
                return;
            }

            settingField.SetValue(settings, value);
            Mod mod = LoadedModManager.ModHandles?.FirstOrDefault(handle => ModType != null && handle.GetType() == ModType);
            if (mod != null)
            {
                mod.WriteSettings();
            }
            else
            {
                NotifySettingsChangedMethod?.Invoke(null, null);
            }
        }
    }

    private static object Settings => SettingsProperty?.GetValue(null, null);
}
