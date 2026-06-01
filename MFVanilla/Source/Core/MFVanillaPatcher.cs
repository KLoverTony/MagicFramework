using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.PawnLifecycle;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using MFPlanarMagicUtility = MagicFramework.Core.PlanarMagicUtility;

namespace MFVanilla.Core;

/// <summary>
/// Harmony patches for suppressing vanilla technology research.
/// </summary>
public static class MFVanillaPatcher
{
    private const string ForbiddenLoreResearchDefName = "MFV_ForbiddenLore";

    private static readonly HashSet<string> SuppressionRoots = new()
    {
        "Electricity",
        "MicroelectronicsBasics",
        "MultiAnalyzer",
    };

    private static readonly HashSet<string> SuppressedResearchDefNames = new();
    private static readonly HashSet<string> LapidaryPlaceholderUnlockDefNames = new()
    {
        GemstoneUtility.BreakChunkRecipeDefName,
        GemstoneUtility.BreakDenseChunkRecipeDefName,
        GemstoneUtility.CutGemstoneRecipeDefName,
    };

    private static bool _isPatched;
    private static bool _suppressionCacheBuilt;

    public static void Patch(Harmony harmony)
    {
        if (_isPatched) return;

        harmony.Patch(
            AccessTools.PropertyGetter(typeof(ResearchProjectDef), nameof(ResearchProjectDef.IsHidden)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(ResearchProjectDef_IsHidden_Postfix))
        );

        harmony.Patch(
            AccessTools.PropertyGetter(typeof(ResearchProjectDef), nameof(ResearchProjectDef.CanStartNow)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(ResearchProjectDef_CanStartNow_Postfix))
        );

        harmony.Patch(
            AccessTools.PropertyGetter(typeof(ResearchProjectDef), nameof(ResearchProjectDef.UnlockedDefs)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(ResearchProjectDef_UnlockedDefs_Postfix))
        );

        harmony.Patch(
            AccessTools.PropertyGetter(typeof(MainTabWindow_Research), nameof(MainTabWindow_Research.VisibleResearchProjects)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(MainTabWindow_Research_VisibleResearchProjects_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(SpellRuntimeGameComponent), nameof(SpellRuntimeGameComponent.HasArcaneGift)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(SpellRuntimeGameComponent_HasArcaneGift_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(SpellRuntimeGameComponent), nameof(SpellRuntimeGameComponent.GetCasterLevel)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(SpellRuntimeGameComponent_GetCasterLevel_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(SpellRuntimeGameComponent), nameof(SpellRuntimeGameComponent.SetArcaneGift)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(SpellRuntimeGameComponent_SetArcaneGift_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(ResearchManager), nameof(ResearchManager.ResearchPerformed)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(ResearchManager_ResearchPerformed_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(ResearchManager), nameof(ResearchManager.FinishProject), new[] { typeof(ResearchProjectDef), typeof(bool), typeof(Pawn), typeof(bool) }),
            prefix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(ResearchManager_FinishProject_Prefix)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(ResearchManager_FinishProject_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(Bill_PawnAllowedToStartAnew_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(Mineable), "TrySpawnYield", new[] { typeof(Map), typeof(bool), typeof(Pawn) }),
            prefix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(Mineable_TrySpawnYield_Prefix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(GenRecipe_MakeRecipeProducts_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(WorkGiver_DoBill), "IsUsableIngredient"),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(WorkGiver_DoBill_IsUsableIngredient_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(WorkGiver_DoBill), "ThingIsUsableBillGiver"),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(WorkGiver_DoBill_ThingIsUsableBillGiver_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(WorkGiver_DoBill), nameof(WorkGiver_DoBill.JobOnThing), new[] { typeof(Pawn), typeof(Thing), typeof(bool) }),
            prefix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(WorkGiver_DoBill_JobOnThing_Prefix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) }),
            prefix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(PawnGenerator_GeneratePawn_Prefix)),
            postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(PawnGenerator_GeneratePawn_Postfix))
        );

        MethodInfo thingKillMethod = AccessTools.Method(typeof(Thing), nameof(Thing.Kill));
        if (thingKillMethod != null)
        {
            harmony.Patch(
                thingKillMethod,
                prefix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(Thing_Kill_Prefix))
            );
        }

        PatchPlanarTransportBlocks(harmony);

        _isPatched = true;
        Log.Message("[MFVanilla] Vanilla tech research suppression patches applied.");
    }

    private static void PatchPlanarTransportBlocks(Harmony harmony)
    {
        PatchTransportGizmos(harmony, "RimWorld.CompLaunchable");
        PatchTransportGizmos(harmony, "RimWorld.CompShuttle");

        Type shuttlePermitType = AccessTools.TypeByName("RimWorld.RoyalTitlePermitWorker_CallShuttle");
        MethodInfo validateTarget = AccessTools.Method(shuttlePermitType, "ValidateTarget", new[] { typeof(LocalTargetInfo), typeof(bool) });
        if (validateTarget != null)
        {
            harmony.Patch(validateTarget, prefix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(RoyalShuttlePermit_ValidateTarget_Prefix)));
        }
    }

    private static void PatchTransportGizmos(Harmony harmony, string typeName)
    {
        Type type = AccessTools.TypeByName(typeName);
        if (type == null)
        {
            return;
        }

        MethodInfo method = AccessTools.Method(type, "CompGetGizmosExtra");
        if (method != null)
        {
            harmony.Patch(method, postfix: new HarmonyMethod(typeof(MFVanillaPatcher), nameof(TransportComp_CompGetGizmosExtra_Postfix)));
        }
    }

    private static void TransportComp_CompGetGizmosExtra_Postfix(ThingComp __instance, ref IEnumerable<Gizmo> __result)
    {
        if (__instance?.parent?.Map == null || !MFPlanarMagicUtility.BlocksOffMapTransport(__instance.parent.Map) || __result == null)
        {
            return;
        }

        __result = DisableTransportGizmos(__result);
    }

    private static IEnumerable<Gizmo> DisableTransportGizmos(IEnumerable<Gizmo> gizmos)
    {
        foreach (Gizmo gizmo in gizmos)
        {
            if (gizmo is Command command && IsLikelyTransportCommand(command))
            {
                command.Disable(MFPlanarMagicUtility.PlanarTransportBlockedMessage);
            }

            yield return gizmo;
        }
    }

    private static bool IsLikelyTransportCommand(Command command)
    {
        string label = command.defaultLabel?.ToString() ?? string.Empty;
        string desc = command.defaultDesc?.ToString() ?? string.Empty;
        return ContainsTransportWord(label) || ContainsTransportWord(desc);
    }

    private static bool ContainsTransportWord(string text)
    {
        if (text.NullOrEmpty())
        {
            return false;
        }

        return text.IndexOf("launch", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("shuttle", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("transport", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("pod", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool RoyalShuttlePermit_ValidateTarget_Prefix(LocalTargetInfo target, bool showMessages, ref bool __result)
    {
        Map map = Find.CurrentMap;
        if (map != null && MFPlanarMagicUtility.BlocksOffMapTransport(map))
        {
            if (showMessages)
            {
                MFPlanarMagicUtility.MessageOffMapTransportBlocked();
            }

            __result = false;
            return false;
        }

        return true;
    }

    public static void ResetSuppressionCache()
    {
        SuppressedResearchDefNames.Clear();
        _suppressionCacheBuilt = false;
    }

    public static void NotifySettingsChanged()
    {
        ResetSuppressionCache();

        foreach (ResearchProjectDef project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
        {
            project.ClearCachedData();
        }

        RefreshOpenResearchWindows();
    }

    public static bool IsResearchSuppressed(ResearchProjectDef project)
    {
        if (MFVanillaMod.Settings == null || !MFVanillaMod.Settings.DisableTechResearch || project == null) return false;

        EnsureSuppressionCache();
        return SuppressedResearchDefNames.Contains(project.defName);
    }

    private static void EnsureSuppressionCache()
    {
        if (_suppressionCacheBuilt) return;

        SuppressedResearchDefNames.Clear();

        List<ResearchProjectDef> coreProjects = DefDatabase<ResearchProjectDef>.AllDefsListForReading
            .Where(project => project?.modContentPack?.IsCoreMod == true)
            .ToList();

        foreach (ResearchProjectDef root in coreProjects.Where(project => SuppressionRoots.Contains(project.defName)))
        {
            SuppressedResearchDefNames.Add(root.defName);
        }

        bool changed;
        do
        {
            changed = false;
            foreach (ResearchProjectDef project in coreProjects)
            {
                if (SuppressedResearchDefNames.Contains(project.defName)) continue;
                if (!DependsOnSuppressedResearch(project)) continue;

                SuppressedResearchDefNames.Add(project.defName);
                changed = true;
            }
        }
        while (changed);

        _suppressionCacheBuilt = true;
        Log.Message($"[MFVanilla] Suppressing {SuppressedResearchDefNames.Count} vanilla tech research projects.");
    }

    private static bool DependsOnSuppressedResearch(ResearchProjectDef project)
    {
        return HasSuppressedPrerequisite(project.prerequisites)
            || HasSuppressedPrerequisite(project.hiddenPrerequisites);
    }

    private static bool HasSuppressedPrerequisite(List<ResearchProjectDef> prerequisites)
    {
        return prerequisites != null
            && prerequisites.Any(prerequisite => prerequisite != null && SuppressedResearchDefNames.Contains(prerequisite.defName));
    }

    private static void ResearchProjectDef_IsHidden_Postfix(ResearchProjectDef __instance, ref bool __result)
    {
        if (IsResearchSuppressed(__instance))
        {
            __result = true;
        }
    }

    private static void ResearchProjectDef_CanStartNow_Postfix(ResearchProjectDef __instance, ref bool __result)
    {
        if (IsResearchSuppressed(__instance))
        {
            __result = false;
        }
    }

    private static void ResearchProjectDef_UnlockedDefs_Postfix(ResearchProjectDef __instance, ref List<Def> __result)
    {
        if (__instance?.defName != "MFV_Lapidary" || __result == null) return;

        __result = __result
            .Where(def => def == null || !LapidaryPlaceholderUnlockDefNames.Contains(def.defName))
            .ToList();
    }

    private static void MainTabWindow_Research_VisibleResearchProjects_Postfix(ref List<ResearchProjectDef> __result)
    {
        if (MFVanillaMod.Settings == null || !MFVanillaMod.Settings.DisableTechResearch || __result == null) return;

        __result = __result
            .Where(project => !IsResearchSuppressed(project))
            .ToList();
    }

    private static void SpellRuntimeGameComponent_HasArcaneGift_Postfix(Pawn pawn, ref bool __result)
    {
        if (!__result && ArcaneGiftUtility.HasArcaneGiftTrait(pawn))
        {
            SpellRuntimeGameComponent.Instance?.SetArcaneGift(pawn, true);
            __result = true;
        }
    }

    private static void SpellRuntimeGameComponent_GetCasterLevel_Postfix(SpellRuntimeGameComponent __instance, Pawn pawn, ref int __result)
    {
        if (__result < 1 && ArcaneGiftUtility.HasArcaneGiftTrait(pawn))
        {
            __instance?.SetArcaneGift(pawn, true);
            __result = 1;
        }
    }

    private static void SpellRuntimeGameComponent_SetArcaneGift_Postfix(Pawn pawn, bool value)
    {
        if (value)
        {
            ArcaneGiftUtility.TryGiveArcaneGiftTraitOnly(pawn);
        }
        else
        {
            ArcaneGiftUtility.TryRemoveArcaneGiftTrait(pawn);
        }
    }

    private static void ResearchManager_ResearchPerformed_Postfix(Pawn researcher)
    {
        if (researcher?.CurJob == null) return;

        Thing bench = researcher.CurJob.GetTarget(TargetIndex.A).Thing;
        ArcanePracticeUtility.NotifyArcaneResearchPerformed(researcher, bench);
    }

    private static void ResearchManager_FinishProject_Prefix(ResearchProjectDef proj, out bool __state)
    {
        __state = proj?.IsFinished == true;
    }

    private static void ResearchManager_FinishProject_Postfix(ResearchProjectDef proj, Pawn researcher, bool __state)
    {
        if (__state || proj?.IsFinished != true) return;

        TryDropMysteriousAidScroll(proj, researcher);
    }

    private static void TryDropMysteriousAidScroll(ResearchProjectDef project, Pawn researcher)
    {
        ThingDef scrollDef = RandomScrollUnlockedBy(project);
        if (scrollDef == null) return;

        Map map = researcher?.MapHeld
            ?? Find.Maps?.FirstOrDefault(candidate => candidate?.IsPlayerHome == true);
        if (map == null) return;

        IntVec3 dropCell = researcher?.Spawned == true ? researcher.Position : map.Center;
        Thing scroll = ThingMaker.MakeThing(scrollDef);
        if (!GenPlace.TryPlaceThing(scroll, dropCell, map, ThingPlaceMode.Near, out Thing placedThing))
        {
            return;
        }

        Messages.Message(
            "A mysterious voice echoes across your town: \"This will aid you in your quest.\"",
            placedThing,
            MessageTypeDefOf.PositiveEvent,
            true);
    }

    private static ThingDef RandomScrollUnlockedBy(ResearchProjectDef project)
    {
        if (project == null) return null;

        List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(thingDef => IsMFVanillaSpellScrollUnlockedBy(thingDef, project))
            .ToList();

        return candidates.Count == 0 ? null : candidates.RandomElement();
    }

    private static bool IsMFVanillaSpellScrollUnlockedBy(ThingDef thingDef, ResearchProjectDef project)
    {
        if (thingDef?.defName == null
            || !thingDef.defName.StartsWith("MFV_SpellScroll_", StringComparison.Ordinal)
            || thingDef.comps == null)
        {
            return false;
        }

        CompProperties_UseEffectLearnSpell learnComp = thingDef.comps
            .OfType<CompProperties_UseEffectLearnSpell>()
            .FirstOrDefault();
        List<ResearchProjectDef> requiredResearch = learnComp?.requiredResearch;
        if (requiredResearch == null || !requiredResearch.Contains(project))
        {
            return false;
        }

        if (requiredResearch.Any(requiredProject => requiredProject?.defName == ForbiddenLoreResearchDefName))
        {
            return false;
        }

        return requiredResearch.All(requiredProject => requiredProject?.IsFinished == true);
    }

    private static void Bill_PawnAllowedToStartAnew_Postfix(Bill __instance, Pawn p, ref bool __result)
    {
        if (!__result || p == null) return;

        SpellDef spell = __instance?.recipe?.GetModExtension<ScribeSpellScrollRecipeExtension>()?.spell;
        if (spell == null) return;

        if (SpellRuntimeGameComponent.Instance?.KnowsSpell(p, spell) != true)
        {
            JobFailReason.Is("MFV_MustKnowSpellToScribe".Translate(spell.LabelCap));
            __result = false;
        }
    }

    private static bool Mineable_TrySpawnYield_Prefix(Mineable __instance, Map map, bool moteOnWaste, Pawn pawn)
    {
        if (!GemstoneUtility.IsGemstoneVein(__instance)) return true;

        GemstoneUtility.SpawnMineYield(__instance, map, pawn);
        return false;
    }

    private static void GenRecipe_MakeRecipeProducts_Postfix(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, IBillGiver billGiver, ref IEnumerable<Thing> __result)
    {
        ArcanePracticeUtility.NotifyArcaneProductionCompleted(worker, recipeDef);

        if (EnchantmentUtility.TryMakeRecipeProducts(recipeDef, worker, ingredients, billGiver as Thing, out List<Thing> enchantedProducts))
        {
            __result = enchantedProducts;
            return;
        }

        if (GemstoneUtility.TryMakeRecipeProducts(recipeDef, worker, ingredients, out List<Thing> products))
        {
            __result = products;
        }
    }

    private static void WorkGiver_DoBill_IsUsableIngredient_Postfix(Thing t, Bill bill, ref bool __result)
    {
        if (!__result || !EnchantmentUtility.IsEnchantmentRecipe(bill?.recipe)) return;
        if (EnchantmentUtility.IsEnchantableSourceWeapon(t, bill.recipe) && !EnchantmentUtility.IsGoodOrBetterSourceWeapon(t, bill.recipe))
        {
            __result = false;
        }
    }

    private static void WorkGiver_DoBill_ThingIsUsableBillGiver_Postfix(Thing thing, ref bool __result)
    {
        if (!__result) return;

        CompArcaneForge arcaneForge = thing?.TryGetComp<CompArcaneForge>();
        if (arcaneForge != null && !arcaneForge.HasRequiredSpires)
        {
            __result = false;
        }
    }

    private static bool WorkGiver_DoBill_JobOnThing_Prefix(Pawn pawn, Thing thing, ref Job __result)
    {
        if (!RequiresArcaneGiftWorker(thing) || SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) == true)
        {
            return true;
        }

        JobFailReason.Is("MFV_RequiresArcaneGiftWorker".Translate());
        __result = null;
        return false;
    }

    private static bool PawnGenerator_GeneratePawn_Prefix(ref PawnGenerationRequest request, ref Pawn __result)
    {
        if (request.KindDef?.defName != "MFV_Skeleton" && request.KindDef?.defName != "MFV_IllusoryReinforcement")
        {
            return true;
        }

        PawnKindDef lifecycleKindDef = request.KindDef;
        PawnGenerationRequest baseRequest = new PawnGenerationRequest(
            kind: PawnKindDefOf.Colonist,
            faction: request.Faction ?? Faction.OfPlayer,
            context: request.Context,
            tile: request.Tile,
            forceGenerateNewPawn: true,
            allowDead: false,
            allowDowned: false,
            canGeneratePawnRelations: false,
            mustBeCapableOfViolence: false,
            colonistRelationChanceFactor: 0f,
            allowPregnant: false,
            allowFood: false,
            allowAddictions: false,
            fixedGender: Gender.Male,
            forceNoIdeo: true,
            forceNoBackstory: true,
            developmentalStages: DevelopmentalStage.Adult,
            dontGiveWeapon: true,
            maximumAgeTraits: 0,
            minimumAgeTraits: 0,
            forceNoGear: true);

        Pawn skeleton = PawnGenerator.GeneratePawn(baseRequest);
        if (skeleton == null)
        {
            __result = null;
            return false;
        }

        if (lifecycleKindDef.defName == "MFV_Skeleton")
        {
            ConvertGeneratedPawnToMFVSkeleton(skeleton, lifecycleKindDef);
        }
        else
        {
            ConvertGeneratedPawnToMFVIllusoryReinforcement(skeleton, lifecycleKindDef);
        }

        __result = skeleton;
        return false;
    }

    private static void PawnGenerator_GeneratePawn_Postfix(Pawn __result, PawnGenerationRequest request)
    {
        if (__result?.kindDef?.defName == "MFV_Skeleton" || __result?.def?.defName == "MFV_Skeleton")
        {
            ApplyMFVSkeletonAppearance(__result);
        }

        if (__result?.Faction?.def?.defName != "MFV_ElementalistTribe"
            || Faction.OfPlayer == null
            || !__result.Faction.HostileTo(Faction.OfPlayer)
            || __result.RaceProps?.Humanlike != true
            || Rand.Chance(0.8f))
        {
            return;
        }

        AssignElementalistAISpells(__result);
    }

    private static bool Thing_Kill_Prefix(Thing __instance)
    {
        if (__instance is not Pawn pawn || pawn.def?.defName != "MFV_IllusoryReinforcement")
        {
            return true;
        }

        if (pawn.Spawned)
        {
            FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastAreaEffect, 0.8f);
            pawn.DeSpawn(DestroyMode.Vanish);
        }

        if (!pawn.Destroyed)
        {
            pawn.Destroy(DestroyMode.Vanish);
        }

        return false;
    }

    private static void ConvertGeneratedPawnToMFVIllusoryReinforcement(Pawn pawn, PawnKindDef illusionKindDef)
    {
        if (pawn == null || illusionKindDef?.race == null)
        {
            return;
        }

        pawn.def = illusionKindDef.race;
        pawn.kindDef = illusionKindDef;
        pawn.gender = Gender.Male;
        pawn.Name = new NameSingle("phantasm");
        ResetPawnRenderer(pawn);
        AttachPawnLifecycleComp(pawn);
        PawnLifecycleEnforcementUtility.NormalizeLifeStage(pawn);
        ApplyMFVSkeletonAppearance(pawn);
        PawnLifecycleEnforcementUtility.EnforceAll(pawn);
        ResetPawnRenderer(pawn);
        pawn.Drawer?.renderer?.EnsureGraphicsInitialized();
    }

    private static void ConvertGeneratedPawnToMFVSkeleton(Pawn pawn, PawnKindDef skeletonKindDef)
    {
        if (pawn == null || skeletonKindDef?.race == null)
        {
            return;
        }

        pawn.def = skeletonKindDef.race;
        pawn.kindDef = skeletonKindDef;
        pawn.gender = Gender.Male;
        pawn.Name = new NameSingle("skeleton");
        ResetPawnRenderer(pawn);
        AttachPawnLifecycleComp(pawn);
        PawnLifecycleEnforcementUtility.NormalizeLifeStage(pawn);
        ApplyMFVSkeletonAppearance(pawn);
        ApplyMFVSkeletonXenotype(pawn);
        PawnLifecycleEnforcementUtility.EnforceAll(pawn);
        ResetPawnRenderer(pawn);
        pawn.Drawer?.renderer?.EnsureGraphicsInitialized();
    }

    private static void ResetPawnRenderer(Pawn pawn)
    {
        if (pawn?.Drawer?.renderer == null)
        {
            return;
        }

        pawn.Drawer.renderer = new PawnRenderer(pawn);
        pawn.Drawer.renderer.renderTree = new PawnRenderTree(pawn);
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    private static void AttachPawnLifecycleComp(Pawn pawn)
    {
        if (pawn?.AllComps == null || pawn.GetComp<CompPawnLifecycleEnforcer>() != null)
        {
            return;
        }

        CompProperties_PawnLifecycleEnforcer compProperties = pawn.def?.comps?
            .OfType<CompProperties_PawnLifecycleEnforcer>()
            .FirstOrDefault();
        if (compProperties == null)
        {
            return;
        }

        CompPawnLifecycleEnforcer comp = new CompPawnLifecycleEnforcer
        {
            parent = pawn
        };
        comp.Initialize(compProperties);
        pawn.AllComps.Add(comp);
    }

    private static void AssignElementalistAISpells(Pawn pawn)
    {
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        SpellAIManagerGameComponent aiManager = SpellAIManagerGameComponent.Instance;
        if (runtime == null || aiManager == null)
        {
            return;
        }

        List<SpellAIEntry> pool = new()
        {
            Entry("MF_Firebolt", SpellAIIntent.Hostile),
            Entry("MF_ForcePush", SpellAIIntent.Hostile),
            Entry("MF_Heal", SpellAIIntent.HealAlly),
            Entry("MF_Stoneskin", SpellAIIntent.BuffAlly),
            Entry("MF_Might", SpellAIIntent.BuffAlly)
        };

        pool.RemoveAll(entry => entry?.spell == null);
        if (pool.Count == 0)
        {
            return;
        }

        pool.Shuffle();
        int spellCount = Math.Min(pool.Count, Rand.RangeInclusive(1, 3));
        List<SpellAIEntry> selected = pool.Take(spellCount).ToList();
        if (selected.Count == 0)
        {
            return;
        }

        runtime.SetArcaneGift(pawn, true);
        runtime.SetCasterLevel(pawn, 3);
        ArcaneDisciplineDef discipline = DefDatabase<ArcaneDisciplineDef>.GetNamedSilentFail("MFV_ArcaneDiscipline_Elementalist");
        if (discipline != null)
        {
            runtime.SetArcaneDiscipline(pawn, discipline);
        }

        for (int i = 0; i < selected.Count; i++)
        {
            runtime.LearnSpell(pawn, selected[i].spell);
        }

        ApplyElementalistCasterGarb(pawn, selected);
        aiManager.RegisterPawn(pawn, selected);
    }

    private static void ApplyMFVSkeletonAppearance(Pawn pawn)
    {
        if (pawn?.story == null)
        {
            return;
        }

        BodyTypeDef bodyTypeDef = DefDatabase<BodyTypeDef>.GetNamedSilentFail("MFV_SkeletonThin");
        HeadTypeDef headTypeDef = DefDatabase<HeadTypeDef>.GetNamedSilentFail("MFV_SkeletonHead");
        HairDef hairDef = DefDatabase<HairDef>.GetNamedSilentFail("Bald");

        if (bodyTypeDef != null)
        {
            pawn.story.bodyType = bodyTypeDef;
        }

        if (headTypeDef != null)
        {
            pawn.story.headType = headTypeDef;
        }

        if (hairDef != null)
        {
            pawn.story.hairDef = hairDef;
        }

        BeardDef beardDef = DefDatabase<BeardDef>.GetNamedSilentFail("NoBeard");
        if (beardDef != null && pawn.style != null)
        {
            pawn.style.beardDef = beardDef;
        }

        pawn.gender = Gender.Male;
        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }

    private static void ApplyMFVSkeletonXenotype(Pawn pawn)
    {
        if (!ModsConfig.BiotechActive || pawn?.genes == null)
        {
            return;
        }

        XenotypeDef skeletonXenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail("MFV_SkeletonXenotype");
        if (skeletonXenotype == null)
        {
            return;
        }

        foreach (Gene gene in pawn.genes.GenesListForReading.ToList())
        {
            pawn.genes.RemoveGene(gene);
        }

        pawn.genes.ClearXenogenes();
        if (pawn.genes.Xenotype != skeletonXenotype)
        {
            pawn.genes.SetXenotypeDirect(skeletonXenotype);
        }
    }

    private static SpellAIEntry Entry(string spellDefName, SpellAIIntent intent)
    {
        return new SpellAIEntry(DefDatabase<SpellDef>.GetNamedSilentFail(spellDefName), intent);
    }

    private static void ApplyElementalistCasterGarb(Pawn pawn, List<SpellAIEntry> selected)
    {
        if (pawn?.apparel == null || selected == null || selected.Count == 0)
        {
            return;
        }

        ElementalCasterRole role = DetermineElementalCasterRole(selected);
        Color color = ElementalCasterColor(role);
        Apparel apparel = EnsureElementalistCasterRobe(pawn) ?? BestElementalistCasterApparel(pawn);
        if (apparel == null)
        {
            return;
        }

        if (apparel.TryGetComp<CompColorable>() != null)
        {
            apparel.DesiredColor = color;
        }
        else
        {
            apparel.SetColor(color, reportFailure: false);
        }
    }

    private static ElementalCasterRole DetermineElementalCasterRole(List<SpellAIEntry> selected)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            string defName = selected[i]?.spell?.defName;
            if (defName == "MF_Firebolt")
            {
                return ElementalCasterRole.Fire;
            }

            if (defName == "MF_ForcePush")
            {
                return ElementalCasterRole.Air;
            }

            if (defName == "MF_Heal")
            {
                return ElementalCasterRole.Water;
            }

            if (defName == "MF_Stoneskin" || defName == "MF_Might")
            {
                return ElementalCasterRole.Earth;
            }
        }

        return ElementalCasterRole.Earth;
    }

    private static Color ElementalCasterColor(ElementalCasterRole role)
    {
        return role switch
        {
            ElementalCasterRole.Fire => new Color(0.78f, 0.18f, 0.06f),
            ElementalCasterRole.Water => new Color(0.08f, 0.36f, 0.78f),
            ElementalCasterRole.Earth => new Color(0.2f, 0.46f, 0.18f),
            ElementalCasterRole.Air => new Color(0.86f, 0.78f, 0.34f),
            _ => Color.white
        };
    }

    private static Apparel EnsureElementalistCasterRobe(Pawn pawn)
    {
        Apparel existingRobe = pawn.apparel.WornApparel.FirstOrDefault(apparel => apparel?.def?.defName == "Apparel_Robe");
        if (existingRobe != null)
        {
            return existingRobe;
        }

        ThingDef robeDef = DefDatabase<ThingDef>.GetNamedSilentFail("Apparel_Robe");
        if (robeDef == null)
        {
            return null;
        }

        ThingDef stuff = GenStuff.DefaultStuffFor(robeDef);
        if (stuff == null)
        {
            return null;
        }

        Apparel robe = ThingMaker.MakeThing(robeDef, stuff) as Apparel;
        if (robe == null || !ApparelUtility.HasPartsToWear(pawn, robeDef))
        {
            return null;
        }

        pawn.apparel.Wear(robe, dropReplacedApparel: false, locked: false);
        return robe;
    }

    private static Apparel BestElementalistCasterApparel(Pawn pawn)
    {
        return pawn.apparel.WornApparel
            .Where(apparel => apparel != null)
            .OrderByDescending(apparel => apparel.def.defName == "Apparel_Robe")
            .ThenByDescending(apparel => apparel.def.apparel?.layers?.Contains(ApparelLayerDefOf.Shell) == true)
            .ThenByDescending(apparel => apparel.def.apparel?.bodyPartGroups?.Contains(BodyPartGroupDefOf.Torso) == true)
            .FirstOrDefault();
    }

    private enum ElementalCasterRole
    {
        Fire,
        Water,
        Earth,
        Air
    }

    private static bool RequiresArcaneGiftWorker(Thing thing)
    {
        string defName = thing?.def?.defName;
        return defName == "MFV_ScribingTable" || defName == "MFV_ArcaneForge";
    }

    private static void RefreshOpenResearchWindows()
    {
        if (Find.WindowStack == null) return;

        foreach (MainTabWindow_Research window in Find.WindowStack.Windows.OfType<MainTabWindow_Research>())
        {
            AccessTools.Field(typeof(MainTabWindow_Research), "cachedVisibleResearchProjects")?.SetValue(window, null);
            AccessTools.Field(typeof(MainTabWindow_Research), "cachedUnlockedDefsGroupedByPrerequisites")?.SetValue(window, null);
            AccessTools.Field(typeof(MainTabWindow_Research), "matchingProjects")?.SetValue(window, null);

            ResearchProjectDef selectedProject = AccessTools.Field(typeof(MainTabWindow_Research), "selectedProject")?.GetValue(window) as ResearchProjectDef;
            if (IsResearchSuppressed(selectedProject))
            {
                AccessTools.Field(typeof(MainTabWindow_Research), "selectedProject")?.SetValue(window, null);
            }
        }
    }
}
