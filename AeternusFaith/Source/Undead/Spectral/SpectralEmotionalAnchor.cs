using RimWorld;
using Verse;

namespace AeternusFaith.Undead.Spectral
{
    public enum SpectralEmotionalAnchorKind
    {
        LovedOne,
        Family,
        Rival,
        Killer
    }

    public class SpectralEmotionalAnchor : IExposable
    {
        public Pawn pawn;
        public string pawnThingId;
        public string pawnLabel;
        public PawnRelationDef relationDef;
        public SpectralEmotionalAnchorKind kind;
        public float weight = 1f;
        public IntVec3 lastKnownPosition = IntVec3.Invalid;

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref pawnThingId, "pawnThingId");
            Scribe_Values.Look(ref pawnLabel, "pawnLabel");
            Scribe_Defs.Look(ref relationDef, "relationDef");
            Scribe_Values.Look(ref kind, "kind", SpectralEmotionalAnchorKind.Family);
            Scribe_Values.Look(ref weight, "weight", 1f);
            Scribe_Values.Look(ref lastKnownPosition, "lastKnownPosition", IntVec3.Invalid);
        }

        public bool TryResolvePawn(Map map, out Pawn resolvedPawn)
        {
            resolvedPawn = null;

            if (pawn?.Destroyed == false)
            {
                resolvedPawn = pawn;
                return true;
            }

            if (map == null || pawnThingId.NullOrEmpty())
                return false;

            foreach (Pawn mapPawn in map.mapPawns.AllPawnsSpawned)
            {
                if (mapPawn?.ThingID == pawnThingId)
                {
                    pawn = mapPawn;
                    resolvedPawn = mapPawn;
                    return true;
                }
            }

            return false;
        }
    }
}
