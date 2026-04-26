using System.Collections.Generic;
using MagicFramework.Definitions;
using Verse;

namespace MagicFramework.Debug;

/// <summary>
/// Provides lightweight built-in spell defs so debug tools can run before XML content is packaged.
/// </summary>
public static class SpellDebugSpellLibrary
{
    public static SpellDef GetFirebolt()
    {
        return GetSpellOrFallback("MF_Firebolt", CreateFallbackFirebolt);
    }

    public static SpellDef GetScalingBolt()
    {
        SpellDef spellDef = CreateFallbackFirebolt();
        spellDef.defName = "MF_ScalingBolt_Debug";
        spellDef.label = "debug scaling bolt";
        spellDef.description = "Built-in debug spell for testing caster-level spell power scaling.";
        return spellDef;
    }

    public static SpellDef GetFireball()
    {
        return GetSpellOrFallback("MF_Fireball", CreateFallbackFireball);
    }

    public static SpellDef GetChainLightning()
    {
        return GetSpellOrFallback("MF_ChainLightning", CreateFallbackChainLightning);
    }

    public static SpellDef GetDelayedBlastRune()
    {
        return GetSpellOrFallback("MF_DelayedBlastRune", CreateFallbackDelayedBlastRune);
    }

    public static SpellDef GetRuneTrap()
    {
        return GetSpellOrFallback("MF_RuneTrap", CreateFallbackRuneTrap);
    }

    public static SpellDef GetWallOfFire()
    {
        return GetSpellOrFallback("MF_WallOfFire", CreateFallbackWallOfFire);
    }

    public static SpellDef GetDisintegrate()
    {
        return GetSpellOrFallback("MF_Disintegrate", CreateFallbackDisintegrate);
    }

    public static SpellDef GetFlameField()
    {
        return GetSpellOrFallback("MF_FlameField", CreateFallbackFlameField);
    }

    public static SpellDef GetForcePush()
    {
        return GetSpellOrFallback("MF_ForcePush", CreateFallbackForcePush);
    }

    public static SpellDef GetForcePull()
    {
        return GetSpellOrFallback("MF_ForcePull", CreateFallbackForcePull);
    }

    public static SpellDef GetBlinkStep()
    {
        return GetSpellOrFallback("MF_BlinkStep", CreateFallbackBlinkStep);
    }

    public static SpellDef GetHaste()
    {
        return GetSpellOrFallback("MF_Haste", CreateFallbackHaste);
    }

    public static SpellDef GetMight()
    {
        return GetSpellOrFallback("MF_Might", CreateFallbackMight);
    }

    public static SpellDef GetForceField()
    {
        return GetSpellOrFallback("MF_ForceField", CreateFallbackForceField);
    }

    public static SpellDef GetManaShield()
    {
        return GetSpellOrFallback("MF_ManaShield", CreateFallbackManaShield);
    }

    public static SpellDef GetHeal()
    {
        return GetSpellOrFallback("MF_Heal", CreateFallbackHeal);
    }

    public static SpellDef GetRegeneration()
    {
        return GetSpellOrFallback("MF_Regeneration", CreateFallbackRegeneration);
    }

    public static SpellDef GetSummonDog()
    {
        return GetSpellOrFallback("MF_SummonDog", CreateFallbackSummonDog);
    }

    public static SpellDef GetCreateFood()
    {
        return GetSpellOrFallback("MF_CreateFood", CreateFallbackCreateFood);
    }

    private static SpellDef CreateFallbackFirebolt()
    {
        return new SpellDef
        {
            defName = "MF_Firebolt_DebugFallback",
            label = "debug firebolt",
            description = "Built-in fallback spell def used when authored XML defs are not loaded yet.",
            range = 24f,
            castTimeTicks = 30,
            gizmoIconPath = "UI/Gizmos/Spells/MF_Firebolt",
            power = new SpellPowerDef
            {
                casterLevelFactor = 1f,
                tiers = new List<SpellPowerTierDef>
                {
                    new() { minPower = 1f, tier = 1 },
                    new() { minPower = 5f, tier = 2 },
                    new() { minPower = 10f, tier = 3 }
                }
            },
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.PawnOrThing,
                pawnAffinity = SpellPawnAffinity.Foe,
                includePawns = true,
                includeBuildings = true,
                includeItems = true,
                allowSelfTarget = false,
                requireLineOfSight = true,
                range = 24f,
                scalableRange = new ScalableFloatDef
                {
                    baseValue = 24f,
                    perPower = 0.5f,
                    max = 34f
                }
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Firebolt sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Firebolt cast effect",
                            effectDef = "Mote_SparkThrownFast",
                            locationSource = SpellEffectLocationSource.Caster
                        },
                        new LaunchProjectileActionDef
                        {
                            debugLabel = "Debug Firebolt projectile",
                            projectileDef = "Bullet_Revolver",
                            onImpactActions = new List<SpellActionDef>
                            {
                                new EffectActionDef
                                {
                                    debugLabel = "Debug Firebolt impact effect",
                                    effectDef = "GiantExplosion",
                                    soundDef = "Explosion_Flame",
                                    locationSource = SpellEffectLocationSource.CurrentTarget,
                                    attachToTarget = true
                                },
                                new DamageActionDef
                                {
                                    debugLabel = "Debug Firebolt damage",
                                    amount = 18f,
                                    scalableAmount = new ScalableFloatDef
                                    {
                                        baseValue = 18f,
                                        perPower = 1.5f,
                                        max = 48f
                                    },
                                    damageDef = "Flame"
                                }
                            }
                        },
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackFireball()
    {
        return new SpellDef
        {
            defName = "MF_Fireball_DebugFallback",
            label = "debug fireball",
            description = "Built-in fallback fireball used when authored XML defs are not loaded yet.",
            range = 28f,
            castTimeTicks = 60,
            gizmoIconPath = "UI/Gizmos/Spells/MF_Fireball",
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Radius,
                primaryTargetType = SpellPrimaryTargetType.PawnOrCell,
                pawnAffinity = SpellPawnAffinity.All,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = false,
                requireLineOfSight = true,
                range = 28f,
                radius = 3.9f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Fireball sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Fireball cast effect",
                            effectDef = "Mote_SparkThrownFast",
                            locationSource = SpellEffectLocationSource.Caster
                        },
                        new LaunchProjectileActionDef
                        {
                            debugLabel = "Debug Fireball projectile",
                            projectileDef = "Bullet_IncendiaryLauncher",
                            onImpactActions = new List<SpellActionDef>
                            {
                                new EffectActionDef
                                {
                                    debugLabel = "Debug Fireball impact effect",
                                    effectDef = "GiantExplosion",
                                    soundDef = "Explosion_Flame",
                                    locationSource = SpellEffectLocationSource.CurrentCell
                                },
                                new ExplosionActionDef
                                {
                                    debugLabel = "Debug Fireball explosion",
                                    radius = 3.9f,
                                    damageAmount = 14f
                                },
                                new ApplyToTargetsActionDef
                                {
                                    debugLabel = "Debug Fireball secondary targets",
                                    targetQuery = new TargetsInRadiusQueryDef
                                    {
                                        debugLabel = "Debug Fireball radius query",
                                        radius = 3.9f,
                                        centerSource = TargetQueryCenterSource.CurrentCell,
                                        includePawns = true,
                                        includeBuildings = false,
                                        includeItems = false,
                                        includeCaster = false,
                                        pawnAffinity = SpellPawnAffinity.All
                                    },
                                    actions = new List<SpellActionDef>
                                    {
                                        new DamageActionDef
                                        {
                                            debugLabel = "Debug Fireball secondary damage",
                                            amount = 10f,
                                            damageDef = "Flame"
                                        },
                                        new ApplyHediffActionDef
                                        {
                                            debugLabel = "Debug Fireball burn",
                                            hediffDef = "Burn",
                                            severity = 0.2f
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackChainLightning()
    {
        return new SpellDef
        {
            defName = "MF_ChainLightning_DebugFallback",
            label = "debug chain lightning",
            description = "Built-in fallback chain lightning spell used when authored XML defs are not loaded yet.",
            range = 22f,
            castTimeTicks = 45,
            gizmoIconPath = "UI/Gizmos/Spells/MF_ChainLightning",
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Foe,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = false,
                requireLineOfSight = true,
                range = 22f
            },
            actions = new List<SpellActionDef>
            {
                new ChainLightningActionDef
                {
                    debugLabel = "Debug Chain Lightning arcs",
                    damageAmount = 12f,
                    damageDef = "Burn",
                    armorPenetration = 0.15f,
                    stunChance = 0.30f,
                    stunTicks = 90,
                    jumpDelayTicks = 10,
                    jumpRadius = 7f,
                    maxHops = 9,
                    minBranches = 1,
                    maxBranches = 2,
                    minForwardScore = -0.10f,
                    allowRepeatTargets = true,
                    includeCaster = false,
                    pawnAffinity = SpellPawnAffinity.Foe,
                    impactFleckDef = "SparkFlash",
                    lineFleckDef = "ElectricalSpark",
                    stunFleckDef = "Mote_Stun",
                    onHitActions = new List<SpellActionDef>
                    {
                        new DamageActionDef
                        {
                            debugLabel = "Debug Chain Lightning shock damage",
                            amount = 12f,
                            damageDef = "Burn",
                            armorPenetration = 0.15f
                        },
                        new StunActionDef
                        {
                            debugLabel = "Debug Chain Lightning stun chance",
                            chance = 0.30f,
                            stunTicks = 90,
                            fleckDef = "Mote_Stun"
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackDelayedBlastRune()
    {
        return new SpellDef
        {
            defName = "MF_DelayedBlastRune_DebugFallback",
            label = "debug delayed blast rune",
            description = "Built-in fallback delayed rune used when authored XML defs are not loaded yet.",
            range = 20f,
            castTimeTicks = 45,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Cell,
                pawnAffinity = SpellPawnAffinity.All,
                includePawns = false,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = false,
                requireLineOfSight = true,
                requireWalkableCell = true,
                range = 20f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Delayed Blast Rune sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Delayed Blast Rune placement effect",
                            effectDef = "Power_Cell_Sparks",
                            locationSource = SpellEffectLocationSource.CurrentCell
                        },
                        new DelayActionDef
                        {
                            debugLabel = "Debug Delayed Blast Rune delay",
                            delayTicks = 180,
                            actions = new List<SpellActionDef>
                            {
                                new EffectActionDef
                                {
                                    debugLabel = "Debug Delayed Blast Rune detonation effect",
                                    effectDef = "GiantExplosion",
                                    soundDef = "Explosion_Flame",
                                    locationSource = SpellEffectLocationSource.CurrentCell
                                },
                                new ExplosionActionDef
                                {
                                    debugLabel = "Debug Delayed Blast Rune explosion",
                                    radius = 2.9f,
                                    damageAmount = 18f
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackRuneTrap()
    {
        return new SpellDef
        {
            defName = "MF_RuneTrap_DebugFallback",
            label = "debug rune trap",
            description = "Built-in fallback proximity rune trap used when authored XML defs are not loaded yet.",
            range = 20f,
            castTimeTicks = 45,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Cell,
                pawnAffinity = SpellPawnAffinity.All,
                includePawns = false,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = false,
                requireLineOfSight = true,
                requireWalkableCell = true,
                range = 20f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Rune Trap sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Rune Trap placement effect",
                            effectDef = "Power_Cell_Sparks",
                            locationSource = SpellEffectLocationSource.CurrentCell
                        },
                        new PersistentEffectActionDef
                        {
                            markerThingDef = "MF_RuneTrapMarker",
                            durationTicks = -1,
                            failsafeDurationTicks = 3600,
                            replaceExistingForCaster = true
                        },
                        new ProximityTriggerActionDef
                        {
                            debugLabel = "Debug Rune Trap armed trigger",
                            triggerRadius = 1.9f,
                            pawnAffinity = SpellPawnAffinity.Foe,
                            includeCaster = false,
                            replaceExistingForCaster = true,
                            removePersistentEffectsForCasterSpell = true,
                            checkIntervalTicks = 15,
                            actions = new List<SpellActionDef>
                            {
                                new EffectActionDef
                                {
                                    debugLabel = "Debug Rune Trap detonation effect",
                                    effectDef = "GiantExplosion",
                                    soundDef = "Explosion_Flame",
                                    locationSource = SpellEffectLocationSource.CurrentCell
                                },
                                new ExplosionActionDef
                                {
                                    debugLabel = "Debug Rune Trap explosion",
                                    radius = 2.4f,
                                    damageAmount = 16f
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackWallOfFire()
    {
        return new SpellDef
        {
            defName = "MF_WallOfFire_DebugFallback",
            label = "debug wall of fire",
            description = "Built-in fallback wall of fire used when authored XML defs are not loaded yet.",
            range = 22f,
            castTimeTicks = 60,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Wall,
                primaryTargetType = SpellPrimaryTargetType.Cell,
                pawnAffinity = SpellPawnAffinity.All,
                includePawns = false,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = false,
                requireLineOfSight = true,
                requireWalkableCell = true,
                range = 22f,
                wallLength = 5
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Wall of Fire sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Wall of Fire cast effect",
                            effectDef = "Mote_SparkThrownFast",
                            locationSource = SpellEffectLocationSource.Caster
                        },
                        new PersistentWallZoneActionDef
                        {
                            debugLabel = "Debug Wall of Fire wall zone",
                            markerThingDef = "MF_WallOfFireMarker",
                            wallLength = 5,
                            pulseRadius = 0.95f,
                            pulseIntervalTicks = 60,
                            durationTicks = 900,
                            failsafeDurationTicks = 1200,
                            pawnAffinity = SpellPawnAffinity.All,
                            includeCaster = false,
                            replaceExistingForCaster = true,
                            actions = new List<SpellActionDef>
                            {
                                new EffectActionDef
                                {
                                    debugLabel = "Debug Wall of Fire pulse effect",
                                    effectDef = "Power_Cell_Sparks",
                                    locationSource = SpellEffectLocationSource.CurrentCell
                                },
                                new DamageActionDef
                                {
                                    debugLabel = "Debug Wall of Fire damage",
                                    amount = 6f,
                                    damageDef = "Flame"
                                },
                                new ApplyHediffActionDef
                                {
                                    debugLabel = "Debug Wall of Fire burn",
                                    hediffDef = "Burn",
                                    severity = 0.08f
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackDisintegrate()
    {
        return new SpellDef
        {
            defName = "MF_Disintegrate_DebugFallback",
            label = "debug disintegrate",
            description = "Built-in fallback disintegrate spell used when authored XML defs are not loaded yet.",
            range = 22f,
            castTimeTicks = 45,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.PawnOrThing,
                pawnAffinity = SpellPawnAffinity.Foe,
                includePawns = true,
                includeBuildings = true,
                includeItems = true,
                allowSelfTarget = false,
                requireLineOfSight = true,
                range = 22f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Disintegrate sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Disintegrate cast effect",
                            effectDef = "Mote_SparkThrownFast",
                            locationSource = SpellEffectLocationSource.Caster
                        },
                        new EffectActionDef
                        {
                            debugLabel = "Debug Disintegrate target effect",
                            effectDef = "GiantExplosion",
                            soundDef = "Explosion_Flame",
                            locationSource = SpellEffectLocationSource.CurrentTarget,
                            attachToTarget = true
                        },
                        new ConditionalActionDef
                        {
                            debugLabel = "Debug Disintegrate conditional",
                            conditionLabel = "If the target is a pawn",
                            condition = new TargetIsPawnConditionDef
                            {
                                targetSource = SpellConditionTargetSource.CurrentTarget
                            },
                            thenActions = new List<SpellActionDef>
                            {
                                new DamageActionDef
                                {
                                    debugLabel = "Debug Disintegrate pawn damage",
                                    amount = 28f,
                                    damageDef = "Flame",
                                    armorPenetration = 0.4f
                                }
                            },
                            elseActions = new List<SpellActionDef>
                            {
                                new ConditionalActionDef
                                {
                                    debugLabel = "Debug Disintegrate building conditional",
                                    conditionLabel = "If the target is a building",
                                    condition = new ThingCategoryConditionDef
                                    {
                                        targetSource = SpellConditionTargetSource.CurrentTarget,
                                        category = ThingCategory.Building
                                    },
                                    thenActions = new List<SpellActionDef>
                                    {
                                        new DamageActionDef
                                        {
                                            debugLabel = "Debug Disintegrate building damage",
                                            amount = 40f,
                                            damageDef = "Flame",
                                            armorPenetration = 1.2f
                                        }
                                    },
                                    elseActions = new List<SpellActionDef>
                                    {
                                        new DestroyThingActionDef
                                        {
                                            debugLabel = "Debug Disintegrate destroy thing",
                                            allowPawns = false
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackFlameField()
    {
        return new SpellDef
        {
            defName = "MF_FlameField_DebugFallback",
            label = "debug flame field",
            description = "Built-in fallback area zone spell used when authored XML defs are not loaded yet.",
            range = 20f,
            castTimeTicks = 45,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Cell,
                pawnAffinity = SpellPawnAffinity.All,
                includePawns = false,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = false,
                requireLineOfSight = true,
                requireWalkableCell = true,
                range = 20f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Flame Field sequence",
                    actions = new List<SpellActionDef>
                    {
                        new PersistentAreaZoneActionDef
                        {
                            debugLabel = "Debug Flame Field area zone",
                            markerThingDef = "MF_FlameFieldMarker",
                            zoneRadius = 3.4f,
                            pulseIntervalTicks = 30,
                            ambientEffectDef = "Vaporize_Heatwave",
                            visualPulseIntervalTicks = 90,
                            emitVisualFromMarkers = false,
                            maxVisualMarkersPerPulse = 1,
                            durationTicks = 720,
                            failsafeDurationTicks = 900,
                            pawnAffinity = SpellPawnAffinity.All,
                            includeCaster = false,
                            replaceExistingForCaster = true,
                            actions = new List<SpellActionDef>
                            {
                                new DamageActionDef
                                {
                                    debugLabel = "Debug Flame Field damage",
                                    amount = 5f,
                                    damageDef = "Flame"
                                },
                                new ApplyHediffActionDef
                                {
                                    debugLabel = "Debug Flame Field burn",
                                    hediffDef = "Burn",
                                    severity = 0.06f
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackForcePush()
    {
        return new SpellDef
        {
            defName = "MF_ForcePush_DebugFallback",
            label = "debug force push",
            description = "Built-in fallback force push used when authored XML defs are not loaded yet.",
            range = 18f,
            castTimeTicks = 30,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Foe,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = false,
                requireLineOfSight = true,
                range = 18f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Force Push sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Force Push cast effect",
                            effectDef = "PsycastAreaEffect",
                            locationSource = SpellEffectLocationSource.Caster
                        },
                        new EffectActionDef
                        {
                            debugLabel = "Debug Force Push target effect",
                            effectDef = "PsycastPsychicEffect",
                            locationSource = SpellEffectLocationSource.CurrentTarget,
                            attachToTarget = true
                        },
                        new KnockbackActionDef
                        {
                            debugLabel = "Debug Force Push knockback",
                            distance = 4,
                            requireStandableDestination = true,
                            requireWalkableDestination = true,
                            allowHitCasterCell = false
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackForcePull()
    {
        return new SpellDef
        {
            defName = "MF_ForcePull_DebugFallback",
            label = "debug force pull",
            description = "Built-in fallback force pull used when authored XML defs are not loaded yet.",
            range = 18f,
            castTimeTicks = 30,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Foe,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = false,
                requireLineOfSight = true,
                range = 18f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Force Pull sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Force Pull cast effect",
                            effectDef = "PsycastAreaEffect",
                            locationSource = SpellEffectLocationSource.Caster
                        },
                        new EffectActionDef
                        {
                            debugLabel = "Debug Force Pull target effect",
                            effectDef = "PsycastPsychicEffect",
                            locationSource = SpellEffectLocationSource.CurrentTarget,
                            attachToTarget = true
                        },
                        new PullActionDef
                        {
                            debugLabel = "Debug Force Pull pull",
                            distance = 4,
                            requireStandableDestination = true,
                            requireWalkableDestination = true,
                            minDistanceFromCaster = 1
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackBlinkStep()
    {
        return new SpellDef
        {
            defName = "MF_BlinkStep_DebugFallback",
            label = "debug blink step",
            description = "Built-in fallback blink spell used when authored XML defs are not loaded yet.",
            range = 14f,
            castTimeTicks = 20,
            power = new SpellPowerDef
            {
                casterLevelFactor = 1f,
                tiers = new List<SpellPowerTierDef>
                {
                    new() { minPower = 1f, tier = 1 },
                    new() { minPower = 5f, tier = 2 },
                    new() { minPower = 10f, tier = 3 }
                }
            },
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Cell,
                pawnAffinity = SpellPawnAffinity.All,
                includePawns = false,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                requireWalkableCell = true,
                requireStandableCell = true,
                range = 14f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Blink Step sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Blink Step departure",
                            effectDef = "PsycastAreaEffect",
                            locationSource = SpellEffectLocationSource.Caster
                        },
                        new TeleportActionDef
                        {
                            debugLabel = "Debug Blink Step teleport",
                            subjectSource = TeleportSubjectSource.Caster,
                            destinationSource = TeleportDestinationSource.CurrentCell,
                            requireStandableDestination = true,
                            requireWalkableDestination = true,
                            allowTeleportOntoCaster = true
                        },
                        new EffectActionDef
                        {
                            debugLabel = "Debug Blink Step arrival",
                            effectDef = "PsycastPsychicEffect",
                            locationSource = SpellEffectLocationSource.Caster,
                            attachToTarget = true
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackHaste()
    {
        return new SpellDef
        {
            defName = "MF_Haste_DebugFallback",
            label = "debug haste",
            description = "Built-in fallback haste spell used when authored XML defs are not loaded yet.",
            range = 18f,
            castTimeTicks = 20,
            gizmoIconPath = "UI/Gizmos/Spells/MF_Haste",
            power = new SpellPowerDef
            {
                casterLevelFactor = 1f,
                tiers = new List<SpellPowerTierDef>
                {
                    new() { minPower = 1f, tier = 1 },
                    new() { minPower = 5f, tier = 2 },
                    new() { minPower = 10f, tier = 3 }
                }
            },
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Ally,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                range = 18f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Haste sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Haste effect",
                            effectDef = "PsycastPsychicEffect",
                            locationSource = SpellEffectLocationSource.CurrentTarget,
                            attachToTarget = true
                        },
                        new ApplyStatModifierActionDef
                        {
                            debugLabel = "Debug Haste buff",
                            targetSource = StatModifierTargetSource.CurrentTarget,
                            durationTicks = 900,
                            scalableDurationTicks = new ScalableFloatDef
                            {
                                baseValue = 900f,
                                perPower = 30f,
                                max = 1500f
                            },
                            replaceExistingFromCasterSpell = true,
                            statusCue = new SpellStatusCueDef
                            {
                                hediffDef = "MF_Hasted",
                                severity = 0.01f,
                                removeOnExpire = true
                            },
                            modifiers = new List<SpellStatModifierDef>
                            {
                                new()
                                {
                                    statDef = "MoveSpeed",
                                    factor = 1.35f
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackMight()
    {
        return new SpellDef
        {
            defName = "MF_Might_DebugFallback",
            label = "debug might",
            description = "Built-in fallback might spell used when authored XML defs are not loaded yet.",
            range = 12f,
            castTimeTicks = 30,
            gizmoIconPath = "UI/Gizmos/Spells/MF_Might",
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Ally,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                range = 12f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Might sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Might effect",
                            effectDef = "PsycastPsychicEffect",
                            locationSource = SpellEffectLocationSource.CurrentTarget,
                            attachToTarget = true
                        },
                        new SustainedStatModifierActionDef
                        {
                            debugLabel = "Debug Might sustained buff",
                            targetSource = StatModifierTargetSource.CurrentTarget,
                            maxDurationTicks = 1800,
                            replaceExistingFromCasterSpell = true,
                            statusCue = new SpellStatusCueDef
                            {
                                hediffDef = "MF_Mighty",
                                severity = 0.01f,
                                removeOnExpire = true
                            },
                            maxRange = 12f,
                            breakWhenCasterDowned = true,
                            breakWhenTargetDowned = false,
                            breakWhenTargetOutOfRange = true,
                            breakWhenLineOfSightLost = true,
                            modifiers = new List<SpellStatModifierDef>
                            {
                                new()
                                {
                                    statDef = "MeleeDamageFactor",
                                    factor = 1.35f
                                },
                                new()
                                {
                                    statDef = "CarryingCapacity",
                                    offset = 25f
                                }
                            },
                            onBreakActions = new List<SpellActionDef>
                            {
                                new EffectActionDef
                                {
                                    debugLabel = "Debug Might break effect",
                                    effectDef = "PsycastPsychicEffect",
                                    locationSource = SpellEffectLocationSource.CurrentTarget,
                                    attachToTarget = true
                                },
                                new ApplyStatModifierActionDef
                                {
                                    debugLabel = "Debug Might backlash weakness",
                                    targetSource = StatModifierTargetSource.CurrentTarget,
                                    durationTicks = 300,
                                    replaceExistingFromCasterSpell = true,
                                    statusCue = new SpellStatusCueDef
                                    {
                                        hediffDef = "MF_Weakened",
                                        severity = 0.01f,
                                        removeOnExpire = true
                                    },
                                    modifiers = new List<SpellStatModifierDef>
                                    {
                                        new()
                                        {
                                            statDef = "MeleeDamageFactor",
                                            factor = 0.75f
                                        },
                                        new()
                                        {
                                            statDef = "CarryingCapacity",
                                            offset = -15f
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackForceField()
    {
        return new SpellDef
        {
            defName = "MF_ForceField_DebugFallback",
            label = "debug force field",
            description = "Built-in fallback force field spell used when authored XML defs are not loaded yet.",
            range = 16f,
            castTimeTicks = 35,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Ally,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                range = 16f
            },
            actions = new List<SpellActionDef>
            {
                new ApplyForceFieldActionDef
                {
                    debugLabel = "Debug Force Field shield",
                    targetSource = StatModifierTargetSource.CurrentTarget,
                    maxDurationTicks = 1200,
                    statusCue = new SpellStatusCueDef
                    {
                        hediffDef = "MF_ForceFielded",
                        severity = 0.01f,
                        removeOnExpire = true
                    },
                    damageFactor = 0.5f,
                    absorbFullyWithMana = false,
                    manaCostPerDamageAbsorbed = 0f,
                    maxRange = 16f,
                    breakWhenCasterDowned = true,
                    breakWhenTargetDowned = false,
                    breakWhenTargetOutOfRange = true,
                    breakWhenLineOfSightLost = true,
                    impactFleckDef = "BulletShieldAreaEffect",
                    impactSoundDef = "EnergyShield_AbsorbDamage",
                    ambientFleckDef = null,
                    ambientFleckIntervalTicks = 90,
                    ambientFleckScale = 1.05f,
                    ambientColorHex = "#65B8FFFF",
                    sustainedOverlayTexturePath = null,
                    sustainedOverlayScale = 1.25f,
                    sustainedOverlayColorHex = "#65B8FF73"
                }
            }
        };
    }

    private static SpellDef CreateFallbackManaShield()
    {
        return new SpellDef
        {
            defName = "MF_ManaShield_DebugFallback",
            label = "debug mana shield",
            description = "Built-in fallback mana shield spell used when authored XML defs are not loaded yet.",
            range = 16f,
            castTimeTicks = 35,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Ally,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                range = 16f
            },
            actions = new List<SpellActionDef>
            {
                new ApplyForceFieldActionDef
                {
                    debugLabel = "Debug Mana Shield absorb field",
                    targetSource = StatModifierTargetSource.CurrentTarget,
                    maxDurationTicks = 1200,
                    statusCue = new SpellStatusCueDef
                    {
                        hediffDef = "MF_ManaShielded",
                        severity = 0.01f,
                        removeOnExpire = true
                    },
                    damageFactor = 1f,
                    absorbFullyWithMana = true,
                    manaCostPerDamageAbsorbed = 1f,
                    maxRange = 16f,
                    breakWhenCasterDowned = true,
                    breakWhenTargetDowned = false,
                    breakWhenTargetOutOfRange = true,
                    breakWhenLineOfSightLost = true,
                    impactFleckDef = "BulletShieldAreaEffect",
                    impactSoundDef = "EnergyShield_AbsorbDamage",
                    ambientFleckDef = null,
                    ambientFleckIntervalTicks = 70,
                    ambientFleckScale = 1.15f,
                    ambientColorHex = "#B87CFFFF",
                    sustainedOverlayTexturePath = null,
                    sustainedOverlayScale = 1.3f,
                    sustainedOverlayColorHex = "#B87CFF78"
                }
            }
        };
    }

    private static SpellDef CreateFallbackHeal()
    {
        return new SpellDef
        {
            defName = "MF_Heal_DebugFallback",
            label = "debug heal",
            description = "Built-in fallback heal spell used when authored XML defs are not loaded yet.",
            range = 16f,
            castTimeTicks = 25,
            gizmoIconPath = "UI/Gizmos/Spells/MF_Heal",
            power = new SpellPowerDef
            {
                casterLevelFactor = 1f,
                tiers = new List<SpellPowerTierDef>
                {
                    new() { minPower = 1f, tier = 1 },
                    new() { minPower = 5f, tier = 2 },
                    new() { minPower = 10f, tier = 3 }
                }
            },
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Ally,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                range = 16f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Heal sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Heal effect",
                            effectDef = "PsycastPsychicEffect",
                            locationSource = SpellEffectLocationSource.CurrentTarget,
                            attachToTarget = true
                        },
                        new HealActionDef
                        {
                            debugLabel = "Debug Heal injuries evenly",
                            amount = 18f,
                            scalableAmount = new ScalableFloatDef
                            {
                                baseValue = 18f,
                                perPower = 1f,
                                max = 42f
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackRegeneration()
    {
        return new SpellDef
        {
            defName = "MF_Regeneration_DebugFallback",
            label = "debug regeneration",
            description = "Built-in fallback regeneration spell used when authored XML defs are not loaded yet.",
            range = 16f,
            castTimeTicks = 35,
            gizmoIconPath = "UI/Gizmos/Spells/MF_Regeneration",
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Pawn,
                pawnAffinity = SpellPawnAffinity.Ally,
                includePawns = true,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                range = 16f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Regeneration sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Regeneration effect",
                            effectDef = "PsycastPsychicEffect",
                            locationSource = SpellEffectLocationSource.CurrentTarget,
                            attachToTarget = true
                        },
                        new RepeatActionDef
                        {
                            debugLabel = "Debug Regeneration healing pulses",
                            intervalTicks = 120,
                            repeatCount = 6,
                            includeImmediate = true,
                            actions = new List<SpellActionDef>
                            {
                                new HealActionDef
                                {
                                    debugLabel = "Debug Regeneration pulse heal",
                                    amount = 4f
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackSummonDog()
    {
        return new SpellDef
        {
            defName = "MF_SummonDog_DebugFallback",
            label = "debug summon dog",
            description = "Built-in fallback summon dog spell used when authored XML defs are not loaded yet.",
            range = 14f,
            castTimeTicks = 30,
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Cell,
                pawnAffinity = SpellPawnAffinity.All,
                includePawns = false,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                requireWalkableCell = true,
                requireStandableCell = true,
                range = 14f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Summon Dog sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Summon Dog cast effect",
                            effectDef = "PsycastAreaEffect",
                            locationSource = SpellEffectLocationSource.CurrentCell
                        },
                        new SummonPawnActionDef
                        {
                            debugLabel = "Debug Summon Dog summon",
                            pawnKindDef = "LabradorRetriever",
                            durationTicks = 1800,
                            replaceExistingForCaster = true,
                            setFactionToPlayer = true,
                            joinLord = true,
                            followMasterWhileDrafted = true,
                            followMasterWhileFieldwork = true,
                            trainableDefs = new List<string>
                            {
                                "Obedience",
                                "Release",
                                "Rescue",
                                "Haul"
                            }
                        },
                        new EffectActionDef
                        {
                            debugLabel = "Debug Summon Dog arrival effect",
                            effectDef = "PsycastPsychicEffect",
                            locationSource = SpellEffectLocationSource.CurrentTarget,
                            attachToTarget = true
                        }
                    }
                }
            }
        };
    }

    private static SpellDef CreateFallbackCreateFood()
    {
        return new SpellDef
        {
            defName = "MF_CreateFood_DebugFallback",
            label = "debug create food",
            description = "Built-in fallback create food spell used when authored XML defs are not loaded yet.",
            range = 12f,
            castTimeTicks = 20,
            power = new SpellPowerDef
            {
                casterLevelFactor = 1f,
                tiers = new List<SpellPowerTierDef>
                {
                    new() { minPower = 1f, tier = 1 },
                    new() { minPower = 5f, tier = 2 },
                    new() { minPower = 10f, tier = 3 }
                }
            },
            targeting = new SpellTargetingDef
            {
                shape = SpellTargetShape.Single,
                primaryTargetType = SpellPrimaryTargetType.Cell,
                pawnAffinity = SpellPawnAffinity.All,
                includePawns = false,
                includeBuildings = false,
                includeItems = false,
                allowSelfTarget = true,
                requireLineOfSight = true,
                requireWalkableCell = true,
                requireStandableCell = true,
                range = 12f
            },
            actions = new List<SpellActionDef>
            {
                new SequenceActionDef
                {
                    debugLabel = "Debug Create Food sequence",
                    actions = new List<SpellActionDef>
                    {
                        new EffectActionDef
                        {
                            debugLabel = "Debug Create Food conjuration effect",
                            effectDef = "PsycastAreaEffect",
                            locationSource = SpellEffectLocationSource.CurrentCell
                        },
                        new SpawnThingActionDef
                        {
                            debugLabel = "Debug Create Food meal",
                            thingDef = "MealSimple",
                            tieredThingDefs = new List<TieredThingDefName>
                            {
                                new() { minTier = 2, thingDef = "MealFine" },
                                new() { minTier = 3, thingDef = "MealLavish" }
                            },
                            stackCount = 1,
                            scalableStackCount = new ScalableFloatDef
                            {
                                baseValue = 1f,
                                perPower = 0.1f,
                                max = 4f
                            },
                            durationTicks = 2500,
                            scalableDurationTicks = new ScalableFloatDef
                            {
                                baseValue = 2500f,
                                perPower = 75f,
                                max = 4000f
                            },
                            forbidden = false,
                            replaceExistingForCasterSpell = false
                        }
                    }
                }
            }
        };
    }

    private static SpellDef GetSpellOrFallback(string defName, System.Func<SpellDef> fallbackFactory)
    {
        SpellDef authoredSpell = DefDatabase<SpellDef>.GetNamedSilentFail(defName);
        if (authoredSpell != null)
        {
            return authoredSpell;
        }

        return fallbackFactory();
    }
}
