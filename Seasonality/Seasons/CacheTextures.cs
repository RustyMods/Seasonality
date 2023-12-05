using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Seasonality.Seasons;

public static class CacheTextures
{
    public static Dictionary<string, Texture> CachedTextures = new();
    public static Dictionary<string, Material> CachedMaterials = new();

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    static class ZoneSystemPatch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            GetAllMaterials();
            GetAllTextures();
        }
        private static void GetAllMaterials()
        {
            Material[] allMats = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material item in allMats)
            {
                if (!item) continue;
                CachedMaterials[item.name.Replace("(Instance)", "")] = item;
            }
        }

        private static void GetAllTextures()
        {
            foreach (Material material in CachedMaterials.Values)
            {
                if (!material) continue;
                string[] properties = material.GetTexturePropertyNames();
                if (Utils.FindTexturePropName(properties, "moss", out string mossProp))
                {
                    Texture? tex = material.GetTexture(mossProp);
                    if (tex) CachedTextures[material.name.Replace("(Instance)", "").Replace(" ", "") + "_moss"] = tex;
                }
                if (!Utils.FindTexturePropName(properties, "main", out string mainProp)) continue;
                CachedTextures[material.name.Replace("(Instance)", "").Replace(" ", "")] = material.GetTexture(mainProp);
            }
        }
    }
    
}