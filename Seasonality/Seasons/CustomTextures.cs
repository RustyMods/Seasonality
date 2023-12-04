using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class CustomTextures
{
    
    public static readonly Sprite? ValknutIcon = RegisterSprite("valknutIcon.png");
    public static readonly Texture? MistLands_Moss = RegisterTexture("mistlands_moss.png");
    public static readonly Texture? Mistlands_Rock_Plant = RegisterTexture("MistlandsVegetation_d.png");

    public static readonly Dictionary<VegDirectories, Dictionary<string, Texture?>> CustomRegisteredTextures = new();

    public static readonly Dictionary<CreatureDirectories, Dictionary<string, Texture?>> CustomRegisteredCreatureTex = new();
    private static Texture? RegisterTexture(string fileName, string folderName = "assets")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{ModName}.{folderName}.{fileName}";

        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);

        return texture.LoadImage(buffer) ? texture : null;
    }

    private static Sprite? RegisterSprite(string fileName, string folderName = "icons")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{ModName}.{folderName}.{fileName}";
        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        
        Sprite? sprite = texture.LoadImage(buffer) ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
        if (sprite != null) sprite.name = fileName;
        return sprite;
    }
    private static Texture? RegisterCustomTexture(string filePath, TextureFormat format, FilterMode filter)
    {
        if (!File.Exists(filePath)) return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(4, 4, format, true, false)
        {
            filterMode = filter,
        };
        if (texture.LoadImage(fileData)) return texture;
        return null;
    }
    
    private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    private static readonly string VegTexturePath = folderPath + Path.DirectorySeparatorChar + "Textures";
    private static readonly string creatureTexPath = folderPath + Path.DirectorySeparatorChar + "Creatures";

    public enum VegDirectories
    {
        Beech,
        BeechSmall,
        Birch,
        Bushes,
        Oak,
        Pine,
        Fir,
        YggaShoot,
        SwampTrees,
        PlainsBush,
        Shrub,
        Moss,
        PlainsMoss,
        SwampMoss,
        Rock,
        MeadowGrass,
        MeadowGrassShort,
        PlainsGrass,
        BlackForestGrass,
        BlackForestGrassAlt,
        SwampGrass,
        MistlandsGrass,
        PlainsFlowers,
        Ormbunke,
        Vass,
        WaterLilies,
        RockPlant,
        Clutter,
        CloudberryBush,
        None
    }

    public enum CreatureDirectories
    {
        None,
        Lox,
        Troll,
    }

    public static bool HDPackLoaded;
    public static void ReadCustomTextures()
    {
        // Create directories if they are missing
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        if (!Directory.Exists(VegTexturePath)) Directory.CreateDirectory(VegTexturePath);

        if (File.Exists(folderPath + Path.DirectorySeparatorChar + "WillyBachHD.md"))
        {
            SeasonalityLogger.LogInfo("Willybach HD loaded");
            HDPackLoaded = true;
        }

        foreach (VegDirectories directory in Enum.GetValues(typeof(VegDirectories)))
        {
            if (directory is VegDirectories.None) continue;
            string type = directory.ToString();
            if (!Directory.Exists(VegTexturePath + Path.DirectorySeparatorChar + type))
            {
                Directory.CreateDirectory(VegTexturePath + Path.DirectorySeparatorChar + type);
            };

            switch (directory)
            {
                case VegDirectories.MistlandsGrass or VegDirectories.PlainsGrass or VegDirectories.SwampGrass or VegDirectories.BlackForestGrass or VegDirectories.BlackForestGrassAlt:
                    Dictionary<string, Texture?> compressedMap = RegisterCustomTextures(type, VegTexturePath, TextureFormat.DXT1, FilterMode.Point);
                    if (compressedMap.Count == 0) continue;
                    CustomRegisteredTextures.Add(directory, compressedMap);
                    break;
                default:
                    Dictionary<string, Texture?> map = RegisterCustomTextures(type, VegTexturePath);
                    if (map.Count == 0) continue;
                    CustomRegisteredTextures.Add(directory, map);
                    break;
            }
            
        }

        foreach (CreatureDirectories creatureDir in Enum.GetValues(typeof(CreatureDirectories)))
        {
            if (creatureDir is CreatureDirectories.None) continue;
            string type = creatureDir.ToString();
            if (!Directory.Exists(creatureTexPath + Path.DirectorySeparatorChar + type))
            {
                Directory.CreateDirectory(creatureTexPath + Path.DirectorySeparatorChar + type);
            }

            Dictionary<string, Texture?> map = RegisterCustomTextures(type, creatureTexPath);
            if (map.Count == 0) continue;
            CustomRegisteredCreatureTex.Add(creatureDir, map);
        }
    }

    private static Dictionary<string, Texture?> RegisterCustomTextures(string type, string path, TextureFormat textureFormat = TextureFormat.DXT5, FilterMode filterMode = FilterMode.Trilinear)
    {
        Dictionary<string, Texture?> textureMap = new();

        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            string filePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + ".png");
            string message = type + "/" + season.ToString().ToLower() + ".png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}"; // Beech/spring.png
            if (File.Exists(filePath))
            {
                Texture? tex = RegisterCustomTexture(filePath, textureFormat, filterMode);
                if (!tex)
                {
                    SeasonalityLogger.LogDebug($"Failed: {message}");
                    continue;
                }
                textureMap.Add(season.ToString(), tex);
                SeasonalityLogger.LogDebug($"Registered: {message}");
            };
            
            string BarkFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_bark.png");
            string barkMessage = type + "/" + season.ToString().ToLower() + "_bark.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";

            if (File.Exists(BarkFilePath))
            {
                Texture? tex = RegisterCustomTexture(filePath, textureFormat, filterMode);
                if (!tex)
                {
                    SeasonalityLogger.LogDebug($"Failed: {barkMessage}");
                    continue;
                }
                textureMap.Add(season.ToString() + "_bark", tex);
                SeasonalityLogger.LogDebug($"Registered: {barkMessage}");
            }
        }

        return textureMap;
    }
}