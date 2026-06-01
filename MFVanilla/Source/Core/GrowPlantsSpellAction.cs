using System.Collections.Generic;
using System.Linq;
using MagicFramework.Actions;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Targeting;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class GrowPlantsActionDef : SpellActionDef
{
    public TargetQueryCenterSource centerSource = TargetQueryCenterSource.CurrentCell;
    public float radius = 4f;
    public ScalableFloatDef scalableRadius;
    public float growthPerPulse = 0.08f;
    public ScalableFloatDef scalableGrowthPerPulse;
    public int maxPlantsPerPulse = 30;
    public ScalableFloatDef scalableMaxPlantsPerPulse;
    public bool cropsOnly = true;
    public bool skipBlighted = true;
    public string fleckDef = "PsycastAreaEffect";
    public float fleckScale = 0.45f;
    public int maxFlecksPerPulse = 8;

    public override SpellActionWorker CreateWorker() => new GrowPlantsActionWorker();
}

public sealed class GrowPlantsActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        GrowPlantsActionDef growDef = actionDef as GrowPlantsActionDef;
        if (growDef == null || context?.map == null)
        {
            return;
        }

        IntVec3 center = ResolveCenter(context, growDef.centerSource);
        if (!center.IsValid || !center.InBounds(context.map))
        {
            return;
        }

        float radius = Mathf.Max(0.1f, SpellEnhancementUtility.ResolveScalableRadius(context, growDef.radius, growDef.scalableRadius));
        float growthPerPulse = Mathf.Max(0f, SpellPowerUtility.ResolveScalableFloat(context, growDef.growthPerPulse, growDef.scalableGrowthPerPulse));
        int maxPlants = Mathf.Max(1, SpellPowerUtility.ResolveScalableInt(context, growDef.maxPlantsPerPulse, growDef.scalableMaxPlantsPerPulse));
        if (growthPerPulse <= 0f)
        {
            return;
        }

        List<Plant> plants = context.map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
            .OfType<Plant>()
            .Where(plant => IsEligiblePlant(plant, center, radius, growDef))
            .OrderBy(plant => plant.Growth)
            .ThenBy(plant => plant.Position.DistanceToSquared(center))
            .Take(maxPlants)
            .ToList();

        FleckDef fleck = DefDatabase<FleckDef>.GetNamedSilentFail(growDef.fleckDef);
        int flecksThrown = 0;
        for (int i = 0; i < plants.Count; i++)
        {
            Plant plant = plants[i];
            plant.Growth = Mathf.Min(1f, plant.Growth + growthPerPulse);
            plant.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlagDefOf.Things);

            if (fleck != null && flecksThrown < growDef.maxFlecksPerPulse)
            {
                FleckMaker.Static(plant.TrueCenter(), plant.Map, fleck, growDef.fleckScale);
                flecksThrown++;
            }
        }
    }

    private static bool IsEligiblePlant(Plant plant, IntVec3 center, float radius, GrowPlantsActionDef growDef)
    {
        if (plant == null || !plant.Spawned || plant.Destroyed || plant.Map == null)
        {
            return false;
        }

        if (plant.Growth >= 0.999f || plant.LifeStage == PlantLifeStage.Mature)
        {
            return false;
        }

        if (growDef.skipBlighted && plant.Blighted)
        {
            return false;
        }

        if (growDef.cropsOnly && !plant.IsCrop)
        {
            return false;
        }

        return plant.Position.InHorDistOf(center, radius);
    }

    private static IntVec3 ResolveCenter(SpellContext context, TargetQueryCenterSource source)
    {
        return source switch
        {
            TargetQueryCenterSource.Caster => context.caster?.Position ?? IntVec3.Invalid,
            TargetQueryCenterSource.CurrentTarget => context.currentTarget.IsValid ? context.currentTarget.Cell : IntVec3.Invalid,
            TargetQueryCenterSource.InitialTarget => context.initialTarget.IsValid ? context.initialTarget.Cell : IntVec3.Invalid,
            _ => context.currentCell.IsValid ? context.currentCell : context.currentTarget.Cell
        };
    }
}
