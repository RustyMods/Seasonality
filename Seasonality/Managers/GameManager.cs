using System;
using HarmonyLib;
using UnityEngine;

namespace Seasonality.Managers;

public static class GameManager
{
    private static Transform m_waterSurface = null!;
    private static Vector3 m_originalWaterLevel;
    private static MeshRenderer m_renderer = null!;
    private static Material m_originalMaterial = null!;

    private static bool m_waterFrozen;
    
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    private static class Game_Start_Patch
    {
        private static void Postfix(Game __instance)
        {
            if (!__instance) return;
            GameObject _GameMain = __instance.gameObject;
            Transform WaterPlane = Utils.FindChild(_GameMain.transform, "WaterPlane");
            if (!WaterPlane) return;
            m_waterSurface = WaterPlane.GetChild(0);

            m_waterSurface.gameObject.AddComponent<FrozenWaterLOD>();
            
            // m_originalWaterLevel = m_waterSurface.position;
            // if (!m_waterSurface.TryGetComponent(out MeshRenderer renderer)) return;
            // m_renderer = renderer;
            // m_originalMaterial = renderer.material;
        }
    }

    public static void UpdateWaterLOD()
    {
        if (!m_waterSurface) return;
        if (SeasonalityPlugin._WinterFreezes.Value is SeasonalityPlugin.Toggle.Off)
        {
            ThawWaterLOD();
            return;
        }
        if (Player.m_localPlayer)
        {
            if (Player.m_localPlayer.GetCurrentBiome() is Heightmap.Biome.AshLands)
            {
                ThawWaterLOD();
                return;
            }
        }
        if (SeasonalityPlugin._Season.Value is SeasonalityPlugin.Season.Winter)
        {
            FreezeWaterLOD();
        }
        else
        {
           ThawWaterLOD();
        }
    }

    private static void FreezeWaterLOD()
    {
        if (m_waterFrozen) return;
        m_renderer.material = ZoneManager.SnowMaterial;
        m_waterSurface.position = m_originalWaterLevel + new Vector3(0f, -0.2f, 0f);
        m_waterFrozen = true;
    }

    private static void ThawWaterLOD()
    {
        if (!m_waterFrozen) return;
        m_renderer.material = m_originalMaterial;
        m_waterSurface.position = m_originalWaterLevel;
        m_waterFrozen = false;
    }
}

public class FrozenWaterLOD : MonoBehaviour
{
    public Vector3 m_originalPos;
    public MeshRenderer m_renderer = null!;
    public Material m_originalMat = null!;

    public bool m_frozen;

    public void Awake()
    {
        m_originalPos = transform.position;
        m_renderer = GetComponent<MeshRenderer>();
        m_originalMat = m_renderer.material;
    }

    public void Start()
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

    public void Update()
    {
        if (SeasonalityPlugin._WinterFreezes.Value is SeasonalityPlugin.Toggle.On
            && SeasonalityPlugin._Season.Value is SeasonalityPlugin.Season.Winter)
        {
            Freeze();
        }
        else
        {
            Thaw();
        }
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