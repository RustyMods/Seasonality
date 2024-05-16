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
            m_originalWaterLevel = m_waterSurface.position;
            if (!m_waterSurface.TryGetComponent(out MeshRenderer renderer)) return;
            m_renderer = renderer;
            m_originalMaterial = renderer.material;
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