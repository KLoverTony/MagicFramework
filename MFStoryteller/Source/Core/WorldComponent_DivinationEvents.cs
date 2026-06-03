using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace MFStoryteller
{
	public class WorldComponent_DivinationEvents : WorldComponent
	{
		private List<PendingDivinationEvent> pendingIncidents = new();
		private List<PendingDivinationEvent> recentIncidents = new();
		private const int HistorySize = 5;
		private const int PendingHistorySize = 20;
		private const int CheckIntervalTicks = 250;
		private const int MinForecastDelayTicks = GenDate.TicksPerDay * 2;
		private const int MaxForecastDelayTicks = GenDate.TicksPerDay * 3;
		private const int ForecastRetryDurationTicks = GenDate.TicksPerDay;
		private const int StalePendingGraceTicks = GenDate.TicksPerDay + GenDate.TicksPerHour;
		private static readonly FieldInfo QueuedIncidentsField = typeof(IncidentQueue).GetField("queuedIncidents", BindingFlags.NonPublic | BindingFlags.Instance);

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

		public void RegisterForecastIncident(FiringIncident firingIncident)
		{
			if (firingIncident?.def == null || firingIncident.parms?.target is not Map)
				return;

			if (pendingIncidents.Count > 0)
				return;

			int currentTick = Find.TickManager?.TicksGame ?? 0;
			int fireTick = currentTick + Rand.RangeInclusive(MinForecastDelayTicks, MaxForecastDelayTicks);
			PendingDivinationEvent pending = new PendingDivinationEvent(firingIncident, fireTick, currentTick);
			pending.queuedWithStoryteller = EnsureQueuedWithStoryteller(pending, currentTick);

			pendingIncidents.Add(pending);
			if (pendingIncidents.Count > PendingHistorySize)
				pendingIncidents.RemoveAt(0);
		}

		public bool HasMatchingPendingIncident(IncidentDef incidentDef, Map targetMap, float threatPoints)
		{
			return FindMatchingPendingIncidentIndex(incidentDef, targetMap) >= 0;
		}

		public void CompleteSelectedIncident(IncidentDef incidentDef, Map targetMap, float threatPoints, bool succeeded)
		{
			if (incidentDef == null || targetMap == null)
				return;

			int index = FindMatchingPendingIncidentIndex(incidentDef, targetMap);

			if (index < 0)
				return;

			PendingDivinationEvent pending = pendingIncidents[index];
			pendingIncidents.RemoveAt(index);
			PurgeMatchingQueuedIncidents(pending);

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

		public PendingDivinationEvent GetNextPendingIncident()
		{
			if (pendingIncidents.Count == 0)
				return null;

			return pendingIncidents.OrderBy(e => e.fireTick).FirstOrDefault();
		}

		public bool TryRevealNextPendingIncident()
		{
			PendingDivinationEvent pending = GetNextPendingIncident();
			if (pending == null)
				return false;

			Find.LetterStack.ReceiveLetter("Stored divination event", DescribePendingIncident(pending), LetterDefOf.NeutralEvent);
			return true;
		}

		public bool TryForceNextPendingIncident()
		{
			if (pendingIncidents.Count == 0)
				return false;

			PendingDivinationEvent pending = pendingIncidents.OrderBy(e => e.fireTick).FirstOrDefault();
			if (pending == null)
				return false;

			pendingIncidents.Remove(pending);
			FireIncidentNow(pending);
			return true;
		}

		public override void WorldComponentTick()
		{
			base.WorldComponentTick();
			if (Find.TickManager == null || Find.TickManager.TicksGame % CheckIntervalTicks != 0)
				return;

			int currentTick = Find.TickManager.TicksGame;
			for (int i = pendingIncidents.Count - 1; i >= 0; i--)
			{
				PendingDivinationEvent pending = pendingIncidents[i];
				if (pending == null)
					continue;

				if (!TryEnsureFiringIncident(pending))
				{
					Log.Warning($"[MFStoryteller] Removing stored divination event '{pending.incidentDef?.defName ?? "<null>"}' because its firing incident could not be reconstructed.");
					pendingIncidents.RemoveAt(i);
					continue;
				}

				pending.queuedWithStoryteller = EnsureQueuedWithStoryteller(pending, currentTick);
				if (!pending.queuedWithStoryteller)
					continue;

				if (currentTick > pending.fireTick + StalePendingGraceTicks && !IsInStorytellerQueue(pending))
				{
					Log.Warning($"[MFStoryteller] Removing stale stored divination event '{pending.incidentDef?.defName ?? "<null>"}' after its storyteller queue window elapsed.");
					pendingIncidents.RemoveAt(i);
				}
			}
		}

		private bool EnsureQueuedWithStoryteller(PendingDivinationEvent pending, int currentTick)
		{
			if (!TryEnsureFiringIncident(pending) || Find.Storyteller?.incidentQueue == null)
				return false;

			int matchingQueued = CountMatchingQueuedIncidents(pending);
			if (matchingQueued == 1)
				return true;

			return QueueWithStoryteller(pending, currentTick);
		}

		private bool QueueWithStoryteller(PendingDivinationEvent pending, int currentTick)
		{
			if (!TryEnsureFiringIncident(pending) || Find.Storyteller?.incidentQueue == null)
				return false;

			int fireTick = Math.Max(pending.fireTick, currentTick + CheckIntervalTicks);
			if (currentTick > pending.fireTick + StalePendingGraceTicks)
				fireTick = currentTick + CheckIntervalTicks;

			pending.fireTick = fireTick;
			PurgeMatchingQueuedIncidents(pending);
			return Find.Storyteller.incidentQueue.Add(pending.firingIncident.def, fireTick, pending.firingIncident.parms, ForecastRetryDurationTicks);
		}

		private bool TryEnsureFiringIncident(PendingDivinationEvent pending)
		{
			if (pending?.firingIncident?.def != null && pending.firingIncident.parms?.target is Map)
				return true;

			if (pending?.incidentDef == null || pending.TargetMap == null)
				return false;

			IncidentParms parms = new IncidentParms
			{
				target = pending.TargetMap,
				points = pending.threatPoints
			};
			pending.firingIncident = new FiringIncident(pending.incidentDef, null, parms);
			return true;
		}

		private bool IsInStorytellerQueue(PendingDivinationEvent pending)
		{
			if (!TryEnsureFiringIncident(pending) || Find.Storyteller?.incidentQueue == null)
				return false;

			foreach (object queuedObject in Find.Storyteller.incidentQueue)
			{
				if (queuedObject is not QueuedIncident queuedIncident)
					continue;

				FiringIncident queuedFiringIncident = queuedIncident.FiringIncident;
				if (queuedFiringIncident?.def != pending.incidentDef)
					continue;

				if (queuedFiringIncident.parms?.target is not Map queuedMap || pending.firingIncident.parms?.target is not Map pendingMap)
					continue;

				if (queuedMap.Tile != pendingMap.Tile)
					continue;

				if (queuedIncident.FireTick < Find.TickManager.TicksGame)
					continue;

				return true;
			}

			return false;
		}

		private int CountMatchingQueuedIncidents(PendingDivinationEvent pending)
		{
			if (!TryEnsureFiringIncident(pending) || Find.Storyteller?.incidentQueue == null)
				return 0;

			int count = 0;
			foreach (object queuedObject in Find.Storyteller.incidentQueue)
			{
				if (queuedObject is QueuedIncident queuedIncident && IsMatchingQueuedIncident(queuedIncident, pending))
					count++;
			}

			return count;
		}

		private int PurgeMatchingQueuedIncidents(PendingDivinationEvent pending)
		{
			if (!TryEnsureFiringIncident(pending) || Find.Storyteller?.incidentQueue == null || QueuedIncidentsField == null)
				return 0;

			if (QueuedIncidentsField.GetValue(Find.Storyteller.incidentQueue) is not List<QueuedIncident> queuedIncidents)
				return 0;

			int removed = queuedIncidents.RemoveAll(queuedIncident => IsMatchingQueuedIncident(queuedIncident, pending));
			if (removed > 0)
				Log.Message($"[MFStoryteller] Removed {removed} queued duplicate(s) for divination event '{pending.incidentDef?.defName ?? "<null>"}'.");

			return removed;
		}

		private bool IsMatchingQueuedIncident(QueuedIncident queuedIncident, PendingDivinationEvent pending)
		{
			if (queuedIncident == null || !TryEnsureFiringIncident(pending))
				return false;

			FiringIncident queuedFiringIncident = queuedIncident.FiringIncident;
			if (queuedFiringIncident?.def != pending.incidentDef)
				return false;

			if (queuedFiringIncident.parms?.target is not Map queuedMap || pending.firingIncident.parms?.target is not Map pendingMap)
				return false;

			if (queuedMap.Tile != pendingMap.Tile)
				return false;

			return true;
		}

		private void FireIncidentNow(PendingDivinationEvent pending)
		{
			if (pending?.firingIncident?.def == null || pending.firingIncident.parms?.target is not Map)
				return;

			PurgeMatchingQueuedIncidents(pending);
			bool succeeded = false;
			try
			{
				succeeded = Find.Storyteller?.TryFire(pending.firingIncident, false) == true;
			}
			catch (Exception ex)
			{
				Log.Warning($"[MFStoryteller] Forced divination incident '{pending.incidentDef?.defName ?? "<null>"}' failed: {ex}");
			}

			if (!succeeded)
				return;

			pending.fireTick = Find.TickManager?.TicksGame ?? pending.fireTick;
			recentIncidents.Add(pending);
			if (recentIncidents.Count > HistorySize)
				recentIncidents.RemoveAt(0);
		}

		private static string DescribePendingIncident(PendingDivinationEvent pending)
		{
			IncidentDef incidentDef = pending.incidentDef;
			string incidentLabel = incidentDef?.LabelCap ?? "Unknown incident";
			string incidentDefName = incidentDef?.defName ?? "unknown";
			string categoryLabel = incidentDef?.category?.LabelCap ?? "unknown category";
			string targetLabel = pending.TargetMap?.Parent?.LabelCap ?? pending.TargetMap?.Tile.ToString() ?? "unknown map";
			int currentTick = Find.TickManager?.TicksGame ?? 0;
			int ticksUntil = Math.Max(0, pending.fireTick - currentTick);
			string timing = ticksUntil < GenDate.TicksPerDay
				? $"about {Math.Max(1, ticksUntil / GenDate.TicksPerHour)} hours"
				: $"about {ticksUntil / (float)GenDate.TicksPerDay:F1} days";

			return $"Stored event: {incidentLabel}\nDef: {incidentDefName}\nCategory: {categoryLabel}\nTarget: {targetLabel}\nThreat points: {pending.threatPoints:F0}\nExpected in: {timing}";
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref pendingIncidents, "pendingIncidents", LookMode.Deep);
			Scribe_Collections.Look(ref recentIncidents, "recentIncidents", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pendingIncidents ??= new List<PendingDivinationEvent>();
				recentIncidents ??= new List<PendingDivinationEvent>();
				pendingIncidents.RemoveAll(e => e == null || e.incidentDef == null);
				recentIncidents.RemoveAll(e => e == null || e.incidentDef == null);
				foreach (PendingDivinationEvent pending in pendingIncidents)
					pending.queuedWithStoryteller = false;
			}
		}

		private int FindMatchingPendingIncidentIndex(IncidentDef incidentDef, Map targetMap)
		{
			if (incidentDef == null || targetMap == null)
				return -1;

			int targetTile = targetMap.Tile;
			return pendingIncidents.FindLastIndex(e =>
				e.incidentDef == incidentDef &&
				e.targetTile == targetTile);
		}
	}
}
