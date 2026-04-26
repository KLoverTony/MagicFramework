using System;
using System.Collections.Generic;
using Verse;

namespace MagicFramework.Context;

/// <summary>
/// Lightweight serializable shared state bag for advanced spell flows.
/// </summary>
public sealed class SpellVariableStore : IExposable
{
    private List<SpellVariableEntry> entries = new();

    public bool TryGetValue<TValue>(string key, out TValue value)
    {
        SpellVariableEntry entry = GetEntry(key);
        if (entry != null && entry.TryGetValue(out TValue typedValue))
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }

    public void SetValue(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        SpellVariableEntry entry = GetEntry(key);
        if (value == null)
        {
            if (entry != null)
            {
                entries.Remove(entry);
            }

            return;
        }

        entry ??= new SpellVariableEntry { key = key };
        entry.SetValue(value);
        if (!entries.Contains(entry))
        {
            entries.Add(entry);
        }
    }

    public SpellVariableStore Clone()
    {
        SpellVariableStore clone = new();
        foreach (SpellVariableEntry entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            clone.entries.Add(entry.Clone());
        }

        return clone;
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref entries, "entries", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && entries == null)
        {
            entries = new List<SpellVariableEntry>();
        }
    }

    private SpellVariableEntry GetEntry(string key)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            SpellVariableEntry entry = entries[i];
            if (entry != null && entry.key == key)
            {
                return entry;
            }
        }

        return null;
    }

    private sealed class SpellVariableEntry : IExposable
    {
        public string key;
        private SpellVariableValueKind kind;
        private int intValue;
        private float floatValue;
        private bool boolValue;
        private string stringValue;

        public SpellVariableEntry Clone()
        {
            return new SpellVariableEntry
            {
                key = key,
                kind = kind,
                intValue = intValue,
                floatValue = floatValue,
                boolValue = boolValue,
                stringValue = stringValue
            };
        }

        public void SetValue(object value)
        {
            switch (value)
            {
                case int typedInt:
                    kind = SpellVariableValueKind.Int;
                    intValue = typedInt;
                    break;
                case float typedFloat:
                    kind = SpellVariableValueKind.Float;
                    floatValue = typedFloat;
                    break;
                case double typedDouble:
                    kind = SpellVariableValueKind.Float;
                    floatValue = (float)typedDouble;
                    break;
                case bool typedBool:
                    kind = SpellVariableValueKind.Bool;
                    boolValue = typedBool;
                    break;
                case string typedString:
                    kind = SpellVariableValueKind.String;
                    stringValue = typedString;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported spell variable type {value.GetType().FullName}.");
            }
        }

        public bool TryGetValue<TValue>(out TValue value)
        {
            object rawValue = kind switch
            {
                SpellVariableValueKind.Int => intValue,
                SpellVariableValueKind.Float => floatValue,
                SpellVariableValueKind.Bool => boolValue,
                SpellVariableValueKind.String => stringValue,
                _ => null
            };

            if (rawValue is TValue typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref key, "key");
            Scribe_Values.Look(ref kind, "kind", SpellVariableValueKind.Int);
            Scribe_Values.Look(ref intValue, "intValue");
            Scribe_Values.Look(ref floatValue, "floatValue");
            Scribe_Values.Look(ref boolValue, "boolValue");
            Scribe_Values.Look(ref stringValue, "stringValue");
        }
    }

    private enum SpellVariableValueKind
    {
        Int,
        Float,
        Bool,
        String
    }
}
