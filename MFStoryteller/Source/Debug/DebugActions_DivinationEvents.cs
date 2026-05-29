using RimWorld;
using Verse;

namespace MFStoryteller
{
	public static class DebugActions_DivinationEvents
	{
		public static void ShowDivinationEvents()
		{
			Find.WindowStack.Add(new Dialog_DebugDivinationEvents());
		}

		public static void ForceNextIncident()
		{
			var divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
			if (divComponent == null)
			{
				Messages.Message("WorldComponent_DivinationEvents not found", MessageTypeDefOf.RejectInput);
				return;
			}

			var pending = divComponent.GetPendingIncidents();
			if (pending.Count == 0)
			{
				Messages.Message("No pending incidents to force", MessageTypeDefOf.RejectInput);
				return;
			}

			var incident = pending[0];
			Messages.Message($"Forcing incident: {incident.incidentDef.label}", MessageTypeDefOf.NeutralEvent);

			IncidentWorker worker = (IncidentWorker)System.Activator.CreateInstance(incident.incidentDef.workerClass);
			IncidentParms parms = new IncidentParms()
			{
				target = incident.TargetMap,
				points = incident.threatPoints
			};

			worker.TryExecute(parms);
		}
	}
}


