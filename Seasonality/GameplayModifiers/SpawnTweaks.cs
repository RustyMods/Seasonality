using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;
using YamlDotNet.Serialization;

namespace Seasonality.GameplayModifiers;

public static class SpawnTweaks
{
    private static readonly string FilePath = TweaksManager.FolderPath + Path.DirectorySeparatorChar + "SpawnTweaks.yml";
    public static Dictionary<string, Dictionary<Configs.Season, bool>> m_data = new();
    private static ConfigEntry<Configs.Toggle> m_enabled = null!;

    public static void Setup()
    {
        m_enabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Spawns Enabled", Configs.Toggle.Off, "If on, spawns are modified per season");
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
                m_data =
                    deserializer.Deserialize<Dictionary<string, Dictionary<Configs.Season, bool>>>(
                        File.ReadAllText(FilePath));
            }
            catch
            {
                SeasonalityPlugin.Record.LogWarning("Failed to deserialize spawn tweaks file: " + Path.GetFileName(FilePath));
            }
        }
    }

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
    private static class SpawnSystem_Spawn_Patch
    {
        private static bool Prefix(SpawnSystem.SpawnData critter)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return true;
            if (!m_data.TryGetValue(critter.m_prefab.name, out Dictionary<Configs.Season, bool> dict)) return true;
            return !dict.TryGetValue(Configs.m_season.Value, out bool result) || result;
        }
    }
    
    [HarmonyPatch(typeof(SpawnArea), nameof(SpawnArea.SpawnOne))]
    private static class SpawnArea_SpawnOne_Patch
    {
        private static bool Prefix(SpawnArea __instance)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return true;
            foreach (var prefab in __instance.m_prefabs)
            {
                if (prefab == null || prefab.m_prefab == null) continue;
                if (!m_data.TryGetValue(prefab.m_prefab.name, out Dictionary<Configs.Season, bool> dict)) continue;
                if (!dict.TryGetValue(Configs.m_season.Value, out bool result)) continue;
                if (!result) return false;
            }

            return true;
        }
    }
}