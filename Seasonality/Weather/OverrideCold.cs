using HarmonyLib;

namespace Seasonality;

public static partial class WeatherManager
{
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
    private static class Override_Cold
    {
        private static void Postfix(ref bool __result)
        {
            if (!Player.m_localPlayer) return;

            switch (SeasonTime.m_currentSeason)
            {
                case Season.Winter:
                    if (Configs.m_winterAlwaysCold.Value is Toggle.Off) return;
                    if (Player.m_localPlayer.GetCurrentBiome() is Heightmap.Biome.AshLands) return;
                    __result = true;
                    break;
                case Season.Summer:
                    if (Configs.m_summerAlwaysWarm.Value is Toggle.Off) return;
                    if (Player.m_localPlayer.GetCurrentBiome() is Heightmap.Biome.Mountain or Heightmap.Biome.DeepNorth)
                        return;
                    __result = false;
                    break;
            }
            
        }
    }
}