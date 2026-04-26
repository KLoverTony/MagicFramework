using System.Collections.Generic;
using HarmonyLib;
using MagicFramework.Debug;
using RimWorld;
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

        if (Prefs.DevMode && __instance != null && (__instance.IsColonistPlayerControlled || __instance.IsPrisonerOfColony || __instance.IsSlaveOfColony))
        {
            yield return SpellDebugCasting.CreateCasterLevelGizmo(__instance);
            yield return SpellDebugCasting.CreateScalingBoltGizmo(__instance);
            yield return SpellDebugCasting.CreateFireboltGizmo(__instance);
            yield return SpellDebugCasting.CreateFireballGizmo(__instance);
            yield return SpellDebugCasting.CreateChainLightningGizmo(__instance);
            yield return SpellDebugCasting.CreateDelayedBlastRuneGizmo(__instance);
            yield return SpellDebugCasting.CreateRuneTrapGizmo(__instance);
            yield return SpellDebugCasting.CreateWallOfFireGizmo(__instance);
            yield return SpellDebugCasting.CreateDisintegrateGizmo(__instance);
            yield return SpellDebugCasting.CreateFlameFieldGizmo(__instance);
            yield return SpellDebugCasting.CreateForcePushGizmo(__instance);
            yield return SpellDebugCasting.CreateForcePullGizmo(__instance);
            yield return SpellDebugCasting.CreateBlinkStepGizmo(__instance);
            yield return SpellDebugCasting.CreateHasteGizmo(__instance);
            yield return SpellDebugCasting.CreateMightGizmo(__instance);
            yield return SpellDebugCasting.CreateForceFieldGizmo(__instance);
            yield return SpellDebugCasting.CreateManaShieldGizmo(__instance);
            yield return SpellDebugCasting.CreateHealGizmo(__instance);
            yield return SpellDebugCasting.CreateRegenerationGizmo(__instance);
            yield return SpellDebugCasting.CreateSummonDogGizmo(__instance);
            yield return SpellDebugCasting.CreateCreateFoodGizmo(__instance);
        }
    }
}
