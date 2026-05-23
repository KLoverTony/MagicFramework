# Long-Term Content Backlog

Long-range MFVanilla and MagicFramework content ideas extracted from ProjectTodo.md. Promote items back to ProjectTodo.md only when they enter an active release band.

### MF-051 Forbidden Lore Mind Control

Goal: add mind-control magic as a dangerous Forbidden Lore feature without making ordinary hostile AI or player control unstable.

Design direction:
- treat mind control as forbidden, costly, and narratively risky rather than a routine crowd-control spell
- prefer short, readable effects first: compel movement, interrupt work, force a target to flee, temporary mental break influence, or brief ally/hostile confusion
- avoid permanent faction conversion, pawn ownership rewrites, or deep job-driver takeover until the smaller effects are reliable
- require clear player feedback, strong cooldown/cost pressure, and save/load-safe state cleanup

First implementation pass:
1. Identify which effects can be authored with existing mental-state, hediff, job, or forced-target primitives.
2. Add one narrow validation spell under `MFV_ForbiddenLore`.
3. Smoke test target validity, downed/dead cleanup, save/load, prisoner/colonist/hostile behavior, and faction edge cases.


### MF-052 Illusionary Pawns

Goal: add illusionary pawns under Illusion research so the school has a distinct battlefield and deception identity.

Design direction:
- start with temporary decoy pawns or mirage pawns that distract enemies without becoming full colonists
- illusion pawns should have strict lifetime, cleanup, and save/load behavior
- avoid inventory, needs, training, medical, romance, ideology, and caravan complexity for the first pass
- make the player-facing difference between summoned creatures, undead, constructs, and illusions obvious

First implementation pass:
1. Define the minimal illusion pawn kind and lifetime behavior.
2. Add one spell or item source under `MFV_Illusion`.
3. Smoke test spawning, targeting, combat distraction, expiry, map transition, save/load, and cleanup.


### MF-053 Necromancy Undead Pawns

Goal: add undead pawns under Necromancy research as the first MFVanilla necromantic creature feature.

Design direction:
- distinguish MFVanilla undead from AeternusFaith ritual undead; MFVanilla should be spell/research driven, while AeternusFaith remains ideology/rite driven
- begin with temporary or limited-control undead before persistent colony members
- require corpse selection, reachability, reservation, faction assignment, expiry or upkeep, and cleanup to be explicit
- avoid pseudo-relationship memory until MF-021 is ready unless the first pass stays temporary

First implementation pass:
1. Choose the first undead type: skeleton, shambling corpse, bound wraith, or similar low-scope pawn.
2. Add a Necromancy-gated spell and scroll path.
3. Smoke test corpse targeting, map cleanup, save/load, death/despawn behavior, and drafted/undrafted control expectations.


### MF-054 Fleshcraft Golems

Goal: add golems under Fleshcraft research as risky created servants or guardians rather than ordinary summons.

Design direction:
- use Fleshcraft for body-made or stitched constructs; keep earth/stone golems under elemental or construct content if that distinction matters later
- first pass should be a controlled creature creation path, not a broad pawn crafting framework
- avoid permanent colonist-equivalent behavior until control, upkeep, and balance are clear
- connect costs to forbidden/advanced materials so Fleshcraft feels distinct from Arcane Forge automata

First implementation pass:
1. Decide whether the first Fleshcraft golem is temporary, bonded, or persistent with upkeep.
2. Prototype one pawn kind and creation spell/recipe under `MFV_Fleshcraft`.
3. Smoke test creation, commandability, expiry/upkeep, death drops, save/load, and cleanup.


### MF-055 Planar Exploration

Goal: add a Planar Magic feature that lets advanced colonies interact with other planes through events, expeditions, or special sites.

Design direction:
- start as a controlled exploration loop rather than full new-world simulation
- possible forms: planar rift site, expedition incident, temporary pocket map, encounter chain, or costly scan that reveals a planar destination
- rewards should include rare scrolls, gemstones, artifacts, legendary weapon inputs, exotic threats, and future planar materials
- risks should be explicit: dangerous defenders, unstable exits, curses, pawn injury, or time pressure

First implementation pass:
1. Choose the minimum viable loop: world site, temporary map, or event chain.
2. Reuse arcane site profile/generation infrastructure where possible.
3. Smoke test generation, travel, map closure, reward extraction, save/load, and abandonment.


### MF-056 Grand Sorcery Legendary Weapons And Buff Ritual

Goal: make Grand Sorcery feel like colony-scale magic by adding legendary weapons and a high-impact magic buff ritual.

Design direction:
- legendary weapons should be rare capstone items, not an infinite ordinary production tier
- buff rituals should affect a colony, party, room, or limited group with strong cost/cooldown/story constraints
- prefer reusing MagicFramework item abilities, passive statuses, and ritual/action-list infrastructure before inventing a separate system
- avoid power creep that makes Arcane Forge weapons obsolete too quickly

First implementation pass:
1. Define one legendary weapon acquisition path: quest reward, grand forge recipe, planar reward, or ritual outcome.
2. Define one Grand Sorcery buff ritual with a clear target and duration.
3. Smoke test save/load, cooldowns, stacking, removal, generated descriptions, and UI clarity.


### MF-057 Chronometric Resurrection

Goal: add Chronomancy resurrection that restores a pawn through time magic rather than ordinary healing or necromancy.

Design direction:
- distinguish this from Soulcraft resurrection, Necromancy undead, and AeternusFaith rites
- possible identity: rewind a recent death, restore from a temporal echo, reverse corpse decay, or resurrect with age/time side effects
- require strict limits so it cannot trivialize death: time window, rare materials, severe cooldown, memory/age injury, or pawn-specific anchor
- save/load and corpse/reference cleanup are release blockers

First implementation pass:
1. Decide whether the effect targets a fresh corpse, grave, pawn record, or pre-made temporal anchor.
2. Implement one narrow Chronomancy-gated spell or ritual.
3. Smoke test corpse state, missing body cases, world pawn references, save/load, failure feedback, and side effects.


### MF-058 Soulcraft Lichdom

Goal: add lichdom under Soulcraft as a major advanced transformation with strong costs, risks, and identity.

Design direction:
- treat lichdom as a capstone character transformation, not just a buff
- base the lich body on a custom mechanoid-like pawn so it inherits the desired immunities, needs behavior, and tireless body traits while presenting as an arcane undead entity
- unlock lichdom from `MFV_Soulcraft`; later tuning can decide whether it also requires an Animus Domini discipline, Forbidden Lore, Necromancy mastery, or rare reagents
- require the player to construct a high-level phylactery building before transformation; this object becomes the pawn-specific soul anchor and should be expensive, vulnerable, and meaningful to defend
- start transformation through a building ritual: a selected player pawn performs the rite, the mortal body is destroyed or consumed, and a player-controlled lich pawn is spawned in its place
- store the original pawn identity in the phylactery, preserving enough pawn data to keep name, backstory-facing identity, learned magic, discipline, relationships, and resurrection/reform ownership coherent
- when the lich body is destroyed, the phylactery should enter a charging state; after several days, it can regenerate the lich using an available humanoid corpse as the vessel
- decide whether regeneration consumes any humanoid corpse, requires a fresh/intact corpse, preserves corpse identity consequences, or creates faction/social penalties for using colonist or prisoner remains
- phylactery loss should be a true failure state: if the phylactery is destroyed while the lich body is absent or reforming, the anchored pawn should be permanently lost or require an extreme recovery path
- avoid implementing until undead, resurrection, and persistent pawn state patterns are stable enough to support it
- player feedback, save/load, death cleanup, and exploit prevention are all release-critical

First implementation pass:
1. Build the minimum viable content shell: lich `ThingDef`/`PawnKindDef`, phylactery `ThingDef`, job def, ritual comp, and dev/test command access.
2. Implement one-way transformation from a player pawn into the lich body, including transfer of MagicFramework runtime state and phylactery identity storage.
3. Add the death/reform loop: detect lich body death, charge the phylactery for several days, validate an available humanoid corpse, consume it, and respawn the same lich identity.
4. Smoke test save/load, destroyed phylactery cases, missing corpse cases, hostile corpse/faction cases, map transfer, caravan/world-pawn references, relationship/name continuity, magic runtime state, and exploit paths.


### MF-060 Paintings And Platinum Consideration

Goal: evaluate whether paintings as decoration and platinum as a trade good belong in the first-party content set.

Consideration notes:
- paintings could add visual variety for colonies, ritual rooms, and arcane/religious spaces, but need a clear art/category role beyond duplicating vanilla sculpture
- platinum could be a high-value trade good, advanced magic reagent, legendary weapon input, or planar reward, but should not dilute silver/gold/gemstone economies
- do not implement either item until a release band needs them for presentation, economy, or reward structure

Decision checklist:
1. Identify which mod owns the content: MFVanilla, AeternusFaith, or a shared decorative pack.
2. Decide whether the content adds distinct gameplay or only visual variety.
3. If approved, split implementation into concrete follow-up tasks.


### MF-024 Magic Tools And Weapons

Goal: explore a framework/content layer for magic equipment.

Ideas:
- wands: cast or grant spells
- staves: boost caster level, spell power, or grant abilities
- swords: martial spell delivery, enchantments, or element riders
- armor: mana capacity, protection fields, ritual bonuses, domain affinities

Open design questions:
- Are these plain apparel/equipment comps, spell containers, requirement modifiers, or a separate enchantment system?
- Should equipment grant spells, modify known spells, or only improve casting?
- How should NPCs use them?


### MF-025 Celestial And Weather Event Hooks

Goal: make solar eclipses, auroras, solar flares, rain, wind, and unusual weather matter to magic.

Near-term content ideas:
- eclipse empowers death/shadow
- aurora empowers arcane/spirit
- rain weakens fire and empowers aquamancy
- wind empowers aeromancy

Implementation notes:
- Add after scalar factors are wired broadly enough to make rules visible in play.
- Consider event-specific incidents only after enhancement rules feel good.


### MF-026 Persistent Visual Support

Goal: keep visuals as a first-class framework feature rather than repeated fleck hacks.

Future needs:
- loadable tight personal shield texture for maintained overlays
- cloned hidden magic shield-belt apparel experiment, if vanilla visuals are worth borrowing
- temporary-equip spell action for spawned apparel and cleanup on expire/break/death/drop
- calmer sustained ambient visuals beyond repeated flecks
- optional multi-point patterns, persistent sounds, and stateful visuals
- validate draw order and scale against vanilla shield belt visuals


### MF-027 Validation Spell Icons

Goal: add custom gizmo icons for future validation spells as they are added.

Current wired icons include all current MFVanilla spell defs:
- `MF_ArcSeeker`, `MF_BlessingOfVigor`, `MF_BlinkStep`, `MF_ChainLightning`, `MF_CreateFood`, `MF_DelayedBlastRune`, `MF_Disintegrate`, `MF_EarthCall`, `MF_Fireball`, `MF_Firebolt`, `MF_FlameField`, `MF_ForceField`, `MF_ForcePull`, `MF_ForcePush`, `MF_Freeze`, `MF_Haste`, `MF_Heal`, `MF_ManaShield`, `MF_Might`, `MF_Regeneration`, `MF_RepulsionWard`, `MF_RescueRecall`, `MF_Resurrection`, `MF_RuneTrap`, `MF_SummonDog`, `MF_ThreatSpike`, `MF_Tornado`, `MF_Transposition`, `MF_TriagePulse`, `MF_WallOfFire`, and `MF_WatersEmbrace`.
- Runtime gizmo icon loading first uses authored `gizmoIconPath`, then falls back to `UI/Gizmos/Spells/{defName}` before the generic DesirePower icon.


### MF-029 Real Fire Integration For Wall Of Fire

Goal: decide whether `Wall of Fire` should use real RimWorld fire objects or remain a custom magical hazard.

Risks:
- real fire can spread, interact with rain/snow/fuel, and create cleanup surprises
- custom hazards are more predictable but may feel less integrated


### MF-030 Persistent World Objects

Goal: represent some long-lived spell effects as world/map objects rather than only runtime state.

Possible uses:
- wards
- beacons
- ritual anchors
- lingering curse/blessing sites
- map hazards created by major spells


