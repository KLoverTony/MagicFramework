using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public static class EnchantmentUtility
{
    private const float QualityBonusChancePerLeylineStrengthSum = 0.015f;
    private const float MaxLeylineQualityBonusChance = 0.35f;

    private static readonly EnchantmentRecipe[] Recipes =
    {
        new("MFV_EnchantFlamingLongsword", "MeleeWeapon_LongSword", "MFV_FlamingLongsword"),
        new("MFV_EnchantZephyrSpear", "MeleeWeapon_Spear", "MFV_ZephyrSpear"),
        new("MFV_EnchantTidebreakerMace", "MeleeWeapon_Mace", "MFV_TidebreakerMace"),
        new("MFV_EnchantStonefallMace", "MeleeWeapon_Mace", "MFV_StonefallMace"),
    };

    public static bool IsEnchantableSourceWeapon(Thing thing, RecipeDef recipeDef)
    {
        EnchantmentRecipe recipe = RecipeFor(recipeDef);
        return recipe != null && thing?.def?.defName == recipe.SourceDefName;
    }

    public static bool IsEnchantmentRecipe(RecipeDef recipeDef)
    {
        return RecipeFor(recipeDef) != null;
    }

    public static bool IsGoodOrBetterSourceWeapon(Thing thing, RecipeDef recipeDef)
    {
        if (!IsEnchantableSourceWeapon(thing, recipeDef)) return false;
        return thing.TryGetQuality(out QualityCategory quality) && quality >= QualityCategory.Good;
    }

    public static bool TryMakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing billGiver, out List<Thing> products)
    {
        products = null;

        EnchantmentRecipe recipe = RecipeFor(recipeDef);
        if (recipe == null) return false;

        Thing sourceWeapon = ingredients?.FirstOrDefault(thing => IsGoodOrBetterSourceWeapon(thing, recipeDef));
        ThingDef productDef = DefDatabase<ThingDef>.GetNamedSilentFail(recipe.ProductDefName);
        if (sourceWeapon == null || productDef == null)
        {
            return false;
        }

        Thing product = ThingMaker.MakeThing(productDef);
        product.HitPoints = product.MaxHitPoints;

        if (sourceWeapon.TryGetQuality(out QualityCategory quality))
        {
            QualityCategory finalQuality = ResolveLeylineEnhancedQuality(quality, billGiver);
            product.TryGetComp<CompQuality>()?.SetQuality(finalQuality, ArtGenerationContext.Colony);
            product.TryGetComp<CompArt>()?.JustCreatedBy(worker);
        }

        products = new List<Thing> { product };
        return true;
    }

    public static float LeylineQualityBonusChance(Thing forge)
    {
        if (forge?.Spawned != true || forge.TryGetComp<CompArcaneForge>() == null)
        {
            return 0f;
        }

        LeylineAreaReading reading = LeylineUtility.ReadThingFootprint(forge);
        return Mathf.Min(MaxLeylineQualityBonusChance, Mathf.Max(0f, reading.SumStrength * QualityBonusChancePerLeylineStrengthSum));
    }

    private static QualityCategory ResolveLeylineEnhancedQuality(QualityCategory sourceQuality, Thing billGiver)
    {
        if (sourceQuality >= QualityCategory.Legendary)
        {
            return sourceQuality;
        }

        float chance = LeylineQualityBonusChance(billGiver);
        if (chance <= 0f || !Rand.Chance(chance))
        {
            return sourceQuality;
        }

        return (QualityCategory)((int)sourceQuality + 1);
    }

    private static EnchantmentRecipe RecipeFor(RecipeDef recipeDef)
    {
        return recipeDef == null ? null : Recipes.FirstOrDefault(recipe => recipe.RecipeDefName == recipeDef.defName);
    }

    private sealed class EnchantmentRecipe
    {
        public EnchantmentRecipe(string recipeDefName, string sourceDefName, string productDefName)
        {
            RecipeDefName = recipeDefName;
            SourceDefName = sourceDefName;
            ProductDefName = productDefName;
        }

        public string RecipeDefName { get; }
        public string SourceDefName { get; }
        public string ProductDefName { get; }
    }
}
