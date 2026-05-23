using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MFVanilla.Core;

public sealed class SitePartWorker_PlanarPocket : SitePartWorker
{
}

public sealed class Building_PlanarGate : Building
{
}

public sealed class CompProperties_PlanarGate : CompProperties
{
    public string sitePartDefName = PlanarMagicUtility.PlanarPocketSitePartDefName;
    public int minSiteDistance = 1;
    public int maxSiteDistance = 4;
    public int activationRadius = 5;

    public CompProperties_PlanarGate()
    {
        compClass = typeof(CompPlanarGate);
    }
}

public sealed class CompPlanarGate : ThingComp
{
    private int siteId = -1;

    private CompProperties_PlanarGate Props => (CompProperties_PlanarGate)props;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        if (parent?.Faction != Faction.OfPlayer)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "Open planar pocket",
            defaultDesc = "Stabilize and view this gate's planar pocket.",
            icon = ContentFinder<Texture2D>.Get("Things/Building/PlanarGate/PlanarGate-removebg-preview", false),
            action = OpenPlanarPocket
        };

        yield return new Command_Action
        {
            defaultLabel = "Send selected through gate",
            defaultDesc = $"Send selected player-controlled pawns within {Props.activationRadius} cells through this planar gate.",
            icon = ContentFinder<Texture2D>.Get("Things/Building/PlanarGate/PlanarGate-removebg-preview", false),
            action = TraverseSelectedPawns
        };

        if (Prefs.DevMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "Reset planar pocket link",
                defaultDesc = "Forget this gate's current planar pocket site so the next use creates a fresh pocket map.",
                icon = ContentFinder<Texture2D>.Get("Things/Building/PlanarGate/PlanarGate-removebg-preview", false),
                action = ResetPlanarPocketLink
            };
        }
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
    {
        foreach (FloatMenuOption option in base.CompFloatMenuOptions(pawn))
        {
            yield return option;
        }

        if (!CanPawnUseGate(pawn, out string failReason))
        {
            yield return new FloatMenuOption($"Use planar gate ({failReason})", null);
            yield break;
        }

        yield return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption("Use planar gate", () =>
            {
                StartTraversalJobs(new List<Pawn> { pawn });
            }),
            pawn,
            parent);
    }

    private void OpenPlanarPocket()
    {
        Map map = GetOrCreatePlanarPocketMap();
        if (map == null)
        {
            Messages.Message("The planar gate could not stabilize a pocket map.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        Current.Game.CurrentMap = map;
        Messages.Message("The planar pocket is stable.", MessageTypeDefOf.PositiveEvent, false);
    }

    private void TraverseSelectedPawns()
    {
        List<Pawn> pawns = new();
        List<object> selectedObjects = Find.Selector?.SelectedObjects;
        if (selectedObjects != null)
        {
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                if (selectedObjects[i] is Pawn pawn && CanPawnUseGate(pawn, out _) && pawn.Position.DistanceTo(parent.Position) <= Props.activationRadius)
                {
                    pawns.Add(pawn);
                }
            }
        }

        if (pawns.Count == 0)
        {
            Messages.Message($"Select one or more player-controlled pawns within {Props.activationRadius} cells of the gate.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        StartTraversalJobs(pawns);
    }

    private void ResetPlanarPocketLink()
    {
        siteId = -1;
        Messages.Message("The planar gate's pocket link has been reset.", MessageTypeDefOf.NeutralEvent, false);
    }

    public bool TryTraversePawn(Pawn pawn)
    {
        if (!CanPawnUseGate(pawn, out string failReason))
        {
            Messages.Message($"Cannot use planar gate: {failReason}.", MessageTypeDefOf.RejectInput, false);
            return false;
        }

        Map destinationMap = GetOrCreatePlanarPocketMap();
        if (destinationMap == null)
        {
            Messages.Message("The planar gate could not stabilize a pocket map.", MessageTypeDefOf.RejectInput, false);
            return false;
        }

        IntVec3 arrivalCenter = new(destinationMap.Size.x / 2, 0, destinationMap.Size.z / 2);
        int moved = PlanarMagicUtility.TransferPawnsThroughGate(new List<Pawn> { pawn }, destinationMap, arrivalCenter);
        if (moved <= 0)
        {
            return false;
        }

        Current.Game.CurrentMap = destinationMap;
        Find.Selector.ClearSelection();
        Find.Selector.Select(pawn);
        CameraJumper.TryJump(pawn);
        Messages.Message($"{pawn.LabelShortCap} steps through the planar gate.", MessageTypeDefOf.PositiveEvent, false);
        return true;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref siteId, "siteId", -1);
    }

    private int StartTraversalJobs(List<Pawn> pawns)
    {
        if (pawns.NullOrEmpty())
        {
            return 0;
        }

        JobDef jobDef = DefDatabase<JobDef>.GetNamedSilentFail("MFV_UsePlanarGate");
        if (jobDef == null)
        {
            Messages.Message("The planar gate traversal job is missing.", MessageTypeDefOf.RejectInput, false);
            return 0;
        }

        int started = 0;
        for (int i = 0; i < pawns.Count; i++)
        {
            Pawn pawn = pawns[i];
            if (!CanPawnUseGate(pawn, out _))
            {
                continue;
            }

            Job job = JobMaker.MakeJob(jobDef, parent);
            job.playerForced = true;
            pawn.jobs.TryTakeOrderedJob(job);
            started++;
        }

        if (started > 0)
        {
            Messages.Message($"Sent {started} pawn(s) to enter the planar gate.", MessageTypeDefOf.PositiveEvent, false);
        }

        return started;
    }

    private Map GetOrCreatePlanarPocketMap()
    {
        Site site = PlanarMagicUtility.FindSiteById(siteId);
        if (site == null)
        {
            if (!PlanarMagicUtility.TryCreatePlanarPocketSite(parent.Map, Props.sitePartDefName, Props.minSiteDistance, Props.maxSiteDistance, selectSite: false, out site))
            {
                return null;
            }

            siteId = site.ID;
        }

        Map map = PlanarMagicUtility.GetOrGenerateSiteMap(site);
        PlanarMagicUtility.EnsurePlanarPocketReady(map);
        return map;
    }

    private bool CanPawnUseGate(Pawn pawn, out string failReason)
    {
        failReason = null;
        if (pawn == null || parent?.Map == null)
        {
            failReason = "invalid pawn or gate";
            return false;
        }

        if (pawn.Faction != Faction.OfPlayer && !pawn.IsPrisonerOfColony)
        {
            failReason = "not player-controlled";
            return false;
        }

        if (!pawn.Spawned || pawn.Map != parent.Map)
        {
            failReason = "not on this map";
            return false;
        }

        if (pawn.Downed)
        {
            failReason = "pawn is downed";
            return false;
        }

        if (!pawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
        {
            failReason = "no reachable path";
            return false;
        }

        return true;
    }
}

public static class PlanarMagicUtility
{
    public const string PlanarPocketSitePartDefName = "MFV_PlanarPocket";
    public const string PlanarPocketMapGeneratorDefName = "MFV_PlanarPocketMap";
    public const int PlanarPocketMapSize = 120;

    public static bool TryCreatePlanarPocketSite(
        Map originMap,
        string sitePartDefName,
        int minDistance,
        int maxDistance,
        bool selectSite,
        out Site site)
    {
        site = null;
        if (originMap == null || originMap.Tile < 0)
        {
            return false;
        }

        SitePartDef sitePartDef = DefDatabase<SitePartDef>.GetNamedSilentFail(sitePartDefName);
        if (sitePartDef == null)
        {
            Log.Warning($"[MFVanilla] Could not create planar pocket because {sitePartDefName} SitePartDef was not found.");
            return false;
        }

        if (!TileFinder.TryFindNewSiteTile(out PlanetTile tile, originMap.Tile, Math.Max(1, minDistance), Math.Max(minDistance, maxDistance)))
        {
            return false;
        }

        site = SiteMaker.MakeSite(sitePartDef, tile, null, ifHostileThenMustRemainHostile: false, threatPoints: 0f);
        if (site == null || site.parts.NullOrEmpty())
        {
            return false;
        }

        Find.WorldObjects.Add(site);
        if (selectSite)
        {
            Find.WorldSelector.ClearSelection();
            Find.WorldSelector.Select(site);
        }

        if (Prefs.DevMode)
        {
            Log.Message($"[MFVanilla] Created planar pocket site at tile {tile} from map tile {originMap.Tile}.");
        }

        return true;
    }

    public static Site FindSiteById(int siteId)
    {
        if (siteId < 0)
        {
            return null;
        }

        List<WorldObject> worldObjects = Find.WorldObjects?.AllWorldObjects;
        if (worldObjects == null)
        {
            return null;
        }

        for (int i = 0; i < worldObjects.Count; i++)
        {
            if (worldObjects[i] is Site site && site.ID == siteId)
            {
                return site;
            }
        }

        return null;
    }

    public static Map GetOrGenerateSiteMap(Site site)
    {
        if (site == null)
        {
            return null;
        }

        if (site.HasMap)
        {
            return site.Map;
        }

        IntVec3 mapSize = new(PlanarPocketMapSize, 1, PlanarPocketMapSize);
        MapGeneratorDef mapGeneratorDef = DefDatabase<MapGeneratorDef>.GetNamedSilentFail(PlanarPocketMapGeneratorDefName);
        IEnumerable<GenStepWithParams> extraGenSteps = null;
        if (mapGeneratorDef == null)
        {
            mapGeneratorDef = site.MapGeneratorDef ?? RimWorld.MapGeneratorDefOf.Encounter;
            extraGenSteps = site.ExtraGenStepDefs;
            Log.Warning($"[MFVanilla] {PlanarPocketMapGeneratorDefName} was not found; falling back to {mapGeneratorDef.defName}.");
        }

        try
        {
            Map map = MapGenerator.GenerateMap(mapSize, site, mapGeneratorDef, extraGenSteps, null, false, false);
            EnsurePlanarPocketReady(map);
            return map;
        }
        catch (Exception ex)
        {
            Log.Error($"[MFVanilla] Failed to generate planar pocket map for site {site.ID}: {ex}");
            return null;
        }
    }

    public static void EnsurePlanarPocketReady(Map map)
    {
        if (map == null || !IsPlanarPocketMap(map))
        {
            return;
        }

        List<TerrainDef> terrains = ResolvePlanarTerrains();
        TerrainDef fallbackTerrain = terrains.Count > 0 ? terrains[0] : TerrainDefOf.Soil;
        bool hasPlanarTerrain = false;
        int checkedCells = 0;
        foreach (IntVec3 cell in map.AllCells)
        {
            TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
            if (terrain != null && terrain.defName != null && terrain.defName.StartsWith("MFV_Planar", StringComparison.Ordinal))
            {
                hasPlanarTerrain = true;
                break;
            }

            checkedCells++;
            if (checkedCells >= 256)
            {
                break;
            }
        }

        if (!hasPlanarTerrain)
        {
            PaintPlanarTerrain(map, terrains, map.Tile ^ 0x25F08A31);
        }

        int mapArea = Math.Max(1, map.Size.x * map.Size.z);
        int plantCount = CountThingsWithDefPrefix(map, "Plant_MFV_Planar");
        if (plantCount < Math.Max(18, mapArea / 125))
        {
            ScatterPlanarPlants(map, map.Tile ^ 0x7156A31B, Math.Max(18, mapArea / 95) - plantCount);
        }

        int mineableCount = CountThingsWithDefNames(map, PlanarMineableDefNames);
        if (mineableCount < Math.Max(8, mapArea / 550))
        {
            ScatterPlanarMineables(map, map.Tile ^ 0x42198E71, Math.Max(8, mapArea / 460) - mineableCount);
        }

        int chunkCount = CountThingsWithDefNames(map, PlanarChunkDefNames);
        if (chunkCount < Math.Max(10, mapArea / 450))
        {
            ScatterPlanarStoneChunks(map, map.Tile ^ 0x31B4D2F1, Math.Max(10, mapArea / 360) - chunkCount);
        }

        FloodFillerFog.FloodUnfog(new IntVec3(map.Size.x / 2, 0, map.Size.z / 2), map);
    }

    private static readonly string[] PlanarChunkDefNames =
    {
        "MFV_ChunkPhaseStone",
        "MFV_ChunkVoidglass"
    };

    private static readonly string[] PlanarMineableDefNames =
    {
        "MFV_PhaseStone",
        "MFV_Voidglass"
    };

    public static List<TerrainDef> ResolvePlanarTerrains()
    {
        List<TerrainDef> terrains = new();
        for (char suffix = 'A'; suffix <= 'D'; suffix++)
        {
            TerrainDef terrainDef = DefDatabase<TerrainDef>.GetNamedSilentFail($"MFV_PlanarTile{suffix}");
            if (terrainDef != null)
            {
                terrains.Add(terrainDef);
            }
        }

        if (terrains.Count == 0)
        {
            terrains.Add(TerrainDefOf.Soil);
        }

        return terrains;
    }

    public static List<ThingDef> ResolvePlanarPlants()
    {
        List<ThingDef> plants = new();
        for (char suffix = 'A'; suffix <= 'J'; suffix++)
        {
            ThingDef plantDef = DefDatabase<ThingDef>.GetNamedSilentFail($"Plant_MFV_Planar{suffix}");
            if (plantDef != null)
            {
                plants.Add(plantDef);
            }
        }

        return plants;
    }

    public static void PaintPlanarTerrain(Map map, List<TerrainDef> terrains, int seed)
    {
        if (map == null || terrains.NullOrEmpty())
        {
            return;
        }

        foreach (IntVec3 cell in map.AllCells)
        {
            if (!cell.InBounds(map))
            {
                continue;
            }

            int patchSalt = ((cell.x / 11) * 397) ^ ((cell.z / 11) * 7919);
            int rippleSalt = ((cell.x + cell.z) / 7) * 104729;
            TerrainDef terrain = terrains[StableIndex(seed, patchSalt ^ rippleSalt, terrains.Count)];
            map.terrainGrid.SetTerrain(cell, terrain);
        }
    }

    public static void ScatterPlanarPlants(Map map, int seed, int targetCount)
    {
        List<ThingDef> plants = ResolvePlanarPlants();
        if (map == null || plants.Count == 0 || targetCount <= 0)
        {
            return;
        }

        int spawned = 0;
        int attempts = Math.Max(200, targetCount * 20);
        for (int i = 0; i < attempts && spawned < targetCount; i++)
        {
            IntVec3 cell = RandomStableCell(map, seed, i);
            if (!CanScatterAt(map, cell))
            {
                continue;
            }

            ThingDef plantDef = plants[StableIndex(seed, i * 3917, plants.Count)];
            Thing thing = ThingMaker.MakeThing(plantDef);
            if (thing is Plant plant)
            {
                plant.Growth = 0.55f + (StableRange(seed, i * 13007) * 0.45f);
            }

            GenSpawn.Spawn(thing, cell, map);
            spawned++;
        }
    }

    public static void ScatterPlanarStoneChunks(Map map, int seed, int targetCount)
    {
        if (map == null || targetCount <= 0)
        {
            return;
        }

        List<ThingDef> chunks = new();
        for (int i = 0; i < PlanarChunkDefNames.Length; i++)
        {
            ThingDef chunkDef = DefDatabase<ThingDef>.GetNamedSilentFail(PlanarChunkDefNames[i]);
            if (chunkDef != null)
            {
                chunks.Add(chunkDef);
            }
        }

        if (chunks.Count == 0)
        {
            return;
        }

        int spawned = 0;
        int attempts = Math.Max(120, targetCount * 16);
        for (int i = 0; i < attempts && spawned < targetCount; i++)
        {
            IntVec3 cell = RandomStableCell(map, seed, i);
            if (!CanScatterAt(map, cell))
            {
                continue;
            }

            ThingDef chunkDef = chunks[StableIndex(seed, i * 7331, chunks.Count)];
            GenSpawn.Spawn(ThingMaker.MakeThing(chunkDef), cell, map);
            spawned++;
        }
    }

    public static void ScatterPlanarMineables(Map map, int seed, int targetCount)
    {
        if (map == null || targetCount <= 0)
        {
            return;
        }

        List<ThingDef> mineables = new();
        for (int i = 0; i < PlanarMineableDefNames.Length; i++)
        {
            ThingDef mineableDef = DefDatabase<ThingDef>.GetNamedSilentFail(PlanarMineableDefNames[i]);
            if (mineableDef != null)
            {
                mineables.Add(mineableDef);
            }
        }

        if (mineables.Count == 0)
        {
            return;
        }

        int spawned = 0;
        int attempts = Math.Max(140, targetCount * 20);
        for (int i = 0; i < attempts && spawned < targetCount; i++)
        {
            IntVec3 cell = RandomStableCell(map, seed, i);
            if (!CanScatterAt(map, cell))
            {
                continue;
            }

            ThingDef mineableDef = mineables[StableIndex(seed, i * 6151, mineables.Count)];
            GenSpawn.Spawn(ThingMaker.MakeThing(mineableDef), cell, map);
            spawned++;
        }
    }

    private static int CountThingsWithDefPrefix(Map map, string defNamePrefix)
    {
        if (map == null)
        {
            return 0;
        }

        int count = 0;
        foreach (IntVec3 cell in map.AllCells)
        {
            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i]?.def?.defName?.StartsWith(defNamePrefix, StringComparison.Ordinal) == true)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static int CountThingsWithDefNames(Map map, string[] defNames)
    {
        if (map == null || defNames.NullOrEmpty())
        {
            return 0;
        }

        int count = 0;
        foreach (IntVec3 cell in map.AllCells)
        {
            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                string defName = things[i]?.def?.defName;
                if (defName == null)
                {
                    continue;
                }

                for (int j = 0; j < defNames.Length; j++)
                {
                    if (defName == defNames[j])
                    {
                        count++;
                        break;
                    }
                }
            }
        }

        return count;
    }

    private static bool CanScatterAt(Map map, IntVec3 cell)
    {
        if (!cell.InBounds(map) || !cell.Standable(map))
        {
            return false;
        }

        List<Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            ThingDef def = things[i]?.def;
            if (def == null)
            {
                continue;
            }

            if (def.category == ThingCategory.Building || def.category == ThingCategory.Item || def.category == ThingCategory.Pawn || def.category == ThingCategory.Plant)
            {
                return false;
            }
        }

        return true;
    }

    private static IntVec3 RandomStableCell(Map map, int seed, int index)
    {
        int x = StableIndex(seed, index * 104729, map.Size.x);
        int z = StableIndex(seed, index * 13007, map.Size.z);
        return new IntVec3(x, 0, z);
    }

    private static int StableIndex(int seed, int salt, int count)
    {
        if (count <= 1)
        {
            return 0;
        }

        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ seed;
            hash = (hash * 397) ^ salt;
            return (hash & int.MaxValue) % count;
        }
    }

    private static float StableRange(int seed, int salt)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ seed;
            hash = (hash * 397) ^ salt;
            return (hash & int.MaxValue) / (float)int.MaxValue;
        }
    }

    public static bool IsPlanarPocketMap(Map map)
    {
        if (map?.Parent is not Site site || site.parts.NullOrEmpty())
        {
            return false;
        }

        for (int i = 0; i < site.parts.Count; i++)
        {
            if (site.parts[i]?.def?.defName == PlanarPocketSitePartDefName)
            {
                return true;
            }
        }

        return false;
    }

    public static Map FindReturnMap(Map pocketMap)
    {
        List<Map> maps = Find.Maps;
        if (maps != null)
        {
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map != null && map != pocketMap && !IsPlanarPocketMap(map) && FindPlanarGate(map) != null)
                {
                    return map;
                }
            }
        }

        return Current.Game?.AnyPlayerHomeMap;
    }

    public static Thing FindPlanarGate(Map map)
    {
        if (map == null)
        {
            return null;
        }

        ThingDef gateDef = DefDatabase<ThingDef>.GetNamedSilentFail("MFV_PlanarGate");
        if (gateDef == null)
        {
            return null;
        }

        List<Thing> things = map.listerThings.ThingsOfDef(gateDef);
        return things.NullOrEmpty() ? null : things[0];
    }

    public static Thing SpawnPlanarGateNearCenter(Map map)
    {
        ThingDef gateDef = DefDatabase<ThingDef>.GetNamedSilentFail("MFV_PlanarGate");
        if (map == null || gateDef == null)
        {
            return null;
        }

        IntVec3 center = new(map.Size.x / 2, 0, map.Size.z / 2);
        if (!CellFinder.TryRandomClosewalkCellNear(center, map, 6, out IntVec3 gateCell, c => c.Standable(map)))
        {
            gateCell = center.InBounds(map) ? center : CellFinder.RandomCell(map);
        }

        Thing gate = ThingMaker.MakeThing(gateDef);
        gate.SetFaction(Faction.OfPlayer);
        return GenSpawn.Spawn(gate, gateCell, map);
    }

    public static int TransferPawnsThroughGate(List<Pawn> pawns, Map destinationMap, IntVec3 destinationCenter)
    {
        if (pawns.NullOrEmpty() || destinationMap == null)
        {
            return 0;
        }

        int moved = 0;
        for (int i = 0; i < pawns.Count; i++)
        {
            Pawn pawn = pawns[i];
            if (pawn == null || pawn.Destroyed)
            {
                continue;
            }

            IntVec3 spawnCell = ResolveArrivalCell(destinationMap, destinationCenter, i);
            pawn.jobs?.StopAll(false, true);
            if (pawn.Spawned)
            {
                pawn.DeSpawn(DestroyMode.Vanish);
            }

            GenSpawn.Spawn(pawn, spawnCell, destinationMap);
            moved++;
        }

        return moved;
    }

    private static IntVec3 ResolveArrivalCell(Map map, IntVec3 center, int index)
    {
        if (CellFinder.TryRandomClosewalkCellNear(center, map, 4 + index, out IntVec3 cell, c => c.Standable(map)))
        {
            return cell;
        }

        if (CellFinder.TryFindRandomCell(map, c => c.Standable(map), out cell))
        {
            return cell;
        }

        return CellFinder.RandomCell(map);
    }
}

public sealed class JobDriver_UsePlanarGate : JobDriver
{
    private const TargetIndex GateInd = TargetIndex.A;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.GetTarget(GateInd), job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(GateInd);
        yield return Toils_Goto.GotoThing(GateInd, PathEndMode.Touch);
        yield return Toils_General.Wait(60, GateInd).WithProgressBarToilDelay(GateInd);
        yield return Toils_General.Do(() =>
        {
            CompPlanarGate gate = TargetThingA?.TryGetComp<CompPlanarGate>();
            gate?.TryTraversePawn(pawn);
        });
    }
}

public sealed class MapComponent_PlanarPocketRepair : MapComponent
{
    private bool repaired;

    public MapComponent_PlanarPocketRepair(Map map)
        : base(map)
    {
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        TryRepair();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref repaired, "repaired", false);
    }

    private void TryRepair()
    {
        if (repaired || !PlanarMagicUtility.IsPlanarPocketMap(map))
        {
            return;
        }

        PlanarMagicUtility.EnsurePlanarPocketReady(map);
        repaired = true;
    }
}

public sealed class GenStep_PlanarPocket : GenStep
{
    public List<TerrainDef> terrainOptions;
    public List<ThingDef> plantOptions;
    public ThingDef gateDef;
    public float plantDensity = 0.08f;
    public float chunkDensity = 0.018f;
    public float mineableDensity = 0.012f;

    public override int SeedPart => 740319227;

    public override void Generate(Map map, GenStepParams parms)
    {
        if (map == null)
        {
            return;
        }

        List<TerrainDef> terrains = ResolveTerrains();
        int seed = ResolveSeed(map, parms);

        foreach (IntVec3 cell in map.AllCells)
        {
            if (!cell.InBounds(map))
            {
                continue;
            }

            ClearPlanarCell(map, cell);
        }

        PlanarMagicUtility.PaintPlanarTerrain(map, terrains, seed);
        int mapArea = Math.Max(1, map.Size.x * map.Size.z);
        PlanarMagicUtility.ScatterPlanarPlants(map, seed ^ 0x6E61C457, Math.Max(20, (int)(mapArea * plantDensity)));
        PlanarMagicUtility.ScatterPlanarMineables(map, seed ^ 0x280E9C2D, Math.Max(6, (int)(mapArea * mineableDensity)));
        PlanarMagicUtility.ScatterPlanarStoneChunks(map, seed ^ 0x3F62B7A9, Math.Max(8, (int)(mapArea * chunkDensity)));
    }

    private List<TerrainDef> ResolveTerrains()
    {
        List<TerrainDef> terrains = terrainOptions?.FindAll(def => def != null);
        if (!terrains.NullOrEmpty())
        {
            return terrains;
        }

        terrains = new List<TerrainDef>
        {
            DefDatabase<TerrainDef>.GetNamedSilentFail("MFV_PlanarTileA"),
            DefDatabase<TerrainDef>.GetNamedSilentFail("MFV_PlanarTileB"),
            DefDatabase<TerrainDef>.GetNamedSilentFail("MFV_PlanarTileC"),
            DefDatabase<TerrainDef>.GetNamedSilentFail("MFV_PlanarTileD")
        };
        terrains.RemoveAll(def => def == null);
        if (terrains.Count == 0)
        {
            terrains.Add(TerrainDefOf.Soil);
        }

        return terrains;
    }

    private static void ClearPlanarCell(Map map, IntVec3 cell)
    {
        List<Thing> things = cell.GetThingList(map);
        for (int i = things.Count - 1; i >= 0; i--)
        {
            Thing thing = things[i];
            ThingDef def = thing?.def;
            if (def == null || def.category == ThingCategory.Filth || def.category == ThingCategory.Plant || def.mineable || def.building?.isNaturalRock == true)
            {
                thing?.Destroy();
            }
        }
    }

    private static int ResolveSeed(Map map, GenStepParams parms)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ (map?.Tile ?? -1);
            hash = (hash * 397) ^ (parms.sitePart?.site?.ID ?? 0);
            hash = (hash * 397) ^ (parms.sitePart?.def?.shortHash ?? 0);
            return hash;
        }
    }

    private static int StableIndex(int seed, int salt, int count)
    {
        if (count <= 1)
        {
            return 0;
        }

        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ seed;
            hash = (hash * 397) ^ salt;
            return (hash & int.MaxValue) % count;
        }
    }

    private static float StableRange(int seed, int salt)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ seed;
            hash = (hash * 397) ^ salt;
            return (hash & int.MaxValue) / (float)int.MaxValue;
        }
    }
}

public static class DebugActions_PlanarMagic
{
    [LudeonTK.DebugAction("MFVanilla - Planar Magic", "Spawn Planar Pocket Near Current Map", actionType = LudeonTK.DebugActionType.Action, allowedGameStates = LudeonTK.AllowedGameStates.PlayingOnMap)]
    public static void SpawnPlanarPocketNearCurrentMap()
    {
        Map map = Find.CurrentMap;
        if (map == null)
        {
            Messages.Message("No current map is available.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        if (PlanarMagicUtility.TryCreatePlanarPocketSite(map, PlanarMagicUtility.PlanarPocketSitePartDefName, 1, 4, selectSite: true, out Site site))
        {
            Messages.Message($"Spawned planar pocket at tile {site.Tile}.", MessageTypeDefOf.PositiveEvent, false);
            return;
        }

        Messages.Message("Could not spawn a planar pocket site.", MessageTypeDefOf.RejectInput, false);
    }
}
