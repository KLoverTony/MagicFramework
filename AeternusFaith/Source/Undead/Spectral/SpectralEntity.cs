using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    public class SpectralEntity : IExposable, ILoadReferenceable
    {
        public string id;
        public string label;
        public SpectralState state = SpectralState.Dormant;
        
        // TODO: Ultimately, the goal will be to use a deceased pawn as a template but for now, just use any pawn.
        public PawnKindDef pawnKind;
        // TODO: This will be defined by the corpse that inspires this spectral pawn.
        public Faction faction;

        public IntVec3 anchorPosition = IntVec3.Invalid;
        public IntVec3 lastKnownPosition = IntVec3.Invalid;
        
        public int nextHauntTick = -1;
        public int manifestationEndTick = -1;
        
        public Pawn cachedPawn;
        public bool persistentPawn = false;
        
        public string lastActionSummary = "Created";

        // Runtime map reference
        public Map CurrentMap => cachedMap;
        private Map cachedMap;

        public SpectralEntity() { }

        public SpectralEntity(Map map)
        {
            this.cachedMap = map;
            this.id = "Spectral_" + Find.UniqueIDsManager.GetNextThingID();
        }

        public string GetUniqueLoadID()
        {
            return id;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref label, "label", "Unknown Spirit");
            Scribe_Values.Look(ref state, "state", SpectralState.Dormant);
            Scribe_Defs.Look(ref pawnKind, "pawnKind");
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref anchorPosition, "anchorPosition", IntVec3.Invalid);
            Scribe_Values.Look(ref lastKnownPosition, "lastKnownPosition", IntVec3.Invalid);
            Scribe_Values.Look(ref nextHauntTick, "nextHauntTick", -1);
            Scribe_Values.Look(ref manifestationEndTick, "manifestationEndTick", -1);
            Scribe_References.Look(ref cachedPawn, "cachedPawn");
            Scribe_Values.Look(ref persistentPawn, "persistentPawn", false);
            Scribe_Values.Look(ref lastActionSummary, "lastActionSummary", "Loaded");
        }

        public void RegisterMap(Map map)
        {
            this.cachedMap = map;
        }

        public void Tick()
        {
            if (state == SpectralState.Banished) return;

            if (state == SpectralState.Manifesting)
            {
                if (Find.TickManager.TicksGame >= manifestationEndTick)
                {
                    Despawn();
                }
                else if (cachedPawn != null && cachedPawn.Spawned)
                {
                    lastKnownPosition = cachedPawn.Position;
                }
            }
            else if (state == SpectralState.WanderingUnseen)
            {
                if (nextHauntTick > 0 && Find.TickManager.TicksGame >= nextHauntTick)
                {
                    Haunt();
                }
            }
        }

        public void Haunt()
        {
            if (state == SpectralState.Manifesting || state == SpectralState.Banished) return;

            state = SpectralState.Haunting;
            
            // Execute the MVP action
            var moveItemAction = new SpectralAction_MoveItem();
            moveItemAction.Init(this);
            moveItemAction.Execute();

            state = SpectralState.WanderingUnseen;
            nextHauntTick = Find.TickManager.TicksGame + 6000; // Next haunt in roughly 2.5 in-game hours
        }

        public void Manifest()
        {
            if (CurrentMap == null || !lastKnownPosition.IsValid || state == SpectralState.Manifesting) return;

            if (cachedPawn == null || (!persistentPawn && cachedPawn.Dead))
            {
                // Resolve the spectre kind; fall back to Colonist if the def hasn't loaded yet.
                PawnKindDef spectreKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("AF_Spectre")
                                         ?? (pawnKind ?? PawnKindDefOf.Colonist);

                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind: spectreKind,
                    faction: faction ?? Faction.OfPlayer,
                    context: PawnGenerationContext.NonPlayer,
                    tile: CurrentMap.Tile,
                    forceGenerateNewPawn: true,
                    forceNoIdeo: false,
                    forceNoBackstory: true,
                    canGeneratePawnRelations: false,
                    allowPregnant: false,
                    allowFood: false,
                    allowAddictions: false,
                    dontGiveWeapon: true,
                    forceNoGear: true);
                cachedPawn = PawnGenerator.GeneratePawn(request);
                cachedPawn.Name = new NameTriple("", label, "");

                // Strip any gear the generator may still have added.
                cachedPawn.apparel?.DestroyAll(DestroyMode.Vanish);
                cachedPawn.equipment?.DestroyAllEquipment(DestroyMode.Vanish);
                cachedPawn.inventory?.DestroyAll(DestroyMode.Vanish);
            }

            GenSpawn.Spawn(cachedPawn, lastKnownPosition, CurrentMap);

            // Suppress social interactions — the spectre doesn't chat.
            // if (cachedPawn.interactions != null)
            //     cachedPawn.interactions.lastInteractTime = Find.TickManager.TicksGame + 9999999;
            state = SpectralState.Manifesting;
            manifestationEndTick = Find.TickManager.TicksGame + 2500; // Roughly 1 in-game hour
            lastActionSummary = "Manifested.";
            
            FleckMaker.ThrowDustPuff(lastKnownPosition, CurrentMap, 2f);
            Messages.Message($"Debug: {label} has manifested.", MessageTypeDefOf.NeutralEvent);
        }

        public void Despawn()
        {
            if (state != SpectralState.Manifesting) return;

            if (cachedPawn != null && cachedPawn.Spawned)
            {
                lastKnownPosition = cachedPawn.Position;
                cachedPawn.DeSpawn();
                if (!persistentPawn)
                {
                    Find.WorldPawns.PassToWorld(cachedPawn, PawnDiscardDecideMode.Discard);
                    cachedPawn = null;
                }
            }

            state = SpectralState.WanderingUnseen;
            manifestationEndTick = -1;
            lastActionSummary = "Despawned.";
            
            FleckMaker.ThrowDustPuff(lastKnownPosition, CurrentMap, 2f);
            Messages.Message($"Debug: {label} has faded away.", MessageTypeDefOf.NeutralEvent);
        }
    }
}
