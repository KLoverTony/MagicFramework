using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MagicFramework.Definitions;
using MagicFramework.Debug;
using MagicFramework.Scheduling;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Applies runtime patches used by debug integration.
/// </summary>
[StaticConstructorOnStartup]
public static class MagicFrameworkHarmony
{
    static MagicFrameworkHarmony()
    {
        Harmony harmony = new("oracle.magicframework");
        PatchStatValueHook(harmony);
        PatchDamageHook(harmony);
        PatchPawnDrawHook(harmony);
        PatchProjectileImpactHook(harmony);
        harmony.PatchAll();
        Log.Message("[MagicFramework] Harmony patches applied.");
    }

    private static void PatchStatValueHook(Harmony harmony)
    {
        var postfix = new HarmonyMethod(typeof(MagicFrameworkHarmony).GetMethod(nameof(ApplySpellStatModifiersPostfix)));
        var statExtensionType = AccessTools.TypeByName("RimWorld.StatExtension") ?? AccessTools.TypeByName("Verse.StatExtension");
        var getStatValueMethod = statExtensionType == null
            ? null
            : AccessTools.Method(statExtensionType, "GetStatValue", new[] { typeof(Thing), typeof(StatDef), typeof(bool), typeof(int) });

        if (getStatValueMethod != null)
        {
            harmony.Patch(getStatValueMethod, postfix: postfix);
            return;
        }

        Log.Warning("[MagicFramework] Could not locate StatExtension.GetStatValue for spell stat modifier patching.");
    }

    public static void ApplySpellStatModifiersPostfix(Thing thing, StatDef stat, ref float __result)
    {
        SpellRuntimeGameComponent.Instance?.ApplyStatAdjustments(thing, stat, ref __result);
    }

    private static void PatchDamageHook(Harmony harmony)
    {
        var prefix = new HarmonyMethod(typeof(MagicFrameworkHarmony).GetMethod(nameof(ApplySpellForceFieldsPrefix)));
        var preApplyDamageMethod = AccessTools.Method(typeof(Thing), nameof(Thing.PreApplyDamage));
        if (preApplyDamageMethod != null)
        {
            harmony.Patch(preApplyDamageMethod, prefix: prefix);
            return;
        }

        Log.Warning("[MagicFramework] Could not locate Thing.PreApplyDamage for spell force field patching.");
    }

    public static bool ApplySpellForceFieldsPrefix(Thing __instance, ref DamageInfo dinfo, ref bool absorbed)
    {
        absorbed = false;
        SpellRuntimeGameComponent.Instance?.ApplyForceFieldDamageReduction(__instance, ref dinfo, ref absorbed);
        if (!absorbed && __instance is Pawn pawn)
        {
            MagicItemUtility.ApplyDamageResistance(pawn, ref dinfo, ref absorbed);
        }

        return !absorbed;
    }

    private static void PatchPawnDrawHook(Harmony harmony)
    {
        var postfix = new HarmonyMethod(typeof(MagicFrameworkHarmony).GetMethod(nameof(DrawSpellForceFieldOverlayPostfix)));
        var drawAtMethod = AccessTools.Method(typeof(Pawn), "DrawAt", new[] { typeof(UnityEngine.Vector3), typeof(bool) });
        if (drawAtMethod != null)
        {
            harmony.Patch(drawAtMethod, postfix: postfix);
            return;
        }

        Log.Warning("[MagicFramework] Could not locate Pawn.DrawAt for spell force field overlay patching.");
    }

    public static void DrawSpellForceFieldOverlayPostfix(Pawn __instance, UnityEngine.Vector3 drawLoc, bool flip = false)
    {
        SpellRuntimeGameComponent.Instance?.DrawForceFieldOverlay(__instance, drawLoc);
    }

    private static void PatchProjectileImpactHook(Harmony harmony)
    {
        var postfix = new HarmonyMethod(typeof(MagicFrameworkHarmony).GetMethod(nameof(CaptureProjectileImpactPostfix)));
        PatchProjectileImpactMethod(harmony, typeof(Projectile), postfix);
        PatchProjectileImpactMethod(harmony, typeof(Projectile_Explosive), postfix);
    }

    private static void PatchProjectileImpactMethod(Harmony harmony, System.Type projectileType, HarmonyMethod postfix)
    {
        var impactMethod = AccessTools.Method(projectileType, "Impact", new[] { typeof(Thing), typeof(bool) });
        if (impactMethod != null)
        {
            harmony.Patch(impactMethod, postfix: postfix);
            return;
        }

        Log.Warning($"[MagicFramework] Could not locate {projectileType.Name}.Impact for spell projectile context patching.");
    }

    public static void CaptureProjectileImpactPostfix(Projectile __instance, Thing hitThing, bool blockedByShield = false)
    {
        Map map = __instance?.MapHeld ?? __instance?.Map;
        map?.GetComponent<ProjectileImpactMapComponent>()?.NotifyProjectileImpact(__instance, hitThing, blockedByShield);
    }
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
public static class MagicFrameworkPawnGizmoPatch
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
    {
        foreach (Gizmo gizmo in __result)
        {
            yield return gizmo;
        }

        if (Prefs.DevMode && MFVanillaDevModeSettings.ShowDevModeSpellGizmos && __instance != null && (__instance.IsColonistPlayerControlled || __instance.IsPrisonerOfColony || __instance.IsSlaveOfColony))
        {
            yield return SpellDebugCasting.CreateCasterLevelGizmo(__instance);
            yield return SpellDebugCasting.CreateArcaneGiftGizmo(__instance);
            yield return SpellDebugCasting.CreateEnhancementDiagnosticsGizmo(__instance);
            yield return SpellDebugCasting.CreateScalingBoltGizmo(__instance);
            yield return SpellDebugCasting.CreateFireboltGizmo(__instance);
            yield return SpellDebugCasting.CreateFireballGizmo(__instance);
            yield return SpellDebugCasting.CreateChainLightningGizmo(__instance);
            yield return SpellDebugCasting.CreateArcSeekerGizmo(__instance);
            yield return SpellDebugCasting.CreateDelayedBlastRuneGizmo(__instance);
            yield return SpellDebugCasting.CreateRuneTrapGizmo(__instance);
            yield return SpellDebugCasting.CreateWallOfFireGizmo(__instance);
            yield return SpellDebugCasting.CreateDisintegrateGizmo(__instance);
            yield return SpellDebugCasting.CreateFlameFieldGizmo(__instance);
            yield return SpellDebugCasting.CreateFreezeGizmo(__instance);
            yield return SpellDebugCasting.CreateEarthCallGizmo(__instance);
            yield return SpellDebugCasting.CreateWatersEmbraceGizmo(__instance);
            yield return SpellDebugCasting.CreateRepulsionWardGizmo(__instance);
            yield return SpellDebugCasting.CreateForcePushGizmo(__instance);
            yield return SpellDebugCasting.CreateForcePullGizmo(__instance);
            yield return SpellDebugCasting.CreateBlinkStepGizmo(__instance);
            yield return SpellDebugCasting.CreateRescueRecallGizmo(__instance);
            yield return SpellDebugCasting.CreateTranspositionGizmo(__instance);
            yield return SpellDebugCasting.CreateHasteGizmo(__instance);
            yield return SpellDebugCasting.CreateBlessingOfVigorGizmo(__instance);
            yield return SpellDebugCasting.CreateMightGizmo(__instance);
            yield return SpellDebugCasting.CreateForceFieldGizmo(__instance);
            yield return SpellDebugCasting.CreateManaShieldGizmo(__instance);
            yield return SpellDebugCasting.CreateHealGizmo(__instance);
            yield return SpellDebugCasting.CreateRegenerationGizmo(__instance);
            yield return SpellDebugCasting.CreateResurrectionGizmo(__instance);
            yield return SpellDebugCasting.CreateSummonDogGizmo(__instance);
            yield return SpellDebugCasting.CreateCreateFoodGizmo(__instance);
        }

        if (__instance != null && (__instance.IsColonistPlayerControlled || __instance.IsPrisonerOfColony || __instance.IsSlaveOfColony))
        {
            if (SpellRuntimeGameComponent.Instance?.HasArcaneGift(__instance) == true)
            {
                yield return new SpellManaGizmo(__instance);
            }

            foreach (Gizmo spellGizmo in SpellGizmoUtility.CreateKnownSpellGizmos(__instance))
            {
                yield return spellGizmo;
            }

            foreach (Gizmo itemAbilityGizmo in MagicItemGizmoUtility.CreateItemAbilityGizmos(__instance))
            {
                yield return itemAbilityGizmo;
            }
        }
    }
}

[HarmonyPatch(typeof(Dialog_InfoCard), "FillCard")]
public static class MagicFrameworkInfoCardImagePatch
{
    private const float MinImageHeight = 64f;
    private const float MaxImageHeight = 260f;
    private const float ImageSpacing = 12f;
    private static readonly FieldInfo ThingField = AccessTools.Field(typeof(Dialog_InfoCard), "thing");
    private static readonly FieldInfo DefField = AccessTools.Field(typeof(Dialog_InfoCard), "def");
    private static readonly FieldInfo TabField = AccessTools.Field(typeof(Dialog_InfoCard), "tab");
    private static readonly Dictionary<string, Texture2D> TextureCache = new();

    public static void Prefix(Dialog_InfoCard __instance, ref Rect cardRect, out Rect __state)
    {
        __state = Rect.zero;
        if (!TryResolveImage(__instance, out InfoCardImageExtension extension, out _))
        {
            return;
        }

        float imageHeight = ResolveImageHeight(extension, cardRect);
        if (imageHeight <= 0f)
        {
            return;
        }

        __state = new Rect(cardRect.x, cardRect.y, cardRect.width, imageHeight);
        cardRect.y += imageHeight + ImageSpacing;
        cardRect.height = Mathf.Max(0f, cardRect.height - imageHeight - ImageSpacing);
    }

    public static void Postfix(Dialog_InfoCard __instance, Rect __state)
    {
        if (__state == Rect.zero || !TryResolveImage(__instance, out _, out Texture2D texture) || texture == null)
        {
            return;
        }

        Rect imageRect = FitTexture(__state, texture);
        GUI.DrawTexture(imageRect, texture, ScaleMode.ScaleToFit, true);
    }

    private static bool TryResolveImage(Dialog_InfoCard dialog, out InfoCardImageExtension extension, out Texture2D texture)
    {
        extension = null;
        texture = null;
        if (!IsStatsTab(dialog))
        {
            return false;
        }

        Def def = ResolveDef(dialog);
        extension = def?.GetModExtension<InfoCardImageExtension>();
        if (extension == null || !extension.showImageInInfoCard)
        {
            return false;
        }

        string imagePath = extension.imagePath.NullOrEmpty() ? extension.ImageToShowInInfoCard : extension.imagePath;
        if (imagePath.NullOrEmpty())
        {
            return false;
        }

        texture = ResolveTexture(imagePath);
        return texture != null;
    }

    private static bool IsStatsTab(Dialog_InfoCard dialog)
    {
        object tab = TabField?.GetValue(dialog);
        return tab == null || tab.ToString() == "Stats";
    }

    private static Def ResolveDef(Dialog_InfoCard dialog)
    {
        if (ThingField?.GetValue(dialog) is Thing thing)
        {
            return thing.def;
        }

        Def def = DefField?.GetValue(dialog) as Def;
        if (def is PawnKindDef pawnKind)
        {
            return pawnKind.race;
        }

        return def;
    }

    private static Texture2D ResolveTexture(string imagePath)
    {
        if (!TextureCache.TryGetValue(imagePath, out Texture2D texture))
        {
            texture = ContentFinder<Texture2D>.Get(imagePath, false);
            TextureCache[imagePath] = texture;
        }

        return texture;
    }

    private static float ResolveImageHeight(InfoCardImageExtension extension, Rect cardRect)
    {
        float authoredHeight = Mathf.Clamp(extension.imageHeight, MinImageHeight, MaxImageHeight);
        return Mathf.Min(authoredHeight, Mathf.Max(0f, cardRect.height * 0.45f));
    }

    private static Rect FitTexture(Rect container, Texture2D texture)
    {
        float textureRatio = texture.width / (float)texture.height;
        float containerRatio = container.width / container.height;
        if (textureRatio > containerRatio)
        {
            float height = container.width / textureRatio;
            return new Rect(container.x, container.y + ((container.height - height) / 2f), container.width, height);
        }

        float width = container.height * textureRatio;
        return new Rect(container.x + ((container.width - width) / 2f), container.y, width, container.height);
    }
}

internal static class MFVanillaDevModeSettings
{
    private static readonly System.Type ModType = AccessTools.TypeByName("MFVanilla.Core.MFVanillaMod");
    private static readonly PropertyInfo SettingsProperty = ModType?.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static);

    public static bool ShowDevModeSpellGizmos
    {
        get
        {
            object settings = SettingsProperty?.GetValue(null, null);
            FieldInfo settingField = settings?.GetType().GetField("ShowDevModeSpellGizmos", BindingFlags.Public | BindingFlags.Instance);
            return settingField != null && (bool)settingField.GetValue(settings);
        }
    }
}
