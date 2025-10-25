using System;
using System.Collections.Generic;
using HarmonyLib;
using Seasonality.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Seasonality.Behaviors;

public class SeasonalIce : MonoBehaviour
{
    private static GameObject Ice = null!;
    private static readonly ZoneSystem.ZoneVegetation ZoneVeg = new ZoneSystem.ZoneVegetation();

    private static readonly List<SeasonalIce> m_instances = new();
    private Destructible m_destructible = null!;
    public void Awake()
    {
        m_destructible = GetComponent<Destructible>();
        m_instances.Add(this);
        InvokeRepeating(nameof(CheckSeason), 0f, 10f);
    }

    public void OnDestroy()
    {
        m_instances.Remove(this);
    }

    public void CheckSeason()
    {
        if (Configs.m_season.Value is not Season.Winter) m_destructible.DestroyNow();
    }

    public static void UpdateAll()
    {
        foreach (var instance in m_instances)
        {
            instance.CheckSeason();
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix(ZNetScene __instance)
        {
            GameObject? ice = __instance.GetPrefab("ice1");
            Ice = Instantiate(ice, SeasonalityPlugin.Root.transform, false);
            Ice.name = "SeasonalIce";
            Ice.AddComponent<SeasonalIce>();
            __instance.m_prefabs.Add(Ice);
            __instance.m_namedPrefabs[Ice.name.GetStableHashCode()] = Ice;

            ZoneVeg.m_name = "SeasonalIce";
            ZoneVeg.m_prefab = Ice;
            ZoneVeg.m_min = 10;
            ZoneVeg.m_max = 20;
            ZoneVeg.m_biome = Heightmap.Biome.Meadows | Heightmap.Biome.BlackForest | Heightmap.Biome.Swamp |
                              Heightmap.Biome.Plains | Heightmap.Biome.Mistlands;
            ZoneVeg.m_biomeArea = Heightmap.BiomeArea.Everything;
            ZoneVeg.m_blockCheck = true;
            ZoneVeg.m_minAltitude = -1000;
            ZoneVeg.m_maxAltitude = -1;
            
            UpdateZoneVeg();
        }
    }

    public static void UpdateZoneVeg()
    {
        if (!ZoneSystem.instance) return;
        
        if (Configs.m_season.Value is Season.Winter && Configs.m_addIceShelves.Value is Toggle.On)
        {
            if (!ZoneSystem.instance.m_vegetation.Contains(ZoneVeg)) ZoneSystem.instance.m_vegetation.Add(ZoneVeg);
        }
        else
        {
            ZoneSystem.instance.m_vegetation.Remove(ZoneVeg);
        }
    }
}