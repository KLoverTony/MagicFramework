using System.Linq;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.PawnLifecycle;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MagicFramework.Scheduling;

/// <summary>
/// Spawns and registers temporary summoned pawns for authored spell actions.
/// </summary>
public sealed class SummonedPawnService
{
    public void CreateSummonedPawn(SpellContext context, SummonPawnActionDef summonPawnActionDef)
    {
        if (context?.map == null || summonPawnActionDef == null)
        {
            return;
        }

        PawnKindDef pawnKindDef = ResolvePawnKindDef(summonPawnActionDef.pawnKindDef);
        if (pawnKindDef == null)
        {
            Log.Warning($"[MagicFramework] SummonPawnActionWorker could not resolve pawn kind '{summonPawnActionDef.pawnKindDef ?? "<null>"}'.");
            return;
        }

        if (!TryResolveSpawnCell(context.currentCell, context.map, out IntVec3 spawnCell))
        {
            Log.Warning($"[MagicFramework] SummonPawnActionWorker could not find a valid spawn cell near {context.currentCell}.");
            return;
        }

        Faction faction = summonPawnActionDef.setFactionToPlayer
            ? Faction.OfPlayer
            : context.caster?.Faction ?? Faction.OfPlayer;
        PawnGenerationRequest request = new(
            pawnKindDef,
            faction,
            PawnGenerationContext.NonPlayer,
            context.map.Tile);
        Pawn summonedPawn = PawnGenerator.GeneratePawn(request);
        if (summonedPawn == null)
        {
            Log.Warning($"[MagicFramework] SummonPawnActionWorker failed to generate pawn kind '{pawnKindDef.defName}'.");
            return;
        }

        GenSpawn.Spawn(summonedPawn, spawnCell, context.map);
        if (faction != null && summonedPawn.Faction != faction)
        {
            summonedPawn.SetFaction(faction);
        }

        ApplyTraining(summonedPawn, context.caster as Pawn, summonPawnActionDef);
        AssignMaster(summonedPawn, context.caster as Pawn, summonPawnActionDef);
        AssignDefensiveDuty(summonedPawn, context.caster as Pawn, summonPawnActionDef);

        int durationTicks = ResolveDurationTicks(context, summonPawnActionDef);
        SpellActionPathUtility.TryCreatePath(context.spellDef, summonPawnActionDef, out var actionPath);
        SummonedPawnRecord record = new(
            context.caster,
            context.spellDef,
            summonedPawn,
            (Find.TickManager?.TicksGame ?? 0) + durationTicks,
            actionPath);
        SummonedPawnMapComponent component = context.map.GetComponent<SummonedPawnMapComponent>();
        component?.Register(record, summonPawnActionDef.replaceExistingForCaster);

        context.SetCurrentTarget(new LocalTargetInfo(summonedPawn));
        component?.RunLifecycleActions(record, SummonedPawnLifecycleEvent.Create);
        MagicLog.Message(MagicLogSubsystem.Summons, $"[MagicFramework] Summoned {summonedPawn.LabelCap} at {spawnCell} for {durationTicks} ticks.");
    }

    private static int ResolveDurationTicks(SpellContext context, SummonPawnActionDef summonPawnActionDef)
    {
        int durationTicks = SpellEnhancementUtility.ResolveScalableDurationTicks(context, summonPawnActionDef.durationTicks, summonPawnActionDef.scalableDurationTicks);
        return durationTicks > 0 ? durationTicks : 1;
    }

    private static PawnKindDef ResolvePawnKindDef(string pawnKindDefName)
    {
        if (string.IsNullOrWhiteSpace(pawnKindDefName))
        {
            return null;
        }

        PawnKindDef pawnKindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(pawnKindDefName);
        if (pawnKindDef != null)
        {
            return pawnKindDef;
        }

        ThingDef raceDef = DefDatabase<ThingDef>.GetNamedSilentFail(pawnKindDefName);
        if (raceDef == null)
        {
            return null;
        }

        return DefDatabase<PawnKindDef>.AllDefsListForReading.FirstOrDefault(candidate => candidate.race == raceDef);
    }

    private static bool TryResolveSpawnCell(IntVec3 centerCell, Map map, out IntVec3 spawnCell)
    {
        if (IsValidSpawnCell(centerCell, map))
        {
            spawnCell = centerCell;
            return true;
        }

        foreach (IntVec3 candidate in GenRadial.RadialCellsAround(centerCell, 2.9f, true))
        {
            if (IsValidSpawnCell(candidate, map))
            {
                spawnCell = candidate;
                return true;
            }
        }

        spawnCell = IntVec3.Invalid;
        return false;
    }

    private static bool IsValidSpawnCell(IntVec3 cell, Map map)
    {
        return cell.IsValid
            && cell.InBounds(map)
            && cell.Walkable(map)
            && cell.Standable(map)
            && cell.GetFirstPawn(map) == null;
    }

    private static void ApplyTraining(Pawn summonedPawn, Pawn trainerPawn, SummonPawnActionDef summonPawnActionDef)
    {
        if (summonedPawn?.training == null || summonPawnActionDef?.trainableDefs == null)
        {
            return;
        }

        for (int i = 0; i < summonPawnActionDef.trainableDefs.Count; i++)
        {
            string trainableDefName = summonPawnActionDef.trainableDefs[i];
            if (string.IsNullOrWhiteSpace(trainableDefName))
            {
                continue;
            }

            TrainableDef trainableDef = DefDatabase<TrainableDef>.GetNamedSilentFail(trainableDefName);
            if (trainableDef == null)
            {
                Log.Warning($"[MagicFramework] Could not resolve trainable def '{trainableDefName}' for summon.");
                continue;
            }

            if (!summonedPawn.training.CanAssignToTrain(trainableDef))
            {
                MagicLog.Message(MagicLogSubsystem.Summons, $"[MagicFramework] Skipped trainable def '{trainableDef.defName}' for {summonedPawn.LabelCap} because this pawn cannot learn it.");
                continue;
            }

            summonedPawn.training.Train(trainableDef, trainerPawn, complete: true);
        }
    }

    private static void AssignMaster(Pawn summonedPawn, Pawn casterPawn, SummonPawnActionDef summonPawnActionDef)
    {
        if (summonedPawn == null || casterPawn == null || summonedPawn.playerSettings == null || summonPawnActionDef == null)
        {
            return;
        }

        bool isMasterBoundLifecyclePawn = PawnLifecycleUtility.TryGetLifecycle(summonedPawn, out PawnLifecycleExtension extension, out _)
            && extension.controlPolicy == PawnLifecycleControlPolicy.MasterBoundMinion;
        if (!isMasterBoundLifecyclePawn
            && (summonedPawn.training == null || !summonedPawn.training.HasLearned(TrainableDefOf.Obedience)))
        {
            return;
        }

        summonedPawn.playerSettings.Master = casterPawn;
        summonedPawn.playerSettings.followDrafted = summonPawnActionDef.followMasterWhileDrafted;
        summonedPawn.playerSettings.followFieldwork = summonPawnActionDef.followMasterWhileFieldwork;
        summonedPawn.GetComp<CompPawnLifecycleEnforcer>()?.AssignMaster(
            casterPawn,
            summonPawnActionDef.followMasterWhileDrafted,
            summonPawnActionDef.followMasterWhileFieldwork);
    }

    private static void AssignDefensiveDuty(Pawn summonedPawn, Pawn casterPawn, SummonPawnActionDef summonPawnActionDef)
    {
        if (summonedPawn?.mindState == null || casterPawn == null || summonPawnActionDef?.joinLord != true)
        {
            return;
        }

        summonedPawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, casterPawn, 8f);
    }
}
