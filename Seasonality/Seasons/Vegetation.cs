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
    private static readonly List<GameObject> BaseVegetation = new();
    private static Texture? MossTexture;
    private static readonly Dictionary<string, Texture> VegetationTextures = new();
    
    private static List<Material[]> BeechMaterials = new();
    private static List<Material[]> BeechSmallMaterials = new();
    private static List<Material[]> BirchMaterials = new();
    private static List<Material[]> OakMaterials = new();
    private static List<Material[]> YggaMaterials = new();
    private static List<Material[]> BushMaterials = new();
    private static List<Material[]> PlainsBushMaterials = new();
    private static List<Material[]> ShrubMaterials = new();
    private static List<Material[]> VinesMaterials = new();
    private static List<Material[]> RaspberryMaterials = new();
    private static List<Material[]> BlueberryMaterials = new();

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
                CacheBaseMaterials(prefab);
                
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

        private static List<Material[]> CreateBaseMaterials(GameObject prefab, string specifier, bool contains = true)
        {
            MeshRenderer? BeechRenderer = prefab.GetComponentInChildren<MeshRenderer>();
            if (!BeechRenderer) return new List<Material[]>();
            VegetationType type = Utils.GetVegetationType(prefab.name);
            Material[]? materials = BeechRenderer.materials;
            // Create List of Material Array and apply unique colors to each
            List<Material[]> newMaterialArray = new()
            {
                new Material[materials.Length], 
                new Material[materials.Length], 
                new Material[materials.Length], 
                new Material[materials.Length]
            };

            for (int index = 0; index < newMaterialArray.Count; index++)
            {
                for (int i = 0; i < newMaterialArray[index].Length; ++i)
                {
                    newMaterialArray[index][i] = new Material(materials[i]); // Give new material array same values as original
                }
                Material? leafMat = contains 
                    ? newMaterialArray[index].FirstOrDefault(x => x.name.ToLower().Contains(specifier)) 
                    : newMaterialArray[index].FirstOrDefault(x => !x.name.ToLower().Contains(specifier));
                if (leafMat == null) continue;
                
                string[] properties = leafMat.GetTexturePropertyNames();
                if (Utils.FindTexturePropName(properties, "main", out string mainProp))
                {
                    VegDirectories directory = Utils.VegToDirectory(type);

                    Texture? tex = Utils.GetCustomTexture(directory, Season.Fall);
                    leafMat.SetTexture(mainProp, tex ? tex : GetDefaultTextures(type, prefab.name));
                    leafMat.color = SeasonColors.FallColors[index];
                }
            }

            return newMaterialArray;
        }

        private static void CacheBaseMaterials(GameObject prefab)
        {
            VegetationType type = Utils.GetVegetationType(prefab.name);

            switch (type)
                {
                    case VegetationType.Beech: BeechMaterials = CreateBaseMaterials(prefab, "leaf"); break;
                    case VegetationType.BeechSmall: BeechSmallMaterials = CreateBaseMaterials(prefab, "leaf"); break; 
                    case VegetationType.Birch: BirchMaterials = CreateBaseMaterials(prefab, "leaf"); break; 
                    case VegetationType.Oak: OakMaterials = CreateBaseMaterials(prefab, "leaf"); break; 
                    case VegetationType.Yggashoot: YggaMaterials = CreateBaseMaterials(prefab, "leaf"); break;
                    case VegetationType.Bush: BushMaterials = CreateBaseMaterials(prefab, "wood", false); break; 
                    case VegetationType.PlainsBush: PlainsBushMaterials = CreateBaseMaterials(prefab, "wood", false); break; 
                    case VegetationType.Shrub: ShrubMaterials = CreateBaseMaterials(prefab, "shrub"); break; 
                    case VegetationType.Vines: VinesMaterials = CreateBaseMaterials(prefab, "vinesbranch", false); break; 
                    case VegetationType.RaspberryBush: RaspberryMaterials = CreateBaseMaterials(prefab, "wood", false); break;  
                    case VegetationType.BlueberryBush: BlueberryMaterials = CreateBaseMaterials(prefab, "wood", false); break;
                }
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.CreateObject))]
    static class GetPrefabPatch
    {
        private static void Postfix(ZNetScene __instance, ref GameObject __result)
        {
            if (!__instance) return;
            ApplyColorMaterials(__result);
        }
    }
    private static void ApplyColorMaterials(GameObject prefab)
    {
        if (prefab == null) return;

        VegetationType type = Utils.GetVegetationType(prefab.name);
        if (type is VegetationType.None) return;
        switch (type)
        {
            case VegetationType.Beech: SetMaterials(BeechMaterials, prefab); break;
            case VegetationType.BeechSmall: SetMaterials(BeechSmallMaterials, prefab); break; 
            case VegetationType.Birch: SetMaterials(BirchMaterials, prefab); break; 
            case VegetationType.Oak: SetMaterials(OakMaterials, prefab); break; 
            case VegetationType.Yggashoot: SetMaterials(YggaMaterials, prefab); break;
            case VegetationType.Bush: SetMaterials(BushMaterials, prefab); break; 
            case VegetationType.PlainsBush: SetMaterials(PlainsBushMaterials, prefab); break; 
            case VegetationType.Shrub: SetMaterials(ShrubMaterials, prefab); break; 
            case VegetationType.Vines: SetMaterials(VinesMaterials, prefab); break; 
            case VegetationType.RaspberryBush: SetMaterials(RaspberryMaterials, prefab); break;  
            case VegetationType.BlueberryBush: SetMaterials(BlueberryMaterials, prefab); break;
        }
    }
    private static void SetMaterials(List<Material[]> materials, GameObject prefab)
    {
        if (materials.Count != 4) return;
        Random random = new Random();
        int randomIndex = random.Next(materials.Count);

        var type = Utils.GetVegetationType(prefab.name);
        for (int i = 0; i < prefab.transform.childCount; ++i)
        {
            Transform? child = prefab.transform.GetChild(i);
            if (type is VegetationType.Birch or VegetationType.Yggashoot)
            {
                // SeasonalityLogger.LogWarning(child.name + " color tinting disabled");
                if (child.name.ToLower() == "lod1") continue;
            }
            if (!child) continue;
            if (!child.TryGetComponent(out MeshRenderer meshRenderer)) continue;
            meshRenderer.materials = materials[randomIndex];
            
            if (child.childCount > 0) SetMaterials(materials, child.gameObject);
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
    
    private static void ApplyMaterialToObj(GameObject obj, VegetationType type)
    {
        Random random = new Random();
        int randomIndex = random.Next(3);

        for (int i = 0; i < obj.transform.childCount; ++i)
        {
            Transform child = obj.transform.GetChild(i);
            
            ModifyMeshRenderer(child, type);
            ModifyParticleSystem(obj.transform, (_Season.Value) switch
            {
                Season.Winter => Color.white,
                Season.Fall => SeasonColors.FallColors[randomIndex],
                Season.Spring => SeasonColors.SpringColors[randomIndex],
                _ => SeasonColors.SummerColors[randomIndex]
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
            VegetationType.Bush => _Season.Value is Season.Fall ? VegetationTextures.TryGetValue("bush01_heath_d", out Texture tex) ? tex : VegetationTextures["bush01_d"] : VegetationTextures["bush01_d"],
            VegetationType.PlainsBush => VegetationTextures["bush02_en_d"],
            VegetationType.Shrub => VegetationTextures["shrub_2"],
            VegetationType.CloudberryBush => VegetationTextures["cloudberry_d"],
            VegetationType.RaspberryBush => _Season.Value is Season.Fall ? VegetationTextures.TryGetValue("bush01_heath_d", out Texture tex) ? tex : VegetationTextures["bush01_d"] : VegetationTextures["bush01_d"],
            VegetationType.BlueberryBush => _Season.Value is Season.Fall ? VegetationTextures.TryGetValue("bush01_heath_d", out Texture tex) ? tex : VegetationTextures["bush01_d"] : VegetationTextures["bush01_d"],
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
            case VegetationType.Beech: SetTextureColor(BeechMaterials, propertyName, tex); break;
            case VegetationType.BeechSmall: SetTextureColor(BeechSmallMaterials, propertyName, tex); break; 
            case VegetationType.Birch: SetTextureColor(BirchMaterials, propertyName, tex); break; 
            case VegetationType.Oak: SetTextureColor(OakMaterials, propertyName, tex); break; 
            case VegetationType.Yggashoot: SetTextureColor(YggaMaterials, propertyName, tex); break;
            case VegetationType.Bush: SetTextureColor(BushMaterials, propertyName, tex); break; 
            case VegetationType.PlainsBush: SetTextureColor(PlainsBushMaterials, propertyName, tex); break; 
            case VegetationType.Shrub: SetTextureColor(ShrubMaterials, propertyName, tex); break; 
            case VegetationType.Vines: SetTextureColor(VinesMaterials, propertyName, tex); break; 
            case VegetationType.RaspberryBush: SetTextureColor(RaspberryMaterials, propertyName, tex); break;  
            case VegetationType.BlueberryBush: SetTextureColor(BlueberryMaterials, propertyName, tex); break;
        }
    }

    private static void SetTextureColor(List<Material[]> materialsList, string propertyName, Texture? tex)
    {
        if (!tex) return;
        for (int i = 0; i < materialsList.Count; ++i)
        {
            for (int j = 0; j < materialsList[i].Length; ++j)
            {
                string materialName = materialsList[i][j].name.ToLower();
                if (materialName.Contains("wood") 
                    || materialName.Contains("bark") 
                    || materialName.Contains("vinesbranch") 
                    || materialName.Contains("trunk")) continue;
                materialsList[i][j].SetTexture(propertyName, tex);
                materialsList[i][j].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
            }
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