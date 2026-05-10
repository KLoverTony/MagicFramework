using System.Collections.Generic;
using System.Text;
using MagicFramework.Context;
using MagicFramework.Core;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Runs authored impact actions after real RimWorld projectiles finish their lifecycle.
/// </summary>
public sealed class ProjectileImpactMapComponent : MapComponent
{
    private readonly SpellActionRunner actionRunner = new();
    private List<PendingProjectileImpact> pendingImpacts = new();

    public ProjectileImpactMapComponent(Map map)
        : base(map)
    {
    }

    public int PendingImpactCount => pendingImpacts?.Count ?? 0;

    public bool Enqueue(PendingProjectileImpact pendingImpact)
    {
        if (pendingImpact == null || !pendingImpact.HasActions)
        {
            return false;
        }

        pendingImpacts ??= new List<PendingProjectileImpact>();
        pendingImpacts.Add(pendingImpact);
        return true;
    }

    public override void MapComponentTick()
    {
        if (pendingImpacts == null || pendingImpacts.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        for (int i = pendingImpacts.Count - 1; i >= 0; i--)
        {
            PendingProjectileImpact pendingImpact = pendingImpacts[i];
            if (pendingImpact == null)
            {
                pendingImpacts.RemoveAt(i);
                continue;
            }

            pendingImpact.RefreshProjectileCell();
            if (!pendingImpact.IsReadyToResolve(currentTick))
            {
                continue;
            }

            pendingImpacts.RemoveAt(i);
            ExecuteImpactActions(pendingImpact);
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref pendingImpacts, "pendingImpacts", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && pendingImpacts == null)
        {
            pendingImpacts = new List<PendingProjectileImpact>();
        }
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new();
        builder.Append("[MagicFramework] Projectile impact runtime for map ");
        builder.Append(map?.Index ?? -1);
        builder.Append(": ");
        builder.Append(PendingImpactCount);
        builder.Append(" pending impact(s).");
        return builder.ToString();
    }

    private void ExecuteImpactActions(PendingProjectileImpact pendingImpact)
    {
        if (!pendingImpact.TryCreateExecutionContext(map, out SpellContext context))
        {
            Log.Warning($"[MagicFramework] Dropped {pendingImpact.DebugLabel} because its execution context could not be rebuilt.");
            return;
        }

        if (context.caster != null && context.caster.Destroyed)
        {
            MagicLog.Message(MagicLogSubsystem.Projectiles, $"[MagicFramework] Skipped {pendingImpact.DebugLabel} because the caster no longer exists.");
            return;
        }

        MagicLog.Message(MagicLogSubsystem.Projectiles, $"[MagicFramework] Executing {pendingImpact.DebugLabel} at {context.currentCell}.");
        foreach (SpellActionDef actionDef in pendingImpact.ResolveActions())
        {
            if (context.executionState.cancelled || context.executionState.failed)
            {
                return;
            }

            actionRunner.RunAction(context, actionDef);
        }
    }
}
