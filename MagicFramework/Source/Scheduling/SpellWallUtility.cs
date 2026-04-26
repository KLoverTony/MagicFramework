using System.Collections.Generic;
using MagicFramework.Context;
using UnityEngine;
using Verse;

namespace MagicFramework.Scheduling;

internal static class SpellWallUtility
{
    public static List<IntVec3> BuildWallCells(SpellContext context, IntVec3 center, int wallLength)
    {
        List<IntVec3> cells = new();
        if (context?.map == null || !center.IsValid)
        {
            return cells;
        }

        Vector2 origin = ToVector2(context.caster?.Position ?? center);
        Vector2 target = ToVector2(center);
        Vector2 direction = target - origin;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.right;
        }

        direction.Normalize();
        Vector2 perpendicular = new(-direction.y, direction.x);
        int length = Mathf.Max(1, wallLength);
        int halfLength = length / 2;

        HashSet<IntVec3> uniqueCells = new();
        for (int i = 0; i < length; i++)
        {
            float offset = i - halfLength;
            if (length % 2 == 0)
            {
                offset += 0.5f;
            }

            Vector2 cellVector = target + (perpendicular * offset);
            IntVec3 cell = new(Mathf.RoundToInt(cellVector.x - 0.5f), 0, Mathf.RoundToInt(cellVector.y - 0.5f));
            if (!cell.InBounds(context.map))
            {
                continue;
            }

            if (uniqueCells.Add(cell))
            {
                cells.Add(cell);
            }
        }

        return cells;
    }

    private static Vector2 ToVector2(IntVec3 cell)
    {
        return new Vector2(cell.x + 0.5f, cell.z + 0.5f);
    }
}
