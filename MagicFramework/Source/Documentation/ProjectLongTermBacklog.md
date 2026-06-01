# Long-Term Content Backlog

Long-range MFVanilla and MagicFramework content ideas extracted from ProjectTodo.md. Promote items back to ProjectTodo.md only when they enter an active release band.

### MF-051 Forbidden Lore Mind Control

Goal: add mind-control magic as a dangerous Forbidden Lore feature without making ordinary hostile AI or player control unstable.

Current status:
- The first implementation has moved into the active MFVanilla backlog as [MF-051 Forbidden Lore Mind Control](ProjectMFVanillaTodo.md#mf-051-forbidden-lore-mind-control).
- This long-term entry now represents expansion beyond Dominate Will: broader control effects, resistance rules, backlash, and advanced Forbidden Lore identity.

Design direction:
- treat mind control as forbidden, costly, and narratively risky rather than a routine crowd-control spell
- prefer short, readable effects first: compel movement, interrupt work, force a target to flee, temporary mental break influence, or brief ally/hostile confusion
- avoid permanent faction conversion, pawn ownership rewrites, or deep job-driver takeover until the smaller effects are reliable
- require clear player feedback, strong cooldown/cost pressure, and save/load-safe state cleanup

First implementation pass:
1. Validate Dominate Will and the temporary allegiance runtime in real combat.
2. Tune cost, duration, range, target restrictions, and cleanup based on that smoke test.
3. Only then choose the next Forbidden Lore control effect.


### MF-052 Illusionary Pawns

Goal: add illusionary pawns under Illusion research so the school has a distinct battlefield and deception identity.

Current active concept:
- First spell candidate: mirror images of the caster. These should appear as viable hostile targets, look like the caster, avoid duplicating weapons or inventory, have only token durability, and dissipate quickly if not destroyed.
- MVP pivot: `Phantom Reinforcements` uses generic illusory allies instead of caster-perfect copies, reducing appearance-copying risk while validating the same temporary pawn lifecycle path.
- Status: first MFVanilla implementation is smoke tested and behaves well after cleanup. The spell now gives the desired decoy effect, phantasms vanish on damage before ordinary death handling, and lifecycle render suppression is hidden behind optional Visuals logging.

Design direction:
- start with temporary decoy pawns or mirage pawns that distract enemies without becoming full colonists
- illusion pawns should have strict lifetime, cleanup, and save/load behavior
- avoid inventory, needs, training, medical, romance, ideology, and caravan complexity for the first pass
- make the player-facing difference between summoned creatures, undead, constructs, and illusions obvious

First implementation pass:
1. Completed first pass: `MFV_IllusoryReinforcement` pawn kind with lifecycle policies, marker hediff, no gear/work/needs, and vanish-on-injury behavior.
2. Completed first pass: `MF_PhantomReinforcements`, gated by `MFV_Illusion`, summons three brief illusory allies.
3. Smoke tested: spawning, hostile targeting/combat distraction, vanish-on-hit, death/corpse/mourning suppression, and normal-play log cleanup.
4. Watch remaining polish: save/load while phantasms are active, map transition behavior, proper illusion art/icon, and whether later true mirror images are worth the extra appearance-copying work.


### MF-053 Necromancy Undead Pawns

Goal: add undead pawns under Necromancy research as the first MFVanilla necromantic creature feature.

Framework dependency:
- Use the shared lifecycle foundation in [MF-063 Shared Undead And Construct Pawn Foundations](ProjectFrameworkBacklog.md#mf-063-shared-undead-and-construct-pawn-foundations) before expanding beyond the current temporary skeleton spell pattern.

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

Current status:
- The first planar foundation has been promoted into the active MFVanilla completion pass as [MF-055 Planar Magic Foundation And Validation](ProjectMFVanillaTodo.md#mf-055-planar-magic-foundation-and-validation).
- This long-term entry now represents expansion beyond the current gate/pocket-map foundation: richer hazards, expedition chains, threats, authored rewards, special sites, and deeper planar ecology.

Design direction:
- start as a controlled exploration loop rather than full new-world simulation
- possible forms: planar rift site, expedition incident, temporary pocket map, encounter chain, or costly scan that reveals a planar destination
- rewards should include rare scrolls, gemstones, artifacts, legendary weapon inputs, exotic threats, and future planar materials
- risks should be explicit: dangerous defenders, unstable exits, curses, pawn injury, or time pressure

First implementation pass:
1. Validate the current planar gate and pocket-map loop through the active MFVanilla task.
2. After the foundation is stable, choose the next expansion shape: hazard layer, threat encounter, resource expedition, planar reward chain, or new site family.
3. Reuse the active foundation where possible instead of creating a second travel/map lifecycle.


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

Status: first MFVanilla implementation is complete and smoke tested. `Temporal Resurrection` resurrects a viable corpse, keeps the pawn in temporal reconstruction while wounds reverse over time, and charges proportional skill-memory loss that caster level can reduce but not eliminate. `Borrowed Season` adds a practical Chronomancy utility spell by pulsing crop growth in a targeted area.

Design direction:
- distinguish this from Soulcraft resurrection, Necromancy undead, and AeternusFaith rites
- possible identity: rewind a recent death, restore from a temporal echo, reverse corpse decay, or resurrect with age/time side effects
- require strict limits so it cannot trivialize death: time window, rare materials, severe cooldown, memory/age injury, or pawn-specific anchor
- save/load and corpse/reference cleanup are release blockers

First implementation pass:
1. Completed: fresh-corpse `Temporal Resurrection` with temporal reconstruction and skill-memory loss.
2. Completed: `Borrowed Season` crop-growth utility field.
3. Watch balance, scroll pricing, icon polish, and whether deeper pawn-memory health snapshots are worth adding later.


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


### MF-064 Magical Transcendence And Arcane Relics

Goal: define a magic-native endgame arc so MFVanilla has a destination beyond ordinary colony survival or a spaceship-equivalent victory.

Design direction:
- treat Transcendence as the magical endgame umbrella: a colony-scale arc built from school mastery, planar materials, leyline control, arcane relics, and high-risk rituals
- avoid implementing the final arc until planar rewards, advanced materials, relic behavior, and capstone transformations have enough foundation
- use near-term work to plant prerequisites instead: exotic planar materials, relic fragments, high-tier research nodes, arcane sites, and map-scale magical infrastructure
- distinguish Transcendence from lichdom: lichdom is a Soulcraft character transformation, while Transcendence should be a colony/endgame arc that may include or compete with lichdom

Arcane relic direction:
- arcane relics should be magical artifacts with noteworthy map-wide effects, not just ideology-style relic objects with flavor text
- effects should be powerful but readable: mana recovery, disease suppression, planar gate stability, weather pressure, crop/plant changes, school empowerment, raid attraction, death protection, or ritual amplification
- each relic should have a cost/risk axis so map-wide benefits create story pressure rather than passive permanent bonuses
- relics can become rewards from planar expeditions, sealed vaults, grand rituals, or advanced research chains
- relics are a good bridge between planar purpose and endgame: exotic materials can forge, awaken, repair, or stabilize them

Possible Transcendence arc:
1. Discover or craft arcane relics tied to major schools or planes.
2. Gather exotic planar materials and advanced magical reagents.
3. Stabilize a leyline/gate/relic network on the player map.
4. Complete escalating ritual or site objectives while surviving magical consequences.
5. Choose an outcome: ascension, permanent planar threshold, colony-wide transformation, or another high-magic finale.

First planning pass:
1. Define 3-5 candidate arcane relic effects and their risks.
2. Identify which existing systems can host relic effects: buildings, comps, map components, enhancement rules, incidents, rituals, or planar gates.
3. Tie at least one planar/exotic material to a relic or capstone recipe so planar travel gains practical purpose.
4. Decide whether Transcendence should be a victory condition, a post-game state, or a repeatable endgame project.
5. Keep lichdom as a related Soulcraft capstone, not the whole magic endgame.


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


