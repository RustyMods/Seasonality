using HarmonyLib;
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
            Texture? snowTex = Utils.GetCustomTexture(CustomTextures.VegDirectories.Moss, Season.Winter);
            switch (_Season.Value)
            {
                case Season.Winter:
                    if (!parent)
                    {
                        if (snowTex) Utils.SetMossTex(altar, snowTex);
                        break;
                    }
                    if (snowTex) Utils.SetMossTex(parent.gameObject, snowTex);
                    break;
                
            }
        }
        
    }
}