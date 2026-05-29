# MF Storyteller: Debug Guide

This guide explains how to access and use the debug features for testing the divination system.

## Debug Methods

Two static methods are available for debugging:

```csharp
// Show a window listing all pending and recent incidents
MFStoryteller.DebugActions_DivinationEvents.ShowDivinationEvents()

// Force the next pending incident to fire immediately
MFStoryteller.DebugActions_DivinationEvents.ForceNextIncident()
```

## How to Access Debug Features

### Option 1: Using RimWorld's Development Mode

1. Launch RimWorld with `-logfile` flag to enable developer mode
2. In-game, press `Ctrl+F10` to open Debug Inspector
3. Search for `MFStoryteller` or `DebugActions_DivinationEvents`
4. Call the static methods from the inspector

### Option 2: Using Mod Settings

If you add a mod menu button, you can call:

```csharp
Find.WindowStack.Add(new MFStoryteller.Dialog_DebugDivinationEvents());
```

### Option 3: From Other Mods

Create a debug action that calls:

```csharp
MFStoryteller.DebugActions_DivinationEvents.ShowDivinationEvents();
```

## Debug Window

The debug window shows:

**PENDING INCIDENTS**
- List of incidents that have been selected but not yet executed
- Shows: incident name, threat points, target map tile
- Up to 20 recent pending incidents stored

**RECENT INCIDENTS**
- List of incidents that have already executed successfully
- Shows: incident name, threat points, time since execution, target map
- Up to 5 most recent incidents stored

**Statistics**
- Count of pending incidents
- Count of recent incidents

## Force Next Incident

The `ForceNextIncident()` method:
- Checks if any incidents are pending
- Takes the first pending incident
- Immediately executes it with the stored threat points
- Shows a message confirming the incident fired

Useful for:
- Testing divination spell mechanics
- Verifying incident execution timing
- Skipping delays between storyteller selections

## Testing Divination Spells

Typical test workflow:

1. **Open Debug Window**: Check what incidents are pending/recent
2. **Wait for incident**: Let storyteller generate incidents, or use `ForceNextIncident()`
3. **Test divination spell**: Cast your spell and verify it reads incident data correctly
4. **Inspect results**: Refresh the debug window to see if incident moved to "recent"

## Example Debug Session

```
1. Launch game with Lord Roth as storyteller
2. Play for a few days until incidents appear
3. Ctrl+F10 -> Find MFStoryteller
4. Call ShowDivinationEvents() - see pending incidents
5. Create test divination spell
6. Test spell calls: Find.World.GetComponent<WorldComponent_DivinationEvents>()
7. Verify spell reads the pending incident data
8. Call ForceNextIncident() to trigger it
9. Call ShowDivinationEvents() again - incident should be in "Recent"
```

## Troubleshooting

**"WorldComponent_DivinationEvents not found"**
- Mod not loaded or game not active
- Check that MFStoryteller.dll is in Assemblies/

**No pending/recent incidents**
- Storyteller hasn't generated any yet
- Use `ForceNextIncident()` to trigger one manually
- Check that Lord Roth is the active storyteller

**Forced incident didn't fire**
- Check the console for error messages
- Verify incident target map still exists
- Target map might have been abandoned or destroyed

## Console Commands (if enabled)

For mod developers, you can also test via console if your mod provides console access:

```
MFStoryteller.DebugActions_DivinationEvents.ShowDivinationEvents()
MFStoryteller.DebugActions_DivinationEvents.ForceNextIncident()
```

## Notes

- Debug features only work during active gameplay
- Incidents are only tracked after they're about to fire (not during generation)
- Forced incidents bypass the normal storyteller delay system
- Debug window automatically refreshes when opened

---

For more information, see `DivinationAPI.md` for spell integration details.
