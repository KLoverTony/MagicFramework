using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class CompProperties_ArcaneForge : CompProperties
{
    public int requiredSpireCount = 4;
    public int lightningIntervalTicksMin = 300;
    public int lightningIntervalTicksMax = 600;
    public float spireRadius = 10f;
    public string spireDefName = "MFV_ArcaneSpire";
    public string lightningFleckDef = "ElectricalSpark";
    public string lightningFlashFleckDef = "SparkFlash";
    public float syncPulseChance = 0.12f;

    public CompProperties_ArcaneForge()
    {
        compClass = typeof(CompArcaneForge);
    }
}

public sealed class CompArcaneForge : ThingComp
{
    private int nextLightningTick;

    private CompProperties_ArcaneForge Props => (CompProperties_ArcaneForge)props;

    public bool HasRequiredSpires => LinkedArcaneSpires().Count() >= Props.requiredSpireCount;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        ScheduleNextLightning();
    }

    public override void CompTick()
    {
        base.CompTick();

        if (!parent.Spawned || Find.TickManager.TicksGame < nextLightningTick) return;

        if (HasRequiredSpires)
        {
            ThrowLightningFlecks();
        }

        ScheduleNextLightning();
    }

    public override string CompInspectStringExtra()
    {
        int linkedCount = LinkedArcaneSpires().Count();
        string leylineText = LeylineQualityText();
        if (linkedCount >= Props.requiredSpireCount)
        {
            return $"Arcane spires linked: {linkedCount}/{Props.requiredSpireCount}\nReady for enchantment.{leylineText}";
        }

        int missingCount = Props.requiredSpireCount - linkedCount;
        string spireLabel = missingCount == 1 ? "arcane spire" : "arcane spires";
        return $"Arcane spires linked: {linkedCount}/{Props.requiredSpireCount}\nNeeds {missingCount} more linked {spireLabel} within {Props.spireRadius:0.#} cells and line of sight before enchantment bills can be worked.{leylineText}";
    }

    public IEnumerable<Thing> LinkedArcaneSpires()
    {
        if (!parent.Spawned || parent.Map == null) yield break;

        ThingDef spireDef = DefDatabase<ThingDef>.GetNamedSilentFail(Props.spireDefName);
        if (spireDef == null) yield break;

        float radiusSquared = Props.spireRadius * Props.spireRadius;

        foreach (Thing spire in parent.Map.listerThings.ThingsOfDef(spireDef))
        {
            if (spire?.Spawned == true && spire.Position.DistanceToSquared(parent.Position) <= radiusSquared && GenSight.LineOfSight(parent.Position, spire.Position, parent.Map))
            {
                yield return spire;
            }
        }
    }

    private void ThrowLightningFlecks()
    {
        FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(Props.lightningFleckDef);
        if (fleckDef == null || parent.Map == null) return;

        FleckDef flashFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(Props.lightningFlashFleckDef);
        Vector3 target = parent.DrawPos;
        List<Thing> spires = LinkedArcaneSpires().ToList();
        if (spires.Count == 0) return;

        if (Rand.Chance(Props.syncPulseChance))
        {
            foreach (Thing spire in spires)
            {
                ThrowLightningPulse(spire.DrawPos, target, parent.Map, fleckDef, flashFleckDef);
            }

            return;
        }

        ThrowLightningPulse(spires.RandomElement().DrawPos, target, parent.Map, fleckDef, flashFleckDef);
    }

    private static void ThrowLightningPulse(Vector3 from, Vector3 to, Map map, FleckDef lineFleckDef, FleckDef flashFleckDef)
    {
        ThrowLightningLine(from, to, map, lineFleckDef);
        if (flashFleckDef == null) return;

        FleckMaker.Static(from, map, flashFleckDef, 0.8f);
        FleckMaker.Static(to, map, flashFleckDef, 1f);
    }

    private static void ThrowLightningLine(Vector3 from, Vector3 to, Map map, FleckDef fleckDef)
    {
        Vector3 delta = to - from;
        float distance = delta.MagnitudeHorizontal();
        int fleckCount = Mathf.Clamp(Mathf.CeilToInt(distance * 1.4f), 3, 18);

        for (int i = 0; i < fleckCount; i++)
        {
            float t = (i + Rand.Value * 0.6f) / fleckCount;
            Vector3 position = from + delta * t;
            position.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            position.x += Rand.Range(-0.08f, 0.08f);
            position.z += Rand.Range(-0.08f, 0.08f);
            FleckMaker.Static(position, map, fleckDef, Rand.Range(0.55f, 0.9f));
        }
    }

    private void ScheduleNextLightning()
    {
        int min = Mathf.Max(1, Props.lightningIntervalTicksMin);
        int max = Mathf.Max(min, Props.lightningIntervalTicksMax);
        nextLightningTick = Find.TickManager.TicksGame + Rand.RangeInclusive(min, max);
    }

    private string LeylineQualityText()
    {
        float chance = EnchantmentUtility.LeylineQualityBonusChance(parent);
        if (chance <= 0f)
        {
            return "\nLeyline resonance: none.";
        }

        return $"\nLeyline resonance: {chance.ToStringPercent()} chance to improve enchanted weapon quality.";
    }
}
