﻿using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class OfferingBowlPatcher
{
    [HarmonyPatch(typeof(OfferingBowl), nameof(OfferingBowl.Awake))]
    static class OfferingBowlPatch
    {
        private static void Postfix(OfferingBowl __instance)
        {
            if (!__instance) return;
            if (_ModEnabled.Value is Toggle.Off) return;
            
            GameObject altar = __instance.gameObject;
            Transform? parent = altar.transform.parent;
            if (!parent) return;
            switch (_Season.Value)
            {
                case Season.Winter:
                    if (!parent)
                    {
                        Utils.SetMossTex(altar, CustomTextures.SnowTexture);
                        break;
                    }
                    Utils.SetMossTex(parent.gameObject, CustomTextures.SnowTexture);
                    break;
                
            }
        }
        
    }
}