using RimWorld;
using Verse;

namespace MagicFramework.Execution;

public static class SpellTerrainUtility
{
    public static bool IsWaterCell(Map map, IntVec3 cell)
    {
        return map != null
            && cell.IsValid
            && cell.InBounds(map)
            && IsWaterTerrain(map.terrainGrid.TerrainAt(cell));
    }

    public static bool IsWaterTerrain(TerrainDef terrainDef)
    {
        if (terrainDef == null)
        {
            return false;
        }

        if (terrainDef.waterBodyType != WaterBodyType.None)
        {
            return true;
        }

        string defName = terrainDef.defName;
        return defName != null
            && (defName.StartsWith("Water") || defName == "Marsh" || defName == "Mud");
    }
}
