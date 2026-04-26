using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Represents a spawned persistent spell marker that can expire or be cleaned up by linked runtime events.
/// </summary>
public sealed class PersistentSpellEffect : IExposable
{
    private Thing caster;
    private SpellDef spellDef;
    private Thing markerThing;
    private IntVec3 cell = IntVec3.Invalid;
    private int expireAtTick = -1;

    public PersistentSpellEffect()
    {
    }

    public PersistentSpellEffect(Thing caster, SpellDef spellDef, Thing markerThing, IntVec3 cell, int expireAtTick)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.markerThing = markerThing;
        this.cell = cell;
        this.expireAtTick = expireAtTick;
    }

    public Thing Caster => caster;

    public SpellDef SpellDef => spellDef;

    public Thing MarkerThing => markerThing;

    public IntVec3 Cell => cell;

    public int ExpireAtTick => expireAtTick;

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_References.Look(ref markerThing, "markerThing");
        Scribe_Values.Look(ref cell, "cell", IntVec3.Invalid);
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);
    }
}
