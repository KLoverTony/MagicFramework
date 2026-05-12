using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MagicFramework.PawnMemory;

public class WorldComponent_PawnMemories : WorldComponent
{
    private Dictionary<string, PawnMemoryRecord> memoriesByPawnId = new Dictionary<string, PawnMemoryRecord>();
    private List<PawnMemoryRecord> tmpValues;
    private List<string> tmpKeys;

    public WorldComponent_PawnMemories(World world) : base(world)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref memoriesByPawnId, "memoriesByPawnId", LookMode.Value, LookMode.Deep, ref tmpKeys, ref tmpValues);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            memoriesByPawnId ??= new Dictionary<string, PawnMemoryRecord>();
        }
    }

    public static WorldComponent_PawnMemories Instance => Find.World?.GetComponent<WorldComponent_PawnMemories>();

    public PawnMemoryRecord GetMemory(string uniquePawnId)
    {
        if (string.IsNullOrEmpty(uniquePawnId)) return null;
        return memoriesByPawnId.TryGetValue(uniquePawnId, out var record) ? record : null;
    }

    public PawnMemoryRecord GetMemory(Pawn pawn)
    {
        if (pawn == null) return null;
        return GetMemory(pawn.ThingID);
    }

    public IEnumerable<PawnMemoryRecord> GetAllRecords()
    {
        return memoriesByPawnId.Values;
    }

    public PawnMemoryRecord GetOrCreateMemory(Pawn pawn)
    {
        if (pawn == null || !pawn.RaceProps.Humanlike) return null;
        
        var record = GetMemory(pawn);
        if (record == null)
        {
            record = new PawnMemoryRecord
            {
                uniquePawnId = pawn.ThingID,
                createdTick = Find.TickManager.TicksGame
            };
            memoriesByPawnId[pawn.ThingID] = record;
            UpdateMemory(pawn, PawnMemoryUpdateReason.EnteredMap);
        }
        return record;
    }

    public void UpdateMemory(Pawn pawn, PawnMemoryUpdateReason reason)
    {
        var record = GetMemory(pawn);
        if (record == null) return;

        record.lastUpdatedTick = Find.TickManager.TicksGame;

        // Identity
        record.name = pawn.Name;
        record.gender = pawn.gender;
        record.raceDef = pawn.def;
        record.pawnKindDef = pawn.kindDef;
        record.factionDef = pawn.Faction?.def;

        if (pawn.IsColonist) record.wasColonist = true;
        if (pawn.IsPrisoner) record.wasPrisoner = true;
        if (pawn.IsSlave) record.wasSlave = true;
        if (pawn.IsQuestLodger() || pawn.HostFaction == Faction.OfPlayer) record.wasGuest = true;
        if (pawn.HostileTo(Faction.OfPlayer)) record.wasHostileToPlayer = true;

        // Map and Location Context
        if (pawn.Map != null)
        {
            record.lastKnownMapId = pawn.Map.uniqueID;
            record.lastKnownMapName = pawn.Map.Parent.LabelCap;
            record.lastKnownCell = pawn.Position;
        }
        record.lastSeenTick = Find.TickManager.TicksGame;

        // Appearance
        if (pawn.story != null)
        {
            record.bodyType = pawn.story.bodyType;
            record.headType = pawn.story.headType;
            record.hairDef = pawn.story.hairDef;
            record.skinColor = pawn.story.SkinColor;
            record.hairColor = pawn.story.HairColor;

            if (pawn.style != null)
            {
                record.beardDef = pawn.style.beardDef;
            }

            if (pawn.story.Childhood != null)
                record.childhoodBackstory = pawn.story.Childhood.identifier;
            if (pawn.story.Adulthood != null)
                record.adulthoodBackstory = pawn.story.Adulthood.identifier;

            if (pawn.story.traits != null)
            {
                record.traits.Clear();
                foreach (var trait in pawn.story.traits.allTraits)
                {
                    record.traits[trait.def.defName] = trait.Degree;
                }
            }
        }

        // Skills
        if (pawn.skills != null)
        {
            record.skills.Clear();
            foreach (var skill in pawn.skills.skills)
            {
                record.skills.Add(new SkillSnapshot
                {
                    skillDef = skill.def,
                    level = skill.Level,
                    passion = skill.passion,
                    xpSinceLastLevel = skill.xpSinceLastLevel
                });
            }
        }

        // Ideology
        if (pawn.ideo != null)
        {
            record.ideo = pawn.ideo.Ideo;
        }

        // Health Snapshot
        record.deadAtCapture = pawn.Dead;
        
        record.majorHediffs.Clear();
        record.missingBodyParts.Clear();

        if (pawn.health?.hediffSet != null)
        {
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_MissingPart missingPart)
                {
                    record.missingBodyParts.Add(missingPart.Part.def.defName);
                }
                else if (hediff.Severity > 0.1f)
                {
                    record.majorHediffs.Add(hediff.def.defName);
                }
            }
        }
    }

    public void NotifyPawnKilled(Pawn pawn, DamageInfo? dinfo, Hediff exactCulprit = null)
    {
        if (pawn == null) return;
        
        // We ensure a record exists, but only for humanlikes to prevent save bloat
        if (!pawn.RaceProps.Humanlike) return;

        var record = GetOrCreateMemory(pawn);
        if (record != null)
        {
            UpdateMemory(pawn, PawnMemoryUpdateReason.OnDeath);

            record.deathTick = Find.TickManager.TicksGame;
            if (pawn.Map != null)
            {
                record.deathMapId = pawn.Map.uniqueID;
                record.deathCell = pawn.Position;
            }

            if (record.state != PawnMemoryState.Released && record.state != PawnMemoryState.Invalidated)
            {
                record.state = PawnMemoryState.DeadPendingRites;
            }
        }
    }
    
    private const int MaintenanceIntervalTicks = 60000; // 1 day
    private int nextMaintenanceTick = 0;

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();

        if (Find.TickManager.TicksGame >= nextMaintenanceTick)
        {
            PerformDailyMaintenance();
            nextMaintenanceTick = Find.TickManager.TicksGame + MaintenanceIntervalTicks;
        }
    }

    private void PerformDailyMaintenance()
    {
        foreach (var map in Find.Maps)
        {
            if (!map.IsPlayerHome) continue;

            foreach (var pawn in map.mapPawns.AllPawns)
            {
                if (!pawn.RaceProps.Humanlike) continue;
                if (!pawn.IsColonist && !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony && !pawn.IsQuestLodger()) continue;

                var record = GetOrCreateMemory(pawn);
                
                if (record != null && Find.TickManager.TicksGame - record.lastUpdatedTick >= MaintenanceIntervalTicks)
                {
                    UpdateMemory(pawn, PawnMemoryUpdateReason.DailyMaintenance);
                }
            }
        }
    }
}
