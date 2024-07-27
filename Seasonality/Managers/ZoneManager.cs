using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Seasonality.Behaviors;
using UnityEngine;

namespace Seasonality.Managers;

public static class ZoneManager
{
    private static AssetBundle _snowBundle = null!;
    public static Material SnowMaterial = null!;

    private static readonly int WaveVel = Shader.PropertyToID("_WaveVel");

    public static void InitSnowBundle()
    {
        _snowBundle = GetAssetBundle("snowmaterialbundle");
        SnowMaterial = _snowBundle.LoadAsset<Material>("BallSnow04");
    }
    private static AssetBundle GetAssetBundle(string fileName)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
        using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
        return AssetBundle.LoadFromStream(stream);
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    private static class ZoneSystem_Start_Patch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            __instance.m_zonePrefab.AddComponent<FrozenZones>();
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
    private static class ZoneSystem_SpawnLocation_Patch
    {
        private static void Postfix(ZoneSystem __instance, ref GameObject __result)
        {
            if (!__instance || !__result) return;
            __result.AddComponent<SeasonalLocation>();
        }
    }

    [HarmonyPatch(typeof(BossStone), nameof(BossStone.Start))]
    private static class BossStone_Start_Patch
    {
        private static void Postfix(BossStone __instance)
        {
            if (!__instance) return;
            __instance.gameObject.AddComponent<SeasonalBossStone>();
        }
    }
    
}