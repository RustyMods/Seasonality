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
        Dictionary<string, VegetationType> conversionMap = new()
        {
            { "beech", VegetationType.Beech },
            { "birch", VegetationType.Birch },
            { "fir", VegetationType.Fir },
            { "pine", VegetationType.Pine },
            { "yggashoot", VegetationType.Yggashoot },
            { "swamptree", VegetationType.Swamp },
            { "oak", VegetationType.Oak },
            { "stubbe", VegetationType.Stubbe },
            { "bush", VegetationType.Bush },
            { "shrub", VegetationType.Shrub },
            { "rock", VegetationType.Rock },
            { "statue", VegetationType.Rock },
            { "cliff", VegetationType.Rock },
            { "giant", VegetationType.Rock },
            { "runestone", VegetationType.Rock }
        };

        foreach (KeyValuePair<string, VegetationType> kvp in conversionMap)
        {

            if (!prefabName.ToLower().Contains(kvp.Key) || prefabName == "GiantBloodSack(Clone)") continue;
            if (prefabName.ToLower().Contains("bonfire")) continue;
            if (prefabName.ToLower().Contains("log")) return VegetationType.Log;
            if (prefabName == "YggdrasilRoot(Clone)") return VegetationType.Log;
            if (prefabName == "Bush02_en(Clone)") return VegetationType.PlainsBush;
            
            return kvp.Value;
        }
        return VegetationType.None;
    }

    public static GrassTypes GetGrassType(string clutterName)
    {
        Dictionary<string, GrassTypes> conversionMap = new()
        {
            { "instanced_meadows_grass", GrassTypes.GreenGrass },
            { "instanced_meadows_grass_short", GrassTypes.GreenGrass },
            { "instanced_shrub", GrassTypes.Shrubs },
            { "clutter_shrub_large", GrassTypes.Shrubs },
            { "instanced_forest_groundcover_brown" , GrassTypes.GroundCover },
            { "instanced_forest_groundcover",GrassTypes.GroundCover },
            { "instanced_swamp_grass", GrassTypes.SwampGrass },
            { "instanced_heathgrass" , GrassTypes.HeathGrass },
            { "grasscross_heath_green", GrassTypes.HeathGrass },
            { "instanced_heathflowers", GrassTypes.HeathFlowers },
            { "instanced_swamp_ormbunke" , GrassTypes.Ormbunke },
            { "instanced_ormbunke" , GrassTypes.Ormbunke },
            { "instanced_vass", GrassTypes.Vass },
            { "instanced_waterlilies", GrassTypes.WaterLilies },
            { "instanced_mistlands_rockplant", GrassTypes.RockPlant },
            { "instanced_small_rock1", GrassTypes.Rocks },
            { "instanced_mistlands_grass_short", GrassTypes.GreenGrass }
        };

        return conversionMap.TryGetValue(clutterName, out GrassTypes result) ? result : GrassTypes.None;
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

    private static bool CustomTextureExist(VegDirectories type, Season key)
    {
        if (!CustomRegisteredTextures.TryGetValue(type, out Dictionary<Season, Texture?> map)) return false;
        return map.ContainsKey(key);
    }
    public static VegDirectories VegToDirectory(VegetationType type)
    {
        vegConversionMap.TryGetValue(type, out VegDirectories result);
        return result;
    }
    
    public static VegDirectories VegToDirectory(GrassTypes type)
    {
        grassConversionMap.TryGetValue(type, out VegDirectories result);
        return result;
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

    private static readonly Dictionary<GrassTypes, VegDirectories> grassConversionMap = new()
    {
        { GrassTypes.GreenGrass , VegDirectories.MeadowGrass },
        { GrassTypes.GroundCover , VegDirectories.BlackForestGrass },
        { GrassTypes.SwampGrass , VegDirectories.SwampGrass },
        { GrassTypes.HeathGrass , VegDirectories.PlainsGrass },
        { GrassTypes.HeathFlowers , VegDirectories.PlainsFlowers },
        { GrassTypes.Ormbunke , VegDirectories.Ormbunke },
        { GrassTypes.Vass , VegDirectories.Vass },
        { GrassTypes.WaterLilies , VegDirectories.WaterLilies },
        { GrassTypes.RockPlant , VegDirectories.RockPlant },
        { GrassTypes.Shrubs , VegDirectories.Clutter }
    };
    
    
    public static void ApplyBasedOnAvailable(
        Season season, 
        GameObject prefab, 
        VegetationType type,
        List<Action> actions)
    {
        if (vegConversionMap.TryGetValue(type, out VegDirectories directories))
        {
            if (CustomTextureExist(directories, season)) ApplyMaterialToObj(prefab, type);
            else ApplySeasonalColors(prefab, actions, type);
        }
        else
        {
            // Make sure you redefine your filters
            ApplySeasonalColors(prefab, actions, type);
        }
    }

    public static bool ApplyBasedOnAvailable(VegDirectories directory, Season season, Material material, string prop)
    {
        if (CustomTextureExist(directory, season))
        {
            Texture? tex = GetCustomTexture(directory, season);
            if (!tex) return false;
            // SeasonalityLogger.LogWarning($"Applying custom texture to {material.name}");
            material.SetTexture(prop, tex);
            material.color = Color.white;
            return true;
        }

        return false;
    }
    
    private static void ApplySeasonalColors(GameObject prefab, List<Action> actions, VegetationType type)
    {
        List<Color> ConfigSummerColors = new()
        {
            _SummerColor1.Value,
            _SummerColor2.Value,
            _SummerColor3.Value,
            _SummerColor4.Value
        };
        
        List<Color> ConfigSpringColors = new()
        {
            _SpringColor1.Value,
            _SpringColor2.Value,
            _SpringColor3.Value,
            _SpringColor4.Value
        };
        
        List<Color> ConfigFallColors = new ()
        {
            _FallColor1.Value,
            _FallColor2.Value,
            _FallColor3.Value,
            _FallColor4.Value
        };

        List<Color> ConfigWinterColors = new()
        {
            _WinterColor1.Value,
            _WinterColor2.Value,
            _WinterColor3.Value,
            _WinterColor4.Value,

        };
        // Filter prefabs here if you want to exclude them from color tinting
        switch (_Season.Value)
        {
            case Season.Spring:
                if (prefab.name.ToLower().Contains("cloud")) break;
                foreach (Color color in ConfigSpringColors) actions.Add( ApplyColor(prefab, color, type));
                break;
            case Season.Summer:
                if (prefab.name.ToLower().Contains("cloud")) break;
                // Do not apply any color tinting for summer
                foreach (Color color in ConfigSummerColors) actions.Add( ApplyColor(prefab, color, type));
                break;
            case Season.Winter:
                if (prefab.name.ToLower().Contains("cloud")) break;
                foreach (Color color in ConfigWinterColors) actions.Add( ApplyColor(prefab, color, type));
                break;
            case Season.Fall:
                foreach (Color color in ConfigFallColors) actions.Add( ApplyColor(prefab, color, type));
                break;
            
        }
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