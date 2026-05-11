using System.Collections.Generic;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Targeting;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Owns persistent wall hazard zones for a single map.
/// </summary>
public sealed class PersistentWallZoneMapComponent : MapComponent
{
    private readonly SpellActionRunner actionRunner = new();
    private List<PersistentWallZone> wallZones = new();

    public PersistentWallZoneMapComponent(Map map)
        : base(map)
    {
    }

    public bool Register(PersistentWallZone wallZone, bool replaceExistingForCaster)
    {
        if (wallZone == null)
        {
            return false;
        }

        wallZones ??= new List<PersistentWallZone>();
        if (replaceExistingForCaster)
        {
            RemoveForCasterSpell(wallZone.Caster, wallZone.SpellDef);
        }

        wallZones.Add(wallZone);
        RunWallZoneLifecycleActions(wallZone, WallZoneLifecycleEvent.Create);
        return true;
    }

    public void RemoveForCasterSpell(Thing caster, SpellDef spellDef)
    {
        if (wallZones == null)
        {
            return;
        }

        for (int i = wallZones.Count - 1; i >= 0; i--)
        {
            PersistentWallZone wallZone = wallZones[i];
            if (wallZone?.Caster == caster && wallZone.SpellDef == spellDef)
            {
                RunWallZoneLifecycleActions(wallZone, WallZoneLifecycleEvent.Remove);
                wallZone.DestroyMarkers();
                wallZones.RemoveAt(i);
            }
        }
    }

    public override void MapComponentTick()
    {
        if (wallZones == null || wallZones.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = wallZones.Count - 1; i >= 0; i--)
        {
            PersistentWallZone wallZone = wallZones[i];
            if (wallZone == null || wallZone.Caster != null && wallZone.Caster.Destroyed)
            {
                RunWallZoneLifecycleActions(wallZone, WallZoneLifecycleEvent.Break);
                wallZone?.DestroyMarkers();
                wallZones.RemoveAt(i);
                continue;
            }

            if (!HasAnyActiveMarker(wallZone))
            {
                RunWallZoneLifecycleActions(wallZone, WallZoneLifecycleEvent.Break);
                wallZones.RemoveAt(i);
                continue;
            }

            if (wallZone.IsExpired(currentTick))
            {
                RunWallZoneLifecycleActions(wallZone, WallZoneLifecycleEvent.Expire);
                wallZone.DestroyMarkers();
                wallZones.RemoveAt(i);
                continue;
            }

            if (wallZone.NextPulseTick > currentTick)
            {
                continue;
            }

            PulseWallZone(wallZone);
            wallZone.ScheduleNextPulse();
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref wallZones, "wallZones", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && wallZones == null)
        {
            wallZones = new List<PersistentWallZone>();
        }
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new();
        builder.Append("[MagicFramework] Wall zone runtime for map ");
        builder.Append(map?.Index ?? -1);
        builder.Append(": ");
        builder.Append(wallZones?.Count ?? 0);
        builder.Append(" active wall zone(s).");

        if (wallZones == null || wallZones.Count == 0)
        {
            return builder.ToString();
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = 0; i < wallZones.Count; i++)
        {
            PersistentWallZone wallZone = wallZones[i];
            builder.AppendLine();
            builder.Append("  [");
            builder.Append(i);
            builder.Append("] spell=");
            builder.Append(wallZone?.SpellDef?.defName ?? "<null>");
            builder.Append(" cells=");
            builder.Append(wallZone?.WallCells?.Count ?? 0);
            builder.Append(" nextPulseIn=");
            builder.Append(wallZone == null ? -1 : wallZone.NextPulseTick - currentTick);
            builder.Append(" expiresIn=");
            builder.Append(wallZone == null || wallZone.ExpireAtTick < 0 ? -1 : wallZone.ExpireAtTick - currentTick);
        }

        return builder.ToString();
    }

    private void PulseWallZone(PersistentWallZone wallZone)
    {
        if (!wallZone.TryResolveActionDef(out PersistentWallZoneActionDef actionDef))
        {
            Log.Warning("[MagicFramework] Dropped wall zone because its authored node could not be resolved.");
            return;
        }

        RunWallZoneLifecycleActions(wallZone, WallZoneLifecycleEvent.Pulse, actionDef);

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

        HashSet<Pawn> affectedPawns = new();
        foreach (IntVec3 wallCell in wallZone.WallCells)
        {
            foreach (Pawn pawn in candidatePawns)
            {
                if (pawn.Destroyed || affectedPawns.Contains(pawn))
                {
                    continue;
                }

                if (!wallZone.IncludeCaster && pawn == wallZone.Caster)
                {
                    continue;
                }

                if (!TargetQueryUtility.MatchesPawnAffinity(wallZone.Caster, pawn, wallZone.PawnAffinity))
                {
                    continue;
                }

                if (pawn.Position.DistanceTo(wallCell) > wallZone.PulseRadius)
                {
                    continue;
                }

                if (!wallZone.TryCreateExecutionContext(map, pawn, wallCell, out SpellContext context))
                {
                    continue;
                }

                affectedPawns.Add(pawn);
                actionRunner.RunActions(context, actionDef.actions);
            }
        }
    }

    private void RunWallZoneLifecycleActions(PersistentWallZone wallZone, WallZoneLifecycleEvent lifecycleEvent, PersistentWallZoneActionDef resolvedActionDef = null)
    {
        if (wallZone == null ||
            (resolvedActionDef == null && !wallZone.TryResolveActionDef(out resolvedActionDef)) ||
            !wallZone.TryCreateCenterExecutionContext(map, out SpellContext context))
        {
            return;
        }

        List<SpellActionDef> specificActions = lifecycleEvent switch
        {
            WallZoneLifecycleEvent.Create => resolvedActionDef.onCreateActions,
            WallZoneLifecycleEvent.Pulse => resolvedActionDef.onPulseActions,
            WallZoneLifecycleEvent.Expire => resolvedActionDef.onExpireActions,
            WallZoneLifecycleEvent.Remove => resolvedActionDef.onRemoveActions,
            WallZoneLifecycleEvent.Break => resolvedActionDef.onBreakActions,
            _ => null
        };

        if (specificActions != null && specificActions.Count > 0)
        {
            actionRunner.RunActions(context, specificActions);
        }
    }

    private static bool HasAnyActiveMarker(PersistentWallZone wallZone)
    {
        if (wallZone?.MarkerThings == null)
        {
            return false;
        }

        foreach (Thing markerThing in wallZone.MarkerThings)
        {
            if (markerThing != null && !markerThing.Destroyed)
            {
                return true;
            }
        }

        return false;
    }

    private enum WallZoneLifecycleEvent
    {
        Create,
        Pulse,
        Expire,
        Remove,
        Break
    }
}
