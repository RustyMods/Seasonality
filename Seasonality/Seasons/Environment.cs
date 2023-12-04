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
            Environments.WarmSnow => "WarmSnow",
            Environments.ClearWarmSnow => "ClearWarmSnow",
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
        WarmSnow,
        ClearWarmSnow,
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
    static class EnvManAwakePatch
    {
        private static void Postfix(EnvMan __instance)
        {
            if (!__instance) return;

            EnvSetup WarmSnow = CloneEnvSetup(__instance, "Snow", "WarmSnow");
            WarmSnow.m_isFreezing = false;
            WarmSnow.m_isFreezingAtNight = false;
            WarmSnow.m_isCold = true;
            WarmSnow.m_isColdAtNight = true;
            WarmSnow.m_lightIntensityDay = 0.6f;

            EnvSetup ClearWarmSnow = CloneEnvSetup(__instance, "Snow", "ClearWarmSnow");
            ClearWarmSnow.m_isFreezing = false;
            ClearWarmSnow.m_isFreezingAtNight = false;
            ClearWarmSnow.m_isCold = true;
            ClearWarmSnow.m_isColdAtNight = true;
            ClearWarmSnow.m_fogDensityMorning = 0.00f;
            ClearWarmSnow.m_fogDensityDay = 0.00f;
            ClearWarmSnow.m_fogDensityEvening = 0.00f;
            ClearWarmSnow.m_fogDensityNight = 0.00f;
            ClearWarmSnow.m_lightIntensityDay = 0.6f;
            
            __instance.m_environments.Add(ClearWarmSnow);
            __instance.m_environments.Add(WarmSnow);
        }

        private static EnvSetup CloneEnvSetup(EnvMan __instance, string originalName, string newName)
        {
            EnvSetup originalSetup = __instance.m_environments.Find(x => x.m_name == originalName);
            EnvSetup newSetup = new EnvSetup()
            {
                m_name = newName,
                m_default = originalSetup.m_default, // Means enabled/disabled
                m_isWet = originalSetup.m_isWet,
                m_isFreezing = originalSetup.m_isFreezing,
                m_isFreezingAtNight = originalSetup.m_isFreezingAtNight,
                m_isCold = originalSetup.m_isCold,
                m_isColdAtNight = originalSetup.m_isColdAtNight,
                m_alwaysDark = originalSetup.m_alwaysDark,
                m_ambColorNight = originalSetup.m_ambColorNight,
                m_ambColorDay = originalSetup.m_ambColorDay,
                m_fogColorNight = originalSetup.m_fogColorNight,
                m_fogColorMorning = originalSetup.m_fogColorMorning,
                m_fogColorEvening = originalSetup.m_fogColorEvening,
                m_fogColorSunNight = originalSetup.m_fogColorSunNight,
                m_fogColorSunMorning = originalSetup.m_fogColorSunMorning,
                m_fogDensityNight = originalSetup.m_fogDensityNight,
                m_fogDensityMorning = originalSetup.m_fogDensityMorning,
                m_fogDensityEvening = originalSetup.m_fogDensityEvening,
                m_sunColorNight = originalSetup.m_sunColorNight,
                m_sunColorMorning = originalSetup.m_fogColorSunMorning,
                m_sunColorDay = originalSetup.m_sunColorDay,
                m_sunColorEvening = originalSetup.m_sunColorEvening,
                m_lightIntensityDay = originalSetup.m_lightIntensityDay,
                m_sunAngle = originalSetup.m_sunAngle,
                m_windMin = originalSetup.m_windMin,
                m_windMax = originalSetup.m_windMax,
                m_envObject = originalSetup.m_envObject,
                m_psystems = originalSetup.m_psystems,
                m_psystemsOutsideOnly = originalSetup.m_psystemsOutsideOnly,
                m_rainCloudAlpha = originalSetup.m_rainCloudAlpha,
                m_ambientLoop = originalSetup.m_ambientLoop,
                m_ambientVol = originalSetup.m_ambientVol,
                m_ambientList = originalSetup.m_ambientList,
                m_musicMorning = originalSetup.m_musicMorning,
                m_musicEvening = originalSetup.m_musicEvening,
                m_musicDay = originalSetup.m_musicDay,
                m_musicNight = originalSetup.m_musicNight
            };

            return newSetup;
        }
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
            long duration = __instance.m_environmentDuration * _WeatherDuration.Value;
            
            List<Action> actions = new();
            if (duration == 0)
            {
                foreach (EnvEntry? env in environments) actions.Add(() => __instance.QueueEnvironment(env.m_environment));
                Utils.ApplyRandomly(actions);
                
                return false;
            }
            long seed = sec / duration;
            if (__instance.m_environmentPeriod == seed) return false;
                
            __instance.m_environmentPeriod = seed;
            __instance.m_currentBiome = biome;

            foreach (EnvEntry? env in environments) actions.Add(() => __instance.QueueEnvironment(env.m_environment));
            Utils.ApplyRandomly(actions);

            return false;
        }
    }
}