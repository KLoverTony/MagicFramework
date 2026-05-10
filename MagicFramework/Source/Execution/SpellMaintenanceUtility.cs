using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Execution;

public static class SpellMaintenanceUtility
{
    public static bool IsMaintenanceBroken(
        SpellMaintenanceDef maintenance,
        Thing caster,
        Thing target,
        Map map,
        IntVec3 anchorCell,
        out string reason)
    {
        reason = null;
        if (maintenance?.profiles == null || maintenance.profiles.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < maintenance.profiles.Count; i++)
        {
            SpellMaintenanceProfile profile = maintenance.profiles[i];
            if (IsProfileBroken(profile, maintenance, caster, target, map, anchorCell, out reason))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsProfileBroken(
        SpellMaintenanceProfile profile,
        SpellMaintenanceDef maintenance,
        Thing caster,
        Thing target,
        Map map,
        IntVec3 anchorCell,
        out string reason)
    {
        reason = null;
        switch (profile)
        {
            case SpellMaintenanceProfile.CasterValid:
                return IsCasterInvalid(caster, out reason);
            case SpellMaintenanceProfile.CasterConscious:
                return IsCasterInvalid(caster, out reason) || IsPawnDeadOrDowned(caster as Pawn, "caster", out reason);
            case SpellMaintenanceProfile.CasterFocused:
                return IsCasterInvalid(caster, out reason) || IsPawnNotFocused(caster as Pawn, "caster", out reason);
            case SpellMaintenanceProfile.TargetValid:
                return IsTargetInvalid(target, out reason);
            case SpellMaintenanceProfile.TargetConscious:
                return IsTargetInvalid(target, out reason) || IsPawnDeadOrDowned(target as Pawn, "target", out reason);
            case SpellMaintenanceProfile.Tethered:
                return IsTetherBroken(maintenance, caster, target, anchorCell, out reason);
            case SpellMaintenanceProfile.LineOfSight:
                return IsLineOfSightBroken(caster, target, map, anchorCell, out reason);
            case SpellMaintenanceProfile.Anchored:
                return IsAnchorInvalid(map, anchorCell, out reason);
            default:
                return false;
        }
    }

    private static bool IsCasterInvalid(Thing caster, out string reason)
    {
        reason = null;
        if (caster == null || caster.Destroyed)
        {
            reason = "caster invalid";
            return true;
        }

        if (caster is Pawn pawn && pawn.Dead)
        {
            reason = "caster dead";
            return true;
        }

        return false;
    }

    private static bool IsTargetInvalid(Thing target, out string reason)
    {
        reason = null;
        if (target == null || target.Destroyed)
        {
            reason = "target invalid";
            return true;
        }

        if (target is Pawn pawn && pawn.Dead)
        {
            reason = "target dead";
            return true;
        }

        return false;
    }

    private static bool IsPawnDeadOrDowned(Pawn pawn, string label, out string reason)
    {
        reason = null;
        if (pawn == null)
        {
            return false;
        }

        if (pawn.Dead)
        {
            reason = $"{label} dead";
            return true;
        }

        if (pawn.Downed)
        {
            reason = $"{label} downed";
            return true;
        }

        return false;
    }

    private static bool IsPawnNotFocused(Pawn pawn, string label, out string reason)
    {
        if (IsPawnDeadOrDowned(pawn, label, out reason))
        {
            return true;
        }

        if (pawn == null)
        {
            reason = null;
            return false;
        }

        if (pawn.stances?.stunner?.Stunned == true)
        {
            reason = $"{label} stunned";
            return true;
        }

        if (pawn.MentalState != null)
        {
            reason = $"{label} mental state";
            return true;
        }

        return false;
    }

    private static bool IsTetherBroken(SpellMaintenanceDef maintenance, Thing caster, Thing target, IntVec3 anchorCell, out string reason)
    {
        reason = null;
        if (caster == null || caster.Destroyed)
        {
            reason = "caster invalid";
            return true;
        }

        IntVec3 tetherCell = ResolveTargetCell(maintenance, target, anchorCell);
        if (!tetherCell.IsValid)
        {
            reason = "tether target invalid";
            return true;
        }

        if (maintenance.maxRange > 0f && caster.Position.DistanceTo(tetherCell) > maintenance.maxRange)
        {
            reason = "target out of range";
            return true;
        }

        return false;
    }

    private static bool IsLineOfSightBroken(Thing caster, Thing target, Map map, IntVec3 anchorCell, out string reason)
    {
        reason = null;
        if (caster == null || caster.Destroyed)
        {
            reason = "caster invalid";
            return true;
        }

        Map casterMap = caster.MapHeld ?? map;
        IntVec3 targetCell = target != null && !target.Destroyed ? target.Position : anchorCell;
        if (casterMap == null || !targetCell.IsValid || !GenSight.LineOfSight(caster.Position, targetCell, casterMap))
        {
            reason = "line of sight lost";
            return true;
        }

        return false;
    }

    private static bool IsAnchorInvalid(Map map, IntVec3 anchorCell, out string reason)
    {
        reason = null;
        if (map == null || !anchorCell.IsValid || !anchorCell.InBounds(map))
        {
            reason = "anchor invalid";
            return true;
        }

        return false;
    }

    private static IntVec3 ResolveTargetCell(SpellMaintenanceDef maintenance, Thing target, IntVec3 anchorCell)
    {
        if (maintenance?.useInitialTargetCell == true)
        {
            return anchorCell;
        }

        return target != null && !target.Destroyed ? target.Position : anchorCell;
    }
}
