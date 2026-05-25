using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AeternusFaith
{
    public static class RitualAdjacencyUtility
    {
        public static bool TryFindOrthogonallyAdjacentThing(Thing source, ThingDef targetDef, out Thing target)
        {
            target = null;
            Map map = source?.Map;
            if (map == null || targetDef == null)
            {
                return false;
            }

            HashSet<IntVec3> checkedCells = new HashSet<IntVec3>();
            foreach (IntVec3 occupiedCell in source.OccupiedRect())
            {
                foreach (IntVec3 offset in GenAdj.CardinalDirections)
                {
                    IntVec3 candidateCell = occupiedCell + offset;
                    if (!candidateCell.InBounds(map) || !checkedCells.Add(candidateCell))
                    {
                        continue;
                    }

                    target = candidateCell.GetThingList(map).FirstOrDefault(thing => thing.def == targetDef);
                    if (target != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
