using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public Dialog_MagicFrameworkSplash(List<SplashNoteDef> notes, string latestKey)
    {
        this.notes = notes ?? new List<SplashNoteDef>();
        this.latestKey = latestKey;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        doCloseX = true;
    }

    public override Vector2 InitialSize => new(720f, 640f);

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), "Magic Framework notes");
        Text.Font = GameFont.Small;

        Rect bodyRect = new(inRect.x, inRect.y + 44f, inRect.width, inRect.height - 94f);
        float viewHeight = Math.Max(bodyRect.height + 1f, EstimateViewHeight(bodyRect.width));
        Rect viewRect = new(0f, 0f, bodyRect.width - 18f, viewHeight);
        Widgets.BeginScrollView(bodyRect, ref scrollPosition, viewRect);

        float y = 0f;
        Widgets.Label(new Rect(0f, y, viewRect.width, 44f), "A few important details from MagicFramework and active first-party magic mods.");
        y += 52f;

        foreach (SplashNoteDef note in notes)
        {
            Text.Font = GameFont.Medium;
            string heading = note.DisplayModName;
            string version = ResolveVersion(note);
            if (!string.IsNullOrWhiteSpace(version))
            {
                heading += " " + version;
            }

            Widgets.Label(new Rect(0f, y, viewRect.width, 30f), heading);
            y += 32f;
            Text.Font = GameFont.Small;

            foreach (string noteText in note.notes)
            {
                string line = "- " + noteText;
                float height = Text.CalcHeight(line, viewRect.width - 12f);
                Widgets.Label(new Rect(12f, y, viewRect.width - 12f, height), line);
                y += height + 8f;
            }

            y += 14f;
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

        return y + 12f;
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
