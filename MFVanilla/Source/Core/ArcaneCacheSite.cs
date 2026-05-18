using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MFVanilla.Core;

public sealed class SitePartWorker_ArcaneCache : SitePartWorker
{
}

public sealed class ArcaneSiteProfileDef : Def
{
    public int roomWidth = 13;
    public int roomHeight = 11;
    public ArcaneSiteLayoutShape layoutShape = ArcaneSiteLayoutShape.Rectangle;
    public TerrainDef floorDef;
    public ThingDef wallStuff;
    public List<ThingDef> wallStuffOptions;
    public ThingDef towerWallStuff;
    public List<ThingDef> towerWallStuffOptions;
    public ThingDef doorDef;
    public ThingDef chestThingDef;
    public int defenderCount = 3;
    public List<PawnKindDef> defenderPawnKinds;
    public List<ArcaneSiteDressingDef> dressing;
    public List<ArcaneSiteRoomModuleDef> roomModules;
    public bool addEntryPath;
    public int entryPathLength = 6;
    public bool addExteriorRuin;
    public int exteriorRuinCount = 10;
}

public enum ArcaneSiteLayoutShape
{
    Rectangle,
    Circle
}

public sealed class ArcaneSiteDressingDef
{
    public ThingDef thingDef;
    public int offsetX;
    public int offsetZ;
}

public sealed class ArcaneSiteRoomModuleDef
{
    public ArcaneSiteAxis axis = ArcaneSiteAxis.North;
    public ArcaneSiteRoomKind kind = ArcaneSiteRoomKind.Empty;
    public int width = 7;
    public int depth = 5;
    public int distanceFromTower = 0;
}

public sealed class ArcaneSiteProfileEntryDef
{
    public ArcaneSiteProfileDef profile;
    public float weight = 1f;
    public float minThreatPoints;
    public float maxThreatPoints = float.MaxValue;
}

public enum ArcaneSiteAxis
{
    North,
    East,
    South,
    West
}

public enum ArcaneSiteRoomKind
{
    Empty,
    Antechamber,
    Bedroom,
    ServantsQuarters,
    Storage
}

public sealed class GenStep_ArcaneCache : GenStep
{
    public ArcaneSiteProfileDef profile;
    public List<ArcaneSiteProfileDef> profilePool;
    public List<ArcaneSiteProfileEntryDef> profileEntries;
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

        ArcaneSiteProfileDef baseProfile = ResolveProfile();
        int siteSeed = ResolveSiteSeed(map, parms, baseProfile);
        float threatPoints = ResolveThreatPoints(parms);
        ArcaneSiteProfileDef resolvedProfile = ResolveProfileForSite(siteSeed, threatPoints, baseProfile);
        IntVec3 center = FindCacheCenter(map);
        CellRect room = CellRect.CenteredOn(center, resolvedProfile.roomWidth, resolvedProfile.roomHeight).ClipInsideMap(map);
        siteSeed = ResolveSiteSeed(map, parms, resolvedProfile);
        ThingDef outerWallStuff = ResolveWallStuff(siteSeed, resolvedProfile, tower: false);
        ThingDef towerWallStuff = ResolveWallStuff(siteSeed, resolvedProfile, tower: true);
        int resolvedDefenderCount = ResolveDefenderCount(parms, resolvedProfile);
        BuildCacheRoom(map, room, resolvedProfile, towerWallStuff);
        List<CellRect> moduleRooms = BuildRoomModules(map, room, resolvedProfile, outerWallStuff);
        BuildExteriorModules(map, room, resolvedProfile, outerWallStuff);
        Thing chest = SpawnCacheChest(map, room.CenterCell, resolvedProfile);
        List<Pawn> defenders = SpawnDefenders(map, room, resolvedDefenderCount, resolvedProfile);
        LogDevGeneration(map, parms, resolvedProfile, siteSeed, room, moduleRooms, chest, defenders);
    }

    private ArcaneSiteProfileDef ResolveProfile()
        {
            return profile
            ?? DefDatabase<ArcaneSiteProfileDef>.GetNamedSilentFail("MFV_ArcaneCache_Default")
            ?? new ArcaneSiteProfileDef
            {
                roomWidth = roomWidth,
                roomHeight = roomHeight,
                layoutShape = ArcaneSiteLayoutShape.Rectangle,
                defenderCount = defenderCount,
                chestThingDef = DefDatabase<ThingDef>.GetNamedSilentFail(chestThingDef)
            };
    }

    private ArcaneSiteProfileDef ResolveProfileForSite(int siteSeed, float threatPoints, ArcaneSiteProfileDef fallback)
    {
        if (!profileEntries.NullOrEmpty())
        {
            List<ArcaneSiteProfileEntryDef> eligible = new();
            float totalWeight = 0f;
            for (int i = 0; i < profileEntries.Count; i++)
            {
                ArcaneSiteProfileEntryDef entry = profileEntries[i];
                if (entry?.profile == null || entry.weight <= 0f || threatPoints < entry.minThreatPoints || threatPoints > entry.maxThreatPoints)
                {
                    continue;
                }

                eligible.Add(entry);
                totalWeight += entry.weight;
            }

            if (eligible.Count > 0 && totalWeight > 0f)
            {
                float pick = StableRange(siteSeed, fallback?.shortHash ?? 0, totalWeight);
                float cursor = 0f;
                for (int i = 0; i < eligible.Count; i++)
                {
                    cursor += eligible[i].weight;
                    if (pick <= cursor)
                    {
                        return eligible[i].profile;
                    }
                }

                return eligible[eligible.Count - 1].profile;
            }
        }

        if (profilePool.NullOrEmpty())
        {
            return fallback;
        }

        int index = StableIndex(siteSeed, fallback?.shortHash ?? 0, profilePool.Count);
        return profilePool[index] ?? fallback;
    }

    private static float ResolveThreatPoints(GenStepParams parms)
    {
        return parms.sitePart?.parms?.threatPoints ?? parms.sitePart?.parms?.points ?? 0f;
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

    private void BuildCacheRoom(Map map, CellRect room, ArcaneSiteProfileDef profile, ThingDef wallStuff)
    {
        TerrainDef floorDef = profile.floorDef ?? DefDatabase<TerrainDef>.GetNamedSilentFail("TileSandstone") ?? TerrainDefOf.Concrete;
        ThingDef wallDef = ThingDefOf.Wall;

        foreach (IntVec3 cell in room.Cells)
        {
            if (!cell.InBounds(map))
            {
                continue;
            }

            ClearCellForCache(map, cell);
            bool interior = IsInteriorCell(room, cell, profile.layoutShape);
            bool isWall = IsWallCell(room, cell, profile.layoutShape);
            if (!interior && !isWall)
            {
                continue;
            }

            map.terrainGrid.SetTerrain(cell, floorDef);

            if (isWall)
            {
                Thing wall = ThingMaker.MakeThing(wallDef, wallStuff);
                GenSpawn.Spawn(wall, cell, map);
            }
        }

        PlaceDoorway(map, new IntVec3(room.CenterCell.x, 0, room.minZ), profile, wallStuff);
        ScatterDressing(map, room, profile);
    }

    private List<CellRect> BuildRoomModules(Map map, CellRect towerRoom, ArcaneSiteProfileDef profile, ThingDef wallStuff)
    {
        List<CellRect> rooms = new();
        if (profile.roomModules.NullOrEmpty())
        {
            return rooms;
        }

        foreach (ArcaneSiteRoomModuleDef module in profile.roomModules)
        {
            if (module == null)
            {
                continue;
            }

            CellRect room = ResolveModuleRoom(towerRoom, module).ClipInsideMap(map);
            BuildRectRoom(map, room, profile, wallStuff);
            ConnectModuleRoom(map, towerRoom, room, module.axis, profile, wallStuff);
            PlaceExteriorModuleDoor(map, room, module, profile, wallStuff);
            DressModuleRoom(map, room, module, wallStuff);
            rooms.Add(room);
        }

        return rooms;
    }

    private static CellRect ResolveModuleRoom(CellRect towerRoom, ArcaneSiteRoomModuleDef module)
    {
        int gap = Math.Max(0, module.distanceFromTower);
        int width = Math.Max(5, module.width);
        int depth = Math.Max(4, module.depth);

        switch (module.axis)
        {
            case ArcaneSiteAxis.East:
                return new CellRect(towerRoom.maxX + 1 + gap, towerRoom.CenterCell.z - (width / 2), depth, width);
            case ArcaneSiteAxis.South:
                return new CellRect(towerRoom.CenterCell.x - (width / 2), towerRoom.minZ - depth - gap, width, depth);
            case ArcaneSiteAxis.West:
                return new CellRect(towerRoom.minX - depth - gap, towerRoom.CenterCell.z - (width / 2), depth, width);
            default:
                return new CellRect(towerRoom.CenterCell.x - (width / 2), towerRoom.maxZ + 1 + gap, width, depth);
        }
    }

    private void BuildRectRoom(Map map, CellRect room, ArcaneSiteProfileDef profile, ThingDef wallStuff)
    {
        TerrainDef floorDef = profile.floorDef ?? DefDatabase<TerrainDef>.GetNamedSilentFail("TileSandstone") ?? TerrainDefOf.Concrete;
        ThingDef wallDef = ThingDefOf.Wall;

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
                GenSpawn.Spawn(ThingMaker.MakeThing(wallDef, wallStuff), cell, map);
            }
        }
    }

    private static void ConnectModuleRoom(Map map, CellRect towerRoom, CellRect moduleRoom, ArcaneSiteAxis axis, ArcaneSiteProfileDef profile, ThingDef wallStuff)
    {
        IntVec3 towerDoor;
        IntVec3 moduleDoor;
        switch (axis)
        {
            case ArcaneSiteAxis.East:
                towerDoor = new IntVec3(towerRoom.maxX, 0, towerRoom.CenterCell.z);
                moduleDoor = new IntVec3(moduleRoom.minX, 0, moduleRoom.CenterCell.z);
                break;
            case ArcaneSiteAxis.South:
                towerDoor = new IntVec3(towerRoom.CenterCell.x, 0, towerRoom.minZ);
                moduleDoor = new IntVec3(moduleRoom.CenterCell.x, 0, moduleRoom.maxZ);
                break;
            case ArcaneSiteAxis.West:
                towerDoor = new IntVec3(towerRoom.minX, 0, towerRoom.CenterCell.z);
                moduleDoor = new IntVec3(moduleRoom.maxX, 0, moduleRoom.CenterCell.z);
                break;
            default:
                towerDoor = new IntVec3(towerRoom.CenterCell.x, 0, towerRoom.maxZ);
                moduleDoor = new IntVec3(moduleRoom.CenterCell.x, 0, moduleRoom.minZ);
                break;
        }

        ClearWallAt(map, towerDoor);
        PlaceDoorway(map, moduleDoor, profile, wallStuff);
    }

    private static void PlaceExteriorModuleDoor(Map map, CellRect moduleRoom, ArcaneSiteRoomModuleDef module, ArcaneSiteProfileDef profile, ThingDef wallStuff)
    {
        if (module.axis != ArcaneSiteAxis.South || module.kind != ArcaneSiteRoomKind.Antechamber)
        {
            return;
        }

        PlaceDoorway(map, new IntVec3(moduleRoom.CenterCell.x, 0, moduleRoom.minZ), profile, wallStuff);
    }

    private static void ClearWallAt(Map map, IntVec3 cell)
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

    private void DressModuleRoom(Map map, CellRect room, ArcaneSiteRoomModuleDef module, ThingDef wallStuff)
    {
        switch (module.kind)
        {
            case ArcaneSiteRoomKind.Bedroom:
                TrySpawnFurniture(map, new IntVec3(room.CenterCell.x, 0, room.maxZ - 2), DefDatabase<ThingDef>.GetNamedSilentFail("RoyalBed") ?? DefDatabase<ThingDef>.GetNamedSilentFail("Bed"), ThingDefOf.WoodLog, Rot4.South);
                TrySpawnFurniture(map, new IntVec3(room.minX + 1, 0, room.minZ + 1), DefDatabase<ThingDef>.GetNamedSilentFail("Dresser"), ThingDefOf.WoodLog, Rot4.North);
                TrySpawnFurniture(map, new IntVec3(room.maxX - 1, 0, room.minZ + 1), DefDatabase<ThingDef>.GetNamedSilentFail("EndTable"), ThingDefOf.WoodLog, Rot4.North);
                TrySpawnFurniture(map, new IntVec3(room.minX + 1, 0, room.maxZ - 1), DefDatabase<ThingDef>.GetNamedSilentFail("EndTable"), ThingDefOf.WoodLog, Rot4.North);
                break;
            case ArcaneSiteRoomKind.ServantsQuarters:
                TrySpawnFurniture(map, new IntVec3(room.CenterCell.x, 0, room.minZ + 2), DefDatabase<ThingDef>.GetNamedSilentFail("Bed"), ThingDefOf.WoodLog, Rot4.North);
                TrySpawnFurniture(map, new IntVec3(room.CenterCell.x, 0, room.maxZ - 2), DefDatabase<ThingDef>.GetNamedSilentFail("Bed"), ThingDefOf.WoodLog, Rot4.South);
                TrySpawnFurniture(map, new IntVec3(room.minX + 1, 0, room.CenterCell.z), DefDatabase<ThingDef>.GetNamedSilentFail("Dresser"), ThingDefOf.WoodLog, Rot4.North);
                TrySpawnFurniture(map, new IntVec3(room.maxX - 1, 0, room.CenterCell.z), DefDatabase<ThingDef>.GetNamedSilentFail("EndTable"), ThingDefOf.WoodLog, Rot4.North);
                break;
            case ArcaneSiteRoomKind.Storage:
                TrySpawnFurniture(map, new IntVec3(room.minX + 1, 0, room.maxZ - 1), DefDatabase<ThingDef>.GetNamedSilentFail("Shelf"), ThingDefOf.WoodLog, Rot4.East);
                TrySpawnFurniture(map, new IntVec3(room.maxX - 1, 0, room.maxZ - 1), DefDatabase<ThingDef>.GetNamedSilentFail("ShelfSmall") ?? DefDatabase<ThingDef>.GetNamedSilentFail("Shelf"), ThingDefOf.WoodLog, Rot4.West);
                TrySpawnItemStack(map, new IntVec3(room.minX + 2, 0, room.maxZ - 1), "MFV_Papyrus", 8);
                TrySpawnItemStack(map, new IntVec3(room.maxX - 2, 0, room.maxZ - 1), "Cloth", 15);
                TrySpawnItemStack(map, new IntVec3(room.minX + 2, 0, room.maxZ - 2), "MFV_ExoticHerbs", 6);
                TrySpawnItemStack(map, new IntVec3(room.maxX - 2, 0, room.maxZ - 2), "MedicineHerbal", 3);
                TrySpawnFurniture(map, room.CenterCell, DefDatabase<ThingDef>.GetNamedSilentFail("ChunkMarble") ?? DefDatabase<ThingDef>.GetNamedSilentFail("ChunkSandstone"), null, Rot4.North);
                TrySpawnFurniture(map, new IntVec3(room.CenterCell.x + 1, 0, room.CenterCell.z - 1), DefDatabase<ThingDef>.GetNamedSilentFail("ChunkGranite"), null, Rot4.North);
                break;
            case ArcaneSiteRoomKind.Antechamber:
                TrySpawnFurniture(map, new IntVec3(room.minX + 1, 0, room.minZ + 1), DefDatabase<ThingDef>.GetNamedSilentFail("Column"), wallStuff, Rot4.North);
                TrySpawnFurniture(map, new IntVec3(room.maxX - 1, 0, room.minZ + 1), DefDatabase<ThingDef>.GetNamedSilentFail("Column"), wallStuff, Rot4.North);
                break;
        }
    }

    private static void TrySpawnFurniture(Map map, IntVec3 cell, ThingDef thingDef, ThingDef stuff, Rot4 rotation)
    {
        if (thingDef == null || !cell.InBounds(map) || !cell.Standable(map))
        {
            return;
        }

        Thing thing = ThingMaker.MakeThing(thingDef, stuff);
        thing.Rotation = rotation;
        ClearCellForCache(map, cell);
        GenSpawn.Spawn(thing, cell, map);
    }

    private static void TrySpawnItemStack(Map map, IntVec3 cell, string thingDefName, int count)
    {
        ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(thingDefName);
        if (thingDef == null || !cell.InBounds(map) || !cell.Standable(map))
        {
            return;
        }

        ClearCellForCache(map, cell);
        Thing thing = ThingMaker.MakeThing(thingDef);
        thing.stackCount = Math.Min(Math.Max(1, count), thingDef.stackLimit);
        GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
    }

    private void BuildExteriorModules(Map map, CellRect towerRoom, ArcaneSiteProfileDef profile, ThingDef wallStuff)
    {
        if (profile.addEntryPath)
        {
            BuildEntryPath(map, towerRoom, profile);
        }

        if (profile.addExteriorRuin)
        {
            ScatterExteriorRuin(map, towerRoom, profile, wallStuff);
        }
    }

    private void BuildEntryPath(Map map, CellRect towerRoom, ArcaneSiteProfileDef profile)
    {
        TerrainDef floorDef = profile.floorDef ?? DefDatabase<TerrainDef>.GetNamedSilentFail("TileSandstone") ?? TerrainDefOf.Concrete;
        CellRect entrySource = ResolveEntrySourceRoom(towerRoom, profile);
        int length = Math.Max(1, profile.entryPathLength);
        for (int i = 1; i <= length; i++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                IntVec3 cell = new(entrySource.CenterCell.x + dx, 0, entrySource.minZ - i);
                if (!cell.InBounds(map))
                {
                    continue;
                }

                ClearCellForCache(map, cell);
                map.terrainGrid.SetTerrain(cell, floorDef);
            }
        }
    }

    private static CellRect ResolveEntrySourceRoom(CellRect towerRoom, ArcaneSiteProfileDef profile)
    {
        if (!profile.roomModules.NullOrEmpty())
        {
            foreach (ArcaneSiteRoomModuleDef module in profile.roomModules)
            {
                if (module?.axis == ArcaneSiteAxis.South && module.kind == ArcaneSiteRoomKind.Antechamber)
                {
                    return ResolveModuleRoom(towerRoom, module);
                }
            }
        }

        return towerRoom;
    }

    private void ScatterExteriorRuin(Map map, CellRect towerRoom, ArcaneSiteProfileDef profile, ThingDef wallStuff)
    {
        int count = Math.Max(0, profile.exteriorRuinCount);
        ThingDef wallDef = ThingDefOf.Wall;
        ThingDef chunkDef = DefDatabase<ThingDef>.GetNamedSilentFail("ChunkSandstone");
        for (int i = 0; i < count; i++)
        {
            IntVec3 cell = ExteriorRuinCell(towerRoom, i);
            if (!cell.InBounds(map) || !cell.Standable(map))
            {
                continue;
            }

            ClearCellForCache(map, cell);
            if (i % 3 == 0)
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(wallDef, wallStuff), cell, map);
            }
            else if (chunkDef != null)
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(chunkDef), cell, map);
            }
        }
    }

    private static IntVec3 ExteriorRuinCell(CellRect towerRoom, int index)
    {
        int ring = 3 + (index % 4);
        int side = index % 4;
        int offset = ((index / 4) % 5) - 2;
        switch (side)
        {
            case 0:
                return new IntVec3(towerRoom.CenterCell.x + offset, 0, towerRoom.maxZ + ring);
            case 1:
                return new IntVec3(towerRoom.maxX + ring, 0, towerRoom.CenterCell.z + offset);
            case 2:
                return new IntVec3(towerRoom.CenterCell.x + offset, 0, towerRoom.minZ - ring);
            default:
                return new IntVec3(towerRoom.minX - ring, 0, towerRoom.CenterCell.z + offset);
        }
    }

    private static void PlaceDoorway(Map map, IntVec3 cell, ArcaneSiteProfileDef profile, ThingDef wallStuff)
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

        ThingDef doorDef = profile.doorDef ?? ThingDefOf.Door;
        if (doorDef != null)
        {
            GenSpawn.Spawn(ThingMaker.MakeThing(doorDef, wallStuff), cell, map);
        }
    }

    private static void ClearCellForCache(Map map, IntVec3 cell)
    {
        List<Thing> things = cell.GetThingList(map);
        for (int i = things.Count - 1; i >= 0; i--)
        {
            Thing thing = things[i];
            if (thing == null || thing.def.category == ThingCategory.Filth || thing.def.category == ThingCategory.Plant || ShouldClearForGeneratedSite(thing))
            {
                thing?.Destroy();
            }
        }
    }

    private static bool ShouldClearForGeneratedSite(Thing thing)
    {
        ThingDef def = thing?.def;
        if (def == null)
        {
            return false;
        }

        return def.mineable || def.building?.isNaturalRock == true;
    }

    private Thing SpawnCacheChest(Map map, IntVec3 cell, ArcaneSiteProfileDef profile)
    {
        ThingDef chestDef = profile.chestThingDef
            ?? DefDatabase<ThingDef>.GetNamedSilentFail(chestThingDef)
            ?? DefDatabase<ThingDef>.GetNamedSilentFail("MFV_ArcaneTreasureChest");
        if (chestDef == null)
        {
            Log.Warning("[MFVanilla] Arcane cache site could not resolve an arcane treasure chest ThingDef.");
            return null;
        }

        Thing chest = ThingMaker.MakeThing(chestDef);
        return GenSpawn.Spawn(chest, cell, map);
    }

    private void ScatterDressing(Map map, CellRect room, ArcaneSiteProfileDef profile)
    {
        if (profile.dressing.NullOrEmpty())
        {
            TrySpawnDressing(map, room, DefDatabase<ThingDef>.GetNamedSilentFail("MFV_MagicTorchLamp"), new IntVec3(room.minX + 2, 0, room.minZ + 2));
            TrySpawnDressing(map, room, DefDatabase<ThingDef>.GetNamedSilentFail("MFV_MagicTorchLamp"), new IntVec3(room.maxX - 2, 0, room.minZ + 2));
            TrySpawnDressing(map, room, DefDatabase<ThingDef>.GetNamedSilentFail("MFV_ArcaneSpire"), new IntVec3(room.minX + 2, 0, room.maxZ - 2));
            TrySpawnDressing(map, room, DefDatabase<ThingDef>.GetNamedSilentFail("MFV_ArcaneSpire"), new IntVec3(room.maxX - 2, 0, room.maxZ - 2));
            return;
        }

        foreach (ArcaneSiteDressingDef entry in profile.dressing)
        {
            if (entry?.thingDef == null)
            {
                continue;
            }

            TrySpawnDressing(map, room, entry.thingDef, new IntVec3(room.minX + entry.offsetX, 0, room.minZ + entry.offsetZ));
        }
    }

    private static void TrySpawnDressing(Map map, CellRect room, ThingDef thingDef, IntVec3 cell)
    {
        if (!cell.InBounds(map) || !room.Contains(cell))
        {
            return;
        }

        if (thingDef == null)
        {
            return;
        }

        GenSpawn.Spawn(ThingMaker.MakeThing(thingDef), cell, map);
    }

    private int ResolveDefenderCount(GenStepParams parms, ArcaneSiteProfileDef profile)
    {
        float points = parms.sitePart?.parms?.threatPoints ?? parms.sitePart?.parms?.points ?? 0f;
        int baseCount = Math.Max(1, profile.defenderCount);
        if (points >= 900f)
        {
            return Math.Max(baseCount + 2, 5);
        }

        if (points >= 500f)
        {
            return Math.Max(baseCount + 1, 4);
        }

        return baseCount;
    }

    private static List<Pawn> SpawnDefenders(Map map, CellRect room, int count, ArcaneSiteProfileDef profile)
    {
        List<Pawn> spawned = new();
        Faction faction = Faction.OfMechanoids;
        List<PawnKindDef> candidates = profile.defenderPawnKinds;
        if (candidates.NullOrEmpty())
        {
            candidates = new List<PawnKindDef>
            {
                PawnKindDefOf.Mech_Scyther,
                DefDatabase<PawnKindDef>.GetNamedSilentFail("Mech_Lancer"),
                DefDatabase<PawnKindDef>.GetNamedSilentFail("Mech_Pikeman")
            };
        }

        List<IntVec3> spawnCells = CandidateDefenderCells(room, map, profile.layoutShape);
        for (int i = 0; i < count; i++)
        {
            PawnKindDef pawnKindDef = candidates[i % candidates.Count];
            if (pawnKindDef == null || spawnCells.Count == 0)
            {
                continue;
            }

            IntVec3 cell = spawnCells[i % spawnCells.Count];
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, faction));
            spawned.Add((Pawn)GenSpawn.Spawn(pawn, cell, map));
        }

        return spawned;
    }

    private static List<IntVec3> CandidateDefenderCells(CellRect room, Map map, ArcaneSiteLayoutShape layoutShape)
    {
        List<IntVec3> cells = new();
        CellRect inner = room.ContractedBy(2);
        foreach (IntVec3 cell in inner.Cells)
        {
            if (cell.InBounds(map) && cell.Standable(map) && cell != room.CenterCell && IsInteriorCell(room, cell, layoutShape))
                {
                    cells.Add(cell);
                }
        }

        cells.SortBy(cell => cell.DistanceToSquared(room.CenterCell));
        return cells;
    }

    private static void LogDevGeneration(Map map, GenStepParams parms, ArcaneSiteProfileDef profile, int siteSeed, CellRect room, List<CellRect> moduleRooms, Thing chest, List<Pawn> defenders)
    {
        if (!Prefs.DevMode)
        {
            return;
        }

        float threatPoints = parms.sitePart?.parms?.threatPoints ?? parms.sitePart?.parms?.points ?? 0f;
        string chestId = chest?.TryGetComp<CompUseEffect_OpenArcaneTreasure>()?.StableChestId ?? "none";
        StringBuilder defenderSummary = new();
        if (!defenders.NullOrEmpty())
        {
            for (int i = 0; i < defenders.Count; i++)
            {
                Pawn defender = defenders[i];
                if (defender == null)
                {
                    continue;
                }

                if (defenderSummary.Length > 0)
                {
                    defenderSummary.Append(", ");
                }

                defenderSummary.Append(defender.kindDef?.defName ?? defender.def?.defName ?? "unknown");
                defenderSummary.Append('@');
                defenderSummary.Append(defender.Position.ToString());
            }
        }

        Log.Message(
            "[MFVanilla] Generated arcane cache site: " +
            $"profile={profile?.defName ?? "fallback"}, seed={siteSeed:X8}, tile={map.Tile}, threatPoints={threatPoints}, " +
            $"room={room.Width}x{room.Height}@{room.CenterCell}, modules={moduleRooms?.Count ?? 0}, chest={chest?.def?.defName ?? "none"}@{chest?.Position.ToString() ?? "none"}, " +
            $"chestId={chestId}, defenders={defenders?.Count ?? 0} [{defenderSummary}]");
    }

    private static bool IsInteriorCell(CellRect room, IntVec3 cell, ArcaneSiteLayoutShape layoutShape)
    {
        if (!room.Contains(cell))
        {
            return false;
        }

        if (layoutShape == ArcaneSiteLayoutShape.Circle)
        {
            float innerRadius = CircleWallRadius(room) - 1f;
            return DistanceSquaredFromRoomCenter(room, cell) <= innerRadius * innerRadius;
        }

        return cell.x > room.minX && cell.x < room.maxX && cell.z > room.minZ && cell.z < room.maxZ;
    }

    private static bool IsWallCell(CellRect room, IntVec3 cell, ArcaneSiteLayoutShape layoutShape)
    {
        if (!room.Contains(cell))
        {
            return false;
        }

        if (layoutShape == ArcaneSiteLayoutShape.Circle)
        {
            float distanceSquared = DistanceSquaredFromRoomCenter(room, cell);
            float wallRadius = CircleWallRadius(room);
            float innerRadius = wallRadius - 1f;
            float outerRadius = wallRadius + 0.75f;
            return distanceSquared > innerRadius * innerRadius && distanceSquared <= outerRadius * outerRadius;
        }

        return cell.x == room.minX || cell.x == room.maxX || cell.z == room.minZ || cell.z == room.maxZ;
    }

    private static float DistanceSquaredFromRoomCenter(CellRect room, IntVec3 cell)
    {
        float dx = cell.x - room.CenterCell.x;
        float dz = cell.z - room.CenterCell.z;
        return (dx * dx) + (dz * dz);
    }

    private static float CircleWallRadius(CellRect room)
    {
        return (Math.Min(room.Width, room.Height) - 1f) / 2f;
    }

    private static ThingDef ResolveWallStuff(int siteSeed, ArcaneSiteProfileDef profile, bool tower)
    {
        List<ThingDef> options = tower && !profile.towerWallStuffOptions.NullOrEmpty()
            ? profile.towerWallStuffOptions
            : profile.wallStuffOptions;
        if (!options.NullOrEmpty())
        {
            int index = StableIndex(siteSeed, profile.shortHash ^ (tower ? 7919 : 0), options.Count);
            ThingDef selected = options[index];
            if (selected != null)
            {
                return selected;
            }
        }

        if (tower && profile.towerWallStuff != null)
        {
            return profile.towerWallStuff;
        }

        return profile.wallStuff ?? DefDatabase<ThingDef>.GetNamedSilentFail("BlocksSandstone") ?? ThingDefOf.Steel;
    }

    private static int ResolveSiteSeed(Map map, GenStepParams parms, ArcaneSiteProfileDef profile)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ (profile?.shortHash ?? 0);
            hash = (hash * 397) ^ (map?.Tile ?? -1);
            if (parms.sitePart?.site != null)
            {
                hash = (hash * 397) ^ parms.sitePart.site.ID;
                hash = (hash * 397) ^ parms.sitePart.site.creationGameTicks;
            }

            hash = (hash * 397) ^ (parms.sitePart?.def?.shortHash ?? 0);
            return hash;
        }
    }

    private static int StableIndex(int tile, int profileHash, int count)
    {
        if (count <= 1)
        {
            return 0;
        }

        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ tile;
            hash = (hash * 397) ^ profileHash;
            return (hash & int.MaxValue) % count;
        }
    }

    private static float StableRange(int siteSeed, int salt, float max)
    {
        if (max <= 0f)
        {
            return 0f;
        }

        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ siteSeed;
            hash = (hash * 397) ^ salt;
            int positive = hash & int.MaxValue;
            return (positive / (float)int.MaxValue) * max;
        }
    }
}
