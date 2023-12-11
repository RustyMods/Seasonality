using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class MaterialReplacer
{
    public static readonly Dictionary<string, Texture> CachedTextures = new();
    public static readonly Dictionary<string, Material> CachedMaterials = new();
    public static readonly Dictionary<string, Material> CustomMaterials = new();

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    static class ZoneSystemPatch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            GetAllMaterials();
            GetAllTextures();
        }
    }
    private static void GetAllMaterials()
    {
        Material[] allMats = Resources.FindObjectsOfTypeAll<Material>();
        foreach (Material item in allMats)
        {
            if (!item) continue;
            CachedMaterials[item.name.Replace("(Instance)", "").Replace(" ", "")] = item;
        }
    }
    private static void GetAllTextures()
    {
        foreach (Material material in CachedMaterials.Values)
        {
            if (!material) continue;
            string[] properties = material.GetTexturePropertyNames();
            if (Utils.FindTexturePropName(properties, "moss", out string mossProp))
            {
                Texture? tex = material.GetTexture(mossProp);
                if (tex) CachedTextures[material.name.Replace("(Instance)", "").Replace(" ", "") + "_moss"] = tex;
            }

            if (Utils.FindTexturePropName(properties, "normal", out string normalProp))
            {
                Texture? normal = material.GetTexture(normalProp);
                if (normal)
                    CachedTextures[material.name.Replace("(Instance)", "").Replace(" ", "") + "_normal"] = normal;
            }
            if (!Utils.FindTexturePropName(properties, "main", out string mainProp)) continue;
            CachedTextures[material.name.Replace("(Instance)", "").Replace(" ", "")] = material.GetTexture(mainProp);
            
        }
    }
    private static void SetMossTexture(string materialName, Texture originalTex)
    {
        if (!CachedTextures.TryGetValue("Pillar_snow_mat_moss", out Texture SnowMoss)) return;
        if (!CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) return;

        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        
        string[] properties = material.GetTexturePropertyNames();
        if (!Utils.FindTexturePropName(properties, "moss", out string mossProp)) return;
        
        switch (_Season.Value)
        {
            case Season.Winter:
                material.SetTexture(mossProp, SnowMoss);
                break;
            case Season.Fall:
                material.SetTexture(mossProp, HeathMoss);
                break;
            default:
                material.SetTexture(mossProp, originalTex);
                break;
        }
    }
    private static void SetCustomMossTexture(string materialName, Texture originalTex)
    {
        if (!CachedTextures.TryGetValue("Pillar_snow_mat_moss", out Texture SnowMoss)) return;
        if (!CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) return;

        if (!CustomMaterials.TryGetValue(materialName, out Material material)) return;
        
        string[] properties = material.GetTexturePropertyNames();
        if (!Utils.FindTexturePropName(properties, "moss", out string mossProp)) return;
        
        switch (_Season.Value)
        {
            case Season.Winter:
                material.SetTexture(mossProp, SnowMoss);
                break;
            case Season.Fall:
                material.SetTexture(mossProp, HeathMoss);
                break;
            default:
                material.SetTexture(mossProp, originalTex);
                break;
        }
    }
    private static void SetMainTexture(string materialName, Texture? tex)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        
        string[] properties = material.GetTexturePropertyNames();
        if (!Utils.FindTexturePropName(properties, "main", out string mainProp)) return;
        
        material.SetTexture(mainProp, tex);
    }
    private static void SetNormalTexture(string materialName, Texture? normal)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;

        string[] properties = material.GetTexturePropertyNames();
        if (normal == null) return;
        if (Utils.FindTexturePropName(properties, "normal", out string normalProp))
        {
            SeasonalityLogger.LogWarning("changing normal map for " + materialName + " " + normal.name);
            material.SetTexture(normalProp, normal);
        }
    }
    private static void SetCustomMainTexture(string materialName, Texture? tex, int index = 0)
    {
        if (!CustomMaterials.TryGetValue(materialName, out Material material))
        {
            SeasonalityLogger.LogWarning("failed to get custom material for = " + materialName);
            return;
        }
        
        string[] properties = material.GetTexturePropertyNames();
        if (!Utils.FindTexturePropName(properties, "main", out string mainProp)) return;
        
        material.SetTexture(mainProp, tex);
        switch (_Season.Value)
        {
            case Season.Fall:
                material.color = SeasonColors.FallColors[index];
                break;
            default:
                material.color = Color.white;
                break;
        }
    }
    public static void ModifyCachedMaterials()
    {
        ModifyMossMaterials();
        ModifyCreatures();
        ModifyVegetation(); 
        ModifyCustomMaterials();
        ModifyPieceMaterials();
        // ModifyNormals();
    }

    private static void ModifyPieceMaterials()
    {
        Dictionary<string, PieceDirectories> PiecesReplacementMap = new()
        {
            { "straw_roof", PieceDirectories.Straw },
            { "straw_roof_alpha", PieceDirectories.Straw },
            { "RoofShingles", PieceDirectories.DarkWood },
            { "GoblinVillage_Cloth", PieceDirectories.GoblinVillage },
            { "GoblinVillage", PieceDirectories.GoblinVillage },
        };
        
        Dictionary<string, PieceDirectories> PiecesWornReplacementMap = new()
        {
            { "straw_roof_worn", PieceDirectories.Straw },
            { "straw_roof_worn_alpha", PieceDirectories.Straw },
            { "RoofShingles_worn" , PieceDirectories.DarkWood },
            { "GoblinVillage", PieceDirectories.GoblinVillage }
        };

        Dictionary<string, PieceDirectories> PieceCornerReplacementMap = new()
        {
            { "straw_roof_corner_alpha", PieceDirectories.Straw },
        };

        Dictionary<string, PieceDirectories> PieceCornerWornReplacementMap = new()
        {
            { "straw_roof_corner_worn_alpha", PieceDirectories.Straw }
        };

        foreach (KeyValuePair<string, PieceDirectories> kvp in PiecesReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, PieceDirectories> kvp in PiecesWornReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key, true);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, PieceDirectories> kvp in PieceCornerReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key, isCorner: true);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, PieceDirectories> kvp in PieceCornerWornReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key, true, true);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }
    }
    private static void ModifyMossMaterials()
    {
        if (!CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) return;
        if (!CachedTextures.TryGetValue("swamptree_log_moss", out Texture StoneMossSwamp)) return;
        if (!CachedTextures.TryGetValue("runetablet_moss", out Texture StoneMoss)) return;
        if (!CachedTextures.TryGetValue("finewood_log_worn_moss", out Texture StoneKitMoss)) return;
        if (!CachedTextures.TryGetValue("dvergr_oak_worn_creep_mat_moss", out Texture MistLandMoss)) return;
        if (!CachedTextures.TryGetValue("rock1_copper_moss", out Texture ForestMoss)) return;
        if (!CachedTextures.TryGetValue("yggdrasil_branch_moss", out Texture YggMoss)) return;

        Dictionary<string, Texture> MossReplacementMap = new()
        {
            { "Altar_mat", HeathMoss },
            { "barnacle", StoneMossSwamp },
            { "beech_bark", StoneMoss },
            { "beech_bark_small", StoneMoss },
            { "bossstone_eikthyr", StoneMoss },
            { "bossstone_theelder", StoneMoss },
            { "bossstone_bonemass", StoneMoss },
            { "bossstone_dragonqueen_mat", StoneMoss },
            { "bossstone_yagluth_mat" , StoneMoss },
            { "bossstone_seekerqueen_mat", StoneMoss },
            { "corgihare", StoneMoss },
            { "Dirtwall", StoneMoss },
            { "dvergr_oak_worn_creep_mat", MistLandMoss },
            { "dvergrrunestone_mat", MistLandMoss },
            { "DvergrTownPiecesWornCreep_Mat", MistLandMoss },
            { "FingerRock", HeathMoss },
            { "finewood_log_worn", StoneKitMoss },
            { "finewood_log_destruction", StoneKitMoss },
            { "finewood_log_broken" , StoneKitMoss },
            { "Firetree_oldlog", StoneMoss },
            { "heathrock", HeathMoss },
            { "hugeskull", StoneMossSwamp },
            { "leviathan", StoneMossSwamp },
            { "leviathan_rock_4", StoneMossSwamp },
            { "NestRock", StoneKitMoss },
            { "oak_bark", StoneKitMoss },
            { "oak_bark_quarter", StoneKitMoss },
            { "ObsidianRock_mat", StoneMoss },
            { "rock1", ForestMoss },
            { "rock1_copper", ForestMoss },
            { "rock1_mountain", StoneMoss },
            { "rock3_silver", ForestMoss },
            { "rock4_coast", StoneMoss },
            { "rock_heath", HeathMoss },
            { "Rocks_3_roughness", ForestMoss },
            { "Rocks_4_roughness", StoneMoss },
            { "Rocks_4_roughness_interior", StoneMoss },
            { "Rocks_4_roughness_plains", HeathMoss },
            { "roundlog", StoneKitMoss },
            { "runestone", StoneMoss },
            { "runetablet", StoneMoss },
            { "runetablet_plains", HeathMoss },
            { "runetablet_vegvisir", StoneMoss },
            { "Shoot_Stump_mat", StoneKitMoss },
            { "Shoot_Trunk_mat", StoneKitMoss },
            { "Stalagtite_mat", StoneMoss },
            { "startplatform", StoneMoss },
            { "statue1", StoneMossSwamp },
            { "stone_huge", StoneMoss },
            { "stone_large", StoneKitMoss },
            { "stone_large_interior", StoneKitMoss },
            { "stoneblock", StoneMoss },
            { "stonechest", StoneKitMoss },
            { "stonechest_plains", HeathMoss },
            { "stonechest_sunkencrypt", HeathMoss },
            { "stonekit_floor_interior", StoneMoss },
            { "stonekit_stone_mat", StoneKitMoss },
            { "stonepillar", StoneMoss },
            { "stoneslab", StoneKitMoss },
            { "stonewall", StoneMoss },
            { "stonewall_1", StoneMoss },
            { "stump", StoneMoss },
            { "sunkenkit_stone_mat_interior1", StoneMoss },
            { "sunkenkit_stone_mat_interior3", StoneMoss },
            { "sunkenkit_stone_mat_interior4", StoneMoss },
            { "sunkenkit_stone_mat_interior5", StoneMoss },
            { "sunkenkit_stone_mat_interior_triplanar", StoneMoss },
            { "swamptree1_bark", StoneMossSwamp },
            { "swamptree2_bark", StoneMossSwamp },
            { "swamptree2_barkInfested", StoneMossSwamp },
            { "swamptree2_log", StoneMossSwamp },
            { "swamptree_log", StoneMossSwamp },
            { "swamptree_stump", StoneMossSwamp },
            { "Tradaerrune_Mat", StoneMoss },
            { "wood_pile_Broken", StoneKitMoss },
            { "wood_pile_Worn", StoneKitMoss },
            { "yggdrasil_branch", YggMoss },

        };
        foreach(KeyValuePair<string, Texture> kvp in MossReplacementMap) SetMossTexture(kvp.Key, kvp.Value);
    }
    private static void ModifyCustomMossMaterials()
    {
        if (!CachedTextures.TryGetValue("runetablet_moss", out Texture StoneMoss)) return;
        if (!CachedTextures.TryGetValue("yggdrasil_branch_moss", out Texture YggMoss)) return;

        Dictionary<string, Texture> CustomMossMap = new()
        {
            { "custom_beech_bark_small_0", StoneMoss },
            { "custom_beech_bark_small_1", StoneMoss },
            { "custom_beech_bark_small_2", StoneMoss },
            { "custom_beech_bark_small_3", StoneMoss },
            
            { "custom_beech_bark_0", StoneMoss },
            { "custom_beech_bark_1", StoneMoss },
            { "custom_beech_bark_2", StoneMoss },
            { "custom_beech_bark_3", StoneMoss },
            
            { "custom_birch_bark_0", StoneMoss },
            { "custom_birch_bark_1", StoneMoss },
            { "custom_birch_bark_2", StoneMoss },
            { "custom_birch_bark_3", StoneMoss },
            
            { "custom_oak_bark_0", StoneMoss },
            { "custom_oak_bark_1", StoneMoss },
            { "custom_oak_bark_2", StoneMoss },
            { "custom_oak_bark_3", StoneMoss },
            
            { "custom_Shoot_Trunk_mat_0", YggMoss },
            { "custom_Shoot_Trunk_mat_1", YggMoss },
            { "custom_Shoot_Trunk_mat_2", YggMoss },
            { "custom_Shoot_Trunk_mat_3", YggMoss },
        };
        
        foreach(KeyValuePair<string, Texture> kvp in CustomMossMap) SetCustomMossTexture(kvp.Key, kvp.Value);
    }
    private static void ModifyCreatures()
    {
        Dictionary<string, CreatureDirectories> CreatureReplacementMap = new()
        {
            { "lox", CreatureDirectories.Lox },
            { "troll", CreatureDirectories.Troll },
            { "Hare_mat", CreatureDirectories.Hare },
            { "Feasting_mat", CreatureDirectories.Tick },
            { "SeaSerpent_mat", CreatureDirectories.Serpent },
            { "swampfish", CreatureDirectories.Leech },
            { "Deathsquito_mat", CreatureDirectories.Deathsquito },
            { "seagal", CreatureDirectories.Gull },
            { "neck", CreatureDirectories.Neck },
            { "wraith", CreatureDirectories.Wraith },
            { "blob", CreatureDirectories.Blob },
            { "blob_elite", CreatureDirectories.Oozer },
            { "gjall_mat", CreatureDirectories.Gjall }

        };
        foreach (KeyValuePair<string, CreatureDirectories> kvp in CreatureReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key);
            if (!texture) continue;
            SetMainTexture(kvp.Key, texture);
        }
    }
    private static void ModifyVegetation()
    {
        Dictionary<string, VegDirectories> VegetationReplacementMap = new()
        {
            { "beech_leaf", VegDirectories.Beech },
            { "beech_leaf_sapling", VegDirectories.BeechSmall },
            { "beech_leaf_small", VegDirectories.BeechSmall },
            { "beech_particle", VegDirectories.Beech },
            { "birch_leaf" , VegDirectories.Birch },
            { "birch_leaf_aut", VegDirectories.Birch },
            { "birch_particle", VegDirectories.Birch },
            { "birch_seeds_leaf", VegDirectories.Birch },
            { "oak_leaf", VegDirectories.Oak },
            { "oak_particle", VegDirectories.Oak },
            { "Pine_tree_small", VegDirectories.FirSmall },
            { "Pine_tree", VegDirectories.Fir },
            { "Pine_tree_small_dead", VegDirectories.FirDead },
            { "branch_dead_particle", VegDirectories.FirDeadParticles },
            { "Pine_tree_xmas", VegDirectories.FirSmall },
            { "branch_particle", VegDirectories.FirParticles },
            { "PineTree_01", VegDirectories.Pine },
            { "PineCone", VegDirectories.PineParticles },
            { "yggdrasil_branch_leafs", VegDirectories.YggaShoot },
            { "Shoot_Leaf_mat", VegDirectories.YggaShoot },
            { "shoot_leaf_particle", VegDirectories.YggaShoot },
            { "swamptree1_branch", VegDirectories.SwampTrees },
            { "swamptree2_branch", VegDirectories.SwampGrass },
            { "Bush01", VegDirectories.Bushes },
            { "Bush01_blueberry", VegDirectories.Bushes },
            { "Bush01_heath", VegDirectories.Bushes },
            { "Bush01_raspberry", VegDirectories.Bushes },
            { "Bush02_en" , VegDirectories.PlainsBush },
            { "Cloudberrybush", VegDirectories.CloudberryBush },
            { "Vines_Mat", VegDirectories.Vines },
            { "VinesBranch_mat", VegDirectories.Vines },
            { "shrub", VegDirectories.Shrub },
            { "shrub_heath", VegDirectories.Shrub },
            { "shrub2_leafparticle", VegDirectories.ShrubParticles },
            { "shrub2_leafparticle_heath", VegDirectories.ShrubParticles },
        };
        
        foreach (KeyValuePair<string, VegDirectories> kvp in VegetationReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }
    }

    private static void ModifyNormals()
    {
        Dictionary<string, VegDirectories> NormalReplacementMap = new()
        {
            {"beech_bark", VegDirectories.Beech}
        };
        foreach (KeyValuePair<string, VegDirectories> kvp in NormalReplacementMap)
        {
            Texture? normal = GetCustomNormals(kvp.Value, kvp.Key);
            if (!normal) continue;
            
            SetNormalTexture(kvp.Key, normal);
        }
    }
    private static void ModifyCustomMaterials()
    {
        Dictionary<string, VegDirectories> CustomReplacementMap = new()
        {
            { "custom_beech_leaf_small_0", VegDirectories.BeechSmall },
            { "custom_beech_leaf_small_1", VegDirectories.BeechSmall },
            { "custom_beech_leaf_small_2", VegDirectories.BeechSmall },
            { "custom_beech_leaf_small_3", VegDirectories.BeechSmall },
            
            { "custom_beech_leaf_0", VegDirectories.Beech },
            { "custom_beech_leaf_1", VegDirectories.Beech },
            { "custom_beech_leaf_2", VegDirectories.Beech },
            { "custom_beech_leaf_3", VegDirectories.Beech },
            
            { "custom_birch_leaf_0", VegDirectories.Birch },
            { "custom_birch_leaf_1", VegDirectories.Birch },
            { "custom_birch_leaf_2", VegDirectories.Birch },
            { "custom_birch_leaf_3", VegDirectories.Birch },
            
            { "custom_birch_leaf_aut_0", VegDirectories.Birch },
            { "custom_birch_leaf_aut_1", VegDirectories.Birch },
            { "custom_birch_leaf_aut_2", VegDirectories.Birch },
            { "custom_birch_leaf_aut_3", VegDirectories.Birch },
            
            { "custom_Bush01_blueberry_0", VegDirectories.Bushes },
            { "custom_Bush01_blueberry_1", VegDirectories.Bushes },
            { "custom_Bush01_blueberry_2", VegDirectories.Bushes },
            { "custom_Bush01_blueberry_3", VegDirectories.Bushes },
            
            { "custom_Bush01_0", VegDirectories.Bushes },
            { "custom_Bush01_1", VegDirectories.Bushes },
            { "custom_Bush01_2", VegDirectories.Bushes },
            { "custom_Bush01_3", VegDirectories.Bushes },
            
            { "custom_Bush01_heath_0", VegDirectories.Bushes },
            { "custom_Bush01_heath_1", VegDirectories.Bushes },
            { "custom_Bush01_heath_2", VegDirectories.Bushes },
            { "custom_Bush01_heath_3", VegDirectories.Bushes },

            { "custom_Bush02_en_0", VegDirectories.PlainsBush },
            { "custom_Bush02_en_1", VegDirectories.PlainsBush },
            { "custom_Bush02_en_2", VegDirectories.PlainsBush },
            { "custom_Bush02_en_3", VegDirectories.PlainsBush },
            
            { "custom_oak_leaf_0", VegDirectories.Oak },
            { "custom_oak_leaf_1", VegDirectories.Oak },
            { "custom_oak_leaf_2", VegDirectories.Oak },
            { "custom_oak_leaf_3", VegDirectories.Oak },
            
            { "custom_Bush01_raspberry_0", VegDirectories.Bushes },
            { "custom_Bush01_raspberry_1", VegDirectories.Bushes },
            { "custom_Bush01_raspberry_2", VegDirectories.Bushes },
            { "custom_Bush01_raspberry_3", VegDirectories.Bushes },
            
            { "custom_shrub_0", VegDirectories.Shrub },
            { "custom_shrub_1", VegDirectories.Shrub },
            { "custom_shrub_2", VegDirectories.Shrub },
            { "custom_shrub_3", VegDirectories.Shrub },
            
            { "custom_shrub_heath_0", VegDirectories.Shrub },
            { "custom_shrub_heath_1", VegDirectories.Shrub },
            { "custom_shrub_heath_2", VegDirectories.Shrub },
            { "custom_shrub_heath_3", VegDirectories.Shrub },
            
            { "custom_Vines_Mat_0", VegDirectories.Vines },
            { "custom_Vines_Mat_1", VegDirectories.Vines },
            { "custom_Vines_Mat_2", VegDirectories.Vines },
            { "custom_Vines_Mat_3", VegDirectories.Vines },
            
            { "custom_Shoot_Leaf_mat_0", VegDirectories.YggaShoot },
            { "custom_Shoot_Leaf_mat_1", VegDirectories.YggaShoot },
            { "custom_Shoot_Leaf_mat_2", VegDirectories.YggaShoot },
            { "custom_Shoot_Leaf_mat_3", VegDirectories.YggaShoot },
        };

        foreach (KeyValuePair<string, VegDirectories> kvp in CustomReplacementMap)
        {
            string normalizedName = kvp.Key.Replace("custom_", "")
                .Replace("_0", "").Replace("_1", "").Replace("_2", "").Replace("_3", "");
            int index = int.Parse(kvp.Key.Substring(kvp.Key.Length - 1));
            Texture? texture = GetCustomTexture(kvp.Value, normalizedName);
            SetCustomMainTexture(kvp.Key, texture, index);
        }
        
        ModifyCustomMossMaterials();
    }
    private static Texture? GetCustomTexture(CreatureDirectories directory, string originalMaterialName)
    {
        Texture? customTexture = Utils.GetCustomTexture(directory, _Season.Value);
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
    private static Texture? GetCustomTexture(PieceDirectories directory, string originalMaterialName, bool isWorn = false, bool isCorner = false)
    {
        string filePath = _Season.Value.ToString();
        string wornFilePath = _Season.Value + "_worn";
        string cornerFilePath = _Season.Value + "_corner";
        string cornerWornFilePath = _Season.Value + "_corner_worn";

        string path = isWorn ? wornFilePath : filePath;
        if (isCorner) path = isWorn ? cornerWornFilePath : cornerFilePath;
        
        Texture? customTexture = Utils.GetCustomTexture(directory, path);
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
    private static Texture? GetCustomTexture(VegDirectories directory, string originalMaterialName, bool isBark = false)
    {
        Texture? customTexture = Utils.GetCustomTexture(directory,  isBark ? _Season.Value + "_bark" : _Season.Value.ToString());
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }

    private static Texture? GetCustomNormals(VegDirectories directory, string originalMaterialName)
    {
        Texture? customTexture = Utils.GetCustomTexture(directory,  _Season.Value + "_normal");
        CachedTextures.TryGetValue(originalMaterialName + "_normal", out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
}