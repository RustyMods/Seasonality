using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Seasonality.Helpers;
using UnityEngine;

namespace Seasonality.Textures;

public static class MaterialController
{
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");
    private static readonly int MossColor = Shader.PropertyToID("_MossColor");
    private static readonly List<string> m_shaderToIgnore = new()
    {
        "Hidden/BlitCopy",
        "Hidden/Interal - Flare",
        "Hidden/Internal-DeferredReflections",
        "UI/Default",
        "Sprites/Mask",
        "Hidden/InternalErroShader",
        "Sprites/Default",
        "TextMeshPro/Distance Field",
        "GUI/Text Shader",
        "TextMeshPro/Sprite",
        "Custom/GuiScroll",
        "UI/Heat Distort",
    };
    private static int m_referenceCount;
    public static readonly Dictionary<string, Material> m_allMaterials = new();
    public static readonly Dictionary<string, MaterialData> m_materials = new();
    public static readonly Dictionary<string, Texture> m_mossTextures = new();
    private static readonly Dictionary<string, SpecialCase> m_cases = new();
    public static readonly Dictionary<string, List<MaterialData>> m_fallMaterials = new();

    private static void Setup()
    {
        SpecialCase MistLandGrassShort = new("grasscross_mistlands_short", material =>
        {
            foreach (KeyValuePair<Configs.Season, Texture> kvp in material.m_mainTextures)
            {
                material.m_mainColors[kvp.Key] = new Color32(255, 255, 255, 255);
                material.m_updateColors = true;
                material.m_specialTextures.AddOrSetNull("_TerrainColorTex", kvp.Key, null);
            }
            return true;
        });

        SpecialCase GrassCrossMeadows = new("grasscross_meadows", material =>
        {
            foreach (KeyValuePair<Configs.Season, Texture> texture in material.m_mainTextures)
            {
                material.m_specialTextures.AddOrSetNull("_TerrainColorTex", texture.Key, null);
            }
            return true;
        });

        SpecialCase GrassCrossMeadowsShort = new("grasscross_meadows_short", material =>
        {
            foreach (var texture in material.m_mainTextures)
            {
                material.m_specialTextures.AddOrSetNull("_TerrainColorTex", texture.Key, null);
            }
            return true;
        });

        SpecialCase HelmetTrollLeather = new("helmet_trollleather", material =>
        {
            foreach (var texture in material.m_mainTextures)
            {
                material.m_mainColors[texture.Key] = new Color32(255, 255, 255, 255);
                material.m_updateColors = true;
            }
            return true;
        });

        SpecialCase RockMistLand = new("rock_mistlands", material =>
        {
            material.m_originalMossColor = new Color32(202, 255, 121, 255);
            material.m_newMossColor = new Color32(255, 255, 255, 255);
            material.m_updateMossColor = true;
            return true;
        });

        SpecialCase MistLandCliff = new("mistlands_cliff", material =>
        {
            material.m_originalMossColor = new Color32(202, 255, 121, 255);
            material.m_newMossColor = new Color32(255, 255, 255, 255);
            material.m_updateMossColor = true;
            return true;
        });

        SpecialCase MistLandCliffInternal = new("mistlands_cliff_internal", material =>
        {
            material.m_originalMossColor = new Color32(202, 255, 121, 255);
            material.m_newMossColor = new Color32(255, 255, 255, 255);
            material.m_updateMossColor = true;
            return true;
        });

        SpecialCase GiantRustSword = new("GiantRustSword_mat", material =>
        {
            material.m_originalMossColor = new Color32(202, 255, 121, 255);
            material.m_newMossColor = new Color32(255, 255, 255, 255);
            material.m_updateMossColor = true;
            return true;
        });

        SpecialCase GiantRustHelm = new("GiantRustHelm_mat", material =>
        {
            material.m_originalMossColor = new Color32(202, 255, 121, 255);
            material.m_newMossColor = new Color32(255, 255, 255, 255);
            material.m_updateMossColor = true;
            return true;
        });

        SpecialCase GiantRustParticle = new("GiantRust_particle", material =>
        {
            material.m_originalMossColor = new Color32(202, 255, 121, 255);
            material.m_newMossColor = new Color32(255, 255, 255, 255);
            material.m_updateMossColor = true;
            return true;
        });

        SpecialCase GiantSkeletonExterior = new("giant_skeleton_exterior", material =>
        {
            material.m_originalMossColor = new Color32(202, 255, 121, 255);
            material.m_newMossColor = new Color32(255, 255, 255, 255);
            material.m_updateMossColor = true;
            return true;
        });

        SpecialCase YggdrasilRoot = new("yggdrasil_root", material =>
        {
            material.m_originalMossColor = new Color32(202, 255, 121, 255);
            material.m_newMossColor = new Color32(255, 255, 255, 255);
            material.m_updateMossColor = true;
            return true;
        });

        SpecialCase CapeTrollHide = new("CapeTrollHide", material =>
        {
            foreach (var texture in material.m_mainTextures)
            {
                material.m_mainColors[texture.Key] = new Color32(255, 255, 255, 255);
                material.m_updateColors = true;
            }
            return true;
        });
        
        SpecialCase InstanceShrub = new("clutter_shrub", material =>
        {
            material.m_mainColors[Configs.Season.Fall] = new Color32(205, 92, 92, 255);
            material.m_updateColors = true;
            return true;
        });
        
        SpecialCase InstancedOrmbunke = new("ormbunke", material =>
        {
            material.m_mainColors[Configs.Season.Fall] = new Color32(205, 92, 92, 255);
            material.m_updateColors = true;
            return true;
        });
        
        SpecialCase InstancedOrmbunkeYellow = new("ormbunke_yellow", material =>
        {
            material.m_mainColors[Configs.Season.Fall] = new Color32(205, 92, 92, 255);
            material.m_updateColors = true;
            return true;
        });
    }

    private static void SetupFallMaterials()
    {
        List<string> materialsToClone = new()
        {
            "beech_leaf", "beech_particle", "beech_leaf_small", "birch_leaf", "birch_leaf_aut", "oak_leaf",
            "Shoot_Leaf_mat", "leaf", "birch_particle", "oak_particle", "shoot_leaf_particle"
        };
        List<Color32> m_colors = new()
         {
             new Color32(205, 92, 92, 255),   // Indian Red
             new Color32(255, 87, 51, 255),   // Pumpkin Orange
             new Color32(218, 165, 32, 255),  // Goldenrod
             new Color32(139, 69, 19, 255),   // Saddle Brown
             new Color32(255, 140, 0, 255),   // Dark Orange
             new Color32(184, 134, 11, 255),  // Dark Goldenrod
             new Color32(153, 101, 21, 255),  // Light Brown
             new Color32(233, 116, 81, 255),  // Autumn Leaf Red
             new Color32(244, 164, 96, 255),  // Sandy Brown
             new Color32(255, 99, 71, 255)    // Tomato Red
         };
        
        foreach (var material in materialsToClone)
        {
            if (!m_materials.TryGetValue(material, out MaterialData original)) continue;
            List<MaterialData> list = new();
            for (var index = 0; index < m_colors.Count; index++)
            {
                var color = m_colors[index];
                string name = $"{original.m_name}_{index}";
                var clone = original.Clone(name);
                clone.m_mainColors[Configs.Season.Fall] = color;
                clone.m_updateColors = true;
                list.Add(clone);
            }
            m_fallMaterials[original.m_name] = list;
        }
    }

    public static void UpdateAll()
    {
        foreach (var material in m_materials) material.Value.Update();
        foreach (var material in m_fallMaterials)
        {
            foreach (var mat in material.Value)
            {
                mat.Update();
            }
        }
    }

    private static int Load(bool clear = false)
    {
        if (clear)
        {
            m_materials.Clear();
            m_allMaterials.Clear();
            m_mossTextures.Clear();
        }
        m_referenceCount = m_materials.Count;
        var materials = Resources.FindObjectsOfTypeAll<Material>();
        foreach (Material? mat in materials)
        {
            if (mat == null) continue;
            if (!m_allMaterials.ContainsKey(mat.name)) m_allMaterials[mat.name] = mat;
            if (m_materials.ContainsKey(mat.name)) continue;
            if (!mat.HasProperty("_MainTex")) continue;
            if (TextureManager.m_texturePacks.TryGetValue(mat.name, out TextureManager.TexturePack texturePack))
            {
                var data = new MaterialData(mat, texturePack);
                data.ApplySpecialCases();
                if (!data.m_isValid) continue;
                m_materials[mat.name] = data;
                SeasonalityPlugin.Record.LogSuccess($"Registered textures to: {data.m_name}");
            }
            else
            {
                if (!mat.HasProperty(MossTex)) continue;
                var data = new MaterialData(mat);
                data.ApplySpecialCases();
                if (!data.m_isValid) continue;
                m_materials[mat.name] = data;
                SeasonalityPlugin.Record.LogSuccess($"Registered moss material: {data.m_name}");
                if (data.m_originalMossTex == null) continue;
                if (!m_mossTextures.ContainsKey(data.m_originalMossTex.name)) continue;
                m_mossTextures[data.m_originalMossTex.name] = data.m_originalMossTex;
            }
        }

        foreach (var materialName in TextureManager.m_texturePacks.Keys)
        {
            if (m_materials.ContainsKey(materialName)) continue;
            SeasonalityPlugin.Record.LogDebug($"Failed to find material: {materialName}");
        }
        var difference = m_materials.Count - m_referenceCount;
        return difference;
    }
    
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    private static class FejdStartup_Awake_Patch
    {
        private static void Postfix()
        {
            Setup();
            if (SeasonalityPlugin.BadgerHDLoaded) return;
            Stopwatch watch = Stopwatch.StartNew();
            var count = Load();
            watch.Stop();
            SeasonalityPlugin.Record.LogInfo($"[FejdStartup Awake] Generation textures time: {watch.ElapsedMilliseconds}ms");
            SeasonalityPlugin.Record.LogDebug($"[FejdStartup Awake]: Registered {count} new materials");
            SeasonalityPlugin.OnSeasonChange();
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
    private static class ZoneSystem_SetupLocations_Patch
    {
        private static void Postfix()
        {
            if (SeasonalityPlugin.BadgerHDLoaded) return;
            Stopwatch watch = Stopwatch.StartNew();
            var count = Load();
            watch.Stop();
            SeasonalityPlugin.Record.LogInfo($"[ZoneSystem SetupLocations] Generation textures time: {watch.ElapsedMilliseconds}ms");
            SetupFallMaterials();
            SeasonalityPlugin.Record.LogDebug($"[ZoneSystem SetupLocations]: Registered {count} new materials");
            SeasonalityPlugin.OnSeasonChange();
        }
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    public static class ZNetScene_Awake_Patch
    {
        [HarmonyAfter(new []{"Badgers.HDValheimTextures"})]
        private static void Postfix()
        {
            Stopwatch watch = Stopwatch.StartNew();
            var count = Load();
            watch.Stop();
            SeasonalityPlugin.Record.LogInfo($"[ZNetScene Awake] Generation textures time: {watch.ElapsedMilliseconds}ms");
            SeasonalityPlugin.Record.LogDebug($"[ZNetScene Awake]: Registered {count} new materials");
            SeasonalityPlugin.OnSeasonChange();
        }
    }

    public class MaterialData
    {
        public string m_name;
        public string m_shaderName;
        public Material m_material; // Can become null
        public Texture? m_originalTex;
        public Color32 m_originalColor;
        public Texture? m_originalMossTex;
        public Dictionary<string, Texture> m_originalTextures = new();
        public Dictionary<Configs.Season, Texture> m_mainTextures = new();
        public Dictionary<Configs.Season, Color32> m_mainColors = new();
        public Dictionary<string, Dictionary<Configs.Season, Texture?>> m_specialTextures = new();
        public Color32 m_originalMossColor;
        public Color32 m_newMossColor;
        public bool m_updateColors;
        public bool m_updateMoss;
        public bool m_updateMossColor;
        public bool m_isAshlandMaterial;
        public bool m_isValid = true;

        public MaterialData(Material material)
        {
            m_shaderName = material.shader.name;
            m_name = material.name;
            m_material = material;
            if (m_shaderToIgnore.Contains(m_shaderName))
            {
                m_isValid = false;
                return;
            }
            m_originalTex = material.mainTexture;
            if (material.HasProperty("_Color")) m_originalColor = material.color;
            CacheOriginalTextures();
            CacheMoss();
        }

        private void CacheMoss()
        {
            if (!m_material.HasProperty(MossTex)) return;
            m_originalMossTex = m_material.GetTexture(MossTex);
            if (m_originalMossTex == null) return;
            m_updateMoss = true;
            if (m_originalMossTex.name == "Ash_d") m_isAshlandMaterial = true;
        }

        private void CacheOriginalTextures()
        {
            foreach (var property in m_material.GetTexturePropertyNames())
            {
                if (m_material.GetTexture(property) is {} tex)
                {
                    m_originalTextures[property] = tex;
                }
            }
        }

        public void ApplySpecialCases()
        {
            if (!m_cases.TryGetValue(m_name, out SpecialCase specialCase)) return;
            specialCase.Run(this);
        }

        private static Texture? Create(Texture2D original, byte[] data)
        {
            Texture2D newTexture = new Texture2D(original.width, original.height, original.format, original.mipmapCount > 1)
            {
                anisoLevel = original.anisoLevel,
                filterMode = original.filterMode,
                mipMapBias = original.mipMapBias,
                wrapMode = original.wrapMode,
                wrapModeU = original.wrapModeU,
                wrapModeV = original.wrapModeV,
                wrapModeW = original.wrapModeW
            };
            if (!newTexture.LoadImage(data)) return null;
            newTexture.Apply();
            return newTexture;
        }

        public MaterialData(Material material, TextureManager.TexturePack package) : this(material)
        {
            foreach (TextureManager.ImageData? data in package.m_images)
            {
                if (!data.m_property.IsNullOrWhiteSpace())
                {
                    if (!m_originalTextures.TryGetValue(data.m_property, out Texture original))
                    {
                        SeasonalityPlugin.Record.LogDebug($"Failed to find original texture [{data.m_property}]: {data.m_fileName}");
                        m_isValid = false;
                        continue;
                    }

                    if ((original as Texture2D) is not { } originalTex)
                    {
                        SeasonalityPlugin.Record.LogDebug($"Failed to convert {data.m_property} textures to 2D: {material.name}");
                        m_isValid = false;
                        continue;
                    }

                    if (Create(originalTex, data.m_bytes) is not { } texture)
                    {
                        SeasonalityPlugin.Record.LogDebug($"Failed to create new texture: {data.m_fileName}");
                        m_isValid = false;
                        continue;
                    }
                    m_specialTextures.AddOrSet(data.m_property, data.m_season, texture);
                }
                else
                {
                    if (m_originalTex == null || m_originalTex as Texture2D is not { } originalTex)
                    {
                        SeasonalityPlugin.Record.LogDebug($"Failed to convert original texture to 2D: {material.name}");
                        m_isValid = false;
                        continue;
                    }

                    if (Create(originalTex, data.m_bytes) is not { } texture)
                    {
                        SeasonalityPlugin.Record.LogDebug($"Failed to create new texture: {data.m_fileName}");
                        m_isValid = false;
                        continue;
                    }

                    if (m_mainTextures.ContainsKey(data.m_season))
                    {
                        SeasonalityPlugin.Record.LogDebug($"Duplicate season, skipping: {data.m_fileName}");
                        m_isValid = false;
                        continue;
                    }
                    m_mainTextures[data.m_season] = texture;
                }
            }
        }
        
        public void Update()
        {
            if (m_material == null) return;
            UpdateTextures();
            UpdateMoss();
            UpdateTexProperties();
            UpdateColors();
        }
        
        private void UpdateTextures()
        {
            if (m_mainTextures.Count <= 0) return;
            m_material.mainTexture = m_mainTextures.TryGetValue(Configs.m_season.Value, out Texture newTexture) ? newTexture : m_originalTex;
        }

        private void UpdateTexProperties()
        {
            if (m_specialTextures.Count <= 0) return;
            // m_material.EnableKeyword("_NORMALMAP");
            // this needs to be applied to each instance...
            foreach (KeyValuePair<string, Dictionary<Configs.Season, Texture?>> kvp in m_specialTextures)
            {
                if (!m_material.HasProperty(kvp.Key)) continue;
                if (kvp.Value.TryGetValue(Configs.m_season.Value, out Texture? texture))
                {
                    m_material.SetTexture(kvp.Key, texture);
                }
                else
                {
                    if (!m_originalTextures.TryGetValue(kvp.Key, out Texture originalTex)) continue;
                    m_material.SetTexture(kvp.Key, originalTex);
                }
            }
        }

        private void UpdateColors()
        {
            if (!m_updateColors || !m_material.HasProperty("_Color")) return;
            m_material.color = m_mainColors.TryGetValue(Configs.m_season.Value, out Color32 newColor) ? newColor : m_originalColor;
        }

        private void UpdateMoss()
        {
            if (!m_updateMoss || m_isAshlandMaterial) return;
            switch (Configs.m_season.Value)
            {
                case Configs.Season.Fall:
                    m_material.SetTexture(MossTex, TextureManager.Stonemoss_heath.m_tex);
                    break;
                case Configs.Season.Winter:
                    m_material.SetTexture(MossTex, TextureManager.AshOnRocks_d.m_tex);
                    break;
                default:
                    if (m_originalMossTex == null) return;
                    m_material.SetTexture(MossTex, m_originalMossTex);
                    break;
            }

            if (!m_updateMossColor) return;
            switch (Configs.m_season.Value)
            {
                case Configs.Season.Fall or Configs.Season.Winter:
                    m_material.SetColor(MossColor, m_newMossColor);
                    break;
                default:
                    m_material.SetColor(MossColor, m_originalMossColor);
                    break;
            }
        }

        public MaterialData Clone(string name)
        {
            var newMat = new Material(m_material);
            newMat.name = name;
            MaterialData clone = new MaterialData(newMat);
            foreach (var field in typeof(MaterialData).GetFields())
            {
                object? value = field.GetValue(this);
                if (value is Material) continue;
                // Ensure dictionaries are deep copied
                if (value is Dictionary<string, Texture> textureDict)
                {
                    field.SetValue(clone, new Dictionary<string, Texture>(textureDict));
                }
                else if (value is Dictionary<Configs.Season, Texture> mainTextureDict)
                {
                    field.SetValue(clone, new Dictionary<Configs.Season, Texture>(mainTextureDict));
                }
                else if (value is Dictionary<Configs.Season, Color32> colorDict)
                {
                    field.SetValue(clone, new Dictionary<Configs.Season, Color32>(colorDict));
                }
                else if (value is Dictionary<string, Dictionary<Configs.Season, Texture?>> specialTexturesDict)
                {
                    var newSpecialTextures = new Dictionary<string, Dictionary<Configs.Season, Texture?>>();
                    foreach (var kvp in specialTexturesDict)
                    {
                        newSpecialTextures[kvp.Key] = new Dictionary<Configs.Season, Texture?>(kvp.Value);
                    }
                    field.SetValue(clone, newSpecialTextures);
                }
                else
                {
                    field.SetValue(clone, value); // Copy all other values directly
                }
            }
            clone.m_name = name;
            return clone;
        }
    }
    
    private class SpecialCase
    {
        private readonly string m_materialName;
        private readonly Func<MaterialData, bool> m_action;
        private bool m_added;

        public SpecialCase(string materialName, Func<MaterialData, bool> action)
        {
            m_materialName = materialName;
            m_action = action;
            m_cases[materialName] = this;
        }

        public void Run(MaterialData data)
        {
            if (m_added) return;
            if (m_action(data)) m_added = true;
        }
    }
}