using LudeonTK;
using RimWorld;
using Verse;
using MagicFramework.Debug;

namespace MagicFramework.PawnMemory
{
    public static class DebugActions_PawnMemory
    {
        [DebugAction("MagicFramework", "Pawn Memory Viewer", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void OpenPawnMemoryViewer()
        {
            Find.WindowStack.Add(new Dialog_PawnMemoryViewer());
        }
    }
}
