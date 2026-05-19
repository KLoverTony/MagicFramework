using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MFVanilla.Core;

public sealed class CompProperties_FleshGolemRegeneration : CompProperties
{
    public int intervalTicks = 180;
    public float healAmount = 4f;
    public string statusHediffDef = "MF_Regenerating";

    public CompProperties_FleshGolemRegeneration()
    {
        compClass = typeof(CompFleshGolemRegeneration);
    }
}

public sealed class CompFleshGolemRegeneration : ThingComp
{
    private int nextHealTick;

    private CompProperties_FleshGolemRegeneration Props => (CompProperties_FleshGolemRegeneration)props;

    private Pawn ParentPawn => parent as Pawn;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        Pawn pawn = ParentPawn;
        if (pawn == null)
        {
            return;
        }

        EnsureStatusHediff(pawn);
        if (!respawningAfterLoad)
        {
            nextHealTick = Find.TickManager.TicksGame + Rand.Range(30, Props.intervalTicks);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref nextHealTick, "nextHealTick", 0);
    }

    public override void CompTick()
    {
        base.CompTick();
        Pawn pawn = ParentPawn;
        if (pawn == null || pawn.Dead || !pawn.Spawned)
        {
            return;
        }

        EnsureStatusHediff(pawn);
        int currentTick = Find.TickManager.TicksGame;
        if (currentTick < nextHealTick)
        {
            return;
        }

        nextHealTick = currentTick + Props.intervalTicks;
        HealWorstInjury(pawn);
    }

    public override string CompInspectStringExtra()
    {
        return "Regenerates wounded tissue.";
    }

    private void EnsureStatusHediff(Pawn pawn)
    {
        HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(Props.statusHediffDef);
        if (hediffDef == null || pawn.health?.hediffSet == null || pawn.health.hediffSet.HasHediff(hediffDef))
        {
            return;
        }

        pawn.health.AddHediff(hediffDef);
    }

    private void HealWorstInjury(Pawn pawn)
    {
        if (Props.healAmount <= 0f || pawn.health?.hediffSet?.hediffs == null)
        {
            return;
        }

        List<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs
            .OfType<Hediff_Injury>()
            .Where(injury => injury.Severity > 0f && !injury.IsPermanent())
            .OrderByDescending(injury => injury.Severity)
            .ToList();

        if (injuries.Count == 0)
        {
            return;
        }

        injuries[0].Heal(Props.healAmount);
        FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("PsycastAreaEffect");
        if (fleckDef != null)
        {
            FleckMaker.Static(pawn.DrawPos, pawn.Map, fleckDef, 0.65f);
        }
    }
}
