using System.Collections.Generic;
using System.Linq;
using MagicFramework.Definitions;
using RimWorld;
using Verse;

namespace MagicFramework.PawnMemory;

public static class SpellResurrectionUtility
{
    private const string ResurrectionSicknessDefName = "ResurrectionSickness";

    public static bool TryValidateCorpseForResurrection(LocalTargetInfo target, out Pawn pawn, out string reason)
    {
        pawn = null;
        reason = null;

        if (!(target.Thing is Corpse corpse))
        {
            reason = "Target must be a corpse.";
            return false;
        }

        pawn = corpse.InnerPawn;
        if (pawn == null)
        {
            reason = "Corpse had no pawn to resurrect.";
            return false;
        }

        if (!pawn.Dead)
        {
            reason = $"{pawn.LabelShortCap} is not dead.";
            return false;
        }

        if (pawn.RaceProps?.Humanlike != true)
        {
            reason = "Only humanlike pawns can be resurrected this way.";
            return false;
        }

        PawnMemoryRecord record = WorldComponent_PawnMemories.Instance?.GetMemory(pawn);
        if (record?.state == PawnMemoryState.Released || record?.rituallyReleased == true)
        {
            reason = "The spirit has moved on and cannot be called back.";
            return false;
        }

        if (record?.resurrectionAllowed == false)
        {
            reason = "This pawn's memory cannot anchor a resurrection.";
            return false;
        }

        if (IsMissingVitalPart(pawn, out string missingPartLabel))
        {
            reason = $"The corpse is missing {missingPartLabel}.";
            return false;
        }

        return true;
    }

    public static bool TryResurrect(
        LocalTargetInfo target,
        bool removeResurrectionSickness,
        bool preserveNonVitalDamage,
        bool updatePawnMemory,
        bool despawnActiveSpirit,
        out Pawn resurrectedPawn,
        out string reason)
    {
        resurrectedPawn = null;
        if (!TryValidateCorpseForResurrection(target, out Pawn pawn, out reason))
        {
            return false;
        }

        List<MissingPartSnapshot> missingParts = preserveNonVitalDamage ? CaptureMissingParts(pawn) : null;
        List<InjurySnapshot> injuries = preserveNonVitalDamage ? CaptureInjuries(pawn) : null;

        if (!ResurrectionUtility.TryResurrect(pawn))
        {
            reason = $"RimWorld could not resurrect {pawn.LabelShortCap}.";
            return false;
        }

        resurrectedPawn = pawn;
        if (removeResurrectionSickness)
        {
            RemoveResurrectionSickness(pawn);
        }

        if (preserveNonVitalDamage)
        {
            RestoreNonVitalMissingParts(pawn, missingParts);
            RestoreNonVitalInjuries(pawn, injuries);
            pawn.health?.Notify_HediffChanged(null);
        }

        if (updatePawnMemory)
        {
            NotifyPawnMemoryResurrected(pawn, despawnActiveSpirit);
        }

        reason = null;
        return true;
    }

    private static bool IsMissingVitalPart(Pawn pawn, out string missingPartLabel)
    {
        missingPartLabel = null;
        if (pawn?.health?.hediffSet?.hediffs == null)
        {
            missingPartLabel = "vital health data";
            return true;
        }

        BodyPartRecord core = pawn.RaceProps?.body?.corePart;
        BodyPartRecord head = FindBodyPart(pawn, "Head");
        BodyPartRecord brain = FindBodyPart(pawn, "Brain");

        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
        {
            if (!(hediff is Hediff_MissingPart missingPart) || missingPart.Part == null)
            {
                continue;
            }

            if (core != null && missingPart.Part == core)
            {
                missingPartLabel = "the core body";
                return true;
            }

            if (brain != null && missingPart.Part == brain)
            {
                missingPartLabel = "the brain";
                return true;
            }

            if (head != null && missingPart.Part == head)
            {
                missingPartLabel = "the head";
                return true;
            }
        }

        return false;
    }

    private static void RemoveResurrectionSickness(Pawn pawn)
    {
        HediffDef sicknessDef = DefDatabase<HediffDef>.GetNamedSilentFail(ResurrectionSicknessDefName);
        if (pawn?.health?.hediffSet?.hediffs == null || sicknessDef == null)
        {
            return;
        }

        List<Hediff> toRemove = pawn.health.hediffSet.hediffs.Where(hediff => hediff.def == sicknessDef).ToList();
        foreach (Hediff hediff in toRemove)
        {
            pawn.health.RemoveHediff(hediff);
        }
    }

    private static void NotifyPawnMemoryResurrected(Pawn pawn, bool despawnActiveSpirit)
    {
        WorldComponent_PawnMemories registry = WorldComponent_PawnMemories.Instance;
        PawnMemoryRecord record = registry?.GetOrCreateMemory(pawn);
        if (record == null)
        {
            return;
        }

        if (despawnActiveSpirit)
        {
            DespawnActiveSpirit(record);
        }

        PawnSoulRiteUtility.NotifyPawnResurrected(pawn);
    }

    private static void DespawnActiveSpirit(PawnMemoryRecord record)
    {
        if (record == null || string.IsNullOrWhiteSpace(record.activeSpiritThingId))
        {
            return;
        }

        foreach (Map map in Find.Maps)
        {
            List<Thing> things = map?.listerThings?.AllThings;
            if (things == null)
            {
                continue;
            }

            for (int i = things.Count - 1; i >= 0; i--)
            {
                Thing thing = things[i];
                if (thing?.ThingID == record.activeSpiritThingId && !thing.Destroyed)
                {
                    thing.Destroy(DestroyMode.Vanish);
                    return;
                }
            }
        }
    }

    private static List<MissingPartSnapshot> CaptureMissingParts(Pawn pawn)
    {
        List<MissingPartSnapshot> snapshots = new();
        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
        {
            if (hediff is Hediff_MissingPart missingPart && missingPart.Part != null && !IsVitalPart(pawn, missingPart.Part))
            {
                snapshots.Add(new MissingPartSnapshot(missingPart.Part, missingPart.IsFresh, missingPart.lastInjury));
            }
        }

        return snapshots;
    }

    private static List<InjurySnapshot> CaptureInjuries(Pawn pawn)
    {
        List<InjurySnapshot> snapshots = new();
        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
        {
            if (hediff is Hediff_Injury injury && injury.Part != null && injury.Severity > 0f && !IsVitalPart(pawn, injury.Part))
            {
                snapshots.Add(new InjurySnapshot(injury.def, injury.Part, injury.Severity));
            }
        }

        return snapshots;
    }

    private static void RestoreNonVitalMissingParts(Pawn pawn, List<MissingPartSnapshot> snapshots)
    {
        if (snapshots == null)
        {
            return;
        }

        foreach (MissingPartSnapshot snapshot in snapshots)
        {
            if (snapshot.Part == null || IsVitalPart(pawn, snapshot.Part) || HasMissingPart(pawn, snapshot.Part))
            {
                continue;
            }

            Hediff_MissingPart missingPart = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, snapshot.Part) as Hediff_MissingPart;
            if (missingPart == null)
            {
                continue;
            }

            missingPart.IsFresh = snapshot.IsFresh;
            missingPart.lastInjury = snapshot.LastInjury;
            pawn.health.AddHediff(missingPart, snapshot.Part);
        }
    }

    private static void RestoreNonVitalInjuries(Pawn pawn, List<InjurySnapshot> snapshots)
    {
        if (snapshots == null)
        {
            return;
        }

        HashSet<BodyPartRecord> notMissingParts = new(pawn.health.hediffSet.GetNotMissingParts());
        foreach (InjurySnapshot snapshot in snapshots)
        {
            if (snapshot.Def == null || snapshot.Part == null || IsVitalPart(pawn, snapshot.Part) || !notMissingParts.Contains(snapshot.Part))
            {
                continue;
            }

            Hediff_Injury injury = HediffMaker.MakeHediff(snapshot.Def, pawn, snapshot.Part) as Hediff_Injury;
            if (injury == null)
            {
                continue;
            }

            injury.Severity = snapshot.Severity;
            pawn.health.AddHediff(injury, snapshot.Part);
        }
    }

    private static bool HasMissingPart(Pawn pawn, BodyPartRecord part)
    {
        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
        {
            if (hediff is Hediff_MissingPart missingPart && missingPart.Part == part)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsVitalPart(Pawn pawn, BodyPartRecord part)
    {
        BodyPartRecord core = pawn.RaceProps?.body?.corePart;
        BodyPartRecord head = FindBodyPart(pawn, "Head");
        BodyPartRecord brain = FindBodyPart(pawn, "Brain");
        return part == core || part == head || part == brain;
    }

    private static BodyPartRecord FindBodyPart(Pawn pawn, string defName)
    {
        if (pawn?.RaceProps?.body?.AllParts == null)
        {
            return null;
        }

        foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
        {
            if (part?.def?.defName == defName)
            {
                return part;
            }
        }

        return null;
    }

    private readonly struct MissingPartSnapshot
    {
        public MissingPartSnapshot(BodyPartRecord part, bool isFresh, HediffDef lastInjury)
        {
            Part = part;
            IsFresh = isFresh;
            LastInjury = lastInjury;
        }

        public BodyPartRecord Part { get; }
        public bool IsFresh { get; }
        public HediffDef LastInjury { get; }
    }

    private readonly struct InjurySnapshot
    {
        public InjurySnapshot(HediffDef def, BodyPartRecord part, float severity)
        {
            Def = def;
            Part = part;
            Severity = severity;
        }

        public HediffDef Def { get; }
        public BodyPartRecord Part { get; }
        public float Severity { get; }
    }
}
