using System.Collections.Generic;
using System.Text;
using Verse;

namespace MagicFramework.Scheduling;

public sealed class ChainLightningMapComponent : MapComponent
{
    private readonly ChainLightningService chainLightningService = new();
    private List<ChainLightningPulse> pulses = new();

    public ChainLightningMapComponent(Map map)
        : base(map)
    {
    }

    public bool Enqueue(ChainLightningPulse pulse)
    {
        if (pulse == null)
        {
            return false;
        }

        pulses ??= new List<ChainLightningPulse>();
        int insertIndex = pulses.Count;
        for (int i = 0; i < pulses.Count; i++)
        {
            ChainLightningPulse existingPulse = pulses[i];
            if (existingPulse == null || existingPulse.ExecuteAtTick > pulse.ExecuteAtTick)
            {
                insertIndex = i;
                break;
            }
        }

        pulses.Insert(insertIndex, pulse);
        return true;
    }

    public override void MapComponentTick()
    {
        if (pulses == null || pulses.Count == 0)
        {
            return;
        }

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        while (pulses.Count > 0)
        {
            ChainLightningPulse pulse = pulses[0];
            if (pulse == null)
            {
                pulses.RemoveAt(0);
                continue;
            }

            if (pulse.ExecuteAtTick > currentTick)
            {
                return;
            }

            pulses.RemoveAt(0);
            chainLightningService.ExecutePulse(map, pulse);
        }
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref pulses, "pulses", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && pulses == null)
        {
            pulses = new List<ChainLightningPulse>();
        }
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new();
        builder.Append("[MagicFramework] Chain lightning runtime for map ");
        builder.Append(map?.Index ?? -1);
        builder.Append(": ");
        builder.Append(pulses?.Count ?? 0);
        builder.Append(" queued pulse(s).");
        return builder.ToString();
    }
}
