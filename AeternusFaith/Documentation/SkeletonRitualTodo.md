# Skeleton Ritual TODO

Current first slice:
- The Ossanith lectern can start a skeleton rite when orthogonally adjacent to an Ossanith Circle.
- A conductor brings a corpse to the circle, performs the rite, destroys the corpse, and spawns a player-faction skeleton pawn.
- The spawned pawn is currently a simple humanlike pawn using vanilla fallback graphics.

## Resources

- Use https://rimworldwiki.com/wiki/Modding_Tutorials as a reference if needed.

## Visuals

- Use the provided `Textures/Things/Undead/Skeleton_*` art for the raised pawn.
- Avoid crashing RimWorld's startup texture atlas; prefer an approach that loads/applies these textures only when needed or uses a properly supported pawn-rendering path.
- Confirm expected RimWorld directional naming and orientation:
  - south/front
  - east
  - north/back
  - west mirrored from east, if supported
- Revisit and improve the skeleton art after the functional pawn behavior is stable.
- Add an appropriate low-memory-safe gizmo icon for the ritual.

## Undead Pawn Definition

- Define the skeleton as a real undead minion, not just a renamed colonist.
- Decide whether the implementation should be:
  - custom pawn race,
  - humanlike pawn with undead hediff/comps,
  - or animal/minion-style pawn with restricted work and master behavior.
- Preserve save/load behavior for any custom master binding or undead state.

## Core Biology

- Does not eat.
- Does not sleep.
- Does not breathe.
- Immune to tox gas and other breath-dependent hazards.
- Does not experience fear, panic, or terror-style mental breaks.
- Does not learn skills.
- Does not lose skills over time.
- Does not need recreation or comfort, unless a later design intentionally adds a necromantic upkeep need.
- Consider whether skeletons should bleed, bleed less, or use a non-blood filth.
- Consider whether skeletons should feel pain or have reduced pain impact.
- Consider whether skeletons should be immune to disease, infection, food poisoning, hypothermia, heatstroke, and age-related conditions.

## Stats And Balance

- Tougher than a normal pawn.
- At least slightly resistant to most damage.
- Slower than a normal pawn.
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

- Restrict to a limited task set.
- Proposed allowed tasks:
  - firefighting, if thematically acceptable,
  - patient/self-maintenance only if needed by RimWorld systems,
  - hauling,
  - cleaning,
  - basic construction or mining if desired,
  - combat.
- Proposed disallowed tasks:
  - doctoring,
  - social/warden/childcare,
  - art,
  - research,
  - cooking,
  - plants/animals, unless later design wants labor-specialized undead.
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
- Decide corpse eligibility:
  - humanlike only,
  - any corpse,
  - fresh only,
  - dessicated allowed,
  - rotten allowed,
  - colonist corpses allowed or forbidden.
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
- Verify skeleton follows master when drafted.
- Verify skeleton attacks hostile threats to master.
- Verify no food, rest, breath, fear, skill learning, or skill decay behavior leaks through.
- Verify save/load preserves master binding and undead state.
- Verify deployed Steam/Common mod copy matches the working copy after changes.

