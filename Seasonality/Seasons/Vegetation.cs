using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class Vegetation
{
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
                // Set terrain back to default values
                TerrainPatch.UpdateTerrain();
                currentModEnabled = _ModEnabled.Value;
                return;
            };
            ModifyPrefab(__instance.gameObject);
        }
    }
    
    private static void ModifyPrefab(GameObject prefab)
    {
        VegetationType type = Utils.GetVegetationType(prefab.name);
        if (type is VegetationType.None) return;

        // If custom textures, use that
        if (!Utils.ApplyIfAvailable(prefab, type)) ApplyMaterialToObj(prefab, type);
        switch (type)
        {
            case VegetationType.Beech or VegetationType.BeechSmall or VegetationType.Birch or VegetationType.Oak or VegetationType.Yggashoot 
                or VegetationType.Bush or VegetationType.PlainsBush or VegetationType.Shrub or VegetationType.Vines or VegetationType.RaspberryBush 
                or VegetationType.BlueberryBush:
                ApplyColorTint(prefab, type);
                break;
        }
    }

    private static void ApplyColorTint(GameObject prefab, VegetationType type)
    {
        List<Action> actions = new();
        switch (_Season.Value)
        {
            case Season.Fall:
                Utils.CreateColorActions(prefab, actions, type);
                break;
            case Season.Spring:
                switch (type)
                {
                    case VegetationType.Beech or VegetationType.BeechSmall or VegetationType.Oak 
                        or VegetationType.Birch or VegetationType.Yggashoot:
                        // Do not apply colors
                        break;
                    default: Utils.CreateColorActions(prefab, actions, type); break;
                }
                break;
            case Season.Summer:
                switch (type)
                {
                    case VegetationType.Rock: 
                        // Do not apply colors
                        break;
                    default: Utils.CreateColorActions(prefab, actions, type); break;
                }
                break;
            case Season.Winter:
                switch (type)
                {
                    default: Utils.CreateColorActions(prefab, actions, type); break;
                }
                break;
                
        }
        Utils.ApplyRandomly(actions);
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
        if (!prefab.TryGetComponent(out ParticleSystem particleSystem)) return;
        ParticleSystem.MainModule main = particleSystem.main;

        switch (_Season.Value)
        {
            case Season.Winter: main.startColor = Color.white; break;
            default: main.startColor = color; break;
        }
    }

    private static void ModifyMeshRenderer(Transform prefab, Color color, modificationType modType, VegetationType type)
    {
        
        if (!prefab.TryGetComponent(out MeshRenderer meshRenderer)) return;
        Material[]? materials = meshRenderer.materials;
        foreach (Material mat in materials)
        {
            string materialName = mat.name.ToLower();
            if (materialName.Contains("raspberry") || materialName.Contains("blueberry"))
            {
                // Raspberry and blueberry bushes fruits are named: sphere
                if (prefab.name.ToLower().Contains("sphere")) return;
            }
            if (modType is modificationType.Material)
            {
                // Modify both moss and main tex
                ModifyMaterialProperties(mat, type, true); 
                continue;
            }
            
            ModifyMaterialProperties(mat, type); // Modify moss 
            if (materialName.Contains("bark") || materialName.Contains("trunk") || materialName.Contains("log") || materialName.Contains("wood") || materialName.Contains("branch")) continue;
            switch (_Season.Value)
            {
                case Season.Fall:
                    switch (type)
                    {
                        case VegetationType.Beech or VegetationType.BeechSmall:
                            ModifyMaterialProperties(mat, type, true);
                            break;
                    }
                    break;
                case Season.Spring:
                    switch (type)
                    {
                        case VegetationType.Vines:
                            continue;
                    }
                    break;
            }
            // For everything else, modify color
            mat.color = color;
        }
    }

    private static void ModifyMaterialProperties(Material mat, VegetationType type, bool modifyMainTex = false)
    {
        // Mist land cliffs do make it here but they do not change texture
        // rock_mistlands (Instance): _BumpMap
        // rock_mistlands (Instance): _EmissiveTex
        // rock_mistlands (Instance): _MossTex
        // rock_mistlands (Instance): _GlossMap
        // rock_mistlands (Instance): _MetalTex

        string materialName = mat.name.ToLower();
        string[]? properties = mat.GetTexturePropertyNames();
        if (Utils.FindTexturePropName(properties, "moss", out string mossProp)) ModifyMossTex(mossProp, mat);
        if (Utils.FindTexturePropName(properties, "main", out string mainProp))
        {
            if (materialName.Contains("bark") 
                || materialName.Contains("trunk") 
                || materialName.Contains("log") 
                || materialName.Contains("wood")
                || materialName.Contains("stump")
               ) return;
            if (modifyMainTex) ModifyMainTex(mainProp, mat, type);
        };
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
                    case VegetationType.Oak:
                        if (normalizedName == "oak_leaf")
                        {
                            Texture? customOak = Utils.GetCustomTexture(VegDirectories.Oak, Season.Winter);
                            if (customOak) tex = customOak;
                        }
                        break;
                    default:
                        tex = Utils.GetCustomTexture(directory, Season.Winter);
                        break;
                }
                break;
            case Season.Spring:
                switch (type)
                {
                    case VegetationType.Beech or VegetationType.BeechSmall:
                        if (normalizedName == "beech_leaf")
                        {
                            Texture? customBeechSpring = Utils.GetCustomTexture(VegDirectories.Beech, Season.Spring);
                            if (customBeechSpring) tex = customBeechSpring;
                            material.color = new Color(1f, 1f, 1f, 0.8f);
                            break;
                        }

                        if (normalizedName == "beech_leaf_small")
                        {
                            Texture? customBeechSmallSpring = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Spring);
                            if (customBeechSmallSpring) tex = customBeechSmallSpring;
                            material.color = new Color(1f, 1f, 1f, 0.8f);
                        }
                        break;
                    case VegetationType.Oak:
                        if (normalizedName == "oak_leaf")
                        {
                            Texture? customOakSpring = Utils.GetCustomTexture(VegDirectories.Oak, Season.Spring);
                            if (customOakSpring) tex = customOakSpring;
                            material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                        }
                        break;
                    case VegetationType.Birch:
                        if (normalizedName == "birch_leaf")
                        {
                            Texture? customBirchSpring = Utils.GetCustomTexture(VegDirectories.Birch, Season.Spring);
                            if (customBirchSpring) tex = customBirchSpring;
                            material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                        }
                        break;
                    case VegetationType.Yggashoot:
                        if (normalizedName == "shoot_leaf_mat")
                        {
                            Texture? customYggaSpring = Utils.GetCustomTexture(VegDirectories.YggaShoot, Season.Spring);
                            if (customYggaSpring) tex = customYggaSpring;
                            material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
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
                        Texture? customPine = Utils.GetCustomTexture(VegDirectories.Pine, Season.Fall);
                        if (customPine) tex = customPine;
                        break;
                    case VegetationType.Fir:
                        Texture? customFir = Utils.GetCustomTexture(VegDirectories.Fir, Season.Fall);
                        if (customFir) tex = customFir;
                        break;
                    case VegetationType.Beech or VegetationType.BeechSmall:
                        if (material.name.Contains("small")) { tex = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Fall); break; }
                        tex = Utils.GetCustomTexture(VegDirectories.Beech, Season.Fall);
                        break;
                    default:
                        tex = Utils.GetCustomTexture(directory, Season.Fall);
                        break;
                }                    
                break;
        }
        if (tex) material.SetTexture(propertyName, tex);
    }

    private static void ModifyMossTex(string propertyName, Material material)
    {
        switch (_Season.Value)
        {
            case Season.Winter:
                Texture? customWinter = Utils.GetCustomTexture(VegDirectories.Moss, Season.Winter);
                if (customWinter) material.SetTexture(propertyName, customWinter);
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