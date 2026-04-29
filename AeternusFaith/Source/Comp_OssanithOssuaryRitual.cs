using RimWorld;
using Verse;

namespace AeternusFaith
{
    public class CompProperties_OssanithOssuaryRitual : CompProperties
    {
        public ThingDef circleDef;
        public ThingDef ossuaryDef;
        public string commandLabel = "Begin rite";
        public string commandDescription = "Begin an Ossanith ossuary rite";
        public string commandIconPath;

        public CompProperties_OssanithOssuaryRitual()
        {
            this.compClass = typeof(Comp_OssanithOssuaryRitual);
        }
    }

    public class Comp_OssanithOssuaryRitual : ThingComp
    {
        private CompProperties_OssanithOssuaryRitual Props => (CompProperties_OssanithOssuaryRitual)this.props;

        public override IEnumerable<Gizmo> CompGetGizmosInOrder()
        {
            foreach (var gizmo in base.CompGetGizmosInOrder())
                yield return gizmo;

            if (CanPerformRitual())
            {
                yield return new Command_Action
                {
                    defaultLabel = Props.commandLabel,
                    defaultDesc = Props.commandDescription,
                    icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true),
                    action = () => PerformRitual()
                };
            }
        }

        private bool CanPerformRitual()
        {
            // Check for adjacent ritual circle with ossuary
            if (Props.circleDef == null || Props.ossuaryDef == null)
                return false;

            IntVec3[] adjacentCells = GenAdj.AdjacentCells8Way(parent.Position).ToArray();
            foreach (IntVec3 cell in adjacentCells)
            {
                if (!cell.InBounds(parent.Map))
                    continue;

                Thing circle = cell.GetThingList(parent.Map).FirstOrDefault(t => t.def == Props.circleDef);
                if (circle != null)
                    return true;
            }
            return false;
        }

        private void PerformRitual()
        {
            // Placeholder ritual logic
            Messages.Message("Ossanith ossuary rite begun.", parent, MessageTypeDefOf.PositiveEvent);
        }
    }
}
