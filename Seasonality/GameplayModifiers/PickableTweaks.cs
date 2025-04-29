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

public static class PickableTweaks
{
    private const string FileName = "Pickables.yml";
    private static readonly string FilePath = TweaksManager.FolderPath + Path.DirectorySeparatorChar + FileName;
    private static readonly CustomSyncedValue<string> ServerPickableConfigs = new(SeasonalityPlugin.ConfigSync, "ServerPickableConfigs", "");
    private static ConfigEntry<Configs.Toggle> m_enabled = null!;
    public static Dictionary<string, Harvest> m_data = new();
    public static void Setup()
    {
        m_enabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Pickable Enabled", Configs.Toggle.Off, "If on, pickable tweaks are enabled");
        ServerPickableConfigs.ValueChanged += () =>
        {
            var deserializer = new DeserializerBuilder().Build();
            try
            {
                m_data = deserializer.Deserialize<Dictionary<string, Harvest>>(ServerPickableConfigs.Value);
            }
            catch
            {
                SeasonalityPlugin.Record.LogWarning("Failed to deserialize server pickable configs");
            }
        };
        Read();
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

    public static void UpdateServerConfigs()
    {
        if (!ZNet.m_instance || !ZNet.m_instance.IsServer()) return;
        var serializer = new SerializerBuilder().Build();
        ServerPickableConfigs.Value = serializer.Serialize(m_data);
    }
    

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
    private static class Pickable_Awake_Patch
    {
        private static void Postfix(Pickable __instance)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return;
            if (!GetHarvestData(__instance.name.Replace("(Clone)", string.Empty), out Harvest.HarvestData data)) return;
            __instance.m_amount = data.Amount;
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    private static class Pickable_Interact_Patch
    {
        private static bool Prefix(Pickable __instance)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return true;
            if (!GetHarvestData(__instance.name.Replace("(Clone)", string.Empty), out Harvest.HarvestData data)) return true;
            return data.CanHarvest;
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.GetHoverText))]
    private static class Pickable_GetHoverText_Patch
    {
        private static void Postfix(Pickable __instance, ref string __result)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return;
            if (!GetHarvestData(__instance.name.Replace("(Clone)", string.Empty), out Harvest.HarvestData data)) return;
            if (data.CanHarvest) return;
            __result += Localization.instance.Localize("\n <color=red>$cannot_pick</color>");
        }
    }

    private static bool GetHarvestData(string prefabName, out Harvest.HarvestData result)
    {
        result = new Harvest.HarvestData();
        if (!m_data.TryGetValue(prefabName, out Harvest data)) return false;
        switch (Configs.m_season.Value)
        {
            case Configs.Season.Fall:
                result = data.Fall;
                break;
            case Configs.Season.Winter:
                result = data.Winter;
                break;
            case Configs.Season.Spring:
                result = data.Spring;
                break;
            case Configs.Season.Summer:
                result = data.Summer;
                break;
            default:
                return false;
        }

        return true;
    }

    public static void Read()
    {
        if (!Directory.Exists(TweaksManager.FolderPath)) Directory.CreateDirectory(TweaksManager.FolderPath);
        if (!File.Exists(FilePath))
        {
            var serializer = new SerializerBuilder().Build();
            var data = serializer.Serialize(m_data);
            File.WriteAllText(FilePath, data);
            return;
        }
        var deserializer = new DeserializerBuilder().Build();
        try
        {
            m_data = deserializer.Deserialize<Dictionary<string, Harvest>>(File.ReadAllText(FilePath));
            
        }
        catch
        {
            SeasonalityPlugin.Record.LogWarning("Failed to deserialize: " + Path.GetFileName(FilePath));
        }
    }

    public static Harvest.HarvestData CreateData(int amount, bool canHarvest = true)
    {
        return new Harvest.HarvestData() { Amount = amount, CanHarvest = canHarvest };
    }
    

    [Serializable]
    public class Harvest
    {
        public HarvestData Summer = new();
        public HarvestData Fall = new();
        public HarvestData Winter = new();
        public HarvestData Spring = new();

        [Serializable]
        public class HarvestData
        {
            public int Amount;
            public bool CanHarvest;
        }
    }
}