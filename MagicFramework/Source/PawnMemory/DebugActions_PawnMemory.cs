using LudeonTK;
using MagicFramework.Core;
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

        [DebugAction("MagicFramework", "Set haunting cap to 10", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void SetHauntingCapToDefault()
        {
            MagicFrameworkSettings.Current.maxHauntingsPerMap = HauntingEvaluator.DefaultMaxHauntingsPerMap;
            LoadedModManager.GetMod<MagicFrameworkMod>()?.WriteSettings();
            Messages.Message("MagicFramework haunting cap set to 10 per map.", MessageTypeDefOf.NeutralEvent, historical: false);
        }

        [DebugAction("MagicFramework", "Reevaluate pending hauntings", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void ReevaluatePendingHauntings()
        {
            WorldComponent_PawnMemories registry = WorldComponent_PawnMemories.Instance;
            if (registry == null)
                return;

            int count = 0;
            foreach (PawnMemoryRecord record in registry.GetAllRecords())
            {
                if (record.state != PawnMemoryState.DeadPendingRites || record.rituallyReleased)
                    continue;

                HauntingEvaluator.ForceReevaluate(record, registry.GetAllRecords());
                count++;
            }

            Messages.Message("Reevaluated " + count + " pending haunting record(s).", MessageTypeDefOf.NeutralEvent, historical: false);
        }
    }
}
