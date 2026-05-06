using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    public class CompProperties_SummonSpectre : CompProperties
    {
        public ThingDef circleDef;
        public string commandLabel = "Summon spectre";
        public string commandDescription = "Summon a test spectre at the linked ritual center.";
        public string commandIconPath;

        public CompProperties_SummonSpectre()
        {
            compClass = typeof(Comp_SummonSpectre);
        }
    }

    public class Comp_SummonSpectre : ThingComp
    {
        private CompProperties_SummonSpectre Props => (CompProperties_SummonSpectre)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            Command_Action command = new Command_Action
            {
                defaultLabel = Props.commandLabel,
                defaultDesc = Props.commandDescription,
                icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true),
                action = SummonSpectre
            };

            if (!TryFindRitualCenter(out _, out string failReason))
                command.Disable(failReason);

            yield return command;
        }

        private bool TryFindRitualCenter(out Thing circle, out string failReason)
        {
            circle = null;
            failReason = null;

            if (Props.circleDef == null)
            {
                failReason = "The spectre summon is missing its ritual center definition.";
                return false;
            }

            foreach (IntVec3 cell in GenAdj.CardinalDirections.Select(offset => parent.Position + offset))
            {
                if (!cell.InBounds(parent.Map))
                    continue;

                circle = cell.GetThingList(parent.Map).FirstOrDefault(t => t.def == Props.circleDef);
                if (circle != null)
                    return true;
            }

            failReason = "Requires an orthogonally adjacent Shroudhymn ritual center.";
            return false;
        }

        private void SummonSpectre()
        {
            if (!TryFindRitualCenter(out Thing circle, out string failReason))
            {
                Messages.Message(failReason, parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Map map = parent.Map;
            MapComponent_SpectralEntities comp = map.GetComponent<MapComponent_SpectralEntities>();
            if (comp == null)
            {
                Messages.Message("Could not find the spectral entity tracker for this map.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            SpectralEntity spirit = new SpectralEntity(map)
            {
                label = "Test Spirit " + Rand.Range(100, 999).ToString(),
                state = SpectralState.WanderingUnseen,
                anchorPosition = circle.Position,
                lastKnownPosition = ResolveManifestCell(circle),
                pawnKind = PawnKindDefOf.Colonist,
                faction = Faction.OfPlayer
            };

            comp.AddSpirit(spirit);
            spirit.Manifest();
        }

        private IntVec3 ResolveManifestCell(Thing circle)
        {
            if (IsValidManifestCell(circle.Position))
                return circle.Position;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(circle.Position, 3f, true).OrderBy(cell => cell.DistanceToSquared(circle.Position)))
            {
                if (IsValidManifestCell(cell))
                    return cell;
            }

            return parent.InteractionCell;
        }

        private bool IsValidManifestCell(IntVec3 cell)
        {
            return cell.IsValid &&
                   cell.InBounds(parent.Map) &&
                   cell.Standable(parent.Map) &&
                   cell.GetFirstPawn(parent.Map) == null;
        }
    }
}
