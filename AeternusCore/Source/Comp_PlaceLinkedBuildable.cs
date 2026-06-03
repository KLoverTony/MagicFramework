using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AeternusFaith
{
    public class CompProperties_PlaceLinkedBuildable : CompProperties
    {
        public ThingDef targetDef;
        public IntVec3 offset = IntVec3.Zero;
        public string commandLabel = "Place building";
        public string commandDescription = "Place a linked building";
        public string commandIconPath;

        public CompProperties_PlaceLinkedBuildable()
        {
            this.compClass = typeof(Comp_PlaceLinkedBuildable);
        }
    }

    public class Comp_PlaceLinkedBuildable : ThingComp
    {
        private CompProperties_PlaceLinkedBuildable Props => (CompProperties_PlaceLinkedBuildable)this.props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            yield return new Command_Action
            {
                defaultLabel = Props.commandLabel,
                defaultDesc = Props.commandDescription,
                icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true),
                action = () => PlaceLinkedBuilding()
            };
        }

        private void PlaceLinkedBuilding()
        {
            if (Props.targetDef == null)
                return;

            IntVec3 buildLoc = parent.Position + Props.offset;
            if (!buildLoc.InBounds(parent.Map))
                return;

            GenConstruct.PlaceBlueprintForBuild(Props.targetDef, buildLoc, parent.Map, Rot4.North, Faction.OfPlayer, null, null, null, false);
        }
    }
}
