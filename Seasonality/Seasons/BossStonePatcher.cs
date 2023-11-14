using HarmonyLib;
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
            switch (_Season.Value)
            {
                case Season.Winter:
                    Utils.SetMossTex(__instance.gameObject, CustomTextures.SnowTexture);
                    break;
            }
        }
    }
}