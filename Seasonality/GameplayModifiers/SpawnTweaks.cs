using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;
using ServerSync;
using YamlDotNet.Serialization;

namespace Seasonality.GameplayModifiers;

public static class SpawnTweaks
{
    private static readonly string FileName = "SpawnTweaks.yml";
    private static readonly string FilePath = Path.Combine(TweaksManager.FolderPath, FileName);
    public static readonly Dictionary<string, Dictionary<Season, bool>> m_data = new();
    private static ConfigEntry<Toggle> m_enabled = null!;
    private static readonly ISerializer serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().Build();

    private static readonly CustomSyncedValue<string> ServerSpawnData =
        new(SeasonalityPlugin.ConfigSync, "Seasonality.Server.SpawnData", "");

    public static void OnServerValueChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(ServerSpawnData.Value)) return;
        try
        {
            m_data.Clear();
            m_data.AddRange(deserializer.Deserialize<Dictionary<string, Dictionary<Season, bool>>>(ServerSpawnData.Value));
        }
        catch
        {
            SeasonalityPlugin.Record.LogWarning("Failed to parse server spawn data.");
        }
    }

    public static void UpdateServerConfigs()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        ServerSpawnData.Value = serializer.Serialize(m_data);
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

    public static void Setup()
    {
        m_enabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Spawns Enabled", Toggle.Off, "If on, spawns are modified per season");
        Read();
        
        ServerSpawnData.ValueChanged += OnServerValueChanged;
        if (m_data.Count == 0)
        {
            TweaksManager.OnZNetScenePrefab += prefab =>
            {
                if (m_data.ContainsKey(prefab.name)) return;
                if (prefab.TryGetComponent(out Character character))
                {
                    if (character is Player) return;
                    m_data[prefab.name] = new Dictionary<Season, bool>()
                    {
                        [Season.Spring] = true,
                        [Season.Summer] = true,
                        [Season.Fall] = true,
                        [Season.Winter] = true,
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

    public static void Write()
    {
        var data = serializer.Serialize(m_data);
        File.WriteAllText(FilePath, data);
    }
    public static void Read()
    {
        if (!File.Exists(FilePath)) return;
        try
        {
            m_data.Clear();
            m_data.AddRange(deserializer.Deserialize<Dictionary<string, Dictionary<Season, bool>>>(File.ReadAllText(FilePath)));
        }
        catch
        {
            SeasonalityPlugin.Record.LogWarning("Failed to deserialize spawn tweaks file: " +
                                                Path.GetFileName(FilePath));
        }
    }

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
    private static class SpawnSystem_Spawn_Patch
    {
        private static bool Prefix(SpawnSystem.SpawnData critter)
        {
            if (m_enabled.Value is Toggle.Off) return true;
            if (!m_data.TryGetValue(critter.m_prefab.name, out Dictionary<Season, bool> dict)) return true;
            return !dict.TryGetValue(Configs.m_season.Value, out bool result) || result;
        }
    }
    
    [HarmonyPatch(typeof(SpawnArea), nameof(SpawnArea.SpawnOne))]
    private static class SpawnArea_SpawnOne_Patch
    {
        private static bool Prefix(SpawnArea __instance)
        {
            if (m_enabled.Value is Toggle.Off) return true;
            foreach (var prefab in __instance.m_prefabs)
            {
                if (prefab == null || prefab.m_prefab == null) continue;
                if (!m_data.TryGetValue(prefab.m_prefab.name, out Dictionary<Season, bool> dict)) continue;
                if (!dict.TryGetValue(Configs.m_season.Value, out bool result)) continue;
                if (!result) return false;
            }

            return true;
        }
    }
}