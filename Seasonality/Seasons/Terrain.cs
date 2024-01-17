using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;
public static class TerrainPatch
{
    private static readonly Dictionary<string, Texture> clutterTexMap = new();
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int TerrainColorTex = Shader.PropertyToID("_TerrainColorTex");

    public static void UpdateTerrain()
    {
        if (!ClutterSystem.instance) return;
        if (_ModEnabled.Value is Toggle.Off) SetDefaultTerrainSettings();
        else SetTerrainSettings();
        ClutterSystem.instance.m_forceRebuild = true;
        ClutterSystem.instance.LateUpdate();
        foreach (Heightmap? map in Heightmap.Instances) map.m_doLateUpdate = true;
        Heightmap.ForceGenerateAll();
    }
    private static void SetDefaultTerrainSettings()
    {
        if (!ClutterSystem.instance) return;
        foreach (ClutterSystem.Clutter? clutter in ClutterSystem.instance.m_clutter)
        {
            GameObject obj = clutter.m_prefab;
            GrassTypes type = Utils.GetGrassType(obj.name);
            if (!obj.TryGetComponent(out InstanceRenderer instanceRenderer)) continue;

            Material mat = instanceRenderer.m_material;
            mat.color = Color.white;
            Texture? texture = GetDefaultTexture(type);
            
            Texture? grassTerrainColor = clutterTexMap["grass_terrain_color"];

            switch (type)
            {
                case GrassTypes.MistlandGrassShort or GrassTypes.GreenGrass or GrassTypes.GreenGrassShort:
                    if (texture) mat.SetTexture(MainTex, texture);
                    if (grassTerrainColor) mat.SetTexture(TerrainColorTex, grassTerrainColor);
                    break;
                default:
                    if (texture) mat.SetTexture(MainTex, texture);
                    break;
            }
        }
    }

    private static Texture? GetDefaultTexture(GrassTypes type)
    {
        return (type) switch
        {
            GrassTypes.GreenGrass => clutterTexMap["grass_meadows"],
            GrassTypes.GreenGrassShort => clutterTexMap["grass_meadows_short"],
            GrassTypes.ClutterShrubs => clutterTexMap["clutter_shrub"],
            GrassTypes.GroundCover => clutterTexMap["forest_groundcover"],
            GrassTypes.GroundCoverBrown => clutterTexMap["forest_groundcover_brown"],
            GrassTypes.SwampGrass => clutterTexMap["grass_toon1_yellow"],
            GrassTypes.HeathGrass => clutterTexMap["grass_heath"],
            GrassTypes.HeathFlowers => clutterTexMap["grass_heath_redflower"],
            GrassTypes.Ormbunke => clutterTexMap["autumn_ormbunke_green"],
            GrassTypes.OrmBunkeSwamp => clutterTexMap["autumn_ormbunke_swamp"],
            GrassTypes.Vass => clutterTexMap["vass_texture01"],
            GrassTypes.WaterLilies => clutterTexMap["waterlilies"],
            GrassTypes.Rocks => clutterTexMap["rock_low"],
            GrassTypes.RockPlant => Mistlands_Rock_Plant,
            GrassTypes.MistlandGrassShort => MistLands_Moss,
            _ => null
        };
    }
    private static void SetTerrainSettings()
    {
        if (!ClutterSystem.instance) return;

        foreach (ClutterSystem.Clutter? clutter in ClutterSystem.instance.m_clutter)
        {
            GameObject obj = clutter.m_prefab;
            GrassTypes type = Utils.GetGrassType(obj.name);
            if (!obj.TryGetComponent(out InstanceRenderer instanceRenderer)) continue;

            VegDirectories directory = Utils.VegToDirectory(type);
            Material mat = instanceRenderer.m_material;
            string[] props = mat.GetTexturePropertyNames();

            // if (!Utils.FindTexturePropName(props, "terrain", out string terrainProp)) continue;
            if (!Utils.FindTexturePropName(props, "main", out string mainProp)) continue;
            
            // Set texture to default value
            Texture? tex = GetDefaultTexture(type);
            mat.color = Color.white;
            if (tex != null) mat.SetTexture(MainTex, tex);
            
            // If texture file recognized, use it and move on
            switch (type)
            {
                case GrassTypes.GreenGrass or GrassTypes.GreenGrassShort or GrassTypes.MistlandGrassShort:
                    mat.SetTexture(TerrainColorTex, null);
                    Utils.ApplyBasedOnAvailable(directory, _Season.Value, mat, mainProp);
                    break;
                case GrassTypes.ClutterShrubs or GrassTypes.Ormbunke or GrassTypes.OrmBunkeSwamp:
                    Utils.ApplyBasedOnAvailable(directory, _Season.Value, mat, mainProp);
                    switch (_Season.Value)
                    {
                        case Season.Fall:
                            AssignColors(obj, SeasonColors.FallColors);
                            break;
                    }
                    continue;
                case GrassTypes.GroundCover:
                    Utils.ApplyBasedOnAvailable(directory, _Season.Value, mat, mainProp);
                    continue;
                default:
                    Utils.ApplyBasedOnAvailable(directory, _Season.Value, mat, mainProp);
                    break;
            }
        }
    }
    private static void AssignColors(GameObject obj, List<Color> colors)
    {
        List<Action> actions = new();
        foreach (Color color in colors) actions.Add(ApplyColor(obj, color));
        Utils.ApplyRandomly(actions);
    }
    private static Action ApplyColor(GameObject obj, Color color) { return () => ApplyColorToObj(obj, color); }
    private static void ApplyColorToObj(GameObject obj, Color color)
    {
        if (!obj.TryGetComponent(out InstanceRenderer instanceRenderer)) return;
        Material mat = instanceRenderer.m_material;
        mat.color = color;
    }

    [HarmonyPatch(typeof(ClutterSystem), nameof(ClutterSystem.Awake))]
    [HarmonyPriority(Priority.Last)]
    static class ClutterSystemPatch
    {
        private static void Postfix(ClutterSystem __instance)
        {
            if (!__instance) return;
            CacheInitialData(__instance);

            if (_ModEnabled.Value is Toggle.Off) return;
            SetTerrainSettings();
        }
        private static void CacheInitialData(ClutterSystem __instance)
        {
            foreach (ClutterSystem.Clutter? clutter in __instance.m_clutter)
            {
                GameObject obj = clutter.m_prefab;
                if (!obj.TryGetComponent(out InstanceRenderer instanceRenderer)) continue;
                // Save textures to use them later on
                // To revert back to original textures
                Material? mat = instanceRenderer.m_material;
                int[]? IDs = mat.GetTexturePropertyNameIDs();
                foreach (int id in IDs)
                {
                    Texture? tex = mat.GetTexture(id);
                    if (!tex) continue;
                    if (clutterTexMap.ContainsKey(tex.name)) continue;
                    clutterTexMap.Add(tex.name, tex);
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

            if (_ModEnabled.Value is Toggle.Off) { __result = conversionMap[biome]; return true;; }
            switch (biome)
            {
                case Heightmap.Biome.Swamp: __result = conversionMap[Heightmap.Biome.Swamp]; break;
                case Heightmap.Biome.Mountain: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                case Heightmap.Biome.BlackForest:
                    switch (_Season.Value)
                    {
                        case Season.Winter: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                        case Season.Fall: __result = HDPackLoaded ? conversionMap[Heightmap.Biome.BlackForest] : conversionMap[Heightmap.Biome.AshLands]; break;
                        default: __result = conversionMap[Heightmap.Biome.BlackForest]; break;
                    }
                    return false;
                case Heightmap.Biome.Plains:
                    switch (_Season.Value)
                    {
                        case Season.Winter: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                        default: __result = conversionMap[Heightmap.Biome.Plains]; break;
                    }
                    return false;
                case Heightmap.Biome.AshLands: __result = conversionMap[Heightmap.Biome.AshLands]; break;
                case Heightmap.Biome.DeepNorth: __result = conversionMap[Heightmap.Biome.DeepNorth]; break;
                case Heightmap.Biome.Mistlands:
                    switch (_Season.Value)
                    {
                        case Season.Winter: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                        default: __result = conversionMap[Heightmap.Biome.Mistlands]; break;
                    }
                    return false;
                case Heightmap.Biome.Meadows:
                    switch (_Season.Value)
                    {
                        case Season.Fall: __result = conversionMap[Heightmap.Biome.Plains]; break;
                        case Season.Winter: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                        default: __result = conversionMap[Heightmap.Biome.Meadows]; break;
                    }
                    return false;
            }
            return true;
        }
    }

}


// For some reason, texture.name comes up as an empty string sometimes. 
// HarmonyPriority Last seemingly resolved issue
            
// grass_meadows: 14682
// grass_terrain_color: 13692
// clutter_shrub: 12738
// clutter_shrub_n: 12480
// forest_groundcover_brown: 14370
// forest_groundcover 1: 14676
// forest_groundcover: 12850
// forest_groundcover 1: 14676
// grass_toon1_yellow: 12378
// grass_heath: 12502
// grass_meadows_short: 12062
// grass_terrain_color: 13692
// grass_heath_redflower: 13400
// rock_low: 14330
// rock_normal_low: 14602
// autumn_ormbunke_swamp: 13838
// autumn_ormbunke_green_n: 13928
// autumn_ormbunke_green: 12840
// autumn_ormbunke_green_n: 13928
// autumn_ormbunke_green: 12840
// autumn_ormbunke_green_n: 13928
// vass_texture01: 12696
// waterlilies: 13890
// waterlilies_n: 13160
            
// -308 
// -354
// clutter_shrub_n: 12480
// forest_groundcover_brown: 14370
// forest_groundcover 1: 14676
// -324
// forest_groundcover 1: 14676
// grass_toon1_yellow: 12378
// grass_heath: 12502
// -316
// grass_heath_redflower: 13400
// rock_low: 14330
// rock_normal_low: 14602
// autumn_ormbunke_swamp: 13838
// autumn_ormbunke_green_n: 13928
// autumn_ormbunke_green: 12840
// autumn_ormbunke_green_n: 13928
// autumn_ormbunke_green: 12840
// autumn_ormbunke_green_n: 13928
// vass_texture01: 12696
// -346
// waterlilies_n: 13160


// private static void RegisterCustomClutter(ClutterSystem __instance)
// {
//     // Get data from vanilla clutter
//     ClutterSystem.Clutter grass = __instance.m_clutter.Find(x => x.m_name.Contains("green"));
//     if (grass == null) return;
//     // Create custom clutter
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