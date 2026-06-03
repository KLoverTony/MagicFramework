using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MagicFramework.Actions;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MFVanilla.Core;

public sealed class DivinationActionDef : SpellActionDef
{
    public int maxEvents = 1;
    public bool revealExactIncident = true;
    public bool revealCategory = true;
    public bool revealThreatPoints;
    public bool revealTargetMap = true;
    public bool sendLetter = true;
    public string letterLabel = "Divination";
    public string noStorytellerMessage = "The spell reaches for fate, but no divination storyteller answers.";
    public string noPendingMessage = "The threads of fate are quiet. No foretold incident is currently held.";

    public override SpellActionWorker CreateWorker() => new DivinationActionWorker();
}

public sealed class DivinationActionWorker : SpellActionWorker
{
    private const string ComponentTypeName = "MFStoryteller.WorldComponent_DivinationEvents";

    public override void Execute(SpellContext context, SpellActionDef actionDef, SpellActionRunner runner)
    {
        DivinationActionDef divinationDef = actionDef as DivinationActionDef;
        if (divinationDef == null)
            return;

        object divinationComponent = ResolveDivinationComponent();
        if (divinationComponent == null)
        {
            Messages.Message(divinationDef.noStorytellerMessage, MessageTypeDefOf.NeutralEvent);
            return;
        }

        List<object> pendingEvents = GetPendingEvents(divinationComponent)
            .OrderBy(GetFireTick)
            .Take(Math.Max(1, divinationDef.maxEvents))
            .ToList();

        if (pendingEvents.Count == 0)
        {
            Messages.Message(divinationDef.noPendingMessage, MessageTypeDefOf.NeutralEvent);
            return;
        }

        string text = BuildDivinationText(pendingEvents, divinationDef);
        if (divinationDef.sendLetter)
        {
            LookTargets targets = context?.caster != null ? new LookTargets(context.caster) : null;
            Find.LetterStack.ReceiveLetter(divinationDef.letterLabel, text, LetterDefOf.NeutralEvent, targets);
        }
        else
        {
            Messages.Message(text, MessageTypeDefOf.NeutralEvent);
        }
    }

    private static object ResolveDivinationComponent()
    {
        Type componentType = GenTypes.GetTypeInAnyAssembly(ComponentTypeName);
        if (componentType == null || Find.World == null)
            return null;

        MethodInfo getComponentMethod = typeof(World)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(method => method.Name == nameof(World.GetComponent)
                                      && method.IsGenericMethodDefinition
                                      && method.GetParameters().Length == 0);

        return getComponentMethod?.MakeGenericMethod(componentType).Invoke(Find.World, null);
    }

    private static IEnumerable<object> GetPendingEvents(object divinationComponent)
    {
        MethodInfo getPendingMethod = divinationComponent.GetType().GetMethod("GetPendingIncidents", BindingFlags.Public | BindingFlags.Instance);
        if (getPendingMethod?.Invoke(divinationComponent, null) is not IEnumerable pending)
            yield break;

        foreach (object item in pending)
        {
            if (item != null)
                yield return item;
        }
    }

    private static string BuildDivinationText(List<object> pendingEvents, DivinationActionDef divinationDef)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("The caster reads the shape of approaching fate.");

        for (int i = 0; i < pendingEvents.Count; i++)
        {
            object pendingEvent = pendingEvents[i];
            IncidentDef incidentDef = GetFieldValue<IncidentDef>(pendingEvent, "incidentDef");
            string incidentLabel = ResolveIncidentLabel(incidentDef, divinationDef);
            string timing = DescribeTiming(GetFireTick(pendingEvent));

            builder.Append($"{i + 1}. {incidentLabel} {timing}");

            if (divinationDef.revealThreatPoints)
            {
                float threatPoints = GetFieldValue<float>(pendingEvent, "threatPoints");
                if (threatPoints > 0f)
                    builder.Append($" ({threatPoints:F0} threat points)");
            }

            if (divinationDef.revealTargetMap)
            {
                Map targetMap = ResolveTargetMap(pendingEvent);
                if (targetMap != null)
                    builder.Append($" near {targetMap.Parent?.LabelCap ?? targetMap.Tile.ToString()}");
            }

            builder.AppendLine(".");
        }

        return builder.ToString().TrimEnd();
    }

    private static string ResolveIncidentLabel(IncidentDef incidentDef, DivinationActionDef divinationDef)
    {
        if (incidentDef == null)
            return "an unclear omen";

        if (divinationDef.revealExactIncident)
            return incidentDef.LabelCap;

        if (divinationDef.revealCategory && incidentDef.category != null)
            return $"{incidentDef.category.LabelCap} omen";

        return "an approaching omen";
    }

    private static string DescribeTiming(int fireTick)
    {
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        int ticksUntil = Math.Max(0, fireTick - currentTick);
        if (ticksUntil <= GenDate.TicksPerHour)
            return "will unfold very soon";

        if (ticksUntil < GenDate.TicksPerDay)
            return $"will unfold in about {Math.Max(1, ticksUntil / GenDate.TicksPerHour)} hours";

        return $"will unfold in about {ticksUntil / (float)GenDate.TicksPerDay:F1} days";
    }

    private static int GetFireTick(object pendingEvent)
    {
        return GetFieldValue<int>(pendingEvent, "fireTick");
    }

    private static Map ResolveTargetMap(object pendingEvent)
    {
        PropertyInfo targetMapProperty = pendingEvent.GetType().GetProperty("TargetMap", BindingFlags.Public | BindingFlags.Instance);
        if (targetMapProperty?.GetValue(pendingEvent) is Map targetMap)
            return targetMap;

        int targetTile = GetFieldValue<int>(pendingEvent, "targetTile");
        return Find.Maps?.FirstOrDefault(map => map.Tile == targetTile);
    }

    private static T GetFieldValue<T>(object instance, string fieldName)
    {
        if (instance == null)
            return default;

        FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (field?.GetValue(instance) is T value)
            return value;

        return default;
    }
}
