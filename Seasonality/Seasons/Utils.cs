using System;
using System.Collections.Generic;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;
using static Seasonality.Seasons.Vegetation;
using Random = System.Random;

namespace Seasonality.Seasons;

public static class Utils
{
    public static VegetationType GetVegetationType(string prefabName)
    {
        string normalizedName = prefabName.Replace("(Clone)", "");
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

            "FirTree_small_dead" => VegetationType.Fir,
            "FirTree" => VegetationType.Fir,
            "FirTree_small" => VegetationType.Fir,

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

    public static GrassTypes GetGrassType(string clutterName)
    {
        return clutterName switch
        {
            "instanced_meadows_grass" => GrassTypes.GreenGrass,
            "instanced_meadows_grass_short" => GrassTypes.GreenGrassShort,
            "instanced_shrub" => GrassTypes.ClutterShrubs,
            "clutter_shrub_large" => GrassTypes.ClutterShrubs,
            "instanced_forest_groundcover_brown" => GrassTypes.GroundCoverBrown,
            "instanced_forest_groundcover" => GrassTypes.GroundCover,
            "instanced_swamp_grass" => GrassTypes.SwampGrass,
            "instanced_heathgrass" => GrassTypes.HeathGrass,
            "grasscross_heath_green" => GrassTypes.GrassHeathGreen,
            "instanced_heathflowers" => GrassTypes.HeathFlowers,
            "instanced_swamp_ormbunke" => GrassTypes.OrmBunkeSwamp,
            "instanced_ormbunke" => GrassTypes.Ormbunke,
            "instanced_vass" => GrassTypes.Vass,
            "instanced_waterlilies" => GrassTypes.WaterLilies,
            "instanced_mistlands_rockplant" => GrassTypes.RockPlant,
            "instanced_small_rock1" => GrassTypes.Rocks,
            "instanced_mistlands_grass_short" => GrassTypes.MistlandGrassShort,
            _ => GrassTypes.None
        };
    }
    
    public static void ApplyRandomly(List<Action> methods)
    {
        if (methods.Count == 0) return;
        Random random = new Random();
        int randomIndex = random.Next(methods.Count);
        methods[randomIndex]();
    }

    public static void SetMossTex(GameObject prefab, Texture? tex)
    {
        if (!tex) return;
        for (int i = 0; i < prefab.transform.childCount; ++i)
        {
            Transform? child = prefab.transform.GetChild(i);
            if (!child) return;
            if (child.name == "Terrain" || child.name.StartsWith("Music")) continue;
                
            if (child.childCount > 0) SetMossTex(child.gameObject, tex);

            child.TryGetComponent(out MeshRenderer meshRenderer);
            if (!meshRenderer) continue;
            Material[]? materials = meshRenderer.materials;
            foreach (Material mat in materials)
            {
                string[] properties = mat.GetTexturePropertyNames();
                foreach (string prop in properties)
                {
                    if (!prop.ToLower().Contains("moss")) continue;
                    mat.SetTexture(prop, tex);
                }
            }
        }
    }

    public static Texture? GetCustomTexture(VegDirectories type, Season key)
    {
        return !CustomRegisteredTextures.TryGetValue(type, out Dictionary<Season, Texture?> map) ? null : map.TryGetValue(key, out Texture? tex) ? tex : null;
    }
    public static Texture? GetCustomTexture(CreatureDirectories type, Season key)
    {
        return !CustomRegisteredCreatureTex.TryGetValue(type, out Dictionary<Season, Texture?> map) ? null : map.TryGetValue(key, out Texture? tex) ? tex : null;
    }
    private static bool CustomTextureExist(VegDirectories type, Season key)
    {
        return CustomRegisteredTextures.TryGetValue(type, out Dictionary<Season, Texture?> map) && map.ContainsKey(key);
    }

    public static VegDirectories VegToDirectory(GrassTypes type)
    {
        return (type) switch
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
        return (type) switch
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
            _ => VegDirectories.None,
        };
    }
    private static readonly Dictionary<VegetationType, VegDirectories> vegConversionMap = new()
    {
        { VegetationType.Beech , VegDirectories.Beech },
        { VegetationType.Birch , VegDirectories.Birch },
        { VegetationType.Bush , VegDirectories.Bushes },
        { VegetationType.Oak , VegDirectories.Oak },
        { VegetationType.Pine , VegDirectories.Pine },
        { VegetationType.Fir , VegDirectories.Fir },
        { VegetationType.Yggashoot , VegDirectories.YggaShoot },
        { VegetationType.PlainsBush , VegDirectories.PlainsBush },
        { VegetationType.Shrub , VegDirectories.Shrub },
        { VegetationType.Rock , VegDirectories.Rock }
    };
    
    public static bool ApplyBasedOnAvailable(VegDirectories directory, Season season, Material material, string prop)
    {
        if (directory is VegDirectories.None) return false;
        if (!CustomTextureExist(directory, season)) return false;
        
        Texture? tex = GetCustomTexture(directory, season);
        if (!tex) return false;

        material.SetTexture(prop, tex);
        material.color = Color.white;
        return true;

    }
    
    // public static void CreateColorActions(GameObject prefab, List<Action> actions, VegetationType type)
    // {
    //     List<Color> ConfigSummerColors = new()
    //     {
    //         _SummerColor1.Value,
    //         _SummerColor2.Value,
    //         _SummerColor3.Value,
    //         _SummerColor4.Value
    //     };
    //     
    //     List<Color> ConfigSpringColors = new()
    //     {
    //         _SpringColor1.Value,
    //         _SpringColor2.Value,
    //         _SpringColor3.Value,
    //         _SpringColor4.Value
    //     };
    //     
    //     List<Color> ConfigFallColors = new ()
    //     {
    //         _FallColor1.Value,
    //         _FallColor2.Value,
    //         _FallColor3.Value,
    //         _FallColor4.Value
    //     };
    //
    //     List<Color> ConfigWinterColors = new()
    //     {
    //         _WinterColor1.Value,
    //         _WinterColor2.Value,
    //         _WinterColor3.Value,
    //         _WinterColor4.Value,
    //
    //     };
    //     // Filter prefabs here if you want to exclude them from color tinting
    //     switch (_Season.Value)
    //     {
    //         case Season.Spring:
    //             if (prefab.name.ToLower().Contains("cloud")) break;
    //             foreach (Color color in ConfigSpringColors) actions.Add( ApplyColor(prefab, color, type));
    //             break;
    //         case Season.Summer:
    //             if (prefab.name.ToLower().Contains("cloud")) break;
    //             // Do not apply any color tinting for summer
    //             foreach (Color color in ConfigSummerColors) actions.Add( ApplyColor(prefab, color, type));
    //             break;
    //         case Season.Winter:
    //             if (prefab.name.ToLower().Contains("cloud")) break;
    //             foreach (Color color in ConfigWinterColors) actions.Add( ApplyColor(prefab, color, type));
    //             break;
    //         case Season.Fall:
    //             foreach (Color color in ConfigFallColors) actions.Add( ApplyColor(prefab, color, type));
    //             break;
    //     }
    // }

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