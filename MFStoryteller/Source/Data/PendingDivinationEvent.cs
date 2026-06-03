using RimWorld;
using System.Linq;
using Verse;

namespace MFStoryteller
{
	public class PendingDivinationEvent : IExposable
	{
		public IncidentDef incidentDef;
		public int fireTick;
		public float threatPoints;
		public int targetTile = -1;
		public int selectedTick;
		public FiringIncident firingIncident;
		public bool queuedWithStoryteller;

		public PendingDivinationEvent()
		{
		}

		public PendingDivinationEvent(IncidentDef incidentDef, int fireTick, float threatPoints, Map targetMap, int selectedTick)
		{
			this.incidentDef = incidentDef;
			this.fireTick = fireTick;
			this.threatPoints = threatPoints;
			this.targetTile = targetMap?.Tile ?? -1;
			this.selectedTick = selectedTick;
		}

		public PendingDivinationEvent(FiringIncident firingIncident, int fireTick, int selectedTick)
			: this(firingIncident?.def, fireTick, firingIncident?.parms?.points ?? 0f, firingIncident?.parms?.target as Map, selectedTick)
		{
			this.firingIncident = firingIncident;
		}

		public Map TargetMap => Find.Maps.FirstOrDefault(m => m.Tile == targetTile);

		public void ExposeData()
		{
			Scribe_Defs.Look(ref incidentDef, "incidentDef");
			Scribe_Values.Look(ref fireTick, "fireTick", 0);
			Scribe_Values.Look(ref threatPoints, "threatPoints", 0f);
			Scribe_Values.Look(ref targetTile, "targetTile", -1);
			Scribe_Values.Look(ref selectedTick, "selectedTick", 0);
			Scribe_Deep.Look(ref firingIncident, "firingIncident");
			Scribe_Values.Look(ref queuedWithStoryteller, "queuedWithStoryteller", false);
		}
	}
}
