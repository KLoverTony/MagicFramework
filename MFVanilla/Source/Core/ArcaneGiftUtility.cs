using MagicFramework.Core;
using RimWorld;
using Verse;

namespace MFVanilla.Core;

public static class ArcaneGiftUtility
{
    public const string TraitDefName = "MFV_ArcaneGift";
    public const string BasicBenchDefName = "MFV_ArcaneResearchBench";
    public const string AdvancedBenchDefName = "MFV_AdvancedArcaneResearchBench";
    public const float ArcanePracticeExposureThreshold = 800f;
    public const float ArcanePracticeGiftChance = 0.01f;

    private static TraitDef _arcaneGiftTrait;

    public static TraitDef ArcaneGiftTrait => _arcaneGiftTrait ??= DefDatabase<TraitDef>.GetNamedSilentFail(TraitDefName);

    public static bool HasArcaneGiftTrait(Pawn pawn)
    {
        TraitDef traitDef = ArcaneGiftTrait;
        return pawn?.story?.traits != null
            && traitDef != null
            && pawn.story.traits.HasTrait(traitDef);
    }

    public static bool TryGiveArcaneGiftTrait(Pawn pawn, bool sendLetter)
    {
        if (!TryGiveArcaneGiftTraitOnly(pawn))
        {
            return false;
        }

        SpellRuntimeGameComponent.Instance?.SetArcaneGift(pawn, true);

        if (sendLetter && pawn.Faction == Faction.OfPlayer)
        {
            Find.LetterStack.ReceiveLetter(
                "Arcane gift awakened",
                $"{pawn.LabelShortCap}'s repeated work with arcane practice has awakened an Arcane Gift.",
                LetterDefOf.PositiveEvent,
                pawn);
        }

        return true;
    }

    public static bool TryGiveArcaneGiftTraitOnly(Pawn pawn)
    {
        TraitDef traitDef = ArcaneGiftTrait;
        if (pawn?.story?.traits == null || traitDef == null || pawn.story.traits.HasTrait(traitDef))
        {
            return false;
        }

        pawn.story.traits.GainTrait(new Trait(traitDef, 0, true), true);
        return true;
    }

    public static bool TryRemoveArcaneGiftTrait(Pawn pawn)
    {
        TraitDef traitDef = ArcaneGiftTrait;
        if (pawn?.story?.traits == null || traitDef == null)
        {
            return false;
        }

        Trait trait = pawn?.story?.traits?.GetTrait(traitDef);
        if (trait == null)
        {
            return false;
        }

        pawn.story.traits.RemoveTrait(trait, true);
        return true;
    }

    public static bool IsArcaneResearchBench(Thing thing)
    {
        string defName = thing?.def?.defName;
        return defName == BasicBenchDefName || defName == AdvancedBenchDefName;
    }

}
