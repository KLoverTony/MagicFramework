# Lord Roth Divination API

This document describes how divination-themed magic can query Lord Roth's (or any storyteller's) upcoming and recent incidents.

## Overview

Lord Roth exposes incident data through `WorldComponent_DivinationEvents`, which tracks:
- **Pending incidents**: Incidents that are about to fire (in the pre-execution phase)
- **Recent incidents**: Incidents that have already fired

Divination spells can query this component to reveal what Lord Roth has planned or what has already transpired.

## Accessing the Component

```csharp
WorldComponent_DivinationEvents divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
if (divComponent == null)
    return; // Component not found (mod not loaded or game not active)
```

## Querying Pending Incidents

Pending incidents are those selected by the storyteller but not yet executed. Use these for "foresight" or "future sight" effects.

```csharp
// Check if any incidents are pending
if (divComponent.HasPendingIncident())
{
    List<PendingDivinationEvent> pending = divComponent.GetPendingIncidents();
    foreach (var incident in pending)
    {
        Log.Message($"Fated to occur: {incident.incidentDef.label}");
        Log.Message($"  Map: {incident.TargetMap?.Label}");
        Log.Message($"  Threat Points: {incident.threatPoints}");
    }
}
```

### Foresight/Fate-Reading Spells
- Reveal the next X pending incidents
- Show threat level or incident category
- Reveal only incident category without specifics
- Create prophetic visions with cryptic details

## Querying Recent Incidents

Recent incidents are those that have already fired successfully. Use these for retrospective divination or scrying effects.

```csharp
// Check if any incidents have recently occurred
if (divComponent.HasRecentIncident())
{
    PendingDivinationEvent recent = divComponent.GetMostRecentIncident();
    Log.Message($"Most recent event: {recent.incidentDef.label}");
}

// Get all recent incidents (up to 5 most recent)
List<PendingDivinationEvent> history = divComponent.GetRecentIncidents();
```

### Retrospective Divination Spells
- Reveal what incident just occurred
- Show a timeline of recent incidents
- Trigger responses based on incident type
- Unlock bonuses for correctly predicting incidents

## PendingDivinationEvent Structure

```csharp
public class PendingDivinationEvent
{
    public IncidentDef incidentDef;      // The incident that fired/will fire
    public int fireTick;                 // When it fired (TicksGame)
    public float threatPoints;           // Threat point budget allocated
    public int targetTile;               // Map tile where incident occurred
    public int selectedTick;             // When incident was selected
    
    public Map TargetMap                 // Helper to get the Map from tile
}
```

## Example: Simple Foresight Spell

```csharp
public class SpellActionDef_RevealFate : SpellActionDef
{
    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        WorldComponent_DivinationEvents divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
        if (divComponent?.HasPendingIncident() != true)
        {
            Messages.Message("No fate is fated.", MessageTypeDefOf.NeutralEvent);
            return;
        }
        
        var incidents = divComponent.GetPendingIncidents();
        if (incidents.Count > 0)
        {
            var nextFate = incidents[0];
            string msg = $"You perceive: {nextFate.incidentDef.label} approaches with {nextFate.threatPoints} points.";
            Messages.Message(msg, MessageTypeDefOf.NeutralEvent);
        }
    }
}
```

## Design Principles for Divination Magic

1. **Clarity vs. Mystery**: Decide whether divination is precise (shows exact incident) or vague (shows category/threat level)
2. **Cost/Benefit**: More powerful divination should cost more resources or have longer cooldowns
3. **Thematic Consistency**: Use the divination data to support your magic school's identity
4. **Limiting Information**: Consider only revealing incident data to the caster, not globally
5. **Integration**: Combine divination with other spell actions (buffs before threats, responses to opportunities)

## Common Divination Patterns

### Pattern 1: Threat Detection
```csharp
var pending = divComponent.GetPendingIncidents();
var threats = pending.Where(e => e.incidentDef.category == IncidentCategory.ThreatBig).ToList();
if (threats.Count > 0)
    // Trigger warning or protective measures
```

### Pattern 2: Opportunity Recognition
```csharp
var recent = divComponent.GetRecentIncidents();
var lastIncident = recent.LastOrDefault();
if (lastIncident?.incidentDef.defName.Contains("Blessing") == true)
    // Reward the colonist who received the blessing
```

### Pattern 3: Prophetic Timing
```csharp
var pending = divComponent.GetPendingIncidents();
int ticksUntilNextEvent = pending.Count > 0 
    ? pending[0].fireTick - Find.TickManager.TicksGame 
    : int.MaxValue;
// Use this for countdowns or preparation timers
```

## Notes

- Pending incidents may not execute if conditions change (map destroyed, colonists fled, etc.)
- Recent incidents history is limited to 5 most recent events
- This API works with any storyteller, not just Lord Roth
- Incidents are registered at execution time; querying before game start returns empty lists

## See Also

- `WorldComponent_DivinationEvents` source: `Source/Core/WorldComponent_DivinationEvents.cs`
- `PendingDivinationEvent` source: `Source/Data/PendingDivinationEvent.cs`
