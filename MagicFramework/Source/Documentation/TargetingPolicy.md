# MagicFramework Targeting And Self-Affect Policy

This document defines first-party authoring conventions for caster, self, ally, neutral, and hostile targeting. It is intentionally conservative: explicit XML should remain preferred until defaults are mature enough to be obvious.

## Primary Targeting

Beneficial pawn spells:
- Use `primaryTargetType` of `Pawn`.
- Use `pawnAffinity` of `Ally`.
- Set `allowSelfTarget` to `true` unless the spell concept explicitly forbids self-casting.
- Set `includeBuildings` and `includeItems` to `false`.
- Examples: `MF_Haste`, `MF_Heal`, `MF_Might`, `MF_ForceField`, `MF_ManaShield`.

Hostile pawn spells:
- Use `primaryTargetType` of `Pawn` when only pawns should be valid.
- Use `pawnAffinity` of `Foe` when the spell should only target hostile pawns.
- Use `pawnAffinity` of `All` when neutral animals or neutral pawns should also be valid targets.
- Set `allowSelfTarget` to `false`.
- Set `includeItems` to `false` unless damaging loose items is central to the spell.
- Example: `MF_Firebolt` uses `PawnOrThing`, `pawnAffinity` `All`, `includeBuildings` `true`, and `includeItems` `false` so it can hit pawns, neutral animals, and buildings, but not chunks or loose items.

Cell-targeted placement spells:
- Use `primaryTargetType` of `Cell`.
- Set `includePawns`, `includeBuildings`, and `includeItems` to `false`.
- Use terrain requirements such as `requireWalkableCell`, `requireStandableCell`, or `requireWaterCell` when placement needs a specific surface.
- `allowSelfTarget` is not gameplay-significant for pure cell targeting, but should be set to the safest readable value for the spell concept.
- Examples: `MF_FlameField`, `MF_WatersEmbrace`, `MF_SummonDog`.

Mixed pawn/cell spells:
- Use `PawnOrCell` only when both targeting modes are meaningful.
- Make pawn affinity explicit even if cell targeting is the common path.
- Example: `MF_Freeze` can be cast on a hostile pawn or a target cell.

## Persistent Area And Wall Effects

Hostile hazards:
- Set `pawnAffinity` to `Foe` when the hazard should only affect hostiles.
- Set `includeCaster` to `false`.
- Examples: `MF_Freeze`, `MF_WatersEmbrace`, `MF_RuneTrap`.

Neutral hazards:
- Set `pawnAffinity` to `All` when the hazard should affect any pawn standing in it.
- Set `includeCaster` to `false` unless self-harm is explicitly intended.
- Examples: `MF_FlameField`, `MF_WallOfFire`.

Beneficial auras:
- Prefer `pawnAffinity` `Ally`.
- Decide whether `includeCaster` should be true based on the spell identity:
  - `true` for self-centered blessing/aura effects that should include the caster.
  - `false` for outward-only support effects.
- Example: `MF_BlessingOfVigor` intentionally excludes the caster to validate ally-radius caster exclusion.

Terrain-only pulses:
- If a persistent zone pulses only terrain/object actions and not pawn-targeted actions, `pawnAffinity` still should be explicit.
- Use `pulseAtCenter` when the action should happen at the anchor even if no pawn is inside.
- Example: `MF_EarthCall`.

## Defaults Policy

Current policy:
- Keep first-party XML explicit.
- Do not infer pawn affinity or caster inclusion from metadata tags yet.
- Missing fields should preserve existing framework behavior.

Safe future default candidates:
- Beneficial pawn spell metadata could default to `pawnAffinity=Ally`, `allowSelfTarget=true`, `includeBuildings=false`, and `includeItems=false`.
- Hostile direct-damage metadata could default to `allowSelfTarget=false` and `includeItems=false`.
- Persistent hostile hazards could default to `includeCaster=false`.
- `gizmoIconPath` could conventionally fall back to `UI/Gizmos/Spells/{defName}` when omitted.

Avoid for now:
- Inferring self-damage or ally safety from metadata alone.
- Hiding dangerous area behavior behind defaults.
- Automatically making all hostile spells target neutral pawns or animals; this should remain an authored choice.

## Audit Notes

Reviewed first-party examples:
- `MF_Firebolt`: configured for pawns, neutral animals, and buildings; excludes loose items/chunks.
- `MF_Haste`: allied pawn buff, self-cast allowed.
- `MF_Heal`: allied pawn heal, self-cast allowed.
- `MF_Freeze`: hostile pawn/cell control spell; lingering field affects foes and excludes caster.
- `MF_FlameField`: neutral hazard; affects all pawns in the field and excludes caster.
- `MF_WatersEmbrace`: water-cell hostile aura; affects foes and excludes caster.
- `MF_SummonDog`: cell placement spell with walkable/standable requirements.
