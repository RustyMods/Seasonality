using HarmonyLib;
using UnityEngine;

namespace Seasonality;

public static class SnowBuildUp
{
    private static void UpdateSnow(WearNTear wnt)
    {
        if (wnt.m_biome is Heightmap.Biome.DeepNorth or Heightmap.Biome.AshLands) return;
        if (!wnt.m_snow) return;
        if (SeasonTime.m_currentSeason is Season.Winter)
        {
            bool haveRoof = wnt.HaveRoof();

            if (haveRoof)
            {
                if (wnt.m_snowBuildup <= 0f) return;
                wnt.m_nview.GetZDO().Set(ZDOVars.s_snow, 0f);
            }
            else
            {
                if (wnt.m_snowBuildup >= 1f) return;
                var value = Mathf.Max(0.26f, Mathf.Clamp01(wnt.m_snowBuildup + Configs.m_snowBuildupSpeed.Value));
                wnt.m_nview.GetZDO().Set(ZDOVars.s_snow, value);
            }
        }
        else
        {
            if (wnt.m_snowBuildup <= 0f) return;
            wnt.m_nview.GetZDO().Set(ZDOVars.s_snow, 0f);
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.UpdateWear))]
    private static class WearNTear_UpdateWear
    {
        private static void Prefix(WearNTear __instance, float time)
        {
            if (Configs.m_addSnow.Value is Toggle.Off) return;
            if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner()) return;
            UpdateSnow(__instance);
        }
    }
}