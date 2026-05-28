using RimWorld;
using Verse;

namespace MFStoryteller
{
	public class IncidentWorker_DivineAudience : IncidentWorker
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

			Pawn recipient = map.mapPawns.FreeColonists.RandomElement();
			string label = "Divine Audience";
			string text = $"{recipient.NameShortColored} experiences a vision from Lord Roth himself.\n\nThe god-like entity speaks of fate and destiny, imparting cryptic insights about the future. {recipient.NameShortColored} feels changed by the experience.";

			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, recipient);
			return true;
		}
	}
}
