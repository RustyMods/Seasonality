using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Seasonality;

public class SeasonalIceMineRock : MonoBehaviour
{
    public static readonly List<SeasonalIceMineRock> m_instances = [];
    public MineRock5 m_mineRock5;
    public ZNetView m_nview;
    public void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_mineRock5 = GetComponent<MineRock5>();
        m_instances.Add(this);
        InvokeRepeating(nameof(CheckSeason), 0f, 1f);
    }
    
    public static void UpdateAll()
    {
        foreach (SeasonalIceMineRock? instance in m_instances)
        {
            instance.CheckSeason();
        }
    }
    
    public void CheckSeason()
    {
        if (m_nview == null || m_mineRock5 == null || !m_nview.IsValid() || !m_nview.IsOwner()) return;
        if (SeasonTime.GetCurrentSeason() is not Season.Winter)
        {
            m_nview.Destroy();
            m_mineRock5.m_allDestroyed = true;
        }
    }
    
}

public class SeasonalIce : MonoBehaviour
{
    private static readonly List<ZoneSystem.ZoneVegetation> list = [];

    private static readonly List<SeasonalIce> m_instances = [];
    private Destructible m_destructible = null!;
    public void Awake()
    {
        m_destructible = GetComponent<Destructible>();
        m_instances.Add(this);
        InvokeRepeating(nameof(CheckSeason), 0f, 1f);
    }

    public void OnDestroy()
    {
        m_instances.Remove(this);
    }

    public void CheckSeason()
    {
        if (SeasonTime.GetCurrentSeason() is not Season.Winter)
        {
            m_destructible.DestroyNow();
        }
    }

    public static void UpdateAll()
    {
        foreach (SeasonalIce? instance in m_instances)
        {
            instance.CheckSeason();
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnZone))]
    private static class ZoneSystem_SpawnZone_Patch
    {
        private static void Prefix() => UpdateZoneVeg();
    }
    
    public static void UpdateZoneVeg()
    {
        if (!ZoneSystem.instance) return;
        
        if (SeasonTime.GetCurrentSeason() is Season.Winter && Configs.m_addIceShelves.Value is Toggle.On)
        {
            foreach (var veg in list)
            {
                if (!ZoneSystem.instance.m_vegetation.Contains(veg))
                {
                    ZoneSystem.instance.m_vegetation.Add(veg);
                }
            }
        }
        else
        {
            foreach (var veg in list)
            {
                ZoneSystem.instance.m_vegetation.Remove(veg);
            }
        }
    }

    public static void Setup()
    {
        Clone ice = new Clone("ice1", "SeasonalIce");
        ice.OnCreated += p =>
        {
            p.AddComponent<SeasonalIce>();
            
            if (p.TryGetComponent(out DropOnDestroyed component))
            {
                Destroy(component);
            }
            
            var veg = new ZoneSystem.ZoneVegetation();
            veg.m_name = "Seasonality Ice";
            veg.m_prefab = p;
            veg.m_min = 7;
            veg.m_max = 13;
            veg.m_biome = Heightmap.Biome.Meadows | Heightmap.Biome.BlackForest | Heightmap.Biome.Swamp |
                              Heightmap.Biome.Plains | Heightmap.Biome.Mistlands;
            veg.m_biomeArea = Heightmap.BiomeArea.Everything;
            veg.m_blockCheck = true;
            veg.m_minAltitude = -1000;
            veg.m_maxAltitude = -19;
            veg.m_snapToWater = true;
            veg.m_maxTerrainDelta = 2;
            
            list.Add(veg);
        };
        
        Clone shore = new Clone("IceShore_1", "SeasonalIceShore_1");
        shore.OnCreated += p =>
        {
            p.AddComponent<SeasonalIce>();

            if (p.TryGetComponent(out Destructible component))
            {
                var frac = component.m_spawnWhenDestroyed;
                if (frac != null)
                {
                    var fracClone = Instantiate(frac, CloneManager.GetRootTransform(), false);
                    fracClone.name = "Seasonal_IceShore_frac";
                    if (fracClone.TryGetComponent(out MineRock5 mine)) mine.m_dropItems.m_drops.Clear();
                    fracClone.AddComponent<SeasonalIceMineRock>();

                    component.m_spawnWhenDestroyed = fracClone;
                    PrefabManager.RegisterPrefab(fracClone);
                    CloneManager.prefabs[fracClone.name] = fracClone;
                }
            }
            
            var veg1 = new ZoneSystem.ZoneVegetation();
            veg1.m_name = "Seasonality Ice Shore 1";
            veg1.m_prefab = p;
            veg1.m_min = 5;
            veg1.m_max = 10;
            veg1.m_biome = Heightmap.Biome.Meadows | Heightmap.Biome.BlackForest | Heightmap.Biome.Swamp |
                               Heightmap.Biome.Plains | Heightmap.Biome.Mistlands;
            veg1.m_biomeArea = Heightmap.BiomeArea.Everything;
            veg1.m_blockCheck = true;
            veg1.m_minAltitude = -20;
            veg1.m_maxAltitude = 0.5f;
            veg1.m_snapToWater = true;
            veg1.m_groundOffset = -0.9f;
            list.Add(veg1);
            
            var veg2 = new ZoneSystem.ZoneVegetation();
            veg2.m_name = "Seasonality Ice Shore 2";
            veg2.m_prefab = p;
            veg2.m_min = 5;
            veg2.m_max = 10;
            veg2.m_biome = Heightmap.Biome.Meadows | Heightmap.Biome.BlackForest | Heightmap.Biome.Swamp |
                           Heightmap.Biome.Plains | Heightmap.Biome.Mistlands;
            veg2.m_biomeArea = Heightmap.BiomeArea.Everything;
            veg2.m_blockCheck = true;
            veg2.m_minAltitude = -20;
            veg2.m_maxAltitude = 0.5f;
            veg2.m_snapToWater = true;
            veg2.m_groundOffset = -0.6f;
            list.Add(veg2);
        };
    }
}