using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Visuals;

public static class MagicFXResolver
{
    private const string FallbackElementDefName = "Arcane";

    public static MagicFXPackage Resolve(SpellDef spellDef, SpellContext context, MagicFXEvent fxEvent)
    {
        if (spellDef == null || spellDef.disableProceduralFx)
        {
            return null;
        }

        MagicFXEvent resolvedEvent = fxEvent == MagicFXEvent.Auto ? InferEvent(spellDef) : fxEvent;
        MagicFXPackage package = ResolveOverride(spellDef, resolvedEvent) ?? ResolveElement(spellDef, resolvedEvent);
        if (package == null)
        {
            package = ResolveFallback(resolvedEvent);
        }

        if (package == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(spellDef.fxColorOverride))
        {
            package.colorHex = spellDef.fxColorOverride;
        }

        float spellMultiplier = spellDef.fxIntensityMultiplier > 0f ? spellDef.fxIntensityMultiplier : 1f;
        float tierMultiplier = 1f + (0.15f * (context?.power?.tier ?? 0));
        package.scale *= spellMultiplier * tierMultiplier;
        return package;
    }

    private static MagicFXPackage ResolveOverride(SpellDef spellDef, MagicFXEvent fxEvent)
    {
        if (string.IsNullOrWhiteSpace(spellDef.fxOverride))
        {
            return null;
        }

        MagicFXDef fxDef = DefDatabase<MagicFXDef>.GetNamedSilentFail(spellDef.fxOverride);
        return fxDef == null ? null : FromProfile(fxDef, fxEvent);
    }

    private static MagicFXPackage ResolveElement(SpellDef spellDef, MagicFXEvent fxEvent)
    {
        string elementDefName = string.IsNullOrWhiteSpace(spellDef.element)
            ? FallbackElementDefName
            : spellDef.element;
        MagicElementDef elementDef = DefDatabase<MagicElementDef>.GetNamedSilentFail(elementDefName)
            ?? DefDatabase<MagicElementDef>.GetNamedSilentFail(FallbackElementDefName);
        return elementDef == null ? null : FromElement(elementDef, fxEvent);
    }

    private static MagicFXPackage ResolveFallback(MagicFXEvent fxEvent)
    {
        MagicFXDef fallbackDef = DefDatabase<MagicFXDef>.GetNamedSilentFail("MF_GenericMagicFX");
        return fallbackDef == null ? null : FromProfile(fallbackDef, fxEvent);
    }

    private static MagicFXEvent InferEvent(SpellDef spellDef)
    {
        if (IsText(spellDef.effectShape, "Explosion") || spellDef.targeting?.shape == SpellTargetShape.Radius)
        {
            return MagicFXEvent.Explosion;
        }

        if (IsText(spellDef.effectShape, "Pulse") || spellDef.targeting?.shape == SpellTargetShape.Line || spellDef.targeting?.shape == SpellTargetShape.Wall)
        {
            return MagicFXEvent.AreaPulse;
        }

        if (IsText(spellDef.effectShape, "Continuous") || IsText(spellDef.effectShape, "Orbiting"))
        {
            return MagicFXEvent.SustainStart;
        }

        return MagicFXEvent.Impact;
    }

    private static MagicFXPackage FromElement(MagicElementDef elementDef, MagicFXEvent fxEvent)
    {
        return new MagicFXPackage
        {
            fleckDef = SelectFleck(elementDef.castFleckDef, elementDef.impactFleckDef, elementDef.areaFleckDef, elementDef.sustainFleckDef, fxEvent),
            effectDef = SelectEffect(elementDef.castEffectDef, elementDef.impactEffectDef, elementDef.explosionEffectDef, fxEvent),
            soundDef = SelectSound(elementDef.castSoundDef, elementDef.impactSoundDef, elementDef.explosionSoundDef, fxEvent),
            colorHex = elementDef.primaryColorHex,
            scale = elementDef.scale <= 0f ? 1f : elementDef.scale
        };
    }

    private static MagicFXPackage FromProfile(MagicFXDef fxDef, MagicFXEvent fxEvent)
    {
        return new MagicFXPackage
        {
            fleckDef = SelectFleck(fxDef.castFleckDef, fxDef.impactFleckDef, fxDef.areaFleckDef, fxDef.sustainFleckDef, fxEvent),
            effectDef = SelectEffect(fxDef.castEffectDef, fxDef.impactEffectDef, fxDef.explosionEffectDef, fxEvent),
            soundDef = SelectSound(fxDef.castSoundDef, fxDef.impactSoundDef, fxDef.explosionSoundDef, fxEvent),
            colorHex = fxDef.primaryColorHex,
            scale = fxDef.scale <= 0f ? 1f : fxDef.scale
        };
    }

    private static string SelectFleck(string castFleck, string impactFleck, string areaFleck, string sustainFleck, MagicFXEvent fxEvent)
    {
        return fxEvent switch
        {
            MagicFXEvent.CastStart or MagicFXEvent.ProjectileLaunch => castFleck ?? impactFleck,
            MagicFXEvent.AreaPulse or MagicFXEvent.Explosion => areaFleck ?? impactFleck ?? castFleck,
            MagicFXEvent.SustainStart or MagicFXEvent.SustainTick or MagicFXEvent.SustainEnd => sustainFleck ?? areaFleck ?? impactFleck,
            _ => impactFleck ?? castFleck
        };
    }

    private static string SelectEffect(string castEffect, string impactEffect, string explosionEffect, MagicFXEvent fxEvent)
    {
        return fxEvent switch
        {
            MagicFXEvent.CastStart or MagicFXEvent.ProjectileLaunch => castEffect ?? impactEffect,
            MagicFXEvent.Explosion => explosionEffect ?? impactEffect ?? castEffect,
            _ => impactEffect ?? castEffect
        };
    }

    private static string SelectSound(string castSound, string impactSound, string explosionSound, MagicFXEvent fxEvent)
    {
        return fxEvent switch
        {
            MagicFXEvent.CastStart or MagicFXEvent.ProjectileLaunch => castSound ?? impactSound,
            MagicFXEvent.Explosion => explosionSound ?? impactSound ?? castSound,
            _ => impactSound ?? castSound
        };
    }

    private static bool IsText(string value, string expected)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Equals(expected, System.StringComparison.OrdinalIgnoreCase);
    }
}
