using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class ForbiddenPlagueExtension : DefModExtension
{
    public int lesionIntervalTicks = 6000;
    public int spreadIntervalTicks = 2500;
    public float spreadRadius = 4.9f;
    public float spreadChance = 0.28f;
    public float tendedSpreadChance = 0f;
    public float infectionSeverity = 0.08f;
    public FloatRange lesionSeverityRange = new(1.5f, 3.5f);
    public List<string> lesionHediffDefs = new() { "MF_PlagueLesion", "MF_PlagueBlister" };
    public bool humanlikeOnly = true;
}

public sealed class ForbiddenPlagueHediff : HediffWithComps
{
    private int nextLesionTick;
    private int nextSpreadTick;

    private ForbiddenPlagueExtension Props => def.GetModExtension<ForbiddenPlagueExtension>() ?? new ForbiddenPlagueExtension();

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        if (nextLesionTick <= 0)
        {
            nextLesionTick = currentTick + Rand.Range(900, Mathf.Max(901, Props.lesionIntervalTicks));
        }

        if (nextSpreadTick <= 0)
        {
            nextSpreadTick = currentTick + Rand.Range(600, Mathf.Max(601, Props.spreadIntervalTicks));
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref nextLesionTick, "nextLesionTick", 0);
        Scribe_Values.Look(ref nextSpreadTick, "nextSpreadTick", 0);
    }

    public override void Tick()
    {
        base.Tick();
        if (!CanAct(pawn))
        {
            return;
        }

        int currentTick = Find.TickManager.TicksGame;
        ForbiddenPlagueExtension props = Props;
        if (currentTick >= nextLesionTick)
        {
            nextLesionTick = currentTick + Mathf.Max(250, props.lesionIntervalTicks);
            TryCauseLesion(props);
        }

        if (currentTick >= nextSpreadTick)
        {
            nextSpreadTick = currentTick + Mathf.Max(250, props.spreadIntervalTicks);
            TrySpread(props);
        }
    }

    private void TryCauseLesion(ForbiddenPlagueExtension props)
    {
        if (IsTended())
        {
            return;
        }

        BodyPartRecord part = RandomOuterBodyPart(pawn);
        HediffDef woundDef = RandomWoundDef(props);
        if (part == null || woundDef == null)
        {
            return;
        }

        Hediff wound = HediffMaker.MakeHediff(woundDef, pawn, part);
        wound.Severity = props.lesionSeverityRange.RandomInRange;
        pawn.health.AddHediff(wound, part);
    }

    private void TrySpread(ForbiddenPlagueExtension props)
    {
        if (pawn?.Map == null || props.spreadRadius <= 0f)
        {
            return;
        }

        float spreadChance = IsTended() ? props.tendedSpreadChance : props.spreadChance;
        if (spreadChance <= 0f)
        {
            return;
        }

        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, props.spreadRadius, true))
        {
            Pawn target = thing as Pawn;
            if (!CanInfect(target, props))
            {
                continue;
            }

            if (!Rand.Chance(spreadChance))
            {
                continue;
            }

            Hediff infection = HediffMaker.MakeHediff(def, target);
            infection.Severity = Mathf.Max(props.infectionSeverity, def.initialSeverity);
            target.health.AddHediff(infection);
            FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("PsycastAreaEffect");
            if (fleckDef != null && target.Map != null)
            {
                FleckMaker.Static(target.DrawPos, target.Map, fleckDef, 0.45f);
            }
        }
    }

    private bool CanInfect(Pawn target, ForbiddenPlagueExtension props)
    {
        if (!CanAct(target) || target == pawn || target.health?.hediffSet == null)
        {
            return false;
        }

        if (props.humanlikeOnly && target.RaceProps?.Humanlike != true)
        {
            return false;
        }

        if (target.RaceProps?.IsMechanoid == true || target.health.hediffSet.HasHediff(def))
        {
            return false;
        }

        return true;
    }

    private bool IsTended()
    {
        return GetComp<HediffComp_TendDuration>()?.IsTended == true;
    }

    private static bool CanAct(Pawn target)
    {
        return target?.health != null && !target.Dead && target.Spawned;
    }

    private static BodyPartRecord RandomOuterBodyPart(Pawn target)
    {
        IEnumerable<BodyPartRecord> parts = target.health?.hediffSet?.GetNotMissingParts()
            .Where(part => part?.def != null && !part.def.conceptual && part.depth == BodyPartDepth.Outside);
        return parts?.TryRandomElementByWeight(part => Mathf.Max(0.01f, part.coverage), out BodyPartRecord result) == true
            ? result
            : null;
    }

    private static HediffDef RandomWoundDef(ForbiddenPlagueExtension props)
    {
        if (props?.lesionHediffDefs == null || props.lesionHediffDefs.Count == 0)
        {
            return null;
        }

        List<HediffDef> woundDefs = props.lesionHediffDefs
            .Select(defName => DefDatabase<HediffDef>.GetNamedSilentFail(defName))
            .Where(hediffDef => hediffDef != null)
            .ToList();
        return woundDefs.Count == 0 ? null : woundDefs.RandomElement();
    }
}
