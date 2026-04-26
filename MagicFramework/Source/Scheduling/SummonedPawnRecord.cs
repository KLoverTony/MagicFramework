using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Tracks a temporary spell-summoned pawn so it can be cleaned up on expiry or replacement.
/// </summary>
public sealed class SummonedPawnRecord : IExposable
{
    private Thing caster;
    private SpellDef spellDef;
    private Pawn summonedPawn;
    private int expireAtTick = -1;

    public SummonedPawnRecord()
    {
    }

    public SummonedPawnRecord(Thing caster, SpellDef spellDef, Pawn summonedPawn, int expireAtTick)
    {
        this.caster = caster;
        this.spellDef = spellDef;
        this.summonedPawn = summonedPawn;
        this.expireAtTick = expireAtTick;
    }

    public Thing Caster => caster;

    public SpellDef SpellDef => spellDef;

    public Pawn SummonedPawn => summonedPawn;

    public int ExpireAtTick => expireAtTick;

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_References.Look(ref summonedPawn, "summonedPawn");
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);
    }
}
