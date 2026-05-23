# AeternusFaith RC1 Plan

This file preserves the current release-candidate design direction for AeternusFaith. The source inspiration is `Aeternus-Codex Vitae Sine Anima` and `Bonewright Adept`, both written for a Dungeons and Dragons context. Treat them as theme and taxonomy references, not as rules to translate literally into RimWorld.

## Core Release Promise

AeternusFaith RC1 should make death spiritually persistent, ritually actionable, and mostly invisible to the player until it matters.

The first release candidate should establish:
- a stable soul tracking baseline
- corpse and soul lookup for deceased-pawn events
- general haunting risk for unreleased souls
- ritual circles for the Bonewright cathedrae
- at least one working ritual per circle
- a more complete Ossanith foundation loop
- temporary and permanent spirit/undead lifecycle foundations
- a baseline spirit jobgiver and non-manifesting/manifesting behavior model
- a complete-enough ideology experience

## Two-Layer Model

### General Death And Soul Ecology

This layer applies to any tracked pawn soul, independent of cathedra doctrine.

- Pawn souls should be tracked by MagicFramework's world-level pawn memory/soul registry.
- RC1 should guarantee tracking for player-relevant pawns first: colonists, prisoners, slaves, guests, and important pawns on player maps.
- The eventual target is all humanlike pawns, once retention and cleanup policy is proven.
- When a pawn dies on-map, especially by violence or in a bad mental/emotional state, the soul may become a haunting risk if not released or otherwise ritually handled.
- Haunting spirits are a baseline consequence of unresolved death, not a feature owned by any single cathedra.
- The player should not normally see the soul tracker. It should remain available through dev tools and backend ritual/event lookups.
- If a corpse exists, systems should be able to resolve it from the associated soul record or locate the soul from the corpse.

### Cathedra Necromantic Practice

This layer is intentional undead creation through Bonewright doctrine.

- The Bonewright cathedrae can create or summon undead at ritual circles.
- In gameplay, the player may perform these rituals whenever prerequisites are satisfied.
- In lore, each cathedra only does so according to its doctrine.
- The undead created by each cathedra should reflect that cathedra's style, failure modes, and intended purpose.
- Ossanith remains the common praxis and gatekeeper: every Bonewright path begins with disciplined corpse/soul handling.

### Bonewright Order Membership

Concept:
- Only Bonewright pawns should perform Bonewright rites or maintain formal relationships with undead created by the cathedrae.
- The existing ideology role can represent a public or liturgical office, but it should not be the only way to mark a pawn as a Bonewright because that would limit the colony to one practitioner.
- Bonewright membership should be an order/initiation state layered on top of ideology, not just a single ideology role.

Likely progression:
- A colony may have a small number of Bonewrights, but not an unlimited number.
- Pawns should begin as Ossanith initiates because Ossanith is the foundational praxis.
- An anointing or initiation ritual can admit another pawn into the Bonewright order. RC1 uses existing ritual circles/centers for this instead of adding a separate anointment building, with a popup to select the Soulwarden/Bonewright conductor and initiate.
- Later, a traversal or dedication ritual can move a Bonewright from Ossanith foundation into another cathedra: Animara, Choralum, Shroudhymn, or Voressai.
- The ideology role, currently represented by Soulwarden-style content, can be the senior officiant or order authority that performs initiations and supervises doctrine.

Possible implementation:
- Add a hediff, comp, gene, ability tracker, or pawn memory extension that marks a pawn as a Bonewright and stores current cathedra alignment. RC1 currently uses `AF_BonewrightOssanithInitiate` as the first lightweight marker.
- Gate ritual conductor eligibility on this marker, not solely on ideology role. RC1 uses the Soulwarden/existing Bonewright only to officiate anointment; corpse/soul/undead rites require an anointed Bonewright.
- Keep a colony-level or map-level cap on Bonewright membership, or use escalating maintenance burdens so the practical number remains small.
- Use the ideology role as a prerequisite for initiating others rather than as the only Bonewright identity.

Maintenance burden:
- Being a Bonewright should have meaningful limits and obligations.
- Candidate model A: routine rites. Bonewrights must periodically perform observances, similar in spirit to royalty meditation, to maintain standing.
- Candidate model B: addiction-like need without normal harmful drug side effects. If neglected, the pawn loses Bonewright standing or access to rites rather than suffering a conventional chemical crash.
- Candidate model C: a custom need or hediff severity that rises/falls with rites performed, corpse care, meditation, or cathedra-specific practice.
- Preferred direction: use a custom need/hediff-style obligation rather than literal addiction if practical, because it communicates discipline and rite observance better than dependency.

Design caution:
- The system should support a few Bonewrights, create real tradeoffs, and prevent every colonist from becoming a casual necromancer.
- Failure to maintain Bonewright obligations should disable or degrade ritual access before it creates harsh colony-ending penalties.
- Voressai traversal may require additional safeguards, corruption risk, or explicit player consent because it is doctrine-fringe and dangerous.

## Soul Tracking Rules

RC1 behavior:
- Ensure soul/memory records exist for player-relevant pawns when they instantiate or enter tracked contexts.
- Record death tick, death map, death cell, cause context where available, mood/violence indicators where practical, and corpse anchor information if present.
- Death records should store a haunting risk score, risk reasons, mood at death, violent/abrupt death flags, final damage/culprit context, and mental-state-at-death where available.
- Haunting should be evaluated once shortly after death, stored on the soul record, delayed before first activity, and capped per map to avoid runaway spirit populations.
- Preserve records across save/load, corpse destruction, burial, map transitions, and spirit manifestation.
- Prefer marking a soul `Released` over deleting it immediately. Released records should block haunt, manifest, bind, and resurrect behavior, then become eligible for later cleanup when no system references them.
- Rituals such as the Ossanith rest/release rite should set the soul into a released/rested state and suppress haunting.
- Corpse destruction should not accidentally erase spiritually relevant records unless a ritual or explicit cleanup policy says the soul has been released, severed, invalidated, or consumed.

Dev-facing needs:
- Keep or improve the existing pawn memory dev viewer/actions.
- Add debug actions for creating/updating records, killing test pawns, marking released, marking corrupted, spawning or resolving spirits, and validating corpse lookup.

## Haunting Spirit Model

Haunting spirits have two states.

Not manifesting:
- invisible
- not a normal pawn
- no direct pawn interaction
- may move or influence things subtly
- may drift around a death site, corpse, grave, ritual circle, or emotional anchor
- may occasionally perform passion/identity-flavored actions, such as cooking, cleaning, repairing, hauling, tending, or other safe limited behaviors if technically practical
- often mischievous rather than malicious

Manifesting:
- visible or pawn-like enough to interact with jobs, targeting, ritual resolution, combat, exorcism, or player events
- should use a stable lifecycle with save/load, despawn, death/downing, and cleanup behavior
- can be temporary or permanent depending on source and ritual
- natural haunting spirits should manifest on pseudo-random, deterministic save-stable schedules and then return to not-manifesting
- Shroudhymn rite-created spectres are persistent manifestations by default; they should not fade merely because a manifestation timer elapsed
- Shroudhymn rite-created spectres are bound to their summoner, and a summoner may maintain only one such rite-bound spectre at a time.
- manifested spirits project an unsettling aura onto nearby living pawns; the baseline implementation is a short-lived `AF_EerieCold` hediff that drives a mild mood thought and falls off after the pawn leaves the spirit's presence

Malevolent hauntings:
- should usually be event-driven or corruption-driven
- should not be the default result of every bad death
- can become a future Voressai, dark magic, failed rite, or hostile incident surface

## Cathedrae And Undead Families

### Ossanith

Doctrine:
- death must be sealed
- the soul's passage is final and should be protected
- improper death creates leakage, corruption, body-rot, echoes, and haunting risk
- animated bodies are tools or remnants under strict controls, not returned loved ones

Gameplay identity:
- foundation circle and first complete vertical slice
- corpse hygiene, funerary order, ossuary preservation, skeletal command, soul release
- stable, obedient, functional body-undead
- Ossanith skeletons are husks, not returned pawns. A completed Ossanith skeleton rite lays the source soul to rest and animates only the remains.

Likely undead family:
- skeletons
- skeletal champions
- ossuary guardians
- silent servitors

RC1 target:
- complete the skeleton rite and ossuary loop
- add or define an Ossanith rite to put a soul to rest
- wire rites into soul release / haunting suppression
- make corpse/soul invalid states clear
- make Bonewright role requirements intentional and readable

Implementation note:
- `PawnSoulRiteUtility` in MagicFramework is the shared integration surface for rites that alter soul records. Ossanith ossuary and skeleton rites should use release/final-rest helpers, while other cathedrae may use bound or active-spirit helpers when their doctrine calls for it.

### Animara

Doctrine:
- the soul leaves memory, emotion, history, and bonds behind
- undeath can be a careful continuation of purpose through memory and identity
- the dead are not tools by default; they are echoes or fragments of self

Gameplay identity:
- memory-guided undeath
- family/relationship/identity-flavored spirits
- passion, skill, or role echoes
- ancestral stewardship

Likely undead family:
- echobound revenants
- memory spirits
- ancestor echoes
- semi-aware guardians or helpers

RC1 target:
- define the circle and one ritual
- at minimum, use Animara as the conceptual basis for identity/passion-flavored haunting behavior
- keep deep pseudo-relationship memory as follow-up unless it becomes necessary for the first ritual

### Choralum

Doctrine:
- life and death are harmonic states
- souls can be stabilized, clarified, preserved, and resonated
- undead can serve as remembrance, guidance, and protective harmony rather than simple labor

Gameplay identity:
- stabilization, calming, protection, mood/psychic resonance, grief handling
- reliquary-style guardians and memorial undead
- anti-haunt pressure or haunt soothing

Likely undead family:
- reliquary wardens
- resonant guardians
- calm revenants
- harmonic spirits

RC1 target:
- define the circle and one ritual
- likely first ritual should stabilize or soothe a soul/spirit rather than create a complex new pawn type
- deeper guardian/aura mechanics can follow after the soul system is stable

### Shroudhymn

Doctrine:
- undeath is a sworn, time-bound duty
- spirits are called for mission, oath, guardianship, messages, or sacred service
- when the task is complete, the spirit should be dismissed or laid to rest

Gameplay identity:
- temporary spirits
- manifesting/non-manifesting behavior
- subtle guardians, watchers, messengers, oath-bound shades
- best fit for the baseline custom spirit jobgiver

Likely undead family:
- veilbound shades
- oath-bound spirits
- temporary manifesting spectres
- task spirits

RC1 target:
- stabilize spectre/temporary spirit lifecycle
- support manifesting and not-manifesting states
- implement a narrow custom spirit jobgiver
- define at least one Shroudhymn ritual that creates or calls a temporary spirit with a clear duration, purpose, and cleanup path
- distinguish rite-bound spectres from naturally haunting spirits: rite spectres can remain manifested, while haunting spirits manifest intermittently

### Voressai

Doctrine:
- identity, flesh, and memory are fetters
- undeath is unraveling, hunger, dissolution, and voidward transition
- their creations are dangerous, unstable, and feared by other cathedrae

Gameplay identity:
- forbidden or fringe doctrine
- hostile/failure/event content
- rot, hunger, void pressure, dissolution, corruption

Likely undead family:
- hungering husks
- wraithspawn
- void-drifters
- unstable predatory undead

RC1 target:
- define the circle and one minimal ritual or lore surface only if time allows
- acceptable first release representation: ideology/research hints, failure states, or one dangerous debug/dev-facing prototype
- do not let Voressai expand the first release beyond the soul/ritual foundation

## Ritual Circle Requirements

RC1 should include ritual circles for all five cathedrae:
- Ossanith
- Animara
- Choralum
- Shroudhymn
- Voressai

Each circle should have:
- a buildable or placeable ritual focus
- research or ideology availability rules
- clear labels/descriptions
- at least one working ritual, even if simple
- stable save/load behavior
- clear invalid-state messages

Ossanith should be more complete than the others because it is the foundation praxis.

## First Ritual Candidates

Preferred RC1 ritual set:
- Ossanith: Raise Skeleton
- Ossanith: Ossuary Rite
- Ossanith: Put Soul To Rest
- Animara: Call Memory Echo or Bind Ancestor Echo
- Choralum: Harmonize Spirit or Soothe Restless Soul
- Shroudhymn: Call Veilbound Shade / Task-Bound Spirit
- Voressai: Mark For Dissolution, Unmake Remnant, or defer to failure/event content

If scope tightens, prioritize:
1. Ossanith release/rest rite
2. Shroudhymn temporary spirit rite
3. Animara or Choralum single stabilizing/identity ritual
4. Voressai as lore/failure hook

## Spirit And Undead Lifecycle Contract

Every created undead or spirit should answer:
- What source soul or corpse is used?
- Does the source soul remain active, bound, released, corrupted, consumed, or split?
- Does the corpse remain, move, get destroyed, become an anchor, or become the body?
- Is the undead temporary or permanent?
- Is it player-controlled, autonomous, duty-bound, or event-controlled?
- What happens when it is downed, killed, dismissed, despawned, resurrected, or map-removed?
- Can it save/load cleanly?
- Does it block or allow future haunting?

RC1 should favor simple, conservative answers over ambitious mechanics.

## Implementation Sequence

1. Soul registry hardening
   - Audit MagicFramework pawn memory coverage.
   - Ensure player-relevant pawns receive records.
   - Ensure death and corpse context are saved. MagicFramework now patches pawn death plus corpse spawn/despawn/destroy so records can anchor bodies after corpse creation timing settles.
   - Added death-context and haunting-risk scoring to soul records, with debug viewer exposure for tuning.
   - Added a haunting evaluator that rolls eligibility from haunting risk, schedules delayed hauntings, stores the decision, and enforces a configurable per-map cap.
   - Ensure release/rest state blocks haunt and manifestation.

2. Corpse/soul lookup helpers
   - Added safe lookup from corpse to record through MagicFramework's pawn memory registry.
   - Added safe lookup from record to corpse by corpse ID, with fallback search by inner pawn ID.
   - Avoid saved direct pawn references where stable IDs are safer.

3. Ossanith foundation
   - Finish skeleton and ossuary smoke testing.
   - Add soul release/rest rite.
   - Wire proper rites into soul state.
   - Improve invalid-state messages.

4. Spirit lifecycle foundation
   - Consolidate temporary and permanent spirit records.
   - Support manifesting and not-manifesting states.
   - Scheduled haunting records now create Aeternus spectral entities in a not-manifesting state, then use separate pseudo-random schedules for subtle haunting actions and visible manifestations.
   - Ensure save/load, despawn, death/downing, map removal, and cleanup are predictable.

5. Baseline spirit jobgiver
   - Start narrow: idle near anchor, drift, guard, subtle interaction.
   - Implemented a custom spectre think tree/jobgiver that makes manifested spectres rest most of the time, with occasional drifting near their anchor or rite-bound summoner.
   - Add passion/identity-flavored actions only when safe.
   - Keep malicious behavior event-driven.

6. Cathedra circles and first rituals
   - Add all five circles as player-facing doctrine anchors.
   - Implement one ritual per circle, with Ossanith deepest.
   - Use simple ritual effects where full undead families are not ready.

7. Ideology completion
   - Verify memes, precepts, roles, rituals, apparel, research, buildings, and starting/preset behavior.
   - Ensure the player can start with the ideology and understand the first death-care loop.
   - Preserve the distinction between ideology office and Bonewright order membership.
   - Decide whether RC1 needs a lightweight Bonewright membership marker, or whether this remains a documented follow-up after the soul/ritual foundation is stable.

8. Packaging and release hygiene
   - Verify `About.xml`, dependencies, preview/icon assets, assembly output, XML load, local deployment, and Workshop/GitHub packaging.

## Verification Checklist

- New player-relevant pawns receive soul records.
- Death on map records death context and corpse anchor.
- A corpse can find its soul record.
- A soul record can find its corpse when present.
- Ossanith release/rest rite suppresses haunting.
- Released souls do not manifest, haunt, bind, or resurrect unless explicitly allowed by future doctrine.
- Resurrected pawns return their soul record to active, cancel pending hauntings, and despawn any spirit entity sourced from that pawn.
- Skeleton rite works, cleans up source corpse intentionally, and updates soul/corpse state.
- Ossuary rite works and updates soul/corpse state.
- Shroudhymn temporary spirit can manifest, unmanifest, save/load, and clean up.
- Non-manifesting spirits can exist without visible pawn clutter.
- Scheduled hauntings create active spectral entities without immediately spawning a visible pawn.
- Baseline spirit jobgiver does not break normal pawn AI.
- Each cathedra circle can be built/unlocked and has at least one ritual.
- Bonewright ritual access has a documented gating strategy, even if the full order membership system is deferred.
- Ideology generation and role assignment are stable.
- Ritual dialogs explain invalid corpse/conductor/circle choices.
- Undead and spirits handle downing, death, despawn, map removal, and save/load.

## Explicit Deferrals

- Full all-humanlike soul tracking until retention and cleanup are proven.
- Rich family-relationship ritual solving unless needed for first release.
- Deep Animara pseudo-relationship memory.
- Full Choralum aura/guardian suite.
- Full Voressai horror/hostile ecosystem.
- Complete item system, beyond any item required to make rituals work or feel legible.
- Custom wall auto-joining unless it becomes a presentation blocker.
- Full Bonewright order progression, traversal rituals, and maintenance-obligation mechanics may be deferred if RC1 only needs role-gated ritual access.
