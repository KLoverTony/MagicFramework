# MagicFramework Spell Design Guide

Status: MF-031 draft scaffold for the version 1 authoring surface.

This guide is for authors building spells with MagicFramework XML. It should document the stable version 1 spell surface first, then call out future or experimental features separately.

## Consumer Presentation

The canonical version of this guide should live in the GitHub repository as plain Markdown so it can be reviewed, diffed, and versioned with the framework. Consumer-facing entry points should link back to that source:

- GitHub: full guide, examples, and reference sections.
- Steam Workshop: short author note with a link to the GitHub guide.
- Spell def builder homepage: in-browser guide view, quick-start examples, and direct links from builder controls to the matching guide sections.

The builder should not become a separate source of truth. It should present this guide, selected examples, and focused field help in browser while keeping the Markdown files as the maintained documentation.

## Authoring Goals

- Make a spell readable from its `SpellDef` outward: metadata, requirements, targeting, costs, execution, lifecycle, and presentation.
- Prefer reusable framework primitives over one-off workers when a spell can be expressed as data.
- Keep first-party validation spells close to the guide so examples stay executable in game.
- Treat save/load cleanup, deterministic gameplay decisions, and player-facing descriptions as part of spell design rather than afterthoughts.

## Documentation Roadmap

MF-031 should become both a tutorial path and a reference manual. The guide will grow in layers:

1. Starter tutorials: direct healing, projectile impact, reusable timed status, raw/progressive hediff, persistent aura, maintained spell, delayed trigger, displacement, chain, and summon/spawn.
2. Authoring references: `SpellDef` anatomy, action catalog, targeting/query catalog, lifecycle hooks, scaling, generated presentation, and validation checklist.
3. Reusable resources: `SpellStatusEffectDef`, status cue hediffs, ordinary hediffs, metadata defs, MagicFX profiles, marker things, projectiles, gizmo icons, research gates, and generated scroll hooks.
4. Built-in resource discovery: how to find usable `HediffDef`, `ThingDef`, `EffecterDef`, `FleckDef`, `SoundDef`, `DamageDef`, `PawnKindDef`, `ResearchProjectDef`, and texture path names in local RimWorld, MagicFramework, and MFVanilla defs.
5. Browser presentation: tutorial pages and SpellForge cards should link into this canonical Markdown guide rather than becoming a separate source of truth.

The immediate next documentation pass should expand the first two tutorials, then add a reusable status tutorial because reusable hediff/status authoring is one of the first places XML authors need clear judgment.

## Your First Spells

A MagicFramework spell is a RimWorld `Def` with a `MagicFramework.Definitions.SpellDef` root. Most spells follow the same broad order:

1. Name and describe the spell.
2. Classify it with metadata.
3. Decide how it is learned.
4. Define targeting.
5. Add cast requirements and costs.
6. Add one or more actions.

Start with one direct spell and one delayed-impact spell. They teach the two most common authoring shapes:

- A direct spell validates a target, pays costs, and runs actions immediately against that target.
- A projectile spell validates a target, launches something now, and runs impact actions later when the projectile resolves.

### First Direct Spell: Minor Heal

This is a minimal targeted healing spell. It is intentionally smaller than `MF_Heal`, but it uses the same core pattern.

What you are building:

- A `SpellDef` that appears as a castable spell once a pawn learns it.
- A single allied pawn targeter that allows the caster to heal themselves.
- A mana requirement/cost pair and cooldown requirement/cost pair.
- A short action sequence: play a visible effect, then run `HealActionDef`.

Where it goes:

```text
YourMod/
  About/
    About.xml
  Defs/
    SpellDefs/
      Example_MinorHeal.xml
  Textures/
    UI/
      Gizmos/
        Spells/
          Example_MinorHeal.png
```

Your mod's `About.xml` should depend on `oracle.magicframework`. If you use MFVanilla metadata defs such as `MF_Element_Life`, `MF_Domain_Vitalism`, or MFVanilla research projects, also depend on `oracle.mfvanilla`. If you want your content mod to stand on MagicFramework alone, define your own metadata defs or omit the optional metadata lists until you add them.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <MagicFramework.Definitions.SpellDef>
    <defName>Example_MinorHeal</defName>
    <label>minor heal</label>
    <description>Restores a small amount of health to an allied pawn.</description>
    <gizmoIconPath>UI/Gizmos/Spells/Example_MinorHeal</gizmoIconPath>
    <range>12</range>
    <castTimeTicks>25</castTimeTicks>

    <meta>
      <tier>1</tier>
      <elements>
        <li>MF_Element_Life</li>
      </elements>
      <domains>
        <li>MF_Domain_Vitalism</li>
      </domains>
      <disciplines>
        <li>MF_Discipline_Healing</li>
      </disciplines>
      <tags>
        <li>MF_Tag_Beneficial</li>
      </tags>
    </meta>

    <learning>
      <canBeLearned>true</canBeLearned>
      <requirements>
        <li Class="MagicFramework.Definitions.ArcaneGiftRequirementDef" />
      </requirements>
    </learning>

    <targeting>
      <shape>Single</shape>
      <primaryTargetType>Pawn</primaryTargetType>
      <pawnAffinity>Ally</pawnAffinity>
      <includePawns>true</includePawns>
      <includeBuildings>false</includeBuildings>
      <includeItems>false</includeItems>
      <allowSelfTarget>true</allowSelfTarget>
      <requireLineOfSight>true</requireLineOfSight>
      <range>12</range>
    </targeting>

    <requirements>
      <li Class="MagicFramework.Definitions.ManaRequirementDef">
        <amount>6</amount>
      </li>
      <li Class="MagicFramework.Definitions.CooldownRequirementDef">
        <cooldownTicks>120</cooldownTicks>
      </li>
    </requirements>

    <costs>
      <li Class="MagicFramework.Definitions.ManaCostDef">
        <amount>6</amount>
      </li>
      <li Class="MagicFramework.Definitions.CooldownCostDef">
        <cooldownTicks>120</cooldownTicks>
      </li>
    </costs>

    <actions>
      <li Class="MagicFramework.Definitions.SequenceActionDef">
        <actions>
          <li Class="MagicFramework.Definitions.EffectActionDef">
            <effectDef>PsycastPsychicEffect</effectDef>
            <locationSource>CurrentTarget</locationSource>
            <attachToTarget>true</attachToTarget>
          </li>
          <li Class="MagicFramework.Definitions.HealActionDef">
            <amount>10</amount>
          </li>
        </actions>
      </li>
    </actions>
  </MagicFramework.Definitions.SpellDef>
</Defs>
```

What this does:

- `defName` is the stable ID. Do not rename it casually after release; saves and generated references may depend on it.
- `meta` classifies the spell for display, filtering, and rules. It does not apply the heal by itself.
- `learning` says the spell can be learned and requires the Arcane gift before acquisition.
- `targeting` opens a single allied pawn targeter and allows self-casting.
- `requirements` block the cast unless the caster has enough mana and the spell is off cooldown.
- `costs` spend the mana and start the cooldown after validation succeeds.
- `actions` play a visual effect on the selected pawn, then heal injuries by distributing the healing amount across current wounds.

For a production spell, add research prerequisites, caster level requirements, scaling, generated description tokens, and a real icon texture. `MF_Heal` in `MFVanilla/Defs/SpellDefs/MF_Heal.xml` is the first-party version of this pattern.

#### Add learning gates

Most published spells should not be freely learnable by every pawn. Add research and caster-level requirements under `learning`:

```xml
<learning>
  <canBeLearned>true</canBeLearned>
  <researchPrerequisites>
    <li>MFV_Vitalism</li>
  </researchPrerequisites>
  <requirements>
    <li Class="MagicFramework.Definitions.ArcaneGiftRequirementDef" />
    <li Class="MagicFramework.Definitions.CasterLevelRequirementDef">
      <minimumLevel>1</minimumLevel>
    </li>
  </requirements>
</learning>
```

Learning requirements decide whether a pawn can acquire the spell. Cast requirements decide whether a pawn can use it right now. It is normal for both to exist: a pawn may know Minor Heal but still fail to cast because they are out of mana or the spell is on cooldown.

#### Add generated description text

Authored descriptions can include generated detail tokens. This keeps the flavor text short while letting MagicFramework describe the current mechanics:

```xml
<description>Restores a small amount of health to an allied pawn.

{MF:SpellSummary}</description>
```

Useful starter tokens:

| Token | Use |
| --- | --- |
| `{MF:SpellSummary}` | Compact combined summary for spell details. |
| `{MF:Effects}` | Generated action/effect summary. |
| `{MF:ManaCost}` | Resolved mana cost text. |
| `{MF:Cooldown}` | Cooldown text. |
| `{MF:Range}` | Targeting range text. |
| `{MF:Requirements}` | Cast and learning requirement text. |
| `{MF:PowerScaling}` | Power/scaling summary when authored. |

Use generated text for mechanics, not lore. Keep the first sentence readable on its own because it appears in places where the full generated detail may not be the main focus.

#### Add caster-level healing scaling

The simplest version of scaling is to compute spell power from caster level, then opt a supported attribute into lightweight global scaling:

```xml
<power>
  <casterLevelFactor>1</casterLevelFactor>
  <scaledAttributes>
    <li>Healing</li>
  </scaledAttributes>
</power>
```

This lets the framework apply the current Magic Framework settings multiplier for healing scaling. Use lightweight `scaledAttributes` when you want normal framework-wide growth. Use explicit scalar defs later when a spell needs unusual tuning.

#### Minor Heal variants

| Variant | Change |
| --- | --- |
| Self-only heal | Set `targeting.useCasterAsTarget` to `true`, keep `primaryTargetType` as `Pawn`, and remove the need for a target prompt. |
| Ally-only non-self heal | Keep `pawnAffinity` as `Ally`, set `allowSelfTarget` to `false`. |
| Longer-range heal | Increase both top-level `range` and `targeting.range`. |
| Emergency cheap heal | Lower mana and cooldown values together in both requirement and cost blocks. |
| Heal-over-time | Use a `RepeatActionDef` around `HealActionDef`, or use a reusable regeneration-style `SpellStatusEffectDef` if the effect should also show a status cue. |

What to change first:

| Change | Field |
| --- | --- |
| Make it stronger or weaker | `HealActionDef.amount` |
| Change how far it can reach | Top-level `range` and `targeting.range` |
| Change the resource cost | `ManaRequirementDef.amount` and `ManaCostDef.amount` |
| Change how often it can be used | `CooldownRequirementDef.cooldownTicks` and `CooldownCostDef.cooldownTicks` |
| Restrict it to non-self targets | `targeting.allowSelfTarget` |

Common mistakes:

- Setting only top-level `range` and forgetting `targeting.range`; the targeter should carry the real validation value.
- Adding a mana cost without a matching mana requirement; the spell should fail clearly before execution if the caster cannot pay.
- Leaving `pawnAffinity` as `All` on beneficial spells; ally-only targeting prevents awkward hostile healing unless the spell is intentionally mixed.

### First Projectile Spell: Ember Bolt

Projectile spells introduce one extra idea: the spell action tree can launch a projectile now and run `onImpactActions` later. Impact actions use the projectile impact context, so `CurrentTarget` resolves to the hit thing when one was captured, and the current cell resolves to the landing or last known projectile cell when there is no hit thing.

What you are building:

- A hostile single-target spell that requires line of sight.
- A cast effect at the caster.
- A real RimWorld projectile launched from the caster toward the selected target.
- Impact actions that play a visual/sound cue and apply flame damage.

Projectile flow:

1. The spell validates targeting, mana, cooldown, and other requirements.
2. The spell pays its costs.
3. `EffectActionDef` plays the cast visual at the caster.
4. `LaunchProjectileActionDef` resolves `projectileDef`, launch origin, and target source.
5. RimWorld launches the projectile.
6. MagicFramework stores the pending impact and waits for impact, destruction, explosion landing, or timeout.
7. `onImpactActions` run with projectile impact context.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <MagicFramework.Definitions.SpellDef>
    <defName>Example_EmberBolt</defName>
    <label>ember bolt</label>
    <description>Launches a small fiery projectile at a visible target.</description>
    <gizmoIconPath>UI/Gizmos/Spells/Example_EmberBolt</gizmoIconPath>
    <range>20</range>
    <castTimeTicks>30</castTimeTicks>

    <meta>
      <tier>1</tier>
      <elements>
        <li>MF_Element_Fire</li>
      </elements>
      <domains>
        <li>MF_Domain_Pyromancy</li>
      </domains>
      <disciplines>
        <li>MF_Discipline_Combat</li>
      </disciplines>
      <tags>
        <li>MF_Tag_Projectile</li>
        <li>MF_Tag_Hostile</li>
        <li>MF_Tag_DirectDamage</li>
      </tags>
    </meta>

    <learning>
      <canBeLearned>true</canBeLearned>
      <requirements>
        <li Class="MagicFramework.Definitions.ArcaneGiftRequirementDef" />
      </requirements>
    </learning>

    <targeting>
      <shape>Single</shape>
      <primaryTargetType>PawnOrThing</primaryTargetType>
      <pawnAffinity>All</pawnAffinity>
      <includePawns>true</includePawns>
      <includeBuildings>true</includeBuildings>
      <includeItems>false</includeItems>
      <allowSelfTarget>false</allowSelfTarget>
      <requireLineOfSight>true</requireLineOfSight>
      <range>20</range>
    </targeting>

    <requirements>
      <li Class="MagicFramework.Definitions.ManaRequirementDef">
        <amount>8</amount>
      </li>
      <li Class="MagicFramework.Definitions.CooldownRequirementDef">
        <cooldownTicks>90</cooldownTicks>
      </li>
    </requirements>

    <costs>
      <li Class="MagicFramework.Definitions.ManaCostDef">
        <amount>8</amount>
      </li>
      <li Class="MagicFramework.Definitions.CooldownCostDef">
        <cooldownTicks>90</cooldownTicks>
      </li>
    </costs>

    <actions>
      <li Class="MagicFramework.Definitions.SequenceActionDef">
        <actions>
          <li Class="MagicFramework.Definitions.EffectActionDef">
            <effectDef>Mote_SparkThrownFast</effectDef>
            <locationSource>Caster</locationSource>
            <attachToTarget>false</attachToTarget>
          </li>
          <li Class="MagicFramework.Definitions.LaunchProjectileActionDef">
            <projectileDef>Bullet_Revolver</projectileDef>
            <launchOrigin>Caster</launchOrigin>
            <targetSource>CurrentTarget</targetSource>
            <onImpactActions>
              <li Class="MagicFramework.Definitions.EffectActionDef">
                <effectDef>GiantExplosion</effectDef>
                <soundDef>Explosion_Flame</soundDef>
                <locationSource>CurrentTarget</locationSource>
                <attachToTarget>true</attachToTarget>
              </li>
              <li Class="MagicFramework.Definitions.DamageActionDef">
                <amount>12</amount>
                <damageDef>Flame</damageDef>
              </li>
            </onImpactActions>
          </li>
        </actions>
      </li>
    </actions>
  </MagicFramework.Definitions.SpellDef>
</Defs>
```

What this does:

- `targeting` allows a visible pawn or building target, but not items or the caster.
- `LaunchProjectileActionDef` launches the authored projectile from the caster toward the current target.
- `onImpactActions` run after the projectile impact is captured by the framework.
- `DamageActionDef` applies flame damage to the impact target when one exists.

`MF_Firebolt` in `MFVanilla/Defs/SpellDefs/MF_Firebolt.xml` is the first-party version of this pattern. It adds research, caster level requirements, debug labels, spell power scaling, and production tuning.

#### Understand projectile context

`LaunchProjectileActionDef` has a few fields that decide where the projectile starts, what it aims at, and what happens if vanilla projectile resolution is delayed or unavailable:

| Field | Meaning |
| --- | --- |
| `projectileDef` | A `ThingDef` whose `thingClass` is a RimWorld `Projectile`. Vanilla projectiles such as `Bullet_Revolver` work. |
| `launchOrigin` | Where the projectile starts: `Caster`, `CurrentTarget`, or `CurrentCell`. |
| `targetSource` | What the projectile aims at: `CurrentTarget`, `CurrentCell`, or `Caster`. |
| `hitFlags` | Vanilla projectile hit flags. Defaults to `All`. |
| `preventFriendlyFire` | Passes friendly-fire prevention into vanilla projectile launch. |
| `impactTimeoutPaddingTicks` | Extra wait time before MagicFramework treats a missing impact callback as timed out. |
| `onImpactActions` | Child actions that run after impact, shield block, destruction, or timeout. |

If `projectileDef` cannot be resolved or is not a projectile, MagicFramework logs a warning and executes `onImpactActions` immediately. That fallback keeps the spell from silently doing nothing, but it is a sign that the XML should be fixed.

Impact actions should use `CurrentTarget` when they need the hit thing, and `CurrentCell` when they need the impact location. Firebolt-style single-target damage usually uses `CurrentTarget`; Fireball-style explosions usually use `CurrentCell`.

#### Finding projectile, effect, and sound names

Projectile spells commonly reference several external defs:

| XML field | Def type | Where to look |
| --- | --- | --- |
| `projectileDef` | `ThingDef` with projectile data | RimWorld `Data/Core/Defs/ThingDefs*`, weapon/projectile XML, or existing MFVanilla spell XML. |
| `effectDef` | `EffecterDef` | RimWorld effecter defs, existing spell XML, or MagicFramework/MFVanilla examples. |
| `soundDef` | `SoundDef` | RimWorld sound defs and existing spell XML. |
| `damageDef` | `DamageDef` | RimWorld damage defs; common examples include `Flame` and `Blunt`. |
| `hediffDef` | `HediffDef` | RimWorld hediff defs, MFVanilla hediff defs, or your own content mod. |

Good first search targets in this workspace:

```powershell
rg -n "<defName>Bullet_|<defName>.*Projectile" "D:\Program Files (x86)\Steam\steamapps\common\RimWorld\Data"
rg -n "<EffecterDef>|<FleckDef>|<SoundDef>|<DamageDef>|<HediffDef>" "D:\Program Files (x86)\Steam\steamapps\common\RimWorld\Data"
rg -n "projectileDef|effectDef|soundDef|damageDef|hediffDef" "D:\RimWorld\Mods\MFVanilla\Defs"
```

Do not guess names blindly. If a def name is wrong, RimWorld or MagicFramework will usually log a warning during XML load or action execution.

#### Add a secondary burn

For a simple single-target burn rider, add an `ApplyHediffActionDef` under `onImpactActions` after the damage:

```xml
<li Class="MagicFramework.Definitions.ApplyHediffActionDef">
  <debugLabel>Apply ember burn</debugLabel>
  <hediffDef>Burn</hediffDef>
  <severity>0.10</severity>
</li>
```

Use raw hediffs when you want to interact directly with RimWorld health state. Use reusable `SpellStatusEffectDef` when the effect is a designed magical status with categories, refresh policy, default duration, stat modifiers, and a visible status cue.

#### Turn it into a small explosion

To make the projectile affect an impact cell instead of only the hit target, add an `ExplosionActionDef` under `onImpactActions` and use `CurrentCell` for visuals:

```xml
<li Class="MagicFramework.Definitions.EffectActionDef">
  <effectDef>GiantExplosion</effectDef>
  <soundDef>Explosion_Flame</soundDef>
  <locationSource>CurrentCell</locationSource>
  <attachToTarget>false</attachToTarget>
</li>
<li Class="MagicFramework.Definitions.ExplosionActionDef">
  <radius>1.9</radius>
  <damageAmount>8</damageAmount>
  <damageDef>Flame</damageDef>
</li>
```

If you want secondary effects on pawns in the radius, use `ApplyToTargetsActionDef` with a radius query, as `MF_Fireball` does.

#### Add projectile scaling

The simplest Firebolt-style scaling is:

```xml
<power>
  <casterLevelFactor>1</casterLevelFactor>
  <scaledAttributes>
    <li>Damage</li>
    <li>Cooldown</li>
  </scaledAttributes>
</power>
```

Use `Damage` when direct or impact damage should grow with caster power. Use `Cooldown` when stronger casters should recover faster according to Magic Framework settings. For area projectile spells, add `Radius` if explosion or query radii should scale too.

#### Targeting and friendly-fire cautions

Projectile spells can look simple while still having tactical side effects:

- `pawnAffinity` controls target selection, not every possible projectile collision or explosion side effect.
- `preventFriendlyFire` helps vanilla projectile launch avoid friendly fire where vanilla supports it, but it does not make your `onImpactActions` safe by itself.
- Explosions, radius queries, chains, and secondary hediffs need their own affinity/query decisions.
- If a spell can target buildings, decide whether that is intentional and whether `DamageActionDef` should use guilt/combat-log settings.
- If you set `primaryTargetType` to `PawnOrThing`, be explicit about `includeBuildings` and `includeItems`.

What to change first:

| Change | Field |
| --- | --- |
| Change the projectile art/flight behavior | `LaunchProjectileActionDef.projectileDef` and the referenced RimWorld projectile `ThingDef` |
| Change where the projectile starts | `launchOrigin` |
| Change what it aims at | `targetSource` |
| Change impact damage | `DamageActionDef.amount` and `DamageActionDef.damageDef` |
| Add an explosion or status on hit | Add more `onImpactActions` |

Common mistakes:

- Putting damage beside the projectile action when the damage should happen on hit. Use `onImpactActions` for hit-dependent effects.
- Forgetting that vanilla projectile defs control much of the visual flight behavior. Use projectile `ThingDef` authoring for arc height, speed, and related projectile presentation.
- Making a hostile projectile target `Ally` or allowing self-targeting by accident.

### Direct Versus Projectile Actions

Use a direct action when the effect should happen as soon as the cast completes:

```xml
<li Class="MagicFramework.Definitions.HealActionDef">
  <amount>10</amount>
</li>
```

Use projectile impact actions when the effect should wait for projectile resolution:

```xml
<li Class="MagicFramework.Definitions.LaunchProjectileActionDef">
  <projectileDef>Bullet_Revolver</projectileDef>
  <launchOrigin>Caster</launchOrigin>
  <targetSource>CurrentTarget</targetSource>
  <onImpactActions>
    <li Class="MagicFramework.Definitions.DamageActionDef">
      <amount>12</amount>
      <damageDef>Flame</damageDef>
    </li>
  </onImpactActions>
</li>
```

This distinction matters for balance and clarity. A direct damage action always runs when the cast executes; an impact damage action only runs after the projectile system reports its result.

## SpellDef Anatomy

`SpellDef` is the top-level object authors usually edit. These are the main fields in the version 1 authoring surface:

| Field | Purpose |
| --- | --- |
| `defName` | Stable unique identifier used by saves, references, generated scrolls, and other defs. Do not rename after release unless you also handle migration. |
| `label` | Player-facing spell name. |
| `description` | Player-facing description. Can include generated detail tokens such as `{MF:SpellSummary}` or `{MF:Effects}` when a spell should insert framework-generated text. |
| `range` | Legacy/default display and targeting range. Prefer also setting `targeting.range` for actual target validation clarity. |
| `castTimeTicks` | Warmup duration before the spell executes. `60` ticks is one in-game second. |
| `gizmoIconPath` | Texture path under `Textures`, without the file extension. |
| `element`, `delivery`, `effectShape` | Legacy procedural FX hints. Prefer `meta` for classification, but these strings still influence older procedural FX resolution. |
| `fxOverride`, `fxColorOverride`, `fxIntensityMultiplier` | Optional procedural FX overrides. |
| `disableProceduralFx`, `disableDecal` | Presentation toggles for spells that should avoid automatic framework visuals. |
| `meta` | Author-facing classification: tier, elements, domains, disciplines, and tags. Used by display, filtering, and enhancement rules. |
| `learning` | Rules for whether a pawn can learn the spell, including research and learning-only requirements. |
| `casting` | Grouped casting requirements and costs. Legacy top-level `requirements` and `costs` remain supported and are still used by first-party XML. |
| `power` | Spell power computation, tiers, and scalar authoring. |
| `targeting` | Player targeter policy: target type, affinity, self-targeting, range, line of sight, cell restrictions, and shape. |
| `requirements` | Cast-time checks such as mana, cooldown, arcane gift, and caster level. |
| `costs` | Resources or state applied after validation succeeds, such as spending mana and starting cooldown. |
| `actions` | The execution tree that actually performs spell effects. |

### Metadata

Use `meta` to describe what a spell is, not what it mechanically does on every line. Metadata is intentionally additive:

```xml
<meta>
  <tier>1</tier>
  <elements>
    <li>MF_Element_Fire</li>
  </elements>
  <domains>
    <li>MF_Domain_Pyromancy</li>
  </domains>
  <disciplines>
    <li>MF_Discipline_Combat</li>
  </disciplines>
  <tags>
    <li>MF_Tag_Projectile</li>
    <li>MF_Tag_Hostile</li>
  </tags>
</meta>
```

- `elements` describe elemental identity such as fire, life, water, or earth.
- `domains` describe magical schools or traditions such as Pyromancy or Vitalism.
- `disciplines` describe functional use such as Combat or Healing.
- `tags` are flexible markers for filtering, generated summaries, and enhancement matching.

### Learning

`learning` decides whether a pawn can acquire the spell. First-party learnable spells usually require the Arcane gift and a research project:

```xml
<learning>
  <canBeLearned>true</canBeLearned>
  <researchPrerequisites>
    <li>MFV_Vitalism</li>
  </researchPrerequisites>
  <requirements>
    <li Class="MagicFramework.Definitions.ArcaneGiftRequirementDef" />
    <li Class="MagicFramework.Definitions.CasterLevelRequirementDef">
      <minimumLevel>1</minimumLevel>
    </li>
  </requirements>
</learning>
```

Use `hiddenUntilResearchUnlocked` or `hiddenUntilRequirementsMet` when recipes, generated scrolls, or builder UIs should hide a spell until the player has a plausible path to it.

### Requirements And Costs

Requirements answer “may this cast happen?” Costs answer “what happens after it is accepted?”

Common requirements:

| Requirement | Use |
| --- | --- |
| `ManaRequirementDef` | Requires the caster to have at least `amount` mana. |
| `CooldownRequirementDef` | Requires the spell cooldown to be ready, using `cooldownTicks` as the eventual cooldown length. |
| `ArcaneGiftRequirementDef` | Requires the caster pawn to have MagicFramework arcane gift metadata. |
| `CasterLevelRequirementDef` | Requires a minimum caster level. |

Common costs:

| Cost | Use |
| --- | --- |
| `ManaCostDef` | Spends `amount` mana after validation succeeds. |
| `CooldownCostDef` | Starts a cooldown for `cooldownTicks`. |

Keep paired requirement/cost values in sync unless the spell deliberately reserves a different amount than it spends. If a spell has both top-level `requirements`/`costs` and grouped `casting` requirements/costs, document why; most spells should use one style consistently.

### Targeting

Targeting controls only the initial player selection. Actions can later retarget through queries, chains, repeats, or child action context.

| Field | Purpose |
| --- | --- |
| `shape` | Initial target shape: `Single`, `Radius`, `Line`, `Wall`, or `Chain`. |
| `primaryTargetType` | What the initial click can select: `Cell`, `Pawn`, `Thing`, `PawnOrThing`, or `PawnOrCell`. |
| `pawnAffinity` | Pawn filter: `All`, `Ally`, or `Foe`. |
| `includePawns`, `includeBuildings`, `includeItems` | Thing-category filters. |
| `allowSelfTarget` | Whether the caster can target themselves. |
| `useCasterAsTarget` | Skip target selection and execute with the caster as the initial target. Useful for self or caster-centered spells. |
| `requireLineOfSight` | Requires line of sight from caster to target. |
| `requireStandableCell`, `requireWalkableCell`, `requireWaterCell` | Cell constraints for terrain or placement spells. |
| `requireResurrectableCorpse` | Corpse-specific gate for resurrection-style spells. |
| `range`, `scalableRange` | Fixed or power-scaled targeting range. |
| `radius`, `lineLength`, `wallLength`, `maxChains` | Shape-specific targeting values. |

First-party conventions:

- Beneficial pawn spells usually use `primaryTargetType` `Pawn`, `pawnAffinity` `Ally`, and explicit `allowSelfTarget`.
- Hostile direct spells usually use `Pawn` or `PawnOrThing`, `pawnAffinity` `Foe` or `All`, and `requireLineOfSight`.
- Terrain and aura spells should make cell restrictions explicit, especially `requireWalkableCell`, `requireStandableCell`, or `requireWaterCell`.
- Caster-centered spells should use `useCasterAsTarget` so players cast them directly from the gizmo without a redundant target prompt.

#### Targeting Compatibility

Targeting fields are cumulative. If a spell enables more than one category, terrain gate, or special requirement, all relevant checks must pass. This is powerful, but it can also make a spell impossible to cast if the fields describe different kinds of targets.

Use this mental split:

- Target categories (`includePawns`, `includeBuildings`, `includeItems`) decide which thing types can be selected or affected.
- Cell requirements (`requireStandableCell`, `requireWalkableCell`, `requireWaterCell`) validate the target location.
- Selection rules (`allowSelfTarget`, `useCasterAsTarget`, `requireLineOfSight`) change how the initial target is chosen.
- Special requirements such as `requireResurrectableCorpse` narrow targeting to a framework-specific case and should be paired deliberately.

Common recipes:

| Spell intent | Primary target | Categories | Requirements and notes |
| --- | --- | --- | --- |
| Ally heal | `Pawn` | Pawns only | `pawnAffinity` `Ally`; `allowSelfTarget` only if the caster may heal themselves. |
| Self buff | `Pawn` | Pawns only | Prefer `useCasterAsTarget=true` when no target prompt is needed. |
| Hostile bolt | `Pawn` or `PawnOrThing` | Pawns, optionally Buildings/Items | `requireLineOfSight=true`; avoid `allowSelfTarget`. |
| Area burst | `PawnOrCell` | Categories describe what later queries or payloads may affect | Cell requirements are optional unless the cast location must be special terrain. |
| Summon or placement spell | `Cell` | Categories usually irrelevant | Use `requireWalkableCell` or `requireStandableCell` for placement safety. |
| Water-only spell | `Cell` or `PawnOrCell` | Depends on payload | `requireWaterCell=true`; combine with walkable/standable only if all gates should be required. |
| Resurrection | `Thing` | Items only | `requireResurrectableCorpse=true`; disable Pawns and Buildings. |

Suspicious combinations to avoid unless you have tested the exact behavior:

- `primaryTargetType=Pawn` with `includePawns=false`.
- `primaryTargetType=Thing` with both `includeBuildings=false` and `includeItems=false`.
- `requireResurrectableCorpse=true` with `primaryTargetType=Pawn`, `Cell`, or building-only targeting.
- `requireResurrectableCorpse=true` with `useCasterAsTarget=true`.
- `allowSelfTarget=true` while pawns are disabled.
- `requireWaterCell=true` plus `requireWalkableCell=true` or `requireStandableCell=true`, unless the spell should require all selected terrain gates.

### Actions

`actions` is the spell's execution tree. Every action node has a `Class` and may have a `debugLabel`. Complex actions own child action lists:

```xml
<actions>
  <li Class="MagicFramework.Definitions.SequenceActionDef">
    <debugLabel>Firebolt sequence</debugLabel>
    <actions>
      <li Class="MagicFramework.Definitions.EffectActionDef">
        <debugLabel>Firebolt cast effect</debugLabel>
        <effectDef>Mote_SparkThrownFast</effectDef>
        <locationSource>Caster</locationSource>
      </li>
      <li Class="MagicFramework.Definitions.DamageActionDef">
        <debugLabel>Firebolt damage</debugLabel>
        <amount>18</amount>
        <damageDef>Flame</damageDef>
      </li>
    </actions>
  </li>
</actions>
```

The current execution context carries the caster, spell, initial target, current target, current cell, spell power, and deterministic random seed. Child actions normally operate on the current target or current cell unless they expose an explicit source field.

For version 1, authors should start from these reliable patterns:

- `SequenceActionDef` for ordered multi-step spells.
- `EffectActionDef` or `ProceduralFXActionDef` for visuals and sound.
- `DamageActionDef`, `HealActionDef`, `ApplyHediffActionDef`, and status/stat actions for direct pawn effects.
- `LaunchProjectileActionDef` with `onImpactActions` for projectile spells.
- `DelayActionDef`, `RepeatActionDef`, `ConditionalActionDef`, and target-query actions for controlled flow.
- Persistent area, wall, trigger, force-field, summon, and spawned-thing actions when the spell needs runtime cleanup or save/load state.

## Builder Homepage Integration

The spell def builder homepage should present MF-031 as part of the authoring workflow:

- Add a visible `Documentation` or `Guide` entry near the builder entry point.
- Render `SpellDesignGuide.md` in browser, with a table of contents and deep links.
- Let field help link to anchors such as `#targeting`, `#requirements-and-costs`, and `#actions`.
- Include a small example browser with first-party examples like `MF_Heal`, `MF_Firebolt`, `MF_ManaShield`, and `MF_WatersEmbrace`.
- Keep an outbound link to the GitHub source so authors can check the exact version they are reading.
- If the builder can export XML, include a `Validate this spell` checklist drawn from the guide's validation section.

Suggested first example cards:

| Card | Guide anchor | Consumer promise |
| --- | --- | --- |
| Minor Heal | `#first-direct-spell-minor-heal` | Build a simple allied pawn spell that validates, pays mana, starts cooldown, and heals immediately. |
| Ember Bolt | `#first-projectile-spell-ember-bolt` | Build a hostile projectile spell that launches now and applies effects on impact. |
| Direct vs Projectile | `#direct-versus-projectile-actions` | Understand when to put actions directly in the spell sequence and when to put them under `onImpactActions`. |
| SpellDef Anatomy | `#spelldef-anatomy` | Learn what each major top-level section controls. |

For each card, the builder should show the example XML, a short explanation, and a `Start from this example` action if template generation exists. Field-level help should link to the relevant section rather than duplicating full explanations in tooltips.

The browser view can be friendly and searchable, but the Markdown guide should remain the maintained source. That gives consumers a polished reading experience without creating parallel docs that drift.

## Version 1 Guide Outline

1. Spell definition anatomy
   - `SpellDef` identity, labels, descriptions, domain/discipline/element metadata, icons, and generated description tokens.
   - Known spell storage, learning flow, scroll integration, research prerequisites, and debug fallback expectations.
2. Targeting
   - Pawn, cell, caster-as-target, and water-cell targeting.
   - Pawn affinity, self-affect policy, range, line of sight, walkable/standable requirements, and first-party targeting conventions.
   - Target and cell sources used inside action trees.
3. Costs, cooldowns, requirements, and warmup
   - Mana costs, cooldown costs, warmup behavior, known-spell checks, and authored requirement gates.
   - How generated summaries and the spell details window should explain these gates.
4. Action trees
   - Execution order, current target/current cell context, nested action lists, repeats, delays, conditions, queries, and action path persistence.
   - Common action families: damage, healing, hediffs, stat/status effects, projectiles, explosions, terrain patches, teleports, displacement, chains, summons, spawned things, force fields, zones, walls, triggers, and cleanup actions.
5. Persistent lifecycle
   - Create, pulse, trigger, expire, remove, break, and legacy end semantics.
   - Replacement policy, clean release, concentration breaks, save/load behavior, and cleanup expectations for each persistent family.
6. Scaling and spell power
   - `SpellPowerDef`, power tiers, `ScalableFloatDef`, lightweight `scaledAttributes`, explicit `SpellPowerScalarDef`, enhancement factors, and settings multipliers.
   - When to use numeric scaling versus structural conditionals.
7. Visuals, sound, and generated presentation
   - Procedural FX metadata, explicit visual/sound actions, status cues, gizmo icons, generated spell summaries, colored generated text, and spell details UI.
8. Determinism and compatibility
   - Deterministic random helpers, stable ordering, saved spell seeds, and multiplayer-sensitive authoring rules.
9. Common spell recipes
   - Projectile damage spell.
   - Area explosion spell.
   - Direct heal and heal-over-time spell.
   - Timed buff/debuff spell.
   - Maintained shield or sustained stat spell.
   - Delayed rune or trap.
   - Persistent aura, wall, or terrain hazard.
   - Teleport, pull, push, or swap spell.
   - Chain spell.
   - Summon or temporary spawned object.
10. Validation checklist
   - Startup and XML load.
   - Cast success and failure messages.
   - Mana/cooldown correctness.
   - Targeting edge cases.
   - Save/load while delayed or persistent effects are active.
   - Cleanup on cancel, expire, caster down/death, target invalidation, map removal, and replacement.
   - Generated descriptions and spell details.

## First Examples To Document

- `MF_Firebolt`: projectile launch, impact actions, damage scaling, generated details.
- `MF_Fireball`: projectile plus explosion, radius scaling, secondary effects.
- `MF_Heal` and `MF_Regeneration`: direct healing and repeated healing.
- `MF_ChainLightning`: delayed branching chain and deterministic target handling.
- `MF_BlinkStep`, `MF_ForcePush`, and `MF_ForcePull`: movement and displacement.
- `MF_ManaShield` and `MF_ForceField`: maintained force fields and clean release.
- `MF_Might`, `MF_Haste`, and `MF_BlessingOfVigor`: status cues and reusable status effects.
- `MF_WatersEmbrace`: concentration aura, water targeting, progressive hediff pressure, undertow, and lifecycle cleanup.
- `MF_Freeze` and `MF_FlameField`: terrain patching and persistent area behavior.
- `MF_SummonDog`: temporary summon lifecycle.
- `MF_DelayedBlastRune` and `MF_RuneTrap`: delayed and triggered spell patterns.

## Version 1 Release Notes To Preserve

- The guide should document current supported authoring, not speculative next-version systems.
- AI casting, magic tools/weapons, real fire integration, broader AeternusFaith systems, and persistent world objects belong in future-version planning unless they expose a current authoring rule.
- Any undocumented field discovered while writing the guide should become either a guide section, a code comment cleanup task, or a deferred roadmap item.
