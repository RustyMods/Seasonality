using System.Collections.Generic;
using System.Text.RegularExpressions;
using Seasonality.DataTypes;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Textures.Directories;
using static Seasonality.Textures.TextureManager;

namespace Seasonality.Helpers;

public static class Utils
{
    public static VegetationType GetVegetationType(string prefabName)
    {
        string normalizedName = prefabName.Replace("(Clone)", "");
        normalizedName = Regex.Replace(normalizedName, @"\(.*?\)", "");
        normalizedName = normalizedName.Replace(" ", "");
        if (normalizedName.ToLower().Contains("runestone")) return VegetationType.Rock;
        return normalizedName switch
        {
            "Beech" => VegetationType.Beech,
            "Beech1" => VegetationType.Beech,
            "Beech_small1" => VegetationType.BeechSmall,
            "Beech_small2" => VegetationType.BeechSmall,

            "Birch" => VegetationType.Birch,
            "Birch1" => VegetationType.Birch,
            "Birch2" => VegetationType.Birch,
            "Birch1_aut" => VegetationType.Birch,
            "Birch2_aut" => VegetationType.Birch,

            "Oak" => VegetationType.Oak,
            "Oak1" => VegetationType.Oak,

            "FirTree_small_dead" => VegetationType.FirDead,
            "FirTree" => VegetationType.Fir,
            "FirTree_small" => VegetationType.FirSmall,

            "Pinetree_01" => VegetationType.Pine,

            "SwampTree1" => VegetationType.Swamp,
            "SwampTree2" => VegetationType.Swamp,
            "SwampTree2_darkland" => VegetationType.Swamp,
            
            "YggaShoot1" => VegetationType.Yggashoot,
            "YggaShoot2" => VegetationType.Yggashoot,
            "YggaShoot3" => VegetationType.Yggashoot,
            "YggaShoot_small1" => VegetationType.YggashootSmall,
            "YggdrasilTree2_RtD" => VegetationType.Yggashoot,

            "Bush01_heath" => VegetationType.Bush,
            "Bush02_en" => VegetationType.PlainsBush,
            "Bush01" => VegetationType.Bush,
            "shrub_2" => VegetationType.Shrub,
            "shrub_2_heath" => VegetationType.Shrub,

            "stubbe" => VegetationType.Stubbe,
            "FirTree_oldLog" => VegetationType.Log,
            "SwampTree2_log" => VegetationType.Log,

            "RockDolmen_1" => VegetationType.Rock,
            "RockDolmen_2" => VegetationType.Rock,
            "RockDolmen_3" => VegetationType.Rock,
            "Rock_3" => VegetationType.Rock,
            "Rock_4" => VegetationType.Rock,
            "Rock_7" => VegetationType.Rock,
            "rock4_forest" => VegetationType.Rock,
            "rock4_copper" => VegetationType.Rock,
            "rock4_coast" => VegetationType.Rock,
            "StatueEvil" => VegetationType.Rock,
            "rock1_mountain" => VegetationType.Rock,
            "rock2_heath" => VegetationType.RockPlains,
            "rock4_heath" => VegetationType.RockPlains,
            "Rock_4_plains" => VegetationType.RockPlains,
            "HeathRockPillar" => VegetationType.RockPlains,
            "Rock_destructible" => VegetationType.Rock,
            "Rock_3_static" => VegetationType.Rock,
            "RockFinger" => VegetationType.RockPlains,
            "RockFingerBroken" => VegetationType.RockPlains,
            "rockformation1" => VegetationType.Rock,
            "RockThumb" => VegetationType.RockPlains,
            "Rocks2" => VegetationType.Rock,
            "highstone" => VegetationType.RockPlains,
            "widestone" => VegetationType.RockPlains,
            "StatueSeed" => VegetationType.Rock,
            
            "RaspberryBush" => VegetationType.RaspberryBush,
            "BlueberryBush" => VegetationType.BlueberryBush,
            "CloudberryBush" => VegetationType.CloudberryBush,
            
            "vines" => VegetationType.Vines,
            _ => VegetationType.None,
        };
    }
    public static Texture? GetCustomTexture(VegDirectories type, string key) => !CustomRegisteredTextures.TryGetValue(type, out Dictionary<string, Texture?> map) ? null : map.TryGetValue(key, out Texture? tex) ? tex : null;
    public static Texture? GetCustomTexture(ArmorDirectories type, string key) => !CustomRegisteredArmorTex.TryGetValue(type, out Dictionary<string, Texture?> map) ? null : map.TryGetValue(key, out Texture? tex) ? tex : null;
    public static Texture? GetCustomTexture(PickableDirectories type, string key) => !CustomRegisteredPickableTex.TryGetValue(type, out Dictionary<string, Texture?> map) ? null : map.TryGetValue(key, out Texture? tex) ? tex : null;
    public static Texture? GetCustomTexture(PieceDirectories type, string key) => !CustomRegisteredPieceTextures.TryGetValue(type, out Dictionary<string, Texture?> map) ? null : map.TryGetValue(key, out Texture? tex) ? tex : null;
    public static Texture? GetCustomTexture(CreatureDirectories type, Season key) => !CustomRegisteredCreatureTex.TryGetValue(type, out Dictionary<string, Texture?> map) ? null : map.TryGetValue(key.ToString(), out Texture? tex) ? tex : null;
    private static bool CustomTextureExist(VegDirectories type, string key) => CustomRegisteredTextures.TryGetValue(type, out Dictionary<string, Texture?> map) && map.ContainsKey(key);
    public static VegDirectories VegToDirectory(GrassTypes type)
    {
        return type switch
        {
            GrassTypes.GreenGrass => VegDirectories.MeadowGrass,
            GrassTypes.GreenGrassShort => VegDirectories.MeadowGrassShort,
            GrassTypes.GroundCover => VegDirectories.BlackForestGrass,
            GrassTypes.GroundCoverBrown => VegDirectories.BlackForestGrassAlt,
            GrassTypes.SwampGrass => VegDirectories.SwampGrass,
            GrassTypes.HeathGrass => VegDirectories.PlainsGrass,
            GrassTypes.HeathFlowers => VegDirectories.PlainsFlowers,
            GrassTypes.Ormbunke => VegDirectories.Ormbunke,
            GrassTypes.OrmBunkeSwamp => VegDirectories.Ormbunke,
            GrassTypes.Vass => VegDirectories.Vass,
            GrassTypes.WaterLilies => VegDirectories.WaterLilies,
            GrassTypes.RockPlant => VegDirectories.RockPlant,
            GrassTypes.ClutterShrubs => VegDirectories.Clutter,
            GrassTypes.MistlandGrassShort => VegDirectories.MistlandsGrass,
            GrassTypes.GrassHeathGreen => VegDirectories.PlainsGrass,
            _ => VegDirectories.None
        };
    }
    public static VegDirectories VegToDirectory(VegetationType type)
    {
        return type switch
        {
            VegetationType.Beech => VegDirectories.Beech,
            VegetationType.BeechSmall => VegDirectories.BeechSmall,
            VegetationType.Birch => VegDirectories.Birch,
            VegetationType.Bush => VegDirectories.Bushes,
            VegetationType.Oak => VegDirectories.Oak,
            VegetationType.Pine => VegDirectories.Pine,
            VegetationType.Fir => VegDirectories.Fir,
            VegetationType.Yggashoot => VegDirectories.YggaShoot,
            VegetationType.PlainsBush => VegDirectories.PlainsBush,
            VegetationType.Shrub => VegDirectories.Shrub,
            VegetationType.Rock => VegDirectories.Rock,
            VegetationType.Swamp => VegDirectories.SwampTrees,
            VegetationType.CloudberryBush => VegDirectories.CloudberryBush,
            VegetationType.Vines => VegDirectories.Vines,
            _ => VegDirectories.None,
        };
    }
    public static bool ApplyBasedOnAvailable(VegDirectories directory, Season season, Material material, string prop)
    {
        if (directory is VegDirectories.None) return false;
        if (!CustomTextureExist(directory, season.ToString())) return false;
        
        Texture? tex = GetCustomTexture(directory, season.ToString());
        if (!tex) return false;

        material.SetTexture(prop, tex);
        material.color = Color.white;
        return true;

    }
    public static bool FindTexturePropName(string[] props, string query, out string result)
    {
        result = "";
        foreach (string prop in props)
        {
            if (!prop.ToLower().Contains(query)) continue;
            result = prop;
            return true;
        }
        return false;
    }
}