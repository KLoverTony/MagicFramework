using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AeternusFaith.Undead.Spectral
{
    public class SpectralAction_MoveItem : SpectralAction
    {
        public override bool CanExecute()
        {
            return spirit != null && spirit.CurrentMap != null && spirit.lastKnownPosition.IsValid;
        }

        public override void Execute()
        {
            if (!CanExecute()) return;

            Map map = spirit.CurrentMap;
            IntVec3 center = spirit.lastKnownPosition;
            float radius = 12f;

            // Find a movable item nearby
            Thing targetItem = GenRadial.RadialDistinctThingsAround(center, map, radius, true)
                .Where(t => t.def.category == ThingCategory.Item && t.def.EverHaulable && !t.Destroyed)
                .RandomElementWithFallback(null);

            if (targetItem != null)
            {
                // Find a valid spot to move it to
                if (CellFinder.TryFindRandomCellNear(targetItem.Position, map, 5, c => c.Standable(map) && c != targetItem.Position && map.reachability.CanReach(targetItem.Position, c, PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors)), out IntVec3 newCell))
                {
                    targetItem.Position = newCell;
                    
                    // Visual/Audio feedback
                    FleckMaker.ThrowDustPuff(newCell, map, 1.5f);
                    DefDatabase<SoundDef>.GetNamedSilentFail("Pawn_Melee_Punch_HitPawn")?.PlayOneShot(new TargetInfo(newCell, map)); // Simple sound for MVP

                    spirit.lastActionSummary = $"Moved {targetItem.LabelCap} to {newCell}.";
                    Messages.Message($"Debug: {spirit.label} moved {targetItem.LabelCap}.", MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    spirit.lastActionSummary = $"Failed to find valid cell to move {targetItem.LabelCap}.";
                }
            }
            else
            {
                spirit.lastActionSummary = "No movable items nearby.";
            }
        }
    }
}
