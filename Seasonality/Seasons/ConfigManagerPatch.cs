using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using ServerSync;
using static Seasonality.Seasons.Environment;

namespace Seasonality.Seasons;

public static class ConfigManagerPatch
{
    private static BaseUnityPlugin? _plugin;

    private static BaseUnityPlugin plugin
    {
        get
        {
            if (_plugin is null)
            {
                IEnumerable<TypeInfo> types;
                try
                {
                    types = Assembly.GetExecutingAssembly().DefinedTypes.ToList();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).Select(t => t.GetTypeInfo());
                }

                _plugin = (BaseUnityPlugin)BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(types.First(t =>
                    t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));
            }

            return _plugin;
        }
    }
    
    private static bool hasConfigSync = true;
    private static object? _configSync;

    private static object? configSync
    {
        get
        {
            if (_configSync == null && hasConfigSync)
            {
                if (Assembly.GetExecutingAssembly().GetType("ServerSync.ConfigSync") is { } configSyncType)
                {
                    _configSync = Activator.CreateInstance(configSyncType, plugin.Info.Metadata.GUID + " ItemManager");
                    configSyncType.GetField("CurrentVersion")
                        .SetValue(_configSync, plugin.Info.Metadata.Version.ToString());
                    configSyncType.GetProperty("IsLocked")!.SetValue(_configSync, true);
                }
                else
                {
                    hasConfigSync = false;
                }
            }

            return _configSync;
        }
    }

    public static readonly List<Environments> registeredEnvironments = new();
    
    internal static void Patch_FejdStartup()
    {
        Assembly? bepinexConfigManager = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "ConfigurationManager");
        
        Type? configManagerType = bepinexConfigManager?.GetType("ConfigurationManager.ConfigurationManager");
        
        bool SaveOnConfigSet = plugin.Config.SaveOnConfigSet;

        foreach (Environments environments in registeredEnvironments)
        {
            ConfigEntry<Environments> environmentConfig(Environments env)
            {
                return config("Environments", "Fall", environments, new ConfigDescription(""));
            }
        }
    }

    private class SerializedEnvironments
    {
        public readonly List<Environments> EnvironmentsList;
        public SerializedEnvironments(List<Environments> envs) => EnvironmentsList = envs;
    }

    private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
    {
        ConfigEntry<T> configEntry = plugin.Config.Bind(group, name, value, description);

        configSync?.GetType().GetMethod("AddConfigEntry")!.MakeGenericMethod(typeof(T))
            .Invoke(configSync, new object[] { configEntry });
        return configEntry;
    }
}