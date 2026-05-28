using RimWorld;
using Verse;

namespace MFStoryteller
{
	public class IncidentWorker_TestOfCharacter : IncidentWorker
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

			Pawn subject = map.mapPawns.FreeColonists.RandomElement();
			string label = "Test of Character";
			string text = $"Lord Roth has set a trial for {subject.NameShortColored}.\n\nA situation has been arranged that will test their moral fiber and resolve. How will they respond to this test of character?";

			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, subject);
			return true;
		}
	}
}
