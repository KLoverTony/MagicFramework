using System.Collections.Generic;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Owns temporary spell-spawned things for a single map.
/// </summary>
public sealed class SpawnedThingMapComponent : MapComponent
{
    private List<SpawnedThingRecord> spawnedThings = new();

    public SpawnedThingMapComponent(Map map)
        : base(map)
    {
    }

    public bool Register(SpawnedThingRecord record, bool replaceExistingForCasterSpell)
    {
        if (record == null)
        {
            return false;
        }

        spawnedThings ??= new List<SpawnedThingRecord>();
        if (replaceExistingForCasterSpell)
        {
            RemoveForCasterSpell(record.Caster, record.SpellDef);
        }

        spawnedThings.Add(record);
        return true;
    }

    public void RemoveForCasterSpell(Thing caster, SpellDef spellDef)
    {
        if (spawnedThings == null)
        {
            return;
        }

        for (int i = spawnedThings.Count - 1; i >= 0; i--)
        {
            SpawnedThingRecord record = spawnedThings[i];
            if (record?.Caster == caster && record.SpellDef == spellDef)
            {
                DestroySpawnedThing(record);
                spawnedThings.RemoveAt(i);
            }
        }
    }

    public override void MapComponentTick()
    {
        if (spawnedThings == null || spawnedThings.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = spawnedThings.Count - 1; i >= 0; i--)
        {
            SpawnedThingRecord record = spawnedThings[i];
            if (record == null)
            {
                spawnedThings.RemoveAt(i);
                continue;
            }

            if (record.SpawnedThing == null || record.SpawnedThing.Destroyed)
            {
                spawnedThings.RemoveAt(i);
                continue;
            }

            if (record.IsExpired(currentTick))
            {
                DestroySpawnedThing(record);
                spawnedThings.RemoveAt(i);
            }
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref spawnedThings, "spawnedThings", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && spawnedThings == null)
        {
            spawnedThings = new List<SpawnedThingRecord>();
        }
    }

    private static void DestroySpawnedThing(SpawnedThingRecord record)
    {
        Thing spawnedThing = record?.SpawnedThing;
        if (spawnedThing != null && !spawnedThing.Destroyed)
        {
            spawnedThing.Destroy(DestroyMode.Vanish);
        }
    }
}
