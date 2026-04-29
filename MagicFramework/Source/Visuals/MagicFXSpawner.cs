using MagicFramework.Context;
using MagicFramework.Definitions;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MagicFramework.Visuals;

public static class MagicFXSpawner
{
    public static bool Play(SpellContext context, MagicFXEvent fxEvent, SpellEffectLocationSource locationSource)
    {
        MagicFXPackage package = MagicFXResolver.Resolve(context?.spellDef, context, fxEvent);
        return Play(context, package, locationSource);
    }

    public static bool Play(SpellContext context, MagicFXPackage package, SpellEffectLocationSource locationSource)
    {
        if (context?.map == null || package == null)
        {
            return false;
        }

        TargetInfo targetInfo = ResolveTargetInfo(context, locationSource);
        bool playedAny = false;

        if (!string.IsNullOrWhiteSpace(package.effectDef))
        {
            EffecterDef effecterDef = DefDatabase<EffecterDef>.GetNamedSilentFail(package.effectDef);
            if (effecterDef != null)
            {
                Effecter effecter = effecterDef.Spawn();
                effecter?.Trigger(targetInfo, targetInfo);
                effecter?.Cleanup();
                playedAny = true;
            }
        }

        if (!string.IsNullOrWhiteSpace(package.fleckDef) && TryResolveFleckLocation(context, locationSource, out Vector3 location, out Map map))
        {
            FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(package.fleckDef);
            if (fleckDef != null)
            {
                FleckCreationData data = FleckMaker.GetDataStatic(location, map, fleckDef, Mathf.Max(0.1f, package.scale));
                if (TryParseColor(package.colorHex, out Color color))
                {
                    data.instanceColor = color;
                }

                map.flecks.CreateFleck(data);
                playedAny = true;
            }
        }

        if (!string.IsNullOrWhiteSpace(package.soundDef))
        {
            SoundDef soundDef = DefDatabase<SoundDef>.GetNamedSilentFail(package.soundDef);
            if (soundDef != null)
            {
                SoundStarter.PlayOneShot(soundDef, targetInfo);
                playedAny = true;
            }
        }

        if (playedAny)
        {
            Log.Message($"[MagicFramework] Played procedural FX for {context.spellDef?.defName ?? "<null spell>"}.");
        }

        return playedAny;
    }

    private static bool TryResolveFleckLocation(SpellContext context, SpellEffectLocationSource locationSource, out Vector3 location, out Map map)
    {
        location = default;
        map = context?.map;

        Thing thing = locationSource switch
        {
            SpellEffectLocationSource.CurrentTarget => context?.currentTarget.Thing,
            SpellEffectLocationSource.InitialTarget => context?.initialTarget.Thing,
            SpellEffectLocationSource.Caster => context?.caster,
            _ => null
        };

        if (thing != null && thing.Spawned)
        {
            location = thing.DrawPos;
            map = thing.Map;
            return map != null;
        }

        IntVec3 cell = locationSource switch
        {
            SpellEffectLocationSource.CurrentCell => context?.currentCell ?? IntVec3.Invalid,
            SpellEffectLocationSource.CurrentTarget => context?.currentTarget.Cell ?? IntVec3.Invalid,
            SpellEffectLocationSource.InitialTarget => context?.initialTarget.Cell ?? IntVec3.Invalid,
            SpellEffectLocationSource.Caster => context?.caster?.Position ?? IntVec3.Invalid,
            _ => IntVec3.Invalid
        };

        if (cell.IsValid && map != null)
        {
            location = cell.ToVector3Shifted();
            return true;
        }

        return false;
    }

    private static TargetInfo ResolveTargetInfo(SpellContext context, SpellEffectLocationSource locationSource)
    {
        Thing thing = locationSource switch
        {
            SpellEffectLocationSource.CurrentTarget => context?.currentTarget.Thing,
            SpellEffectLocationSource.InitialTarget => context?.initialTarget.Thing,
            SpellEffectLocationSource.Caster => context?.caster,
            _ => null
        };

        if (thing != null && thing.Spawned)
        {
            return new TargetInfo(thing);
        }

        IntVec3 cell = locationSource switch
        {
            SpellEffectLocationSource.CurrentCell => context?.currentCell ?? IntVec3.Invalid,
            SpellEffectLocationSource.CurrentTarget => context?.currentTarget.Cell ?? IntVec3.Invalid,
            SpellEffectLocationSource.InitialTarget => context?.initialTarget.Cell ?? IntVec3.Invalid,
            SpellEffectLocationSource.Caster => context?.caster?.Position ?? IntVec3.Invalid,
            _ => IntVec3.Invalid
        };

        return new TargetInfo(cell, context?.map);
    }

    private static bool TryParseColor(string colorHex, out Color color)
    {
        color = Color.white;
        if (string.IsNullOrWhiteSpace(colorHex))
        {
            return false;
        }

        string normalized = colorHex.StartsWith("#") ? colorHex : "#" + colorHex;
        return ColorUtility.TryParseHtmlString(normalized, out color);
    }
}
