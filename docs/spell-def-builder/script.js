document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('spell-form');
    const xmlContainer = document.getElementById('xmlContainer');
    const xmlPreview = document.getElementById('xmlPreview');
    const humanSummary = document.getElementById('humanSummary');
    const viewXmlBtn = document.getElementById('viewXmlBtn');
    const copyXmlBtn = document.getElementById('copyXmlBtn');
    
    const targetShape = document.getElementById('targetShape');
    const radiusGroup = document.getElementById('radiusGroup');
    const actionsContainer = document.getElementById('actions-container');

    // Settings Modal Elements
    const settingsBtn = document.getElementById('settingsBtn');
    const settingsModal = document.getElementById('settingsModal');
    const closeSettingsBtn = document.getElementById('closeSettingsBtn');
    const saveSettingsBtn = document.getElementById('saveSettingsBtn');
    const aiProviderSelect = document.getElementById('aiProvider');
    const aiApiKeyInput = document.getElementById('aiApiKey');

    // AI Elements
    const aiGenerateBtn = document.getElementById('aiGenerateBtn');
    const aiPromptInput = document.getElementById('aiPrompt');
    const aiLoading = document.getElementById('aiLoading');

    // Load Settings
    aiProviderSelect.value = localStorage.getItem('aiProvider') || 'gemini';
    aiApiKeyInput.value = localStorage.getItem('aiApiKey') || '';

    // Settings Modal Logic
    settingsBtn.addEventListener('click', () => settingsModal.style.display = 'flex');
    closeSettingsBtn.addEventListener('click', () => settingsModal.style.display = 'none');
    saveSettingsBtn.addEventListener('click', () => {
        localStorage.setItem('aiProvider', aiProviderSelect.value);
        localStorage.setItem('aiApiKey', aiApiKeyInput.value);
        settingsModal.style.display = 'none';
    });

    // Wizard Navigation Logic
    let currentStep = 1;
    const totalSteps = 4;

    function showStep(stepIndex) {
        document.querySelectorAll('.wizard-step').forEach(step => step.classList.remove('active'));
        document.querySelectorAll('.wizard-header .step').forEach(header => header.classList.remove('active'));
        
        document.getElementById(`step-${stepIndex}`).classList.add('active');
        document.querySelector(`.wizard-header .step[data-step="${stepIndex}"]`).classList.add('active');
    }

    document.querySelectorAll('.next-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            if (currentStep < totalSteps) {
                currentStep++;
                showStep(currentStep);
            }
        });
    });

    document.querySelectorAll('.prev-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            if (currentStep > 1) {
                currentStep--;
                showStep(currentStep);
            }
        });
    });

    document.getElementById('finalizeBtn').addEventListener('click', () => {
        alert('Spell complete! Check the Spellbook Entry and export your XML.');
    });

    // Show/hide radius based on shape
    targetShape.addEventListener('change', (e) => {
        if (e.target.value === 'Radius' || e.target.value === 'Explosion') {
            radiusGroup.style.display = 'block';
        } else {
            radiusGroup.style.display = 'none';
        }
        updatePreviews();
    });

    const labelInput = document.getElementById('label');
    const defNameInput = document.getElementById('defName');
    const gizmoInput = document.getElementById('gizmoIconPath');

    labelInput.addEventListener('input', () => {
        const rawLabel = labelInput.value;
        if (!rawLabel) return;
        
        const safeName = rawLabel.split(/[\s_\-]+/)
            .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
            .join('').replace(/[^a-zA-Z0-9]/g, '');

        if (safeName) {
            setVal('defName', 'MF_' + safeName);
            setVal('gizmoIconPath', 'UI/Gizmos/Spells/MF_' + safeName);
        }
    });

    form.addEventListener('input', updatePreviews);
    updatePreviews();

    let isXmlVisible = false;
    viewXmlBtn.addEventListener('click', () => {
        isXmlVisible = !isXmlVisible;
        if (isXmlVisible) {
            xmlContainer.style.display = 'block';
            humanSummary.style.display = 'none';
            viewXmlBtn.innerText = 'View Spellbook';
        } else {
            xmlContainer.style.display = 'none';
            humanSummary.style.display = 'block';
            viewXmlBtn.innerText = 'View XML';
        }
    });

    copyXmlBtn.addEventListener('click', () => {
        navigator.clipboard.writeText(xmlPreview.innerText).then(() => {
            const originalText = copyXmlBtn.innerText;
            copyXmlBtn.innerText = 'Copied!';
            setTimeout(() => {
                copyXmlBtn.innerText = originalText;
            }, 2000);
        });
    });

    const persistentZoneChildDefaults = {
        EffectActionDef: 'onPulseActions',
        DamageActionDef: 'actions',
        KnockbackActionDef: 'actions',
        HealActionDef: 'actions'
    };

    const knownDamageAliases = {
        Wind: 'Blunt',
        Air: 'Blunt',
        Arcane: 'Bomb',
        Lightning: 'EMP'
    };

    function normalizeDamageDef(damageDef) {
        if (!damageDef) return 'Blunt';
        return knownDamageAliases[damageDef] || damageDef;
    }

    function getActionPhaseControl(type, phase) {
        if (!phase) return '';

        return `
            <div class="form-group">
                <label>Persistent Zone Role</label>
                <select class="action-input zoneChildPhase">
                    <option value="actions" ${phase === 'actions' ? 'selected' : ''}>Per pawn/cell pulse</option>
                    <option value="onPulseActions" ${phase === 'onPulseActions' ? 'selected' : ''}>Center lifecycle pulse</option>
                </select>
            </div>
        `;
    }

    window.addAction = function(type, initialData = null, parentId = null) {
        const actionId = Date.now() + Math.floor(Math.random() * 1000);
        const actionDiv = document.createElement('div');
        actionDiv.className = 'action-item';
        actionDiv.id = `action-${actionId}`;
        actionDiv.dataset.type = type;
        if (parentId) {
            const phase = initialData?.zoneChildPhase || persistentZoneChildDefaults[type] || 'actions';
            actionDiv.dataset.zoneChild = 'true';
            actionDiv.dataset.zoneChildPhase = phase;
        }

        let content = `<div class="action-header">
            <h4>${type.replace('ActionDef', '')}</h4>
            <button type="button" class="btn btn-danger" onclick="removeAction(${actionId})">Remove</button>
        </div>`;

        content += getActionPhaseControl(type, actionDiv.dataset.zoneChildPhase);

        if (type === 'EffectActionDef') {
            const effectDef = initialData?.effectDef || 'PsycastAreaEffect';
            const soundDef = initialData?.soundDef || '';
            const loc = initialData?.locationSource || (parentId ? 'CurrentCell' : 'Caster');
            const attach = initialData?.attachToTarget ? 'checked' : '';

            content += `
                <div class="form-row">
                    <div class="form-group">
                        <label>Effect Def</label>
                        <input type="text" class="action-input effectDef" value="${effectDef}">
                    </div>
                    <div class="form-group">
                        <label>Sound Def</label>
                        <input type="text" class="action-input soundDef" value="${soundDef}">
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label>Location Source</label>
                        <select class="action-input locationSource">
                            <option value="Caster" ${loc==='Caster'?'selected':''}>Caster</option>
                            <option value="CurrentTarget" ${loc==='CurrentTarget'?'selected':''}>Current Target</option>
                            <option value="CurrentCell" ${loc==='CurrentCell'?'selected':''}>Current Cell</option>
                        </select>
                    </div>
                    <div class="checkbox-group" style="margin-bottom: 0; align-items: flex-end; padding-bottom: 0.5rem;">
                        <label><input type="checkbox" class="action-input attachToTarget" ${attach}> Attach to Target</label>
                    </div>
                </div>
            `;
        } else if (type === 'LaunchProjectileActionDef') {
            const projDef = initialData?.projectileDef || 'MF_NewProjectile';
            content += `
                <div class="form-group">
                    <label>Projectile Def</label>
                    <input type="text" class="action-input projectileDef" value="${projDef}">
                </div>
            `;
        } else if (type === 'DamageActionDef') {
            const amt = initialData?.amount || 10;
            const dmgDef = normalizeDamageDef(initialData?.damageDef || 'Blunt');
            content += `
                <div class="form-row">
                    <div class="form-group">
                        <label>Amount</label>
                        <input type="number" class="action-input amount" value="${amt}">
                    </div>
                    <div class="form-group">
                        <label>Damage Type</label>
                        <input type="text" class="action-input damageDef" value="${dmgDef}">
                    </div>
                </div>
            `;
        } else if (type === 'KnockbackActionDef') {
            const distance = initialData?.distance || 3;
            content += `
                <div class="form-group">
                    <label>Knockback Distance (Cells)</label>
                    <input type="number" class="action-input distance" value="${distance}">
                </div>
            `;
        } else if (type === 'HealActionDef') {
            const amount = initialData?.amount || 15;
            content += `
                <div class="form-group">
                    <label>Heal Amount</label>
                    <input type="number" class="action-input amount" value="${amount}" step="0.1">
                </div>
            `;
        } else if (type === 'PersistentAreaZoneActionDef') {
            const duration = initialData?.durationTicks || 600;
            const radius = initialData?.zoneRadius || 3.0;
            const pulse = initialData?.pulseIntervalTicks || 60;
            const marker = initialData?.markerThingDef || 'MF_FlameFieldMarker';
            const affinity = initialData?.pawnAffinity || 'All';
            const includeCaster = initialData?.includeCaster ? 'checked' : '';
            const replaceExisting = initialData?.replaceExistingForCaster === false ? '' : 'checked';
            const pulseAtCenter = initialData?.pulseAtCenter ? 'checked' : '';
            const ambientEffectDef = initialData?.ambientEffectDef || '';
            content += `
                <div class="form-row">
                    <div class="form-group">
                        <label>Marker ThingDef</label>
                        <input type="text" class="action-input markerThingDef" value="${marker}">
                    </div>
                    <div class="form-group">
                        <label>Duration (Ticks)</label>
                        <input type="number" class="action-input durationTicks" value="${duration}">
                    </div>
                    <div class="form-group">
                        <label>Zone Radius</label>
                        <input type="number" class="action-input zoneRadius" value="${radius}" step="0.1">
                    </div>
                    <div class="form-group">
                        <label>Pulse Interval (Ticks)</label>
                        <input type="number" class="action-input pulseIntervalTicks" value="${pulse}">
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label>Zone Pawn Affinity</label>
                        <select class="action-input zonePawnAffinity">
                            <option value="All" ${affinity==='All'?'selected':''}>All</option>
                            <option value="Ally" ${affinity==='Ally'?'selected':''}>Ally</option>
                            <option value="Foe" ${affinity==='Foe'?'selected':''}>Foe</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>Ambient Effect Def</label>
                        <input type="text" class="action-input ambientEffectDef" value="${ambientEffectDef}" placeholder="Optional, e.g. Vaporize_Heatwave">
                    </div>
                </div>
                <div class="checkbox-group">
                    <label><input type="checkbox" class="action-input includeCaster" ${includeCaster}> Include caster in zone pulses</label>
                    <label><input type="checkbox" class="action-input replaceExistingForCaster" ${replaceExisting}> Replace existing zone for caster</label>
                    <label><input type="checkbox" class="action-input pulseAtCenter" ${pulseAtCenter}> Also run per-pulse actions at center cell</label>
                </div>
                <p class="step-desc" style="margin-bottom: 0.75rem;">Use per pawn/cell pulse for damage, healing, and knockback. Use center lifecycle pulse for zone visuals and sounds.</p>
                <div class="child-actions-container"></div>
                <div class="child-action-buttons">
                    <button type="button" class="btn btn-secondary" onclick="addAction('EffectActionDef', null, ${actionId})">+ Effect</button>
                    <button type="button" class="btn btn-secondary" onclick="addAction('DamageActionDef', null, ${actionId})">+ Damage</button>
                    <button type="button" class="btn btn-secondary" onclick="addAction('KnockbackActionDef', null, ${actionId})">+ Knockback</button>
                    <button type="button" class="btn btn-secondary" onclick="addAction('HealActionDef', null, ${actionId})">+ Heal</button>
                </div>
            `;
        }

        actionDiv.innerHTML = content;
        
        if (parentId) {
            const parentContainer = document.querySelector(`#action-${parentId} > .child-actions-container`);
            if (parentContainer) parentContainer.appendChild(actionDiv);
        } else {
            actionsContainer.appendChild(actionDiv);
        }

        const inputs = actionDiv.querySelectorAll('.action-input');
        inputs.forEach(input => {
            input.addEventListener('input', updatePreviews);
            input.addEventListener('change', () => {
                if (input.classList.contains('zoneChildPhase')) {
                    actionDiv.dataset.zoneChildPhase = input.value;
                }

                updatePreviews();
            });
        });

        updatePreviews();
        return actionId;
    };

    window.removeAction = function(id) {
        const actionDiv = document.getElementById(`action-${id}`);
        if (actionDiv) {
            actionDiv.remove();
            updatePreviews();
        }
    };

    function escapeXML(unsafe) {
        if (!unsafe) return '';
        return unsafe.toString().replace(/[<>&'"]/g, function (c) {
            switch (c) {
                case '<': return '&lt;';
                case '>': return '&gt;';
                case '&': return '&amp;';
                case '\'': return '&apos;';
                case '"': return '&quot;';
            }
        });
    }

    function getVal(id) {
        const el = document.getElementById(id);
        if (!el) return '';
        if (el.type === 'checkbox') return el.checked;
        return el.value;
    }

    function setVal(id, val) {
        const el = document.getElementById(id);
        if (!el) return;
        if (el.type === 'checkbox') {
            el.checked = !!val;
        } else {
            el.value = val;
        }
        // Dispatch event so change listeners catch it if necessary
        el.dispatchEvent(new Event('change'));
    }

    function updatePreviews() {
        updateHumanSummary();
        updateXML();
    }

    function buildSummaryHtml(container) {
        const actions = Array.from(container.children).filter(c => c.classList.contains('action-item'));
        if (actions.length === 0) return '';

        let html = '';
        actions.forEach(actionDiv => {
            const type = actionDiv.dataset.type;
            let itemHtml = '';
            if (type === 'EffectActionDef') {
                const fx = actionDiv.querySelector('.effectDef').value;
                const loc = actionDiv.querySelector('.locationSource').value;
                itemHtml = `Plays visual effect <strong>${fx || 'None'}</strong> at the <strong>${loc.toLowerCase()}</strong>.`;
            } else if (type === 'LaunchProjectileActionDef') {
                const proj = actionDiv.querySelector('.projectileDef').value;
                itemHtml = `Launches a <strong>${proj || 'Projectile'}</strong> towards the target.`;
            } else if (type === 'DamageActionDef') {
                const amt = actionDiv.querySelector('.amount').value;
                const dmg = actionDiv.querySelector('.damageDef').value;
                itemHtml = `Deals <strong>${amt} ${dmg}</strong> damage.`;
            } else if (type === 'KnockbackActionDef') {
                const dist = actionDiv.querySelector('.distance').value;
                itemHtml = `Pushes the target back by <strong>${dist} cells</strong>.`;
            } else if (type === 'HealActionDef') {
                const amt = actionDiv.querySelector('.amount').value;
                itemHtml = `Heals the target for <strong>${amt}</strong> points.`;
            } else if (type === 'PersistentAreaZoneActionDef') {
                const dur = actionDiv.querySelector('.durationTicks').value;
                const rad = actionDiv.querySelector('.zoneRadius').value;
                const pulse = actionDiv.querySelector('.pulseIntervalTicks').value;
                const marker = actionDiv.querySelector('.markerThingDef').value;
                itemHtml = `Creates a persistent zone of radius <strong>${rad}</strong> anchored by <strong>${marker}</strong> that lasts <strong>${dur} ticks</strong> and pulses every <strong>${pulse} ticks</strong>.`;
            }

            const childContainer = Array.from(actionDiv.children).find(c => c.classList.contains('child-actions-container'));
            if (childContainer && Array.from(childContainer.children).filter(c => c.classList.contains('action-item')).length > 0) {
                const childHtml = buildSummaryHtml(childContainer);
                html += `<li>${itemHtml}<ul class="summary-list" style="margin-top: 0.5rem;">${childHtml}</ul></li>`;
            } else {
                html += `<li>${itemHtml}</li>`;
            }
        });
        return html;
    }

    function updateHumanSummary() {
        const label = getVal('label') || 'Unnamed Spell';
        const defName = getVal('defName') || 'No_ID';
        const description = getVal('description') || 'No description provided.';
        const castTimeTicks = getVal('castTimeTicks');
        const manaCost = getVal('manaCost');
        const cooldownTicks = getVal('cooldownTicks');
        
        const shape = getVal('targetShape');
        const primaryTargetType = getVal('primaryTargetType');
        const pawnAffinity = getVal('pawnAffinity');
        const range = getVal('range');
        const radius = getVal('targetRadius');
        const reqLos = getVal('requireLineOfSight') ? "Requires line of sight." : "Ignores line of sight.";
        
        let targetText = `Targets a <strong>${shape.toLowerCase()} ${primaryTargetType.toLowerCase()}</strong> up to <strong>${range} cells</strong> away.`;
        if (shape === 'Radius') targetText += ` Hits everything within a <strong>${radius} cell radius</strong>.`;
        if (primaryTargetType.includes('Pawn')) targetText += ` Affects <strong>${pawnAffinity.toLowerCase()}</strong> pawns.`;
        
        let html = `
            <div class="summary-title">${label.toUpperCase()}</div>
            <div class="summary-id">${defName}</div>
            <div class="summary-desc">"${description}"</div>
            
            <div class="summary-section">
                <h3>Casting & Costs</h3>
                <p>Takes <strong>${castTimeTicks} ticks</strong> to cast. Costs <strong>${manaCost} mana</strong> and has a cooldown of <strong>${cooldownTicks} ticks</strong>.</p>
            </div>
            
            <div class="summary-section">
                <h3>Targeting</h3>
                <p>${targetText}</p>
                <p style="font-size: 0.9em; color: var(--text-muted); margin-top: 0.5rem;">${reqLos}</p>
            </div>
            
            <div class="summary-section">
                <h3>Execution Sequence</h3>
                <ol class="summary-list">
        `;

        const topActions = Array.from(actionsContainer.children).filter(c => c.classList.contains('action-item'));
        if (topActions.length === 0) {
            html += `<li><em>No effects defined yet.</em></li>`;
        } else {
            html += buildSummaryHtml(actionsContainer);
        }

        html += `
                </ol>
            </div>
        `;

        humanSummary.innerHTML = html;
    }

    function buildXML(container, indentLevel, label, phaseFilter = null) {
        const actions = Array.from(container.children).filter(c => c.classList.contains('action-item'));
        if (actions.length === 0) return '';
        
        let xml = '';
        const indent = '  '.repeat(indentLevel);
        actions.forEach(actionDiv => {
            if (phaseFilter && actionDiv.dataset.zoneChildPhase !== phaseFilter) return;

            const type = actionDiv.dataset.type;
            xml += `\n${indent}<li Class="MagicFramework.Definitions.${type}">`;
            
            if (type === 'EffectActionDef') {
                const effectDef = actionDiv.querySelector('.effectDef').value;
                const soundDef = actionDiv.querySelector('.soundDef').value;
                const locationSource = actionDiv.querySelector('.locationSource').value;
                const attachToTarget = actionDiv.querySelector('.attachToTarget').checked;
                xml += `\n${indent}  <debugLabel>Play ${label} effect</debugLabel>`;
                if (effectDef) xml += `\n${indent}  <effectDef>${escapeXML(effectDef)}</effectDef>`;
                if (soundDef) xml += `\n${indent}  <soundDef>${escapeXML(soundDef)}</soundDef>`;
                xml += `\n${indent}  <locationSource>${locationSource}</locationSource>`;
                xml += `\n${indent}  <attachToTarget>${attachToTarget}</attachToTarget>`;
            } else if (type === 'LaunchProjectileActionDef') {
                const projectileDef = actionDiv.querySelector('.projectileDef').value;
                xml += `\n${indent}  <debugLabel>Launch ${label} projectile</debugLabel>`;
                if (projectileDef) xml += `\n${indent}  <projectileDef>${escapeXML(projectileDef)}</projectileDef>`;
            } else if (type === 'DamageActionDef') {
                const amount = actionDiv.querySelector('.amount').value;
                const damageDef = normalizeDamageDef(actionDiv.querySelector('.damageDef').value);
                xml += `\n${indent}  <debugLabel>Apply ${label} damage</debugLabel>`;
                xml += `\n${indent}  <amount>${amount}</amount>`;
                if (damageDef) xml += `\n${indent}  <damageDef>${escapeXML(damageDef)}</damageDef>`;
            } else if (type === 'KnockbackActionDef') {
                const dist = actionDiv.querySelector('.distance').value;
                xml += `\n${indent}  <debugLabel>Apply ${label} knockback</debugLabel>`;
                xml += `\n${indent}  <distance>${dist}</distance>`;
            } else if (type === 'HealActionDef') {
                const amt = actionDiv.querySelector('.amount').value;
                xml += `\n${indent}  <debugLabel>Apply ${label} healing</debugLabel>`;
                xml += `\n${indent}  <amount>${amt}</amount>`;
            } else if (type === 'PersistentAreaZoneActionDef') {
                const dur = actionDiv.querySelector('.durationTicks').value;
                const rad = actionDiv.querySelector('.zoneRadius').value;
                const pulse = actionDiv.querySelector('.pulseIntervalTicks').value;
                const markerThingDef = actionDiv.querySelector('.markerThingDef').value;
                const zonePawnAffinity = actionDiv.querySelector('.zonePawnAffinity').value;
                const includeCaster = actionDiv.querySelector('.includeCaster').checked;
                const replaceExistingForCaster = actionDiv.querySelector('.replaceExistingForCaster').checked;
                const pulseAtCenter = actionDiv.querySelector('.pulseAtCenter').checked;
                const ambientEffectDef = actionDiv.querySelector('.ambientEffectDef').value;
                xml += `\n${indent}  <debugLabel>Create ${label} persistent zone</debugLabel>`;
                xml += `\n${indent}  <markerThingDef>${escapeXML(markerThingDef || 'MF_FlameFieldMarker')}</markerThingDef>`;
                xml += `\n${indent}  <zoneRadius>${rad}</zoneRadius>`;
                xml += `\n${indent}  <pulseIntervalTicks>${pulse}</pulseIntervalTicks>`;
                if (ambientEffectDef) xml += `\n${indent}  <ambientEffectDef>${escapeXML(ambientEffectDef)}</ambientEffectDef>`;
                xml += `\n${indent}  <durationTicks>${dur}</durationTicks>`;
                xml += `\n${indent}  <pulseAtCenter>${pulseAtCenter}</pulseAtCenter>`;
                xml += `\n${indent}  <pawnAffinity>${zonePawnAffinity}</pawnAffinity>`;
                xml += `\n${indent}  <includeCaster>${includeCaster}</includeCaster>`;
                xml += `\n${indent}  <replaceExistingForCaster>${replaceExistingForCaster}</replaceExistingForCaster>`;
                
                const childContainer = Array.from(actionDiv.children).find(c => c.classList.contains('child-actions-container'));
                if (childContainer && Array.from(childContainer.children).filter(c => c.classList.contains('action-item')).length > 0) {
                    const centerPulseXml = buildXML(childContainer, indentLevel + 2, label, 'onPulseActions');
                    if (centerPulseXml) {
                        xml += `\n${indent}  <onPulseActions>`;
                        xml += centerPulseXml;
                        xml += `\n${indent}  </onPulseActions>`;
                    }

                    const areaPulseXml = buildXML(childContainer, indentLevel + 2, label, 'actions');
                    if (areaPulseXml) {
                        xml += `\n${indent}  <actions>`;
                        xml += areaPulseXml;
                        xml += `\n${indent}  </actions>`;
                    }
                }
            }
            
            xml += `\n${indent}</li>`;
        });
        return xml;
    }

    function updateXML() {
        const defName = escapeXML(getVal('defName'));
        const label = escapeXML(getVal('label'));
        const description = escapeXML(getVal('description'));
        const range = getVal('range');
        const castTimeTicks = getVal('castTimeTicks');
        const gizmoIconPath = escapeXML(getVal('gizmoIconPath'));

        const targetShape = getVal('targetShape');
        const primaryTargetType = getVal('primaryTargetType');
        const pawnAffinity = getVal('pawnAffinity');
        const includePawns = getVal('includePawns');
        const includeBuildings = getVal('includeBuildings');
        const includeItems = getVal('includeItems');
        const allowSelfTarget = getVal('allowSelfTarget');
        const requireLineOfSight = getVal('requireLineOfSight');
        const targetRadius = getVal('targetRadius');

        const manaCost = getVal('manaCost');
        const cooldownTicks = getVal('cooldownTicks');

        let xml = `<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <MagicFramework.Definitions.SpellDef>
    <defName>${defName}</defName>
    <label>${label}</label>
    <description>${description}</description>
    <range>${range}</range>
    <castTimeTicks>${castTimeTicks}</castTimeTicks>
    <gizmoIconPath>${gizmoIconPath}</gizmoIconPath>

    <targeting>
      <shape>${targetShape}</shape>
      <primaryTargetType>${primaryTargetType}</primaryTargetType>
      <pawnAffinity>${pawnAffinity}</pawnAffinity>
      <includePawns>${includePawns}</includePawns>
      <includeBuildings>${includeBuildings}</includeBuildings>
      <includeItems>${includeItems}</includeItems>
      <allowSelfTarget>${allowSelfTarget}</allowSelfTarget>
      <requireLineOfSight>${requireLineOfSight}</requireLineOfSight>`;
      
      if (targetShape === 'Radius' || targetShape === 'Explosion') {
          xml += `\n      <radius>${targetRadius}</radius>`;
      }

      xml += `\n      <range>${range}</range>
    </targeting>

    <requirements>
      <li Class="MagicFramework.Definitions.ManaRequirementDef">
        <debugLabel>Enough mana for ${label}</debugLabel>
        <amount>${manaCost}</amount>
      </li>
      <li Class="MagicFramework.Definitions.CooldownRequirementDef">
        <debugLabel>${label} cooldown ready</debugLabel>
        <cooldownTicks>${cooldownTicks}</cooldownTicks>
      </li>
    </requirements>

    <costs>
      <li Class="MagicFramework.Definitions.ManaCostDef">
        <debugLabel>Spend mana for ${label}</debugLabel>
        <amount>${manaCost}</amount>
      </li>
      <li Class="MagicFramework.Definitions.CooldownCostDef">
        <debugLabel>Start ${label} cooldown</debugLabel>
        <cooldownTicks>${cooldownTicks}</cooldownTicks>
      </li>
    </costs>

    <actions>
      <li Class="MagicFramework.Definitions.SequenceActionDef">
        <debugLabel>${label} sequence</debugLabel>
        <actions>`;

        const topActions = Array.from(actionsContainer.children).filter(c => c.classList.contains('action-item'));
        if (topActions.length === 0) {
            xml += `\n          <!-- Add actions here -->`;
        } else {
            xml += buildXML(actionsContainer, 5, label);
        }

        xml += `
        </actions>
      </li>
    </actions>
  </MagicFramework.Definitions.SpellDef>
</Defs>`;

        xmlPreview.textContent = xml;
    }

    // AI Generation Logic
    const systemPrompt = `You are an AI assistant helping a player design a MagicFramework spell for RimWorld.
Return a STRICT JSON response containing the spell configuration. Do NOT include markdown blocks. Do NOT include any text outside the JSON.

Expected JSON schema:
{
    "defName": "String (e.g. MF_Frostbolt)",
    "label": "String (e.g. frostbolt)",
    "description": "String",
    "range": Number (e.g. 24),
    "castTimeTicks": Number (e.g. 30),
    "gizmoIconPath": "String (e.g. UI/Gizmos/Spells/MF_Frostbolt)",
    "targetShape": "String (Single, Radius, Line, Wall)",
    "primaryTargetType": "String (Pawn, Cell, Thing, PawnOrThing, PawnOrCell)",
    "pawnAffinity": "String (All, Ally, Foe)",
    "targetRadius": Number (e.g. 3.9),
    "includePawns": Boolean,
    "includeBuildings": Boolean,
    "includeItems": Boolean,
    "allowSelfTarget": Boolean,
    "requireLineOfSight": Boolean,
    "manaCost": Number,
    "cooldownTicks": Number,
    "actions": [
        {
            "type": "EffectActionDef" | "LaunchProjectileActionDef" | "DamageActionDef" | "KnockbackActionDef" | "HealActionDef" | "PersistentAreaZoneActionDef",
            "effectDef": "String (optional)",
            "soundDef": "String (optional)",
            "locationSource": "String (Caster, CurrentTarget, CurrentCell)",
            "attachToTarget": Boolean,
            "projectileDef": "String (optional)",
            "amount": Number (optional),
            "damageDef": "String (optional)",
            "distance": Number (optional),
            "durationTicks": Number (optional),
            "zoneRadius": Number (optional),
            "pulseIntervalTicks": Number (optional),
            "markerThingDef": "String (optional, required for PersistentAreaZoneActionDef; use MF_FlameFieldMarker, MF_FreezeMarker, MF_EarthCallMarker, or MF_WatersEmbraceMarker)",
            "zonePawnAffinity": "String (optional, All, Ally, Foe)",
            "includeCaster": Boolean,
            "replaceExistingForCaster": Boolean,
            "pulseAtCenter": Boolean,
            "ambientEffectDef": "String (optional)",
            "childActions": [ "Array of action objects (optional, per pawn/cell pulse actions for PersistentAreaZoneActionDef)" ],
            "onPulseActions": [ "Array of action objects (optional, center lifecycle pulse actions for PersistentAreaZoneActionDef; best for visuals and sounds)" ]
        }
    ]
}`;

    aiGenerateBtn.addEventListener('click', async () => {
        const prompt = aiPromptInput.value.trim();
        if (!prompt) {
            alert('Please describe the spell you want to generate.');
            return;
        }

        const provider = localStorage.getItem('aiProvider');
        const apiKey = localStorage.getItem('aiApiKey');

        if (!apiKey) {
            alert('Please enter an API key in Settings first.');
            settingsModal.style.display = 'flex';
            return;
        }

        aiGenerateBtn.disabled = true;
        aiLoading.style.display = 'flex';

        try {
            let jsonText = '';

            if (provider === 'openai') {
                const res = await fetch('https://api.openai.com/v1/chat/completions', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${apiKey}`
                    },
                    body: JSON.stringify({
                        model: 'gpt-3.5-turbo',
                        messages: [
                            { role: 'system', content: systemPrompt },
                            { role: 'user', content: prompt }
                        ],
                        temperature: 0.7
                    })
                });

                if (!res.ok) {
                    let errBody = '';
                    try { errBody = JSON.stringify(await res.json()); } catch(e) { errBody = await res.text(); }
                    throw new Error('OpenAI API Error (' + res.status + '): ' + (errBody || res.statusText || 'Unknown Error'));
                }
                const data = await res.json();
                jsonText = data.choices[0].message.content;
            } else if (provider === 'gemini') {
                const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro-latest:generateContent?key=${apiKey}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        system_instruction: { parts: [{text: systemPrompt}] },
                        contents: [{ parts: [{ text: prompt }] }]
                    })
                });

                if (!res.ok) {
                    let errBody = '';
                    try { errBody = JSON.stringify(await res.json()); } catch(e) { errBody = await res.text(); }
                    throw new Error('Gemini API Error (' + res.status + '): ' + (errBody || res.statusText || 'Unknown Error'));
                }
                const data = await res.json();
                jsonText = data.candidates[0].content.parts[0].text;
            }

            // Cleanup markdown if present
            jsonText = jsonText.replace(/```json/g, '').replace(/```/g, '').trim();
            
            let config;
            try {
                config = JSON.parse(jsonText);
            } catch (parseErr) {
                throw new Error('Failed to parse AI response as JSON. Response was:\n' + jsonText);
            }
            
            applyConfigToForm(config);

        } catch (err) {
            console.error(err);
            alert('Failed to generate spell: ' + err.message);
        } finally {
            aiGenerateBtn.disabled = false;
            aiLoading.style.display = 'none';
        }
    });

    function applyConfigToForm(config) {
        // Strict Validation and Application to prevent corrupted XML
        if (typeof config.defName === 'string') setVal('defName', config.defName);
        if (typeof config.label === 'string') setVal('label', config.label);
        if (typeof config.description === 'string') setVal('description', config.description);
        if (typeof config.range === 'number') setVal('range', config.range);
        if (typeof config.castTimeTicks === 'number') setVal('castTimeTicks', config.castTimeTicks);
        if (typeof config.gizmoIconPath === 'string') setVal('gizmoIconPath', config.gizmoIconPath);

        const validShapes = ['Single', 'Radius', 'Line', 'Wall'];
        if (validShapes.includes(config.targetShape)) setVal('targetShape', config.targetShape);
        
        const validTypes = ['Pawn', 'Cell', 'Thing', 'PawnOrThing', 'PawnOrCell'];
        if (validTypes.includes(config.primaryTargetType)) setVal('primaryTargetType', config.primaryTargetType);
        
        const validAffinities = ['All', 'Ally', 'Foe'];
        if (validAffinities.includes(config.pawnAffinity)) setVal('pawnAffinity', config.pawnAffinity);

        if (typeof config.targetRadius === 'number') setVal('targetRadius', config.targetRadius);
        
        if (typeof config.includePawns === 'boolean') setVal('includePawns', config.includePawns);
        if (typeof config.includeBuildings === 'boolean') setVal('includeBuildings', config.includeBuildings);
        if (typeof config.includeItems === 'boolean') setVal('includeItems', config.includeItems);
        if (typeof config.allowSelfTarget === 'boolean') setVal('allowSelfTarget', config.allowSelfTarget);
        if (typeof config.requireLineOfSight === 'boolean') setVal('requireLineOfSight', config.requireLineOfSight);

        if (typeof config.manaCost === 'number') setVal('manaCost', config.manaCost);
        if (typeof config.cooldownTicks === 'number') setVal('cooldownTicks', config.cooldownTicks);

        // Actions
        if (Array.isArray(config.actions)) {
            // Clear existing actions
            actionsContainer.innerHTML = '';
            
            function applyActionsConfig(actionsArr, parentId = null) {
                if (!Array.isArray(actionsArr)) return;
                actionsArr.forEach(act => {
                    if (!act || typeof act !== 'object') return;
                    let actionId = null;
                    if (act.type === 'EffectActionDef') {
                        actionId = window.addAction('EffectActionDef', {
                            zoneChildPhase: act.zoneChildPhase,
                            effectDef: typeof act.effectDef === 'string' ? act.effectDef : 'PsycastAreaEffect',
                            soundDef: typeof act.soundDef === 'string' ? act.soundDef : '',
                            locationSource: typeof act.locationSource === 'string' ? act.locationSource : (parentId ? 'CurrentCell' : 'Caster'),
                            attachToTarget: !!act.attachToTarget
                        }, parentId);
                    } else if (act.type === 'LaunchProjectileActionDef') {
                        actionId = window.addAction('LaunchProjectileActionDef', {
                            zoneChildPhase: act.zoneChildPhase,
                            projectileDef: typeof act.projectileDef === 'string' ? act.projectileDef : 'MF_NewProjectile'
                        }, parentId);
                    } else if (act.type === 'DamageActionDef') {
                        actionId = window.addAction('DamageActionDef', {
                            zoneChildPhase: act.zoneChildPhase,
                            amount: typeof act.amount === 'number' ? act.amount : 10,
                            damageDef: typeof act.damageDef === 'string' ? act.damageDef : 'Blunt'
                        }, parentId);
                    } else if (act.type === 'KnockbackActionDef') {
                        actionId = window.addAction('KnockbackActionDef', {
                            zoneChildPhase: act.zoneChildPhase,
                            distance: typeof act.distance === 'number' ? act.distance : 3
                        }, parentId);
                    } else if (act.type === 'HealActionDef') {
                        actionId = window.addAction('HealActionDef', {
                            zoneChildPhase: act.zoneChildPhase,
                            amount: typeof act.amount === 'number' ? act.amount : 15
                        }, parentId);
                    } else if (act.type === 'PersistentAreaZoneActionDef') {
                        actionId = window.addAction('PersistentAreaZoneActionDef', {
                            zoneChildPhase: act.zoneChildPhase,
                            durationTicks: typeof act.durationTicks === 'number' ? act.durationTicks : 600,
                            zoneRadius: typeof act.zoneRadius === 'number' ? act.zoneRadius : 3.0,
                            pulseIntervalTicks: typeof act.pulseIntervalTicks === 'number' ? act.pulseIntervalTicks : 60,
                            markerThingDef: typeof act.markerThingDef === 'string' ? act.markerThingDef : 'MF_FlameFieldMarker',
                            pawnAffinity: typeof act.zonePawnAffinity === 'string' ? act.zonePawnAffinity : (typeof act.pawnAffinity === 'string' ? act.pawnAffinity : 'All'),
                            includeCaster: typeof act.includeCaster === 'boolean' ? act.includeCaster : false,
                            replaceExistingForCaster: typeof act.replaceExistingForCaster === 'boolean' ? act.replaceExistingForCaster : true,
                            pulseAtCenter: typeof act.pulseAtCenter === 'boolean' ? act.pulseAtCenter : false,
                            ambientEffectDef: typeof act.ambientEffectDef === 'string' ? act.ambientEffectDef : ''
                        }, parentId);
                    }

                    if (actionId && Array.isArray(act.childActions)) {
                        applyActionsConfig(act.childActions
                            .filter(child => child && typeof child === 'object')
                            .map(child => ({
                                ...child,
                                zoneChildPhase: child.zoneChildPhase || persistentZoneChildDefaults[child.type] || 'actions'
                            })), actionId);
                    }

                    if (actionId && Array.isArray(act.onPulseActions)) {
                        applyActionsConfig(act.onPulseActions
                            .filter(child => child && typeof child === 'object')
                            .map(child => ({ ...child, zoneChildPhase: 'onPulseActions' })), actionId);
                    }
                });
            }

            applyActionsConfig(config.actions);
        }

        updatePreviews();
    }
});
