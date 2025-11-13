using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Seasonality.Behaviors;
using ServerSync;
using UnityEngine;

namespace Seasonality.Helpers;

[PublicAPI]
public enum Season
{
    Summer = 0, 
    Fall = 1, 
    Winter = 2, 
    Spring = 3
}

public enum Toggle
{
    On, Off
}

public enum DisplayType
{
    Above, Below
}
public class Configs
{
    private readonly ConfigSync ConfigSync;
    private readonly ConfigFile ConfigFile;
    private readonly string FileName;
    private readonly string FilePath;
    public static object? configManager;

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    private static class FejdStartup_Awake_Patch
    {
        private static void Postfix()
        {
            Assembly? bepinexConfigManager = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ConfigurationManager");

            Type? configManagerType = bepinexConfigManager?.GetType("ConfigurationManager.ConfigurationManager");
            configManager = configManagerType == null
                ? null
                : BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(configManagerType);
        }
    }
    
    public static readonly List<StatusEffectConfig> m_statusEffectConfigs = new()
    {
        new ("Carry Weight", "Increase or decrease max carry weight", 0f, -500f, 500f),
        new ("Health Regeneration", "Multiply the amount of health regeneration from food", 1f, 0f, 10f),
        new ("Damage", "Multiply the amount of damage inflicted on enemies", 1f, 0f, 10f),
        new ("Speed", "Multiply the speed", 1f, 0f, 10f),
        new ("Eitr Regeneration", "Multiply the amount of eitr regeneration from food", 1f, 0f, 10f),
        new ("Raise Skill", "Multiply the amount of experience gained for skills", 1f, 0f, 10f),
        new ("Stamina Regeneration", "Multiply the amount of stamina regeneration", 1f, 0f, 10f),
    };

    public static ConfigEntry<Season> m_season = null!;
    public static ConfigEntry<Toggle> m_waterFreezes = null!;
    public static ConfigEntry<Toggle> m_winterAlwaysCold = null!;
    public static ConfigEntry<double> m_lastSeasonChange = null!;
    public static readonly Dictionary<Season, ConfigEntry<Vector3>> m_durations = new();
    public static ConfigEntry<Toggle> m_seasonFades = null!;
    public static ConfigEntry<float> m_fadeLength = null!;
    public static ConfigEntry<Toggle> m_sleepOverride = null!;
    public static ConfigEntry<Toggle> m_displaySeason = null!;
    public static ConfigEntry<Toggle> m_displayTimer = null!;
    public static ConfigEntry<DisplayType> m_displayType = null!;
    public static readonly Dictionary<Season, Dictionary<string, ConfigEntry<float>>> m_effectConfigs = new();
    public static ConfigEntry<Toggle> m_fadeToBlackImmune = null!;
    public static ConfigEntry<Toggle> m_weatherEnabled = null!;
    public static ConfigEntry<int> m_weatherDuration = null!;
    public static readonly Dictionary<Season, Dictionary<Heightmap.Biome, ConfigEntry<string>>> m_weatherOptions = new();
    public static ConfigEntry<Toggle> m_displayWeather = null!;
    public static ConfigEntry<Toggle> m_displayWeatherTimer = null!;
    public static ConfigEntry<Toggle> m_enableModifiers = null!;
    public static ConfigEntry<string> m_rootTextureFolder = null!;
    public static ConfigEntry<Toggle> m_randomColors = null!;
    public static ConfigEntry<Toggle> m_particlesController = null!;
    public static ConfigEntry<Color> m_fallColor1 = null!;
    public static ConfigEntry<Color> m_fallColor2 = null!;
    public static ConfigEntry<Color> m_fallColor3 = null!;
    public static ConfigEntry<Color> m_fallColor4 = null!;
    public static ConfigEntry<string> m_fallObjects = null!;
    public static ConfigEntry<string> m_fallMaterials = null!;
    public static ConfigEntry<Toggle> m_fixShader = null!;
    public static ConfigEntry<Toggle> m_addIceShelves = null!;

    private void Init()
    {
        m_season = config("1 - Settings", "Season", Season.Summer, "Set current season");
        m_season.SettingChanged += (_, _) => SeasonalityPlugin.OnSeasonChange();
        
        m_lastSeasonChange = config("1 - Settings", "Last Season Change", 0.0, "Record of last season change, reset if you create a new world");
        m_sleepOverride = config("1 - Settings", "Sleep Override", Toggle.Off, "If on, season changes when players sleep, instead of on timer end");
        m_sleepOverride.SettingChanged += SeasonalTimer.OnConfigChange;
        m_sleepOverride.SettingChanged += (_, _) =>
        {
            if (m_sleepOverride.Value is Toggle.Off)
            {
                SeasonalTimer.m_sleepOverride = false;
            }
        };
        
        m_seasonFades = config("1 - Settings", "Fade To Black", Toggle.Off, "If on, screen fades to black when season is changing");
        m_fadeLength = config("1 - Settings", "Fade Length (seconds)", 3f, "Set length of fade to black");
        m_fadeToBlackImmune = config("1 - Settings", "Fade Immune", Toggle.Off, "If on, player immune while fading to black");
        m_enableModifiers = config("1 - Settings", "Modifiers Enabled", Toggle.Off, "If on, season status effect modifiers are enabled");
        m_rootTextureFolder = config("1 - Settings", "Texture Folder", "Default", "Set the root folder to register textures");
        m_particlesController = config("1 - Settings", "Particles", Toggle.Off, "If on, particles are affected");
        m_fallObjects = config("Fall", "Fall Objects",
            new SerializedNameList("Beech1", "Birch1", "Birch2", "Birch1_aut", "Birch2_aut", "Beech_small1",
                "Beech_small2", "YggaShoot1", "YggaShoot2", "YggaShoot3", "YggaShoot_small1", "Oak1", "vfx_beech_cut",
                "vfx_birch1_cut", "vfx_birch2_cut", "vfx_birch1_aut_cut", "vfx_birch2_aut_cut", "vfx_oak_cut",
                "vfx_beech_small1_destroy", "vfx_beech_small2_destroy", "vfx_yggashoot_cut",
                "vfx_yggashoot_small1_destroy").ToString(),
            new ConfigDescription("List of objects affected by random fall colors", null, new ConfigurationManagerAttributes()
            {
                Category = "Fall",
                CustomDrawer = SerializedNameList.Draw
            }));
        m_fallMaterials = config("Fall", "Fall Materials", new SerializedNameList("beech_leaf", "beech_particle",
                "beech_leaf_small", "birch_leaf", "birch_leaf_aut", "oak_leaf",
                "Shoot_Leaf_mat", "leaf", "birch_particle", "oak_particle", "shoot_leaf_particle").ToString(),
            new ConfigDescription("List of materials affected by random fall colors", null,
                new ConfigurationManagerAttributes()
                {
                    Category = "Fall",
                    CustomDrawer = SerializedNameList.Draw
                }));
        
        m_displayTimer = config("2 - HUD", "Display Timer", Toggle.On, "If on, timer is displayed");
        m_displayType = config("2 - HUD", "Display Type", DisplayType.Above, "Set where the name of season is displayed");
        m_displaySeason = config("2 - HUD", "Display Season", Toggle.On, "If on, season is displayed as a status effect");
        m_displayWeather = config("2 - HUD", "Display Weather", Toggle.Off, "If on, weather environments are displayed as status effect");
        m_displayWeather.SettingChanged += WeatherManager.OnDisplayConfigChange;
        m_displayWeatherTimer = config("2 - HUD", "Weather Timer", Toggle.Off, "If on, weather timer is displayed");

        m_weatherEnabled = config("1 - Settings", "Weather Enabled", Toggle.On, "If on, plugin overrides weather");
        m_weatherDuration = config("1 - Settings", "Weather Length (minutes)", 20, "Set length of environment weather");
        m_weatherDuration.SettingChanged += WeatherManager.OnWeatherDurationChange;
        m_waterFreezes = config("Winter", "Water Freezes", Toggle.Off, "If on, water freezes and players can walk on it");
        m_waterFreezes.SettingChanged += (_, _) =>
        {
            FrozenZones.UpdateAll();
            FrozenWaterLOD.UpdateAll();
        };
        m_winterAlwaysCold = config("Winter", "Always Cold", Toggle.Off, "If on, winter is always cold, and applies Cold Status Effect");
        m_addIceShelves = config("Winter", "Spawn Ice Shelves", Toggle.Off, "If on, ice shelves spawn around coastlines");
        m_addIceShelves.SettingChanged +=  (_, _) => SeasonalIce.UpdateZoneVeg();
        
        m_displaySeason.SettingChanged += SeasonSE.OnSeasonDisplayConfigChange;
        m_randomColors = config("Fall", "Random Colors", Toggle.On, "If on, random colors are applied to targeted prefabs");
        m_fallColor1 = config("Fall", "Color 1", new Color(0.803f, 0.360f, 0.360f, 1f), "Set fall color 1");
        m_fallColor2 = config("Fall", "Color 2", new Color(0.855f, 0.647f, 0.125f, 1f), "Set fall color 2");
        m_fallColor3 = config("Fall", "Color 3", new Color(0.914f, 0.455f, 0.318f, 1f), "Set fall color 3");
        m_fallColor4 = config("Fall", "Color 4", new Color(0.545f, 0.270f, 0.074f, 1f), "Set fall color 4");
        m_fixShader = config("1 - General", "Fix Shaders", Toggle.Off, "If on, plugin will try to fix shaders");
        
        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            m_durations[season] = config(season.ToString(), "In-Game Duration", new Vector3(0, 5, 0),
                new ConfigDescription(
                    $"Set length of {season.ToString().ToLower()}, days, hours, minutes (30min real-time = 1 day", null,
                    new ConfigurationManagerAttributes() { Category = season.ToString(), CustomDrawer = StatusEffectConfig.Draw }));
            m_durations[season].SettingChanged += SeasonalTimer.OnConfigChange;
            
            foreach (var data in m_statusEffectConfigs)
            {
                m_effectConfigs.AddOrSet(season, data.m_name,
                    config(season.ToString(), data.m_name, data.m_value,
                        new ConfigDescription(data.m_description,
                            new AcceptableValueRange<float>(data.m_range.Key, data.m_range.Value))));
            }

            foreach (Heightmap.Biome biome in Enum.GetValues(typeof(Heightmap.Biome)))
            {
                if (biome is Heightmap.Biome.None or Heightmap.Biome.All) continue;
                var weathers = new SerializedWeather();
                if (season is Season.Winter)
                {
                    if (biome is Heightmap.Biome.Meadows or Heightmap.Biome.BlackForest or Heightmap.Biome.Swamp
                        or Heightmap.Biome.Plains or Heightmap.Biome.Mistlands or Heightmap.Biome.Ocean)
                    {
                        weathers.Add("WarmSnow", 1f);
                        weathers.Add("Twilight_Snow", 0.5f);
                        weathers.Add("WarmSnowStorm", 0.1f);
                    }
                }

                m_weatherOptions.AddOrSet(season, biome, config(season.ToString(), $"{biome} Weather",
                    weathers.ToString(), new ConfigDescription(
                        "List of environment names and weights, [env]:[weight],[env]:[weight],...", null,
                        new ConfigurationManagerAttributes() { Category = season.ToString(), CustomDrawer = SerializedWeather.Draw, })));
            }
        }

    }

    public class SerializedNameList
    {
        public readonly List<string> m_names;

        public SerializedNameList(List<string> prefabs) => m_names = prefabs;

        public SerializedNameList(params string[] prefabs) => m_names = prefabs.ToList(); 

        public SerializedNameList(string config) => m_names = config.Split(',').ToList();
        
        public override string ToString() => string.Join(",", m_names);

        public static void Draw(ConfigEntryBase cfg)
        {
            bool locked = cfg.Description.Tags
                .Select(a =>
                    a.GetType().Name == "ConfigurationManagerAttributes"
                        ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                        : null).FirstOrDefault(v => v != null) ?? false;
            bool wasUpdated = false;
            List<string> prefabs = new();
            GUILayout.BeginVertical();
            foreach (var prefab in new SerializedNameList((string)cfg.BoxedValue).m_names)
            {
                GUILayout.BeginHorizontal();
                var prefabName = prefab;
                var nameField = GUILayout.TextField(prefab);
                if (nameField != prefab && !locked)
                {
                    wasUpdated = true;
                    prefabName = nameField;
                }

                if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
                {
                    wasUpdated = true;
                }
                else
                {
                    prefabs.Add(prefabName);
                }

                if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
                {
                    prefabs.Add("");
                    wasUpdated = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            if (wasUpdated)
            {
                cfg.BoxedValue = new SerializedNameList(prefabs).ToString();
            }
        }
    }

    public class StatusEffectConfig
     {
         public readonly string m_name;
         public readonly string m_description;
         public readonly float m_value;
         public KeyValuePair<float, float> m_range;

         public StatusEffectConfig(string name, string description, float value, float min, float max)
         {
             m_name = name;
             m_description = description;
             m_value = value;
             m_range = new KeyValuePair<float, float>(min, max);
         }

         public static void Draw(ConfigEntryBase cfg)
         {
             bool locked = cfg.Description.Tags
                 .Select(a =>
                     a.GetType().Name == "ConfigurationManagerAttributes"
                         ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                         : null).FirstOrDefault(v => v != null) ?? false;
             Vector3 value = (Vector3)cfg.BoxedValue;
             GUILayout.BeginVertical();
             GUILayout.BeginHorizontal();
             string? x = GUILayout.TextField(value.x.ToString("0"),
                 new GUIStyle(GUI.skin.textField) { fixedWidth = 50 });
             GUILayout.Label("Days", new GUIStyle(GUI.skin.label) { fixedWidth = 50 });
             string? y = GUILayout.TextField(value.y.ToString("0"),
                 new GUIStyle(GUI.skin.textField) { fixedWidth = 50 });
             GUILayout.Label("Hours", new GUIStyle(GUI.skin.label) { fixedWidth = 50 });
             string? z = GUILayout.TextField(value.z.ToString("0"),
                 new GUIStyle(GUI.skin.textField) { fixedWidth = 50 });
             GUILayout.Label("Minutes", new GUIStyle(GUI.skin.label) { fixedWidth = 50 });
             GUILayout.EndHorizontal();
             GUILayout.EndVertical();
             if (locked) return;
             var newValue = new Vector3(
                 float.TryParse(x, out float days) ? days : 0f,
                 float.TryParse(y, out float hours) ? hours : 0,
                 float.TryParse(z, out float minutes) ? minutes : 0f);
             if (newValue != value)
             {
                 cfg.BoxedValue = newValue;
             }
         }
     }
    
    public class SerializedWeather
    {
        public struct WeatherData
        {
            public string name;
            public float weight;
        }
        
        public readonly List<WeatherData> environments;

        public SerializedWeather() => environments = new List<WeatherData>();
        
        public SerializedWeather(List<WeatherData> weathers) => environments = weathers;

        public SerializedWeather(string weathers)
        {
            environments = weathers
                .Split(',')
                .Select(input =>
                {
                    var parts = input.Split(':');
                    return new WeatherData
                    {
                        name = parts[0],
                        weight = parts.Length > 1 && float.TryParse(parts[1], out float w) ? w : 1f
                    };
                })
                .ToList();
        }

        public void Add(string env, float weight) => environments.Add(new WeatherData()
        {
            name = env,
            weight = weight
        });

        public override string ToString()
        {
            return string.Join(",", environments.Select(e => $"{e.name}:{e.weight}"));
        }

        public static void Draw(ConfigEntryBase cfg)
        {
            bool locked = cfg.Description.Tags
                .Select(a =>
                    a.GetType().Name == "ConfigurationManagerAttributes"
                        ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                        : null).FirstOrDefault(v => v != null) ?? false;

            List<WeatherData> newWeathers = new();
            bool wasUpdated = false;

            int RightColumnWidth =
                (int)(configManager?.GetType()
                    .GetProperty("RightColumnWidth",
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetGetMethod(true)
                    .Invoke(configManager, Array.Empty<object>()) ?? 130);


            GUILayout.BeginVertical();
            foreach (var env in new SerializedWeather((string)cfg.BoxedValue).environments)
            {
                GUILayout.BeginHorizontal();

                float weight = env.weight;

                var nameWidth = Mathf.Max(RightColumnWidth - 40 - 21 - 21, 180);
                string newEnvironment = GUILayout.TextField(env.name,
                    new GUIStyle(GUI.skin.textField) { fixedWidth = nameWidth });
                string envName = locked ? env.name : newEnvironment;
                wasUpdated = wasUpdated || envName != env.name;

                if (float.TryParse(
                        GUILayout.TextField(weight.ToString(CultureInfo.InvariantCulture),
                            new GUIStyle(GUI.skin.textField) { fixedWidth = 40 }),
                        out float newAmount) && Math.Abs(newAmount - weight) > 0.01f && !locked)
                {
                    weight = newAmount;
                    wasUpdated = true;
                }

                if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) &&
                    !locked)
                {
                    wasUpdated = true;
                }
                else
                {
                    newWeathers.Add(new WeatherData() { name = envName, weight = weight });
                }

                if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) &&
                    !locked)
                {
                    wasUpdated = true;
                    newWeathers.Add(new WeatherData() { weight = 1f, name = "" });
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            if (wasUpdated)
            {
                cfg.BoxedValue = new SerializedWeather(newWeathers).ToString();
            }
        }
    }

    // [PublicAPI]
    // public enum Season
    // {
    //     Summer = 0, 
    //     Fall = 1, 
    //     Winter = 2, 
    //     Spring = 3
    // }
    //
    // public enum Toggle
    // {
    //     On, Off
    // }
    //
    // public enum DisplayType
    // {
    //     Above, Below
    // }
    
    public Configs(ConfigFile file, ConfigSync sync, string fileName, string path)
    {
        ConfigFile = file;
        ConfigSync = sync;
        FileName = fileName;
        FilePath = path;
        Init();
        SetupWatcher();
    }
    
    private void SetupWatcher()
    {
        FileSystemWatcher watcher = new(Paths.ConfigPath, FileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(FilePath)) return;
        try
        {
            SeasonalityPlugin.Record.LogDebug("ReadConfigValues called");
            ConfigFile.Reload();
        }
        catch
        {
            SeasonalityPlugin.Record.LogError($"There was an issue loading your {FilePath}");
            SeasonalityPlugin.Record.LogError("Please check your config entries for spelling and format!");
        }
    }

    public ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
    {
        ConfigDescription extendedDescription =
            new(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);
        ConfigEntry<T> configEntry = ConfigFile.Bind(group, name, value, extendedDescription);
        SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    public ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

    public class ConfigurationManagerAttributes
    {
        [UsedImplicitly] public int? Order = null!;
        [UsedImplicitly] public bool? Browsable = null!;
        [UsedImplicitly] public string? Category = null!;
        [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
    }
}