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
        public int nextManifestTick = -1;
        public int manifestationEndTick = -1;
        
        public Pawn cachedPawn;
        public bool persistentPawn = false;
        public bool persistentManifestation = false;
        public bool intermittentManifestation = true;
        public bool riteBoundSpectre = false;
        public Pawn boundSummoner;
        public string boundSummonerThingId;
        public Pawn sourcePawn;
        public string sourcePawnThingId;
        public string sourceMemoryId;
        public Ideo sourceIdeo;
        
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
            Scribe_Values.Look(ref nextManifestTick, "nextManifestTick", -1);
            Scribe_Values.Look(ref manifestationEndTick, "manifestationEndTick", -1);
            Scribe_References.Look(ref cachedPawn, "cachedPawn");
            Scribe_Values.Look(ref persistentPawn, "persistentPawn", false);
            Scribe_Values.Look(ref persistentManifestation, "persistentManifestation", false);
            Scribe_Values.Look(ref intermittentManifestation, "intermittentManifestation", true);
            Scribe_Values.Look(ref riteBoundSpectre, "riteBoundSpectre", false);
            Scribe_References.Look(ref boundSummoner, "boundSummoner");
            Scribe_Values.Look(ref boundSummonerThingId, "boundSummonerThingId");
            Scribe_References.Look(ref sourcePawn, "sourcePawn");
            Scribe_Values.Look(ref sourcePawnThingId, "sourcePawnThingId");
            Scribe_Values.Look(ref sourceMemoryId, "sourceMemoryId");
            Scribe_References.Look(ref sourceIdeo, "sourceIdeo");
            Scribe_Values.Look(ref lastActionSummary, "lastActionSummary", "Loaded");
        }

        public bool IsBoundTo(Pawn pawn)
        {
            if (pawn == null || !riteBoundSpectre || state == SpectralState.Banished)
                return false;

            if (boundSummoner == pawn)
                return true;

            return !boundSummonerThingId.NullOrEmpty() && boundSummonerThingId == pawn.ThingID;
        }

        public bool IsSourcedFrom(Pawn pawn)
        {
            if (pawn == null || state == SpectralState.Banished)
                return false;

            if (sourcePawn == pawn || boundSummoner == pawn || cachedPawn == pawn)
                return true;

            return sourcePawn?.ThingID == pawn.ThingID ||
                   sourcePawnThingId == pawn.ThingID ||
                   sourceMemoryId == pawn.ThingID ||
                   boundSummonerThingId == pawn.ThingID ||
                   cachedPawn?.ThingID == pawn.ThingID;
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
                if (!persistentManifestation && manifestationEndTick > 0 && Find.TickManager.TicksGame >= manifestationEndTick)
                {
                    Despawn();
                }
                else if (cachedPawn != null && cachedPawn.Spawned)
                {
                    EnsureGuestStatus();
                    lastKnownPosition = cachedPawn.Position;
                }
            }
            else if (state == SpectralState.WanderingUnseen)
            {
                if (nextHauntTick > 0 && Find.TickManager.TicksGame >= nextHauntTick)
                {
                    Haunt();
                }

                if (intermittentManifestation && nextManifestTick > 0 && Find.TickManager.TicksGame >= nextManifestTick)
                {
                    ManifestTemporary();
                }
            }
        }

        public void Haunt()
        {
            if (state == SpectralState.Manifesting || state == SpectralState.Banished) return;

            state = SpectralState.Haunting;
            
            SpectralAction action = Rand.Chance(0.45f)
                ? new SpectralAction_FlickerLight()
                : new SpectralAction_MoveItem();
            action.Init(this);
            if (action.CanExecute())
                action.Execute();

            state = SpectralState.WanderingUnseen;
            ScheduleNextHaunt();
        }

        public void Manifest()
        {
            Manifest(persistentManifestation);
        }

        public void ManifestTemporary()
        {
            Manifest(false);
        }

        public void ManifestPersistent()
        {
            persistentPawn = true;
            persistentManifestation = true;
            intermittentManifestation = false;
            Manifest(true);
        }

        private void Manifest(bool persistent)
        {
            if (CurrentMap == null || !lastKnownPosition.IsValid || state == SpectralState.Manifesting) return;

            if (cachedPawn == null || (!persistentPawn && cachedPawn.Dead))
            {
                PawnKindDef spectreKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("AF_Spectre");
                if (spectreKind == null)
                {
                    Log.Error("[AeternusFaith] Could not manifest a spectre because PawnKindDef AF_Spectre is missing.");
                    return;
                }

                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind: PawnKindDefOf.Colonist,
                    faction: null,
                    context: PawnGenerationContext.NonPlayer,
                    tile: CurrentMap.Tile,
                    forceGenerateNewPawn: true,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: false,
                    mustBeCapableOfViolence: false,
                    colonistRelationChanceFactor: 0f,
                    forceNoBackstory: true,
                    allowPregnant: false,
                    allowFood: false,
                    allowAddictions: false,
                    fixedGender: Gender.Male,
                    forceNoIdeo: true,
                    developmentalStages: DevelopmentalStage.Adult,
                    dontGiveWeapon: true,
                    maximumAgeTraits: 0,
                    minimumAgeTraits: 0,
                    forceNoGear: true);
                cachedPawn = PawnGenerator.GeneratePawn(request);
                cachedPawn.def = spectreKind.race;
                cachedPawn.kindDef = spectreKind;
                SkeletonUndeadUtility.NormalizeSkeletonLifeStage(cachedPawn);
                SkeletonUndeadUtility.EnsureUndeadCleanupComp(cachedPawn);
                cachedPawn.Name = new NameTriple("", label, "");

                if (ModsConfig.IdeologyActive && cachedPawn.ideo != null)
                {
                    Ideo ideoToApply = sourceIdeo ?? Faction.OfPlayer?.ideos?.PrimaryIdeo;
                    if (ideoToApply != null)
                        cachedPawn.ideo.SetIdeo(ideoToApply);
                }

                SkeletonUndeadUtility.EnforceUndeadState(cachedPawn, resetSkills: true);
                SkeletonUndeadUtility.CopyBackstoriesFromSource(sourcePawn, cachedPawn);
                SkeletonUndeadUtility.CopySkillsFromSource(sourcePawn, cachedPawn);
                SkeletonUndeadUtility.RemoveNonUndeadHediffs(cachedPawn);
                SkeletonUndeadUtility.ApplyRaceBasedUndeadHediffs(cachedPawn);
                SkeletonUndeadUtility.ApplyRaceBasedUndeadXenotype(cachedPawn);
                SkeletonUndeadUtility.SuppressUndeadSocialInteractions(cachedPawn);
                ApplySpectreAppearance(cachedPawn);
                cachedPawn.Name = new NameTriple("", label, "");
                Log.Message("[AeternusFaith] Manifested spectre conversion result: def=" + cachedPawn.def?.defName +
                            ", kindDef=" + cachedPawn.kindDef?.defName +
                            ", xenotype=" + (ModsConfig.BiotechActive ? cachedPawn.genes?.Xenotype?.defName : "BiotechInactive") +
                            ", undead=" + SkeletonUndeadUtility.IsUndead(cachedPawn) +
                            ", spectral=" + SkeletonUndeadUtility.IsSpectralUndead(cachedPawn));
            }

            EnsureGuestStatus();

            GenSpawn.Spawn(cachedPawn, lastKnownPosition, CurrentMap);
            EnsureGuestStatus();
            SkeletonUndeadUtility.SuppressUndeadSocialInteractions(cachedPawn);
            state = SpectralState.Manifesting;
            if (persistent)
            {
                persistentPawn = true;
                persistentManifestation = true;
                manifestationEndTick = -1;
                nextManifestTick = -1;
            }
            else
            {
                persistentManifestation = false;
                manifestationEndTick = Find.TickManager.TicksGame + Rand.RangeInclusive(1800, 5000);
            }
            lastActionSummary = "Manifested.";
            
            FleckMaker.ThrowDustPuff(lastKnownPosition, CurrentMap, 2f);
            Messages.Message($"Debug: {label} has manifested.", MessageTypeDefOf.NeutralEvent);
        }

        private void EnsureGuestStatus()
        {
            faction = null;
            if (cachedPawn == null)
                return;

            if (cachedPawn.Faction != null)
                cachedPawn.SetFaction(null);

            cachedPawn.guest?.SetGuestStatus(Faction.OfPlayer, GuestStatus.Guest);
            if (cachedPawn.Drafted)
                cachedPawn.drafter.Drafted = false;
        }

        private void ApplySpectreAppearance(Pawn spectre)
        {
            if (spectre?.story == null)
                return;

            BodyTypeDef bodyTypeDef = DefDatabase<BodyTypeDef>.GetNamedSilentFail("AF_SpectreBody");
            HeadTypeDef headTypeDef = DefDatabase<HeadTypeDef>.GetNamedSilentFail("AF_SpectreHead");
            HairDef hairDef = DefDatabase<HairDef>.GetNamedSilentFail("Bald");

            if (bodyTypeDef != null)
                spectre.story.bodyType = bodyTypeDef;
            if (headTypeDef != null)
                spectre.story.headType = headTypeDef;
            if (hairDef != null)
                spectre.story.hairDef = hairDef;

            BeardDef beardDef = DefDatabase<BeardDef>.GetNamedSilentFail("NoBeard");
            if (beardDef != null && spectre.style != null)
                spectre.style.beardDef = beardDef;

            spectre.Drawer?.renderer?.SetAllGraphicsDirty();
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
            if (intermittentManifestation)
                ScheduleNextManifestation();
            lastActionSummary = "Despawned.";
            
            FleckMaker.ThrowDustPuff(lastKnownPosition, CurrentMap, 2f);
            Messages.Message($"Debug: {label} has faded away.", MessageTypeDefOf.NeutralEvent);
        }

        public void ScheduleNextHaunt()
        {
            nextHauntTick = Find.TickManager.TicksGame + Rand.RangeInclusive(5000, 18000);
        }

        public void ScheduleNextManifestation()
        {
            nextManifestTick = Find.TickManager.TicksGame + Rand.RangeInclusive(15000, 60000);
        }
    }
}
