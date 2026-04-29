using Verse;

namespace MagicFramework.Visuals;

/// <summary>
/// Element-level visual language used by procedural spell FX.
/// </summary>
public class MagicElementDef : Def
{
    public string primaryColorHex;
    public string secondaryColorHex;
    public string castFleckDef;
    public string castEffectDef;
    public string castSoundDef;
    public string impactFleckDef;
    public string impactEffectDef;
    public string impactSoundDef;
    public string explosionEffectDef;
    public string explosionSoundDef;
    public string areaFleckDef;
    public string sustainFleckDef;
    public float scale = 1f;
}

/// <summary>
/// Spell-specific or category-level procedural FX override.
/// </summary>
public class MagicFXDef : Def
{
    public string primaryColorHex;
    public string secondaryColorHex;
    public string castFleckDef;
    public string castEffectDef;
    public string castSoundDef;
    public string impactFleckDef;
    public string impactEffectDef;
    public string impactSoundDef;
    public string explosionEffectDef;
    public string explosionSoundDef;
    public string areaFleckDef;
    public string sustainFleckDef;
    public float scale = 1f;
}

public sealed class MagicFXPackage
{
    public string fleckDef;
    public string effectDef;
    public string soundDef;
    public string colorHex;
    public float scale = 1f;
}
