using System;
using System.Collections.Generic;
using System.Linq;
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
            { "instanced_small_rock1", GrassTypes.Rocks }
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
    
    public static void ApplyRandomly(List<(Action, float)> weightedMethods)
    {
        if (weightedMethods.Count == 0) return;

        // Calculate total weight
        float totalWeight = weightedMethods.Sum(pair => pair.Item2);

        // Generate a random value within the total weight range
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);

        // Iterate through the weighted methods and find the one corresponding to the random value
        float cumulativeWeight = 0f;
        foreach (var (method, weight) in weightedMethods)
        {
            cumulativeWeight += weight;

            if (randomValue <= cumulativeWeight)
            {
                method.Invoke();
                break;
            }
        }
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
                    if (prop.ToLower().Contains("moss"))
                    {
                        mat.SetTexture(prop, tex);
                    }
                }
            }

        }
    }

    public static Texture? GetCustomTexture(VegetationDirectories type, Season key)
    {
        return !CustomRegisteredTextures.TryGetValue(type, out Dictionary<Season, Texture?> map)
            ?
            null
            : map.TryGetValue(key, out Texture? tex)
                ? tex
                : null;
    }

    private static bool CustomTextureExist(VegetationDirectories type, Season key)
    {
        if (!CustomRegisteredTextures.TryGetValue(type, out Dictionary<Season, Texture?> map)) return false;
        return map.ContainsKey(key);
    }

    public static VegetationDirectories VegToDirectory(VegetationType type)
    {
        vegConversionMap.TryGetValue(type, out VegetationDirectories result);
        return result;
    }

    private static readonly Dictionary<VegetationType, VegetationDirectories> vegConversionMap = new()
    {
        { VegetationType.Beech , VegetationDirectories.Beech },
        { VegetationType.Birch , VegetationDirectories.Birch },
        { VegetationType.Bush , VegetationDirectories.Bushes },
        { VegetationType.Oak , VegetationDirectories.Oak },
        { VegetationType.Pine , VegetationDirectories.Pine },
        { VegetationType.Fir , VegetationDirectories.Fir },
        { VegetationType.Yggashoot , VegetationDirectories.YggaShoot },
        { VegetationType.PlainsBush , VegetationDirectories.PlainsBush },
        { VegetationType.Shrub , VegetationDirectories.Shrub },
        { VegetationType.Rock , VegetationDirectories.Rock }
    };
    public static void ApplyBasedOnAvailable(
        Season season, 
        GameObject prefab, 
        VegetationType type,
        List<Action> actions)
    {
        if (vegConversionMap.TryGetValue(type, out VegetationDirectories directories))
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

    private static void ApplySeasonalColors(GameObject prefab, List<Action> actions, VegetationType type)
    {
        // Filter prefabs here if you want to exclude them from color tinting
        switch (_Season.Value)
        {
            case Season.Spring:
                if (prefab.name.ToLower().Contains("cloud")) break;
                foreach (Color color in SeasonColors.FallColors) actions.Add( ApplyColor(prefab, color, type));
                break;
            case Season.Summer:
                // Do not apply any color tinting for summer
                break;
            default:
                foreach (Color color in SeasonColors.FallColors) actions.Add( ApplyColor(prefab, color, type));
                break;
        }
    }
}