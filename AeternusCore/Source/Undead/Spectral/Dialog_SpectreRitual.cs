using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    public class Dialog_SpectreRitual : Dialog_ParticipantSelection
    {
        public Dialog_SpectreRitual(
            Thing lectern,
            Thing circle,
            Action<Pawn, List<Pawn>, Corpse, Thing> startRitual,
            Func<Corpse, AcceptanceReport> corpseValidator,
            Func<Pawn, Corpse, Thing, AcceptanceReport> conductorValidator,
            Func<Pawn, AcceptanceReport> audienceValidator,
            string dialogTitle = "Animate Veilbound Shade",
            string acceptLabel = "Animate Veilbound Shade")
            : base(
                dialogTitle,
                acceptLabel,
                lectern?.Map?.mapPawns?.FreeColonistsSpawned.Where(pawn => audienceValidator(pawn).Accepted) ?? Enumerable.Empty<Pawn>(),
                lectern?.Map?.listerThings?.AllThings.OfType<Corpse>() ?? Enumerable.Empty<Corpse>(),
                corpseValidator,
                (pawn, corpse) => conductorValidator(pawn, corpse, circle),
                audienceValidator,
                result => startRitual(result.conductor, result.audience, result.corpse, circle))
        {
        }
    }
}
