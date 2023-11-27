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

            if (!Player.m_localPlayer) return true;
            if (Player.m_localPlayer.IsDead()) return true;
            Heightmap.Biome currentBiome = Heightmap.FindBiome(Player.m_localPlayer.transform.position);

            List<EnvEntry> entries = new();
            List<Environments> configs = new();
            
            switch (currentBiome)
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
            if (configs.TrueForAll(x => x is Environments.None)) return true;

            AddToEntries(configs, entries);

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

            long seed = sec / _WeatherDuration.Value;
            if (__instance.m_environmentPeriod == seed) return false;
            __instance.m_environmentPeriod = seed;
            __instance.m_currentBiome = biome;

            List<Action> actions = new();
            foreach (EnvEntry? env in environments) actions.Add(() => __instance.QueueEnvironment(env.m_environment));
            Utils.ApplyRandomly(actions);

            return false;
        }
    }
}