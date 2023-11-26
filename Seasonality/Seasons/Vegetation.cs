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
            GameObject prefab = __instance.gameObject;
            
            ModifyPrefab(prefab);
        }
    }
    
    private static void ModifyPrefab(GameObject prefab)
    {
        VegetationType type = Utils.GetVegetationType(prefab.name);
        if (type is VegetationType.None) return;

        // If custom textures, use that
        if (Utils.ApplyIfAvailable(prefab, type)) return;
        // Else use plugin settings
        switch (type)
        {
            case VegetationType.Beech or VegetationType.BeechSmall or VegetationType.Birch or VegetationType.Oak or VegetationType.Yggashoot 
                or VegetationType.Bush or VegetationType.PlainsBush or VegetationType.Shrub or VegetationType.Vines:
                ApplyColorTint(prefab, type);
                break;
            default:
                ApplyMaterialToObj(prefab, type);
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
                        ApplyMaterialToObj(prefab, type);
                        break;
                    default: Utils.CreateColorActions(prefab, actions, type); break;
                }
                break;
            case Season.Summer:
                switch (type)
                {
                    case VegetationType.Rock: break;
                    default: Utils.CreateColorActions(prefab, actions, type); break;
                }
                break;
            case Season.Winter:
                switch (type)
                {
                    case VegetationType.Shrub or VegetationType.PlainsBush: 
                        ApplyMaterialToObj(prefab, type); break;
                    case VegetationType.Beech or VegetationType.BeechSmall or VegetationType.Birch:
                        if (prefab.name.ToLower().Contains("small"))
                        {
                            Utils.CreateColorActions(prefab, actions, type);
                        }
                        else ApplyMaterialToObj(prefab, type);
                        break;
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
                case Season.Winter:
                    switch (type)
                    {
                        case VegetationType.Beech or VegetationType.BeechSmall or VegetationType.Birch or VegetationType.Oak or VegetationType.Yggashoot:
                            // Set leaves to be invisible
                            mat.color = Color.clear; 
                            break;
                        default: mat.color = color; break;
                    }
                    continue;
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
                            return;
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
        foreach (string prop in properties)
        {
            if (prop.ToLower().Contains("moss"))
            {
                // if (mat.name.ToLower().Contains("mistland")) return;
                ModifyMossTex(prop, mat);
            }
            if (!prop.ToLower().Contains("main") || !modifyMainTex) continue;
            // Make sure only the leaves are affected 
            if (materialName.Contains("bark") 
                || materialName.Contains("trunk") 
                || materialName.Contains("log") 
                || materialName.Contains("wood")
                || materialName.Contains("stump")
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
                        Texture? plainBushWinter = Utils.GetCustomTexture(VegDirectories.PlainsBush, Season.Winter);
                        if (plainBushWinter) tex = plainBushWinter;
                        break;
                    case VegetationType.Shrub:
                        tex = Shrub02_Winter;
                        Texture? customShrubWinter = Utils.GetCustomTexture(VegDirectories.Shrub, Season.Winter);
                        if (customShrubWinter) tex = customShrubWinter;
                        break;
                    case VegetationType.Beech:
                        tex = BeechLeaf_Winter;
                        Texture? customBeechLeafWinter = Utils.GetCustomTexture(VegDirectories.Beech, Season.Winter);
                        if (customBeechLeafWinter) tex = customBeechLeafWinter;
                        break;
                    case VegetationType.BeechSmall:
                        tex = BeechLeaf_Winter;
                        Texture? customBeechSmallLeafWinter = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Winter);
                        if (customBeechSmallLeafWinter) tex = customBeechSmallLeafWinter;
                        break;
                    case VegetationType.Birch:
                        tex = BirchLeaf_Winter;
                        Texture? customBirchLeafWinter = Utils.GetCustomTexture(VegDirectories.Birch, Season.Winter);
                        if (customBirchLeafWinter) tex = customBirchLeafWinter;
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
                    case VegetationType.Beech or VegetationType.BeechSmall:
                        if (normalizedName == "beech_leaf")
                        {
                            tex = BeechLeaf_Spring;
                            Texture? customBeechSpring = Utils.GetCustomTexture(VegDirectories.Beech, Season.Spring);
                            if (customBeechSpring) tex = customBeechSpring;
                            material.color = new Color(1f, 1f, 1f, 0.8f);
                            break;
                        }

                        if (normalizedName == "beech_leaf_small")
                        {
                            tex = BeechLeaf_Small_Spring;
                            Texture? customBeechSmallSpring = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Spring);
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
                        }
                        break;
                    case VegetationType.Birch:
                        if (normalizedName == "birch_leaf")
                        {
                            tex = BirchLeaf_Spring;
                            Texture? customBirchSpring = Utils.GetCustomTexture(VegDirectories.Birch, Season.Spring);
                            if (customBirchSpring) tex = customBirchSpring;
                            material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                        }
                        break;
                    case VegetationType.Yggashoot:
                        if (normalizedName == "shoot_leaf_mat")
                        {
                            tex = ShootLeaf_Spring;
                            Texture? customYggaSpring = Utils.GetCustomTexture(VegDirectories.YggaShoot, Season.Spring);
                            if (customYggaSpring) tex = customYggaSpring;
                            material.color = new Color(0.8f, 0.7f, 0.8f, 1f);
                        }
                        break;
                    case VegetationType.Swamp:
                        tex = Dead_Branch_Spring;
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
                        if (customPine) tex = customPine;
                        break;
                    case VegetationType.Fir:
                        tex = FirTree_Fall;
                        Texture? customFir = Utils.GetCustomTexture(VegDirectories.Fir, Season.Fall);
                        if (customFir) tex = customFir;
                        break;
                    case VegetationType.Beech or VegetationType.BeechSmall:
                        if (material.name.Contains("small")) { tex = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Fall); break; }
                        tex = Utils.GetCustomTexture(VegDirectories.Beech, Season.Fall);
                        if (!tex) tex = BeechLeaf_White;
                        break;
                    case VegetationType.Swamp:
                        tex = Dead_Branch_Summer;
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