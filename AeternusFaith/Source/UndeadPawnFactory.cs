using MagicFramework.PawnLifecycle;
using RimWorld;
using Verse;

namespace AeternusFaith
{
    public class UndeadPawnCreationOptions
    {
        public Faction faction;
        public PawnGenerationContext context = PawnGenerationContext.NonPlayer;
        public int tile = -1;
        public Gender fixedGender = Gender.Male;
        public string label;
        public Pawn sourcePawn;
        public Ideo sourceIdeo;
        public Pawn master;
        public bool followMasterWhileDrafted = true;
        public bool followMasterWhileFieldwork = true;
        public bool resetSkills = true;
        public bool copyBackstories;
        public bool copySkills;
        public bool copyOnlySimpleSkills;
        public float copiedSkillFactor = 1f;
        public bool forceNoBackstory = true;
    }

    public static class UndeadPawnFactory
    {
        public static bool CanHandleKind(PawnKindDef pawnKindDef)
        {
            if (pawnKindDef?.race == null)
                return false;
            if (pawnKindDef.defName?.StartsWith("AF_") != true)
                return false;

            PawnLifecycleExtension extension = pawnKindDef.GetModExtension<PawnLifecycleExtension>() ??
                                               pawnKindDef.race.GetModExtension<PawnLifecycleExtension>();
            return extension != null && PawnLifecycleUtility.IsUndead(extension);
        }

        public static Pawn GeneratePawn(PawnKindDef pawnKindDef, UndeadPawnCreationOptions options = null)
        {
            if (!CanHandleKind(pawnKindDef))
                return null;

            options ??= new UndeadPawnCreationOptions();
            PawnGenerationRequest request = new PawnGenerationRequest(
                kind: PawnKindDefOf.Colonist,
                faction: options.faction,
                context: options.context,
                tile: options.tile,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 0f,
                allowPregnant: false,
                allowFood: false,
                allowAddictions: false,
                fixedGender: options.fixedGender == Gender.Female ? Gender.Female : Gender.Male,
                forceNoIdeo: true,
                forceNoBackstory: options.forceNoBackstory,
                developmentalStages: DevelopmentalStage.Adult,
                dontGiveWeapon: true,
                maximumAgeTraits: 0,
                minimumAgeTraits: 0,
                forceNoGear: true);

            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (pawn == null)
                return null;

            ConvertPawn(pawn, pawnKindDef, options);
            return pawn;
        }

        public static void ConvertPawn(Pawn pawn, PawnKindDef pawnKindDef, UndeadPawnCreationOptions options = null)
        {
            if (pawn == null || pawnKindDef?.race == null)
                return;

            options ??= new UndeadPawnCreationOptions();

            pawn.def = pawnKindDef.race;
            pawn.kindDef = pawnKindDef;
            pawn.gender = options.fixedGender == Gender.Female ? Gender.Female : Gender.Male;
            if (!options.label.NullOrEmpty())
                pawn.Name = new NameSingle(options.label);

            SkeletonUndeadUtility.NormalizeSkeletonLifeStage(pawn);

            if (ModsConfig.IdeologyActive && pawn.ideo != null)
            {
                Ideo ideoToApply = options.sourceIdeo ?? Faction.OfPlayer?.ideos?.PrimaryIdeo;
                if (ideoToApply != null)
                    pawn.ideo.SetIdeo(ideoToApply);
            }

            SkeletonUndeadUtility.ResetPawnRenderer(pawn);
            SkeletonUndeadUtility.EnsureUndeadCleanupComp(pawn);
            ApplyLifecycleAppearance(pawn);
            SkeletonUndeadUtility.RemoveLivingResurrectionHediffs(pawn);
            SkeletonUndeadUtility.EnforceUndeadState(pawn, options.resetSkills);
            SkeletonUndeadUtility.EnsureFrameworkLifecycleComp(pawn);

            if (options.copyBackstories)
                SkeletonUndeadUtility.CopyBackstoriesFromSource(options.sourcePawn, pawn);
            if (options.copySkills)
                SkeletonUndeadUtility.CopySkillsFromSource(options.sourcePawn, pawn, options.copiedSkillFactor, options.copyOnlySimpleSkills);

            SkeletonUndeadUtility.RemoveNonUndeadHediffs(pawn);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadHediffs(pawn);
            AssignMaster(pawn, options);
            SkeletonUndeadUtility.ApplyRaceBasedUndeadXenotype(pawn);
            SkeletonUndeadUtility.SuppressUndeadSocialInteractions(pawn);
            ApplyLifecycleAppearance(pawn);
            SkeletonUndeadUtility.ResetPawnRenderer(pawn);
            SkeletonUndeadUtility.TryInitializeRenderer(pawn);
        }

        private static void AssignMaster(Pawn pawn, UndeadPawnCreationOptions options)
        {
            if (pawn == null || options?.master == null)
                return;

            CompPawnLifecycleEnforcer lifecycleComp = pawn.GetComp<CompPawnLifecycleEnforcer>();
            lifecycleComp?.AssignMaster(options.master, options.followMasterWhileDrafted, options.followMasterWhileFieldwork);
            BoundUndeadMinionUtility.AssignMaster(pawn, options.master);
        }

        private static void ApplyLifecycleAppearance(Pawn pawn)
        {
            PawnLifecycleExtension extension = PawnLifecycleUtility.GetLifecycle(pawn);
            if (extension?.bodyForm == PawnLifecycleBodyForm.Spectral || pawn?.def?.defName == "AF_SpectreRace")
            {
                SkeletonUndeadUtility.ApplySpectreAppearance(pawn);
                return;
            }

            if (extension?.bodyForm == PawnLifecycleBodyForm.Skeletal || pawn?.def?.defName == "AF_SkeletonRace")
            {
                SkeletonUndeadUtility.ApplySkeletonAppearance(pawn);
            }
        }
    }
}
