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
            Func<Corpse, bool> corpseValidator,
            Func<Pawn, Corpse, Thing, bool> conductorValidator,
            Func<Pawn, bool> audienceValidator)
            : base(
                "Skeleton rite",
                "Begin rite",
                lectern?.Map?.mapPawns?.FreeColonistsSpawned.Where(pawn => audienceValidator(pawn)) ?? Enumerable.Empty<Pawn>(),
                lectern?.Map?.listerThings?.AllThings.OfType<Corpse>() ?? Enumerable.Empty<Corpse>(),
                corpse => corpseValidator(corpse) ? true : "The rite requires a humanlike mortal corpse.",
                (pawn, corpse) => conductorValidator(pawn, corpse, circle) ? true : "Cannot reach and reserve the corpse, lectern, and circle.",
                pawn => audienceValidator(pawn) ? true : "Not available to attend this rite.",
                result => startRitual(result.conductor, result.audience, result.corpse, circle))
        {
        }
    }
}
