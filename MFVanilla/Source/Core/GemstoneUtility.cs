using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MFVanilla.Core;

public static class GemstoneUtility
{
    public const string GemstoneVeinDefName = "MFV_GemstoneVein";
    public const string BreakChunkRecipeDefName = "MFV_BreakGemstoneChunk";
    public const string BreakDenseChunkRecipeDefName = "MFV_BreakDenseGemstoneChunk";
    public const string CutGemstoneRecipeDefName = "MFV_CutGemstone";
    public const string GemstoneDustDefName = "MFV_GemstoneDust";

    private static readonly GemstoneFamily[] Families =
    {
        new("Ruby", "MFV_RubyChunk", "MFV_DenseRubyChunk", "MFV_RawRubyPiece", "MFV_CommonRuby", "MFV_FineRuby", "MFV_ExquisiteRuby"),
        new("Sapphire", "MFV_SapphireChunk", "MFV_DenseSapphireChunk", "MFV_RawSapphirePiece", "MFV_CommonSapphire", "MFV_FineSapphire", "MFV_ExquisiteSapphire"),
        new("Emerald", "MFV_EmeraldChunk", "MFV_DenseEmeraldChunk", "MFV_RawEmeraldPiece", "MFV_CommonEmerald", "MFV_FineEmerald", "MFV_ExquisiteEmerald"),
        new("Diamond", "MFV_DiamondChunk", "MFV_DenseDiamondChunk", "MFV_RawDiamondPiece", "MFV_CommonDiamond", "MFV_FineDiamond", "MFV_ExquisiteDiamond"),
        new("Amethyst", "MFV_AmethystChunk", "MFV_DenseAmethystChunk", "MFV_RawAmethystPiece", "MFV_CommonAmethyst", "MFV_FineAmethyst", "MFV_ExquisiteAmethyst"),
        new("Topaz", "MFV_TopazChunk", "MFV_DenseTopazChunk", "MFV_RawTopazPiece", "MFV_CommonTopaz", "MFV_FineTopaz", "MFV_ExquisiteTopaz"),
    };

    public static bool IsGemstoneVein(Mineable mineable)
    {
        return mineable?.def?.defName == GemstoneVeinDefName;
    }

    public static void SpawnMineYield(Mineable mineable, Map map, Pawn miner)
    {
        if (mineable == null || map == null) return;

        int skill = SkillLevel(miner, SkillDefOf.Mining);
        int chunkCount = Rand.Chance(Mathf.Clamp(0.1f + skill * 0.025f, 0.1f, 0.6f)) ? 2 : 1;
        float denseChance = Mathf.Clamp(0.08f + skill * 0.018f, 0.08f, 0.35f);

        for (int i = 0; i < chunkCount; i++)
        {
            GemstoneFamily family = RandomFamily();
            ThingDef chunkDef = family.ResolveChunkDef(Rand.Chance(denseChance));
            if (chunkDef == null) continue;

            Thing chunk = ThingMaker.MakeThing(chunkDef);
            chunk.stackCount = 1;
            GenPlace.TryPlaceThing(chunk, mineable.Position, map, ThingPlaceMode.Near);
        }
    }

    public static bool TryMakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, out List<Thing> products)
    {
        products = null;
        if (recipeDef == null) return false;

        if (recipeDef.defName == BreakChunkRecipeDefName)
        {
            GemstoneFamily family = FamilyFromIngredient(ingredients, GemstoneInputKind.StandardChunk);
            products = family == null ? new List<Thing>() : MakeProducts(family.RawPieceDefName, 3);
            return true;
        }

        if (recipeDef.defName == BreakDenseChunkRecipeDefName)
        {
            GemstoneFamily family = FamilyFromIngredient(ingredients, GemstoneInputKind.DenseChunk);
            products = family == null ? new List<Thing>() : MakeProducts(family.RawPieceDefName, 7);
            return true;
        }

        if (recipeDef.defName == CutGemstoneRecipeDefName)
        {
            GemstoneFamily family = FamilyFromIngredient(ingredients, GemstoneInputKind.RawPiece);
            if (family == null)
            {
                products = new List<Thing>();
                return true;
            }

            GemstoneCutResult result = family.CutResultForSkill(SkillLevel(worker, SkillDefOf.Crafting));
            products = MakeProducts(result.DefName, 1);
            AddProduct(products, GemstoneDustDefName, result.DustYield);
            return true;
        }

        return false;
    }

    private static GemstoneFamily RandomFamily()
    {
        return Families[Rand.RangeInclusive(0, Families.Length - 1)];
    }

    private static GemstoneFamily FamilyFromIngredient(List<Thing> ingredients, GemstoneInputKind inputKind)
    {
        if (ingredients == null) return null;

        foreach (Thing ingredient in ingredients)
        {
            string defName = ingredient?.def?.defName;
            if (defName == null) continue;

            GemstoneFamily family = Families.FirstOrDefault(candidate => candidate.Matches(defName, inputKind));
            if (family != null) return family;
        }

        return null;
    }

    private static List<Thing> MakeProducts(string defName, int count)
    {
        ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        if (thingDef == null) return new List<Thing>();

        Thing thing = ThingMaker.MakeThing(thingDef);
        thing.stackCount = count;
        return new List<Thing> { thing };
    }

    private static void AddProduct(List<Thing> products, string defName, int count)
    {
        ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        if (thingDef == null) return;

        Thing thing = ThingMaker.MakeThing(thingDef);
        thing.stackCount = count;
        products.Add(thing);
    }

    private static int SkillLevel(Pawn pawn, SkillDef skillDef)
    {
        return pawn?.skills?.GetSkill(skillDef)?.Level ?? 0;
    }

    private enum GemstoneInputKind
    {
        StandardChunk,
        DenseChunk,
        RawPiece,
    }

    private sealed class GemstoneFamily
    {
        private readonly string _standardChunkDefName;
        private readonly string _denseChunkDefName;
        public readonly string RawPieceDefName;
        private readonly GemstoneCutResult _commonCut;
        private readonly GemstoneCutResult _fineCut;
        private readonly GemstoneCutResult _exquisiteCut;

        public GemstoneFamily(string label, string standardChunkDefName, string denseChunkDefName, string rawPieceDefName, string commonCutDefName, string fineCutDefName, string exquisiteCutDefName)
        {
            Label = label;
            _standardChunkDefName = standardChunkDefName;
            _denseChunkDefName = denseChunkDefName;
            RawPieceDefName = rawPieceDefName;
            _commonCut = new GemstoneCutResult(commonCutDefName, 3);
            _fineCut = new GemstoneCutResult(fineCutDefName, 2);
            _exquisiteCut = new GemstoneCutResult(exquisiteCutDefName, 1);
        }

        public string Label { get; }

        public ThingDef ResolveChunkDef(bool dense)
        {
            string defName = dense ? _denseChunkDefName : _standardChunkDefName;
            return DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        }

        public bool Matches(string defName, GemstoneInputKind inputKind)
        {
            return inputKind switch
            {
                GemstoneInputKind.StandardChunk => defName == _standardChunkDefName,
                GemstoneInputKind.DenseChunk => defName == _denseChunkDefName,
                GemstoneInputKind.RawPiece => defName == RawPieceDefName,
                _ => false,
            };
        }

        public GemstoneCutResult CutResultForSkill(int craftingSkill)
        {
            float normalizedSkill = Mathf.Clamp01(craftingSkill / 20f);
            float exquisiteChance = Mathf.Lerp(0f, 0.18f, Mathf.InverseLerp(8f, 20f, craftingSkill));
            float fineChance = Mathf.Lerp(0.08f, 0.55f, normalizedSkill);
            float roll = Rand.Value;

            if (roll < exquisiteChance) return _exquisiteCut;
            if (roll < exquisiteChance + fineChance) return _fineCut;
            return _commonCut;
        }
    }

    private readonly struct GemstoneCutResult
    {
        public GemstoneCutResult(string defName, int dustYield)
        {
            DefName = defName;
            DustYield = dustYield;
        }

        public string DefName { get; }
        public int DustYield { get; }
    }
}
