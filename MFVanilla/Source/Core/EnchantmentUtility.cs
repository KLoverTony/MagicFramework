using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public static class EnchantmentUtility
{
    private const string FlamingLongswordRecipeDefName = "MFV_EnchantFlamingLongsword";
    private const string LongswordDefName = "MeleeWeapon_LongSword";
    private const string FlamingLongswordDefName = "MFV_FlamingLongsword";

    public static bool IsLongsword(Thing thing)
    {
        return thing?.def?.defName == LongswordDefName;
    }

    public static bool IsGoodOrBetterLongsword(Thing thing)
    {
        if (!IsLongsword(thing)) return false;
        return thing.TryGetQuality(out QualityCategory quality) && quality >= QualityCategory.Good;
    }

    public static bool TryMakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, out List<Thing> products)
    {
        products = null;

        if (recipeDef?.defName != FlamingLongswordRecipeDefName) return false;

        Thing sourceSword = ingredients?.FirstOrDefault(IsLongsword);
        ThingDef productDef = DefDatabase<ThingDef>.GetNamedSilentFail(FlamingLongswordDefName);
        if (sourceSword == null || productDef == null)
        {
            return false;
        }

        Thing product = ThingMaker.MakeThing(productDef, sourceSword.Stuff);
        product.HitPoints = GenMath.RoundRandom(product.MaxHitPoints * (sourceSword.HitPoints / (float)sourceSword.MaxHitPoints));
        product.HitPoints = Mathf.Clamp(product.HitPoints, 1, product.MaxHitPoints);
        product.SetFactionDirect(sourceSword.Faction);

        if (sourceSword.TryGetQuality(out QualityCategory quality))
        {
            product.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
        }

        products = new List<Thing> { product };
        return true;
    }
}

public class SpecialThingFilterWorker_GoodOrBetterLongsword : SpecialThingFilterWorker
{
    public override bool Matches(Thing t)
    {
        return EnchantmentUtility.IsGoodOrBetterLongsword(t);
    }

    public override bool CanEverMatch(ThingDef def)
    {
        return def?.defName == "MeleeWeapon_LongSword";
    }
}

public class SpecialThingFilterWorker_NormalOrWorseLongsword : SpecialThingFilterWorker
{
    public override bool Matches(Thing t)
    {
        if (t?.def?.defName != "MeleeWeapon_LongSword") return false;
        return !t.TryGetQuality(out QualityCategory quality) || quality < QualityCategory.Good;
    }

    public override bool CanEverMatch(ThingDef def)
    {
        return def?.defName == "MeleeWeapon_LongSword";
    }
}
