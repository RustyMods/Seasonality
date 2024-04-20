using System.Collections.Generic;
using HarmonyLib;
using Seasonality.Textures;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class MaterialReplacer
{
    public static readonly Dictionary<string, Texture> CachedTextures = new();
    public static readonly Dictionary<string, Material> CachedMaterials = new();
    public static readonly Dictionary<string, Material> CustomMaterials = new();
    private static readonly int ChestTex = Shader.PropertyToID("_ChestTex");
    private static readonly int LegsTex = Shader.PropertyToID("_LegsTex");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");
    private static readonly int MossColorProp = Shader.PropertyToID("_MossColor");
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
            CachedMaterials[normalizedName] = item;
        }
    }
    private static void SetArmorColors(Season season)
    {
        if (CachedMaterials.TryGetValue("CapeDeerHide", out Material capeDeerMat))
        {
            capeDeerMat.SetColor(ColorProp,
                GetCustomTexture(season, Directories.ArmorDirectories.Leather, "CapeDeerHide", isCape: true)
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(182, 125, 102, 255));
        }

        if (CachedMaterials.TryGetValue("CapeTrollHide", out Material capeTrollMat))
        {
            capeTrollMat.SetColor(ColorProp, 
                GetCustomTexture(season, Directories.ArmorDirectories.Troll, "CapeTrollHide", isCape: true) 
                    ? new Color32(255, 255, 255, 255) 
                    : new Color32(102, 149, 182, 255));
        }
        
        if (CachedMaterials.TryGetValue("helmet_trollleather", out Material helmTrollMat))
        {
            helmTrollMat.SetColor(ColorProp, 
                GetCustomTexture(season, Directories.ArmorDirectories.Troll, "helmet_trollleather", isHelmet: true) 
                    ? new Color32(255, 255, 255, 255) 
                    : new Color32(88, 123, 151, 255));
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
    private static void SetMossTexture(string materialName, Texture originalTex, Season season)
    {
        if (!CachedTextures.TryGetValue("Pillar_snow_mat_moss", out Texture SnowMoss)) return;
        if (!CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) return;

        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        
        switch (season)
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
    private static void SetMainTexture(string materialName, Texture? tex)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        material.SetTexture(MainTex, tex);
    }
    private static void SetMossColor(string materialName, Color32 color)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        material.SetColor(MossColorProp, color);
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
        if (!normal) return;
        material.SetTexture(BumpMap, normal);
    }
    private static void SetCustomMainTexture(string materialName, Texture? tex, int index = 0)
    {
        if (!CustomMaterials.TryGetValue(materialName, out Material material)) return;

        material.SetTexture(MainTex, tex);
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
    public static void ModifyCachedMaterials(Season season)
    {
        ModifyMossMaterials(season);
        if(_ReplaceCreatureTextures.Value is Toggle.On) ModifyCreatures(season);
        ModifyVegetation(season); 
        ModifyCustomMaterials(season);
        ModifyPieceMaterials(season);
        ModifyBarkMaterials(season);
        ModifyPickableMaterials(season);
        // ModifyNormals();
        if (_ReplaceArmorTextures.Value is Toggle.On)
        {
            SetArmorColors(season);
            ModifyArmorMaterials(season);
        }
        ModifyMistLandRocks(season);
    }

    private static bool MistLandRocksTurnedWhite = false;
    private static void ModifyMistLandRocks(Season season)
    {
        if (season is Season.Winter)
        {
            SetMistLandRocksWhite();
            MistLandRocksTurnedWhite = true;
        }
        else
        {
            if (!MistLandRocksTurnedWhite) return;
            SetMistLandRocksDefault();
            MistLandRocksTurnedWhite = false;
        }
    }
    private static void SetMistLandRocksDefault()
    {
        Color32 MossColor = new Color32(202, 255, 121, 255);
        
        List<string> MaterialToReplace = new()
        {
            "rock_mistlands",
            "mistlands_cliff",
            "mistlands_cliff_internal",
            "mistlands_cliff_dungeon"
        };

        foreach (string material in MaterialToReplace)
        {
            SetMossColor(material, MossColor);
        }
    }
    private static void SetMistLandRocksWhite()
    {
        List<string> MaterialToReplace = new()
        {
            "rock_mistlands",
            "mistlands_cliff",
            "mistlands_cliff_internal",
            "mistlands_cliff_dungeon"
        };

        foreach (string material in MaterialToReplace)
        {
            SetMossColor(material, Color.white);
        }
    }
    private static void ModifyArmorMaterials(Season season)
    {
        Dictionary<string, Directories.ArmorDirectories> ChestReplacementMap = new()
        {
            {"RagsChest",Directories.ArmorDirectories.Rags},
            {"LeatherChest",Directories.ArmorDirectories.Leather},
            {"TrollLeatherChest",Directories.ArmorDirectories.Troll},
        };
        Dictionary<string, Directories.ArmorDirectories> LegsReplacementMap = new()
        {
            {"RagsLegs",Directories.ArmorDirectories.Rags},
            {"LeatherPants",Directories.ArmorDirectories.Leather},
            {"TrollLeatherPants",Directories.ArmorDirectories.Troll}
        };
        Dictionary<string, Directories.ArmorDirectories> CapeReplacementMap = new()
        {
            {"CapeDeerHide", Directories.ArmorDirectories.Leather},
            {"CapeTrollHide",Directories.ArmorDirectories.Troll},
            {"WolfCape",Directories.ArmorDirectories.Wolf},
            {"LoxCape_Mat",Directories.ArmorDirectories.Padded},
            {"feathercape_mat",Directories.ArmorDirectories.Mage}
        };
        Dictionary<string, Directories.ArmorDirectories> HelmetReplacementMap = new()
        {
            {"helmet_leather_mat", Directories.ArmorDirectories.Leather},
            {"helmet_trollleather",Directories.ArmorDirectories.Troll},
            {"helmet_bronze_mat",Directories.ArmorDirectories.Bronze},
            {"helmet_iron_mat",Directories.ArmorDirectories.Iron},
        };
        Dictionary<string, Directories.ArmorDirectories> ArmorReplacementMap = new()
        {
            {"BronzeArmorMesh_Mat",Directories.ArmorDirectories.Bronze},
            {"IronArmorChest_mat",Directories.ArmorDirectories.Iron},
            {"SilverArmourChest_mat", Directories.ArmorDirectories.Wolf},
            {"DragonVisor_Mat",Directories.ArmorDirectories.Wolf},
            {"Padded_mat",Directories.ArmorDirectories.Padded},
            {"carapacearmor_mat",Directories.ArmorDirectories.Carapace},
            {"MageArmor_mat",Directories.ArmorDirectories.Mage}
        };
        Dictionary<string, Directories.ArmorDirectories> ArmorLegsReplacementMap = new()
        {
            {"IronArmorLegs_mat",Directories.ArmorDirectories.Iron}
        };
        foreach (KeyValuePair<string, Directories.ArmorDirectories> kvp in ChestReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key + "_chest", out Texture tex)) continue;
                SetChestTexture(kvp.Key, tex);
                continue;
            }
            SetChestTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, Directories.ArmorDirectories> kvp in LegsReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key,  true);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key + "_legs", out Texture tex)) continue;
                SetLegsTexture(kvp.Key, tex);
                continue;
            }
            SetLegsTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, Directories.ArmorDirectories> kvp in CapeReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key, isCape: true);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key + "_cape", out Texture tex)) continue;
                SetMainTexture(kvp.Key, tex);
                continue;
            }
            SetMainTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, Directories.ArmorDirectories> kvp in HelmetReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key, isHelmet: true);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key, out Texture tex)) continue;
                SetMainTexture(kvp.Key, tex);
                continue;
            }
            SetMainTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, Directories.ArmorDirectories> kvp in ArmorReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key, out Texture tex)) continue;
                SetMainTexture(kvp.Key, tex);
                continue;
            }
            SetMainTexture(kvp.Key, texture);
        }
        foreach (KeyValuePair<string, Directories.ArmorDirectories> kvp in ArmorLegsReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key, isLegs: true);
            if (!texture)
            {
                if (!CachedTextures.TryGetValue(kvp.Key, out Texture tex)) continue;
                SetMainTexture(kvp.Key, tex);
                continue;
            }
            SetMainTexture(kvp.Key, texture);
        }
    }
    private static void ModifyPieceMaterials(Season season)
    {
        Dictionary<string, Directories.PieceDirectories> PiecesReplacementMap = new()
        {
            { "straw_roof", Directories.PieceDirectories.Straw },
            { "straw_roof_alpha", Directories.PieceDirectories.Straw },
            { "RoofShingles", Directories.PieceDirectories.DarkWood },
            { "GoblinVillage_Cloth", Directories.PieceDirectories.GoblinVillage },
            { "GoblinVillage", Directories.PieceDirectories.GoblinVillage },
        };
        Dictionary<string, Directories.PieceDirectories> PiecesWornReplacementMap = new()
        {
            { "straw_roof_worn", Directories.PieceDirectories.Straw },
            { "straw_roof_worn_alpha", Directories.PieceDirectories.Straw },
            { "RoofShingles_worn" , Directories.PieceDirectories.DarkWood },
            { "GoblinVillage", Directories.PieceDirectories.GoblinVillage }
        };
        Dictionary<string, Directories.PieceDirectories> PieceCornerReplacementMap = new()
        {
            { "straw_roof_corner_alpha", Directories.PieceDirectories.Straw },
        };
        Dictionary<string, Directories.PieceDirectories> PieceCornerWornReplacementMap = new()
        {
            { "straw_roof_corner_worn_alpha", Directories.PieceDirectories.Straw }
        };
        foreach (KeyValuePair<string, Directories.PieceDirectories> kvp in PiecesReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }
        foreach (KeyValuePair<string, Directories.PieceDirectories> kvp in PiecesWornReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key, true);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }
        foreach (KeyValuePair<string, Directories.PieceDirectories> kvp in PieceCornerReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key, isCorner: true);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }
        foreach (KeyValuePair<string, Directories.PieceDirectories> kvp in PieceCornerWornReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key, true, true);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }
    }
    private static void ModifyPickableMaterials(Season season)
    {
        Dictionary<string, Directories.PickableDirectories> PickableReplacementMap = new()
        {
            {"Boletus_edulis", Directories.PickableDirectories.Mushroom},
            {"Boletus_Yellow", Directories.PickableDirectories.MushroomYellow},
            {"Boletus_blue", Directories.PickableDirectories.MushroomBlue},
            {"MistlandsMushrooms_balls",Directories.PickableDirectories.JotunPuff},
            {"MistlandsMushrooms_magecap", Directories.PickableDirectories.Magecap},
            {"raspberry", Directories.PickableDirectories.Raspberry},
            {"blueberry",Directories.PickableDirectories.Blueberry},
            {"Dandelion", Directories.PickableDirectories.Dandelion},
            {"barley_ripe",Directories.PickableDirectories.Barley},
            {"flax_ripe", Directories.PickableDirectories.Flax},
            {"flax_item", Directories.PickableDirectories.Flax},
            {"carrot",Directories.PickableDirectories.Carrot},
            {"turnip_0",Directories.PickableDirectories.Turnip},
            {"onion_ripe",Directories.PickableDirectories.Onion},
            {"carrot_flower",Directories.PickableDirectories.CarrotSeed},
            {"turnip_flower",Directories.PickableDirectories.TurnipSeed},
            {"onion_seedling_ripe",Directories.PickableDirectories.OnionSeed},
            {"branch",Directories.PickableDirectories.Branches},
            {"flint",Directories.PickableDirectories.Flint},
            {"stone",Directories.PickableDirectories.Rock},
            {"fi_village_catacombs",Directories.PickableDirectories.BoneRemains},
            {"surtlingcore",Directories.PickableDirectories.SurtlingCore},
            {"BlackCore_mat",Directories.PickableDirectories.BlackCore},
            {"swampplant2",Directories.PickableDirectories.Thistle}
        };
        foreach (KeyValuePair<string, Directories.PickableDirectories> kvp in PickableReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key);
            if (!texture) continue;
            
            SetMainTexture(kvp.Key, texture);
        }
    }
    private static void ModifyBarkMaterials(Season season)
    {
        Dictionary<string, Directories.VegDirectories> BarkReplacementMap = new()
        {
            {"olive_treebark2_darkland", Directories.VegDirectories.SwampTrees},
            {"swamptree1_bark", Directories.VegDirectories.SwampTrees},
            {"swamptree2_bark", Directories.VegDirectories.SwampTrees},
        };

        Dictionary<string, Directories.VegDirectories> CustomBarkReplacementMap = new()
        {
            {"custom_beech_bark_0", Directories.VegDirectories.Beech},
            {"custom_beech_bark_1", Directories.VegDirectories.Beech},
            {"custom_beech_bark_2", Directories.VegDirectories.Beech},
            {"custom_beech_bark_3", Directories.VegDirectories.Beech},
            
            {"custom_beech_bark_small_0", Directories.VegDirectories.BeechSmall},
            {"custom_beech_bark_small_1", Directories.VegDirectories.BeechSmall},
            {"custom_beech_bark_small_2", Directories.VegDirectories.BeechSmall},
            {"custom_beech_bark_small_3", Directories.VegDirectories.BeechSmall},
            
            {"custom_birch_bark_0", Directories.VegDirectories.Birch},
            {"custom_birch_bark_1", Directories.VegDirectories.Birch},
            {"custom_birch_bark_2", Directories.VegDirectories.Birch},
            {"custom_birch_bark_3", Directories.VegDirectories.Birch},
            
            {"custom_oak_bark_0", Directories.VegDirectories.Oak},
            {"custom_oak_bark_1", Directories.VegDirectories.Oak},
            {"custom_oak_bark_2", Directories.VegDirectories.Oak},
            {"custom_oak_bark_3", Directories.VegDirectories.Oak},
            
            {"custom_Shoot_Trunk_mat_0", Directories.VegDirectories.YggaShoot},
            {"custom_Shoot_Trunk_mat_1", Directories.VegDirectories.YggaShoot},
            {"custom_Shoot_Trunk_mat_2", Directories.VegDirectories.YggaShoot},
            {"custom_Shoot_Trunk_mat_3", Directories.VegDirectories.YggaShoot},
        };

        foreach (KeyValuePair<string, Directories.VegDirectories> kvp in BarkReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key, true);
            if (!texture) continue;
            SetBarkMaterial(kvp.Key, texture);
        }
        
        foreach (KeyValuePair<string, Directories.VegDirectories> kvp in CustomBarkReplacementMap)
        {
            string normalizedName = kvp.Key
                .Replace("custom_", "")
                .Replace("_0", "")
                .Replace("_1", "")
                .Replace("_2", "")
                .Replace("_3", "");
            Texture? texture = GetCustomTexture(season, kvp.Value, normalizedName, true);
            if (!texture) continue;
            SetBarkMaterial(kvp.Key, texture, true);
        }
    }
    private static void SetBarkMaterial(string materialName, Texture? tex, bool isCustom = false)
    {
        if (isCustom)
        {
            if (!CustomMaterials.TryGetValue(materialName, out Material customMaterial)) return;
            customMaterial.SetTexture(MainTex, tex);
            return;
        }
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        material.SetTexture(MainTex, tex);
    }
    private static void ModifyMossMaterials(Season season)
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
        foreach(KeyValuePair<string, Texture> kvp in MossReplacementMap) SetMossTexture(kvp.Key, kvp.Value, season);
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
    private static void ModifyCreatures(Season season)
    {
        Dictionary<string, Directories.CreatureDirectories> CreatureReplacementMap = new()
        {
            { "lox", Directories.CreatureDirectories.Lox },
            { "HildirsLox", Directories.CreatureDirectories.Lox },
            { "lox_calf", Directories.CreatureDirectories.LoxCalf },
            { "troll", Directories.CreatureDirectories.Troll },
            { "Hare_mat", Directories.CreatureDirectories.Hare },
            { "Feasting_mat", Directories.CreatureDirectories.Tick },
            { "SeaSerpent_mat", Directories.CreatureDirectories.Serpent },
            { "swampfish", Directories.CreatureDirectories.Leech },
            { "Deathsquito_mat", Directories.CreatureDirectories.Deathsquito },
            { "seagal", Directories.CreatureDirectories.Gull },
            { "neck", Directories.CreatureDirectories.Neck },
            { "wraith", Directories.CreatureDirectories.Wraith },
            { "blob", Directories.CreatureDirectories.Blob },
            { "blob_elite", Directories.CreatureDirectories.Oozer },
            { "gjall_mat", Directories.CreatureDirectories.Gjall },
            { "Skeleton", Directories.CreatureDirectories.Skeleton },
            { "WolfSkinGrey", Directories.CreatureDirectories.Wolf },
            { "WolfSkinGreycub", Directories.CreatureDirectories.WolfCub },
            { "Deer2", Directories.CreatureDirectories.Deer },
            { "BoarSkinValheimpiggy", Directories.CreatureDirectories.Piggy },
            { "BoarSkinValheim", Directories.CreatureDirectories.Boar },
            { "GoblinShaman_mat", Directories.CreatureDirectories.GoblinShaman },
            { "GoblinBrute_mat",Directories.CreatureDirectories.GoblinBrute },
            { "goblin",Directories.CreatureDirectories.Goblin },
            { "seeker_Brute_mat",Directories.CreatureDirectories.SeekerSoldier },
            { "babyseeker",Directories.CreatureDirectories.SeekerBrood },
            { "seeker",Directories.CreatureDirectories.Seeker },
            { "greydwarf", Directories.CreatureDirectories.Greydwarf },
            { "greydwarf_shaman", Directories.CreatureDirectories.GreydwarfShaman },
            { "greydwarf_elite", Directories.CreatureDirectories.Greydwarf }
        };
        foreach (KeyValuePair<string, Directories.CreatureDirectories> kvp in CreatureReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key);
            if (!texture) continue;
            SetMainTexture(kvp.Key, texture);
        }
    }
    private static void ModifyVegetation(Season season)
    {
        Dictionary<string, Directories.VegDirectories> VegetationReplacementMap = new()
        {
            { "birch_particle", Directories.VegDirectories.Birch },
            { "birch_seeds_leaf", Directories.VegDirectories.Birch },
            { "oak_particle", Directories.VegDirectories.Oak },
            { "Pine_tree_small", Directories.VegDirectories.FirSmall },
            { "Pine_tree", Directories.VegDirectories.Fir },
            { "Pine_tree_small_dead", Directories.VegDirectories.FirDead },
            { "branch_dead_particle", Directories.VegDirectories.FirDeadParticles },
            { "Pine_tree_xmas", Directories.VegDirectories.FirSmall },
            { "branch_particle", Directories.VegDirectories.FirParticles },
            { "PineTree_01", Directories.VegDirectories.Pine },
            { "PineCone", Directories.VegDirectories.PineParticles },
            { "yggdrasil_branch_leafs", Directories.VegDirectories.YggdrasilSkyTree },
            { "swamptree1_branch", Directories.VegDirectories.SwampTrees },
            { "swamptree2_branch", Directories.VegDirectories.SwampTrees },
            { "Cloudberrybush", Directories.VegDirectories.CloudberryBush },
            { "shrub2_leafparticle", Directories.VegDirectories.ShrubParticles },
            { "shrub2_leafparticle_heath", Directories.VegDirectories.ShrubParticles },
        };
        Dictionary<string, Directories.VegDirectories> VinesMap = new()
        {
            { "Vines_Mat", Directories.VegDirectories.Vines },
            { "VinesBranch_mat", Directories.VegDirectories.Vines },
        };
        foreach (KeyValuePair<string, Directories.VegDirectories> kvp in VegetationReplacementMap)
        {
            Texture? texture = GetCustomTexture(season, kvp.Value, kvp.Key);
            if (!texture) {continue;}
            SetMainTexture(kvp.Key, texture);
        }

        foreach (KeyValuePair<string, Directories.VegDirectories> kvp in VinesMap)
        {
            bool flag = GetCustomTexture(kvp.Value, kvp.Key, out Texture? texture);
            if (!flag)
            {
                SetVineColorDefault(kvp.Key);
                SetMainTexture(kvp.Key, texture);
            }
            else
            {
                ModifyVinesColor(kvp.Key);
                SetMainTexture(kvp.Key, texture);
            }
        }
    }
    private static void ModifyVinesColor(string materialName)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        material.SetColor(ColorProp, new Color32(255, 255, 255, 255));
    }
    private static void SetVineColorDefault(string materialName)
    {
        if (!CachedMaterials.TryGetValue(materialName, out Material material)) return;
        material.SetColor(ColorProp, new Color32(186, 255, 134, 255));
    }
    private static void ModifyNormals()
    {
        Dictionary<string, Directories.VegDirectories> NormalReplacementMap = new()
        {
            {"beech_bark", Directories.VegDirectories.Beech},
            {"oak_bark", Directories.VegDirectories.Oak},
            {"birch_bark",Directories.VegDirectories.Birch},
            {"beech_bark_small",Directories.VegDirectories.BeechSmall},
            {"birch_bark_quarter", Directories.VegDirectories.Birch},
            {"olive_treebark2_darkland",Directories.VegDirectories.SwampTrees},
            {"swamptree1_bark",Directories.VegDirectories.SwampTrees},
            {"swamptree2_bark",Directories.VegDirectories.SwampTrees},
            {"oak_bark_quarter",Directories.VegDirectories.Oak}
        };
        foreach (KeyValuePair<string, Directories.VegDirectories> kvp in NormalReplacementMap)
        {
            Texture? normal = GetCustomNormals(kvp.Value, kvp.Key);
            if (!normal) continue;
            SetNormalTexture(kvp.Key, normal);
        }
    }
    private static void ModifyCustomMaterials(Season season)
    {
        Dictionary<string, Directories.VegDirectories> CustomReplacementMap = new()
        {
            { "custom_beech_leaf_small_0", Directories.VegDirectories.BeechSmall },
            { "custom_beech_leaf_small_1", Directories.VegDirectories.BeechSmall },
            { "custom_beech_leaf_small_2", Directories.VegDirectories.BeechSmall },
            { "custom_beech_leaf_small_3", Directories.VegDirectories.BeechSmall },
            
            { "custom_beech_leaf_0", Directories.VegDirectories.Beech },
            { "custom_beech_leaf_1", Directories.VegDirectories.Beech },
            { "custom_beech_leaf_2", Directories.VegDirectories.Beech },
            { "custom_beech_leaf_3", Directories.VegDirectories.Beech },
            
            { "custom_birch_leaf_0", Directories.VegDirectories.Birch },
            { "custom_birch_leaf_1", Directories.VegDirectories.Birch },
            { "custom_birch_leaf_2", Directories.VegDirectories.Birch },
            { "custom_birch_leaf_3", Directories.VegDirectories.Birch },
            
            { "custom_birch_leaf_aut_0", Directories.VegDirectories.Birch },
            { "custom_birch_leaf_aut_1", Directories.VegDirectories.Birch },
            { "custom_birch_leaf_aut_2", Directories.VegDirectories.Birch },
            { "custom_birch_leaf_aut_3", Directories.VegDirectories.Birch },
            
            { "custom_Bush01_blueberry_0", Directories.VegDirectories.Bushes },
            { "custom_Bush01_blueberry_1", Directories.VegDirectories.Bushes },
            { "custom_Bush01_blueberry_2", Directories.VegDirectories.Bushes },
            { "custom_Bush01_blueberry_3", Directories.VegDirectories.Bushes },
            
            { "custom_Bush01_0", Directories.VegDirectories.Bushes },
            { "custom_Bush01_1", Directories.VegDirectories.Bushes },
            { "custom_Bush01_2", Directories.VegDirectories.Bushes },
            { "custom_Bush01_3", Directories.VegDirectories.Bushes },
            
            { "custom_Bush01_heath_0", Directories.VegDirectories.Bushes },
            { "custom_Bush01_heath_1", Directories.VegDirectories.Bushes },
            { "custom_Bush01_heath_2", Directories.VegDirectories.Bushes },
            { "custom_Bush01_heath_3", Directories.VegDirectories.Bushes },

            { "custom_Bush02_en_0", Directories.VegDirectories.PlainsBush },
            { "custom_Bush02_en_1", Directories.VegDirectories.PlainsBush },
            { "custom_Bush02_en_2", Directories.VegDirectories.PlainsBush },
            { "custom_Bush02_en_3", Directories.VegDirectories.PlainsBush },
            
            { "custom_oak_leaf_0", Directories.VegDirectories.Oak },
            { "custom_oak_leaf_1", Directories.VegDirectories.Oak },
            { "custom_oak_leaf_2", Directories.VegDirectories.Oak },
            { "custom_oak_leaf_3", Directories.VegDirectories.Oak },
            
            { "custom_Bush01_raspberry_0", Directories.VegDirectories.Bushes },
            { "custom_Bush01_raspberry_1", Directories.VegDirectories.Bushes },
            { "custom_Bush01_raspberry_2", Directories.VegDirectories.Bushes },
            { "custom_Bush01_raspberry_3", Directories.VegDirectories.Bushes },
            
            { "custom_shrub_0", Directories.VegDirectories.Shrub },
            { "custom_shrub_1", Directories.VegDirectories.Shrub },
            { "custom_shrub_2", Directories.VegDirectories.Shrub },
            { "custom_shrub_3", Directories.VegDirectories.Shrub },
            
            { "custom_shrub_heath_0", Directories.VegDirectories.Shrub },
            { "custom_shrub_heath_1", Directories.VegDirectories.Shrub },
            { "custom_shrub_heath_2", Directories.VegDirectories.Shrub },
            { "custom_shrub_heath_3", Directories.VegDirectories.Shrub },
            
            // { "custom_Vines_Mat_0", VegDirectories.Vines },
            // { "custom_Vines_Mat_1", VegDirectories.Vines },
            // { "custom_Vines_Mat_2", VegDirectories.Vines },
            // { "custom_Vines_Mat_3", VegDirectories.Vines },
            
            { "custom_Shoot_Leaf_mat_0", Directories.VegDirectories.YggaShoot },
            { "custom_Shoot_Leaf_mat_1", Directories.VegDirectories.YggaShoot },
            { "custom_Shoot_Leaf_mat_2", Directories.VegDirectories.YggaShoot },
            { "custom_Shoot_Leaf_mat_3", Directories.VegDirectories.YggaShoot },
        };

        foreach (KeyValuePair<string, Directories.VegDirectories> kvp in CustomReplacementMap)
        {
            string normalizedName = kvp.Key
                .Replace("custom_", "")
                .Replace("_0", "")
                .Replace("_1", "")
                .Replace("_2", "")
                .Replace("_3", "");
            int index = int.Parse(kvp.Key.Substring(kvp.Key.Length - 1));
            Texture? texture = GetCustomTexture(season, kvp.Value, normalizedName);
            SetCustomMainTexture(kvp.Key, texture, index);
        }
        
        ModifyCustomMossMaterials();
    }
    private static Texture? GetCustomTexture(Season season, Directories.CreatureDirectories directory, string originalMaterialName)
    {
        Texture? customTexture = SeasonUtility.Utils.GetCustomTexture(directory, season);
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
    private static Texture? GetCustomTexture(Season season, Directories.PieceDirectories directory, string originalMaterialName, bool isWorn = false, bool isCorner = false)
    {
        string filePath = season.ToString();
        string wornFilePath = season + "_worn";
        string cornerFilePath = season + "_corner";
        string cornerWornFilePath = season + "_corner_worn";

        string path = isWorn ? wornFilePath : filePath;
        if (isCorner) path = isWorn ? cornerWornFilePath : cornerFilePath;
        
        Texture? customTexture = SeasonUtility.Utils.GetCustomTexture(directory, path);
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
    private static Texture? GetCustomTexture(Season season, Directories.VegDirectories directory, string originalMaterialName, bool isBark = false)
    {
        Texture? customTexture = SeasonUtility.Utils.GetCustomTexture(directory,  isBark ? season + "_bark" : season.ToString());
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }

    private static bool GetCustomTexture(Directories.VegDirectories directory, string originalMaterialName, out Texture? texture, bool isBark = false)
    {
        texture = null;
        Texture? customTexture = SeasonUtility.Utils.GetCustomTexture(directory,  isBark ? _Season.Value + "_bark" : _Season.Value.ToString());
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);

        texture = customTexture ? customTexture : originalTexture ? originalTexture : null;
        
        return customTexture;
    }
    private static Texture? GetCustomNormals(Directories.VegDirectories directory, string originalMaterialName)
    {
        Texture? customTexture = SeasonUtility.Utils.GetCustomTexture(directory,  _Season.Value + "_normal");
        CachedTextures.TryGetValue(originalMaterialName + "_normal", out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
    private static Texture? GetCustomTexture(Season season, Directories.PickableDirectories directory, string originalMaterialName)
    {
        Texture? customTexture = SeasonUtility.Utils.GetCustomTexture(directory, season.ToString());
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
    private static Texture? GetCustomTexture(Season season, Directories.ArmorDirectories directory, string originalMaterialName, bool isLegs = false, bool isCape = false, bool isHelmet = false)
    {
        Texture? customTexture = isHelmet ? SeasonUtility.Utils.GetCustomTexture(directory, season +  "_helmet") : SeasonUtility.Utils.GetCustomTexture(directory, season + (isCape ? "_cape" : isLegs ? "_legs" : "_chest"));
        CachedTextures.TryGetValue(originalMaterialName, out Texture originalTexture);
        return customTexture ? customTexture : originalTexture ? originalTexture : null;
    }
}