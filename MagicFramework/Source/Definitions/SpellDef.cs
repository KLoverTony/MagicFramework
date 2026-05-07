using System.Collections.Generic;
using Verse;

namespace MagicFramework.Definitions;

/// <summary>
/// XML-backed definition for a spell and its authored execution tree.
/// </summary>
public class SpellDef : Def
{
    public float range;
    public int castTimeTicks;
    public string gizmoIconPath;
    public string element;
    public string delivery;
    public string effectShape;
    public string fxOverride;
    public string fxColorOverride;
    public float fxIntensityMultiplier = 1f;
    public bool disableProceduralFx;
    public bool disableDecal;
    public SpellMetaProperties meta = new();
    public SpellLearningProperties learning = new();
    public SpellCastingProperties casting = new();
    public SpellPowerDef power;
    public SpellTargetingDef targeting = new();
    public List<SpellRequirementDef> requirements = new();
    public List<SpellCostDef> costs = new();
    public List<SpellActionDef> actions = new();

    public override void PostLoad()
    {
        base.PostLoad();
        meta ??= new SpellMetaProperties();
        learning ??= new SpellLearningProperties();
        casting ??= new SpellCastingProperties();
    }
}
