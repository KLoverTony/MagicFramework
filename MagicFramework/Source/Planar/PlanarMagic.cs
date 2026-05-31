using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicFramework.Core;

public sealed class SitePartWorker_PlanarPocket : SitePartWorker
{
}

public sealed class PlanarDimensionDef : Def
{
    public WorldObjectDef worldObjectDef;
    public MapGeneratorDef mapGeneratorDef;
    public ThingDef returnGateDef;
    public List<TerrainDef> terrainOptions = new();
    public List<ThingDef> plantOptions = new();
    public List<ThingDef> chunkOptions = new();
    public List<ThingDef> mineableOptions = new();
    public List<PlanarFlowFeatureDef> flowFeatures = new();
    public List<PlanarPlantClusterDef> plantClusters = new();
    public int mapSize = 120;
    public float plantDensity = 0.075f;
    public float chunkDensity = 0.018f;
    public float mineableDensity = 0.012f;
    public bool blocksOffMapTransport = true;
    public string transportBlockedMessage = "Planar interference prevents conventional off-map transport from this pocket.";
}

public sealed class PlanarFlowFeatureDef : Def
{
    public TerrainDef channelTerrain;
    public TerrainDef edgeTerrain;
    public TerrainDef bankTerrain;
    public List<ThingDef> bankThings = new();
    public int minWidth = 3;
    public int maxWidth = 7;
    public int bankWidth = 2;
    public float meanderStrength = 0.35f;
    public float bankThingDensity = 0.01f;
    public bool clearPlantsInChannel = true;
    public bool clearBuildingsInChannel;
}

public sealed class PlanarPlantClusterDef : Def
{
    public ThingDef plantDef;
    public List<ThingDef> anchorDefs = new();
    public int radius = 4;
    public int minPlantsPerAnchor = 2;
    public int maxPlantsPerAnchor = 5;
    public float growthMin = 0.65f;
    public float growthMax = 1f;
}

public sealed class PlanarPocketParent : PocketMapParent
{
    public PlanarDimensionDef dimension;
    public int originMapId = -1;
    public IntVec3 originGatePosition = IntVec3.Invalid;
    public int forcedReturnTick = -1;
    public int generationSeed;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref dimension, "dimension");
        Scribe_Values.Look(ref originMapId, "originMapId", -1);
        Scribe_Values.Look(ref originGatePosition, "originGatePosition", IntVec3.Invalid);
        Scribe_Values.Look(ref forcedReturnTick, "forcedReturnTick", -1);
        Scribe_Values.Look(ref generationSeed, "generationSeed", 0);
    }
}

public sealed class Building_PlanarGate : Building
{
}

public sealed class CompProperties_PlanarGate : CompProperties
{
    public PlanarDimensionDef dimension;
    public string dimensionDefName;
    public List<PlanarDimensionDef> dimensionOptions = new();
    public string sitePartDefName = PlanarMagicUtility.PlanarPocketSitePartDefName;
    public int minSiteDistance = 1;
    public int maxSiteDistance = 4;
    public int activationRadius = 5;
    public int alignmentTicksMin = 30000;
    public int alignmentTicksMax = 90000;
    public string spireDefName = "MFV_ArcaneSpire";
    public float spireRadius = 10f;
    public int maxAlignmentSpires = 4;
    public float alignmentPowerPerSpire = 0.35f;
    public float alignmentCycleDays = 12f;
    public float baseAlignmentWindowDays = 1f;
    public float alignmentWindowDaysPerSpire = 0.5f;
    public string activeFleckDef = "ElectricalSpark";
    public string activeFlashFleckDef = "SparkFlash";
    public int activeFleckIntervalTicksMin = 120;
    public int activeFleckIntervalTicksMax = 240;
    public float activeFleckRadius = 1.35f;
    public float activeFleckScale = 0.8f;

    public CompProperties_PlanarGate()
    {
        compClass = typeof(CompPlanarGate);
    }

    public PlanarDimensionDef ResolvedDimension
    {
        get
        {
            if (dimension != null)
            {
                return dimension;
            }

            return string.IsNullOrEmpty(dimensionDefName) ? null : DefDatabase<PlanarDimensionDef>.GetNamedSilentFail(dimensionDefName);
        }
    }

    public List<PlanarDimensionDef> ResolvedDimensions
    {
        get
        {
            List<PlanarDimensionDef> resolved = dimensionOptions?.FindAll(def => def != null) ?? new List<PlanarDimensionDef>();
            PlanarDimensionDef singleDimension = ResolvedDimension;
            if (singleDimension != null && !resolved.Contains(singleDimension))
            {
                resolved.Insert(0, singleDimension);
            }

            return resolved;
        }
    }
}

public sealed class PlanarAlignmentGameComponent : GameComponent
{
    public PlanarAlignmentGameComponent(Game game)
    {
    }

    public static PlanarAlignmentGameComponent Instance => Current.Game?.GetComponent<PlanarAlignmentGameComponent>();

    public bool IsAligned(CompProperties_PlanarGate props, float gatePower)
    {
        return TicksIntoCycle(props) < AlignmentWindowTicks(props, gatePower);
    }

    public int RemainingTicks(CompProperties_PlanarGate props, float gatePower)
    {
        int ticksIntoCycle = TicksIntoCycle(props);
        int windowTicks = AlignmentWindowTicks(props, gatePower);
        int cycleTicks = AlignmentCycleTicks(props);
        if (ticksIntoCycle < windowTicks)
        {
            return 0;
        }

        return Mathf.Max(0, cycleTicks - ticksIntoCycle);
    }

    public string StatusText(CompProperties_PlanarGate props, float gatePower)
    {
        int remainingTicks = RemainingTicks(props, gatePower);
        if (remainingTicks <= 0)
        {
            int windowRemainingTicks = AlignmentWindowTicks(props, gatePower) - TicksIntoCycle(props);
            return $"The celestial planes are aligned. The gate can open for {Mathf.Max(0, windowRemainingTicks).ToStringTicksToPeriod()}.";
        }

        return $"Waiting for celestial alignment: {remainingTicks.ToStringTicksToPeriod()} remaining.";
    }

    public int CurrentWindowEndTick(CompProperties_PlanarGate props, float gatePower)
    {
        int ticksIntoCycle = TicksIntoCycle(props);
        int windowTicks = AlignmentWindowTicks(props, gatePower);
        if (ticksIntoCycle >= windowTicks)
        {
            return GenTicks.TicksGame;
        }

        return GenTicks.TicksGame + (windowTicks - ticksIntoCycle);
    }

    public void NotifyGateOpened(CompProperties_PlanarGate props)
    {
    }

    public int CurrentCycleIndex(CompProperties_PlanarGate props)
    {
        int cycleTicks = AlignmentCycleTicks(props);
        return cycleTicks <= 0 ? 0 : GenTicks.TicksGame / cycleTicks;
    }

    private static int TicksIntoCycle(CompProperties_PlanarGate props)
    {
        int cycleTicks = AlignmentCycleTicks(props);
        return cycleTicks <= 0 ? 0 : GenTicks.TicksGame % cycleTicks;
    }

    private static int AlignmentCycleTicks(CompProperties_PlanarGate props)
    {
        float cycleDays = Mathf.Max(0.1f, props?.alignmentCycleDays ?? 12f);
        return Mathf.Max(1, Mathf.RoundToInt(cycleDays * GenDate.TicksPerDay));
    }

    private static int AlignmentWindowTicks(CompProperties_PlanarGate props, float gatePower)
    {
        float cycleDays = Mathf.Max(0.1f, props?.alignmentCycleDays ?? 12f);
        float baseDays = Mathf.Max(0.01f, props?.baseAlignmentWindowDays ?? 1f);
        float bonusDays = Mathf.Max(0f, gatePower - 1f) * ((props?.alignmentWindowDaysPerSpire ?? 0.5f) / Mathf.Max(0.01f, props?.alignmentPowerPerSpire ?? 0.35f));
        float windowDays = Mathf.Min(cycleDays, baseDays + bonusDays);
        return Mathf.Max(1, Mathf.RoundToInt(windowDays * GenDate.TicksPerDay));
    }
}

public sealed class CompPlanarGate : ThingComp
{
    private int pocketParentId = -1;
    private int nextActiveFleckTick;
    private string selectedDimensionDefName;
    private Dictionary<string, int> pocketParentIdsByDimension = new();

    private CompProperties_PlanarGate Props => (CompProperties_PlanarGate)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        ScheduleNextActiveFleck();
    }

    public override void CompTick()
    {
        base.CompTick();

        if (parent?.Spawned != true || parent.Map == null || Find.TickManager.TicksGame < nextActiveFleckTick)
        {
            return;
        }

        if (PlanarAlignmentGameComponent.Instance?.IsAligned(Props, AlignmentPower()) == true)
        {
            ThrowActiveFlecks();
        }

        ScheduleNextActiveFleck();
    }

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

        if (PlanarMagicUtility.IsPlanarPocketMap(parent.Map))
        {
            yield return new Command_Action
            {
                defaultLabel = "Return through gate",
                defaultDesc = "Return selected travelers and supplies from this planar realm to the gate that opened it.",
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/Planar/PlanarGateOpen", false),
                action = ReturnSelectedFromPocket
            };
            yield break;
        }

        float alignmentPower = AlignmentPower();
        PlanarAlignmentGameComponent alignment = PlanarAlignmentGameComponent.Instance;
        bool isAligned = alignment?.IsAligned(Props, alignmentPower) == true;
        Command_Action command = new()
        {
            defaultLabel = "Send selected through gate",
            defaultDesc = $"Send selected player-controlled pawns within {Props.activationRadius} cells through this planar gate.\n\nDestination: {CurrentDimensionLabel()}.\n{AlignmentStatusText(alignmentPower)}",
            icon = ContentFinder<Texture2D>.Get("UI/Gizmos/Planar/PlanarGateOpen", false),
            action = TraverseSelectedPawns
        };
        if (!isAligned)
        {
            command.Disable("The celestial planes are not aligned.");
        }

        yield return command;

        List<PlanarDimensionDef> dimensions = Props.ResolvedDimensions;
        if (dimensions.Count > 1)
        {
            yield return new Command_Action
            {
                defaultLabel = "Tune planar gate",
                defaultDesc = $"Choose which reachable realm this planar gate opens into.\n\nCurrent destination: {CurrentDimensionLabel()}.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", false),
                action = OpenDestinationMenu
            };
        }
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
    {
        foreach (FloatMenuOption option in base.CompFloatMenuOptions(pawn))
        {
            yield return option;
        }

        if (PlanarMagicUtility.IsPlanarPocketMap(parent?.Map))
        {
            if (pawn == null || pawn.Destroyed || !pawn.Spawned || pawn.Map != parent.Map)
            {
                yield return new FloatMenuOption("Return through planar gate (invalid pawn or gate)", null);
                yield break;
            }

            yield return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption("Return through planar gate", () =>
                {
                    PlanarMagicUtility.TryReturnSelectedFromPlanarPocket(parent.Map, new List<Thing> { pawn });
                }),
                pawn,
                parent);
            yield break;
        }

        if (!CanPawnUseGate(pawn, out string failReason))
        {
            yield return new FloatMenuOption($"Use planar gate ({failReason})", null);
            yield break;
        }

        yield return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption($"Use planar gate to {CurrentDimensionLabel()}", () =>
            {
                StartTraversalJobs(new List<Pawn> { pawn });
            }),
            pawn,
            parent);
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

    private void ReturnSelectedFromPocket()
    {
        List<Thing> selectedThings = Find.Selector?.SelectedObjects?.OfType<Thing>()
            .Where(thing => thing != null && !thing.Destroyed && thing.Spawned && thing.Map == parent.Map)
            .ToList() ?? new List<Thing>();
        if (selectedThings.Count == 0)
        {
            Messages.Message("Select one or more travelers or supplies in this planar realm.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        PlanarMagicUtility.TryReturnSelectedFromPlanarPocket(parent.Map, selectedThings);
    }

    public bool TryTraversePawn(Pawn pawn)
    {
        if (!CanPawnUseGate(pawn, out string failReason))
        {
            Messages.Message($"Cannot use planar gate: {failReason}.", MessageTypeDefOf.RejectInput, false);
            return false;
        }

        PlanarAlignmentGameComponent alignment = PlanarAlignmentGameComponent.Instance;
        if (alignment?.IsAligned(Props, AlignmentPower()) != true)
        {
            Messages.Message(AlignmentStatusText(AlignmentPower()), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        Map sourceMap = pawn.Map;
        float alignmentPower = AlignmentPower();
        PlanarDimensionDef dimension = SelectedDimension();
        Map destinationMap = GetOrCreatePlanarPocketMap(dimension);
        if (destinationMap == null)
        {
            if (sourceMap != null)
            {
                Current.Game.CurrentMap = sourceMap;
            }

            Messages.Message("The planar gate could not stabilize a pocket map.", MessageTypeDefOf.RejectInput, false);
            return false;
        }

        if (destinationMap.Parent is PlanarPocketParent pocketParent)
        {
            pocketParent.forcedReturnTick = alignment.CurrentWindowEndTick(Props, alignmentPower);
        }

        IntVec3 arrivalCenter = new(destinationMap.Size.x / 2, 0, destinationMap.Size.z / 2);
        int moved = PlanarMagicUtility.TransferPawnsThroughGate(new List<Pawn> { pawn }, destinationMap, arrivalCenter);
        if (moved <= 0)
        {
            if (sourceMap != null)
            {
                Current.Game.CurrentMap = sourceMap;
            }

            return false;
        }

        alignment.NotifyGateOpened(Props);
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
        Scribe_Values.Look(ref pocketParentId, "pocketParentId", -1);
        Scribe_Values.Look(ref selectedDimensionDefName, "selectedDimensionDefName");
        Scribe_Collections.Look(ref pocketParentIdsByDimension, "pocketParentIdsByDimension", LookMode.Value, LookMode.Value);
        if (Scribe.mode == LoadSaveMode.LoadingVars && pocketParentId < 0)
        {
            int legacySiteId = -1;
            Scribe_Values.Look(ref legacySiteId, "siteId", -1);
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            pocketParentIdsByDimension ??= new Dictionary<string, int>();
            PlanarDimensionDef dimension = SelectedDimension();
            if (dimension != null && pocketParentId >= 0 && !pocketParentIdsByDimension.ContainsKey(dimension.defName))
            {
                pocketParentIdsByDimension[dimension.defName] = pocketParentId;
            }
        }
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

    private Map GetOrCreatePlanarPocketMap(PlanarDimensionDef dimension)
    {
        string dimensionKey = dimension?.defName ?? "default";
        int storedParentId = PocketParentIdFor(dimensionKey);
        PlanarPocketParent pocketParent = PlanarMagicUtility.FindPlanarPocketParentById(storedParentId);
        if (pocketParent == null)
        {
            int cycleIndex = PlanarAlignmentGameComponent.Instance?.CurrentCycleIndex(Props) ?? 0;
            if (!PlanarMagicUtility.TryCreatePlanarPocketParent(parent.Map, parent.Position, dimension, cycleIndex, out pocketParent))
            {
                return null;
            }

            SetPocketParentIdFor(dimensionKey, pocketParent.ID);
        }

        Map map = PlanarMagicUtility.GetOrGeneratePocketMap(pocketParent);
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

    public override string CompInspectStringExtra()
    {
        int linkedSpires = LinkedArcaneSpires().Count();
        float power = AlignmentPower(linkedSpires);
        string spireText = linkedSpires == 1 ? "1 arcane spire linked" : $"{linkedSpires} arcane spires linked";
        return $"{AlignmentStatusText(power)}\nDestination: {CurrentDimensionLabel()}.\nPlanar gate power: {power:0.##}x ({spireText}).";
    }

    private void OpenDestinationMenu()
    {
        List<PlanarDimensionDef> dimensions = Props.ResolvedDimensions;
        if (dimensions.Count <= 1)
        {
            return;
        }

        List<FloatMenuOption> options = new();
        for (int i = 0; i < dimensions.Count; i++)
        {
            PlanarDimensionDef dimension = dimensions[i];
            string label = dimension == SelectedDimension() ? $"{dimension.LabelCap} (current)" : dimension.LabelCap;
            options.Add(new FloatMenuOption(label, () =>
            {
                selectedDimensionDefName = dimension.defName;
                Messages.Message($"Planar gate tuned to {dimension.LabelCap}.", parent, MessageTypeDefOf.PositiveEvent, false);
            }));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private PlanarDimensionDef SelectedDimension()
    {
        List<PlanarDimensionDef> dimensions = Props.ResolvedDimensions;
        if (!string.IsNullOrWhiteSpace(selectedDimensionDefName))
        {
            PlanarDimensionDef selected = dimensions.FirstOrDefault(def => def.defName == selectedDimensionDefName)
                ?? DefDatabase<PlanarDimensionDef>.GetNamedSilentFail(selectedDimensionDefName);
            if (selected != null)
            {
                return selected;
            }
        }

        return dimensions.Count > 0 ? dimensions[0] : Props.ResolvedDimension;
    }

    private string CurrentDimensionLabel()
    {
        PlanarDimensionDef dimension = SelectedDimension();
        return dimension?.LabelCap ?? "an unstable pocket";
    }

    private int PocketParentIdFor(string dimensionKey)
    {
        pocketParentIdsByDimension ??= new Dictionary<string, int>();
        if (pocketParentIdsByDimension.TryGetValue(dimensionKey, out int parentId))
        {
            return parentId;
        }

        return dimensionKey == (Props.ResolvedDimension?.defName ?? "default") ? pocketParentId : -1;
    }

    private void SetPocketParentIdFor(string dimensionKey, int parentId)
    {
        pocketParentIdsByDimension ??= new Dictionary<string, int>();
        pocketParentIdsByDimension[dimensionKey] = parentId;
        if (dimensionKey == (Props.ResolvedDimension?.defName ?? "default"))
        {
            pocketParentId = parentId;
        }
    }

    public void NotifyPlanarPocketReturned(PlanarDimensionDef dimension, int returnedParentId)
    {
        string dimensionKey = dimension?.defName ?? "default";
        pocketParentIdsByDimension ??= new Dictionary<string, int>();
        if (pocketParentIdsByDimension.TryGetValue(dimensionKey, out int storedParentId) && storedParentId == returnedParentId)
        {
            pocketParentIdsByDimension.Remove(dimensionKey);
        }

        if (pocketParentId == returnedParentId)
        {
            pocketParentId = -1;
        }
    }

    private string AlignmentStatusText(float alignmentPower)
    {
        return PlanarAlignmentGameComponent.Instance?.StatusText(Props, alignmentPower) ?? "The gate cannot read the celestial alignment.";
    }

    private float AlignmentPower()
    {
        return AlignmentPower(LinkedArcaneSpires().Count());
    }

    private float AlignmentPower(int linkedSpireCount)
    {
        int spireCount = Mathf.Clamp(linkedSpireCount, 0, Mathf.Max(0, Props.maxAlignmentSpires));
        return 1f + spireCount * Mathf.Max(0f, Props.alignmentPowerPerSpire);
    }

    private IEnumerable<Thing> LinkedArcaneSpires()
    {
        if (parent?.Spawned != true || parent.Map == null)
        {
            yield break;
        }

        ThingDef spireDef = DefDatabase<ThingDef>.GetNamedSilentFail(Props.spireDefName);
        if (spireDef == null)
        {
            yield break;
        }

        float radiusSquared = Props.spireRadius * Props.spireRadius;
        int yielded = 0;
        foreach (Thing spire in parent.Map.listerThings.ThingsOfDef(spireDef))
        {
            if (spire?.Spawned == true
                && spire.Position.DistanceToSquared(parent.Position) <= radiusSquared
                && GenSight.LineOfSight(parent.Position, spire.Position, parent.Map))
            {
                yield return spire;
                yielded++;
                if (yielded >= Props.maxAlignmentSpires)
                {
                    yield break;
                }
            }
        }
    }

    private void ThrowActiveFlecks()
    {
        FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(Props.activeFleckDef);
        if (fleckDef == null || parent.Map == null)
        {
            return;
        }

        FleckDef flashFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(Props.activeFlashFleckDef);
        Vector3 center = parent.DrawPos;
        center.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        int tick = Find.TickManager.TicksGame;
        float radius = Mathf.Max(0.1f, Props.activeFleckRadius);
        int fleckCount = Mathf.Clamp(3 + LinkedArcaneSpires().Count(), 3, 7);

        for (int i = 0; i < fleckCount; i++)
        {
            float angle = ((tick / 19f) + (360f / fleckCount * i)) * Mathf.Deg2Rad;
            Vector3 position = center;
            position.x += Mathf.Cos(angle) * radius;
            position.z += Mathf.Sin(angle) * radius;
            FleckMaker.Static(position, parent.Map, fleckDef, Mathf.Max(0.1f, Props.activeFleckScale));
        }

        if (flashFleckDef != null)
        {
            FleckMaker.Static(center, parent.Map, flashFleckDef, Mathf.Max(0.1f, Props.activeFleckScale * 1.15f));
        }
    }

    private void ScheduleNextActiveFleck()
    {
        int min = Mathf.Max(1, Props.activeFleckIntervalTicksMin);
        int max = Mathf.Max(min, Props.activeFleckIntervalTicksMax);
        int range = max - min + 1;
        int offset = min + Mathf.Abs(Gen.HashCombineInt(parent?.thingIDNumber ?? 0, Find.TickManager.TicksGame)) % range;
        nextActiveFleckTick = Find.TickManager.TicksGame + offset;
    }
}

public static class PlanarMagicUtility
{
    public const string PlanarPocketSitePartDefName = "MF_PlanarDimension";
    public const string PlanarPocketParentDefName = "MF_PlanarDimensionParent";
    public const string PlanarPocketMapGeneratorDefName = "MF_PlanarDimensionMap";
    public const int PlanarPocketMapSize = 120;
    public const string PlanarTransportBlockedMessage = "Planar interference prevents conventional off-map transport from this pocket.";

    public static bool BlocksOffMapTransport(Map map)
    {
        return IsPlanarPocketMap(map);
    }

    public static void MessageOffMapTransportBlocked()
    {
        Messages.Message(PlanarTransportBlockedMessage, MessageTypeDefOf.RejectInput, false);
    }

    public static bool TryCreatePlanarPocketParent(Map originMap, IntVec3 originGatePosition, PlanarDimensionDef dimension, int cycleIndex, out PlanarPocketParent pocketParent)
    {
        pocketParent = null;
        if (originMap == null || originMap.Tile < 0)
        {
            return false;
        }

        WorldObjectDef parentDef = dimension?.worldObjectDef ?? DefDatabase<WorldObjectDef>.GetNamedSilentFail(PlanarPocketParentDefName);
        if (parentDef == null)
        {
            Log.Warning($"[MagicFramework] Could not create planar pocket because {PlanarPocketParentDefName} WorldObjectDef was not found.");
            return false;
        }

        pocketParent = WorldObjectMaker.MakeWorldObject(parentDef) as PlanarPocketParent;
        if (pocketParent == null)
        {
            Log.Warning($"[MagicFramework] Could not create planar pocket because {PlanarPocketParentDefName} did not make a PlanarPocketParent.");
            return false;
        }

        pocketParent.Tile = originMap.Tile;
        pocketParent.dimension = dimension;
        pocketParent.originMapId = originMap.uniqueID;
        pocketParent.originGatePosition = originGatePosition;
        pocketParent.generationSeed = ResolvePlanarGenerationSeed(originMap, originGatePosition, dimension, cycleIndex);
        Find.WorldObjects.Add(pocketParent);
        if (Prefs.DevMode)
        {
            Log.Message($"[MagicFramework] Created hidden planar pocket parent {pocketParent.ID} from map tile {originMap.Tile}.");
        }

        return true;
    }

    private static int ResolvePlanarGenerationSeed(Map originMap, IntVec3 originGatePosition, PlanarDimensionDef dimension, int cycleIndex)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ (originMap?.uniqueID ?? -1);
            hash = (hash * 397) ^ (originMap?.Tile ?? -1);
            hash = (hash * 397) ^ originGatePosition.x;
            hash = (hash * 397) ^ originGatePosition.z;
            hash = (hash * 397) ^ (dimension?.shortHash ?? 0);
            hash = (hash * 397) ^ cycleIndex;
            return hash;
        }
    }

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
            Log.Warning($"[MagicFramework] Could not create planar pocket because {sitePartDefName} SitePartDef was not found.");
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
            Log.Message($"[MagicFramework] Created planar pocket site at tile {tile} from map tile {originMap.Tile}.");
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

    public static PlanarPocketParent FindPlanarPocketParentById(int parentId)
    {
        if (parentId < 0)
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
            if (worldObjects[i] is PlanarPocketParent parent && parent.ID == parentId)
            {
                return parent;
            }
        }

        return null;
    }

    public static Map GetOrGeneratePocketMap(MapParent parent)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.HasMap)
        {
            return parent.Map;
        }

        PlanarDimensionDef dimension = (parent as PlanarPocketParent)?.dimension;
        int resolvedMapSize = Mathf.Max(20, dimension?.mapSize ?? PlanarPocketMapSize);
        IntVec3 mapSize = new(resolvedMapSize, 1, resolvedMapSize);
        MapGeneratorDef mapGeneratorDef = dimension?.mapGeneratorDef ?? DefDatabase<MapGeneratorDef>.GetNamedSilentFail(PlanarPocketMapGeneratorDefName);
        IEnumerable<GenStepWithParams> extraGenSteps = null;
        if (mapGeneratorDef == null)
        {
            mapGeneratorDef = RimWorld.MapGeneratorDefOf.Encounter;
            Log.Warning($"[MagicFramework] {PlanarPocketMapGeneratorDefName} was not found; falling back to {mapGeneratorDef.defName}.");
        }

        try
        {
            Map map = MapGenerator.GenerateMap(mapSize, parent, mapGeneratorDef, extraGenSteps, null, false, false);
            EnsurePlanarPocketReady(map);
            return map;
        }
        catch (Exception ex)
        {
            Log.Error($"[MagicFramework] Failed to generate planar pocket map for parent {parent.ID}: {ex}");
            return null;
        }
    }

    public static Map GetOrGenerateSiteMap(Site site)
    {
        return GetOrGeneratePocketMap(site);
    }

    public static void EnsurePlanarPocketReady(Map map)
    {
        if (map == null || !IsPlanarPocketMap(map))
        {
            return;
        }

        PlanarDimensionDef dimension = ResolveDimension(map);
        List<TerrainDef> terrains = ResolvePlanarTerrains(dimension);
        TerrainDef fallbackTerrain = terrains.Count > 0 ? terrains[0] : TerrainDefOf.Soil;
        bool hasPlanarTerrain = false;
        int checkedCells = 0;
        foreach (IntVec3 cell in map.AllCells)
        {
            TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
            if (terrain != null && terrains.Contains(terrain))
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

        if (!HasPlanarFlowTerrain(map, dimension))
        {
            GeneratePlanarFlows(map, dimension, map.Tile ^ 0x59D3A41);
        }

        int mapArea = Math.Max(1, map.Size.x * map.Size.z);
        List<ThingDef> plants = ResolvePlanarPlants(dimension);
        int plantCount = CountThingsWithDefs(map, plants);
        if (plantCount < Math.Max(18, mapArea / 125))
        {
            ScatterPlanarPlants(map, map.Tile ^ 0x7156A31B, Math.Max(18, mapArea / 95) - plantCount, plants);
        }

        List<ThingDef> mineables = ResolvePlanarMineables(dimension);
        int mineableCount = CountThingsWithDefs(map, mineables);
        if (mineableCount < Math.Max(8, mapArea / 550))
        {
            ScatterPlanarMineables(map, map.Tile ^ 0x42198E71, Math.Max(8, mapArea / 460) - mineableCount, mineables);
        }

        List<ThingDef> chunks = ResolvePlanarChunks(dimension);
        int chunkCount = CountThingsWithDefs(map, chunks);
        if (chunkCount < Math.Max(10, mapArea / 450))
        {
            ScatterPlanarStoneChunks(map, map.Tile ^ 0x31B4D2F1, Math.Max(10, mapArea / 360) - chunkCount, chunks);
        }

        ScatterPlanarPlantClusters(map, dimension, map.Tile ^ 0x19B64C7D);
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

    public static void GeneratePlanarFlows(Map map, PlanarDimensionDef dimension, int seed)
    {
        if (map == null || dimension?.flowFeatures.NullOrEmpty() != false)
        {
            return;
        }

        for (int i = 0; i < dimension.flowFeatures.Count; i++)
        {
            PlanarFlowFeatureDef flow = dimension.flowFeatures[i];
            if (flow != null)
            {
                PlanarFlowFeatureUtility.Generate(map, flow, seed ^ (i * 104729));
            }
        }
    }

    private static bool HasPlanarFlowTerrain(Map map, PlanarDimensionDef dimension)
    {
        if (map == null || dimension?.flowFeatures.NullOrEmpty() != false)
        {
            return true;
        }

        HashSet<TerrainDef> flowTerrains = new();
        for (int i = 0; i < dimension.flowFeatures.Count; i++)
        {
            PlanarFlowFeatureDef flow = dimension.flowFeatures[i];
            if (flow?.channelTerrain != null)
            {
                flowTerrains.Add(flow.channelTerrain);
            }

            if (flow?.edgeTerrain != null)
            {
                flowTerrains.Add(flow.edgeTerrain);
            }

            if (flow?.bankTerrain != null)
            {
                flowTerrains.Add(flow.bankTerrain);
            }
        }

        if (flowTerrains.Count == 0)
        {
            return true;
        }

        int matches = 0;
        int requiredMatches = Math.Max(8, map.Size.x / 4);
        foreach (IntVec3 cell in map.AllCells)
        {
            if (flowTerrains.Contains(map.terrainGrid.TerrainAt(cell)))
            {
                matches++;
                if (matches >= requiredMatches)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static PlanarDimensionDef ResolveDimension(Map map)
    {
        return (map?.Parent as PlanarPocketParent)?.dimension;
    }

    public static List<TerrainDef> ResolvePlanarTerrains(PlanarDimensionDef dimension = null)
    {
        if (dimension?.terrainOptions.NullOrEmpty() == false)
        {
            return dimension.terrainOptions.FindAll(def => def != null);
        }

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

    public static List<ThingDef> ResolvePlanarPlants(PlanarDimensionDef dimension = null)
    {
        if (dimension?.plantOptions.NullOrEmpty() == false)
        {
            return dimension.plantOptions.FindAll(def => def != null);
        }

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

    public static List<ThingDef> ResolvePlanarChunks(PlanarDimensionDef dimension = null)
    {
        if (dimension?.chunkOptions.NullOrEmpty() == false)
        {
            return dimension.chunkOptions.FindAll(def => def != null);
        }

        return ResolveThingDefs(PlanarChunkDefNames);
    }

    public static List<ThingDef> ResolvePlanarMineables(PlanarDimensionDef dimension = null)
    {
        if (dimension?.mineableOptions.NullOrEmpty() == false)
        {
            return dimension.mineableOptions.FindAll(def => def != null);
        }

        return ResolveThingDefs(PlanarMineableDefNames);
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

    public static void ScatterPlanarPlants(Map map, int seed, int targetCount, List<ThingDef> plantOptions = null)
    {
        List<ThingDef> plants = plantOptions?.FindAll(def => def != null) ?? ResolvePlanarPlants();
        if (map == null || plants.Count == 0 || targetCount <= 0)
        {
            return;
        }

        int spawned = 0;
        List<IntVec3> cells = JitteredScatterCells(map, seed, targetCount);
        for (int i = 0; i < cells.Count && spawned < targetCount; i++)
        {
            IntVec3 cell = cells[i];
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

        int attempts = Math.Max(200, targetCount * 8);
        for (int i = 0; i < attempts && spawned < targetCount; i++)
        {
            IntVec3 cell = HashedStableCell(map, seed, i);
            if (!CanScatterAt(map, cell))
            {
                continue;
            }

            ThingDef plantDef = plants[StableIndex(seed, i * 17491, plants.Count)];
            Thing thing = ThingMaker.MakeThing(plantDef);
            if (thing is Plant plant)
            {
                plant.Growth = 0.55f + (StableRange(seed, i * 23497) * 0.45f);
            }

            GenSpawn.Spawn(thing, cell, map);
            spawned++;
        }
    }

    public static void ScatterPlanarStoneChunks(Map map, int seed, int targetCount, List<ThingDef> chunkOptions = null)
    {
        if (map == null || targetCount <= 0)
        {
            return;
        }

        List<ThingDef> chunks = chunkOptions?.FindAll(def => def != null) ?? ResolvePlanarChunks();

        if (chunks.Count == 0)
        {
            return;
        }

        int spawned = 0;
        int attempts = Math.Max(120, targetCount * 16);
        for (int i = 0; i < attempts && spawned < targetCount; i++)
        {
            IntVec3 cell = HashedStableCell(map, seed, i);
            if (!CanScatterAt(map, cell))
            {
                continue;
            }

            ThingDef chunkDef = chunks[StableIndex(seed, i * 7331, chunks.Count)];
            GenSpawn.Spawn(ThingMaker.MakeThing(chunkDef), cell, map);
            spawned++;
        }
    }

    public static void ScatterPlanarMineables(Map map, int seed, int targetCount, List<ThingDef> mineableOptions = null)
    {
        if (map == null || targetCount <= 0)
        {
            return;
        }

        List<ThingDef> mineables = mineableOptions?.FindAll(def => def != null) ?? ResolvePlanarMineables();

        if (mineables.Count == 0)
        {
            return;
        }

        int spawned = 0;
        int attempts = Math.Max(140, targetCount * 20);
        for (int i = 0; i < attempts && spawned < targetCount; i++)
        {
            IntVec3 cell = HashedStableCell(map, seed, i);
            if (!CanScatterAt(map, cell))
            {
                continue;
            }

            ThingDef mineableDef = mineables[StableIndex(seed, i * 6151, mineables.Count)];
            GenSpawn.Spawn(ThingMaker.MakeThing(mineableDef), cell, map);
            spawned++;
        }
    }

    public static void ScatterPlanarPlantClusters(Map map, PlanarDimensionDef dimension, int seed)
    {
        if (map == null || dimension?.plantClusters.NullOrEmpty() != false)
        {
            return;
        }

        for (int i = 0; i < dimension.plantClusters.Count; i++)
        {
            PlanarPlantClusterDef cluster = dimension.plantClusters[i];
            if (cluster?.plantDef == null || cluster.anchorDefs.NullOrEmpty())
            {
                continue;
            }

            List<Thing> anchors = ResolveAnchorThings(map, cluster.anchorDefs);
            for (int j = 0; j < anchors.Count; j++)
            {
                Thing anchor = anchors[j];
                if (anchor?.Spawned != true)
                {
                    continue;
                }

                ScatterPlantClusterAround(map, cluster, anchor.Position, seed ^ (i * 104729) ^ (j * 3917));
            }
        }
    }

    private static List<Thing> ResolveAnchorThings(Map map, List<ThingDef> anchorDefs)
    {
        List<Thing> anchors = new();
        for (int i = 0; i < anchorDefs.Count; i++)
        {
            ThingDef anchorDef = anchorDefs[i];
            if (anchorDef == null)
            {
                continue;
            }

            List<Thing> things = map.listerThings.ThingsOfDef(anchorDef);
            if (!things.NullOrEmpty())
            {
                anchors.AddRange(things);
            }
        }

        return anchors;
    }

    private static void ScatterPlantClusterAround(Map map, PlanarPlantClusterDef cluster, IntVec3 center, int seed)
    {
        int min = Math.Max(0, cluster.minPlantsPerAnchor);
        int max = Math.Max(min, cluster.maxPlantsPerAnchor);
        int targetCount = min + StableIndex(seed, 0x57CC, max - min + 1);
        int attempts = Math.Max(24, targetCount * 10);
        int spawned = 0;
        int radius = Math.Max(1, cluster.radius);
        for (int i = 0; i < attempts && spawned < targetCount; i++)
        {
            IntVec3 cell = center + GenRadial.RadialPattern[StableIndex(seed, i * 92821, GenRadial.NumCellsInRadius(radius))];
            if (!CanScatterAt(map, cell))
            {
                continue;
            }

            Thing thing = ThingMaker.MakeThing(cluster.plantDef);
            if (thing is Plant plant)
            {
                float minGrowth = Mathf.Clamp01(cluster.growthMin);
                float maxGrowth = Mathf.Clamp01(Mathf.Max(minGrowth, cluster.growthMax));
                plant.Growth = minGrowth + StableRange(seed, i * 17033) * (maxGrowth - minGrowth);
            }

            GenSpawn.Spawn(thing, cell, map);
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

    private static int CountThingsWithDefs(Map map, List<ThingDef> defs)
    {
        if (map == null || defs.NullOrEmpty())
        {
            return 0;
        }

        int count = 0;
        foreach (IntVec3 cell in map.AllCells)
        {
            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                if (defs.Contains(things[i]?.def))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static List<ThingDef> ResolveThingDefs(string[] defNames)
    {
        List<ThingDef> defs = new();
        if (defNames.NullOrEmpty())
        {
            return defs;
        }

        for (int i = 0; i < defNames.Length; i++)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defNames[i]);
            if (def != null)
            {
                defs.Add(def);
            }
        }

        return defs;
    }

    private static bool CanScatterAt(Map map, IntVec3 cell)
    {
        if (!cell.InBounds(map) || !cell.Standable(map))
        {
            return false;
        }

        if (PlanarMagicUtility.IsFluidTerrain(map.terrainGrid.TerrainAt(cell)))
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

    public static bool IsFluidTerrain(TerrainDef terrain)
    {
        if (terrain == null)
        {
            return false;
        }

        string defName = terrain.defName ?? string.Empty;
        if (defName.IndexOf("Water", StringComparison.OrdinalIgnoreCase) >= 0
            || defName.IndexOf("River", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (terrain.tags != null)
        {
            for (int i = 0; i < terrain.tags.Count; i++)
            {
                string tag = terrain.tags[i] ?? string.Empty;
                if (tag.IndexOf("Water", StringComparison.OrdinalIgnoreCase) >= 0
                    || tag.IndexOf("River", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static List<IntVec3> JitteredScatterCells(Map map, int seed, int targetCount)
    {
        List<IntVec3> cells = new();
        int usableCount = Math.Max(1, targetCount);
        float cellArea = Math.Max(1f, (map.Size.x * map.Size.z) / (float)usableCount);
        int stride = Math.Max(3, Mathf.RoundToInt(Mathf.Sqrt(cellArea)));
        for (int xBase = 0; xBase < map.Size.x; xBase += stride)
        {
            for (int zBase = 0; zBase < map.Size.z; zBase += stride)
            {
                int salt = (xBase * 73856093) ^ (zBase * 19349663);
                int width = Math.Min(stride, map.Size.x - xBase);
                int height = Math.Min(stride, map.Size.z - zBase);
                int x = xBase + StableIndex(seed, salt ^ 0x24F1A, width);
                int z = zBase + StableIndex(seed, salt ^ 0x6D2B7, height);
                cells.Add(new IntVec3(x, 0, z));
            }
        }

        cells.Sort((a, b) => StableHash(seed, a.x * 397 ^ a.z * 7919).CompareTo(StableHash(seed, b.x * 397 ^ b.z * 7919)));
        return cells;
    }

    private static IntVec3 HashedStableCell(Map map, int seed, int index)
    {
        int hash = StableHash(seed, index * 104729);
        int x = (hash & int.MaxValue) % map.Size.x;
        int z = ((hash >> 16) ^ (hash * 1103515245)) & int.MaxValue;
        z %= map.Size.z;
        return new IntVec3(x, 0, z);
    }

    private static int StableHash(int seed, int salt)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ seed;
            hash = (hash * 397) ^ salt;
            hash ^= hash >> 16;
            hash *= -2048144789;
            hash ^= hash >> 13;
            hash *= -1028477387;
            hash ^= hash >> 16;
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

    public static bool IsPlanarPocketMap(Map map)
    {
        if (map?.Parent is PlanarPocketParent)
        {
            return true;
        }

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
        if (pocketMap?.Parent is PlanarPocketParent pocketParent)
        {
            Map originMap = FindMapByUniqueId(pocketParent.originMapId);
            if (originMap != null)
            {
                return originMap;
            }
        }

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

        ThingDef gateDef = ResolveDimension(map)?.returnGateDef ?? DefDatabase<ThingDef>.GetNamedSilentFail("MFV_PlanarGate");
        if (gateDef == null)
        {
            return null;
        }

        List<Thing> things = map.listerThings.ThingsOfDef(gateDef);
        return things.NullOrEmpty() ? null : things[0];
    }

    public static Thing FindOriginPlanarGate(PlanarPocketParent pocketParent)
    {
        Map originMap = FindMapByUniqueId(pocketParent?.originMapId ?? -1);
        if (originMap == null)
        {
            return null;
        }

        ThingDef gateDef = pocketParent?.dimension?.returnGateDef ?? DefDatabase<ThingDef>.GetNamedSilentFail("MFV_PlanarGate");
        if (gateDef == null)
        {
            return null;
        }

        if (pocketParent.originGatePosition.IsValid)
        {
            List<Thing> thingsAtOrigin = pocketParent.originGatePosition.GetThingList(originMap);
            for (int i = 0; i < thingsAtOrigin.Count; i++)
            {
                Thing thing = thingsAtOrigin[i];
                if (thing?.def == gateDef)
                {
                    return thing;
                }
            }
        }

        List<Thing> gates = originMap.listerThings.ThingsOfDef(gateDef);
        if (gates.NullOrEmpty())
        {
            return null;
        }

        if (!pocketParent.originGatePosition.IsValid)
        {
            return gates[0];
        }

        Thing closestGate = null;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < gates.Count; i++)
        {
            Thing gate = gates[i];
            if (gate == null || gate.Destroyed)
            {
                continue;
            }

            float distance = gate.Position.DistanceToSquared(pocketParent.originGatePosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestGate = gate;
            }
        }

        return closestGate;
    }

    public static Thing SpawnPlanarGateNearCenter(Map map)
    {
        ThingDef gateDef = ResolveDimension(map)?.returnGateDef ?? DefDatabase<ThingDef>.GetNamedSilentFail("MFV_PlanarGate");
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
        if (CellFinder.TryRandomClosewalkCellNear(center, map, 4, out IntVec3 cell, c => c.Standable(map)))
        {
            return cell;
        }

        if (CellFinder.TryRandomClosewalkCellNear(center, map, 8, out cell, c => c.Standable(map)))
        {
            return cell;
        }

        if (CellFinder.TryFindRandomCell(map, c => c.Standable(map), out cell))
        {
            return cell;
        }

        return CellFinder.RandomCell(map);
    }

    public static Map FindMapByUniqueId(int uniqueId)
    {
        if (uniqueId < 0)
        {
            return null;
        }

        List<Map> maps = Find.Maps;
        if (maps == null)
        {
            return null;
        }

        for (int i = 0; i < maps.Count; i++)
        {
            if (maps[i]?.uniqueID == uniqueId)
            {
                return maps[i];
            }
        }

        return null;
    }

    public static bool TryReturnSelectedFromPlanarPocket(Map pocketMap, List<Thing> selectedThings)
    {
        if (pocketMap == null || selectedThings.NullOrEmpty())
        {
            return false;
        }

        PlanarPocketParent pocketParent = pocketMap.Parent as PlanarPocketParent;
        Map destinationMap = FindReturnMap(pocketMap);
        string planeLabel = PlaneLabel(pocketParent);
        if (destinationMap == null)
        {
            Messages.Message($"{planeLabel} cannot find a stable return point.", MessageTypeDefOf.RejectInput, false);
            return false;
        }

        float carriedMass = ReturnCarriedMass(selectedThings);
        float carryingCapacity = ReturnCarryingCapacity(selectedThings);
        if (carriedMass > carryingCapacity)
        {
            Messages.Message($"The selected supplies are too heavy to return: {carriedMass:0.#} / {carryingCapacity:0.#} kg.", MessageTypeDefOf.RejectInput, false);
            return false;
        }

        IntVec3 center = pocketParent != null && pocketParent.originGatePosition.IsValid
            ? pocketParent.originGatePosition
            : new IntVec3(destinationMap.Size.x / 2, 0, destinationMap.Size.z / 2);

        Thing firstReturned = null;
        int returned = 0;
        for (int i = 0; i < selectedThings.Count; i++)
        {
            Thing thing = selectedThings[i];
            if (thing == null || thing.Destroyed)
            {
                continue;
            }

            IntVec3 cell = ResolveArrivalCell(destinationMap, center, returned);
            if (thing is Pawn pawn)
            {
                pawn.jobs?.StopAll(false, true);
            }

            if (thing.Spawned)
            {
                thing.DeSpawn(DestroyMode.Vanish);
            }

            GenSpawn.Spawn(thing, cell, destinationMap);
            firstReturned ??= thing;
            returned++;
        }

        if (returned <= 0)
        {
            return false;
        }

        Current.Game.CurrentMap = destinationMap;
        Find.Selector.ClearSelection();
        if (firstReturned != null && !firstReturned.Destroyed)
        {
            Find.Selector.Select(firstReturned);
            CameraJumper.TryJump(firstReturned);
        }
        else
        {
            CameraJumper.TryJump(center, destinationMap);
        }

        Messages.Message($"{returned} traveler(s) and supplies return from {planeLabel}.", MessageTypeDefOf.PositiveEvent, false);
        TryCleanupPlanarPocketMap(pocketMap);
        return true;
    }

    private static void TryCleanupPlanarPocketMap(Map pocketMap)
    {
        if (pocketMap == null || !IsPlanarPocketMap(pocketMap))
        {
            return;
        }

        PlanarPocketParent pocketParent = pocketMap.Parent as PlanarPocketParent;

        if (HasAnySpawnedPawn(pocketMap))
        {
            string planeLabel = PlaneLabel(pocketParent);
            Log.Warning($"[MagicFramework] {planeLabel} map {pocketMap.uniqueID} was not removed because at least one pawn remains on it.");
            Messages.Message($"{planeLabel} remains unstable: someone is still inside.", MessageTypeDefOf.CautionInput, false);
            return;
        }

        MapParent parent = pocketMap.Parent;
        try
        {
            NotifyOriginGatePocketReturned(pocketParent);
            Current.Game.DeinitAndRemoveMap(pocketMap, false);
            if (parent != null && Find.WorldObjects.Contains(parent))
            {
                Find.WorldObjects.Remove(parent);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[MagicFramework] Failed to clean up planar pocket map {pocketMap.uniqueID}: {ex}");
        }
    }

    private static void NotifyOriginGatePocketReturned(PlanarPocketParent pocketParent)
    {
        if (pocketParent == null)
        {
            return;
        }

        Thing originGate = FindOriginPlanarGate(pocketParent);
        CompPlanarGate gateComp = originGate?.TryGetComp<CompPlanarGate>();
        gateComp?.NotifyPlanarPocketReturned(pocketParent.dimension, pocketParent.ID);
    }

    public static string PlaneLabel(PlanarPocketParent pocketParent)
    {
        return pocketParent?.dimension?.LabelCap ?? "The planar realm";
    }

    private static bool HasAnySpawnedPawn(Map map)
    {
        IReadOnlyList<Pawn> pawns = map?.mapPawns?.AllPawnsSpawned;
        if (pawns == null)
        {
            return false;
        }

        for (int i = 0; i < pawns.Count; i++)
        {
            Pawn pawn = pawns[i];
            if (pawn != null && !pawn.Destroyed && pawn.Spawned)
            {
                return true;
            }
        }

        return false;
    }

    public static float ReturnCarryingCapacity(IEnumerable<Thing> selectedThings)
    {
        if (selectedThings == null)
        {
            return 0f;
        }

        float capacity = 0f;
        foreach (Thing thing in selectedThings)
        {
            if (thing is Pawn pawn && !pawn.Destroyed)
            {
                capacity += pawn.GetStatValue(StatDefOf.CarryingCapacity, true);
            }
        }

        return capacity;
    }

    public static float ReturnCarriedMass(IEnumerable<Thing> selectedThings)
    {
        if (selectedThings == null)
        {
            return 0f;
        }

        float mass = 0f;
        foreach (Thing thing in selectedThings)
        {
            if (thing == null || thing.Destroyed || thing is Pawn)
            {
                continue;
            }

            mass += thing.GetStatValue(StatDefOf.Mass, true) * thing.stackCount;
        }

        return mass;
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

public sealed class Dialog_PlanarPocketReturn : Window
{
    private readonly Map pocketMap;
    private readonly Action<bool> onClosed;
    private readonly List<Thing> candidates = new();
    private readonly HashSet<Thing> selected = new();
    private Vector2 scrollPosition;
    private bool completedReturn;

    public Dialog_PlanarPocketReturn(Map pocketMap, Action<bool> onClosed)
    {
        this.pocketMap = pocketMap;
        this.onClosed = onClosed;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        doCloseX = false;
        BuildCandidates();
    }

    public override Vector2 InitialSize => new(640f, 620f);

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        string planeLabel = PlanarMagicUtility.PlaneLabel(pocketMap?.Parent as PlanarPocketParent);
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 32f), $"{planeLabel} return");
        Text.Font = GameFont.Small;
        Rect descriptionRect = new(inRect.x, inRect.y + 38f, inRect.width, 52f);
        Widgets.Label(descriptionRect, $"{planeLabel} begins to collapse. Select travelers and supplies to return to the gate map.");

        float carriedMass = PlanarMagicUtility.ReturnCarriedMass(selected);
        float carryingCapacity = PlanarMagicUtility.ReturnCarryingCapacity(selected);
        bool overCapacity = carriedMass > carryingCapacity;
        Rect massRect = new(inRect.x, inRect.y + 88f, inRect.width, 24f);
        GUI.color = overCapacity ? Color.red : Color.white;
        Widgets.Label(massRect, $"Selected supplies: {carriedMass:0.#} / {carryingCapacity:0.#} kg");
        GUI.color = Color.white;

        Rect outRect = new(inRect.x, inRect.y + 118f, inRect.width, inRect.height - 172f);
        Rect viewRect = new(0f, 0f, outRect.width - 16f, candidates.Count * 30f + 8f);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        float y = 4f;
        for (int i = 0; i < candidates.Count; i++)
        {
            Thing thing = candidates[i];
            if (thing == null || thing.Destroyed)
            {
                continue;
            }

            bool isForcedPawn = thing is Pawn;
            bool isSelected = isForcedPawn || selected.Contains(thing);
            string label = thing is Pawn pawn ? $"{pawn.LabelShortCap} (capacity {pawn.GetStatValue(StatDefOf.CarryingCapacity, true):0.#} kg)" : $"{thing.LabelCap} ({thing.GetStatValue(StatDefOf.Mass, true) * thing.stackCount:0.#} kg)";
            if (thing.stackCount > 1)
            {
                label = $"{label} x{thing.stackCount}";
            }

            Widgets.CheckboxLabeled(new Rect(4f, y, viewRect.width - 8f, 28f), label, ref isSelected);
            if (isForcedPawn || isSelected)
            {
                selected.Add(thing);
            }
            else
            {
                selected.Remove(thing);
            }

            y += 30f;
        }

        Widgets.EndScrollView();

        float buttonY = inRect.yMax - 40f;
        if (Widgets.ButtonText(new Rect(inRect.x, buttonY, 160f, 36f), "Select all"))
        {
            selected.Clear();
            for (int i = 0; i < candidates.Count; i++)
            {
                selected.Add(candidates[i]);
            }
        }

        if (Widgets.ButtonText(new Rect(inRect.x + 170f, buttonY, 160f, 36f), "Clear items"))
        {
            selected.RemoveWhere(thing => thing is not Pawn);
        }

        if (overCapacity)
        {
            Widgets.DrawHighlight(new Rect(inRect.xMax - 170f, buttonY, 170f, 36f));
        }

        if (Widgets.ButtonText(new Rect(inRect.xMax - 170f, buttonY, 170f, 36f), "Return selected"))
        {
            TryReturnSelected();
        }
    }

    public override void PostClose()
    {
        base.PostClose();
        onClosed?.Invoke(completedReturn);
    }

    private void BuildCandidates()
    {
        candidates.Clear();
        selected.Clear();
        if (pocketMap == null)
        {
            return;
        }

        IReadOnlyList<Pawn> pawns = pocketMap.mapPawns?.AllPawnsSpawned;
        if (pawns != null)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn != null && !pawn.Destroyed && (pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony))
                {
                    candidates.Add(pawn);
                    selected.Add(pawn);
                }
            }
        }

        List<Thing> things = pocketMap.listerThings?.AllThings;
        if (things != null)
        {
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing != null && !thing.Destroyed && thing.def.category == ThingCategory.Item)
                {
                    candidates.Add(thing);
                }
            }
        }
    }

    private void TryReturnSelected()
    {
        ForceSelectAllPawns();
        bool hasPawn = false;
        foreach (Thing thing in selected)
        {
            if (thing is Pawn)
            {
                hasPawn = true;
                break;
            }
        }

        if (!hasPawn)
        {
            string planeLabel = PlanarMagicUtility.PlaneLabel(pocketMap?.Parent as PlanarPocketParent);
            Messages.Message($"At least one traveler must return from {planeLabel}.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        float carriedMass = PlanarMagicUtility.ReturnCarriedMass(selected);
        float carryingCapacity = PlanarMagicUtility.ReturnCarryingCapacity(selected);
        if (carriedMass > carryingCapacity)
        {
            Messages.Message($"The selected supplies are too heavy to return: {carriedMass:0.#} / {carryingCapacity:0.#} kg.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        if (PlanarMagicUtility.TryReturnSelectedFromPlanarPocket(pocketMap, selected.ToList()))
        {
            completedReturn = true;
            Close(false);
        }
    }

    private void ForceSelectAllPawns()
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i] is Pawn)
            {
                selected.Add(candidates[i]);
            }
        }
    }
}

public sealed class MapComponent_PlanarPocketRepair : MapComponent
{
    private bool repaired;
    private const int FallbackReturnDelayTicks = 7500;
    private const int LavaHazardIntervalTicks = 120;
    private const float DeepLavaBurnDamage = 8f;
    private const float ShallowLavaBurnDamage = 4f;
    private const float DeepLavaIgniteChance = 0.28f;
    private const float ShallowLavaIgniteChance = 0.12f;
    private int returnDueTick = -1;
    private bool returnPromptOpen;
    private bool returnCompleted;

    public MapComponent_PlanarPocketRepair(Map map)
        : base(map)
    {
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        TryRepair();
        EnsureReturnTimer();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref repaired, "repaired", false);
        Scribe_Values.Look(ref returnDueTick, "returnDueTick", -1);
        Scribe_Values.Look(ref returnPromptOpen, "returnPromptOpen", false);
        Scribe_Values.Look(ref returnCompleted, "returnCompleted", false);
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (!PlanarMagicUtility.IsPlanarPocketMap(map) || returnCompleted)
        {
            return;
        }

        EnsureReturnTimer();
        ApplyDeltaLavaHazards();
        if (returnDueTick >= 0 && Find.TickManager.TicksGame >= returnDueTick && !returnPromptOpen)
        {
            OpenReturnDialog();
        }
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

    private void EnsureReturnTimer()
    {
        if (!PlanarMagicUtility.IsPlanarPocketMap(map) || returnCompleted || returnDueTick >= 0)
        {
            return;
        }

        if (map.Parent is PlanarPocketParent pocketParent && pocketParent.forcedReturnTick > Find.TickManager.TicksGame)
        {
            returnDueTick = pocketParent.forcedReturnTick;
            return;
        }

        returnDueTick = Find.TickManager.TicksGame + FallbackReturnDelayTicks;
    }

    private void OpenReturnDialog()
    {
        returnPromptOpen = true;
        Find.WindowStack.Add(new Dialog_PlanarPocketReturn(map, completed =>
        {
            returnPromptOpen = false;
            if (completed)
            {
                returnCompleted = true;
            }
            else
            {
                returnDueTick = Find.TickManager.TicksGame + 250;
            }
        }));
    }

    private void ApplyDeltaLavaHazards()
    {
        if (Find.TickManager.TicksGame % LavaHazardIntervalTicks != 0)
        {
            return;
        }

        IReadOnlyList<Pawn> pawns = map?.mapPawns?.AllPawnsSpawned;
        if (pawns == null)
        {
            return;
        }

        for (int i = 0; i < pawns.Count; i++)
        {
            Pawn pawn = pawns[i];
            if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned)
            {
                continue;
            }

            TerrainDef terrain = map.terrainGrid.TerrainAt(pawn.Position);
            if (!TryResolveDeltaLavaHazard(terrain, out float damage, out float igniteChance, out float fireSize))
            {
                continue;
            }

            pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, damage));
            if (!pawn.Destroyed && !pawn.Dead && Rand.Chance(igniteChance))
            {
                pawn.TryAttachFire(fireSize, null);
            }
        }
    }

    private static bool TryResolveDeltaLavaHazard(TerrainDef terrain, out float damage, out float igniteChance, out float fireSize)
    {
        damage = 0f;
        igniteChance = 0f;
        fireSize = 0f;
        string defName = terrain?.defName;
        if (defName == "MFV_DeltaManaRiverDeep")
        {
            damage = DeepLavaBurnDamage;
            igniteChance = DeepLavaIgniteChance;
            fireSize = 0.45f;
            return true;
        }

        if (defName == "MFV_DeltaManaRiverShallow")
        {
            damage = ShallowLavaBurnDamage;
            igniteChance = ShallowLavaIgniteChance;
            fireSize = 0.25f;
            return true;
        }

        return false;
    }
}

public static class PlanarFlowFeatureUtility
{
    public static void Generate(Map map, PlanarFlowFeatureDef flow, int seed)
    {
        if (map == null || flow == null || flow.channelTerrain == null)
        {
            return;
        }

        HashSet<IntVec3> channelCells = new();
        HashSet<IntVec3> edgeCells = new();
        HashSet<IntVec3> bankCells = new();
        List<IntVec3> path = BuildPath(map, seed, flow);
        for (int i = 0; i < path.Count; i++)
        {
            IntVec3 center = path[i];
            int width = ResolveWidth(flow, seed, i);
            int edgeRadius = width + 1;
            int bankRadius = edgeRadius + Mathf.Max(0, flow.bankWidth);
            CellRect rect = CellRect.CenteredOn(center, bankRadius);
            foreach (IntVec3 cell in rect)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                float distance = Mathf.Sqrt((cell.x - center.x) * (cell.x - center.x) + (cell.z - center.z) * (cell.z - center.z));
                if (distance <= width)
                {
                    channelCells.Add(cell);
                    edgeCells.Remove(cell);
                    bankCells.Remove(cell);
                }
                else if (distance <= edgeRadius && flow.edgeTerrain != null)
                {
                    if (!channelCells.Contains(cell))
                    {
                        edgeCells.Add(cell);
                        bankCells.Remove(cell);
                    }
                }
                else if (distance <= bankRadius)
                {
                    if (!channelCells.Contains(cell) && !edgeCells.Contains(cell))
                    {
                        bankCells.Add(cell);
                    }
                }
            }
        }

        foreach (IntVec3 cell in channelCells)
        {
            ClearFlowCell(map, cell, flow);
            map.terrainGrid.SetTerrain(cell, flow.channelTerrain);
        }

        if (flow.edgeTerrain != null)
        {
            foreach (IntVec3 cell in edgeCells)
            {
                ClearFlowCell(map, cell, flow);
                map.terrainGrid.SetTerrain(cell, flow.edgeTerrain);
            }
        }

        if (flow.bankTerrain != null)
        {
            HashSet<IntVec3> shorelineCells = ResolveShorelineCells(map, channelCells, edgeCells, bankCells);
            foreach (IntVec3 cell in shorelineCells)
            {
                if (!PlanarMagicUtility.IsFluidTerrain(map.terrainGrid.TerrainAt(cell)))
                {
                    map.terrainGrid.SetTerrain(cell, flow.bankTerrain);
                }
            }

            ScatterBankThings(map, flow, shorelineCells, seed ^ 0x4D75A2B);
        }
    }

    private static HashSet<IntVec3> ResolveShorelineCells(Map map, HashSet<IntVec3> channelCells, HashSet<IntVec3> edgeCells, HashSet<IntVec3> candidateBankCells)
    {
        HashSet<IntVec3> shorelineCells = new();
        foreach (IntVec3 waterCell in channelCells)
        {
            AddAdjacentShorelineCells(map, waterCell, channelCells, edgeCells, candidateBankCells, shorelineCells);
        }

        foreach (IntVec3 waterCell in edgeCells)
        {
            AddAdjacentShorelineCells(map, waterCell, channelCells, edgeCells, candidateBankCells, shorelineCells);
        }

        return shorelineCells;
    }

    private static void AddAdjacentShorelineCells(Map map, IntVec3 waterCell, HashSet<IntVec3> channelCells, HashSet<IntVec3> edgeCells, HashSet<IntVec3> candidateBankCells, HashSet<IntVec3> shorelineCells)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0)
                {
                    continue;
                }

                IntVec3 cell = new(waterCell.x + dx, 0, waterCell.z + dz);
                if (!cell.InBounds(map) || channelCells.Contains(cell) || edgeCells.Contains(cell))
                {
                    continue;
                }

                if (candidateBankCells.Contains(cell))
                {
                    shorelineCells.Add(cell);
                }
            }
        }
    }

    private static List<IntVec3> BuildPath(Map map, int seed, PlanarFlowFeatureDef flow)
    {
        List<IntVec3> path = new();
        bool horizontal = (StableHash(seed, 0x51E2) & 1) == 0;
        int length = horizontal ? map.Size.x : map.Size.z;
        int crossSize = horizontal ? map.Size.z : map.Size.x;
        int baseCross = crossSize / 2 + StableOffset(seed, 0x21A7, Mathf.Max(2, crossSize / 7));
        float waveA = 0.05f + StableRange(seed, 0x7A10) * 0.09f;
        float waveB = 0.11f + StableRange(seed, 0x1F36) * 0.13f;
        float phaseA = StableRange(seed, 0x33BB) * Mathf.PI * 2f;
        float phaseB = StableRange(seed, 0x6D91) * Mathf.PI * 2f;
        float amplitude = Mathf.Clamp01(flow.meanderStrength) * crossSize * 0.28f;

        for (int along = 0; along < length; along++)
        {
            float cross = baseCross
                + Mathf.Sin(along * waveA + phaseA) * amplitude
                + Mathf.Sin(along * waveB + phaseB) * amplitude * 0.45f;
            int crossInt = Mathf.Clamp(Mathf.RoundToInt(cross), 2, crossSize - 3);
            path.Add(horizontal ? new IntVec3(along, 0, crossInt) : new IntVec3(crossInt, 0, along));
        }

        return path;
    }

    private static int ResolveWidth(PlanarFlowFeatureDef flow, int seed, int index)
    {
        int min = Mathf.Max(1, flow.minWidth);
        int max = Mathf.Max(min, flow.maxWidth);
        if (min == max)
        {
            return min;
        }

        float wave = (Mathf.Sin(index * 0.075f + StableRange(seed, 0x17C3) * Mathf.PI * 2f) + 1f) * 0.5f;
        return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(min, max, wave)), min, max);
    }

    private static void ClearFlowCell(Map map, IntVec3 cell, PlanarFlowFeatureDef flow)
    {
        List<Thing> things = cell.GetThingList(map);
        for (int i = things.Count - 1; i >= 0; i--)
        {
            Thing thing = things[i];
            ThingDef def = thing?.def;
            if (def == null)
            {
                continue;
            }

            if (def.category == ThingCategory.Filth
                || (flow.clearPlantsInChannel && def.category == ThingCategory.Plant)
                || (flow.clearBuildingsInChannel && def.category == ThingCategory.Building))
            {
                thing.Destroy();
            }
        }
    }

    private static void ScatterBankThings(Map map, PlanarFlowFeatureDef flow, IEnumerable<IntVec3> bankCells, int seed)
    {
        List<IntVec3> cells = bankCells?.ToList();
        if (flow.bankThings.NullOrEmpty() || cells.NullOrEmpty() || flow.bankThingDensity <= 0f)
        {
            return;
        }

        List<ThingDef> things = flow.bankThings.FindAll(def => def != null);
        if (things.Count == 0)
        {
            return;
        }

        int targetCount = Mathf.RoundToInt(cells.Count * flow.bankThingDensity);
        int spawned = 0;
        cells.Sort((a, b) => StableHash(seed, a.x * 397 ^ a.z * 7919).CompareTo(StableHash(seed, b.x * 397 ^ b.z * 7919)));
        for (int i = 0; i < cells.Count && spawned < targetCount; i++)
        {
            IntVec3 cell = cells[i];
            if (!cell.Standable(map) || PlanarMagicUtility.IsFluidTerrain(map.terrainGrid.TerrainAt(cell)) || !cell.GetThingList(map).NullOrEmpty())
            {
                continue;
            }

            ThingDef thingDef = things[StableIndex(seed, i * 92821, things.Count)];
            Thing thing = ThingMaker.MakeThing(thingDef);
            if (thing is Plant plant)
            {
                plant.Growth = 0.45f + StableRange(seed, i * 17033) * 0.5f;
            }

            GenSpawn.Spawn(thing, cell, map);
            spawned++;
        }
    }

    private static int StableOffset(int seed, int salt, int radius)
    {
        if (radius <= 0)
        {
            return 0;
        }

        return StableIndex(seed, salt, radius * 2 + 1) - radius;
    }

    private static int StableIndex(int seed, int salt, int count)
    {
        if (count <= 1)
        {
            return 0;
        }

        return (StableHash(seed, salt) & int.MaxValue) % count;
    }

    private static float StableRange(int seed, int salt)
    {
        return (StableHash(seed, salt) & int.MaxValue) / (float)int.MaxValue;
    }

    private static int StableHash(int seed, int salt)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 397) ^ seed;
            hash = (hash * 397) ^ salt;
            hash ^= hash >> 16;
            hash *= -2048144789;
            hash ^= hash >> 13;
            hash *= -1028477387;
            hash ^= hash >> 16;
            return hash;
        }
    }
}

public sealed class GenStep_PlanarPocket : GenStep
{
    public PlanarDimensionDef dimension;
    public List<TerrainDef> terrainOptions;
    public List<ThingDef> plantOptions;
    public List<ThingDef> chunkOptions;
    public List<ThingDef> mineableOptions;
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

        PlanarDimensionDef resolvedDimension = dimension ?? (map.Parent as PlanarPocketParent)?.dimension;
        List<TerrainDef> terrains = ResolveTerrains(resolvedDimension);
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
        PlanarMagicUtility.GeneratePlanarFlows(map, resolvedDimension, seed ^ 0x59D3A41);
        int mapArea = Math.Max(1, map.Size.x * map.Size.z);
        PlanarMagicUtility.ScatterPlanarPlants(map, seed ^ 0x6E61C457, Math.Max(20, (int)(mapArea * (resolvedDimension?.plantDensity ?? plantDensity))), ResolvePlants(resolvedDimension));
        PlanarMagicUtility.ScatterPlanarMineables(map, seed ^ 0x280E9C2D, Math.Max(6, (int)(mapArea * (resolvedDimension?.mineableDensity ?? mineableDensity))), ResolveMineables(resolvedDimension));
        PlanarMagicUtility.ScatterPlanarStoneChunks(map, seed ^ 0x3F62B7A9, Math.Max(8, (int)(mapArea * (resolvedDimension?.chunkDensity ?? chunkDensity))), ResolveChunks(resolvedDimension));
        PlanarMagicUtility.ScatterPlanarPlantClusters(map, resolvedDimension, seed ^ 0x19B64C7D);
    }

    private List<TerrainDef> ResolveTerrains(PlanarDimensionDef resolvedDimension)
    {
        List<TerrainDef> terrains = terrainOptions?.FindAll(def => def != null);
        if (!terrains.NullOrEmpty())
        {
            return terrains;
        }

        terrains = PlanarMagicUtility.ResolvePlanarTerrains(resolvedDimension);
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

    private List<ThingDef> ResolvePlants(PlanarDimensionDef resolvedDimension)
    {
        return plantOptions?.FindAll(def => def != null) ?? PlanarMagicUtility.ResolvePlanarPlants(resolvedDimension);
    }

    private List<ThingDef> ResolveChunks(PlanarDimensionDef resolvedDimension)
    {
        return chunkOptions?.FindAll(def => def != null) ?? PlanarMagicUtility.ResolvePlanarChunks(resolvedDimension);
    }

    private List<ThingDef> ResolveMineables(PlanarDimensionDef resolvedDimension)
    {
        return mineableOptions?.FindAll(def => def != null) ?? PlanarMagicUtility.ResolvePlanarMineables(resolvedDimension);
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
        if (map?.Parent is PlanarPocketParent pocketParent && pocketParent.generationSeed != 0)
        {
            return pocketParent.generationSeed;
        }

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
    [LudeonTK.DebugAction("MagicFramework - Planar", "Spawn Planar Pocket Near Current Map", actionType = LudeonTK.DebugActionType.Action, allowedGameStates = LudeonTK.AllowedGameStates.PlayingOnMap)]
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

