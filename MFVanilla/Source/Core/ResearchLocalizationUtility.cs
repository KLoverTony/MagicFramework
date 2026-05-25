using Verse;

namespace MFVanilla.Core;

[StaticConstructorOnStartup]
public static class ResearchLocalizationUtility
{
    static ResearchLocalizationUtility()
    {
        ApplyTranslations();
    }

    public static void ApplyTranslations()
    {
        foreach (ResearchProjectDef project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
        {
            if (project?.customUnlockTexts == null)
            {
                continue;
            }

            for (int i = 0; i < project.customUnlockTexts.Count; i++)
            {
                string key = project.defName + ".customUnlockTexts." + i;
                string translated = key.Translate().ToString();
                if (translated != key)
                {
                    project.customUnlockTexts[i] = translated;
                }
            }
        }
    }
}
