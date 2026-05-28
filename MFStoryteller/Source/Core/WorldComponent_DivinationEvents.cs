using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace MFStoryteller
{
	public class WorldComponent_DivinationEvents : WorldComponent
	{
		private List<PendingDivinationEvent> pendingIncidents = new();
		private List<PendingDivinationEvent> recentIncidents = new();
		private const int HistorySize = 5;
		private const int PendingHistorySize = 20;

		public WorldComponent_DivinationEvents(World world) : base(world)
		{
		}

		public void RegisterSelectedIncident(IncidentDef incidentDef, Map targetMap, float threatPoints)
		{
			if (incidentDef == null || targetMap == null)
				return;

			PendingDivinationEvent pending = new PendingDivinationEvent(
				incidentDef,
				Find.TickManager.TicksGame,
				threatPoints,
				targetMap,
				Find.TickManager.TicksGame
			);

			pendingIncidents.Add(pending);
			if (pendingIncidents.Count > PendingHistorySize)
				pendingIncidents.RemoveAt(0);
		}

		public void CompleteSelectedIncident(IncidentDef incidentDef, Map targetMap, float threatPoints, bool succeeded)
		{
			if (incidentDef == null || targetMap == null)
				return;

			int targetTile = targetMap.Tile;
			int index = pendingIncidents.FindLastIndex(e =>
				e.incidentDef == incidentDef &&
				e.targetTile == targetTile &&
				System.Math.Abs(e.threatPoints - threatPoints) < 0.01f);

			if (index < 0)
				return;

			PendingDivinationEvent pending = pendingIncidents[index];
			pendingIncidents.RemoveAt(index);

			if (!succeeded)
				return;

			recentIncidents.Add(pending);
			if (recentIncidents.Count > HistorySize)
				recentIncidents.RemoveAt(0);
		}

		public bool HasPendingIncident()
		{
			return pendingIncidents.Count > 0;
		}

		public bool HasRecentIncident()
		{
			return recentIncidents.Count > 0;
		}

		public PendingDivinationEvent GetMostRecentIncident()
		{
			if (recentIncidents.Count == 0)
				return null;
			return recentIncidents[recentIncidents.Count - 1];
		}

		public List<PendingDivinationEvent> GetPendingIncidents()
		{
			return new List<PendingDivinationEvent>(pendingIncidents);
		}

		public List<PendingDivinationEvent> GetRecentIncidents()
		{
			return new List<PendingDivinationEvent>(recentIncidents);
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref pendingIncidents, "pendingIncidents", LookMode.Deep);
			Scribe_Collections.Look(ref recentIncidents, "recentIncidents", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pendingIncidents ??= new List<PendingDivinationEvent>();
				recentIncidents ??= new List<PendingDivinationEvent>();
			}
		}
	}
}
