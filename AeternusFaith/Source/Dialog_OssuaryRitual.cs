using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework;
using Verse;

namespace AeternusFaith
{
    public class Dialog_OssuaryRitual : Dialog_ParticipantSelection
    {
        public Dialog_OssuaryRitual(
            Thing lectern,
            Thing circle,
            Thing ossuary,
            Action<Pawn, List<Pawn>, Corpse, Thing, Thing> startRitual,
            Func<Corpse, AcceptanceReport> corpseValidator,
            Func<Pawn, Corpse, Thing, AcceptanceReport> conductorValidator,
            Func<Pawn, AcceptanceReport> audienceValidator)
            : base(
                "Ossuary rite",
                "Begin rite",
                lectern?.Map?.mapPawns?.FreeColonistsSpawned.Where(pawn => audienceValidator(pawn).Accepted) ?? Enumerable.Empty<Pawn>(),
                lectern?.Map?.listerThings?.AllThings.OfType<Corpse>() ?? Enumerable.Empty<Corpse>(),
                corpseValidator,
                (pawn, corpse) => conductorValidator(pawn, corpse, ossuary),
                audienceValidator,
                result => startRitual(result.conductor, result.audience, result.corpse, circle, ossuary))
        {
        }
    }
}
