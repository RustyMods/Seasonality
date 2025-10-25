using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Seasonality.Helpers;
using ServerSync;
using YamlDotNet.Serialization;

namespace Seasonality.GameplayModifiers;

public static class PickableTweaks
{
    private const string FileName = "Pickables.yml";
    private static readonly string FilePath = TweaksManager.FolderPath + Path.DirectorySeparatorChar + FileName;
    private static readonly CustomSyncedValue<string> ServerPickableConfigs = new(SeasonalityPlugin.ConfigSync, "ServerPickableConfigs", "");
    private static ConfigEntry<Toggle> m_enabled = null!;
    public static readonly Dictionary<string, Harvest> m_data = new();
    private static readonly ISerializer serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().Build();

    public static void Setup()
    {
        m_enabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Pickable Enabled", Toggle.Off, "If on, pickable tweaks are enabled");
        Read();
        ServerPickableConfigs.ValueChanged += OnServerValueChanged;
        
        if (m_data.Count == 0)
        {
            TweaksManager.OnZNetScenePrefab += prefab =>
            {
                if (m_data.ContainsKey(prefab.name)) return;
                if (prefab.TryGetComponent(out Pickable pickable))
                {
                    m_data[prefab.name] = new Harvest()
                    {
                        Summer = CreateData(pickable.m_amount, true),
                        Fall = CreateData(pickable.m_amount, true),
                        Winter = CreateData(pickable.m_amount, false),
                        Spring = CreateData(pickable.m_amount, true)
                    };
                }
            };
            TweaksManager.OnFinishSetup += Write;
        }
        
        TweaksManager.OnZNetAwake += () =>
        {
            UpdateServerConfigs();
            SetupFileWatch();
        };
    }

    private static void OnServerValueChanged()
    {
        if (!ZNet.m_instance || ZNet.m_instance.IsServer()) return;
        if (string.IsNullOrEmpty(ServerPickableConfigs.Value)) return;
        try
        {
            m_data.Clear();
            m_data.AddRange(deserializer.Deserialize<Dictionary<string, Harvest>>(ServerPickableConfigs.Value));
        }
        catch
        {
            SeasonalityPlugin.Record.LogWarning("Failed to deserialize server pickable configs");
        }
    }
    
    public static void SetupFileWatch()
    {
        void ReadConfigValues(object sender, FileSystemEventArgs args)
        {
            if (!ZNet.instance || !ZNet.instance.IsServer()) return;
            Read();
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
        ServerPickableConfigs.Value = serializer.Serialize(m_data);
    }
    

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
    private static class Pickable_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Pickable __instance)
        {
            if (m_enabled.Value is Toggle.Off) return;
            if (!GetHarvestData(__instance.name.Replace("(Clone)", string.Empty), out Harvest.HarvestData data)) return;
            __instance.m_amount = data.Amount;
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    private static class Pickable_Interact_Patch
    {
        [UsedImplicitly]
        private static bool Prefix(Pickable __instance)
        {
            if (m_enabled.Value is Toggle.Off) return true;
            if (!GetHarvestData(__instance.name.Replace("(Clone)", string.Empty), out Harvest.HarvestData data)) return true;
            return data.CanHarvest;
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.GetHoverText))]
    private static class Pickable_GetHoverText_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Pickable __instance, ref string __result)
        {
            if (m_enabled.Value is Toggle.Off) return;
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
            case Season.Fall:
                result = data.Fall;
                break;
            case Season.Winter:
                result = data.Winter;
                break;
            case Season.Spring:
                result = data.Spring;
                break;
            case Season.Summer:
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
        if (!File.Exists(FilePath)) return;
        try
        {
            m_data.Clear();
            m_data.AddRange(deserializer.Deserialize<Dictionary<string, Harvest>>(File.ReadAllText(FilePath)));
        }
        catch
        {
            SeasonalityPlugin.Record.LogWarning("Failed to deserialize: " + Path.GetFileName(FilePath));
        }
    }

    public static void Write()
    {
        if (!Directory.Exists(TweaksManager.FolderPath)) Directory.CreateDirectory(TweaksManager.FolderPath);
        string data = serializer.Serialize(m_data);
        File.WriteAllText(FilePath, data);
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