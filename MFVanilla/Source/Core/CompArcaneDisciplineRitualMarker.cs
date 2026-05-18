using System.Collections.Generic;
using System.Linq;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MFVanilla.Core;

public sealed class CompProperties_ArcaneDisciplineRitualMarker : CompProperties
{
    public string commandLabel = "Embrace discipline";
    public string commandDescription = "Begin a short rite for an Arcane Gift pawn to embrace an available Arcane Discipline.";
    public string commandIconPath = "UI/Commands/DesirePower";

    public CompProperties_ArcaneDisciplineRitualMarker()
    {
        compClass = typeof(CompArcaneDisciplineRitualMarker);
    }
}

public sealed class CompArcaneDisciplineRitualMarker : ThingComp
{
    private const int RitualTicks = 600;
    private CompProperties_ArcaneDisciplineRitualMarker Props => (CompProperties_ArcaneDisciplineRitualMarker)props;
    private Pawn activePawn;
    private ArcaneDisciplineDef activeDiscipline;

    public Pawn ActivePawn => activePawn;
    public ArcaneDisciplineDef ActiveDiscipline => activeDiscipline;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref activePawn, "activePawn");
        Scribe_Defs.Look(ref activeDiscipline, "activeDiscipline");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        ClearStaleActiveRitual();
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        Command_Action command = new()
        {
            defaultLabel = Props.commandLabel,
            defaultDesc = Props.commandDescription,
            icon = ContentFinder<Texture2D>.Get(Props.commandIconPath, false),
            action = OpenDialog
        };

        if (activePawn != null)
        {
            command.Disable($"{activePawn.LabelShortCap} is already using this marker.");
        }

        yield return command;
    }

    public bool TryStartRitual(Pawn pawn, ArcaneDisciplineDef discipline)
    {
        ClearStaleActiveRitual();
        if (activePawn != null)
        {
            Messages.Message($"{activePawn.LabelShortCap} is already using this marker.", parent, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        AcceptanceReport pawnReport = CanPawnUseMarker(pawn);
        if (!pawnReport.Accepted)
        {
            Messages.Message(pawnReport.Reason, parent, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        if (!ArcaneDisciplineUtility.CanPawnEmbraceDiscipline(pawn, discipline, out string reason))
        {
            Messages.Message(reason, pawn, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        JobDef jobDef = DefDatabase<JobDef>.GetNamedSilentFail("MFV_EmbraceArcaneDiscipline");
        if (jobDef == null)
        {
            Messages.Message("The arcane discipline ritual job is missing.", parent, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        activePawn = pawn;
        activeDiscipline = discipline;

        Job job = JobMaker.MakeJob(jobDef, parent);
        job.locomotionUrgency = LocomotionUrgency.Walk;
        job.playerForced = true;

        if (pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc))
        {
            Messages.Message($"{pawn.LabelShortCap} begins embracing {discipline.LabelCap}.", parent, MessageTypeDefOf.PositiveEvent, false);
            return true;
        }

        ClearActiveRitual();
        Messages.Message($"{pawn.LabelShortCap} could not start the discipline rite.", pawn, MessageTypeDefOf.RejectInput, false);
        return false;
    }

    public void FinishRitual(Pawn pawn)
    {
        if (pawn == null || pawn != activePawn || activeDiscipline == null)
        {
            ClearActiveRitual();
            return;
        }

        if (!ArcaneDisciplineUtility.CanPawnEmbraceDiscipline(pawn, activeDiscipline, out string reason))
        {
            Messages.Message(reason, pawn, MessageTypeDefOf.RejectInput, false);
            ClearActiveRitual();
            return;
        }

        SpellRuntimeGameComponent.Instance?.SetArcaneDiscipline(pawn, activeDiscipline);
        Messages.Message($"{pawn.LabelShortCap} embraces {activeDiscipline.LabelCap}.", pawn, MessageTypeDefOf.PositiveEvent, false);
        ClearActiveRitual();
    }

    public void ClearActiveRitual()
    {
        activePawn = null;
        activeDiscipline = null;
    }

    public AcceptanceReport CanPawnUseMarker(Pawn pawn)
    {
        if (pawn == null)
        {
            return "No pawn selected.";
        }

        if (pawn.Dead || pawn.Downed || !pawn.Spawned || pawn.InMentalState)
        {
            return $"{pawn.LabelShortCap} cannot perform the rite.";
        }

        if (pawn.Faction != Faction.OfPlayer)
        {
            return $"{pawn.LabelShortCap} is not part of the colony.";
        }

        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
        {
            return $"{pawn.LabelShortCap} cannot reach the marker.";
        }

        if (!pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
        {
            return $"{pawn.LabelShortCap} cannot reach and reserve the marker.";
        }

        if (SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) != true)
        {
            return $"{pawn.LabelShortCap} does not have the Arcane gift.";
        }

        return true;
    }

    public IEnumerable<Pawn> PawnCandidates()
    {
        return parent.Map?.mapPawns?.FreeColonistsSpawned ?? Enumerable.Empty<Pawn>();
    }

    public IEnumerable<ArcaneDisciplineDef> DisciplineCandidates()
    {
        return DefDatabase<ArcaneDisciplineDef>.AllDefsListForReading
            .OrderBy(def => def.displayOrder)
            .ThenBy(def => def.label);
    }

    public int RitualTicksForJob() => RitualTicks;

    private void OpenDialog()
    {
        ClearStaleActiveRitual();
        if (parent.Map == null)
        {
            Messages.Message("The marker is not on a map.", parent, MessageTypeDefOf.RejectInput, false);
            return;
        }

        Find.WindowStack.Add(new Dialog_ArcaneDisciplineRitual(this));
    }

    private void ClearStaleActiveRitual()
    {
        if (activePawn == null)
        {
            activeDiscipline = null;
            return;
        }

        Job currentJob = activePawn.CurJob;
        if (currentJob?.def?.defName == "MFV_EmbraceArcaneDiscipline"
            && currentJob.GetTarget(TargetIndex.A).Thing == parent)
        {
            return;
        }

        ClearActiveRitual();
    }
}
