using System.Collections.Generic;
using System.Linq;
using MagicFramework.PawnMemory;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Debug;

public class Dialog_PawnMemoryViewer : Window
{
    private Vector2 scrollPosition;
    private PawnMemoryState? filterState = null;
    private string searchString = "";
    private float viewHeight;

    public override Vector2 InitialSize => new Vector2(900f, 700f);

    public Dialog_PawnMemoryViewer()
    {
        doCloseX = true;
        doCloseButton = true;
        closeOnClickedOutside = true;
        absorbInputAroundWindow = false;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "Pawn Memory Viewer");
        Text.Font = GameFont.Small;

        // Filters and Search
        Rect filterRect = new Rect(0f, 40f, inRect.width, 30f);
        searchString = Widgets.TextField(new Rect(filterRect.x, filterRect.y, 200f, 24f), searchString);
        
        Rect stateFilterRect = new Rect(filterRect.x + 220f, filterRect.y, 200f, 24f);
        if (Widgets.ButtonText(stateFilterRect, filterState.HasValue ? filterState.Value.ToString() : "All States"))
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption("All States", () => filterState = null)
            };
            foreach (PawnMemoryState state in System.Enum.GetValues(typeof(PawnMemoryState)))
            {
                PawnMemoryState localState = state;
                options.Add(new FloatMenuOption(localState.ToString(), () => filterState = localState));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        // List
        Rect listRect = new Rect(0f, 80f, inRect.width, inRect.height - 140f);
        
        var registry = WorldComponent_PawnMemories.Instance;
        if (registry == null)
        {
            Widgets.Label(listRect, "WorldComponent_PawnMemories not found.");
            return;
        }

        var records = registry.GetAllRecords().Where(r => 
            (!filterState.HasValue || r.state == filterState.Value) &&
            (string.IsNullOrEmpty(searchString) || 
             (r.name != null && r.name.ToStringFull.ToLower().Contains(searchString.ToLower())) || 
             r.uniquePawnId.ToLower().Contains(searchString.ToLower()))
        ).ToList();

        Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, viewHeight);
        Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
        
        float curY = 0f;
        foreach (var record in records)
        {
            DrawRecordRow(new Rect(0f, curY, viewRect.width, 90f), record, registry);
            curY += 95f;
        }
        viewHeight = curY;
        
        Widgets.EndScrollView();
    }

    private void DrawRecordRow(Rect rect, PawnMemoryRecord record, WorldComponent_PawnMemories registry)
    {
        Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.2f, 0.2f, 0.5f));
        
        Rect innerRect = rect.ContractedBy(4f);
        
        Text.Font = GameFont.Small;
        string title = $"{record.name?.ToStringFull ?? "Unknown"} ({record.uniquePawnId}) - State: {record.state}";
        Widgets.Label(new Rect(innerRect.x, innerRect.y, innerRect.width, 24f), title);
        
        string details = $"Map: {record.lastKnownMapName ?? "None"} | Death Map: {record.deathMapId?.ToString() ?? "None"} | Traits: {record.traits.Count} | Skills: {record.skills.Count}";
        Widgets.Label(new Rect(innerRect.x, innerRect.y + 24f, innerRect.width, 24f), details);

        // Buttons
        float btnX = innerRect.x;
        float btnY = innerRect.y + 50f;
        
        if (Widgets.ButtonText(new Rect(btnX, btnY, 100f, 24f), "Mark Released"))
        {
            record.state = PawnMemoryState.Released;
            record.rituallyReleased = true;
        }
        btnX += 110f;
        
        if (Widgets.ButtonText(new Rect(btnX, btnY, 120f, 24f), "Mark Proper Rites"))
        {
            record.properRitesPerformed = true;
        }
        btnX += 130f;

        if (Widgets.ButtonText(new Rect(btnX, btnY, 100f, 24f), "Mark Corrupted"))
        {
            record.state = PawnMemoryState.Corrupted;
            record.corrupted = true;
        }
        btnX += 110f;

        if (Widgets.ButtonText(new Rect(btnX, btnY, 100f, 24f), "Delete"))
        {
            record.state = PawnMemoryState.Invalidated;
            record.invalidationReason = "Deleted via debug";
        }
        btnX += 110f;

        Pawn livePawn = Find.Maps.SelectMany(m => m.mapPawns.AllPawns).FirstOrDefault(p => p.ThingID == record.uniquePawnId);
        if (livePawn != null && !livePawn.Dead)
        {
            if (Widgets.ButtonText(new Rect(btnX, btnY, 100f, 24f), "Update Live"))
            {
                registry.UpdateMemory(livePawn, PawnMemoryUpdateReason.ManualDebug);
            }
        }
    }
}
