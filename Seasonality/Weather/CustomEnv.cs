using HarmonyLib;

namespace Seasonality;

public static partial class WeatherManager
{
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
    private static class ZoneSystem_SetupLocations_Patch
    {
        private static void Postfix()
        {
            RegisterEnvironments(EnvMan.instance);
        }
    }
    
    private static void RegisterEnvironments(EnvMan __instance)
    {
        EnvSetup? snow = __instance.GetEnv("Snow");
        EnvSetup? snowStorm = __instance.GetEnv("SnowStorm");
        EnvSetup? twilightSnow = __instance.GetEnv("Twilight_SnowStorm");
        
        if (snow is null || snowStorm is null) return;
        
        EnvSetup? warmSnow = snow.Clone();
        warmSnow.m_isFreezing = false;
        warmSnow.m_isFreezingAtNight = false;
        warmSnow.m_name = "WarmSnow";

        EnvSetup? warmSnowStorm = snowStorm.Clone();
        warmSnowStorm.m_isFreezing = false;
        warmSnowStorm.m_isFreezingAtNight = false;
        warmSnowStorm.m_name = "WarmSnowStorm";

        EnvSetup? nightFrost = snow.Clone();
        nightFrost.m_isFreezing = false;
        nightFrost.m_name = "NightFrost";
        nightFrost.m_isFreezingAtNight = true;

        var warmTwilight = twilightSnow.Clone();
        warmTwilight.m_isFreezing = false;
        warmTwilight.m_isFreezingAtNight = false;
        warmTwilight.m_name = "Warm_Twilight_SnowStorm";
        
        RegisterEnv(warmSnow);
        RegisterEnv(warmSnowStorm);
        RegisterEnv(nightFrost);
        RegisterEnv(warmTwilight);
    }
    
    private static void RegisterEnv(EnvSetup setup)
    {
        if (!EnvMan.instance || EnvMan.instance.m_environments.Contains(setup)) return;
        EnvMan.instance.m_environments.Add(setup);
    }
}