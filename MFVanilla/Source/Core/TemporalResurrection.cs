using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework.Actions;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.PawnMemory;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class TemporalResurrectionActionDef : SpellActionDef
{
    public string hediffDef = "MF_TemporalReconstruction";
    public float baseSkillLossFraction = 0.24f;
    public float skillLossReductionPerCasterLevel = 0.007f;
    public float minSkillLossFraction = 0.06f;
    public int pulseIntervalTicks = 90;
    public float healingPerPulse = 4f;

    public override SpellActionWorker CreateWorker() => new TemporalResurrectionActionWorker();
}

public sealed class TemporalResurrectionActionWorker : SpellActionWorker
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        TemporalResurrectionActionDef temporalDef = actionDef as TemporalResurrectionActionDef;
        if (temporalDef == null)
        {
            return;
        }

        if (!SpellResurrectionUtility.TryResurrect(
                context.currentTarget,
                removeResurrectionSickness: true,
                preserveNonVitalDamage: true,
                updatePawnMemory: true,
                despawnActiveSpirit: true,
                out Pawn resurrectedPawn,
                out string reason))
        {
            Log.Warning($"[MFVanilla] Temporal resurrection failed: {reason}");
            return;
        }

        context.SetCurrentTarget(new LocalTargetInfo(resurrectedPawn));
        HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(temporalDef.hediffDef);
        if (hediffDef == null)
        {
            Log.Warning($"[MFVanilla] Temporal resurrection hediff '{temporalDef.hediffDef}' could not be resolved.");
            return;
        }

        int casterLevel = context.caster is Pawn caster
            ? SpellRuntimeGameComponent.Instance?.GetCasterLevel(caster) ?? 0
            : 0;
        float lossFraction = Mathf.Max(
            temporalDef.minSkillLossFraction,
            temporalDef.baseSkillLossFraction - casterLevel * temporalDef.skillLossReductionPerCasterLevel);

        Hediff_TemporalReconstruction hediff = HediffMaker.MakeHediff(hediffDef, resurrectedPawn) as Hediff_TemporalReconstruction;
        if (hediff == null)
        {
            Log.Warning("[MFVanilla] Temporal resurrection hediff was not Hediff_TemporalReconstruction.");
            return;
        }

        hediff.Initialize(lossFraction, temporalDef.pulseIntervalTicks, temporalDef.healingPerPulse);
        resurrectedPawn.health.AddHediff(hediff);
        Messages.Message($"{resurrectedPawn.LabelShortCap} has been pulled back through time.", resurrectedPawn, MessageTypeDefOf.PositiveEvent, false);
    }
}

public sealed class Hediff_TemporalReconstruction : HediffWithComps
{
    private Dictionary<string, int> targetSkillLevels = new();
    private List<string> scarEligiblePartKeys = new();
    private bool initialized;
    private int pulseIntervalTicks = 90;
    private float healingPerPulse = 4f;
    private int nextPulseTick;

    public void Initialize(float skillLossFraction, int intervalTicks, float healingAmount)
    {
        pulseIntervalTicks = Mathf.Max(30, intervalTicks);
        healingPerPulse = Mathf.Max(0.5f, healingAmount);
        nextPulseTick = Find.TickManager.TicksGame + pulseIntervalTicks;
        targetSkillLevels = BuildSkillTargets(pawn, skillLossFraction);
        scarEligiblePartKeys = BuildScarEligiblePartKeys(pawn);
        initialized = true;
    }

    public override void Tick()
    {
        base.Tick();
        if (!initialized)
        {
            Initialize(0.12f, pulseIntervalTicks, healingPerPulse);
        }

        if (pawn == null || pawn.Dead || pawn.Destroyed)
        {
            return;
        }

        int ticksGame = Find.TickManager.TicksGame;
        if (ticksGame < nextPulseTick)
        {
            return;
        }

        nextPulseTick = ticksGame + pulseIntervalTicks;
        bool changedHealth = ReverseOneHealthStep();
        bool changedSkill = RewindOneSkillPoint();
        if (changedHealth || changedSkill)
        {
            FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastAreaEffect, 0.7f);
            pawn.health?.Notify_HediffChanged(this);
        }

        if (!HasMoreHealthToReverse() && !HasMoreSkillLoss())
        {
            pawn.health.RemoveHediff(this);
        }
    }

    public override string LabelInBrackets => ProgressLabel();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref targetSkillLevels, "targetSkillLevels", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref scarEligiblePartKeys, "scarEligiblePartKeys", LookMode.Value);
        Scribe_Values.Look(ref initialized, "initialized");
        Scribe_Values.Look(ref pulseIntervalTicks, "pulseIntervalTicks", 90);
        Scribe_Values.Look(ref healingPerPulse, "healingPerPulse", 4f);
        Scribe_Values.Look(ref nextPulseTick, "nextPulseTick");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            targetSkillLevels ??= new Dictionary<string, int>();
            scarEligiblePartKeys ??= new List<string>();
            if (nextPulseTick <= 0)
            {
                nextPulseTick = Find.TickManager.TicksGame + pulseIntervalTicks;
            }
        }
    }

    private static Dictionary<string, int> BuildSkillTargets(Pawn pawn, float lossFraction)
    {
        Dictionary<string, int> targets = new();
        if (pawn?.skills?.skills == null)
        {
            return targets;
        }

        List<SkillRecord> skills = pawn.skills.skills.Where(skill => skill?.def != null && skill.Level > 0).ToList();
        int totalLevels = skills.Sum(skill => skill.Level);
        if (totalLevels <= 0)
        {
            return targets;
        }

        int totalLoss = Mathf.Clamp(Mathf.RoundToInt(totalLevels * Mathf.Clamp01(lossFraction)), 1, totalLevels);
        Dictionary<SkillDef, int> losses = skills.ToDictionary(skill => skill.def, _ => 0);
        List<(SkillRecord skill, float remainder)> remainders = new();
        int assigned = 0;

        foreach (SkillRecord skill in skills)
        {
            float exactLoss = totalLoss * (skill.Level / (float)totalLevels);
            int loss = Mathf.FloorToInt(exactLoss);
            loss = Mathf.Min(loss, skill.Level);
            losses[skill.def] = loss;
            assigned += loss;
            remainders.Add((skill, exactLoss - loss));
        }

        foreach ((SkillRecord skill, _) in remainders.OrderByDescending(item => item.remainder))
        {
            if (assigned >= totalLoss)
            {
                break;
            }

            if (losses[skill.def] < skill.Level)
            {
                losses[skill.def]++;
                assigned++;
            }
        }

        foreach (SkillRecord skill in skills)
        {
            targets[skill.def.defName] = Mathf.Max(0, skill.Level - losses[skill.def]);
        }

        return targets;
    }

    private static List<string> BuildScarEligiblePartKeys(Pawn pawn)
    {
        HashSet<string> keys = new();
        List<Hediff> hediffs = pawn?.health?.hediffSet?.hediffs;
        if (hediffs == null)
        {
            return new List<string>();
        }

        foreach (Hediff hediff in hediffs)
        {
            if (hediff.Part == null)
            {
                continue;
            }

            if (hediff is Hediff_MissingPart || hediff is Hediff_Injury injury && !injury.IsPermanent())
            {
                keys.Add(BodyPartKey(hediff.Part));
            }
        }

        return keys.ToList();
    }

    private bool ReverseOneHealthStep()
    {
        Hediff_MissingPart missingPart = FindRestorableMissingPart();
        if (missingPart != null)
        {
            BodyPartRecord part = missingPart.Part;
            HediffDef injuryDef = missingPart.lastInjury ?? HediffDefOf.Cut;
            pawn.health.RemoveHediff(missingPart);
            Hediff_Injury injury = HediffMaker.MakeHediff(injuryDef, pawn, part) as Hediff_Injury;
            if (injury != null)
            {
                injury.Severity = Mathf.Max(1f, part.def.GetMaxHealth(pawn) * 0.35f);
                pawn.health.AddHediff(injury, part);
            }

            return true;
        }

        Hediff_Injury worstInjury = FindWorstInjury();
        if (worstInjury == null)
        {
            return false;
        }

        worstInjury.Severity -= healingPerPulse;
        if (worstInjury.Severity <= 0.01f)
        {
            pawn.health.RemoveHediff(worstInjury);
        }

        return true;
    }

    private bool RewindOneSkillPoint()
    {
        if (pawn?.skills?.skills == null || targetSkillLevels.NullOrEmpty())
        {
            return false;
        }

        SkillRecord skill = pawn.skills.skills
            .Where(record => record?.def != null && targetSkillLevels.TryGetValue(record.def.defName, out int target) && record.Level > target)
            .OrderByDescending(record => record.Level - targetSkillLevels[record.def.defName])
            .ThenByDescending(record => record.Level)
            .FirstOrDefault();
        if (skill == null)
        {
            return false;
        }

        skill.Level = Mathf.Max(targetSkillLevels[skill.def.defName], skill.Level - 1);
        skill.xpSinceLastLevel = 0f;
        skill.xpSinceMidnight = 0f;
        return true;
    }

    private Hediff_MissingPart FindRestorableMissingPart()
    {
        if (pawn?.health?.hediffSet?.hediffs == null)
        {
            return null;
        }

        return pawn.health.hediffSet.hediffs
            .OfType<Hediff_MissingPart>()
            .Where(part => part.Part != null && !IsVitalPart(part.Part))
            .OrderByDescending(part => part.Part.depth)
            .FirstOrDefault();
    }

    private Hediff_Injury FindWorstInjury()
    {
        if (pawn?.health?.hediffSet?.hediffs == null)
        {
            return null;
        }

        return pawn.health.hediffSet.hediffs
            .OfType<Hediff_Injury>()
            .Where(IsReversibleInjury)
            .OrderBy(injury => injury.IsPermanent())
            .ThenByDescending(injury => injury.Severity)
            .FirstOrDefault();
    }

    private bool HasMoreHealthToReverse()
    {
        return FindRestorableMissingPart() != null || FindWorstInjury() != null;
    }

    private bool HasMoreSkillLoss()
    {
        if (pawn?.skills?.skills == null || targetSkillLevels.NullOrEmpty())
        {
            return false;
        }

        return pawn.skills.skills.Any(skill =>
            skill?.def != null
            && targetSkillLevels.TryGetValue(skill.def.defName, out int target)
            && skill.Level > target);
    }

    private bool IsVitalPart(BodyPartRecord part)
    {
        BodyPartRecord core = pawn.RaceProps?.body?.corePart;
        return part == core || part.def?.defName == "Head" || part.def?.defName == "Brain";
    }

    private bool IsReversibleInjury(Hediff_Injury injury)
    {
        if (injury?.Part == null || injury.Severity <= 0f)
        {
            return false;
        }

        return !injury.IsPermanent() || scarEligiblePartKeys.Contains(BodyPartKey(injury.Part));
    }

    private static string BodyPartKey(BodyPartRecord part)
    {
        if (part == null)
        {
            return string.Empty;
        }

        List<string> path = new();
        BodyPartRecord current = part;
        while (current != null)
        {
            path.Add(current.def?.defName ?? current.Label);
            current = current.parent;
        }

        path.Reverse();
        return string.Join("/", path);
    }

    private string ProgressLabel()
    {
        int pendingSkill = 0;
        if (pawn?.skills?.skills != null && !targetSkillLevels.NullOrEmpty())
        {
            pendingSkill = pawn.skills.skills.Sum(skill =>
                skill?.def != null && targetSkillLevels.TryGetValue(skill.def.defName, out int target)
                    ? Mathf.Max(0, skill.Level - target)
                    : 0);
        }

        int injuries = pawn?.health?.hediffSet?.hediffs?.OfType<Hediff_Injury>().Count(IsReversibleInjury) ?? 0;
        int missing = pawn?.health?.hediffSet?.hediffs?.OfType<Hediff_MissingPart>().Count(part => part.Part != null && !IsVitalPart(part.Part)) ?? 0;
        return $"{injuries + missing} wounds, {pendingSkill} skill";
    }
}
