using RimWorld;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    /// <summary>
    /// Base class for structured spectral effects/actions to allow easy expansion.
    /// </summary>
    public abstract class SpectralAction : IExposable
    {
        protected SpectralEntity spirit;

        public virtual void Init(SpectralEntity spirit)
        {
            this.spirit = spirit;
        }

        public abstract bool CanExecute();
        public abstract void Execute();

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref spirit, "spirit");
        }
    }
}
