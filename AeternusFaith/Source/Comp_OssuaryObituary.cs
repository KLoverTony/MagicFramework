using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AeternusFaith
{
    public class CompProperties_OssuaryObituary : CompProperties
    {
        public CompProperties_OssuaryObituary()
        {
            compClass = typeof(Comp_OssuaryObituary);
        }
    }

    public class Comp_OssuaryObituary : ThingComp
    {
        private bool filled;
        private string obituary;

        public bool HasRemains => filled;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref filled, "filled");
            Scribe_Values.Look(ref obituary, "obituary");
        }

        public override string CompInspectStringExtra()
        {
            return string.IsNullOrEmpty(obituary) ? null : obituary;
        }

        public void Record(Corpse corpse, Pawn conductor)
        {
            filled = true;
            Pawn deceased = corpse?.InnerPawn;
            if (deceased == null)
            {
                obituary = "An unnamed body is sealed within this ossuary.";
                return;
            }

            string ageText = deceased.ageTracker != null ? ", age " + deceased.ageTracker.AgeBiologicalYears : "";
            string kindText = string.IsNullOrEmpty(deceased.KindLabel) ? "pawn" : deceased.KindLabel;
            string conductorText = conductor != null ? " Sealed by " + conductor.LabelShortCap + "." : string.Empty;
            obituary = "Here lie the remains of " + deceased.LabelShortCap + ageText + ", " + kindText + "." + conductorText;
        }

        public void CopyFrom(Comp_OssuaryObituary other)
        {
            if (other == null)
                return;

            filled = other.filled;
            obituary = other.obituary;
        }
    }

    public class CompProperties_PlaceOssuaryInAlcove : CompProperties
    {
        public ThingDef graniteAlcoveWallDef;
        public ThingDef slateAlcoveWallDef;
        public ThingDef filledGraniteAlcoveWallDef;
        public ThingDef filledSlateAlcoveWallDef;
        public string commandLabel = "Place in alcove";
        public string commandDescription = "Place this filled ossuary bone box in an adjacent alcove wall.";
        public string commandIconPath = "Things/Walls/Alcove wall - filled";

        public CompProperties_PlaceOssuaryInAlcove()
        {
            compClass = typeof(Comp_PlaceOssuaryInAlcove);
        }
    }

    public class Comp_PlaceOssuaryInAlcove : ThingComp
    {
        private CompProperties_PlaceOssuaryInAlcove Props => (CompProperties_PlaceOssuaryInAlcove)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            Comp_OssuaryObituary contents = parent.GetComp<Comp_OssuaryObituary>();
            if (contents == null || !contents.HasRemains)
                yield break;

            Command_Action command = new Command_Action
            {
                defaultLabel = Props.commandLabel,
                defaultDesc = Props.commandDescription,
                icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, true),
                action = InstallInAdjacentAlcove
            };

            if (!parent.Spawned)
                command.Disable("Place the filled ossuary bone box beside an alcove wall first.");
            else if (!TryFindAdjacentAlcoveWall(out _, out _))
                command.Disable("Requires an adjacent empty alcove wall.");

            yield return command;
        }

        private void InstallInAdjacentAlcove()
        {
            Comp_OssuaryObituary contents = parent.GetComp<Comp_OssuaryObituary>();
            if (contents == null || !contents.HasRemains)
            {
                Messages.Message("Only a filled ossuary bone box can be placed in an alcove wall.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (!TryFindAdjacentAlcoveWall(out Thing wall, out ThingDef filledWallDef))
            {
                Messages.Message("Place the filled ossuary bone box adjacent to an empty alcove wall.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            Map map = parent.Map;
            IntVec3 wallPosition = wall.Position;
            Rot4 wallRotation = wall.Rotation;
            Faction wallFaction = wall.Faction;
            int wallHitPoints = wall.HitPoints;

            ThingWithComps filledWall = ThingMaker.MakeThing(filledWallDef) as ThingWithComps;
            if (filledWall == null)
            {
                Messages.Message("The filled alcove wall could not be created.", parent, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            filledWall.GetComp<Comp_OssuaryObituary>()?.CopyFrom(contents);
            filledWall.HitPoints = Mathf.Min(wallHitPoints, filledWall.MaxHitPoints);
            if (wallFaction != null)
                filledWall.SetFaction(wallFaction);

            wall.DeSpawn(DestroyMode.Vanish);
            GenSpawn.Spawn(filledWall, wallPosition, map, wallRotation);
            wall.Destroy(DestroyMode.Vanish);
            parent.Destroy(DestroyMode.Vanish);

            Messages.Message("The ossuary bone box has been sealed into the alcove wall.", filledWall, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        private bool TryFindAdjacentAlcoveWall(out Thing wall, out ThingDef filledWallDef)
        {
            wall = null;
            filledWallDef = null;

            if (!parent.Spawned)
                return false;

            foreach (IntVec3 cell in GenAdj.CardinalDirections.Select(offset => parent.Position + offset))
            {
                if (!cell.InBounds(parent.Map))
                    continue;

                foreach (Thing candidate in cell.GetThingList(parent.Map))
                {
                    ThingDef candidateFilledDef = FilledWallDefFor(candidate.def);
                    if (candidateFilledDef == null)
                        continue;

                    wall = candidate;
                    filledWallDef = candidateFilledDef;
                    return true;
                }
            }

            return false;
        }

        private ThingDef FilledWallDefFor(ThingDef wallDef)
        {
            if (wallDef == Props.graniteAlcoveWallDef)
                return Props.filledGraniteAlcoveWallDef;

            if (wallDef == Props.slateAlcoveWallDef)
                return Props.filledSlateAlcoveWallDef;

            return null;
        }
    }
}
