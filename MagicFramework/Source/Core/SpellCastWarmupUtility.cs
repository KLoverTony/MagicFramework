using System;
using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Visuals;
using RimWorld;
using Verse;

namespace MagicFramework.Core;

public static class SpellCastWarmupUtility
{
    public static void StartOrExecute(
        Pawn pawn,
        SpellDef spellDef,
        LocalTargetInfo target,
        SpellExecutor executor,
        Action<bool, SpellContext> onComplete)
    {
        if (pawn == null || spellDef == null || executor == null)
        {
            onComplete?.Invoke(false, null);
            return;
        }

        int warmupTicks = Math.Max(0, spellDef.castTimeTicks);
        if (warmupTicks <= 0)
        {
            bool completed = executor.TryExecute(spellDef, pawn, target, out SpellContext immediateContext);
            onComplete?.Invoke(completed, immediateContext);
            return;
        }

        SpellContext validationContext = executor.BuildContext(spellDef, pawn, target);
        if (!new SpellCastValidator().TryValidate(validationContext))
        {
            onComplete?.Invoke(false, validationContext);
            return;
        }

        pawn.stances?.SetStance(new SpellWarmupStance(warmupTicks, target));
        MagicFXSpawner.Play(validationContext, MagicFXEvent.CastStart, SpellEffectLocationSource.Caster);
        SpellWarmupGameComponent.Instance?.Enqueue(new PendingSpellWarmup(
            pawn,
            spellDef,
            target,
            Find.TickManager.TicksGame + warmupTicks,
            executor,
            onComplete));
    }
}

public sealed class SpellWarmupGameComponent : GameComponent
{
    private List<PendingSpellWarmup> pendingWarmups = new();

    public SpellWarmupGameComponent(Game game)
    {
    }

    public static SpellWarmupGameComponent Instance => Current.Game?.GetComponent<SpellWarmupGameComponent>();

    public void Enqueue(PendingSpellWarmup pendingWarmup)
    {
        pendingWarmups ??= new List<PendingSpellWarmup>();
        pendingWarmups.Add(pendingWarmup);
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        if (pendingWarmups == null || pendingWarmups.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager.TicksGame;
        for (int i = pendingWarmups.Count - 1; i >= 0; i--)
        {
            PendingSpellWarmup pendingWarmup = pendingWarmups[i];
            if (pendingWarmup.ExecuteAtTick > currentTick)
            {
                continue;
            }

            pendingWarmups.RemoveAt(i);
            pendingWarmup.Execute();
        }
    }
}

public sealed class SpellWarmupStance : Stance_Busy
{
    public SpellWarmupStance()
    {
    }

    public SpellWarmupStance(int ticks, LocalTargetInfo focusTarget) : base(ticks, focusTarget, null)
    {
    }
}

public sealed class PendingSpellWarmup
{
    private readonly Pawn caster;
    private readonly SpellDef spellDef;
    private readonly LocalTargetInfo target;
    private readonly SpellExecutor executor;
    private readonly Action<bool, SpellContext> onComplete;

    public PendingSpellWarmup(
        Pawn caster,
        SpellDef spellDef,
        LocalTargetInfo target,
        int executeAtTick,
        SpellExecutor executor,
        Action<bool, SpellContext> onComplete)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.target = target;
        this.executor = executor;
        this.onComplete = onComplete;
        ExecuteAtTick = executeAtTick;
    }

    public int ExecuteAtTick { get; }

    public void Execute()
    {
        if (caster == null || caster.Destroyed || caster.Dead || caster.Downed || !caster.Spawned)
        {
            SpellContext failedContext = executor?.BuildContext(spellDef, caster, target);
            if (failedContext != null)
            {
                failedContext.executionState.failed = true;
                failedContext.executionState.failureReason = "Caster could not complete the spell.";
            }

            onComplete?.Invoke(false, failedContext);
            return;
        }

        bool completed = executor.TryExecute(spellDef, caster, target, out SpellContext context, false);
        onComplete?.Invoke(completed, context);
    }
}
