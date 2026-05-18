using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class LeylineMapComponent : MapComponent
{
    private const string LeylineSensitivityResearchDefName = "MFV_LeylineSensitivity";
    private const int BaseLineStrength = 2;
    private const int FalloffStrength = 1;
    public const int NexusThreshold = 4;
    private const float NumericOverlayMinCellPixels = 22f;

    private static readonly Color WeakColor = new(0.25f, 0.9f, 1f, 0.24f);
    private static readonly Color LineColor = new(0.35f, 0.62f, 1f, 0.38f);
    private static readonly Color StrongColor = new(0.72f, 0.42f, 1f, 0.48f);
    private static readonly Color NexusColor = new(1f, 0.78f, 0.28f, 0.62f);

    private List<byte> leylineStrengthByCell;
    private List<LeylineSegmentRecord> generatedSegments;
    private bool generated;
    private int generationSeed;
    private int maxStrength;
    private int influencedCellCount;
    private int nexusCellCount;

    public LeylineMapComponent(Map map)
        : base(map)
    {
    }

    public static bool OverlayVisible { get; private set; }

    public bool HasLeylineData => generated && leylineStrengthByCell != null && leylineStrengthByCell.Count == map.cellIndices.NumGridCells;

    public int MaxStrength
    {
        get
        {
            EnsureGenerated();
            return maxStrength;
        }
    }

    public int InfluencedCellCount
    {
        get
        {
            EnsureGenerated();
            return influencedCellCount;
        }
    }

    public int NexusCellCount
    {
        get
        {
            EnsureGenerated();
            return nexusCellCount;
        }
    }

    public int SegmentCount
    {
        get
        {
            EnsureGenerated();
            return generatedSegments?.Count ?? 0;
        }
    }

    public int StrengthAt(IntVec3 cell)
    {
        EnsureGenerated();
        if (!cell.InBounds(map) || leylineStrengthByCell == null)
        {
            return 0;
        }

        int index = map.cellIndices.CellToIndex(cell);
        return index >= 0 && index < leylineStrengthByCell.Count ? leylineStrengthByCell[index] : 0;
    }

    public LeylineAreaReading ReadCell(IntVec3 cell)
    {
        int strength = StrengthAt(cell);
        return strength > 0
            ? new LeylineAreaReading(strength, strength, 1, strength >= NexusThreshold ? 1 : 0)
            : new LeylineAreaReading(0, 0, cell.InBounds(map) ? 1 : 0, 0);
    }

    public bool IsNexus(IntVec3 cell)
    {
        return StrengthAt(cell) >= NexusThreshold;
    }

    public static bool LeylineSensitivityResearched()
    {
        ResearchProjectDef research = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(LeylineSensitivityResearchDefName);
        return research?.IsFinished == true;
    }

    public static void ToggleOverlay()
    {
        OverlayVisible = !OverlayVisible;
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        EnsureGenerated();
    }

    public override void MapGenerated()
    {
        base.MapGenerated();
        EnsureGenerated();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref leylineStrengthByCell, "leylineStrengthByCell", LookMode.Value);
        Scribe_Collections.Look(ref generatedSegments, "generatedSegments", LookMode.Deep);
        Scribe_Values.Look(ref generated, "generated", false);
        Scribe_Values.Look(ref generationSeed, "generationSeed", 0);
        Scribe_Values.Look(ref maxStrength, "maxStrength", 0);
        Scribe_Values.Look(ref influencedCellCount, "influencedCellCount", 0);
        Scribe_Values.Look(ref nexusCellCount, "nexusCellCount", 0);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (leylineStrengthByCell != null && map != null && leylineStrengthByCell.Count == map.cellIndices.NumGridCells)
            {
                generated = true;
                RecalculateStats();
            }
            else
            {
                generated = false;
                leylineStrengthByCell = null;
                generatedSegments = null;
            }
        }
    }

    public override void MapComponentOnGUI()
    {
        base.MapComponentOnGUI();
        if (Find.CurrentMap != map || !ShouldShowLeylineControls())
        {
            return;
        }

        const float width = 122f;
        const float height = 32f;
        Rect rect = new(UI.screenWidth - width - 16f, 112f, width, height);
        Color originalColor = GUI.color;
        GUI.color = OverlayVisible ? new Color(0.58f, 0.92f, 1f, 1f) : Color.white;
        if (Widgets.ButtonText(rect, OverlayVisible ? "Leylines: on" : "Leylines"))
        {
            ToggleOverlay();
        }

        GUI.color = originalColor;
        TooltipHandler.TipRegion(rect, LeylineSensitivityResearched()
            ? "Show the hidden leyline strength map revealed by Leyline Sensitivity."
            : "Dev mode preview of the hidden leyline strength map. Research Leyline Sensitivity to unlock this overlay in normal play.");

        DrawStrengthNumbersIfEnabled();
    }

    public override void MapComponentDraw()
    {
        base.MapComponentDraw();
        if (!OverlayVisible || Find.CurrentMap != map || !ShouldShowLeylineControls())
        {
            return;
        }

        EnsureGenerated();
        if (leylineStrengthByCell == null)
        {
            return;
        }

        CellRect viewRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(1).ClipInsideMap(map);
        foreach (IntVec3 cell in viewRect.Cells)
        {
            if (cell.Fogged(map))
            {
                continue;
            }

            int strength = StrengthAt(cell);
            if (strength <= 0)
            {
                continue;
            }

            CellRenderer.RenderCell(cell, SolidColorMaterials.SimpleSolidColorMaterial(ColorForStrength(strength)));
        }
    }

    public void RegenerateForDebug()
    {
        generated = false;
        leylineStrengthByCell = null;
        generatedSegments = null;
        EnsureGenerated();
    }

    public string Summary()
    {
        EnsureGenerated();
        return $"Leyline map: seed={generationSeed}, segments={generatedSegments?.Count ?? 0}, influenced cells={influencedCellCount}, nexus cells={nexusCellCount}, max strength={maxStrength}";
    }

    private bool ShouldShowLeylineControls()
    {
        return LeylineSensitivityResearched() || Prefs.DevMode;
    }

    private void DrawStrengthNumbersIfEnabled()
    {
        if (!OverlayVisible
            || MFVanillaMod.Settings?.ShowLeylineStrengthNumbers != true
            || Find.CameraDriver.CellSizePixels < NumericOverlayMinCellPixels)
        {
            return;
        }

        EnsureGenerated();
        CellRect viewRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(1).ClipInsideMap(map);
        GameFont originalFont = Text.Font;
        TextAnchor originalAnchor = Text.Anchor;
        Color originalColor = GUI.color;
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.MiddleCenter;

        foreach (IntVec3 cell in viewRect.Cells)
        {
            int strength = StrengthAt(cell);
            if (strength <= 0)
            {
                continue;
            }

            Vector2 position = cell.ToUIPosition();
            Rect rect = new(position.x - 10f, position.y - 9f, 20f, 18f);
            GUI.color = strength >= NexusThreshold ? new Color(1f, 0.92f, 0.55f, 1f) : Color.white;
            Widgets.Label(rect, strength.ToString());
        }

        Text.Font = originalFont;
        Text.Anchor = originalAnchor;
        GUI.color = originalColor;
    }

    private void EnsureGenerated()
    {
        if (HasLeylineData)
        {
            return;
        }

        Generate();
    }

    private void Generate()
    {
        int cellCount = map.cellIndices.NumGridCells;
        leylineStrengthByCell = Enumerable.Repeat((byte)0, cellCount).ToList();
        generatedSegments = new List<LeylineSegmentRecord>();
        generationSeed = StableHash($"{Find.World?.info?.seedString}|{map.Tile}|{map.Size.x}|{map.Size.z}|MFV_Leylines");
        System.Random random = new(generationSeed);

        int lineCount = Mathf.Clamp(Mathf.RoundToInt((map.Size.x + map.Size.z) / 70f), 4, 9);
        for (int i = 0; i < lineCount; i++)
        {
            IntVec3 start = RandomEdgeCell(random, map);
            IntVec3 end = RandomEdgeCell(random, map);
            int guard = 0;
            while (SameEdge(start, end, map) && guard < 12)
            {
                end = RandomEdgeCell(random, map);
                guard++;
            }

            generatedSegments.Add(new LeylineSegmentRecord(start, end));
            DrawLine(start, end);
        }

        generated = true;
        RecalculateStats();
    }

    private void DrawLine(IntVec3 start, IntVec3 end)
    {
        int x0 = start.x;
        int z0 = start.z;
        int x1 = end.x;
        int z1 = end.z;
        int dx = Math.Abs(x1 - x0);
        int dz = Math.Abs(z1 - z0);
        int sx = x0 < x1 ? 1 : -1;
        int sz = z0 < z1 ? 1 : -1;
        int err = dx - dz;

        while (true)
        {
            IntVec3 cell = new(x0, 0, z0);
            AddStrength(cell, BaseLineStrength);
            AddStrength(cell + IntVec3.North, FalloffStrength);
            AddStrength(cell + IntVec3.East, FalloffStrength);
            AddStrength(cell + IntVec3.South, FalloffStrength);
            AddStrength(cell + IntVec3.West, FalloffStrength);

            if (x0 == x1 && z0 == z1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 > -dz)
            {
                err -= dz;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                z0 += sz;
            }
        }
    }

    private void AddStrength(IntVec3 cell, int amount)
    {
        if (!cell.InBounds(map) || amount <= 0)
        {
            return;
        }

        int index = map.cellIndices.CellToIndex(cell);
        int value = leylineStrengthByCell[index] + amount;
        leylineStrengthByCell[index] = (byte)Mathf.Clamp(value, 0, byte.MaxValue);
    }

    private void RecalculateStats()
    {
        maxStrength = 0;
        influencedCellCount = 0;
        nexusCellCount = 0;
        if (leylineStrengthByCell == null)
        {
            return;
        }

        for (int i = 0; i < leylineStrengthByCell.Count; i++)
        {
            int strength = leylineStrengthByCell[i];
            if (strength <= 0)
            {
                continue;
            }

            influencedCellCount++;
            if (strength >= NexusThreshold)
            {
                nexusCellCount++;
            }

            if (strength > maxStrength)
            {
                maxStrength = strength;
            }
        }
    }

    private static Color ColorForStrength(int strength)
    {
        if (strength >= NexusThreshold)
        {
            return NexusColor;
        }

        if (strength >= 3)
        {
            return StrongColor;
        }

        return strength == 2 ? LineColor : WeakColor;
    }

    private static IntVec3 RandomEdgeCell(System.Random random, Map map)
    {
        int edge = random.Next(4);
        return edge switch
        {
            0 => new IntVec3(random.Next(map.Size.x), 0, 0),
            1 => new IntVec3(random.Next(map.Size.x), 0, map.Size.z - 1),
            2 => new IntVec3(0, 0, random.Next(map.Size.z)),
            _ => new IntVec3(map.Size.x - 1, 0, random.Next(map.Size.z)),
        };
    }

    private static bool SameEdge(IntVec3 a, IntVec3 b, Map map)
    {
        return (a.z == 0 && b.z == 0)
            || (a.z == map.Size.z - 1 && b.z == map.Size.z - 1)
            || (a.x == 0 && b.x == 0)
            || (a.x == map.Size.x - 1 && b.x == map.Size.x - 1);
    }

    private static int StableHash(string text)
    {
        unchecked
        {
            int hash = 23;
            for (int i = 0; i < text.Length; i++)
            {
                hash = hash * 31 + text[i];
            }

            return hash;
        }
    }
}

public readonly struct LeylineAreaReading
{
    public LeylineAreaReading(int peakStrength, int sumStrength, int cellCount, int nexusCellCount)
    {
        PeakStrength = peakStrength;
        SumStrength = sumStrength;
        CellCount = cellCount;
        NexusCellCount = nexusCellCount;
    }

    public int PeakStrength { get; }
    public int SumStrength { get; }
    public int CellCount { get; }
    public int NexusCellCount { get; }
    public float AverageStrength => CellCount <= 0 ? 0f : (float)SumStrength / CellCount;
    public bool HasNexus => NexusCellCount > 0;
}

public static class LeylineUtility
{
    public static LeylineAreaReading ReadCell(Map map, IntVec3 cell)
    {
        return map?.GetComponent<LeylineMapComponent>()?.ReadCell(cell) ?? default;
    }

    public static LeylineAreaReading ReadRadius(Map map, IntVec3 center, float radius)
    {
        if (map == null || !center.InBounds(map) || radius < 0f)
        {
            return default;
        }

        LeylineMapComponent component = map.GetComponent<LeylineMapComponent>();
        if (component == null)
        {
            return default;
        }

        int peak = 0;
        int sum = 0;
        int cells = 0;
        int nexus = 0;
        int radiusCeil = Mathf.CeilToInt(radius);
        float radiusSquared = radius * radius;
        CellRect rect = CellRect.CenteredOn(center, radiusCeil).ClipInsideMap(map);
        foreach (IntVec3 cell in rect.Cells)
        {
            if ((cell - center).LengthHorizontalSquared > radiusSquared)
            {
                continue;
            }

            cells++;
            int strength = component.StrengthAt(cell);
            if (strength <= 0)
            {
                continue;
            }

            sum += strength;
            peak = Mathf.Max(peak, strength);
            if (strength >= LeylineMapComponent.NexusThreshold)
            {
                nexus++;
            }
        }

        return new LeylineAreaReading(peak, sum, cells, nexus);
    }

    public static LeylineAreaReading ReadRect(Map map, CellRect rect)
    {
        if (map == null)
        {
            return default;
        }

        LeylineMapComponent component = map.GetComponent<LeylineMapComponent>();
        if (component == null)
        {
            return default;
        }

        int peak = 0;
        int sum = 0;
        int cells = 0;
        int nexus = 0;
        foreach (IntVec3 cell in rect.ClipInsideMap(map).Cells)
        {
            cells++;
            int strength = component.StrengthAt(cell);
            if (strength <= 0)
            {
                continue;
            }

            sum += strength;
            peak = Mathf.Max(peak, strength);
            if (strength >= LeylineMapComponent.NexusThreshold)
            {
                nexus++;
            }
        }

        return new LeylineAreaReading(peak, sum, cells, nexus);
    }

    public static LeylineAreaReading ReadThingFootprint(Thing thing)
    {
        return thing?.Spawned == true ? ReadRect(thing.Map, thing.OccupiedRect()) : default;
    }

    public static float PeakStrengthBonus(LeylineAreaReading reading, float bonusPerStrength = 0.06f, float maxBonus = 0.30f)
    {
        return Mathf.Min(Mathf.Max(0f, reading.PeakStrength * bonusPerStrength), Mathf.Max(0f, maxBonus));
    }
}

public sealed class LeylineSegmentRecord : IExposable
{
    public IntVec3 start;
    public IntVec3 end;

    public LeylineSegmentRecord()
    {
    }

    public LeylineSegmentRecord(IntVec3 start, IntVec3 end)
    {
        this.start = start;
        this.end = end;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref start, "start");
        Scribe_Values.Look(ref end, "end");
    }
}

public static class DebugActions_Leylines
{
    [DebugAction("MFVanilla - Leylines", "Toggle Leyline Overlay", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void ToggleLeylineOverlay()
    {
        LeylineMapComponent.ToggleOverlay();
    }

    [DebugAction("MFVanilla - Leylines", "Regenerate Current Map Leylines", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void RegenerateCurrentMapLeylines()
    {
        Find.CurrentMap?.GetComponent<LeylineMapComponent>()?.RegenerateForDebug();
        LogLeylineSummary();
    }

    [DebugAction("MFVanilla - Leylines", "Log Current Map Leyline Summary", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void LogLeylineSummary()
    {
        LeylineMapComponent component = Find.CurrentMap?.GetComponent<LeylineMapComponent>();
        Log.Message(component == null ? "[MFVanilla] No leyline component on current map." : "[MFVanilla] " + component.Summary());
    }
}
