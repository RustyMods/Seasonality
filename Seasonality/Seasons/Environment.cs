using System;
using System.Collections.Generic;
using HarmonyLib;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class Environment
{
    public static string GetEnvironmentName(Environments options)
    {
        Dictionary<Environments, string> conversionMap = new()
        {
            { Environments.None , "" },
            { Environments.Clear , "Clear" },
            { Environments.Misty , "Misty" },
            { Environments.Darklands_dark , "Darklands_dark" },
            { Environments.HeathClear , "Heath clear" },
            { Environments.DeepForestMist , "DeepForest Mist" },
            { Environments.GDKing , "GDKing" },
            { Environments.Rain, "Rain" },
            { Environments.LightRain, "LightRain" },
            { Environments.ThunderStorm, "ThunderStorm" },
            { Environments.Eikthyr, "Eikthyr" },
            { Environments.GoblinKing, "GoblinKing" },
            { Environments.nofogts, "nofogts" },
            { Environments.SwampRain, "SwampRain" },
            { Environments.Bonemass, "Bonemass" },
            { Environments.Snow, "Snow" },
            { Environments.Twilight_Clear , "Twilight_Clear" },
            { Environments.Twilight_Snow, "Twilight_Snow" },
            { Environments.Twilight_SnowStorm, "Twilight_SnowStorm" },
            { Environments.SnowStorm, "SnowStorm" },
            { Environments.Moder, "Moder" },
            { Environments.Ashrain, "Ashrain" },
            { Environments.Crypt, "Crypt" },
            { Environments.SunkenCrypt, "SunkenCrypt" },
            { Environments.Caves, "Caves" },
            { Environments.Mistlands_clear, "Mistlands_clear" },
            { Environments.Mistlands_rain, "Mistlands_rain" },
            { Environments.Mistlands_thunder, "Mistlands_thunder" },
            { Environments.InfectedMine, "InfectedMine" },
            { Environments.Queen, "Queen" }
        };

        return conversionMap[options];
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
    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateEnvironment))]
    static class EnvManPatch
    {
        private static bool Prefix(EnvMan __instance, long sec, Heightmap.Biome biome)
        {
            string environmentOverride = __instance.GetEnvironmentOverride();
            if (!string.IsNullOrEmpty(environmentOverride))
            {
                __instance.m_environmentPeriod = -1L;
                __instance.m_currentBiome = __instance.GetBiome();
                __instance.QueueEnvironment(environmentOverride);
                return false;
            }

            switch (_Season.Value)
            {
                case Season.Winter:
                    return ModifyEnvironment(__instance, sec, biome, new List<EnvEntry>()
                    {
                        new ()
                        {
                            m_environment = "Snow",
                            m_weight = 1f
                        },
                    });
                default:
                    return true;
            }

        }

        private static bool ModifyEnvironment(EnvMan __instance, long sec, Heightmap.Biome biome, List<EnvEntry> environments)
        {
            if (_WeatherDuration.Value == 0)
            {
                __instance.m_environmentPeriod = -1L;
                __instance.m_currentBiome = biome;
                List<Action> actions = new();
                foreach (EnvEntry? env in environments)
                {
                    actions.Add(new(() => __instance.QueueEnvironment(env.m_environment)));
                }
                Utils.ApplyRandomly(actions);
            
                return false;
            }
            else
            {
                long seed = sec / _WeatherDuration.Value;
                if (__instance.m_environmentPeriod == seed) return false;
                __instance.m_environmentPeriod = seed;
                __instance.m_currentBiome = biome;
                List<Action> actions = new();
                foreach (EnvEntry? env in environments)
                {
                    actions.Add(new(() => __instance.QueueEnvironment(env.m_environment)));
                }
                Utils.ApplyRandomly(actions);
                
                return false;
            }
        }
    }
}