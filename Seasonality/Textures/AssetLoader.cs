using System;
using System.Diagnostics;
using System.IO;
using BepInEx;
using Seasonality.Helpers;
using UnityEngine;

namespace Seasonality.Textures;

public static class AssetLoader
{
    private static readonly string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";

    public static void Read()
    {
        var filePath = ConfigFolder + Path.DirectorySeparatorChar + Configs.m_rootTextureFolder.Value + ".bundle";
        if (!File.Exists(filePath)) return;
        AssetBundle? bundle = AssetBundle.LoadFromFile(filePath);
        if (bundle == null) return;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        foreach (var asset in bundle.LoadAllAssets<Texture>())
        {
            if (!Helpers.Utils.ParseName(asset.name, out string materialName, out Season season, out string property)) continue;
            TextureManager.m_texturePacks.AddOrSet(materialName, new TextureManager.ImageData(asset, asset.name + ".png", materialName, season, property));
        }
        stopwatch.Stop();
        SeasonalityPlugin.Record.LogDebug("Reading textures from bundle took: " + stopwatch.ElapsedMilliseconds + "ms");
    }

}