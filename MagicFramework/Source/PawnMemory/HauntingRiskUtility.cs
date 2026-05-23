using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace MagicFramework.PawnMemory;

public static class HauntingRiskUtility
{
    public static void CaptureDeathContext(Pawn pawn, PawnMemoryRecord record, DamageInfo? dinfo, Hediff exactCulprit)
    {
        if (pawn == null || record == null)
            return;

        List<string> reasons = new List<string>();
        int score = 10;
        reasons.Add("base death pressure +10");

        record.moodAtDeath = ResolveMood(pawn);
        record.diedInMentalState = pawn.InMentalState;
        record.deathCulpritHediffDef = exactCulprit?.def?.defName;
        record.deathDamageDef = dinfo?.Def?.defName;
        record.deathInstigatorThingId = dinfo?.Instigator?.ThingID;
        record.deathInstigatorLabel = dinfo?.Instigator?.LabelShortCap;
        record.deathWeaponDef = dinfo?.Weapon?.defName;

        bool violent = IsViolentDeath(pawn, dinfo);
        bool pawnInstigator = dinfo?.Instigator is Pawn;
        bool abrupt = IsAbruptDeath(pawn, dinfo, exactCulprit);
        bool longIllness = IsLongIllnessDeath(dinfo, exactCulprit);

        record.deathWasViolent = violent;
        record.deathWasAbrupt = abrupt;

        if (violent)
        {
            score += 35;
            reasons.Add("violent death +35");
        }

        if (pawnInstigator)
        {
            score += 10;
            reasons.Add("killed by pawn +10");
        }

        if (abrupt)
        {
            score += 20;
            reasons.Add("abrupt death +20");
        }

        if (longIllness)
        {
            score -= 20;
            reasons.Add("gradual illness or age-like death -20");
        }

        if (record.moodAtDeath >= 0f)
        {
            if (record.moodAtDeath < 0.15f)
            {
                score += 25;
                reasons.Add("extreme low mood +25");
            }
            else if (record.moodAtDeath < 0.35f)
            {
                score += 15;
                reasons.Add("low mood +15");
            }
        }

        int memoryPressure = CalculateNegativeMemoryPressure(pawn, reasons);
        score += memoryPressure;

        if (record.diedInMentalState)
        {
            score += 10;
            reasons.Add("mental state at death +10");
        }

        if (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony || pawn.IsQuestLodger())
        {
            score += 10;
            reasons.Add("player-relevant pawn +10");
        }

        if (pawn.Map?.IsPlayerHome == true)
        {
            score += 10;
            reasons.Add("died on player home map +10");
        }

        record.hauntingRiskScore = Math.Max(0, score);
        record.hauntingRiskReasons = reasons;
    }

    public static string RiskBand(int score)
    {
        if (score >= 75) return "Severe";
        if (score >= 50) return "High";
        if (score >= 25) return "Restless";
        return "Quiet";
    }

    private static float ResolveMood(Pawn pawn)
    {
        try
        {
            return pawn.needs?.mood?.CurLevel ?? -1f;
        }
        catch
        {
            return -1f;
        }
    }

    private static bool IsViolentDeath(Pawn pawn, DamageInfo? dinfo)
    {
        if (!dinfo.HasValue || dinfo.Value.Def == null)
            return false;

        try
        {
            if (dinfo.Value.Def.ExternalViolenceFor(pawn))
                return true;
        }
        catch
        {
        }

        string defName = dinfo.Value.Def.defName ?? string.Empty;
        return defName.IndexOf("Bullet", StringComparison.OrdinalIgnoreCase) >= 0 ||
               defName.IndexOf("Bomb", StringComparison.OrdinalIgnoreCase) >= 0 ||
               defName.IndexOf("Cut", StringComparison.OrdinalIgnoreCase) >= 0 ||
               defName.IndexOf("Stab", StringComparison.OrdinalIgnoreCase) >= 0 ||
               defName.IndexOf("Blunt", StringComparison.OrdinalIgnoreCase) >= 0 ||
               defName.IndexOf("Flame", StringComparison.OrdinalIgnoreCase) >= 0 ||
               defName.IndexOf("Bite", StringComparison.OrdinalIgnoreCase) >= 0 ||
               defName.IndexOf("Scratch", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsAbruptDeath(Pawn pawn, DamageInfo? dinfo, Hediff exactCulprit)
    {
        if (dinfo.HasValue && dinfo.Value.Amount >= Math.Max(15f, pawn.HealthScale * 10f))
            return true;

        if (IsLongIllnessDeath(dinfo, exactCulprit))
            return false;

        return IsViolentDeath(pawn, dinfo);
    }

    private static bool IsLongIllnessDeath(DamageInfo? dinfo, Hediff exactCulprit)
    {
        string hediffName = exactCulprit?.def?.defName ?? string.Empty;
        string damageName = dinfo?.Def?.defName ?? string.Empty;
        string combined = hediffName + " " + damageName;

        return ContainsAny(combined, "OldAge", "HeartAttack", "Carcinoma", "Infection", "Disease", "Flu", "Plague",
            "Malaria", "SleepingSickness", "BloodLoss", "Hypothermia", "Heatstroke", "Starvation", "Dehydration");
    }

    private static bool ContainsAny(string text, params string[] needles)
    {
        foreach (string needle in needles)
        {
            if (text.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    private static int CalculateNegativeMemoryPressure(Pawn pawn, List<string> reasons)
    {
        int pressure = 0;
        int named = 0;

        foreach (object memory in EnumerateMoodMemories(pawn))
        {
            float moodOffset = ResolveMemoryMoodOffset(memory);
            if (moodOffset >= -3f)
                continue;

            int contribution = moodOffset <= -12f ? 8 : moodOffset <= -8f ? 5 : 3;
            pressure += contribution;

            if (named < 3)
            {
                reasons.Add("negative memory " + ResolveMemoryLabel(memory) + " +" + contribution);
                named++;
            }

            if (pressure >= 20)
            {
                pressure = 20;
                break;
            }
        }

        if (pressure > 0 && named == 0)
            reasons.Add("negative memories +" + pressure);

        return pressure;
    }

    private static IEnumerable<object> EnumerateMoodMemories(Pawn pawn)
    {
        object memoriesHandler = pawn?.needs?.mood?.thoughts?.memories;
        if (memoriesHandler == null)
            yield break;

        PropertyInfo memoriesProperty = memoriesHandler.GetType().GetProperty("Memories", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object memories = memoriesProperty?.GetValue(memoriesHandler, null);
        if (memories is not IEnumerable enumerable)
            yield break;

        foreach (object memory in enumerable)
        {
            if (memory != null)
                yield return memory;
        }
    }

    private static float ResolveMemoryMoodOffset(object memory)
    {
        try
        {
            MethodInfo moodOffsetMethod = memory.GetType().GetMethod("MoodOffset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (moodOffsetMethod != null)
                return Convert.ToSingle(moodOffsetMethod.Invoke(memory, null));
        }
        catch
        {
        }

        try
        {
            object def = memory.GetType().GetField("def", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(memory);
            object stages = def?.GetType().GetField("stages", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(def);
            if (stages is IList list && list.Count > 0)
            {
                object stage = list[0];
                object effect = stage.GetType().GetField("baseMoodEffect", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(stage);
                if (effect != null)
                    return Convert.ToSingle(effect);
            }
        }
        catch
        {
        }

        return 0f;
    }

    private static string ResolveMemoryLabel(object memory)
    {
        try
        {
            object def = memory.GetType().GetField("def", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(memory);
            object label = def?.GetType().GetField("label", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(def);
            if (label != null)
                return label.ToString();
        }
        catch
        {
        }

        return memory.GetType().Name;
    }
}
