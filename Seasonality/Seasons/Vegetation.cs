using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class Vegetation
{
    private static Season currentSeason = _Season.Value;
    private static Toggle currentModEnabled;
    private enum modificationType { Color, Material }
    
    [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Awake))]
    public static class ZNetViewAwakePatch
    {
        private static void Postfix(ZNetView __instance)
        {
            if (!__instance) return;
            if (_ModEnabled.Value is Toggle.Off)
            {
                if (currentModEnabled == _ModEnabled.Value) return;
                TerrainPatch.UpdateTerrain();
                currentModEnabled = _ModEnabled.Value;
                return;
            };
            GameObject prefab = __instance.gameObject;
            ModifyPrefab(prefab);
            
            if (_Season.Value != currentSeason) TerrainPatch.UpdateTerrain();
            currentSeason = _Season.Value;
        }
    }
    
    private static void ModifyPrefab(GameObject prefab)
    {
        VegetationType type = Utils.GetVegetationType(prefab.name);

        if (type is VegetationType.None) return;
        switch (type)
        {
            case VegetationType.Beech or VegetationType.Birch or VegetationType.Oak or VegetationType.Yggashoot 
                or VegetationType.Bush or VegetationType.PlainsBush or VegetationType.Shrub:
                // These prefabs are well suited for color tinting
                // Some cases where it is better to replace texture
                // Within AssignMods method, are filters to redirect towards texture replacement
                AssignMods(prefab, modificationType.Color, type);
                break;
            case VegetationType.Pine or VegetationType.Fir or VegetationType.Log
                or VegetationType.Swamp or VegetationType.Stubbe:
                // These prefabs contain a single texture
                // Tinting is not recommended, so always choose texture replace
                AssignMods(prefab, modificationType.Material, type);
                break;
            case VegetationType.Rock:
                // Similar to above case
                // Differentiated in order to filter certain prefabs
                if (prefab.name.ToLower().Contains("minerock")) break;
                if (prefab.name.ToLower().Contains("vein")) break;
                if (prefab.name.ToLower().Contains("frac")) break;
                if (prefab.name.ToLower().Contains("destruction")) break;
                AssignMods(prefab, modificationType.Material, type);
                break;
        }
    }

    private static void AssignMods(GameObject prefab, modificationType modType, VegetationType type)
    {
        switch (modType)
        {
            case modificationType.Material:
                if (prefab.name.Contains("_Stub")) break;
                ApplyMaterialToObj(prefab, type);
                break;
            
            case modificationType.Color:
                if (prefab.name.Contains("_Stub")) break;
                List<Action> actions = new();
                switch (_Season.Value)
                {
                    case Season.Fall:
                        // use this method to make sure that the texture is available
                        // and directory type matches a vegetation type
                        Utils.ApplyBasedOnAvailable(Season.Fall, prefab, type, actions);
                        break;
                    case Season.Spring:
                        switch (type)
                        {
                            case VegetationType.Beech or VegetationType.Oak 
                                or VegetationType.Birch or VegetationType.Yggashoot:
                                ApplyMaterialToObj(prefab, type);
                                break;
                            default:
                                Utils.ApplyBasedOnAvailable(Season.Spring, prefab, type, actions);
                                break;
                        }
                        break;
                    case Season.Summer:
                        switch (type)
                        {
                            case VegetationType.Rock:
                                break;
                            default:
                                Utils.ApplyBasedOnAvailable(Season.Summer, prefab, type, actions);
                                break;
                        }
                        break;
                    case Season.Winter:
                        switch (type)
                        {
                            case VegetationType.Shrub or VegetationType.PlainsBush:
                                ApplyMaterialToObj(prefab, type);
                                break;
                            default:
                                Utils.ApplyBasedOnAvailable(Season.Winter, prefab, type, actions);
                                break;
                        }

                        break;
                        
                }
                Utils.ApplyRandomly(actions);
                break;
        }
    }
    
    public static Action ApplyColor(GameObject obj, Color color, VegetationType type) { return () => ApplyColorToObj(obj, color, type); }
    
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

    public static void ApplyMaterialToObj(GameObject obj, VegetationType type)
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
                case modificationType.Material:
                    ModifyMaterialProperties(mat, type, true); // Modify both moss and main tex
                    break;
                
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
            }
        }
    }

    private static void ModifyMaterialProperties(Material mat, VegetationType type, bool modifyMainTex = false)
    {
        string[]? properties = mat.GetTexturePropertyNames();
        foreach (string prop in properties)
        {
            if (prop.ToLower().Contains("moss"))
            {
                ModifyMossTex(prop, mat);
            }

            if (!prop.ToLower().Contains("main") || !modifyMainTex) continue;
            // Make sure only the leaves are affected 
            if (mat.name.Contains("bark") 
                || mat.name.Contains("trunk") 
                || mat.name.Contains("log") 
                || mat.name.Contains("wood")
                || mat.name.Contains("stump")
                ) continue;

            ModifyMainTex(prop, mat, type);

        }
    }

    private static void ModifyMainTex(string propertyName, Material material, VegetationType type)
    {
        string normalizedName = material.name.ToLower().Replace(" (instance)", "");
        VegDirectories directory = Utils.VegToDirectory(type);
        Texture? tex = null!;
        switch (_Season.Value)
        {
            case Season.Winter:
                switch (type)
                {
                    case VegetationType.Pine:
                        tex = PineTree_Winter;
                        Texture? customPineWinter = Utils.GetCustomTexture(VegDirectories.Pine, Season.Winter);
                        if (customPineWinter) tex = customPineWinter;
                        break;
                    case VegetationType.Fir:
                        tex = FirTree_Winter;
                        Texture? customFirWinter = Utils.GetCustomTexture(VegDirectories.Fir, Season.Winter);
                        if (customFirWinter) tex = customFirWinter;
                        break;
                    case VegetationType.PlainsBush:
                        tex = PlainsBush_Winter;
                        Texture? plainBushWinter =
                            Utils.GetCustomTexture(VegDirectories.PlainsBush, Season.Winter);
                        if (plainBushWinter) tex = plainBushWinter;
                        break;
                    case VegetationType.Shrub:
                        tex = Shrub02_Winter;
                        Texture? customShrubWinter = Utils.GetCustomTexture(VegDirectories.Shrub, Season.Winter);
                        if (customShrubWinter) tex = customShrubWinter;
                        break;
                    default:
                        tex = Utils.GetCustomTexture(directory, Season.Winter);
                        break;
                }
                break;
            case Season.Spring:
                switch (type)
                {
                    case VegetationType.Pine:
                        tex = PineTree_Spring;
                        Texture? customPineSpring = Utils.GetCustomTexture(VegDirectories.Pine, Season.Spring);
                        if (customPineSpring) tex = customPineSpring;
                        break;
                    case VegetationType.Fir:
                        tex = FirTree_Spring;
                        Texture? customFirSpring = Utils.GetCustomTexture(VegDirectories.Fir, Season.Spring);
                        if (customFirSpring) tex = customFirSpring;
                        break;
                    case VegetationType.Beech:
                        if (normalizedName == "beech_leaf")
                        {
                            tex = BeechLeaf_Spring;
                            Texture? customBeechSpring =
                                Utils.GetCustomTexture(VegDirectories.Beech, Season.Spring);
                            if (customBeechSpring) tex = customBeechSpring;
                            material.color = new Color(1f, 1f, 1f, 0.8f);
                            break;
                        }

                        if (normalizedName == "beech_leaf_small")
                        {
                            tex = BeechLeaf_Small_Spring;
                            Texture? customBeechSmallSpring =
                                Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Spring);
                            if (customBeechSmallSpring) tex = customBeechSmallSpring;
                            material.color = new Color(1f, 1f, 1f, 0.8f);
                        }
                        break;
                    case VegetationType.Oak:
                        if (normalizedName == "oak_leaf")
                        {
                            tex = OakLeaf_Spring;
                            Texture? customOakSpring = Utils.GetCustomTexture(VegDirectories.Oak, Season.Spring);
                            if (customOakSpring) tex = customOakSpring;
                            material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                            break;
                        }
                        break;
                    case VegetationType.Birch:
                        if (normalizedName == "birch_leaf")
                        {
                            tex = BirchLeaf_Spring;
                            Texture? customBirchSpring =
                                Utils.GetCustomTexture(VegDirectories.Birch, Season.Spring);
                            if (customBirchSpring) tex = customBirchSpring;
                            material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                        }
                        break;
                    case VegetationType.Yggashoot:
                        if (normalizedName == "shoot_leaf_mat")
                        {
                            tex = ShootLeaf_Spring;
                            Texture? customYggaSpring =
                                Utils.GetCustomTexture(VegDirectories.YggaShoot, Season.Spring);
                            if (customYggaSpring) tex = customYggaSpring;
                            material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                            break;
                        }
                        break;
                    default:
                        tex = Utils.GetCustomTexture(directory, Season.Spring);
                        break;
                }                        
                break;
            case Season.Summer:
                tex = Utils.GetCustomTexture(directory, Season.Summer);
                break;
            case Season.Fall:
                switch (type)
                {
                    case VegetationType.Pine:
                        tex = PineTree_Fall;
                        Texture? customPine = Utils.GetCustomTexture(VegDirectories.Pine, Season.Fall);
                        if (tex) tex = customPine;
                        break;
                    case VegetationType.Fir:
                        tex = FirTree_Fall;
                        Texture? customFir = Utils.GetCustomTexture(VegDirectories.Fir, Season.Fall);
                        if (customFir) tex = customFir;
                        break;
                    case VegetationType.Beech:
                        if (material.name.Contains("small")) { tex = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Fall); break; }
                        tex = Utils.GetCustomTexture(VegDirectories.Beech, Season.Fall);
                        break;
                    default:
                        tex = Utils.GetCustomTexture(VegDirectories.Shrub, Season.Fall);
                        break;
                }                    
                break;
        }
        if (tex) material.SetTexture(propertyName, tex);
    }

    private static void ModifyMossTex(string propertyName, Material material)
    {
        // Mist land cliffs do make it here but they do not change texture
        switch (_Season.Value)
        {
            case Season.Winter:
                Texture? customWinter = Utils.GetCustomTexture(VegDirectories.Moss, Season.Winter);
                material.SetTexture(propertyName, customWinter ? customWinter : SnowTexture);
                break;
            case Season.Fall:
                Texture? customFall = Utils.GetCustomTexture(VegDirectories.Moss, Season.Fall);
                if (customFall) material.SetTexture(propertyName, customFall);
                break;
            case Season.Spring:
                Texture? customSpring = Utils.GetCustomTexture(VegDirectories.Moss, Season.Spring);
                if (customSpring) material.SetTexture(propertyName, customSpring);
                break;
            case Season.Summer:
                Texture? customSummer = Utils.GetCustomTexture(VegDirectories.Moss, Season.Summer);
                if (customSummer) material.SetTexture(propertyName, customSummer);
                break;
        }
    }
}