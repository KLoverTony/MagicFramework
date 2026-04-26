using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Conditions;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Scheduling;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MagicFramework.Actions;

public sealed class SequenceActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        SequenceActionDef sequenceDef = actionDef as SequenceActionDef;
        runner.RunActions(context, sequenceDef?.actions);
    }
}

public sealed class LogMessageActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        LogMessageActionDef logDef = actionDef as LogMessageActionDef;
        Log.Message($"[MagicFramework] {logDef?.message ?? "LogMessageActionWorker executed."}");
    }
}

public sealed class EffectActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        EffectActionDef effectDef = actionDef as EffectActionDef;
        if (effectDef == null)
        {
            return;
        }

        string locationSummary = effectDef.locationSource.ToString();
        TargetInfo targetInfo = ResolveTargetInfo(context, effectDef.locationSource);
        bool playedAnyEffect = false;

        if (!string.IsNullOrWhiteSpace(effectDef.effectDef))
        {
            EffecterDef resolvedEffecter = DefDatabase<EffecterDef>.GetNamedSilentFail(effectDef.effectDef);
            if (resolvedEffecter != null)
            {
                Effecter effecter = resolvedEffecter.Spawn();
                effecter?.Trigger(targetInfo, targetInfo);
                effecter?.Cleanup();
                playedAnyEffect = true;
            }
            else
            {
                FleckDef resolvedFleck = DefDatabase<FleckDef>.GetNamedSilentFail(effectDef.effectDef);
                if (resolvedFleck != null && TryResolveFleckLocation(context, effectDef.locationSource, out Vector3 fleckLocation, out Map fleckMap))
                {
                    FleckMaker.Static(fleckLocation, fleckMap, resolvedFleck);
                    playedAnyEffect = true;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(effectDef.soundDef))
        {
            SoundDef resolvedSound = DefDatabase<SoundDef>.GetNamedSilentFail(effectDef.soundDef);
            if (resolvedSound != null)
            {
                SoundStarter.PlayOneShot(resolvedSound, targetInfo);
                playedAnyEffect = true;
            }
        }

        Log.Message(
            $"[MagicFramework] Effect action requested. Effect={effectDef.effectDef ?? "<none>"}, Sound={effectDef.soundDef ?? "<none>"}, Location={locationSummary}, AttachToTarget={effectDef.attachToTarget}, Resolved={playedAnyEffect}.");
    }

    private static bool TryResolveFleckLocation(SpellContext context, SpellEffectLocationSource locationSource, out Vector3 location, out Map map)
    {
        location = default;
        map = context?.map;

        Thing thing = locationSource switch
        {
            SpellEffectLocationSource.CurrentTarget => context?.currentTarget.Thing,
            SpellEffectLocationSource.InitialTarget => context?.initialTarget.Thing,
            SpellEffectLocationSource.Caster => context?.caster,
            _ => null
        };

        if (thing != null && thing.Spawned)
        {
            location = thing.DrawPos;
            map = thing.Map;
            return map != null;
        }

        IntVec3 cell = locationSource switch
        {
            SpellEffectLocationSource.CurrentCell => context?.currentCell ?? IntVec3.Invalid,
            SpellEffectLocationSource.CurrentTarget => context?.currentTarget.Cell ?? IntVec3.Invalid,
            SpellEffectLocationSource.InitialTarget => context?.initialTarget.Cell ?? IntVec3.Invalid,
            SpellEffectLocationSource.Caster => context?.caster?.Position ?? IntVec3.Invalid,
            _ => IntVec3.Invalid
        };

        if (cell.IsValid && map != null)
        {
            location = cell.ToVector3Shifted();
            return true;
        }

        return false;
    }

    private static TargetInfo ResolveTargetInfo(SpellContext context, SpellEffectLocationSource locationSource)
    {
        Thing thing = locationSource switch
        {
            SpellEffectLocationSource.CurrentTarget => context?.currentTarget.Thing,
            SpellEffectLocationSource.InitialTarget => context?.initialTarget.Thing,
            SpellEffectLocationSource.Caster => context?.caster,
            _ => null
        };

        if (thing != null && thing.Spawned)
        {
            return new TargetInfo(thing);
        }

        IntVec3 cell = locationSource switch
        {
            SpellEffectLocationSource.CurrentCell => context?.currentCell ?? IntVec3.Invalid,
            SpellEffectLocationSource.CurrentTarget => context?.currentTarget.Cell ?? IntVec3.Invalid,
            SpellEffectLocationSource.InitialTarget => context?.initialTarget.Cell ?? IntVec3.Invalid,
            SpellEffectLocationSource.Caster => context?.caster?.Position ?? IntVec3.Invalid,
            _ => IntVec3.Invalid
        };

        return new TargetInfo(cell, context?.map);
    }
}

public sealed class DelayActionWorker : SpellActionWorker
{
    private readonly SpellScheduler scheduler = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        DelayActionDef delayDef = actionDef as DelayActionDef;
        if (delayDef == null || delayDef.actions == null)
        {
            return;
        }

        int executeAtTick = (Find.TickManager?.TicksGame ?? 0) + delayDef.delayTicks;
        foreach (SpellActionDef childAction in delayDef.actions)
        {
            scheduler.Schedule(context, executeAtTick, childAction);
        }
    }
}

public sealed class RepeatActionWorker : SpellActionWorker
{
    private readonly SpellScheduler scheduler = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        RepeatActionDef repeatDef = actionDef as RepeatActionDef;
        if (repeatDef == null || repeatDef.actions == null)
        {
            return;
        }

        int repeatCount = Mathf.Max(0, SpellPowerUtility.ResolveScalableInt(context, repeatDef.repeatCount, repeatDef.scalableRepeatCount));
        if (repeatCount <= 0)
        {
            return;
        }

        int intervalTicks = Mathf.Max(1, SpellPowerUtility.ResolveScalableInt(context, repeatDef.intervalTicks, repeatDef.scalableIntervalTicks));
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        int firstScheduledPulse = repeatDef.includeImmediate ? 1 : 0;

        if (repeatDef.includeImmediate)
        {
            runner.RunActions(context, repeatDef.actions);
        }

        for (int pulseIndex = firstScheduledPulse; pulseIndex < repeatCount; pulseIndex++)
        {
            int delayMultiplier = repeatDef.includeImmediate ? pulseIndex : pulseIndex + 1;
            int executeAtTick = currentTick + (intervalTicks * delayMultiplier);
            foreach (SpellActionDef childAction in repeatDef.actions)
            {
                scheduler.Schedule(context, executeAtTick, childAction);
            }
        }
    }
}

public sealed class ProximityTriggerActionWorker : SpellActionWorker
{
    private readonly SpellTriggerService triggerService = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ProximityTriggerActionDef triggerActionDef = actionDef as ProximityTriggerActionDef;
        if (triggerActionDef == null)
        {
            return;
        }

        triggerService.ArmProximityTrigger(context, triggerActionDef);
    }
}

public sealed class PersistentEffectActionWorker : SpellActionWorker
{
    private readonly PersistentSpellEffectService persistentEffectService = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        PersistentEffectActionDef persistentEffectActionDef = actionDef as PersistentEffectActionDef;
        if (persistentEffectActionDef == null)
        {
            return;
        }

        persistentEffectService.CreatePersistentEffect(context, persistentEffectActionDef);
    }
}

public sealed class PersistentWallZoneActionWorker : SpellActionWorker
{
    private readonly PersistentWallZoneService persistentWallZoneService = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        PersistentWallZoneActionDef persistentWallZoneActionDef = actionDef as PersistentWallZoneActionDef;
        if (persistentWallZoneActionDef == null)
        {
            return;
        }

        persistentWallZoneService.CreateWallZone(context, persistentWallZoneActionDef);
    }
}

public sealed class PersistentAreaZoneActionWorker : SpellActionWorker
{
    private readonly PersistentAreaZoneService persistentAreaZoneService = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        PersistentAreaZoneActionDef persistentAreaZoneActionDef = actionDef as PersistentAreaZoneActionDef;
        if (persistentAreaZoneActionDef == null)
        {
            return;
        }

        persistentAreaZoneService.CreateAreaZone(context, persistentAreaZoneActionDef);
    }
}

public sealed class SummonPawnActionWorker : SpellActionWorker
{
    private readonly SummonedPawnService summonedPawnService = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        SummonPawnActionDef summonPawnActionDef = actionDef as SummonPawnActionDef;
        if (summonPawnActionDef == null)
        {
            return;
        }

        summonedPawnService.CreateSummonedPawn(context, summonPawnActionDef);
    }
}

public sealed class SpawnThingActionWorker : SpellActionWorker
{
    private readonly SpawnedThingService spawnedThingService = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        SpawnThingActionDef spawnThingActionDef = actionDef as SpawnThingActionDef;
        if (spawnThingActionDef == null)
        {
            return;
        }

        spawnedThingService.CreateSpawnedThing(context, spawnThingActionDef);
    }
}

public sealed class KnockbackActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        KnockbackActionDef knockbackActionDef = actionDef as KnockbackActionDef;
        Pawn targetPawn = context?.currentTarget.Thing as Pawn;
        Pawn casterPawn = context?.caster as Pawn;
        Map map = context?.map;
        if (knockbackActionDef == null || targetPawn == null || casterPawn == null || map == null || targetPawn.Destroyed)
        {
            return;
        }

        if (!TryResolveDestination(casterPawn, targetPawn, map, knockbackActionDef, out IntVec3 destination))
        {
            Log.Message($"[MagicFramework] KnockbackActionWorker could not find a valid destination for {targetPawn.LabelCap}.");
            return;
        }

        targetPawn.pather?.StopDead();
        targetPawn.Position = destination;
        targetPawn.stances?.CancelBusyStanceHard();
        context.SetCurrentTarget(new LocalTargetInfo(targetPawn));
        Log.Message($"[MagicFramework] Knocked back {targetPawn.LabelCap} to {destination}.");
    }

    private static bool TryResolveDestination(Pawn casterPawn, Pawn targetPawn, Map map, KnockbackActionDef actionDef, out IntVec3 destination)
    {
        destination = IntVec3.Invalid;
        IntVec3 start = targetPawn.Position;
        int dx = start.x - casterPawn.Position.x;
        int dz = start.z - casterPawn.Position.z;
        if (dx == 0 && dz == 0)
        {
            dz = 1;
        }

        IntVec3 direction = new(
            dx == 0 ? 0 : dx > 0 ? 1 : -1,
            0,
            dz == 0 ? 0 : dz > 0 ? 1 : -1);

        IntVec3 bestCell = start;
        for (int step = 1; step <= (actionDef.distance > 0 ? actionDef.distance : 1); step++)
        {
            IntVec3 candidate = start + (direction * step);
            if (!candidate.InBounds(map))
            {
                break;
            }

            if (!actionDef.allowHitCasterCell && candidate == casterPawn.Position)
            {
                break;
            }

            if (!IsValidDestination(candidate, map, actionDef))
            {
                break;
            }

            bestCell = candidate;
        }

        if (bestCell == start)
        {
            return false;
        }

        destination = bestCell;
        return true;
    }

    private static bool IsValidDestination(IntVec3 cell, Map map, KnockbackActionDef actionDef)
    {
        if (!cell.InBounds(map))
        {
            return false;
        }

        if (actionDef.requireWalkableDestination && !cell.Walkable(map))
        {
            return false;
        }

        if (actionDef.requireStandableDestination && !cell.Standable(map))
        {
            return false;
        }

        return true;
    }
}

public sealed class PullActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        PullActionDef pullActionDef = actionDef as PullActionDef;
        Pawn targetPawn = context?.currentTarget.Thing as Pawn;
        Pawn casterPawn = context?.caster as Pawn;
        Map map = context?.map;
        if (pullActionDef == null || targetPawn == null || casterPawn == null || map == null || targetPawn.Destroyed)
        {
            return;
        }

        if (!TryResolveDestination(casterPawn, targetPawn, map, pullActionDef, out IntVec3 destination))
        {
            Log.Message($"[MagicFramework] PullActionWorker could not find a valid destination for {targetPawn.LabelCap}.");
            return;
        }

        targetPawn.pather?.StopDead();
        targetPawn.Position = destination;
        targetPawn.stances?.CancelBusyStanceHard();
        context.SetCurrentTarget(new LocalTargetInfo(targetPawn));
        Log.Message($"[MagicFramework] Pulled {targetPawn.LabelCap} to {destination}.");
    }

    private static bool TryResolveDestination(Pawn casterPawn, Pawn targetPawn, Map map, PullActionDef actionDef, out IntVec3 destination)
    {
        destination = IntVec3.Invalid;
        IntVec3 start = targetPawn.Position;
        int dx = casterPawn.Position.x - start.x;
        int dz = casterPawn.Position.z - start.z;
        if (dx == 0 && dz == 0)
        {
            return false;
        }

        IntVec3 direction = new(
            dx == 0 ? 0 : dx > 0 ? 1 : -1,
            0,
            dz == 0 ? 0 : dz > 0 ? 1 : -1);

        IntVec3 bestCell = start;
        for (int step = 1; step <= (actionDef.distance > 0 ? actionDef.distance : 1); step++)
        {
            IntVec3 candidate = start + (direction * step);
            if (!candidate.InBounds(map))
            {
                break;
            }

            if (candidate.DistanceTo(casterPawn.Position) < actionDef.minDistanceFromCaster)
            {
                break;
            }

            if (!IsValidDestination(candidate, map, actionDef))
            {
                break;
            }

            bestCell = candidate;
        }

        if (bestCell == start)
        {
            return false;
        }

        destination = bestCell;
        return true;
    }

    private static bool IsValidDestination(IntVec3 cell, Map map, PullActionDef actionDef)
    {
        if (!cell.InBounds(map))
        {
            return false;
        }

        if (actionDef.requireWalkableDestination && !cell.Walkable(map))
        {
            return false;
        }

        if (actionDef.requireStandableDestination && !cell.Standable(map))
        {
            return false;
        }

        return true;
    }
}

public sealed class TeleportActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        TeleportActionDef teleportActionDef = actionDef as TeleportActionDef;
        if (teleportActionDef == null || context?.map == null)
        {
            return;
        }

        Pawn subjectPawn = ResolveSubjectPawn(context, teleportActionDef.subjectSource);
        if (subjectPawn == null || subjectPawn.Destroyed)
        {
            Log.Warning("[MagicFramework] TeleportActionWorker skipped because the teleport subject was not a valid pawn.");
            return;
        }

        if (teleportActionDef.swapWithCaster)
        {
            ExecuteSwapWithCaster(context, teleportActionDef, subjectPawn);
            return;
        }

        if (!TryResolveDestination(context, subjectPawn, teleportActionDef, out IntVec3 destination))
        {
            Log.Message($"[MagicFramework] TeleportActionWorker could not find a valid destination for {subjectPawn.LabelCap}.");
            return;
        }

        Map map = subjectPawn.MapHeld ?? context.map;
        if (map == null)
        {
            return;
        }

        MovePawn(subjectPawn, destination, map);

        if (teleportActionDef.subjectSource == TeleportSubjectSource.CurrentTarget)
        {
            context.SetCurrentTarget(new LocalTargetInfo(subjectPawn));
        }
        else
        {
            context.currentCell = destination;
        }

        Log.Message($"[MagicFramework] Teleported {subjectPawn.LabelCap} to {destination}.");
    }

    private static Pawn ResolveSubjectPawn(SpellContext context, TeleportSubjectSource subjectSource)
    {
        return subjectSource switch
        {
            TeleportSubjectSource.CurrentTarget => context?.currentTarget.Thing as Pawn,
            TeleportSubjectSource.InitialTarget => context?.initialTarget.Thing as Pawn,
            _ => context?.caster as Pawn
        };
    }

    private static bool TryResolveDestination(SpellContext context, Pawn subjectPawn, TeleportActionDef actionDef, out IntVec3 destination)
    {
        destination = actionDef.destinationSource switch
        {
            TeleportDestinationSource.InitialTargetCell => context?.initialTarget.Cell ?? IntVec3.Invalid,
            TeleportDestinationSource.CurrentTargetCell => context?.currentTarget.Cell ?? IntVec3.Invalid,
            TeleportDestinationSource.CasterCell => context?.caster?.Position ?? IntVec3.Invalid,
            TeleportDestinationSource.CasterAdjacentCell => ResolveAdjacentCell(context?.caster?.Position ?? IntVec3.Invalid, context?.map, subjectPawn, actionDef),
            TeleportDestinationSource.RandomCellNearSubject => ResolveRandomCellNear(subjectPawn?.Position ?? IntVec3.Invalid, context?.map, subjectPawn, actionDef),
            TeleportDestinationSource.RandomCellNearCaster => ResolveRandomCellNear(context?.caster?.Position ?? IntVec3.Invalid, context?.map, subjectPawn, actionDef),
            TeleportDestinationSource.RandomCellNearCurrentCell => ResolveRandomCellNear(context?.currentCell ?? IntVec3.Invalid, context?.map, subjectPawn, actionDef),
            TeleportDestinationSource.RandomCellNearInitialTarget => ResolveRandomCellNear(context?.initialTarget.Cell ?? IntVec3.Invalid, context?.map, subjectPawn, actionDef),
            _ => context?.currentCell ?? IntVec3.Invalid
        };

        if (context?.map == null || !destination.IsValid || !destination.InBounds(context.map))
        {
            return false;
        }

        if (!actionDef.allowSameCell && destination == subjectPawn.Position)
        {
            return false;
        }

        if (!actionDef.allowTeleportOntoCaster && context.caster != null && subjectPawn != context.caster && destination == context.caster.Position)
        {
            return false;
        }

        return IsValidDestination(destination, context.map, subjectPawn, actionDef);
    }

    private static void ExecuteSwapWithCaster(SpellContext context, TeleportActionDef actionDef, Pawn subjectPawn)
    {
        Pawn casterPawn = context?.caster as Pawn;
        if (casterPawn == null || casterPawn.Destroyed || subjectPawn == casterPawn)
        {
            Log.Warning("[MagicFramework] TeleportActionWorker could not swap because the caster and subject were not distinct valid pawns.");
            return;
        }

        Map map = subjectPawn.MapHeld ?? casterPawn.MapHeld ?? context?.map;
        if (map == null || subjectPawn.MapHeld != casterPawn.MapHeld)
        {
            Log.Warning("[MagicFramework] TeleportActionWorker could not swap because the caster and subject were not on the same map.");
            return;
        }

        IntVec3 subjectCell = subjectPawn.Position;
        IntVec3 casterCell = casterPawn.Position;
        if (!IsValidDestination(casterCell, map, subjectPawn, actionDef, ignoreOccupants: true)
            || !IsValidDestination(subjectCell, map, casterPawn, actionDef, ignoreOccupants: true))
        {
            Log.Message("[MagicFramework] TeleportActionWorker could not swap because one of the destination cells was invalid.");
            return;
        }

        SwapPawns(subjectPawn, casterPawn, subjectCell, casterCell, map);
        context.SetCurrentTarget(new LocalTargetInfo(subjectPawn));
        Log.Message($"[MagicFramework] Swapped {subjectPawn.LabelCap} with {casterPawn.LabelCap}.");
    }

    private static IntVec3 ResolveAdjacentCell(IntVec3 center, Map map, Pawn subjectPawn, TeleportActionDef actionDef)
    {
        if (map == null || !center.IsValid)
        {
            return IntVec3.Invalid;
        }

        IntVec3 bestCell = IntVec3.Invalid;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < 8; i++)
        {
            IntVec3 candidate = center + GenAdj.AdjacentCells[i];
            if (!IsValidDestination(candidate, map, subjectPawn, actionDef))
            {
                continue;
            }

            float distance = subjectPawn != null ? candidate.DistanceTo(subjectPawn.Position) : 0f;
            if (!bestCell.IsValid || distance < bestDistance)
            {
                bestCell = candidate;
                bestDistance = distance;
            }
        }

        return bestCell;
    }

    private static IntVec3 ResolveRandomCellNear(IntVec3 center, Map map, Pawn subjectPawn, TeleportActionDef actionDef)
    {
        if (map == null || !center.IsValid)
        {
            return IntVec3.Invalid;
        }

        int maxRadius = Mathf.Max(1, actionDef.randomRadius);
        int minRadius = Mathf.Clamp(actionDef.randomMinRadius, 0, maxRadius);
        int attempts = Mathf.Max(1, actionDef.randomCellSearchAttempts);
        for (int i = 0; i < attempts; i++)
        {
            int xOffset = Rand.RangeInclusive(-maxRadius, maxRadius);
            int zOffset = Rand.RangeInclusive(-maxRadius, maxRadius);
            IntVec3 candidate = new(center.x + xOffset, center.y, center.z + zOffset);
            float distance = candidate.DistanceTo(center);
            if (distance < minRadius || distance > maxRadius)
            {
                continue;
            }

            if (IsValidDestination(candidate, map, subjectPawn, actionDef))
            {
                return candidate;
            }
        }

        return IntVec3.Invalid;
    }

    private static bool IsValidDestination(
        IntVec3 destination,
        Map map,
        Pawn subjectPawn,
        TeleportActionDef actionDef,
        bool ignoreOccupants = false)
    {
        if (map == null || !destination.IsValid || !destination.InBounds(map))
        {
            return false;
        }

        if (actionDef.requireWalkableDestination && !destination.Walkable(map))
        {
            return false;
        }

        if (actionDef.requireStandableDestination && !destination.Standable(map))
        {
            return false;
        }

        if (!ignoreOccupants && actionDef.requireUnoccupiedDestination && IsOccupiedByBlockingPawn(destination, map, subjectPawn))
        {
            return false;
        }

        return true;
    }

    private static bool IsOccupiedByBlockingPawn(IntVec3 destination, Map map, Pawn subjectPawn)
    {
        List<Thing> things = destination.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            if (things[i] is Pawn pawn && pawn != subjectPawn && !pawn.Destroyed)
            {
                return true;
            }
        }

        return false;
    }

    private static void MovePawn(Pawn pawn, IntVec3 destination, Map map)
    {
        if (pawn.Spawned)
        {
            pawn.pather?.StopDead();
            pawn.stances?.CancelBusyStanceHard();
            pawn.DeSpawn();
            GenSpawn.Spawn(pawn, destination, map);
        }
        else
        {
            pawn.Position = destination;
        }

        pawn.pather?.StopDead();
        pawn.stances?.CancelBusyStanceHard();
    }

    private static void SwapPawns(Pawn subjectPawn, Pawn casterPawn, IntVec3 subjectCell, IntVec3 casterCell, Map map)
    {
        bool subjectSpawned = subjectPawn.Spawned;
        bool casterSpawned = casterPawn.Spawned;

        subjectPawn.pather?.StopDead();
        subjectPawn.stances?.CancelBusyStanceHard();
        casterPawn.pather?.StopDead();
        casterPawn.stances?.CancelBusyStanceHard();

        if (subjectSpawned)
        {
            subjectPawn.DeSpawn();
        }

        if (casterSpawned)
        {
            casterPawn.DeSpawn();
        }

        if (subjectSpawned)
        {
            GenSpawn.Spawn(subjectPawn, casterCell, map);
        }
        else
        {
            subjectPawn.Position = casterCell;
        }

        if (casterSpawned)
        {
            GenSpawn.Spawn(casterPawn, subjectCell, map);
        }
        else
        {
            casterPawn.Position = subjectCell;
        }

        subjectPawn.pather?.StopDead();
        subjectPawn.stances?.CancelBusyStanceHard();
        casterPawn.pather?.StopDead();
        casterPawn.stances?.CancelBusyStanceHard();
    }
}

public sealed class ApplyStatModifierActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ApplyStatModifierActionDef statModifierActionDef = actionDef as ApplyStatModifierActionDef;
        if (statModifierActionDef == null)
        {
            return;
        }

        Thing targetThing = statModifierActionDef.targetSource == StatModifierTargetSource.Caster
            ? context?.caster
            : context?.currentTarget.Thing;
        if (targetThing == null || targetThing.Destroyed)
        {
            Log.Warning("[MagicFramework] ApplyStatModifierActionWorker skipped because the target thing was invalid.");
            return;
        }

        if (statModifierActionDef.modifiers == null || statModifierActionDef.modifiers.Count == 0)
        {
            Log.Warning("[MagicFramework] ApplyStatModifierActionWorker skipped because no modifiers were authored.");
            return;
        }

        SpellRuntimeGameComponent.Instance?.ApplyStatModifiers(
            targetThing,
            context?.caster,
            context?.spellDef,
            Mathf.Max(1, SpellPowerUtility.ResolveScalableInt(context, statModifierActionDef.durationTicks, statModifierActionDef.scalableDurationTicks)),
            statModifierActionDef.replaceExistingFromCasterSpell,
            SpellStatusCueUtility.ResolveStatusCue(context, statModifierActionDef.statusCue, statModifierActionDef.indicatorHediffDef, statModifierActionDef.indicatorSeverity, statModifierActionDef.removeIndicatorOnExpire),
            statModifierActionDef.modifiers);
    }
}

public sealed class SustainedStatModifierActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        SustainedStatModifierActionDef statModifierActionDef = actionDef as SustainedStatModifierActionDef;
        if (statModifierActionDef == null)
        {
            return;
        }

        Thing targetThing = statModifierActionDef.targetSource == StatModifierTargetSource.Caster
            ? context?.caster
            : context?.currentTarget.Thing;
        if (targetThing == null || targetThing.Destroyed)
        {
            Log.Warning("[MagicFramework] SustainedStatModifierActionWorker skipped because the target thing was invalid.");
            return;
        }

        if (statModifierActionDef.modifiers == null || statModifierActionDef.modifiers.Count == 0)
        {
            Log.Warning("[MagicFramework] SustainedStatModifierActionWorker skipped because no modifiers were authored.");
            return;
        }

        SpellActionPathUtility.TryCreatePath(context?.spellDef, statModifierActionDef, out List<int> sourceActionPath);
        SpellRuntimeGameComponent.Instance?.ApplySustainedStatModifiers(
            targetThing,
            context?.caster,
            context?.spellDef,
            SpellActionScalingUtility.ResolveOptionalPositiveTicks(context, statModifierActionDef.maxDurationTicks, statModifierActionDef.scalableMaxDurationTicks),
            statModifierActionDef.replaceExistingFromCasterSpell,
            SpellStatusCueUtility.ResolveStatusCue(context, statModifierActionDef.statusCue, statModifierActionDef.indicatorHediffDef, statModifierActionDef.indicatorSeverity, statModifierActionDef.removeIndicatorOnExpire),
            statModifierActionDef.maxRange,
            statModifierActionDef.breakWhenCasterDowned,
            statModifierActionDef.breakWhenTargetDowned,
            statModifierActionDef.breakWhenTargetOutOfRange,
            statModifierActionDef.breakWhenLineOfSightLost,
            sourceActionPath,
            statModifierActionDef.modifiers);
    }
}

public sealed class ApplyForceFieldActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ApplyForceFieldActionDef forceFieldActionDef = actionDef as ApplyForceFieldActionDef;
        if (forceFieldActionDef == null)
        {
            return;
        }

        Thing targetThing = forceFieldActionDef.targetSource == StatModifierTargetSource.Caster
            ? context?.caster
            : context?.currentTarget.Thing;
        if (targetThing == null || targetThing.Destroyed)
        {
            Log.Warning("[MagicFramework] ApplyForceFieldActionWorker skipped because the target thing was invalid.");
            return;
        }

        SpellActionPathUtility.TryCreatePath(context?.spellDef, forceFieldActionDef, out List<int> sourceActionPath);
        SpellRuntimeGameComponent.Instance?.ApplyForceField(
            targetThing,
            context?.caster,
            context?.spellDef,
            SpellActionScalingUtility.ResolveOptionalPositiveTicks(context, forceFieldActionDef.maxDurationTicks, forceFieldActionDef.scalableMaxDurationTicks),
            SpellStatusCueUtility.ResolveStatusCue(context, forceFieldActionDef.statusCue, null, 0.01f, true),
            forceFieldActionDef.damageFactor,
            forceFieldActionDef.absorbFullyWithMana,
            forceFieldActionDef.manaCostPerDamageAbsorbed,
            forceFieldActionDef.maxRange,
            forceFieldActionDef.breakWhenCasterDowned,
            forceFieldActionDef.breakWhenTargetDowned,
            forceFieldActionDef.breakWhenTargetOutOfRange,
            forceFieldActionDef.breakWhenLineOfSightLost,
            forceFieldActionDef.impactFleckDef,
            forceFieldActionDef.impactSoundDef,
            forceFieldActionDef.ambientFleckDef,
            forceFieldActionDef.ambientFleckIntervalTicks,
            forceFieldActionDef.ambientFleckScale,
            forceFieldActionDef.ambientColorHex,
            forceFieldActionDef.sustainedOverlayTexturePath,
            forceFieldActionDef.sustainedOverlayScale,
            forceFieldActionDef.sustainedOverlayColorHex,
            sourceActionPath);
    }
}

public sealed class ClearStatModifiersActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ClearStatModifiersActionDef clearActionDef = actionDef as ClearStatModifiersActionDef;
        if (clearActionDef == null)
        {
            return;
        }

        Thing targetThing = clearActionDef.targetSource == StatModifierTargetSource.Caster
            ? context?.caster
            : context?.currentTarget.Thing;
        if (targetThing == null || targetThing.Destroyed)
        {
            Log.Warning("[MagicFramework] ClearStatModifiersActionWorker skipped because the target thing was invalid.");
            return;
        }

        SpellRuntimeGameComponent.Instance?.ClearStatModifiers(
            targetThing,
            context?.caster,
            context?.spellDef,
            clearActionDef.scope,
            clearActionDef.spellDef,
            clearActionDef.statusHediffDef,
            clearActionDef.runBreakActions);
    }
}

public sealed class ChainLightningActionWorker : SpellActionWorker
{
    private readonly ChainLightningService chainLightningService = new();

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ChainLightningActionDef chainLightningActionDef = actionDef as ChainLightningActionDef;
        if (chainLightningActionDef == null)
        {
            return;
        }

        Thing initialTarget = context?.currentTarget.Thing;
        if (initialTarget == null || initialTarget.Destroyed)
        {
            Log.Warning("[MagicFramework] ChainLightningActionWorker skipped because the current target was invalid.");
            return;
        }

        chainLightningService.StartChain(context, chainLightningActionDef, initialTarget);
    }
}

public sealed class StunActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        StunActionDef stunActionDef = actionDef as StunActionDef;
        Pawn targetPawn = context?.currentTarget.Thing as Pawn;
        if (stunActionDef == null || targetPawn == null || targetPawn.Destroyed)
        {
            return;
        }

        if (stunActionDef.chance < 1f && !Rand.Chance(Mathf.Max(0f, stunActionDef.chance)))
        {
            return;
        }

        targetPawn.stances?.stunner?.StunFor(stunActionDef.stunTicks > 0 ? stunActionDef.stunTicks : 1, context?.caster);
        if (!string.IsNullOrWhiteSpace(stunActionDef.fleckDef))
        {
            FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(stunActionDef.fleckDef);
            if (fleckDef != null && targetPawn.Map != null)
            {
                FleckMaker.Static(targetPawn.DrawPos, targetPawn.Map, fleckDef, 1f);
            }
        }

        Log.Message($"[MagicFramework] Stunned {targetPawn.LabelCap} for {stunActionDef.stunTicks} ticks.");
    }
}

public sealed class HealActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        HealActionDef healActionDef = actionDef as HealActionDef;
        Pawn targetPawn = context?.currentTarget.Thing as Pawn;
        if (healActionDef == null || targetPawn == null || targetPawn.Destroyed || targetPawn.health?.hediffSet == null)
        {
            Log.Warning("[MagicFramework] HealActionWorker skipped because the current target was not a valid pawn.");
            return;
        }

        float amount = Mathf.Max(0f, SpellPowerUtility.ResolveScalableFloat(context, healActionDef.amount, healActionDef.scalableAmount));
        if (amount <= 0f)
        {
            return;
        }

        float healed = HealInjuriesEvenly(targetPawn, amount);
        if (healed > 0f)
        {
            targetPawn.health.Notify_HediffChanged(null);
        }

        Log.Message($"[MagicFramework] Healed {healed:0.##}/{amount:0.##} injury severity on {targetPawn.LabelCap}.");
    }

    private static float HealInjuriesEvenly(Pawn pawn, float amount)
    {
        List<Hediff_Injury> injuries = new();
        List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++)
        {
            if (hediffs[i] is Hediff_Injury injury && injury.Severity > 0f)
            {
                injuries.Add(injury);
            }
        }

        float remainingHeal = amount;
        float totalHealed = 0f;
        while (remainingHeal > 0f && injuries.Count > 0)
        {
            float share = remainingHeal / injuries.Count;
            float spentThisPass = 0f;

            for (int i = injuries.Count - 1; i >= 0; i--)
            {
                Hediff_Injury injury = injuries[i];
                float healAmount = Mathf.Min(share, injury.Severity);
                if (healAmount <= 0f)
                {
                    injuries.RemoveAt(i);
                    continue;
                }

                injury.Heal(healAmount);
                spentThisPass += healAmount;
                totalHealed += healAmount;

                if (injury.Severity <= 0f || !pawn.health.hediffSet.hediffs.Contains(injury))
                {
                    injuries.RemoveAt(i);
                }
            }

            if (spentThisPass <= 0f)
            {
                break;
            }

            remainingHeal -= spentThisPass;
        }

        return totalHealed;
    }
}

internal static class SpellStatusCueUtility
{
    public static SpellStatusCueDef ResolveStatusCue(
        SpellContext context,
        SpellStatusCueDef authoredStatusCue,
        string legacyIndicatorHediffDef,
        float legacyIndicatorSeverity,
        bool legacyRemoveIndicatorOnExpire)
    {
        if (authoredStatusCue != null)
        {
            return authoredStatusCue.enabled ? authoredStatusCue : null;
        }

        if (!string.IsNullOrWhiteSpace(legacyIndicatorHediffDef))
        {
            return new SpellStatusCueDef
            {
                hediffDef = legacyIndicatorHediffDef,
                severity = legacyIndicatorSeverity,
                removeOnExpire = legacyRemoveIndicatorOnExpire
            };
        }

        Log.Warning($"[MagicFramework] {context?.spellDef?.defName ?? "<unknown spell>"} applies a pawn stat modifier without a statusCue; using the generic status cue.");
        string spellLabel = context?.spellDef?.LabelCap ?? context?.spellDef?.defName ?? "spell";
        return new SpellStatusCueDef
        {
            label = $"Affected by {spellLabel}",
            description = $"This pawn is affected by {spellLabel}.",
            severity = 0.01f,
            removeOnExpire = true
        };
    }
}

internal static class SpellActionScalingUtility
{
    public static int ResolveOptionalPositiveTicks(SpellContext context, int fallbackTicks, ScalableFloatDef scalableTicks)
    {
        int resolvedTicks = SpellPowerUtility.ResolveScalableInt(context, fallbackTicks, scalableTicks);
        if (resolvedTicks <= 0)
        {
            return -1;
        }

        return resolvedTicks;
    }
}

public sealed class DamageActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        DamageActionDef damageActionDef = actionDef as DamageActionDef;
        if (damageActionDef == null)
        {
            return;
        }

        Thing targetThing = context?.currentTarget.Thing;
        if (targetThing == null || targetThing.Destroyed)
        {
            Log.Warning("[MagicFramework] DamageActionWorker skipped because the current target was not a valid Thing.");
            return;
        }

        DamageDef resolvedDamageDef = ResolveDamageDef(damageActionDef);
        if (resolvedDamageDef == null)
        {
            Log.Warning($"[MagicFramework] DamageActionWorker skipped because damage def '{damageActionDef.damageDef ?? "<null>"}' could not be resolved.");
            return;
        }

        float damageAmount = SpellPowerUtility.ResolveScalableFloat(context, damageActionDef.amount, damageActionDef.scalableAmount);
        float armorPenetration = SpellPowerUtility.ResolveScalableFloat(context, damageActionDef.armorPenetration, damageActionDef.scalableArmorPenetration);
        DamageInfo damageInfo = new(
            resolvedDamageDef,
            damageAmount,
            armorPenetration,
            instigator: context?.caster);
        targetThing.TakeDamage(damageInfo);
        Log.Message(
            $"[MagicFramework] Applied {damageAmount} {resolvedDamageDef.defName} damage to {targetThing.LabelCap} with {armorPenetration} armor penetration.");
    }

    private static DamageDef ResolveDamageDef(DamageActionDef damageActionDef)
    {
        if (string.IsNullOrWhiteSpace(damageActionDef.damageDef))
        {
            return DamageDefOf.Blunt;
        }

        DamageDef authoredDamageDef = DefDatabase<DamageDef>.GetNamedSilentFail(damageActionDef.damageDef);
        if (authoredDamageDef != null)
        {
            return authoredDamageDef;
        }

        return null;
    }
}

public sealed class ApplyHediffActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ApplyHediffActionDef hediffActionDef = actionDef as ApplyHediffActionDef;
        if (hediffActionDef == null)
        {
            return;
        }

        Pawn targetPawn = context?.currentTarget.Thing as Pawn;
        if (targetPawn == null || targetPawn.Destroyed || targetPawn.health == null)
        {
            Log.Warning("[MagicFramework] ApplyHediffActionWorker skipped because the current target was not a valid pawn.");
            return;
        }

        HediffDef resolvedHediffDef = ResolveHediffDef(hediffActionDef);
        if (resolvedHediffDef == null)
        {
            Log.Warning($"[MagicFramework] ApplyHediffActionWorker skipped because hediff def '{hediffActionDef.hediffDef ?? "<null>"}' could not be resolved.");
            return;
        }

        HealthUtility.AdjustSeverity(targetPawn, resolvedHediffDef, hediffActionDef.severity);
        Log.Message($"[MagicFramework] Applied hediff {resolvedHediffDef.defName} with severity {hediffActionDef.severity} to {targetPawn.LabelCap}.");
    }

    private static HediffDef ResolveHediffDef(ApplyHediffActionDef hediffActionDef)
    {
        if (string.IsNullOrWhiteSpace(hediffActionDef?.hediffDef))
        {
            return null;
        }

        return DefDatabase<HediffDef>.GetNamedSilentFail(hediffActionDef.hediffDef);
    }
}

public sealed class DestroyThingActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        DestroyThingActionDef destroyActionDef = actionDef as DestroyThingActionDef;
        Thing targetThing = context?.currentTarget.Thing;
        if (destroyActionDef == null || targetThing == null || targetThing.Destroyed)
        {
            return;
        }

        if (!destroyActionDef.allowPawns && targetThing is Pawn)
        {
            Log.Warning($"[MagicFramework] DestroyThingActionWorker skipped pawn target {targetThing.LabelCap} because allowPawns was false.");
            return;
        }

        targetThing.Destroy(DestroyMode.Vanish);
        Log.Message($"[MagicFramework] Destroyed {targetThing.LabelCap} via disintegration-style effect.");
    }
}

public sealed class ExplosionActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ExplosionActionDef explosionActionDef = actionDef as ExplosionActionDef;
        if (explosionActionDef == null)
        {
            return;
        }

        if (context?.map == null || !context.currentCell.IsValid)
        {
            Log.Warning("[MagicFramework] ExplosionActionWorker skipped because the current cell or map was invalid.");
            return;
        }

        float radius = SpellPowerUtility.ResolveScalableFloat(context, explosionActionDef.radius, explosionActionDef.scalableRadius);
        float damageAmount = SpellPowerUtility.ResolveScalableFloat(context, explosionActionDef.damageAmount, explosionActionDef.scalableDamageAmount);

        GenExplosion.DoExplosion(
            context.currentCell,
            context.map,
            radius,
            DamageDefOf.Flame,
            context.caster,
            damAmount: Mathf.RoundToInt(damageAmount),
            armorPenetration: -1f,
            explosionSound: DefDatabase<SoundDef>.GetNamedSilentFail("Explosion_Flame"),
            projectile: context.currentTarget.Thing?.def,
            intendedTarget: context.currentTarget.Thing,
            chanceToStartFire: 0.35f,
            damageFalloff: true);

        Log.Message(
            $"[MagicFramework] Triggered flame explosion at {context.currentCell} with radius {radius} and damage {damageAmount}.");
    }
}

public sealed class LaunchProjectileActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        LaunchProjectileActionDef projectileActionDef = actionDef as LaunchProjectileActionDef;
        if (projectileActionDef == null)
        {
            return;
        }

        Thing launcher = context?.caster;
        Map map = context?.map;
        LocalTargetInfo target = ResolveTarget(context);
        ThingDef projectileDef = ResolveProjectileDef(projectileActionDef);
        string projectileLabel = projectileDef?.defName ?? projectileActionDef.projectileDef ?? "<none>";

        if (launcher == null || map == null || !target.IsValid)
        {
            Log.Warning("[MagicFramework] LaunchProjectileActionWorker could not launch because the caster, map, or target was invalid. Executing impact actions immediately.");
            RunImpactActionsImmediately(context, projectileActionDef, runner);
            return;
        }

        if (projectileDef == null || projectileDef.thingClass == null || !typeof(Projectile).IsAssignableFrom(projectileDef.thingClass))
        {
            Log.Warning($"[MagicFramework] LaunchProjectileActionWorker could not resolve projectile def '{projectileActionDef.projectileDef ?? "<null>"}'. Executing impact actions immediately.");
            RunImpactActionsImmediately(context, projectileActionDef, runner);
            return;
        }

        Projectile projectile = GenSpawn.Spawn(projectileDef, launcher.Position, map) as Projectile;
        if (projectile == null)
        {
            Log.Warning($"[MagicFramework] LaunchProjectileActionWorker failed to spawn projectile '{projectileLabel}'. Executing impact actions immediately.");
            RunImpactActionsImmediately(context, projectileActionDef, runner);
            return;
        }

        context.SetCurrentTarget(target);
        Vector3 origin = launcher.Spawned ? launcher.DrawPos : launcher.Position.ToVector3Shifted();
        projectile.Launch(
            launcher,
            origin,
            target,
            target,
            projectileActionDef.hitFlags,
            projectileActionDef.preventFriendlyFire);

        Log.Message(
            $"[MagicFramework] Launched projectile {projectileLabel} from {launcher.LabelCap} to {DescribeTarget(target)}.");

        if (projectileActionDef.onImpactActions == null || projectileActionDef.onImpactActions.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        int flightTicks = EstimateFlightTicks(launcher, target, projectileDef);
        int explosionDelayTicks = Mathf.Max(0, projectileDef.projectile?.explosionDelay ?? 0);
        int timeoutPaddingTicks = Mathf.Max(1, projectileActionDef.impactTimeoutPaddingTicks);
        int timeoutTick = currentTick + flightTicks + explosionDelayTicks + timeoutPaddingTicks;
        PendingProjectileImpact pendingImpact = new(projectile, timeoutTick, context, projectileActionDef.onImpactActions);
        ProjectileImpactMapComponent impactRuntime = map.GetComponent<ProjectileImpactMapComponent>();
        if (impactRuntime == null || !impactRuntime.Enqueue(pendingImpact))
        {
            Log.Warning("[MagicFramework] Projectile impact runtime was unavailable. Executing impact actions immediately.");
            RunImpactActionsImmediately(context, projectileActionDef, runner);
        }
    }

    private static LocalTargetInfo ResolveTarget(SpellContext context)
    {
        if (context == null)
        {
            return LocalTargetInfo.Invalid;
        }

        if (context.currentTarget.IsValid)
        {
            return context.currentTarget;
        }

        return context.currentCell.IsValid ? new LocalTargetInfo(context.currentCell) : LocalTargetInfo.Invalid;
    }

    private static ThingDef ResolveProjectileDef(LaunchProjectileActionDef projectileActionDef)
    {
        if (string.IsNullOrWhiteSpace(projectileActionDef?.projectileDef))
        {
            return null;
        }

        return DefDatabase<ThingDef>.GetNamedSilentFail(projectileActionDef.projectileDef);
    }

    private static int EstimateFlightTicks(Thing launcher, LocalTargetInfo target, ThingDef projectileDef)
    {
        if (launcher == null || projectileDef?.projectile == null || !target.Cell.IsValid)
        {
            return 1;
        }

        float speed = projectileDef.projectile.SpeedTilesPerTick;
        if (speed <= 0f)
        {
            return 1;
        }

        float distance = launcher.Position.DistanceTo(target.Cell);
        return Mathf.Max(1, Mathf.CeilToInt(distance / speed));
    }

    private static void RunImpactActionsImmediately(SpellContext context, LaunchProjectileActionDef projectileActionDef, SpellActionRunner runner)
    {
        if (projectileActionDef?.onImpactActions == null || projectileActionDef.onImpactActions.Count == 0)
        {
            return;
        }

        runner.RunActions(context, projectileActionDef.onImpactActions);
    }

    private static string DescribeTarget(LocalTargetInfo target)
    {
        if (target.Thing != null)
        {
            return target.Thing.LabelCap;
        }

        return target.Cell.IsValid ? target.Cell.ToString() : "<invalid target>";
    }
}

public sealed class ApplyToTargetsActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ApplyToTargetsActionDef applyDef = actionDef as ApplyToTargetsActionDef;
        if (applyDef?.targetQuery == null)
        {
            Log.Message("[MagicFramework] ApplyToTargetsActionWorker skipped because no target query was provided.");
            return;
        }

        var targets = applyDef.targetQuery.CreateWorker().ResolveTargets(context, applyDef.targetQuery);
        foreach (LocalTargetInfo target in targets)
        {
            context.SetCurrentTarget(target);
            runner.RunActions(context, applyDef.actions);
        }
    }
}

public sealed class ApplyChainTargetsActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ApplyChainTargetsActionDef chainDef = actionDef as ApplyChainTargetsActionDef;
        if (chainDef?.chainQuery == null)
        {
            Log.Message("[MagicFramework] ApplyChainTargetsActionWorker skipped because no chain query was provided.");
            return;
        }

        LocalTargetInfo originalTarget = context.currentTarget;
        IntVec3 originalCell = context.currentCell;
        List<LocalTargetInfo> originalTargets = new(context.currentTargets);

        var targets = chainDef.chainQuery.CreateWorker().ResolveTargets(context, chainDef.chainQuery);
        context.currentTargets.Clear();
        context.currentTargets.AddRange(targets);

        foreach (LocalTargetInfo target in targets)
        {
            if (context.executionState.cancelled || context.executionState.failed)
            {
                break;
            }

            context.SetCurrentTarget(target);
            runner.RunActions(context, chainDef.actions);
        }

        context.currentTargets.Clear();
        context.currentTargets.AddRange(originalTargets);
        context.currentTarget = originalTarget;
        context.currentCell = originalCell;
    }
}

public sealed class ConditionalActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        ConditionalActionDef conditionalDef = actionDef as ConditionalActionDef;
        if (conditionalDef == null)
        {
            return;
        }

        bool result = conditionalDef.condition == null || SpellConditionEvaluator.Evaluate(context, conditionalDef.condition);
        runner.RunActions(context, result ? conditionalDef.thenActions : conditionalDef.elseActions);
    }
}
