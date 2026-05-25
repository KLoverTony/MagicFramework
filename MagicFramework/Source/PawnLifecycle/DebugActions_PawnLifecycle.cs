using LudeonTK;
using RimWorld;
using Verse;

namespace MagicFramework.PawnLifecycle;

public static class DebugActions_PawnLifecycle
{
    [DebugAction("MagicFramework", "Log selected pawn lifecycle", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void LogSelectedPawnLifecycle(Pawn pawn)
    {
        Log.Message(PawnLifecycleUtility.GetDebugSummary(pawn));
    }

    [DebugAction("MagicFramework", "Log map pawn lifecycle profiles", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void LogMapPawnLifecycleProfiles()
    {
        Map map = Find.CurrentMap;
        if (map?.mapPawns?.AllPawnsSpawned == null)
        {
            Log.Message("[MagicFramework] No current map was available for pawn lifecycle inspection.");
            return;
        }

        int count = 0;
        foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
        {
            if (!PawnLifecycleUtility.HasLifecycle(pawn))
            {
                continue;
            }

            Log.Message(PawnLifecycleUtility.GetDebugSummary(pawn));
            count++;
        }

        Messages.Message("Logged " + count + " pawn lifecycle profile(s).", MessageTypeDefOf.NeutralEvent, historical: false);
    }
}
