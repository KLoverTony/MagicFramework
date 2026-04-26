using System.Collections.Generic;
using MagicFramework.Definitions;
using RimWorld;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Represents an active timed stat modifier applied by the spell framework.
/// </summary>
public sealed class ActiveSpellStatModifier : IExposable
{
    public Thing target;
    public Thing caster;
    public SpellDef spellDef;
    public int expireAtTick = -1;
    public HediffDef indicatorHediffDef;
    public float indicatorSeverity = 0.01f;
    public bool removeIndicatorOnExpire = true;
    public string statusCueLabel;
    public string statusCueDescription;
    public bool isSustained;
    public float maxRange = -1f;
    public bool breakWhenCasterDowned = true;
    public bool breakWhenTargetDowned;
    public bool breakWhenTargetOutOfRange = true;
    public bool breakWhenLineOfSightLost = true;
    public List<int> sourceActionPath = new();
    public List<ActiveSpellStatModifierEntry> modifiers = new();

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public bool MatchesSource(Thing sourceCaster, SpellDef sourceSpellDef)
    {
        return target != null && caster == sourceCaster && spellDef == sourceSpellDef;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref target, "target");
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);
        Scribe_Defs.Look(ref indicatorHediffDef, "indicatorHediffDef");
        Scribe_Values.Look(ref indicatorSeverity, "indicatorSeverity", 0.01f);
        Scribe_Values.Look(ref removeIndicatorOnExpire, "removeIndicatorOnExpire", true);
        Scribe_Values.Look(ref statusCueLabel, "statusCueLabel");
        Scribe_Values.Look(ref statusCueDescription, "statusCueDescription");
        Scribe_Values.Look(ref isSustained, "isSustained");
        Scribe_Values.Look(ref maxRange, "maxRange", -1f);
        Scribe_Values.Look(ref breakWhenCasterDowned, "breakWhenCasterDowned", true);
        Scribe_Values.Look(ref breakWhenTargetDowned, "breakWhenTargetDowned");
        Scribe_Values.Look(ref breakWhenTargetOutOfRange, "breakWhenTargetOutOfRange", true);
        Scribe_Values.Look(ref breakWhenLineOfSightLost, "breakWhenLineOfSightLost", true);
        Scribe_Collections.Look(ref sourceActionPath, "sourceActionPath", LookMode.Value);
        Scribe_Collections.Look(ref modifiers, "modifiers", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && sourceActionPath == null)
        {
            sourceActionPath = new List<int>();
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit && modifiers == null)
        {
            modifiers = new List<ActiveSpellStatModifierEntry>();
        }
    }
}

public sealed class ActiveSpellStatModifierEntry : IExposable
{
    public StatDef statDef;
    public float offset;
    public float factor = 1f;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref statDef, "statDef");
        Scribe_Values.Look(ref offset, "offset");
        Scribe_Values.Look(ref factor, "factor", 1f);
    }
}
