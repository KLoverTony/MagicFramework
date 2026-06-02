using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework;
using Verse;

namespace AeternusFaith
{
    public class Dialog_SkeletonRitual : Dialog_ParticipantSelection
    {
        public Dialog_SkeletonRitual(
            Thing lectern,
            Thing circle,
            Action<Pawn, List<Pawn>, Corpse, Thing> startRitual,
            Func<Corpse, AcceptanceReport> corpseValidator,
            Func<Pawn, Corpse, Thing, AcceptanceReport> conductorValidator,
            Func<Pawn, AcceptanceReport> audienceValidator,
            string dialogTitle = "Animate undead",
            string acceptLabel = "Animate")
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
