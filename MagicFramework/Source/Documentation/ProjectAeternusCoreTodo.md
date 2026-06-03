# Aeternus Core Todo

Aeternus Core release-candidate and follow-up planning extracted from ProjectTodo.md. The source folder and many code namespaces still use `AeternusFaith`, but the public mod name is now Aeternus Core because RC1 includes both the Faith of Aeternus frame and a larger Bonewright order gameplay foundation.

Primary design guide: [AeternusCoreRC1Plan.md](../../../AeternusCore/Documentation/AeternusCoreRC1Plan.md) documents the RC1 soul ecology, Bonewright cathedrae, undead families, ritual-circle plan, and implementation sequence.

### MF-039 Aeternus Core First Edition

Goal: get Aeternus Core ready for its first public release as an early-development death, faith, and Bonewright ritual content mod.

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
- The universal Bonewright lectern now exposes rites based on adjacent supported ritual circles, hides irrelevant ritual gizmos, and shows one disabled fallback gizmo when no supported circle is adjacent.
- Choralum's `Animate Reliquary Warden` rite is implemented and smoke tested. It requires a humanlike corpse plus nearby plate/flak armor, consumes both, and creates `AF_ReliquaryWarden`, a tougher armored skeletal undead than the Ossanith Skeleton. It now uses the limited-labor intelligence tier and inherits a reduced practical skill echo from the source pawn.
- Animara's `Animate Echobound Revenant` rite has a first real pawn kind/race/job path. It no longer borrows the Ossanith skeleton result; it creates `AF_EchoboundRevenant`, a skeletal limited-labor undead with task-bound echoes and reduced practical source-skill copying.
- Current animation rites enforce one bound undead minion per Bonewright through the lifecycle master binding. Additional animation attempts are rejected before corpse/armor consumption.
- Bound undead minions follow drafted masters through the MagicFramework lifecycle escort loop and default to attacking hostiles near their drafted master.
- Ossanith now provides a dismissal rite for the Bonewright's bound undead minion. The minion walks to the ritual circle/ossuary center, waits for dismissal, is destroyed, and fills the ossuary bone box with a dismissal obituary.
- Bonewright membership now exists as `AF_BonewrightOssanithInitiate`. Existing ritual circles/centers expose a Bonewright anointment popup, and Soulwardens or existing Bonewrights can anoint new initiates.
- Ossuary, skeleton, and shade-calling rite conductors must be anointed Bonewrights.
- Skeleton, ossuary, dismissal, and shade-calling ritual setup now reports specific invalid-state reasons for wrong corpse, non-Bonewright conductors, reachability, reservations, missing targets, already-bound shades, and existing bound minions.
- Spectral debug actions have been hardened for listing, spawning, manifesting, clearing, and smoke-test inspection.
- June 3, 2026 RC1 validation passed for the listed Bonewright anointment, ritual dialog, universal lectern, Echobound Revenant, Reliquary Warden, Veilbound Shade, bound-minion, master-loss/lost-undead, dismissal/bone-box, and undead aura checks. The dismissal ceremony timing quirk is now fixed and regression tested: the conductor waits for the minion to reach the ossuary before proceeding.

First-edition emphasis:
- Ossanith skeleton and ossuary loops should be reliable and understandable.
- Shroudhymn Veilbound Shade content should be stable enough that it does not leave stale pawns, spectral state, or player confusion. Rite-bound shades are currently persistent; natural hauntings are the first model for intermittent manifestation.
- Animara's first Echobound Revenant implementation is in place and now needs live smoke testing and tuning: limited intellect, retained echoes, master protection, and simple-task ability should be verified against Ossanith Skeleton expectations.
- Choralum now has its first player-facing summon through the Reliquary Warden. Its limited-labor tier and reduced practical skill echoes need live tuning; deeper Choralum guardian/aura mechanics remain follow-up.
- Shroudhymn should align its spectre foundation with the Veilbound Shade: an oath-bound spirit with some intellect, no ordinary physical work, reduced practical skill echoes for identity/combat/future spectral interactions, and limited ability to harm living hostiles.
- Voressai should move toward the Hungering Husk: a fleshy, controlled undead that is dangerous and poor at complex work, with mundane tasks available only when directed.
- Master death/destruction should not leave bound undead as ordinary servants. Absence is acceptable, but if the master is truly gone the minion should enter a delayed instability window and eventually become a lost undead if not resolved: Hollowborn, Fractured Echobound, Wailwright, Errant Soul, or Void Drifter depending on cathedra.
- First-pass lost-undead behavior is implemented for current bound undead. When the master dies or is destroyed, the minion starts a grace timer; if unresolved, it is renamed to its lost form, unbound, and begins rampaging against nearby living pawns.
- Spawned undead now project `AF_UnnervingAura` onto nearby living non-Bonewright pawns. Ordinary undead apply the lower severity, while Veilbound Shades and the planned Hungering Husk use the stronger severity. The effect lingers for several hours and excludes undead and anointed Bonewrights.
- Bonewright role requirements should make ritual access feel intentional rather than arbitrary. The Soulwarden is now the order office/initiator, while Bonewright membership is the actual ritual-access marker.

Current RC1 priorities, in order:
1. Decide whether Voressai's Hungering Husk is player-facing, dev-facing, or documented-only for RC1. Current recommendation: keep Voressai to lore/failure hooks unless packaging review exposes a coherence gap.
2. Review ideology completeness: memes, precepts, roles, apparel, research gates, starting ideology/preset, and build availability.
3. Packaging and release hygiene: `About.xml`, dependencies, preview/icon assets, assembly output, XML load, local deployment, and Workshop/GitHub packaging expectations.
4. Prepare first-edition release notes from the validated feature surface.

Intentionally paused:
- Haunting resolution, exorcism, or release of active hauntings should not be a simple cleanup action. Leave this design open until the haunting-response plan is more satisfying.
- Deep spirit behavior, family visits, remembered work, and passion-flavored activity are exciting but should wait until the current lifecycle and priority list are stable.


### MF-059 Aeternus Core Decorative And Religious Statues

Goal: add decorative and religious statues that strengthen Aeternus Core presentation and ritual spaces.

Design direction:
- start with placeable art/building defs that fit ossuary, bonewright, grave, shrine, and ritual-room themes
- support beauty, room impressiveness, ritual flavor, and ideology presentation without needing new ritual code
- prefer a small coherent set over many nearly identical objects

First implementation pass:
1. Define the first statue set and research/ideology availability rules.
2. Add textures, thing defs, costs, categories, descriptions, and room/stat effects.
3. Smoke test placement, minification, beauty/room stats, ritual room compatibility, and save/load.


### MF-019 Ritual Dialog Improvements

Goal: make Aeternus Core ritual setup clearer and more polished.

Status: implementation complete for the current RC1 ritual surface. Remaining work is validation under MF-039 rather than a separate active slice.

Current state:
- MagicFramework provides `Dialog_ParticipantSelection` as a reusable participant-selection shell.
- The reusable dialog supports corpse selection plus pawn buckets for conductor, audience, and available pawns.
- Bucket rows use pawn/corpse icons, disabled-row reasons, and a validation summary before accept.
- Aeternus Core skeleton, Choralum warden, ossuary, and spectre rite dialogs now use thin adapters over the shared participant dialog.
- The shared accept button scales for longer labels such as `Animate Reliquary Warden`.
- Bonewright anointment uses its own compact popup and follows the same validation posture.

Completed first pass:
- Replaced plain checkbox/radio lists with pawn/corpse rows.
- Shows why a corpse or conductor is unavailable.
- Surfaces reachability, reservation, role, corpse-state, ingredient, and existing-bound-minion failure reasons where practical.
- Keeps the current UI compact enough for the RC1 ritual flow.

Validation folded into MF-039:
- Smoke test the specific reachability, reservation, role, corpse-state, missing-ingredient, existing-bound-minion, and Bonewright conductor reasons exposed by the current ritual dialogs.
- Smoke test skeleton, ossuary, Choralum, spectre, and Bonewright anointment dialogs at small resolutions.
- Consider dedicated slot labels and optional min/max participant counts only if future rites need them.


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

Goal: improve Aeternus Core wall visuals with custom joining.

Notes:
- Likely needs atlas art, neighbor detection, and careful testing around blueprints, frames, minified things, corners, and save/load.


