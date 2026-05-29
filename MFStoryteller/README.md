# MF Storyteller: Lord Roth

## Overview

**Lord Roth** is a custom storyteller for RimWorld 1.6 that enables divination-themed magic. Unlike standard storytellers, Lord Roth exposes his upcoming (and recently completed) incidents to divination spells, allowing magic-based foresight and retrospective divination.

## What This Mod Does

1. **Adds Lord Roth as a Storyteller Option**: Select him when starting a new game. He uses standard RimWorld threat scaling and incident generation.

2. **Exposes Incident Information**:
   - **Pending Incidents**: Incidents about to fire (for "foresight" spells)
   - **Recent Incidents**: Recently executed incidents (for "scrying" spells)

3. **Provides a Query API** for divination magic to access incident data through `WorldComponent_DivinationEvents`.

## Features

- **5 Thematic Incidents**: Divine Audience, Test of Character, Twist of Fate, Blessing of Favor, Curse of Disfavor
- **Non-Intrusive**: Also tracks incidents from standard storytellers (Cassandra, Phoebe, Randy)
- **Saves/Loads Correctly**: All incident data persists across game saves
- **Extensible**: Designed for spell mods to build divination mechanics on top

## Dependencies

- **Harmony** (Required) — for Harmony patching
- **Magic Framework** (Optional) — for portrait support; storyteller works without it

## How Divination Magic Works

Spell authors can query Lord Roth's incidents to create divination effects:

```csharp
// Check for upcoming incidents (foresight)
var divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
if (divComponent?.HasPendingIncident() == true)
{
    var nextIncident = divComponent.GetPendingIncidents()[0];
    // Design a spell that reveals this information
}
```

See `Documentation/DivinationAPI.md` for complete API reference and examples.

## Gameplay Impact

- **Slightly reduced difficulty**: Players can now divine threats in advance
- **New playstyle**: Focus on preparation and reaction to known events
- **Spell integration**: Divination spells become tactically valuable for planning

## Configuration

No configuration needed—just install and select Lord Roth as your storyteller.

## Technical Details

### How It Works

1. Harmony patches `IncidentWorker.TryExecute()` with both prefix and postfix hooks
2. **Prefix** registers incident before it executes (for pending queries)
3. **Postfix** moves it to "recent" after successful execution
4. `WorldComponent_DivinationEvents` maintains two histories:
   - `pendingIncidents`: up to 20 queued incidents
   - `recentIncidents`: up to 5 executed incidents

### Serialization

- All incident data properly implements `IExposable` for save/load
- Uses map tiles instead of direct Map references for stability
- PostLoadInit cleanup ensures no null reference errors

## Files

```
MFStoryteller/
├── About/
│   ├── About.xml
│   └── Preview.png
├── Assemblies/
│   └── MFStoryteller.dll
├── Defs/
│   ├── IncidentDefs/LordRoth_CustomIncidents.xml
│   ├── StorytellerDefs/LordRoth_Storytellers.xml
│   └── (other defs as needed)
├── Documentation/
│   └── DivinationAPI.md
└── Languages/
    └── English/
        └── Keyed/
            └── LordRoth_Strings.xml
```

## For Spell Developers

To create divination magic that uses Lord Roth's data:

1. Read `Documentation/DivinationAPI.md`
2. Query `WorldComponent_DivinationEvents` in your spell action
3. Design thematic effects around the incident data
4. Consider cooldowns and power scaling

Example spells to build:
- **Glimpse of Fate**: Reveal next pending incident's threat level
- **Scrying Pool**: Show recent incident history
- **Omens**: Detect upcoming threats and prepare counters
- **Prophecy**: Create random but correct predictions

## Compatibility

- Works with other storytellers (non-exclusive)
- Does not modify vanilla incidents
- Safe with existing mods
- Recommended: Use with Magic Framework for full theming

## Notes

- Lord Roth reveals information to spells, but the player must still choose how to act on it
- Divination accuracy is only as good as the spell's complexity
- Incidents may fail to execute even if pending (map destroyed, colony wiped, etc.)
- This is a foundation mod—full divination magic is built by spell mods on top of this

## Support & Credits

- Built on Magic Framework architecture
- Uses Harmony for incident interception
- Designed for modular divination spell systems

---

**Version**: 0.1.0  
**Author**: Oracle  
**For**: RimWorld 1.6
