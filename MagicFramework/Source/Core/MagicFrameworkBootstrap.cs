using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Logs assembly initialization so we can confirm the framework loads in RimWorld.
/// </summary>
[StaticConstructorOnStartup]
public static class MagicFrameworkBootstrap
{
    static MagicFrameworkBootstrap()
    {
        Log.Message("[MagicFramework] Framework bootstrap initialized.");
    }
}
