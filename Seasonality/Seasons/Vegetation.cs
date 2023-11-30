using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;
using Random = System.Random;

namespace Seasonality.Seasons;

public static class Vegetation
{
    private static Toggle currentModEnabled;
    private static readonly List<GameObject> BaseVegetation = new();
    private static Texture? MossTexture;
    private static readonly Dictionary<string, Texture> VegetationTextures = new();


    private static List<Material> BeechMaterials = new();
    private static List<Material> BeechSmallMaterials = new();
    private static List<Material> BirchMaterials = new();
    private static List<Material> OakMaterials = new();
    private static List<Material> YggaMaterials = new();
    private static List<Material> BushMaterials = new();
    private static List<Material> PlainsBushMaterials = new();
    private static List<Material> ShrubMaterials = new();
    private static List<Material> VinesMaterials = new();
    private static List<Material> RaspberryMaterials = new();
    private static List<Material> BlueberryMaterials = new();

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [HarmonyPriority(Priority.Last)]
    static class ZNetSceneVegetationPatch
    {
        private static void Postfix(ZNetScene __instance)
        {
            if (!__instance) return;
            List<GameObject>? prefabs = __instance.m_prefabs;
            
            bool foundMossTex = false;
            foreach (GameObject? prefab in prefabs)
            {
                VegetationType type = Utils.GetVegetationType(prefab.name);
                if (type is VegetationType.None) continue;
                
                CacheVegetation(prefab);
                CreateBaseMaterials(prefab);
                
                // Cache moss texture
                if (foundMossTex) continue;
                if (prefab.name == "Beech1")
                {
                    MeshRenderer? renderer = prefab.GetComponentInChildren<MeshRenderer>();
                    if (!renderer) return;
                    Material[]? materials = renderer.materials;
                    foreach (Material mat in materials)
                    {
                        string[] properties = mat.GetTexturePropertyNames();
                        if (!Utils.FindTexturePropName(properties, "moss", out string MossProp)) continue;
                        Texture? MossTex = mat.GetTexture(MossProp);
                        if (!MossTex) continue;
                        MossTexture = MossTex;
                        foundMossTex = true;
                    }
                };
            }
        }

        private static void CacheVegetation(GameObject prefab)
        {
            if (BaseVegetation.Contains(prefab)) return;
            
            BaseVegetation.Add(prefab); // Cache GameObjects to modify
            // Cache original textures
            for (int i = 0; i < prefab.transform.childCount; ++i)
            {
                Transform child = prefab.transform.GetChild(i);
                if (!child) continue;
                if (!child.TryGetComponent(out MeshRenderer meshRenderer)) continue;
                Material[]? materials = meshRenderer.materials;
                foreach (Material mat in materials)
                {
                    string[] properties = mat.GetTexturePropertyNames();
                    if (!Utils.FindTexturePropName(properties, "main", out string mainProp)) continue;
                    Texture? tex = mat.GetTexture(mainProp);
                    if (!tex) continue;
                    if (VegetationTextures.ContainsKey(tex.name.ToLower())) continue;
                    VegetationTextures.Add(tex.name.ToLower(), tex);
                }
            }
        }

        private static List<Material> CreateBaseMaterials(Material material, VegetationType type, string prefabName)
        {
            string[] properties = material.GetTexturePropertyNames();
            if (!Utils.FindTexturePropName(properties, "main", out string mainProp)) return new List<Material>();
            VegDirectories directory = Utils.VegToDirectory(type);

            Material mat1 = new Material(material);
            Material mat2 = new Material(material);
            Material mat3 = new Material(material);
            Material mat4 = new Material(material);
                                
            List<Material> newMats = new List<Material>() { mat1, mat2, mat3, mat4 };
                                
            for (int i = 0; i < newMats.Count; ++i)
            {
                Texture? tex = Utils.GetCustomTexture(directory, Season.Fall);
                newMats[i].SetTexture(mainProp, tex ? tex : GetDefaultTextures(type, prefabName));
                newMats[i].color = SeasonColors.FallColors[i];
            }

            return newMats;
        }

        private static void CreateBaseMaterials(GameObject prefab)
        {
            VegetationType type = Utils.GetVegetationType(prefab.name);

            switch (type)
                {
                    case VegetationType.Beech:
                        MeshRenderer? BeechRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (!BeechRenderer) break;
                        {
                            Material[]? materials = BeechRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => x.name.ToLower().Contains("leaf"));
                            if (material != null)
                            {
                               BeechMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break;
                    case VegetationType.BeechSmall:
                        MeshRenderer? BeechSmallRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (BeechSmallRenderer)
                        {
                            Material[]? materials = BeechSmallRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => x.name.ToLower().Contains("leaf"));
                            if (material != null)
                            {
                                BeechSmallMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break; 
                    case VegetationType.Birch:
                        MeshRenderer? BirchRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (BirchRenderer)
                        {
                            Material[]? materials = BirchRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => x.name.ToLower().Contains("leaf"));
                            if (material != null)
                            {
                                BirchMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break; 
                    case VegetationType.Oak:
                        MeshRenderer? OakRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (OakRenderer)
                        {
                            Material[]? materials = OakRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => x.name.ToLower().Contains("leaf"));
                            if (material != null)
                            {
                                OakMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break; 
                    case VegetationType.Yggashoot:
                        MeshRenderer? YggaRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (YggaRenderer)
                        {
                            Material[]? materials = YggaRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => x.name.ToLower().Contains("leaf"));
                            if (material != null)
                            {
                                YggaMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break;
                    case VegetationType.Bush:
                        MeshRenderer? BushRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (BushRenderer)
                        {
                            Material[]? materials = BushRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => !x.name.ToLower().Contains("wood"));
                            if (material != null)
                            {
                                BushMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break; 
                    case VegetationType.PlainsBush:
                        MeshRenderer? PlainsBushRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (PlainsBushRenderer)
                        {
                            Material[]? materials = PlainsBushRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => !x.name.ToLower().Contains("wood"));
                            if (material != null)
                            {
                                PlainsBushMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break; 
                    case VegetationType.Shrub: 
                        MeshRenderer? ShrubRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (ShrubRenderer)
                        {
                            Material[]? materials = ShrubRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => x.name.ToLower().Contains("shrub"));
                            if (material != null)
                            {
                                ShrubMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break; 
                    case VegetationType.Vines:
                        MeshRenderer? VinesRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (VinesRenderer)
                        {
                            Material[]? materials = VinesRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => !x.name.ToLower().Contains("vinesbranch"));
                            if (material != null)
                            {
                                VinesMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break; 
                    case VegetationType.RaspberryBush:
                        MeshRenderer? RaspberryRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (RaspberryRenderer)
                        {
                            Material[]? materials = RaspberryRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => !x.name.ToLower().Contains("wood"));
                            if (material != null)
                            {
                                RaspberryMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break;  
                    case VegetationType.BlueberryBush:
                        MeshRenderer? BlueberryRenderer = prefab.GetComponentInChildren<MeshRenderer>();
                        if (BlueberryRenderer)
                        {
                            Material[]? materials = BlueberryRenderer.materials;
                            Material? material = materials.FirstOrDefault(x => !x.name.ToLower().Contains("wood"));
                            if (material != null)
                            {
                                BlueberryMaterials = CreateBaseMaterials(material, type, prefab.name);
                            }
                        }
                        break;
                }
        }
        
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.GetPrefab), typeof(int))]
    static class ZNetSceneCreateObjectPatch
    {
        private static void Postfix(ZNetScene __instance, int hash, out GameObject? __result)
        {
            __result = null;
            if (!__instance) return;

            if (!__instance.m_namedPrefabs.TryGetValue(hash, out GameObject prefab)) return;

            VegetationType type = Utils.GetVegetationType(prefab.name);
            if (type is VegetationType.None) return;
            
            if (_Season.Value is not Season.Fall) return;
            switch (type)
            {
                case VegetationType.Beech:
                    SetColors(BeechMaterials, prefab);
                    break;
                case VegetationType.BeechSmall:
                    SetColors(BeechSmallMaterials, prefab);
                    break; 
                case VegetationType.Birch:
                    SetColors(BirchMaterials, prefab);
                    break; 
                case VegetationType.Oak:
                    SetColors(OakMaterials, prefab);
                    break; 
                case VegetationType.Yggashoot:
                    SetColors(YggaMaterials, prefab);
                    break;
                case VegetationType.Bush:
                    SetColors(BushMaterials, prefab);
                    break; 
                case VegetationType.PlainsBush:
                    SetColors(PlainsBushMaterials, prefab);
                    break; 
                case VegetationType.Shrub: 
                    SetColors(ShrubMaterials, prefab);
                    break; 
                case VegetationType.Vines:
                    SetColors(VinesMaterials, prefab);
                    break; 
                case VegetationType.RaspberryBush:
                    SetColors(RaspberryMaterials, prefab);
                    break;  
                case VegetationType.BlueberryBush:
                    SetColors(BlueberryMaterials, prefab);
                    break;
            }

            __result = prefab;
        }
    }

    private static void SetColors(List<Material> materials, GameObject prefab)
    {
        if (materials.Count != 4) return;
        Random random = new Random();
        int randomIndex = random.Next(BeechMaterials.Count);
                    
        for (int i = 0; i < prefab.transform.childCount; ++i)
        {
            Transform? child = prefab.transform.GetChild(i);
            if (!child) continue;
            if (!child.TryGetComponent(out MeshRenderer meshRenderer)) continue;
            meshRenderer.material = BeechMaterials[randomIndex];
        }
    }

    public static void ModifyBaseVegetation()
    {
        foreach (GameObject? prefab in BaseVegetation)
        {
            VegetationType type = Utils.GetVegetationType(prefab.name);
            if (type is VegetationType.None) return;

            ApplyMaterialToObj(prefab, type);
        }
    }

    public static void ResetBaseVegetation()
    {
        foreach (GameObject? prefab in BaseVegetation)
        {
            VegetationType type = Utils.GetVegetationType(prefab.name);
            if (type is VegetationType.None) return;
            _SeasonControl.Value = Toggle.On;
            _Season.Value = Season.Summer;
            ApplyMaterialToObj(prefab, type);
        }
    }
    
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
        }
    }
    private static void ApplyMaterialToObj(GameObject obj, VegetationType type)
    {
        for (int i = 0; i < obj.transform.childCount; ++i)
        {
            Transform child = obj.transform.GetChild(i);
            
            ModifyMeshRenderer(child, type);
            ModifyParticleSystem(obj.transform, (_Season.Value) switch
            {
                Season.Winter => Color.white,
                Season.Fall => SeasonColors.FallColors[0],
                Season.Spring => SeasonColors.SpringColors[1],
                _ => SeasonColors.SummerColors[0]
            });
            
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

    private static void ModifyMeshRenderer(Transform prefab, VegetationType type)
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
            ModifyMaterialProperties(mat, type, true);
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

    private static Texture? GetDefaultTextures(VegetationType type, string name)
    {
         // beech_leaf_small
         // beech_bark
         // beech_leaf
         // birch_leaf
         // birch_bark
         // birch_leaf_yellow
         // bush01_d
         // (wood 26) tree bark seamless texture 2048x2048
         // bush01_heath_d
         // bush02_en_d
         // cloudberry_d
         // pine_tree_texture_d
         // pine_tree_log_texture
         // pine_tree_texture_small
         // pine_tree_texture_small_dead
         // heathrock_d
         // gouacherock_big
         // oak_bark
         // oak_leaf
         // rocks_3_4_texture
         // rocks_4_texture
         // copper_ore_big_d
         // diffuse_grey
         // shrub_2
         // shrub_3_heath
         // diffuse
         // rock_med
         // stump
         // olivetree_trunk01
         // deadbranch
         // shootleaf_d
         // shoottrunk_d

         return (type) switch
         {
            VegetationType.Beech => VegetationTextures["beech_leaf"],
            VegetationType.BeechSmall => VegetationTextures["beech_leaf_small"],
            VegetationType.Birch => VegetationTextures["birch_leaf"],
            VegetationType.Fir => name.Contains("small") ? VegetationTextures["pine_tree_texture_small"] : VegetationTextures["pine_tree_texture_d"],
            VegetationType.Pine => VegetationTextures.TryGetValue("PineTree_01", out Texture tex) ? tex : null,
            VegetationType.Swamp => VegetationTextures.TryGetValue("deadbranch", out Texture tex) ? tex : null,
            VegetationType.Yggashoot => VegetationTextures["shootleaf_d"],
            VegetationType.Oak => VegetationTextures["oak_leaf"],
            VegetationType.Stubbe => VegetationTextures["stump"],
            VegetationType.Bush => _Season.Value is Season.Fall ? VegetationTextures["bush01_heath_d"] : VegetationTextures["bush01_d"],
            VegetationType.PlainsBush => VegetationTextures["bush02_en_d"],
            VegetationType.Shrub => VegetationTextures["shrub_2"],
            VegetationType.CloudberryBush => VegetationTextures["cloudberry_d"],
            _ => null
        };
    } 

    private static void ModifyMainTex(string propertyName, Material material, VegetationType type)
    {
        string normalizedName = material.name.ToLower().Replace(" (instance)", "");
        VegDirectories directory = Utils.VegToDirectory(type);
        // Get default textures
        Texture? tex = GetDefaultTextures(type, normalizedName);
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
                        Texture? customTex = Utils.GetCustomTexture(directory, Season.Winter);
                        if (customTex) tex = customTex;
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
                            if (customBeechSpring) {tex = customBeechSpring;}
                            break;
                        }

                        if (normalizedName == "beech_leaf_small")
                        {
                            Texture? customBeechSmallSpring = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Spring);
                            if (customBeechSmallSpring) tex = customBeechSmallSpring;
                        }
                        break;
                    case VegetationType.Oak:
                        if (normalizedName == "oak_leaf")
                        {
                            Texture? customOakSpring = Utils.GetCustomTexture(VegDirectories.Oak, Season.Spring);
                            if (customOakSpring) tex = customOakSpring;
                        }
                        break;
                    case VegetationType.Birch:
                        if (normalizedName == "birch_leaf" || normalizedName == "birch_leaf_aut")
                        {
                            Texture? customBirchSpring = Utils.GetCustomTexture(VegDirectories.Birch, Season.Spring);
                            if (customBirchSpring) tex = customBirchSpring;
                        }
                        break;
                    case VegetationType.Yggashoot:
                        if (normalizedName == "shoot_leaf_mat")
                        {
                            Texture? customYggaSpring = Utils.GetCustomTexture(VegDirectories.YggaShoot, Season.Spring);
                            if (customYggaSpring) tex = customYggaSpring;
                        }
                        break;
                    default:
                        Texture? customTex = Utils.GetCustomTexture(directory, Season.Spring);
                        if (customTex) tex = customTex;
                        break;
                }                        
                break;
            case Season.Summer:
                Texture? customSummerTex = Utils.GetCustomTexture(directory, Season.Summer);
                if (customSummerTex) tex = customSummerTex;   
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
                        Texture? customTex = Utils.GetCustomTexture(directory, Season.Fall);
                        if (customTex) tex = customTex;
                        break;
                }                    
                break;
        }

        if (!tex) return;
        material.SetTexture(propertyName, tex);
        switch (type)
        {
            case VegetationType.Beech:
                for (int i = 0; i < BeechMaterials.Count; ++i)
                {
                    BeechMaterials[i].SetTexture(propertyName, tex);
                    BeechMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break;
            case VegetationType.BeechSmall:
                for (int i = 0; i < BeechSmallMaterials.Count; ++i)
                {
                    BeechSmallMaterials[i].SetTexture(propertyName, tex);
                    BeechSmallMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break; 
            case VegetationType.Birch:
                for (int i = 0; i < BirchMaterials.Count; ++i)
                {
                    BirchMaterials[i].SetTexture(propertyName, tex);
                    BirchMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break; 
            case VegetationType.Oak:
                for (int i = 0; i < OakMaterials.Count; ++i)
                {
                    OakMaterials[i].SetTexture(propertyName, tex);
                    OakMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break; 
            case VegetationType.Yggashoot:
                for (int i = 0; i < YggaMaterials.Count; ++i)
                {
                    YggaMaterials[i].SetTexture(propertyName, tex);
                    YggaMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break;
            case VegetationType.Bush:
                for (int i = 0; i < BushMaterials.Count; ++i)
                {
                    BushMaterials[i].SetTexture(propertyName, tex);
                    BushMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break; 
            case VegetationType.PlainsBush:
                for (int i = 0; i < PlainsBushMaterials.Count; ++i)
                {
                    PlainsBushMaterials[i].SetTexture(propertyName, tex);
                    PlainsBushMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break; 
            case VegetationType.Shrub: 
                for (int i = 0; i < ShrubMaterials.Count; ++i)
                {
                    ShrubMaterials[i].SetTexture(propertyName, tex);
                    ShrubMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break; 
            case VegetationType.Vines:
                for (int i = 0; i < VinesMaterials.Count; ++i)
                {
                    VinesMaterials[i].SetTexture(propertyName, tex);
                    VinesMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break; 
            case VegetationType.RaspberryBush:
                for (int i = 0; i < RaspberryMaterials.Count; ++i)
                {
                    RaspberryMaterials[i].SetTexture(propertyName, tex);
                    RaspberryMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break;  
            case VegetationType.BlueberryBush:
                for (int i = 0; i < BlueberryMaterials.Count; ++i)
                {
                    BlueberryMaterials[i].SetTexture(propertyName, tex);
                    BlueberryMaterials[i].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }
                break;
        }
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
                material.SetTexture(propertyName, customFall ? customFall : MossTexture);
                break;
            case Season.Spring:
                Texture? customSpring = Utils.GetCustomTexture(VegDirectories.Moss, Season.Spring);
                material.SetTexture(propertyName, customSpring ? customSpring : MossTexture);
                break;
            case Season.Summer:
                Texture? customSummer = Utils.GetCustomTexture(VegDirectories.Moss, Season.Summer);
                material.SetTexture(propertyName, customSummer ? customSummer : MossTexture);
                break;
        }
    }
}