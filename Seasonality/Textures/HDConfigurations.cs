using System;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Seasonality.Textures;

public static class HDConfigurations
{
    private static readonly string SeasonalityFolderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    private static readonly string HDConfigFilePath = SeasonalityFolderPath + Path.DirectorySeparatorChar + "HDConfiguration.yml";

    public static HDConfiguration HdConfigurations = new();
    
    [Serializable]
    public class HDConfiguration
    {
        public TextureSettings defaultSettings = new ()
        {
            TextureFormat = TextureFormat.DXT5,
            TextureFilter = FilterMode.Trilinear
        };
        public TextureSettings GrassSettings = new()
        {
            TextureFormat = TextureFormat.DXT1,
            TextureFilter = FilterMode.Point
        };
        public TextureSettings CreatureSettings = new()
        {
            TextureFormat = TextureFormat.BC7,
            TextureFilter = FilterMode.Point
        };
        public TextureSettings PieceSettings = new()
        {
            TextureFormat = TextureFormat.BC7,
            TextureFilter = FilterMode.Point
        };
        public TextureSettings PickableSettings = new()
        {
            TextureFormat = TextureFormat.BC7,
            TextureFilter = FilterMode.Point
        };
        public TextureSettings ArmorSettings = new()
        {
            TextureFormat = TextureFormat.BC7,
            TextureFilter = FilterMode.Point
        };
    }
    [Serializable]
    public class TextureSettings
    {
        public TextureFormat TextureFormat;
        public FilterMode TextureFilter;
    }

    public static void InitHDConfigurations()
    {
        if (!File.Exists(HDConfigFilePath))
        {
            ISerializer serializer = new SerializerBuilder().Build();
            string data = serializer.Serialize(new HDConfiguration());
            File.WriteAllText(HDConfigFilePath, data);
        }

        ReadHDConfigs();
    }

    private static void ReadHDConfigs()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        HDConfiguration data = deserializer.Deserialize<HDConfiguration>(File.ReadAllText(HDConfigFilePath));
        HdConfigurations = data;
    }
}