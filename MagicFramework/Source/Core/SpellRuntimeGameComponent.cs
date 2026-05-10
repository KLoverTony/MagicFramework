using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Scheduling;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MagicFramework.Core;

/// <summary>
/// Stores lightweight per-caster spell runtime state such as mana and cooldowns.
/// </summary>
public sealed class SpellRuntimeGameComponent : GameComponent
{
    private const float DefaultStartingMana = 100f;
    private List<CasterRuntimeState> casterStates = new();
    private List<ActiveSpellStatModifier> activeStatModifiers = new();
    private List<ActiveSpellForceField> activeForceFields = new();
    private static readonly Dictionary<string, Material> ForceFieldOverlayMaterials = new();
    private bool cleaningStatModifiers;

    public SpellRuntimeGameComponent(Game game)
    {
    }

    public static SpellRuntimeGameComponent Instance => Current.Game?.GetComponent<SpellRuntimeGameComponent>();

    public float GetCurrentMana(Thing caster)
    {
        return GetOrCreateState(caster).currentMana;
    }

    public float GetMaxMana(Thing caster)
    {
        return DefaultStartingMana;
    }

    public bool HasArcaneGift(Pawn pawn)
    {
        return pawn != null && GetOrCreateState(pawn).hasArcaneGift;
    }

    public void SetArcaneGift(Pawn pawn, bool value)
    {
        if (pawn == null)
        {
            return;
        }

        GetOrCreateState(pawn).hasArcaneGift = value;
    }

    public int GetCasterLevel(Pawn pawn)
    {
        if (pawn == null)
        {
            return 0;
        }

        return GetOrCreateState(pawn).casterLevel;
    }

    public void SetCasterLevel(Pawn pawn, int level)
    {
        if (pawn == null)
        {
            return;
        }

        CasterRuntimeState state = GetOrCreateState(pawn);
        state.casterLevel = Mathf.Max(0, level);
        state.debugCasterLevel = state.casterLevel;
    }

    public int GetDebugCasterLevel(Thing caster)
    {
        if (caster == null)
        {
            return 0;
        }

        return GetOrCreateState(caster).casterLevel;
    }

    public int CycleDebugCasterLevel(Thing caster)
    {
        if (caster == null)
        {
            return 0;
        }

        CasterRuntimeState state = GetOrCreateState(caster);
        state.casterLevel = state.casterLevel switch
        {
            0 => 1,
            1 => 3,
            3 => 5,
            5 => 10,
            10 => 20,
            _ => 0
        };
        state.debugCasterLevel = state.casterLevel;
        return state.casterLevel;
    }

    public bool HasEnoughMana(Thing caster, float amount)
    {
        return GetCurrentMana(caster) >= amount;
    }

    public void SpendMana(Thing caster, float amount)
    {
        if (caster == null || amount <= 0f)
        {
            return;
        }

        CasterRuntimeState state = GetOrCreateState(caster);
        state.currentMana = state.currentMana > amount ? state.currentMana - amount : 0f;
    }

    public int GetCooldownRemainingTicks(Thing caster, SpellDef spellDef)
    {
        if (caster == null || spellDef == null)
        {
            return 0;
        }

        int readyTick = GetOrCreateState(caster).GetCooldownReadyTick(spellDef.defName);
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        return readyTick > currentTick ? readyTick - currentTick : 0;
    }

    public bool IsOnCooldown(Thing caster, SpellDef spellDef)
    {
        return GetCooldownRemainingTicks(caster, spellDef) > 0;
    }

    public void StartCooldown(Thing caster, SpellDef spellDef, int cooldownTicks)
    {
        if (caster == null || spellDef == null || cooldownTicks <= 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        GetOrCreateState(caster).SetCooldownReadyTick(spellDef.defName, currentTick + cooldownTicks);
    }

    public void ApplyStatModifiers(
        Thing target,
        Thing caster,
        SpellDef spellDef,
        int durationTicks,
        bool replaceExistingFromCasterSpell,
        SpellStatusCueDef statusCue,
        IEnumerable<SpellStatModifierDef> authoredModifiers)
    {
        if (target == null || spellDef == null || authoredModifiers == null)
        {
            return;
        }

        activeStatModifiers ??= new List<ActiveSpellStatModifier>();
        int expireAtTick = (Find.TickManager?.TicksGame ?? 0) + (durationTicks > 0 ? durationTicks : 1);

        ActiveSpellStatModifier modifier = new()
        {
            target = target,
            caster = caster,
            spellDef = spellDef,
            expireAtTick = expireAtTick,
            indicatorSeverity = statusCue?.severity ?? 0.01f,
            removeIndicatorOnExpire = statusCue?.removeOnExpire ?? true,
            statusCueLabel = ResolveStatusCueLabel(statusCue, spellDef),
            statusCueDescription = ResolveStatusCueDescription(statusCue, spellDef)
        };

        HediffDef indicatorHediffDef = ResolveStatusCueHediffDef(statusCue);
        if (indicatorHediffDef != null)
        {
            modifier.indicatorHediffDef = indicatorHediffDef;
        }

        foreach (SpellStatModifierDef authoredModifier in authoredModifiers)
        {
            if (authoredModifier == null || string.IsNullOrWhiteSpace(authoredModifier.statDef))
            {
                continue;
            }

            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(authoredModifier.statDef);
            if (statDef == null)
            {
                Log.Warning($"[MagicFramework] Could not resolve stat def '{authoredModifier.statDef}' for timed modifier.");
                continue;
            }

            modifier.modifiers.Add(new ActiveSpellStatModifierEntry
            {
                statDef = statDef,
                offset = authoredModifier.offset,
                factor = authoredModifier.factor
            });
        }

        if (modifier.modifiers.Count == 0)
        {
            return;
        }

        if (replaceExistingFromCasterSpell && TryRefreshStatModifier(target, caster, spellDef, modifier))
        {
            EnsureIndicatorApplied(modifier);
            return;
        }

        if (replaceExistingFromCasterSpell)
        {
            RemoveStatModifiers(target, caster, spellDef);
        }

        activeStatModifiers.Add(modifier);
        EnsureIndicatorApplied(modifier);
    }

    public void ApplySustainedStatModifiers(
        Thing target,
        Thing caster,
        SpellDef spellDef,
        int maxDurationTicks,
        bool replaceExistingFromCasterSpell,
        SpellStatusCueDef statusCue,
        float maxRange,
        bool breakWhenCasterDowned,
        bool breakWhenTargetDowned,
        bool breakWhenTargetOutOfRange,
        bool breakWhenLineOfSightLost,
        SpellMaintenanceDef maintenance,
        int pulseIntervalTicks,
        IEnumerable<int> sourceActionPath,
        IEnumerable<SpellStatModifierDef> authoredModifiers)
    {
        if (target == null || spellDef == null || authoredModifiers == null)
        {
            return;
        }

        activeStatModifiers ??= new List<ActiveSpellStatModifier>();
        if (replaceExistingFromCasterSpell)
        {
            RemoveStatModifiers(target, caster, spellDef);
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        ActiveSpellStatModifier modifier = new()
        {
            target = target,
            caster = caster,
            spellDef = spellDef,
            expireAtTick = maxDurationTicks > 0 ? currentTick + maxDurationTicks : -1,
            indicatorSeverity = statusCue?.severity ?? 0.01f,
            removeIndicatorOnExpire = statusCue?.removeOnExpire ?? true,
            statusCueLabel = ResolveStatusCueLabel(statusCue, spellDef),
            statusCueDescription = ResolveStatusCueDescription(statusCue, spellDef),
            isSustained = true,
            maxRange = maxRange,
            breakWhenCasterDowned = breakWhenCasterDowned,
            breakWhenTargetDowned = breakWhenTargetDowned,
            breakWhenTargetOutOfRange = breakWhenTargetOutOfRange,
            breakWhenLineOfSightLost = breakWhenLineOfSightLost,
            maintenance = maintenance,
            pulseIntervalTicks = pulseIntervalTicks,
            nextPulseTick = pulseIntervalTicks > 0 ? currentTick + pulseIntervalTicks : -1,
            sourceActionPath = sourceActionPath != null ? new List<int>(sourceActionPath) : new List<int>()
        };

        HediffDef indicatorHediffDef = ResolveStatusCueHediffDef(statusCue);
        if (indicatorHediffDef != null)
        {
            modifier.indicatorHediffDef = indicatorHediffDef;
        }

        foreach (SpellStatModifierDef authoredModifier in authoredModifiers)
        {
            if (authoredModifier == null || string.IsNullOrWhiteSpace(authoredModifier.statDef))
            {
                continue;
            }

            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(authoredModifier.statDef);
            if (statDef == null)
            {
                Log.Warning($"[MagicFramework] Could not resolve stat def '{authoredModifier.statDef}' for sustained modifier.");
                continue;
            }

            modifier.modifiers.Add(new ActiveSpellStatModifierEntry
            {
                statDef = statDef,
                offset = authoredModifier.offset,
                factor = authoredModifier.factor
            });
        }

        if (modifier.modifiers.Count == 0)
        {
            return;
        }

        activeStatModifiers.Add(modifier);
        EnsureIndicatorApplied(modifier);
        string durationLabel = maxDurationTicks > 0 ? $"{maxDurationTicks} ticks" : "until broken";
        MagicLog.Message(MagicLogSubsystem.StatModifiers, $"[MagicFramework] Applied {modifier.modifiers.Count} sustained stat modifier(s) to {target.LabelCap} for {durationLabel}.");
    }

    public void ApplyForceField(
        Thing target,
        Thing caster,
        SpellDef spellDef,
        int maxDurationTicks,
        SpellStatusCueDef statusCue,
        float damageFactor,
        bool absorbFullyWithMana,
        float manaCostPerDamageAbsorbed,
        float sustainedManaCost,
        int sustainedManaCostIntervalTicks,
        float maxRange,
        bool breakWhenCasterDowned,
        bool breakWhenTargetDowned,
        bool breakWhenTargetOutOfRange,
        bool breakWhenLineOfSightLost,
        SpellMaintenanceDef maintenance,
        int pulseIntervalTicks,
        string impactFleckDef,
        string impactSoundDef,
        string ambientFleckDef,
        int ambientFleckIntervalTicks,
        float ambientFleckScale,
        string ambientColorHex,
        string sustainedOverlayTexturePath,
        float sustainedOverlayScale,
        string sustainedOverlayColorHex,
        IEnumerable<int> sourceActionPath)
    {
        if (target == null || spellDef == null)
        {
            return;
        }

        activeForceFields ??= new List<ActiveSpellForceField>();
        RemoveForceFields(target, caster, spellDef);

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        ActiveSpellForceField forceField = new()
        {
            target = target,
            caster = caster,
            spellDef = spellDef,
            expireAtTick = maxDurationTicks > 0 ? currentTick + maxDurationTicks : -1,
            damageFactor = Mathf.Clamp01(damageFactor),
            absorbFullyWithMana = absorbFullyWithMana,
            manaCostPerDamageAbsorbed = manaCostPerDamageAbsorbed,
            sustainedManaCost = Mathf.Max(0f, sustainedManaCost),
            sustainedManaCostIntervalTicks = Mathf.Max(1, sustainedManaCostIntervalTicks),
            maxRange = maxRange,
            breakWhenCasterDowned = breakWhenCasterDowned,
            breakWhenTargetDowned = breakWhenTargetDowned,
            breakWhenTargetOutOfRange = breakWhenTargetOutOfRange,
            breakWhenLineOfSightLost = breakWhenLineOfSightLost,
            maintenance = maintenance,
            pulseIntervalTicks = pulseIntervalTicks,
            indicatorSeverity = statusCue?.severity ?? 0.01f,
            removeIndicatorOnExpire = statusCue?.removeOnExpire ?? true,
            statusCueLabel = ResolveStatusCueLabel(statusCue, spellDef),
            statusCueDescription = ResolveStatusCueDescription(statusCue, spellDef),
            impactFleckDef = impactFleckDef,
            impactSoundDef = impactSoundDef,
            ambientFleckDef = ambientFleckDef,
            ambientFleckIntervalTicks = ambientFleckIntervalTicks,
            ambientFleckScale = ambientFleckScale,
            ambientColorHex = ambientColorHex,
            sustainedOverlayTexturePath = sustainedOverlayTexturePath,
            sustainedOverlayScale = sustainedOverlayScale,
            sustainedOverlayColorHex = sustainedOverlayColorHex,
            nextAmbientFleckTick = currentTick,
            nextPulseTick = pulseIntervalTicks > 0 ? currentTick + pulseIntervalTicks : -1,
            nextSustainedManaCostTick = currentTick + Mathf.Max(1, sustainedManaCostIntervalTicks),
            sourceActionPath = sourceActionPath != null ? new List<int>(sourceActionPath) : new List<int>()
        };

        HediffDef indicatorHediffDef = ResolveStatusCueHediffDef(statusCue);
        if (indicatorHediffDef != null)
        {
            forceField.indicatorHediffDef = indicatorHediffDef;
        }

        activeForceFields.Add(forceField);
        EnsureForceFieldIndicatorApplied(forceField);
        SpawnForceFieldImpact(forceField, 1.6f);
        RunForceFieldLifecycleActions(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Create));
        MagicLog.Message(MagicLogSubsystem.ForceFields, $"[MagicFramework] Applied force field to {target.LabelCap}.");
    }

    public void ApplyForceFieldDamageReduction(Thing thing, ref DamageInfo dinfo, ref bool absorbed)
    {
        if (thing == null || activeForceFields == null || activeForceFields.Count == 0 || absorbed)
        {
            return;
        }

        CleanupExpiredForceFields(Find.TickManager?.TicksGame ?? 0);
        for (int i = activeForceFields.Count - 1; i >= 0; i--)
        {
            ActiveSpellForceField forceField = activeForceFields[i];
            if (forceField?.target != thing)
            {
                continue;
            }

            float incomingAmount = dinfo.Amount;
            if (incomingAmount <= 0f)
            {
                return;
            }

            if (forceField.absorbFullyWithMana)
            {
                float manaCost = incomingAmount * Mathf.Max(0f, forceField.manaCostPerDamageAbsorbed);
                if (HasEnoughMana(forceField.caster, manaCost))
                {
                    SpendMana(forceField.caster, manaCost);
                    absorbed = true;
                    SpawnForceFieldImpact(forceField, 1.2f);
                    MagicLog.Message(MagicLogSubsystem.ForceFields, $"[MagicFramework] Force field absorbed {incomingAmount:0.##} damage for {manaCost:0.##} mana.");
                    return;
                }

                activeForceFields.RemoveAt(i);
                CleanupForceField(forceField);
                RunForceFieldLifecycleActions(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Break, "insufficient mana to absorb damage"));
                return;
            }

            float damageFactor = Mathf.Clamp01(forceField.damageFactor);
            if (damageFactor >= 1f)
            {
                continue;
            }

            float reducedAmount = Mathf.Max(0f, incomingAmount * damageFactor);
            float preventedAmount = incomingAmount - reducedAmount;
            if (preventedAmount <= 0f)
            {
                return;
            }

            float manaCostPerDamage = Mathf.Max(0f, forceField.manaCostPerDamageAbsorbed);
            if (manaCostPerDamage > 0f)
            {
                float manaCost = preventedAmount * manaCostPerDamage;
                if (!HasEnoughMana(forceField.caster, manaCost))
                {
                    activeForceFields.RemoveAt(i);
                    CleanupForceField(forceField);
                    RunForceFieldLifecycleActions(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Break, "insufficient mana to reduce damage"));
                    return;
                }

                SpendMana(forceField.caster, manaCost);
            }

            dinfo.SetAmount(reducedAmount);
            SpawnForceFieldImpact(forceField, 1f);
            MagicLog.Message(MagicLogSubsystem.ForceFields, $"[MagicFramework] Force field reduced incoming damage from {incomingAmount:0.##} to {reducedAmount:0.##}.");
            return;
        }
    }

    public void DrawForceFieldOverlay(Pawn pawn, Vector3 drawLoc)
    {
        if (pawn == null || activeForceFields == null || activeForceFields.Count == 0)
        {
            return;
        }

        ActiveSpellForceField forceField = null;
        for (int i = activeForceFields.Count - 1; i >= 0; i--)
        {
            if (activeForceFields[i]?.target == pawn)
            {
                forceField = activeForceFields[i];
                break;
            }
        }

        if (forceField == null || string.IsNullOrWhiteSpace(forceField.sustainedOverlayTexturePath))
        {
            return;
        }

        Material material = ResolveForceFieldOverlayMaterial(forceField);
        if (material == null)
        {
            return;
        }

        float scale = Mathf.Max(0.1f, forceField.sustainedOverlayScale);
        float pulse = 1f + (Mathf.Sin((Find.TickManager?.TicksGame ?? 0) / 12f) * 0.018f);
        Vector3 position = drawLoc;
        position.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(scale * pulse, 1f, scale * pulse));
        Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
    }

    public bool HasActiveMaintainedSpell(Thing caster, SpellDef spellDef)
    {
        if (caster == null || spellDef == null)
        {
            return false;
        }

        if (activeStatModifiers != null)
        {
            for (int i = 0; i < activeStatModifiers.Count; i++)
            {
                ActiveSpellStatModifier modifier = activeStatModifiers[i];
                if (modifier?.isSustained == true && modifier.caster == caster && modifier.spellDef == spellDef)
                {
                    return true;
                }
            }
        }

        if (activeForceFields != null)
        {
            for (int i = 0; i < activeForceFields.Count; i++)
            {
                ActiveSpellForceField forceField = activeForceFields[i];
                if (forceField?.caster == caster && forceField.spellDef == spellDef)
                {
                    return true;
                }
            }
        }

        Map map = caster.MapHeld;
        if (map?.GetComponent<PersistentAreaZoneMapComponent>()?.HasForCasterSpell(caster, spellDef) == true)
        {
            return true;
        }

        return false;
    }

    public bool KnowsSpell(Pawn pawn, SpellDef spellDef)
    {
        if (pawn == null || spellDef == null)
        {
            return false;
        }

        return GetOrCreateState(pawn).KnowsSpell(spellDef.defName);
    }

    public bool LearnSpell(Pawn pawn, SpellDef spellDef)
    {
        if (pawn == null || spellDef == null)
        {
            return false;
        }

        return GetOrCreateState(pawn).LearnSpell(spellDef.defName);
    }

    public bool ForgetSpell(Pawn pawn, SpellDef spellDef)
    {
        if (pawn == null || spellDef == null)
        {
            return false;
        }

        return GetOrCreateState(pawn).ForgetSpell(spellDef.defName);
    }

    public IEnumerable<SpellDef> GetKnownSpells(Pawn pawn)
    {
        if (pawn == null)
        {
            yield break;
        }

        CasterRuntimeState state = GetOrCreateState(pawn);
        foreach (string spellDefName in state.KnownSpellDefNames)
        {
            SpellDef spellDef = DefDatabase<SpellDef>.GetNamedSilentFail(spellDefName);
            if (spellDef != null)
            {
                yield return spellDef;
            }
        }
    }

    public int CancelMaintainedSpell(Thing caster, SpellDef spellDef, bool runBreakActions)
    {
        if (caster == null || spellDef == null)
        {
            return 0;
        }

        int removedCount = 0;
        List<SustainedBreakRecord> statBreakRecords = new();
        if (activeStatModifiers != null)
        {
            for (int i = activeStatModifiers.Count - 1; i >= 0; i--)
            {
                ActiveSpellStatModifier modifier = activeStatModifiers[i];
                if (modifier?.isSustained != true || modifier.caster != caster || modifier.spellDef != spellDef)
                {
                    continue;
                }

                activeStatModifiers.RemoveAt(i);
                CleanupModifier(modifier);
                removedCount++;
                if (runBreakActions)
                {
                    statBreakRecords.Add(new SustainedBreakRecord(modifier, "manually cancelled"));
                }
            }
        }

        List<ForceFieldLifecycleRecord> forceFieldLifecycleRecords = new();
        if (activeForceFields != null)
        {
            for (int i = activeForceFields.Count - 1; i >= 0; i--)
            {
                ActiveSpellForceField forceField = activeForceFields[i];
                if (forceField?.caster != caster || forceField.spellDef != spellDef)
                {
                    continue;
                }

                activeForceFields.RemoveAt(i);
                CleanupForceField(forceField);
                removedCount++;
                forceFieldLifecycleRecords.Add(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Remove, "manually cancelled"));
            }
        }

        Map map = caster.MapHeld;
        PersistentAreaZoneMapComponent areaZoneRuntime = map?.GetComponent<PersistentAreaZoneMapComponent>();
        if (areaZoneRuntime != null)
        {
            removedCount += areaZoneRuntime.RemoveForCasterSpell(caster, spellDef);
        }

        for (int i = 0; i < statBreakRecords.Count; i++)
        {
            RunSustainedBreakActions(statBreakRecords[i]);
        }

        for (int i = 0; i < forceFieldLifecycleRecords.Count; i++)
        {
            RunForceFieldLifecycleActions(forceFieldLifecycleRecords[i]);
        }

        if (removedCount > 0)
        {
            MagicLog.Message(MagicLogSubsystem.Execution, $"[MagicFramework] Cancelled {removedCount} maintained effect(s) for {spellDef.defName ?? "<unknown spell>"}.");
        }

        return removedCount;
    }

    public void ClearStatModifiers(
        Thing target,
        Thing caster,
        SpellDef currentSpellDef,
        ClearStatModifierScope scope,
        string specificSpellDefName,
        string statusHediffDefName,
        bool runBreakActions)
    {
        if (target == null || activeStatModifiers == null || activeStatModifiers.Count == 0)
        {
            return;
        }

        SpellDef specificSpellDef = string.IsNullOrWhiteSpace(specificSpellDefName)
            ? null
            : DefDatabase<SpellDef>.GetNamedSilentFail(specificSpellDefName);
        HediffDef statusHediffDef = string.IsNullOrWhiteSpace(statusHediffDefName)
            ? null
            : DefDatabase<HediffDef>.GetNamedSilentFail(statusHediffDefName);

        if (scope == ClearStatModifierScope.FromSpecificSpell && specificSpellDef == null)
        {
            Log.Warning($"[MagicFramework] ClearStatModifiersAction skipped because spell def '{specificSpellDefName ?? "<null>"}' could not be resolved.");
            return;
        }

        if (scope == ClearStatModifierScope.WithStatusHediff && statusHediffDef == null)
        {
            Log.Warning($"[MagicFramework] ClearStatModifiersAction skipped because status hediff def '{statusHediffDefName ?? "<null>"}' could not be resolved.");
            return;
        }

        List<SustainedBreakRecord> breakRecords = new();
        int removedCount = 0;
        for (int i = activeStatModifiers.Count - 1; i >= 0; i--)
        {
            ActiveSpellStatModifier modifier = activeStatModifiers[i];
            if (!MatchesClearScope(modifier, target, caster, currentSpellDef, scope, specificSpellDef, statusHediffDef))
            {
                continue;
            }

            activeStatModifiers.RemoveAt(i);
            CleanupModifier(modifier);
            removedCount++;
            if (runBreakActions && modifier?.isSustained == true)
            {
                breakRecords.Add(new SustainedBreakRecord(modifier, "cleared by spell action"));
            }
        }

        if (removedCount > 0)
        {
            MagicLog.Message(MagicLogSubsystem.StatModifiers, $"[MagicFramework] Cleared {removedCount} active stat modifier effect(s) from {target.LabelCap}.");
        }

        for (int i = 0; i < breakRecords.Count; i++)
        {
            RunSustainedBreakActions(breakRecords[i]);
        }
    }

    public void ApplyStatAdjustments(Thing thing, StatDef statDef, ref float value)
    {
        if (thing == null || statDef == null || activeStatModifiers == null || activeStatModifiers.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        CleanupExpiredStatModifiers(currentTick);

        float totalOffset = 0f;
        float totalFactor = 1f;
        for (int i = 0; i < activeStatModifiers.Count; i++)
        {
            ActiveSpellStatModifier modifier = activeStatModifiers[i];
            if (modifier?.target != thing)
            {
                continue;
            }

            for (int j = 0; j < modifier.modifiers.Count; j++)
            {
                ActiveSpellStatModifierEntry entry = modifier.modifiers[j];
                if (entry?.statDef != statDef)
                {
                    continue;
                }

                totalOffset += entry.offset;
                totalFactor *= entry.factor;
            }
        }

        value = (value + totalOffset) * totalFactor;
    }

    public override void GameComponentTick()
    {
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        CleanupExpiredStatModifiers(currentTick);
        CleanupExpiredForceFields(currentTick);
        TickSustainedStatModifierPulses(currentTick);
        TickForceFieldPulses(currentTick);
        TickForceFieldSustainedManaCosts(currentTick);
        TickForceFieldAmbientVisuals(currentTick);
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref casterStates, "casterStates", LookMode.Deep);
        Scribe_Collections.Look(ref activeStatModifiers, "activeStatModifiers", LookMode.Deep);
        Scribe_Collections.Look(ref activeForceFields, "activeForceFields", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && casterStates == null)
        {
            casterStates = new List<CasterRuntimeState>();
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit && activeStatModifiers == null)
        {
            activeStatModifiers = new List<ActiveSpellStatModifier>();
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit && activeForceFields == null)
        {
            activeForceFields = new List<ActiveSpellForceField>();
        }
    }

    private CasterRuntimeState GetOrCreateState(Thing caster)
    {
        if (caster == null)
        {
            return new CasterRuntimeState();
        }

        casterStates ??= new List<CasterRuntimeState>();
        for (int i = casterStates.Count - 1; i >= 0; i--)
        {
            CasterRuntimeState state = casterStates[i];
            if (state?.caster == null || state.caster.Destroyed)
            {
                casterStates.RemoveAt(i);
                continue;
            }

            if (state.caster == caster)
            {
                return state;
            }
        }

        CasterRuntimeState createdState = new()
        {
            caster = caster,
            currentMana = DefaultStartingMana
        };
        casterStates.Add(createdState);
        return createdState;
    }

    private void RemoveStatModifiers(Thing target, Thing caster, SpellDef spellDef)
    {
        if (activeStatModifiers == null)
        {
            return;
        }

        for (int i = activeStatModifiers.Count - 1; i >= 0; i--)
        {
            ActiveSpellStatModifier modifier = activeStatModifiers[i];
            if (modifier == null || modifier.target == null || modifier.target.Destroyed)
            {
                activeStatModifiers.RemoveAt(i);
                continue;
            }

            if (modifier.target == target && modifier.caster == caster && modifier.spellDef == spellDef)
            {
                activeStatModifiers.RemoveAt(i);
                CleanupModifier(modifier);
            }
        }
    }

    private bool TryRefreshStatModifier(Thing target, Thing caster, SpellDef spellDef, ActiveSpellStatModifier replacement)
    {
        if (activeStatModifiers == null || replacement == null)
        {
            return false;
        }

        for (int i = activeStatModifiers.Count - 1; i >= 0; i--)
        {
            ActiveSpellStatModifier existing = activeStatModifiers[i];
            if (existing == null || existing.target == null || existing.target.Destroyed)
            {
                activeStatModifiers.RemoveAt(i);
                continue;
            }

            if (existing.isSustained || !CanRefreshStatModifier(existing, target, caster, spellDef, replacement))
            {
                continue;
            }

            existing.expireAtTick = replacement.expireAtTick;
            existing.indicatorHediffDef = replacement.indicatorHediffDef;
            existing.indicatorSeverity = replacement.indicatorSeverity;
            existing.removeIndicatorOnExpire = replacement.removeIndicatorOnExpire;
            existing.statusCueLabel = replacement.statusCueLabel;
            existing.statusCueDescription = replacement.statusCueDescription;
            existing.modifiers = replacement.modifiers;
            return true;
        }

        return false;
    }

    private static bool CanRefreshStatModifier(
        ActiveSpellStatModifier existing,
        Thing target,
        Thing caster,
        SpellDef spellDef,
        ActiveSpellStatModifier replacement)
    {
        if (existing?.target != target || replacement == null)
        {
            return false;
        }

        if (existing.caster == caster && existing.spellDef == spellDef)
        {
            return true;
        }

        return existing.indicatorHediffDef != null
            && existing.indicatorHediffDef == replacement.indicatorHediffDef;
    }

    private void RemoveForceFields(Thing target, Thing caster, SpellDef spellDef)
    {
        if (activeForceFields == null)
        {
            return;
        }

        List<ForceFieldLifecycleRecord> removeRecords = new();
        for (int i = activeForceFields.Count - 1; i >= 0; i--)
        {
            ActiveSpellForceField forceField = activeForceFields[i];
            if (forceField == null || forceField.target == null || forceField.target.Destroyed)
            {
                activeForceFields.RemoveAt(i);
                continue;
            }

            if (forceField.target == target && forceField.caster == caster && forceField.spellDef == spellDef)
            {
                activeForceFields.RemoveAt(i);
                CleanupForceField(forceField);
                removeRecords.Add(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Remove, "replaced by recast"));
            }
        }

        for (int i = 0; i < removeRecords.Count; i++)
        {
            RunForceFieldLifecycleActions(removeRecords[i]);
        }
    }

    private void CleanupExpiredForceFields(int currentTick)
    {
        if (activeForceFields == null)
        {
            return;
        }

        List<ForceFieldLifecycleRecord> lifecycleRecords = new();
        for (int i = activeForceFields.Count - 1; i >= 0; i--)
        {
            ActiveSpellForceField forceField = activeForceFields[i];
            bool remove = forceField == null
                || forceField.target == null
                || forceField.target.Destroyed;

            if (!remove && forceField.IsExpired(currentTick))
            {
                remove = true;
                lifecycleRecords.Add(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Expire, "duration expired"));
            }

            if (!remove && TryGetForceFieldBreakReason(forceField, out string breakReason))
            {
                remove = true;
                lifecycleRecords.Add(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Break, breakReason));
            }

            if (remove)
            {
                activeForceFields.RemoveAt(i);
                CleanupForceField(forceField);
            }
        }

        for (int i = 0; i < lifecycleRecords.Count; i++)
        {
            RunForceFieldLifecycleActions(lifecycleRecords[i]);
        }
    }

    private static bool TryGetForceFieldBreakReason(ActiveSpellForceField forceField, out string reason)
    {
        reason = null;
        if (forceField == null)
        {
            return true;
        }

        if (forceField.maintenance?.profiles != null && forceField.maintenance.profiles.Count > 0)
        {
            Map map = forceField.target?.MapHeld ?? forceField.caster?.MapHeld;
            IntVec3 anchorCell = forceField.target != null && !forceField.target.Destroyed ? forceField.target.Position : IntVec3.Invalid;
            return SpellMaintenanceUtility.IsMaintenanceBroken(forceField.maintenance, forceField.caster, forceField.target, map, anchorCell, out reason);
        }

        if (forceField.caster == null || forceField.caster.Destroyed)
        {
            reason = "caster invalid";
            return true;
        }

        if (forceField.target == null || forceField.target.Destroyed)
        {
            reason = "target invalid";
            return true;
        }

        Pawn casterPawn = forceField.caster as Pawn;
        Pawn targetPawn = forceField.target as Pawn;
        if (casterPawn != null && (casterPawn.Dead || (forceField.breakWhenCasterDowned && casterPawn.Downed)))
        {
            reason = casterPawn.Dead ? "caster dead" : "caster downed";
            return true;
        }

        if (targetPawn != null && (targetPawn.Dead || (forceField.breakWhenTargetDowned && targetPawn.Downed)))
        {
            reason = targetPawn.Dead ? "target dead" : "target downed";
            return true;
        }

        Map casterMap = forceField.caster.MapHeld;
        Map targetMap = forceField.target.MapHeld;
        if (casterMap == null || targetMap == null || casterMap != targetMap)
        {
            reason = "caster and target are not on the same map";
            return true;
        }

        if (forceField.breakWhenTargetOutOfRange && forceField.maxRange > 0f
            && forceField.caster.Position.DistanceTo(forceField.target.Position) > forceField.maxRange)
        {
            reason = "target out of range";
            return true;
        }

        if (forceField.breakWhenLineOfSightLost
            && !GenSight.LineOfSight(forceField.caster.Position, forceField.target.Position, casterMap))
        {
            reason = "line of sight lost";
            return true;
        }

        return false;
    }

    private void CleanupForceField(ActiveSpellForceField forceField)
    {
        if (forceField?.removeIndicatorOnExpire != true || forceField.indicatorHediffDef == null || forceField.target is not Pawn pawn || pawn.health == null)
        {
            return;
        }

        Hediff existingIndicator = pawn.health.hediffSet?.GetFirstHediffOfDef(forceField.indicatorHediffDef);
        if (existingIndicator != null)
        {
            pawn.health.RemoveHediff(existingIndicator);
        }
    }

    private void TickForceFieldSustainedManaCosts(int currentTick)
    {
        if (activeForceFields == null || activeForceFields.Count == 0)
        {
            return;
        }

        List<ForceFieldLifecycleRecord> breakRecords = new();
        for (int i = activeForceFields.Count - 1; i >= 0; i--)
        {
            ActiveSpellForceField forceField = activeForceFields[i];
            if (forceField == null || forceField.sustainedManaCost <= 0f)
            {
                continue;
            }

            int intervalTicks = Mathf.Max(1, forceField.sustainedManaCostIntervalTicks);
            if (forceField.nextSustainedManaCostTick <= 0)
            {
                forceField.nextSustainedManaCostTick = currentTick + intervalTicks;
                continue;
            }

            if (currentTick < forceField.nextSustainedManaCostTick)
            {
                continue;
            }

            if (!HasEnoughMana(forceField.caster, forceField.sustainedManaCost))
            {
                activeForceFields.RemoveAt(i);
                CleanupForceField(forceField);
                breakRecords.Add(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Break, "insufficient mana for upkeep"));
                continue;
            }

            SpendMana(forceField.caster, forceField.sustainedManaCost);
            forceField.nextSustainedManaCostTick = currentTick + intervalTicks;
            MagicLog.Message(MagicLogSubsystem.ForceFields, $"[MagicFramework] Sustained force field spent {forceField.sustainedManaCost:0.##} mana for upkeep.");
        }

        for (int i = 0; i < breakRecords.Count; i++)
        {
            RunForceFieldLifecycleActions(breakRecords[i]);
        }
    }

    private static void EnsureForceFieldIndicatorApplied(ActiveSpellForceField forceField)
    {
        if (forceField?.indicatorHediffDef == null || forceField.target is not Pawn pawn || pawn.health == null)
        {
            return;
        }

        Hediff existingIndicator = pawn.health.hediffSet?.GetFirstHediffOfDef(forceField.indicatorHediffDef);
        if (existingIndicator == null)
        {
            existingIndicator = HediffMaker.MakeHediff(forceField.indicatorHediffDef, pawn);
            pawn.health.AddHediff(existingIndicator);
        }

        if (existingIndicator is SpellStatusCueHediff statusCueHediff)
        {
            statusCueHediff.statusLabel = forceField.statusCueLabel;
            statusCueHediff.statusDescription = forceField.statusCueDescription;
        }

        if (forceField.indicatorSeverity > 0f && existingIndicator.Severity < forceField.indicatorSeverity)
        {
            existingIndicator.Severity = forceField.indicatorSeverity;
        }
    }

    private static void SpawnForceFieldImpact(ActiveSpellForceField forceField, float scale)
    {
        if (forceField?.target == null || forceField.target.MapHeld == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(forceField.impactFleckDef))
        {
            FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(forceField.impactFleckDef);
            if (fleckDef != null)
            {
                FleckMaker.Static(forceField.target.DrawPos, forceField.target.MapHeld, fleckDef, scale);
            }
        }

        if (!string.IsNullOrWhiteSpace(forceField.impactSoundDef))
        {
            SoundDef soundDef = DefDatabase<SoundDef>.GetNamedSilentFail(forceField.impactSoundDef);
            if (soundDef != null)
            {
                SoundStarter.PlayOneShot(soundDef, new TargetInfo(forceField.target));
            }
        }
    }

    private static void TickForceFieldAmbientVisuals(int currentTick)
    {
        List<ActiveSpellForceField> forceFields = Instance?.activeForceFields;
        if (forceFields == null || forceFields.Count == 0)
        {
            return;
        }

        for (int i = 0; i < forceFields.Count; i++)
        {
            ActiveSpellForceField forceField = forceFields[i];
            if (forceField == null || currentTick < forceField.nextAmbientFleckTick)
            {
                continue;
            }

            int interval = Mathf.Max(1, forceField.ambientFleckIntervalTicks);
            forceField.nextAmbientFleckTick = currentTick + interval;
            SpawnForceFieldAmbientFleck(forceField);
        }
    }

    private static void SpawnForceFieldAmbientFleck(ActiveSpellForceField forceField)
    {
        if (forceField?.target == null || forceField.target.MapHeld == null || string.IsNullOrWhiteSpace(forceField.ambientFleckDef))
        {
            return;
        }

        FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(forceField.ambientFleckDef);
        if (fleckDef == null)
        {
            return;
        }

        Vector3 drawPos = forceField.target.DrawPos;
        Map map = forceField.target.MapHeld;
        float scale = Mathf.Max(0.1f, forceField.ambientFleckScale);
        if (TryResolveColor(forceField.ambientColorHex, out Color color))
        {
            FleckCreationData data = FleckMaker.GetDataStatic(drawPos, map, fleckDef, scale);
            data.instanceColor = color;
            map.flecks.CreateFleck(data);
            return;
        }

        FleckMaker.Static(drawPos, map, fleckDef, scale);
    }

    private static bool TryResolveColor(string colorHex, out Color color)
    {
        color = Color.white;
        if (string.IsNullOrWhiteSpace(colorHex))
        {
            return false;
        }

        string normalized = colorHex.Trim();
        if (!normalized.StartsWith("#"))
        {
            normalized = "#" + normalized;
        }

        return ColorUtility.TryParseHtmlString(normalized, out color);
    }

    private static Material ResolveForceFieldOverlayMaterial(ActiveSpellForceField forceField)
    {
        string texturePath = forceField?.sustainedOverlayTexturePath;
        if (string.IsNullOrWhiteSpace(texturePath))
        {
            return null;
        }

        Color color = new(0.65f, 0.85f, 1f, 0.45f);
        TryResolveColor(forceField.sustainedOverlayColorHex, out color);
        string key = texturePath + "|" + ColorUtility.ToHtmlStringRGBA(color);
        if (ForceFieldOverlayMaterials.TryGetValue(key, out Material material) && material != null)
        {
            return material;
        }

        Texture2D texture = ContentFinder<Texture2D>.Get(texturePath, false);
        if (texture == null)
        {
            ForceFieldOverlayMaterials[key] = null;
            return null;
        }

        material = MaterialPool.MatFrom(texture, ShaderDatabase.Transparent, color);
        ForceFieldOverlayMaterials[key] = material;
        return material;
    }

    private static bool MatchesClearScope(
        ActiveSpellStatModifier modifier,
        Thing target,
        Thing caster,
        SpellDef currentSpellDef,
        ClearStatModifierScope scope,
        SpellDef specificSpellDef,
        HediffDef statusHediffDef)
    {
        if (modifier?.target != target)
        {
            return false;
        }

        return scope switch
        {
            ClearStatModifierScope.FromCurrentCaster => modifier.caster == caster,
            ClearStatModifierScope.FromCurrentSpell => modifier.spellDef == currentSpellDef,
            ClearStatModifierScope.FromSpecificSpell => modifier.spellDef == specificSpellDef,
            ClearStatModifierScope.WithStatusHediff => modifier.indicatorHediffDef == statusHediffDef,
            _ => true
        };
    }

    private void CleanupExpiredStatModifiers(int currentTick)
    {
        if (activeStatModifiers == null || cleaningStatModifiers)
        {
            return;
        }

        cleaningStatModifiers = true;
        List<SustainedBreakRecord> breakRecords = new();
        try
        {
            for (int i = activeStatModifiers.Count - 1; i >= 0; i--)
            {
                ActiveSpellStatModifier modifier = activeStatModifiers[i];
                if (TryGetRemovalReason(modifier, currentTick, out string breakReason, out bool runBreakActions))
                {
                    activeStatModifiers.RemoveAt(i);
                    CleanupModifier(modifier);
                    if (runBreakActions)
                    {
                        breakRecords.Add(new SustainedBreakRecord(modifier, breakReason));
                    }
                }
            }
        }
        finally
        {
            cleaningStatModifiers = false;
        }

        for (int i = 0; i < breakRecords.Count; i++)
        {
            RunSustainedBreakActions(breakRecords[i]);
        }
    }

    private static bool TryGetRemovalReason(ActiveSpellStatModifier modifier, int currentTick, out string breakReason, out bool runBreakActions)
    {
        breakReason = null;
        runBreakActions = false;

        if (modifier == null)
        {
            return true;
        }

        if (modifier.IsExpired(currentTick))
        {
            return true;
        }

        if (TryGetSustainedBreakReason(modifier, out breakReason))
        {
            runBreakActions = modifier.isSustained;
            return true;
        }

        return false;
    }

    private static bool TryGetSustainedBreakReason(ActiveSpellStatModifier modifier, out string reason)
    {
        reason = null;
        if (modifier?.isSustained != true)
        {
            return false;
        }

        if (modifier.maintenance?.profiles != null && modifier.maintenance.profiles.Count > 0)
        {
            Map map = modifier.target?.MapHeld ?? modifier.caster?.MapHeld;
            IntVec3 anchorCell = modifier.target != null && !modifier.target.Destroyed ? modifier.target.Position : IntVec3.Invalid;
            return SpellMaintenanceUtility.IsMaintenanceBroken(modifier.maintenance, modifier.caster, modifier.target, map, anchorCell, out reason);
        }

        if (modifier.caster == null || modifier.caster.Destroyed)
        {
            reason = "caster invalid";
            return true;
        }

        if (modifier.target == null || modifier.target.Destroyed)
        {
            reason = "target invalid";
            return true;
        }

        Pawn casterPawn = modifier.caster as Pawn;
        Pawn targetPawn = modifier.target as Pawn;
        if (casterPawn != null && casterPawn.Dead)
        {
            reason = "caster dead";
            return true;
        }

        if (casterPawn != null && modifier.breakWhenCasterDowned && casterPawn.Downed)
        {
            reason = "caster downed";
            return true;
        }

        if (targetPawn != null && targetPawn.Dead)
        {
            reason = "target dead";
            return true;
        }

        if (targetPawn != null && modifier.breakWhenTargetDowned && targetPawn.Downed)
        {
            reason = "target downed";
            return true;
        }

        Map casterMap = modifier.caster.MapHeld;
        Map targetMap = modifier.target.MapHeld;
        if (casterMap == null || targetMap == null || casterMap != targetMap)
        {
            reason = "caster and target are not on the same map";
            return true;
        }

        if (modifier.breakWhenTargetOutOfRange && modifier.maxRange > 0f
            && modifier.caster.Position.DistanceTo(modifier.target.Position) > modifier.maxRange)
        {
            reason = "target out of range";
            return true;
        }

        if (modifier.breakWhenLineOfSightLost
            && !GenSight.LineOfSight(modifier.caster.Position, modifier.target.Position, casterMap))
        {
            reason = "line of sight lost";
            return true;
        }

        return false;
    }

    private void CleanupModifier(ActiveSpellStatModifier modifier)
    {
        if (modifier?.removeIndicatorOnExpire != true || modifier.indicatorHediffDef == null || modifier.target is not Pawn pawn || pawn.health == null)
        {
            return;
        }

        if (HasOtherActiveIndicator(pawn, modifier))
        {
            return;
        }

        Hediff existingIndicator = FindMatchingIndicator(pawn, modifier);
        if (existingIndicator != null)
        {
            pawn.health.RemoveHediff(existingIndicator);
        }
    }

    private bool HasOtherActiveIndicator(Pawn pawn, ActiveSpellStatModifier removedModifier)
    {
        if (activeStatModifiers == null)
        {
            return false;
        }

        for (int i = 0; i < activeStatModifiers.Count; i++)
        {
            ActiveSpellStatModifier modifier = activeStatModifiers[i];
            if (modifier?.target == pawn
                && modifier.indicatorHediffDef == removedModifier.indicatorHediffDef
                && modifier.statusCueLabel == removedModifier.statusCueLabel)
            {
                return true;
            }
        }

        return false;
    }

    private static void EnsureIndicatorApplied(ActiveSpellStatModifier modifier)
    {
        if (modifier?.indicatorHediffDef == null || modifier.target is not Pawn pawn || pawn.health == null)
        {
            return;
        }

        Hediff existingIndicator = FindMatchingIndicator(pawn, modifier);
        if (existingIndicator == null)
        {
            existingIndicator = HediffMaker.MakeHediff(modifier.indicatorHediffDef, pawn);
            pawn.health.AddHediff(existingIndicator);
        }

        if (existingIndicator is SpellStatusCueHediff statusCueHediff)
        {
            statusCueHediff.statusLabel = modifier.statusCueLabel;
            statusCueHediff.statusDescription = modifier.statusCueDescription;
        }

        if (modifier.indicatorSeverity > 0f && existingIndicator.Severity < modifier.indicatorSeverity)
        {
            existingIndicator.Severity = modifier.indicatorSeverity;
        }
    }

    private static HediffDef ResolveStatusCueHediffDef(SpellStatusCueDef statusCue)
    {
        if (statusCue == null)
        {
            return null;
        }

        string hediffDefName = string.IsNullOrWhiteSpace(statusCue.hediffDef)
            ? "MF_GenericSpellStatusCue"
            : statusCue.hediffDef;

        HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffDefName);
        if (hediffDef == null)
        {
            Log.Warning($"[MagicFramework] Could not resolve hediff def '{hediffDefName}' for spell status cue.");
        }

        return hediffDef;
    }

    private static Hediff FindMatchingIndicator(Pawn pawn, ActiveSpellStatModifier modifier)
    {
        if (pawn?.health?.hediffSet?.hediffs == null || modifier?.indicatorHediffDef == null)
        {
            return null;
        }

        for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
        {
            Hediff hediff = pawn.health.hediffSet.hediffs[i];
            if (hediff?.def != modifier.indicatorHediffDef)
            {
                continue;
            }

            if (hediff is SpellStatusCueHediff statusCueHediff
                && !string.IsNullOrWhiteSpace(modifier.statusCueLabel)
                && statusCueHediff.statusLabel != modifier.statusCueLabel)
            {
                continue;
            }

            return hediff;
        }

        return null;
    }

    private static string ResolveStatusCueLabel(SpellStatusCueDef statusCue, SpellDef spellDef)
    {
        if (!string.IsNullOrWhiteSpace(statusCue?.label))
        {
            return statusCue.label;
        }

        string spellLabel = spellDef?.LabelCap ?? spellDef?.defName ?? "spell";
        return $"Affected by {spellLabel}";
    }

    private static string ResolveStatusCueDescription(SpellStatusCueDef statusCue, SpellDef spellDef)
    {
        if (!string.IsNullOrWhiteSpace(statusCue?.description))
        {
            return statusCue.description;
        }

        string spellLabel = spellDef?.LabelCap ?? spellDef?.defName ?? "spell";
        return $"This pawn is affected by {spellLabel}.";
    }

    private static void TickSustainedStatModifierPulses(int currentTick)
    {
        List<ActiveSpellStatModifier> modifiers = Instance?.activeStatModifiers;
        if (modifiers == null || modifiers.Count == 0)
        {
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            ActiveSpellStatModifier modifier = modifiers[i];
            if (modifier?.isSustained != true || modifier.pulseIntervalTicks <= 0)
            {
                continue;
            }

            if (modifier.nextPulseTick <= 0)
            {
                modifier.nextPulseTick = currentTick + Mathf.Max(1, modifier.pulseIntervalTicks);
                continue;
            }

            if (currentTick < modifier.nextPulseTick)
            {
                continue;
            }

            modifier.nextPulseTick = currentTick + Mathf.Max(1, modifier.pulseIntervalTicks);
            RunSustainedPulseActions(modifier);
        }
    }

    private static void TickForceFieldPulses(int currentTick)
    {
        List<ActiveSpellForceField> forceFields = Instance?.activeForceFields;
        if (forceFields == null || forceFields.Count == 0)
        {
            return;
        }

        for (int i = 0; i < forceFields.Count; i++)
        {
            ActiveSpellForceField forceField = forceFields[i];
            if (forceField == null || forceField.pulseIntervalTicks <= 0)
            {
                continue;
            }

            if (forceField.nextPulseTick <= 0)
            {
                forceField.nextPulseTick = currentTick + Mathf.Max(1, forceField.pulseIntervalTicks);
                continue;
            }

            if (currentTick < forceField.nextPulseTick)
            {
                continue;
            }

            forceField.nextPulseTick = currentTick + Mathf.Max(1, forceField.pulseIntervalTicks);
            RunForceFieldLifecycleActions(new ForceFieldLifecycleRecord(forceField, ForceFieldLifecycleEvent.Pulse));
        }
    }

    private static void RunSustainedPulseActions(ActiveSpellStatModifier modifier)
    {
        if (modifier?.spellDef == null
            || modifier.sourceActionPath == null
            || modifier.sourceActionPath.Count == 0
            || SpellActionPathUtility.ResolveAction(modifier.spellDef, modifier.sourceActionPath) is not SustainedStatModifierActionDef sourceAction
            || sourceAction.onPulseActions == null
            || sourceAction.onPulseActions.Count == 0)
        {
            return;
        }

        if (!TryCreateMaintainedContext(modifier.caster, modifier.target, modifier.spellDef, out SpellContext context))
        {
            return;
        }

        new SpellActionRunner().RunActions(context, sourceAction.onPulseActions);
    }

    private static void RunSustainedBreakActions(SustainedBreakRecord breakRecord)
    {
        ActiveSpellStatModifier modifier = breakRecord?.modifier;
        if (modifier?.isSustained != true || modifier.spellDef == null)
        {
            return;
        }

        MagicLog.Message(MagicLogSubsystem.StatModifiers, $"[MagicFramework] Sustained effect {modifier.spellDef.defName ?? "<unknown spell>"} broke: {breakRecord.reason ?? "unknown reason"}.");

        if (modifier.sourceActionPath == null
            || modifier.sourceActionPath.Count == 0
            || SpellActionPathUtility.ResolveAction(modifier.spellDef, modifier.sourceActionPath) is not SustainedStatModifierActionDef sourceAction
            || sourceAction.onBreakActions == null
            || sourceAction.onBreakActions.Count == 0)
        {
            return;
        }

        if (!TryCreateMaintainedContext(modifier.caster, modifier.target, modifier.spellDef, out SpellContext context))
        {
            return;
        }

        new SpellActionRunner().RunActions(context, sourceAction.onBreakActions);
    }

    private static void RunForceFieldLifecycleActions(ForceFieldLifecycleRecord lifecycleRecord)
    {
        ActiveSpellForceField forceField = lifecycleRecord?.forceField;
        if (forceField?.spellDef == null)
        {
            return;
        }

        if (lifecycleRecord.lifecycleEvent == ForceFieldLifecycleEvent.Break)
        {
            MagicLog.Message(MagicLogSubsystem.ForceFields, $"[MagicFramework] Force field {forceField.spellDef.defName ?? "<unknown spell>"} broke: {lifecycleRecord.reason ?? "unknown reason"}.");
        }

        if (forceField.sourceActionPath == null
            || forceField.sourceActionPath.Count == 0
            || SpellActionPathUtility.ResolveAction(forceField.spellDef, forceField.sourceActionPath) is not ApplyForceFieldActionDef sourceAction)
        {
            return;
        }

        List<SpellActionDef> actions = lifecycleRecord.lifecycleEvent switch
        {
            ForceFieldLifecycleEvent.Create => sourceAction.onCreateActions,
            ForceFieldLifecycleEvent.Pulse => sourceAction.onPulseActions,
            ForceFieldLifecycleEvent.Expire => sourceAction.onExpireActions,
            ForceFieldLifecycleEvent.Remove => sourceAction.onRemoveActions,
            ForceFieldLifecycleEvent.Break => sourceAction.onBreakActions,
            _ => null
        };

        if (actions == null || actions.Count == 0)
        {
            return;
        }

        if (!TryCreateMaintainedContext(forceField.caster, forceField.target, forceField.spellDef, out SpellContext context))
        {
            return;
        }

        new SpellActionRunner().RunActions(context, actions);
    }

    private static bool TryCreateMaintainedContext(Thing caster, Thing target, SpellDef spellDef, out SpellContext context)
    {
        context = null;
        Map map = target?.MapHeld ?? caster?.MapHeld;
        if (map == null)
        {
            return false;
        }

        LocalTargetInfo targetInfo = target != null && !target.Destroyed
            ? new LocalTargetInfo(target)
            : LocalTargetInfo.Invalid;
        IntVec3 currentCell = targetInfo.IsValid
            ? targetInfo.Cell
            : caster?.Position ?? IntVec3.Invalid;

        context = new SpellContext
        {
            caster = caster,
            map = map,
            spellDef = spellDef,
            initialTarget = targetInfo,
            currentTarget = targetInfo,
            currentCell = currentCell,
            randomSeed = Find.TickManager?.TicksGame ?? 0
        };
        context.executionState.costsApplied = true;
        if (targetInfo.IsValid)
        {
            context.currentTargets.Add(targetInfo);
        }

        return true;
    }

    private sealed class SustainedBreakRecord
    {
        public readonly ActiveSpellStatModifier modifier;
        public readonly string reason;

        public SustainedBreakRecord(ActiveSpellStatModifier modifier, string reason)
        {
            this.modifier = modifier;
            this.reason = reason;
        }
    }

    private sealed class ForceFieldLifecycleRecord
    {
        public readonly ActiveSpellForceField forceField;
        public readonly ForceFieldLifecycleEvent lifecycleEvent;
        public readonly string reason;

        public ForceFieldLifecycleRecord(ActiveSpellForceField forceField, ForceFieldLifecycleEvent lifecycleEvent, string reason = null)
        {
            this.forceField = forceField;
            this.lifecycleEvent = lifecycleEvent;
            this.reason = reason;
        }
    }

    private enum ForceFieldLifecycleEvent
    {
        Create,
        Pulse,
        Expire,
        Remove,
        Break
    }

    private sealed class CasterRuntimeState : IExposable
    {
        public Thing caster;
        public float currentMana;
        public bool hasArcaneGift;
        public int casterLevel;
        public int debugCasterLevel;
        private List<SpellCooldownEntry> cooldowns = new();
        private List<string> knownSpellDefNames = new();

        public IEnumerable<string> KnownSpellDefNames => knownSpellDefNames ?? new List<string>();

        public bool KnowsSpell(string spellDefName)
        {
            return !string.IsNullOrWhiteSpace(spellDefName)
                && knownSpellDefNames != null
                && knownSpellDefNames.Contains(spellDefName);
        }

        public bool LearnSpell(string spellDefName)
        {
            if (string.IsNullOrWhiteSpace(spellDefName))
            {
                return false;
            }

            knownSpellDefNames ??= new List<string>();
            if (knownSpellDefNames.Contains(spellDefName))
            {
                return false;
            }

            knownSpellDefNames.Add(spellDefName);
            return true;
        }

        public bool ForgetSpell(string spellDefName)
        {
            return !string.IsNullOrWhiteSpace(spellDefName)
                && knownSpellDefNames != null
                && knownSpellDefNames.Remove(spellDefName);
        }

        public int GetCooldownReadyTick(string spellDefName)
        {
            SpellCooldownEntry entry = FindCooldown(spellDefName);
            return entry?.readyTick ?? 0;
        }

        public void SetCooldownReadyTick(string spellDefName, int readyTick)
        {
            if (string.IsNullOrWhiteSpace(spellDefName))
            {
                return;
            }

            SpellCooldownEntry entry = FindCooldown(spellDefName);
            if (entry == null)
            {
                entry = new SpellCooldownEntry { spellDefName = spellDefName };
                cooldowns.Add(entry);
            }

            entry.readyTick = readyTick;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref caster, "caster");
            Scribe_Values.Look(ref currentMana, "currentMana", DefaultStartingMana);
            Scribe_Values.Look(ref hasArcaneGift, "hasArcaneGift");
            Scribe_Values.Look(ref casterLevel, "casterLevel");
            Scribe_Values.Look(ref debugCasterLevel, "debugCasterLevel");
            Scribe_Collections.Look(ref cooldowns, "cooldowns", LookMode.Deep);
            Scribe_Collections.Look(ref knownSpellDefNames, "knownSpellDefNames", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && cooldowns == null)
            {
                cooldowns = new List<SpellCooldownEntry>();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit && knownSpellDefNames == null)
            {
                knownSpellDefNames = new List<string>();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit && casterLevel == 0 && debugCasterLevel > 0)
            {
                casterLevel = debugCasterLevel;
            }
        }

        private SpellCooldownEntry FindCooldown(string spellDefName)
        {
            for (int i = 0; i < cooldowns.Count; i++)
            {
                SpellCooldownEntry entry = cooldowns[i];
                if (entry != null && entry.spellDefName == spellDefName)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    private sealed class SpellCooldownEntry : IExposable
    {
        public string spellDefName;
        public int readyTick;

        public void ExposeData()
        {
            Scribe_Values.Look(ref spellDefName, "spellDefName");
            Scribe_Values.Look(ref readyTick, "readyTick");
        }
    }
}
