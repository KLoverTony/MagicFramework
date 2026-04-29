using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Requirements;

public sealed class ManaRequirementWorker : SpellRequirementWorker
{
    public override bool CanCast(SpellContext context, SpellRequirementDef requirementDef, out string reason)
    {
        ManaRequirementDef manaRequirementDef = requirementDef as ManaRequirementDef;
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        float requiredAmount = manaRequirementDef?.amount ?? 0f;

        if (runtime == null)
        {
            reason = "Spell runtime component was unavailable.";
            return false;
        }

        if (runtime.HasEnoughMana(context?.caster, requiredAmount))
        {
            reason = null;
            return true;
        }

        float currentMana = runtime.GetCurrentMana(context?.caster);
        reason = $"Not enough mana. Required {requiredAmount:0.##}, current {currentMana:0.##}.";
        return false;
    }
}

public sealed class CooldownRequirementWorker : SpellRequirementWorker
{
    public override bool CanCast(SpellContext context, SpellRequirementDef requirementDef, out string reason)
    {
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime == null)
        {
            reason = "Spell runtime component was unavailable.";
            return false;
        }

        int remainingTicks = runtime.GetCooldownRemainingTicks(context?.caster, context?.spellDef);
        if (remainingTicks <= 0)
        {
            reason = null;
            return true;
        }

        reason = $"Spell is on cooldown for {remainingTicks} more ticks.";
        return false;
    }
}

public sealed class ArcaneGiftRequirementWorker : SpellRequirementWorker
{
    public override bool CanCast(SpellContext context, SpellRequirementDef requirementDef, out string reason)
    {
        if (context?.caster is not Pawn pawn)
        {
            reason = "Only pawns can use arcane gift requirements.";
            return false;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime == null)
        {
            reason = "Spell runtime component was unavailable.";
            return false;
        }

        if (runtime.HasArcaneGift(pawn))
        {
            reason = null;
            return true;
        }

        reason = $"{pawn.LabelShortCap} does not have the Arcane gift.";
        return false;
    }
}

public sealed class CasterLevelRequirementWorker : SpellRequirementWorker
{
    public override bool CanCast(SpellContext context, SpellRequirementDef requirementDef, out string reason)
    {
        CasterLevelRequirementDef casterLevelRequirementDef = requirementDef as CasterLevelRequirementDef;
        int minimumLevel = casterLevelRequirementDef?.minimumLevel ?? 1;

        if (context?.caster is not Pawn pawn)
        {
            reason = "Only pawns can use caster level requirements.";
            return false;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime == null)
        {
            reason = "Spell runtime component was unavailable.";
            return false;
        }

        int casterLevel = runtime.GetCasterLevel(pawn);
        if (casterLevel >= minimumLevel)
        {
            reason = null;
            return true;
        }

        reason = $"{pawn.LabelShortCap} needs caster level {minimumLevel}; current level is {casterLevel}.";
        return false;
    }
}
