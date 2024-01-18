using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;
public static class WaterMaterial
{
    private static AssetBundle _snowBundle = null!;
    private static Material SnowMaterial = null!;

    private static Material OriginalWaterMaterial = null!;
    private static Material OriginalWaterLoDMaterial = null!;

    private static MeshRenderer WaterLoDRenderer = null!;
    private static MeshRenderer WaterSurfaceRenderer = null!;
    
    private static Transform WaterLoD = null!;
    private static Vector3 OriginalWaterLevel;

    private static Transform ZoneWaterVolume = null!;
    private static MeshCollider ZoneWaterCollider = null!;
    
    private static readonly int WaveVel = Shader.PropertyToID("_WaveVel");

    public static void InitSnowBundle()
    {
        _snowBundle = GetAssetBundle("snowmaterialbundle");
        SnowMaterial = _snowBundle.LoadAsset<Material>("BallSnow04");
    }
    private static void CacheWaterLoD(Game instance)
    {
        if (!instance) return;
        GameObject _GameMain = instance.gameObject;
        Transform WaterPlane = global::Utils.FindChild(_GameMain.transform, "WaterPlane");
        if (!WaterPlane) return;
        Transform waterSurface = WaterPlane.GetChild(0);
        
        WaterLoD = waterSurface;
        OriginalWaterLevel = waterSurface.position;
        if (!waterSurface.TryGetComponent(out MeshRenderer meshRenderer)) return;
        WaterLoDRenderer = meshRenderer;
        OriginalWaterLoDMaterial = meshRenderer.material;
    }
    private static void CacheZoneWater(ZoneSystem instance)
    {
        if (!instance) return;
        Transform Water = global::Utils.FindChild(instance.m_zonePrefab.transform, "Water");
        if (!Water) return;
        Transform WaterSurface = global::Utils.FindChild(Water.transform, "WaterSurface");
        if (!WaterSurface) return;
        if (WaterSurface.TryGetComponent(out MeshFilter meshFilter))
        {
            MeshCollider collider = WaterSurface.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = meshFilter.sharedMesh;
            collider.enabled = false;
            ZoneWaterCollider = collider;
        }

        if (!WaterSurface.TryGetComponent(out MeshRenderer meshRenderer)) return;
        OriginalWaterMaterial = meshRenderer.material;
        WaterSurfaceRenderer = meshRenderer;
        Transform WaterVolume = global::Utils.FindChild(Water.transform, "WaterVolume");
        if (!WaterVolume) return;
        ZoneWaterVolume = WaterVolume;
        
        // Make sure water is frozen if user logs near water zone during winter
        if (_WinterFreezesWater.Value is Toggle.On) ModifyWater();
    }
    private static void ReplaceWaterLoD()
    {
        if (_WinterFreezesWater.Value is Toggle.Off) return;
        WaterLoDRenderer.material = _Season.Value is Season.Winter ? SnowMaterial : OriginalWaterLoDMaterial;
        WaterLoD.position = _Season.Value is Season.Winter
            ? OriginalWaterLevel + new Vector3(0f, -0.2f, 0f)
            : OriginalWaterLevel;
    }
    
    private static void ReplaceZoneWater()
    {
        if (_WinterFreezesWater.Value is Toggle.Off) return;
        WaterSurfaceRenderer.material = _Season.Value is Season.Winter ? SnowMaterial : OriginalWaterMaterial;
        ZoneWaterCollider.enabled = _Season.Value is Season.Winter;
        
        if (!MaterialReplacer.CachedMaterials.TryGetValue("water", out Material water)) return;
        water.SetFloat(WaveVel, _Season.Value is Season.Winter ? 0.0f : 1.0f);
        if (ZoneWaterVolume.TryGetComponent(out WaterVolume waterVolume))
        {
            waterVolume.m_useGlobalWind = _Season.Value is not Season.Winter;
        }
    }

    public static void ModifyWater()
    {
        ReplaceZoneWater();
        ReplaceWaterLoD();
    }

    [HarmonyPatch(typeof(AudioMan), nameof(AudioMan.FixedUpdate))]
    static class AudioManPatch
    {
        private static void Postfix(AudioMan __instance)
        {
            if (!__instance) return;
            __instance.m_haveOcean = _Season.Value is not Season.Winter;
        }
    }
    
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    static class GameStartPatch
    {
        private static void Postfix(Game __instance) => CacheWaterLoD(__instance);
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    static class SpawnZonePatch
    {
        private static void Postfix(ZoneSystem __instance) => CacheZoneWater(__instance);
    }
    
    private static AssetBundle GetAssetBundle(string fileName)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
        using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
        return AssetBundle.LoadFromStream(stream);
    }
    
}