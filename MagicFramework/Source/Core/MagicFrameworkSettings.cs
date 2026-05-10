using Verse;

namespace MagicFramework.Core;

public sealed class MagicFrameworkSettings : ModSettings
{
    public bool logExecution;
    public bool logCosts;
    public bool logRequirements;
    public bool logTargeting;
    public bool logTriggers;
    public bool logPersistentEffects;
    public bool logWallZones;
    public bool logAreaZones;
    public bool logStatModifiers;
    public bool logDisplacement;
    public bool logProjectiles;
    public bool logForceFields;
    public bool logEnhancements;
    public bool logVisuals;
    public bool logSummons;

    public static MagicFrameworkSettings Current { get; private set; } = new();

    public static void SetCurrent(MagicFrameworkSettings settings)
    {
        Current = settings ?? new MagicFrameworkSettings();
    }

    public static bool ShouldLog(MagicLogSubsystem subsystem)
    {
        MagicFrameworkSettings settings = Current;
        if (settings == null)
        {
            return false;
        }

        return subsystem switch
        {
            MagicLogSubsystem.Execution => settings.logExecution,
            MagicLogSubsystem.Costs => settings.logCosts,
            MagicLogSubsystem.Requirements => settings.logRequirements,
            MagicLogSubsystem.Targeting => settings.logTargeting,
            MagicLogSubsystem.Triggers => settings.logTriggers,
            MagicLogSubsystem.PersistentEffects => settings.logPersistentEffects,
            MagicLogSubsystem.WallZones => settings.logWallZones,
            MagicLogSubsystem.AreaZones => settings.logAreaZones,
            MagicLogSubsystem.StatModifiers => settings.logStatModifiers,
            MagicLogSubsystem.Displacement => settings.logDisplacement,
            MagicLogSubsystem.Projectiles => settings.logProjectiles,
            MagicLogSubsystem.ForceFields => settings.logForceFields,
            MagicLogSubsystem.Enhancements => settings.logEnhancements,
            MagicLogSubsystem.Visuals => settings.logVisuals,
            MagicLogSubsystem.Summons => settings.logSummons,
            _ => false
        };
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref logExecution, "logExecution");
        Scribe_Values.Look(ref logCosts, "logCosts");
        Scribe_Values.Look(ref logRequirements, "logRequirements");
        Scribe_Values.Look(ref logTargeting, "logTargeting");
        Scribe_Values.Look(ref logTriggers, "logTriggers");
        Scribe_Values.Look(ref logPersistentEffects, "logPersistentEffects");
        Scribe_Values.Look(ref logWallZones, "logWallZones");
        Scribe_Values.Look(ref logAreaZones, "logAreaZones");
        Scribe_Values.Look(ref logStatModifiers, "logStatModifiers");
        Scribe_Values.Look(ref logDisplacement, "logDisplacement");
        Scribe_Values.Look(ref logProjectiles, "logProjectiles");
        Scribe_Values.Look(ref logForceFields, "logForceFields");
        Scribe_Values.Look(ref logEnhancements, "logEnhancements");
        Scribe_Values.Look(ref logVisuals, "logVisuals");
        Scribe_Values.Look(ref logSummons, "logSummons");
    }
}
