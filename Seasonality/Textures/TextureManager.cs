using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BepInEx;
using Seasonality.Helpers;
using UnityEngine;

namespace Seasonality.Textures;

public static class TextureManager
{
    private static readonly string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    public static readonly TextureRef Stonemoss_heath = new ("stonemoss_heath");
    public static readonly TextureRef AshOnRocks_d = new("AshOnRocks_d");
    public static readonly TextureRef stonemoss = new("stonemoss");
    public static readonly TextureRef stonekit_moss = new("stonekit_moss");
    public static readonly TextureRef FinalPortal_moss = new("FinalPortal_moss");
    public static readonly TextureRef stonemoss_swamp = new("stonemoss_swamp");
    public static readonly TextureRef dead_moss = new("dead_moss");
    public static readonly TextureRef stonekit_moss_hildir = new("stonekit_moss_hildir");
    public static readonly TextureRef stonemoss_bw = new("stonemoss_bw");
    public static readonly TextureRef groundcreep_d = new("groundcreep_d");
    
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
        public readonly Dictionary<string, ImageData> m_textures = new();

        public TexturePack(ImageData imageData)
        {
            m_materialName = imageData.m_materialName;
            m_textures[imageData.m_fileName] = imageData;
            m_texturePacks[m_materialName] = this;
        }

        public void Add(ImageData imageData)
        {
            m_textures[imageData.m_fileName] = imageData;
        }
    }

    public class ImageData
    {
        public readonly string m_fileName;
        public readonly string m_materialName = "";
        public readonly string m_property = "";
        public readonly byte[] m_bytes = null!;
        public readonly Texture m_texture = null!;
        public readonly Configs.Season m_season;
        public readonly bool m_isValid = true;
        public readonly bool m_isTex = false;

        public ImageData(Texture texture, string fileName, string materialName, Configs.Season season, string property)
        {
            m_texture = texture;
            m_fileName = fileName;
            m_materialName = materialName;
            m_season = season;
            m_property = property;
            m_isTex = true;
        }

        public ImageData(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            m_fileName = fileName;
            if (!Helpers.Utils.ParseName(fileName.Replace(".png", string.Empty), out string materialName, out Configs.Season season, out string property))
            {
                m_isValid = false;
                return;
            }
            m_property = property;
            m_season = season;
            m_materialName = materialName;
            m_bytes = File.ReadAllBytes(filePath);
        }
    }
    public static void Read()
    {
        Stopwatch watch = Stopwatch.StartNew();
        var folderPath = ConfigFolder + Path.DirectorySeparatorChar + Configs.m_rootTextureFolder.Value;
        if (!Directory.Exists(folderPath)) return;
        var filePaths = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);
        if (filePaths.Length <= 0) return;
        foreach (var filePath in filePaths)
        {
            var data = new ImageData(filePath);
            if (!data.m_isValid) continue;
            m_texturePacks.AddOrSet(data.m_materialName, data);
            SeasonalityPlugin.Record.LogSuccess($"Registered: {data.m_fileName}");
        }
        watch.Stop();
        SeasonalityPlugin.Record.LogDebug($"Reading textures took: {watch.ElapsedMilliseconds}ms");
    }

    public static void Save(Material material, string path)
    {
        var ignoredProperties = new List<string>() { "_NoiseTex", "_BumpMap", "_RefractionNormal", "_Normal" };

        if (material == null) return;
        if (material.name.Contains($"{Path.DirectorySeparatorChar}")) return;
        if (material.GetInstanceID() < 0) return;
        foreach (var property in material.GetTexturePropertyNames())
        {
            if (ignoredProperties.Contains(property)) continue;
            var texture = material.GetTexture(property) as Texture2D;
            if (texture == null) continue;
            var fileName = material.name + property.Replace("_", "#");
            var filePath = path + Path.DirectorySeparatorChar + fileName + ".png";
            if (File.Exists(filePath)) continue;
            try
            {
                RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0,
                    RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(texture, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;
                Texture2D newTex = new Texture2D(texture.width, texture.height);
                newTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                newTex.Apply();

                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);

                var encoded = newTex.EncodeToPNG();
                File.WriteAllBytes(filePath, encoded);
                SeasonalityPlugin.Record.LogInfo("Saved: " + fileName);
            }
            catch
            {
                SeasonalityPlugin.Record.LogWarning("Failed: " + fileName);
            }
        }
    }

    private static Texture? RegisterTexture(string fileName, string folderName = "assets")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{SeasonalityPlugin.ModName}.{folderName}.{fileName}";

        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        var _ = stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        texture.name = fileName.Replace(".png", string.Empty);

        return texture.LoadImage(buffer) ? texture : null;
    }
}