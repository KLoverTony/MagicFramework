using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AeternusFaith
{
    public class CompProperties_BonewrightLecternRitualFallback : CompProperties
    {
        public List<ThingDef> supportedCircleDefs;
        public string commandLabel = "Bonewright rites unavailable";
        public string commandDescription = "Place this lectern orthogonally adjacent to a Bonewright ritual circle to begin that circle's rites.";
        public string commandIconPath = "Things/RitualLectern/Ossanith_Lectern";

        public CompProperties_BonewrightLecternRitualFallback()
        {
            compClass = typeof(Comp_BonewrightLecternRitualFallback);
        }
    }

    public class Comp_BonewrightLecternRitualFallback : ThingComp
    {
        private CompProperties_BonewrightLecternRitualFallback Props => (CompProperties_BonewrightLecternRitualFallback)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (HasAdjacentSupportedCircle())
                yield break;

            Command_Action command = new Command_Action
            {
                defaultLabel = Props.commandLabel,
                defaultDesc = Props.commandDescription,
                icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true)
            };
            command.Disable("Requires an orthogonally adjacent Bonewright ritual circle.");
            yield return command;
        }

        private bool HasAdjacentSupportedCircle()
        {
            if (Props.supportedCircleDefs == null || Props.supportedCircleDefs.Count == 0)
                return false;

            return Props.supportedCircleDefs.Any(def =>
                def != null &&
                RitualAdjacencyUtility.TryFindOrthogonallyAdjacentThing(parent, def, out _));
        }
    }
}
