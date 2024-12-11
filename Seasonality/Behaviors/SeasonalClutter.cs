using System.Collections.Generic;
using HarmonyLib;
using Seasonality.DataTypes;
using Seasonality.Seasons;
using UnityEngine;

namespace Seasonality.Behaviors;

public class SeasonalClutter : MonoBehaviour
{
    private static readonly Dictionary<string, ClutterData> m_originalData = new();
    private static readonly int TerrainColorTex = Shader.PropertyToID("_TerrainColorTex");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly List<SeasonalClutter> m_instances = new();
    private Material m_material = null!;

    private GrassTypes m_type = GrassTypes.None;

    private InstanceRenderer m_instanceRenderer = null!;

    public void Awake()
    {
        m_instanceRenderer = GetComponent<InstanceRenderer>();
        if (!m_instanceRenderer) return;
        m_material = m_instanceRenderer.m_material;
        m_type = GetGrassType(name.Replace("(Clone)", string.Empty));
    }

    public void Enable()
    {
        SetClutter();
        m_instances.Add(this);
    }

    public void Reset()
    {
        if (!m_originalData.TryGetValue(name.Replace("(Clone)", string.Empty), out ClutterData data)) return;
        m_material.SetTexture(MainTex, data.m_mainTex);
        m_material.color = data.m_color;
        if (m_material.HasProperty(TerrainColorTex)) m_material.SetTexture(TerrainColorTex, data.m_terrainTex);
    }

    public void SetClutter()
    {
        Reset();
        var directory = Helpers.Utils.VegToDirectory(m_type);
        if (Helpers.Utils.ApplyBasedOnAvailable(directory, Configurations._Season.Value, m_material, "_MainTex"))
        {
            switch (m_type)
            {
                case GrassTypes.GreenGrass:
                case GrassTypes.GreenGrassShort:
                case GrassTypes.MistlandGrassShort:
                    m_material.SetTexture(TerrainColorTex, null);
                    break;
                case GrassTypes.ClutterShrubs:
                case GrassTypes.Ormbunke:
                case GrassTypes.OrmBunkeSwamp:
                    switch (Configurations._Season.Value)
                    {
                        case SeasonalityPlugin.Season.Fall:
                            m_material.color = new Color(0.8f, 0.5f, 0f, 1f);
                            break;
                    }
                    break;
            }
        }
    }

    public static void UpdateClutter()
    {
        foreach (SeasonalClutter instance in m_instances)
        {
            instance.SetClutter();
        }
    }
    
    
    private static GrassTypes GetGrassType(string clutterName)
    {
        return clutterName switch
        {
            "instanced_meadows_grass" => GrassTypes.GreenGrass,
            "instanced_meadows_grass_short" => GrassTypes.GreenGrassShort,
            "instanced_shrub" => GrassTypes.ClutterShrubs,
            "clutter_shrub_large" => GrassTypes.ClutterShrubs,
            "instanced_forest_groundcover_brown" => GrassTypes.GroundCoverBrown,
            "instanced_forest_groundcover" => GrassTypes.GroundCover,
            "instanced_swamp_grass" => GrassTypes.SwampGrass,
            "instanced_heathgrass" => GrassTypes.HeathGrass,
            "grasscross_heath_green" => GrassTypes.GrassHeathGreen,
            "instanced_heathflowers" => GrassTypes.HeathFlowers,
            "instanced_swamp_ormbunke" => GrassTypes.OrmBunkeSwamp,
            "instanced_ormbunke" => GrassTypes.Ormbunke,
            "instanced_vass" => GrassTypes.Vass,
            "instanced_waterlilies" => GrassTypes.WaterLilies,
            "instanced_mistlands_rockplant" => GrassTypes.RockPlant,
            "instanced_small_rock1" => GrassTypes.Rocks,
            "instanced_mistlands_grass_short" => GrassTypes.MistlandGrassShort,
            _ => GrassTypes.None
        };
    }
    
    [HarmonyPatch(typeof(ClutterSystem), nameof(ClutterSystem.Awake))]
    private static class ClutterSystem_Awake_Patch
    {
        private static void Postfix(ClutterSystem __instance)
        {
            if (!__instance) return;
            foreach (var clutter in __instance.m_clutter)
            {
                if (!clutter.m_prefab.TryGetComponent(out InstanceRenderer renderer)) continue;
                ClutterData data = new(clutter.m_prefab.name, renderer.m_material.color, renderer.m_material.mainTexture);
                if (renderer.m_material.HasProperty(TerrainColorTex))
                {
                    data.m_terrainTex = renderer.m_material.GetTexture(TerrainColorTex);
                }
                clutter.m_prefab.AddComponent<SeasonalClutter>();
            }
        }
    }

    [HarmonyPatch(typeof(InstanceRenderer), nameof(InstanceRenderer.OnEnable))]
    private static class InstanceRenderer_OnEnable_Patch
    {
        private static void Postfix(InstanceRenderer __instance)
        {
            if (!__instance.TryGetComponent(out SeasonalClutter component)) return;
            component.Enable();
        }
    }

    [HarmonyPatch(typeof(InstanceRenderer), nameof(InstanceRenderer.OnDisable))]
    private static class InstanceRenderer_OnDisable_Patch
    {
        private static void Postfix(InstanceRenderer __instance)
        {
            if (!__instance.TryGetComponent(out SeasonalClutter component)) return;
            m_instances.Remove(component);
        }
    }

    private class ClutterData
    {
        public readonly Color m_color;
        public readonly Texture m_mainTex;
        public Texture? m_terrainTex;

        public ClutterData(string name, Color color, Texture main)
        {
            m_color = color;
            m_mainTex = main;
            m_originalData[name] = this;
        }
    }
}