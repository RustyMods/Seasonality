using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;
using ServerSync;
using YamlDotNet.Serialization;

namespace Seasonality.GameplayModifiers;

public static class PlantTweaks
{
    private const string FileName = "Plants.yml";
    private static readonly string FilePath = TweaksManager.FolderPath + Path.DirectorySeparatorChar + FileName;
    private static readonly CustomSyncedValue<string> ServerConfigs = new(SeasonalityPlugin.ConfigSync, "ServerPlantConfigs", "");
    private static ConfigEntry<Configs.Toggle> m_enabled = null!;
    public static Dictionary<string, Plants> m_data = new();

    public static void Setup()
    {
        m_enabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Plants Tweaks", Configs.Toggle.Off, "If on, plant tweaks are enabled");
        ServerConfigs.ValueChanged += () =>
        {
            if (!ZNet.m_instance || ZNet.m_instance.IsServer()) return;
            var deserializer = new DeserializerBuilder().Build();
            try
            {
                m_data = deserializer.Deserialize<Dictionary<string, Plants>>(ServerConfigs.Value);
            }
            catch
            {
                SeasonalityPlugin.Record.LogWarning("Failed to deserialize server plant configs");
            }
        };
    }

    public static void UpdateServerConfigs()
    {
        if (!ZNet.m_instance || !ZNet.m_instance.IsServer()) return;
        var serializer = new SerializerBuilder().Build();
        ServerConfigs.Value = serializer.Serialize(m_data);
    }

    public static void SetupFileWatch()
    {
        void ReadConfigValues(object sender, FileSystemEventArgs args)
        {
            UpdateServerConfigs();
        }
        FileSystemWatcher watcher = new FileSystemWatcher(TweaksManager.FolderPath, FileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = false;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    public static void Read()
    {
        if (!File.Exists(FilePath))
        {
            var serializer = new SerializerBuilder().Build();
            var data = serializer.Serialize(m_data);
            File.WriteAllText(FilePath, data);
        }
        else
        {
            var deserializer = new DeserializerBuilder().Build();
            try
            {
                m_data = deserializer.Deserialize<Dictionary<string, Plants>>(File.ReadAllText(FilePath));
            }
            catch
            {
                SeasonalityPlugin.Record.LogWarning("Failed to deserialize plant tweaks: " + Path.GetFileName(FilePath));
            }
        }
    }
    
    public static Plants.Data Create(float min, float max, float growTimeMax, float growTime, bool canHarvest = true)
    {
        return new Plants.Data()
        {
            MaxScale = max, MinScale = min, GrowTime = growTime, GrowTimeMax = growTimeMax, CanHarvest = canHarvest
        };
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Grow))]
    private static class Plant_Grow_Patch
    {
        private static bool Prefix(Plant __instance)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return true;
            if (!m_data.TryGetValue(__instance.name.Replace("(Clone)", string.Empty), out Plants data)) return true;
            return Configs.m_season.Value switch
            {
                Configs.Season.Spring => data.Spring.CanHarvest,
                Configs.Season.Summer => data.Summer.CanHarvest,
                Configs.Season.Fall => data.Fall.CanHarvest,
                Configs.Season.Winter => data.Winter.CanHarvest,
                _ => true
            };
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.GetStatus))]
    private static class Plant_GetStatus_Patch
    {
        private static void Postfix(Plant __instance, ref Plant.Status __result)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return;
            if (!m_data.TryGetValue(__instance.name.Replace("(Clone)", string.Empty), out Plants data)) return;
            switch (Configs.m_season.Value)
            {
                case Configs.Season.Spring:
                    if (data.Spring.CanHarvest) return;
                    __result = Plant.Status.NoSpace;
                    break;
                case Configs.Season.Summer:
                    if (data.Summer.CanHarvest) return;
                    __result = Plant.Status.NoSpace;
                    break;
                case Configs.Season.Fall:
                    if (data.Fall.CanHarvest) return;
                    __result = Plant.Status.NoSpace;
                    break;
                case Configs.Season.Winter:
                    if (data.Winter.CanHarvest) return;
                    __result = Plant.Status.NoSpace;
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.GetHoverText))]
    private static class Plant_GetHoverText_Patch
    {
        private static void Postfix(Plant __instance, ref string __result)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return;
            if (!m_data.TryGetValue(__instance.name.Replace("(Clone)", string.Empty), out Plants data)) return;
            switch (Configs.m_season.Value)
            {
                case Configs.Season.Spring:
                    if (data.Spring.CanHarvest) return;
                    break;
                case Configs.Season.Summer:
                    if (data.Summer.CanHarvest) return;
                    break;
                case Configs.Season.Fall:
                    if (data.Fall.CanHarvest) return;
                    break;
                case Configs.Season.Winter:
                    if (data.Winter.CanHarvest) return;
                    break;
            }
            __result += Localization.instance.Localize($"\n <color=red>${Configs.m_season.Value.ToString().ToLower()}_cannot_grow</color>");
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
    private static class Plant_Awake_Patch
    {
        private static void Postfix(Plant __instance)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return;
            if (!m_data.TryGetValue(__instance.name.Replace("(Clone)", string.Empty), out Plants data)) return;
            Plants.Data plantData;
            switch (Configs.m_season.Value)
            {
                case Configs.Season.Spring:
                    plantData = data.Spring;
                    break;
                case Configs.Season.Summer:
                    plantData = data.Summer;
                    break;
                case Configs.Season.Fall:
                    plantData = data.Fall;
                    break;
                case Configs.Season.Winter:
                    plantData = data.Winter;
                    break;
                default:
                    return;
            }

            __instance.m_maxScale = plantData.MaxScale;
            __instance.m_minScale = plantData.MinScale;
            __instance.m_growTimeMax = plantData.GrowTimeMax;
            __instance.m_growTime = plantData.GrowTime;
        }
    }

    [Serializable]
    public class Plants
    {
        public Data Summer = new();
        public Data Fall = new();
        public Data Winter = new();
        public Data Spring = new();

        [Serializable]
        public class Data
        {
            public float MaxScale;
            public float MinScale;
            public float GrowTimeMax;
            public float GrowTime;
            public bool CanHarvest;
        }
    }
}