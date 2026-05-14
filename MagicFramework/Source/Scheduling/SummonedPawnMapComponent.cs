using System.Collections.Generic;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Scheduling;

public enum SummonedPawnLifecycleEvent
{
    Create,
    Expire,
    Remove,
    Break
}

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
                RunLifecycleActions(record, SummonedPawnLifecycleEvent.Remove);
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
                RunLifecycleActions(record, SummonedPawnLifecycleEvent.Break);
                summonedPawns.RemoveAt(i);
                continue;
            }

            if (record.IsExpired(currentTick))
            {
                RunLifecycleActions(record, SummonedPawnLifecycleEvent.Expire);
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

    public void RunLifecycleActions(SummonedPawnRecord record, SummonedPawnLifecycleEvent lifecycleEvent)
    {
        if (record?.SpellDef == null
            || record.ActionPath == null
            || record.ActionPath.Count == 0
            || SpellActionPathUtility.ResolveAction(record.SpellDef, record.ActionPath) is not SummonPawnActionDef actionDef)
        {
            return;
        }

        List<SpellActionDef> actions = lifecycleEvent switch
        {
            SummonedPawnLifecycleEvent.Create => actionDef.onCreateActions,
            SummonedPawnLifecycleEvent.Expire => actionDef.onExpireActions,
            SummonedPawnLifecycleEvent.Remove => actionDef.onRemoveActions,
            SummonedPawnLifecycleEvent.Break => actionDef.onBreakActions,
            _ => null
        };

        if (actions == null || actions.Count == 0 || !TryCreateContext(record, out SpellContext context))
        {
            return;
        }

        new SpellActionRunner().RunActions(context, actions);
    }

    private bool TryCreateContext(SummonedPawnRecord record, out SpellContext context)
    {
        context = null;
        Pawn summonedPawn = record?.SummonedPawn;
        Thing caster = record?.Caster;
        Map contextMap = summonedPawn?.MapHeld ?? caster?.MapHeld ?? map;
        if (contextMap == null)
        {
            return false;
        }

        LocalTargetInfo targetInfo = summonedPawn != null && !summonedPawn.Destroyed
            ? new LocalTargetInfo(summonedPawn)
            : LocalTargetInfo.Invalid;
        IntVec3 currentCell = targetInfo.IsValid ? targetInfo.Cell : caster?.Position ?? IntVec3.Invalid;
        context = new SpellContext
        {
            caster = caster,
            map = contextMap,
            spellDef = record.SpellDef,
            initialTarget = targetInfo,
            currentTarget = targetInfo,
            currentCell = currentCell,
            randomSeed = Find.TickManager?.TicksGame ?? 0
        };
        context.executionState.costsApplied = true;
        if (targetInfo.IsValid)
        {
            context.currentTargets.Add(targetInfo);
        }

        return true;
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
