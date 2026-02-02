using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using JetBrains.Annotations;
using UnityEngine;

namespace Seasonality;

[PublicAPI]
public enum Season
{
    Summer = 0, 
    Fall = 1, 
    Winter = 2, 
    Spring = 3
}
public enum DisplayType
{
    Above, Below
}
public static class Configs
{
    private static readonly List<StatusEffectConfig> m_statusEffectConfigs = new()
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

    public static void Init()
    {
        m_season = ConfigManager.config("1 - Settings", "Season", Season.Summer, "Set current season");
        m_season.SettingChanged += (_, _) => SeasonalityPlugin.OnSeasonChange();
        
        m_sleepOverride = ConfigManager.config("1 - Settings", "Sleep Override", Toggle.Off, "If on, season changes when players sleep, instead of on timer end");
        m_sleepOverride.SettingChanged += SeasonalTimer.OnConfigChange;
        m_sleepOverride.SettingChanged += (_, _) =>
        {
            if (m_sleepOverride.Value is Toggle.Off)
            {
                SeasonalTimer.m_sleepOverride = false;
            }
        };
        
        m_seasonFades = ConfigManager.config("1 - Settings", "Fade To Black", Toggle.Off, "If on, screen fades to black when season is changing");
        m_fadeLength = ConfigManager.config("1 - Settings", "Fade Length (seconds)", 3f, "Set length of fade to black");
        m_fadeToBlackImmune = ConfigManager.config("1 - Settings", "Fade Immune", Toggle.Off, "If on, player immune while fading to black");
        m_enableModifiers = ConfigManager.config("1 - Settings", "Modifiers Enabled", Toggle.Off, "If on, season status effect modifiers are enabled");
        m_rootTextureFolder = ConfigManager.config("1 - Settings", "Texture Folder", "Default", "Set the root folder to register textures");
        m_particlesController = ConfigManager.config("1 - Settings", "Particles", Toggle.Off, "If on, particles are affected");
        m_fallObjects = ConfigManager.config("Fall", "Fall Objects",
            new StringListConfig("Beech1", "Birch1", "Birch2", "Birch1_aut", "Birch2_aut", "Beech_small1",
                "Beech_small2", "YggaShoot1", "YggaShoot2", "YggaShoot3", "YggaShoot_small1", "Oak1", "vfx_beech_cut",
                "vfx_birch1_cut", "vfx_birch2_cut", "vfx_birch1_aut_cut", "vfx_birch2_aut_cut", "vfx_oak_cut",
                "vfx_beech_small1_destroy", "vfx_beech_small2_destroy", "vfx_yggashoot_cut",
                "vfx_yggashoot_small1_destroy").ToString(),
            new ConfigDescription("List of objects affected by random fall colors", null, 
                StringListConfig.attributes));
        m_fallMaterials = ConfigManager.config("Fall", "Fall Materials", new StringListConfig("beech_leaf", "beech_particle",
                "beech_leaf_small", "birch_leaf", "birch_leaf_aut", "oak_leaf",
                "Shoot_Leaf_mat", "leaf", "birch_particle", "oak_particle", "shoot_leaf_particle").ToString(),
            new ConfigDescription("List of materials affected by random fall colors", null,
                StringListConfig.attributes));
        
        m_displayTimer = ConfigManager.config("2 - HUD", "Display Timer", Toggle.On, "If on, timer is displayed");
        m_displayType = ConfigManager.config("2 - HUD", "Display Type", DisplayType.Above, "Set where the name of season is displayed");
        m_displaySeason = ConfigManager.config("2 - HUD", "Display Season", Toggle.On, "If on, season is displayed as a status effect");
        m_displayWeather = ConfigManager.config("2 - HUD", "Display Weather", Toggle.Off, "If on, weather environments are displayed as status effect");
        m_displayWeather.SettingChanged += WeatherManager.OnDisplayConfigChange;
        m_displayWeatherTimer = ConfigManager.config("2 - HUD", "Weather Timer", Toggle.Off, "If on, weather timer is displayed");

        m_weatherEnabled = ConfigManager.config("1 - Settings", "Weather Enabled", Toggle.On, "If on, plugin overrides weather");
        m_weatherDuration = ConfigManager.config("1 - Settings", "Weather Length (minutes)", 20, "Set length of environment weather");
        m_weatherDuration.SettingChanged += WeatherManager.OnWeatherDurationChange;
        m_waterFreezes = ConfigManager.config("Winter", "Water Freezes", Toggle.Off, "If on, water freezes and players can walk on it");
        m_waterFreezes.SettingChanged += (_, _) =>
        {
            FrozenZones.UpdateAll();
            FrozenWaterLOD.UpdateAll();
        };
        m_winterAlwaysCold = ConfigManager.config("Winter", "Always Cold", Toggle.Off, "If on, winter is always cold, and applies Cold Status Effect");
        m_addIceShelves = ConfigManager.config("Winter", "Spawn Ice Shelves", Toggle.Off, "If on, ice shelves spawn around coastlines");
        m_addIceShelves.SettingChanged +=  (_, _) => SeasonalIce.UpdateZoneVeg();
        m_displaySeason.SettingChanged += SeasonSE.OnSeasonDisplayConfigChange;
        m_randomColors = ConfigManager.config("Fall", "Random Colors", Toggle.On, "If on, random colors are applied to targeted prefabs");
        m_fallColor1 = ConfigManager.config("Fall", "Color 1", new Color(0.803f, 0.360f, 0.360f, 1f), "Set fall color 1");
        m_fallColor2 = ConfigManager.config("Fall", "Color 2", new Color(0.855f, 0.647f, 0.125f, 1f), "Set fall color 2");
        m_fallColor3 = ConfigManager.config("Fall", "Color 3", new Color(0.914f, 0.455f, 0.318f, 1f), "Set fall color 3");
        m_fallColor4 = ConfigManager.config("Fall", "Color 4", new Color(0.545f, 0.270f, 0.074f, 1f), "Set fall color 4");
        m_fixShader = ConfigManager.config("1 - General", "Fix Shaders", Toggle.Off, "If on, plugin will try to fix shaders");
        
        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            m_durations[season] = ConfigManager.config(season.ToString(), "In-Game Duration", new Vector3(0, 5, 0),
                new ConfigDescription(
                    $"Set length of {season.ToString().ToLower()}, days, hours, minutes (30min real-time = 1 day", null,
                    StatusEffectConfig.attributes));
            m_durations[season].SettingChanged += SeasonalTimer.OnConfigChange;
            
            foreach (StatusEffectConfig? data in m_statusEffectConfigs)
            {
                m_effectConfigs.AddOrSet(season, data.m_name,
                    ConfigManager.config(season.ToString(), data.m_name, data.m_value,
                        new ConfigDescription(data.m_description,
                            new AcceptableValueRange<float>(data.m_range.Key, data.m_range.Value))));
            }

            foreach (Heightmap.Biome biome in Enum.GetValues(typeof(Heightmap.Biome)))
            {
                if (biome is Heightmap.Biome.None or Heightmap.Biome.All) continue;
                WeatherConfig weathers = new WeatherConfig();
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

                m_weatherOptions.AddOrSet(season, biome, 
                    ConfigManager.config(season.ToString(), 
                        $"{biome} Weather", weathers.ToString(), 
                    new ConfigDescription("List of environment names and weights, [env]:[weight],[env]:[weight],...", 
                        null,
                        WeatherConfig.attributes)));
            }
        }
    }
}