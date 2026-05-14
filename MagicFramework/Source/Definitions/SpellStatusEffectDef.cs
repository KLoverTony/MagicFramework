using System.Collections.Generic;
using Verse;

namespace MagicFramework.Definitions;

/// <summary>
/// Reusable status/buff definition that spells can invoke without re-authoring common behavior.
/// </summary>
public class SpellStatusEffectDef : Def
{
    public int durationTicks = 300;
    public ScalableFloatDef scalableDurationTicks;
    public List<string> categories = new();
    public SpellStatusCueDef statusCue;
    public List<SpellStatModifierDef> statModifiers = new();
    public List<SpellActionDef> onApplyActions = new();

    public override void PostLoad()
    {
        base.PostLoad();
        categories ??= new List<string>();
        statModifiers ??= new List<SpellStatModifierDef>();
        onApplyActions ??= new List<SpellActionDef>();
    }
}
