using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Seasonality.Behaviors;
using Seasonality.DataTypes;
using Seasonality.Textures;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.Configurations;
using static Seasonality.Seasons.MaterialReplacer;
using static Seasonality.Textures.Directories;
using Random = System.Random;

namespace Seasonality.Seasons;

public static class FallColors
{
    private static List<Material[]> BeechMaterials = new();
    private static List<Material[]> BeechSmallMaterials = new();
    private static List<Material[]> BirchMaterials = new();
    private static List<Material[]> OakMaterials = new();
    private static List<Material[]> YggaMaterials = new();
    private static List<Material[]> BushMaterials = new();
    private static List<Material[]> PlainsBushMaterials = new();
    private static List<Material[]> ShrubMaterials = new();
    private static List<Material[]> RaspberryMaterials = new();
    private static List<Material[]> BlueberryMaterials = new();
    private static readonly int ColorProp = Shader.PropertyToID("_Color");
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [HarmonyPriority(Priority.Last)]
    static class ZNetSceneVegetationPatch
    {
        private static void Postfix(ZNetScene __instance)
        {
            if (!__instance) return;
            foreach (GameObject? prefab in __instance.m_prefabs)
            {
                if (Helpers.Utils.GetVegetationType(prefab.name) is VegetationType.None) continue;
                CacheBaseMaterials(prefab);
            }
            RegisterCustomSeasonalPrefabs(__instance);
        }
        private static List<Material[]> CreateBaseMaterials(GameObject prefab, string specifier, bool contains = true)
        {
            if (prefab.GetComponentInChildren<MeshRenderer>() is not { } PrefabRenderer) return new List<Material[]>();
            
            VegetationType type = Helpers.Utils.GetVegetationType(prefab.name);
            Material[]? materials = PrefabRenderer.materials;
            // Create List of Material Array and apply unique colors to each
            List<Material[]> newMaterialArray = new()
            {
                new Material[materials.Length],
                new Material[materials.Length],
                new Material[materials.Length],
                new Material[materials.Length]
            };

            for (int index = 0; index < newMaterialArray.Count; index++)
            {
                for (int i = 0; i < newMaterialArray[index].Length; ++i)
                {
                    var mat = new Material(materials[i]);
                    mat.name = "custom_" + materials[i].name.Replace("(Instance)", "").Replace(" ", "") + "_" + index;
                    newMaterialArray[index][i] = mat; // Give new material array same values as original
                    CustomMaterials[mat.name] = mat;
                    if (newMaterialArray[index][i].HasProperty("_MossTex"))
                    {
                        m_customMossMaterials[newMaterialArray[index][i]] =
                            newMaterialArray[index][i].GetTexture(MossTex);
                    }
                }

                // Add color tint to specified material
                Material? leafMat = contains
                    ? newMaterialArray[index].FirstOrDefault(x => x.name.ToLower().Contains(specifier))
                    : newMaterialArray[index].FirstOrDefault(x => !x.name.ToLower().Contains(specifier));
                if (leafMat == null) continue;

                string[] properties = leafMat.GetTexturePropertyNames();
                if (Helpers.Utils.FindTexturePropName(properties, "main", out string mainProp))
                {
                    VegDirectories directory = Helpers.Utils.VegToDirectory(type);

                    Texture? tex = Helpers.Utils.GetCustomTexture(directory, Season.Fall.ToString());
                    leafMat.SetTexture(mainProp, tex ? tex : GetDefaultTextures(type, prefab.name));
                    leafMat.color = _fallColors[index].Value;
                    if (directory is VegDirectories.Vines)
                    {
                        leafMat.SetColor(ColorProp, Color.white);
                    }
                }
            }

            return newMaterialArray;
        }
        private static void CacheBaseMaterials(GameObject prefab)
        {
            switch (Helpers.Utils.GetVegetationType(prefab.name))
            {
                case VegetationType.Beech:
                    BeechMaterials = CreateBaseMaterials(prefab, "leaf");
                    break;
                case VegetationType.BeechSmall:
                    BeechSmallMaterials = CreateBaseMaterials(prefab, "leaf");
                    break;
                case VegetationType.Birch:
                    BirchMaterials = CreateBaseMaterials(prefab, "leaf");
                    break;
                case VegetationType.Oak:
                    OakMaterials = CreateBaseMaterials(prefab, "leaf");
                    break;
                case VegetationType.Yggashoot:
                    YggaMaterials = CreateBaseMaterials(prefab, "leaf");
                    break;
                case VegetationType.Bush:
                    BushMaterials = CreateBaseMaterials(prefab, "wood", false);
                    break;
                case VegetationType.PlainsBush:
                    PlainsBushMaterials = CreateBaseMaterials(prefab, "wood", false);
                    break;
                case VegetationType.Shrub:
                    ShrubMaterials = CreateBaseMaterials(prefab, "shrub");
                    break;
                case VegetationType.RaspberryBush:
                    RaspberryMaterials = CreateBaseMaterials(prefab, "wood", false);
                    break;
                case VegetationType.BlueberryBush:
                    BlueberryMaterials = CreateBaseMaterials(prefab, "wood", false);
                    break;
            }
        }
    }

    private static void RegisterCustomSeasonalPrefabs(ZNetScene __instance)
    {
        foreach (var kvp in TextureManager.RegisteredCustomTextures)
        {
            if (__instance.GetPrefab(kvp.Key) is not { } prefab) continue;
            prefab.AddComponent<CustomSeason>();
        }
    }
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.CreateObject))]
    static class GetPrefabPatch
    {
        private static void Postfix(ZNetScene __instance, ref GameObject __result)
        {
            if (!__instance) return;
            ApplyColorMaterials(__result);
        }
    }
    private static void ApplyColorMaterials(GameObject prefab)
    {
        if (prefab == null) return;
        VegetationType type = Helpers.Utils.GetVegetationType(prefab.name);
        if (type is VegetationType.None) return;
        switch (type)
        {
            case VegetationType.Beech:
                SetMaterials(BeechMaterials, prefab);
                break;
            case VegetationType.BeechSmall:
                SetMaterials(BeechSmallMaterials, prefab);
                break;
            case VegetationType.Birch:
                SetMaterials(BirchMaterials, prefab);
                break;
            case VegetationType.Oak:
                SetMaterials(OakMaterials, prefab);
                break;
            case VegetationType.Yggashoot:
                SetMaterials(YggaMaterials, prefab);
                break;
            case VegetationType.Bush:
                SetMaterials(BushMaterials, prefab);
                break;
            case VegetationType.PlainsBush:
                SetMaterials(PlainsBushMaterials, prefab);
                break;
            case VegetationType.Shrub:
                SetMaterials(ShrubMaterials, prefab);
                break;
            case VegetationType.RaspberryBush:
                SetMaterials(RaspberryMaterials, prefab);
                break;
            case VegetationType.BlueberryBush:
                SetMaterials(BlueberryMaterials, prefab);
                break;
        }
    }
    private static void SetMaterials(List<Material[]> materials, GameObject prefab)
    {
        if (materials.Count != 4) return;
        Random random = new Random();
        int randomIndex = random.Next(materials.Count);

        VegetationType type = Helpers.Utils.GetVegetationType(prefab.name);
        
        for (int i = 0; i < prefab.transform.childCount; ++i)
        {
            Transform? child = prefab.transform.GetChild(i);
            if (!child || !child.TryGetComponent(out MeshRenderer meshRenderer)) continue;
            switch (type)
            {
                case VegetationType.Birch or VegetationType.Yggashoot:
                {
                    if (child.name.ToLower() is "lod1")
                    {
                        materials = materials
                            .Select(array => array.Reverse().ToArray())
                            .ToList();
                    }

                    break;
                }
                case VegetationType.PlainsBush:
                {
                    if (child.name.ToLower() is "low")
                    {
                        materials = materials
                            .Where(array => array.Length > 0)          // Exclude empty arrays
                            .Select(array => new Material[] { array[1] }) // Create a single-item array with the second element
                            .ToList();
                    }

                    break;
                }
            }
            
            meshRenderer.materials = materials[randomIndex];

            if (child.childCount > 0) SetMaterials(materials, child.gameObject);
        }
    }
    private static void ModifyParticleSystem(Transform prefab, Color color)
    {
        if (!prefab.TryGetComponent(out ParticleSystem particleSystem)) return;
        ParticleSystem.MainModule main = particleSystem.main;

        switch (_Season.Value)
        {
            case Season.Winter:
                main.startColor = Color.white;
                break;
            default:
                main.startColor = color;
                break;
        }
    }
    private static Texture? GetDefaultTextures(VegetationType type, string name, bool isBark = false)
    {
        try
        {
            return type switch
            {
                VegetationType.Beech => isBark
                    ? CachedTextures["beech_bark"]
                    : CachedTextures["beech_leaf"],
                VegetationType.BeechSmall => isBark
                    ? CachedTextures["beech_bark_small"]
                    : CachedTextures["beech_leaf_small"],
                VegetationType.Birch => isBark
                    ? CachedTextures["birch_bark"]
                    : CachedTextures.TryGetValue("birch_leaf", out Texture tex)
                        ? tex
                        : null,
                VegetationType.Fir => name.Contains("small")
                    ? CachedTextures["Pine_tree_small"]
                    : CachedTextures["Pine_tree"],
                VegetationType.Pine => CachedTextures["PineTree_01"],
                VegetationType.Swamp => isBark
                    ? CachedTextures["swamptree1_bark"]
                    : CachedTextures["swamptree1_branch"],
                VegetationType.Oak => isBark
                    ? CachedTextures["oak_bark"]
                    : CachedTextures["oak_leaf"],
                VegetationType.Stubbe => CachedTextures["stump"],
                VegetationType.Bush => CachedTextures["Bush01"],
                VegetationType.PlainsBush => CachedTextures.TryGetValue("Bush02_en", out Texture tex) ? tex : null,
                VegetationType.Shrub => CachedTextures["shrub"],
                VegetationType.CloudberryBush => CachedTextures["Cloudberrybush"],
                VegetationType.RaspberryBush => CachedTextures.TryGetValue("Bush01_raspberry", out Texture tex)
                    ? tex
                    : null,
                VegetationType.BlueberryBush => CachedTextures.TryGetValue("Bush01_blueberry", out Texture tex)
                    ? tex
                    : null,
                VegetationType.Vines => CachedTextures.TryGetValue("Vines_Mat", out Texture tex) ? tex : null,
                VegetationType.YggashootSmall or VegetationType.Yggashoot => isBark
                    ? CachedTextures.TryGetValue("Shoot_Trunk_mat", out Texture trunk) ? trunk : null
                    : CachedTextures.TryGetValue("Shoot_Leaf_mat", out Texture tex)
                        ? tex
                        : null,
                _ => null
            };

        }
        catch (Exception)
        {
            // SeasonalityLogger.LogWarning("Failed to get texture for " + type);
            return null;
        }
    }
}
    
