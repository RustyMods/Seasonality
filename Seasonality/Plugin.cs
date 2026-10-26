using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using LocalizationManager;
using UnityEngine;

namespace Seasonality;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class SeasonalityPlugin : BaseUnityPlugin
{
    internal const string ModName = "Seasonality";
    internal const string ModVersion = "3.8.6";
    internal const string Author = "RustyMods";
    public const string ModGUID = Author + "." + ModName;
    internal static string ConnectionError = "";

    private readonly Harmony _harmony = new(ModGUID);
    public static SeasonalityPlugin instance = null!;
    public static Harmony harmony => instance._harmony;

    public static readonly Material FrozenWaterMat1 =
        AssetBundleManager.LoadAsset<Material>("seasonality", "FrozenWater_mat3")!;

    public static bool BadgerHDLoaded;
    public static Records Record = null!;

    
    public void Awake()
    {
        instance = this;
        Record = new Records(Logger, ModName, $"{ModName}-LogOutput.log");
        ConfigManager.Start();
        Localizer.Load();
        AssetLoader.Read();
        TextureManager.Read();
        TweaksManager.Setup();
        BlackList.Read();
        SeasonalIce.Setup();
        PrefabManager.Start();
        
        Assembly assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly);
        BadgerHDLoaded = Chainloader.PluginInfos.ContainsKey("Badgers.HDValheimTextures");
        if (BadgerHDLoaded) Record.LogInfo("HD Valheim Textures loaded");
    }

    public void OnDisable()
    {
        Record.Write();
    }

    public static void OnSeasonChange()
    {
        TextureReplacer.UpdateAll();
        RandomColors.UpdateAll();
        LocationFix.UpdateAll();
        TerrainController.UpdateTerrain();
        MossController.UpdateAll();
        GlobalKeyManager.UpdateSeasonalKey();
        VisEquipController.UpdateAll();
        
        FrozenZones.UpdateAll();
        FrozenWaterLOD.UpdateAll();
        SeasonalIce.UpdateAll();
        SeasonalIceMineRock.UpdateAll();
        SeasonalIce.UpdateZoneVeg();

        if (EnvMan.instance && Configs.m_weatherEnabled.Value is Toggle.On)
        {
            EnvMan.instance.m_environmentPeriod = -1L;
        }
    }

    private void OnDestroy()
    {
        Config.Save();
    }

    public static void LogInfo(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Info)) return;
        instance.Logger.LogInfo(msg);
    }

    public static void LogError(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Error)) return;
        instance.Logger.LogError(msg);
    }

    public static void LogWarning(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Warning)) return;
        instance.Logger.LogWarning(msg);
    }

    public static void LogDebug(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Debug)) return;
        instance.Logger.LogDebug(msg);
    }

    public static void LogFatal(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Fatal)) return;
        instance.Logger.LogFatal(msg);
    }
}