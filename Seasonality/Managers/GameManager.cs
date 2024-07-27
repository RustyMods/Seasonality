using HarmonyLib;
using Seasonality.Behaviors;

namespace Seasonality.Managers;

public static class GameManager
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    private static class Game_Start_Patch
    {
        private static void Postfix(Game __instance)
        {
            if (!__instance) return;
            // GameObject _GameMain = __instance.gameObject;
            // Transform WaterPlane = Utils.FindChild(_GameMain.transform, "WaterPlane");
            // if (!WaterPlane) return;
            // Transform? m_waterSurface = Utils.FindChild(__instance.gameObject.transform, "WaterPlane").GetChild(0);
            try
            {
                Utils.FindChild(__instance.gameObject.transform, "WaterPlane").GetChild(0).gameObject
                    .AddComponent<FrozenWaterLOD>();
            }
            catch
            {
                SeasonalityPlugin.SeasonalityLogger.LogDebug("Failed to find water LOD");
            }
        }
    }
}