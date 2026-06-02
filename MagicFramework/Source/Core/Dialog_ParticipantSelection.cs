using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework
{
    public class Dialog_ParticipantSelection : Window
    {
        private const float RowHeight = 48f;
        private const float BucketGap = 12f;
        private const float BucketHeaderHeight = 30f;
        private const float BucketPadding = 8f;

        private readonly string title;
        private readonly string acceptLabel;
        private readonly IEnumerable<Pawn> pawnCandidates;
        private readonly IEnumerable<Corpse> corpseCandidates;
        private readonly Func<Corpse, AcceptanceReport> corpseValidator;
        private readonly Func<Pawn, Corpse, AcceptanceReport> conductorValidator;
        private readonly Func<Pawn, AcceptanceReport> audienceValidator;
        private readonly Action<ParticipantSelectionResult> onAccepted;
        private readonly List<Pawn> audience = new List<Pawn>();
        private Vector2 corpseScrollPosition;
        private Vector2 pawnScrollPosition;
        private Pawn conductor;
        private Corpse corpse;

        public Dialog_ParticipantSelection(
            string title,
            string acceptLabel,
            IEnumerable<Pawn> pawnCandidates,
            IEnumerable<Corpse> corpseCandidates,
            Func<Corpse, AcceptanceReport> corpseValidator,
            Func<Pawn, Corpse, AcceptanceReport> conductorValidator,
            Func<Pawn, AcceptanceReport> audienceValidator,
            Action<ParticipantSelectionResult> onAccepted)
        {
            this.title = title ?? "Select participants";
            this.acceptLabel = acceptLabel ?? "Accept";
            this.pawnCandidates = pawnCandidates ?? Enumerable.Empty<Pawn>();
            this.corpseCandidates = corpseCandidates ?? Enumerable.Empty<Corpse>();
            this.corpseValidator = corpseValidator ?? (_ => true);
            this.conductorValidator = conductorValidator ?? ((_, __) => true);
            this.audienceValidator = audienceValidator ?? (_ => true);
            this.onAccepted = onAccepted;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            doCloseX = true;
        }

        public override Vector2 InitialSize => new Vector2(820f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), title);
            Text.Font = GameFont.Small;

            List<Corpse> corpses = corpseCandidates.Where(c => c != null && !c.Destroyed).OrderBy(CorpseLabel).ToList();
            List<Pawn> pawns = pawnCandidates.Where(p => p != null && !p.Destroyed).OrderBy(p => p.LabelShortCap).ToList();
            PruneSelections(corpses, pawns);

            Rect corpseRect = new Rect(inRect.x, inRect.y + 42f, inRect.width, 124f);
            DrawCorpseBucket(corpseRect, corpses);

            Rect pawnRect = new Rect(inRect.x, corpseRect.yMax + 12f, inRect.width, inRect.height - 220f);
            DrawPawnBuckets(pawnRect, pawns);

            AcceptanceReport canStart = CanAccept();
            float buttonWidth = Mathf.Clamp(Text.CalcSize(acceptLabel).x + 36f, 160f, Mathf.Min(300f, inRect.width * 0.45f));
            Rect buttonRect = new Rect(inRect.xMax - buttonWidth, inRect.yMax - 38f, buttonWidth, 38f);
            Rect reasonRect = new Rect(inRect.x, inRect.yMax - 38f, inRect.width - buttonWidth - 16f, 38f);
            if (!canStart.Accepted)
            {
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(reasonRect, canStart.Reason);
                GUI.color = Color.white;
            }

            if (!canStart.Accepted)
                GUI.color = Color.gray;

            if (Widgets.ButtonText(buttonRect, acceptLabel) && canStart.Accepted)
            {
                onAccepted?.Invoke(new ParticipantSelectionResult(conductor, audience.ToList(), corpse));
                Close();
            }

            GUI.color = Color.white;
        }

        private void PruneSelections(List<Corpse> corpses, List<Pawn> pawns)
        {
            if (corpse != null && !corpses.Contains(corpse))
                corpse = null;
            if (conductor != null && !pawns.Contains(conductor))
                conductor = null;
            audience.RemoveAll(p => p == null || !pawns.Contains(p) || p == conductor);
        }

        private void DrawCorpseBucket(Rect rect, List<Corpse> corpses)
        {
            DrawBucketFrame(rect, "Corpse");
            Rect content = InnerBucketRect(rect);
            if (corpses.Count == 0)
            {
                DrawEmptyRow(content, "No valid corpses are available.");
                return;
            }

            Rect view = new Rect(0f, 0f, content.width - 16f, Mathf.Max(content.height, corpses.Count * RowHeight));
            Widgets.BeginScrollView(content, ref corpseScrollPosition, view);
            float y = 0f;
            foreach (Corpse candidate in corpses)
            {
                AcceptanceReport report = corpseValidator(candidate);
                bool selected = corpse == candidate;
                DrawCorpseRow(new Rect(0f, y, view.width, RowHeight), candidate, selected, report);
                y += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private void DrawPawnBuckets(Rect rect, List<Pawn> pawns)
        {
            float bucketWidth = (rect.width - (BucketGap * 2f)) / 3f;
            Rect conductorRect = new Rect(rect.x, rect.y, bucketWidth, rect.height);
            Rect audienceRect = new Rect(conductorRect.xMax + BucketGap, rect.y, bucketWidth, rect.height);
            Rect availableRect = new Rect(audienceRect.xMax + BucketGap, rect.y, bucketWidth, rect.height);

            DrawSelectedConductorBucket(conductorRect);
            DrawAudienceBucket(audienceRect);
            DrawAvailablePawnBucket(availableRect, pawns);
        }

        private void DrawSelectedConductorBucket(Rect rect)
        {
            DrawBucketFrame(rect, "Conductor");
            Rect content = InnerBucketRect(rect);
            if (conductor == null)
            {
                DrawEmptyRow(content, "No conductor selected.");
                return;
            }

            AcceptanceReport report = corpse == null ? "Select a corpse first." : conductorValidator(conductor, corpse);
            DrawAssignedPawnRow(new Rect(content.x, content.y, content.width, RowHeight), conductor, report, () => conductor = null);
        }

        private void DrawAudienceBucket(Rect rect)
        {
            DrawBucketFrame(rect, "Audience");
            Rect content = InnerBucketRect(rect);
            if (audience.Count == 0)
            {
                DrawEmptyRow(content, "No audience selected.");
                return;
            }

            float viewHeight = Mathf.Max(content.height, audience.Count * RowHeight);
            Rect view = new Rect(0f, 0f, content.width - 16f, viewHeight);
            Vector2 localScroll = pawnScrollPosition;
            Widgets.BeginScrollView(content, ref localScroll, view);
            pawnScrollPosition = localScroll;
            float y = 0f;
            foreach (Pawn pawn in audience.ToList())
            {
                AcceptanceReport report = audienceValidator(pawn);
                DrawAssignedPawnRow(new Rect(0f, y, view.width, RowHeight), pawn, report, () => audience.Remove(pawn));
                y += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private void DrawAvailablePawnBucket(Rect rect, List<Pawn> pawns)
        {
            DrawBucketFrame(rect, "Available");
            Rect content = InnerBucketRect(rect);
            List<Pawn> available = pawns.Where(p => p != conductor && !audience.Contains(p)).ToList();
            if (available.Count == 0)
            {
                DrawEmptyRow(content, "No available pawns.");
                return;
            }

            float viewHeight = Mathf.Max(content.height, available.Count * RowHeight);
            Rect view = new Rect(0f, 0f, content.width - 16f, viewHeight);
            Vector2 localScroll = pawnScrollPosition;
            Widgets.BeginScrollView(content, ref localScroll, view);
            pawnScrollPosition = localScroll;
            float y = 0f;
            foreach (Pawn pawn in available)
            {
                AcceptanceReport conductorReport = corpse == null ? "Select a corpse first." : conductorValidator(pawn, corpse);
                AcceptanceReport audienceReport = audienceValidator(pawn);
                DrawAvailablePawnRow(new Rect(0f, y, view.width, RowHeight), pawn, conductorReport, audienceReport);
                y += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private void DrawCorpseRow(Rect row, Corpse candidate, bool selected, AcceptanceReport report)
        {
            DrawRowBackground(row, selected);
            DrawThingIcon(new Rect(row.x + 6f, row.y + 6f, 36f, 36f), candidate);
            DrawRowText(new Rect(row.x + 48f, row.y, row.width - 132f, row.height), CorpseLabel(candidate), CorpseSubLabel(candidate), report);

            Rect button = new Rect(row.xMax - 76f, row.y + 10f, 68f, 28f);
            GUI.color = report.Accepted || selected ? Color.white : Color.gray;
            if (Widgets.ButtonText(button, selected ? "Set" : "Use") && (report.Accepted || selected))
                corpse = candidate;
            GUI.color = Color.white;
        }

        private void DrawAssignedPawnRow(Rect row, Pawn pawn, AcceptanceReport report, Action onRemove)
        {
            DrawRowBackground(row, selected: true);
            DrawThingIcon(new Rect(row.x + 6f, row.y + 6f, 36f, 36f), pawn);
            DrawRowText(new Rect(row.x + 48f, row.y, row.width - 92f, row.height), pawn.LabelShortCap, PawnSubLabel(pawn), report);
            if (Widgets.ButtonText(new Rect(row.xMax - 36f, row.y + 10f, 28f, 28f), "x"))
                onRemove?.Invoke();
        }

        private void DrawAvailablePawnRow(Rect row, Pawn pawn, AcceptanceReport conductorReport, AcceptanceReport audienceReport)
        {
            bool canDoAnything = conductorReport.Accepted || audienceReport.Accepted;
            DrawRowBackground(row, selected: false);
            GUI.color = canDoAnything ? Color.white : Color.gray;
            DrawThingIcon(new Rect(row.x + 6f, row.y + 6f, 36f, 36f), pawn);
            AcceptanceReport displayReport = conductorReport.Accepted || audienceReport.Accepted ? (AcceptanceReport)true : conductorReport;
            DrawRowText(new Rect(row.x + 48f, row.y, row.width - 116f, row.height), pawn.LabelShortCap, PawnSubLabel(pawn), displayReport);

            Rect leadButton = new Rect(row.xMax - 64f, row.y + 5f, 56f, 19f);
            Rect audienceButton = new Rect(row.xMax - 64f, row.y + 25f, 56f, 19f);
            GUI.color = conductorReport.Accepted ? Color.white : Color.gray;
            if (Widgets.ButtonText(leadButton, "Lead") && conductorReport.Accepted)
            {
                if (conductor != null && !audience.Contains(conductor) && audienceValidator(conductor).Accepted)
                    audience.Add(conductor);
                conductor = pawn;
                audience.Remove(pawn);
            }

            GUI.color = audienceReport.Accepted ? Color.white : Color.gray;
            if (Widgets.ButtonText(audienceButton, "Join") && audienceReport.Accepted)
                audience.Add(pawn);
            GUI.color = Color.white;
        }

        private AcceptanceReport CanAccept()
        {
            if (corpse == null)
                return "Select a corpse.";
            AcceptanceReport corpseReport = corpseValidator(corpse);
            if (!corpseReport.Accepted)
                return corpseReport;
            if (conductor == null)
                return "Select a conductor.";
            return conductorValidator(conductor, corpse);
        }

        private static void DrawBucketFrame(Rect rect, string label)
        {
            Widgets.DrawMenuSection(rect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x + BucketPadding, rect.y + 4f, rect.width - (BucketPadding * 2f), 26f), label);
            Text.Font = GameFont.Small;
        }

        private static Rect InnerBucketRect(Rect rect)
        {
            return new Rect(
                rect.x + BucketPadding,
                rect.y + BucketHeaderHeight,
                rect.width - (BucketPadding * 2f),
                rect.height - BucketHeaderHeight - BucketPadding);
        }

        private static void DrawEmptyRow(Rect rect, string label)
        {
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.Label(new Rect(rect.x + 4f, rect.y + 6f, rect.width - 8f, 28f), label);
            GUI.color = Color.white;
        }

        private static void DrawRowBackground(Rect row, bool selected)
        {
            if (selected)
                Widgets.DrawHighlightSelected(row);
            else if (Mouse.IsOver(row))
                Widgets.DrawHighlight(row);
        }

        private static void DrawThingIcon(Rect rect, Thing thing)
        {
            if (thing != null)
                Widgets.ThingIcon(rect, thing);
        }

        private static void DrawRowText(Rect row, string label, string subLabel, AcceptanceReport report)
        {
            Color originalColor = GUI.color;
            Widgets.Label(new Rect(row.x, row.y + 5f, row.width, 22f), label);
            string detail = report.Accepted ? subLabel : report.Reason;
            GUI.color = report.Accepted ? ColoredText.SubtleGrayColor : ColorLibrary.RedReadable;
            Widgets.Label(new Rect(row.x, row.y + 25f, row.width, 20f), detail);
            GUI.color = originalColor;
        }

        private static string CorpseLabel(Corpse candidate)
        {
            Pawn pawn = candidate?.InnerPawn;
            return pawn?.LabelShortCap ?? candidate?.LabelShortCap ?? "Corpse";
        }

        private static string CorpseSubLabel(Corpse candidate)
        {
            Pawn pawn = candidate?.InnerPawn;
            return pawn?.KindLabel ?? candidate?.LabelShortCap ?? string.Empty;
        }

        private static string PawnSubLabel(Pawn pawn)
        {
            if (pawn == null)
                return string.Empty;
            string role = pawn.story?.TitleCap;
            return string.IsNullOrEmpty(role) ? pawn.KindLabel : role;
        }
    }

    public sealed class ParticipantSelectionResult
    {
        public readonly Pawn conductor;
        public readonly List<Pawn> audience;
        public readonly Corpse corpse;

        public ParticipantSelectionResult(Pawn conductor, List<Pawn> audience, Corpse corpse)
        {
            this.conductor = conductor;
            this.audience = audience ?? new List<Pawn>();
            this.corpse = corpse;
        }
    }
}
