using System;
using System.Collections.Generic;
using HarmonyLib;
using Seasonality.Configurations;
using Seasonality.Textures;
using ServerSync;
using YamlDotNet.Serialization;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Weather.Utils;

namespace Seasonality.Weather;

public static class WeatherManager
{
    private static readonly CustomSyncedValue<string> SyncedWeatherData = new(SeasonalityPlugin.ConfigSync, "ServerWeather", "");
    
    private static readonly Dictionary<Heightmap.Biome, List<EnvEntry>> ServerWeatherMap = new();
    private static readonly Dictionary<Heightmap.Biome, int> ServerWeatherIndexes = new ();
    
    private static Heightmap.Biome lastBiome = Heightmap.Biome.None;
    private static Season lastSeason = Season.Fall;
    private static string currentEnv = null!;
    private static bool WeatherTweaked;

    private static int MeadowIndex;
    private static int BlackForestIndex;
    private static int SwampIndex;
    private static int MountainIndex;
    private static int PlainsIndex;
    private static int MistLandsIndex;
    private static int AshLandsIndex;
    private static int DeepNorthIndex;
    private static int OceanIndex;
    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateEnvironment))]
    public static class EnvManPatch
    {
        private static bool Prefix(EnvMan __instance, long sec, Heightmap.Biome biome)
        {
            if (_ModEnabled.Value is Toggle.Off || _WeatherControl.Value is Toggle.Off)
            {
                if (!Player.m_localPlayer) return true;
                if (!Player.m_localPlayer.GetSEMan().HaveStatusEffect("WeatherMan_SE".GetStableHashCode())) return true;
                Player.m_localPlayer.GetSEMan().RemoveStatusEffect("WeatherMan_SE".GetStableHashCode());
                return true;
            }
            
            if (workingAsType is WorkingAs.Server)
            {
                ServerSyncedWeatherMan(__instance);
                return false;
            }
            
            // If client is overriding weather system
            string environmentOverride = __instance.GetEnvironmentOverride();
            if (!string.IsNullOrEmpty(environmentOverride))
            {
                // If debug mode is active and user forces environment
                __instance.m_environmentPeriod = -1L;
                __instance.m_currentBiome = __instance.GetBiome();
                __instance.QueueEnvironment(environmentOverride);
                if (__instance.m_currentEnv.m_name == currentEnv) return false;
                SetWeatherMan(environmentOverride);
                currentEnv = __instance.m_currentEnv.m_name;
                WeatherTweaked = false;
                return false;
            }

            if (!Player.m_localPlayer) return true;
            if (Player.m_localPlayer.IsDead()) return true;

            if (!Player.m_localPlayer.GetSEMan().HaveStatusEffect("WeatherMan_SE".GetStableHashCode()))
            {
                SetWeatherMan(__instance.m_currentEnv.m_name);
            }
            
            if (__instance.m_currentEnv.m_name != currentEnv)
            {
                SetWeatherMan(__instance.m_currentEnv.m_name);
                currentEnv = __instance.m_currentEnv.m_name;
            }

            if (biome == Heightmap.Biome.None) return true;

            List<EnvEntry> entries = new();
            List<Environments> configs = new();

            if (_YamlConfigurations.Value is Toggle.On)
            {
                switch (biome)
                {
                    case Heightmap.Biome.Meadows:
                        switch(_Season.Value)
                        {
                            case Season.Winter:
                                foreach (string option in YamlConfigurations.winterData.meadowWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.meadowWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.meadowWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.meadowWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.blackForestWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.blackForestWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.summerData.blackForestWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.swampWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.swampWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.swampWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.mountainWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.mountainWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.mountainWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.plainWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.plainWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.plainWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.mistLandWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.mistLandWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.mistLandWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.ashLandWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.ashLandWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.ashLandWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.deepNorthWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.deepNorthWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.deepNorthWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.oceanWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.oceanWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.oceanWeather)
                                {
                                    entries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                        }

                        break;
                }
            }
            else
            {
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
            }

            if (configs.TrueForAll(x => x is Environments.None) && _YamlConfigurations.Value is Toggle.Off)
            {
                CheckVanillaLocaleChange(__instance, biome);
                WeatherTweaked = false;
                return true;
            }

            switch (_YamlConfigurations.Value)
            {
                case Toggle.Off:
                    AddToEntries(configs, entries);
                    break;
                case Toggle.On when entries.Count == 0:
                    CheckVanillaLocaleChange(__instance, biome);
                    WeatherTweaked = false;
                    return true;
            }

            // If client is server
            if (ZNet.instance.IsServer()) return LocalWeatherMan(__instance, sec, entries, biome);
            
            // If client is connected to server, then use server index
            if (lastBiome != biome)
            {
                if (SyncedWeatherData.Value == "") return LocalWeatherMan(__instance, sec, entries, biome);
                ServerSyncedChangeWeather(biome, __instance, entries, sec, false);
                lastBiome = biome;
                return false;
            }

            if (lastSeason != _Season.Value)
            {
                if (SyncedWeatherData.Value == "") return LocalWeatherMan(__instance, sec, entries, biome);
                ServerSyncedChangeWeather(biome, __instance, entries, sec, false);
                lastSeason = _Season.Value;
                return false;
            }

            long duration = _WeatherDuration.Value * 60; // Total seconds

            if (duration == 0)
            {
                // Throttle weather change to a minimum of 1 minute
                if ((lastEnvironmentChange + 60) - EnvMan.instance.m_totalSeconds > 0) return false;
                ServerSyncedChangeWeather(biome, __instance, entries, sec);
            }

            if ((lastEnvironmentChange + duration) - EnvMan.instance.m_totalSeconds > 0)
            {
                if (!WeatherTweaked) ServerSyncedChangeWeather(biome, __instance, entries, sec, false);
                return false;
            }
            ServerSyncedChangeWeather(biome, __instance, entries, sec);
            return false;
        }
    }
    
    private static void CheckVanillaLocaleChange(EnvMan instance, Heightmap.Biome currentBiome)
    {
        if (instance.m_currentEnv.m_name != currentEnv)
        {
            SetWeatherMan(instance.m_currentEnv.m_name);
            currentEnv = instance.m_currentEnv.m_name;
        }
        if (lastBiome != currentBiome)
        {
            instance.m_environmentPeriod = instance.m_environmentDuration + 1;
            lastBiome = currentBiome;
        }
        if (lastSeason != _Season.Value)
        {
            instance.m_environmentPeriod = instance.m_environmentDuration + 1;
            lastSeason = _Season.Value;
        }
    }
    private static void ServerSyncedWeatherMan(EnvMan __instance)
    {
        foreach (Heightmap.Biome land in Enum.GetValues(typeof(Heightmap.Biome)))
        {
            if (land is Heightmap.Biome.None) continue;
            
            List<Environments> weathers = new();
            switch (land)
            {
                case Heightmap.Biome.Meadows:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_Meadows_Weather1.Value);
                            weathers.Add(_Winter_Meadows_Weather2.Value);
                            weathers.Add(_Winter_Meadows_Weather3.Value);
                            weathers.Add(_Winter_Meadows_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_Meadows_Weather1.Value);
                            weathers.Add(_Fall_Meadows_Weather2.Value);
                            weathers.Add(_Fall_Meadows_Weather3.Value);
                            weathers.Add(_Fall_Meadows_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_Meadows_Weather1.Value);
                            weathers.Add(_Spring_Meadows_Weather2.Value);
                            weathers.Add(_Spring_Meadows_Weather3.Value);
                            weathers.Add(_Spring_Meadows_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_Meadows_Weather1.Value);
                            weathers.Add(_Summer_Meadows_Weather2.Value);
                            weathers.Add(_Summer_Meadows_Weather3.Value);
                            weathers.Add(_Summer_Meadows_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.BlackForest:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_BlackForest_Weather1.Value);
                            weathers.Add(_Winter_BlackForest_Weather2.Value);
                            weathers.Add(_Winter_BlackForest_Weather3.Value);
                            weathers.Add(_Winter_BlackForest_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_BlackForest_Weather1.Value);
                            weathers.Add(_Fall_BlackForest_Weather2.Value);
                            weathers.Add(_Fall_BlackForest_Weather3.Value);
                            weathers.Add(_Fall_BlackForest_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_BlackForest_Weather1.Value);
                            weathers.Add(_Spring_BlackForest_Weather2.Value);
                            weathers.Add(_Spring_BlackForest_Weather3.Value);
                            weathers.Add(_Spring_BlackForest_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_BlackForest_Weather1.Value);
                            weathers.Add(_Summer_BlackForest_Weather2.Value);
                            weathers.Add(_Summer_BlackForest_Weather3.Value);
                            weathers.Add(_Summer_BlackForest_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Swamp:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_Swamp_Weather1.Value);
                            weathers.Add(_Winter_Swamp_Weather2.Value);
                            weathers.Add(_Winter_Swamp_Weather3.Value);
                            weathers.Add(_Winter_Swamp_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_Swamp_Weather1.Value);
                            weathers.Add(_Fall_Swamp_Weather2.Value);
                            weathers.Add(_Fall_Swamp_Weather3.Value);
                            weathers.Add(_Fall_Swamp_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_Swamp_Weather1.Value);
                            weathers.Add(_Spring_Swamp_Weather2.Value);
                            weathers.Add(_Spring_Swamp_Weather3.Value);
                            weathers.Add(_Spring_Swamp_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_Swamp_Weather1.Value);
                            weathers.Add(_Summer_Swamp_Weather2.Value);
                            weathers.Add(_Summer_Swamp_Weather3.Value);
                            weathers.Add(_Summer_Swamp_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Mountain:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_Mountains_Weather1.Value);
                            weathers.Add(_Winter_Mountains_Weather2.Value);
                            weathers.Add(_Winter_Mountains_Weather3.Value);
                            weathers.Add(_Winter_Mountains_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_Mountains_Weather1.Value);
                            weathers.Add(_Fall_Mountains_Weather2.Value);
                            weathers.Add(_Fall_Mountains_Weather3.Value);
                            weathers.Add(_Fall_Mountains_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_Mountains_Weather1.Value);
                            weathers.Add(_Spring_Mountains_Weather2.Value);
                            weathers.Add(_Spring_Mountains_Weather3.Value);
                            weathers.Add(_Spring_Mountains_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_Mountains_Weather1.Value);
                            weathers.Add(_Summer_Mountains_Weather2.Value);
                            weathers.Add(_Summer_Mountains_Weather3.Value);
                            weathers.Add(_Summer_Mountains_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Plains:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_Plains_Weather1.Value);
                            weathers.Add(_Winter_Plains_Weather2.Value);
                            weathers.Add(_Winter_Plains_Weather3.Value);
                            weathers.Add(_Winter_Plains_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_Plains_Weather1.Value);
                            weathers.Add(_Fall_Plains_Weather2.Value);
                            weathers.Add(_Fall_Plains_Weather3.Value);
                            weathers.Add(_Fall_Plains_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_Plains_Weather1.Value);
                            weathers.Add(_Spring_Plains_Weather2.Value);
                            weathers.Add(_Spring_Plains_Weather3.Value);
                            weathers.Add(_Spring_Plains_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_Plains_Weather1.Value);
                            weathers.Add(_Summer_Plains_Weather2.Value);
                            weathers.Add(_Summer_Plains_Weather3.Value);
                            weathers.Add(_Summer_Plains_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Mistlands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_MistLands_Weather1.Value);
                            weathers.Add(_Winter_MistLands_Weather2.Value);
                            weathers.Add(_Winter_MistLands_Weather3.Value);
                            weathers.Add(_Winter_MistLands_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_MistLands_Weather1.Value);
                            weathers.Add(_Fall_MistLands_Weather2.Value);
                            weathers.Add(_Fall_MistLands_Weather3.Value);
                            weathers.Add(_Fall_MistLands_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_MistLands_Weather1.Value);
                            weathers.Add(_Spring_MistLands_Weather2.Value);
                            weathers.Add(_Spring_MistLands_Weather3.Value);
                            weathers.Add(_Spring_MistLands_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_MistLands_Weather1.Value);
                            weathers.Add(_Summer_MistLands_Weather2.Value);
                            weathers.Add(_Summer_MistLands_Weather3.Value);
                            weathers.Add(_Summer_MistLands_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Ocean:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_Ocean_Weather1.Value);
                            weathers.Add(_Winter_Ocean_Weather2.Value);
                            weathers.Add(_Winter_Ocean_Weather3.Value);
                            weathers.Add(_Winter_Ocean_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_Ocean_Weather1.Value);
                            weathers.Add(_Fall_Ocean_Weather2.Value);
                            weathers.Add(_Fall_Ocean_Weather3.Value);
                            weathers.Add(_Fall_Ocean_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_Ocean_Weather1.Value);
                            weathers.Add(_Spring_Ocean_Weather2.Value);
                            weathers.Add(_Spring_Ocean_Weather3.Value);
                            weathers.Add(_Spring_Ocean_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_Ocean_Weather1.Value);
                            weathers.Add(_Summer_Ocean_Weather2.Value);
                            weathers.Add(_Summer_Ocean_Weather3.Value);
                            weathers.Add(_Summer_Ocean_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.AshLands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_AshLands_Weather1.Value);
                            weathers.Add(_Winter_AshLands_Weather2.Value);
                            weathers.Add(_Winter_AshLands_Weather3.Value);
                            weathers.Add(_Winter_AshLands_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_AshLands_Weather1.Value);
                            weathers.Add(_Fall_AshLands_Weather2.Value);
                            weathers.Add(_Fall_AshLands_Weather3.Value);
                            weathers.Add(_Fall_AshLands_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_AshLands_Weather1.Value);
                            weathers.Add(_Spring_AshLands_Weather2.Value);
                            weathers.Add(_Spring_AshLands_Weather3.Value);
                            weathers.Add(_Spring_AshLands_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_AshLands_Weather1.Value);
                            weathers.Add(_Summer_AshLands_Weather2.Value);
                            weathers.Add(_Summer_AshLands_Weather3.Value);
                            weathers.Add(_Summer_AshLands_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.DeepNorth:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            weathers.Add(_Winter_DeepNorth_Weather1.Value);
                            weathers.Add(_Winter_DeepNorth_Weather2.Value);
                            weathers.Add(_Winter_DeepNorth_Weather3.Value);
                            weathers.Add(_Winter_DeepNorth_Weather4.Value);
                            break;
                        case Season.Fall:
                            weathers.Add(_Fall_DeepNorth_Weather1.Value);
                            weathers.Add(_Fall_DeepNorth_Weather2.Value);
                            weathers.Add(_Fall_DeepNorth_Weather3.Value);
                            weathers.Add(_Fall_DeepNorth_Weather4.Value);
                            break;
                        case Season.Spring:
                            weathers.Add(_Spring_DeepNorth_Weather1.Value);
                            weathers.Add(_Spring_DeepNorth_Weather2.Value);
                            weathers.Add(_Spring_DeepNorth_Weather3.Value);
                            weathers.Add(_Spring_DeepNorth_Weather4.Value);
                            break;
                        case Season.Summer:
                            weathers.Add(_Summer_DeepNorth_Weather1.Value);
                            weathers.Add(_Summer_DeepNorth_Weather2.Value);
                            weathers.Add(_Summer_DeepNorth_Weather3.Value);
                            weathers.Add(_Summer_DeepNorth_Weather4.Value);
                            break;
                    }
                    break;
            }

            List<EnvEntry> serverEntries = new();

            if (_YamlConfigurations.Value is Toggle.Off)
            {
                AddToEntries(weathers, serverEntries);
            }
            else
            {
                switch (land)
                {
                    case Heightmap.Biome.Meadows:
                        switch(_Season.Value)
                        {
                            case Season.Winter:
                                foreach (string option in YamlConfigurations.winterData.meadowWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.meadowWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.meadowWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.meadowWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.blackForestWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.blackForestWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.summerData.blackForestWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.swampWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.swampWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.swampWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.mountainWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.mountainWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.mountainWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.plainWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.plainWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.plainWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.mistLandWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.mistLandWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.mistLandWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.ashLandWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.ashLandWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }
                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.ashLandWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.deepNorthWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.deepNorthWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.deepNorthWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
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
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Fall:
                                foreach (string option in YamlConfigurations.fallData.oceanWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Summer:
                                foreach (string option in YamlConfigurations.summerData.oceanWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                            case Season.Spring:
                                foreach (string option in YamlConfigurations.springData.oceanWeather)
                                {
                                    serverEntries.Add(new EnvEntry(){m_environment = option, m_weight = 1f});
                                }

                                break;
                        }

                        break;
                }
            }
            ServerWeatherMap[land] = serverEntries;
        }
        
        double totalSeconds = (lastEnvironmentChange + _WeatherDuration.Value * 60) - __instance.m_totalSeconds;
        if (_WeatherDuration.Value == 0) totalSeconds = (lastEnvironmentChange + 60) - __instance.m_totalSeconds;

        if (totalSeconds > 3) return;

        lastEnvironmentChange = __instance.m_totalSeconds;

        if (ServerWeatherMap[Heightmap.Biome.Meadows].Count != 0)
        {
            MeadowIndex = (MeadowIndex + 1) % ServerWeatherMap[Heightmap.Biome.Meadows].Count;
        }

        if (ServerWeatherMap[Heightmap.Biome.BlackForest].Count != 0)
        {
            BlackForestIndex = (BlackForestIndex + 1) % ServerWeatherMap[Heightmap.Biome.BlackForest].Count;
        }

        if (ServerWeatherMap[Heightmap.Biome.Swamp].Count != 0)
        {
            SwampIndex = (SwampIndex + 1) % ServerWeatherMap[Heightmap.Biome.Swamp].Count;
        }

        if (ServerWeatherMap[Heightmap.Biome.Mountain].Count != 0)
        {
            MountainIndex = (MountainIndex + 1) % ServerWeatherMap[Heightmap.Biome.Mountain].Count;
        }

        if (ServerWeatherMap[Heightmap.Biome.Plains].Count != 0)
        {
            PlainsIndex = (PlainsIndex + 1) % ServerWeatherMap[Heightmap.Biome.Plains].Count;
        }

        if (ServerWeatherMap[Heightmap.Biome.Mistlands].Count != 0)
        {
            MistLandsIndex = (MistLandsIndex + 1) % ServerWeatherMap[Heightmap.Biome.Mistlands].Count;
        }

        if (ServerWeatherMap[Heightmap.Biome.AshLands].Count != 0)
        {
            AshLandsIndex = (AshLandsIndex + 1) % ServerWeatherMap[Heightmap.Biome.AshLands].Count;
        }

        if (ServerWeatherMap[Heightmap.Biome.DeepNorth].Count != 0)
        {
            DeepNorthIndex = (DeepNorthIndex + 1) % ServerWeatherMap[Heightmap.Biome.DeepNorth].Count;
        }

        if (ServerWeatherMap[Heightmap.Biome.Ocean].Count != 0)
        {
            OceanIndex = (OceanIndex + 1) % ServerWeatherMap[Heightmap.Biome.Ocean].Count;
        }

        ServerWeatherIndexes[Heightmap.Biome.Meadows] = MeadowIndex;
        ServerWeatherIndexes[Heightmap.Biome.BlackForest] = BlackForestIndex;
        ServerWeatherIndexes[Heightmap.Biome.Swamp] = SwampIndex;
        ServerWeatherIndexes[Heightmap.Biome.Mountain] = MountainIndex;
        ServerWeatherIndexes[Heightmap.Biome.Plains] = PlainsIndex;
        ServerWeatherIndexes[Heightmap.Biome.Mistlands] = MistLandsIndex;
        ServerWeatherIndexes[Heightmap.Biome.AshLands] = AshLandsIndex;
        ServerWeatherIndexes[Heightmap.Biome.DeepNorth] = DeepNorthIndex;
        ServerWeatherIndexes[Heightmap.Biome.Ocean] = OceanIndex;
        
        UpdateServerWeatherMan();
    }
    private static void ServerSyncedChangeWeather(
        Heightmap.Biome currentBiome, EnvMan __instance, List<EnvEntry> entries, long sec, bool resetTimer = true)
    {
        try
        {
            int serverIndex = GetServerWeatherManIndex(currentBiome);
            __instance.QueueEnvironment(entries[serverIndex].m_environment);
            SetWeatherMan(entries[serverIndex].m_environment);
            currentEnv = entries[serverIndex].m_environment;
            if (resetTimer) lastEnvironmentChange = sec;
            WeatherTweaked = true;
            SeasonalityLogger.LogDebug("Server: Updating synced weather");
        }
        catch (Exception)
        {
            LocalWeatherMan(__instance, sec, entries, currentBiome);
        }
    }
    
    private static int environmentIndex;
    private static double lastEnvironmentChange = EnvMan.instance.m_totalSeconds;
    public static string GetEnvironmentCountDown()
    {
        if (EnvMan.instance == null || !WeatherTweaked || _WeatherTimerEnabled.Value == Toggle.Off) return "";

        double totalSeconds = (lastEnvironmentChange + _WeatherDuration.Value * 60) - EnvMan.instance.m_totalSeconds;
        if (_WeatherDuration.Value == 0) totalSeconds = (lastEnvironmentChange + 60) - EnvMan.instance.m_totalSeconds;

        int hours = TimeSpan.FromSeconds(totalSeconds).Hours;
        int minutes = TimeSpan.FromSeconds(totalSeconds).Minutes;
        int seconds = TimeSpan.FromSeconds(totalSeconds).Seconds;

        if (totalSeconds < 0) return "";
        
        return hours > 0 ? $"{hours}:{minutes:D2}:{seconds:D2}" : minutes > 0 ? $"{minutes}:{seconds:D2}" : $"{seconds}";
    }
    private static bool LocalWeatherMan(EnvMan __instance, long sec, List<EnvEntry> environments, Heightmap.Biome biome)
    {
        if (lastBiome != biome)
        {
            ChangeWeather(__instance, environments, sec, false);
            lastBiome = biome;
            return false;
        }

        if (lastSeason != _Season.Value)
        {
            ChangeWeather(__instance, environments, sec, false);
            lastSeason = _Season.Value;
            return false;
        }
        
        long duration = _WeatherDuration.Value * 60; // Total seconds

        if (duration == 0)
        {
            if ((lastEnvironmentChange + 60) - EnvMan.instance.m_totalSeconds > 0) return false;
            ChangeWeather(__instance, environments, sec);
            return false;
        }

        if ((lastEnvironmentChange + duration) - EnvMan.instance.m_totalSeconds > 0)
        {
            if (!WeatherTweaked) ChangeWeather(__instance, environments, sec, false);
            return false;
        }
        ChangeWeather(__instance, environments, sec);
        return false;
    }
    private static void ChangeWeather(EnvMan __instance, List<EnvEntry> environments, long sec, bool resetTimer = true)
    {
        if (environments.Count <= 0) return;
        environmentIndex = (environmentIndex + 1) % (environments.Count);
        __instance.QueueEnvironment(environments[environmentIndex].m_environment);
        SetWeatherMan(environments[environmentIndex].m_environment);
        currentEnv = environments[environmentIndex].m_environment;
        if (resetTimer) lastEnvironmentChange = sec;
        WeatherTweaked = true;
        SeasonalityLogger.LogDebug("Client: Changing weather using local settings");
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
    
    private static void AddToEntries(List<Environments> environments, List<EnvEntry> entries)
    {
        foreach (Environments value in environments)
        {
            if (value is Environments.None) continue;
            EnvEntry entry = new EnvEntry()
            {
                m_environment = GetEnvironmentName(value),
                m_weight = 1f
            };
            entries.Add(entry);
        }
    }
    
    private static int GetServerWeatherManIndex(Heightmap.Biome land)
    {
        if (SyncedWeatherData.Value == "") return 0;
        
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<Heightmap.Biome, int> data = deserializer.Deserialize<Dictionary<Heightmap.Biome, int>>(SyncedWeatherData.Value);

        return data.TryGetValue(land, out int index) ? index : 0;
    }
    
    private static void UpdateServerWeatherMan()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(ServerWeatherIndexes);
        SyncedWeatherData.Value = data;
    }
}