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
    public static readonly Texture? FirTree_Fall = RegisterTexture("Pine_tree_texture_d_fall.png");
    public static readonly Texture? FirTree_Spring = RegisterTexture("Pine_tree_texture_d_spring.png");
    public static readonly Texture? FirTree_Winter = RegisterTexture("Pine_tree_texture_d_snow.png");
    
    public static readonly Texture? PineTree_Fall = RegisterTexture("PineTree_01_fall.png");
    public static readonly Texture? PineTree_Spring = RegisterTexture("PineTree_01_spring.png");
    public static readonly Texture? PineTree_Winter = RegisterTexture("PineTree_01_snow.png");

    public static readonly Texture? SnowTexture = RegisterTexture("snow_clone.png");
    public static readonly Texture? PlainsBush_Winter = RegisterTexture("Bush02_en_d_snow.png");
    public static readonly Texture? Shrub02_Winter = RegisterTexture("shrub_2_snow.png");

    public static readonly Texture? BeechLeaf_Spring = RegisterTexture("beech_leaf_spring.png");
    public static readonly Texture? BeechLeaf_Winter = RegisterTexture("beech_leaf_winter.png");
    
    public static readonly Texture? BeechLeaf_Small_Spring = RegisterTexture("beech_leaf_small_spring.png");
    
    public static readonly Texture? OakLeaf_Spring = RegisterTexture("oak_leaf_spring.png");
    
    public static readonly Texture? BirchLeaf_Spring = RegisterTexture("birch_leaf_spring.png");
    public static readonly Texture? BirchLeaf_Winter = RegisterTexture("birch_leaf_winter.png");

    public static readonly Texture? ShootLeaf_Spring = RegisterTexture("ShootLeaf_d_spring.png");

    public static readonly Sprite? ValknutIcon = RegisterSprite("valknutIcon.png");

    public static readonly Texture? ClutterShrub_Winter = RegisterTexture("clutter_shrub_winter.png");
    public static readonly Texture? Ormbunke_Winter = RegisterTexture("autumn_ormbunke_green_winter.png");

    public static readonly Texture? GrassTerrainFall = RegisterTexture("grass_terrain_color _fall.png");

    public static readonly Texture? Vass_Fall = RegisterTexture("vass_texture01_fall.png");
    public static readonly Texture? Vass_Winter = RegisterTexture("vass_texture01_winter.png");

    public static readonly Texture? WaterLilies_Fall = RegisterTexture("waterlilies_fall.png");
    public static readonly Texture? WaterLilies_Spring = RegisterTexture("waterlilies_spring.png");
    public static readonly Texture? WaterLilies_Winter = RegisterTexture("waterlilies_winter.png");


    public static readonly Dictionary<VegDirectories, Dictionary<Season, Texture?>> CustomRegisteredTextures = new();
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
        
        return texture.LoadImage(buffer) ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
    }
    private static Texture? RegisterCustomTexture(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        return null;
    }
    private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    private static readonly string texturePath = folderPath + Path.DirectorySeparatorChar + "Textures";

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
        PlainsBush,
        Shrub,
        Moss,
        Rock,
        MeadowGrass,
        PlainsGrass,
        BlackForestGrass,
        SwampGrass,
        PlainsFlowers,
        Ormbunke,
        Vass,
        WaterLilies,
        RockPlant,
        Clutter,
        None
    }
    public static void ReadCustomTextures()
    {
        // Create directories if they are missing
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        if (!Directory.Exists(texturePath)) Directory.CreateDirectory(texturePath);

        foreach (VegDirectories directory in Enum.GetValues(typeof(VegDirectories)))
        {
            if (directory is VegDirectories.None) continue;
            string type = directory.ToString();
            if (!Directory.Exists(texturePath + Path.DirectorySeparatorChar + type))
            {
                Directory.CreateDirectory(texturePath + Path.DirectorySeparatorChar + type);
            };

            Dictionary<Season, Texture?> map = RegisterCustomTextures(type);
            if (map.Count == 0) continue;
            CustomRegisteredTextures.Add(directory, map);
        }
    }

    private static Dictionary<Season, Texture?> RegisterCustomTextures(string type)
    {
        Dictionary<Season, Texture?> textureMap = new();

        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            string filePath = texturePath + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + ".png");
            if (!File.Exists(filePath)) continue;
            
            string message = type + "/" + season.ToString().ToLower() + ".png"; // Beech/spring.png
            Texture? tex = RegisterCustomTexture(filePath);
            if (!tex)
            {
                SeasonalityLogger.LogDebug($"Failed to register texture: {message}");
                continue;
            }
            textureMap.Add(season, tex);
            SeasonalityLogger.LogDebug($"Registered custom texture: {message}");
        }

        return textureMap;
    }
}