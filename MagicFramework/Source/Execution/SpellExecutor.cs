using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Scheduling;
using MagicFramework.Visuals;
using UnityEngine;
using RimWorld;
using Verse;

namespace MagicFramework.Execution;

/// <summary>
/// High-level orchestration entry point for casting spells.
/// </summary>
public sealed class SpellExecutor
{
    private const float CastSpellBaseExperience = 5f;
    private const float CastSpellExperiencePerTier = 3f;

    private readonly SpellCastValidator validator;
    private readonly SpellCostProcessor costProcessor;
    private readonly SpellActionRunner actionRunner;
    private readonly SpellScheduler scheduler;

    public SpellExecutor(
        SpellCastValidator validator = null,
        SpellCostProcessor costProcessor = null,
        SpellActionRunner actionRunner = null,
        SpellScheduler scheduler = null)
    {
        this.validator = validator ?? new SpellCastValidator();
        this.costProcessor = costProcessor ?? new SpellCostProcessor();
        this.actionRunner = actionRunner ?? new SpellActionRunner();
        this.scheduler = scheduler ?? new SpellScheduler();
    }

    public SpellContext BuildContext(SpellDef spellDef, Thing caster, LocalTargetInfo initialTarget)
    {
        SpellContext context = new()
        {
            caster = caster,
            map = caster?.Map,
            spellDef = spellDef,
            initialTarget = initialTarget,
            power = SpellPowerUtility.ComputePower(spellDef, caster),
            randomSeed = Find.TickManager?.TicksGame ?? 0
        };

        context.SetCurrentTarget(initialTarget);
        context.currentTargets.Add(initialTarget);
        return context;
    }

    public bool TryExecute(SpellDef spellDef, Thing caster, LocalTargetInfo initialTarget, out SpellContext context, bool playCastStartFx = true)
    {
        return TryExecute(spellDef, caster, initialTarget, out context, playCastStartFx, null);
    }

    public bool TryExecute(
        SpellDef spellDef,
        Thing caster,
        LocalTargetInfo initialTarget,
        out SpellContext context,
        bool playCastStartFx,
        System.Action<SpellContext> configureContext)
    {
        context = BuildContext(spellDef, caster, initialTarget);
        configureContext?.Invoke(context);

        if (!validator.TryValidate(context))
        {
            return false;
        }

        costProcessor.ApplyCosts(context);
        if (playCastStartFx)
        {
            MagicFXSpawner.Play(context, MagicFXEvent.CastStart, SpellEffectLocationSource.Caster);
        }

        actionRunner.RunRootActions(context);
        scheduler.FlushDebugSchedule(context);
        bool completed = !context.executionState.failed && !context.executionState.cancelled;
        if (completed)
        {
            AwardCastingExperience(context);
        }

        return completed;
    }

    private static void AwardCastingExperience(SpellContext context)
    {
        if (context?.caster is not Pawn pawn || context.spellDef == null)
        {
            return;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime == null
            || !runtime.HasArcaneGift(pawn)
            || !runtime.KnowsSpell(pawn, context.spellDef)
            || context.sourceItem != null)
        {
            return;
        }

        float xp = CastSpellBaseExperience + (CastSpellExperiencePerTier * SpellTier(context.spellDef));
        runtime.GainCasterExperience(pawn, xp);
    }

    private static int SpellTier(SpellDef spellDef)
    {
        return Mathf.Max(1, spellDef?.meta?.tier ?? 1);
    }
}
