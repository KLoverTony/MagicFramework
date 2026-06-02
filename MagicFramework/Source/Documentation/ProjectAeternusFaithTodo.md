# AeternusFaith Todo

AeternusFaith release-candidate and follow-up planning extracted from ProjectTodo.md.

Primary design guide: [AeternusFaithRC1Plan.md](../../../AeternusFaith/Documentation/AeternusFaithRC1Plan.md) documents the RC1 soul ecology, Bonewright cathedrae, undead families, ritual-circle plan, and implementation sequence.

### MF-039 AeternusFaith First Edition

Goal: get AeternusFaith ready for its first public release as a focused faith/ritual content mod.

Current implemented baseline:
- MagicFramework soul tracking now covers player-relevant pawns, death context, corpse anchors, resurrection cleanup, release state, haunting risk scoring, haunting scheduling, per-map haunting caps, and dev-mode inspection.
- Ossanith skeleton and ossuary rites are wired into the soul registry. Ossanith-created skeletons release the source soul and treat the skeleton as a husk, not the returned pawn.
- Natural hauntings can be scheduled from unreleased soul records, begin as non-manifesting spectral entities, perform subtle actions, and manifest intermittently.
- Manifested spirits have a baseline aura through `AF_EerieCold`, producing a mild disturbing-presence mood effect on nearby living pawns.
- Shroudhymn shade-calling rites create persistent rite-bound Veilbound Shades. A summoner can maintain only one rite-bound shade at a time.
- Spectral entities now distinguish persistent rite-bound shades from natural intermittent hauntings.
- Veilbound Shades use the existing spectre think tree/jobgiver foundation, keeping them inactive most of the time with occasional movement near their anchor or summoner.
- Manifested Veilbound Shades now behave as non-player guest pawns rather than player-managed allied pawns; smoke testing confirmed ownership/control expectations, save/load, death/downing, map removal, and cleanup behavior.
- AF skeleton and shade pawn creation now route through a reusable undead pawn factory, so future close variants can share template generation, lifecycle enforcement, source ideology/identity transfer, race markers, xenotypes, and appearance setup instead of copying the old skeleton/spectre conversion path.
- Bonewright membership now exists as `AF_BonewrightOssanithInitiate`. Existing ritual circles/centers expose a Bonewright anointment popup, and Soulwardens or existing Bonewrights can anoint new initiates.
- Ossuary, skeleton, and shade-calling rite conductors must be anointed Bonewrights.
- Skeleton, ossuary, and shade-calling ritual setup now reports specific invalid-state reasons for wrong corpse, non-Bonewright conductors, reachability, reservations, missing targets, and already-bound shades.
- Spectral debug actions have been hardened for listing, spawning, manifesting, clearing, and smoke-test inspection.

First-edition emphasis:
- Ossanith skeleton and ossuary loops should be reliable and understandable.
- Shroudhymn Veilbound Shade content should be stable enough that it does not leave stale pawns, spectral state, or player confusion. Rite-bound shades are currently persistent; natural hauntings are the first model for intermittent manifestation.
- Animara should move toward the Echobound Revenant: a skeletal undead with limited intellect, retained echoes, master protection, and simple-task ability. Deep pseudo-relationship memory can remain follow-up unless it becomes necessary for this first summon.
- Choralum should move toward the Reliquary Warden: an armored skeletal guardian that protects its master and performs mundane tasks before deeper guardian/aura mechanics.
- Shroudhymn should align its spectre foundation with the Veilbound Shade: an oath-bound spirit with some intellect, very little physical agency, and limited ability to harm living hostiles.
- Voressai should move toward the Hungering Husk: a fleshy, controlled undead that is dangerous and poor at complex work, with mundane tasks available only when directed.
- Bonewright role requirements should make ritual access feel intentional rather than arbitrary. The Soulwarden is now the order office/initiator, while Bonewright membership is the actual ritual-access marker.

Current RC1 priorities, in order:
1. Smoke-test the Bonewright anointment loop: circle command, popup selection, Soulwarden bootstrap, cap behavior, save/load, and rite gating.
2. Stabilize the existing undead and spirit lifecycle under normal play: spectre guest ownership/control, manifest/unmanifest, load while manifested, clear map, pawn death/downing, and debug cleanup have passed smoke testing; keep resurrection/source-soul edge cases under observation.
3. Smoke-test ritual invalid-state messaging in the setup dialogs and start-job rejection paths, especially "not a Bonewright", "cannot reserve", "cannot reach", "wrong corpse", "missing circle", and "already bound".
4. Finish the cathedra circle surface around simple ritual circles: confirm Ossanith, Animara, Shroudhymn are functional; add or stub Choralum and Voressai ritual centers if they are not yet player-facing.
5. Define the first summon implementation for each cathedra in small, generator-backed slices: Ossanith Skeleton, Echobound Revenant, Reliquary Warden, Veilbound Shade, and Hungering Husk.
6. Decide which of the non-Ossanith summons are RC1 player-facing and which remain documented or dev-facing until their lifecycle/control rules feel good.
7. Review ideology completeness: memes, precepts, roles, apparel, research gates, starting ideology/preset, and build availability.
8. Packaging and release hygiene: `About.xml`, dependencies, preview/icon assets, assembly output, XML load, local deployment, and Workshop/GitHub packaging expectations.

Intentionally paused:
- Haunting resolution, exorcism, or release of active hauntings should not be a simple cleanup action. Leave this design open until the haunting-response plan is more satisfying.
- Deep spirit behavior, family visits, remembered work, and passion-flavored activity are exciting but should wait until the current lifecycle and priority list are stable.


### MF-059 AeternusFaith Decorative And Religious Statues

Goal: add decorative and religious statues that strengthen AeternusFaith presentation and ritual spaces.

Design direction:
- start with placeable art/building defs that fit ossuary, bonewright, grave, shrine, and ritual-room themes
- support beauty, room impressiveness, ritual flavor, and ideology presentation without needing new ritual code
- prefer a small coherent set over many nearly identical objects

First implementation pass:
1. Define the first statue set and research/ideology availability rules.
2. Add textures, thing defs, costs, categories, descriptions, and room/stat effects.
3. Smoke test placement, minification, beauty/room stats, ritual room compatibility, and save/load.


### MF-019 Ritual Dialog Improvements

Goal: make AeternusFaith ritual setup clearer and more polished.

Current state:
- MagicFramework provides `Dialog_ParticipantSelection` as a reusable participant-selection shell.
- The reusable dialog supports corpse selection plus pawn buckets for conductor, audience, and available pawns.
- Bucket rows use pawn/corpse icons, disabled-row reasons, and a validation summary before accept.
- AeternusFaith skeleton, ossuary, and spectre rite dialogs now use thin adapters over the shared participant dialog.

Possible first pass:
- Replace plain checkbox/radio lists with pawn rows that include portraits.
- Show why a corpse or conductor is unavailable.
- Surface reachability/reservation failure reasons where practical.
- Keep the UI compact enough for small screens.

Remaining work:
- Smoke test the specific reachability, reservation, role, corpse-state, and Bonewright conductor reasons now exposed by skeleton, ossuary, and spectre ritual dialogs.
- Consider dedicated slot labels and optional min/max participant counts if future rites need them.
- Smoke test the skeleton, ossuary, spectre, and Bonewright anointment dialogs at small resolutions.


### MF-020 Psychic Sensitivity In Haunting Effects

Goal: make haunting and spectral systems react to pawn psychic sensitivity.

Possible behavior:
- psychic sensitivity scales chance to notice, suffer, resist, or amplify haunt effects
- psychically deaf pawns are less affected or immune to some haunt cues
- highly sensitive pawns can become preferred targets or stronger conduits

Implementation notes:
- Keep early changes as tuning factors, not a new progression system.
- Add clear log/debug output while tuning.


### MF-021 Undead Pseudo-Relationship Memory

Goal: preserve family-memory flavor without allowing undead to resume normal social relationships.

Problem:
- Normal RimWorld relationships can cause strange or inappropriate behavior if an undead skeleton tries to resume spouse/parent/child social roles.

Target behavior:
- Store a separate memory list for important source-corpse relationships.
- Apply readable effects to the undead and related living pawns.
- Avoid normal relationship graph entries unless explicitly intended.

Complexity notes:
- Needs save/load.
- Needs relationship-copy policy.
- Needs UI/inspect or thought/status feedback.


### MF-023 Custom Wall Atlas And Auto-Joining

Goal: improve AeternusFaith wall visuals with custom joining.

Notes:
- Likely needs atlas art, neighbor detection, and careful testing around blueprints, frames, minified things, corners, and save/load.


