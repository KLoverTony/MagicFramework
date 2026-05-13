using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Creates player-facing spell gizmos for pawns that know framework spells.
/// </summary>
public static class SpellGizmoUtility
{
    private static readonly SpellExecutor Executor = new();

    public static Gizmo CreateKnownSpellsGizmo(Pawn pawn)
    {
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (pawn == null || runtime?.HasArcaneGift(pawn) != true)
        {
            return null;
        }

        List<SpellDef> knownSpells = new();
        foreach (SpellDef spellDef in runtime.GetKnownSpells(pawn))
        {
            if (spellDef != null)
            {
                knownSpells.Add(spellDef);
            }
        }

        if (knownSpells.Count == 0)
        {
            return null;
        }

        knownSpells.Sort((left, right) => string.Compare(left.LabelCap, right.LabelCap, System.StringComparison.OrdinalIgnoreCase));
        return new Command_Action
        {
            defaultLabel = "Spells",
            defaultDesc = "Cast a known spell.",
            icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true),
            action = () => OpenKnownSpellMenu(pawn, knownSpells)
        };
    }

    public static IEnumerable<Gizmo> CreateKnownSpellGizmos(Pawn pawn)
    {
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (pawn == null || runtime?.HasArcaneGift(pawn) != true)
        {
            yield break;
        }

        List<SpellDef> knownSpells = new();
        foreach (SpellDef spellDef in runtime.GetKnownSpells(pawn))
        {
            if (spellDef != null)
            {
                knownSpells.Add(spellDef);
            }
        }

        knownSpells.Sort((left, right) => string.Compare(left.LabelCap, right.LabelCap, System.StringComparison.OrdinalIgnoreCase));
        for (int i = 0; i < knownSpells.Count; i++)
        {
            Gizmo gizmo = CreateKnownSpellGizmo(pawn, knownSpells[i]);
            if (gizmo != null)
            {
                yield return gizmo;
            }
        }
    }

    public static Gizmo CreateKnownSpellGizmo(Pawn pawn, SpellDef spellDef)
    {
        if (pawn == null || spellDef == null)
        {
            return null;
        }

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime?.HasActiveMaintainedSpell(pawn, spellDef) == true)
        {
            return new Command_Action
            {
                defaultLabel = $"Release {spellDef.LabelCap}",
                defaultDesc = $"Safely ends the maintained {spellDef.LabelCap} spell without triggering break effects.",
                icon = ResolveSpellIcon(spellDef),
                action = () => ReleaseMaintainedSpell(pawn, spellDef, runtime)
            };
        }

        Command_Action command = new()
        {
            defaultLabel = spellDef.LabelCap,
            defaultDesc = SpellDescriptionUtility.GetGizmoDescription(spellDef),
            icon = ResolveSpellIcon(spellDef),
            action = () => BeginSpellTargeting(pawn, spellDef)
        };

        ApplyCooldownDisabledState(command, pawn, spellDef);
        return command;
    }

    private static void OpenKnownSpellMenu(Pawn pawn, List<SpellDef> knownSpells)
    {
        List<FloatMenuOption> options = new();
        List<SpellResearchGroup> groups = BuildResearchGroups(knownSpells);
        for (int i = 0; i < groups.Count; i++)
        {
            SpellResearchGroup group = groups[i];
            options.Add(new FloatMenuOption(group.Label, () => OpenSpellGroupMenu(pawn, group.Spells)));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private static void OpenSpellGroupMenu(Pawn pawn, List<SpellDef> knownSpells)
    {
        List<FloatMenuOption> options = new();
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        for (int i = 0; i < knownSpells.Count; i++)
        {
            SpellDef spellDef = knownSpells[i];
            if (runtime?.HasActiveMaintainedSpell(pawn, spellDef) == true)
            {
                options.Add(new FloatMenuOption($"Release {spellDef.LabelCap}", () => ReleaseMaintainedSpell(pawn, spellDef, runtime)));
                continue;
            }

            if (!TryValidateCasterRequirements(pawn, spellDef, out string reason))
            {
                options.Add(new FloatMenuOption($"{spellDef.LabelCap}: {reason}", null));
                continue;
            }

            options.Add(new FloatMenuOption(spellDef.LabelCap, () => BeginSpellTargeting(pawn, spellDef)));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private static List<SpellResearchGroup> BuildResearchGroups(List<SpellDef> knownSpells)
    {
        Dictionary<string, SpellResearchGroup> groupsByKey = new();
        for (int i = 0; i < knownSpells.Count; i++)
        {
            SpellDef spellDef = knownSpells[i];
            string key = GetResearchGroupKey(spellDef, out string label);
            if (!groupsByKey.TryGetValue(key, out SpellResearchGroup group))
            {
                group = new SpellResearchGroup(key, label);
                groupsByKey[key] = group;
            }

            group.Spells.Add(spellDef);
        }

        List<SpellResearchGroup> groups = new(groupsByKey.Values);
        groups.Sort((left, right) => string.Compare(left.Label, right.Label, System.StringComparison.OrdinalIgnoreCase));
        for (int i = 0; i < groups.Count; i++)
        {
            groups[i].Spells.Sort((left, right) => string.Compare(left.LabelCap, right.LabelCap, System.StringComparison.OrdinalIgnoreCase));
        }

        return groups;
    }

    private static string GetResearchGroupKey(SpellDef spellDef, out string label)
    {
        List<ResearchProjectDef> researchPrerequisites = spellDef?.learning?.researchPrerequisites;
        if (researchPrerequisites == null || researchPrerequisites.Count == 0)
        {
            label = "Ungated spells";
            return "<ungated>";
        }

        ResearchProjectDef firstResearch = researchPrerequisites[0];
        if (firstResearch == null)
        {
            label = "Ungated spells";
            return "<ungated>";
        }

        label = firstResearch.LabelCap;
        return firstResearch.defName;
    }

    private static void ReleaseMaintainedSpell(Pawn pawn, SpellDef spellDef, SpellRuntimeGameComponent runtime)
    {
        int cancelledCount = runtime?.CancelMaintainedSpell(pawn, spellDef, false) ?? 0;
        MessageTypeDef messageType = cancelledCount > 0 ? MessageTypeDefOf.TaskCompletion : MessageTypeDefOf.RejectInput;
        Messages.Message(cancelledCount > 0 ? $"Released {spellDef.LabelCap}." : $"{spellDef.LabelCap} is not active.", pawn, messageType, false);
    }

    private static void ApplyCooldownDisabledState(Command_Action command, Pawn pawn, SpellDef spellDef)
    {
        int cooldownTicks = SpellRuntimeGameComponent.Instance?.GetCooldownRemainingTicks(pawn, spellDef) ?? 0;
        if (cooldownTicks > 0)
        {
            command.Disable($"Cooldown: {cooldownTicks.ToStringTicksToPeriod()} remaining.");
        }
    }

    private static Texture2D ResolveSpellIcon(SpellDef spellDef)
    {
        if (!string.IsNullOrWhiteSpace(spellDef.gizmoIconPath))
        {
            Texture2D authoredIcon = ContentFinder<Texture2D>.Get(spellDef.gizmoIconPath, false);
            if (authoredIcon != null)
            {
                return authoredIcon;
            }

            Log.Warning($"[MagicFramework] Could not load gizmo icon '{spellDef.gizmoIconPath}' for {spellDef.defName ?? "<unknown spell>"}.");
        }

        return ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true);
    }

    private static void BeginSpellTargeting(Pawn pawn, SpellDef spellDef)
    {
        if (!CanCastFromPawn(pawn))
        {
            Messages.Message("Select a spawned colonist or controllable pawn to cast this spell.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        if (SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) != true)
        {
            Messages.Message($"{pawn.LabelShortCap} does not have the Arcane gift.", pawn, MessageTypeDefOf.RejectInput, false);
            return;
        }

        if (spellDef?.targeting?.useCasterAsTarget == true)
        {
            TryCastSpell(pawn, spellDef, new LocalTargetInfo(pawn));
            return;
        }

        Find.Targeter.BeginTargeting(
            BuildTargetingParameters(pawn, spellDef),
            target => TryCastSpell(pawn, spellDef, target),
            pawn,
            null,
            null,
            false);
    }

    private static void TryCastSpell(Pawn pawn, SpellDef spellDef, LocalTargetInfo target)
    {
        SpellCastWarmupUtility.StartOrExecute(pawn, spellDef, target, Executor, (completed, context) =>
        {
            if (completed)
            {
                Messages.Message($"Cast {spellDef.LabelCap}.", target.ToTargetInfo(pawn.Map), MessageTypeDefOf.TaskCompletion, false);
                return;
            }

            string reason = context?.executionState?.failed == true
                ? context.executionState.failureReason ?? "Spell validation or execution failed. Check the log for details."
                : "Spell cast did not complete.";
            Messages.Message(reason, target.ToTargetInfo(pawn.Map), MessageTypeDefOf.RejectInput, false);
        });
    }

    private static bool TryValidateCasterRequirements(Pawn pawn, SpellDef spellDef, out string reason)
    {
        reason = null;
        if (pawn == null || spellDef == null)
        {
            reason = "Missing pawn or spell.";
            return false;
        }

        if (SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) != true)
        {
            reason = "Arcane gift required.";
            return false;
        }

        SpellContext context = SpellRequirementUtility.CreatePawnContext(pawn, spellDef);
        return SpellRequirementUtility.CanCastSpell(context, spellDef, out reason, true);
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

        parameters.validator = targetInfo => IsValidTarget(caster, spellDef, targeting, targetInfo);
        return parameters;
    }

    private static bool IsValidTarget(Pawn caster, SpellDef spellDef, SpellTargetingDef targeting, TargetInfo targetInfo)
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

        if (!MatchesPrimaryTargetType(targeting.primaryTargetType, hasThing, isPawn)
            || !MatchesCategoryFilters(targeting, targetThing, hasThing, isPawn))
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
        float range = SpellEnhancementUtility.ResolveScalableRadius(rangeContext, targeting.range, targeting.scalableRange);
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

        if (caster == null || targetThing is not Pawn)
        {
            return false;
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

    private static bool CanCastFromPawn(Pawn pawn)
    {
        return pawn != null
            && pawn.Spawned
            && pawn.Map != null
            && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony);
    }

    private sealed class SpellResearchGroup
    {
        public readonly string Key;
        public readonly string Label;
        public readonly List<SpellDef> Spells = new();

        public SpellResearchGroup(string key, string label)
        {
            Key = key;
            Label = label;
        }
    }
}
