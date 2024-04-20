using System;
using Seasonality.Seasons;
using UnityEngine;

namespace Seasonality.Textures;

public class FreezeWater : MonoBehaviour
{
    public Material SnowMaterial = null!;
    public Material OriginalMaterial = null!;

    public MeshRenderer WaterSurfaceRenderer = null!;

    public Transform ZoneWaterVolume = null!;
    public MeshCollider ZoneWaterCollider = null!;

    private readonly int WaveVel = Shader.PropertyToID("_WaveVel");

    private SeasonalityPlugin.Season currentSeason = SeasonalityPlugin.Season.Summer;

    public void Awake()
    {
        Transform WaterSurface = Utils.FindChild(transform, "WaterSurface");
        if (!WaterSurface) return;
        if (!WaterSurface.TryGetComponent(out MeshFilter meshFilter)) return;
        ZoneWaterCollider = WaterSurface.gameObject.AddComponent<MeshCollider>();
        ZoneWaterCollider.sharedMesh = meshFilter.sharedMesh;
        ZoneWaterCollider.enabled = false;
        if (!WaterSurface.TryGetComponent(out MeshRenderer meshRenderer)) return;
        OriginalMaterial = meshRenderer.material;
        WaterSurfaceRenderer = meshRenderer;
        Transform WaterVolume = Utils.FindChild(transform, "WaterVolume");
        if (!WaterVolume) return;
        ZoneWaterVolume = WaterVolume;
    }

    public void OnSeasonChange(SeasonalityPlugin.Season season)
    {
        if (SeasonalityPlugin._WinterFreezesWater.Value is SeasonalityPlugin.Toggle.Off) return;
        WaterSurfaceRenderer.material = season is SeasonalityPlugin.Season.Winter ? SnowMaterial : OriginalMaterial;
        ZoneWaterCollider.enabled = season is SeasonalityPlugin.Season.Winter;
        
        if (!MaterialReplacer.CachedMaterials.TryGetValue("water", out Material water)) return;
        water.SetFloat(WaveVel, season is SeasonalityPlugin.Season.Winter ? 0.0f : 1.0f);
        if (ZoneWaterVolume.TryGetComponent(out WaterVolume waterVolume))
        {
            waterVolume.m_useGlobalWind = season is not SeasonalityPlugin.Season.Winter;
        }
    }

    public void Update()
    {
        if (currentSeason == SeasonalityPlugin._Season.Value) return;
        OnSeasonChange(SeasonalityPlugin._Season.Value);
        currentSeason = SeasonalityPlugin._Season.Value;
    }
}

public class FreezeWaterLoD : MonoBehaviour
{
    public Material SnowMaterial = null!;
    public Material OriginalMaterial = null!;

    public Transform WaterSurface = null!;

    public Vector3 OriginalWaterLevel;
    public MeshRenderer WaterLoDRenderer = null!;

    public SeasonalityPlugin.Season currentSeason = SeasonalityPlugin.Season.Summer;

    public void Awake()
    {
        WaterSurface = transform;
        OriginalWaterLevel = WaterSurface.position;
        if (!WaterSurface.TryGetComponent(out MeshRenderer meshRenderer)) return;
        WaterLoDRenderer = meshRenderer;
        OriginalMaterial = meshRenderer.material;
    }

    public void OnSeasonChange(SeasonalityPlugin.Season season)
    {
        if (SeasonalityPlugin._WinterFreezesWater.Value is SeasonalityPlugin.Toggle.Off) return;
        WaterLoDRenderer.material = season is SeasonalityPlugin.Season.Winter ? SnowMaterial : OriginalMaterial;
        WaterSurface.position = season is SeasonalityPlugin.Season.Winter
            ? OriginalWaterLevel
            : OriginalWaterLevel + new Vector3(0f, -0.2f, 0f);
    }

    public void Update()
    {
        if (currentSeason == SeasonalityPlugin._Season.Value) return;
        OnSeasonChange(SeasonalityPlugin._Season.Value);
        currentSeason = SeasonalityPlugin._Season.Value;
    }
}