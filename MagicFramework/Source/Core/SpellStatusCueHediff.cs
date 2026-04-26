using Verse;

namespace MagicFramework.Core;

/// <summary>
/// Generic status hediff whose visible text can be set by a spell at runtime.
/// </summary>
public sealed class SpellStatusCueHediff : HediffWithComps
{
    public string statusLabel;
    public string statusDescription;

    public override string LabelBase => string.IsNullOrWhiteSpace(statusLabel) ? base.LabelBase : statusLabel;

    public override string TipStringExtra => string.IsNullOrWhiteSpace(statusDescription) ? base.TipStringExtra : statusDescription;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref statusLabel, "statusLabel");
        Scribe_Values.Look(ref statusDescription, "statusDescription");
    }
}
