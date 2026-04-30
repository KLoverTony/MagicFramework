using RimWorld;
using Verse;

namespace AeternusFaith
{
    public class CompProperties_OssuaryObituary : CompProperties
    {
        public CompProperties_OssuaryObituary()
        {
            compClass = typeof(Comp_OssuaryObituary);
        }
    }

    public class Comp_OssuaryObituary : ThingComp
    {
        private string obituary;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref obituary, "obituary");
        }

        public override string CompInspectStringExtra()
        {
            return string.IsNullOrEmpty(obituary) ? null : obituary;
        }

        public void Record(Corpse corpse, Pawn conductor)
        {
            Pawn deceased = corpse?.InnerPawn;
            if (deceased == null)
            {
                obituary = "An unnamed body is sealed within this ossuary.";
                return;
            }

            string ageText = deceased.ageTracker != null ? ", age " + deceased.ageTracker.AgeBiologicalYears : "";
            string kindText = string.IsNullOrEmpty(deceased.KindLabel) ? "pawn" : deceased.KindLabel;
            string conductorText = conductor != null ? " Sealed by " + conductor.LabelShortCap + "." : string.Empty;
            obituary = "Here lie the remains of " + deceased.LabelShortCap + ageText + ", " + kindText + "." + conductorText;
        }
    }
}
