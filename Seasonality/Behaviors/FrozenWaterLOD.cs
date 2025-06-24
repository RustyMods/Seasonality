using System.Collections.Generic;
using HarmonyLib;
using Seasonality.Helpers;
using Seasonality.Textures;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seasonality.Behaviors;

public class FrozenWaterLOD : MonoBehaviour
{
    public Vector3 m_originalPos;
    public MeshRenderer m_renderer = null!;
    public Material m_originalMat = null!;

    public bool m_frozen;

    private static readonly List<FrozenWaterLOD> Instances = new();

    public static void UpdateAll()
    {
        foreach (FrozenWaterLOD instance in Instances)
        {
            if (Configs.m_season.Value is Configs.Season.Winter && Configs.m_waterFreezes.Value is Configs.Toggle.On)
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
        if (Configs.m_waterFreezes.Value is Configs.Toggle.On
            && Configs.m_season.Value is Configs.Season.Winter)
        {
            m_renderer.material = SeasonalityPlugin.FrozenWaterMat;
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
        m_renderer.material = SeasonalityPlugin.FrozenWaterMat;
        transform.position = m_originalPos + new Vector3(0f, -0.2f, 0f);
        m_frozen = true;
    }
    
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    private static class Game_Start_Patch
    {
        private static void Postfix(Game __instance)
        {
            if (!__instance) return;
            try
            {
                Utils.FindChild(__instance.gameObject.transform, "WaterPlane").GetChild(0).gameObject
                    .AddComponent<FrozenWaterLOD>();
            }
            catch
            {
                SeasonalityPlugin.Record.LogDebug("Failed to find water LOD");
            }
        }
    }
}