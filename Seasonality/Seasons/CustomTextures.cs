using System.Reflection;
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
    public static readonly Texture? BeechLeaf_Small_Spring = RegisterTexture("beech_leaf_small_spring.png");
    public static readonly Texture? OakLeaf_Spring = RegisterTexture("oak_leaf_spring.png");
    public static readonly Texture? BirchLeaf_Spring = RegisterTexture("birch_leaf_spring.png");

    public static readonly Texture? MeadowGrass_Spring = RegisterTexture("grass_terrain_color_spring.png");
    public static readonly Texture? ShootLeaf_Spring = RegisterTexture("ShootLeaf_d_spring.png");

    public static readonly Sprite? WinterIcon = RegisterSprite("WinterIcon.png");
    public static readonly Sprite? FallIcon = RegisterSprite("FallIcon.png");
    public static readonly Sprite? SpringIcon = RegisterSprite("SpringIcon.png");
    public static readonly Sprite? SummerIcon = RegisterSprite("SummerIcon.png");
    
    public static readonly Sprite? ValknutIcon = RegisterSprite("valknutIcon.png");

    
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
    
    public static Sprite? RegisterSprite(string fileName, string folderName = "icons")
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
}