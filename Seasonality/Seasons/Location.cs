using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class Location
{
    [HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.Awake))]
    static class LocationProxyAwakePatch
    {
        private static void Postfix(LocationProxy __instance)
        {
            if (!__instance) return;
            if (_ModEnabled.Value is Toggle.Off) return;
            
            GameObject? prefab = __instance.m_instance;
            if (!prefab) return;
            Texture? snowTex = Utils.GetCustomTexture(CustomTextures.VegDirectories.Moss, Season.Winter.ToString());
            switch (_Season.Value)
            {
                case Season.Winter:
                    if (snowTex) Utils.SetMossTex(prefab, snowTex);
                    break;
                case Season.Fall:
                    Utils.SetMossTex(prefab, Vegetation.HeathMossTexture);
                    break;
            }
        }

        
    }
}