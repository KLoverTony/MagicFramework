using System.Linq;
using System.Reflection;
using MagicFramework.Definitions;
using Verse;

namespace MFVanilla.Core;

[StaticConstructorOnStartup]
public static class SpellScrollLocalizationUtility
{
    static SpellScrollLocalizationUtility()
    {
        ApplyTranslations();
    }

    public static void ApplyTranslations()
    {
        foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
        {
            CompProperties_UseEffectLearnSpell learnComp = thingDef?.comps?
                .OfType<CompProperties_UseEffectLearnSpell>()
                .FirstOrDefault();
            SpellDef spell = learnComp?.spell;
            if (spell == null)
            {
                continue;
            }

            string spellLabel = spell.LabelCap.ToString();
            thingDef.label = "MFV_SpellScrollLabel".Translate(spellLabel).ToString();
            thingDef.description = "MFV_SpellScrollDescription".Translate(spellLabel).ToString();

            CompProperties usableComp = thingDef.comps
                .FirstOrDefault(comp => comp?.GetType().Name == "CompProperties_Usable");
            FieldInfo useLabelField = usableComp?.GetType().GetField("useLabel");
            if (useLabelField != null)
            {
                useLabelField.SetValue(usableComp, "MFV_ReadSpellScroll".Translate().ToString());
            }
        }

        foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefsListForReading)
        {
            SpellDef spell = recipeDef?.GetModExtension<ScribeSpellScrollRecipeExtension>()?.spell;
            if (spell == null)
            {
                continue;
            }

            string spellLabel = spell.LabelCap.ToString();
            recipeDef.label = "MFV_ScribeSpellScrollLabel".Translate(spellLabel).ToString();
            recipeDef.description = "MFV_ScribeSpellScrollDescription".Translate(spellLabel).ToString();
            recipeDef.jobString = "MFV_ScribeSpellScrollJobString".Translate(spellLabel).ToString();
        }
    }
}
