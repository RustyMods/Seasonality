using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Seasonality.Seasons;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Managers;

public static class ZoneManager
{
    private static AssetBundle _snowBundle = null!;
    public static Material SnowMaterial = null!;

    private static readonly int WaveVel = Shader.PropertyToID("_WaveVel");

    public static void InitSnowBundle()
    {
        _snowBundle = GetAssetBundle("snowmaterialbundle");
        SnowMaterial = _snowBundle.LoadAsset<Material>("BallSnow04");
    }
    private static AssetBundle GetAssetBundle(string fileName)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
        using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
        return AssetBundle.LoadFromStream(stream);
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    private static class ZoneSystem_Start_Patch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            __instance.m_zonePrefab.AddComponent<FrozenZones>();
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
    private static class ZoneSystem_SpawnLocation_Patch
    {
        private static void Postfix(ZoneSystem __instance, ref GameObject __result)
        {
            if (!__instance || !__result) return;
            __result.AddComponent<SeasonalLocation>();
        }
    }
}

public class FrozenZones : MonoBehaviour
{
    public MeshCollider m_surfaceCollider = null!;
    public MeshRenderer m_surfaceRenderer = null!;
    public WaterVolume m_waterVolume = null!;
    public Material m_originalMaterial = null!;

    public bool m_frozen;
    public void Awake()
    {
        Transform water = transform.Find("Water");
        Transform surface = water.Find("WaterSurface");
        Transform volume = water.Find("WaterVolume");

        MeshFilter filter = surface.GetComponent<MeshFilter>();
        m_surfaceRenderer = surface.GetComponent<MeshRenderer>();
        m_waterVolume = volume.GetComponent<WaterVolume>();
        m_surfaceCollider = surface.gameObject.AddComponent<MeshCollider>();
        m_originalMaterial = m_surfaceRenderer.material;
        m_surfaceCollider.sharedMesh = filter.sharedMesh;
    }

    public void Start()
    {
        if (SeasonalityPlugin._Season.Value is SeasonalityPlugin.Season.Winter)
        {
            m_surfaceRenderer.material = ZoneManager.SnowMaterial;
            m_surfaceCollider.enabled = true;
            m_waterVolume.m_useGlobalWind = false;
            m_frozen = true;
        }
        else
        {
            m_surfaceRenderer.material = m_originalMaterial;
            m_surfaceCollider.enabled = false;
            m_waterVolume.m_useGlobalWind = true;
            m_frozen = false;
        }
    }

    public void Update()
    {
        if (!m_surfaceCollider || !m_surfaceRenderer || !m_waterVolume || !m_originalMaterial) return;
        if (!Player.m_localPlayer) return;
        if (SeasonalityPlugin._WinterFreezes.Value is SeasonalityPlugin.Toggle.Off)
        {
            ThawWater();
            return;
        }

        if (Player.m_localPlayer.GetCurrentBiome() is Heightmap.Biome.AshLands)
        {
            ThawWater();
            return;
        }
        if (SeasonalityPlugin._Season.Value is SeasonalityPlugin.Season.Winter)
        {
            FreezeWater();
        }
        else
        {
            ThawWater();
        }
    }

    public void FreezeWater()
    {
        if (m_frozen) return;
        m_surfaceRenderer.material = ZoneManager.SnowMaterial;
        m_surfaceCollider.enabled = true;
        m_waterVolume.m_useGlobalWind = false;
        m_frozen = true;
    }

    public void ThawWater()
    {
        if (!m_frozen) return;
        m_surfaceRenderer.material = m_originalMaterial;
        m_surfaceCollider.enabled = false;
        m_waterVolume.m_useGlobalWind = true;
        m_frozen = false;
    }
}

public class SeasonalLocation : MonoBehaviour
{
    public Renderer[] m_renderers = null!;
    public List<Material> m_materials = new();
    private readonly Dictionary<Material, Texture> m_textureMap = new();
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");

    private SeasonalityPlugin.Season m_lastSeason;

    public void Awake()
    {
        m_renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in m_renderers)
        {
            foreach (var material in renderer.materials)
            {
                m_materials.Add(material);
            }
        }

        foreach (var material in m_materials)
        {
            if (material.HasProperty("_MossTex"))
            {
                m_textureMap[material] = material.GetTexture(MossTex);
            }
        }
    }

    public void Update()
    {
        if (m_lastSeason == SeasonalityPlugin._Season.Value) return;
        switch (SeasonalityPlugin._Season.Value)
        {
            case SeasonalityPlugin.Season.Spring:
                ResetMossTexture();
                m_lastSeason = SeasonalityPlugin.Season.Spring;
                break;
            case SeasonalityPlugin.Season.Summer:
                ResetMossTexture();
                m_lastSeason = SeasonalityPlugin.Season.Summer;
                break;
            case SeasonalityPlugin.Season.Fall:
                if (!MaterialReplacer.CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) return;
                ModifyMoss(HeathMoss);
                m_lastSeason = SeasonalityPlugin.Season.Fall;
                break;
            case SeasonalityPlugin.Season.Winter:
                if (TextureManager.Pillar_Snow == null) return;
                ModifyMoss(TextureManager.Pillar_Snow);
                m_lastSeason = SeasonalityPlugin.Season.Winter;
                break;
        }
    }

    public void ResetMossTexture()
    {
        foreach (var kvp in m_textureMap)
        {
            kvp.Key.SetTexture(MossTex, kvp.Value);
        }
    }

    public void ModifyMoss(Texture texture)
    {
        foreach (var kvp in m_textureMap)
        {
            kvp.Key.SetTexture(MossTex, texture);
        }
    }
}