# MagicFramework Lifecycle Hooks

This document defines the intended lifecycle semantics for persistent and maintained spell state. `ProjectTodo.md` remains the task tracker; this file is the authoring contract for hook behavior as systems are backfilled.

## Hook Semantics

- `onCreateActions`: runs after persistent state has been created and registered successfully.
- `onPulseActions`: runs once when a persistent state performs a scheduled pulse, before normal per-target pulse actions.
- `onTriggerActions`: runs when an armed trigger activates. Existing trigger child `actions` are currently the trigger body.
- `onExpireActions`: runs when authored duration or failsafe duration naturally ends the state.
- `onRemoveActions`: runs when state is intentionally removed, cancelled, cleared, or replaced by recast.
- `onBreakActions`: runs when state ends because maintenance failed or the runtime state became invalid.
- `onEndActions`: legacy catch-all for older authored area-zone cleanup. It runs after the specific terminal hook for expire, remove, or break.

## Terminal Categories

Natural expiry:
- Duration or failsafe duration reaches its end tick.
- Runs `onExpireActions`, then legacy `onEndActions`.

Intentional removal:
- Player/manual cancellation.
- Replacement by recasting a state with `replaceExistingForCaster`.
- Explicit clear/remove actions.
- Runs `onRemoveActions`, then legacy `onEndActions`.

Break:
- Caster, target, marker, or required runtime state becomes invalid.
- Concentration fails.
- Range, line-of-sight, mana upkeep, or other maintenance requirement fails.
- Runs `onBreakActions`, then legacy `onEndActions`.

Trigger:
- A persistent trigger condition activates.
- Runs trigger body actions. Future trigger systems should expose this as `onTriggerActions` while preserving existing `actions` authoring.

## First-Pass Coverage

Implemented first:
- Persistent area zones support `onCreateActions`, `onPulseActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`, with `onEndActions` retained as a legacy catch-all.
- Maintained force fields support `onCreateActions`, `onPulseActions`, `onExpireActions`, `onRemoveActions`, and `onBreakActions`.
- Sustained stat modifiers support `onPulseActions` and `onBreakActions`.
- `SpellMaintenanceDef` supports composable maintenance profiles for interruption rules.
- Missing maintenance defs use system-specific legacy behavior so existing spells do not change meaning.

## Maintenance Profiles

Authored maintained effects can define a `maintenance` block with a list of profiles:

```xml
<maintenance>
  <profiles>
    <li>CasterConscious</li>
    <li>TargetValid</li>
    <li>Tethered</li>
    <li>LineOfSight</li>
  </profiles>
  <maxRange>16</maxRange>
</maintenance>
```

Profiles:
- `CasterValid`: caster exists and is not destroyed or dead.
- `CasterConscious`: caster is valid and not downed.
- `CasterFocused`: caster is conscious, not stunned, and not in a mental state.
- `TargetValid`: target exists and is not destroyed or dead.
- `TargetConscious`: target is valid and not downed.
- `Tethered`: caster must stay within `maxRange` of the target or anchor cell.
- `LineOfSight`: caster must maintain line of sight to the target or anchor cell.
- `Anchored`: anchor cell must remain valid and in bounds.

Optional fields:
- `maxRange`: used by `Tethered`.
- `useInitialTargetCell`: makes tether checks use the original anchor cell instead of a moving target when relevant.

First-party validation examples:
- `MF_ForceField`, `MF_ManaShield`, and `MF_Might` use `CasterConscious`, `TargetValid`, `Tethered`, and `LineOfSight`.
- `MF_WatersEmbrace` uses `CasterFocused` and `Anchored`.
- `MF_ManaShield` validates maintained force-field pulses with a harmless periodic visual pulse.

Pending backfill:
- Persistent effects.
- Wall zones.
- Proximity triggers.
- Summoned pawns.
- Spawned things.
- Sustained stat modifiers.
