using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using Seasonality.Helpers;
using UnityEngine;

namespace Seasonality.Textures;

public static class TextureManager
{
    private static readonly string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    // public static readonly Texture? PillarSnowD2 = RegisterTexture("pillar_snow_d2.png");
    public static readonly TextureRef Stonemoss_heath = new ("stonemoss_heath");
    public static readonly TextureRef AshOnRocks_d = new("AshOnRocks_d");
    public static readonly TextureRef stonemoss = new("stonemoss");
    private static readonly Dictionary<string, Texture> m_cachedTextures = new();
    public static readonly Dictionary<string, TexturePack> m_texturePacks = new();
    public static Dictionary<string, Texture> GetAllTextures(bool clear = false)
    {
        if (clear) m_cachedTextures.Clear();
        if (m_cachedTextures.Count > 0) return m_cachedTextures;
        foreach (var texture in Resources.FindObjectsOfTypeAll<Texture>())
        {
            if (m_cachedTextures.ContainsKey(texture.name)) continue;
            m_cachedTextures[texture.name] = texture;
        }
        return m_cachedTextures;
    }

    public class TextureRef
    {
        private static readonly Texture m_emptyTex = new Texture2D(4, 4);
        private readonly string m_name;
        public Texture? m_tex
        {
            get
            {
                if (tex != null) return tex;
                if (GetAllTextures().TryGetValue(m_name, out var match))
                {
                    tex = match;
                    return tex;
                }
                SeasonalityPlugin.Record.LogWarning($"Failed to find reference texture: {m_name}");
                return m_emptyTex;
            }  
        }

        private Texture? tex;

        public TextureRef(string textureName)
        {
            m_name = textureName;
        }
    }

    public class TexturePack
    {
        public readonly string m_materialName;
        public readonly List<ImageData> m_images = new();

        public TexturePack(ImageData imageData)
        {
            m_materialName = imageData.m_materialName;
            m_images.Add(imageData);
            m_texturePacks[m_materialName] = this;
        }

        public void Add(ImageData imageData) => m_images.Add(imageData);
    }

    public class ImageData
    {
        public readonly string m_fileName;
        public readonly string m_materialName = "";
        public readonly string m_property = "";
        public readonly byte[] m_bytes = null!;
        public readonly Configs.Season m_season;
        public readonly bool m_isValid = true;

        public ImageData(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            m_fileName = fileName;
            if (!fileName.Contains("@"))
            {
                SeasonalityPlugin.Record.LogWarning($"Invalid file name: {fileName}, missing @ to distinguish season");
                m_isValid = false;
                return;
            }
            var name = fileName.Replace(".png", string.Empty);
            var parts = name.Split('@');
            var materialName = parts[0];
            if (!Enum.TryParse(parts[1], true, out Configs.Season season))
            {
                SeasonalityPlugin.Record.LogWarning($"Invalid season: {fileName} - [{parts[1]} is invalid]");
                m_isValid = false;
                return;
            }
            if (materialName.Contains("#"))
            {
                var matParts = materialName.Split('#');
                materialName = matParts[0];
                m_property = "_" + matParts[1];;
            }
            m_season = season;
            m_materialName = materialName;
            m_bytes = File.ReadAllBytes(filePath);
        }
    }
    public static void Read()
    {
        Stopwatch watch = Stopwatch.StartNew();
        var folderPath = ConfigFolder + Path.DirectorySeparatorChar + Configs.m_rootTextureFolder.Value;
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        var filePaths = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);
        if (filePaths.Length <= 0) return;
        foreach (var filePath in filePaths)
        {
            var data = new ImageData(filePath);
            if (!data.m_isValid) continue;
            if (m_texturePacks.TryGetValue(data.m_materialName, out TexturePack group))
            {
                group.Add(data);
            }
            else
            {
                var _ = new TexturePack(data);
            }
            SeasonalityPlugin.Record.LogSuccess($"Registered: {data.m_fileName}");
        }
        watch.Stop();
        SeasonalityPlugin.Record.LogInfo($"Reading textures took: {watch.ElapsedMilliseconds}ms");
    }

    private static Texture? RegisterTexture(string fileName, string folderName = "assets")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{SeasonalityPlugin.ModName}.{folderName}.{fileName}";

        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        var _ =stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        texture.name = fileName.Replace(".png", string.Empty);

        return texture.LoadImage(buffer) ? texture : null;
    }
}