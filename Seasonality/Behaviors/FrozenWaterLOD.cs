using System.Collections.Generic;
using Seasonality.Managers;
using UnityEngine;

namespace Seasonality.Behaviors;

public class FrozenWaterLOD : MonoBehaviour
{
    public Vector3 m_originalPos;
    public MeshRenderer m_renderer = null!;
    public Material m_originalMat = null!;

    public bool m_frozen;

    private static readonly List<FrozenWaterLOD> Instances = new();

    public static void UpdateInstances()
    {
        foreach (FrozenWaterLOD instance in Instances)
        {
            if (SeasonalityPlugin._Season.Value is SeasonalityPlugin.Season.Winter 
                && SeasonalityPlugin._WinterFreezes.Value is SeasonalityPlugin.Toggle.On
               && Player.m_localPlayer && !WorldGenerator.IsAshlands(Player.m_localPlayer.transform.position.x, Player.m_localPlayer.transform.position.z))
            {
                instance.Freeze();
            }
            else
            {
                instance.Thaw();
            }
        }
    }

    public void Awake()
    {
        m_originalPos = transform.position;
        m_renderer = GetComponent<MeshRenderer>();
        m_originalMat = m_renderer.material;
        SetInitialValues();
        Instances.Add(this);
    }

    private void SetInitialValues()
    {
        if (!m_renderer) return;
        if (SeasonalityPlugin._WinterFreezes.Value is SeasonalityPlugin.Toggle.On
            && SeasonalityPlugin._Season.Value is SeasonalityPlugin.Season.Winter)
        {
            m_renderer.material = ZoneManager.SnowMaterial;
            transform.position = m_originalPos + new Vector3(0f, -0.2f, 0f);
            m_frozen = true;
        }
        else
        {
            m_renderer.material = m_originalMat;
            transform.position = m_originalPos;
            m_frozen = false;
        }
    }

    public void OnDestroy()
    {
        Instances.Remove(this);
    }

    public void Thaw()
    {
        if (!m_frozen) return;
        m_renderer.material = m_originalMat;
        transform.position = m_originalPos;
        m_frozen = false;
    }

    public void Freeze()
    {
        if (m_frozen) return;
        m_renderer.material = ZoneManager.SnowMaterial;
        transform.position = m_originalPos + new Vector3(0f, -0.2f, 0f);
        m_frozen = true;
    }
}