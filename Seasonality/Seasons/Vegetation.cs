using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class Vegetation
{
    private static Season currentSeason = Season.Summer;
    [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Awake))]
    static class ZNetViewAwakePatch
    {
        private static void Postfix(ZNetView __instance)
        {
            GameObject prefab = __instance.gameObject;
            ModifyPrefab(prefab);
            
            if (_Season.Value != currentSeason) TerrainPatch.UpdateTerrain();
            currentSeason = _Season.Value;
        }
    
        private static void ModifyPrefab(GameObject prefab)
        {
            VegetationType type = Utils.GetVegetationType(prefab.name);

            if (type is VegetationType.None) return;
            switch (type)
            {
                case VegetationType.Beech or VegetationType.Birch or VegetationType.Oak or VegetationType.Yggashoot or VegetationType.Bush or VegetationType.PlainsBush or VegetationType.Shrub:
                    AssignMods(prefab, modificationType.Color, type);
                    break;
                case VegetationType.Pine or VegetationType.Fir or VegetationType.Log:
                    // These prefabs contain a single texture
                    AssignMods(prefab, modificationType.Material, type);
                    break;
                case VegetationType.Swamp or VegetationType.Stubbe:
                    AssignMods(prefab, modificationType.Material, type);
                    break;
                case VegetationType.Rock:
                    if (prefab.name.ToLower().Contains("minerock")) break;
                    if (prefab.name.ToLower().Contains("vein")) break;
                    if (prefab.name.ToLower().Contains("frac")) break;
                    if (prefab.name.ToLower().Contains("destruction")) break;
                    AssignMods(prefab, modificationType.Material, type);
                    break;
            }
        }
    
        private enum modificationType
        {
            Color,
            Material
        }
    
        private static void AssignMods(GameObject prefab, modificationType modType, VegetationType type)
        {
            switch (modType)
            {
                case modificationType.Color:
                    List<Action> actions = new();
                    switch (_Season.Value)
                    {
                        case Season.Fall: foreach (Color color in SeasonColors.FallColors) actions.Add(ApplyColor(prefab, color, type)); break;
                        case Season.Spring:
                            switch (type)
                            {
                                case VegetationType.Beech or VegetationType.Oak or VegetationType.Birch or VegetationType.Yggashoot:
                                    ApplyMaterialToObj(prefab, type);
                                    break;
                                case VegetationType.Bush:
                                    if (prefab.name.ToLower().Contains("cloud")) break;
                                    foreach (Color color in SeasonColors.SpringColors) actions.Add(ApplyColor(prefab, color, type));
                                    break;
                                default:
                                    foreach (Color color in SeasonColors.SpringColors) actions.Add(ApplyColor(prefab, color, type));
                                    break;
                            }
                            break;
                        case Season.Summer: 
                            break;
                        case Season.Winter:
                            switch (type)
                            {
                                case VegetationType.Shrub or VegetationType.PlainsBush:
                                    ApplyMaterialToObj(prefab, type);
                                    break;
                                default:
                                    foreach (Color color in SeasonColors.WinterColors) actions.Add(ApplyColor(prefab, color, type)); 
                                    break;
                            }

                            break;
                            
                    }
                    Utils.ApplyRandomly(actions);
                    break;
                case modificationType.Material:
                    if (prefab.name.Contains("_Stub")) break;
                    ApplyMaterialToObj(prefab, type);
                    break;
            }
        }
        
        private static Action ApplyColor(GameObject obj, Color color, VegetationType type) { return () => ApplyColorToObj(obj, color, type); }
        
        private static void ApplyColorToObj(GameObject obj, Color color, VegetationType type)
        {
            for (int i = 0; i < obj.transform.childCount; ++i)
            {
                Transform child = obj.transform.GetChild(i);
                
                ModifyMeshRenderer(child, color, modificationType.Color, type);
                ModifyParticleSystem(child, color);
                
                // Recursively apply changes to all children
                if (child.childCount > 0) ApplyColorToObj(child.gameObject, color, type);
            }
        }
    
        private static void ApplyMaterialToObj(GameObject obj, VegetationType type)
        {
            Color color = Color.white;
            for (int i = 0; i < obj.transform.childCount; ++i)
            {
                Transform child = obj.transform.GetChild(i);
                
                ModifyMeshRenderer(child, color, modificationType.Material, type);
                
                // Recursively apply changes to all children
                if (child.childCount > 0) ApplyMaterialToObj(child.gameObject, type);
            }
        }
        
        private static void ModifyParticleSystem(Transform prefab, Color color)
        {
            prefab.TryGetComponent(out ParticleSystem particleSystem);
            if (!particleSystem) return;
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
    
        private static void ModifyMeshRenderer(Transform prefab, Color color, modificationType modType, VegetationType type)
        {
            prefab.TryGetComponent(out MeshRenderer meshRenderer);
            if (!meshRenderer) return;
            
            Material[]? materials = meshRenderer.materials;
            foreach (Material mat in materials)
            {
                string materialName = mat.name.ToLower();
    
                switch (modType)
                {
                    case modificationType.Color:
                        ModifyMaterialProperties(mat, type); // Modify moss 

                        if (materialName.Contains("bark") || materialName.Contains("trunk") || materialName.Contains("log") || materialName.Contains("wood")) { continue; }

                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                switch (type)
                                {
                                    case VegetationType.Shrub or VegetationType.Bush:
                                        mat.color = color;
                                        break;
                                    default:
                                        mat.color = Color.clear; // Set leaves to be invisible
                                        break;
                                }
                                break;
                            default:
                                mat.color = color;
                                break;
                        }
                        break;
                    case modificationType.Material:
                        ModifyMaterialProperties(mat, type, true);
                        break;
                }
            }
        }
    
        private static void ModifyMaterialProperties(Material mat, VegetationType type, bool modifyMainTex = false)
        {
            string[]? properties = mat.GetTexturePropertyNames();
            foreach (string prop in properties)
            {
                if (prop.ToLower().Contains("main") && modifyMainTex)
                {
                    ModifyMainTex(prop, mat, type);
                }
                if (prop.ToLower().Contains("moss"))
                {
                    ModifyMossTex(prop, mat, type);
                }
            }
        }

        private static void ModifyMainTex(string propertyName, Material material, VegetationType type)
        {
            string normalizedName = material.name.ToLower().Replace(" (instance)", "");
            switch (_Season.Value)
            {
                case Season.Winter:
                    switch (type)
                    {
                        case VegetationType.Pine:
                            material.SetTexture(propertyName, PineTree_Winter);
                            break;
                        case VegetationType.Fir:
                            material.SetTexture(propertyName, FirTree_Winter);
                            break;
                        case VegetationType.PlainsBush:
                            material.SetTexture(propertyName, PlainsBush_Winter);
                            break;
                        case VegetationType.Shrub:
                            material.SetTexture(propertyName, Shrub02_Winter);
                            break;
                    }
                    break;
                case Season.Spring:
                    switch (type)
                    {
                        case VegetationType.Pine:
                            material.SetTexture(propertyName, PineTree_Spring);
                            break;
                        case VegetationType.Fir:
                            material.SetTexture(propertyName, FirTree_Spring);
                            break;
                        case VegetationType.Beech:
                            if (normalizedName == "beech_leaf")
                            {
                                material.SetTexture(propertyName, BeechLeaf_Spring);
                                material.color = new Color(1f, 1f, 1f, 0.8f);
                                break;
                            }

                            if (normalizedName == "beech_leaf_small")
                            {
                                material.SetTexture(propertyName, BeechLeaf_Small_Spring);
                                material.color = new Color(1f, 1f, 1f, 0.8f);
                                break;
                            }

                            break;
                        case VegetationType.Oak:
                            if (normalizedName == "oak_leaf")
                            {
                                material.SetTexture(propertyName, OakLeaf_Spring);
                                material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                                break;
                            }
                            break;
                        case VegetationType.Birch:
                            if (normalizedName == "birch_leaf")
                            {
                                material.SetTexture(propertyName, BirchLeaf_Spring);
                                material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                                break;
                            }

                            break;
                        case VegetationType.Yggashoot:
                            if (normalizedName == "shoot_leaf_mat")
                            {
                                material.SetTexture(propertyName, ShootLeaf_Spring);
                                material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                                break;
                            }
                            break;
                    }                        
                    break;
                case Season.Summer:
                    break;
                case Season.Fall:
                    switch (type)
                    {
                        case VegetationType.Pine:
                            material.SetTexture(propertyName, PineTree_Fall);
                            break;
                        case VegetationType.Fir:
                            material.SetTexture(propertyName, FirTree_Fall);
                            break;
                    }                    
                    break;
            }
        }

        private static void ModifyMossTex(string propertyName, Material material, VegetationType type)
        {
            switch (_Season.Value)
            {
                case Season.Winter:
                    
                    // Mistland cliffs do make it here but they do not change texture
                    
                    material.SetTexture(propertyName, SnowTexture);
                    break;
            }
        }
    }
}