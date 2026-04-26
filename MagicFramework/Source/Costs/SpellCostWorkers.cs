using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using Verse;

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
        runtime?.SpendMana(context?.caster, manaCostDef.amount);
        Log.Message($"[MagicFramework] Spent {manaCostDef.amount:0.##} mana for {context?.spellDef?.defName ?? "<unknown spell>"}.");
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
        runtime?.StartCooldown(context?.caster, context?.spellDef, cooldownCostDef.cooldownTicks);
        Log.Message($"[MagicFramework] Started cooldown of {cooldownCostDef.cooldownTicks} ticks for {context?.spellDef?.defName ?? "<unknown spell>"}.");
    }
}
