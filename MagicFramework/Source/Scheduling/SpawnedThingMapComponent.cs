using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Scheduling;

public enum SpawnedThingLifecycleEvent
{
    Create,
    Expire,
    Remove,
    Break
}

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
                RunLifecycleActions(record, SpawnedThingLifecycleEvent.Remove);
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
                RunLifecycleActions(record, SpawnedThingLifecycleEvent.Break);
                spawnedThings.RemoveAt(i);
                continue;
            }

            if (record.IsExpired(currentTick))
            {
                RunLifecycleActions(record, SpawnedThingLifecycleEvent.Expire);
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

    public void RunLifecycleActions(SpawnedThingRecord record, SpawnedThingLifecycleEvent lifecycleEvent)
    {
        if (record?.SpellDef == null
            || record.ActionPath == null
            || record.ActionPath.Count == 0
            || SpellActionPathUtility.ResolveAction(record.SpellDef, record.ActionPath) is not SpawnThingActionDef actionDef)
        {
            return;
        }

        List<SpellActionDef> actions = lifecycleEvent switch
        {
            SpawnedThingLifecycleEvent.Create => actionDef.onCreateActions,
            SpawnedThingLifecycleEvent.Expire => actionDef.onExpireActions,
            SpawnedThingLifecycleEvent.Remove => actionDef.onRemoveActions,
            SpawnedThingLifecycleEvent.Break => actionDef.onBreakActions,
            _ => null
        };

        if (actions == null || actions.Count == 0 || !TryCreateContext(record, out SpellContext context))
        {
            return;
        }

        new SpellActionRunner().RunActions(context, actions);
    }

    private bool TryCreateContext(SpawnedThingRecord record, out SpellContext context)
    {
        context = null;
        Thing spawnedThing = record?.SpawnedThing;
        Thing caster = record?.Caster;
        Map contextMap = spawnedThing?.MapHeld ?? caster?.MapHeld ?? map;
        if (contextMap == null)
        {
            return false;
        }

        LocalTargetInfo targetInfo = spawnedThing != null && !spawnedThing.Destroyed
            ? new LocalTargetInfo(spawnedThing)
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

    private static void DestroySpawnedThing(SpawnedThingRecord record)
    {
        Thing spawnedThing = record?.SpawnedThing;
        if (spawnedThing != null && !spawnedThing.Destroyed)
        {
            spawnedThing.Destroy(DestroyMode.Vanish);
        }
    }
}
