using System;
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

    public static event Action? OnZNetAwake;

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
        [UsedImplicitly]
        private static void Postfix(ZNet __instance)
        {
            if (!__instance.IsServer()) return;
            OnZNetAwake?.Invoke();
        }
    }

    public static event Action<GameObject>? OnZNetScenePrefab;
    public static event Action? OnFinishSetup;
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ZNetScene __instance)
        {
            foreach (GameObject? prefab in __instance.m_prefabs)
            {
                if (prefab == null) continue;
                OnZNetScenePrefab?.Invoke(prefab);
            }
            OnFinishSetup?.Invoke();
        }
    }
}