using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    public class MapComponent_SpectralEntities : MapComponent
    {
        public List<SpectralEntity> spirits = new List<SpectralEntity>();

        public MapComponent_SpectralEntities(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            
            for (int i = spirits.Count - 1; i >= 0; i--)
            {
                SpectralEntity spirit = spirits[i];
                spirit.Tick();

                if (spirit.state == SpectralState.Banished)
                {
                    spirits.RemoveAt(i);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref spirits, "spirits", LookMode.Deep);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (spirits == null)
                    spirits = new List<SpectralEntity>();
                    
                foreach (var spirit in spirits)
                {
                    spirit.RegisterMap(map);
                }
            }
        }

        public void AddSpirit(SpectralEntity spirit)
        {
            if (!spirits.Contains(spirit))
            {
                spirit.RegisterMap(map);
                spirits.Add(spirit);
            }
        }

        public void RemoveSpirit(SpectralEntity spirit)
        {
            if (spirits.Contains(spirit))
            {
                if (spirit.state == SpectralState.Manifesting)
                {
                    spirit.Despawn();
                }
                spirit.state = SpectralState.Banished; // Will be cleaned up next tick
            }
        }
    }
}
