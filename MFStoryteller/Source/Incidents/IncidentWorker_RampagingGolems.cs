using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MFStoryteller
{
	public class IncidentWorker_RampagingGolems : IncidentWorker
	{
		private static readonly string[] CommonGolemKinds =
		{
			"MFV_ClayAutomaton",
			"MFV_RuneSlasherAutomaton",
			"MFV_FleshGolem"
		};

		private static readonly string[] AdvancedGolemKinds =
		{
			"MFV_CrystalSentinel",
			"MFV_RuneBallistaConstruct",
			"MFV_DeepIronGolem"
		};

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = parms?.target as Map;
			return map != null
				&& map.mapPawns.FreeColonistsSpawnedCount > 0
				&& ResolveGolemKinds(parms?.points ?? 0f).Count > 0
				&& RCellFinder.TryFindRandomPawnEntryCell(out _, map, CellFinder.EdgeRoadChance_Hostile, false, cell => IsValidEntryCell(cell, map));
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = parms?.target as Map;
			if (map == null)
				return false;

			List<PawnKindDef> golemKinds = ResolveGolemKinds(parms.points);
			if (golemKinds.Count == 0)
				return false;

			if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entryCell, map, CellFinder.EdgeRoadChance_Hostile, false, cell => IsValidEntryCell(cell, map)))
				return false;

			List<Pawn> spawnedGolems = SpawnGolems(map, entryCell, golemKinds, parms.points);
			if (spawnedGolems.Count == 0)
				return false;

			Faction faction = Faction.OfMechanoids;
			LordJob lordJob = new LordJob_AssaultColony(faction, false, false, false, false, false, false, false);
			LordMaker.MakeNewLord(faction, lordJob, map, spawnedGolems);

			string label = "Rampaging golems";
			string text = "A group of unstable arcane constructs has wandered onto the map. They are moving with destructive purpose and will smash through anything in their path.";
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatBig, spawnedGolems);
			return true;
		}

		private static List<PawnKindDef> ResolveGolemKinds(float threatPoints)
		{
			IEnumerable<string> defNames = threatPoints >= 700f
				? CommonGolemKinds.Concat(AdvancedGolemKinds)
				: CommonGolemKinds;

			return defNames
				.Select(defName => DefDatabase<PawnKindDef>.GetNamedSilentFail(defName))
				.Where(kindDef => kindDef != null)
				.ToList();
		}

		private static List<Pawn> SpawnGolems(Map map, IntVec3 entryCell, List<PawnKindDef> golemKinds, float threatPoints)
		{
			int count = ResolveGolemCount(threatPoints);
			List<Pawn> spawned = new List<Pawn>();
			for (int i = 0; i < count; i++)
			{
				PawnKindDef kindDef = golemKinds.RandomElementByWeight(kind => GolemWeight(kind, threatPoints));
				Pawn pawn = PawnGenerator.GeneratePawn(kindDef, Faction.OfMechanoids, map.Tile);
				IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(entryCell, map, 6);
				GenSpawn.Spawn(pawn, spawnCell, map);
				spawned.Add(pawn);
			}

			return spawned;
		}

		private static int ResolveGolemCount(float threatPoints)
		{
			if (threatPoints >= 1400f)
				return Rand.RangeInclusive(4, 6);

			if (threatPoints >= 800f)
				return Rand.RangeInclusive(3, 5);

			return Rand.RangeInclusive(2, 3);
		}

		private static float GolemWeight(PawnKindDef kindDef, float threatPoints)
		{
			if (kindDef == null)
				return 0f;

			if (kindDef.defName == "MFV_DeepIronGolem")
				return threatPoints >= 1200f ? 0.35f : 0.05f;

			if (AdvancedGolemKinds.Contains(kindDef.defName))
				return threatPoints >= 800f ? 0.6f : 0.1f;

			return 1f;
		}

		private static bool IsValidEntryCell(IntVec3 cell, Map map)
		{
			return cell.Standable(map);
		}
	}
}
