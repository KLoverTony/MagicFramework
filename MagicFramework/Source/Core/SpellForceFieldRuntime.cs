using System.Collections.Generic;
using MagicFramework.Definitions;
using RimWorld;
using Verse;

namespace MagicFramework.Core;

public sealed class ActiveSpellForceField : IExposable
{
    public Thing target;
    public Thing caster;
    public SpellDef spellDef;
    public int expireAtTick = -1;
    public float damageFactor = 0.5f;
    public bool absorbFullyWithMana;
    public float manaCostPerDamageAbsorbed = 1f;
    public float sustainedManaCost;
    public int sustainedManaCostIntervalTicks = 60;
    public int nextSustainedManaCostTick;
    public float maxRange = -1f;
    public bool breakWhenCasterDowned = true;
    public bool breakWhenTargetDowned;
    public bool breakWhenTargetOutOfRange = true;
    public bool breakWhenLineOfSightLost = true;
    public SpellMaintenanceDef maintenance;
    public HediffDef indicatorHediffDef;
    public float indicatorSeverity = 0.01f;
    public bool removeIndicatorOnExpire = true;
    public string statusCueLabel;
    public string statusCueDescription;
    public string impactFleckDef = "BulletShieldAreaEffect";
    public string impactSoundDef = "EnergyShield_AbsorbDamage";
    public string ambientFleckDef = "BulletShieldAreaEffect";
    public int ambientFleckIntervalTicks = 90;
    public float ambientFleckScale = 1f;
    public string ambientColorHex;
    public int nextAmbientFleckTick;
    public string sustainedOverlayTexturePath = "Things/Mote/ShieldBubble";
    public float sustainedOverlayScale = 1.2f;
    public string sustainedOverlayColorHex;
    public int pulseIntervalTicks = -1;
    public int nextPulseTick;
    public List<int> sourceActionPath = new();

    public bool IsExpired(int currentTick)
    {
        return expireAtTick >= 0 && currentTick >= expireAtTick;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref target, "target");
        Scribe_References.Look(ref caster, "caster");
        Scribe_Defs.Look(ref spellDef, "spellDef");
        Scribe_Values.Look(ref expireAtTick, "expireAtTick", -1);
        Scribe_Values.Look(ref damageFactor, "damageFactor", 0.5f);
        Scribe_Values.Look(ref absorbFullyWithMana, "absorbFullyWithMana");
        Scribe_Values.Look(ref manaCostPerDamageAbsorbed, "manaCostPerDamageAbsorbed", 1f);
        Scribe_Values.Look(ref sustainedManaCost, "sustainedManaCost");
        Scribe_Values.Look(ref sustainedManaCostIntervalTicks, "sustainedManaCostIntervalTicks", 60);
        Scribe_Values.Look(ref nextSustainedManaCostTick, "nextSustainedManaCostTick");
        Scribe_Values.Look(ref maxRange, "maxRange", -1f);
        Scribe_Values.Look(ref breakWhenCasterDowned, "breakWhenCasterDowned", true);
        Scribe_Values.Look(ref breakWhenTargetDowned, "breakWhenTargetDowned");
        Scribe_Values.Look(ref breakWhenTargetOutOfRange, "breakWhenTargetOutOfRange", true);
        Scribe_Values.Look(ref breakWhenLineOfSightLost, "breakWhenLineOfSightLost", true);
        Scribe_Deep.Look(ref maintenance, "maintenance");
        Scribe_Defs.Look(ref indicatorHediffDef, "indicatorHediffDef");
        Scribe_Values.Look(ref indicatorSeverity, "indicatorSeverity", 0.01f);
        Scribe_Values.Look(ref removeIndicatorOnExpire, "removeIndicatorOnExpire", true);
        Scribe_Values.Look(ref statusCueLabel, "statusCueLabel");
        Scribe_Values.Look(ref statusCueDescription, "statusCueDescription");
        Scribe_Values.Look(ref impactFleckDef, "impactFleckDef", "BulletShieldAreaEffect");
        Scribe_Values.Look(ref impactSoundDef, "impactSoundDef", "EnergyShield_AbsorbDamage");
        Scribe_Values.Look(ref ambientFleckDef, "ambientFleckDef", "BulletShieldAreaEffect");
        Scribe_Values.Look(ref ambientFleckIntervalTicks, "ambientFleckIntervalTicks", 90);
        Scribe_Values.Look(ref ambientFleckScale, "ambientFleckScale", 1f);
        Scribe_Values.Look(ref ambientColorHex, "ambientColorHex");
        Scribe_Values.Look(ref nextAmbientFleckTick, "nextAmbientFleckTick");
        Scribe_Values.Look(ref sustainedOverlayTexturePath, "sustainedOverlayTexturePath", "Things/Mote/ShieldBubble");
        Scribe_Values.Look(ref sustainedOverlayScale, "sustainedOverlayScale", 1.2f);
        Scribe_Values.Look(ref sustainedOverlayColorHex, "sustainedOverlayColorHex");
        Scribe_Values.Look(ref pulseIntervalTicks, "pulseIntervalTicks", -1);
        Scribe_Values.Look(ref nextPulseTick, "nextPulseTick");
        Scribe_Collections.Look(ref sourceActionPath, "sourceActionPath", LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && sourceActionPath == null)
        {
            sourceActionPath = new List<int>();
        }
    }
}
