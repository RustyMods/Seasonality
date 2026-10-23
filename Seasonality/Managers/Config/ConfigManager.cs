using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using YamlDotNet.Serialization;

namespace Seasonality;

public enum Toggle { On = 1, Off = 0 }

public static class ConfigManager
{
    private static readonly ConfigFile Config;
    public static readonly ConfigSync ConfigSync;
    private static readonly string ConfigFileName;
    private static readonly string ConfigFileFullPath;
    public static readonly string DirectoryPath;
    private static object? configManager;
    public static int ConfigManagerRightColumnWidth = 130;
    private static readonly ISerializer serializer;
    private static readonly IDeserializer deserializer;

    private static readonly ConfigEntry<LogLevel> logLevels;
    
    static ConfigManager()
    {
        Config = SeasonalityPlugin.instance.Config;
        ConfigFileName = SeasonalityPlugin.ModGUID + ".cfg";
        ConfigFileFullPath = Path.Combine(Paths.ConfigPath, ConfigFileName);
        ConfigSync = new ConfigSync(SeasonalityPlugin.ModGUID)
        {
            DisplayName = SeasonalityPlugin.ModName,
            CurrentVersion = SeasonalityPlugin.ModVersion,
            MinimumRequiredVersion = SeasonalityPlugin.ModVersion
        };

        DirectoryPath = Path.Combine(Paths.ConfigPath, SeasonalityPlugin.ModName);
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }
        
        ConfigEntry<Toggle> serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
            "If on, the configuration is locked and can be changed by server admins only.");
        _ = ConfigSync.AddLockingConfigEntry(serverConfigLocked);

        logLevels = config("1 - General", "Log Levels", LogLevel.All, "Set log levels", false);
        
        serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
            .DisableAliases()
            .Build();
        deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
    }
    
    public static bool ShouldLog(LogLevel type)
    {
        return logLevels.Value.HasFlag(type);
    }
    
    public static T Deserialize<T>(string data) => deserializer.Deserialize<T>(data);
    public static string Serialize<T>(T obj)
    {
        if (obj == null) return "";
        using StringWriter sw = new StringWriter();
        serializer.Serialize(sw, obj, obj.GetType());
        return sw.ToString();
    }

    private static void GetConfigurationManager()
    {
        Assembly? bepinexConfigManager = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ConfigurationManager");

        Type? configManagerType = bepinexConfigManager?.GetType("ConfigurationManager.ConfigurationManager");
        configManager = configManagerType == null
            ? null
            : BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(configManagerType);
        TryGetConfigManagerWidth();
    }

    private static void TryGetConfigManagerWidth()
    {
        if (configManager == null) return;
        Type type = configManager.GetType();
        PropertyInfo? property = type.GetProperty("RightColumnWidth", BindingFlags.Instance | BindingFlags.NonPublic);
        if (property == null) return;
        MethodInfo? method = property.GetGetMethod(true);
        if (method == null) return;
        object? result = method.Invoke(configManager, Array.Empty<object>());
        if (result is not int width) return;
        ConfigManagerRightColumnWidth = width;
    }

    public static void Start()
    {
        Harmony harmony = SeasonalityPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(FejdStartup), nameof(FejdStartup.Awake)),
            new HarmonyMethod(AccessTools.Method(typeof(ConfigManager), nameof(GetConfigurationManager))));

        Configs.Init();
    }
    
    public static void SetupWatcher()
    {
        FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(ConfigFileFullPath)) return;
        try
        {
            SeasonalityPlugin.LogDebug("ReadConfigValues called");
            Config.Reload();
        }
        catch
        {
            SeasonalityPlugin.LogError($"There was an issue loading your {ConfigFileName}");
            SeasonalityPlugin.LogError("Please check your config entries for spelling and format!");
        }
    }
    
    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        ConfigDescription extendedDescription =
            new(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);
        ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);

        SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    public static ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }
}