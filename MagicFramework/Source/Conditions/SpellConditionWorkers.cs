using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Targeting;
using Verse;

namespace MagicFramework.Conditions;

public sealed class AllOfConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        AllOfConditionDef allOfDef = conditionDef as AllOfConditionDef;
        if (allOfDef?.conditions == null || allOfDef.conditions.Count == 0)
        {
            return false;
        }

        foreach (SpellConditionDef childCondition in allOfDef.conditions)
        {
            if (!SpellConditionEvaluator.Evaluate(context, childCondition))
            {
                return false;
            }
        }

        return true;
    }
}

public sealed class AnyOfConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        AnyOfConditionDef anyOfDef = conditionDef as AnyOfConditionDef;
        if (anyOfDef?.conditions == null || anyOfDef.conditions.Count == 0)
        {
            return false;
        }

        foreach (SpellConditionDef childCondition in anyOfDef.conditions)
        {
            if (SpellConditionEvaluator.Evaluate(context, childCondition))
            {
                return true;
            }
        }

        return false;
    }
}

public sealed class NotConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        NotConditionDef notDef = conditionDef as NotConditionDef;
        return !SpellConditionEvaluator.Evaluate(context, notDef?.condition);
    }
}

public sealed class TargetExistsConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        TargetExistsConditionDef targetExistsDef = conditionDef as TargetExistsConditionDef;
        LocalTargetInfo target = SpellConditionUtility.ResolveTarget(context, targetExistsDef?.targetSource ?? SpellConditionTargetSource.CurrentTarget);
        return target.IsValid;
    }
}

public sealed class TargetIsPawnConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        TargetIsPawnConditionDef targetIsPawnDef = conditionDef as TargetIsPawnConditionDef;
        return SpellConditionUtility.ResolvePawn(context, targetIsPawnDef?.targetSource ?? SpellConditionTargetSource.CurrentTarget) != null;
    }
}

public sealed class PawnAffinityConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        PawnAffinityConditionDef affinityDef = conditionDef as PawnAffinityConditionDef;
        Pawn targetPawn = SpellConditionUtility.ResolvePawn(context, affinityDef?.targetSource ?? SpellConditionTargetSource.CurrentTarget);
        if (targetPawn == null)
        {
            return false;
        }

        Thing caster = context?.caster;
        return TargetQueryUtility.MatchesPawnAffinity(caster, targetPawn, affinityDef?.pawnAffinity ?? SpellPawnAffinity.Foe);
    }
}

public sealed class HasHediffConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        HasHediffConditionDef hasHediffDef = conditionDef as HasHediffConditionDef;
        Pawn targetPawn = SpellConditionUtility.ResolvePawn(context, hasHediffDef?.targetSource ?? SpellConditionTargetSource.CurrentTarget);
        if (targetPawn?.health?.hediffSet == null || string.IsNullOrWhiteSpace(hasHediffDef?.hediffDef))
        {
            return false;
        }

        HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hasHediffDef.hediffDef);
        return hediffDef != null && targetPawn.health.hediffSet.HasHediff(hediffDef);
    }
}

public sealed class HealthBelowConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        HealthBelowConditionDef healthBelowDef = conditionDef as HealthBelowConditionDef;
        Pawn targetPawn = SpellConditionUtility.ResolvePawn(context, healthBelowDef?.targetSource ?? SpellConditionTargetSource.CurrentTarget);
        if (targetPawn?.health?.summaryHealth == null)
        {
            return false;
        }

        float threshold = healthBelowDef?.thresholdPercent ?? 0.5f;
        threshold = threshold < 0f ? 0f : threshold > 1f ? 1f : threshold;
        return targetPawn.health.summaryHealth.SummaryHealthPercent < threshold;
    }
}

public sealed class TargetDownedConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        TargetDownedConditionDef downedDef = conditionDef as TargetDownedConditionDef;
        Pawn targetPawn = SpellConditionUtility.ResolvePawn(context, downedDef?.targetSource ?? SpellConditionTargetSource.CurrentTarget);
        return targetPawn != null && targetPawn.Downed;
    }
}

public sealed class TargetDeadConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        TargetDeadConditionDef deadDef = conditionDef as TargetDeadConditionDef;
        Pawn targetPawn = SpellConditionUtility.ResolvePawn(context, deadDef?.targetSource ?? SpellConditionTargetSource.CurrentTarget);
        return targetPawn != null && targetPawn.Dead;
    }
}

public sealed class CellOccupiedConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        CellOccupiedConditionDef cellOccupiedDef = conditionDef as CellOccupiedConditionDef;
        IntVec3 cell = SpellConditionUtility.ResolveCell(context, cellOccupiedDef?.cellSource ?? SpellConditionCellSource.CurrentCell);
        Map map = context?.map;
        return map != null && cell.IsValid && cell.InBounds(map) && cell.GetThingList(map).Count > 0;
    }
}

public sealed class DistanceConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        DistanceConditionDef distanceDef = conditionDef as DistanceConditionDef;
        IntVec3 fromCell = SpellConditionUtility.ResolveCell(context, distanceDef?.fromCellSource ?? SpellConditionCellSource.CasterCell);
        IntVec3 toCell = SpellConditionUtility.ResolveCell(context, distanceDef?.toCellSource ?? SpellConditionCellSource.CurrentCell);
        if (!fromCell.IsValid || !toCell.IsValid)
        {
            return false;
        }

        float distance = fromCell.DistanceTo(toCell);
        float minDistance = distanceDef?.minDistance ?? -1f;
        float maxDistance = distanceDef?.maxDistance ?? -1f;
        if (minDistance >= 0f && distance < minDistance)
        {
            return false;
        }

        return maxDistance < 0f || distance <= maxDistance;
    }
}

public sealed class LineOfSightConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        LineOfSightConditionDef lineOfSightDef = conditionDef as LineOfSightConditionDef;
        IntVec3 fromCell = SpellConditionUtility.ResolveCell(context, lineOfSightDef?.fromCellSource ?? SpellConditionCellSource.CasterCell);
        IntVec3 toCell = SpellConditionUtility.ResolveCell(context, lineOfSightDef?.toCellSource ?? SpellConditionCellSource.CurrentCell);
        Map map = context?.map;
        return map != null
            && fromCell.IsValid
            && toCell.IsValid
            && fromCell.InBounds(map)
            && toCell.InBounds(map)
            && GenSight.LineOfSight(fromCell, toCell, map);
    }
}

public sealed class ThingCategoryConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        ThingCategoryConditionDef thingCategoryDef = conditionDef as ThingCategoryConditionDef;
        Thing targetThing = SpellConditionUtility.ResolveTarget(context, thingCategoryDef?.targetSource ?? SpellConditionTargetSource.CurrentTarget).Thing;
        return targetThing != null && targetThing.def != null && targetThing.def.category == (thingCategoryDef?.category ?? ThingCategory.Item);
    }
}

public sealed class RandomChanceConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        RandomChanceConditionDef chanceDef = conditionDef as RandomChanceConditionDef;
        float chance = chanceDef?.chance ?? 1f;
        chance = chance < 0f ? 0f : chance > 1f ? 1f : chance;
        return SpellDeterministicRandom.Chance(chance, SpellDeterministicRandom.ContextSalt(context, "RandomChanceCondition"));
    }
}

public sealed class PowerTierConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        PowerTierConditionDef powerTierDef = conditionDef as PowerTierConditionDef;
        int tier = context?.power?.tier ?? 0;
        return tier >= (powerTierDef?.minTier ?? int.MinValue)
            && tier <= (powerTierDef?.maxTier ?? int.MaxValue);
    }
}

public sealed class SpellPowerConditionWorker : SpellConditionWorker
{
    public override bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        SpellPowerConditionDef powerDef = conditionDef as SpellPowerConditionDef;
        float power = context?.power?.value ?? 0f;
        return power >= (powerDef?.minPower ?? float.MinValue)
            && power <= (powerDef?.maxPower ?? float.MaxValue);
    }
}

internal static class SpellConditionEvaluator
{
    public static bool Evaluate(SpellContext context, SpellConditionDef conditionDef)
    {
        if (conditionDef == null)
        {
            return false;
        }

        SpellConditionWorker worker = conditionDef.CreateWorker();
        return worker != null && worker.Evaluate(context, conditionDef);
    }
}

internal static class SpellConditionUtility
{
    public static LocalTargetInfo ResolveTarget(SpellContext context, SpellConditionTargetSource targetSource)
    {
        return targetSource switch
        {
            SpellConditionTargetSource.InitialTarget => context?.initialTarget ?? LocalTargetInfo.Invalid,
            SpellConditionTargetSource.Caster => context?.caster != null ? new LocalTargetInfo(context.caster) : LocalTargetInfo.Invalid,
            _ => context?.currentTarget ?? LocalTargetInfo.Invalid
        };
    }

    public static Pawn ResolvePawn(SpellContext context, SpellConditionTargetSource targetSource)
    {
        return ResolveTarget(context, targetSource).Thing as Pawn;
    }

    public static IntVec3 ResolveCell(SpellContext context, SpellConditionCellSource cellSource)
    {
        return cellSource switch
        {
            SpellConditionCellSource.CurrentTargetCell => context?.currentTarget.Cell ?? IntVec3.Invalid,
            SpellConditionCellSource.InitialTargetCell => context?.initialTarget.Cell ?? IntVec3.Invalid,
            SpellConditionCellSource.CasterCell => context?.caster?.Position ?? IntVec3.Invalid,
            _ => context?.currentCell ?? IntVec3.Invalid
        };
    }
}
