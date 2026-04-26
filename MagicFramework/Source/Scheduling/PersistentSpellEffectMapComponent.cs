using System.Collections.Generic;
using System.Text;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Owns persistent spell markers for a single map and cleans them up on expiry or explicit removal.
/// </summary>
public sealed class PersistentSpellEffectMapComponent : MapComponent
{
    private List<PersistentSpellEffect> persistentEffects = new();

    public PersistentSpellEffectMapComponent(Map map)
        : base(map)
    {
    }

    public bool Register(PersistentSpellEffect persistentEffect, bool replaceExistingForCaster)
    {
        if (persistentEffect == null)
        {
            return false;
        }

        persistentEffects ??= new List<PersistentSpellEffect>();
        if (replaceExistingForCaster)
        {
            RemoveForCasterSpell(persistentEffect.Caster, persistentEffect.SpellDef);
        }

        persistentEffects.Add(persistentEffect);
        return true;
    }

    public void RemoveForCasterSpell(Thing caster, SpellDef spellDef)
    {
        if (persistentEffects == null)
        {
            return;
        }

        for (int i = persistentEffects.Count - 1; i >= 0; i--)
        {
            PersistentSpellEffect effect = persistentEffects[i];
            if (effect?.Caster == caster && effect.SpellDef == spellDef)
            {
                DestroyMarker(effect);
                persistentEffects.RemoveAt(i);
            }
        }
    }

    public override void MapComponentTick()
    {
        if (persistentEffects == null || persistentEffects.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = persistentEffects.Count - 1; i >= 0; i--)
        {
            PersistentSpellEffect effect = persistentEffects[i];
            if (effect == null)
            {
                persistentEffects.RemoveAt(i);
                continue;
            }

            if (effect.MarkerThing == null || effect.MarkerThing.Destroyed)
            {
                persistentEffects.RemoveAt(i);
                continue;
            }

            if (effect.IsExpired(currentTick))
            {
                DestroyMarker(effect);
                persistentEffects.RemoveAt(i);
            }
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref persistentEffects, "persistentEffects", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && persistentEffects == null)
        {
            persistentEffects = new List<PersistentSpellEffect>();
        }
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new();
        builder.Append("[MagicFramework] Persistent effect runtime for map ");
        builder.Append(map?.Index ?? -1);
        builder.Append(": ");
        builder.Append(persistentEffects?.Count ?? 0);
        builder.Append(" active marker(s).");

        if (persistentEffects == null || persistentEffects.Count == 0)
        {
            return builder.ToString();
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = 0; i < persistentEffects.Count; i++)
        {
            PersistentSpellEffect effect = persistentEffects[i];
            builder.AppendLine();
            builder.Append("  [");
            builder.Append(i);
            builder.Append("] cell=");
            builder.Append(effect?.Cell ?? IntVec3.Invalid);
            builder.Append(" spell=");
            builder.Append(effect?.SpellDef?.defName ?? "<null>");
            builder.Append(" marker=");
            builder.Append(effect?.MarkerThing?.def?.defName ?? "<null>");
            builder.Append(" expiresIn=");
            builder.Append(effect == null || effect.ExpireAtTick < 0 ? -1 : effect.ExpireAtTick - currentTick);
        }

        return builder.ToString();
    }

    private static void DestroyMarker(PersistentSpellEffect effect)
    {
        if (effect?.MarkerThing != null && !effect.MarkerThing.Destroyed)
        {
            effect.MarkerThing.Destroy();
        }
    }
}
