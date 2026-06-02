document.addEventListener('DOMContentLoaded', () => {
    const appContainer = document.querySelector('.app-container');
    const form = document.getElementById('spell-form');
    const patternGrid = document.getElementById('patternGrid');
    const humanSummary = document.getElementById('humanSummary');
    const xmlContainer = document.getElementById('xmlContainer');
    const xmlPreview = document.getElementById('xmlPreview');
    const viewXmlBtn = document.getElementById('viewXmlBtn');
    const copyXmlBtn = document.getElementById('copyXmlBtn');
    const resetBtn = document.getElementById('resetBtn');
    const patternHint = document.getElementById('patternHint');
    const payloadToolbar = document.getElementById('payloadToolbar');
    const payloadStack = document.getElementById('payloadStack');
    const modeButtons = document.querySelectorAll('.mode-button');
    const simpleWorkflow = document.querySelector('.simple-workflow');
    const advancedWorkflow = document.querySelector('.advanced-workflow');
    const rootActionType = document.getElementById('rootActionType');
    const addRootActionBtn = document.getElementById('addRootActionBtn');
    const regenerateTreeBtn = document.getElementById('regenerateTreeBtn');
    const actionTree = document.getElementById('actionTree');
    const actionInspector = document.getElementById('actionInspector');
    const validationPanel = document.getElementById('validationPanel');
    const workspaceTabs = document.querySelectorAll('.workspace-tab');
    const tabPanels = document.querySelectorAll('[data-tab-panel]');
    const targetingCompatibility = document.getElementById('targetingCompatibility');

    const customDefOption = '__new__';
    const knownDefs = {
        damageDef: ['Blunt', 'Flame', 'Burn', 'EMP', 'Bomb', 'Cut'],
        statusEffectDef: [
            'MFV_Status_Haste',
            'MFV_Status_Might',
            'MFV_Status_MightBacklashWeakness',
            'MFV_Status_BlessedVigor',
            'MFV_Status_Regenerating',
            'MFV_Status_FrozenSlow',
            'MFV_Status_WaterboundSlow'
        ],
        hediffDef: [
            'Burn',
            'MF_Hasted',
            'MF_BlessedVigor',
            'MF_Mighty',
            'MF_Weakened',
            'MF_ForceFielded',
            'MF_ManaShielded',
            'MF_Regenerating',
            'MF_SharedEssence',
            'MF_Frozen',
            'MF_Waterbound',
            'MF_HeldUnder'
        ],
        pawnKindDef: ['Husky']
    };

    const statusCueHediffs = {
        MFV_Status_Haste: 'MF_Hasted',
        MFV_Status_Might: 'MF_Mighty',
        MFV_Status_MightBacklashWeakness: 'MF_Weakened',
        MFV_Status_BlessedVigor: 'MF_BlessedVigor',
        MFV_Status_Regenerating: 'MF_Regenerating',
        MFV_Status_FrozenSlow: 'MF_Frozen',
        MFV_Status_WaterboundSlow: 'MF_Waterbound'
    };

    const formDefaults = {
        delivery: 'instant',
        targetShape: 'Single',
        primaryTargetType: 'PawnOrThing',
        pawnAffinity: 'All',
        targetRadius: 3.9,
        lineLength: 12,
        coneAngleDegrees: 60,
        wallLength: 7,
        maxChains: 5,
        includePawns: true,
        includeBuildings: false,
        includeItems: false,
        allowSelfTarget: false,
        useCasterAsTarget: false,
        requireLineOfSight: false,
        requireStandableCell: false,
        requireWalkableCell: false,
        requireWaterCell: false,
        requireResurrectableCorpse: false,
        durationTicks: 600,
        zoneMarkerDef: 'MF_FlameFieldMarker',
        pulseIntervalTicks: 60,
        researchPrerequisite: '',
        minimumCasterLevel: 0,
        casterLevelFactor: 1,
        canBeLearned: true,
        requireArcaneGift: true,
        appendSpellSummary: true
    };

    const payloadTypes = {
        damage: {
            title: 'Damage',
            summary: payload => `deal <strong>${escapeHtml(payload.amount)} ${escapeHtml(payload.damageDef)}</strong> damage`,
            fields: [
                { key: 'amount', label: 'Damage Amount', type: 'number', step: '0.1' },
                { key: 'damageDef', label: 'Damage Def', type: 'def', options: knownDefs.damageDef }
            ],
            defaults: { amount: 10, damageDef: 'Blunt' }
        },
        knockback: {
            title: 'Knockback',
            summary: payload => `knock the target back <strong>${escapeHtml(payload.distance)}</strong> cells`,
            fields: [
                { key: 'distance', label: 'Distance', type: 'number', step: '1' }
            ],
            defaults: { distance: 3 }
        },
        heal: {
            title: 'Healing',
            summary: payload => `heal for <strong>${escapeHtml(payload.amount)}</strong>`,
            fields: [
                { key: 'amount', label: 'Heal Amount', type: 'number', step: '0.1' }
            ],
            defaults: { amount: 15 }
        },
        status: {
            title: 'Status',
            summary: payload => `apply status <strong>${escapeHtml(payload.statusEffectDef)}</strong>`,
            fields: [
                { key: 'statusEffectDef', label: 'Status Effect Def', type: 'def', options: knownDefs.statusEffectDef },
                { key: 'durationTicks', label: 'Override Duration', type: 'number', step: '1' },
                { key: 'showCue', label: 'Show Cue', type: 'checkbox' },
                { key: 'statusCue', label: 'Visible Hediff Cue', type: 'statusCue' }
            ],
            defaults: { statusEffectDef: 'MFV_Status_Haste', durationTicks: -1, showCue: true }
        },
        hediff: {
            title: 'Hediff',
            summary: payload => `apply hediff <strong>${escapeHtml(payload.hediffDef)}</strong>`,
            fields: [
                { key: 'hediffDef', label: 'Hediff Def', type: 'def', options: knownDefs.hediffDef },
                { key: 'severity', label: 'Severity', type: 'number', step: '0.01' },
                { key: 'durationTicks', label: 'Duration', type: 'number', step: '1' }
            ],
            defaults: { hediffDef: 'Burn', severity: 0.2, durationTicks: 0 }
        },
        summon: {
            title: 'Summon',
            summary: payload => `summon <strong>${escapeHtml(payload.pawnKindDef)}</strong> for <strong>${escapeHtml(payload.durationTicks)}</strong> ticks`,
            fields: [
                { key: 'pawnKindDef', label: 'PawnKind Def', type: 'def', options: knownDefs.pawnKindDef },
                { key: 'durationTicks', label: 'Duration', type: 'number', step: '1' }
            ],
            defaults: { pawnKindDef: 'Husky', durationTicks: 2500 }
        }
    };

    const patterns = {
        projectileDamage: {
            title: 'Projectile Strike',
            tag: 'Offense',
            hint: 'Cast visual, projectile launch, then impact visual and payload.',
            values: {
                label: 'firebolt',
                description: 'Launches a bolt of fire at one target.',
                delivery: 'projectile',
                targetShape: 'Single',
                primaryTargetType: 'PawnOrThing',
                pawnAffinity: 'Foe',
                includeBuildings: true,
                includeItems: true,
                payloads: [
                    { type: 'damage', amount: 18, damageDef: 'Flame' }
                ],
                manaCost: 8,
                cooldownTicks: 90,
                range: 24,
                castTimeTicks: 30
            }
        },
        areaBurst: {
            title: 'Area Burst',
            tag: 'Blast',
            hint: 'A cell-targeted explosion plus optional radius target query.',
            values: {
                label: 'fireball',
                description: 'Detonates at the target and burns everything nearby.',
                delivery: 'area',
                targetShape: 'Radius',
                primaryTargetType: 'PawnOrCell',
                pawnAffinity: 'All',
                includeBuildings: true,
                includeItems: true,
                scaledAttributes: ['Damage', 'Radius', 'Cooldown'],
                payloads: [
                    { type: 'damage', amount: 10, damageDef: 'Flame' }
                ],
                targetRadius: 3.9,
                manaCost: 20,
                cooldownTicks: 300,
                range: 28,
                castTimeTicks: 60
            }
        },
        buff: {
            title: 'Ally Buff',
            tag: 'Support',
            hint: 'Applies a reusable SpellStatusEffectDef to an allied pawn.',
            values: {
                label: 'haste',
                description: 'Accelerates an allied pawn for a short time.',
                delivery: 'instant',
                targetShape: 'Single',
                primaryTargetType: 'Pawn',
                pawnAffinity: 'Ally',
                allowSelfTarget: true,
                payloads: [
                    { type: 'status', statusEffectDef: 'MFV_Status_Haste', durationTicks: -1 }
                ],
                manaCost: 12,
                cooldownTicks: 240,
                range: 18,
                castTimeTicks: 20
            }
        },
        heal: {
            title: 'Healing Touch',
            tag: 'Support',
            hint: 'Direct healing on the selected pawn with optional visual feedback.',
            values: {
                label: 'mend wounds',
                description: 'Restores health to a nearby ally.',
                delivery: 'instant',
                targetShape: 'Single',
                primaryTargetType: 'Pawn',
                pawnAffinity: 'Ally',
                allowSelfTarget: true,
                payloads: [
                    { type: 'heal', amount: 20 }
                ],
                manaCost: 14,
                cooldownTicks: 240,
                range: 12,
                castTimeTicks: 45
            }
        },
        persistentZone: {
            title: 'Persistent Field',
            tag: 'Zone',
            hint: 'Creates a marker-backed area that pulses child actions over time.',
            values: {
                label: 'flame field',
                description: 'Creates a burning field that damages pawns standing inside it.',
                delivery: 'persistent',
                targetShape: 'Radius',
                primaryTargetType: 'Cell',
                pawnAffinity: 'Foe',
                scaledAttributes: ['Damage', 'Radius', 'Duration'],
                payloads: [
                    { type: 'damage', amount: 8, damageDef: 'Flame' }
                ],
                targetRadius: 3,
                durationTicks: 600,
                pulseIntervalTicks: 60,
                zoneMarkerDef: 'MF_FlameFieldMarker',
                manaCost: 30,
                cooldownTicks: 600,
                range: 20,
                castTimeTicks: 60
            }
        },
        summon: {
            title: 'Summon',
            tag: 'Creature',
            hint: 'Spawns a temporary pawn or thing at the target cell.',
            values: {
                label: 'summon dog',
                description: 'Summons a temporary animal helper near the target cell.',
                delivery: 'instant',
                targetShape: 'Single',
                primaryTargetType: 'Cell',
                pawnAffinity: 'Ally',
                requireWalkableCell: true,
                scaledAttributes: ['Duration', 'Cooldown'],
                payloads: [
                    { type: 'summon', pawnKindDef: 'Husky', durationTicks: 2500 }
                ],
                manaCost: 35,
                cooldownTicks: 900,
                range: 16,
                castTimeTicks: 90
            }
        },
        displacement: {
            title: 'Displacement',
            tag: 'Control',
            hint: 'Moves or pushes targets without hiding the simple control payload.',
            values: {
                label: 'force push',
                description: 'Shoves a hostile pawn away from the caster.',
                delivery: 'instant',
                targetShape: 'Single',
                primaryTargetType: 'Pawn',
                pawnAffinity: 'Foe',
                payloads: [
                    { type: 'damage', amount: 6, damageDef: 'Blunt' },
                    { type: 'knockback', distance: 4 }
                ],
                manaCost: 10,
                cooldownTicks: 180,
                range: 12,
                castTimeTicks: 15
            }
        },
        sustained: {
            title: 'Sustained Link',
            tag: 'Maintained',
            hint: 'Applies a maintained status effect with break rules.',
            values: {
                label: 'might',
                description: 'Maintains a strengthening enchantment while range and line of sight hold.',
                delivery: 'sustained',
                targetShape: 'Single',
                primaryTargetType: 'Pawn',
                pawnAffinity: 'Ally',
                allowSelfTarget: true,
                scaledAttributes: ['Duration', 'Cooldown'],
                payloads: [
                    { type: 'status', statusEffectDef: 'MFV_Status_Might', durationTicks: -1 }
                ],
                durationTicks: 1800,
                manaCost: 18,
                cooldownTicks: 360,
                range: 12,
                castTimeTicks: 30
            }
        },
        forcefield: {
            title: 'Force Field',
            tag: 'Defense',
            hint: 'Maintains a protective force field on an allied pawn with break rules.',
            values: {
                label: 'mana shield',
                description: 'Wraps an allied pawn in a protective field that blunts incoming damage.',
                delivery: 'forcefield',
                targetShape: 'Single',
                primaryTargetType: 'Pawn',
                pawnAffinity: 'Ally',
                allowSelfTarget: true,
                payloads: [],
                durationTicks: 1800,
                manaCost: 25,
                cooldownTicks: 600,
                range: 12,
                castTimeTicks: 45,
                scaledAttributes: ['Duration', 'Cooldown']
            }
        }
    };

    const actionTypes = {
        SequenceActionDef: {
            title: 'Sequence',
            className: 'MagicFramework.Definitions.SequenceActionDef',
            summary: node => `${countSlot(node, 'actions')} ordered child action(s)`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Spell sequence')
            ],
            slots: {
                actions: 'Actions'
            }
        },
        EffectActionDef: {
            title: 'Effect',
            className: 'MagicFramework.Definitions.EffectActionDef',
            summary: node => `${node.fields.effectDef || 'No effect'} at ${node.fields.locationSource || 'CurrentTarget'}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Play effect'),
                textField('effectDef', 'Effect Def', 'PsycastPsychicEffect'),
                textField('soundDef', 'Sound Def', ''),
                selectField('locationSource', 'Location Source', ['CurrentCell', 'CurrentTarget', 'InitialTarget', 'Caster'], 'CurrentTarget'),
                boolField('attachToTarget', 'Attach To Target', true)
            ]
        },
        ProceduralFXActionDef: {
            title: 'Procedural FX',
            className: 'MagicFramework.Definitions.ProceduralFXActionDef',
            summary: node => `${node.fields.fxEvent || 'Auto'} at ${node.fields.locationSource || 'CurrentTarget'}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Procedural FX'),
                selectField('fxEvent', 'FX Event', ['Auto', 'CastStart', 'ProjectileLaunch', 'ProjectileImpact', 'Impact', 'AreaPulse', 'Explosion', 'SustainStart', 'SustainTick', 'SustainEnd'], 'Auto'),
                selectField('locationSource', 'Location Source', ['CurrentCell', 'CurrentTarget', 'InitialTarget', 'Caster'], 'CurrentTarget')
            ]
        },
        LaunchProjectileActionDef: {
            title: 'Launch Projectile',
            className: 'MagicFramework.Definitions.LaunchProjectileActionDef',
            summary: node => `${node.fields.projectileDef || 'Projectile'} with ${countSlot(node, 'onImpactActions')} impact action(s)`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Launch projectile'),
                textField('projectileDef', 'Projectile Def', 'MF_Projectile'),
                selectField('launchOrigin', 'Launch Origin', ['Caster', 'CurrentTarget', 'CurrentCell'], 'Caster'),
                selectField('targetSource', 'Target Source', ['CurrentTarget', 'CurrentCell', 'Caster'], 'CurrentTarget'),
                boolField('preventFriendlyFire', 'Prevent Friendly Fire', false),
                numberField('impactTimeoutPaddingTicks', 'Impact Timeout Padding', 60, 1)
            ],
            slots: {
                onImpactActions: 'On Impact'
            }
        },
        ExplosionActionDef: {
            title: 'Explosion',
            className: 'MagicFramework.Definitions.ExplosionActionDef',
            summary: node => `${node.fields.damageAmount || 0} ${node.fields.damageDef || 'Flame'} in radius ${node.fields.radius || 0}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Explosion'),
                numberField('radius', 'Radius', 3.9, 0.1),
                numberField('damageAmount', 'Damage Amount', 12, 0.1),
                textField('damageDef', 'Damage Def', 'Flame'),
                numberField('fireChance', 'Fire Chance', 0.35, 0.01),
                boolField('damageFalloff', 'Damage Falloff', false),
                textField('explosionSoundDef', 'Explosion Sound', ''),
                textField('explosionEffectDef', 'Explosion Effect', '')
            ]
        },
        ApplyToTargetsActionDef: {
            title: 'Apply To Targets',
            className: 'MagicFramework.Definitions.ApplyToTargetsActionDef',
            summary: node => `Query radius ${(node.query && node.query.radius) || 0}, then ${countSlot(node, 'actions')} action(s)`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Apply to targets')
            ],
            query: true,
            slots: {
                actions: 'Actions'
            }
        },
        PersistentAreaZoneActionDef: {
            title: 'Persistent Area Zone',
            className: 'MagicFramework.Definitions.PersistentAreaZoneActionDef',
            summary: node => `${node.fields.markerThingDef || 'Marker'} for ${node.fields.durationTicks || 0} ticks`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Persistent area zone'),
                textField('markerThingDef', 'Marker Thing Def', 'MF_FlameFieldMarker'),
                numberField('zoneRadius', 'Zone Radius', 3, 0.1),
                numberField('pulseIntervalTicks', 'Pulse Interval', 60, 1),
                numberField('durationTicks', 'Duration Ticks', 600, 1),
                boolField('pulseAtCenter', 'Pulse At Center', false),
                selectField('pawnAffinity', 'Pawn Affinity', ['All', 'Ally', 'Foe'], 'All'),
                boolField('includeCaster', 'Include Caster', false),
                boolField('replaceExistingForCaster', 'Replace Existing For Caster', true),
                boolField('requiresConcentration', 'Requires Concentration', false),
                numberField('sustainedManaCost', 'Sustained Mana Cost', 0, 0.1),
                numberField('sustainedManaCostIntervalTicks', 'Sustained Mana Interval', 60, 1)
            ],
            slots: {
                onCreateActions: 'On Create',
                onPulseActions: 'On Pulse',
                actions: 'Pulse Payload',
                onBreakActions: 'On Break',
                onEndActions: 'On End'
            }
        },
        SustainedStatModifierActionDef: {
            title: 'Sustained Modifier',
            className: 'MagicFramework.Definitions.SustainedStatModifierActionDef',
            summary: node => `${node.fields.statusEffectDef || 'Status'} up to ${node.fields.maxDurationTicks || -1} ticks`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Sustained modifier'),
                textField('statusEffectDef', 'Status Effect Def', 'MFV_Status_Might'),
                selectField('targetSource', 'Target Source', ['Caster', 'CurrentTarget'], 'CurrentTarget'),
                numberField('maxDurationTicks', 'Max Duration', 1800, 1),
                numberField('maxRange', 'Max Range', 12, 0.1),
                boolField('breakWhenCasterDowned', 'Break When Caster Downed', true),
                boolField('breakWhenTargetDowned', 'Break When Target Downed', false),
                boolField('breakWhenTargetOutOfRange', 'Break When Target Out Of Range', true),
                boolField('breakWhenLineOfSightLost', 'Break When LOS Lost', true),
                numberField('pulseIntervalTicks', 'Pulse Interval', -1, 1)
            ],
            slots: {
                onPulseActions: 'On Pulse',
                onBreakActions: 'On Break'
            }
        },
        ApplyForceFieldActionDef: {
            title: 'Force Field',
            className: 'MagicFramework.Definitions.ApplyForceFieldActionDef',
            summary: node => `Damage factor ${node.fields.damageFactor || 0.5}, duration ${node.fields.maxDurationTicks || -1}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Force field'),
                selectField('targetSource', 'Target Source', ['Caster', 'CurrentTarget'], 'CurrentTarget'),
                numberField('maxDurationTicks', 'Max Duration', 1800, 1),
                numberField('maxRange', 'Max Range', 12, 0.1),
                numberField('damageFactor', 'Damage Factor', 0.5, 0.05),
                boolField('absorbFullyWithMana', 'Absorb Fully With Mana', false),
                numberField('manaCostPerDamageAbsorbed', 'Mana Per Damage', 1, 0.1),
                boolField('breakWhenCasterDowned', 'Break When Caster Downed', true),
                boolField('breakWhenTargetOutOfRange', 'Break When Target Out Of Range', true),
                boolField('breakWhenLineOfSightLost', 'Break When LOS Lost', true)
            ],
            slots: {
                onCreateActions: 'On Create',
                onPulseActions: 'On Pulse',
                onExpireActions: 'On Expire',
                onBreakActions: 'On Break'
            }
        },
        DelayActionDef: {
            title: 'Delay',
            className: 'MagicFramework.Definitions.DelayActionDef',
            summary: node => `${node.fields.delayTicks || 0} ticks, then ${countSlot(node, 'actions')} action(s)`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Delay'),
                numberField('delayTicks', 'Delay Ticks', 60, 1),
                boolField('replaceExistingForCaster', 'Replace Existing For Caster', true)
            ],
            slots: {
                actions: 'Delayed Actions'
            }
        },
        RepeatActionDef: {
            title: 'Repeat',
            className: 'MagicFramework.Definitions.RepeatActionDef',
            summary: node => `${node.fields.repeatCount || 1} repeats every ${node.fields.intervalTicks || 60} ticks`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Repeat'),
                numberField('intervalTicks', 'Interval Ticks', 60, 1),
                numberField('repeatCount', 'Repeat Count', 1, 1),
                boolField('includeImmediate', 'Include Immediate', true),
                boolField('replaceExistingForCaster', 'Replace Existing For Caster', true)
            ],
            slots: {
                actions: 'Repeated Actions'
            }
        },
        DamageActionDef: {
            title: 'Damage',
            className: 'MagicFramework.Definitions.DamageActionDef',
            summary: node => `${node.fields.amount || 0} ${node.fields.damageDef || 'damage'}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Apply damage'),
                numberField('amount', 'Amount', 10, 0.1),
                textField('damageDef', 'Damage Def', 'Blunt'),
                numberField('armorPenetration', 'Armor Penetration', 0, 0.01),
                selectField('guiltPolicy', 'Guilt Policy', ['None', 'Damage'], 'None'),
                boolField('useCombatLog', 'Use Combat Log', false)
            ]
        },
        HealActionDef: {
            title: 'Heal',
            className: 'MagicFramework.Definitions.HealActionDef',
            summary: node => `Heal ${node.fields.amount || 0}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Apply healing'),
                numberField('amount', 'Amount', 15, 0.1),
                numberField('permanentHealingAmount', 'Permanent Healing', 0, 0.1),
                boolField('healPermanentInjuries', 'Heal Permanent Injuries', false),
                boolField('regenerateMissingParts', 'Regenerate Missing Parts', false)
            ]
        },
        ApplyStatusEffectActionDef: {
            title: 'Apply Status',
            className: 'MagicFramework.Definitions.ApplyStatusEffectActionDef',
            summary: node => node.fields.statusEffectDef || 'Status effect',
            fields: [
                textField('debugLabel', 'Debug Label', 'Apply status'),
                textField('statusEffectDef', 'Status Effect Def', 'MFV_Status_Haste'),
                selectField('targetSource', 'Target Source', ['Caster', 'CurrentTarget'], 'CurrentTarget'),
                numberField('durationTicks', 'Override Duration', -1, 1),
                boolField('replaceExistingFromCasterSpell', 'Replace Existing', true)
            ]
        },
        ApplyHediffActionDef: {
            title: 'Apply Hediff',
            className: 'MagicFramework.Definitions.ApplyHediffActionDef',
            summary: node => `${node.fields.hediffDef || 'Hediff'} severity ${node.fields.severity || 0}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Apply hediff'),
                textField('hediffDef', 'Hediff Def', 'Burn'),
                selectField('targetSource', 'Target Source', ['Caster', 'CurrentTarget'], 'CurrentTarget'),
                numberField('severity', 'Severity', 0.2, 0.01),
                selectField('addMode', 'Add Mode', ['Default', 'Replace', 'TryAdd', 'SoftReplace'], 'Default'),
                boolField('removeAfterDuration', 'Remove After Duration', false),
                numberField('durationTicks', 'Duration', 0, 1)
            ]
        },
        KnockbackActionDef: {
            title: 'Knockback',
            className: 'MagicFramework.Definitions.KnockbackActionDef',
            summary: node => `${node.fields.distance || 0} cells from ${node.fields.originSource || 'Caster'}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Knockback'),
                selectField('originSource', 'Origin Source', ['CurrentCell', 'CurrentTarget', 'InitialTarget', 'Caster'], 'Caster'),
                numberField('distance', 'Distance', 3, 1),
                boolField('requireStandableDestination', 'Require Standable', true),
                boolField('requireWalkableDestination', 'Require Walkable', true),
                numberField('impactDamageAmount', 'Impact Damage', 0, 0.1),
                textField('impactDamageDef', 'Impact Damage Def', 'Blunt')
            ]
        },
        SummonPawnActionDef: {
            title: 'Summon Pawn',
            className: 'MagicFramework.Definitions.SummonPawnActionDef',
            summary: node => `${node.fields.pawnKindDef || 'Pawn'} for ${node.fields.durationTicks || 0} ticks`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Summon pawn'),
                textField('pawnKindDef', 'PawnKind Def', 'Husky'),
                numberField('durationTicks', 'Duration', 2500, 1),
                boolField('replaceExistingForCaster', 'Replace Existing For Caster', true),
                boolField('setFactionToPlayer', 'Set Faction To Player', true),
                boolField('joinLord', 'Join Lord', true)
            ],
            slots: {
                onCreateActions: 'On Create',
                onExpireActions: 'On Expire',
                onBreakActions: 'On Break'
            }
        },
        TeleportActionDef: {
            title: 'Teleport',
            className: 'MagicFramework.Definitions.TeleportActionDef',
            summary: node => `${node.fields.subjectSource || 'Caster'} to ${node.fields.destinationSource || 'CurrentCell'}`,
            fields: [
                textField('debugLabel', 'Debug Label', 'Teleport'),
                selectField('subjectSource', 'Subject Source', ['Caster', 'CurrentTarget', 'InitialTarget'], 'Caster'),
                selectField('destinationSource', 'Destination Source', ['CurrentCell', 'InitialTargetCell', 'CurrentTargetCell', 'CasterCell', 'CasterAdjacentCell', 'RandomCellNearSubject', 'RandomCellNearCaster', 'RandomCellNearCurrentCell', 'RandomCellNearInitialTarget'], 'CurrentCell'),
                boolField('swapWithCaster', 'Swap With Caster', false),
                boolField('requireStandableDestination', 'Require Standable', true),
                boolField('requireWalkableDestination', 'Require Walkable', true),
                boolField('preserveDrafted', 'Preserve Drafted', true),
                numberField('postTeleportStunTicks', 'Post Teleport Stun', 0, 1)
            ]
        }
    };

    const rootActionOptions = [
        'SequenceActionDef',
        'EffectActionDef',
        'ProceduralFXActionDef',
        'LaunchProjectileActionDef',
        'ExplosionActionDef',
        'ApplyToTargetsActionDef',
        'PersistentAreaZoneActionDef',
        'SustainedStatModifierActionDef',
        'ApplyForceFieldActionDef',
        'DelayActionDef',
        'RepeatActionDef',
        'DamageActionDef',
        'HealActionDef',
        'ApplyStatusEffectActionDef',
        'ApplyHediffActionDef',
        'KnockbackActionDef',
        'SummonPawnActionDef',
        'TeleportActionDef'
    ];

    let selectedPattern = 'projectileDamage';
    let activeBuilderTab = 'spell';
    let currentMode = 'simple';
    let xmlVisible = false;
    let currentPayloads = [];
    let payloadIdSeed = 1;
    let actionIdSeed = 1;
    let advancedActions = [];
    let selectedActionId = '';
    let advancedDirty = false;

    function init() {
        workspaceTabs.forEach(button => button.addEventListener('click', () => applyBuilderTab(button.dataset.builderTab, false)));
        renderActionTypeOptions();
        renderPatterns();
        applyPattern(selectedPattern);
        applyBuilderTab(activeBuilderTab, false);
        form.addEventListener('input', event => {
            if (!event.target.closest('.advanced-workflow')) updatePreview();
        });
        form.addEventListener('change', event => {
            if (!event.target.closest('.advanced-workflow')) updatePreview();
        });
        document.getElementById('label').addEventListener('input', syncGeneratedDefs);
        payloadToolbar.addEventListener('click', addPayloadFromButton);
        payloadStack.addEventListener('input', handlePayloadInput);
        payloadStack.addEventListener('change', handlePayloadInput);
        payloadStack.addEventListener('click', handlePayloadClick);
        modeButtons.forEach(button => button.addEventListener('click', () => setMode(button.dataset.mode)));
        actionTree.addEventListener('click', handleActionTreeClick);
        actionInspector.addEventListener('input', handleInspectorInput);
        actionInspector.addEventListener('change', handleInspectorInput);
        actionInspector.addEventListener('click', handleInspectorClick);
        addRootActionBtn.addEventListener('click', addRootAction);
        regenerateTreeBtn.addEventListener('click', regenerateAdvancedTree);
        viewXmlBtn.addEventListener('click', toggleXml);
        copyXmlBtn.addEventListener('click', copyXml);
        resetBtn.addEventListener('click', () => applyPattern(selectedPattern));
    }

    function renderPatterns() {
        patternGrid.innerHTML = Object.entries(patterns).map(([key, pattern]) => `
            <button type="button" class="pattern-card" data-pattern="${key}">
                <span>${pattern.tag}</span>
                <strong>${pattern.title}</strong>
                <small>${pattern.hint}</small>
            </button>
        `).join('');

        patternGrid.querySelectorAll('.pattern-card').forEach(button => {
            button.addEventListener('click', () => applyPattern(button.dataset.pattern));
        });
    }

    function applyPattern(patternKey) {
        selectedPattern = patternKey;
        const pattern = patterns[patternKey];
        Object.entries(formDefaults).forEach(([id, value]) => {
            if (id !== 'scaledAttributes') setVal(id, value, false);
        });
        setScaledAttributes(pattern.values.scaledAttributes || defaultScaledAttributesForPattern(patternKey));
        Object.entries(pattern.values).forEach(([id, value]) => {
            if (id !== 'payloads' && id !== 'scaledAttributes') setVal(id, value);
        });
        currentPayloads = clonePayloads(pattern.values.payloads || [{ type: 'damage', amount: 10, damageDef: 'Blunt' }]);
        renderPayloadStack();
        syncGeneratedDefs();
        advancedDirty = false;
        advancedActions = buildGeneratedActionTree(getState());
        selectedActionId = firstActionId(advancedActions);
        patternGrid.querySelectorAll('.pattern-card').forEach(button => {
            button.classList.toggle('active', button.dataset.pattern === patternKey);
        });
        updatePreview();
    }

    function defaultScaledAttributesForPattern(patternKey) {
        if (['heal', 'buff', 'sustained'].includes(patternKey)) return ['Healing', 'Duration', 'Cooldown'];
        if (patternKey === 'persistentZone') return ['Damage', 'Radius', 'Duration'];
        if (patternKey === 'summon') return ['Duration', 'Cooldown'];
        return ['Damage', 'Cooldown'];
    }

    function setScaledAttributes(values) {
        const selected = new Set(values || []);
        document.querySelectorAll('.scaled-attribute').forEach(input => {
            input.checked = selected.has(input.value);
        });
    }

    function clonePayloads(payloads) {
        return payloads.map(payload => ({
            ...(payloadTypes[payload.type] ? payloadTypes[payload.type].defaults : {}),
            ...payload,
            id: `payload-${payloadIdSeed++}`
        }));
    }

    function addPayloadFromButton(event) {
        const button = event.target.closest('.payload-add');
        if (!button || button.disabled) return;

        const type = button.dataset.payloadType;
        currentPayloads.push({
            ...payloadTypes[type].defaults,
            type,
            id: `payload-${payloadIdSeed++}`
        });
        renderPayloadStack();
        updatePreview();
    }

    function handlePayloadInput(event) {
        const input = event.target.closest('[data-payload-id][data-payload-key]');
        if (!input) return;

        const payload = currentPayloads.find(item => item.id === input.dataset.payloadId);
        if (!payload) return;

        const field = getPayloadField(payload.type, input.dataset.payloadKey);
        if (input.dataset.defSelect === 'true') {
            const customKey = customDefFlagKey(input.dataset.payloadKey);
            if (input.value === customDefOption) {
                payload[customKey] = true;
                payload[input.dataset.payloadKey] = customDefValue(payload, field);
            } else {
                delete payload[customKey];
                payload[input.dataset.payloadKey] = input.value;
            }
            renderPayloadStack();
            updatePreview();
            return;
        }

        if (input.dataset.customDefInput === 'true') {
            payload[customDefFlagKey(input.dataset.payloadKey)] = true;
        }

        if (input.type === 'checkbox') {
            payload[input.dataset.payloadKey] = input.checked;
        } else {
            payload[input.dataset.payloadKey] = input.type === 'number' ? parseNumber(input.value) : input.value;
        }
        if (input.dataset.payloadKey === 'showCue') {
            renderPayloadStack();
        }
        updatePreview();
    }

    function handlePayloadClick(event) {
        const removeButton = event.target.closest('[data-remove-payload]');
        if (!removeButton) return;

        const id = removeButton.dataset.removePayload;
        currentPayloads = currentPayloads.filter(payload => payload.id !== id);
        if (currentPayloads.length === 0) {
            currentPayloads = clonePayloads([{ type: 'damage', amount: 10, damageDef: 'Blunt' }]);
        }
        renderPayloadStack();
        updatePreview();
    }

    function renderPayloadStack() {
        payloadStack.innerHTML = currentPayloads.map((payload, index) => {
            const def = payloadTypes[payload.type];
            const fields = def.fields.map(field => renderPayloadField(payload, field)).join('');

            return `
                <article class="payload-card" data-payload-card="${payload.id}">
                    <div class="payload-card-header">
                        <div>
                            <span>Payload ${index + 1}</span>
                            <strong>${def.title}</strong>
                        </div>
                        <button type="button" class="payload-remove" data-remove-payload="${payload.id}" aria-label="Remove ${def.title} payload">Remove</button>
                    </div>
                    <div class="payload-fields">${fields}</div>
                </article>
            `;
        }).join('');
    }

    function renderPayloadField(payload, field) {
        if (field.type === 'def') {
            return renderDefField(payload, field);
        }

        if (field.type === 'checkbox') {
            return `
                <label class="payload-check-field">
                    <input
                        type="checkbox"
                        ${payload[field.key] ? 'checked' : ''}
                        data-payload-id="${payload.id}"
                        data-payload-key="${field.key}">
                    <span>${field.label}</span>
                </label>
            `;
        }

        if (field.type === 'statusCue') {
            return renderStatusCueField(payload, field);
        }

        return `
            <label>
                <span>${field.label}</span>
                <input
                    type="${field.type}"
                    value="${escapeHtml(payload[field.key] == null ? '' : payload[field.key])}"
                    step="${field.step || ''}"
                    data-payload-id="${payload.id}"
                    data-payload-key="${field.key}">
            </label>
        `;
    }

    function renderStatusCueField(payload, field) {
        const cue = statusCueHediffs[payload.statusEffectDef] || 'Defined by the selected status effect';
        return `
            <div class="status-cue-field ${payload.showCue ? '' : 'is-hidden'}" title="The status effect supplies gameplay behavior. The hediff is the visible pawn health/status cue attached by that status.">
                <span>${field.label}</span>
                <strong>${escapeHtml(cue)}</strong>
                <small>Status effects own duration, scaling, cleanup, and stat modifiers. The hediff cue is only the visible pawn marker.</small>
            </div>
        `;
    }

    function renderDefField(payload, field) {
        const value = String(payload[field.key] == null ? '' : payload[field.key]);
        const options = field.options || [];
        const isKnownValue = options.includes(value);
        const showCustom = payload[customDefFlagKey(field.key)] || (value && !isKnownValue);
        const selectValue = showCustom ? customDefOption : value;
        const optionMarkup = options.map(option => `
            <option value="${escapeHtml(option)}" ${selectValue === option ? 'selected' : ''}>${escapeHtml(option)}</option>
        `).join('');

        return `
            <label class="def-select-field">
                <span>${field.label}</span>
                <select
                    data-payload-id="${payload.id}"
                    data-payload-key="${field.key}"
                    data-def-select="true">
                    ${optionMarkup}
                    <option value="${customDefOption}" ${selectValue === customDefOption ? 'selected' : ''}>New / custom...</option>
                </select>
                <input
                    class="custom-def-input"
                    type="text"
                    value="${escapeHtml(showCustom ? value : '')}"
                    placeholder="Enter new defName"
                    data-payload-id="${payload.id}"
                    data-payload-key="${field.key}"
                    data-custom-def-input="true"
                    ${showCustom ? '' : 'hidden'}>
            </label>
        `;
    }

    function getPayloadField(type, key) {
        return payloadTypes[type] ? payloadTypes[type].fields.find(field => field.key === key) : undefined;
    }

    function customDefValue(payload, field) {
        const currentValue = String(payload[field.key] == null ? '' : payload[field.key]);
        return field.options && field.options.includes(currentValue) ? '' : currentValue;
    }

    function customDefFlagKey(key) {
        return `__custom_${key}`;
    }

    function syncGeneratedDefs() {
        const label = getVal('label');
        const safeName = toPascal(label);
        if (!safeName) return;
        const defName = `MF_${safeName}`;
        setVal('defName', defName, false);
        setVal('gizmoIconPath', `UI/Gizmos/Spells/${defName}`, false);

        const suffixes = {
            projectileDef: 'Projectile',
            castEffectDef: 'CastEffect',
            impactEffectDef: 'ImpactEffect'
        };
        Object.entries(suffixes).forEach(([id, suffix]) => {
            const el = document.getElementById(id);
            if (el && (!el.value || el.value.startsWith('MF_'))) {
                el.value = `${defName}${suffix}`;
            }
        });
    }

    function getState() {
        return {
            pattern: patterns[selectedPattern],
            label: getVal('label'),
            defName: getVal('defName'),
            description: getVal('description'),
            range: getVal('range'),
            castTimeTicks: getVal('castTimeTicks'),
            gizmoIconPath: getVal('gizmoIconPath'),
            delivery: getVal('delivery'),
            targetShape: getVal('targetShape'),
            primaryTargetType: getVal('primaryTargetType'),
            pawnAffinity: getVal('pawnAffinity'),
            targetRadius: getVal('targetRadius'),
            lineLength: getVal('lineLength'),
            coneAngleDegrees: getVal('coneAngleDegrees'),
            wallLength: getVal('wallLength'),
            maxChains: getVal('maxChains'),
            projectileDef: getVal('projectileDef'),
            includePawns: getVal('includePawns'),
            includeBuildings: getVal('includeBuildings'),
            includeItems: getVal('includeItems'),
            allowSelfTarget: getVal('allowSelfTarget'),
            useCasterAsTarget: getVal('useCasterAsTarget'),
            requireLineOfSight: getVal('requireLineOfSight'),
            requireStandableCell: getVal('requireStandableCell'),
            requireWalkableCell: getVal('requireWalkableCell'),
            requireWaterCell: getVal('requireWaterCell'),
            requireResurrectableCorpse: getVal('requireResurrectableCorpse'),
            durationTicks: getVal('durationTicks'),
            zoneMarkerDef: getVal('zoneMarkerDef'),
            pulseIntervalTicks: getVal('pulseIntervalTicks'),
            castEffectDef: getVal('castEffectDef'),
            impactEffectDef: getVal('impactEffectDef'),
            soundDef: getVal('soundDef'),
            manaCost: getVal('manaCost'),
            cooldownTicks: getVal('cooldownTicks'),
            researchPrerequisite: getVal('researchPrerequisite'),
            minimumCasterLevel: getVal('minimumCasterLevel'),
            casterLevelFactor: getVal('casterLevelFactor'),
            canBeLearned: getVal('canBeLearned'),
            requireArcaneGift: getVal('requireArcaneGift'),
            appendSpellSummary: getVal('appendSpellSummary'),
            scaledAttributes: Array.from(document.querySelectorAll('.scaled-attribute:checked')).map(input => input.value),
            payloads: currentPayloads.map(payload => cleanPayloadForState(payload))
        };
    }

    function cleanPayloadForState(payload) {
        const result = {};
        Object.keys(payload).forEach(key => {
            if (key !== 'id' && !key.startsWith('__custom_')) {
                result[key] = payload[key];
            }
        });
        return result;
    }

    function updatePreview(renderInspectorPanel = true) {
        const state = getState();
        updateFieldVisibility(state);
        patternHint.textContent = state.pattern.hint;
        renderAdvancedWorkflow(state, renderInspectorPanel);
        humanSummary.innerHTML = buildSummary(state);
        xmlPreview.innerHTML = highlightXml(buildXml(state));
    }

    function updateFieldVisibility(state) {
        const showProjectile = state.delivery === 'projectile';
        const showRadius = state.targetShape === 'Radius' || state.delivery === 'area' || state.delivery === 'persistent';
        const showLine = state.targetShape === 'Line' || state.targetShape === 'Cone';
        const showCone = state.targetShape === 'Cone';
        const showWall = state.targetShape === 'Wall';
        const showChain = state.targetShape === 'Chain';
        const showZone = state.delivery === 'persistent';
        const showDuration = ['persistent', 'sustained', 'forcefield'].includes(state.delivery);
        const showImpactFx = ['projectile', 'area', 'persistent'].includes(state.delivery);

        setGroupAvailable('targetRadius', showRadius, 'Radius is only used by radius, burst, and persistent-zone targeting.');
        setGroupAvailable('lineLength', showLine, 'Line Length is only used by Line and Cone targeting.');
        setGroupAvailable('coneAngleDegrees', showCone, 'Cone Angle is only used by Cone targeting.');
        setGroupAvailable('wallLength', showWall, 'Wall Length is only used by Wall targeting.');
        setGroupAvailable('maxChains', showChain, 'Max Chains is only used by Chain targeting.');
        setGroupAvailable('projectileDef', showProjectile, 'Projectile defs are only used when delivery is Projectile.');
        setGroupAvailable('durationTicks', showDuration, 'Effect Duration is used by persistent zones, sustained links, and force fields. Payload-specific durations live inside payload cards.');
        setGroupAvailable('zoneMarkerDef', showZone, 'Zone Marker Def is only used by Persistent Zone delivery.');
        setGroupAvailable('pulseIntervalTicks', showZone, 'Pulse Interval is only used by Persistent Zone delivery.');
        setGroupAvailable('impactEffectDef', showImpactFx, 'Impact effects are used by projectile, area burst, and persistent zone patterns.');
        updateTargetingCompatibility(state);
        updatePayloadButtons(state);
    }

    function updateTargetingCompatibility(state) {
        const messages = targetingCompatibilityMessages(state);
        targetingCompatibility.innerHTML = messages.length
            ? messages.map(message => `<div class="compatibility-message">${escapeHtml(message)}</div>`).join('')
            : '<div class="compatibility-message ok">Targeting choices are internally consistent.</div>';
    }

    function targetingCompatibilityMessages(state) {
        const messages = [];
        const primaryIsPawnOnly = state.primaryTargetType === 'Pawn';
        const primaryCanTargetThing = ['Thing', 'PawnOrThing'].includes(state.primaryTargetType);
        const primaryCanTargetCell = ['Cell', 'PawnOrCell'].includes(state.primaryTargetType);

        if (primaryIsPawnOnly && !state.includePawns) {
            messages.push('Primary Target is Pawn, but Pawns are disabled in Target Categories.');
        }

        if (state.primaryTargetType === 'Thing' && !state.includeBuildings && !state.includeItems) {
            messages.push('Primary Target is Thing, but both Buildings and Items are disabled.');
        }

        if (state.primaryTargetType === 'PawnOrThing' && !state.includePawns && !state.includeBuildings && !state.includeItems) {
            messages.push('Primary Target is PawnOrThing, but every target category is disabled.');
        }

        if (state.requireResurrectableCorpse) {
            if (!primaryCanTargetThing) {
                messages.push('Resurrectable corpse requires a thing-style primary target. Use Primary Target: Thing or PawnOrThing.');
            }
            if (state.includePawns || state.includeBuildings || !state.includeItems) {
                messages.push('Resurrectable corpse targeting should normally use Items only: disable Pawns and Buildings, enable Items.');
            }
            if (state.useCasterAsTarget) {
                messages.push('Use caster as target conflicts with Resurrectable corpse; the caster cannot be the corpse target.');
            }
        }

        if (state.useCasterAsTarget) {
            if (primaryCanTargetCell || state.targetShape !== 'Single') {
                messages.push('Use caster as target skips normal target selection, so cell shape/radius/line targeting settings will not choose an initial target.');
            }
            if (!state.allowSelfTarget && !state.requireResurrectableCorpse) {
                messages.push('Use caster as target usually pairs with Self target enabled for clarity.');
            }
        }

        if (state.requireWaterCell && (state.requireWalkableCell || state.requireStandableCell)) {
            messages.push('Water cell combined with Walkable or Standable requires all selected terrain gates at once; that is rare and may reject most targets.');
        }

        return messages;
    }

    function updatePayloadButtons(state) {
        payloadToolbar.querySelectorAll('.payload-add').forEach(button => {
            const type = button.dataset.payloadType;
            const reason = payloadDisabledReason(type, state);
            button.disabled = Boolean(reason);
            button.classList.toggle('is-disabled', Boolean(reason));
            button.title = reason || `Add ${payloadTypes[type].title} payload`;
        });
    }

    function payloadDisabledReason(type, state) {
        if (state.delivery === 'forcefield') {
            return 'Force Field authors ApplyForceFieldActionDef directly; payload stacks are not used for this pattern.';
        }

        if (state.delivery === 'sustained' && type !== 'status') {
            return 'Sustained Link currently authors one maintained status payload; use Instant or Projectile for mixed payload stacks.';
        }

        if (state.delivery === 'persistent' && type === 'summon') {
            return 'Persistent zones pulse effects over cells or pawns; summoning every pulse would usually be unsafe.';
        }

        if (state.delivery === 'area' && type === 'summon') {
            return 'Area Burst applies payloads to targets in a radius; summoning per target is intentionally not exposed here.';
        }

        return '';
    }

    function setGroupAvailable(id, available, reason) {
        const el = document.getElementById(id);
        const group = el ? el.closest('.form-group') : null;
        if (!el || !group) return;

        el.disabled = !available;
        group.classList.toggle('is-disabled', !available);
        group.title = available ? '' : reason;

        let help = group.querySelector('.field-state');
        if (!help) {
            help = document.createElement('small');
            help.className = 'field-state';
            group.appendChild(help);
        }

        help.textContent = available ? '' : reason;
    }

    function setMode(mode) {
        currentMode = mode === 'advanced' ? 'advanced' : 'simple';
        modeButtons.forEach(button => button.classList.toggle('active', button.dataset.mode === currentMode));
        simpleWorkflow.hidden = currentMode !== 'simple';
        advancedWorkflow.hidden = currentMode !== 'advanced';

        if (currentMode === 'advanced' && !advancedActions.length) {
            regenerateAdvancedTree();
            return;
        }

        updatePreview();
    }

    function applyBuilderTab(tab, refresh = true) {
        activeBuilderTab = tab || 'spell';
        workspaceTabs.forEach(button => button.classList.toggle('active', button.dataset.builderTab === activeBuilderTab));
        tabPanels.forEach(panel => panel.classList.toggle('is-tab-hidden', panel.dataset.tabPanel !== activeBuilderTab));
        if (appContainer) appContainer.classList.toggle('actions-focus', activeBuilderTab === 'actions');
        if (refresh) updatePreview();
    }

    function renderActionTypeOptions() {
        const options = rootActionOptions.map(type => `<option value="${type}">${escapeHtml(actionTypes[type].title)}</option>`).join('');
        rootActionType.innerHTML = options;
    }

    function renderAdvancedWorkflow(state, renderInspectorPanel = true) {
        if (currentMode === 'advanced' && !advancedActions.length) {
            advancedActions = buildGeneratedActionTree(state);
            selectedActionId = selectedActionId || firstActionId(advancedActions);
        }

        if (currentMode !== 'advanced') return;

        if (!selectedActionId || !findAction(advancedActions, selectedActionId)) {
            selectedActionId = firstActionId(advancedActions);
        }

        actionTree.innerHTML = advancedActions.length
            ? advancedActions.map(node => renderActionNode(node)).join('')
            : '<div class="slot-empty">No root actions yet.</div>';
        if (renderInspectorPanel) renderActionInspector();
        renderValidation(state);
    }

    function renderActionNode(node) {
        const def = actionTypes[node.type];
        const slots = def.slots || {};
        const slotMarkup = Object.entries(slots).map(([slotKey, slotLabel]) => renderActionSlot(node, slotKey, slotLabel)).join('');
        return `
            <div class="action-node" data-action-id="${node.id}">
                <button type="button" class="action-node-main ${node.id === selectedActionId ? 'active' : ''}" data-select-action="${node.id}">
                    <span class="action-node-title">
                        <span>${escapeHtml(def.title)}</span>
                        <small>${escapeHtml(node.fields.debugLabel || node.type)}</small>
                    </span>
                    <span class="action-node-summary">${escapeHtml(def.summary ? def.summary(node) : '')}</span>
                </button>
                ${slotMarkup}
            </div>
        `;
    }

    function renderActionSlot(parentNode, slotKey, slotLabel) {
        const nodes = parentNode.slots && parentNode.slots[slotKey] ? parentNode.slots[slotKey] : [];
        const addOptions = rootActionOptions.map(type => `<option value="${type}">${escapeHtml(actionTypes[type].title)}</option>`).join('');
        return `
            <div class="action-slot" data-slot-owner="${parentNode.id}" data-slot-key="${slotKey}">
                <div class="action-slot-header">
                    <span>${escapeHtml(slotLabel)}</span>
                    <select data-add-type-for="${parentNode.id}" data-add-slot="${slotKey}">${addOptions}</select>
                    <button type="button" class="btn btn-secondary" data-add-child="${parentNode.id}" data-add-slot="${slotKey}">Add</button>
                </div>
                ${nodes.length ? nodes.map(node => renderActionNode(node)).join('') : '<div class="slot-empty">Empty slot.</div>'}
            </div>
        `;
    }

    function renderActionInspector() {
        const found = findAction(advancedActions, selectedActionId);
        if (!found) {
            actionInspector.innerHTML = '<div class="slot-empty">Select an action to edit it.</div>';
            return;
        }

        const node = found.node;
        const def = actionTypes[node.type];
        const typeOptions = rootActionOptions.map(type => `
            <option value="${type}" ${node.type === type ? 'selected' : ''}>${escapeHtml(actionTypes[type].title)}</option>
        `).join('');
        const coreFields = def.fields.slice(0, 4).map(field => renderInspectorField(node, field)).join('');
        const advancedFields = def.fields.slice(4).map(field => renderInspectorField(node, field)).join('');
        const queryFields = def.query ? renderQueryFields(node) : '';
        const canRemove = Boolean(found.parent) || advancedActions.length > 1;

        actionInspector.innerHTML = `
            <div class="inspector-heading">
                <span>Selected Action</span>
                <strong>${escapeHtml(def.title)}</strong>
            </div>
            <label>
                <span>Action Type</span>
                <select data-action-type="${node.id}">${typeOptions}</select>
            </label>
            <div class="inspector-fields">${coreFields}</div>
            ${advancedFields ? `
                <details class="field-details">
                    <summary>Advanced Fields</summary>
                    <div class="inspector-fields">${advancedFields}</div>
                </details>
            ` : ''}
            ${queryFields}
            <div class="inspector-actions">
                <button type="button" class="btn btn-secondary" data-move-action="${node.id}" data-direction="up">Move Up</button>
                <button type="button" class="btn btn-secondary" data-move-action="${node.id}" data-direction="down">Move Down</button>
                <button type="button" class="btn danger-button" data-remove-action="${node.id}" ${canRemove ? '' : 'disabled'}>Remove</button>
            </div>
        `;
    }

    function renderInspectorField(node, field) {
        const value = node.fields[field.key] != null ? node.fields[field.key] : (field.default != null ? field.default : '');
        if (field.type === 'checkbox') {
            return `
                <label class="payload-check-field">
                    <input type="checkbox" data-action-id="${node.id}" data-action-field="${field.key}" ${value ? 'checked' : ''}>
                    <span>${escapeHtml(field.label)}</span>
                </label>
            `;
        }

        if (field.type === 'select') {
            return `
                <label>
                    <span>${escapeHtml(field.label)}</span>
                    <select data-action-id="${node.id}" data-action-field="${field.key}">
                        ${field.options.map(option => `<option value="${escapeHtml(option)}" ${String(value) === option ? 'selected' : ''}>${escapeHtml(option)}</option>`).join('')}
                    </select>
                </label>
            `;
        }

        return `
            <label>
                <span>${escapeHtml(field.label)}</span>
                <input type="${field.type}" value="${escapeHtml(value)}" step="${field.step || ''}" data-action-id="${node.id}" data-action-field="${field.key}">
            </label>
        `;
    }

    function renderQueryFields(node) {
        const query = node.query || defaultQuery();
        const affinityOptions = ['All', 'Ally', 'Foe'].map(value => `<option value="${value}" ${query.pawnAffinity === value ? 'selected' : ''}>${value}</option>`).join('');
        const centerOptions = ['CurrentCell', 'CurrentTarget', 'InitialTarget', 'Caster'].map(value => `<option value="${value}" ${query.centerSource === value ? 'selected' : ''}>${value}</option>`).join('');
        return `
            <details class="field-details" open>
                <summary>Target Query</summary>
                <div class="query-fields">
                    <label><span>Query Radius</span><input type="number" step="0.1" value="${escapeHtml(query.radius)}" data-query-field="radius" data-action-id="${node.id}"></label>
                    <label><span>Center Source</span><select data-query-field="centerSource" data-action-id="${node.id}">${centerOptions}</select></label>
                    <label><span>Pawn Affinity</span><select data-query-field="pawnAffinity" data-action-id="${node.id}">${affinityOptions}</select></label>
                    <label class="payload-check-field"><input type="checkbox" data-query-field="includePawns" data-action-id="${node.id}" ${query.includePawns ? 'checked' : ''}><span>Include Pawns</span></label>
                    <label class="payload-check-field"><input type="checkbox" data-query-field="includeBuildings" data-action-id="${node.id}" ${query.includeBuildings ? 'checked' : ''}><span>Include Buildings</span></label>
                    <label class="payload-check-field"><input type="checkbox" data-query-field="includeItems" data-action-id="${node.id}" ${query.includeItems ? 'checked' : ''}><span>Include Items</span></label>
                    <label class="payload-check-field"><input type="checkbox" data-query-field="includeCaster" data-action-id="${node.id}" ${query.includeCaster ? 'checked' : ''}><span>Include Caster</span></label>
                </div>
            </details>
        `;
    }

    function handleActionTreeClick(event) {
        const selectButton = event.target.closest('[data-select-action]');
        if (selectButton) {
            selectedActionId = selectButton.dataset.selectAction;
            renderAdvancedWorkflow(getState());
            return;
        }

        const addButton = event.target.closest('[data-add-child]');
        if (!addButton) return;

        const select = actionTree.querySelector(`[data-add-type-for="${addButton.dataset.addChild}"][data-add-slot="${addButton.dataset.addSlot}"]`);
        addChildAction(addButton.dataset.addChild, addButton.dataset.addSlot, select ? select.value : 'EffectActionDef');
    }

    function handleInspectorInput(event) {
        const typeSelect = event.target.closest('[data-action-type]');
        if (typeSelect) {
            changeActionType(typeSelect.dataset.actionType, typeSelect.value);
            return;
        }

        const queryInput = event.target.closest('[data-query-field][data-action-id]');
        if (queryInput) {
            updateQueryField(queryInput);
            return;
        }

        const input = event.target.closest('[data-action-id][data-action-field]');
        if (!input) return;

        const found = findAction(advancedActions, input.dataset.actionId);
        if (!found) return;

        const field = actionTypes[found.node.type].fields.find(item => item.key === input.dataset.actionField);
        if (!field) return;
        found.node.fields[field.key] = parseFieldValue(input, field);
        markAdvancedDirty(false);
    }

    function handleInspectorClick(event) {
        const removeButton = event.target.closest('[data-remove-action]');
        if (removeButton && !removeButton.disabled) {
            removeAction(removeButton.dataset.removeAction);
            return;
        }

        const moveButton = event.target.closest('[data-move-action]');
        if (moveButton) {
            moveAction(moveButton.dataset.moveAction, moveButton.dataset.direction);
        }
    }

    function addRootAction() {
        const node = createActionNode(rootActionType.value || 'SequenceActionDef');
        advancedActions.push(node);
        selectedActionId = node.id;
        markAdvancedDirty();
    }

    function addChildAction(parentId, slotKey, type) {
        const found = findAction(advancedActions, parentId);
        if (!found) return;
        if (!found.node.slots) found.node.slots = {};
        if (!found.node.slots[slotKey]) found.node.slots[slotKey] = [];
        const node = createActionNode(type);
        found.node.slots[slotKey].push(node);
        selectedActionId = node.id;
        markAdvancedDirty();
    }

    function regenerateAdvancedTree() {
        advancedDirty = false;
        advancedActions = buildGeneratedActionTree(getState());
        selectedActionId = firstActionId(advancedActions);
        updatePreview();
    }

    function changeActionType(id, type) {
        const found = findAction(advancedActions, id);
        if (!found || !actionTypes[type]) return;
        const oldLabel = found.node.fields.debugLabel;
        const replacement = createActionNode(type);
        replacement.id = found.node.id;
        if (oldLabel) replacement.fields.debugLabel = oldLabel;
        found.node.type = replacement.type;
        found.node.fields = replacement.fields;
        found.node.slots = replacement.slots;
        found.node.query = replacement.query;
        markAdvancedDirty();
    }

    function updateQueryField(input) {
        const found = findAction(advancedActions, input.dataset.actionId);
        if (!found) return;
        if (!found.node.query) found.node.query = defaultQuery();
        found.node.query[input.dataset.queryField] = input.type === 'checkbox' ? input.checked : (input.type === 'number' ? parseNumber(input.value) : input.value);
        markAdvancedDirty(false);
    }

    function removeAction(id) {
        const found = findAction(advancedActions, id);
        if (!found) return;

        if (found.parent) {
            found.collection.splice(found.index, 1);
        } else if (advancedActions.length > 1) {
            advancedActions.splice(found.index, 1);
        }

        selectedActionId = firstActionId(advancedActions);
        markAdvancedDirty();
    }

    function moveAction(id, direction) {
        const found = findAction(advancedActions, id);
        if (!found) return;
        const targetIndex = direction === 'up' ? found.index - 1 : found.index + 1;
        if (targetIndex < 0 || targetIndex >= found.collection.length) return;
        const [node] = found.collection.splice(found.index, 1);
        found.collection.splice(targetIndex, 0, node);
        markAdvancedDirty();
    }

    function markAdvancedDirty(renderInspectorPanel = true) {
        advancedDirty = true;
        updatePreview(renderInspectorPanel);
    }

    function renderValidation(state) {
        const warnings = validateActionTree(advancedActions, state);
        const modeText = advancedDirty
            ? 'Advanced tree is custom. Simple payload changes will not alter it unless Regenerate is used.'
            : 'Advanced tree currently mirrors the simple pattern.';
        const items = [
            `<div class="validation-item ok">${escapeHtml(modeText)}</div>`,
            ...warnings.map(warning => `<div class="validation-item warning">${escapeHtml(warning)}</div>`)
        ];
        validationPanel.innerHTML = items.join('');
    }

    function validateActionTree(nodes) {
        const warnings = [];
        if (!nodes.length) warnings.push('The spell has no root actions.');
        walkActions(nodes, (node, ancestors) => {
            if (node.type === 'LaunchProjectileActionDef' && !countSlot(node, 'onImpactActions')) {
                warnings.push('A projectile has no on-impact actions.');
            }
            if (node.type === 'SummonPawnActionDef' && ancestors.some(parent => parent.type === 'PersistentAreaZoneActionDef' || parent.type === 'RepeatActionDef')) {
                warnings.push('A summon action is nested under a repeating or persistent action; verify this is intentional.');
            }
            if (node.type === 'ApplyToTargetsActionDef' && !countSlot(node, 'actions')) {
                warnings.push('An Apply To Targets node has a query but no child actions.');
            }
        });
        return warnings;
    }

    function buildGeneratedActionTree(state) {
        const payloadNodes = state.payloads.map(payload => payloadToActionNode(payload, state));
        const primaryDamage = state.payloads.find(payload => payload.type === 'damage') || payloadTypes.damage.defaults;
        const sustainedStatus = state.payloads.find(payload => payload.type === 'status') || payloadTypes.status.defaults;
        const areaPayloads = state.payloads.filter(payload => payload.type !== 'damage').map(payload => payloadToActionNode(payload, state));

        if (state.delivery === 'projectile') {
            return [
                actionNode('SequenceActionDef', { debugLabel: `${cap(state.label)} sequence` }, {
                    actions: [
                        effectNode(state, state.castEffectDef, 'Caster', false),
                        actionNode('LaunchProjectileActionDef', { debugLabel: `Launch ${state.label} projectile`, projectileDef: state.projectileDef }, {
                            onImpactActions: [
                                effectNode(state, state.impactEffectDef, 'CurrentTarget', true),
                                ...payloadNodes
                            ]
                        })
                    ]
                })
            ];
        }

        if (state.delivery === 'area') {
            const actions = [
                effectNode(state, state.castEffectDef, 'Caster', false),
                actionNode('ExplosionActionDef', {
                    debugLabel: `${cap(state.label)} explosion`,
                    radius: state.targetRadius,
                    damageAmount: primaryDamage.amount,
                    damageDef: primaryDamage.damageDef
                })
            ];
            if (areaPayloads.length) {
                actions.push(actionNode('ApplyToTargetsActionDef', { debugLabel: `Apply ${state.label} payload in radius` }, {
                    actions: areaPayloads
                }, defaultQuery({
                    debugLabel: `Targets around ${state.label} impact`,
                    radius: state.targetRadius,
                    centerSource: 'CurrentCell',
                    includePawns: state.includePawns,
                    includeBuildings: state.includeBuildings,
                    includeItems: state.includeItems,
                    includeCaster: state.allowSelfTarget,
                    pawnAffinity: state.pawnAffinity
                })));
            }
            return [actionNode('SequenceActionDef', { debugLabel: `${cap(state.label)} area burst` }, { actions })];
        }

        if (state.delivery === 'persistent') {
            return [
                actionNode('PersistentAreaZoneActionDef', {
                    debugLabel: `Create ${state.label} persistent zone`,
                    markerThingDef: state.zoneMarkerDef,
                    zoneRadius: state.targetRadius,
                    pulseIntervalTicks: state.pulseIntervalTicks,
                    durationTicks: state.durationTicks,
                    pulseAtCenter: false,
                    pawnAffinity: state.pawnAffinity,
                    includeCaster: state.allowSelfTarget,
                    replaceExistingForCaster: true
                }, {
                    onPulseActions: [effectNode(state, state.impactEffectDef, 'CurrentCell', false)],
                    actions: payloadNodes
                })
            ];
        }

        if (state.delivery === 'sustained') {
            return [
                actionNode('SustainedStatModifierActionDef', {
                    debugLabel: `${cap(state.label)} sustained effect`,
                    statusEffectDef: sustainedStatus.statusEffectDef,
                    targetSource: 'CurrentTarget',
                    maxDurationTicks: state.durationTicks,
                    maxRange: state.range,
                    breakWhenCasterDowned: true,
                    breakWhenTargetOutOfRange: true,
                    breakWhenLineOfSightLost: state.requireLineOfSight
                })
            ];
        }

        if (state.delivery === 'forcefield') {
            return [
                actionNode('ApplyForceFieldActionDef', {
                    debugLabel: `${cap(state.label)} force field`,
                    targetSource: 'CurrentTarget',
                    maxDurationTicks: state.durationTicks,
                    maxRange: state.range,
                    breakWhenCasterDowned: true,
                    breakWhenTargetOutOfRange: true,
                    breakWhenLineOfSightLost: state.requireLineOfSight,
                    damageFactor: 0.5
                })
            ];
        }

        return [
            actionNode('SequenceActionDef', { debugLabel: `${cap(state.label)} direct sequence` }, {
                actions: [
                    effectNode(state, state.castEffectDef, 'CurrentTarget', true),
                    ...payloadNodes
                ]
            })
        ];
    }

    function payloadToActionNode(payload, state) {
        if (payload.type === 'damage') {
            return actionNode('DamageActionDef', { debugLabel: `Apply ${state.label} damage`, amount: payload.amount, damageDef: payload.damageDef });
        }
        if (payload.type === 'heal') {
            return actionNode('HealActionDef', { debugLabel: `Apply ${state.label} healing`, amount: payload.amount });
        }
        if (payload.type === 'status') {
            return actionNode('ApplyStatusEffectActionDef', { debugLabel: `Apply ${state.label} status`, statusEffectDef: payload.statusEffectDef, targetSource: 'CurrentTarget', durationTicks: payload.durationTicks });
        }
        if (payload.type === 'hediff') {
            return actionNode('ApplyHediffActionDef', { debugLabel: `Apply ${state.label} hediff`, hediffDef: payload.hediffDef, severity: payload.severity, removeAfterDuration: Number(payload.durationTicks) > 0, durationTicks: payload.durationTicks });
        }
        if (payload.type === 'knockback') {
            return actionNode('KnockbackActionDef', { debugLabel: `Apply ${state.label} knockback`, distance: payload.distance });
        }
        return actionNode('SummonPawnActionDef', { debugLabel: `Summon ${payload.pawnKindDef}`, pawnKindDef: payload.pawnKindDef, durationTicks: payload.durationTicks });
    }

    function effectNode(state, effectDef, locationSource, attachToTarget) {
        return actionNode('EffectActionDef', {
            debugLabel: `Play ${state.label} effect`,
            effectDef,
            soundDef: state.soundDef,
            locationSource,
            attachToTarget
        });
    }

    function actionNode(type, fieldOverrides = {}, slotOverrides = {}, query = null) {
        const node = createActionNode(type);
        node.fields = { ...node.fields, ...fieldOverrides };
        Object.entries(slotOverrides).forEach(([slotKey, nodes]) => {
            node.slots[slotKey] = nodes;
        });
        if (query) node.query = query;
        return node;
    }

    function createActionNode(type) {
        const def = actionTypes[type] || actionTypes.EffectActionDef;
        const fields = {};
        def.fields.forEach(field => {
            fields[field.key] = field.default != null ? field.default : defaultFieldValue(field);
        });
        const slots = {};
        Object.keys(def.slots || {}).forEach(slotKey => {
            slots[slotKey] = [];
        });
        const node = {
            id: `action-${actionIdSeed++}`,
            type,
            fields,
            slots
        };
        if (def.query) node.query = defaultQuery();
        return node;
    }

    function defaultQuery(overrides = {}) {
        return {
            debugLabel: 'Targets in radius',
            radius: 3,
            centerSource: 'CurrentCell',
            includePawns: true,
            includeBuildings: false,
            includeItems: false,
            includeCaster: false,
            pawnAffinity: 'All',
            ...overrides
        };
    }

    function findAction(nodes, id, parent = null, slotKey = '') {
        for (let index = 0; index < nodes.length; index++) {
            const node = nodes[index];
            if (node.id === id) return { node, parent, slotKey, collection: nodes, index };
            for (const [childSlotKey, childNodes] of Object.entries(node.slots || {})) {
                const found = findAction(childNodes, id, node, childSlotKey);
                if (found) return found;
            }
        }
        return null;
    }

    function firstActionId(nodes) {
        if (!nodes.length) return '';
        return nodes[0].id;
    }

    function walkActions(nodes, visit, ancestors = []) {
        nodes.forEach(node => {
            visit(node, ancestors);
            Object.values(node.slots || {}).forEach(childNodes => walkActions(childNodes, visit, [...ancestors, node]));
        });
    }

    function countSlot(node, slotKey) {
        return node.slots && node.slots[slotKey] ? node.slots[slotKey].length : 0;
    }

    function parseFieldValue(input, field) {
        if (field.type === 'checkbox') return input.checked;
        if (field.type === 'number') return parseNumber(input.value);
        return input.value;
    }

    function defaultFieldValue(field) {
        if (field.type === 'checkbox') return false;
        if (field.type === 'number') return 0;
        return '';
    }

    function textField(key, label, defaultValue = '') {
        return { key, label, type: 'text', default: defaultValue };
    }

    function numberField(key, label, defaultValue = 0, step = 1) {
        return { key, label, type: 'number', default: defaultValue, step };
    }

    function boolField(key, label, defaultValue = false) {
        return { key, label, type: 'checkbox', default: defaultValue };
    }

    function selectField(key, label, options, defaultValue) {
        return { key, label, type: 'select', options, default: defaultValue != null ? defaultValue : options[0] };
    }

    function buildSummary(state) {
        const flow = describeFlow(state);
        return `
            <div class="summary-title">${escapeHtml(state.label || 'Unnamed Spell')}</div>
            <div class="summary-id">${escapeHtml(state.defName || 'No_ID')}</div>
            <div class="summary-desc">${escapeHtml(state.description || 'No description provided.')}</div>
            <div class="summary-section">
                <h3>Pattern</h3>
                <p><strong>${state.pattern.title}</strong> - ${escapeHtml(state.pattern.hint)}</p>
            </div>
            <div class="summary-section">
                <h3>Targeting</h3>
                <p>${escapeHtml(state.targetShape)} ${escapeHtml(state.primaryTargetType)} within <strong>${escapeHtml(state.range)}</strong> cells. Pawn affinity: <strong>${escapeHtml(state.pawnAffinity)}</strong>.</p>
                <p class="muted">${state.requireLineOfSight ? 'Requires line of sight.' : 'Ignores line of sight.'} ${state.useCasterAsTarget ? 'Uses caster as the target.' : (state.allowSelfTarget ? 'Self targeting is allowed.' : 'Self targeting is blocked.')} ${cellGateSummary(state)}</p>
            </div>
            <div class="summary-section">
                <h3>Action Tree</h3>
                <ol class="summary-list">${flow.map(item => `<li>${item}</li>`).join('')}</ol>
            </div>
            <div class="summary-section">
                <h3>Cost</h3>
                <p><strong>${escapeHtml(state.manaCost)}</strong> mana, <strong>${escapeHtml(state.cooldownTicks)}</strong> tick cooldown, <strong>${escapeHtml(state.castTimeTicks)}</strong> tick cast time.</p>
                <p class="muted">${learningSummary(state)} ${state.scaledAttributes.length ? `Scales: ${escapeHtml(state.scaledAttributes.join(', '))}.` : 'No lightweight scaling selected.'}</p>
            </div>
        `;
    }

    function cellGateSummary(state) {
        const gates = [];
        if (state.requireStandableCell) gates.push('standable');
        if (state.requireWalkableCell) gates.push('walkable');
        if (state.requireWaterCell) gates.push('water');
        if (state.requireResurrectableCorpse) gates.push('resurrectable corpse');
        return gates.length ? `Requires ${gates.join(', ')} targeting.` : '';
    }

    function learningSummary(state) {
        if (!state.canBeLearned) return 'Not learnable by normal spell learning.';
        const gates = [];
        if (state.requireArcaneGift) gates.push('Arcane Gift');
        if (Number(state.minimumCasterLevel) > 0) gates.push(`caster level ${state.minimumCasterLevel}`);
        if (state.researchPrerequisite) gates.push(state.researchPrerequisite);
        return gates.length ? `Learning gates: ${escapeHtml(gates.join(', '))}.` : 'No learning gates.';
    }

    function describeFlow(state) {
        if (currentMode === 'advanced') {
            const lines = [];
            walkActions(advancedActions, (node, ancestors) => {
                const def = actionTypes[node.type];
                const indent = ancestors.length ? `${'--'.repeat(ancestors.length)} ` : '';
                lines.push(`${escapeHtml(indent)}<strong>${escapeHtml(def.title)}</strong>: ${escapeHtml((def.summary ? def.summary(node) : '') || node.fields.debugLabel || node.type)}`);
            });
            return lines.length ? lines : ['No advanced actions have been authored.'];
        }

        const payload = describePayloadStack(state);
        if (state.delivery === 'projectile') {
            return [
                `Play cast effect <strong>${escapeHtml(state.castEffectDef)}</strong> at the caster.`,
                `Launch projectile <strong>${escapeHtml(state.projectileDef)}</strong>.`,
                `On impact, play <strong>${escapeHtml(state.impactEffectDef)}</strong> and ${payload}.`
            ];
        }
        if (state.delivery === 'area') {
            const secondaryPayload = describePayloadStack({ ...state, payloads: state.payloads.filter(payload => payload.type !== 'damage') });
            return [
                `Play cast effect <strong>${escapeHtml(state.castEffectDef)}</strong>.`,
                `Create explosion centered on the selected cell with radius <strong>${escapeHtml(state.targetRadius)}</strong>.`,
                `Apply secondary radius payload: ${secondaryPayload}.`
            ];
        }
        if (state.delivery === 'persistent') {
            return [
                `Create marker <strong>${escapeHtml(state.zoneMarkerDef)}</strong> for <strong>${escapeHtml(state.durationTicks)}</strong> ticks.`,
                `Pulse every <strong>${escapeHtml(state.pulseIntervalTicks)}</strong> ticks in radius <strong>${escapeHtml(state.targetRadius)}</strong>.`,
                `Each pulse will ${payload}.`
            ];
        }
        if (state.delivery === 'sustained') {
            const sustainedPayload = describePayloadStack({ ...state, payloads: state.payloads.filter(payload => payload.type === 'status') });
            return [
                `Start a sustained modifier on the current target.`,
                `Maintain for up to <strong>${escapeHtml(state.durationTicks)}</strong> ticks while range and line of sight hold.`,
                `Apply sustained payload: ${sustainedPayload}.`
            ];
        }
        if (state.delivery === 'forcefield') {
            return [
                `Apply a force field to the current target for up to <strong>${escapeHtml(state.durationTicks)}</strong> ticks.`,
                `Break when the caster is downed${state.requireLineOfSight ? ', line of sight is lost,' : ''} or the target leaves <strong>${escapeHtml(state.range)}</strong> cells.`,
                `Use field impact and sustain visuals from the framework defaults.`
            ];
        }
        return [
            `Play effect <strong>${escapeHtml(state.castEffectDef)}</strong>.`,
            `Apply payload directly to the current target: ${payload}.`
        ];
    }

    function describePayloadStack(state) {
        if (!state.payloads.length) return 'run no payload actions';
        return state.payloads
            .map(payload => payloadTypes[payload.type] ? payloadTypes[payload.type].summary(payload) : 'run a payload action')
            .join(', then ');
    }

    function buildXml(state) {
        const description = buildDescription(state);
        return `<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <MagicFramework.Definitions.SpellDef>
    <defName>${xml(state.defName)}</defName>
    <label>${xml(state.label)}</label>
    <description>${xml(description)}</description>
    <range>${xml(state.range)}</range>
    <castTimeTicks>${xml(state.castTimeTicks)}</castTimeTicks>
    <gizmoIconPath>${xml(state.gizmoIconPath)}</gizmoIconPath>

${buildLearningXml(state)}
${buildPowerXml(state)}

    <targeting>
      <shape>${xml(state.targetShape)}</shape>
      <primaryTargetType>${xml(state.primaryTargetType)}</primaryTargetType>
      <pawnAffinity>${xml(state.pawnAffinity)}</pawnAffinity>
      <includePawns>${state.includePawns}</includePawns>
      <includeBuildings>${state.includeBuildings}</includeBuildings>
      <includeItems>${state.includeItems}</includeItems>
      <allowSelfTarget>${state.allowSelfTarget}</allowSelfTarget>
      <useCasterAsTarget>${state.useCasterAsTarget}</useCasterAsTarget>
      <requireLineOfSight>${state.requireLineOfSight}</requireLineOfSight>
      <requireStandableCell>${state.requireStandableCell}</requireStandableCell>
      <requireWalkableCell>${state.requireWalkableCell}</requireWalkableCell>
      <requireWaterCell>${state.requireWaterCell}</requireWaterCell>
      <requireResurrectableCorpse>${state.requireResurrectableCorpse}</requireResurrectableCorpse>
      <range>${xml(state.range)}</range>${targetShapeXml(state)}
    </targeting>

    <requirements>
      <li Class="MagicFramework.Definitions.ManaRequirementDef">
        <debugLabel>Enough mana for ${xml(cap(state.label))}</debugLabel>
        <amount>${xml(state.manaCost)}</amount>
      </li>
      <li Class="MagicFramework.Definitions.CooldownRequirementDef">
        <debugLabel>${xml(cap(state.label))} cooldown ready</debugLabel>
        <cooldownTicks>${xml(state.cooldownTicks)}</cooldownTicks>
      </li>
    </requirements>

    <costs>
      <li Class="MagicFramework.Definitions.ManaCostDef">
        <debugLabel>Spend mana for ${xml(cap(state.label))}</debugLabel>
        <amount>${xml(state.manaCost)}</amount>
      </li>
      <li Class="MagicFramework.Definitions.CooldownCostDef">
        <debugLabel>Start ${xml(cap(state.label))} cooldown</debugLabel>
        <cooldownTicks>${xml(state.cooldownTicks)}</cooldownTicks>
      </li>
    </costs>

    <actions>${buildActionsXml(state, 3)}
    </actions>
  </MagicFramework.Definitions.SpellDef>
</Defs>`;
    }

    function buildDescription(state) {
        const description = state.description || '';
        if (!state.appendSpellSummary || description.includes('{MF:SpellSummary}')) {
            return description;
        }
        return `${description}\n\n{MF:SpellSummary}`;
    }

    function buildLearningXml(state) {
        if (!state.canBeLearned) {
            return `    <learning>
      <canBeLearned>false</canBeLearned>
    </learning>`;
        }

        const requirements = [];
        if (state.requireArcaneGift) {
            requirements.push('      <li Class="MagicFramework.Definitions.ArcaneGiftRequirementDef" />');
        }
        if (Number(state.minimumCasterLevel) > 0) {
            requirements.push(`      <li Class="MagicFramework.Definitions.CasterLevelRequirementDef">
        <minimumLevel>${xml(state.minimumCasterLevel)}</minimumLevel>
      </li>`);
        }

        return `    <learning>
      <canBeLearned>${state.canBeLearned}</canBeLearned>${state.researchPrerequisite ? `
      <researchPrerequisites>
        <li>${xml(state.researchPrerequisite)}</li>
      </researchPrerequisites>` : ''}${requirements.length ? `
      <requirements>
${requirements.join('\n')}
      </requirements>` : ''}
    </learning>`;
    }

    function buildPowerXml(state) {
        if (!state.scaledAttributes.length && Number(state.casterLevelFactor) <= 0) {
            return '';
        }
        return `    <power>
      <casterLevelFactor>${xml(state.casterLevelFactor)}</casterLevelFactor>${state.scaledAttributes.length ? `
      <scaledAttributes>
${state.scaledAttributes.map(attribute => `        <li>${xml(attribute)}</li>`).join('\n')}
      </scaledAttributes>` : ''}
    </power>`;
    }

    function targetShapeXml(state) {
        if (state.targetShape === 'Radius') return `\n      <radius>${xml(state.targetRadius)}</radius>`;
        if (state.targetShape === 'Line') return `\n      <lineLength>${xml(state.lineLength)}</lineLength>`;
        if (state.targetShape === 'Cone') return `\n      <lineLength>${xml(state.lineLength)}</lineLength>\n      <coneAngleDegrees>${xml(state.coneAngleDegrees)}</coneAngleDegrees>`;
        if (state.targetShape === 'Wall') return `\n      <wallLength>${xml(state.wallLength)}</wallLength>`;
        if (state.targetShape === 'Chain') return `\n      <maxChains>${xml(state.maxChains)}</maxChains>`;
        return '';
    }

    function actionNodesXml(nodes, level) {
        return nodes.map(node => actionNodeXml(node, level)).join('\n');
    }

    function actionNodeXml(node, level) {
        const def = actionTypes[node.type];
        const indent = '  '.repeat(level);
        const fieldXml = def.fields
            .filter(field => shouldWriteActionField(node, field))
            .map(field => `${indent}  <${field.key}>${xml(node.fields[field.key])}</${field.key}>`)
            .join('\n');
        const queryXml = def.query ? `\n${targetQueryXml(node.query || defaultQuery(), level + 1)}` : '';
        const slotXml = Object.keys(def.slots || {})
            .map(slotKey => actionSlotXml(node, slotKey, level + 1))
            .filter(Boolean)
            .join('\n');
        const body = [fieldXml, queryXml.trimEnd(), slotXml].filter(Boolean).join('\n');
        return `${indent}<li Class="${xml(def.className)}">${body ? `\n${body}\n${indent}` : ''}</li>`;
    }

    function shouldWriteActionField(node, field) {
        if (field.key === 'debugLabel') return Boolean(node.fields[field.key]);
        if (field.key === 'soundDef' || field.key === 'explosionSoundDef' || field.key === 'explosionEffectDef') return Boolean(node.fields[field.key]);
        if (field.key === 'durationTicks' && node.type === 'ApplyStatusEffectActionDef') return Number(node.fields[field.key]) >= 0;
        if (field.key === 'durationTicks' && node.type === 'ApplyHediffActionDef') return Number(node.fields[field.key]) > 0;
        if (field.key === 'removeAfterDuration' && node.type === 'ApplyHediffActionDef') return Boolean(node.fields[field.key]);
        return node.fields[field.key] !== undefined && node.fields[field.key] !== '';
    }

    function actionSlotXml(node, slotKey, level) {
        const nodes = node.slots && node.slots[slotKey] ? node.slots[slotKey] : [];
        if (!nodes.length) return '';
        const indent = '  '.repeat(level);
        return `${indent}<${slotKey}>\n${actionNodesXml(nodes, level + 1)}\n${indent}</${slotKey}>`;
    }

    function targetQueryXml(query, level) {
        const indent = '  '.repeat(level);
        return `${indent}<targetQuery Class="MagicFramework.Definitions.TargetsInRadiusQueryDef">
${indent}  <debugLabel>${xml(query.debugLabel || 'Targets in radius')}</debugLabel>
${indent}  <radius>${xml(query.radius)}</radius>
${indent}  <centerSource>${xml(query.centerSource)}</centerSource>
${indent}  <includePawns>${Boolean(query.includePawns)}</includePawns>
${indent}  <includeBuildings>${Boolean(query.includeBuildings)}</includeBuildings>
${indent}  <includeItems>${Boolean(query.includeItems)}</includeItems>
${indent}  <includeCaster>${Boolean(query.includeCaster)}</includeCaster>
${indent}  <pawnAffinity>${xml(query.pawnAffinity)}</pawnAffinity>
${indent}</targetQuery>`;
    }

    function buildActionsXml(state, level) {
        if (currentMode === 'advanced') {
            return `\n${actionNodesXml(advancedActions, level)}`;
        }

        const primaryDamage = state.payloads.find(payload => payload.type === 'damage') || payloadTypes.damage.defaults;
        const sustainedStatus = state.payloads.find(payload => payload.type === 'status') || payloadTypes.status.defaults;
        const areaPayloads = state.payloads.filter(payload => payload.type !== 'damage');

        if (state.delivery === 'projectile') {
            return `
      <li Class="MagicFramework.Definitions.SequenceActionDef">
        <debugLabel>${xml(cap(state.label))} sequence</debugLabel>
        <actions>
${effectXml(state, level + 3, state.castEffectDef, 'Caster', false)}
          <li Class="MagicFramework.Definitions.LaunchProjectileActionDef">
            <debugLabel>Launch ${xml(state.label)} projectile</debugLabel>
            <projectileDef>${xml(state.projectileDef)}</projectileDef>
            <onImpactActions>
${effectXml(state, level + 4, state.impactEffectDef, 'CurrentTarget', true)}
${payloadsXml(state.payloads, state, level + 4)}
            </onImpactActions>
          </li>
        </actions>
      </li>`;
        }

        if (state.delivery === 'area') {
            return `
      <li Class="MagicFramework.Definitions.SequenceActionDef">
        <debugLabel>${xml(cap(state.label))} area burst</debugLabel>
        <actions>
${effectXml(state, level + 3, state.castEffectDef, 'Caster', false)}
          <li Class="MagicFramework.Definitions.ExplosionActionDef">
            <debugLabel>${xml(cap(state.label))} explosion</debugLabel>
            <radius>${xml(state.targetRadius)}</radius>
            <damageAmount>${xml(primaryDamage.amount)}</damageAmount>
            <damageDef>${xml(primaryDamage.damageDef)}</damageDef>
          </li>${areaPayloads.length ? `
          <li Class="MagicFramework.Definitions.ApplyToTargetsActionDef">
            <debugLabel>Apply ${xml(state.label)} payload in radius</debugLabel>
            <targetQuery Class="MagicFramework.Definitions.TargetsInRadiusQueryDef">
              <debugLabel>Targets around ${xml(state.label)} impact</debugLabel>
              <radius>${xml(state.targetRadius)}</radius>
              <centerSource>CurrentCell</centerSource>
              <includePawns>${state.includePawns}</includePawns>
              <includeBuildings>${state.includeBuildings}</includeBuildings>
              <includeItems>${state.includeItems}</includeItems>
              <includeCaster>${state.allowSelfTarget}</includeCaster>
            </targetQuery>
            <actions>
${payloadsXml(areaPayloads, state, level + 4)}
            </actions>
          </li>` : ''}
        </actions>
      </li>`;
        }

        if (state.delivery === 'persistent') {
            return `
      <li Class="MagicFramework.Definitions.PersistentAreaZoneActionDef">
        <debugLabel>Create ${xml(state.label)} persistent zone</debugLabel>
        <markerThingDef>${xml(state.zoneMarkerDef)}</markerThingDef>
        <zoneRadius>${xml(state.targetRadius)}</zoneRadius>
        <pulseIntervalTicks>${xml(state.pulseIntervalTicks)}</pulseIntervalTicks>
        <durationTicks>${xml(state.durationTicks)}</durationTicks>
        <pulseAtCenter>false</pulseAtCenter>
        <pawnAffinity>${xml(state.pawnAffinity)}</pawnAffinity>
        <includeCaster>${state.allowSelfTarget}</includeCaster>
        <replaceExistingForCaster>true</replaceExistingForCaster>
        <onPulseActions>
${effectXml(state, level + 3, state.impactEffectDef, 'CurrentCell', false)}
        </onPulseActions>
        <actions>
${payloadsXml(state.payloads, state, level + 3)}
        </actions>
      </li>`;
        }

        if (state.delivery === 'sustained') {
            return `
      <li Class="MagicFramework.Definitions.SustainedStatModifierActionDef">
        <debugLabel>${xml(cap(state.label))} sustained effect</debugLabel>
        <statusEffectDef>${xml(sustainedStatus.statusEffectDef)}</statusEffectDef>
        <targetSource>CurrentTarget</targetSource>
        <maxDurationTicks>${xml(state.durationTicks)}</maxDurationTicks>
        <maxRange>${xml(state.range)}</maxRange>
        <breakWhenCasterDowned>true</breakWhenCasterDowned>
        <breakWhenTargetOutOfRange>true</breakWhenTargetOutOfRange>
        <breakWhenLineOfSightLost>${state.requireLineOfSight}</breakWhenLineOfSightLost>
      </li>`;
        }

        if (state.delivery === 'forcefield') {
            return `
      <li Class="MagicFramework.Definitions.ApplyForceFieldActionDef">
        <debugLabel>${xml(cap(state.label))} force field</debugLabel>
        <targetSource>CurrentTarget</targetSource>
        <maxDurationTicks>${xml(state.durationTicks)}</maxDurationTicks>
        <maxRange>${xml(state.range)}</maxRange>
        <breakWhenCasterDowned>true</breakWhenCasterDowned>
        <breakWhenTargetOutOfRange>true</breakWhenTargetOutOfRange>
        <breakWhenLineOfSightLost>${state.requireLineOfSight}</breakWhenLineOfSightLost>
        <damageFactor>0.5</damageFactor>
      </li>`;
        }

        return `
      <li Class="MagicFramework.Definitions.SequenceActionDef">
        <debugLabel>${xml(cap(state.label))} direct sequence</debugLabel>
        <actions>
${effectXml(state, level + 3, state.castEffectDef, 'CurrentTarget', true)}
${payloadsXml(state.payloads, state, level + 3)}
        </actions>
      </li>`;
    }

    function effectXml(state, level, effectDef, locationSource, attachToTarget) {
        const indent = '  '.repeat(level);
        return `${indent}<li Class="MagicFramework.Definitions.EffectActionDef">
${indent}  <debugLabel>Play ${xml(state.label)} effect</debugLabel>
${indent}  <effectDef>${xml(effectDef)}</effectDef>${state.soundDef ? `\n${indent}  <soundDef>${xml(state.soundDef)}</soundDef>` : ''}
${indent}  <locationSource>${locationSource}</locationSource>
${indent}  <attachToTarget>${attachToTarget}</attachToTarget>
${indent}</li>`;
    }

    function payloadsXml(payloads, state, level) {
        return payloads.map(payload => payloadXml(payload, state, level)).join('\n');
    }

    function payloadXml(payload, state, level) {
        const indent = '  '.repeat(level);
        if (payload.type === 'damage') {
            return `${indent}<li Class="MagicFramework.Definitions.DamageActionDef">
${indent}  <debugLabel>Apply ${xml(state.label)} damage</debugLabel>
${indent}  <amount>${xml(payload.amount)}</amount>
${indent}  <damageDef>${xml(payload.damageDef)}</damageDef>
${indent}</li>`;
        }
        if (payload.type === 'heal') {
            return `${indent}<li Class="MagicFramework.Definitions.HealActionDef">
${indent}  <debugLabel>Apply ${xml(state.label)} healing</debugLabel>
${indent}  <amount>${xml(payload.amount)}</amount>
${indent}</li>`;
        }
        if (payload.type === 'status') {
            return `${indent}<li Class="MagicFramework.Definitions.ApplyStatusEffectActionDef">
${indent}  <debugLabel>Apply ${xml(state.label)} status</debugLabel>
${indent}  <statusEffectDef>${xml(payload.statusEffectDef)}</statusEffectDef>
${indent}  <targetSource>CurrentTarget</targetSource>
${Number(payload.durationTicks) >= 0 ? `${indent}  <durationTicks>${xml(payload.durationTicks)}</durationTicks>\n` : ''}${indent}</li>`;
        }
        if (payload.type === 'hediff') {
            return `${indent}<li Class="MagicFramework.Definitions.ApplyHediffActionDef">
${indent}  <debugLabel>Apply ${xml(state.label)} hediff</debugLabel>
${indent}  <hediffDef>${xml(payload.hediffDef)}</hediffDef>
${indent}  <severity>${xml(payload.severity)}</severity>
${Number(payload.durationTicks) > 0 ? `${indent}  <removeAfterDuration>true</removeAfterDuration>\n${indent}  <durationTicks>${xml(payload.durationTicks)}</durationTicks>\n` : ''}${indent}</li>`;
        }
        if (payload.type === 'knockback') {
            return `${indent}<li Class="MagicFramework.Definitions.KnockbackActionDef">
${indent}  <debugLabel>Apply ${xml(state.label)} knockback</debugLabel>
${indent}  <distance>${xml(payload.distance)}</distance>
${indent}</li>`;
        }
        return `${indent}<li Class="MagicFramework.Definitions.SummonPawnActionDef">
${indent}  <debugLabel>Summon ${xml(payload.pawnKindDef)}</debugLabel>
${indent}  <pawnKindDef>${xml(payload.pawnKindDef)}</pawnKindDef>
${indent}  <durationTicks>${xml(payload.durationTicks)}</durationTicks>
${indent}</li>`;
    }

    function toggleXml() {
        xmlVisible = !xmlVisible;
        xmlContainer.hidden = !xmlVisible;
        humanSummary.hidden = xmlVisible;
        viewXmlBtn.textContent = xmlVisible ? 'View Spellbook' : 'View XML';
    }

    function copyXml() {
        navigator.clipboard.writeText(xmlPreview.textContent).then(() => {
            const oldText = copyXmlBtn.textContent;
            copyXmlBtn.textContent = 'Copied';
            setTimeout(() => copyXmlBtn.textContent = oldText, 1500);
        });
    }

    function setVal(id, value, dispatch = true) {
        const el = document.getElementById(id);
        if (!el) return;
        if (el.type === 'checkbox') {
            el.checked = Boolean(value);
        } else {
            el.value = value;
        }
        if (dispatch) el.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function getVal(id) {
        const el = document.getElementById(id);
        if (!el) return '';
        return el.type === 'checkbox' ? el.checked : el.value;
    }

    function parseNumber(value) {
        const parsed = Number(value);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    function toPascal(value) {
        return String(value || '')
            .split(/[\s_\-]+/)
            .filter(Boolean)
            .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
            .join('')
            .replace(/[^a-zA-Z0-9]/g, '');
    }

    function cap(value) {
        const text = String(value || '');
        return text.charAt(0).toUpperCase() + text.slice(1);
    }

    function xml(value) {
        return String(value == null ? '' : value).replace(/[<>&'"]/g, c => ({
            '<': '&lt;',
            '>': '&gt;',
            '&': '&amp;',
            '\'': '&apos;',
            '"': '&quot;'
        }[c]));
    }

    function escapeHtml(value) {
        return xml(value);
    }

    function highlightXml(source) {
        const escaped = xml(source);
        const defPattern = /^(MF_|MFV_|Example_|Bullet_|Mote_|Psycast|Explosion_|Flame$|Burn$|Blunt$|Husky$|Spark|YellowSpark|ElectricalSpark|GiantExplosion|EnergyShield_)/;

        function span(className, text) {
            return className ? `<span class="${className}">${text}</span>` : text;
        }

        function classifyText(text) {
            if (/^(true|false)$/i.test(text)) return 'xml-boolean';
            if (/^-?\d+(\.\d+)?$/.test(text)) return 'xml-number';
            if (defPattern.test(text)) return 'xml-def';
            return '';
        }

        return escaped
            .replace(/(&lt;!--[\s\S]*?--&gt;)/g, '<span class="xml-comment">$1</span>')
            .replace(/(&lt;\/?)([\w.:-]+)((?:\s+[\w.:-]+=&quot;[^&]*&quot;)*\s*)(\/?&gt;)/g,
                (_match, open, tag, attrs, close) => {
                    const highlightedAttrs = attrs.replace(/([\w.:-]+)=&quot;([^&]*)&quot;/g,
                        (_attrMatch, name, value) => {
                            const valueClass = value.startsWith('MagicFramework.') ? 'xml-mf-class' : classifyText(value) || 'xml-string';
                            return `${span('xml-attribute', name)}<span class="xml-punctuation">=</span>&quot;${span(valueClass, value)}&quot;`;
                        });

                    return `${span('xml-punctuation', open)}${span('xml-tag', tag)}${highlightedAttrs}${span('xml-punctuation', close)}`;
                })
            .replace(/(&gt;)([^<&]+)(&lt;)/g, (_match, before, text, after) => {
                const trimmed = text.trim();
                if (!trimmed) return `${before}${text}${after}`;

                const leading = text.match(/^\s*/)[0];
                const trailing = text.match(/\s*$/)[0];
                return `${before}${leading}${span(classifyText(trimmed), trimmed)}${trailing}${after}`;
            });
    }

    init();
});
