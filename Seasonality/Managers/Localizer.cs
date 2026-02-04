using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace Seasonality;

public static class Localizer
{
    private const string FolderName = "Localizations";
    private static readonly string FolderPath;
    private static readonly Dictionary<string, string[]> localizations;
    private static readonly ConfigEntry<Toggle> _fileWatcherEnabled;
    
    static Localizer()
    {
        FolderPath = Path.Combine(ConfigManager.DirectoryPath, FolderName);
        localizations = new Dictionary<string, string[]>();
        _fileWatcherEnabled = ConfigManager.config("File Watcher", FolderName, Toggle.On,
            $"If on, YML files under {FolderName} folder will trigger update on changed, created, or renamed", false);
        
        Harmony harmony = SeasonalityPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(Localization), nameof(Localization.LoadCSV)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(Localizer),
                nameof(Patch_Localization_LoadCSV))));
    }
    
    private static bool IsFileWatcherEnabled() => _fileWatcherEnabled.Value is Toggle.On;

    public static void Start()
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

        string[] files = Directory.GetFiles(FolderPath, "*.yml", SearchOption.AllDirectories);
        if (files.Length <= 0)
        {
            string filePath = Path.Combine(FolderPath, "English.yml");
            List<string> defaultLines = Extensions.ReadAssemblyFile("English.yml", "translations");
            File.WriteAllLines(filePath, defaultLines);
            localizations["English"] = defaultLines.ToArray();
        }
        else
        {
            for (int i = 0; i < files.Length; ++i)
            {
                string filePath = files[i];
                string? fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] extraLines = File.ReadAllLines(filePath);
                List<string> lines = new();
                if (localizations.TryGetValue(fileName, out string[] translations))
                {
                    lines.AddRange(translations);
                    lines.AddRange(extraLines);
                }
                else
                {
                    lines.AddRange(extraLines);
                }
                localizations[fileName] = lines.ToArray();
                SeasonalityPlugin.LogInfo($"Loaded {fileName} localizations");
            }
        }
        
        
        SetupFileWatcher();

    }

    private static void SetupFileWatcher()
    {
        FileSystemWatcher watcher = new(FolderPath, "*.yml");
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private static void Patch_Localization_LoadCSV(Localization __instance, string language)
    {
        if (!localizations.TryGetValue(language, out string[] lines)) return;
        ParseLines(__instance, lines);
    }

    private static void ParseLines(Localization instance, string[] lines)
    {
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;
            string[] parts = line.Split(':');
            if (parts.Length < 2) continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();
            value = instance.StripCitations(value);

            instance.AddWord(key, value);
        }
    }
    
    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!IsFileWatcherEnabled()) return;
        if (!ZNet.instance || !ZNet.instance.IsServer() || Localization.instance == null) return;
        
        string? language = Path.GetFileNameWithoutExtension(e.FullPath);
        string[] lines = File.ReadAllLines(e.FullPath);
        
        Dictionary<string, string> dict = new();

        if (localizations.TryGetValue(language, out string[] translations))
        {
            for (int i = 0; i < translations.Length; ++i)
            {
                string line = translations[i];
                if (string.IsNullOrEmpty(line) || line.StartsWith("$")) continue;
                string[] lineParts = line.Split(':');
                if (lineParts.Length < 2) continue;
                string key =  lineParts[0].Trim();
                string value =  lineParts[1].Trim();
                dict[key] = value;
            }
        }
        
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line) || line.StartsWith("$")) continue;
            string[] lineParts = line.Split(':');
            if (lineParts.Length < 2) continue;
            string key =  lineParts[0].Trim();
            string value =  lineParts[1].Trim();
            dict[key] = value;
        }
        
        string[] newLines = dict.Select(f => string.Join(":", f.Key, f.Value)).ToArray();
        
        localizations[language] = newLines;
        Update();
    }

    private static void Update()
    {
        if (Localization.instance == null) return;
        string lang = Localization.instance.GetSelectedLanguage();
        if (!localizations.TryGetValue(lang, out string[] translations)) return;
        ParseLines(Localization.instance, translations);
        Localization.instance.m_cache.EvictAll();
        SeasonalityPlugin.LogDebug($"Updated {lang} localizations");
    }
}