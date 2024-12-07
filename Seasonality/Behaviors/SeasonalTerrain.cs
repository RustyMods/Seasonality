using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Seasonality.Behaviors;

public class SeasonalTerrain : MonoBehaviour
{
    private static readonly List<SeasonalTerrain> m_instances = new();

    private Heightmap m_heightmap = null!;

    public static void UpdateTerrain()
    {
        foreach(var instance in m_instances) instance.ChangeTerrainColor();
    }

    public void Load()
    {
        m_heightmap = GetComponent<Heightmap>();
        m_instances.Add(this);
    }

    public void ChangeTerrainColor()
    {
        Heightmap.s_tempColors.Clear();
        var instance = WorldGenerator.instance;
        var vector3 = transform.position +
                      new Vector3((float)(m_heightmap.m_width * (double)m_heightmap.m_scale * -0.5), 0.0f,
                          (float)(m_heightmap.m_width * (double)m_heightmap.m_scale * -0.5));
        var num = m_heightmap.m_width + 1;
        for (var index1 = 0; index1 < num; ++index1)
        {
            var iy = DUtils.SmoothStep(0.0f, 1f, (float)index1 / m_heightmap.m_width);
            for (var index2 = 0; index2 < num; ++index2)
            {
                var ix = DUtils.SmoothStep(0.0f, 1f, index2 / (float)m_heightmap.m_width);
                Heightmap.s_tempUVs.Add(new Vector2(index2 / (float)m_heightmap.m_width,
                    index1 / (float)m_heightmap.m_width));
                if (m_heightmap.m_isDistantLod)
                {
                    var wx = vector3.x + index2 * m_heightmap.m_scale;
                    var wy = vector3.z + index1 * m_heightmap.m_scale;
                    var biome = instance.GetBiome(wx, wy);
                    Heightmap.s_tempColors.Add(Heightmap.GetBiomeColor(biome));
                }
                else
                {
                    Heightmap.s_tempColors.Add(m_heightmap.GetBiomeColor(ix, iy));
                }
            }
        }
        
        m_heightmap.m_renderMesh.SetColors(Heightmap.s_tempColors);
    }


    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.Awake))]
    private static class Heightmap_Awake_Patch
    {
        private static void Postfix(Heightmap __instance)
        {
            __instance.gameObject.AddComponent<SeasonalTerrain>();
        }
    }

    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.OnEnable))]
    private static class Heightmap_OnEnable_Patch
    {
        private static void Postfix(Heightmap __instance)
        {
            if (!__instance.TryGetComponent(out SeasonalTerrain component)) return;
            component.Load();
        }
    }

    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.OnDisable))]
    private static class Heightmap_OnDisable_Patch
    {
        private static void Postfix(Heightmap __instance)
        {
            if (!__instance.TryGetComponent(out SeasonalTerrain component)) return;
            m_instances.Remove(component);
        }
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