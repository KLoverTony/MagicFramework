using System.Collections.Generic;
using System.Linq;
using AeternusFaith;
using MagicFramework.PawnMemory;
using RimWorld;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    public class MapComponent_SpectralEntities : MapComponent
    {
        private const int HauntingBridgeIntervalTicks = 2500;
        private const int SpectralAuraIntervalTicks = 250;
        private const float SpectralAuraRadius = 6f;

        public List<SpectralEntity> spirits = new List<SpectralEntity>();
        private List<SpectralLightFlicker> lightFlickers = new List<SpectralLightFlicker>();
        private int nextHauntingBridgeTick;
        private int nextSpectralAuraTick;
        private HediffDef eerieColdHediffDef;

        public MapComponent_SpectralEntities(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (Find.TickManager.TicksGame >= nextHauntingBridgeTick)
            {
                StartReadyScheduledHauntings();
                nextHauntingBridgeTick = Find.TickManager.TicksGame + HauntingBridgeIntervalTicks;
            }

            if (Find.TickManager.TicksGame >= nextSpectralAuraTick)
            {
                ApplyManifestedSpectreAuras();
                nextSpectralAuraTick = Find.TickManager.TicksGame + SpectralAuraIntervalTicks;
            }
            
            for (int i = spirits.Count - 1; i >= 0; i--)
            {
                SpectralEntity spirit = spirits[i];
                spirit.Tick();

                if (spirit.state == SpectralState.Banished)
                {
                    spirits.RemoveAt(i);
                }
            }

            for (int i = lightFlickers.Count - 1; i >= 0; i--)
            {
                SpectralLightFlicker flicker = lightFlickers[i];
                flicker.Tick();
                if (flicker.Finished)
                {
                    flicker.Finish();
                    lightFlickers.RemoveAt(i);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref spirits, "spirits", LookMode.Deep);
            Scribe_Collections.Look(ref lightFlickers, "lightFlickers", LookMode.Deep);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (spirits == null)
                    spirits = new List<SpectralEntity>();

                if (lightFlickers == null)
                    lightFlickers = new List<SpectralLightFlicker>();
                    
                foreach (var spirit in spirits)
                {
                    spirit.RegisterMap(map);
                }
            }
        }

        private void ApplyManifestedSpectreAuras()
        {
            if (spirits.Count == 0 || map?.mapPawns == null)
                return;

            eerieColdHediffDef ??= DefDatabase<HediffDef>.GetNamedSilentFail("AF_EerieCold");
            if (eerieColdHediffDef == null)
                return;

            foreach (SpectralEntity spirit in spirits)
            {
                Pawn spectre = spirit?.cachedPawn;
                if (spirit?.state != SpectralState.Manifesting || spectre?.Spawned != true)
                    continue;

                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (!CanReceiveSpectralAura(pawn, spectre))
                        continue;

                    ApplyOrRefreshAuraHediff(pawn);
                }
            }
        }

        private bool CanReceiveSpectralAura(Pawn pawn, Pawn spectre)
        {
            return pawn != null &&
                   pawn != spectre &&
                   pawn.Spawned &&
                   !pawn.Dead &&
                   pawn.RaceProps?.Humanlike == true &&
                   !SkeletonUndeadUtility.IsUndead(pawn) &&
                   pawn.Position.InHorDistOf(spectre.Position, SpectralAuraRadius);
        }

        private void ApplyOrRefreshAuraHediff(Pawn pawn)
        {
            Hediff existing = pawn.health?.hediffSet?.GetFirstHediffOfDef(eerieColdHediffDef);
            if (existing != null)
                pawn.health.RemoveHediff(existing);

            Hediff hediff = HediffMaker.MakeHediff(eerieColdHediffDef, pawn);
            hediff.Severity = 1f;
            pawn.health.AddHediff(hediff);
        }

        public void AddSpirit(SpectralEntity spirit)
        {
            if (!spirits.Contains(spirit))
            {
                spirit.RegisterMap(map);
                spirits.Add(spirit);
            }
        }

        public void StartLightFlicker(Thing light, int durationTicks)
        {
            if (light == null || light.Destroyed)
                return;

            lightFlickers.Add(new SpectralLightFlicker(light, durationTicks));
        }

        private void StartReadyScheduledHauntings()
        {
            WorldComponent_PawnMemories registry = WorldComponent_PawnMemories.Instance;
            if (registry == null)
                return;

            foreach (PawnMemoryRecord record in registry.GetAllRecords().ToList())
            {
                if (record == null || record.hauntingMapId != map.uniqueID)
                    continue;

                if (!HauntingEvaluator.IsReadyToHaunt(record))
                    continue;

                if (HasSpiritForRecord(record))
                {
                    PawnSoulRiteUtility.NotifySpiritManifested(record, FindExistingSpiritId(record), permanent: false);
                    continue;
                }

                SpectralEntity spirit = CreateNaturalHauntingSpirit(record, registry);
                if (spirit == null)
                    continue;

                AddSpirit(spirit);
                PawnSoulRiteUtility.NotifySpiritManifested(record, spirit.id, permanent: false);
                Messages.Message(spirit.label + " has begun haunting " + map.Parent.Label + ".", MessageTypeDefOf.NeutralEvent, historical: false);
            }
        }

        private SpectralEntity CreateNaturalHauntingSpirit(PawnMemoryRecord record, WorldComponent_PawnMemories registry)
        {
            IntVec3 anchor = ResolveHauntingAnchor(record, registry, out Pawn sourcePawn);
            if (!anchor.IsValid || !anchor.InBounds(map))
                return null;

            SpectralEntity spirit = new SpectralEntity(map)
            {
                label = "Restless spirit of " + ResolveRecordName(record),
                state = SpectralState.WanderingUnseen,
                anchorPosition = anchor,
                lastKnownPosition = ResolveNearbyStandableCell(anchor),
                pawnKind = PawnKindDefOf.Colonist,
                faction = null,
                persistentPawn = false,
                persistentManifestation = false,
                intermittentManifestation = true,
                riteBoundSpectre = false,
                sourcePawn = sourcePawn,
                sourcePawnThingId = record.uniquePawnId,
                sourceMemoryId = record.uniquePawnId,
                sourceIdeo = record.ideo
            };

            spirit.ScheduleNextHaunt();
            spirit.ScheduleNextManifestation();
            spirit.lastActionSummary = "Haunting started.";
            return spirit;
        }

        private IntVec3 ResolveHauntingAnchor(PawnMemoryRecord record, WorldComponent_PawnMemories registry, out Pawn sourcePawn)
        {
            sourcePawn = null;
            Corpse corpse = registry.TryFindCorpse(record);
            if (corpse?.Map == map)
            {
                sourcePawn = corpse.InnerPawn;
                return corpse.PositionHeld;
            }

            if (record.corpseMapId == map.uniqueID && record.corpseCell.HasValue)
                return record.corpseCell.Value;

            if (record.deathMapId == map.uniqueID && record.deathCell.HasValue)
                return record.deathCell.Value;

            if (record.lastKnownMapId == map.uniqueID && record.lastKnownCell.HasValue)
                return record.lastKnownCell.Value;

            return IntVec3.Invalid;
        }

        private IntVec3 ResolveNearbyStandableCell(IntVec3 anchor)
        {
            if (IsValidManifestCell(anchor))
                return anchor;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(anchor, 5f, true))
            {
                if (IsValidManifestCell(cell))
                    return cell;
            }

            return anchor;
        }

        private bool IsValidManifestCell(IntVec3 cell)
        {
            return cell.IsValid &&
                   cell.InBounds(map) &&
                   cell.Standable(map) &&
                   cell.GetFirstPawn(map) == null;
        }

        private bool HasSpiritForRecord(PawnMemoryRecord record)
        {
            return FindExistingSpiritId(record) != null;
        }

        private string FindExistingSpiritId(PawnMemoryRecord record)
        {
            if (record == null)
                return null;

            foreach (SpectralEntity spirit in spirits)
            {
                if (spirit == null || spirit.state == SpectralState.Banished)
                    continue;

                if (spirit.sourceMemoryId == record.uniquePawnId || spirit.sourcePawnThingId == record.uniquePawnId)
                    return spirit.id;
            }

            return null;
        }

        private string ResolveRecordName(PawnMemoryRecord record)
        {
            string name = record?.name?.ToStringShort;
            if (name.NullOrEmpty())
                name = record?.uniquePawnId;
            return name.NullOrEmpty() ? "the dead" : name;
        }

        public static bool HasActiveRiteBoundSpectre(Pawn summoner, out SpectralEntity boundSpectre)
        {
            boundSpectre = null;
            if (summoner == null)
                return false;

            foreach (Map currentMap in Find.Maps)
            {
                MapComponent_SpectralEntities comp = currentMap.GetComponent<MapComponent_SpectralEntities>();
                if (comp?.spirits == null)
                    continue;

                foreach (SpectralEntity spirit in comp.spirits)
                {
                    if (spirit?.IsBoundTo(summoner) == true)
                    {
                        boundSpectre = spirit;
                        return true;
                    }
                }
            }

            return false;
        }

        public static int RemoveSpiritsForSourcePawn(Pawn sourcePawn)
        {
            if (sourcePawn == null)
                return 0;

            int removed = 0;
            foreach (Map currentMap in Find.Maps)
            {
                MapComponent_SpectralEntities comp = currentMap.GetComponent<MapComponent_SpectralEntities>();
                if (comp?.spirits == null)
                    continue;

                for (int i = comp.spirits.Count - 1; i >= 0; i--)
                {
                    SpectralEntity spirit = comp.spirits[i];
                    if (spirit?.IsSourcedFrom(sourcePawn) == true)
                    {
                        comp.RemoveSpirit(spirit);
                        removed++;
                    }
                }
            }

            return removed;
        }

        public static bool TryGetSpiritForManifestedPawn(Pawn pawn, out SpectralEntity spirit)
        {
            spirit = null;
            if (pawn?.Map == null)
                return false;

            MapComponent_SpectralEntities comp = pawn.Map.GetComponent<MapComponent_SpectralEntities>();
            if (comp?.spirits == null)
                return false;

            foreach (SpectralEntity candidate in comp.spirits)
            {
                if (candidate == null || candidate.state == SpectralState.Banished)
                    continue;

                if (candidate.cachedPawn == pawn)
                {
                    spirit = candidate;
                    return true;
                }
            }

            return false;
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
