# Pawn Lifecycle Profiles

`PawnLifecycleExtension` is the MagicFramework XML surface for composable undead, spirits, constructs, and other nonstandard pawns.

Attach it to a pawn race `ThingDef` or a `PawnKindDef`. If both exist, the pawn kind is treated as the more specific profile and wins during framework queries.

`CompPawnLifecycleEnforcer` can enforce the first conservative runtime policies when attached to a pawn race. Enforcement is opt-in through the `enforce...` flags on the lifecycle profile.

Current enforcement support:
- needs: removes food/rest for policies that do not use ordinary eating or sleeping; `None` also removes joy, comfort, mood, beauty, room, outdoors/indoors, drug desire, and chemical needs
- social: suppresses ordinary social interaction for `None` and `SuppressedBothWays`
- gear: strips all gear, apparel only, or equipment only for the matching gear policies
- work: `HaulingCleaningOnly` and `MundaneLabor` initialize work settings and restrict active work to dumb labor; `None` and `CombatOnly` disable work
- life stage: locks the pawn to the first life stage
- markers: applies listed lifecycle marker hediffs if missing

Recovery, death cleanup, soul contracts, upkeep, and advanced interaction policies are defined but not yet enforced.

## Example Skeleton

```xml
<modExtensions>
  <li Class="MagicFramework.PawnLifecycle.PawnLifecycleExtension">
    <bodyForm>Skeletal</bodyForm>
    <intelligence>Mindless</intelligence>
    <needsPolicy>None</needsPolicy>
    <socialPolicy>None</socialPolicy>
    <gearPolicy>StripAll</gearPolicy>
    <controlPolicy>MasterBoundMinion</controlPolicy>
    <workPolicy>MundaneLabor</workPolicy>
    <recoveryPolicy>Reassembly</recoveryPolicy>
    <deathPolicy>LeaveRemains</deathPolicy>
    <soulPolicy>ReleasedSourceSoul</soulPolicy>
    <durationPolicy>Permanent</durationPolicy>
    <isUndead>true</isUndead>
    <enforceNeeds>true</enforceNeeds>
    <enforceSocialPolicy>true</enforceSocialPolicy>
    <enforceGearPolicy>true</enforceGearPolicy>
    <enforceWorkPolicy>true</enforceWorkPolicy>
    <enforceLifeStage>true</enforceLifeStage>
    <enforceMarkers>true</enforceMarkers>
  </li>
</modExtensions>
<comps>
  <li Class="MagicFramework.PawnLifecycle.CompProperties_PawnLifecycleEnforcer" />
</comps>
```

## Example Spectre

```xml
<modExtensions>
  <li Class="MagicFramework.PawnLifecycle.PawnLifecycleExtension">
    <bodyForm>Spectral</bodyForm>
    <intelligence>TaskBound</intelligence>
    <needsPolicy>None</needsPolicy>
    <socialPolicy>AuraOnly</socialPolicy>
    <gearPolicy>None</gearPolicy>
    <controlPolicy>AutonomousGuest</controlPolicy>
    <workPolicy>RitualOnly</workPolicy>
    <recoveryPolicy>AnchorReform</recoveryPolicy>
    <deathPolicy>ReturnToAnchor</deathPolicy>
    <soulPolicy>ActiveSpirit</soulPolicy>
    <durationPolicy>AnchorUpkeep</durationPolicy>
    <isUndead>true</isUndead>
    <isSpirit>true</isSpirit>
    <enforceNeeds>true</enforceNeeds>
    <enforceSocialPolicy>true</enforceSocialPolicy>
    <enforceGearPolicy>true</enforceGearPolicy>
    <enforceLifeStage>true</enforceLifeStage>
  </li>
</modExtensions>
<comps>
  <li Class="MagicFramework.PawnLifecycle.CompProperties_PawnLifecycleEnforcer" />
</comps>
```

## Example Lich

```xml
<modExtensions>
  <li Class="MagicFramework.PawnLifecycle.PawnLifecycleExtension">
    <bodyForm>PhylacteryReformed</bodyForm>
    <intelligence>FullSapience</intelligence>
    <needsPolicy>None</needsPolicy>
    <socialPolicy>FullRelationships</socialPolicy>
    <gearPolicy>FullGear</gearPolicy>
    <controlPolicy>FullPlayerControl</controlPolicy>
    <workPolicy>FullWork</workPolicy>
    <recoveryPolicy>PhylacteryReform</recoveryPolicy>
    <deathPolicy>ReturnToAnchor</deathPolicy>
    <soulPolicy>PhylacteryAnchored</soulPolicy>
    <durationPolicy>Permanent</durationPolicy>
    <isUndead>true</isUndead>
    <enforceNeeds>true</enforceNeeds>
    <enforceLifeStage>true</enforceLifeStage>
  </li>
</modExtensions>
<comps>
  <li Class="MagicFramework.PawnLifecycle.CompProperties_PawnLifecycleEnforcer" />
</comps>
```

## Example Arcane Construct

```xml
<modExtensions>
  <li Class="MagicFramework.PawnLifecycle.PawnLifecycleExtension">
    <bodyForm>Construct</bodyForm>
    <intelligence>TaskBound</intelligence>
    <needsPolicy>None</needsPolicy>
    <socialPolicy>None</socialPolicy>
    <gearPolicy>RestrictedLoadout</gearPolicy>
    <controlPolicy>HostileOnly</controlPolicy>
    <workPolicy>CombatOnly</workPolicy>
    <recoveryPolicy>Repair</recoveryPolicy>
    <deathPolicy>DropConstructMaterials</deathPolicy>
    <soulPolicy>ConstructCore</soulPolicy>
    <durationPolicy>Permanent</durationPolicy>
    <isConstruct>true</isConstruct>
    <enforceNeeds>true</enforceNeeds>
    <enforceSocialPolicy>true</enforceSocialPolicy>
    <enforceGearPolicy>true</enforceGearPolicy>
    <enforceLifeStage>true</enforceLifeStage>
  </li>
</modExtensions>
<comps>
  <li Class="MagicFramework.PawnLifecycle.CompProperties_PawnLifecycleEnforcer" />
</comps>
```
