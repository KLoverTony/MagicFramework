using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

public sealed class Dialog_SpellDetails : Window
{
    private readonly Pawn caster;
    private readonly SpellDef spellDef;
    private Vector2 scrollPosition;
    private float viewHeight;

    public override Vector2 InitialSize => new(760f, 680f);

    public Dialog_SpellDetails(Pawn caster, SpellDef spellDef)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        doCloseX = true;
        doCloseButton = true;
        closeOnClickedOutside = true;
        absorbInputAroundWindow = false;
    }

    public override void DoWindowContents(Rect inRect)
    {
        if (spellDef == null)
        {
            Widgets.Label(inRect, "No spell selected.");
            return;
        }

        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), spellDef.LabelCap);
        Text.Font = GameFont.Small;

        Rect scrollRect = new(0f, 40f, inRect.width, inRect.height - 80f);
        Rect viewRect = new(0f, 0f, scrollRect.width - 16f, viewHeight);
        Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);

        float curY = 0f;
        AddTextBlock(viewRect.width, ref curY, SpellDescriptionUtility.GetResolvedDescription(spellDef, caster));
        if (!SpellDescriptionUtility.HasDescriptionTokens(spellDef))
        {
            AddSection(viewRect.width, ref curY, "Classification", BuildClassificationText());
            AddSection(viewRect.width, ref curY, "Learning", BuildLearningText());
            AddSection(viewRect.width, ref curY, "Casting", BuildCastingText());
            AddSection(viewRect.width, ref curY, "Targeting", BuildTargetingText());
            AddSection(viewRect.width, ref curY, "Active Modifiers", BuildActiveModifiersText());
            AddSection(viewRect.width, ref curY, "Effects", StripEffectsHeader(SpellDescriptionUtility.GetDetails(spellDef)));
        }

        viewHeight = curY + 8f;
        Widgets.EndScrollView();
    }

    private void AddSection(float width, ref float curY, string title, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        curY += 10f;
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, curY, width, 28f), title);
        curY += 30f;
        Text.Font = GameFont.Small;
        AddTextBlock(width, ref curY, body);
    }

    private static void AddTextBlock(float width, ref float curY, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        float height = Text.CalcHeight(text, width);
        Widgets.Label(new Rect(0f, curY, width, height), text);
        curY += height + 4f;
    }

    private string BuildClassificationText()
    {
        List<string> lines = new();
        lines.Add("Tier: " + (spellDef.meta?.tier ?? 1));
        AddDefListLine(lines, "Elements", spellDef.meta?.elements);
        AddDefListLine(lines, "Domains", spellDef.meta?.domains);
        AddDefListLine(lines, "Disciplines", spellDef.meta?.disciplines);
        AddDefListLine(lines, "Tags", spellDef.meta?.tags);

        if (!string.IsNullOrWhiteSpace(spellDef.element))
        {
            lines.Add("Legacy element: " + spellDef.element);
        }

        return string.Join("\n", lines);
    }

    private string BuildLearningText()
    {
        SpellLearningProperties learning = spellDef.learning;
        if (learning == null)
        {
            return "No authored learning data.";
        }

        List<string> lines = new()
        {
            learning.canBeLearned ? "Can be learned." : "Cannot be learned through normal learning."
        };

        AddResearchLines(lines, learning.researchPrerequisites);
        AddRequirementLines(lines, "Learning requirements", learning.requirements);
        return string.Join("\n", lines);
    }

    private string BuildCastingText()
    {
        List<string> lines = new();
        SpellContext context = SpellRequirementUtility.CreatePawnContext(caster, spellDef);
        if (caster != null)
        {
            int casterLevel = SpellRuntimeGameComponent.Instance?.GetCasterLevel(caster) ?? 0;
            lines.Add("Caster: " + caster.LabelShortCap + " (level " + casterLevel + ")");
        }

        AddCostLines(lines, spellDef.casting?.costs ?? spellDef.costs, context);
        AddRequirementLines(lines, "Casting requirements", spellDef.casting?.requirements ?? spellDef.requirements);

        if (spellDef.castTimeTicks > 0)
        {
            lines.Add("Cast time: " + spellDef.castTimeTicks.ToStringTicksToPeriod());
        }

        if (SpellRequirementUtility.CanCastSpell(context, spellDef, out string reason, true))
        {
            lines.Add("Current status: ready.");
        }
        else
        {
            lines.Add("Current status: " + reason);
        }

        return string.Join("\n", lines);
    }

    private string BuildTargetingText()
    {
        SpellTargetingDef targeting = spellDef.targeting;
        if (targeting == null)
        {
            return "No authored targeting data.";
        }

        List<string> lines = new()
        {
            "Target type: " + targeting.primaryTargetType,
            "Shape: " + targeting.shape,
            "Affinity: " + targeting.pawnAffinity
        };

        SpellContext context = SpellRequirementUtility.CreatePawnContext(caster, spellDef);
        if (targeting.range > 0f)
        {
            lines.Add("Range: " + FormatNumber(SpellEnhancementUtility.ResolveScalableRadius(context, targeting.range, targeting.scalableRange)));
        }

        if (targeting.radius > 0f)
        {
            lines.Add("Radius: " + FormatNumber(targeting.radius));
        }

        if (targeting.lineLength > 0f)
        {
            lines.Add("Line length: " + FormatNumber(targeting.lineLength));
        }

        if (targeting.wallLength > 0)
        {
            lines.Add("Wall length: " + targeting.wallLength);
        }

        AddFlag(lines, targeting.useCasterAsTarget, "Uses the caster as the target.");
        AddFlag(lines, targeting.requireLineOfSight, "Requires line of sight.");
        AddFlag(lines, targeting.requireStandableCell, "Requires a standable cell.");
        AddFlag(lines, targeting.requireWalkableCell, "Requires a walkable cell.");
        AddFlag(lines, targeting.requireWaterCell, "Requires water-like terrain.");
        AddFlag(lines, targeting.requireResurrectableCorpse, "Requires a resurrectable corpse.");
        return string.Join("\n", lines);
    }

    private string BuildActiveModifiersText()
    {
        List<string> lines = new();
        foreach (SpellEnhancementRuleDef rule in SpellEnhancementUtility.GetActiveRules(spellDef, caster?.Map))
        {
            if (rule != null)
            {
                lines.Add("- " + rule.LabelCap);
            }
        }

        return lines.Count == 0 ? "No active enhancement rules." : string.Join("\n", lines);
    }

    private static void AddCostLines(List<string> lines, List<SpellCostDef> costs, SpellContext context)
    {
        if (costs == null || costs.Count == 0)
        {
            lines.Add("Costs: none authored.");
            return;
        }

        for (int i = 0; i < costs.Count; i++)
        {
            switch (costs[i])
            {
                case ManaCostDef mana:
                    float amount = SpellEnhancementUtility.ResolveManaCost(context, mana.amount);
                    lines.Add("Mana cost: " + FormatNumber(amount));
                    break;
                case CooldownCostDef cooldown:
                    int ticks = SpellEnhancementUtility.ResolveCooldownTicks(context, cooldown.cooldownTicks);
                    lines.Add("Cooldown: " + ticks.ToStringTicksToPeriod());
                    break;
            }
        }
    }

    private static void AddResearchLines(List<string> lines, List<ResearchProjectDef> researchDefs)
    {
        if (researchDefs == null || researchDefs.Count == 0)
        {
            lines.Add("Research prerequisites: none.");
            return;
        }

        for (int i = 0; i < researchDefs.Count; i++)
        {
            ResearchProjectDef research = researchDefs[i];
            if (research == null)
            {
                continue;
            }

            bool finished = research.IsFinished;
            lines.Add("Research: " + research.LabelCap + (finished ? " (met)" : " (unmet)"));
        }
    }

    private static void AddRequirementLines(List<string> lines, string label, List<SpellRequirementDef> requirements)
    {
        if (requirements == null || requirements.Count == 0)
        {
            lines.Add(label + ": none authored.");
            return;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            string suffix = DescribeRequirement(requirements[i]);
            if (!string.IsNullOrWhiteSpace(suffix))
            {
                lines.Add(label + ": " + suffix);
            }
        }
    }

    private static string DescribeRequirement(SpellRequirementDef requirement)
    {
        return requirement switch
        {
            ManaRequirementDef mana => "at least " + FormatNumber(mana.amount) + " mana",
            CooldownRequirementDef => "spell cooldown ready",
            ArcaneGiftRequirementDef => "Arcane gift",
            CasterLevelRequirementDef casterLevel => "caster level " + casterLevel.minimumLevel + "+",
            null => null,
            _ => requirement.GetType().Name
        };
    }

    private static void AddDefListLine<TDef>(List<string> lines, string label, List<TDef> defs)
        where TDef : Def
    {
        if (defs == null || defs.Count == 0)
        {
            lines.Add(label + ": none.");
            return;
        }

        List<string> labels = new();
        for (int i = 0; i < defs.Count; i++)
        {
            if (defs[i] != null)
            {
                labels.Add(defs[i].LabelCap);
            }
        }

        lines.Add(label + ": " + (labels.Count == 0 ? "none." : string.Join(", ", labels)));
    }

    private static void AddFlag(List<string> lines, bool active, string label)
    {
        if (active)
        {
            lines.Add(label);
        }
    }

    private static string StripEffectsHeader(string details)
    {
        if (string.IsNullOrWhiteSpace(details))
        {
            return "No generated effect summary available.";
        }

        const string header = "Effects:\n";
        return details.StartsWith(header) ? details.Substring(header.Length) : details;
    }

    private static string FormatNumber(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
