using RimWorld;
using Verse;

namespace MFVanilla.Core;

public sealed class Plant_CinderRosebush : Plant
{
    public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
    {
        if (by != null && !by.Dead && !by.Destroyed)
        {
            by.TakeDamage(new DamageInfo(DamageDefOf.Cut, 1f, armorPenetration: 0f, instigator: this));
        }

        base.PlantCollected(by, plantDestructionMode);
    }
}
