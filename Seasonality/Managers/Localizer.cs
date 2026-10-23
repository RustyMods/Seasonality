using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace LocalizationManager;

[PublicAPI]
public class Localizer
{
    private static readonly Dictionary<string, Dictionary<string, Func<string>>> PlaceholderProcessors = new();

    private static readonly Dictionary<string, Dictionary<string, string>> loadedTexts = new();

    private static readonly ConditionalWeakTable<Localization, string> localizationLanguage = new();

    private static readonly List<WeakReference<Localization>> localizationObjects = [];

    private static BaseUnityPlugin? _plugin;
    public static event Action? OnLocalizationComplete;

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

                _plugin = (BaseUnityPlugin)Chainloader.ManagerObject.GetComponent(types.First(t => t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));
            }

            return _plugin;
        }
    }

    private static readonly List<string> fileExtensions = [".json", ".yml"];

    private static void UpdatePlaceholderText(Localization localization, string key)
    {
        localizationLanguage.TryGetValue(localization, out string language);
        string text = loadedTexts[language][key];
        if (PlaceholderProcessors.TryGetValue(key, out Dictionary<string, Func<string>> textProcessors))
        {
            text = textProcessors.Aggregate(text, (current, kv) => current.Replace("{" + kv.Key + "}", kv.Value()));
        }

        localization.AddWord(key, text);
    }

    public static void AddPlaceholder<T>(string key, string placeholder, ConfigEntry<T> config, Func<T, string>? convertConfigValue = null) where T : notnull
    {
        convertConfigValue ??= val => val.ToString();
        if (!PlaceholderProcessors.ContainsKey(key))
        {
            PlaceholderProcessors[key] = new Dictionary<string, Func<string>>();
        }

        void UpdatePlaceholder()
        {
            PlaceholderProcessors[key][placeholder] = () => convertConfigValue(config.Value);
            UpdatePlaceholderText(Localization.instance, key);
        }

        config.SettingChanged += (_, _) => UpdatePlaceholder();
        if (loadedTexts.ContainsKey(Localization.instance.GetSelectedLanguage()))
        {
            UpdatePlaceholder();
        }
    }

    public static void AddText(string key, string text)
    {
        List<WeakReference<Localization>> remove = [];
        foreach (WeakReference<Localization> reference in localizationObjects)
        {
            if (reference.TryGetTarget(out Localization localization))
            {
                Dictionary<string, string> texts = loadedTexts[localizationLanguage.GetOrCreateValue(localization)];
                if (!localization.m_translations.ContainsKey(key))
                {
                    texts[key] = text;
                    localization.AddWord(key, text);
                }
            }
            else
            {
                remove.Add(reference);
            }
        }

        foreach (WeakReference<Localization> reference in remove)
        {
            localizationObjects.Remove(reference);
        }
    }

    public static void Load() => _ = plugin;

    public static void LoadLocalizationLater(Localization __instance) => LoadLocalization(Localization.instance, __instance.GetSelectedLanguage());
    public static void SafeCallLocalizeComplete() => OnLocalizationComplete?.Invoke();

    private static void LoadLocalization(Localization __instance, string language)
    {
        if (!localizationLanguage.Remove(__instance))
        {
            localizationObjects.Add(new WeakReference<Localization>(__instance));
        }

        localizationLanguage.Add(__instance, language);

        Dictionary<string, string> localizationFiles = new();
        foreach (string file in Directory.GetFiles(Path.GetDirectoryName(Paths.PluginPath)!, $"{plugin.Info.Metadata.Name}.*", SearchOption.AllDirectories).Where(f => fileExtensions.IndexOf(Path.GetExtension(f)) >= 0))
        {
            string[] parts = Path.GetFileNameWithoutExtension(file).Split('.');
            if (parts.Length < 2)
            {
                continue;
            }

            string key = parts[1];
            if (localizationFiles.ContainsKey(key))
            {
                // Handle duplicate key
                Debug.LogWarning($"Duplicate key {key} found for {plugin.Info.Metadata.Name}. The duplicate file found at {file} will be skipped.");
            }
            else
            {
                localizationFiles[key] = file;
            }
        }

        if (LoadTranslationFromAssembly("English") is not { } englishAssemblyData)
        {
            throw new Exception($"Found no English localizations in mod {plugin.Info.Metadata.Name}. Expected an embedded resource translations/English.json or translations/English.yml.");
        }

        Dictionary<string, string>? localizationTexts = new DeserializerBuilder().IgnoreFields().Build().Deserialize<Dictionary<string, string>?>(Encoding.UTF8.GetString(englishAssemblyData));
        if (localizationTexts is null)
        {
            throw new Exception($"Localization for mod {plugin.Info.Metadata.Name} failed: Localization file was empty.");
        }

        string? localizationData = null;
        if (language != "English")
        {
            if (localizationFiles.TryGetValue(language, out string? localizationFile))
            {
                localizationData = File.ReadAllText(localizationFile);
            }
            else if (LoadTranslationFromAssembly(language) is { } languageAssemblyData)
            {
                localizationData = Encoding.UTF8.GetString(languageAssemblyData);
            }
        }

        if (localizationData is null && localizationFiles.TryGetValue("English", out string? localizationFile1))
        {
            localizationData = File.ReadAllText(localizationFile1);
        }

        if (localizationData is not null)
        {
            foreach (KeyValuePair<string, string> kv in new DeserializerBuilder().IgnoreFields().Build().Deserialize<Dictionary<string, string>?>(localizationData) ?? new Dictionary<string, string>())
            {
                localizationTexts[kv.Key] = kv.Value;
            }
        }

        loadedTexts[language] = localizationTexts;
        foreach (KeyValuePair<string, string> s in localizationTexts)
        {
            UpdatePlaceholderText(__instance, s.Key);
        }
    }

    static Localizer()
    {
        Harmony harmony = new("org.bepinex.helpers.LocalizationManager");
        harmony.Patch(AccessTools.DeclaredMethod(typeof(Localization), nameof(Localization.SetupLanguage)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Localizer), nameof(LoadLocalization))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.SetupGui)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Localizer), nameof(LoadLocalizationLater))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.Start)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Localizer), nameof(SafeCallLocalizeComplete))));
    }

    private static byte[]? LoadTranslationFromAssembly(string language)
    {
        foreach (string extension in fileExtensions)
        {
            if (ReadEmbeddedFileBytes("translations." + language + extension) is { } data)
            {
                return data;
            }
        }

        return null;
    }

    public static byte[]? ReadEmbeddedFileBytes(string resourceFileName, Assembly? containingAssembly = null)
    {
        using MemoryStream stream = new();
        containingAssembly ??= Assembly.GetCallingAssembly();
        if (containingAssembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(resourceFileName, StringComparison.Ordinal)) is { } name)
        {
            containingAssembly.GetManifestResourceStream(name)?.CopyTo(stream);
        }

        return stream.Length == 0 ? null : stream.ToArray();
    }
}

public static class LocalizationManagerVersion
{
    public const string Version = "1.4.1";
}