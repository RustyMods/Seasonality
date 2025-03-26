using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Seasonality.Helpers;

public class Records
{
    private static string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    private static string FileName = "Seasonality-LogOutput.log";
    private static string FilePath = ConfigFolder + Path.DirectorySeparatorChar + FileName;
    private readonly ManualLogSource m_manualLogSource;
    private readonly List<string> m_records = new();

    public Records(ManualLogSource logger, string folderName, string fileName)
    {
        m_manualLogSource = logger;
        ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + folderName;
        FileName = fileName;
        FilePath = ConfigFolder + Path.DirectorySeparatorChar + fileName;
    }

    public void LogSuccess(string log)
    {
        m_records.Add($"[Success]: {log}");
    }

    public void LogDebug(string log)
    {
        m_records.Add($"[Debug]: {log}");
        m_manualLogSource.LogDebug(log);
    }

    public void LogInfo(string log)
    {
        m_records.Add($"[Info]: {log}");
        m_manualLogSource.LogInfo(log);
    }

    public void LogWarning(string log)
    {
        m_records.Add($"[Warning]: {log}");
        m_manualLogSource.LogWarning(log);
    }

    public void LogError(string log)
    {
        m_records.Add($"[Error]: {log}");
        m_manualLogSource.LogError(log);
    }

    public void Write()
    {
        if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);
        File.WriteAllLines(FilePath, m_records);
        LogInfo($"{FileName} wrote to file: {FilePath}");
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    private static class Game_Logout_Patch
    {
        private static void Prefix() => SeasonalityPlugin.Record.Write();
    }
}