using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class BossStonePatcher
{
    [HarmonyPatch(typeof(BossStone), nameof(BossStone.Start))]
    static class BossStonePatch
    {
        private static void Postfix(BossStone __instance)
        {
            if (!__instance) return;
            if (_ModEnabled.Value is Toggle.Off) return;
            Texture? snowTex = Utils.GetCustomTexture(CustomTextures.VegDirectories.Moss, Season.Winter.ToString());
            switch (_Season.Value)
            {
                case Season.Winter:
                    if (snowTex) Utils.SetMossTex(__instance.gameObject, snowTex);
                    break;
                case Season.Fall:
                    Utils.SetMossTex(__instance.gameObject, CacheTextures.CachedTextures["rock_heath_moss"]);
                    break;
            }
        }
    }
}