using RimWorld;
using UnityEngine;
using Verse;

namespace MFStoryteller
{
	public class MFStorytellerMod : Mod
	{
		public MFStorytellerMod(ModContentPack content) : base(content)
		{
		}

		public override string SettingsCategory()
		{
			return "MF Storyteller";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard();
			listing.Begin(inRect);

			listing.Label("Divination diagnostics");
			listing.Gap();

			if (listing.ButtonText("Reveal stored event"))
			{
				WorldComponent_DivinationEvents divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
				if (divComponent == null)
				{
					Messages.Message("No active world divination component was found.", MessageTypeDefOf.NeutralEvent);
				}
				else if (!divComponent.TryRevealNextPendingIncident())
				{
					Messages.Message("No divination event is currently stored.", MessageTypeDefOf.NeutralEvent);
				}
			}

			listing.Label("Shows the next event currently held by Lord Roth's divination scheduler, if one exists.");
			listing.End();
		}
	}
}
