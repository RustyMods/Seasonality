using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Weather.Utils;

namespace Seasonality.Weather;

public static class Environment
{
    private static readonly CustomSyncedValue<List<string>> SyncedCustomEnvironments = new(SeasonalityPlugin.ConfigSync, "ServerEnvironments", new());
    [Serializable]
    public class WeatherSetup
    {
        public string m_name = "";
        public bool m_default;
        public bool m_isWet;
        public bool m_isFreezing;
        public bool m_isFreezingAtNight;
        public bool m_isCold;
        public bool m_isColdAtNight = true;
        public bool m_alwaysDark;
        public string m_ambColorNight = Color.white.ToString();
        public string m_ambColorDay = Color.white.ToString();
        public string m_fogColorNight = Color.white.ToString();
        public string m_fogColorMorning = Color.white.ToString();
        public string m_fogColorDay = Color.white.ToString();
        public string m_fogColorEvening = Color.white.ToString();
        public string m_fogColorSunNight = Color.white.ToString();
        public string m_fogColorSunMorning = Color.white.ToString();
        public string m_fogColorSunDay = Color.white.ToString();
        public string m_fogColorSunEvening = Color.white.ToString();
        public float m_fogDensityNight = 0.01f;
        public float m_fogDensityMorning = 0.01f;
        public float m_fogDensityDay = 0.01f;
        public float m_fogDensityEvening = 0.01f;
        public string m_sunColorNight = Color.white.ToString();
        public string m_sunColorMorning = Color.white.ToString();
        public string m_sunColorDay = Color.white.ToString();
        public string m_sunColorEvening = Color.white.ToString();
        public float m_lightIntensityDay = 1.2f;
        public float m_lightIntensityNight;
        public float m_sunAngle = 60f;
        public float m_windMin;
        public float m_windMax = 1f;
        public bool m_psystemsOutsideOnly;
        public float m_rainCloudAlpha;
        public string m_ambientLoop = "SW008_Wendland_Autumn_Wind_In_Reeds_Medium_Distance_Leaves_Only";
        public float m_ambientVol = 0.3f;
        public string m_ambientList = "";
        public string m_musicMorning = "";
        public string m_musicEvening = "";
        public string m_musicDay = "";
        public string m_musicNight = "";
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

            EnvSetup NightFrost = CloneEnvSetup(__instance, "Snow", "NightFrost");
            NightFrost.m_isFreezing = false;
            NightFrost.m_isFreezingAtNight = true;
            NightFrost.m_isCold = true;
            NightFrost.m_isColdAtNight = false;
            NightFrost.m_fogDensityMorning = 0.00f;
            NightFrost.m_fogDensityDay = 0.00f;
            NightFrost.m_fogDensityEvening = 0.00f;
            NightFrost.m_fogDensityNight = 0.00f;
            NightFrost.m_lightIntensityDay = 0.6f;

            EnvSetup WinterClear = CloneEnvSetup(__instance, "Clear", "WinterClear");
            WinterClear.m_lightIntensityDay = 0.6f;

            __instance.m_environments.Add(NightFrost);
            __instance.m_environments.Add(ClearWarmSnow);
            __instance.m_environments.Add(WarmSnow);
            __instance.m_environments.Add(WinterClear);
            
            EnvSetup? exampleSetup = __instance.m_environments.Find(x => x.m_name == "Snow");
            
            foreach (EnvSetup setup in ReadWeatherSetup())
            {
                if (!__instance.m_environments.Exists(x => x.m_name == setup.m_name))
                {
                    setup.m_psystems = exampleSetup.m_psystems;
                    setup.m_envObject = exampleSetup.m_envObject;
                    __instance.m_environments.Add(setup);
                }
            }

            WriteWeatherSetup(__instance);
        }
        private static void WriteWeatherSetup(EnvMan __instance)
        {
            ISerializer serializer = new SerializerBuilder().Build();
            string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality" +
                                Path.DirectorySeparatorChar +
                                "Environments";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string registryPath = folderPath + Path.DirectorySeparatorChar + "Examples";

            if (!Directory.Exists(registryPath)) Directory.CreateDirectory(registryPath);

            foreach (EnvSetup env in __instance.m_environments)
            {
                try
                {
                    WeatherSetup setup = new WeatherSetup()
                    {
                        m_name = env.m_name,
                        m_default = env.m_default,
                        m_isWet = env.m_isWet,
                        m_isFreezing = env.m_isFreezing,
                        m_isFreezingAtNight = env.m_isFreezingAtNight,
                        m_isCold = env.m_isCold,
                        m_isColdAtNight = env.m_isColdAtNight,
                        m_alwaysDark = env.m_alwaysDark,
                        m_ambColorNight = env.m_ambColorNight.ToString(),
                        m_ambColorDay = env.m_ambColorDay.ToString(),
                        m_fogColorNight = env.m_fogColorNight.ToString(),
                        m_fogColorMorning = env.m_fogColorMorning.ToString(),
                        m_fogColorDay = env.m_fogColorDay.ToString(),
                        m_fogColorEvening = env.m_fogColorEvening.ToString(),
                        m_fogColorSunEvening = env.m_fogColorSunEvening.ToString(),
                        m_fogDensityNight = env.m_fogDensityNight,
                        m_fogDensityMorning = env.m_fogDensityMorning,
                        m_fogDensityDay = env.m_fogDensityDay,
                        m_fogDensityEvening = env.m_fogDensityEvening,
                        m_sunColorNight = env.m_sunColorNight.ToString(),
                        m_sunColorMorning = env.m_sunColorMorning.ToString(),
                        m_sunColorDay = env.m_sunColorDay.ToString(),
                        m_sunColorEvening = env.m_sunColorEvening.ToString(),
                        m_lightIntensityDay = env.m_lightIntensityDay,
                        m_lightIntensityNight = env.m_lightIntensityNight,
                        m_sunAngle = env.m_sunAngle,
                        m_windMin = env.m_windMin,
                        m_windMax = env.m_windMax,
                        m_psystemsOutsideOnly = env.m_psystemsOutsideOnly,
                        m_rainCloudAlpha = env.m_rainCloudAlpha,
                        m_ambientLoop = env.m_ambientLoop.name,
                        m_ambientVol = env.m_ambientVol,
                        m_ambientList = env.m_ambientList,
                        m_musicMorning = env.m_musicMorning,
                        m_musicEvening = env.m_musicEvening,
                        m_musicDay = env.m_musicDay,
                        m_musicNight = env.m_musicNight
                    };
                    string data = serializer.Serialize(setup);
                    File.WriteAllText(registryPath + Path.DirectorySeparatorChar + $"{setup.m_name}.yml", data);
                }
                catch (NullReferenceException)
                {

                }
            }
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
    public static void RegisterServerEnvironments(EnvMan __instance)
    {
        EnvSetup? exampleSetup = __instance.m_environments.Find(x => x.m_name == "Snow");

        foreach (EnvSetup setup in GetServerEnvironmentSetup())
        {
            if (!__instance.m_environments.Exists(x => x.m_name == setup.m_name))
            {
                setup.m_psystems = exampleSetup.m_psystems;
                setup.m_envObject = exampleSetup.m_envObject;
                __instance.m_environments.Add(setup);
            }
        }
    }
    private static List<EnvSetup> GetServerEnvironmentSetup()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        List<EnvSetup> output = new();
        foreach (string data in SyncedCustomEnvironments.Value)
        {
            WeatherSetup setup = deserializer.Deserialize<WeatherSetup>(data);
            EnvSetup newEnvSetup = new EnvSetup()
            {
                m_name = setup.m_name,
                m_default = setup.m_default,
                m_isWet = setup.m_isWet,
                m_isFreezing = setup.m_isFreezing,
                m_isFreezingAtNight = setup.m_isFreezingAtNight,
                m_isCold = setup.m_isCold,
                m_isColdAtNight = setup.m_isColdAtNight,
                m_alwaysDark = setup.m_alwaysDark,
                m_ambColorNight = GetColorFromString(setup.m_ambColorNight),
                m_ambColorDay = GetColorFromString(setup.m_ambColorDay),
                m_fogColorNight = GetColorFromString(setup.m_fogColorNight),
                m_fogColorMorning = GetColorFromString(setup.m_fogColorMorning),
                m_fogColorDay = GetColorFromString(setup.m_fogColorDay),
                m_fogColorEvening = GetColorFromString(setup.m_fogColorEvening),
                m_fogColorSunNight = GetColorFromString(setup.m_fogColorSunNight),
                m_fogColorSunMorning = GetColorFromString(setup.m_fogColorSunMorning),
                m_fogColorSunDay = GetColorFromString(setup.m_fogColorSunDay),
                m_fogColorSunEvening = GetColorFromString(setup.m_fogColorSunEvening),
                m_fogDensityNight = setup.m_fogDensityNight,
                m_fogDensityMorning = setup.m_fogDensityMorning,
                m_fogDensityDay = setup.m_fogDensityDay,
                m_fogDensityEvening = setup.m_fogDensityEvening,
                m_sunColorNight = GetColorFromString(setup.m_sunColorNight),
                m_sunColorMorning = GetColorFromString(setup.m_sunColorMorning),
                m_sunColorDay = GetColorFromString(setup.m_sunColorDay),
                m_sunColorEvening = GetColorFromString(setup.m_sunColorEvening),
                m_lightIntensityDay = setup.m_lightIntensityDay,
                m_lightIntensityNight = setup.m_lightIntensityNight,
                m_sunAngle = setup.m_sunAngle,
                m_windMin = setup.m_windMin,
                m_windMax = setup.m_windMax,
                m_psystemsOutsideOnly = setup.m_psystemsOutsideOnly,
                m_rainCloudAlpha = setup.m_rainCloudAlpha,
                m_ambientLoop = Resources.Load<AudioClip>(setup.m_ambientLoop),
                m_ambientVol = setup.m_ambientVol,
                m_ambientList = setup.m_ambientList,
                m_musicMorning = setup.m_musicMorning,
                m_musicEvening = setup.m_musicEvening,
                m_musicDay = setup.m_musicDay,
                m_musicNight = setup.m_musicNight
            };
            output.Add(newEnvSetup);
        }

        return output;
    }
    private static List<EnvSetup> ReadWeatherSetup()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality" +
                            Path.DirectorySeparatorChar +
                            "Environments";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string[] files = Directory.GetFiles(folderPath, "*.yml");
        List<EnvSetup> output = new();
        foreach (string file in files)
        {
            string rawData = File.ReadAllText(file);
            if (workingAsType is WorkingAs.Server)
            { 
                SyncedCustomEnvironments.Value.Add(rawData);
            }
            WeatherSetup setup = deserializer.Deserialize<WeatherSetup>(rawData);
            EnvSetup newEnvSetup = new EnvSetup()
            {
                m_name = setup.m_name,
                m_default = setup.m_default,
                m_isWet = setup.m_isWet,
                m_isFreezing = setup.m_isFreezing,
                m_isFreezingAtNight = setup.m_isFreezingAtNight,
                m_isCold = setup.m_isCold,
                m_isColdAtNight = setup.m_isColdAtNight,
                m_alwaysDark = setup.m_alwaysDark,
                m_ambColorNight = GetColorFromString(setup.m_ambColorNight),
                m_ambColorDay = GetColorFromString(setup.m_ambColorDay),
                m_fogColorNight = GetColorFromString(setup.m_fogColorNight),
                m_fogColorMorning = GetColorFromString(setup.m_fogColorMorning),
                m_fogColorDay = GetColorFromString(setup.m_fogColorDay),
                m_fogColorEvening = GetColorFromString(setup.m_fogColorEvening),
                m_fogColorSunNight = GetColorFromString(setup.m_fogColorSunNight),
                m_fogColorSunMorning = GetColorFromString(setup.m_fogColorSunMorning),
                m_fogColorSunDay = GetColorFromString(setup.m_fogColorSunDay),
                m_fogColorSunEvening = GetColorFromString(setup.m_fogColorSunEvening),
                m_fogDensityNight = setup.m_fogDensityNight,
                m_fogDensityMorning = setup.m_fogDensityMorning,
                m_fogDensityDay = setup.m_fogDensityDay,
                m_fogDensityEvening = setup.m_fogDensityEvening,
                m_sunColorNight = GetColorFromString(setup.m_sunColorNight),
                m_sunColorMorning = GetColorFromString(setup.m_sunColorMorning),
                m_sunColorDay = GetColorFromString(setup.m_sunColorDay),
                m_sunColorEvening = GetColorFromString(setup.m_sunColorEvening),
                m_lightIntensityDay = setup.m_lightIntensityDay,
                m_lightIntensityNight = setup.m_lightIntensityNight,
                m_sunAngle = setup.m_sunAngle,
                m_windMin = setup.m_windMin,
                m_windMax = setup.m_windMax,
                m_psystemsOutsideOnly = setup.m_psystemsOutsideOnly,
                m_rainCloudAlpha = setup.m_rainCloudAlpha,
                m_ambientLoop = Resources.Load<AudioClip>(setup.m_ambientLoop),
                m_ambientVol = setup.m_ambientVol,
                m_ambientList = setup.m_ambientList,
                m_musicMorning = setup.m_musicMorning,
                m_musicEvening = setup.m_musicEvening,
                m_musicDay = setup.m_musicDay,
                m_musicNight = setup.m_musicNight
            };
            output.Add(newEnvSetup);
        }

        return output;
    }
}