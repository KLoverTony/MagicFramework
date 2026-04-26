namespace MagicFramework.Debug;

/// <summary>
/// Simple debug entry captured during spell execution.
/// </summary>
public sealed class SpellDebugEntry
{
    public SpellDebugEntry(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
