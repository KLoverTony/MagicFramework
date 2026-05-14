using MagicFramework.Definitions;
using System.Collections.Generic;
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
    private List<int> actionPath = new();

    public SpawnedThingRecord()
    {
    }

    public SpawnedThingRecord(Thing caster, SpellDef spellDef, Thing spawnedThing, int expireAtTick, IEnumerable<int> actionPath)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.spawnedThing = spawnedThing;
        this.expireAtTick = expireAtTick;
        this.actionPath = actionPath != null ? new List<int>(actionPath) : new List<int>();
    }

    public Thing Caster => caster;

    public SpellDef SpellDef => spellDef;

    public Thing SpawnedThing => spawnedThing;

    public int ExpireAtTick => expireAtTick;

    public IReadOnlyList<int> ActionPath => actionPath;

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
        Scribe_Collections.Look(ref actionPath, "actionPath", LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && actionPath == null)
        {
            actionPath = new List<int>();
        }
    }
}
