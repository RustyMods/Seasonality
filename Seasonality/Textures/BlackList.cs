using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace Seasonality;

public static class BlackList
{
    private static readonly string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    private const string FileName = "MaterialBlackList.yml";

    private static readonly List<string> IgnoreMaterialList = new();

    public static void Read()
    {
        if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);
        string folderPath = ConfigFolder + Path.DirectorySeparatorChar + Configs.m_rootTextureFolder.Value;
        if (!Directory.Exists(folderPath)) return;
        string filePath = Path.Combine(folderPath, FileName);
        if (!File.Exists(filePath)) return;
        string[] lines = File.ReadAllLines(filePath);
        foreach (string? line in lines)
        {
            if (line.StartsWith("#")) continue;
            IgnoreMaterialList.Add(line);
        }
    }
    
    public static bool ShouldIgnoreMaterial(string material)
    {
        bool flag = IgnoreMaterialList.Contains(material);
        if (flag) SeasonalityPlugin.Record.LogDebug($"Material blacklisted: {material}, ignoring");
        return flag;
    }
}