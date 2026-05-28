using HarmonyLib;
using RimWorld;
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
