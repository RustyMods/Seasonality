using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CacheTextures;
using static Seasonality.Seasons.CustomTextures;
using Random = System.Random;

namespace Seasonality.Seasons;

public static class Vegetation
{
    private static readonly List<GameObject> BaseVegetation = new();
    
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
            foreach (GameObject? prefab in prefabs)
            {
                VegetationType type = Utils.GetVegetationType(prefab.name);
                if (type is VegetationType.None) continue;
                
                CacheVegetation(prefab);
                CacheBaseMaterials(prefab);
            }
        }

        private static void CacheVegetation(GameObject prefab)
        {
            if (!BaseVegetation.Contains(prefab)) BaseVegetation.Add(prefab); // Cache GameObjects to modify
        }

        private static List<Material[]> CreateBaseMaterials(GameObject prefab, string specifier, bool contains = true)
        {
            MeshRenderer? PrefabRenderer = prefab.GetComponentInChildren<MeshRenderer>();
            if (!PrefabRenderer) return new List<Material[]>();
            VegetationType type = Utils.GetVegetationType(prefab.name);
            Material[]? materials = PrefabRenderer.materials;
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
                // Add color tint to specified material
                Material? leafMat = contains 
                    ? newMaterialArray[index].FirstOrDefault(x => x.name.ToLower().Contains(specifier)) 
                    : newMaterialArray[index].FirstOrDefault(x => !x.name.ToLower().Contains(specifier));
                if (leafMat == null) continue;
                
                string[] properties = leafMat.GetTexturePropertyNames();
                if (Utils.FindTexturePropName(properties, "main", out string mainProp))
                {
                    VegDirectories directory = Utils.VegToDirectory(type);

                    Texture? tex = Utils.GetCustomTexture(directory, Season.Fall.ToString());
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

        VegetationType type = Utils.GetVegetationType(prefab.name);
        for (int i = 0; i < prefab.transform.childCount; ++i)
        {
            Transform? child = prefab.transform.GetChild(i);
            if (type is VegetationType.Birch or VegetationType.Yggashoot or VegetationType.PlainsBush)
            {
                // SeasonalityLogger.LogWarning(child.name + " color tinting disabled");
                if (child.name.ToLower() is "lod1" or "low") continue;
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
        switch (type)
        {
            case VegetationType.Beech: ModifyCustomMaterials(BeechMaterials, type); break;
            case VegetationType.BeechSmall: ModifyCustomMaterials(BeechSmallMaterials, type); break; 
            case VegetationType.Birch: ModifyCustomMaterials(BirchMaterials, type); break; 
            case VegetationType.Oak: ModifyCustomMaterials(OakMaterials, type); break; 
            case VegetationType.Yggashoot: ModifyCustomMaterials(YggaMaterials, type); break;
            case VegetationType.Bush: ModifyCustomMaterials(BushMaterials, type); break; 
            case VegetationType.PlainsBush: ModifyCustomMaterials(PlainsBushMaterials, type); break; 
            case VegetationType.Shrub: ModifyCustomMaterials(ShrubMaterials, type); break; 
            case VegetationType.Vines: ModifyCustomMaterials(VinesMaterials, type); break; 
            case VegetationType.RaspberryBush: ModifyCustomMaterials(RaspberryMaterials, type); break;  
            case VegetationType.BlueberryBush: ModifyCustomMaterials(BlueberryMaterials, type); break;
        }
    }
    private static void ModifyCustomMaterials(List<Material[]> materialsList, VegetationType type)
    {
        foreach (Material[]? materials in materialsList)
        {
            foreach (Material? mat in materials)
            {
                string[] properties = mat.GetTexturePropertyNames();
                if (Utils.FindTexturePropName(properties, "moss", out string mossProp))
                {
                    ModifyMossTex(mossProp, mat, type);
                }
            }
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
        if (Utils.FindTexturePropName(properties, "moss", out string mossProp))
        {
            // SeasonalityLogger.LogWarning($"{materialName} changing moss");
            ModifyMossTex(mossProp, mat, type);
            
        }

        if (!modifyMainTex) return;
        if (Utils.FindTexturePropName(properties, "main", out string mainProp))
        {
            if (materialName.Contains("bark")
                || materialName.Contains("trunk")
                || materialName.Contains("log")
                || materialName.Contains("wood")
                || materialName.Contains("stump")
               )
            {
                ModifyMainTex(mainProp, mat, type, true);
            }
            else
            {
                ModifyMainTex(mainProp, mat, type, false);
            }
        };
    }
    private static Texture? GetDefaultTextures(VegetationType type, string name, bool isBark = false)
    {
        try
        {
            return (type) switch
            {
                VegetationType.Beech => isBark 
                    ? CachedTextures["beech_bark"] 
                    : CachedTextures["beech_leaf"],
                VegetationType.BeechSmall => isBark
                    ? CachedTextures["beech_bark_small"]
                    : CachedTextures["beech_leaf_small"],
                VegetationType.Birch => isBark 
                    ? CachedTextures["birch_bark"] 
                    : CachedTextures.TryGetValue("birch_leaf_aut", out Texture tex) ? tex : null,
                VegetationType.Fir => name.Contains("small")
                    ? CachedTextures["Pine_tree_small"]
                    : CachedTextures["Pine_tree"],
                VegetationType.Pine => CachedTextures["PineTree_01"],
                VegetationType.Swamp => isBark 
                    ? CachedTextures["swamptree1_bark"] :
                    CachedTextures["swamptree1_branch"],
                VegetationType.Oak => isBark 
                ? CachedTextures["oak_bark"] 
                : CachedTextures["oak_leaf"],
                VegetationType.Stubbe => CachedTextures["stump"],
                VegetationType.Bush => CachedTextures["Bush01"],
                VegetationType.PlainsBush => CachedTextures.TryGetValue("Bush02_en", out Texture tex) ? tex : null,
                VegetationType.Shrub => CachedTextures["shrub"],
                VegetationType.CloudberryBush => CachedTextures["Cloudberrybush"],
                VegetationType.RaspberryBush => CachedTextures.TryGetValue("Bush01_raspberry", out Texture tex) ? tex : null,
                VegetationType.BlueberryBush => CachedTextures.TryGetValue("Bush01_blueberry", out Texture tex) ? tex : null,
                VegetationType.Vines => CachedTextures.TryGetValue("Vines_Mat", out Texture tex) ? tex : null,
                VegetationType.YggashootSmall or VegetationType.Yggashoot => isBark
                    ? CachedTextures.TryGetValue("Shoot_Trunk_mat", out Texture trunk) ? trunk : null
                    : CachedTextures.TryGetValue("Shoot_Leaf_mat", out Texture tex) ? tex : null,
                _ => null
            };

        }
        catch (Exception)
        {
            SeasonalityLogger.LogWarning("Failed to get texture for " + type);
            return null;
        }
    }
    private static void ModifyMainTex(string propertyName, Material material, VegetationType type, bool isBark = false)
    {
        string normalizedName = material.name.ToLower().Replace(" (instance)", "");
        VegDirectories directory = Utils.VegToDirectory(type);
        // Get default textures
        Texture? tex = GetDefaultTextures(type, normalizedName, isBark);
        switch (_Season.Value)
        {
            case Season.Winter:
                switch (type)
                {
                    case VegetationType.Beech:
                        if (isBark)
                        {
                            Texture? customBeechBark = Utils.GetCustomTexture(VegDirectories.Beech, Season.Winter + "_bark");
                            if (customBeechBark != null) tex = customBeechBark;
                        }
                        else
                        {
                            Texture? customBeechWinter = Utils.GetCustomTexture(VegDirectories.Beech, Season.Winter.ToString());
                            if (customBeechWinter != null) tex = customBeechWinter;
                        }
                        break;
                    case VegetationType.BeechSmall:
                        if (isBark)
                        {
                            Texture? customBeechBark = Utils.GetCustomTexture(VegDirectories.Beech, Season.Winter + "_bark");
                            if (customBeechBark != null) tex = customBeechBark;
                        }
                        else
                        {
                            Texture? customBeechSmallWinter = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Winter.ToString());
                            if (customBeechSmallWinter != null) tex = customBeechSmallWinter;
                        }
                        break;
                    case VegetationType.Oak:
                        if (isBark)
                        {
                            Texture? customOakBark = Utils.GetCustomTexture(VegDirectories.Oak, Season.Winter.ToString() + "_bark");
                            if (customOakBark != null) tex = customOakBark;
                        }
                        else
                        {
                            Texture? customOak = Utils.GetCustomTexture(VegDirectories.Oak, Season.Winter.ToString());
                            if (customOak != null) tex = customOak;
                        }
                        break;
                    default:
                        Texture? customTex = Utils.GetCustomTexture(directory,  isBark ? Season.Winter.ToString() + "_bark" : Season.Winter.ToString());
                        if (customTex != null) tex = customTex;
                        break;
                }
                break;
            case Season.Spring:
                switch (type)
                {
                    case VegetationType.Beech:
                        if (isBark)
                        {
                            Texture? customBeechBark = Utils.GetCustomTexture(VegDirectories.Beech, Season.Spring + "_bark");
                            if (customBeechBark != null) tex = customBeechBark;
                        }
                        else
                        {
                            Texture? customBeechSpring = Utils.GetCustomTexture(VegDirectories.Beech, Season.Spring.ToString());
                            if (customBeechSpring != null) tex = customBeechSpring;
                        }
                        break;
                    case VegetationType.BeechSmall:
                        if (isBark)
                        {
                            Texture? customBeechBark = Utils.GetCustomTexture(VegDirectories.Beech, Season.Spring + "_bark");
                            if (customBeechBark != null) tex = customBeechBark;
                        }
                        else
                        {
                            Texture? customBeechSmallSpring = Utils.GetCustomTexture(VegDirectories.BeechSmall, Season.Spring.ToString());
                            if (customBeechSmallSpring != null) tex = customBeechSmallSpring;
                        }
                        break;
                    case VegetationType.Oak:
                        if (isBark)
                        {
                            Texture? customOakBark = Utils.GetCustomTexture(VegDirectories.Oak, Season.Spring + "_bark");
                            if (customOakBark != null) tex = customOakBark;
                        }
                        else
                        {
                            Texture? customOakSpring = Utils.GetCustomTexture(VegDirectories.Oak, Season.Spring.ToString());
                            if (customOakSpring != null) tex = customOakSpring;
                        }
                        break;
                    case VegetationType.Birch:
                        if (normalizedName == "birch_leaf" || normalizedName == "birch_leaf_aut")
                        {
                            Texture? customBirchSpring = Utils.GetCustomTexture(VegDirectories.Birch, Season.Spring.ToString());
                            if (customBirchSpring != null) tex = customBirchSpring;
                        }
                        if (isBark)
                        {
                            Texture? customBirchBark =
                                Utils.GetCustomTexture(VegDirectories.Birch, Season.Spring + "_bark");
                            if (customBirchBark != null) tex = customBirchBark;
                        }
                        break;
                    case VegetationType.Yggashoot:
                        if (normalizedName == "shoot_leaf_mat")
                        {
                            Texture? customYggaSpring = Utils.GetCustomTexture(VegDirectories.YggaShoot, Season.Spring.ToString());
                            if (customYggaSpring != null) tex = customYggaSpring;
                        }

                        if (isBark)
                        {
                            Texture? customYggaBark =
                                Utils.GetCustomTexture(VegDirectories.YggaShoot, Season.Spring + "_bark");
                            if (customYggaBark != null) tex = customYggaBark;
                        }
                        break;
                    default:
                        Texture? customTex = Utils.GetCustomTexture(directory, isBark ? Season.Spring.ToString() + "_bark" : Season.Spring.ToString());
                        if (customTex != null) tex = customTex;
                        break;
                }                        
                break;
            case Season.Summer:
                switch (type)
                {
                    default:
                        Texture? customSummerTex = Utils.GetCustomTexture(directory, isBark ? Season.Summer.ToString() + "_bark" : Season.Summer.ToString());
                        if (customSummerTex != null) tex = customSummerTex;
                        break;
                }

                break;
            case Season.Fall:
                switch (type)
                {
                    case VegetationType.Pine:
                        Texture? customPine = Utils.GetCustomTexture(VegDirectories.Pine, isBark ? Season.Fall.ToString() + "_bark" : Season.Fall.ToString());
                        if (customPine != null) tex = customPine;
                        break;
                    case VegetationType.Fir:
                        Texture? customFir = Utils.GetCustomTexture(VegDirectories.Fir, isBark ? Season.Fall.ToString() + "_bark" : Season.Fall.ToString());
                        if (customFir != null) tex = customFir;
                        break;
                    case VegetationType.BeechSmall:
                        Texture? customSmall = Utils.GetCustomTexture(VegDirectories.BeechSmall, isBark ? Season.Fall.ToString() + "_bark" : Season.Fall.ToString());
                        if (customSmall != null) tex = customSmall;
                        break;
                    case VegetationType.Beech:
                        Texture? customBig = Utils.GetCustomTexture(VegDirectories.Beech, isBark ? Season.Fall.ToString() + "_bark" : Season.Fall.ToString());
                        if (customBig != null) tex = customBig;
                        break;
                    default:
                        Texture? customTex = Utils.GetCustomTexture(directory, isBark ? Season.Fall.ToString() + "_bark" : Season.Fall.ToString());
                        if (customTex != null) tex = customTex;
                        break;
                }                    
                break;
        }

        if (tex == null) return;
        material.SetTexture(propertyName, tex);
        switch (type)
        {
            case VegetationType.Beech: SetTextureColor(BeechMaterials, propertyName, tex, isBark); break;
            case VegetationType.BeechSmall: SetTextureColor(BeechSmallMaterials, propertyName, tex, isBark); break; 
            case VegetationType.Birch: SetTextureColor(BirchMaterials, propertyName, tex, isBark); break; 
            case VegetationType.Oak: SetTextureColor(OakMaterials, propertyName, tex, isBark); break; 
            case VegetationType.Yggashoot: SetTextureColor(YggaMaterials, propertyName, tex, isBark); break;
            case VegetationType.Bush: SetTextureColor(BushMaterials, propertyName, tex, isBark); break; 
            case VegetationType.PlainsBush: SetTextureColor(PlainsBushMaterials, propertyName, tex, isBark); break; 
            case VegetationType.Shrub: SetTextureColor(ShrubMaterials, propertyName, tex, isBark); break; 
            case VegetationType.Vines: SetTextureColor(VinesMaterials, propertyName, tex, isBark); break; 
            case VegetationType.RaspberryBush: SetTextureColor(RaspberryMaterials, propertyName, tex, isBark); break;  
            case VegetationType.BlueberryBush: SetTextureColor(BlueberryMaterials, propertyName, tex, isBark); break;
        }
    }
    private static void SetTextureColor(List<Material[]> materialsList, string propertyName, Texture? tex, bool isBark)
    {
        // if (CachedMaterials.TryGetValue("VinesBranch_mat", out Material vineMat))
        // {
        //     SeasonalityLogger.LogWarning("Tweaking the shit of vines");
        //     Color color = new Color();
        //     switch (_Season.Value)
        //     {
        //         case Season.Fall:
        //             color = _FallColor1.Value;
        //             break;
        //         case Season.Spring:
        //             color = _SpringColor1.Value;
        //             break;
        //         case Season.Winter:
        //             color = _WinterColor1.Value;
        //             break;
        //         case Season.Summer:
        //             color = _SummerColor1.Value;
        //             break;
        //     }
        //
        //     vineMat.color = color;
        // }
        
        if (!tex) return;
        // List of 4
        for (int i = 0; i < materialsList.Count; ++i)
        {
            // Unknown list of materials
            for (int j = 0; j < materialsList[i].Length; ++j)
            {
                string materialName = materialsList[i][j].name.ToLower().Replace("(instance)", "");
                if (materialName.Contains("vinesbranch")
                    || materialName.Contains("bark")
                    || materialName.Contains("trunk")
                    || materialName.Contains("log")
                    || materialName.Contains("wood")
                    || materialName.Contains("stump"))
                {
                    if (!isBark) continue;
                    // SeasonalityLogger.LogWarning(materialName + " is bark, applying " + tex.name);
                    materialsList[i][j].SetTexture(propertyName, tex);
                }
                else
                {
                    if (isBark) continue;
                    // SeasonalityLogger.LogWarning(materialName + " is not bark, applying " + tex.name);
                    materialsList[i][j].SetTexture(propertyName, tex);
                    materialsList[i][j].color = _Season.Value is Season.Fall ? SeasonColors.FallColors[i] : Color.white;
                }

            }
        }
    }
    private static void ModifyMossTex(string propertyName, Material material, VegetationType type)
    {
        Texture? tex = null;
        // Get default moss texture based on type
        switch (type)
        {
            case VegetationType.RockPlains: tex = CachedTextures["rock_heath_moss"]; break;
            case VegetationType.Swamp: tex = CachedTextures["swamptree1_bark_moss"]; break;
            default: tex = CachedTextures["rock4_coast_moss"]; break;
        }
        
        switch (_Season.Value)
        {
            case Season.Winter:
                switch (type)
                {
                    case VegetationType.RockPlains:
                        Texture? plainsWinter = Utils.GetCustomTexture(VegDirectories.PlainsMoss, Season.Winter.ToString());
                        if (plainsWinter) tex = plainsWinter;
                        break;
                    case VegetationType.Swamp:
                        Texture? swampWinter = Utils.GetCustomTexture(VegDirectories.SwampMoss, Season.Winter.ToString());
                        if (swampWinter) tex = swampWinter;
                        break;
                    default:
                        Texture? customWinter = Utils.GetCustomTexture(VegDirectories.Moss, Season.Winter.ToString());
                        if (customWinter) tex = customWinter;
                        break;
                }
                break;
            case Season.Fall:
                switch (type)
                {
                    case VegetationType.RockPlains:
                        Texture? plainsWinter = Utils.GetCustomTexture(VegDirectories.PlainsMoss, Season.Fall.ToString());
                        if (plainsWinter) tex = plainsWinter;
                        break;
                    case VegetationType.Swamp:
                        Texture? swampWinter = Utils.GetCustomTexture(VegDirectories.SwampMoss, Season.Fall.ToString());
                        if (swampWinter) tex = swampWinter;
                        break;
                    default:
                        tex = CachedTextures["rock_heath_moss"];
                        Texture? customFall = Utils.GetCustomTexture(VegDirectories.Moss, Season.Fall.ToString());
                        if (customFall) tex = customFall;
                        break;
                }
                break;
            case Season.Spring:
                switch (type)
                {
                    case VegetationType.RockPlains:
                        Texture? plainsWinter = Utils.GetCustomTexture(VegDirectories.PlainsMoss, Season.Spring.ToString());
                        if (plainsWinter) tex = plainsWinter;
                        break;
                    case VegetationType.Swamp:
                        Texture? swampWinter = Utils.GetCustomTexture(VegDirectories.SwampMoss, Season.Spring.ToString());
                        if (swampWinter) tex = swampWinter;
                        break;
                    default:
                        Texture? customSpring = Utils.GetCustomTexture(VegDirectories.Moss, Season.Spring.ToString());
                        if (customSpring) tex = customSpring;
                        break;
                }
                break;
            case Season.Summer:
                switch (type)
                {
                    case VegetationType.RockPlains:
                        Texture? plainsWinter = Utils.GetCustomTexture(VegDirectories.PlainsMoss, Season.Summer.ToString());
                        if (plainsWinter) tex = plainsWinter;
                        break;
                    case VegetationType.Swamp:
                        Texture? swampWinter = Utils.GetCustomTexture(VegDirectories.SwampMoss, Season.Summer.ToString());
                        if (swampWinter) tex = swampWinter;
                        break;
                    default:
                        Texture? customSummer = Utils.GetCustomTexture(VegDirectories.Moss, Season.Summer.ToString());
                        if (customSummer) tex = customSummer;
                        break;
                }
                break;
        }
        
        if (tex) material.SetTexture(propertyName, tex);
    }
}