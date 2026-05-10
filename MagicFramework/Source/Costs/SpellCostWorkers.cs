using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;

namespace MagicFramework.Costs;

public sealed class ManaCostWorker : SpellCostWorker
{
    public override void ApplyCost(SpellContext context, SpellCostDef costDef)
    {
        ManaCostDef manaCostDef = costDef as ManaCostDef;
        if (manaCostDef == null)
        {
            return;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        float resolvedAmount = SpellEnhancementUtility.ResolveManaCost(context, manaCostDef.amount);
        runtime?.SpendMana(context?.caster, resolvedAmount);
        MagicLog.Message(MagicLogSubsystem.Costs, $"[MagicFramework] Spent {resolvedAmount:0.##} mana for {context?.spellDef?.defName ?? "<unknown spell>"}.");
    }
}

public sealed class CooldownCostWorker : SpellCostWorker
{
    public override void ApplyCost(SpellContext context, SpellCostDef costDef)
    {
        CooldownCostDef cooldownCostDef = costDef as CooldownCostDef;
        if (cooldownCostDef == null)
        {
            return;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        int resolvedTicks = SpellEnhancementUtility.ResolveCooldownTicks(context, cooldownCostDef.cooldownTicks);
        runtime?.StartCooldown(context?.caster, context?.spellDef, resolvedTicks);
        MagicLog.Message(MagicLogSubsystem.Costs, $"[MagicFramework] Started cooldown of {resolvedTicks} ticks for {context?.spellDef?.defName ?? "<unknown spell>"}.");
    }
}
