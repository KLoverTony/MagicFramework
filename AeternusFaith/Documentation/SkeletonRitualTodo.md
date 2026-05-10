# Skeleton Ritual Status And TODO

Current implemented slice:
- The Ossanith lectern can start a skeleton rite when orthogonally adjacent to an Ossanith Circle.
- A conductor brings a corpse to the circle, performs the rite, destroys the corpse, and spawns a player-faction skeleton pawn.
- The raised pawn is converted to `AF_SkeletonRace` / `AF_Skeleton` and named after the source corpse.
- Skeletons use custom body/head defs and the provided skeleton graphics through `AF_SkeletonThin` and `AF_SkeletonHead`.
- Skeletons receive undead cleanup/state handling through `Comp_SkeletonUndeadCleanup`, `AF_UndeadNature`, `AF_SkeletalBody`, and `AF_SkeletonXenotype` when available.
- The race has no hunger/rest/breath, no bleeding, no learning, strong temperature/tox resistance, psychic immunity, modest armor, slower movement, and a restricted work set.
- Rite attendees can be assigned through the dialog and are released when the ritual finishes.

## Resources

- Use https://rimworldwiki.com/wiki/Modding_Tutorials as a reference if needed.

## Visuals

- Current skeleton pawn visuals use the provided `Textures/Things/Undead/Skeleton_*` art through supported body/head defs.
- Confirm in-game orientation and atlas behavior:
  - south/front
  - east
  - north/back
  - west mirrored from east, if supported
- Revisit and improve the skeleton art after the functional pawn behavior is stable.
- The current ritual command icon uses the Ossanith Circle texture; add a dedicated low-memory-safe skeleton rite icon later.

## Undead Pawn Definition

- The skeleton is currently a custom humanlike pawn race with undead hediff/comps and restricted work behavior.
- Preserve save/load behavior for any custom master binding or undead state.

## Core Biology

- Implemented: no eating, no sleeping, no breathing, no bleeding, no learning, no skill decay by learning factor, no recreation/comfort by undead cleanup, infection immunity, toxic resistance, wide comfortable temperature range, and psychic immunity.
- Still verify in game: fear/panic/terror-style mental breaks, disease/food poisoning leakage, heatstroke/hypothermia edge cases, and whether pain should be normal, reduced, or absent.

## Stats And Balance

- Implemented first pass: tougher than a normal pawn, slightly armored, slower than a normal pawn, non-flammable compared with living pawns, and modest melee attacks.
- Decide whether resistance is best represented through:
  - armor stats,
  - hediff stat offsets,
  - custom damage handling,
  - or race/body part health changes.
- Define weaknesses so the pawn has texture, not just upside:
  - possible vulnerability to blunt, holy, fire, EMP, psychic, or anti-undead effects.
- Decide whether skeletons can use weapons and apparel.
- Decide whether skeletons can be healed, repaired, reassembled, or permanently destroyed.

## Work And Behavior

- Implemented allowed work settings: firefighting, basic work, construction, mining, hauling, cleaning, plus combat behavior through the pawn kind.
- Disallowed work remains the default absence from the skeleton race work settings: doctoring, social/warden/childcare, art, research, cooking, plants, animals, and other skilled living-colonist roles.
- Do not allow learning or skill decay even if the pawn performs work.
- Ensure work restrictions are visible and understandable in the Work tab.

## Master Binding

- Bind the raised skeleton to the ritual conductor as its master.
- Automatically attack anything hostile to its master.
- Follow the master when the master is drafted.
- Decide whether it should follow the master while undrafted, doing fieldwork, or guarding.
- Decide what happens if the master dies, leaves the map, becomes hostile, or is downed:
  - idle near the circle,
  - defend the body,
  - transfer to another Bonewright,
  - become uncontrolled,
  - collapse.
- Decide whether the player can reassign the master.

## Ritual Rules

- Keep no-ossuary-box requirement for this ritual.
- Implemented corpse eligibility: humanlike, non-undead corpses only.
- Decide remaining corpse eligibility details:
  - fresh only,
  - dessicated allowed,
  - rotten allowed,
  - colonist corpses allowed or forbidden,
  - whether ossuary rites should eventually accept beloved pets separately from skeleton-raising.
- Decide whether corpse identity matters:
  - skeleton named after deceased,
  - blank minion,
  - retains some skills or traits,
  - ritual quality affects retained traits.
- Decide whether raising a corpse completes funeral obligations or creates ideology/social consequences.
- Add failure, imperfect success, and stronger success outcomes later.

## UX And Feedback

- Make the ritual dialog explain why a corpse or conductor is unavailable.
- Add clear messages for successful raising, failed raising, and invalid setup.
- Add inspect text or a hediff label that marks the pawn as undead/bound.
- Add a gizmo or status entry showing the master.

## Testing

- Load test with the custom textures enabled.
- Start rite with valid circle/corpse/conductor.
- Try missing circle, forbidden corpse, reserved corpse, unreachable corpse, and interrupted conductor.
- Verify the raised pawn is `AF_SkeletonRace` / `AF_Skeleton`, not a renamed colonist.
- Verify undead hediffs/xenotype apply and persist after save/load.
- Verify skeleton follows master when drafted.
- Verify skeleton attacks hostile threats to master.
- Verify no food, rest, breath, fear, skill learning, or skill decay behavior leaks through.
- Verify save/load preserves master binding and undead state.
- Verify deployed Steam/Common mod copy matches the working copy after changes.

