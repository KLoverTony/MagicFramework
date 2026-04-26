using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Tracks a spell-spawned thing so temporary conjurations can expire cleanly.
/// </summary>
public sealed class SpawnedThingRecord : IExposable
{
    private Thing caster;
    private SpellDef spellDef;
    private Thing spawnedThing;
    private int expireAtTick = -1;

    public SpawnedThingRecord()
    {
    }

    public SpawnedThingRecord(Thing caster, SpellDef spellDef, Thing spawnedThing, int expireAtTick)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.spawnedThing = spawnedThing;
        this.expireAtTick = expireAtTick;
    }

    public Thing Caster => caster;

    public SpellDef SpellDef => spellDef;

    public Thing SpawnedThing => spawnedThing;

    public int ExpireAtTick => expireAtTick;

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_References.Look(ref spawnedThing, "spawnedThing");
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);
    }
}
