using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace MFStoryteller
{
	[StaticConstructorOnStartup]
	public class MFStorytellerHarmony
	{
		static MFStorytellerHarmony()
		{
			Harmony harmony = new Harmony("oracle.mfstoryteller");
			harmony.PatchAll();
		}
	}

	[HarmonyPatch]
	public static class Patch_Storyteller_MakeIncidentsForInterval
	{
		static MethodBase TargetMethod()
		{
			return AccessTools.Method(typeof(Storyteller), nameof(Storyteller.MakeIncidentsForInterval), Type.EmptyTypes);
		}

		static void Postfix(Storyteller __instance, ref IEnumerable<FiringIncident> __result)
		{
			if (__instance?.def?.defName != "LordRoth" || __result == null)
				return;

			WorldComponent_DivinationEvents divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
			if (divComponent == null)
				return;

			if (divComponent.HasPendingIncident())
			{
				__result = Enumerable.Empty<FiringIncident>();
				return;
			}

			List<FiringIncident> incidents = __result.Where(incident => incident?.def != null).ToList();
			if (incidents.Count == 0)
				return;

			divComponent.RegisterForecastIncident(incidents[0]);

			__result = Enumerable.Empty<FiringIncident>();
		}
	}

	[HarmonyPatch(typeof(IncidentWorker), nameof(IncidentWorker.TryExecute))]
	public static class Patch_IncidentWorker_TryExecute
	{
		static void Prefix(IncidentWorker __instance, IncidentParms parms)
		{
			if (parms?.target is not Map map)
				return;

			if (__instance.def == null)
				return;

			WorldComponent_DivinationEvents divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
			if (divComponent == null)
				return;

			if (divComponent.HasMatchingPendingIncident(__instance.def, map, parms.points))
				return;

			divComponent.RegisterSelectedIncident(__instance.def, map, parms.points);
		}

		static void Postfix(IncidentWorker __instance, IncidentParms parms, bool __result)
		{
			if (parms?.target is not Map map)
				return;

			if (__instance.def == null)
				return;

			WorldComponent_DivinationEvents divComponent = Find.World?.GetComponent<WorldComponent_DivinationEvents>();
			if (divComponent == null)
				return;

			divComponent.CompleteSelectedIncident(__instance.def, map, parms.points, __result);
		}
	}
}
