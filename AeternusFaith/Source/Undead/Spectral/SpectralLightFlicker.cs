using RimWorld;
using Verse;
using Verse.Sound;

namespace AeternusFaith.Undead.Spectral
{
    public class SpectralLightFlicker : IExposable
    {
        private Thing target;
        private int endTick;
        private int nextToggleTick;
        private bool restoreSwitchOn;

        private const int ToggleIntervalTicks = 180;

        public bool Finished => target == null || target.Destroyed || Find.TickManager.TicksGame >= endTick;

        public SpectralLightFlicker()
        {
        }

        public SpectralLightFlicker(Thing target, int durationTicks)
        {
            this.target = target;
            endTick = Find.TickManager.TicksGame + durationTicks;
            nextToggleTick = Find.TickManager.TicksGame;
            restoreSwitchOn = target?.TryGetComp<CompFlickable>()?.SwitchIsOn ?? true;
        }

        public void Tick()
        {
            if (target == null || target.Destroyed)
                return;

            if (Find.TickManager.TicksGame >= endTick)
            {
                RestoreOriginalState();
                return;
            }

            if (Find.TickManager.TicksGame < nextToggleTick)
                return;

            Toggle();
            nextToggleTick = Find.TickManager.TicksGame + ToggleIntervalTicks;
        }

        public void Finish()
        {
            RestoreOriginalState();
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref target, "target");
            Scribe_Values.Look(ref endTick, "endTick");
            Scribe_Values.Look(ref nextToggleTick, "nextToggleTick");
            Scribe_Values.Look(ref restoreSwitchOn, "restoreSwitchOn", true);
        }

        private void RestoreOriginalState()
        {
            CompFlickable flickable = target?.TryGetComp<CompFlickable>();
            if (flickable != null && flickable.SwitchIsOn != restoreSwitchOn)
                Toggle();
        }

        private void Toggle()
        {
            CompFlickable flickable = target?.TryGetComp<CompFlickable>();
            if (flickable == null)
                return;

            flickable.DoFlick();
            SoundDefOf.FlickSwitch.PlayOneShot(new TargetInfo(target.PositionHeld, target.MapHeld));
            if (target.MapHeld != null && target.PositionHeld.IsValid)
                FleckMaker.ThrowDustPuff(target.PositionHeld, target.MapHeld, 0.7f);
        }
    }
}
