using Verse;
using RimWorld;

namespace MagicFramework.Core;

/// <summary>
/// Spell-driven drowning state: high severity can kill after a short downed grace period.
/// </summary>
public sealed class SpellDrowningHediff : HediffWithComps
{
    private const float LethalSeverity = 0.9f;
    private const int GraceTicks = 600;
    private int downedAtTick = -1;

    public override void Tick()
    {
        base.Tick();
        if (pawn == null || pawn.Dead || Severity < LethalSeverity)
        {
            downedAtTick = -1;
            return;
        }

        if (!pawn.Downed)
        {
            downedAtTick = -1;
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        if (downedAtTick < 0)
        {
            downedAtTick = currentTick;
            return;
        }

        if (currentTick - downedAtTick >= GraceTicks)
        {
            pawn.Kill(null, this);
        }
    }

    public override string TipStringExtra
    {
        get
        {
            string text = base.TipStringExtra;
            if (Severity < LethalSeverity || pawn == null || pawn.Dead || !pawn.Downed || downedAtTick < 0)
            {
                return text;
            }

            int currentTick = Find.TickManager?.TicksGame ?? 0;
            int remainingTicks = GraceTicks - (currentTick - downedAtTick);
            if (remainingTicks <= 0)
            {
                return text;
            }

            string warning = $"Drowning grace: {remainingTicks.ToStringTicksToPeriod()} remaining.";
            return string.IsNullOrWhiteSpace(text) ? warning : $"{text}\n{warning}";
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref downedAtTick, "downedAtTick", -1);
    }
}
