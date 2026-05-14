document.addEventListener('DOMContentLoaded', () => {
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

    const payloadTypes = {
        damage: {
            title: 'Damage',
            summary: payload => `deal <strong>${escapeHtml(payload.amount)} ${escapeHtml(payload.damageDef)}</strong> damage`,
            fields: [
                { key: 'amount', label: 'Damage Amount', type: 'number', step: '0.1' },
                { key: 'damageDef', label: 'Damage Def', type: 'text' }
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
                { key: 'statusEffectDef', label: 'Status Effect Def', type: 'text' },
                { key: 'durationTicks', label: 'Override Duration', type: 'number', step: '1' }
            ],
            defaults: { statusEffectDef: 'MFV_Status_Haste', durationTicks: -1 }
        },
        hediff: {
            title: 'Hediff',
            summary: payload => `apply hediff <strong>${escapeHtml(payload.hediffDef)}</strong>`,
            fields: [
                { key: 'hediffDef', label: 'Hediff Def', type: 'text' },
                { key: 'severity', label: 'Severity', type: 'number', step: '0.01' },
                { key: 'durationTicks', label: 'Duration', type: 'number', step: '1' }
            ],
            defaults: { hediffDef: 'MF_Burning', severity: 0.2, durationTicks: 0 }
        },
        summon: {
            title: 'Summon',
            summary: payload => `summon <strong>${escapeHtml(payload.pawnKindDef)}</strong> for <strong>${escapeHtml(payload.durationTicks)}</strong> ticks`,
            fields: [
                { key: 'pawnKindDef', label: 'PawnKind Def', type: 'text' },
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
                payloads: [
                    { type: 'damage', amount: 10, damageDef: 'Flame' },
                    { type: 'hediff', hediffDef: 'MF_Burning', severity: 0.2, durationTicks: 300 }
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
                payloads: [
                    { type: 'status', statusEffectDef: 'MFV_Status_Might', durationTicks: -1 }
                ],
                durationTicks: 1800,
                manaCost: 18,
                cooldownTicks: 360,
                range: 12,
                castTimeTicks: 30
            }
        }
    };

    let selectedPattern = 'projectileDamage';
    let xmlVisible = false;
    let currentPayloads = [];
    let payloadIdSeed = 1;

    function init() {
        renderPatterns();
        applyPattern(selectedPattern);
        form.addEventListener('input', updatePreview);
        form.addEventListener('change', updatePreview);
        document.getElementById('label').addEventListener('input', syncGeneratedDefs);
        payloadToolbar.addEventListener('click', addPayloadFromButton);
        payloadStack.addEventListener('input', handlePayloadInput);
        payloadStack.addEventListener('click', handlePayloadClick);
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
        Object.entries(pattern.values).forEach(([id, value]) => {
            if (id !== 'payloads') setVal(id, value);
        });
        currentPayloads = clonePayloads(pattern.values.payloads || [{ type: 'damage', amount: 10, damageDef: 'Blunt' }]);
        renderPayloadStack();
        syncGeneratedDefs();
        patternGrid.querySelectorAll('.pattern-card').forEach(button => {
            button.classList.toggle('active', button.dataset.pattern === patternKey);
        });
        updatePreview();
    }

    function clonePayloads(payloads) {
        return payloads.map(payload => ({
            ...payloadTypes[payload.type]?.defaults,
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

        payload[input.dataset.payloadKey] = input.type === 'number' ? parseNumber(input.value) : input.value;
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
            const fields = def.fields.map(field => `
                <label>
                    <span>${field.label}</span>
                    <input
                        type="${field.type}"
                        value="${escapeHtml(payload[field.key] ?? '')}"
                        step="${field.step || ''}"
                        data-payload-id="${payload.id}"
                        data-payload-key="${field.key}">
                </label>
            `).join('');

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
            projectileDef: getVal('projectileDef'),
            includePawns: getVal('includePawns'),
            includeBuildings: getVal('includeBuildings'),
            includeItems: getVal('includeItems'),
            allowSelfTarget: getVal('allowSelfTarget'),
            requireLineOfSight: getVal('requireLineOfSight'),
            durationTicks: getVal('durationTicks'),
            zoneMarkerDef: getVal('zoneMarkerDef'),
            pulseIntervalTicks: getVal('pulseIntervalTicks'),
            castEffectDef: getVal('castEffectDef'),
            impactEffectDef: getVal('impactEffectDef'),
            soundDef: getVal('soundDef'),
            manaCost: getVal('manaCost'),
            cooldownTicks: getVal('cooldownTicks'),
            payloads: currentPayloads.map(({ id, ...payload }) => ({ ...payload }))
        };
    }

    function updatePreview() {
        const state = getState();
        updateFieldVisibility(state);
        patternHint.textContent = state.pattern.hint;
        humanSummary.innerHTML = buildSummary(state);
        xmlPreview.innerHTML = highlightXml(buildXml(state));
    }

    function updateFieldVisibility(state) {
        const showProjectile = state.delivery === 'projectile';
        const showRadius = state.targetShape === 'Radius' || state.delivery === 'area' || state.delivery === 'persistent';
        const showZone = state.delivery === 'persistent';
        const showImpactFx = ['projectile', 'area', 'persistent'].includes(state.delivery);

        setGroupAvailable('targetRadius', showRadius, 'Radius is only used by radius, burst, and persistent-zone targeting.');
        setGroupAvailable('projectileDef', showProjectile, 'Projectile defs are only used when delivery is Projectile.');
        setGroupAvailable('durationTicks', showZone || state.delivery === 'sustained', 'Effect Duration is used by persistent zones and sustained links. Payload-specific durations live inside payload cards.');
        setGroupAvailable('zoneMarkerDef', showZone, 'Zone Marker Def is only used by Persistent Zone delivery.');
        setGroupAvailable('pulseIntervalTicks', showZone, 'Pulse Interval is only used by Persistent Zone delivery.');
        setGroupAvailable('impactEffectDef', showImpactFx, 'Impact effects are used by projectile, area burst, and persistent zone patterns.');
        updatePayloadButtons(state);
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
        const group = el?.closest('.form-group');
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
                <p class="muted">${state.requireLineOfSight ? 'Requires line of sight.' : 'Ignores line of sight.'} ${state.allowSelfTarget ? 'Self targeting is allowed.' : 'Self targeting is blocked.'}</p>
            </div>
            <div class="summary-section">
                <h3>Action Tree</h3>
                <ol class="summary-list">${flow.map(item => `<li>${item}</li>`).join('')}</ol>
            </div>
            <div class="summary-section">
                <h3>Cost</h3>
                <p><strong>${escapeHtml(state.manaCost)}</strong> mana, <strong>${escapeHtml(state.cooldownTicks)}</strong> tick cooldown, <strong>${escapeHtml(state.castTimeTicks)}</strong> tick cast time.</p>
            </div>
        `;
    }

    function describeFlow(state) {
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
        return [
            `Play effect <strong>${escapeHtml(state.castEffectDef)}</strong>.`,
            `Apply payload directly to the current target: ${payload}.`
        ];
    }

    function describePayloadStack(state) {
        if (!state.payloads.length) return 'run no payload actions';
        return state.payloads
            .map(payload => payloadTypes[payload.type]?.summary(payload) || 'run a payload action')
            .join(', then ');
    }

    function buildXml(state) {
        return `<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <MagicFramework.Definitions.SpellDef>
    <defName>${xml(state.defName)}</defName>
    <label>${xml(state.label)}</label>
    <description>${xml(state.description)}</description>
    <range>${xml(state.range)}</range>
    <castTimeTicks>${xml(state.castTimeTicks)}</castTimeTicks>
    <gizmoIconPath>${xml(state.gizmoIconPath)}</gizmoIconPath>

    <targeting>
      <shape>${xml(state.targetShape)}</shape>
      <primaryTargetType>${xml(state.primaryTargetType)}</primaryTargetType>
      <pawnAffinity>${xml(state.pawnAffinity)}</pawnAffinity>
      <includePawns>${state.includePawns}</includePawns>
      <includeBuildings>${state.includeBuildings}</includeBuildings>
      <includeItems>${state.includeItems}</includeItems>
      <allowSelfTarget>${state.allowSelfTarget}</allowSelfTarget>
      <requireLineOfSight>${state.requireLineOfSight}</requireLineOfSight>
      <range>${xml(state.range)}</range>${state.targetShape === 'Radius' ? `\n      <radius>${xml(state.targetRadius)}</radius>` : ''}
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

    function buildActionsXml(state, level) {
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
        return String(value ?? '').replace(/[<>&'"]/g, c => ({
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
