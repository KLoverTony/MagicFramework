using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public static class EnchantmentUtility
{
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

    public static bool TryMakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, out List<Thing> products)
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
        product.HitPoints = GenMath.RoundRandom(product.MaxHitPoints * (sourceWeapon.HitPoints / (float)sourceWeapon.MaxHitPoints));
        product.HitPoints = Mathf.Clamp(product.HitPoints, 1, product.MaxHitPoints);

        if (sourceWeapon.TryGetQuality(out QualityCategory quality))
        {
            product.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
        }

        products = new List<Thing> { product };
        return true;
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
