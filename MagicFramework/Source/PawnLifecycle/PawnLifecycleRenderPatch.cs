using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MagicFramework.Core;
using Verse;

namespace MagicFramework.PawnLifecycle;

[StaticConstructorOnStartup]
public static class PawnLifecycleRenderPatch
{
    private static readonly FieldInfo PawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");
    private static readonly HashSet<string> LoggedPawnIds = new();

    static PawnLifecycleRenderPatch()
    {
        try
        {
            Harmony harmony = new Harmony("oracle.magicframework.pawnlifecycle.render");
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt)),
                finalizer: new HarmonyMethod(typeof(PawnLifecycleRenderPatch), nameof(RenderPawnAt_Finalizer)));
        }
        catch (Exception ex)
        {
            Log.Warning("[MagicFramework] Could not install skeletal pawn render diagnostics: " + ex);
        }
    }

    public static Exception RenderPawnAt_Finalizer(PawnRenderer __instance, Exception __exception)
    {
        if (__exception == null)
        {
            return null;
        }

        Pawn pawn = PawnField?.GetValue(__instance) as Pawn;
        if (!PawnLifecycleUtility.TryGetLifecycle(pawn, out PawnLifecycleExtension extension, out _)
            || extension.bodyForm is not (PawnLifecycleBodyForm.Skeletal or PawnLifecycleBodyForm.Spectral))
        {
            return __exception;
        }

        string pawnId = pawn?.ThingID ?? "<null>";
        if (LoggedPawnIds.Add(pawnId))
        {
            MagicLog.Message(MagicLogSubsystem.Visuals, "[MagicFramework] Suppressed lifecycle pawn render exception for " +
                (pawn?.LabelShort ?? "<null>") +
                " def=" + (pawn?.def?.defName ?? "<null>") +
                " kind=" + (pawn?.kindDef?.defName ?? "<null>") +
                " bodyForm=" + extension.bodyForm +
                " gender=" + (pawn?.gender.ToString() ?? "<null>") +
                " bodyType=" + (pawn?.story?.bodyType?.defName ?? "<null>") +
                " headType=" + (pawn?.story?.headType?.defName ?? "<null>") +
                ": " + __exception.GetType().Name + " " + __exception.Message);
        }

        return null;
    }
}
