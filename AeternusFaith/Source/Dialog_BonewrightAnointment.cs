using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AeternusFaith
{
    public class Dialog_BonewrightAnointment : Window
    {
        private const float RowHeight = 48f;
        private const float BucketGap = 12f;
        private const float HeaderHeight = 30f;
        private const float Padding = 8f;

        private readonly Thing circle;
        private readonly IEnumerable<Pawn> pawnCandidates;
        private readonly Action<Pawn, Pawn> onAccepted;
        private Vector2 conductorScroll;
        private Vector2 initiateScroll;
        private Pawn conductor;
        private Pawn initiate;

        public Dialog_BonewrightAnointment(Thing circle, IEnumerable<Pawn> pawnCandidates, Action<Pawn, Pawn> onAccepted)
        {
            this.circle = circle;
            this.pawnCandidates = pawnCandidates ?? Enumerable.Empty<Pawn>();
            this.onAccepted = onAccepted;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            doCloseX = true;
        }

        public override Vector2 InitialSize => new Vector2(760f, 560f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), "Bonewright anointment");
            Text.Font = GameFont.Small;

            List<Pawn> pawns = pawnCandidates.Where(pawn => pawn != null && !pawn.Destroyed).OrderBy(pawn => pawn.LabelShortCap).ToList();
            PruneSelections(pawns);

            Rect bucketRect = new Rect(inRect.x, inRect.y + 42f, inRect.width, inRect.height - 94f);
            float bucketWidth = (bucketRect.width - BucketGap) / 2f;
            DrawConductorBucket(new Rect(bucketRect.x, bucketRect.y, bucketWidth, bucketRect.height), pawns);
            DrawInitiateBucket(new Rect(bucketRect.x + bucketWidth + BucketGap, bucketRect.y, bucketWidth, bucketRect.height), pawns);

            AcceptanceReport canStart = CanAccept();
            Rect reasonRect = new Rect(inRect.x, inRect.yMax - 38f, inRect.width - 176f, 38f);
            if (!canStart.Accepted)
            {
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(reasonRect, canStart.Reason);
                GUI.color = Color.white;
            }

            Rect buttonRect = new Rect(inRect.xMax - 160f, inRect.yMax - 38f, 160f, 38f);
            GUI.color = canStart.Accepted ? Color.white : Color.gray;
            if (Widgets.ButtonText(buttonRect, "Begin") && canStart.Accepted)
            {
                onAccepted?.Invoke(conductor, initiate);
                Close();
            }

            GUI.color = Color.white;
        }

        private void PruneSelections(List<Pawn> pawns)
        {
            if (conductor != null && !pawns.Contains(conductor))
                conductor = null;
            if (initiate != null && !pawns.Contains(initiate))
                initiate = null;
            if (conductor == initiate)
                initiate = null;
        }

        private void DrawConductorBucket(Rect rect, List<Pawn> pawns)
        {
            DrawBucketFrame(rect, "Conductor");
            Rect content = InnerBucketRect(rect);
            List<Pawn> conductors = pawns.Where(BonewrightUtility.CanOfficiateAnointment).ToList();
            DrawPawnRows(content, conductors, ref conductorScroll, pawn =>
            {
                AcceptanceReport report = BonewrightUtility.CanOfficiateAnointment(pawn) ? true : "Must be a Soulwarden or Bonewright.";
                DrawSelectablePawnRow(pawn, report, pawn == conductor, "Lead", () => conductor = pawn);
            }, "No Soulwarden or Bonewright is available.");
        }

        private void DrawInitiateBucket(Rect rect, List<Pawn> pawns)
        {
            DrawBucketFrame(rect, "Initiate");
            Rect content = InnerBucketRect(rect);
            DrawPawnRows(content, pawns, ref initiateScroll, pawn =>
            {
                AcceptanceReport report = pawn == conductor
                    ? "The conductor cannot anoint themselves in this rite."
                    : BonewrightUtility.CanBeAnointed(pawn, circle?.Map, out string failReason)
                        ? true
                        : failReason;
                DrawSelectablePawnRow(pawn, report, pawn == initiate, "Choose", () => initiate = pawn);
            }, "No pawn can be anointed.");
        }

        private delegate void DrawPawnRow(Pawn pawn);

        private void DrawPawnRows(Rect content, List<Pawn> pawns, ref Vector2 scrollPosition, DrawPawnRow drawRow, string emptyLabel)
        {
            if (pawns.Count == 0)
            {
                DrawEmptyRow(content, emptyLabel);
                return;
            }

            Rect view = new Rect(0f, 0f, content.width - 16f, Mathf.Max(content.height, pawns.Count * RowHeight));
            Widgets.BeginScrollView(content, ref scrollPosition, view);
            float y = 0f;
            foreach (Pawn pawn in pawns)
            {
                currentRow = new Rect(0f, y, view.width, RowHeight);
                drawRow(pawn);
                y += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private Rect currentRow;

        private void DrawSelectablePawnRow(Pawn pawn, AcceptanceReport report, bool selected, string buttonLabel, Action onSelect)
        {
            Rect row = currentRow;
            if (selected)
                Widgets.DrawHighlightSelected(row);
            else if (Mouse.IsOver(row))
                Widgets.DrawHighlight(row);

            Widgets.ThingIcon(new Rect(row.x + 6f, row.y + 6f, 36f, 36f), pawn);
            Widgets.Label(new Rect(row.x + 48f, row.y + 5f, row.width - 128f, 22f), pawn.LabelShortCap);
            GUI.color = report.Accepted ? ColoredText.SubtleGrayColor : ColorLibrary.RedReadable;
            Widgets.Label(new Rect(row.x + 48f, row.y + 25f, row.width - 128f, 20f), report.Accepted ? PawnSubLabel(pawn) : report.Reason);
            GUI.color = report.Accepted ? Color.white : Color.gray;

            if (Widgets.ButtonText(new Rect(row.xMax - 72f, row.y + 10f, 64f, 28f), selected ? "Set" : buttonLabel) && report.Accepted)
                onSelect?.Invoke();

            GUI.color = Color.white;
        }

        private AcceptanceReport CanAccept()
        {
            if (conductor == null)
                return "Select a conductor.";
            if (!BonewrightUtility.CanOfficiateAnointment(conductor))
                return "The selected conductor must be a Soulwarden or Bonewright.";
            if (initiate == null)
                return "Select an initiate.";
            if (conductor == initiate)
                return "The conductor cannot anoint themselves in this rite.";
            return BonewrightUtility.CanBeAnointed(initiate, circle?.Map, out string failReason) ? true : failReason;
        }

        private static void DrawBucketFrame(Rect rect, string label)
        {
            Widgets.DrawMenuSection(rect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x + Padding, rect.y + 4f, rect.width - (Padding * 2f), 26f), label);
            Text.Font = GameFont.Small;
        }

        private static Rect InnerBucketRect(Rect rect)
        {
            return new Rect(
                rect.x + Padding,
                rect.y + HeaderHeight,
                rect.width - (Padding * 2f),
                rect.height - HeaderHeight - Padding);
        }

        private static void DrawEmptyRow(Rect rect, string label)
        {
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.Label(new Rect(rect.x + 4f, rect.y + 6f, rect.width - 8f, 28f), label);
            GUI.color = Color.white;
        }

        private static string PawnSubLabel(Pawn pawn)
        {
            if (pawn == null)
                return string.Empty;

            Precept_Role role = ModsConfig.IdeologyActive ? pawn.Ideo?.GetRole(pawn) : null;
            if (role != null)
                return role.LabelCap;

            return BonewrightUtility.IsBonewright(pawn) ? "Bonewright" : pawn.KindLabel;
        }
    }
}
