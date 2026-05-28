using RimWorld;
using Verse;

namespace MFStoryteller
{
	public class IncidentWorker_FateTwist : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = parms?.target as Map;
			return map != null && map.mapPawns.FreeColonists.Count > 0;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = parms?.target as Map;
			if (map == null)
				return false;

			bool isFavorable = Rand.Chance(0.5f);
			string label = isFavorable ? "Twist of Fortune" : "Cruel Twist";
			string text;
			LookTargets lookTarget;

			if (isFavorable)
			{
				Pawn fortunateOne = map.mapPawns.FreeColonists.RandomElement();
				text = $"The winds of fate favor {fortunateOne.NameShortColored}.\n\nLord Roth has woven a twist of good fortune into their thread of destiny. A moment of unexpected grace has befallen them.";
				lookTarget = fortunateOne;
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, lookTarget);
			}
			else
			{
				Pawn firstColonist = map.mapPawns.FreeColonists[0];
				text = $"Lord Roth weaves a cruel twist into the fabric of fate.\n\nAn unexpected complication has arisen, testing the resolve of your colony. The winds of destiny shift in unpredictable ways.";
				lookTarget = firstColonist;
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatSmall, lookTarget);
			}

			return true;
		}
	}
}

