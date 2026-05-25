using System.Collections.Generic;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using Verse;

namespace MFVanilla.Core;

public sealed class CompProperties_UseEffectLearnSpell : CompProperties_UseEffect
{
    public SpellDef spell;
    public List<ResearchProjectDef> requiredResearch;
    public bool requireHumanlike = true;

    public CompProperties_UseEffectLearnSpell()
    {
        compClass = typeof(CompUseEffect_LearnSpell);
    }
}

public sealed class CompUseEffect_LearnSpell : CompUseEffect
{
    private CompProperties_UseEffectLearnSpell Props => (CompProperties_UseEffectLearnSpell)props;

    public override AcceptanceReport CanBeUsedBy(Pawn p)
    {
        AcceptanceReport baseReport = base.CanBeUsedBy(p);
        if (!baseReport.Accepted)
        {
            return baseReport;
        }

        return ValidateUse(p);
    }

    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);

        AcceptanceReport report = ValidateUse(usedBy);
        if (!report.Accepted)
        {
            Messages.Message(report.Reason, parent, MessageTypeDefOf.RejectInput, false);
            return;
        }

        SpellDef spellDef = Props.spell;
        if (SpellRuntimeGameComponent.Instance.LearnSpell(usedBy, spellDef))
        {
            ArcanePracticeUtility.NotifySpellLearnedFromScroll(usedBy, spellDef);
            Messages.Message("MFV_PawnLearnedSpellFromScroll".Translate(usedBy.LabelShortCap, spellDef.LabelCap), usedBy, MessageTypeDefOf.TaskCompletion, false);
        }
    }

    public override string CompInspectStringExtra()
    {
        string baseText = base.CompInspectStringExtra();
        string details = SpellDescriptionUtility.HasDescriptionTokens(Props.spell)
            ? SpellDescriptionUtility.GetResolvedDescription(Props.spell)
            : SpellDescriptionUtility.GetDetails(Props.spell);
        if (string.IsNullOrWhiteSpace(details))
        {
            return baseText;
        }

        return string.IsNullOrWhiteSpace(baseText) ? details : baseText + "\n" + details;
    }

    private AcceptanceReport ValidateUse(Pawn pawn)
    {
        if (pawn == null)
        {
            return "MFV_NoPawnSelected".Translate();
        }

        if (pawn.Dead || pawn.Destroyed)
        {
            return "MFV_PawnCannotUseThis".Translate(pawn.LabelShortCap);
        }

        if (Props.requireHumanlike && !pawn.RaceProps.Humanlike)
        {
            return "MFV_PawnCannotLearnSpells".Translate(pawn.LabelShortCap);
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime == null)
        {
            return "MFV_MagicFrameworkRuntimeUnavailable".Translate();
        }

        if (!runtime.HasArcaneGift(pawn))
        {
            return "MFV_PawnDoesNotHaveArcaneGift".Translate(pawn.LabelShortCap);
        }

        SpellDef spellDef = Props.spell;
        if (spellDef == null)
        {
            return "MFV_ScrollNotLinkedToSpell".Translate();
        }

        ResearchProjectDef missingResearch = FirstMissingResearch();
        if (missingResearch != null)
        {
            return "MFV_RequiresCompletedResearch".Translate(missingResearch.LabelCap);
        }

        if (!SpellRequirementUtility.CanLearnSpell(pawn, spellDef, out string reason))
        {
            return reason;
        }

        if (MFVanillaMod.Settings?.IgnoreArcaneDisciplineRestrictions != true
            && !ArcaneDisciplineUtility.CanPawnDisciplineLearnSpell(pawn, spellDef, out reason))
        {
            return reason;
        }

        return true;
    }

    private ResearchProjectDef FirstMissingResearch()
    {
        if (Props.requiredResearch == null)
        {
            return null;
        }

        for (int i = 0; i < Props.requiredResearch.Count; i++)
        {
            ResearchProjectDef project = Props.requiredResearch[i];
            if (project != null && !project.IsFinished)
            {
                return project;
            }
        }

        return null;
    }
}
