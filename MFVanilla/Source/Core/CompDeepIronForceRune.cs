using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public sealed class CompProperties_DeepIronForceRune : CompProperties
{
    public int chargesRequired = 5;
    public int maxCharges = 10;
    public int abilityCooldownTicks = 900;
    public int shieldDurationTicks = 2500;
    public float shieldDamageFactor = 0.5f;
    public float lowHealthShieldThreshold = 0.5f;
    public float blastRadius = 5.9f;
    public float blastDamage = 28f;
    public int blastKnockbackDistance = 4;
    public int blastMinimumHostilePawns = 2;
    public string shieldOverlayTexturePath = "Things/Mote/ShieldBubble";
    public string shieldImpactFleckDef = "BulletShieldAreaEffect";
    public float shieldOverlayScale = 3.8f;

    public CompProperties_DeepIronForceRune()
    {
        compClass = typeof(CompDeepIronForceRune);
    }
}

public sealed class CompDeepIronForceRune : ThingComp
{
    private static readonly Dictionary<string, Material> OverlayMaterials = new();

    private int charges;
    private int shieldUntilTick = -1;
    private int nextAbilityTick;

    private CompProperties_DeepIronForceRune Props => (CompProperties_DeepIronForceRune)props;

    private Pawn ParentPawn => parent as Pawn;

    private bool ShieldActive => Find.TickManager != null && shieldUntilTick > Find.TickManager.TicksGame;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref charges, "charges", 0);
        Scribe_Values.Look(ref shieldUntilTick, "shieldUntilTick", -1);
        Scribe_Values.Look(ref nextAbilityTick, "nextAbilityTick", 0);
    }

    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        absorbed = false;
        if (ShieldActive && dinfo.Amount > 0f)
        {
            float originalAmount = dinfo.Amount;
            dinfo.SetAmount(Mathf.Max(0f, dinfo.Amount * Mathf.Clamp01(Props.shieldDamageFactor)));
            TrySpawnShieldImpact(ParentPawn);
            if (Prefs.DevMode && originalAmount > dinfo.Amount)
            {
                Log.Message($"[MFVanilla] Deep Iron force shield reduced {originalAmount:0.#} incoming damage to {dinfo.Amount:0.#}.");
            }
        }
    }

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        if (totalDamageDealt <= 0f || ParentPawn == null || ParentPawn.Dead)
        {
            return;
        }

        int gained = IsLikelyMagicalOrEnergyDamage(dinfo) ? 2 : 1;
        charges = Mathf.Min(Props.maxCharges, charges + gained);
    }

    public override void CompTick()
    {
        base.CompTick();
        Pawn pawn = ParentPawn;
        if (pawn == null || pawn.Dead || !pawn.Spawned || Find.TickManager == null)
        {
            return;
        }

        int currentTick = Find.TickManager.TicksGame;
        if (currentTick < nextAbilityTick || charges < Props.chargesRequired)
        {
            return;
        }

        if (ShouldUseShield(pawn))
        {
            ActivateShield(pawn, currentTick);
            return;
        }

        if (CountHostilePawnsInBlastRadius(pawn) >= Props.blastMinimumHostilePawns)
        {
            ActivateForceBlast(pawn, currentTick);
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
        Pawn pawn = ParentPawn;
        if (!ShieldActive || pawn == null || !pawn.Spawned)
        {
            return;
        }

        Material material = ResolveShieldOverlayMaterial();
        if (material == null)
        {
            return;
        }

        int ticks = Find.TickManager?.TicksGame ?? 0;
        float pulse = 1f + Mathf.Sin(ticks / 11f) * 0.035f;
        float scale = Mathf.Max(0.1f, Props.shieldOverlayScale) * pulse;
        Vector3 drawPos = pawn.DrawPos;
        drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.identity, new Vector3(scale, 1f, scale));
        Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
    }

    public override string CompInspectStringExtra()
    {
        StringBuilder builder = new();
        builder.Append("Force rune: ");
        builder.Append(charges);
        builder.Append('/');
        builder.Append(Props.maxCharges);
        builder.Append(" charge");
        if (charges != 1)
        {
            builder.Append('s');
        }

        if (ShieldActive)
        {
            int remainingTicks = shieldUntilTick - Find.TickManager.TicksGame;
            builder.AppendLine();
            builder.Append("Force shield active: ");
            builder.Append(remainingTicks.ToStringTicksToPeriod());
            builder.Append(" remaining");
        }

        return builder.ToString();
    }

    private bool ShouldUseShield(Pawn pawn)
    {
        if (ShieldActive || pawn.health == null || pawn.health.summaryHealth == null)
        {
            return false;
        }

        return pawn.health.summaryHealth.SummaryHealthPercent <= Props.lowHealthShieldThreshold;
    }

    private void ActivateShield(Pawn pawn, int currentTick)
    {
        charges -= Props.chargesRequired;
        shieldUntilTick = currentTick + Mathf.Max(1, Props.shieldDurationTicks);
        nextAbilityTick = currentTick + Mathf.Max(1, Props.abilityCooldownTicks);
        TrySpawnShieldImpact(pawn, 1.8f);
        TryShowMessage(
            pawn,
            "Deep Iron Golem: Force Shield",
            pawn.LabelCap + "'s chest rune consumes stored force and hardens into a visible shield, reducing incoming damage for "
            + Props.shieldDurationTicks.ToStringTicksToPeriod() + ".");
    }

    private void ActivateForceBlast(Pawn pawn, int currentTick)
    {
        charges -= Props.chargesRequired;
        nextAbilityTick = currentTick + Mathf.Max(1, Props.abilityCooldownTicks);

        List<Pawn> targets = new();
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, Props.blastRadius, true))
        {
            if (!cell.InBounds(pawn.Map))
            {
                continue;
            }

            List<Thing> things = cell.GetThingList(pawn.Map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Pawn target && target != pawn && !target.Dead && target.HostileTo(pawn))
                {
                    targets.Add(target);
                }
            }
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Pawn target = targets[i];
            target.TakeDamage(new DamageInfo(DamageDefOf.Blunt, Props.blastDamage, armorPenetration: 0.4f, instigator: pawn));
            TryKnockBack(pawn, target, Props.blastKnockbackDistance);
        }

        FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("BlastFlash");
        if (fleckDef != null)
        {
            FleckMaker.Static(pawn.DrawPos, pawn.Map, fleckDef, Mathf.Max(2f, Props.blastRadius * 0.55f));
        }

        TryShowMessage(
            pawn,
            "Deep Iron Golem: Force Blast",
            pawn.LabelCap + "'s chest rune erupts, damaging and throwing back nearby enemies.");
    }

    private static bool IsLikelyMagicalOrEnergyDamage(DamageInfo dinfo)
    {
        DamageDef def = dinfo.Def;
        if (def == null)
        {
            return false;
        }

        string defName = def.defName ?? string.Empty;
        return def == DamageDefOf.Flame
            || def == DamageDefOf.EMP
            || defName.IndexOf("burn", System.StringComparison.OrdinalIgnoreCase) >= 0
            || defName.IndexOf("flame", System.StringComparison.OrdinalIgnoreCase) >= 0
            || defName.IndexOf("frost", System.StringComparison.OrdinalIgnoreCase) >= 0
            || defName.IndexOf("psychic", System.StringComparison.OrdinalIgnoreCase) >= 0
            || defName.IndexOf("magic", System.StringComparison.OrdinalIgnoreCase) >= 0
            || defName.IndexOf("force", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private int CountHostilePawnsInBlastRadius(Pawn pawn)
    {
        int count = 0;
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, Props.blastRadius, true))
        {
            if (!cell.InBounds(pawn.Map))
            {
                continue;
            }

            List<Thing> things = cell.GetThingList(pawn.Map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Pawn target && target != pawn && !target.Dead && target.HostileTo(pawn))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static void TryKnockBack(Pawn source, Pawn target, int distance)
    {
        if (distance <= 0 || source?.Map == null || target == null || target.DestroyedOrNull())
        {
            return;
        }

        IntVec3 bestCell = target.Position;
        IntVec3 direction = new(Mathf.Clamp(target.Position.x - source.Position.x, -1, 1), 0, Mathf.Clamp(target.Position.z - source.Position.z, -1, 1));
        if (direction == IntVec3.Zero)
        {
            return;
        }

        for (int step = 1; step <= distance; step++)
        {
            IntVec3 candidate = target.Position + new IntVec3(direction.x * step, 0, direction.z * step);
            if (!candidate.InBounds(source.Map) || !candidate.Standable(source.Map) || candidate.GetFirstPawn(source.Map) != null)
            {
                break;
            }

            bestCell = candidate;
        }

        if (bestCell != target.Position)
        {
            target.Position = bestCell;
            target.Notify_Teleported(false, true);
        }
    }

    private Material ResolveShieldOverlayMaterial()
    {
        string texturePath = string.IsNullOrWhiteSpace(Props.shieldOverlayTexturePath)
            ? "Things/Mote/ShieldBubble"
            : Props.shieldOverlayTexturePath;
        string key = texturePath + "|deepIron";
        if (OverlayMaterials.TryGetValue(key, out Material material) && material != null)
        {
            return material;
        }

        Texture2D texture = ContentFinder<Texture2D>.Get(texturePath, false);
        if (texture == null)
        {
            OverlayMaterials[key] = null;
            return null;
        }

        material = MaterialPool.MatFrom(texture, ShaderDatabase.Transparent, new Color(0.48f, 0.78f, 1f, 0.52f));
        OverlayMaterials[key] = material;
        return material;
    }

    private void TrySpawnShieldImpact(Pawn pawn, float scale = 1.2f)
    {
        if (pawn?.Spawned != true)
        {
            return;
        }

        FleckDef fleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(Props.shieldImpactFleckDef);
        if (fleckDef != null)
        {
            FleckMaker.Static(pawn.DrawPos, pawn.Map, fleckDef, scale);
        }
    }

    private static void TryShowMessage(Pawn pawn, string title, string text)
    {
        if (pawn?.Map == null || !pawn.Map.mapPawns.AnyColonistSpawned)
        {
            return;
        }

        Messages.Message(text, pawn, MessageTypeDefOf.ThreatBig, historical: false);
        Find.LetterStack.ReceiveLetter(title, text, LetterDefOf.ThreatSmall, pawn);
    }
}
