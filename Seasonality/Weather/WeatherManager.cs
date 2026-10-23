using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Seasonality;

public static partial class WeatherManager
{
    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.GetAvailableEnvironments))]
    private static class Override_Environments
    {
        private static void Postfix(BiomeSector biome, ref List<EnvEntry> __result)
        {
            if (Configs.m_weatherEnabled.Value is Toggle.Off) return;
            if (biome.Biome is Heightmap.Biome.None or Heightmap.Biome.AshLands or Heightmap.Biome.DeepNorth) return;
            __result = GetEnvironments(biome.Biome, __result);
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
    private static class Initialize_Weather_Duration
    {
        private static void Postfix(EnvMan __instance)
        {
            if (!__instance) return;
            
            if (Configs.m_weatherEnabled.Value is Toggle.Off) return;
            __instance.m_environmentDuration = Configs.m_weatherDuration.Value * 60;
        }
    }
    

    public static void OnWeatherDurationChange(object sender, EventArgs e)
    {
        if (!EnvMan.instance) return;
        if (Configs.m_weatherEnabled.Value is Toggle.Off) return;
        EnvMan.instance.m_environmentDuration = Configs.m_weatherDuration.Value * 60;
    }

    private static List<EnvEntry> GetEnvironments(Heightmap.Biome biome, List<EnvEntry> entries)
    {
        if (!EnvMan.instance) return entries;
        List<EnvEntry> output = [];
        if (!Configs.m_weatherOptions.TryGetValue(SeasonTime.GetCurrentSeason(), out Dictionary<Heightmap.Biome, ConfigEntry<string>> configs)) return entries;
        if (!configs.TryGetValue(biome, out ConfigEntry<string> config)) return entries;
        if (config.Value.IsNullOrWhiteSpace()) return entries;
        
        foreach (WeatherConfig.WeatherData weather in new WeatherConfig(config.Value).environments)
        {
            if (EnvMan.instance.GetEnv(weather.name) is not { } setup) continue;
            var entry = new EnvEntry()
            {
                m_environment = weather.name,
                m_weight = weather.weight,
                m_env = setup
            };
            output.Add(entry);
        }
        return output.Count > 0 ? output : entries;
    }
    






    
}