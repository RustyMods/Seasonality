using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using Seasonality.Behaviors;
using Seasonality.Helpers;
using Seasonality.Managers;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class Configurations
{
    public enum DurationType { Day, Hours, Minutes }
    public enum DisplayType { Above, Below }
    
    private static readonly Dictionary<List<string>, float> intConfigs = new()
    {
        { new() { "Carry Weight", "Increase or decrease max carry weight" }, 0f },
    };
        
    private static readonly Dictionary<List<string>, float> floatConfigs = new()
    {
        {new(){"Health Regeneration", "Multiply the amount of health regeneration from food"}, 1f},
        {new(){"Damage", "Multiply the amount of damage inflicted on enemies"}, 1f},
        {new(){"Speed", "Multiply the speed"}, 1f},
        {new(){"Eitr Regeneration", "Multiply the amount of eitr regeneration from food"}, 1f},
        {new(){"Raise Skill", "Multiply the amount experience gained for skills"}, 1f},
        {new(){"Stamina Regeneration", "Define the rate of stamina regeneration"}, 1f}
    };
    
    private static ConfigEntry<Toggle> _serverConfigLocked = null!;
    public static ConfigEntry<Season> _Season = null!;

    public static ConfigEntry<Toggle> _ReplaceArmorTextures = null!;
    public static ConfigEntry<Toggle> _ReplaceCreatureTextures = null!;

    public static ConfigEntry<Toggle> _SeasonFades = null!;
    public static ConfigEntry<Toggle> _SleepOverride = null!;
    public static ConfigEntry<float> _FadeLength = null!;
    public static ConfigEntry<Toggle> _WinterFreezes = null!;
    public static ConfigEntry<Toggle> _WinterAlwaysCold = null!;
    public static ConfigEntry<Toggle> _DisplaySeason = null!;
    public static ConfigEntry<Toggle> _DisplaySeasonTimer = null!;
    public static ConfigEntry<DisplayType> _DisplayType = null!;

    public static ConfigEntry<int> _WeatherDuration = null!;
    public static ConfigEntry<Toggle> _DisplayWeather = null!;
    public static ConfigEntry<Toggle> _EnableWeather = null!;
    public static ConfigEntry<Toggle> _DisplayWeatherTimer = null!;
    public static ConfigEntry<Toggle> _EnableModifiers = null!;

    public static readonly Dictionary<Season, Dictionary<DurationType, ConfigEntry<int>>> _Durations = new();

    public static readonly Dictionary<Season, Dictionary<Heightmap.Biome, ConfigEntry<string>>> _WeatherConfigs = new();
        
    public static readonly Dictionary<Season, Dictionary<string, ConfigEntry<float>>> effectConfigs = new();

    public static ConfigEntry<Toggle> _fadeToBlackImmune = null!;

    public static ConfigEntry<double> m_lastSeasonChange = null!;

    public static readonly List<ConfigEntry<Color>> _fallColors = new();
    // public static ConfigEntry<Toggle> _ReplaceGrassTextures = null!;
    
    public static void Init()
    {
        _serverConfigLocked = _plugin.config("1 - General", "1 - Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
        _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
        m_lastSeasonChange = _plugin.config("9 - Data", "Last Season Change", 0.0, "Reset to set last season change");
        _Season = _plugin.config("1 - General", "Current Season", Season.Fall, "Set duration to 0, and select your season, else season is determined by plugin");
        _ReplaceArmorTextures = _plugin.config("2 - Settings", "Replace Armor Textures", Toggle.On, "If on, plugin modifies armor textures");
        _ReplaceCreatureTextures = _plugin.config("2 - Settings", "Replace Creature Textures", Toggle.On, "If on, creature skins change with the seasons");
        _SeasonFades = _plugin.config("2 - Settings", "Fade to Black", Toggle.On, "If on, plugin fades to black before season change");
        _fadeToBlackImmune = _plugin.config("2 - Settings", "Fade Immunity", Toggle.Off, "If on, while fading to black, player is immune to damage");
        _SleepOverride = _plugin.config("2 - Settings", "Sleep Season Change", Toggle.Off, "If on, seasons can only change if everyone is asleep");
        _SleepOverride.SettingChanged += (_, _) => SeasonTimer.m_sleepOverride = _SleepOverride.Value is Toggle.On;
        _FadeLength = _plugin.config("2 - Settings", "Fade Length (seconds)", 3f, new ConfigDescription("Set the length of fade to black", new AcceptableValueRange<float>(1f, 101f)));
        _DisplaySeason = _plugin.config("2 - Settings", "Display Season", Toggle.On, "If on, season will be displayed alongside HUD Status Effects");
        _DisplaySeason.SettingChanged += SeasonManager.OnSeasonDisplayConfigChange;
        _DisplaySeasonTimer = _plugin.config("2 - Settings", "Display Season Timer", Toggle.On, "If on, season timer will be displayed");
        _EnableModifiers = _plugin.config("2 - Settings", "Modifiers", Toggle.Off, "If on, modifiers, as in health regeneration, carry weight etc... are enabled");
        _DisplayType = _plugin.config("2 - Settings", "Name Display", DisplayType.Above, "Set if name of season should be displayed above or below icon");
        _DisplayType.SettingChanged += SeasonManager.OnSeasonDisplayConfigChange;
        _WinterFreezes = _plugin.config("4 - Winter Settings", "Frozen Water", Toggle.On, "If on, winter freezes water");
        _WinterFreezes.SettingChanged += (_, _) =>
        {
            FrozenWaterLOD.UpdateInstances();
            FrozenZones.UpdateInstances();
        };
        _WinterAlwaysCold = _plugin.config("4 - Winter Settings", "Always Cold", Toggle.On, "If on, winter will always make player cold");
        _Season.SettingChanged += SeasonManager.OnSeasonConfigChange;
        _WeatherDuration = _plugin.config("3 - Weather Settings", "Weather Duration (Minutes)", 20, new ConfigDescription("Set duration of custom weather", new AcceptableValueRange<int>(1, 1000)));
        _WeatherDuration.SettingChanged += WeatherManager.OnWeatherDurationChange;
        _EnableWeather = _plugin.config("3 - Weather Settings", "Enabled", Toggle.On, "If on, plugin can control weather and display information");
        _EnableWeather.SettingChanged += WeatherManager.OnDisplayConfigChange;
        _DisplayWeather = _plugin.config("3 - Weather Settings", "Weather Display", Toggle.Off, "If on, plugin can control weather and displays current weather as status effect");
        _DisplayWeather.SettingChanged += WeatherManager.OnDisplayConfigChange;
        _DisplayWeatherTimer = _plugin.config("3 - Weather Settings", "Weather Timer", Toggle.On, "If on, weather status effect displays countdown till next environment");
        List<Color> FallColors = new() { new Color(0.8f, 0.5f, 0f, 1f), new Color(0.8f, 0.3f, 0f, 1f), new Color(0.8f, 0.2f, 0f, 1f), new Color(0.9f, 0.5f, 0f, 1f) };
        for (int index = 0; index < 4; ++index) _fallColors.Add(_plugin.config("5 - Fall Colors", $"Color {index}", FallColors[index], $"Set fall color index {index}"));
        InitDurationConfigs();
        InitWeatherConfigs();
        InitSeasonEffectConfigs();
        
        // _ReplaceGrassTextures = _plugin.config("2 - Settings", "Replace Grass Textures", Toggle.On, "If on, grass change with the seasons");
        // _ReplaceGrassTextures.SettingChanged += (_, _) =>
        // {
        //     SeasonalClutter.UpdateClutter(_ReplaceGrassTextures.Value is Toggle.Off);
        // };
    }
    
    private static void InitSeasonEffectConfigs()
    {
        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            Dictionary<string, ConfigEntry<float>> configs = new Dictionary<string, ConfigEntry<float>>();
            foreach (KeyValuePair<List<string>, float> kvp in floatConfigs)
            {
                string section = kvp.Key[0];
                string description = kvp.Key[1];
                float value = kvp.Value;
                
                ConfigEntry<float> config = _plugin.config($"{season} Modifiers", section, value, new ConfigDescription($"{description}", new AcceptableValueRange<float>(0f, 2f)));
                configs[section] = config;
            }
            foreach (KeyValuePair<List<string>, float> kvp in intConfigs)
            {
                var section = kvp.Key[0];
                var description = kvp.Key[1];
                float value = kvp.Value;
                
                var config = _plugin.config($"{season} Modifiers", section, value, new ConfigDescription($"{description}", new AcceptableValueRange<float>(-300f, 300f)));
                configs[section] = config;
            }
            
            effectConfigs[season] = configs;
        }
    }

    private static void InitWeatherConfigs()
    {
        Assembly? bepinexConfigManager = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ConfigurationManager");

        Type? configManagerType = bepinexConfigManager?.GetType("ConfigurationManager.ConfigurationManager");
        configManager = configManagerType == null
            ? null
            : BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(configManagerType);

        // void ReloadConfigDisplay()
        // {
        //     if (configManagerType?.GetProperty("DisplayingWindow")!.GetValue(configManager) is true)
        //     {
        //         configManagerType.GetMethod("BuildSettingList")!.Invoke(configManager, Array.Empty<object>());
        //     }
        // }
        
        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            Dictionary<Heightmap.Biome, ConfigEntry<string>> configs = new();
            var winterConfig = new SerializedWeather();
            winterConfig.Add("WarmSnow", 1f);
            winterConfig.Add("Twilight_Snow", 0.5f);
            winterConfig.Add("WarmSnowStorm", 0.1f);
            foreach (Heightmap.Biome biome in Enum.GetValues(typeof(Heightmap.Biome)))
            {
                if (biome is Heightmap.Biome.None or Heightmap.Biome.AshLands or Heightmap.Biome.All) continue;
                int index = BiomeIndex(biome);
                ConfigurationManagerAttributes attributes = new() 
                { 
                    CustomDrawer = DrawConfigTable, 
                    Order = 0, 
                    Category = $"{season} Weather Options" 
                };

                if (season is Season.Winter && biome != Heightmap.Biome.Mountain)
                {
                    configs[biome] = _plugin.config($"{season} Weather Options", $"{index} - {biome}", winterConfig.ToString(),
                        new ConfigDescription("Set weather options, [name]:[weight]", null, attributes));
                }
                else
                {
                    configs[biome] = _plugin.config($"{season} Weather Options", $"{index} - {biome}", "",
                        new ConfigDescription("Set weather options, [name]:[weight]", null, attributes));
                }
            }

            _WeatherConfigs[season] = configs;
        }
    }

    private static void InitDurationConfigs()
    {
        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            Dictionary<DurationType, ConfigEntry<int>> configs = new();
            foreach (DurationType type in Enum.GetValues(typeof(DurationType)))
            {
                if (type is DurationType.Hours)
                {
                    configs[type] = _plugin.config($"Duration - {season}", $"Real-Time {type}", 1,
                        new ConfigDescription($"Set the length of {season}", new AcceptableValueRange<int>(0, 1000)));
                }
                else
                {
                    configs[type] = _plugin.config($"Duration - {season}", $"Real-Time {type}", 0,
                        new ConfigDescription($"Set the length of {season}", new AcceptableValueRange<int>(0, 1000)));
                }
            }

            _Durations[season] = configs;
        }
    }

    private static int BiomeIndex(Heightmap.Biome biome)
    {
        return biome switch
        {
            Heightmap.Biome.Meadows => 1,
            Heightmap.Biome.BlackForest => 2,
            Heightmap.Biome.Swamp => 3,
            Heightmap.Biome.Mountain => 4,
            Heightmap.Biome.Plains => 5,
            Heightmap.Biome.Mistlands => 6,
            Heightmap.Biome.Ocean => 7,
            Heightmap.Biome.DeepNorth => 8,
            _ => 0,
        };
    }
    
    private static object? configManager;

    private static void DrawConfigTable(ConfigEntryBase cfg)
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
                .GetProperty("RightColumnWidth", BindingFlags.Instance | BindingFlags.NonPublic)!.GetGetMethod(true)
                .Invoke(configManager, Array.Empty<object>()) ?? 130);
        
        
        GUILayout.BeginVertical();
        foreach (var env in new SerializedWeather((string)cfg.BoxedValue).environments)
        {
            GUILayout.BeginHorizontal();

            float weight = env.weight;
            
            var nameWidth = Mathf.Max(RightColumnWidth - 40 - 21 - 21, 180);
            string newEnvironment = GUILayout.TextField(env.name, new GUIStyle(GUI.skin.textField) { fixedWidth = nameWidth});
            string envName = locked ? env.name : newEnvironment;
            wasUpdated = wasUpdated || envName != env.name;
            
            if (float.TryParse(
                    GUILayout.TextField(weight.ToString(CultureInfo.InvariantCulture), new GUIStyle(GUI.skin.textField) { fixedWidth = 40 }),
                    out float newAmount) && Math.Abs(newAmount - weight) > 0.01f && !locked)
            {
                weight = newAmount;
                wasUpdated = true;
            }

            if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
            {
                wasUpdated = true;
            }
            else
            {
                newWeathers.Add(new WeatherData() { name = envName, weight = weight });
            }

            if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
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

    public class SerializedWeather
    {
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
    }

    public struct WeatherData
    {
        public string name;
        public float weight;
    }
}