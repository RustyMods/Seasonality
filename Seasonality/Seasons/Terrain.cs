using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;
public static class TerrainPatch
{
    private static readonly List<Texture> clutterTextures = new();
    public static void UpdateTerrain()
    {
        if (!ClutterSystem.instance) return;
        if (_ModEnabled.Value is Toggle.Off) SetDefaultTerrainSettings();
        else SetTerrainSettings();
        ClutterSystem.instance.m_forceRebuild = true;
        ClutterSystem.instance.LateUpdate();
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
            string[] props = mat.GetTexturePropertyNames();
            if (!Utils.FindTexturePropName(props, "terrain", out string property)) continue;

            Texture? texture = (type) switch
            {
                GrassTypes.GreenGrass => clutterTextures.Find(x => x.name == "grass_terrain_color"),
                GrassTypes.GreenGrassShort => clutterTextures.Find(x => x.name == "grass_terrain_color"),
                GrassTypes.ClutterShrubs => clutterTextures.Find(x => x.name == "clutter_shrub"),
                GrassTypes.GroundCover => clutterTextures.Find(x => x.name == "forest_groundcover"),
                GrassTypes.GroundCoverBrown => clutterTextures.Find(x => x.name == "forest_groundcover_brown"),
                GrassTypes.SwampGrass => clutterTextures.Find(x => x.name == "grass_toon1_yellow"),
                GrassTypes.HeathGrass => clutterTextures.Find(x => x.name == "grass_heath"),
                GrassTypes.HeathFlowers => clutterTextures.Find(x => x.name == "grass_heath_redflower"),
                GrassTypes.Ormbunke => clutterTextures.Find(x => x.name == "autumn_ormbunke_green"),
                GrassTypes.OrmBunkeSwamp => clutterTextures.Find(x => x.name == "autumn_ormbunke_swamp"),
                GrassTypes.Vass => clutterTextures.Find(x => x.name == "vass_texture01"),
                GrassTypes.WaterLilies => clutterTextures.Find(x => x.name == "waterlilies"),
                GrassTypes.RockPlant => clutterTextures.Find(x => x.name == "MistlandsVegetation_d"),
                GrassTypes.Rocks => clutterTextures.Find(x => x.name == "rock_low"),
                GrassTypes.MistlandGrassShort => clutterTextures.Find(x => x.name == "mistlands_moss"),
                _ => null
            };
            if (texture) mat.SetTexture(property, texture);
        }
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
            if (!Utils.FindTexturePropName(props, "terrain", out string terrainProp)) continue;
            if (!Utils.FindTexturePropName(props, "main", out string mainProp)) continue;
            // If texture file recognized, use it and move on
            
            // Failure in getting correct textures.
            // Black grass caused by using wrong texture
            switch (type)
            {
                case GrassTypes.GreenGrass or GrassTypes.GreenGrassShort:
                    if (Utils.ApplyBasedOnAvailable(directory, _Season.Value, mat, terrainProp)) continue;
                    break;
                default:
                    if (Utils.ApplyBasedOnAvailable(directory, _Season.Value, mat, mainProp)) continue;
                    break;
            }
            // Else use plugin settings
            // Set texture to default value
            Texture? tex = (type) switch
            {
                GrassTypes.GreenGrass => clutterTextures.Find(x => x.name == "grass_terrain_color"),
                GrassTypes.GreenGrassShort => clutterTextures.Find(x => x.name == "grass_terrain_color"),
                GrassTypes.ClutterShrubs => clutterTextures.Find(x => x.name == "clutter_shrub"),
                GrassTypes.GroundCover => clutterTextures.Find(x => x.name == "forest_groundcover"),
                GrassTypes.GroundCoverBrown => clutterTextures.Find(x => x.name == "forest_groundcover_brown"),
                GrassTypes.SwampGrass => clutterTextures.Find(x => x.name == "grass_toon1_yellow"),
                GrassTypes.HeathGrass => clutterTextures.Find(x => x.name == "grass_heath"),
                GrassTypes.HeathFlowers => clutterTextures.Find(x => x.name == "grass_heath_redflower"),
                GrassTypes.Ormbunke => clutterTextures.Find(x => x.name == "autumn_ormbunke_green"),
                GrassTypes.OrmBunkeSwamp => clutterTextures.Find(x => x.name == "autumn_ormbunke_swamp"),
                GrassTypes.Vass => clutterTextures.Find(x => x.name == "vass_texture01"),
                GrassTypes.WaterLilies => clutterTextures.Find(x => x.name == "waterlilies"),
                GrassTypes.RockPlant => clutterTextures.Find(x => x.name == "MistlandsVegetation_d"),
                GrassTypes.Rocks => clutterTextures.Find(x => x.name == "rock_low"),
                GrassTypes.MistlandGrassShort => clutterTextures.Find(x => x.name == "mistlands_moss"),
                _ => null
            };
            mat.color = Color.white;
            // Set texture and color depending on season
            switch (_Season.Value)
            {
                case Season.Fall:
                    switch (type)
                    {
                        case GrassTypes.GreenGrass or GrassTypes.GreenGrassShort:
                            tex = GrassTerrainFall;
                            break;
                        case GrassTypes.ClutterShrubs:
                            tex = ClutterShrub_Winter;
                            AssignColors(obj, new List<Color>(){_FallColor1.Value, _FallColor2.Value, _FallColor3.Value}, type);
                            break;
                        case GrassTypes.GroundCover:
                            tex = clutterTextures.Find(x => x.name == "forest_groundcover_brown"); // redish forest ground cover
                            break;
                        case GrassTypes.Ormbunke:
                            tex = Ormbunke_Winter;
                            AssignColors(obj, new List<Color>(){_FallColor1.Value, _FallColor2.Value, _FallColor3.Value}, type);
                            break;
                        case GrassTypes.Vass:
                            tex = Vass_Fall;
                            break;
                        case GrassTypes.WaterLilies:
                            tex = WaterLilies_Fall;
                            break;
                    }
                    break;
                case Season.Spring:
                    switch (type)
                    {
                        case GrassTypes.GroundCover:
                            tex = clutterTextures.Find(x => x.name == "forest_groundcover_brown");
                            mat.color = new Color(0.5f, 0.6f, 0f, 1f);
                            break;
                        case GrassTypes.WaterLilies:
                            tex = WaterLilies_Spring;
                            break;
                    }
                    break;
                case Season.Summer:
                    switch (type)
                    {
                        case GrassTypes.GroundCover:
                            tex = clutterTextures.Find(x => x.name == "forest_groundcover_brown");
                            break;
                    }
                    break;
                case Season.Winter:
                    switch (type)
                    {
                        case GrassTypes.GreenGrass or GrassTypes.GreenGrassShort:
                            tex = SnowTexture;
                            break;
                        case GrassTypes.GroundCover:
                            tex = clutterTextures.Find(x => x.name == "forest_groundcover_brown");
                            break;
                        case GrassTypes.ClutterShrubs:
                            tex = ClutterShrub_Winter;
                            break;
                        case GrassTypes.Ormbunke:
                            tex = Ormbunke_Winter;
                            break;
                        case GrassTypes.Vass:
                            tex = Vass_Winter;
                            break;
                        case GrassTypes.WaterLilies:
                            tex = WaterLilies_Winter;
                            break;
                    }   
                    break;
            }

            if (tex != null)
            {
                switch (type)
                {
                    case GrassTypes.GreenGrassShort or GrassTypes.GreenGrass:
                        mat.SetTexture(terrainProp, tex);
                        break;
                    default:
                        mat.SetTexture(mainProp, tex);
                        break;
                }
            }
        }
    }
    private static void AssignColors(GameObject obj, List<Color> colors, GrassTypes type)
    {
        List<Action> actions = new();
        foreach (Color color in colors) actions.Add(ApplyColor(obj, color));
        Utils.ApplyRandomly(actions);
    }
    private static Action ApplyColor(GameObject obj, Color color) { return () => ApplyColorToObj(obj, color); }
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
            SetTerrainSettings();
        }
        private static void RegisterCustomClutter(ClutterSystem __instance)
        {
            // Get data from vanilla clutter
            ClutterSystem.Clutter grass = __instance.m_clutter.Find(x => x.m_name.Contains("green"));
            if (grass == null) return;
            // Create custom clutter
            ClutterSystem.Clutter MountainGrass = new ClutterSystem.Clutter();
            MountainGrass.m_name = "mountain_grass";
            MountainGrass.m_enabled = true;
            MountainGrass.m_biome = Heightmap.Biome.Mountain;
            MountainGrass.m_instanced = true;
            MountainGrass.m_prefab = grass.m_prefab;
            MountainGrass.m_amount = 80;
            MountainGrass.m_onUncleared = grass.m_onUncleared;
            MountainGrass.m_onCleared = grass.m_onCleared;
            MountainGrass.m_minVegetation = grass.m_minVegetation;
            MountainGrass.m_maxVegetation = grass.m_maxVegetation;
            MountainGrass.m_scaleMin = 1f;
            MountainGrass.m_scaleMax = 1f;
            MountainGrass.m_maxTilt = grass.m_maxTilt;
            MountainGrass.m_minTilt = grass.m_minTilt;
            MountainGrass.m_minAlt = grass.m_minAlt;
            MountainGrass.m_snapToWater = false;
            MountainGrass.m_terrainTilt = true;
            MountainGrass.m_randomOffset = grass.m_randomOffset;
            MountainGrass.m_minOceanDepth = grass.m_minOceanDepth;
            MountainGrass.m_maxOceanDepth = grass.m_maxOceanDepth;
            MountainGrass.m_inForest = true;
            MountainGrass.m_forestTresholdMin = grass.m_forestTresholdMin;
            MountainGrass.m_forestTresholdMax = grass.m_forestTresholdMax;
            MountainGrass.m_fractalScale = grass.m_fractalScale;
            MountainGrass.m_fractalOffset = grass.m_fractalOffset;
            MountainGrass.m_fractalTresholdMin = grass.m_fractalTresholdMin;
            MountainGrass.m_fractalTresholdMax = grass.m_fractalTresholdMax;
                
            __instance.m_clutter.Add(MountainGrass);
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

            if (_ModEnabled.Value is Toggle.Off) { __result = conversionMap[biome]; return false; }
            switch (biome)
            {
                case Heightmap.Biome.Swamp: __result = conversionMap[Heightmap.Biome.Swamp]; break;
                case Heightmap.Biome.Mountain: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                case Heightmap.Biome.BlackForest:
                    switch (_Season.Value)
                    {
                        case Season.Winter: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                        case Season.Fall: __result = conversionMap[Heightmap.Biome.AshLands]; break;
                        default: __result = conversionMap[Heightmap.Biome.BlackForest]; break;
                    }
                    break;
                case Heightmap.Biome.Plains:
                    switch (_Season.Value)
                    {
                        case Season.Winter: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                        default: __result = conversionMap[Heightmap.Biome.Plains]; break;
                    }
                    break;
                case Heightmap.Biome.AshLands: __result = conversionMap[Heightmap.Biome.AshLands]; break;
                case Heightmap.Biome.DeepNorth: __result = conversionMap[Heightmap.Biome.DeepNorth]; break;
                case Heightmap.Biome.Mistlands:
                    switch (_Season.Value)
                    {
                        case Season.Winter: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                        default: __result = conversionMap[Heightmap.Biome.Mistlands]; break;
                    }
                    break;
                case Heightmap.Biome.Meadows:
                    switch (_Season.Value)
                    {
                        case Season.Fall: __result = conversionMap[Heightmap.Biome.Plains]; break;
                        case Season.Winter: __result = conversionMap[Heightmap.Biome.Mountain]; break;
                        default: __result = conversionMap[Heightmap.Biome.Meadows]; break;
                    }
                    break;
                default: __result = conversionMap[Heightmap.Biome.Meadows]; break;
            }
            return false;
        }
    }

}