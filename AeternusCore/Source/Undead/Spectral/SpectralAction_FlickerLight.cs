using System.Linq;
using RimWorld;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    public class SpectralAction_FlickerLight : SpectralAction
    {
        private const int FlickerDurationTicks = 3600;

        public override bool CanExecute()
        {
            return spirit != null && spirit.CurrentMap != null && spirit.lastKnownPosition.IsValid;
        }

        public override void Execute()
        {
            if (!CanExecute())
                return;

            Map map = spirit.CurrentMap;
            Thing light = GenRadial.RadialDistinctThingsAround(spirit.lastKnownPosition, map, 18f, true)
                .Where(IsValidLight)
                .RandomElementWithFallback(null);

            if (light == null)
            {
                spirit.lastActionSummary = "No flickerable light nearby.";
                return;
            }

            map.GetComponent<MapComponent_SpectralEntities>()?.StartLightFlicker(light, FlickerDurationTicks);
            spirit.lastActionSummary = "Flickered " + light.LabelCap + ".";
            Messages.Message("Debug: " + spirit.label + " flickered " + light.LabelCap + ".", MessageTypeDefOf.NeutralEvent, historical: false);
        }

        private static bool IsValidLight(Thing thing)
        {
            if (thing == null || thing.Destroyed || !thing.Spawned)
                return false;

            if (thing.TryGetComp<CompGlower>() == null || thing.TryGetComp<CompFlickable>() == null)
                return false;

            CompPowerTrader powerTrader = thing.TryGetComp<CompPowerTrader>();
            return powerTrader == null || powerTrader.PowerOn;
        }
    }
}
