using System.Collections.Generic;
using System.Linq;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class Dialog_ArcaneDisciplineRitual : Window
{
    private const float RowHeight = 44f;
    private readonly CompArcaneDisciplineRitualMarker marker;
    private Vector2 pawnScroll;
    private Vector2 disciplineScroll;
    private Pawn selectedPawn;
    private ArcaneDisciplineDef selectedDiscipline;

    public Dialog_ArcaneDisciplineRitual(CompArcaneDisciplineRitualMarker marker)
    {
        this.marker = marker;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        doCloseX = true;
    }

    public override Vector2 InitialSize => new(720f, 560f);

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), "Embrace Arcane Discipline");
        Text.Font = GameFont.Small;

        Rect pawnRect = new(inRect.x, inRect.y + 44f, (inRect.width - 12f) / 2f, inRect.height - 100f);
        Rect disciplineRect = new(pawnRect.xMax + 12f, pawnRect.y, pawnRect.width, pawnRect.height);
        DrawPawnList(pawnRect);
        DrawDisciplineList(disciplineRect);

        AcceptanceReport canStart = CanStart();
        Rect reasonRect = new(inRect.x, inRect.yMax - 38f, inRect.width - 176f, 38f);
        if (!canStart.Accepted)
        {
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.Label(reasonRect, canStart.Reason);
            GUI.color = Color.white;
        }

        Rect buttonRect = new(inRect.xMax - 160f, inRect.yMax - 38f, 160f, 38f);
        GUI.color = canStart.Accepted ? Color.white : Color.gray;
        if (Widgets.ButtonText(buttonRect, "Begin rite") && canStart.Accepted)
        {
            marker.TryStartRitual(selectedPawn, selectedDiscipline);
            Close();
        }

        GUI.color = Color.white;
    }

    private void DrawPawnList(Rect rect)
    {
        Widgets.DrawMenuSection(rect);
        Widgets.Label(new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 26f), "Pawn");
        Rect content = new(rect.x + 8f, rect.y + 34f, rect.width - 16f, rect.height - 42f);
        List<Pawn> pawns = marker.PawnCandidates().Where(pawn => pawn != null && !pawn.Destroyed).OrderBy(pawn => pawn.LabelShortCap).ToList();
        if (pawns.Count == 0)
        {
            DrawEmpty(content, "No colonists available.");
            return;
        }

        Rect view = new(0f, 0f, content.width - 16f, Mathf.Max(content.height, pawns.Count * RowHeight));
        Widgets.BeginScrollView(content, ref pawnScroll, view);
        float y = 0f;
        foreach (Pawn pawn in pawns)
        {
            AcceptanceReport report = marker.CanPawnUseMarker(pawn);
            DrawPawnRow(new Rect(0f, y, view.width, RowHeight), pawn, report);
            y += RowHeight;
        }

        Widgets.EndScrollView();
    }

    private void DrawDisciplineList(Rect rect)
    {
        Widgets.DrawMenuSection(rect);
        Widgets.Label(new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 26f), "Discipline");
        Rect content = new(rect.x + 8f, rect.y + 34f, rect.width - 16f, rect.height - 42f);
        List<ArcaneDisciplineDef> disciplines = marker.DisciplineCandidates().ToList();
        if (disciplines.Count == 0)
        {
            DrawEmpty(content, "No disciplines are defined.");
            return;
        }

        Rect view = new(0f, 0f, content.width - 16f, Mathf.Max(content.height, disciplines.Count * RowHeight));
        Widgets.BeginScrollView(content, ref disciplineScroll, view);
        float y = 0f;
        foreach (ArcaneDisciplineDef discipline in disciplines)
        {
            AcceptanceReport report = selectedPawn == null
                ? "Select a pawn first."
                : DisciplineReport(selectedPawn, discipline);
            DrawDisciplineRow(new Rect(0f, y, view.width, RowHeight), discipline, report);
            y += RowHeight;
        }

        Widgets.EndScrollView();
    }

    private void DrawPawnRow(Rect row, Pawn pawn, AcceptanceReport report)
    {
        DrawRowBackground(row, selectedPawn == pawn);
        Widgets.ThingIcon(new Rect(row.x + 4f, row.y + 4f, 36f, 36f), pawn);
        string current = SpellRuntimeGameComponent.Instance?.GetArcaneDiscipline(pawn)?.LabelCap ?? "No discipline";
        DrawRowText(new Rect(row.x + 46f, row.y, row.width - 112f, row.height), pawn.LabelShortCap, current, report);
        DrawUseButton(new Rect(row.xMax - 58f, row.y + 8f, 54f, 28f), report, () => selectedPawn = pawn);
    }

    private void DrawDisciplineRow(Rect row, ArcaneDisciplineDef discipline, AcceptanceReport report)
    {
        DrawRowBackground(row, selectedDiscipline == discipline);
        ResearchProjectDef research = ArcaneDisciplineUtility.GetUnlockResearch(discipline);
        string detail = research == null ? "No research link" : research.LabelCap;
        DrawRowText(new Rect(row.x + 4f, row.y, row.width - 70f, row.height), discipline.LabelCap, detail, report);
        DrawUseButton(new Rect(row.xMax - 58f, row.y + 8f, 54f, 28f), report, () => selectedDiscipline = discipline);
    }

    private AcceptanceReport CanStart()
    {
        if (selectedPawn == null)
        {
            return "Select an Arcane Gift pawn.";
        }

        AcceptanceReport pawnReport = marker.CanPawnUseMarker(selectedPawn);
        if (!pawnReport.Accepted)
        {
            return pawnReport;
        }

        if (selectedDiscipline == null)
        {
            return "Select an Arcane Discipline.";
        }

        return DisciplineReport(selectedPawn, selectedDiscipline);
    }

    private static AcceptanceReport DisciplineReport(Pawn pawn, ArcaneDisciplineDef discipline)
    {
        return ArcaneDisciplineUtility.CanPawnEmbraceDiscipline(pawn, discipline, out string reason)
            ? true
            : reason;
    }

    private static void DrawUseButton(Rect rect, AcceptanceReport report, System.Action action)
    {
        GUI.color = report.Accepted ? Color.white : Color.gray;
        if (Widgets.ButtonText(rect, "Use") && report.Accepted)
        {
            action?.Invoke();
        }

        GUI.color = Color.white;
    }

    private static void DrawRowBackground(Rect row, bool selected)
    {
        if (selected)
        {
            Widgets.DrawHighlightSelected(row);
        }
        else if (Mouse.IsOver(row))
        {
            Widgets.DrawHighlight(row);
        }
    }

    private static void DrawRowText(Rect row, string label, string subLabel, AcceptanceReport report)
    {
        Color originalColor = GUI.color;
        Widgets.Label(new Rect(row.x, row.y + 4f, row.width, 21f), label);
        GUI.color = report.Accepted ? ColoredText.SubtleGrayColor : ColorLibrary.RedReadable;
        Widgets.Label(new Rect(row.x, row.y + 24f, row.width, 18f), report.Accepted ? subLabel : report.Reason);
        GUI.color = originalColor;
    }

    private static void DrawEmpty(Rect rect, string text)
    {
        GUI.color = ColoredText.SubtleGrayColor;
        Widgets.Label(new Rect(rect.x + 4f, rect.y + 6f, rect.width - 8f, 28f), text);
        GUI.color = Color.white;
    }
}
