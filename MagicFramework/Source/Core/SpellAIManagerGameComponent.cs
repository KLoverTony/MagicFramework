using System.Collections.Generic;
using System.Linq;
using MagicFramework.Context;
using MagicFramework.Definitions;
using MagicFramework.Execution;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

public enum SpellAIIntent
{
    Hostile,
    HealAlly,
    BuffAlly
}

public sealed class SpellAIEntry : IExposable
{
    public SpellDef spell;
    public SpellAIIntent intent;

    public SpellAIEntry()
    {
    }

    public SpellAIEntry(SpellDef spell, SpellAIIntent intent)
    {
        this.spell = spell;
        this.intent = intent;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref spell, "spell");
        Scribe_Values.Look(ref intent, "intent", SpellAIIntent.Hostile);
    }
}

public sealed class SpellAIManagerGameComponent : GameComponent
{
    private const int TickInterval = 60;
    private const int MaxCandidateTargets = 8;
    private const int SuccessThrottleTicks = 420;
    private const int FailureThrottleTicks = 180;
    private const int NoTargetThrottleTicks = 240;
    private const float HostileCastThreshold = 0.45f;
    private const float HealCastThreshold = 0.6f;
    private const float BuffCastThreshold = 0.5f;
    private const float MinimumCastThreshold = 0.2f;
    private const float MaximumCastThreshold = 0.9f;

    private static readonly SpellExecutor Executor = new();
    private readonly SpellCastValidator validator = new();
    private List<SpellAIPawnState> pawnStates = new();
    private int nextGlobalTick;

    public SpellAIManagerGameComponent(Game game)
    {
    }

    public static SpellAIManagerGameComponent Instance => Current.Game?.GetComponent<SpellAIManagerGameComponent>();

    public void RegisterPawn(Pawn pawn, IEnumerable<SpellAIEntry> entries)
    {
        if (pawn == null || entries == null)
        {
            return;
        }

        List<SpellAIEntry> usableEntries = entries
            .Where(entry => entry?.spell != null)
            .Select(entry => new SpellAIEntry(entry.spell, entry.intent))
            .ToList();
        if (usableEntries.Count == 0)
        {
            return;
        }

        pawnStates ??= new List<SpellAIPawnState>();
        SpellAIPawnState state = pawnStates.FirstOrDefault(existing => existing?.pawn == pawn);
        if (state == null)
        {
            state = new SpellAIPawnState
            {
                pawn = pawn,
                nextAssessmentTick = Find.TickManager?.TicksGame + Rand.Range(30, TickInterval + 30) ?? 0,
                castBias = Rand.Range(-0.1f, 0.1f)
            };
            pawnStates.Add(state);
        }

        state.entries = usableEntries;
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        if (pawnStates == null || pawnStates.Count == 0 || Find.TickManager == null)
        {
            return;
        }

        int currentTick = Find.TickManager.TicksGame;
        if (currentTick < nextGlobalTick)
        {
            return;
        }

        nextGlobalTick = currentTick + TickInterval;
        for (int i = pawnStates.Count - 1; i >= 0; i--)
        {
            SpellAIPawnState state = pawnStates[i];
            if (state?.pawn == null || state.pawn.Destroyed || state.pawn.Dead)
            {
                pawnStates.RemoveAt(i);
                continue;
            }

            if (currentTick >= state.nextAssessmentTick)
            {
                TryAssessAndCast(state, currentTick);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref pawnStates, "pawnStates", LookMode.Deep);
        Scribe_Values.Look(ref nextGlobalTick, "nextGlobalTick");

        if (Scribe.mode == LoadSaveMode.PostLoadInit && pawnStates == null)
        {
            pawnStates = new List<SpellAIPawnState>();
        }
    }

    private void TryAssessAndCast(SpellAIPawnState state, int currentTick)
    {
        Pawn pawn = state.pawn;
        if (!CanAssessPawn(pawn) || state.entries == null || state.entries.Count == 0)
        {
            state.nextAssessmentTick = currentTick + NoTargetThrottleTicks;
            return;
        }

        List<SpellAIEntry> availableEntries = AvailableEntries(pawn, state.entries);
        if (availableEntries.Count == 0)
        {
            state.nextAssessmentTick = currentTick + NoTargetThrottleTicks;
            return;
        }

        availableEntries.Shuffle();
        for (int i = 0; i < availableEntries.Count; i++)
        {
            SpellAIEntry entry = availableEntries[i];
            if (entry.intent != SpellAIIntent.Hostile && !IsCombatEngaged(pawn))
            {
                continue;
            }

            if (TryFindTarget(pawn, entry, out LocalTargetInfo target, out float score)
                && score >= CastThreshold(entry.intent, state.castBias))
            {
                state.nextAssessmentTick = currentTick + SuccessThrottleTicks;
                SpellCastWarmupUtility.StartOrExecute(pawn, entry.spell, target, Executor, (completed, _) =>
                {
                    if (!completed)
                    {
                        state.nextAssessmentTick = Find.TickManager.TicksGame + FailureThrottleTicks;
                    }
                });
                return;
            }
        }

        state.nextAssessmentTick = currentTick + NoTargetThrottleTicks;
    }

    private static bool CanAssessPawn(Pawn pawn)
    {
        if (pawn == null || pawn.Dead || pawn.Downed || !pawn.Spawned || pawn.Map == null)
        {
            return false;
        }

        if (pawn.Faction == null || Faction.OfPlayer == null || !pawn.Faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }

        if (pawn.stances?.curStance is SpellWarmupStance)
        {
            return false;
        }

        if (pawn.MentalState != null)
        {
            return false;
        }

        return SpellRuntimeGameComponent.Instance?.HasArcaneGift(pawn) == true;
    }

    private static List<SpellAIEntry> AvailableEntries(Pawn pawn, List<SpellAIEntry> entries)
    {
        List<SpellAIEntry> availableEntries = new();
        for (int i = 0; i < entries.Count; i++)
        {
            SpellAIEntry entry = entries[i];
            if (entry?.spell == null)
            {
                continue;
            }

            SpellContext context = SpellRequirementUtility.CreatePawnContext(pawn, entry.spell);
            if (SpellRequirementUtility.CanCastSpell(context, entry.spell, out _, requireKnownSpell: true))
            {
                availableEntries.Add(entry);
            }
        }

        return availableEntries;
    }

    private bool TryFindTarget(Pawn pawn, SpellAIEntry entry, out LocalTargetInfo target, out float score)
    {
        target = LocalTargetInfo.Invalid;
        score = 0f;
        List<Pawn> candidates = entry.intent switch
        {
            SpellAIIntent.Hostile => HostileCandidates(pawn, entry.spell),
            SpellAIIntent.HealAlly => HealCandidates(pawn, entry.spell),
            SpellAIIntent.BuffAlly => BuffCandidates(pawn, entry.spell),
            _ => new List<Pawn>()
        };

        int count = Mathf.Min(MaxCandidateTargets, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            LocalTargetInfo candidateTarget = new(candidates[i]);
            SpellContext context = Executor.BuildContext(entry.spell, pawn, candidateTarget);
            if (validator.TryValidate(context))
            {
                float candidateScore = ScoreCandidate(pawn, entry, candidates[i]);
                if (candidateScore <= score)
                {
                    continue;
                }

                target = candidateTarget;
                score = candidateScore;
            }
        }

        return target.IsValid;
    }

    private static float CastThreshold(SpellAIIntent intent, float castBias)
    {
        float baseThreshold = intent switch
        {
            SpellAIIntent.Hostile => HostileCastThreshold,
            SpellAIIntent.HealAlly => HealCastThreshold,
            SpellAIIntent.BuffAlly => BuffCastThreshold,
            _ => 1f
        };

        return Mathf.Clamp(baseThreshold - castBias, MinimumCastThreshold, MaximumCastThreshold);
    }

    private static float ScoreCandidate(Pawn caster, SpellAIEntry entry, Pawn target)
    {
        if (caster == null || target == null)
        {
            return 0f;
        }

        float distanceScore = DistanceScore(caster, target, entry.spell);
        return entry.intent switch
        {
            SpellAIIntent.Hostile => ScoreHostileCandidate(caster, target, distanceScore),
            SpellAIIntent.HealAlly => ScoreHealCandidate(target, distanceScore),
            SpellAIIntent.BuffAlly => ScoreBuffCandidate(caster, target, distanceScore),
            _ => 0f
        };
    }

    private static float ScoreHostileCandidate(Pawn caster, Pawn target, float distanceScore)
    {
        float currentTargetScore = target == CurrentHostileJobTarget(caster) ? 0.25f : 0f;
        float downedRiskScore = Mathf.Clamp01(1f - (target.health?.summaryHealth?.SummaryHealthPercent ?? 1f)) * 0.2f;
        float combatTargetScore = IsThreatened(caster) ? 0.1f : 0f;
        return Mathf.Clamp01(0.3f + currentTargetScore + downedRiskScore + combatTargetScore + distanceScore * 0.25f);
    }

    private static float ScoreHealCandidate(Pawn target, float distanceScore)
    {
        float injuryScore = Mathf.Clamp01(TotalNonPermanentInjurySeverity(target) / 18f);
        float healthScore = Mathf.Clamp01(1f - (target.health?.summaryHealth?.SummaryHealthPercent ?? 1f));
        return Mathf.Clamp01(injuryScore * 0.55f + healthScore * 0.35f + distanceScore * 0.1f);
    }

    private static float ScoreBuffCandidate(Pawn caster, Pawn target, float distanceScore)
    {
        float selfScore = target == caster ? 0.2f : 0f;
        float threatenedScore = IsThreatened(target) ? 0.45f : 0f;
        float meleeScore = IsMeleeCombatant(target) ? 0.15f : 0f;
        return Mathf.Clamp01(selfScore + threatenedScore + meleeScore + distanceScore * 0.2f);
    }

    private static float DistanceScore(Pawn caster, Pawn target, SpellDef spell)
    {
        float range = Mathf.Max(0f, spell?.targeting?.range ?? 0f);
        if (range <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01(1f - caster.Position.DistanceTo(target.Position) / (range + 3f));
    }

    private static List<Pawn> HostileCandidates(Pawn pawn, SpellDef spell)
    {
        List<Pawn> candidates = new();
        Pawn currentTarget = CurrentHostileJobTarget(pawn);
        if (IsValidHostileTarget(pawn, currentTarget))
        {
            candidates.Add(currentTarget);
        }

        foreach (Pawn candidate in pawn.Map.mapPawns.AllPawnsSpawned)
        {
            if (candidate == currentTarget || !IsValidHostileTarget(pawn, candidate))
            {
                continue;
            }

            candidates.Add(candidate);
        }

        float range = Mathf.Max(0f, spell?.targeting?.range ?? 0f);
        return candidates
            .Where(candidate => range <= 0f || pawn.Position.DistanceTo(candidate.Position) <= range + 3f)
            .OrderByDescending(candidate => candidate == currentTarget)
            .ThenBy(candidate => pawn.Position.DistanceToSquared(candidate.Position))
            .ToList();
    }

    private static List<Pawn> HealCandidates(Pawn pawn, SpellDef spell)
    {
        float range = Mathf.Max(0f, spell?.targeting?.range ?? 0f);
        return AlliedCandidates(pawn, range)
            .Where(HasNonPermanentInjury)
            .OrderBy(candidate => candidate.health?.summaryHealth?.SummaryHealthPercent ?? 1f)
            .ThenBy(candidate => pawn.Position.DistanceToSquared(candidate.Position))
            .ToList();
    }

    private static List<Pawn> BuffCandidates(Pawn pawn, SpellDef spell)
    {
        float range = Mathf.Max(0f, spell?.targeting?.range ?? 0f);
        List<Pawn> candidates = AlliedCandidates(pawn, range)
            .Where(candidate => IsCombatEngaged(candidate) || IsThreatened(candidate))
            .OrderByDescending(candidate => candidate == pawn)
            .ThenBy(candidate => pawn.Position.DistanceToSquared(candidate.Position))
            .ToList();

        if (spell?.defName == "MF_Might")
        {
            candidates = candidates.Where(IsMeleeCombatant).ToList();

            HediffDef mighty = DefDatabase<HediffDef>.GetNamedSilentFail("MF_Mighty");
            if (mighty != null)
            {
                candidates = candidates.Where(candidate => !candidate.health.hediffSet.HasHediff(mighty)).ToList();
            }
        }

        return candidates;
    }

    private static IEnumerable<Pawn> AlliedCandidates(Pawn pawn, float range)
    {
        foreach (Pawn candidate in pawn.Map.mapPawns.AllPawnsSpawned)
        {
            if (candidate == null || candidate.Dead || candidate.Downed || !candidate.Spawned || candidate.Faction != pawn.Faction)
            {
                continue;
            }

            if (range > 0f && pawn.Position.DistanceTo(candidate.Position) > range + 3f)
            {
                continue;
            }

            yield return candidate;
        }
    }

    private static bool IsValidHostileTarget(Pawn caster, Pawn target)
    {
        return target != null
            && target != caster
            && !target.Dead
            && !target.Downed
            && target.Spawned
            && caster.HostileTo(target);
    }

    private static Pawn CurrentHostileJobTarget(Pawn pawn)
    {
        Pawn target = pawn?.CurJob?.targetA.Thing as Pawn;
        return IsValidHostileTarget(pawn, target) ? target : null;
    }

    private static bool IsCombatEngaged(Pawn pawn)
    {
        if (pawn?.CurJob == null)
        {
            return false;
        }

        if (CurrentHostileJobTarget(pawn) != null)
        {
            return true;
        }

        string jobDefName = pawn.CurJob.def?.defName;
        return jobDefName == "AttackMelee"
            || jobDefName == "Wait_Combat"
            || jobDefName == "CastAbilityOnThing";
    }

    private static bool IsMeleeCombatant(Pawn pawn)
    {
        ThingWithComps primary = pawn?.equipment?.Primary;
        return primary == null || primary.def.IsMeleeWeapon;
    }

    private static bool HasNonPermanentInjury(Pawn pawn)
    {
        return TotalNonPermanentInjurySeverity(pawn) > 1f;
    }

    private static float TotalNonPermanentInjurySeverity(Pawn pawn)
    {
        if (pawn?.health?.hediffSet?.hediffs == null)
        {
            return 0f;
        }

        float severity = 0f;
        for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
        {
            if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury injury && injury.Severity > 1f && !injury.IsPermanent())
            {
                severity += injury.Severity;
            }
        }

        return severity;
    }

    private static bool IsThreatened(Pawn pawn)
    {
        if (pawn?.Map == null)
        {
            return false;
        }

        foreach (Pawn other in pawn.Map.mapPawns.AllPawnsSpawned)
        {
            if (other != pawn && !other.Dead && !other.Downed && other.Spawned && other.HostileTo(pawn)
                && other.Position.DistanceTo(pawn.Position) <= 18f)
            {
                return true;
            }
        }

        return false;
    }

    private sealed class SpellAIPawnState : IExposable
    {
        public Pawn pawn;
        public List<SpellAIEntry> entries = new();
        public int nextAssessmentTick;
        public float castBias;

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Collections.Look(ref entries, "entries", LookMode.Deep);
            Scribe_Values.Look(ref nextAssessmentTick, "nextAssessmentTick");
            Scribe_Values.Look(ref castBias, "castBias");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && entries == null)
            {
                entries = new List<SpellAIEntry>();
            }
        }
    }
}
