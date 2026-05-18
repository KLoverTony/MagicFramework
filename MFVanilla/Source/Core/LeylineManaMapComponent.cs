using MagicFramework.Core;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class LeylineManaMapComponent : MapComponent
{
    private const int CheckIntervalTicks = 250;
    private const float BaseManaPerCheck = 0.25f;
    private const float PawnSampleRadius = 1.01f;
    private const float BonusPerPeakStrength = 0.06f;
    private const float MaxLeylineBonus = 0.30f;

    public LeylineManaMapComponent(Map map)
        : base(map)
    {
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        if ((currentTick + map.uniqueID) % CheckIntervalTicks != 0)
        {
            return;
        }

        TickPawnManaRecovery();
    }

    private void TickPawnManaRecovery()
    {
        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        if (runtime == null || map.mapPawns?.AllPawnsSpawned == null)
        {
            return;
        }

        foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
        {
            if (!ShouldRecoverMana(pawn, runtime))
            {
                continue;
            }

            LeylineAreaReading reading = LeylineUtility.ReadRadius(map, pawn.Position, PawnSampleRadius);
            float leylineBonus = LeylineUtility.PeakStrengthBonus(reading, BonusPerPeakStrength, MaxLeylineBonus);
            runtime.RestoreMana(pawn, BaseManaPerCheck * (1f + leylineBonus));
        }
    }

    private static bool ShouldRecoverMana(Pawn pawn, SpellRuntimeGameComponent runtime)
    {
        if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned || !runtime.HasArcaneGift(pawn))
        {
            return false;
        }

        return runtime.GetCurrentMana(pawn) < runtime.GetMaxMana(pawn) - 0.001f;
    }
}
