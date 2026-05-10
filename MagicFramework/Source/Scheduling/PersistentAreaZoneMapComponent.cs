using System.Collections.Generic;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Targeting;
using Verse;
using Verse.Sound;

namespace MagicFramework.Scheduling;

/// <summary>
/// Owns persistent circular area zones for a single map.
/// </summary>
public sealed class PersistentAreaZoneMapComponent : MapComponent
{
    private readonly SpellActionRunner actionRunner = new();
    private List<PersistentAreaZone> areaZones = new();

    public PersistentAreaZoneMapComponent(Map map)
        : base(map)
    {
    }

    public bool Register(PersistentAreaZone areaZone, bool replaceExistingForCaster)
    {
        if (areaZone == null)
        {
            return false;
        }

        areaZones ??= new List<PersistentAreaZone>();
        if (replaceExistingForCaster)
        {
            RemoveForCasterSpell(areaZone.Caster, areaZone.SpellDef);
        }

        areaZones.Add(areaZone);
        RunAreaZoneLifecycleActions(areaZone, AreaZoneLifecycleEvent.Create);
        return true;
    }

    public int RemoveForCasterSpell(Thing caster, SpellDef spellDef)
    {
        if (areaZones == null)
        {
            return 0;
        }

        int removedCount = 0;
        for (int i = areaZones.Count - 1; i >= 0; i--)
        {
            PersistentAreaZone areaZone = areaZones[i];
            if (areaZone?.Caster == caster && areaZone.SpellDef == spellDef)
            {
                RunAreaZoneLifecycleActions(areaZone, AreaZoneLifecycleEvent.Remove);
                areaZone.DestroyMarkers();
                areaZones.RemoveAt(i);
                removedCount++;
            }
        }

        return removedCount;
    }

    public bool HasForCasterSpell(Thing caster, SpellDef spellDef)
    {
        if (caster == null || spellDef == null || areaZones == null)
        {
            return false;
        }

        for (int i = 0; i < areaZones.Count; i++)
        {
            PersistentAreaZone areaZone = areaZones[i];
            if (areaZone?.Caster == caster && areaZone.SpellDef == spellDef)
            {
                return true;
            }
        }

        return false;
    }

    public override void MapComponentTick()
    {
        if (areaZones == null || areaZones.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = areaZones.Count - 1; i >= 0; i--)
        {
            PersistentAreaZone areaZone = areaZones[i];
            if (areaZone == null || areaZone.Caster != null && areaZone.Caster.Destroyed)
            {
                RunAreaZoneLifecycleActions(areaZone, AreaZoneLifecycleEvent.Break);
                areaZone?.DestroyMarkers();
                areaZones.RemoveAt(i);
                continue;
            }

            if (areaZone.IsConcentrationBroken(out string breakReason))
            {
                MagicLog.Message(MagicLogSubsystem.AreaZones, $"[MagicFramework] Area zone {areaZone.SpellDef?.defName ?? "<unknown spell>"} ended because concentration broke: {breakReason ?? "unknown reason"}.");
                RunAreaZoneLifecycleActions(areaZone, AreaZoneLifecycleEvent.Break);
                areaZone.DestroyMarkers();
                areaZones.RemoveAt(i);
                continue;
            }

            if (!HasAnyActiveMarker(areaZone))
            {
                RunAreaZoneLifecycleActions(areaZone, AreaZoneLifecycleEvent.Break);
                areaZones.RemoveAt(i);
                continue;
            }

            if (areaZone.IsExpired(currentTick))
            {
                RunAreaZoneLifecycleActions(areaZone, AreaZoneLifecycleEvent.Expire);
                areaZone.DestroyMarkers();
                areaZones.RemoveAt(i);
                continue;
            }

            if (areaZone.NextPulseTick > currentTick)
            {
            }
            else
            {
                PulseAreaZone(areaZone);
                areaZone.ScheduleNextPulse();
            }

            if (areaZone.NextVisualTick <= currentTick)
            {
                EmitAreaZoneVisuals(areaZone);
                areaZone.ScheduleNextVisualPulse();
            }
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref areaZones, "areaZones", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && areaZones == null)
        {
            areaZones = new List<PersistentAreaZone>();
        }
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new();
        builder.Append("[MagicFramework] Area zone runtime for map ");
        builder.Append(map?.Index ?? -1);
        builder.Append(": ");
        builder.Append(areaZones?.Count ?? 0);
        builder.Append(" active area zone(s).");

        if (areaZones == null || areaZones.Count == 0)
        {
            return builder.ToString();
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = 0; i < areaZones.Count; i++)
        {
            PersistentAreaZone areaZone = areaZones[i];
            builder.AppendLine();
            builder.Append("  [");
            builder.Append(i);
            builder.Append("] spell=");
            builder.Append(areaZone?.SpellDef?.defName ?? "<null>");
            builder.Append(" center=");
            builder.Append(areaZone?.CenterCell ?? IntVec3.Invalid);
            builder.Append(" radius=");
            builder.Append(areaZone?.ZoneRadius ?? 0f);
            builder.Append(" markers=");
            builder.Append(areaZone?.MarkerThings?.Count ?? 0);
            builder.Append(" nextVisualIn=");
            builder.Append(areaZone == null ? -1 : areaZone.NextVisualTick - currentTick);
            builder.Append(" nextPulseIn=");
            builder.Append(areaZone == null ? -1 : areaZone.NextPulseTick - currentTick);
            builder.Append(" expiresIn=");
            builder.Append(areaZone == null || areaZone.ExpireAtTick < 0 ? -1 : areaZone.ExpireAtTick - currentTick);
        }

        return builder.ToString();
    }

    private void PulseAreaZone(PersistentAreaZone areaZone)
    {
        if (!areaZone.TryResolveActionDef(out PersistentAreaZoneActionDef actionDef))
        {
            Log.Warning("[MagicFramework] Dropped area zone because its authored node could not be resolved.");
            return;
        }

        RunAreaZoneLifecycleActions(areaZone, AreaZoneLifecycleEvent.Pulse, actionDef);

        if (areaZone.PulseAtCenter && areaZone.TryCreateCenterExecutionContext(map, out SpellContext centerContext))
        {
            actionRunner.RunActions(centerContext, actionDef.actions);
        }

        List<Pawn> candidatePawns = new();
        List<Thing> allThings = map.listerThings?.AllThings;
        if (allThings != null)
        {
            foreach (Thing thing in allThings)
            {
                if (thing is Pawn pawn && !pawn.Destroyed)
                {
                    candidatePawns.Add(pawn);
                }
            }
        }

        foreach (Pawn pawn in candidatePawns)
        {
            if (!areaZone.IncludeCaster && pawn == areaZone.Caster)
            {
                continue;
            }

            if (!TargetQueryUtility.MatchesPawnAffinity(areaZone.Caster, pawn, areaZone.PawnAffinity))
            {
                continue;
            }

            if (pawn.Position.DistanceTo(areaZone.CenterCell) > areaZone.ZoneRadius)
            {
                continue;
            }

            if (!areaZone.TryCreateExecutionContext(map, pawn, out SpellContext context))
            {
                continue;
            }

            actionRunner.RunActions(context, actionDef.actions);
        }
    }

    private void RunAreaZoneLifecycleActions(PersistentAreaZone areaZone, AreaZoneLifecycleEvent lifecycleEvent, PersistentAreaZoneActionDef resolvedActionDef = null)
    {
        if (areaZone == null ||
            (resolvedActionDef == null && !areaZone.TryResolveActionDef(out resolvedActionDef)) ||
            !areaZone.TryCreateCenterExecutionContext(map, out SpellContext context))
        {
            return;
        }

        List<SpellActionDef> specificActions = lifecycleEvent switch
        {
            AreaZoneLifecycleEvent.Create => resolvedActionDef.onCreateActions,
            AreaZoneLifecycleEvent.Pulse => resolvedActionDef.onPulseActions,
            AreaZoneLifecycleEvent.Expire => resolvedActionDef.onExpireActions,
            AreaZoneLifecycleEvent.Remove => resolvedActionDef.onRemoveActions,
            AreaZoneLifecycleEvent.Break => resolvedActionDef.onBreakActions,
            _ => null
        };

        if (specificActions != null && specificActions.Count > 0)
        {
            actionRunner.RunActions(context, specificActions);
        }

        if (IsTerminalLifecycleEvent(lifecycleEvent) && resolvedActionDef.onEndActions != null && resolvedActionDef.onEndActions.Count > 0)
        {
            actionRunner.RunActions(context, resolvedActionDef.onEndActions);
        }
    }

    private static bool IsTerminalLifecycleEvent(AreaZoneLifecycleEvent lifecycleEvent)
    {
        return lifecycleEvent == AreaZoneLifecycleEvent.Expire
            || lifecycleEvent == AreaZoneLifecycleEvent.Remove
            || lifecycleEvent == AreaZoneLifecycleEvent.Break;
    }

    private static bool HasAnyActiveMarker(PersistentAreaZone areaZone)
    {
        if (areaZone?.MarkerThings == null)
        {
            return false;
        }

        foreach (Thing markerThing in areaZone.MarkerThings)
        {
            if (markerThing != null && !markerThing.Destroyed)
            {
                return true;
            }
        }

        return false;
    }

    private void EmitAreaZoneVisuals(PersistentAreaZone areaZone)
    {
        if (string.IsNullOrWhiteSpace(areaZone?.AmbientEffectDef) && string.IsNullOrWhiteSpace(areaZone?.AmbientSoundDef))
        {
            return;
        }

        List<TargetInfo> targets = new();
        if (areaZone.EmitVisualFromMarkers && areaZone.MarkerThings != null)
        {
            int markerLimit = areaZone.MaxVisualMarkersPerPulse;
            int markerCount = areaZone.MarkerThings.Count;
            int step = markerLimit > 0 && markerCount > markerLimit
                ? (markerCount + markerLimit - 1) / markerLimit
                : 1;

            for (int i = 0; i < markerCount; i += step)
            {
                Thing markerThing = areaZone.MarkerThings[i];
                if (markerThing != null && !markerThing.Destroyed)
                {
                    targets.Add(new TargetInfo(markerThing));
                }
            }
        }

        if (targets.Count == 0)
        {
            targets.Add(new TargetInfo(areaZone.CenterCell, map));
        }

        EffecterDef effecterDef = string.IsNullOrWhiteSpace(areaZone.AmbientEffectDef)
            ? null
            : DefDatabase<EffecterDef>.GetNamedSilentFail(areaZone.AmbientEffectDef);
        SoundDef soundDef = string.IsNullOrWhiteSpace(areaZone.AmbientSoundDef)
            ? null
            : DefDatabase<SoundDef>.GetNamedSilentFail(areaZone.AmbientSoundDef);

        foreach (TargetInfo targetInfo in targets)
        {
            if (effecterDef != null)
            {
                Effecter effecter = effecterDef.Spawn();
                effecter?.Trigger(targetInfo, targetInfo);
                effecter?.Cleanup();
            }

            if (soundDef != null)
            {
                SoundStarter.PlayOneShot(soundDef, targetInfo);
            }
        }
    }

    private enum AreaZoneLifecycleEvent
    {
        Create,
        Pulse,
        Expire,
        Remove,
        Break
    }
}
