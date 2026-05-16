using System.Collections.Generic;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using MagicFramework.Scheduling;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

public sealed class CompProperties_MagicItemAbilities : CompProperties
{
    public List<MagicItemAbilityDef> abilities = new();
    public List<MagicItemPassiveStatusDef> passiveStatuses = new();
    public List<MagicItemMeleeTriggerDef> meleeTriggers = new();
    public List<MagicItemDamageResistanceDef> damageResistances = new();
    public float fireAttachChanceFactor = 1f;
    public bool extinguishOwnerWhenBurning;

    public CompProperties_MagicItemAbilities()
    {
        compClass = typeof(CompMagicItemAbilities);
    }
}

public sealed class MagicItemAbilityDef
{
    public SpellDef spell;
    public string label;
    public string description;
    public string iconPath;
    public int cooldownTicks;
    public bool requireEquipped = true;
    public bool requireArcaneGift;
    public bool consumeOnUse;
}

public sealed class MagicItemPassiveStatusDef
{
    public string statusEffectDef;
    public bool requireEquipped = true;
    public bool requireArcaneGift;
    public int refreshIntervalTicks = 120;
}

public sealed class MagicItemMeleeTriggerDef
{
    public string label;
    public float chance = 1f;
    public int cooldownTicks;
    public List<SpellActionDef> actions = new();
}

public sealed class MagicItemDamageResistanceDef
{
    public string damageDef;
    public float damageFactor = 1f;
    public List<string> preventedHediffs = new();
    public bool requireEquipped = true;
}

public sealed class CompMagicItemAbilities : ThingComp
{
    private List<MagicItemAbilityCooldown> cooldowns = new();

    public CompProperties_MagicItemAbilities Props => (CompProperties_MagicItemAbilities)props;

    public IEnumerable<MagicItemAbilityDef> Abilities => Props?.abilities ?? new List<MagicItemAbilityDef>();

    public IEnumerable<MagicItemPassiveStatusDef> PassiveStatuses => Props?.passiveStatuses ?? new List<MagicItemPassiveStatusDef>();

    public IEnumerable<MagicItemMeleeTriggerDef> MeleeTriggers => Props?.meleeTriggers ?? new List<MagicItemMeleeTriggerDef>();

    public IEnumerable<MagicItemDamageResistanceDef> DamageResistances => Props?.damageResistances ?? new List<MagicItemDamageResistanceDef>();

    public float FireAttachChanceFactor => Props?.fireAttachChanceFactor ?? 1f;

    public bool ExtinguishOwnerWhenBurning => Props?.extinguishOwnerWhenBurning == true;

    public int GetCooldownRemainingTicks(MagicItemAbilityDef ability)
    {
        if (ability?.spell == null)
        {
            return 0;
        }

        int readyTick = FindCooldown(AbilityKey(ability))?.readyTick ?? 0;
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        return readyTick > currentTick ? readyTick - currentTick : 0;
    }

    public void StartCooldown(MagicItemAbilityDef ability)
    {
        if (ability?.spell == null || ability.cooldownTicks <= 0)
        {
            return;
        }

        string key = AbilityKey(ability);
        MagicItemAbilityCooldown cooldown = FindCooldown(key);
        if (cooldown == null)
        {
            cooldown = new MagicItemAbilityCooldown { abilityKey = key };
            cooldowns ??= new List<MagicItemAbilityCooldown>();
            cooldowns.Add(cooldown);
        }

        cooldown.readyTick = (Find.TickManager?.TicksGame ?? 0) + ability.cooldownTicks;
    }

    public int GetMeleeTriggerCooldownRemainingTicks(MagicItemMeleeTriggerDef trigger)
    {
        string key = MeleeTriggerKey(trigger);
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }

        int readyTick = FindCooldown(key)?.readyTick ?? 0;
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        return readyTick > currentTick ? readyTick - currentTick : 0;
    }

    public void StartMeleeTriggerCooldown(MagicItemMeleeTriggerDef trigger)
    {
        if (trigger == null || trigger.cooldownTicks <= 0)
        {
            return;
        }

        string key = MeleeTriggerKey(trigger);
        MagicItemAbilityCooldown cooldown = FindCooldown(key);
        if (cooldown == null)
        {
            cooldown = new MagicItemAbilityCooldown { abilityKey = key };
            cooldowns ??= new List<MagicItemAbilityCooldown>();
            cooldowns.Add(cooldown);
        }

        cooldown.readyTick = (Find.TickManager?.TicksGame ?? 0) + trigger.cooldownTicks;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref cooldowns, "magicItemAbilityCooldowns", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && cooldowns == null)
        {
            cooldowns = new List<MagicItemAbilityCooldown>();
        }
    }

    private MagicItemAbilityCooldown FindCooldown(string abilityKey)
    {
        if (string.IsNullOrWhiteSpace(abilityKey) || cooldowns == null)
        {
            return null;
        }

        for (int i = 0; i < cooldowns.Count; i++)
        {
            MagicItemAbilityCooldown cooldown = cooldowns[i];
            if (cooldown?.abilityKey == abilityKey)
            {
                return cooldown;
            }
        }

        return null;
    }

    private static string AbilityKey(MagicItemAbilityDef ability)
    {
        return ability?.spell?.defName ?? "<missing>";
    }

    private static string MeleeTriggerKey(MagicItemMeleeTriggerDef trigger)
    {
        return "melee:" + (trigger?.label ?? "trigger");
    }
}

public sealed class MagicItemAbilityCooldown : IExposable
{
    public string abilityKey;
    public int readyTick;

    public void ExposeData()
    {
        Scribe_Values.Look(ref abilityKey, "abilityKey");
        Scribe_Values.Look(ref readyTick, "readyTick");
    }
}

public sealed class MagicItemPassiveStatusGameComponent : GameComponent
{
    private const int ScanIntervalTicks = 120;
    private List<ActiveMagicItemPassiveStatus> activeStatuses = new();

    public MagicItemPassiveStatusGameComponent(Game game)
    {
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        if (currentTick % ScanIntervalTicks != 0)
        {
            return;
        }

        CleanupInactiveStatuses();
        foreach (Map map in Find.Maps)
        {
            List<Pawn> pawns = map?.mapPawns?.FreeColonistsSpawned;
            if (pawns == null)
            {
                continue;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                RefreshPawnStatuses(pawns[i], currentTick);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref activeStatuses, "activeMagicItemPassiveStatuses", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && activeStatuses == null)
        {
            activeStatuses = new List<ActiveMagicItemPassiveStatus>();
        }
    }

    private void RefreshPawnStatuses(Pawn pawn, int currentTick)
    {
        if (pawn == null || pawn.Destroyed || pawn.Dead)
        {
            return;
        }

        foreach (Thing item in MagicItemUtility.EquippedWornAndCarriedItems(pawn))
        {
            CompMagicItemAbilities comp = item?.TryGetComp<CompMagicItemAbilities>();
            if (comp == null)
            {
                continue;
            }

            if (comp.ExtinguishOwnerWhenBurning && MagicItemUtility.IsEquippedOrWornBy(pawn, item))
            {
                MagicItemUtility.ExtinguishAttachedFire(pawn, 9999f);
            }

            foreach (MagicItemPassiveStatusDef passiveStatus in comp.PassiveStatuses)
            {
                TryApplyPassiveStatus(pawn, item, passiveStatus, currentTick);
            }
        }
    }

    private void TryApplyPassiveStatus(Pawn pawn, Thing item, MagicItemPassiveStatusDef passiveStatus, int currentTick)
    {
        if (pawn == null || item == null || passiveStatus == null || string.IsNullOrWhiteSpace(passiveStatus.statusEffectDef))
        {
            return;
        }

        if (passiveStatus.requireEquipped && !MagicItemUtility.IsEquippedOrWornBy(pawn, item))
        {
            return;
        }

        if (passiveStatus.requireArcaneGift && SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) != true)
        {
            return;
        }

        SpellStatusEffectDef statusEffectDef = DefDatabase<SpellStatusEffectDef>.GetNamedSilentFail(passiveStatus.statusEffectDef);
        if (statusEffectDef == null)
        {
            Log.Warning($"[MagicFramework] Magic item passive status skipped because '{passiveStatus.statusEffectDef}' could not be resolved.");
            return;
        }

        if (statusEffectDef.statModifiers == null || statusEffectDef.statModifiers.Count == 0)
        {
            return;
        }

        int refreshInterval = Mathf.Max(30, passiveStatus.refreshIntervalTicks);
        int durationTicks = Mathf.Max(refreshInterval * 3, statusEffectDef.durationTicks);
        SpellRuntimeGameComponent.Instance?.ApplyStatModifiers(
            pawn,
            item,
            null,
            durationTicks,
            true,
            SpellStatusRefreshPolicy.RefreshDuration,
            statusEffectDef.statusCue,
            statusEffectDef.statModifiers);

        ActiveMagicItemPassiveStatus activeStatus = FindOrCreateActiveStatus(item, pawn, passiveStatus.statusEffectDef);
        activeStatus.lastSeenTick = currentTick;
    }

    private ActiveMagicItemPassiveStatus FindOrCreateActiveStatus(Thing item, Pawn pawn, string statusEffectDefName)
    {
        activeStatuses ??= new List<ActiveMagicItemPassiveStatus>();
        for (int i = 0; i < activeStatuses.Count; i++)
        {
            ActiveMagicItemPassiveStatus activeStatus = activeStatuses[i];
            if (activeStatus?.sourceItem == item && activeStatus.target == pawn && activeStatus.statusEffectDefName == statusEffectDefName)
            {
                return activeStatus;
            }
        }

        ActiveMagicItemPassiveStatus created = new()
        {
            sourceItem = item,
            target = pawn,
            statusEffectDefName = statusEffectDefName
        };
        activeStatuses.Add(created);
        return created;
    }

    private void CleanupInactiveStatuses()
    {
        if (activeStatuses == null || activeStatuses.Count == 0)
        {
            return;
        }

        int staleBeforeTick = (Find.TickManager?.TicksGame ?? 0) - ScanIntervalTicks * 2;
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            ActiveMagicItemPassiveStatus activeStatus = activeStatuses[i];
            if (activeStatus == null
                || activeStatus.sourceItem == null
                || activeStatus.target == null
                || activeStatus.target.Destroyed
                || activeStatus.target.Dead
                || activeStatus.lastSeenTick < staleBeforeTick
                || !MagicItemUtility.IsEquippedOrWornBy(activeStatus.target, activeStatus.sourceItem))
            {
                SpellRuntimeGameComponent.Instance?.ClearItemStatusEffects(activeStatus?.sourceItem, activeStatus?.target);
                activeStatuses.RemoveAt(i);
            }
        }
    }
}

public sealed class ActiveMagicItemPassiveStatus : IExposable
{
    public Thing sourceItem;
    public Pawn target;
    public string statusEffectDefName;
    public int lastSeenTick;

    public void ExposeData()
    {
        Scribe_References.Look(ref sourceItem, "sourceItem");
        Scribe_References.Look(ref target, "target");
        Scribe_Values.Look(ref statusEffectDefName, "statusEffectDefName");
        Scribe_Values.Look(ref lastSeenTick, "lastSeenTick");
    }
}

public static class MagicItemUtility
{
    private static readonly System.Reflection.FieldInfo AttachableParentField = AccessTools.Field(typeof(AttachableThing), "parent");

    public static IEnumerable<Thing> EquippedWornAndCarriedItems(Pawn pawn)
    {
        Thing primary = pawn?.equipment?.Primary;
        if (primary != null)
        {
            yield return primary;
        }

        List<Apparel> apparel = pawn?.apparel?.WornApparel;
        if (apparel != null)
        {
            for (int i = 0; i < apparel.Count; i++)
            {
                if (apparel[i] != null)
                {
                    yield return apparel[i];
                }
            }
        }

        ThingOwner<Thing> inventory = pawn?.inventory?.innerContainer;
        if (inventory != null)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] != null)
                {
                    yield return inventory[i];
                }
            }
        }
    }

    public static bool IsEquippedOrWornBy(Pawn pawn, Thing item)
    {
        if (pawn?.equipment?.Primary == item)
        {
            return true;
        }

        List<Apparel> apparel = pawn?.apparel?.WornApparel;
        return item is Apparel wornApparel && apparel != null && apparel.Contains(wornApparel);
    }

    public static float FireAttachChanceFactorFor(Pawn pawn)
    {
        if (pawn == null)
        {
            return 1f;
        }

        float factor = 1f;
        foreach (Thing item in EquippedWornAndCarriedItems(pawn))
        {
            CompMagicItemAbilities comp = item?.TryGetComp<CompMagicItemAbilities>();
            if (comp == null || !IsEquippedOrWornBy(pawn, item))
            {
                continue;
            }

            factor *= Mathf.Clamp01(comp.FireAttachChanceFactor);
        }

        return factor;
    }

    public static bool HasFireProtection(Pawn pawn)
    {
        return FireAttachChanceFactorFor(pawn) <= 0f;
    }

    public static float DamageFactorFor(Pawn pawn, DamageDef damageDef)
    {
        if (pawn == null || damageDef == null)
        {
            return 1f;
        }

        float factor = 1f;
        foreach (Thing item in EquippedWornAndCarriedItems(pawn))
        {
            CompMagicItemAbilities comp = item?.TryGetComp<CompMagicItemAbilities>();
            if (comp == null)
            {
                continue;
            }

            foreach (MagicItemDamageResistanceDef resistance in comp.DamageResistances)
            {
                if (!MatchesResistance(pawn, item, resistance) || !DefNameMatches(resistance.damageDef, damageDef.defName))
                {
                    continue;
                }

                factor *= Mathf.Max(0f, resistance.damageFactor);
            }
        }

        return factor;
    }

    public static bool PreventsHediff(Pawn pawn, HediffDef hediffDef)
    {
        if (pawn == null || hediffDef == null)
        {
            return false;
        }

        foreach (Thing item in EquippedWornAndCarriedItems(pawn))
        {
            CompMagicItemAbilities comp = item?.TryGetComp<CompMagicItemAbilities>();
            if (comp == null)
            {
                continue;
            }

            foreach (MagicItemDamageResistanceDef resistance in comp.DamageResistances)
            {
                if (!MatchesResistance(pawn, item, resistance) || resistance.preventedHediffs == null)
                {
                    continue;
                }

                for (int i = 0; i < resistance.preventedHediffs.Count; i++)
                {
                    if (DefNameMatches(resistance.preventedHediffs[i], hediffDef.defName))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static void ApplyDamageResistance(Pawn pawn, ref DamageInfo dinfo, ref bool absorbed)
    {
        DamageDef damageDef = dinfo.Def;
        float factor = DamageFactorFor(pawn, damageDef);
        if (factor >= 0.999f)
        {
            return;
        }

        if (factor <= 0f)
        {
            absorbed = true;
            dinfo.SetAmount(0f);
            return;
        }

        dinfo.SetAmount(dinfo.Amount * factor);
    }

    public static void ExtinguishAttachedFire(Pawn pawn, float damageAmount)
    {
        if (pawn?.Map == null)
        {
            return;
        }

        List<Thing> fires = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Fire);
        if (fires == null)
        {
            return;
        }

        for (int i = fires.Count - 1; i >= 0; i--)
        {
            if (fires[i] is Fire fire && AttachedParent(fire) == pawn)
            {
                fire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, damageAmount));
            }
        }
    }

    public static Thing AttachedParent(AttachableThing attachableThing)
    {
        return AttachableParentField?.GetValue(attachableThing) as Thing;
    }

    private static bool MatchesResistance(Pawn pawn, Thing item, MagicItemDamageResistanceDef resistance)
    {
        if (resistance == null)
        {
            return false;
        }

        return !resistance.requireEquipped || IsEquippedOrWornBy(pawn, item);
    }

    private static bool DefNameMatches(string configuredDefName, string actualDefName)
    {
        return !string.IsNullOrWhiteSpace(configuredDefName)
            && !string.IsNullOrWhiteSpace(actualDefName)
            && configuredDefName == actualDefName;
    }
}

[HarmonyPatch(typeof(Fire), nameof(Fire.AttachTo))]
public static class MagicItemFireAttachPatch
{
    public static void Postfix(Fire __instance, Thing newParent)
    {
        if (newParent is Pawn pawn && MagicItemUtility.FireAttachChanceFactorFor(pawn) <= 0f)
        {
            __instance?.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, 9999f));
        }
    }
}

[HarmonyPatch(typeof(FireUtility), nameof(FireUtility.ChanceToAttachFireFromEvent))]
public static class MagicItemFireEventChancePatch
{
    public static void Postfix(Thing t, ref float __result)
    {
        if (t is Pawn pawn)
        {
            __result *= MagicItemUtility.FireAttachChanceFactorFor(pawn);
        }
    }
}

[HarmonyPatch(typeof(FireUtility), nameof(FireUtility.ChanceToAttachFireCumulative))]
public static class MagicItemFireCumulativeChancePatch
{
    public static void Postfix(Thing t, ref float __result)
    {
        if (t is Pawn pawn)
        {
            __result *= MagicItemUtility.FireAttachChanceFactorFor(pawn);
        }
    }
}

[HarmonyPatch(typeof(Thing), nameof(Thing.PostApplyDamage))]
public static class MagicItemMeleeDamagePatch
{
    public static void Postfix(Thing __instance, DamageInfo dinfo, float totalDamageDealt)
    {
        if (totalDamageDealt <= 0f || __instance == null || dinfo.Instigator is not Pawn attacker)
        {
            return;
        }

        ThingWithComps weapon = attacker.equipment?.Primary;
        CompMagicItemAbilities comp = weapon?.TryGetComp<CompMagicItemAbilities>();
        if (weapon == null || comp == null || dinfo.Weapon != weapon.def || !weapon.def.IsMeleeWeapon)
        {
            return;
        }

        foreach (MagicItemMeleeTriggerDef trigger in comp.MeleeTriggers)
        {
            TryRunMeleeTrigger(attacker, __instance, weapon, comp, trigger);
        }
    }

    private static void TryRunMeleeTrigger(Pawn attacker, Thing target, Thing weapon, CompMagicItemAbilities comp, MagicItemMeleeTriggerDef trigger)
    {
        if (attacker?.Map == null || target == null || weapon == null || comp == null || trigger?.actions == null || trigger.actions.Count == 0)
        {
            return;
        }

        if (comp.GetMeleeTriggerCooldownRemainingTicks(trigger) > 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        bool passesChance = SpellDeterministicRandom.Chance(
            trigger.chance,
            currentTick,
            attacker,
            target,
            weapon,
            trigger.label ?? string.Empty);
        if (!passesChance)
        {
            return;
        }

        SpellContext context = new()
        {
            caster = attacker,
            sourceItem = weapon,
            map = attacker.Map,
            initialTarget = new LocalTargetInfo(target),
            currentTarget = new LocalTargetInfo(target),
            currentCell = target.Position,
            randomSeed = currentTick
        };
        context.currentTargets.Add(new LocalTargetInfo(target));
        new SpellActionRunner().RunActions(context, trigger.actions);
        comp.StartMeleeTriggerCooldown(trigger);
    }
}

public static class MagicItemGizmoUtility
{
    private static readonly SpellExecutor Executor = new();

    public static IEnumerable<Gizmo> CreateItemAbilityGizmos(Pawn pawn)
    {
        if (!CanUseItemAbilities(pawn))
        {
            yield break;
        }

        foreach (Thing item in MagicItemUtility.EquippedWornAndCarriedItems(pawn))
        {
            CompMagicItemAbilities comp = item?.TryGetComp<CompMagicItemAbilities>();
            if (comp == null)
            {
                continue;
            }

            foreach (MagicItemAbilityDef ability in comp.Abilities)
            {
                Gizmo gizmo = CreateItemAbilityGizmo(pawn, item, comp, ability);
                if (gizmo != null)
                {
                    yield return gizmo;
                }
            }
        }
    }

    private static Gizmo CreateItemAbilityGizmo(Pawn pawn, Thing item, CompMagicItemAbilities comp, MagicItemAbilityDef ability)
    {
        SpellDef spellDef = ability?.spell;
        if (pawn == null || item == null || comp == null || spellDef == null)
        {
            return null;
        }

        Command_Action command = new()
        {
            defaultLabel = string.IsNullOrWhiteSpace(ability.label) ? spellDef.LabelCap : ability.label,
            defaultDesc = BuildDescription(pawn, item, ability),
            icon = ResolveIcon(ability, spellDef),
            action = () => BeginTargeting(pawn, item, comp, ability)
        };

        if (!CanUseAbilityNow(pawn, item, comp, ability, out string reason))
        {
            command.Disable(reason);
        }

        return command;
    }

    private static string BuildDescription(Pawn pawn, Thing item, MagicItemAbilityDef ability)
    {
        string authoredDescription = ability.description;
        string spellDescription = SpellDescriptionUtility.GetGizmoDescription(ability.spell, pawn);
        string source = item?.LabelCap ?? "magic item";

        if (string.IsNullOrWhiteSpace(authoredDescription))
        {
            return $"{spellDescription}\n\nSource: {source}.";
        }

        return $"{authoredDescription}\n\n{spellDescription}\n\nSource: {source}.";
    }

    private static void BeginTargeting(Pawn pawn, Thing item, CompMagicItemAbilities comp, MagicItemAbilityDef ability)
    {
        if (!CanUseAbilityNow(pawn, item, comp, ability, out string reason))
        {
            Messages.Message(reason, pawn, MessageTypeDefOf.RejectInput, false);
            return;
        }

        SpellDef spellDef = ability.spell;
        if (spellDef.targeting?.useCasterAsTarget == true)
        {
            TryUseAbility(pawn, item, comp, ability, new LocalTargetInfo(pawn));
            return;
        }

        Find.Targeter.BeginTargeting(
            BuildTargetingParameters(pawn, spellDef),
            target => TryUseAbility(pawn, item, comp, ability, target),
            pawn,
            null,
            null,
            false);
    }

    private static void TryUseAbility(Pawn pawn, Thing item, CompMagicItemAbilities comp, MagicItemAbilityDef ability, LocalTargetInfo target)
    {
        SpellDef spellDef = ability.spell;
        SpellCastWarmupUtility.StartOrExecute(pawn, spellDef, target, Executor, (completed, context) =>
        {
            if (completed)
            {
                comp.StartCooldown(ability);
                Messages.Message($"Used {DisplayLabel(ability)}.", target.ToTargetInfo(pawn.Map), MessageTypeDefOf.TaskCompletion, false);

                if (ability.consumeOnUse && item.stackCount > 0)
                {
                    item.SplitOff(1).Destroy();
                }

                return;
            }

            string failureReason = context?.executionState?.failed == true
                ? context.executionState.failureReason ?? "Magic item ability failed."
                : "Magic item ability did not complete.";
            Messages.Message(failureReason, pawn, MessageTypeDefOf.RejectInput, false);
        }, context => context.sourceItem = item);
    }

    private static bool CanUseAbilityNow(Pawn pawn, Thing item, CompMagicItemAbilities comp, MagicItemAbilityDef ability, out string reason)
    {
        reason = null;
        if (!CanUseItemAbilities(pawn))
        {
            reason = "A controllable spawned pawn must use this item ability.";
            return false;
        }

        if (item == null || comp == null || ability?.spell == null)
        {
            reason = "This magic item ability is not configured correctly.";
            return false;
        }

        if (ability.requireEquipped && !MagicItemUtility.IsEquippedOrWornBy(pawn, item))
        {
            reason = "This ability requires the item to be equipped or worn.";
            return false;
        }

        if (ability.requireArcaneGift && SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) != true)
        {
            reason = "Arcane gift required.";
            return false;
        }

        int cooldownTicks = comp.GetCooldownRemainingTicks(ability);
        if (cooldownTicks > 0)
        {
            reason = $"Item cooldown: {cooldownTicks.ToStringTicksToPeriod()} remaining.";
            return false;
        }

        SpellContext context = SpellRequirementUtility.CreatePawnContext(pawn, ability.spell);
        context.sourceItem = item;
        if (!SpellRequirementUtility.CanCastSpell(context, ability.spell, out reason, false))
        {
            return false;
        }

        return true;
    }

    private static TargetingParameters BuildTargetingParameters(Pawn caster, SpellDef spellDef)
    {
        SpellTargetingDef targeting = spellDef?.targeting ?? new SpellTargetingDef();
        TargetingParameters parameters = targeting.primaryTargetType == SpellPrimaryTargetType.Cell
            ? TargetingParameters.ForCell()
            : TargetingParameters.ForThing();

        parameters.canTargetLocations = targeting.primaryTargetType == SpellPrimaryTargetType.Cell
            || targeting.primaryTargetType == SpellPrimaryTargetType.PawnOrCell;
        parameters.canTargetPawns = targeting.includePawns;
        parameters.canTargetBuildings = targeting.includeBuildings;
        parameters.canTargetItems = targeting.includeItems;
        parameters.canTargetAnimals = targeting.includePawns;
        parameters.canTargetHumans = targeting.includePawns;
        parameters.canTargetMechs = targeting.includePawns;
        parameters.canTargetSelf = targeting.allowSelfTarget;

        if (targeting.pawnAffinity == SpellPawnAffinity.Ally)
        {
            parameters.neverTargetHostileFaction = true;
        }

        parameters.validator = targetInfo => IsValidTarget(caster, spellDef, targeting, targetInfo);
        return parameters;
    }

    private static bool IsValidTarget(Pawn caster, SpellDef spellDef, SpellTargetingDef targeting, TargetInfo targetInfo)
    {
        LocalTargetInfo localTarget = targetInfo.Thing != null
            ? new LocalTargetInfo(targetInfo.Thing)
            : new LocalTargetInfo(targetInfo.Cell);

        if (!localTarget.IsValid)
        {
            return false;
        }

        Thing targetThing = localTarget.Thing;
        bool hasThing = targetThing != null;
        bool isPawn = targetThing is Pawn;

        if (!MatchesPrimaryTargetType(targeting.primaryTargetType, hasThing, isPawn)
            || !MatchesCategoryFilters(targeting, targetThing, hasThing, isPawn))
        {
            return false;
        }

        if (!targeting.allowSelfTarget && hasThing && caster != null && targetThing == caster)
        {
            return false;
        }

        if (isPawn && !MatchesPawnAffinity(caster, targetThing, targeting.pawnAffinity))
        {
            return false;
        }

        IntVec3 targetCell = localTarget.Cell;
        if (!targetCell.IsValid)
        {
            return false;
        }

        SpellContext rangeContext = new()
        {
            caster = caster,
            map = caster?.Map,
            spellDef = spellDef,
            power = SpellPowerUtility.ComputePower(spellDef, caster)
        };
        float range = SpellEnhancementUtility.ResolveScalableRadius(rangeContext, targeting.range, targeting.scalableRange);
        if (caster != null && range > 0f && caster.Position.DistanceTo(targetCell) > range)
        {
            return false;
        }

        if (targeting.requireLineOfSight && !HasLineOfSight(caster, localTarget))
        {
            return false;
        }

        if (RequiresCellValidation(targeting.primaryTargetType))
        {
            if (caster?.Map == null)
            {
                return false;
            }

            if (targeting.requireStandableCell && !targetCell.Standable(caster.Map))
            {
                return false;
            }

            if (targeting.requireWalkableCell && !targetCell.Walkable(caster.Map))
            {
                return false;
            }

            if (targeting.requireWaterCell && !SpellTerrainUtility.IsWaterCell(caster.Map, targetCell))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CanUseItemAbilities(Pawn pawn)
    {
        return pawn != null
            && pawn.Spawned
            && pawn.Map != null
            && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony);
    }

    private static Texture2D ResolveIcon(MagicItemAbilityDef ability, SpellDef spellDef)
    {
        if (!string.IsNullOrWhiteSpace(ability?.iconPath))
        {
            Texture2D authoredIcon = ContentFinder<Texture2D>.Get(ability.iconPath, false);
            if (authoredIcon != null)
            {
                return authoredIcon;
            }

            Log.Warning($"[MagicFramework] Could not load magic item ability icon '{ability.iconPath}'.");
        }

        if (!string.IsNullOrWhiteSpace(spellDef?.gizmoIconPath))
        {
            Texture2D spellIcon = ContentFinder<Texture2D>.Get(spellDef.gizmoIconPath, false);
            if (spellIcon != null)
            {
                return spellIcon;
            }
        }

        if (!string.IsNullOrWhiteSpace(spellDef?.defName))
        {
            Texture2D conventionIcon = ContentFinder<Texture2D>.Get($"UI/Gizmos/Spells/{spellDef.defName}", false);
            if (conventionIcon != null)
            {
                return conventionIcon;
            }
        }

        return ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true);
    }

    private static string DisplayLabel(MagicItemAbilityDef ability)
    {
        return string.IsNullOrWhiteSpace(ability?.label) ? ability?.spell?.LabelCap ?? "magic item ability" : ability.label;
    }

    private static bool MatchesPrimaryTargetType(SpellPrimaryTargetType primaryTargetType, bool hasThing, bool isPawn)
    {
        switch (primaryTargetType)
        {
            case SpellPrimaryTargetType.Cell:
                return !hasThing;
            case SpellPrimaryTargetType.Pawn:
                return isPawn;
            case SpellPrimaryTargetType.Thing:
            case SpellPrimaryTargetType.PawnOrThing:
                return hasThing;
            case SpellPrimaryTargetType.PawnOrCell:
                return !hasThing || isPawn;
            default:
                return false;
        }
    }

    private static bool MatchesCategoryFilters(SpellTargetingDef targeting, Thing targetThing, bool hasThing, bool isPawn)
    {
        if (!hasThing)
        {
            return true;
        }

        if (isPawn)
        {
            return targeting.includePawns;
        }

        return targetThing.def.category switch
        {
            ThingCategory.Building => targeting.includeBuildings,
            ThingCategory.Item => targeting.includeItems,
            _ => true
        };
    }

    private static bool MatchesPawnAffinity(Pawn caster, Thing targetThing, SpellPawnAffinity pawnAffinity)
    {
        if (pawnAffinity == SpellPawnAffinity.All)
        {
            return true;
        }

        if (caster == null || targetThing is not Pawn)
        {
            return false;
        }

        Faction casterFaction = caster.Faction;
        Faction targetFaction = targetThing.Faction;
        bool sameFaction = casterFaction != null && targetFaction != null && casterFaction == targetFaction;
        bool hostile = casterFaction != null && targetFaction != null && casterFaction.HostileTo(targetFaction);

        switch (pawnAffinity)
        {
            case SpellPawnAffinity.Ally:
                return targetThing == caster || sameFaction;
            case SpellPawnAffinity.Foe:
                return hostile;
            default:
                return true;
        }
    }

    private static bool RequiresCellValidation(SpellPrimaryTargetType primaryTargetType)
    {
        switch (primaryTargetType)
        {
            case SpellPrimaryTargetType.Cell:
            case SpellPrimaryTargetType.PawnOrCell:
                return true;
            default:
                return false;
        }
    }

    private static bool HasLineOfSight(Pawn caster, LocalTargetInfo target)
    {
        if (caster?.Map == null || !target.IsValid)
        {
            return false;
        }

        if (target.Thing != null)
        {
            CellRect startRect = caster.OccupiedRect();
            CellRect endRect = target.Thing.OccupiedRect();
            return GenSight.LineOfSight(caster.Position, target.Cell, caster.Map, startRect, endRect);
        }

        return GenSight.LineOfSight(caster.Position, target.Cell, caster.Map);
    }
}
