using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class MaterialReplacer
{
    public static readonly Dictionary<string, Texture> CachedTextures = new();
    public static readonly Dictionary<string, Material> CachedMaterials = new();
    public static readonly Dictionary<string, Material> CustomMaterials = new();
    private static readonly List<string> DefaultColorChange = new()
    {
        "CapeDeerHide",
        "CapeTrollHide",
        "helmet_trollleather"
    };
    private static readonly int ChestTex = Shader.PropertyToID("_ChestTex");
    private static readonly int LegsTex = Shader.PropertyToID("_LegsTex");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");
    private static readonly int ColorProp = Shader.PropertyToID("_Color");
    private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");


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
            string normalizedName = item.name.Replace("(Instance)", "").Replace(" ", "");
            if (DefaultColorChange.Contains(normalizedName))
            {
                if (!HDPackLoaded && _ReplaceArmorTextures.Value is Toggle.On)
                {
                    item.SetColor(ColorProp, Color.white);
                }
            }
            CachedMaterials[normalizedName] = item;
        }
    }
    private static void GetAllTextures()
    {
        foreach (Material material in CachedMaterials.Values)
        {
            if (!material) continue;
            string normalizedName = material.name.Replace("(Instance)", "").Replace(" ", "");
            if (material.HasProperty(MainTex)) CachedTextures[normalizedName] = material.GetTexture(MainTex);
            if (material.HasProperty(MossTex)) CachedTextures[normalizedName + "_moss"] = material.GetTexture(MossTex);
            if (material.HasProperty(BumpMap)) CachedTextures[normalizedName + "_normal"] = material.GetTexture(BumpMap);
            if (material.HasProperty(ChestTex)) CachedTextures[normalizedName + "_chest"] = material.GetTexture(ChestTex);
            if (material.HasProperty(LegsTex)) CachedTextures[normalizedName + "_legs"] = material.GetTexture(LegsTex);
        }
    }
    private static void SetMossTexture(string materialName, Texture originalTex)
    {
        if (!CachedTextures.TryGetValue("Pillar_snow_mat_moss", out Texture SnowMoss)) return;
        if (!CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) return;

        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        
        switch (_Season.Value)
        {
            case Season.Winter:
                material.SetTexture(MossTex, SnowMoss);
                break;
            case Season.Fall:
                material.SetTexture(MossTex, HeathMoss);
                break;
            default:
                material.SetTexture(MossTex, originalTex);
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
        material.SetTexture(MainTex, tex);
    }
    private static void SetColor(string materialName, Color32 color)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        material.SetColor(ColorProp, color);
    }
    private static void SetChestTexture(string materialName, Texture? tex)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        material.SetTexture(ChestTex, tex);
    }
    private static void SetLegsTexture(string materialName, Texture? tex)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        material.SetTexture(LegsTex, tex);
    }
    private static void SetNormalTexture(string materialName, Texture? normal)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        string[] properties = material.GetTexturePropertyNames();
        if (normal == null) return;
        if (Utils.FindTexturePropName(properties, "bump", out string normalProp))
        {
            SeasonalityLogger.LogDebug($"changing {normalProp} for " + materialName + " " + normal.name);
            material.SetTexture(normalProp, normal);
        }
    }
    private static void SetCustomMainTexture(string materialName, Texture? tex, int index = 0)
    {
        if (!CustomMaterials.TryGetValue(materialName, out Material material))
        {
            // SeasonalityLogger.LogWarning("failed to get custom material for = " + materialName);
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
        ModifyBarkMaterials();
        ModifyPickableMaterials();
        ModifyNormals();
        if (_ReplaceArmorTextures.Value is Toggle.On) ModifyArmorMaterials();
    }

    private static void ModifyArmorMaterials()
    {
        Dictionary<string, ArmorDirectories> ChestReplacementMap = new()
        {
            {"RagsChest",ArmorDirectories.Rags},
            {"LeatherChest",ArmorDirectories.Leather},
            {"TrollLeatherChest",ArmorDirectories.Troll},
        };
        Dictionary<string, ArmorDirectories> LegsReplacementMap = new()
        {
            {"RagsLegs",ArmorDirectories.Rags},
            {"LeatherPants",ArmorDirectories.Leather},
            {"TrollLeatherPants",ArmorDirectories.Troll}
        };
        Dictionary<string, ArmorDirectories> CapeReplacementMap = new()
        {
            {"CapeDeerHide", ArmorDirectories.Leather},
            {"CapeTrollHide",ArmorDirectories.Troll},
            {"WolfCape",ArmorDirectories.Wolf},
            {"LoxCape_Mat",ArmorDirectories.Padded},
            {"feathercape_mat",ArmorDirectories.Mage}
        };
        Dictionary<string, ArmorDirectories> HelmetReplacementMap = new()
        {
            {"helmet_leather_mat", ArmorDirectories.Leather},
            {"helmet_trollleather",ArmorDirectories.Troll},
            {"helmet_bronze_mat",ArmorDirectories.Bronze},
            {"helmet_iron_mat",ArmorDirectories.Iron},
        };
        Dictionary<string, ArmorDirectories> ArmorReplacementMap = new()
        {
            {"BronzeArmorMesh_Mat",ArmorDirectories.Bronze},
            {"IronArmorChest_mat",ArmorDirectories.Iron},
            {"SilverArmourChest_mat", ArmorDirectories.Wolf},
            {"DragonVisor_Mat",ArmorDirectories.Wolf},
            {"Padded_mat",ArmorDirectories.Padded},
            {"carapacearmor_mat",ArmorDirectories.Carapace},
            {"MageArmor_mat",ArmorDirectories.Mage}
        };
        Dictionary<string, ArmorDirectories> ArmorLegsReplacementMap = new()
        {
            {"IronArmorLegs_mat",ArmorDirectories.Iron}
        };
        foreach (KeyValuePair<string, ArmorDirectories> kvp in ChestReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key + "_chest", out Texture tex)) continue;
                SetChestTexture(kvp.Key, tex);
                continue;
            }
            SetChestTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, ArmorDirectories> kvp in LegsReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key,  true);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key + "_legs", out Texture tex)) continue;
                SetLegsTexture(kvp.Key, tex);
                continue;
            }
            SetLegsTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, ArmorDirectories> kvp in CapeReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key, isCape: true);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key + "_cape", out Texture tex)) continue;
                SetMainTexture(kvp.Key, tex);
                continue;
            }
            SetMainTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, ArmorDirectories> kvp in HelmetReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key, isHelmet: true);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key, out Texture tex)) continue;
                SetMainTexture(kvp.Key, tex);
                continue;
            }
            SetMainTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, ArmorDirectories> kvp in ArmorReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key, out Texture tex)) continue;
                SetMainTexture(kvp.Key, tex);
                continue;
            }
            SetMainTexture(kvp.Key, texture);
        }
        foreach (KeyValuePair<string, ArmorDirectories> kvp in ArmorLegsReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key, isLegs: true);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key, out Texture tex)) continue;
                SetMainTexture(kvp.Key, tex);
                continue;
            }
            SetMainTexture(kvp.Key, texture);
        }
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
    private static void ModifyPickableMaterials()
    {
        Dictionary<string, PickableDirectories> PickableReplacementMap = new()
        {
            {"Boletus_edulis", PickableDirectories.Mushroom},
            {"Boletus_Yellow", PickableDirectories.MushroomYellow},
            {"Boletus_blue", PickableDirectories.MushroomBlue},
            {"MistlandsMushrooms_balls",PickableDirectories.JotunPuff},
            {"MistlandsMushrooms_magecap", PickableDirectories.Magecap},
            {"raspberry", PickableDirectories.Raspberry},
            {"blueberry",PickableDirectories.Blueberry},
            {"Dandelion", PickableDirectories.Dandelion},
            {"barley_ripe",PickableDirectories.Barley},
            {"flax_ripe", PickableDirectories.Flax},
            {"flax_item", PickableDirectories.Flax},
            {"carrot",PickableDirectories.Carrot},
            {"turnip_0",PickableDirectories.Turnip},
            {"onion_ripe",PickableDirectories.Onion},
            {"carrot_flower",PickableDirectories.CarrotSeed},
            {"turnip_flower",PickableDirectories.TurnipSeed},
            {"onion_seedling_ripe",PickableDirectories.OnionSeed},
            {"branch",PickableDirectories.Branches},
            {"flint",PickableDirectories.Flint},
            {"stone",PickableDirectories.Rock},
            {"fi_village_catacombs",PickableDirectories.BoneRemains},
            {"surtlingcore",PickableDirectories.SurtlingCore},
            {"BlackCore_mat",PickableDirectories.BlackCore},
            {"swampplant2",PickableDirectories.Thistle}
        };
        foreach (KeyValuePair<string, PickableDirectories> kvp in PickableReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }
    }
    private static void ModifyBarkMaterials()
    {
        Dictionary<string, VegDirectories> BarkReplacementMap = new()
        {
            {"olive_treebark2_darkland", VegDirectories.SwampTrees},
            {"swamptree1_bark", VegDirectories.SwampTrees},
            {"swamptree2_bark", VegDirectories.SwampTrees},
        };

        Dictionary<string, VegDirectories> CustomBarkReplacementMap = new()
        {
            {"custom_beech_bark_0", VegDirectories.Beech},
            {"custom_beech_bark_1", VegDirectories.Beech},
            {"custom_beech_bark_2", VegDirectories.Beech},
            {"custom_beech_bark_3", VegDirectories.Beech},
            
            {"custom_beech_bark_small_0", VegDirectories.BeechSmall},
            {"custom_beech_bark_small_1", VegDirectories.BeechSmall},
            {"custom_beech_bark_small_2", VegDirectories.BeechSmall},
            {"custom_beech_bark_small_3", VegDirectories.BeechSmall},
            
            {"custom_birch_bark_0", VegDirectories.Birch},
            {"custom_birch_bark_1", VegDirectories.Birch},
            {"custom_birch_bark_2", VegDirectories.Birch},
            {"custom_birch_bark_3", VegDirectories.Birch},
            
            {"custom_oak_bark_0", VegDirectories.Oak},
            {"custom_oak_bark_1", VegDirectories.Oak},
            {"custom_oak_bark_2", VegDirectories.Oak},
            {"custom_oak_bark_3", VegDirectories.Oak},
            
            {"custom_Shoot_Trunk_mat_0", VegDirectories.YggaShoot},
            {"custom_Shoot_Trunk_mat_1", VegDirectories.YggaShoot},
            {"custom_Shoot_Trunk_mat_2", VegDirectories.YggaShoot},
            {"custom_Shoot_Trunk_mat_3", VegDirectories.YggaShoot},
        };

        foreach (KeyValuePair<string, VegDirectories> kvp in BarkReplacementMap)
        {
            Texture? texture = GetCustomTexture(kvp.Value, kvp.Key, true);
            if (!texture) continue;
            SetBarkMaterial(kvp.Key, texture);
        }
        
        foreach (KeyValuePair<string, VegDirectories> kvp in CustomBarkReplacementMap)
        {
            string normalizedName = kvp.Key.Replace("custom_", "")
                .Replace("_0", "").Replace("_1", "").Replace("_2", "").Replace("_3", "");
            Texture? texture = GetCustomTexture(kvp.Value, normalizedName, true);
            if (!texture) continue;
            SetBarkMaterial(kvp.Key, texture, true);
        }
    }
    private static void SetBarkMaterial(string materialName, Texture? tex, bool isCustom = false)
    {
        if (isCustom)
        {
            if (!CustomMaterials.TryGetValue(materialName, out Material customMaterial)) return;
            string[] customProperties = customMaterial.GetTexturePropertyNames();
            if (!Utils.FindTexturePropName(customProperties, "main", out string customMainProp)) return;
            customMaterial.SetTexture(customMainProp, tex);
            return;
        }
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        string[] properties = material.GetTexturePropertyNames();
        if (!Utils.FindTexturePropName(properties, "main", out string mainProp)) return;
        material.SetTexture(mainProp, tex);
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
            { "HildirsLox", CreatureDirectories.Lox },
            { "lox_calf", CreatureDirectories.LoxCalf },
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
            { "gjall_mat", CreatureDirectories.Gjall },
            { "Skeleton", CreatureDirectories.Skeleton }

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
            // { "beech_leaf", VegDirectories.Beech },
            // { "beech_leaf_sapling", VegDirectories.BeechSmall },
            // { "beech_leaf_small", VegDirectories.BeechSmall },
            // { "beech_particle", VegDirectories.Beech },
            // { "birch_leaf" , VegDirectories.Birch },
            // { "birch_leaf_aut", VegDirectories.Birch },
            { "birch_particle", VegDirectories.Birch },
            { "birch_seeds_leaf", VegDirectories.Birch },
            // { "oak_leaf", VegDirectories.Oak },
            { "oak_particle", VegDirectories.Oak },
            { "Pine_tree_small", VegDirectories.FirSmall },
            { "Pine_tree", VegDirectories.Fir },
            { "Pine_tree_small_dead", VegDirectories.FirDead },
            { "branch_dead_particle", VegDirectories.FirDeadParticles },
            { "Pine_tree_xmas", VegDirectories.FirSmall },
            { "branch_particle", VegDirectories.FirParticles },
            { "PineTree_01", VegDirectories.Pine },
            { "PineCone", VegDirectories.PineParticles },
            { "yggdrasil_branch_leafs", VegDirectories.YggdrasilSkyTree },
            // { "Shoot_Leaf_mat", VegDirectories.YggaShoot },
            // { "shoot_leaf_particle", VegDirectories.YggaShoot },
            { "swamptree1_branch", VegDirectories.SwampTrees },
            { "swamptree2_branch", VegDirectories.SwampGrass },
            // { "Bush01", VegDirectories.Bushes },
            // { "Bush01_blueberry", VegDirectories.Bushes },
            // { "Bush01_heath", VegDirectories.Bushes },
            // { "Bush01_raspberry", VegDirectories.Bushes },
            // { "Bush02_en" , VegDirectories.PlainsBush },
            { "Cloudberrybush", VegDirectories.CloudberryBush },
            { "Vines_Mat", VegDirectories.Vines },
            { "VinesBranch_mat", VegDirectories.Vines },
            // { "shrub", VegDirectories.Shrub },
            // { "shrub_heath", VegDirectories.Shrub },
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
            {"beech_bark", VegDirectories.Beech},
            {"oak_bark", VegDirectories.Oak},
            {"birch_bark",VegDirectories.Birch},
            {"beech_bark_small",VegDirectories.BeechSmall},
            {"birch_bark_quarter", VegDirectories.Birch},
            {"olive_treebark2_darkland",VegDirectories.SwampTrees},
            {"swamptree1_bark",VegDirectories.SwampTrees},
            {"swamptree2_bark",VegDirectories.SwampTrees},
            {"oak_bark_quarter",VegDirectories.Oak}
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
            
            // { "custom_Vines_Mat_0", VegDirectories.Vines },
            // { "custom_Vines_Mat_1", VegDirectories.Vines },
            // { "custom_Vines_Mat_2", VegDirectories.Vines },
            // { "custom_Vines_Mat_3", VegDirectories.Vines },
            
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
    private static Texture? GetCustomTexture(PickableDirectories directory, string originalMaterialName)
    {
        Texture? customTexture = Utils.GetCustomTexture(directory, _Season.Value.ToString());
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
    private static Texture? GetCustomTexture(ArmorDirectories directory, string originalMaterialName, bool isLegs = false, bool isCape = false, bool isHelmet = false)
    {
        Texture? customTexture = isHelmet ? Utils.GetCustomTexture(directory, _Season.Value +  "_helmet") : Utils.GetCustomTexture(directory, _Season.Value + (isCape ? "_cape" : isLegs ? "_legs" : "_chest"));
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
}