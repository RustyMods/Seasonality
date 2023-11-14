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
            
            // Goblin king location not affected ??
            // GDking location either
            
            GameObject? prefab = __instance.m_instance;
            if (!prefab) return;
            switch (_Season.Value)
            {
                case Season.Winter:
                    Utils.SetMossTex(prefab, CustomTextures.SnowTexture);
                    break;
            }
        }

        
    }
}