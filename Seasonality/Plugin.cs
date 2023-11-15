using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Seasonality.Seasons;
using ServerSync;
using static Seasonality.Seasons.Environment;


namespace Seasonality
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class SeasonalityPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Seasonality";
        internal const string ModVersion = "0.1.0";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource SeasonalityLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle
        {
            On = 1,
            Off = 0
        }
        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            
            CustomTextures.ReadCustomTextures();
            
            InitConfigs();
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        public enum Season
        {
            Spring = 0,
            Summer = 1,
            Fall = 2,
            Winter = 3
        }

        public static ConfigEntry<Season> _Season = null!;
        public static ConfigEntry<int> _SeasonDuration = null!;
        public static ConfigEntry<int> _WeatherDuration = null!;

        public static ConfigEntry<Toggle> _SeasonalEffectsEnabled = null!;
        
        public static ConfigEntry<string> _SpringName = null!;
        public static ConfigEntry<string> _SpringStartMsg = null!;
        public static ConfigEntry<string> _SpringTooltip = null!;
        public static ConfigEntry<Modifier> _SpringModifier = null!;
        public static ConfigEntry<string> _SpringResistance = null!;
        public static ConfigEntry<float> _SpringValue = null!;
        
        public static ConfigEntry<string> _FallName = null!;
        public static ConfigEntry<string> _FallStartMsg = null!;
        public static ConfigEntry<string> _FallTooltip = null!;
        public static ConfigEntry<Modifier> _FallModifier = null!;
        public static ConfigEntry<string> _FallResistance = null!;
        public static ConfigEntry<float> _FallValue = null!;
        
        public static ConfigEntry<string> _WinterName = null!;
        public static ConfigEntry<string> _WinterStartMsg = null!;
        public static ConfigEntry<string> _WinterTooltip = null!;
        public static ConfigEntry<Modifier> _WinterModifier = null!;
        public static ConfigEntry<string> _WinterResistance = null!;
        public static ConfigEntry<float> _WinterValue = null!;
        
        public static ConfigEntry<string> _SummerName = null!;
        public static ConfigEntry<string> _SummerStartMsg = null!;
        public static ConfigEntry<string> _SummerTooltip = null!;
        public static ConfigEntry<Modifier> _SummerModifier = null!;
        public static ConfigEntry<string> _SummerResistance = null!;
        public static ConfigEntry<float> _SummerValue = null!;

        public static ConfigEntry<Environments> _FallWeather1 = null!;
        public static ConfigEntry<Environments> _FallWeather2 = null!;
        public static ConfigEntry<Environments> _FallWeather3 = null!;
        public static ConfigEntry<Environments> _FallWeather4 = null!;
        
        public static ConfigEntry<Environments> _WinterWeather1 = null!;
        public static ConfigEntry<Environments> _WinterWeather2 = null!;
        public static ConfigEntry<Environments> _WinterWeather3 = null!;
        public static ConfigEntry<Environments> _WinterWeather4 = null!;
        
        public static ConfigEntry<Environments> _SpringWeather1 = null!;
        public static ConfigEntry<Environments> _SpringWeather2 = null!;
        public static ConfigEntry<Environments> _SpringWeather3 = null!;
        public static ConfigEntry<Environments> _SpringWeather4 = null!;
        
        public static ConfigEntry<Environments> _SummerWeather1 = null!;
        public static ConfigEntry<Environments> _SummerWeather2 = null!;
        public static ConfigEntry<Environments> _SummerWeather3 = null!;
        public static ConfigEntry<Environments> _SummerWeather4 = null!;

        private void InitConfigs()
        {
            _SeasonDuration = config("2 - Utilities", "2 - Duration (Days)", 5, "Time in real days between season changes");
            _Season = config("2 - Utilities", "3 - Season", Season.Fall, "Select the season");
            _WeatherDuration = config("2 - Utilities", "4 - Weather Duration (Minutes)", 20, "Duration between weather change, if season applies weather");
            _SeasonalEffectsEnabled = config("2 - Utilities", "1 - Effects Enabled", Toggle.On,
                "If on, season effects are enabled");

            
            #region SpringConfigs
            _SpringName = config("3 - Spring", "Name", "Spring", "Display name");
            _SpringStartMsg = config("3 - Spring", "Start Message", "Spring has finally arrived",
                "Start of the season message");
            _SpringTooltip = config("3 - Spring", "Tooltip", "The land is bursting with energy",
                "Status effect tooltip");
            _SpringModifier = config("3 - Spring", "Modifier", Modifier.None, "Stats modifier");
            _SpringResistance = config("3 - Spring", "Resistance", "", "Resistance modifier");
            _SpringValue = config("3 - Spring", "Modifying value", 0.0f, "Value applied to modifier");
            
            _SpringWeather1 = config("3 - Spring", "Weather 1", Environments.None, "Environments set by spring season");
            _SpringWeather2 = config("3 - Spring", "Weather 2", Environments.None, "Environments set by spring season");
            _SpringWeather3 = config("3 - Spring", "Weather 3", Environments.None, "Environments set by spring season");
            _SpringWeather4 = config("3 - Spring", "Weather 4", Environments.None, "Environments set by spring season");

            #endregion
            #region FallConfigs
            _FallName = config("4 - Fall", "Name", "Fall", "Display name");
            _FallStartMsg = config("4 - Fall", "Start Message", "Fall is upon us", "Start of the season message");
            _FallTooltip = config("4 - Fall", "Tooltip", "The ground is wet", "Status effect tooltip");
            _FallModifier = config("4 - Fall", "Modifier", Modifier.None, "Stats modifier");
            _FallResistance = config("4 - Fall", "Resistance", "", "Resistance modifier");
            _FallValue = config("4 - Fall", "Modifying value", 0.0f, "Value applied to modifier");

            _FallWeather1 = config("4 - Fall", "Weather 1", Environments.None, "Environments set by fall season");
            _FallWeather2 = config("4 - Fall", "Weather 2", Environments.None, "Environments set by fall season");
            _FallWeather3 = config("4 - Fall", "Weather 3", Environments.None, "Environments set by fall season");
            _FallWeather4 = config("4 - Fall", "Weather 4", Environments.None, "Environments set by fall season");

            #endregion
            #region WinterConfigs
            _WinterName = config("5 - Winter", "Name", "Winter", "Display name");
            _WinterStartMsg = config("5 - Winter", "Start Message", "Winter is coming!", "Start of the season message");
            _WinterTooltip = config("5 - Winter", "Tooltip", "The air is cold", "Status effect tooltip");
            _WinterModifier = config("5 - Winter", "Modifier", Modifier.StaminaRegen, "Stats modifier");
            _WinterResistance = config("5 - Winter", "Resistance", "Fire=Resistant", "Resistance modifier");
            _WinterValue = config("5 - Winter", "Modifying value", 0.9f, "Value applied to modifier");

            _WinterWeather1 = config("5 - Winter", "Weather 1", Environments.Snow, "Environment set by winter season.");
            _WinterWeather2 = config("5 - Winter", "Weather 2", Environments.None, "Environment set by winter season.");
            _WinterWeather3 = config("5 - Winter", "Weather 3", Environments.None, "Environment set by winter season.");
            _WinterWeather4 = config("5 - Winter", "Weather 4", Environments.None, "Environment set by winter season.");
            
            #endregion
            #region SummerConfigs
            _SummerName = config("6 - Summer", "Name", "Summer", "Display name");
            _SummerStartMsg = config("6 - Summer", "Start Message", "Summer has landed", "Start of the season message");
            _SummerTooltip = config("6 - Summer", "Tooltip", "The air is warm", "Status effect tooltip");
            _SummerModifier = config("6 - Summer", "Modifier", Modifier.MaxCarryWeight, "Stats modifier");
            _SummerResistance = config("6 - Summer", "Resistance", "", "Resistance modifier");
            _SummerValue = config("6 - Summer", "Modifying value", -50f, "Value applied to modifier");
            
            _SummerWeather1 = config("6 - Summer", "Weather 1", Environments.None, "Environment set by summer season.");
            _SummerWeather2 = config("6 - Summer", "Weather 2", Environments.None, "Environment set by summer season.");
            _SummerWeather3 = config("6 - Summer", "Weather 3", Environments.None, "Environment set by summer season.");
            _SummerWeather4 = config("6 - Summer", "Weather 4", Environments.None, "Environment set by summer season.");

            #endregion
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

        // private class ConfigurationManagerAttributes
        // {
        //     [UsedImplicitly] public int? Order = null!;
        //     [UsedImplicitly] public bool? Browsable = null!;
        //     [UsedImplicitly] public string? Category = null!;
        //     [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        // }
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