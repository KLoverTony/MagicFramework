using RimWorld;
using Verse;

namespace MFStoryteller
{
	public class IncidentWorker_CurseOfDisfavor : IncidentWorker
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

			Pawn cursed = map.mapPawns.FreeColonists.RandomElement();
			string label = "Curse of Disfavor";
			string text = $"{cursed.NameShortColored} has incurred the displeasure of Lord Roth.\n\nThe god-like entity has placed a curse upon them. They feel beset by misfortune and ill luck.";

			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatSmall, cursed);

			return true;
		}
	}
}
