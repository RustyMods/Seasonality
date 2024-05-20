using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seasonality.Managers;

public static class TerrainManager
{
    public static void UpdateTerrain()
    {
        if (SystemInfo.graphicsDeviceType is GraphicsDeviceType.Null) return;
        foreach (IMonoUpdater? monoUpdater in Heightmap.Instances)
        {
            Heightmap? map = (Heightmap)monoUpdater;
            map.m_doLateUpdate = true;
        }

        Heightmap.ForceGenerateAll();
    }
    
    private static readonly Dictionary<Heightmap.Biome, Color32> defaultMap = new()
    {
        { Heightmap.Biome.Meadows , new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0) },
        { Heightmap.Biome.BlackForest , new Color32((byte) 0, (byte) 0, byte.MaxValue, (byte) 0) },
        { Heightmap.Biome.Swamp , new Color32(byte.MaxValue, (byte) 0, (byte) 0, (byte) 0) },
        { Heightmap.Biome.Mountain , new Color32((byte) 0, byte.MaxValue, (byte) 0, (byte) 0) },
        { Heightmap.Biome.Plains , new Color32((byte) 0, (byte) 0, (byte) 0, byte.MaxValue) },
        { Heightmap.Biome.Mistlands , new Color32((byte) 0, (byte) 0, byte.MaxValue, byte.MaxValue) },
        { Heightmap.Biome.DeepNorth , new Color32((byte) 0, byte.MaxValue, (byte) 0, (byte) 0) },
        { Heightmap.Biome.AshLands , new Color32(byte.MaxValue, (byte) 0, (byte) 0, byte.MaxValue) },
        { Heightmap.Biome.Ocean , new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0) },
    };
    
    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.GetBiomeColor), typeof(Heightmap.Biome))]
    private static class Heightmap_GetBiomeColor_Patch
    {
        private static void Postfix(Heightmap.Biome biome, ref Color32 __result)
        {
            switch (SeasonalityPlugin._Season.Value)
            {
                case SeasonalityPlugin.Season.Winter:
                    switch (biome)
                    {
                        case Heightmap.Biome.Meadows:
                        case Heightmap.Biome.BlackForest:
                        case Heightmap.Biome.Swamp:
                        case Heightmap.Biome.Plains:
                        case Heightmap.Biome.Mistlands:
                            __result = defaultMap[Heightmap.Biome.Mountain];
                            break;
                    }
                    break;
                case SeasonalityPlugin.Season.Fall:
                    switch (biome)
                    {
                        case Heightmap.Biome.Meadows:
                            __result = defaultMap[Heightmap.Biome.Plains];
                            break;
                    }
                    break;
            }
        }
    }
}