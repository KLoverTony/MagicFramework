using RimWorld;
using UnityEngine;
using Verse;

namespace MFStoryteller
{
	public class Dialog_DebugDivinationEvents : Window
	{
		private Vector2 scrollPosition = Vector2.zero;
		private WorldComponent_DivinationEvents divComponent;

		public Dialog_DebugDivinationEvents()
		{
			this.divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
			this.doCloseButton = true;
			this.doCloseX = true;
			this.closeOnClickedOutside = false;
			this.absorbInputAroundWindow = true;
		}

		public override Vector2 InitialSize => new Vector2(800f, 600f);

		public override void DoWindowContents(Rect inRect)
		{
			if (divComponent == null)
			{
				Widgets.Label(inRect, "WorldComponent_DivinationEvents not found!");
				return;
			}

			Rect outRect = inRect.AtZero();
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, 500f);

			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);

			float y = 0f;

			// Pending Incidents
			Widgets.Label(new Rect(0f, y, viewRect.width, 30f), "PENDING INCIDENTS");
			y += 30f;

			var pending = divComponent.GetPendingIncidents();
			if (pending.Count == 0)
			{
				Widgets.Label(new Rect(0f, y, viewRect.width, 30f), "  (none)");
				y += 30f;
			}
			else
			{
				for (int i = 0; i < pending.Count; i++)
				{
					var incident = pending[i];
					string label = $"{i + 1}. {incident.incidentDef.label} ({incident.threatPoints:F0} pts)";
					if (incident.TargetMap != null)
						label += $" @ {incident.TargetMap.Tile}";

					Widgets.Label(new Rect(20f, y, viewRect.width - 40f, 30f), label);
					y += 30f;
				}
			}

			y += 20f;

			// Recent Incidents
			Widgets.Label(new Rect(0f, y, viewRect.width, 30f), "RECENT INCIDENTS");
			y += 30f;

			var recent = divComponent.GetRecentIncidents();
			if (recent.Count == 0)
			{
				Widgets.Label(new Rect(0f, y, viewRect.width, 30f), "  (none)");
				y += 30f;
			}
			else
			{
				for (int i = 0; i < recent.Count; i++)
				{
					var incident = recent[i];
					int ticksAgo = Find.TickManager.TicksGame - incident.fireTick;
					string timeStr = (ticksAgo / 60000f).ToString("F1");
					string label = $"{i + 1}. {incident.incidentDef.label} ({incident.threatPoints:F0} pts) - {timeStr} days ago";
					if (incident.TargetMap != null)
						label += $" @ {incident.TargetMap.Tile}";

					Widgets.Label(new Rect(20f, y, viewRect.width - 40f, 30f), label);
					y += 30f;
				}
			}

			viewRect.height = y + 20f;
			Widgets.EndScrollView();

			// Stats at bottom
			Rect statsRect = new Rect(inRect.x, inRect.y + inRect.height - 60f, inRect.width, 50f);
			Widgets.Label(statsRect, $"Pending: {pending.Count}  |  Recent: {recent.Count}");
		}
	}
}
