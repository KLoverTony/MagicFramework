using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MFVanilla.Core;

public sealed class Verb_StoneGolemBoulder : Verb_Shoot
{
    private const int ChunksToRaise = 3;
    private const float ChunkRaiseRadius = 2.8f;
    private const float AmmoSearchRadius = 2.8f;
    private const string FallbackChunkDefName = "ChunkSandstone";

    private static readonly string[] ChunkDefNames =
    {
        "ChunkGranite",
        "ChunkLimestone",
        "ChunkSandstone",
        "ChunkSlate",
        "ChunkMarble"
    };

    protected override bool TryCastShot()
    {
        Map map = caster?.Map;
        if (caster == null || map == null)
        {
            return base.TryCastShot();
        }

        Thing ammoChunk = FindNearestChunk(caster.Position, map, AmmoSearchRadius);
        if (ammoChunk == null)
        {
            RaiseStoneChunks(caster.Position, map);
            return true;
        }

        ammoChunk.Destroy();
        return base.TryCastShot();
    }

    private static void RaiseStoneChunks(IntVec3 center, Map map)
    {
        int raised = 0;
        int index = 0;
        foreach (IntVec3 offset in GenRadial.RadialPattern)
        {
            if (raised >= ChunksToRaise)
            {
                return;
            }

            IntVec3 cell = center + offset;
            if (!cell.InBounds(map) || cell == center || !cell.Standable(map))
            {
                continue;
            }

            if (cell.DistanceTo(center) > ChunkRaiseRadius || cell.GetFirstItem(map) != null)
            {
                continue;
            }

            ThingDef chunkDef = ChunkDefForIndex(index++);
            if (chunkDef == null)
            {
                continue;
            }

            Thing chunk = ThingMaker.MakeThing(chunkDef);
            if (GenPlace.TryPlaceThing(chunk, cell, map, ThingPlaceMode.Near))
            {
                raised++;
            }
        }
    }

    private static Thing FindNearestChunk(IntVec3 center, Map map, float radius)
    {
        Thing nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(center, map, radius, true))
        {
            if (!IsStoneChunk(thing))
            {
                continue;
            }

            float distance = thing.Position.DistanceToSquared(center);
            if (distance < nearestDistance)
            {
                nearest = thing;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private static ThingDef ChunkDefForIndex(int index)
    {
        string defName = ChunkDefNames[Math.Abs(index) % ChunkDefNames.Length];
        return DefDatabase<ThingDef>.GetNamedSilentFail(defName)
            ?? DefDatabase<ThingDef>.GetNamedSilentFail(FallbackChunkDefName);
    }

    private static bool IsStoneChunk(Thing thing)
    {
        if (thing?.def == null)
        {
            return false;
        }

        for (int i = 0; i < ChunkDefNames.Length; i++)
        {
            if (thing.def.defName == ChunkDefNames[i])
            {
                return true;
            }
        }

        return thing.def.IsWithinCategory(ThingCategoryDefOf.StoneChunks)
            || thing.def.defName?.StartsWith("Chunk") == true;
    }
}
