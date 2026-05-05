using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AeternusFaith
{
    /// <summary>
    /// Describes a single surrounding piece entry for CompProperties_PlaceRitualRoomBlueprints.
    /// </summary>
    public class RitualRoomPieceEntry
    {
        /// <summary>The ThingDef to place as a blueprint.</summary>
        public ThingDef def;

        /// <summary>
        /// Offset in cells from the center piece's south-west corner (parent.Position).
        /// X = east, Z = north.
        /// </summary>
        public IntVec3 offset = IntVec3.Zero;

        /// <summary>Rotation to use when placing the blueprint (default North).</summary>
        public Rot4 rotation = Rot4.North;
    }

    public class CompProperties_PlaceRitualRoomBlueprints : CompProperties
    {
        /// <summary>List of surrounding pieces to blueprint when the gizmo is activated.</summary>
        public List<RitualRoomPieceEntry> pieces = new List<RitualRoomPieceEntry>();

        public string commandLabel = "Place ritual room blueprints";
        public string commandDescription = "Place construction blueprints for all surrounding ritual room pieces.";
        public string commandIconPath = "UI/Commands/AddToArea";

        public CompProperties_PlaceRitualRoomBlueprints()
        {
            this.compClass = typeof(Comp_PlaceRitualRoomBlueprints);
        }
    }

    public class Comp_PlaceRitualRoomBlueprints : ThingComp
    {
        private CompProperties_PlaceRitualRoomBlueprints Props =>
            (CompProperties_PlaceRitualRoomBlueprints)this.props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            Texture2D icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true);

            Command_Action cmd = new Command_Action
            {
                defaultLabel = Props.commandLabel,
                defaultDesc = Props.commandDescription,
                icon = icon,
                action = PlaceAllBlueprints
            };

            // Disable the gizmo if every piece is already built or blueprinted.
            if (AllPiecesPresent())
                cmd.Disable("All surrounding pieces are already placed or blueprinted.");

            yield return cmd;
        }

        /// <summary>
        /// Returns true if every configured piece already has a built thing or a blueprint/frame
        /// occupying its target cells on the map.
        /// </summary>
        private bool AllPiecesPresent()
        {
            if (Props.pieces.NullOrEmpty())
                return true;

            Map map = parent.Map;
            if (map == null)
                return true;

            foreach (RitualRoomPieceEntry entry in Props.pieces)
            {
                if (entry.def == null)
                    continue;

                IntVec3 loc = parent.Position + entry.offset;
                if (!loc.InBounds(map))
                    continue;

                if (!PieceIsPresent(entry.def, loc, map))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Places blueprints for every configured surrounding piece that does not already have
        /// a built building, blueprint, or construction frame occupying the target location.
        /// </summary>
        private void PlaceAllBlueprints()
        {
            Map map = parent.Map;
            if (map == null || Props.pieces.NullOrEmpty())
                return;

            int placed = 0;
            int skipped = 0;

            foreach (RitualRoomPieceEntry entry in Props.pieces)
            {
                if (entry.def == null)
                    continue;

                IntVec3 loc = parent.Position + entry.offset;
                if (!loc.InBounds(map))
                    continue;

                if (PieceIsPresent(entry.def, loc, map))
                {
                    skipped++;
                    continue;
                }

                GenConstruct.PlaceBlueprintForBuild(
                    entry.def,
                    loc,
                    map,
                    entry.rotation,
                    Faction.OfPlayer,
                    null,
                    null,
                    null,
                    false
                );
                placed++;
            }

            if (placed > 0)
                Messages.Message(
                    $"Placed {placed} ritual room blueprint{(placed != 1 ? "s" : "")}." +
                    (skipped > 0 ? $" ({skipped} already present, skipped.)" : ""),
                    parent,
                    MessageTypeDefOf.TaskCompletion,
                    historical: false
                );
            else
                Messages.Message(
                    "All surrounding ritual room pieces are already placed or blueprinted.",
                    parent,
                    MessageTypeDefOf.RejectInput,
                    historical: false
                );
        }

        /// <summary>
        /// Returns true if the given cell already contains the target built thing, a blueprint,
        /// or a construction frame for the same def.
        /// </summary>
        private static bool PieceIsPresent(ThingDef def, IntVec3 loc, Map map)
        {
            foreach (Thing t in loc.GetThingList(map))
            {
                // Already fully built
                if (t.def == def)
                    return true;

                // Blueprint waiting to be built
                if (t is Blueprint_Build bp && bp.def.entityDefToBuild == def)
                    return true;

                // Construction frame in progress
                if (t is Frame frame && frame.def.entityDefToBuild == def)
                    return true;
            }
            return false;
        }
    }
}
