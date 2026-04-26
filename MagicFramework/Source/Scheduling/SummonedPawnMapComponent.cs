using System.Collections.Generic;
using System.Text;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Owns temporary summoned pawns for a single map and removes them on expiry or replacement.
/// </summary>
public sealed class SummonedPawnMapComponent : MapComponent
{
    private List<SummonedPawnRecord> summonedPawns = new();

    public SummonedPawnMapComponent(Map map)
        : base(map)
    {
    }

    public bool Register(SummonedPawnRecord record, bool replaceExistingForCaster)
    {
        if (record == null)
        {
            return false;
        }

        summonedPawns ??= new List<SummonedPawnRecord>();
        if (replaceExistingForCaster)
        {
            RemoveForCasterSpell(record.Caster, record.SpellDef);
        }

        summonedPawns.Add(record);
        return true;
    }

    public void RemoveForCasterSpell(Thing caster, SpellDef spellDef)
    {
        if (summonedPawns == null)
        {
            return;
        }

        for (int i = summonedPawns.Count - 1; i >= 0; i--)
        {
            SummonedPawnRecord record = summonedPawns[i];
            if (record?.Caster == caster && record.SpellDef == spellDef)
            {
                DespawnSummon(record);
                summonedPawns.RemoveAt(i);
            }
        }
    }

    public override void MapComponentTick()
    {
        if (summonedPawns == null || summonedPawns.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = summonedPawns.Count - 1; i >= 0; i--)
        {
            SummonedPawnRecord record = summonedPawns[i];
            if (record == null)
            {
                summonedPawns.RemoveAt(i);
                continue;
            }

            if (record.SummonedPawn == null || record.SummonedPawn.Destroyed)
            {
                summonedPawns.RemoveAt(i);
                continue;
            }

            if (record.IsExpired(currentTick))
            {
                DespawnSummon(record);
                summonedPawns.RemoveAt(i);
            }
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref summonedPawns, "summonedPawns", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && summonedPawns == null)
        {
            summonedPawns = new List<SummonedPawnRecord>();
        }
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new();
        builder.Append("[MagicFramework] Summoned pawn runtime for map ");
        builder.Append(map?.Index ?? -1);
        builder.Append(": ");
        builder.Append(summonedPawns?.Count ?? 0);
        builder.Append(" active summon(s).");

        if (summonedPawns == null || summonedPawns.Count == 0)
        {
            return builder.ToString();
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = 0; i < summonedPawns.Count; i++)
        {
            SummonedPawnRecord record = summonedPawns[i];
            builder.AppendLine();
            builder.Append("  [");
            builder.Append(i);
            builder.Append("] pawn=");
            builder.Append(record?.SummonedPawn?.LabelCap ?? "<null>");
            builder.Append(" spell=");
            builder.Append(record?.SpellDef?.defName ?? "<null>");
            builder.Append(" expiresIn=");
            builder.Append(record == null || record.ExpireAtTick < 0 ? -1 : record.ExpireAtTick - currentTick);
        }

        return builder.ToString();
    }

    private static void DespawnSummon(SummonedPawnRecord record)
    {
        Pawn summonedPawn = record?.SummonedPawn;
        if (summonedPawn == null || summonedPawn.Destroyed)
        {
            return;
        }

        summonedPawn.jobs?.StopAll();
        if (summonedPawn.Spawned)
        {
            summonedPawn.DeSpawn();
        }

        summonedPawn.Destroy(DestroyMode.Vanish);
    }
}
