using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Scheduling;

/// <summary>
/// Tracks child actions that should run when a launched projectile resolves.
/// </summary>
public sealed class PendingProjectileImpact : IExposable
{
    private List<ProjectileImpactActionPath> actionPaths = new();
    private Thing caster;
    private IntVec3 currentCell = IntVec3.Invalid;
    private LocalTargetInfo currentTarget;
    private List<LocalTargetInfo> currentTargets = new();
    private LocalTargetInfo initialTarget;
    private bool impactBlockedByShield;
    private bool impactCaptured;
    private Thing impactHitThing;
    private ProjectileImpactResultKind impactResult = ProjectileImpactResultKind.Pending;
    private IntVec3 lastKnownProjectileCell = IntVec3.Invalid;
    private float powerValue;
    private int powerTier;
    private Thing projectile;
    private int randomSeed;
    private SpellDef spellDef;
    private int timeoutTick;
    private SpellVariableStore variables = new();

    public PendingProjectileImpact()
    {
    }

    public PendingProjectileImpact(
        Projectile projectile,
        int timeoutTick,
        SpellContext context,
        IEnumerable<SpellActionDef> impactActions)
    {
        this.projectile = projectile;
        this.timeoutTick = timeoutTick;
        spellDef = context?.spellDef;
        caster = context?.caster;
        initialTarget = context?.initialTarget ?? LocalTargetInfo.Invalid;
        currentTarget = context?.currentTarget ?? LocalTargetInfo.Invalid;
        currentTargets = context?.currentTargets != null ? new List<LocalTargetInfo>(context.currentTargets) : new List<LocalTargetInfo>();
        currentCell = context?.currentCell ?? IntVec3.Invalid;
        powerValue = context?.power?.value ?? 0f;
        powerTier = context?.power?.tier ?? 0;
        randomSeed = context?.randomSeed ?? 0;
        variables = context?.executionState?.variables?.Clone() ?? new SpellVariableStore();
        lastKnownProjectileCell = projectile?.Position ?? currentCell;

        if (impactActions == null || spellDef == null)
        {
            return;
        }

        foreach (SpellActionDef impactAction in impactActions)
        {
            if (SpellActionPathUtility.TryCreatePath(spellDef, impactAction, out List<int> actionPath))
            {
                actionPaths.Add(new ProjectileImpactActionPath(actionPath));
            }
            else
            {
                Log.Warning($"[MagicFramework] Could not resolve projectile impact action path for {impactAction?.GetType().Name ?? "<null>"}.");
            }
        }
    }

    public Thing Projectile => projectile;

    public int TimeoutTick => timeoutTick;

    public IntVec3 LastKnownProjectileCell => lastKnownProjectileCell;

    public bool HasActions => actionPaths != null && actionPaths.Count > 0;

    public string DebugLabel => $"{spellDef?.defName ?? "<unknown spell>"} projectile impact";

    public void MarkImpact(Thing hitThing, bool blockedByShield)
    {
        impactCaptured = true;
        impactBlockedByShield = blockedByShield;
        impactHitThing = hitThing;
        impactResult = ResolveCapturedImpactResult(hitThing, blockedByShield);
        if (hitThing != null && !hitThing.Destroyed)
        {
            lastKnownProjectileCell = hitThing.Position;
        }
        else if (projectile != null)
        {
            lastKnownProjectileCell = projectile.Position;
        }
    }

    public void RefreshProjectileCell()
    {
        if (projectile != null && projectile.Spawned)
        {
            lastKnownProjectileCell = projectile.Position;
        }
    }

    public bool IsReadyToResolve(int currentTick)
    {
        if (impactCaptured)
        {
            return true;
        }

        if (currentTick >= timeoutTick)
        {
            impactResult = ProjectileImpactResultKind.Timeout;
            return true;
        }

        if (projectile == null || projectile.Destroyed)
        {
            impactResult = ProjectileImpactResultKind.Destroyed;
            return true;
        }

        return false;
    }

    public bool TryCreateExecutionContext(Map map, out SpellContext context)
    {
        context = null;
        if (spellDef == null || map == null)
        {
            return false;
        }

        IntVec3 impactCell = lastKnownProjectileCell.IsValid ? lastKnownProjectileCell : currentCell;
        LocalTargetInfo resolvedCurrentTarget = ResolveCurrentTargetAtImpact(impactCell);

        context = new SpellContext
        {
            caster = caster,
            map = map,
            spellDef = spellDef,
            initialTarget = initialTarget,
            currentTarget = resolvedCurrentTarget,
            currentCell = impactCell,
            power = new SpellPowerContext
            {
                value = powerValue,
                tier = powerTier
            },
            randomSeed = randomSeed
        };
        context.executionState.costsApplied = true;
        context.executionState.variables = variables?.Clone() ?? new SpellVariableStore();
        context.executionState.variables.SetValue("ProjectileImpactCaptured", impactCaptured);
        context.executionState.variables.SetValue("ProjectileImpactResult", impactResult.ToString());
        context.executionState.variables.SetValue("ProjectileBlockedByShield", impactBlockedByShield);
        context.executionState.variables.SetValue("ProjectileHitThing", impactHitThing?.ThingID ?? string.Empty);

        if (currentTargets != null)
        {
            context.currentTargets.AddRange(currentTargets);
        }

        return true;
    }

    public IEnumerable<SpellActionDef> ResolveActions()
    {
        if (actionPaths == null)
        {
            yield break;
        }

        foreach (ProjectileImpactActionPath actionPath in actionPaths)
        {
            SpellActionDef actionDef = SpellActionPathUtility.ResolveAction(spellDef, actionPath?.Path);
            if (actionDef != null)
            {
                yield return actionDef;
            }
        }
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref projectile, "projectile");
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_TargetInfo.Look(ref initialTarget, "initialTarget");
        Scribe_References.Look(ref impactHitThing, "impactHitThing");
        Scribe_Values.Look(ref impactCaptured, "impactCaptured");
        Scribe_Values.Look(ref impactBlockedByShield, "impactBlockedByShield");
        Scribe_Values.Look(ref impactResult, "impactResult", ProjectileImpactResultKind.Pending);
        Scribe_TargetInfo.Look(ref currentTarget, "currentTarget");
        Scribe_Collections.Look(ref currentTargets, "currentTargets", LookMode.TargetInfo);
        Scribe_Values.Look(ref currentCell, "currentCell", IntVec3.Invalid);
        Scribe_Values.Look(ref lastKnownProjectileCell, "lastKnownProjectileCell", IntVec3.Invalid);
        Scribe_Values.Look(ref powerValue, "powerValue");
        Scribe_Values.Look(ref powerTier, "powerTier");
        Scribe_Values.Look(ref randomSeed, "randomSeed");
        Scribe_Values.Look(ref timeoutTick, "timeoutTick");
        Scribe_Deep.Look(ref variables, "variables");
        Scribe_Collections.Look(ref actionPaths, "actionPaths", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            currentTargets ??= new List<LocalTargetInfo>();
            variables ??= new SpellVariableStore();
            actionPaths ??= new List<ProjectileImpactActionPath>();
        }
    }

    private LocalTargetInfo ResolveCurrentTargetAtImpact(IntVec3 impactCell)
    {
        if (impactHitThing != null && !impactHitThing.Destroyed)
        {
            return new LocalTargetInfo(impactHitThing);
        }

        if (currentTarget.Thing != null && !currentTarget.Thing.Destroyed)
        {
            return currentTarget;
        }

        return impactCell.IsValid ? new LocalTargetInfo(impactCell) : currentTarget;
    }

    private static ProjectileImpactResultKind ResolveCapturedImpactResult(Thing hitThing, bool blockedByShield)
    {
        if (blockedByShield)
        {
            return ProjectileImpactResultKind.ShieldBlocked;
        }

        return hitThing != null ? ProjectileImpactResultKind.HitThing : ProjectileImpactResultKind.ImpactNoThing;
    }

    private enum ProjectileImpactResultKind
    {
        Pending,
        HitThing,
        ShieldBlocked,
        ImpactNoThing,
        Destroyed,
        Timeout
    }

    private sealed class ProjectileImpactActionPath : IExposable
    {
        private List<int> path = new();

        public ProjectileImpactActionPath()
        {
        }

        public ProjectileImpactActionPath(IEnumerable<int> path)
        {
            this.path = path != null ? new List<int>(path) : new List<int>();
        }

        public IReadOnlyList<int> Path => path;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref path, "path", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && path == null)
            {
                path = new List<int>();
            }
        }
    }
}
