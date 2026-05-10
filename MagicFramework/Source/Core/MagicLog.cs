using Verse;

namespace MagicFramework.Core;

public enum MagicLogSubsystem
{
    Execution,
    Costs,
    Requirements,
    Targeting,
    Triggers,
    PersistentEffects,
    WallZones,
    AreaZones,
    StatModifiers,
    Displacement,
    Projectiles,
    ForceFields,
    Enhancements,
    Visuals,
    Summons
}

public static class MagicLog
{
    public static void Message(MagicLogSubsystem subsystem, string message)
    {
        if (MagicFrameworkSettings.ShouldLog(subsystem))
        {
            Log.Message(message);
        }
    }

    public static void Warning(string message)
    {
        Log.Warning(message);
    }

    public static void Error(string message)
    {
        Log.Error(message);
    }
}
