using MagicFramework.Definitions;
using RimWorld;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Tracks a pawn whose faction has been temporarily overridden by a maintained spell.
/// </summary>
public sealed class ActiveTemporaryAllegiance : IExposable
{
    public Pawn target;
    public Thing caster;
    public SpellDef spellDef;
    public Faction originalFaction;
    public Faction temporaryFaction;
    public HediffDef indicatorHediffDef;
    public float indicatorSeverity = 0.01f;
    public bool removeIndicatorOnExpire = true;
    public string statusCueLabel;
    public string statusCueDescription;
    public int expireAtTick = -1;
    public float maxRange = -1f;
    public SpellMaintenanceDef maintenance;
    public float sustainedManaCost;
    public int sustainedManaCostIntervalTicks = 60;
    public int nextSustainedManaCostTick;

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref target, "target");
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_References.Look(ref originalFaction, "originalFaction");
        Scribe_References.Look(ref temporaryFaction, "temporaryFaction");
        Scribe_Defs.Look(ref indicatorHediffDef, "indicatorHediffDef");
        Scribe_Values.Look(ref indicatorSeverity, "indicatorSeverity", 0.01f);
        Scribe_Values.Look(ref removeIndicatorOnExpire, "removeIndicatorOnExpire", true);
        Scribe_Values.Look(ref statusCueLabel, "statusCueLabel");
        Scribe_Values.Look(ref statusCueDescription, "statusCueDescription");
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);
        Scribe_Values.Look(ref maxRange, "maxRange", -1f);
        Scribe_Deep.Look(ref maintenance, "maintenance");
        Scribe_Values.Look(ref sustainedManaCost, "sustainedManaCost");
        Scribe_Values.Look(ref sustainedManaCostIntervalTicks, "sustainedManaCostIntervalTicks", 60);
        Scribe_Values.Look(ref nextSustainedManaCostTick, "nextSustainedManaCostTick");
    }
}
