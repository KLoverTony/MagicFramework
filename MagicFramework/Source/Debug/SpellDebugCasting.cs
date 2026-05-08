using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Context;
using MagicFramework.Core;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Debug;

/// <summary>
/// Debug-only helpers for manually casting framework spells from the UI.
/// </summary>
public static class SpellDebugCasting
{
    private static readonly SpellExecutor Executor = new();

    public static Gizmo CreateFireboltGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(
            pawn,
            SpellDebugSpellLibrary.GetFirebolt(),
            "Debug: Cast Firebolt",
            "Starts target selection and casts the current debug Firebolt spell through Magic Framework.",
            "UI/Commands/DesirePower",
            BeginFireboltTargeting);
    }

    public static Gizmo CreateScalingBoltGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(
            pawn,
            SpellDebugSpellLibrary.GetScalingBolt(),
            "Debug: Cast Scaling Bolt",
            "Starts target selection and casts a built-in scalable bolt using the current debug caster level.",
            "UI/Commands/DesirePower",
            selectedPawn => BeginSpellTargeting(selectedPawn, SpellDebugSpellLibrary.GetScalingBolt()));
    }

    public static Gizmo CreateFireballGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetFireball(), "Debug: Cast Fireball", "Starts target selection and casts the current debug Fireball spell through Magic Framework.", "UI/Commands/Attack");
    }

    public static Gizmo CreateChainLightningGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetChainLightning(), "Debug: Cast Chain Lightning", "Starts target selection and casts the current debug Chain Lightning spell through Magic Framework.", "UI/Commands/Attack");
    }

    public static Gizmo CreateDelayedBlastRuneGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetDelayedBlastRune(), "Debug: Cast Delayed Blast Rune", "Starts target selection and casts the current debug Delayed Blast Rune spell through Magic Framework.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateRuneTrapGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetRuneTrap(), "Debug: Cast Rune Trap", "Starts target selection and casts the current debug Rune Trap spell through Magic Framework.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateWallOfFireGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetWallOfFire(), "Debug: Cast Wall of Fire", "Starts target selection and casts the current debug Wall of Fire spell through Magic Framework.", "UI/Commands/Attack");
    }

    public static Gizmo CreateDisintegrateGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetDisintegrate(), "Debug: Cast Disintegrate", "Starts target selection and casts the current debug Disintegrate spell through Magic Framework.", "UI/Commands/Attack");
    }

    public static Gizmo CreateFlameFieldGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetFlameField(), "Debug: Cast Flame Field", "Starts target selection and casts the current debug Flame Field spell through Magic Framework.", "UI/Commands/Attack");
    }

    public static Gizmo CreateFreezeGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetFreeze(), "Debug: Cast Freeze", "Starts target selection and casts the current debug Freeze spell through Magic Framework.", "UI/Commands/Attack");
    }

    public static Gizmo CreateEarthCallGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetEarthCall(), "Debug: Cast Earth Call", "Starts target selection and casts the current debug Earth Call spell through Magic Framework.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateWatersEmbraceGizmo(Pawn pawn)
    {
        return CreateMaintainedSpellGizmo(pawn, SpellDebugSpellLibrary.GetWatersEmbrace(), "Debug: Cast Water's Embrace", "Debug: Cancel Water's Embrace", "Starts target selection and casts the current debug Water's Embrace spell through Magic Framework.", "Cleanly ends the active Water's Embrace aura.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateForcePushGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetForcePush(), "Debug: Cast Force Push", "Starts target selection and casts the current debug Force Push spell through Magic Framework.", "UI/Commands/Attack");
    }

    public static Gizmo CreateForcePullGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetForcePull(), "Debug: Cast Force Pull", "Starts target selection and casts the current debug Force Pull spell through Magic Framework.", "UI/Commands/Attack");
    }

    public static Gizmo CreateBlinkStepGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetBlinkStep(), "Debug: Cast Blink Step", "Starts target selection and casts the current debug Blink Step spell through Magic Framework.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateHasteGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetHaste(), "Debug: Cast Haste", "Starts target selection and casts the current debug Haste spell through Magic Framework.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateMightGizmo(Pawn pawn)
    {
        return CreateMaintainedSpellGizmo(pawn, SpellDebugSpellLibrary.GetMight(), "Debug: Cast Might", "Debug: Cancel Might", "Starts target selection and casts the current debug Might spell through Magic Framework.", "Cleanly ends the maintained Might spell without triggering break backlash.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateForceFieldGizmo(Pawn pawn)
    {
        return CreateMaintainedSpellGizmo(pawn, SpellDebugSpellLibrary.GetForceField(), "Debug: Cast Force Field", "Debug: Cancel Force Field", "Starts target selection and casts the current debug Force Field spell through Magic Framework.", "Cleanly ends the maintained Force Field spell.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateManaShieldGizmo(Pawn pawn)
    {
        return CreateMaintainedSpellGizmo(pawn, SpellDebugSpellLibrary.GetManaShield(), "Debug: Cast Mana Shield", "Debug: Cancel Mana Shield", "Starts target selection and casts the current debug Mana Shield spell through Magic Framework.", "Cleanly ends the maintained Mana Shield spell.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateHealGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetHeal(), "Debug: Cast Heal", "Starts target selection and casts the current debug Heal spell through Magic Framework.", "UI/Commands/Tend");
    }

    public static Gizmo CreateRegenerationGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetRegeneration(), "Debug: Cast Regeneration", "Starts target selection and casts the current debug Regeneration spell through Magic Framework.", "UI/Commands/Tend");
    }

    public static Gizmo CreateSummonDogGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetSummonDog(), "Debug: Cast Summon Dog", "Starts target selection and casts the current debug Summon Dog spell through Magic Framework.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateCreateFoodGizmo(Pawn pawn)
    {
        return CreateSpellGizmo(pawn, SpellDebugSpellLibrary.GetCreateFood(), "Debug: Cast Create Food", "Starts target selection and casts the current debug Create Food spell through Magic Framework.", "UI/Commands/DesirePower");
    }

    public static Gizmo CreateCasterLevelGizmo(Pawn pawn)
    {
        int currentLevel = SpellRuntimeGameComponent.Instance?.GetCasterLevel(pawn) ?? 0;
        return new Command_Action
        {
            defaultLabel = $"Debug: Caster Level {currentLevel}",
            defaultDesc = "Cycles the Magic Framework caster level used by spell power scaling and level requirements.",
            icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true),
            action = () =>
            {
                int newLevel = SpellRuntimeGameComponent.Instance?.CycleDebugCasterLevel(pawn) ?? 0;
                Messages.Message($"Magic Framework caster level set to {newLevel}.", pawn, MessageTypeDefOf.TaskCompletion, false);
            }
        };
    }

    public static Gizmo CreateArcaneGiftGizmo(Pawn pawn)
    {
        bool hasGift = SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) == true;
        return new Command_Action
        {
            defaultLabel = hasGift ? "Debug: Arcane Gift On" : "Debug: Arcane Gift Off",
            defaultDesc = "Toggles the Magic Framework Arcane gift metadata for this pawn.",
            icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true),
            action = () =>
            {
                SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
                bool newValue = runtime?.HasArcaneGift(pawn) != true;
                runtime?.SetArcaneGift(pawn, newValue);
                Messages.Message($"Magic Framework Arcane gift {(newValue ? "enabled" : "disabled")} for {pawn.LabelShortCap}.", pawn, MessageTypeDefOf.TaskCompletion, false);
            }
        };
    }

    public static Gizmo CreateEnhancementDiagnosticsGizmo(Pawn pawn)
    {
        return new Command_Action
        {
            defaultLabel = "Debug: Spell Enhancements",
            defaultDesc = "Logs active enhancement rules and final modifier factors for a selected spell.",
            icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true),
            action = () => OpenEnhancementDiagnosticsMenu(pawn)
        };
    }

    private static void OpenEnhancementDiagnosticsMenu(Pawn pawn)
    {
        List<SpellDef> spells = DefDatabase<SpellDef>.AllDefsListForReading.ListFullCopy();
        spells.Sort((left, right) => string.Compare(left.LabelCap, right.LabelCap, System.StringComparison.OrdinalIgnoreCase));

        List<FloatMenuOption> options = new();
        for (int i = 0; i < spells.Count; i++)
        {
            SpellDef spellDef = spells[i];
            if (spellDef == null)
            {
                continue;
            }

            options.Add(new FloatMenuOption(spellDef.LabelCap, () =>
            {
                SpellDebugUtility.LogSpellEnhancementReport(pawn, spellDef);
                Messages.Message($"Logged enhancement report for {spellDef.LabelCap}.", pawn, MessageTypeDefOf.TaskCompletion, false);
            }));
        }

        if (options.Count == 0)
        {
            options.Add(new FloatMenuOption("No Magic Framework spells loaded", null));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private static Gizmo CreateSpellGizmo(Pawn pawn, SpellDef spellDef, string label, string description, string fallbackIconPath, System.Action<Pawn> beginTargeting = null)
    {
        Command_Action command = new()
        {
            defaultLabel = label,
            defaultDesc = description,
            icon = ResolveSpellIcon(spellDef, fallbackIconPath),
            action = () =>
            {
                if (beginTargeting != null)
                {
                    beginTargeting(pawn);
                    return;
                }

                BeginSpellTargeting(pawn, spellDef);
            }
        };

        ApplyCooldownDisabledState(command, pawn, spellDef);
        return command;
    }

    private static Gizmo CreateMaintainedSpellGizmo(Pawn pawn, SpellDef spellDef, string castLabel, string cancelLabel, string castDescription, string cancelDescription, string fallbackIconPath)
    {
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime?.HasActiveMaintainedSpell(pawn, spellDef) == true)
        {
            return new Command_Action
            {
                defaultLabel = cancelLabel,
                defaultDesc = cancelDescription,
                icon = ResolveSpellIcon(spellDef, fallbackIconPath),
                action = () =>
                {
                    int cancelledCount = runtime.CancelMaintainedSpell(pawn, spellDef, false);
                    MessageTypeDef messageType = cancelledCount > 0 ? MessageTypeDefOf.TaskCompletion : MessageTypeDefOf.RejectInput;
                    Messages.Message(cancelledCount > 0 ? $"Cancelled {spellDef.LabelCap}." : $"{spellDef.LabelCap} is not active.", pawn, messageType, false);
                }
            };
        }

        return CreateSpellGizmo(pawn, spellDef, castLabel, castDescription, fallbackIconPath);
    }

    private static void ApplyCooldownDisabledState(Command_Action command, Pawn pawn, SpellDef spellDef)
    {
        int cooldownTicks = SpellRuntimeGameComponent.Instance?.GetCooldownRemainingTicks(pawn, spellDef) ?? 0;
        if (cooldownTicks <= 0)
        {
            return;
        }

        command.Disable($"Cooldown: {cooldownTicks.ToStringTicksToPeriod()} remaining.");
    }

    private static Texture2D ResolveSpellIcon(SpellDef spellDef, string fallbackIconPath)
    {
        if (!string.IsNullOrWhiteSpace(spellDef?.gizmoIconPath))
        {
            Texture2D authoredIcon = ContentFinder<Texture2D>.Get(spellDef.gizmoIconPath, false);
            if (authoredIcon != null)
            {
                return authoredIcon;
            }

            Log.Warning($"[MagicFramework] Could not load gizmo icon '{spellDef.gizmoIconPath}' for {spellDef.defName ?? "<unknown spell>"}.");
        }

        return ContentFinder<Texture2D>.Get(fallbackIconPath, true);
    }

    public static void BeginFireboltTargeting(Pawn pawn)
    {
        if (!CanDebugCastFromPawn(pawn))
        {
            Messages.Message("Select a spawned colonist or controllable pawn to debug-cast Firebolt.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        BeginSpellTargeting(pawn, SpellDebugSpellLibrary.GetFirebolt());
    }

    private static void TryCastSpell(Pawn pawn, SpellDef spellDef, LocalTargetInfo target)
    {
        if (pawn == null || spellDef == null)
        {
            Messages.Message("Magic Framework debug cast could not start because the pawn or spell was missing.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        if (Executor.TryExecute(spellDef, pawn, target, out SpellContext context))
        {
            string sourceLabel = spellDef.defName != null && spellDef.defName.EndsWith("_DebugFallback") ? "fallback" : "authored";
            Messages.Message($"Magic Framework cast {spellDef.label ?? spellDef.defName} using {sourceLabel} debug content.", target.ToTargetInfo(pawn.Map), MessageTypeDefOf.TaskCompletion, false);
            return;
        }

        string reason = context?.executionState?.failed == true
            ? "Spell validation or execution failed. Check the log for details."
            : "Spell cast did not complete.";
        Messages.Message(reason, target.ToTargetInfo(pawn.Map), MessageTypeDefOf.RejectInput, false);
    }

    private static void BeginSpellTargeting(Pawn pawn, SpellDef spellDef)
    {
        Find.Targeter.BeginTargeting(
            BuildTargetingParameters(pawn, spellDef),
            target => TryCastSpell(pawn, spellDef, target),
            pawn,
            null,
            null,
            false);
    }

    private static TargetingParameters BuildTargetingParameters(Pawn caster, SpellDef spellDef)
    {
        SpellTargetingDef targeting = spellDef?.targeting ?? new SpellTargetingDef();
        TargetingParameters parameters = targeting.primaryTargetType == SpellPrimaryTargetType.Cell
            ? TargetingParameters.ForCell()
            : TargetingParameters.ForThing();

        parameters.canTargetLocations = targeting.primaryTargetType == SpellPrimaryTargetType.Cell
            || targeting.primaryTargetType == SpellPrimaryTargetType.PawnOrCell;
        parameters.canTargetPawns = targeting.includePawns;
        parameters.canTargetBuildings = targeting.includeBuildings;
        parameters.canTargetItems = targeting.includeItems;
        parameters.canTargetAnimals = targeting.includePawns;
        parameters.canTargetHumans = targeting.includePawns;
        parameters.canTargetMechs = targeting.includePawns;
        parameters.canTargetSelf = targeting.allowSelfTarget;

        if (targeting.pawnAffinity == SpellPawnAffinity.Ally)
        {
            parameters.neverTargetHostileFaction = true;
        }

        parameters.validator = targetInfo => IsValidDebugTarget(caster, spellDef, targeting, targetInfo);

        return parameters;
    }

    private static bool IsValidDebugTarget(Pawn caster, SpellDef spellDef, SpellTargetingDef targeting, TargetInfo targetInfo)
    {
        LocalTargetInfo localTarget = targetInfo.Thing != null
            ? new LocalTargetInfo(targetInfo.Thing)
            : new LocalTargetInfo(targetInfo.Cell);

        if (!localTarget.IsValid)
        {
            return false;
        }

        Thing targetThing = localTarget.Thing;
        bool hasThing = targetThing != null;
        bool isPawn = targetThing is Pawn;

        if (!MatchesPrimaryTargetType(targeting.primaryTargetType, hasThing, isPawn))
        {
            return false;
        }

        if (!MatchesCategoryFilters(targeting, targetThing, hasThing, isPawn))
        {
            return false;
        }

        if (!targeting.allowSelfTarget && hasThing && caster != null && targetThing == caster)
        {
            return false;
        }

        if (isPawn && !MatchesPawnAffinity(caster, targetThing, targeting.pawnAffinity))
        {
            return false;
        }

        IntVec3 targetCell = localTarget.Cell;
        if (!targetCell.IsValid)
        {
            return false;
        }

        SpellContext rangeContext = new()
        {
            caster = caster,
            map = caster?.Map,
            spellDef = spellDef,
            power = SpellPowerUtility.ComputePower(spellDef, caster)
        };
        float range = SpellPowerUtility.ResolveScalableFloat(rangeContext, targeting.range, targeting.scalableRange);
        if (caster != null && range > 0f && caster.Position.DistanceTo(targetCell) > range)
        {
            return false;
        }

        if (targeting.requireLineOfSight && !HasLineOfSight(caster, localTarget))
        {
            return false;
        }

        if (RequiresCellValidation(targeting.primaryTargetType))
        {
            if (caster?.Map == null)
            {
                return false;
            }

            if (targeting.requireStandableCell && !targetCell.Standable(caster.Map))
            {
                return false;
            }

            if (targeting.requireWalkableCell && !targetCell.Walkable(caster.Map))
            {
                return false;
            }

            if (targeting.requireWaterCell && !SpellTerrainUtility.IsWaterCell(caster.Map, targetCell))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesPrimaryTargetType(SpellPrimaryTargetType primaryTargetType, bool hasThing, bool isPawn)
    {
        switch (primaryTargetType)
        {
            case SpellPrimaryTargetType.Cell:
                return !hasThing;
            case SpellPrimaryTargetType.Pawn:
                return isPawn;
            case SpellPrimaryTargetType.Thing:
                return hasThing;
            case SpellPrimaryTargetType.PawnOrThing:
                return hasThing;
            case SpellPrimaryTargetType.PawnOrCell:
                return !hasThing || isPawn;
            default:
                return false;
        }
    }

    private static bool MatchesCategoryFilters(SpellTargetingDef targeting, Thing targetThing, bool hasThing, bool isPawn)
    {
        if (!hasThing)
        {
            return true;
        }

        if (isPawn)
        {
            return targeting.includePawns;
        }

        return targetThing.def.category switch
        {
            ThingCategory.Building => targeting.includeBuildings,
            ThingCategory.Item => targeting.includeItems,
            _ => true
        };
    }

    private static bool MatchesPawnAffinity(Pawn caster, Thing targetThing, SpellPawnAffinity pawnAffinity)
    {
        if (pawnAffinity == SpellPawnAffinity.All)
        {
            return true;
        }

        if (caster == null)
        {
            return false;
        }

        if (targetThing is not Pawn)
        {
            return true;
        }

        Faction casterFaction = caster.Faction;
        Faction targetFaction = targetThing.Faction;
        bool sameFaction = casterFaction != null && targetFaction != null && casterFaction == targetFaction;
        bool hostile = casterFaction != null && targetFaction != null && casterFaction.HostileTo(targetFaction);

        switch (pawnAffinity)
        {
            case SpellPawnAffinity.Ally:
                return targetThing == caster || sameFaction;
            case SpellPawnAffinity.Foe:
                return hostile;
            default:
                return true;
        }
    }

    private static bool RequiresCellValidation(SpellPrimaryTargetType primaryTargetType)
    {
        switch (primaryTargetType)
        {
            case SpellPrimaryTargetType.Cell:
            case SpellPrimaryTargetType.PawnOrCell:
                return true;
            default:
                return false;
        }
    }

    private static bool HasLineOfSight(Pawn caster, LocalTargetInfo target)
    {
        if (caster?.Map == null || !target.IsValid)
        {
            return false;
        }

        if (target.Thing != null)
        {
            CellRect startRect = caster.OccupiedRect();
            CellRect endRect = target.Thing.OccupiedRect();
            return GenSight.LineOfSight(caster.Position, target.Cell, caster.Map, startRect, endRect);
        }

        return GenSight.LineOfSight(caster.Position, target.Cell, caster.Map);
    }

    private static bool CanDebugCastFromPawn(Pawn pawn)
    {
        return pawn != null
            && pawn.Spawned
            && pawn.Map != null
            && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony);
    }
}
