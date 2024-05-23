using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Seasonality.Managers;
using Seasonality.Textures;
using ServerSync;


namespace Seasonality
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class SeasonalityPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Seasonality";
        internal const string ModVersion = "3.3.2";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource SeasonalityLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public static SeasonalityPlugin _plugin = null!;
            
        public enum Toggle { On = 1, Off = 0 }
        public void Awake()
        {
            _plugin = this;
            TextureManager.ReadCustomTextures();
            ZoneManager.InitSnowBundle();
            InitConfigs();
            
            Localizer.Load();
            
            Assembly assembly = Assembly.GetExecutingAssembly(); 
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        public void Update()
        {
            SeasonManager.UpdateSeason();
            SeasonManager.UpdateSeasonEffects();
            GameManager.UpdateWaterLOD();

        }

        public enum Season
        {
            Spring = 0,
            Summer = 1,
            Fall = 2,
            Winter = 3
        }

        public enum DurationType
        {
            Day, Hours, Minutes
        }
        
        #region CustomConfigs
        public static ConfigEntry<Season> _Season = null!;

        public static ConfigEntry<Toggle> _ReplaceArmorTextures = null!;
        public static ConfigEntry<Toggle> _ReplaceCreatureTextures = null!;
        public static ConfigEntry<Toggle> _SeasonFades = null!;
        public static ConfigEntry<float> _FadeLength = null!;
        public static ConfigEntry<Toggle> _WinterFreezes = null!;
        public static ConfigEntry<Toggle> _WeatherFreezes = null!;
        public static ConfigEntry<Toggle> _WinterAlwaysCold = null!;
        public static ConfigEntry<Toggle> _DisplaySeason = null!;
        public static ConfigEntry<Toggle> _DisplaySeasonTimer = null!;

        public static ConfigEntry<int> _WeatherDuration = null!;
        public static ConfigEntry<Toggle> _DisplayWeather = null!;
        public static ConfigEntry<Toggle> _EnableWeather = null!;
        public static ConfigEntry<Toggle> _DisplayWeatherTimer = null!;

        public static readonly Dictionary<Season, Dictionary<DurationType, ConfigEntry<int>>> _Durations = new();

        public static readonly Dictionary<Season, Dictionary<Heightmap.Biome, ConfigEntry<string>>> _WeatherConfigs = new();

        #endregion
        private void InitConfigs()
        {
            _serverConfigLocked = config("1 - General", "1 - Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            
            _Season = config("1 - General", "Current Season", Season.Fall, "Set duration to 0, and select your season, else season is determined by plugin");
            
            _ReplaceArmorTextures = config("2 - Settings", "Replace Armor Textures", Toggle.On, "If on, plugin modifies armor textures");
            _ReplaceCreatureTextures = config("2 - Settings", "Replace Creature Textures", Toggle.On, "If on, creature skins change with the seasons");
            _SeasonFades = config("2 - Settings", "Fade to Black", Toggle.On, "If on, plugin fades to black before season change");
            _FadeLength = config("2 - Settings", "Fade Length (seconds)", 3f, new ConfigDescription("Set the length of fade to black", new AcceptableValueRange<float>(1f, 101f)));
            _DisplaySeason = config("2 - Settings", "Display Season", Toggle.On, "If on, season will be displayed alongside HUD Status Effects");
            _DisplaySeason.SettingChanged += SeasonManager.OnSeasonDisplayConfigChange;
            _DisplaySeasonTimer = config("2 - Settings", "Display Season Timer", Toggle.On, "If on, season timer will be displayed");
            
            _WinterFreezes = config("4 - Winter Settings", "Frozen Water", Toggle.On, "If on, winter freezes water");
            _WeatherFreezes = config("4 - Winter Settings", "Weather Freezes", Toggle.Off, "If off, plugin protects player from freezing except in the mountains");
            _WinterAlwaysCold = config("4 - Winter Settings", "Always Cold", Toggle.On, "If on, winter will always make player cold");
            
            _Season.SettingChanged += SeasonManager.OnSeasonConfigChange;
            _WeatherDuration = config("3 - Weather Settings", "Weather Duration (Minutes)", 20, new ConfigDescription("Set duration of custom weather", new AcceptableValueRange<int>(1, 1000)));
            _WeatherDuration.SettingChanged += WeatherManager.OnWeatherDurationChange;

            _EnableWeather = config("3 - Weather Settings", "Enabled", Toggle.On, "If on, plugin can control weather and display information");
            _EnableWeather.SettingChanged += WeatherManager.OnDisplayConfigChange;
            _DisplayWeather = config("3 - Weather Settings", "Weather Display", Toggle.Off, "If on, plugin can control weather and displays current weather as status effect");
            _DisplayWeather.SettingChanged += WeatherManager.OnDisplayConfigChange;
            _DisplayWeatherTimer = config("3 - Weather Settings", "Weather Timer", Toggle.On, "If on, weather status effect displays countdown till next environment");
            
            InitDurationConfigs();
            
            InitWeatherConfigs();
        }

        private void InitWeatherConfigs()
        {
            foreach (Season season in Enum.GetValues(typeof(Season)))
            {
                Dictionary<Heightmap.Biome, ConfigEntry<string>> configs = new();
                foreach (Heightmap.Biome biome in Enum.GetValues(typeof(Heightmap.Biome)))
                {
                    if (biome is Heightmap.Biome.None or Heightmap.Biome.AshLands or Heightmap.Biome.All) continue;
                    int index = BiomeIndex(biome);
                    if (season is Season.Winter)
                    {
                        configs[biome] = _plugin.config($"{season} Weather Options", $"{index} - {biome}", "Snow:1,Twilight_Snow:0.5,SnowStorm:0.1",
                            "Set weather options, [name]:[weight]");
                    }
                    else
                    {
                        configs[biome] = _plugin.config($"{season} Weather Options", $"{index} - {biome}", "",
                            "Set weather options, [name]:[weight]");
                    }
                }

                _WeatherConfigs[season] = configs;
            }
        }

        private void InitDurationConfigs()
        {
            foreach (Season season in Enum.GetValues(typeof(Season)))
            {
                Dictionary<DurationType, ConfigEntry<int>> configs = new();
                foreach (DurationType type in Enum.GetValues(typeof(DurationType)))
                {
                    if (type is DurationType.Hours)
                    {
                        configs[type] = _plugin.config($"Duration - {season}", $"In-game {type}", 1,
                            new ConfigDescription($"Set the length of {season}", new AcceptableValueRange<int>(0, 1000)));
                    }
                    else
                    {
                        configs[type] = _plugin.config($"Duration - {season}", $"In-game {type}", 0,
                            new ConfigDescription($"Set the length of {season}", new AcceptableValueRange<int>(0, 1000)));
                    }
                }

                _Durations[season] = configs;
            }
        }

        private int BiomeIndex(Heightmap.Biome biome)
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

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                SeasonalityLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                SeasonalityLogger.LogError($"There was an issue loading your {ConfigFileName}");
                SeasonalityLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }
        
        //
        // class AcceptableShortcuts : AcceptableValueBase
        // {
        //     public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
        //     {
        //     }
        //
        //     public override object Clamp(object value) => value;
        //     public override bool IsValid(object value) => true;
        //
        //     public override string ToDescriptionString() =>
        //         "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        // }

        #endregion
    }
}