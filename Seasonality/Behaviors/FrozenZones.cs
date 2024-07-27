using System.Collections.Generic;
using Seasonality.Managers;
using UnityEngine;

namespace Seasonality.Behaviors;

public class FrozenZones : MonoBehaviour
{
    public MeshCollider m_surfaceCollider = null!;
    public MeshRenderer m_surfaceRenderer = null!;
    public WaterVolume m_waterVolume = null!;
    public Material m_originalMaterial = null!;

    private static readonly List<FrozenZones> Instances = new();
    public bool m_frozen;

    public static void UpdateInstances()
    {
        foreach (FrozenZones instance in Instances)
        {
            if (SeasonalityPlugin._WinterFreezes.Value is SeasonalityPlugin.Toggle.Off)
            {
                instance.ThawWater();
            }
            else
            {
                if (SeasonalityPlugin._Season.Value is SeasonalityPlugin.Season.Winter 
                    && !instance.IsAshlands()) 
                    instance.FreezeWater();
                else instance.ThawWater();
            }
        }
    }
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
        
        SetInitialValues();
        if (Instances.Contains(this)) return;
        Instances.Add(this);
    }

    private bool IsAshlands() => WorldGenerator.IsAshlands(transform.position.x, transform.position.z);

    public void OnDestroy()
    {
        Instances.Remove(this);
    }

    private void SetInitialValues()
    {
        if (SeasonalityPlugin._Season.Value is SeasonalityPlugin.Season.Winter 
            && SeasonalityPlugin._WinterFreezes.Value is SeasonalityPlugin.Toggle.On 
            && !IsAshlands())
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

    public void FreezeWater()
    {
        if (!m_surfaceCollider || !m_surfaceCollider || !m_waterVolume) return;
        if (m_frozen) return;

        m_surfaceRenderer.material = ZoneManager.SnowMaterial;
        m_surfaceCollider.enabled = true;
        m_waterVolume.m_useGlobalWind = false;
        m_frozen = true;
    }

    public void ThawWater()
    {
        if (!m_surfaceCollider || !m_surfaceCollider || !m_waterVolume) return;
        if (!m_frozen) return;
        
        m_surfaceRenderer.material = m_originalMaterial;
        m_surfaceCollider.enabled = false;
        m_waterVolume.m_useGlobalWind = true;
        
        m_waterVolume.Start();
        
        m_frozen = false;
    }
}