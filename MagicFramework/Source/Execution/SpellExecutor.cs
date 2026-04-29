using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Scheduling;
using MagicFramework.Visuals;
using Verse;

namespace MagicFramework.Execution;

/// <summary>
/// High-level orchestration entry point for casting spells.
/// </summary>
public sealed class SpellExecutor
{
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

    public bool TryExecute(SpellDef spellDef, Thing caster, LocalTargetInfo initialTarget, out SpellContext context)
    {
        context = BuildContext(spellDef, caster, initialTarget);

        if (!validator.TryValidate(context))
        {
            return false;
        }

        costProcessor.ApplyCosts(context);
        MagicFXSpawner.Play(context, MagicFXEvent.CastStart, SpellEffectLocationSource.Caster);
        actionRunner.RunRootActions(context);
        scheduler.FlushDebugSchedule(context);
        return !context.executionState.failed && !context.executionState.cancelled;
    }
}
