using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace MFVanilla.Core;

public sealed class ArcaneTreasureTableDef : Def
{
    public List<ArcaneTreasureBucketDef> buckets = new();
}

public sealed class ArcaneTreasureBucketDef
{
    public string label;
    public int rolls = 1;
    public List<ArcaneTreasureEntryDef> entries = new();
}

public sealed class ArcaneTreasureEntryDef
{
    public ThingDef thingDef;
    public float weight = 1f;
    public int minCount = 1;
    public int maxCount = 1;
    public int minTier;
    public int maxTier = int.MaxValue;
    public QualityCategory? quality;
}

public sealed class CompProperties_UseEffectOpenArcaneTreasure : CompProperties_UseEffect
{
    public ArcaneTreasureTableDef treasureTable;
    public int tier = 1;
    public int targetValue;

    public CompProperties_UseEffectOpenArcaneTreasure()
    {
        compClass = typeof(CompUseEffect_OpenArcaneTreasure);
    }
}

public sealed class CompUseEffect_OpenArcaneTreasure : CompUseEffect
{
    private string stableChestId;
    private bool opened;

    private CompProperties_UseEffectOpenArcaneTreasure Props => (CompProperties_UseEffectOpenArcaneTreasure)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        EnsureStableChestId();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref stableChestId, "stableChestId");
        Scribe_Values.Look(ref opened, "opened");
    }

    public override AcceptanceReport CanBeUsedBy(Pawn p)
    {
        AcceptanceReport baseReport = base.CanBeUsedBy(p);
        if (!baseReport.Accepted)
        {
            return baseReport;
        }

        if (opened)
        {
            return "This arcane chest has already been opened.";
        }

        if (Props.treasureTable == null)
        {
            return "This arcane chest has no treasure table.";
        }

        return true;
    }

    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        AcceptanceReport report = CanBeUsedBy(usedBy);
        if (!report.Accepted)
        {
            Messages.Message(report.Reason, parent, MessageTypeDefOf.RejectInput, false);
            return;
        }

        EnsureStableChestId();
        List<Thing> rewards = ArcaneTreasureGenerator.Generate(Props.treasureTable, Props.tier, stableChestId);
        if (rewards.Count == 0)
        {
            Messages.Message("The arcane chest was empty.", parent, MessageTypeDefOf.NeutralEvent, false);
            opened = true;
            parent.Destroy();
            return;
        }

        Map map = parent.Map;
        IntVec3 position = parent.Position;
        StringBuilder rewardLabels = new();
        for (int i = 0; i < rewards.Count; i++)
        {
            Thing reward = rewards[i];
            if (reward == null)
            {
                continue;
            }

            GenPlace.TryPlaceThing(reward, position, map, ThingPlaceMode.Near);
            if (rewardLabels.Length > 0)
            {
                rewardLabels.Append(", ");
            }

            rewardLabels.Append(reward.LabelCap);
        }

        opened = true;
        Messages.Message($"Opened arcane chest: {rewardLabels}.", new TargetInfo(position, map), MessageTypeDefOf.PositiveEvent, false);
        parent.Destroy();
    }

    public override string CompInspectStringExtra()
    {
        EnsureStableChestId();
        return $"Arcane cache tier: {Props.tier}\nCache ID: {stableChestId}";
    }

    private void EnsureStableChestId()
    {
        if (!string.IsNullOrWhiteSpace(stableChestId) || parent == null)
        {
            return;
        }

        int tile = parent.MapHeld?.Tile ?? -1;
        stableChestId = DeterministicTreasureHash.HashString($"{parent.def.defName}|{parent.thingIDNumber}|{tile}|{parent.Position.x}|{parent.Position.z}").ToString("X8");
    }
}

public static class ArcaneTreasureGenerator
{
    public static List<Thing> Generate(ArcaneTreasureTableDef table, int tier, string stableChestId)
    {
        List<Thing> rewards = new();
        if (table?.buckets == null || string.IsNullOrWhiteSpace(stableChestId))
        {
            return rewards;
        }

        for (int bucketIndex = 0; bucketIndex < table.buckets.Count; bucketIndex++)
        {
            ArcaneTreasureBucketDef bucket = table.buckets[bucketIndex];
            if (bucket?.entries == null || bucket.entries.Count == 0)
            {
                continue;
            }

            int rolls = Math.Max(0, bucket.rolls);
            for (int roll = 0; roll < rolls; roll++)
            {
                DeterministicTreasureRandom random = new(stableChestId, table.defName, bucket.label, bucketIndex, roll, tier);
                ArcaneTreasureEntryDef entry = PickEntry(bucket.entries, tier, random);
                Thing reward = MakeReward(entry, tier, random);
                if (reward != null)
                {
                    rewards.Add(reward);
                }
            }
        }

        return rewards;
    }

    private static ArcaneTreasureEntryDef PickEntry(List<ArcaneTreasureEntryDef> entries, int tier, DeterministicTreasureRandom random)
    {
        float totalWeight = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            ArcaneTreasureEntryDef entry = entries[i];
            if (IsEligible(entry, tier))
            {
                totalWeight += Math.Max(0f, entry.weight);
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float pick = random.Range(0f, totalWeight);
        for (int i = 0; i < entries.Count; i++)
        {
            ArcaneTreasureEntryDef entry = entries[i];
            if (!IsEligible(entry, tier))
            {
                continue;
            }

            pick -= Math.Max(0f, entry.weight);
            if (pick <= 0f)
            {
                return entry;
            }
        }

        return null;
    }

    private static Thing MakeReward(ArcaneTreasureEntryDef entry, int tier, DeterministicTreasureRandom random)
    {
        if (entry?.thingDef == null)
        {
            return null;
        }

        Thing thing = ThingMaker.MakeThing(entry.thingDef);
        int minCount = Math.Max(1, entry.minCount);
        int maxCount = Math.Max(minCount, entry.maxCount);
        thing.stackCount = Math.Min(thing.def.stackLimit, random.RangeInclusive(minCount, maxCount));

        CompQuality quality = thing.TryGetComp<CompQuality>();
        if (quality != null)
        {
            quality.SetQuality(entry.quality ?? QualityForTier(tier), ArtGenerationContext.Outsider);
        }

        return thing;
    }

    private static bool IsEligible(ArcaneTreasureEntryDef entry, int tier)
    {
        return entry?.thingDef != null
            && entry.weight > 0f
            && tier >= entry.minTier
            && tier <= entry.maxTier;
    }

    private static QualityCategory QualityForTier(int tier)
    {
        if (tier >= 5)
        {
            return QualityCategory.Masterwork;
        }

        if (tier >= 4)
        {
            return QualityCategory.Excellent;
        }

        if (tier >= 3)
        {
            return QualityCategory.Good;
        }

        return QualityCategory.Normal;
    }
}

public sealed class DeterministicTreasureRandom
{
    private uint state;

    public DeterministicTreasureRandom(params object[] seedParts)
    {
        state = DeterministicTreasureHash.HashString(string.Join("|", seedParts));
        if (state == 0)
        {
            state = 0x6D2B79F5u;
        }
    }

    public int RangeInclusive(int min, int max)
    {
        if (max <= min)
        {
            return min;
        }

        uint span = (uint)(max - min + 1);
        return min + (int)(NextUInt() % span);
    }

    public float Range(float min, float max)
    {
        if (max <= min)
        {
            return min;
        }

        return min + NextFloat() * (max - min);
    }

    private float NextFloat()
    {
        return (NextUInt() & 0xFFFFFF) / 16777216f;
    }

    private uint NextUInt()
    {
        uint x = state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        state = x;
        return x;
    }
}

public static class DeterministicTreasureHash
{
    public static uint HashString(string value)
    {
        unchecked
        {
            uint hash = 2166136261u;
            if (value == null)
            {
                return hash;
            }

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619u;
            }

            return hash;
        }
    }
}
