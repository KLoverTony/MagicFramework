using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace MagicFramework.PawnLifecycle;

public static class PawnLifecycleUtility
{
    public static PawnLifecycleExtension GetLifecycle(Pawn pawn)
    {
        return TryGetLifecycle(pawn, out PawnLifecycleExtension extension, out _) ? extension : null;
    }

    public static bool TryGetLifecycle(Pawn pawn, out PawnLifecycleExtension extension, out Def sourceDef)
    {
        extension = null;
        sourceDef = null;

        if (pawn == null)
        {
            return false;
        }

        extension = pawn.kindDef?.GetModExtension<PawnLifecycleExtension>();
        if (extension != null)
        {
            sourceDef = pawn.kindDef;
            return true;
        }

        extension = pawn.def?.GetModExtension<PawnLifecycleExtension>();
        if (extension != null)
        {
            sourceDef = pawn.def;
            return true;
        }

        return false;
    }

    public static bool HasLifecycle(Pawn pawn)
    {
        return TryGetLifecycle(pawn, out _, out _);
    }

    public static bool IsUndead(Pawn pawn)
    {
        PawnLifecycleExtension extension = GetLifecycle(pawn);
        return extension != null && IsUndead(extension);
    }

    public static bool IsSpirit(Pawn pawn)
    {
        PawnLifecycleExtension extension = GetLifecycle(pawn);
        return extension != null && IsSpirit(extension);
    }

    public static bool IsConstruct(Pawn pawn)
    {
        PawnLifecycleExtension extension = GetLifecycle(pawn);
        return extension != null && IsConstruct(extension);
    }

    public static bool IsUndead(PawnLifecycleExtension extension)
    {
        if (extension == null)
        {
            return false;
        }

        return extension.isUndead ||
               extension.bodyForm == PawnLifecycleBodyForm.Skeletal ||
               extension.bodyForm == PawnLifecycleBodyForm.Spectral ||
               extension.bodyForm == PawnLifecycleBodyForm.CorpseHosted ||
               extension.bodyForm == PawnLifecycleBodyForm.PhylacteryReformed ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.CorpseOnlyHusk ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.ReleasedSourceSoul ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.BoundSourceSoul ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.ActiveSpirit ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.CopiedEcho ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.SplitEcho ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.ConsumedSoul ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.CorruptedSoul ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.PhylacteryAnchored;
    }

    public static bool IsSpirit(PawnLifecycleExtension extension)
    {
        if (extension == null)
        {
            return false;
        }

        return extension.isSpirit ||
               extension.bodyForm == PawnLifecycleBodyForm.Spectral ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.ActiveSpirit ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.CopiedEcho ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.SplitEcho;
    }

    public static bool IsConstruct(PawnLifecycleExtension extension)
    {
        if (extension == null)
        {
            return false;
        }

        return extension.isConstruct ||
               extension.bodyForm == PawnLifecycleBodyForm.Construct ||
               extension.soulPolicy == PawnLifecycleSoulPolicy.ConstructCore;
    }

    public static bool HasLifecycleTag(Pawn pawn, string tag)
    {
        if (tag.NullOrEmpty())
        {
            return false;
        }

        PawnLifecycleExtension extension = GetLifecycle(pawn);
        return extension?.lifecycleTags?.Contains(tag) == true;
    }

    public static IEnumerable<HediffDef> MarkerHediffs(Pawn pawn)
    {
        PawnLifecycleExtension extension = GetLifecycle(pawn);
        if (extension?.markerHediffs == null)
        {
            yield break;
        }

        foreach (HediffDef hediffDef in extension.markerHediffs)
        {
            if (hediffDef != null)
            {
                yield return hediffDef;
            }
        }
    }

    public static string GetDebugSummary(Pawn pawn)
    {
        if (!TryGetLifecycle(pawn, out PawnLifecycleExtension extension, out Def sourceDef))
        {
            return "[MagicFramework] " + (pawn?.LabelShortCap ?? "<null pawn>") + " has no pawn lifecycle profile.";
        }

        StringBuilder builder = new();
        builder.AppendLine("[MagicFramework] Pawn lifecycle profile");
        builder.AppendLine("  Pawn: " + (pawn?.LabelShortCap ?? "<null pawn>"));
        builder.AppendLine("  Pawn def: " + (pawn?.def?.defName ?? "<none>"));
        builder.AppendLine("  Pawn kind: " + (pawn?.kindDef?.defName ?? "<none>"));
        builder.AppendLine("  Source: " + (sourceDef?.defName ?? "<none>"));
        builder.AppendLine("  Classifications: undead=" + IsUndead(extension) + ", spirit=" + IsSpirit(extension) + ", construct=" + IsConstruct(extension));
        builder.AppendLine("  Body: " + extension.bodyForm);
        builder.AppendLine("  Intelligence: " + extension.intelligence);
        builder.AppendLine("  Needs: " + extension.needsPolicy);
        builder.AppendLine("  Social: " + extension.socialPolicy);
        builder.AppendLine("  Gear: " + extension.gearPolicy);
        builder.AppendLine("  Control: " + extension.controlPolicy);
        builder.AppendLine("  Work: " + extension.workPolicy);
        builder.AppendLine("  Recovery: " + extension.recoveryPolicy);
        builder.AppendLine("  Death: " + extension.deathPolicy);
        builder.AppendLine("  Soul: " + extension.soulPolicy);
        builder.AppendLine("  Duration: " + extension.durationPolicy);
        builder.AppendLine("  Enforcement: needs=" + extension.enforceNeeds +
                           ", social=" + extension.enforceSocialPolicy +
                           ", gear=" + extension.enforceGearPolicy +
                           ", lifeStage=" + extension.enforceLifeStage +
                           ", markers=" + extension.enforceMarkers);
        builder.AppendLine("  Marker hediffs: " + FormatDefNames(extension.markerHediffs));
        builder.AppendLine("  Tags: " + FormatTags(extension.lifecycleTags));
        return builder.ToString().TrimEnd();
    }

    private static string FormatDefNames<TDef>(List<TDef> defs)
        where TDef : Def
    {
        if (defs == null || defs.Count == 0)
        {
            return "<none>";
        }

        StringBuilder builder = new();
        foreach (TDef def in defs)
        {
            if (def == null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(def.defName);
        }

        return builder.Length == 0 ? "<none>" : builder.ToString();
    }

    private static string FormatTags(List<string> tags)
    {
        if (tags == null || tags.Count == 0)
        {
            return "<none>";
        }

        StringBuilder builder = new();
        foreach (string tag in tags)
        {
            if (tag.NullOrEmpty())
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(tag);
        }

        return builder.Length == 0 ? "<none>" : builder.ToString();
    }
}
