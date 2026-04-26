using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Targeting;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MagicFramework.Scheduling;

public sealed class ChainLightningService
{
    private readonly SpellActionRunner actionRunner = new();

    public void StartChain(SpellContext context, ChainLightningActionDef actionDef, Thing initialTarget)
    {
        if (context?.map == null || context.spellDef == null || actionDef == null || initialTarget == null)
        {
            return;
        }

        if (!SpellActionPathUtility.TryCreatePath(context.spellDef, actionDef, out List<int> actionPath))
        {
            Log.Warning($"[MagicFramework] Could not start chain lightning for {context.spellDef.defName} because the action path could not be resolved.");
            return;
        }

        ChainLightningMapComponent runtime = context.map.GetComponent<ChainLightningMapComponent>();
        if (runtime == null)
        {
            Log.Warning("[MagicFramework] Could not start chain lightning because the map runtime was unavailable.");
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        runtime.Enqueue(new ChainLightningPulse(
            context.caster,
            context.spellDef,
            context.caster,
            initialTarget,
            context.caster?.Position ?? initialTarget.Position,
            initialTarget.Position,
            currentTick,
            0,
            actionPath));
    }

    public void ExecutePulse(Map map, ChainLightningPulse pulse)
    {
        if (map == null || pulse?.SpellDef == null || pulse.TargetThing == null || pulse.TargetThing.Destroyed)
        {
            return;
        }

        if (SpellActionPathUtility.ResolveAction(pulse.SpellDef, pulse.ActionPath) is not ChainLightningActionDef actionDef)
        {
            Log.Warning($"[MagicFramework] Dropped chain lightning pulse for {pulse.SpellDef.defName} because its action could not be resolved.");
            return;
        }

        Thing sourceThing = pulse.SourceThing != null && !pulse.SourceThing.Destroyed ? pulse.SourceThing : null;
        IntVec3 sourceCell = pulse.SourceCell.IsValid ? pulse.SourceCell : sourceThing?.Position ?? pulse.Caster?.Position ?? pulse.TargetThing.Position;
        StrikeTarget(map, pulse.Caster, pulse.SpellDef, sourceThing, sourceCell, pulse.TargetThing, actionDef);

        if (pulse.HopIndex >= actionDef.maxHops)
        {
            return;
        }

        List<Thing> nextTargets = FindNextTargets(map, pulse.Caster, sourceCell, pulse.TargetThing, actionDef);
        if (nextTargets.Count == 0)
        {
            return;
        }

        ChainLightningMapComponent runtime = map.GetComponent<ChainLightningMapComponent>();
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        int executeAtTick = currentTick + (actionDef.jumpDelayTicks > 0 ? actionDef.jumpDelayTicks : 1);
        for (int i = 0; i < nextTargets.Count; i++)
        {
            runtime?.Enqueue(new ChainLightningPulse(
                pulse.Caster,
                pulse.SpellDef,
                pulse.TargetThing,
                nextTargets[i],
                pulse.TargetThing.Position,
                nextTargets[i].Position,
                executeAtTick,
                pulse.HopIndex + 1,
                pulse.ActionPath));
        }
    }

    private void StrikeTarget(Map map, Thing caster, SpellDef spellDef, Thing sourceThing, IntVec3 sourceCell, Thing targetThing, ChainLightningActionDef actionDef)
    {
        DrawLightning(map, sourceThing, sourceCell, targetThing, actionDef);

        if (actionDef.onHitActions != null && actionDef.onHitActions.Count > 0)
        {
            SpellContext hitContext = new()
            {
                caster = caster,
                map = map,
                spellDef = spellDef,
                initialTarget = new LocalTargetInfo(targetThing),
                currentTarget = new LocalTargetInfo(targetThing),
                currentCell = targetThing.Position,
                randomSeed = Find.TickManager?.TicksGame ?? 0
            };
            hitContext.currentTargets.Add(hitContext.currentTarget);
            hitContext.executionState.costsApplied = true;
            actionRunner.RunActions(hitContext, actionDef.onHitActions);
        }
        else
        {
            DamageDef damageDef = ResolveDamageDef(actionDef.damageDef);
            DamageInfo damageInfo = new(damageDef, actionDef.damageAmount, actionDef.armorPenetration, instigator: caster);
            targetThing.TakeDamage(damageInfo);

            if (targetThing is Pawn targetPawn && actionDef.stunChance > 0f && Rand.Chance(actionDef.stunChance))
            {
                targetPawn.stances?.stunner?.StunFor(actionDef.stunTicks > 0 ? actionDef.stunTicks : 1, caster);
                SpawnFleck(map, targetPawn.DrawPos, actionDef.stunFleckDef, 1f);
            }

            Log.Message($"[MagicFramework] Chain lightning struck {targetThing.LabelCap} for {actionDef.damageAmount:0.##} {damageDef.defName} damage.");
        }

        if (!string.IsNullOrWhiteSpace(actionDef.soundDef))
        {
            SoundDef soundDef = DefDatabase<SoundDef>.GetNamedSilentFail(actionDef.soundDef);
            if (soundDef != null)
            {
                SoundStarter.PlayOneShot(soundDef, new TargetInfo(targetThing));
            }
        }
    }

    private static List<Thing> FindNextTargets(Map map, Thing caster, IntVec3 sourceCell, Thing currentThing, ChainLightningActionDef actionDef)
    {
        List<Thing> candidates = new();
        Vector2 forward = ResolveForward(caster, sourceCell, currentThing);
        foreach (Thing thing in map.listerThings?.AllThings ?? new List<Thing>())
        {
            if (thing == null || thing.Destroyed || thing == currentThing)
            {
                continue;
            }

            if (!TargetQueryUtility.MatchesThingFilter(
                    BuildFilterContext(map, caster),
                    thing,
                    includePawns: true,
                    includeBuildings: false,
                    includeItems: false,
                    actionDef.includeCaster,
                    actionDef.pawnAffinity))
            {
                continue;
            }

            float distance = thing.Position.DistanceTo(currentThing.Position);
            if (distance > actionDef.jumpRadius)
            {
                continue;
            }

            float forwardScore = TargetQueryUtility.ForwardScore(currentThing.Position, thing.Position, forward);
            if (forwardScore < actionDef.minForwardScore)
            {
                continue;
            }

            candidates.Add(thing);
        }

        if (candidates.Count == 0)
        {
            return candidates;
        }

        Shuffle(candidates);
        candidates.Sort((left, right) =>
        {
            float leftScore = TargetQueryUtility.ForwardScore(currentThing.Position, left.Position, forward);
            float rightScore = TargetQueryUtility.ForwardScore(currentThing.Position, right.Position, forward);
            int scoreCompare = rightScore.CompareTo(leftScore);
            return scoreCompare != 0 ? scoreCompare : left.Position.DistanceTo(currentThing.Position).CompareTo(right.Position.DistanceTo(currentThing.Position));
        });

        int minBranches = Mathf.Max(1, actionDef.minBranches);
        int maxBranches = Mathf.Max(minBranches, actionDef.maxBranches);
        int desiredCount = Rand.RangeInclusive(minBranches, maxBranches);
        int takeCount = Mathf.Min(desiredCount, candidates.Count);
        if (!actionDef.allowRepeatTargets)
        {
            takeCount = Mathf.Min(takeCount, candidates.Count);
        }

        List<Thing> results = new();
        for (int i = 0; i < takeCount; i++)
        {
            results.Add(candidates[i]);
        }

        return results;
    }

    private static SpellContext BuildFilterContext(Map map, Thing caster)
    {
        return new SpellContext
        {
            caster = caster,
            map = map
        };
    }

    private static Vector2 ResolveForward(Thing caster, IntVec3 sourceCell, Thing currentThing)
    {
        IntVec3 resolvedSourceCell = sourceCell.IsValid ? sourceCell : caster?.Position ?? currentThing.Position;
        Vector2 forward = TargetQueryUtility.ToVector2(currentThing.Position) - TargetQueryUtility.ToVector2(resolvedSourceCell);
        if (forward.sqrMagnitude < 0.001f && caster != null)
        {
            forward = TargetQueryUtility.ToVector2(currentThing.Position) - TargetQueryUtility.ToVector2(caster.Position);
        }

        return forward;
    }

    private static void DrawLightning(Map map, Thing sourceThing, IntVec3 sourceCell, Thing targetThing, ChainLightningActionDef actionDef)
    {
        if (map == null || targetThing == null)
        {
            return;
        }

        Vector3 source = sourceThing != null && !sourceThing.Destroyed
            ? sourceThing.DrawPos
            : sourceCell.IsValid ? sourceCell.ToVector3Shifted() : targetThing.DrawPos;
        Vector3 target = targetThing.DrawPos;
        int steps = Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(source, target) * 2.2f), 3, 18);
        Vector3 previousPoint = source;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 point = Vector3.Lerp(source, target, t);
            point.x += Rand.Range(-0.20f, 0.20f);
            point.z += Rand.Range(-0.20f, 0.20f);
            SpawnFleck(map, point, actionDef.lineFleckDef, Rand.Range(0.75f, 1.15f));
            if (i % 3 == 0)
            {
                SpawnFleck(map, Vector3.Lerp(previousPoint, point, 0.5f), "MicroSparksFast", 0.7f);
            }

            previousPoint = point;
        }

        SpawnFleck(map, target, actionDef.impactFleckDef, 1.4f);
        SpawnFleck(map, target, "SparkFlash", 1.1f);
    }

    private static void SpawnFleck(Map map, Vector3 position, string fleckDefName, float scale)
    {
        if (map == null || string.IsNullOrWhiteSpace(fleckDefName))
        {
            return;
        }

        FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(fleckDefName);
        if (fleckDef != null)
        {
            FleckMaker.Static(position, map, fleckDef, scale);
        }
    }

    private static DamageDef ResolveDamageDef(string damageDefName)
    {
        if (!string.IsNullOrWhiteSpace(damageDefName))
        {
            DamageDef damageDef = DefDatabase<DamageDef>.GetNamedSilentFail(damageDefName);
            if (damageDef != null)
            {
                return damageDef;
            }
        }

        return DamageDefOf.Burn;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Rand.RangeInclusive(0, i);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
