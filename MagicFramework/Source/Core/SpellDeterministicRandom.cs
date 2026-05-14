using System;
using System.Collections.Generic;
using MagicFramework.Context;
using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Stable random-looking values derived only from explicit gameplay state.
/// Use this for spell gameplay decisions instead of ambient RNG.
/// Framework-owned gameplay randomness should prefer this utility so results
/// are independent of local RNG state, call order, and frame timing.
/// </summary>
public static class SpellDeterministicRandom
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    public static ulong Hash(params object[] values)
    {
        ulong hash = OffsetBasis;
        if (values == null)
        {
            return hash;
        }

        for (int i = 0; i < values.Length; i++)
        {
            AddValue(ref hash, values[i]);
        }

        return Mix(hash);
    }

    public static float Float01(params object[] values)
    {
        ulong hash = Hash(values);
        return (hash >> 40) / 16777216f;
    }

    public static bool Chance(float chance, params object[] values)
    {
        if (chance <= 0f)
        {
            return false;
        }

        if (chance >= 1f)
        {
            return true;
        }

        return Float01(values) < chance;
    }

    public static int RangeInclusive(int minInclusive, int maxInclusive, params object[] values)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        ulong range = (ulong)(maxInclusive - minInclusive + 1);
        return minInclusive + (int)(Hash(values) % range);
    }

    public static float Range(float minInclusive, float maxInclusive, params object[] values)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        return minInclusive + ((maxInclusive - minInclusive) * Float01(values));
    }

    public static void Shuffle<T>(IList<T> list, params object[] values)
    {
        if (list == null)
        {
            return;
        }

        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = RangeInclusive(0, i, Append(values, i));
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }

    public static object[] ContextSalt(SpellContext context, string channel)
    {
        return new object[]
        {
            channel,
            context?.randomSeed ?? 0,
            context?.spellDef?.defName ?? string.Empty,
            StableThingId(context?.caster),
            context?.map?.uniqueID ?? -1,
            StableTargetId(context?.initialTarget ?? LocalTargetInfo.Invalid),
            StableTargetId(context?.currentTarget ?? LocalTargetInfo.Invalid),
            StableCellId(context?.currentCell ?? IntVec3.Invalid)
        };
    }

    public static int StableThingId(Thing thing)
    {
        return thing?.thingIDNumber ?? 0;
    }

    public static int StableTargetId(LocalTargetInfo target)
    {
        if (target.Thing != null)
        {
            return StableThingId(target.Thing);
        }

        return StableCellId(target.Cell);
    }

    public static int StableCellId(IntVec3 cell)
    {
        return cell.IsValid ? ((cell.x & 0x3fff) << 18) ^ ((cell.z & 0x3fff) << 4) ^ (cell.y & 0xf) : 0;
    }

    public static object[] Append(object[] values, params object[] extraValues)
    {
        int valueCount = values?.Length ?? 0;
        int extraCount = extraValues?.Length ?? 0;
        object[] result = new object[valueCount + extraCount];
        if (valueCount > 0)
        {
            Array.Copy(values, result, valueCount);
        }

        if (extraCount > 0)
        {
            Array.Copy(extraValues, 0, result, valueCount, extraCount);
        }

        return result;
    }

    private static void AddValue(ref ulong hash, object value)
    {
        switch (value)
        {
            case null:
                AddUInt64(ref hash, 0);
                break;
            case string text:
                AddString(ref hash, text);
                break;
            case int intValue:
                AddUInt64(ref hash, unchecked((uint)intValue));
                break;
            case uint uintValue:
                AddUInt64(ref hash, uintValue);
                break;
            case long longValue:
                AddUInt64(ref hash, unchecked((ulong)longValue));
                break;
            case ulong ulongValue:
                AddUInt64(ref hash, ulongValue);
                break;
            case bool boolValue:
                AddUInt64(ref hash, boolValue ? 1UL : 0UL);
                break;
            case float floatValue:
                AddUInt64(ref hash, unchecked((uint)BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0)));
                break;
            case double doubleValue:
                AddUInt64(ref hash, unchecked((ulong)BitConverter.DoubleToInt64Bits(doubleValue)));
                break;
            case IntVec3 cell:
                AddUInt64(ref hash, unchecked((uint)StableCellId(cell)));
                break;
            case Thing thing:
                AddUInt64(ref hash, unchecked((uint)StableThingId(thing)));
                break;
            default:
                AddString(ref hash, value.ToString());
                break;
        }

        AddUInt64(ref hash, 0xff);
    }

    private static void AddString(ref ulong hash, string value)
    {
        if (value == null)
        {
            AddUInt64(ref hash, 0);
            return;
        }

        for (int i = 0; i < value.Length; i++)
        {
            AddUInt64(ref hash, value[i]);
        }
    }

    private static void AddUInt64(ref ulong hash, ulong value)
    {
        for (int i = 0; i < 8; i++)
        {
            hash ^= (value >> (i * 8)) & 0xffUL;
            hash *= Prime;
        }
    }

    private static ulong Mix(ulong value)
    {
        value ^= value >> 33;
        value *= 0xff51afd7ed558ccdUL;
        value ^= value >> 33;
        value *= 0xc4ceb9fe1a85ec53UL;
        value ^= value >> 33;
        return value;
    }
}
