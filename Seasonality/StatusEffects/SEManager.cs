using HarmonyLib;
using UnityEngine;

namespace Seasonality;

public static class SEManager
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateEnvStatusEffects))]
    private static class Player_UpdateEnvStatusEffects_Patch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance.GetSEMan().HaveStatusEffect("SE_Seasons".GetStableHashCode()))
            {
                __instance.GetSEMan().AddStatusEffect("SE_Seasons".GetStableHashCode());
            }

            if (!__instance.GetSEMan().HaveStatusEffect("SE_Weather".GetStableHashCode()))
            {
                __instance.GetSEMan().AddStatusEffect("SE_Weather".GetStableHashCode());
            }
        }
    }
    
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix()
        {
            if (!ObjectDB.instance || !ZNetScene.instance) return;
            SE_Season season = ScriptableObject.CreateInstance<SE_Season>();
            season.name = "SE_Seasons";
            season.m_name = "Seasonality";
            season.m_icon = SE_Season.GetIcon();
            if (!ObjectDB.instance.m_StatusEffects.Contains(season))
            {
                ObjectDB.instance.m_StatusEffects.Add(season);
            }
            
            SE_WeatherMan weather = ScriptableObject.CreateInstance<SE_WeatherMan>();
            weather.name = "SE_Weather";
            weather.m_name = "Weatherman";
            weather.m_icon = SpriteManager.ValknutIcon;
            if (ObjectDB.instance.m_StatusEffects.Contains(weather)) return;
            ObjectDB.instance.m_StatusEffects.Add(weather);
        }
    }
}