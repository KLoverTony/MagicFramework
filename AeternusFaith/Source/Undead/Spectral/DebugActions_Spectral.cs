using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    public static class DebugActions_Spectral
    {
        [DebugAction("AeternusFaith - Spectral", "Spawn Test Spirit Here", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnTestSpirit()
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = UI.MouseCell();

            if (map == null || !cell.InBounds(map)) return;

            var comp = map.GetComponent<MapComponent_SpectralEntities>();
            if (comp == null) return;

            SpectralEntity spirit = new SpectralEntity(map)
            {
                label = "Test Spirit " + Verse.Rand.Range(100, 999).ToString(),
                state = SpectralState.WanderingUnseen,
                anchorPosition = cell,
                lastKnownPosition = cell,
                pawnKind = PawnKindDefOf.Colonist,
                faction = Faction.OfPlayer
            };

            comp.AddSpirit(spirit);
            Messages.Message($"Spawned spectral entity '{spirit.label}' at {cell}.", MessageTypeDefOf.TaskCompletion, false);
            Log.Message($"[AeternusFaith] Spawned spectral entity '{spirit.label}' at {cell}.");
        }

        [DebugAction("AeternusFaith - Spectral", "Force Spirit Haunt", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ForceSpiritHaunt()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            var comp = map.GetComponent<MapComponent_SpectralEntities>();
            if (comp == null || !comp.spirits.Any())
            {
                Messages.Message("No spirits on current map.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();
            foreach (var spirit in comp.spirits)
            {
                var capturedSpirit = spirit;
                options.Add(new DebugMenuOption(capturedSpirit.label + $" ({capturedSpirit.state})", DebugMenuOptionMode.Action, () =>
                {
                    capturedSpirit.Haunt();
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("AeternusFaith - Spectral", "Force Spirit Manifest", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ForceSpiritManifest()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            var comp = map.GetComponent<MapComponent_SpectralEntities>();
            if (comp == null || !comp.spirits.Any())
            {
                Messages.Message("No spirits on current map.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();
            foreach (var spirit in comp.spirits)
            {
                var capturedSpirit = spirit;
                options.Add(new DebugMenuOption(capturedSpirit.label + $" ({capturedSpirit.state})", DebugMenuOptionMode.Action, () =>
                {
                    capturedSpirit.Manifest();
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("AeternusFaith - Spectral", "List Spectral Entities", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ListSpectralEntities()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            var comp = map.GetComponent<MapComponent_SpectralEntities>();
            if (comp == null) return;

            Log.Message("--- Spectral Entities ---");
            if (!comp.spirits.Any())
            {
                Log.Message("None.");
            }
            else
            {
                foreach (var spirit in comp.spirits)
                {
                    Log.Message($"- {spirit.label} (ID: {spirit.id})");
                    Log.Message($"  State: {spirit.state}");
                    Log.Message($"  Anchor: {spirit.anchorPosition}, Last Known: {spirit.lastKnownPosition}");
                    Log.Message($"  PawnKind: {spirit.pawnKind?.defName ?? "null"}, Faction: {spirit.faction?.Name ?? "null"}");
                    Log.Message($"  Cached Pawn: {(spirit.cachedPawn != null ? spirit.cachedPawn.Name.ToStringShort : "null")}");
                    Log.Message($"  Last Action: {spirit.lastActionSummary}");
                }
            }
            Log.Message("-------------------------");
        }

        [DebugAction("AeternusFaith - Spectral", "Clear Spectral Entities", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ClearSpectralEntities()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            var comp = map.GetComponent<MapComponent_SpectralEntities>();
            if (comp == null) return;

            int count = comp.spirits.Count;
            // Iterate backwards or copy to array since RemoveSpirit modifies the list (sort of, but technically RemoveSpirit just sets Banished)
            foreach (var spirit in comp.spirits.ToList())
            {
                comp.RemoveSpirit(spirit);
            }
            
            Messages.Message($"Cleared {count} spectral entities.", MessageTypeDefOf.TaskCompletion, false);
        }
    }
}
