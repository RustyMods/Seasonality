using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Behaviors;
using Seasonality.Helpers;
using UnityEngine;

namespace Seasonality.Textures;

public static class TextureReplacer
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

    public static readonly Dictionary<Material, MaterialData> m_mats = new();

    private static bool TestSnowShader = true;

    [Description("Setup special cases")]
    private static void Setup()
    {
        SpecialCase MistLandGrassShort = new("grasscross_mistlands_short", material =>
        {
            foreach (KeyValuePair<Configs.Season, Texture?> kvp in material.m_mainTextures)
            {
                material.m_mainColors[kvp.Key] = new Color32(255, 255, 255, 255);
                material.m_updateColors = true;
                material.m_specialTextures.AddOrSetNull("_TerrainColorTex", kvp.Key, null);
            }
            return true;
        });

        SpecialCase GrassCrossMeadows = new("grasscross_meadows", material =>
        {
            foreach (KeyValuePair<Configs.Season, Texture?> texture in material.m_mainTextures)
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
            material.m_mainColors[Configs.Season.Fall] = Configs.m_fallColor1.Value;
            Configs.m_fallColor1.SettingChanged += (_, _) =>
            {
                material.m_mainColors[Configs.Season.Fall] = Configs.m_fallColor1.Value;
                material.UpdateColors();
            };
            material.m_updateColors = true;
            return true;
        });
        
        SpecialCase InstancedOrmbunke = new("ormbunke", material =>
        {
            material.m_mainColors[Configs.Season.Fall] = Configs.m_fallColor1.Value;
            Configs.m_fallColor1.SettingChanged += (_, _) =>
            {
                material.m_mainColors[Configs.Season.Fall] = Configs.m_fallColor1.Value;
                material.UpdateColors();
            };
            material.m_updateColors = true;
            return true;
        });
        
        SpecialCase InstancedOrmbunkeYellow = new("ormbunke_yellow", material =>
        {
            material.m_mainColors[Configs.Season.Fall] = Configs.m_fallColor1.Value;
            Configs.m_fallColor1.SettingChanged += (_, _) =>
            {
                material.m_mainColors[Configs.Season.Fall] = Configs.m_fallColor1.Value;
                material.UpdateColors();
            };
            material.m_updateColors = true;
            return true;
        });
    }
    
    [Description("Using registered materials, clone fall variants")]
    private static void SetupFallMaterials()
    {
        List<ConfigEntry<Color>> colorConfigs = new()
         {
             Configs.m_fallColor1,
             Configs.m_fallColor2,
             Configs.m_fallColor3,
             Configs.m_fallColor4
         };
        
        foreach (var material in new Configs.SerializedNameList(Configs.m_fallMaterials.Value).m_names)
        {
            if (!m_materials.TryGetValue(material, out MaterialData original)) continue;
            List<MaterialData> list = new();
            for (var index = 0; index < colorConfigs.Count; index++)
            {
                var color = colorConfigs[index];
                string name = $"{original.m_name}_{index}";
                var clone = original.Clone(name);
                clone.m_mainColors[Configs.Season.Fall] = color.Value;
                color.SettingChanged += (_, _) =>
                {
                    clone.m_mainColors[Configs.Season.Fall] = color.Value;
                    clone.UpdateColors();
                };
                clone.m_updateColors = true;
                list.Add(clone);
            }
            m_fallMaterials[original.m_name] = list;
        }

        Configs.m_fallMaterials.SettingChanged += (_, _) =>
        {
            m_fallMaterials.Clear();
            foreach (var material in new Configs.SerializedNameList(Configs.m_fallMaterials.Value).m_names)
            {
                if (!m_materials.TryGetValue(material, out MaterialData original)) continue;
                List<MaterialData> list = new();
                for (var index = 0; index < colorConfigs.Count; index++)
                {
                    var color = colorConfigs[index];
                    string name = $"{original.m_name}_{index}";
                    var clone = original.Clone(name);
                    clone.m_mainColors[Configs.Season.Fall] = color.Value;
                    color.SettingChanged += (_, _) =>
                    {
                        clone.m_mainColors[Configs.Season.Fall] = color.Value;
                        clone.UpdateColors();
                    };
                    clone.m_updateColors = true;
                    list.Add(clone);
                }
                m_fallMaterials[original.m_name] = list;
            }
        };
    }
    public static void UpdateAll()
    {
        foreach(var material in m_mats.Values) material.Update();
        foreach (var material in m_fallMaterials)
        {
            foreach (var mat in material.Value)
            {
                mat.Update();
            }
        }
    }

    [Description("Find all materials in resources, and register")]
    private static int Load(bool clear = false)
    {
        if (clear)
        {
            m_materials.Clear();
            m_allMaterials.Clear();
            m_mossTextures.Clear();
            m_fallMaterials.Clear();
            m_mats.Clear();
        }
        m_referenceCount = m_mats.Count;

        foreach (Material? mat in Resources.FindObjectsOfTypeAll<Material>())
        {
            if (mat == null) continue;
            if (!m_allMaterials.ContainsKey(mat.name)) m_allMaterials[mat.name] = mat;
            if (mat.GetInstanceID() < 0) continue;
            // negative instance id is an instance, while positive is an a asset
            if (m_mats.ContainsKey(mat)) continue;
            
            if (!mat.HasProperty("_MainTex")) continue;
            var name = mat.name;
            if (name == "oak_leaf_nosnow") name = "oak_leaf"; // MonsterLabz asset that needs to be switched to the original version
            if (TextureManager.m_texturePacks.TryGetValue(name, out TextureManager.TexturePack texturePack))
            {
                // Register any materials that match imported textures
                var data = new MaterialData(mat, texturePack);
                data.ApplySpecialCases();
                data.Conclude();
                if (!data.m_isValid) continue;
                m_materials[mat.name] = data; // for reference for fall materials
                m_mats[mat] = data; 
                
                SeasonalityPlugin.Record.LogSuccess($"Registered textures to: {data.m_name}");
            }
            else
            {
                if (mat.shader.name == "Custom/Piece")
                {
                    if (TestSnowShader)
                    {
                        // Register all piece materials
                        var data = new MaterialData(mat);
                        data.ApplySpecialCases();
                        if (!data.m_isValid) continue;
                    
                        m_mats[mat] = data;
                        m_materials[mat.name] = data;
                        SeasonalityPlugin.Record.LogSuccess($"Registered piece material: {data.m_name}");
                    }
                }
                else
                {
                    // Register all moss materials
                    if (!mat.HasProperty(MossTex)) continue;
                    var data = new MaterialData(mat);
                    data.ApplySpecialCases();
                    if (!data.m_isValid) continue;

                    m_mats[mat] = data;
                    m_materials[mat.name] = data;
                    SeasonalityPlugin.Record.LogSuccess($"Registered moss material: {data.m_name}");
                    if (data.m_originalMossTex == null) continue;
                    if (!m_mossTextures.ContainsKey(data.m_originalMossTex.name)) continue;
                    m_mossTextures[data.m_originalMossTex.name] = data.m_originalMossTex;
                }
            }
        }
        
        var difference = m_mats.Count - m_referenceCount;
        return difference;
    }
    
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    private static class FejdStartup_Awake_Patch
    {
        private static void Postfix()
        {
            ShaderFix.CacheShaders();
            Setup();
            if (SeasonalityPlugin.BadgerHDLoaded) return;
            Stopwatch watch = Stopwatch.StartNew();
            var count = Load();
            watch.Stop();
            SeasonalityPlugin.Record.LogDebug($"[FejdStartup Awake] Generation textures time: {watch.ElapsedMilliseconds}ms");
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
            SeasonalityPlugin.Record.LogDebug($"[ZoneSystem SetupLocations] Generation textures time: {watch.ElapsedMilliseconds}ms");
            SeasonalityPlugin.Record.LogDebug($"[ZoneSystem SetupLocations]: Registered {count} new materials");
            SetupFallMaterials();
            if (Configs.m_fixShader.Value is Configs.Toggle.On) FixShaders();
            Configs.m_fixShader.SettingChanged += (sender, args) =>
            {
                if (Configs.m_fixShader.Value is Configs.Toggle.On) FixShaders();
            };
            SeasonalityPlugin.OnSeasonChange();
        }
    }

    public static void FixShaders()
    {
        foreach (var data in m_materials)
        {
            if (data.Value.m_material == null) continue;
            data.Value.m_material.shader =
                ShaderFix.GetShader(data.Value.m_material.shader.name, data.Value.m_material.shader);
        }
        foreach (var materials in m_fallMaterials)
        {
            foreach (var data in materials.Value)
            {
                if (data.m_material == null) continue;
                data.m_material.shader =
                    ShaderFix.GetShader(data.m_material.shader.name, data.m_material.shader);
            }
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
            SeasonalityPlugin.Record.LogDebug($"[ZNetScene Awake] Generation textures time: {watch.ElapsedMilliseconds}ms");
            SeasonalityPlugin.Record.LogDebug($"[ZNetScene Awake]: Registered {count} new materials");
            SeasonalityPlugin.OnSeasonChange();
        }
    }

    public class MaterialData
    {
        public string m_name;
        public string m_shaderName;
        public Shader m_originalShader;
        public Material m_material; // Can become null
        public Texture? m_originalTex;
        public Color32 m_originalColor;
        public Texture? m_originalMossTex;
        public Dictionary<string, Texture> m_originalTextures = new();
        public Dictionary<Configs.Season, Texture?> m_mainTextures = new();
        public Dictionary<Configs.Season, Color32> m_mainColors = new();
        public Dictionary<string, Dictionary<Configs.Season, Texture?>> m_specialTextures = new();
        public Color32 m_originalMossColor;
        public Color32 m_newMossColor;
        public bool m_updateColors;
        public bool m_updateMoss;
        public bool m_updateMossColor;
        public bool m_isAshlandMaterial;
        public readonly bool m_isValid = true;

        public MaterialData Clone(string name)
        {
            var newMat = new Material(m_material);
            newMat.name = name;
            MaterialData clone = new MaterialData(newMat);
            clone.m_mainTextures = new Dictionary<Configs.Season, Texture?>(m_mainTextures);
            clone.m_mainColors = new Dictionary<Configs.Season, Color32>(m_mainColors);
            var specialTex = new Dictionary<string, Dictionary<Configs.Season, Texture?>>();
            foreach (var kvp in m_specialTextures)
            {
                specialTex[kvp.Key] = new Dictionary<Configs.Season, Texture?>(kvp.Value);
            }
            clone.m_specialTextures = specialTex;
            clone.m_newMossColor = m_newMossColor;
            clone.m_updateColors = m_updateColors;
            clone.m_updateMoss = m_updateMoss;
            clone.m_updateMossColor = m_updateMossColor;
            clone.m_isAshlandMaterial = m_isAshlandMaterial;
            clone.m_name = name;
            return clone;
        }
        public MaterialData(Material material)
        {
            m_originalShader = material.shader;
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

            // if (TestSnowShader)
            // {
            //     if (m_shaderName == "Custom/Piece")
            //     {
            //         material.shader = FrozenManager.SnowPieces;
            //     }
            // }
        }

        // public void ToggleSnowShader(bool value)
        // {
        //     if (m_originalShader.name == "Custom/Piece")
        //     {
        //         m_material.shader = value ? FrozenManager.SnowPieces : m_originalShader;
        //     }
        // }

        public void Conclude()
        {
            // After reading custom textures, make sure dictionary has original texture set to seasons without custom textures
            // to fix problem of returning to original textures
            foreach (Configs.Season season in Enum.GetValues(typeof(Configs.Season)))
            {
                if (m_mainTextures.ContainsKey(season)) continue;
                m_mainTextures[season] = m_originalTex;
            }
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
            foreach (string property in m_material.GetTexturePropertyNames())
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
        private static Texture? Create(Texture2D original, TextureManager.ImageData data)
        {
            if (data.m_isTex) return data.m_texture;
            try
            {
                Texture2D texture = new Texture2D(original.width, original.height, original.format, original.mipmapCount > 1)
                {
                    anisoLevel = original.anisoLevel,
                    filterMode = original.filterMode,
                    mipMapBias = original.mipMapBias,
                    wrapMode = original.wrapMode,
                    wrapModeU = original.wrapModeU,
                    wrapModeV = original.wrapModeV,
                    wrapModeW = original.wrapModeW
                };
                if (!texture.LoadImage(data.m_bytes)) return null;
                texture.Apply();
                texture.name = data.m_fileName;
                return texture;
            }
            catch 
            {
                SeasonalityPlugin.Record.LogError("Failed to generate texture for: " + data.m_fileName);
                return null;
            }
        }
        public MaterialData(Material material, TextureManager.TexturePack package) : this(material)
        {
            foreach (TextureManager.ImageData? data in package.m_textures.Values)
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

                    if (Create(originalTex, data) is not { } texture)
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
                    if (Create(originalTex, data) is not { } texture)
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
            UpdateMainTex();
            UpdateMoss();
            UpdateTexProperties();
            UpdateColors();
        }
        private void UpdateMainTex()
        {
            if (m_mainTextures.Count <= 0) return;
            // original textures changes to the season which game starts, but still use this as backup to make sure it switches to something
            // when trying to go to original texture
            m_material.mainTexture = m_mainTextures.TryGetValue(Configs.m_season.Value, out Texture? newTexture) ? newTexture : m_originalTex;
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
        public void UpdateColors()
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
    }
    
    private class SpecialCase
    {
        private readonly Func<MaterialData, bool> m_action;
        private bool m_added;

        public SpecialCase(string materialName, Func<MaterialData, bool> action)
        {
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