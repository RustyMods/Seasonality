using System;
using System.Collections.Generic;
using HarmonyLib;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class Environment
{
    private static string GetEnvironmentName(Environments options)
    {
        return options switch
        {
            Environments.None => "",
            Environments.Clear => "Clear",
            Environments.Misty => "Misty",
            Environments.Darklands_dark => "Darklands_dark",
            Environments.HeathClear => "Heath clear",
            Environments.DeepForestMist => "DeepForest Mist",
            Environments.GDKing => "GDKing",
            Environments.Rain => "Rain",
            Environments.LightRain => "LightRain",
            Environments.ThunderStorm => "ThunderStorm",
            Environments.Eikthyr => "Eikthyr",
            Environments.GoblinKing => "GoblinKing",
            Environments.nofogts => "nofogts",
            Environments.SwampRain => "SwampRain",
            Environments.Bonemass => "Bonemass",
            Environments.Snow => "Snow",
            Environments.Twilight_Clear => "Twilight_Clear",
            Environments.Twilight_Snow => "Twilight_Snow",
            Environments.Twilight_SnowStorm => "Twilight_SnowStorm",
            Environments.SnowStorm => "SnowStorm",
            Environments.Moder => "Moder",
            Environments.Ashrain => "AshRain",
            Environments.Crypt => "Crypt",
            Environments.SunkenCrypt => "SunkenCrypt",
            Environments.Caves => "Caves",
            Environments.Mistlands_clear => "Mistlands_clear",
            Environments.Mistlands_rain => "Mistlands_rain",
            Environments.Mistlands_thunder => "Mistlands_thunder",
            Environments.InfectedMine => "InfectedMine",
            Environments.Queen => "Queen",
            _ => ""
        };
    }
    public enum Environments
    {
        None,
        Clear,
        Twilight_Clear,
        Misty,
        Darklands_dark,
        HeathClear,
        DeepForestMist,
        GDKing,
        Rain,
        LightRain,
        ThunderStorm,
        Eikthyr,
        GoblinKing,
        nofogts,
        SwampRain,
        Bonemass,
        Snow,
        Twilight_Snow,
        Twilight_SnowStorm,
        SnowStorm,
        Moder,
        Ashrain,
        Crypt,
        SunkenCrypt,
        Caves,
        Mistlands_clear,
        Mistlands_rain,
        Mistlands_thunder,
        InfectedMine,
        Queen,
    }
    [Serializable]
    public class WeatherEntry
    {
        public Environments m_environment = Environments.None;
        public float m_weight = 1f;
    }
    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateEnvironment))]
    static class EnvManPatch
    {
        private static bool Prefix(EnvMan __instance, long sec, Heightmap.Biome biome)
        {
            if (_ModEnabled.Value is Toggle.Off || _WeatherControl.Value is Toggle.Off) return true;
            string environmentOverride = __instance.GetEnvironmentOverride();
            if (!string.IsNullOrEmpty(environmentOverride))
            {
                // If debug mode is active and user forces environment
                __instance.m_environmentPeriod = -1L;
                __instance.m_currentBiome = __instance.GetBiome();
                __instance.QueueEnvironment(environmentOverride);
                return false;
            }
            List<EnvEntry> entries = new();
            switch (_Season.Value)
            {
                case Season.Winter:
                    List<Environments> winterConfigs = new()
                    {
                        _WinterWeather1.Value,
                        _WinterWeather2.Value,
                        _WinterWeather3.Value,
                        _WinterWeather4.Value
                    };
                    if (winterConfigs.TrueForAll(x => x is Environments.None)) return true;
                    AddToEntries(winterConfigs, entries);
                    break;
                case Season.Fall:
                    List<Environments> fallConfigs = new()
                    {
                        _FallWeather1.Value,
                        _FallWeather2.Value,
                        _FallWeather3.Value,
                        _FallWeather4.Value
                    };
                    if (fallConfigs.TrueForAll(x => x is Environments.None)) return true;
                    AddToEntries(fallConfigs, entries);
                    break;
                case Season.Spring:
                    List<Environments> springConfigs = new()
                    {
                        _SpringWeather1.Value,
                        _SpringWeather2.Value,
                        _SpringWeather3.Value,
                        _SpringWeather4.Value
                    };
                    if (springConfigs.TrueForAll(x => x is Environments.None)) return true;
                    AddToEntries(springConfigs, entries);
                    break;
                case Season.Summer:
                    List<Environments> summerConfigs = new()
                    {
                        _SummerWeather1.Value,
                        _SummerWeather2.Value,
                        _SummerWeather3.Value,
                        _SummerWeather4.Value
                    };
                    if (summerConfigs.TrueForAll(x => x is Environments.None)) return true;
                    AddToEntries(summerConfigs, entries);
                    break;
                default: return true;
            }
            return ModifyEnvironment(__instance, sec, biome, entries);
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

        private static bool ModifyEnvironment(EnvMan __instance, long sec, Heightmap.Biome biome, List<EnvEntry> environments)
        {
            if (_WeatherDuration.Value == 0)
            {
                __instance.m_environmentPeriod = -1L;
                __instance.m_currentBiome = biome;
                List<Action> actions = new();
                foreach (EnvEntry? env in environments) actions.Add(() => __instance.QueueEnvironment(env.m_environment));
                Utils.ApplyRandomly(actions);
                return false;
            }
            else
            {
                long seed = sec / _WeatherDuration.Value;
                if (__instance.m_environmentPeriod == seed) return false;
                __instance.m_environmentPeriod = seed;
                __instance.m_currentBiome = biome;

                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState((int) seed);
                
                __instance.QueueEnvironment(__instance.SelectWeightedEnvironment(environments));
                UnityEngine.Random.state = state;

                return false;
                
                
                List<Action> actions = new();
                foreach (EnvEntry? env in environments) actions.Add(() => __instance.QueueEnvironment(env.m_environment));
                Utils.ApplyRandomly(actions);
                return false;
            }
        }
    }
}