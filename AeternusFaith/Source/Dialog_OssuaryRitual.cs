using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AeternusFaith
{
    public class Dialog_OssuaryRitual : Window
    {
        private readonly Thing lectern;
        private readonly Thing circle;
        private readonly Thing ossuary;
        private readonly Action<Pawn, List<Pawn>, Corpse, Thing, Thing> startRitual;
        private readonly Func<Corpse, bool> corpseValidator;
        private readonly Func<Pawn, Corpse, Thing, bool> conductorValidator;
        private readonly Func<Pawn, bool> audienceValidator;
        private readonly List<Pawn> audience = new List<Pawn>();
        private Vector2 scrollPosition;
        private Pawn conductor;
        private Corpse corpse;

        public Dialog_OssuaryRitual(
            Thing lectern,
            Thing circle,
            Thing ossuary,
            Action<Pawn, List<Pawn>, Corpse, Thing, Thing> startRitual,
            Func<Corpse, bool> corpseValidator,
            Func<Pawn, Corpse, Thing, bool> conductorValidator,
            Func<Pawn, bool> audienceValidator)
        {
            this.lectern = lectern;
            this.circle = circle;
            this.ossuary = ossuary;
            this.startRitual = startRitual;
            this.corpseValidator = corpseValidator;
            this.conductorValidator = conductorValidator;
            this.audienceValidator = audienceValidator;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            doCloseX = true;
        }

        public override Vector2 InitialSize => new Vector2(720f, 620f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), "Ossuary rite");
            Text.Font = GameFont.Small;

            Rect contentRect = new Rect(inRect.x, inRect.y + 42f, inRect.width, inRect.height - 92f);
            float viewHeight = 120f + EligiblePawns.Count() * 34f + ValidCorpses.Count() * 34f;
            Rect viewRect = new Rect(0f, 0f, contentRect.width - 16f, Mathf.Max(contentRect.height, viewHeight));

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);
            float y = 0f;
            DrawCorpseSelector(viewRect, ref y);
            y += 16f;
            DrawConductorSelector(viewRect, ref y);
            y += 16f;
            DrawAudienceSelector(viewRect, ref y);
            Widgets.EndScrollView();

            Rect buttonRect = new Rect(inRect.xMax - 160f, inRect.yMax - 38f, 160f, 38f);
            bool canStart = corpse != null && conductor != null && conductorValidator(conductor, corpse, ossuary);
            if (!canStart)
                GUI.color = Color.gray;

            if (Widgets.ButtonText(buttonRect, "Begin rite") && canStart)
            {
                startRitual(conductor, audience.ToList(), corpse, circle, ossuary);
                Close();
            }

            GUI.color = Color.white;
        }

        private IEnumerable<Pawn> EligiblePawns => lectern.Map.mapPawns.FreeColonistsSpawned.Where(audienceValidator).OrderBy(pawn => pawn.LabelShortCap);

        private IEnumerable<Corpse> ValidCorpses => lectern.Map.listerThings.AllThings.OfType<Corpse>().Where(corpseValidator).OrderBy(c => c.LabelShortCap);

        private void DrawCorpseSelector(Rect viewRect, ref float y)
        {
            Widgets.Label(new Rect(0f, y, viewRect.width, 28f), "Corpse");
            y += 30f;

            foreach (Corpse candidate in ValidCorpses)
            {
                bool selected = corpse == candidate;
                if (Widgets.RadioButtonLabeled(new Rect(12f, y, viewRect.width - 24f, 28f), CorpseLabel(candidate), selected))
                    corpse = candidate;
                y += 30f;
            }
        }

        private void DrawConductorSelector(Rect viewRect, ref float y)
        {
            Widgets.Label(new Rect(0f, y, viewRect.width, 28f), "Conductor");
            y += 30f;

            foreach (Pawn candidate in EligiblePawns)
            {
                bool selected = conductor == candidate;
                bool valid = corpse != null && conductorValidator(candidate, corpse, ossuary);
                GUI.color = valid || selected ? Color.white : Color.gray;
                if (Widgets.RadioButtonLabeled(new Rect(12f, y, viewRect.width - 24f, 28f), candidate.LabelShortCap, selected))
                {
                    conductor = candidate;
                    audience.Remove(candidate);
                }
                GUI.color = Color.white;
                y += 30f;
            }
        }

        private void DrawAudienceSelector(Rect viewRect, ref float y)
        {
            Widgets.Label(new Rect(0f, y, viewRect.width, 28f), "Audience");
            y += 30f;

            foreach (Pawn candidate in EligiblePawns)
            {
                if (candidate == conductor)
                    continue;

                bool selected = audience.Contains(candidate);
                bool wasSelected = selected;
                Widgets.CheckboxLabeled(new Rect(12f, y, viewRect.width - 24f, 28f), candidate.LabelShortCap, ref selected);
                if (selected != wasSelected)
                {
                    if (selected)
                        audience.Add(candidate);
                    else
                        audience.Remove(candidate);
                }
                y += 30f;
            }
        }

        private string CorpseLabel(Corpse candidate)
        {
            Pawn pawn = candidate.InnerPawn;
            if (pawn == null)
                return candidate.LabelShortCap;

            return pawn.LabelShortCap + " (" + pawn.KindLabel + ")";
        }
    }
}
