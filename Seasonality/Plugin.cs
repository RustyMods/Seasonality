using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Seasonality.Behaviors;
using Seasonality.GameplayModifiers;
using Seasonality.Helpers;
using Seasonality.Managers;
using Seasonality.Textures;
using ServerSync;
using UnityEngine;

namespace Seasonality
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class SeasonalityPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Seasonality";
        internal const string ModVersion = "3.7.6";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private const string ConfigFileName = ModGUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        private static readonly ManualLogSource SeasonalityLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public static SeasonalityPlugin _plugin = null!;
        public static readonly AssetBundle NewIce = GetAssetBundle("seasonality");
        
        public static Configs ConfigManager = null!;
        public static bool BadgerHDLoaded;
        public static readonly Records Record = new (SeasonalityLogger, ModName, $"{ModName}-LogOutput.log");
        public static GameObject Root = null!;
        
        public void Awake()
        {
            ConfigManager = new Configs(Config, ConfigSync, ConfigFileName, ConfigFileFullPath);
            _plugin = this;
            Root = new GameObject("root");
            DontDestroyOnLoad(Root);
            Root.SetActive(false);
            Localizer.Load();
            AssetLoader.Read();
            TextureManager.Read();
            TweaksManager.Setup();
            FrozenManager.Setup();
            BlackList.Read();
            gameObject.AddComponent<SeasonalTimer>();
            
            Assembly assembly = Assembly.GetExecutingAssembly(); 
            _harmony.PatchAll(assembly);
            BadgerHDLoaded = Chainloader.PluginInfos.ContainsKey("Badgers.HDValheimTextures");
            if (BadgerHDLoaded) Record.LogInfo("HD Valheim Textures loaded");
        }

        public void OnDisable()
        {
            Record.Write();
        }

        public static void RPC_OnShelfRollChange(long sender, bool freeze)
        {
            SeasonalityLogger.LogDebug($"Rolled shelf to ice ratio, water freezes: {freeze}");

            if (freeze)
            {
                FrozenZones.UpdateAll();
                FrozenWaterLOD.UpdateAll();
            }
            else
            {
                SeasonalIce.UpdateAll();
                SeasonalIce.UpdateZoneVeg();
            }
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

            if (ZNet.instance && Configs.m_season.Value is Season.Winter && Configs.m_waterFreezes.Value is Toggle.On && Configs.m_addIceShelves.Value is Toggle.On)
            {
                RollIceToFreeze();
            }
            else
            {
                FrozenZones.UpdateAll();
                FrozenWaterLOD.UpdateAll();
                SeasonalIce.UpdateAll();
                SeasonalIce.UpdateZoneVeg();
            }
            
            SeasonSE.UpdateStatus();
        }
        
        

        public void Update()
        {
            float dt = Time.deltaTime;
            // SeasonTimer.CheckTransition(dt);
            SeasonSE.CheckOrSet(dt);
            WeatherManager.CheckOrSet(dt);
        }

        private void OnDestroy() => Config.Save();

        private static AssetBundle GetAssetBundle(string fileName)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
            return AssetBundle.LoadFromStream(stream);
        }

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        private static class ZnetScene_Awake_Patch
        {
            private static void Postfix()
            {
                ZRoutedRpc.instance.Register<bool>(nameof(RPC_OnShelfRollChange),RPC_OnShelfRollChange);
                
                RollIceToFreeze();
            }
        }

        public static void RollIceToFreeze()
        {
            if (ZNet.instance && Configs.m_season.Value is Season.Winter && Configs.m_waterFreezes.Value is Toggle.On && Configs.m_addIceShelves.Value is Toggle.On)
            {
                if (ZNet.instance.IsServer())
                {
                    float roll = Random.value;
                    bool waterFreezes = roll < Configs.m_frozenWaterToIceShelfRatio.Value;
                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, nameof(RPC_OnShelfRollChange), waterFreezes);
                }
                else if (SeasonalTimer.hasValidServer)
                {
                    // let server roll
                }
                else
                {
                    float roll = Random.value;
                    bool waterFreezes = roll < Configs.m_frozenWaterToIceShelfRatio.Value;
                    RPC_OnShelfRollChange(0L, waterFreezes);
                }
            }
        }
    }
}