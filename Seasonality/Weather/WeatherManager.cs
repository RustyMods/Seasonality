using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Seasonality.Configurations;
using Seasonality.Textures;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Weather.Utils;

namespace Seasonality.Weather;

public static class WeatherManager
{
    private static string currentEnv = "";

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.GetAvailableEnvironments))]
    private static class EnvManGetEnvironmentPatch
    {
        private static void Postfix(Heightmap.Biome biome, ref List<EnvEntry> __result)
        {
            if (_ModEnabled.Value is Toggle.Off || _WeatherControl.Value is Toggle.Off) return;
            if (GetEnvironmentEntries(biome, out List<EnvEntry> entries))
            {
                __result = entries;
            }
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateEnvironment))]
    private static class EnvManUpdatePatch
    {
        private static void Postfix(EnvMan __instance)
        {
            if (!__instance || !Player.m_localPlayer) return;
            
            if (_ModEnabled.Value is Toggle.Off || _WeatherControl.Value is Toggle.Off)
            {
                if (!Player.m_localPlayer.GetSEMan().HaveStatusEffect("WeatherMan_SE".GetStableHashCode())) return;
                Player.m_localPlayer.GetSEMan().RemoveStatusEffect("WeatherMan_SE".GetStableHashCode());
                return;
            }
            
            __instance.m_environmentDuration = _WeatherDuration.Value * 60;
            if (__instance.m_currentEnv == null) return;
            if (__instance.m_currentEnv.m_name.IsNullOrWhiteSpace()) return;
            if (!Player.m_localPlayer.GetSEMan().HaveStatusEffect("WeatherMan_SE".GetStableHashCode()))
            {
                SetWeatherMan(__instance.m_currentEnv.m_name);
            }
            
            if (__instance.m_currentEnv.m_name != currentEnv)
            {
                SetWeatherMan(__instance.m_currentEnv.m_name);
            }
        }
    }
    private static bool GetEnvironmentEntries(Heightmap.Biome biome, out List<EnvEntry> result)
    {
        if (_YamlConfigurations.Value is Toggle.On)
        {
            if (GetYmlEnvironments(biome, out List<EnvEntry> YmlEntries))
            {
                result = YmlEntries;
                return true;
            };
            result = new List<EnvEntry>();
            return false;
        }
        if (GetConfigEnvironments(biome, out List<EnvEntry> configEntries))
        {
            result = configEntries;
            return true;
        }
        result = new List<EnvEntry>();
        return false;
    }

    private static bool GetConfigEnvironments(Heightmap.Biome biome, out List<EnvEntry> result)
    {
        List<Environments> configs = new();

        result = new List<EnvEntry>();
        
        switch (biome)
            {
                case Heightmap.Biome.Meadows:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Meadows_Weather1.Value);
                            configs.Add(_Winter_Meadows_Weather2.Value);
                            configs.Add(_Winter_Meadows_Weather3.Value);
                            configs.Add(_Winter_Meadows_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Meadows_Weather1.Value);
                            configs.Add(_Fall_Meadows_Weather2.Value);
                            configs.Add(_Fall_Meadows_Weather3.Value);
                            configs.Add(_Fall_Meadows_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Meadows_Weather1.Value);
                            configs.Add(_Spring_Meadows_Weather2.Value);
                            configs.Add(_Spring_Meadows_Weather3.Value);
                            configs.Add(_Spring_Meadows_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Meadows_Weather1.Value);
                            configs.Add(_Summer_Meadows_Weather2.Value);
                            configs.Add(_Summer_Meadows_Weather3.Value);
                            configs.Add(_Summer_Meadows_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.BlackForest:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_BlackForest_Weather1.Value);
                            configs.Add(_Winter_BlackForest_Weather2.Value);
                            configs.Add(_Winter_BlackForest_Weather3.Value);
                            configs.Add(_Winter_BlackForest_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_BlackForest_Weather1.Value);
                            configs.Add(_Fall_BlackForest_Weather2.Value);
                            configs.Add(_Fall_BlackForest_Weather3.Value);
                            configs.Add(_Fall_BlackForest_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_BlackForest_Weather1.Value);
                            configs.Add(_Spring_BlackForest_Weather2.Value);
                            configs.Add(_Spring_BlackForest_Weather3.Value);
                            configs.Add(_Spring_BlackForest_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_BlackForest_Weather1.Value);
                            configs.Add(_Summer_BlackForest_Weather2.Value);
                            configs.Add(_Summer_BlackForest_Weather3.Value);
                            configs.Add(_Summer_BlackForest_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Swamp:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Swamp_Weather1.Value);
                            configs.Add(_Winter_Swamp_Weather2.Value);
                            configs.Add(_Winter_Swamp_Weather3.Value);
                            configs.Add(_Winter_Swamp_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Swamp_Weather1.Value);
                            configs.Add(_Fall_Swamp_Weather2.Value);
                            configs.Add(_Fall_Swamp_Weather3.Value);
                            configs.Add(_Fall_Swamp_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Swamp_Weather1.Value);
                            configs.Add(_Spring_Swamp_Weather2.Value);
                            configs.Add(_Spring_Swamp_Weather3.Value);
                            configs.Add(_Spring_Swamp_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Swamp_Weather1.Value);
                            configs.Add(_Summer_Swamp_Weather2.Value);
                            configs.Add(_Summer_Swamp_Weather3.Value);
                            configs.Add(_Summer_Swamp_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Mountain:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Mountains_Weather1.Value);
                            configs.Add(_Winter_Mountains_Weather2.Value);
                            configs.Add(_Winter_Mountains_Weather3.Value);
                            configs.Add(_Winter_Mountains_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Mountains_Weather1.Value);
                            configs.Add(_Fall_Mountains_Weather2.Value);
                            configs.Add(_Fall_Mountains_Weather3.Value);
                            configs.Add(_Fall_Mountains_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Mountains_Weather1.Value);
                            configs.Add(_Spring_Mountains_Weather2.Value);
                            configs.Add(_Spring_Mountains_Weather3.Value);
                            configs.Add(_Spring_Mountains_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Mountains_Weather1.Value);
                            configs.Add(_Summer_Mountains_Weather2.Value);
                            configs.Add(_Summer_Mountains_Weather3.Value);
                            configs.Add(_Summer_Mountains_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Plains:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Plains_Weather1.Value);
                            configs.Add(_Winter_Plains_Weather2.Value);
                            configs.Add(_Winter_Plains_Weather3.Value);
                            configs.Add(_Winter_Plains_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Plains_Weather1.Value);
                            configs.Add(_Fall_Plains_Weather2.Value);
                            configs.Add(_Fall_Plains_Weather3.Value);
                            configs.Add(_Fall_Plains_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Plains_Weather1.Value);
                            configs.Add(_Spring_Plains_Weather2.Value);
                            configs.Add(_Spring_Plains_Weather3.Value);
                            configs.Add(_Spring_Plains_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Plains_Weather1.Value);
                            configs.Add(_Summer_Plains_Weather2.Value);
                            configs.Add(_Summer_Plains_Weather3.Value);
                            configs.Add(_Summer_Plains_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Mistlands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_MistLands_Weather1.Value);
                            configs.Add(_Winter_MistLands_Weather2.Value);
                            configs.Add(_Winter_MistLands_Weather3.Value);
                            configs.Add(_Winter_MistLands_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_MistLands_Weather1.Value);
                            configs.Add(_Fall_MistLands_Weather2.Value);
                            configs.Add(_Fall_MistLands_Weather3.Value);
                            configs.Add(_Fall_MistLands_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_MistLands_Weather1.Value);
                            configs.Add(_Spring_MistLands_Weather2.Value);
                            configs.Add(_Spring_MistLands_Weather3.Value);
                            configs.Add(_Spring_MistLands_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_MistLands_Weather1.Value);
                            configs.Add(_Summer_MistLands_Weather2.Value);
                            configs.Add(_Summer_MistLands_Weather3.Value);
                            configs.Add(_Summer_MistLands_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Ocean:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Ocean_Weather1.Value);
                            configs.Add(_Winter_Ocean_Weather2.Value);
                            configs.Add(_Winter_Ocean_Weather3.Value);
                            configs.Add(_Winter_Ocean_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Ocean_Weather1.Value);
                            configs.Add(_Fall_Ocean_Weather2.Value);
                            configs.Add(_Fall_Ocean_Weather3.Value);
                            configs.Add(_Fall_Ocean_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Ocean_Weather1.Value);
                            configs.Add(_Spring_Ocean_Weather2.Value);
                            configs.Add(_Spring_Ocean_Weather3.Value);
                            configs.Add(_Spring_Ocean_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Ocean_Weather1.Value);
                            configs.Add(_Summer_Ocean_Weather2.Value);
                            configs.Add(_Summer_Ocean_Weather3.Value);
                            configs.Add(_Summer_Ocean_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.AshLands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_AshLands_Weather1.Value);
                            configs.Add(_Winter_AshLands_Weather2.Value);
                            configs.Add(_Winter_AshLands_Weather3.Value);
                            configs.Add(_Winter_AshLands_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_AshLands_Weather1.Value);
                            configs.Add(_Fall_AshLands_Weather2.Value);
                            configs.Add(_Fall_AshLands_Weather3.Value);
                            configs.Add(_Fall_AshLands_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_AshLands_Weather1.Value);
                            configs.Add(_Spring_AshLands_Weather2.Value);
                            configs.Add(_Spring_AshLands_Weather3.Value);
                            configs.Add(_Spring_AshLands_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_AshLands_Weather1.Value);
                            configs.Add(_Summer_AshLands_Weather2.Value);
                            configs.Add(_Summer_AshLands_Weather3.Value);
                            configs.Add(_Summer_AshLands_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.DeepNorth:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_DeepNorth_Weather1.Value);
                            configs.Add(_Winter_DeepNorth_Weather2.Value);
                            configs.Add(_Winter_DeepNorth_Weather3.Value);
                            configs.Add(_Winter_DeepNorth_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_DeepNorth_Weather1.Value);
                            configs.Add(_Fall_DeepNorth_Weather2.Value);
                            configs.Add(_Fall_DeepNorth_Weather3.Value);
                            configs.Add(_Fall_DeepNorth_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_DeepNorth_Weather1.Value);
                            configs.Add(_Spring_DeepNorth_Weather2.Value);
                            configs.Add(_Spring_DeepNorth_Weather3.Value);
                            configs.Add(_Spring_DeepNorth_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_DeepNorth_Weather1.Value);
                            configs.Add(_Summer_DeepNorth_Weather2.Value);
                            configs.Add(_Summer_DeepNorth_Weather3.Value);
                            configs.Add(_Summer_DeepNorth_Weather4.Value);
                            break;
                    }
                    break;
            }

        if (configs.TrueForAll(x => x is Environments.None))
        {
            return false;
        }
        
        foreach (Environments value in configs)
        {
            if (value is Environments.None) continue;
            EnvEntry entry = new EnvEntry()
            {
                m_environment = GetEnvironmentName(value),
                m_weight = 1f,
                m_env = EnvMan.instance.GetEnv(GetEnvironmentName(value))
            };
            result.Add(entry);
        }
        return true;
    }

    private static bool GetYmlEnvironments(Heightmap.Biome biome, out List<EnvEntry> result)
    {
        result = new List<EnvEntry>();

        switch (biome)
            {
                case Heightmap.Biome.Meadows:
                    switch(_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.meadowWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.meadowWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.meadowWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.meadowWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                    }

                    break;
                case Heightmap.Biome.BlackForest:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.blackForestWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.blackForestWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.blackForestWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.blackForestWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                    }
                    break;
                case Heightmap.Biome.Swamp:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.swampWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.swampWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.swampWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.swampWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                    }
                    break;
                case Heightmap.Biome.Mountain:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.mountainWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.mountainWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.mountainWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.mountainWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                    }
                    break;
                case Heightmap.Biome.Plains:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.plainWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.plainWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.plainWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.plainWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                    }
                    break;
                case Heightmap.Biome.Mistlands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.mistLandWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.mistLandWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.mistLandWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.mistLandWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                    }
                    break;
                case Heightmap.Biome.AshLands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.ashLandWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.ashLandWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.ashLandWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.ashLandWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }
                            break;
                    }
                    break;
                case Heightmap.Biome.DeepNorth:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.deepNorthWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }

                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.deepNorthWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }

                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.deepNorthWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }

                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.deepNorthWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }

                            break;
                    }

                    break;
                case Heightmap.Biome.Ocean:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            foreach (string option in YamlConfigurations.winterData.oceanWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }

                            break;
                        case Season.Fall:
                            foreach (string option in YamlConfigurations.fallData.oceanWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }

                            break;
                        case Season.Summer:
                            foreach (string option in YamlConfigurations.summerData.oceanWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }

                            break;
                        case Season.Spring:
                            foreach (string option in YamlConfigurations.springData.oceanWeather)
                            {
                                result.Add(new EnvEntry(){m_environment = option, m_weight = 1f, m_env = EnvMan.instance.GetEnv(option)});
                            }

                            break;
                    }

                    break;
            }

        if (result.Count == 0) return false;

        return true;
    }
    
    public static string GetEnvironmentCountDown()
    {
        if (EnvMan.instance == null || _WeatherTimerEnabled.Value == Toggle.Off) return "";
        
        double seed = EnvMan.instance.m_totalSeconds / EnvMan.instance.m_environmentDuration;
        double fraction = 1 - (seed - Math.Truncate(seed));
        double total = fraction * EnvMan.instance.m_environmentDuration;

        int hours = TimeSpan.FromSeconds(total).Hours;
        int minutes = TimeSpan.FromSeconds(total).Minutes;
        int seconds = TimeSpan.FromSeconds(total).Seconds;
    
        if (total < 0) return "";
        
        return hours > 0 ? $"{hours}:{minutes:D2}:{seconds:D2}" : minutes > 0 ? $"{minutes}:{seconds:D2}" : $"{seconds}";
    }
    private static void SetWeatherMan(string env)
    {
        if (!Player.m_localPlayer) return;
        
        EnvironmentEffectData EnvData = new EnvironmentEffectData()
        {
            name = "WeatherMan_SE",
            m_name = Localization.instance.Localize(GetEnvironmentDisplayName(env)),
            m_sprite = SpriteManager.ValknutIcon,
            m_start_msg = Localization.instance.Localize("$weather_changing_to") + Localization.instance.Localize(GetEnvironmentDisplayName(env)),
            m_tooltip = Localization.instance.Localize(GetEnvironmentTooltip(env)) 
        };
        if (Player.m_localPlayer.GetSEMan().HaveStatusEffect("WeatherMan_SE".GetStableHashCode()))
        {
            Player.m_localPlayer.GetSEMan().RemoveStatusEffect("WeatherMan_SE".GetStableHashCode());
        }
        StatusEffect WeatherEffect = EnvData.InitEnvEffect();
        Player.m_localPlayer.GetSEMan().AddStatusEffect(WeatherEffect);
        currentEnv = env;
        SeasonalityLogger.LogDebug("Weatherman: setting new weather to " + env);
    }
}