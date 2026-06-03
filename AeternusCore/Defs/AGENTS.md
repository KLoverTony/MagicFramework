# Aeternus Core Def Notes

Scope: planned and playable RimWorld Defs for Aeternus Core. The source namespace remains `AeternusFaith` for now.

Use the `AF_` prefix for new Def names.

Planned groups:
- `IdeoDefs`: faith structure, roles, and ideology definitions.
- `PreceptDefs`: burial, remembrance, sacred space, magic acceptance, and corruption hostility.
- `ThingDefs`: shrines, floors, ritual buildings, and related objects.
- `RitualPatternDefs`: devotion, funerary, purification, and Bonewright rituals.
- `HediffDefs`: devotion markers, ritual instability, corruption, and spiritual residue.
- `IncidentDefs`: blessings, omens, hauntings, and divine intervention events.
- `ThoughtDefs`: mood reactions from rites, graves, spirits, and sacred rooms.
- `ResearchProjectDefs`: optional gates for Bonewright, Spikecore, and shrine progression.

Guidance:
- Keep README placeholders until a feature is ready for valid, loadable XML.
- Add dependencies in `About/About.xml` when a Def requires another mod or DLC.
- Favor coherent small feature sets over isolated orphan Defs.
