using RimWorld;
using Verse;

namespace MFStoryteller
{
	public class IncidentWorker_BlessingOfFavor : IncidentWorker
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

			Pawn blessed = map.mapPawns.FreeColonists.RandomElement();
			string label = "Blessing of Favor";
			string text = $"{blessed.NameShortColored} has earned the favor of Lord Roth.\n\nThe god-like entity has blessed them with enhanced fortune and capability. They feel invigorated and blessed.";

			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, blessed);

			return true;
		}
	}
}
