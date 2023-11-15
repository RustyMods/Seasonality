﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;
using Object = System.Object;

namespace Seasonality.Seasons;

public static class TerrainPatch
{
    private static readonly List<Texture> clutterTextures = new();

    public static void UpdateTerrain()
    {
        SetTerrainSettings();
        ClutterSystem.instance.m_forceRebuild = true;
        ClutterSystem.instance.LateUpdate();
    }    
    
    private static void SetTerrainSettings()
    {
        if (!ClutterSystem.instance) return;

        foreach (ClutterSystem.Clutter? clutter in ClutterSystem.instance.m_clutter)
        {
            GameObject obj = clutter.m_prefab;
            GrassTypes type = Utils.GetGrassType(obj.name);
            obj.TryGetComponent(out InstanceRenderer instanceRenderer);
            if (!instanceRenderer) continue;

            VegDirectories directory = Utils.VegToDirectory(type);
            Material mat = instanceRenderer.m_material;
            string[] props = mat.GetTexturePropertyNames();
            if (!Utils.FindTexturePropName(props, "terrain", out string property)) continue;
            if (Utils.ApplyBasedOnAvailable(directory, _Season.Value, mat, property)) continue; // If texture file recognized, use it and move on

            switch (_Season.Value)
            {
                case Season.Fall:
                    switch (type)
                    {
                        case GrassTypes.GreenGrass or GrassTypes.HeathGrass:
                            Texture? grass_heath = clutterTextures.Find(x => x.name == "grass_heath");
                            if (grass_heath) mat.SetTexture(property, grass_heath);
                            break;
                        case GrassTypes.Shrubs:
                            AssignColors(obj, new List<Color>(){_FallColor1.Value, _FallColor2.Value}, type);
                            break;
                        case GrassTypes.HeathFlowers:
                            Texture? grass_heath_redflower = clutterTextures.Find(x => x.name == "grass_heath_redflower");
                            if (grass_heath_redflower) mat.SetTexture(property, grass_heath_redflower);
                            break;
                        case GrassTypes.GroundCover:
                            Texture? forest_groundcover = clutterTextures.Find(x => x.name == "forest_groundcover 1"); // redish forest ground cover
                            if (forest_groundcover) mat.SetTexture(property, forest_groundcover);
                            break;
                        case GrassTypes.Ormbunke:
                            mat.color = _FallColor1.Value;
                            break;
                        case GrassTypes.None:
                            break;
                    }
                    break;
                case Season.Spring:
                    switch (type)
                    {
                        case GrassTypes.GroundCover:
                            Texture? forest_groundcover = clutterTextures.Find(x => x.name == "forest_groundcover 1");
                            if (forest_groundcover)
                            {
                                mat.SetTexture(property, forest_groundcover);
                                mat.color = new Color(0.5f, 0.6f, 0f, 1f);
                            }
                            break;
                        case GrassTypes.HeathGrass:
                            Texture? grass_heath = clutterTextures.Find(x => x.name == "grass_heath");
                            if (grass_heath) mat.SetTexture(property, grass_heath);
                            break;
                        case GrassTypes.GreenGrass:
                            Texture? originalTex = clutterTextures.Find(x => x.name == "grass_terrain_color");
                            mat.SetTexture(property, originalTex );
                            break;
                        case GrassTypes.Shrubs:
                            AssignColors(obj, new List<Color>(){new (1f, 1f, 1f, 0.8f)}, type);
                            break;
                        case GrassTypes.None:
                            break;
                    }
                    break;
                case Season.Summer:
                    switch (type)
                    {
                        case GrassTypes.Ormbunke:
                            mat.color = Color.white;
                            break;
                        case GrassTypes.HeathGrass:
                            mat.SetTexture(property, SnowTexture);
                            break;
                        case GrassTypes.GroundCover:
                            Texture? forest_groundcover = clutterTextures.Find(x => x.name == "forest_groundcover 1");
                            if (forest_groundcover) mat.SetTexture(property, forest_groundcover);
                            break;
                        case GrassTypes.GreenGrass:
                            Texture? originalTex = clutterTextures.Find(x => x.name == "grass_terrain_color");
                            if (originalTex) mat.SetTexture(property, originalTex);
                            break;
                        case GrassTypes.Shrubs:
                            Texture? clutter_shrub = clutterTextures.Find(x => x.name == "clutter_shrub");
                            if (clutter_shrub) mat.SetTexture(property, clutter_shrub);
                            mat.color = Color.white;
                            break;
                        case GrassTypes.None:
                            break;
                    }
                    break;
                case Season.Winter:
                    switch (type)
                    {
                        case GrassTypes.GreenGrass or GrassTypes.HeathGrass:
                            mat.SetTexture(property, SnowTexture);
                            break;
                        case GrassTypes.GroundCover:
                            Texture? forest_groundcover = clutterTextures.Find(x => x.name == "forest_groundcover 1");
                            if (forest_groundcover) mat.SetTexture(property, forest_groundcover);
                            break;
                        case GrassTypes.Shrubs:
                            AssignColors(obj, new List<Color>(){new Color(0.8f, 0.8f, 0.8f, 1f)}, type);
                            break;
                        case GrassTypes.None:
                            break;
                    }
                    break;
            }
        }
    }

    private static void AssignColors(GameObject obj, List<Color> colors, GrassTypes type)
    {
        switch (type)
        {
            default:
                List<Action> actions = new();
                foreach (Color color in colors)
                {
                    actions.Add(ApplyColor(obj, color));
                }
                Utils.ApplyRandomly(actions);
                break;
        }
    }

    private static Action ApplyColor(GameObject obj, Color color)
    {
        return () => ApplyColorToObj(obj, color);
    }

    private static void ApplyColorToObj(GameObject obj, Color color)
    {
        obj.TryGetComponent(out InstanceRenderer instanceRenderer);
        if (!instanceRenderer) return;
        Material mat = instanceRenderer.m_material;
        mat.color = color;
    }

    [HarmonyPatch(typeof(ClutterSystem), nameof(ClutterSystem.Awake))]
    static class ClutterSystemPatch
    {
        private static void Postfix(ClutterSystem __instance)
        {
            if (!__instance) return;
            CacheInitialData(__instance);
            if (_ModEnabled.Value is Toggle.Off) return;
            // ClutterSystem.Clutter grass = __instance.m_clutter.Find(x => x.m_name.Contains("green"));
            // if (grass != null)
            // {
            //     ClutterSystem.Clutter MountainGrass = new ClutterSystem.Clutter();
            //     MountainGrass.m_name = "mountain_grass";
            //     MountainGrass.m_enabled = true;
            //     MountainGrass.m_biome = Heightmap.Biome.Mountain;
            //     MountainGrass.m_instanced = true;
            //     MountainGrass.m_prefab = grass.m_prefab;
            //     MountainGrass.m_amount = 80;
            //     MountainGrass.m_onUncleared = grass.m_onUncleared;
            //     MountainGrass.m_onCleared = grass.m_onCleared;
            //     MountainGrass.m_minVegetation = grass.m_minVegetation;
            //     MountainGrass.m_maxVegetation = grass.m_maxVegetation;
            //     MountainGrass.m_scaleMin = 1f;
            //     MountainGrass.m_scaleMax = 1f;
            //     MountainGrass.m_maxTilt = grass.m_maxTilt;
            //     MountainGrass.m_minTilt = grass.m_minTilt;
            //     MountainGrass.m_minAlt = grass.m_minAlt;
            //     MountainGrass.m_snapToWater = false;
            //     MountainGrass.m_terrainTilt = true;
            //     MountainGrass.m_randomOffset = grass.m_randomOffset;
            //     MountainGrass.m_minOceanDepth = grass.m_minOceanDepth;
            //     MountainGrass.m_maxOceanDepth = grass.m_maxOceanDepth;
            //     MountainGrass.m_inForest = true;
            //     MountainGrass.m_forestTresholdMin = grass.m_forestTresholdMin;
            //     MountainGrass.m_forestTresholdMax = grass.m_forestTresholdMax;
            //     MountainGrass.m_fractalScale = grass.m_fractalScale;
            //     MountainGrass.m_fractalOffset = grass.m_fractalOffset;
            //     MountainGrass.m_fractalTresholdMin = grass.m_fractalTresholdMin;
            //     MountainGrass.m_fractalTresholdMax = grass.m_fractalTresholdMax;
            //     
            //     __instance.m_clutter.Add(MountainGrass);
            // }
            SetTerrainSettings();
        }

        private static void CacheInitialData(ClutterSystem __instance)
        {
            foreach (ClutterSystem.Clutter? clutter in __instance.m_clutter)
            {
                GameObject obj = clutter.m_prefab;
                obj.TryGetComponent(out InstanceRenderer instanceRenderer);
                if (!instanceRenderer) continue;
            
                // Save textures to use them later on
                // To revert back to original textures
                Material mat = instanceRenderer.m_material;
                int[]? IDs = mat.GetTexturePropertyNameIDs();
                foreach (int id in IDs)
                {
                    Texture? tex = mat.GetTexture(id);
                    if (clutterTextures.Contains(tex)) continue;
                    if (tex) clutterTextures.Add(tex);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.GetBiomeColor), new[] { typeof(Heightmap.Biome)})]
    static class GetBiomeColorPatch
    {
        private static bool Prefix(Heightmap.Biome biome, ref Color32 __result)
        {
            Dictionary<Heightmap.Biome, Color32> conversionMap = new()
            {
                { Heightmap.Biome.Meadows , new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0) },
                { Heightmap.Biome.BlackForest , new Color32((byte) 0, (byte) 0, byte.MaxValue, (byte) 0) },
                { Heightmap.Biome.Swamp , new Color32(byte.MaxValue, (byte) 0, (byte) 0, (byte) 0) },
                { Heightmap.Biome.Mountain , new Color32((byte) 0, byte.MaxValue, (byte) 0, (byte) 0) },
                { Heightmap.Biome.Plains , new Color32((byte) 0, (byte) 0, (byte) 0, byte.MaxValue) },
                { Heightmap.Biome.Mistlands , new Color32((byte) 0, (byte) 0, byte.MaxValue, byte.MaxValue) },
                { Heightmap.Biome.DeepNorth , new Color32((byte) 0, byte.MaxValue, (byte) 0, (byte) 0) },
                { Heightmap.Biome.AshLands , new Color32(byte.MaxValue, (byte) 0, (byte) 0, byte.MaxValue) },
                { Heightmap.Biome.Ocean , new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0) },
            };

            if (_ModEnabled.Value is Toggle.Off)
            {
                __result = conversionMap[biome];
                return false;
            }
            switch (biome)
            {
                case Heightmap.Biome.Swamp:
                    __result = conversionMap[Heightmap.Biome.Swamp];
                    break;
                case Heightmap.Biome.Mountain:
                    switch (_Season.Value)
                    {
                        default:
                            __result = conversionMap[Heightmap.Biome.Mountain];
                            break;
                    }
                    break;
                case Heightmap.Biome.BlackForest:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            __result = conversionMap[Heightmap.Biome.Mountain];
                            break;
                        case Season.Fall:
                            __result = conversionMap[Heightmap.Biome.AshLands];
                            break;
                        default:
                            __result = conversionMap[Heightmap.Biome.BlackForest];
                            break;
                    }
                    break;
                case Heightmap.Biome.Plains:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            __result = conversionMap[Heightmap.Biome.Mountain];
                            break;
                        default:
                            __result = conversionMap[Heightmap.Biome.Plains];
                            break;
                    }
                    break;
                case Heightmap.Biome.AshLands:
                    __result = conversionMap[Heightmap.Biome.AshLands];
                    break;
                case Heightmap.Biome.DeepNorth:
                    __result = conversionMap[Heightmap.Biome.DeepNorth];
                    break;
                case Heightmap.Biome.Mistlands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            __result = conversionMap[Heightmap.Biome.Mountain];
                            break;
                        default:
                            __result = conversionMap[Heightmap.Biome.Mistlands];
                            break;
                    }
                    break;
                case Heightmap.Biome.Meadows:
                    switch (_Season.Value)
                    {
                        case Season.Fall:
                            __result = conversionMap[Heightmap.Biome.Plains];
                            break;
                        case Season.Winter:
                            __result = conversionMap[Heightmap.Biome.Mountain];
                            break;
                        default:
                            __result = conversionMap[Heightmap.Biome.Meadows];
                            break;
                    }
                    break;
                default:
                    __result = conversionMap[Heightmap.Biome.Meadows];
                    break;
            }
            return false;
        }
    }
    
}