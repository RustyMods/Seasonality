using System.Collections.Generic;
using HarmonyLib;
using Seasonality.DataTypes;
using Seasonality.Textures;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seasonality.Managers;

public static class ClutterManager
{
    private static readonly Dictionary<string, ClutterMaterialData> m_data = new();
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int TerrainColorTex = Shader.PropertyToID("_TerrainColorTex");

    [HarmonyPatch(typeof(ClutterSystem), nameof(ClutterSystem.Awake))]
    private static class ClutterSystem_Awake_Patch
    {
        private static void Postfix(ClutterSystem __instance)
        {
            if (!__instance) return;
            CacheTextures(__instance);
            SetClutter();
        }
    }

    private static void UpdateClutterSystem()
    {
        if (!ClutterSystem.instance) return;
        ClutterSystem.instance.m_forceRebuild = true;
        ClutterSystem.instance.LateUpdate();
    }
    
    private static void CacheTextures(ClutterSystem instance)
    {
        foreach (ClutterSystem.Clutter clutter in instance.m_clutter)
        {
            if (!clutter.m_prefab.TryGetComponent(out InstanceRenderer renderer)) continue;
            SaveMaterialData(renderer.m_material);
        }
    }

    private static void SaveMaterialData(Material material)
    {
        if (m_data.ContainsKey(material.name)) return;
        var data = new ClutterMaterialData()
        {
            MainColor = material.color,
        };
        string[] textureProperties = material.GetTexturePropertyNames();
        Dictionary<string, Texture> textures = new();
        foreach (string property in textureProperties)
        {
            Texture? texture = material.GetTexture(property);
            if (!texture) continue;
            textures[property] = texture;
        }

        data.Textures = textures;
        m_data[material.name] = data;
    }

    public static void UpdateClutter()
    {
        if (!ClutterSystem.instance || SystemInfo.graphicsDeviceType is GraphicsDeviceType.Null) return;
        SetClutter();
    }

    private static void ResetClutter()
    {
        if (!ClutterSystem.instance) return;
        foreach (ClutterSystem.Clutter? clutter in ClutterSystem.instance.m_clutter)
        {
            if (!clutter.m_prefab.TryGetComponent(out InstanceRenderer renderer)) continue;
            Material? material = renderer.m_material;
            SetDefaultMaterial(material);
        }
        UpdateClutterSystem();
    }

    private static void SetDefaultMaterial(Material material)
    {
        if (!m_data.TryGetValue(material.name, out ClutterMaterialData data)) return;
        material.color = data.MainColor;
        foreach (var kvp in data.Textures)
        {
            material.SetTexture(kvp.Key, kvp.Value);
        }
    }

    private static void SetClutter()
    {
        if (!ClutterSystem.instance) return;
        foreach (ClutterSystem.Clutter? clutter in ClutterSystem.instance.m_clutter)
        {
            GrassTypes type = GetGrassType(clutter.m_prefab.name);
            if (!clutter.m_prefab.TryGetComponent(out InstanceRenderer renderer)) continue;
            Directories.VegDirectories directory = SeasonUtility.Utils.VegToDirectory(type);
            Material material = renderer.m_material;
            SetDefaultMaterial(material);
            if (SeasonUtility.Utils.ApplyBasedOnAvailable(directory, SeasonalityPlugin._Season.Value, material, "_MainTex"))
            {
                switch (type)
                {
                    case GrassTypes.GreenGrass:
                    case GrassTypes.GreenGrassShort:
                    case GrassTypes.MistlandGrassShort:
                        material.SetTexture(TerrainColorTex, null);
                        break;
                    case GrassTypes.ClutterShrubs:
                    case GrassTypes.Ormbunke:
                    case GrassTypes.OrmBunkeSwamp:
                        switch (SeasonalityPlugin._Season.Value)
                        {
                            case SeasonalityPlugin.Season.Fall:
                                material.color = new Color(0.8f, 0.5f, 0f, 1f);
                                break;
                        }
                        break;
                    
                }
                
            }
            UpdateClutterSystem();
        }
    }

    private static GrassTypes GetGrassType(string clutterName)
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

    private class ClutterMaterialData
    {
        public Dictionary<string, Texture> Textures = new();
        public Color MainColor;
    }
}