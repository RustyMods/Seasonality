using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using Seasonality.Helpers;
using UnityEngine;

namespace Seasonality.GameplayModifiers;

public static class TweaksManager
{
    private static readonly string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    public static readonly string FolderPath = ConfigFolder + Path.DirectorySeparatorChar + "Tweaks";

    public static void Setup()
    {
        if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
        PickableTweaks.Setup();
        PlantTweaks.Setup();
        FishTweaks.Setup();
        SpawnTweaks.Setup();
        TraderTweaks.Setup();
        BuildTweaks.Setup();
        BeeHiveTweaks.Setup();
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static class ZNet_Awake_Patch
    {
        private static void Postfix(ZNet __instance)
        {
            if (!__instance.IsServer()) return;
            PlantTweaks.UpdateServerConfigs();
            PickableTweaks.UpdateServerConfigs();
            PlantTweaks.SetupFileWatch();
            PickableTweaks.SetupFileWatch();
        }
    }
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ZNetScene __instance)
        {
            foreach (GameObject? prefab in __instance.m_prefabs)
            {
                if (prefab == null) continue;
                if (prefab.TryGetComponent(out Pickable pickable))
                {
                    PickableTweaks.m_data[prefab.name] = new PickableTweaks.Harvest()
                    {
                        Summer = PickableTweaks.CreateData(pickable.m_amount, true),
                        Fall = PickableTweaks.CreateData(pickable.m_amount, true),
                        Winter = PickableTweaks.CreateData(pickable.m_amount, false),
                        Spring = PickableTweaks.CreateData(pickable.m_amount, true)
                    };
                }
                else if (prefab.TryGetComponent(out Plant plant))
                {
                    PlantTweaks.m_data[prefab.name] = new PlantTweaks.Plants()
                    {
                        Summer = PlantTweaks.Create(plant.m_minScale, plant.m_maxScale, plant.m_growTimeMax, plant.m_growTime, true),
                        Fall = PlantTweaks.Create(plant.m_minScale, plant.m_maxScale, plant.m_growTimeMax, plant.m_growTime, true),
                        Winter = PlantTweaks.Create(plant.m_minScale, plant.m_maxScale, plant.m_growTimeMax, plant.m_growTime, false),
                        Spring = PlantTweaks.Create(plant.m_minScale, plant.m_maxScale, plant.m_growTimeMax, plant.m_growTime, true),
                    };
                }
                else if (prefab.TryGetComponent(out Character character))
                {
                    if (character is Player) continue;
                    SpawnTweaks.m_data[prefab.name] = new Dictionary<Configs.Season, bool>()
                    {
                        [Configs.Season.Spring] = true,
                        [Configs.Season.Summer] = true,
                        [Configs.Season.Fall] = true,
                        [Configs.Season.Winter] = true,
                    };
                }
            }
            
            PickableTweaks.Read();
            PlantTweaks.Read();
            SpawnTweaks.Read();
        }
    }
}