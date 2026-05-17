using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MFVanilla.Core;

public sealed class SitePartWorker_ArcaneCache : SitePartWorker
{
}

public sealed class GenStep_ArcaneCache : GenStep
{
    public string chestThingDef = "MFV_ArcaneTreasureChest";
    public int roomWidth = 13;
    public int roomHeight = 11;
    public int defenderCount = 3;

    public override int SeedPart => 941735421;

    public override void Generate(Map map, GenStepParams parms)
    {
        if (map == null)
        {
            return;
        }

        IntVec3 center = FindCacheCenter(map);
        CellRect room = CellRect.CenteredOn(center, roomWidth, roomHeight).ClipInsideMap(map);
        BuildCacheRoom(map, room);
        SpawnCacheChest(map, room.CenterCell);
        SpawnDefenders(map, room, ResolveDefenderCount(parms));
    }

    private static IntVec3 FindCacheCenter(Map map)
    {
        IntVec3 mapCenter = new(map.Size.x / 2, 0, map.Size.z / 2);
        if (mapCenter.InBounds(map))
        {
            return mapCenter;
        }

        return CellFinder.RandomCell(map);
    }

    private void BuildCacheRoom(Map map, CellRect room)
    {
        TerrainDef floorDef = DefDatabase<TerrainDef>.GetNamedSilentFail("TileSandstone") ?? TerrainDefOf.Concrete;
        ThingDef wallDef = ThingDefOf.Wall;
        ThingDef wallStuff = DefDatabase<ThingDef>.GetNamedSilentFail("BlocksSandstone") ?? ThingDefOf.Steel;

        foreach (IntVec3 cell in room.Cells)
        {
            if (!cell.InBounds(map))
            {
                continue;
            }

            ClearCellForCache(map, cell);
            map.terrainGrid.SetTerrain(cell, floorDef);

            if (cell.x == room.minX || cell.x == room.maxX || cell.z == room.minZ || cell.z == room.maxZ)
            {
                Thing wall = ThingMaker.MakeThing(wallDef, wallStuff);
                GenSpawn.Spawn(wall, cell, map);
            }
        }

        OpenDoorway(map, new IntVec3(room.CenterCell.x, 0, room.minZ));
        ScatterDressing(map, room);
    }

    private static void OpenDoorway(Map map, IntVec3 cell)
    {
        if (!cell.InBounds(map))
        {
            return;
        }

        List<Thing> things = cell.GetThingList(map);
        for (int i = things.Count - 1; i >= 0; i--)
        {
            Thing thing = things[i];
            if (thing?.def == ThingDefOf.Wall)
            {
                thing.Destroy();
            }
        }
    }

    private static void ClearCellForCache(Map map, IntVec3 cell)
    {
        List<Thing> things = cell.GetThingList(map);
        for (int i = things.Count - 1; i >= 0; i--)
        {
            Thing thing = things[i];
            if (thing == null || thing.def.category == ThingCategory.Filth || thing.def.category == ThingCategory.Plant)
            {
                thing?.Destroy();
            }
        }
    }

    private void SpawnCacheChest(Map map, IntVec3 cell)
    {
        ThingDef chestDef = DefDatabase<ThingDef>.GetNamedSilentFail(chestThingDef)
            ?? DefDatabase<ThingDef>.GetNamedSilentFail("MFV_ArcaneTreasureChest");
        if (chestDef == null)
        {
            Log.Warning("[MFVanilla] Arcane cache site could not resolve an arcane treasure chest ThingDef.");
            return;
        }

        Thing chest = ThingMaker.MakeThing(chestDef);
        GenSpawn.Spawn(chest, cell, map);
    }

    private void ScatterDressing(Map map, CellRect room)
    {
        TrySpawnDressing(map, room, "MFV_MagicTorchLamp", new IntVec3(room.minX + 2, 0, room.minZ + 2));
        TrySpawnDressing(map, room, "MFV_MagicTorchLamp", new IntVec3(room.maxX - 2, 0, room.minZ + 2));
        TrySpawnDressing(map, room, "MFV_ArcaneSpire", new IntVec3(room.minX + 2, 0, room.maxZ - 2));
        TrySpawnDressing(map, room, "MFV_ArcaneSpire", new IntVec3(room.maxX - 2, 0, room.maxZ - 2));
    }

    private static void TrySpawnDressing(Map map, CellRect room, string thingDefName, IntVec3 cell)
    {
        if (!cell.InBounds(map) || !room.Contains(cell))
        {
            return;
        }

        ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(thingDefName);
        if (thingDef == null)
        {
            return;
        }

        GenSpawn.Spawn(ThingMaker.MakeThing(thingDef), cell, map);
    }

    private int ResolveDefenderCount(GenStepParams parms)
    {
        float points = parms.sitePart?.parms?.threatPoints ?? parms.sitePart?.parms?.points ?? 0f;
        if (points >= 900f)
        {
            return Math.Max(defenderCount + 2, 5);
        }

        if (points >= 500f)
        {
            return Math.Max(defenderCount + 1, 4);
        }

        return Math.Max(1, defenderCount);
    }

    private static void SpawnDefenders(Map map, CellRect room, int count)
    {
        Faction faction = Faction.OfMechanoids;
        PawnKindDef[] candidates =
        {
            PawnKindDefOf.Mech_Scyther,
            DefDatabase<PawnKindDef>.GetNamedSilentFail("Mech_Lancer"),
            DefDatabase<PawnKindDef>.GetNamedSilentFail("Mech_Pikeman")
        };

        List<IntVec3> spawnCells = CandidateDefenderCells(room, map);
        for (int i = 0; i < count; i++)
        {
            PawnKindDef pawnKindDef = candidates[i % candidates.Length];
            if (pawnKindDef == null || spawnCells.Count == 0)
            {
                continue;
            }

            IntVec3 cell = spawnCells[i % spawnCells.Count];
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, faction));
            GenSpawn.Spawn(pawn, cell, map);
        }
    }

    private static List<IntVec3> CandidateDefenderCells(CellRect room, Map map)
    {
        List<IntVec3> cells = new();
        CellRect inner = room.ContractedBy(2);
        foreach (IntVec3 cell in inner.Cells)
        {
            if (cell.InBounds(map) && cell.Standable(map) && cell != room.CenterCell)
            {
                cells.Add(cell);
            }
        }

        cells.SortBy(cell => cell.DistanceToSquared(room.CenterCell));
        return cells;
    }
}
