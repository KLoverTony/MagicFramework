using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MFVanilla.Core;

public static class DebugActions_ArcaneSites
{
    [DebugAction("MFVanilla - Arcane Sites", "Spawn Arcane Cache Site Near Current Map", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void SpawnArcaneCacheSiteNearCurrentMap()
    {
        SpawnSiteNearCurrentMap("MFV_ArcaneCache", "arcane cache site");
    }

    [DebugAction("MFVanilla - Arcane Sites", "Spawn Arcane Cache Showcase Near Current Map", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void SpawnArcaneCacheShowcaseNearCurrentMap()
    {
        SpawnSiteNearCurrentMap("MFV_ArcaneCache_Showcase", "arcane cache showcase");
    }

    private static void SpawnSiteNearCurrentMap(string sitePartDefName, string label)
    {
        Map map = Find.CurrentMap;
        if (map == null)
        {
            Messages.Message("No current map is available.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        SitePartDef sitePartDef = DefDatabase<SitePartDef>.GetNamedSilentFail(sitePartDefName);
        if (sitePartDef == null)
        {
            Messages.Message($"{sitePartDefName} SitePartDef could not be found.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        if (!TileFinder.TryFindNewSiteTile(out PlanetTile tile, map.Tile, 3, 12))
        {
            Messages.Message($"Could not find a nearby tile for an {label}.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        Site site = SiteMaker.MakeSite(sitePartDef, tile, null, ifHostileThenMustRemainHostile: true, threatPoints: 300f);
        if (site == null || site.parts.NullOrEmpty())
        {
            Messages.Message($"Failed to create an {label} with a valid site part.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        Find.WorldObjects.Add(site);
        Find.WorldSelector.ClearSelection();
        Find.WorldSelector.Select(site);
        Messages.Message($"Spawned {label} at tile {tile}.", MessageTypeDefOf.PositiveEvent, false);
    }

    [DebugAction("MFVanilla - Arcane Sites", "Remove Bare Empty Sites", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
    public static void RemoveBareEmptySites()
    {
        List<WorldObject> toRemove = new();
        List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
        for (int i = 0; i < worldObjects.Count; i++)
        {
            if (worldObjects[i] is Site site && site.parts.NullOrEmpty())
            {
                toRemove.Add(site);
            }
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            Find.WorldObjects.Remove(toRemove[i]);
        }

        Messages.Message($"Removed {toRemove.Count} bare empty site(s).", MessageTypeDefOf.NeutralEvent, false);
    }
}
