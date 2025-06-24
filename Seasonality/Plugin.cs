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
        internal const string ModVersion = "3.5.8";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private const string ConfigFileName = ModGUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        private static readonly ManualLogSource SeasonalityLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public static SeasonalityPlugin _plugin = null!;
        private static readonly AssetBundle Assets = GetAssetBundle("snowmaterialbundle");
        // private static readonly AssetBundle NewIce = GetAssetBundle("seasonality");
        public static Material FrozenWaterMat = null!;
        // public static Material DistanceFrozenWaterMat = null!;
        public static Configs ConfigManager = null!;
        public static bool BadgerHDLoaded;
        public static readonly Records Record = new (SeasonalityLogger, ModName, $"{ModName}-LogOutput.log");
        public static GameObject Root = null!;
        public void Awake()
        {
            ConfigManager = new Configs(Config, ConfigSync, ConfigFileName, ConfigFileFullPath);
            _plugin = this;
            FrozenWaterMat = Assets.LoadAsset<Material>("BallSnow04");
            // DistanceFrozenWaterMat = NewIce.LoadAsset<Material>("darkness");
            // FrozenWaterMat = NewIce.LoadAsset<Material>("FrozenWater_mat2");
            // SnowMaterial.Setup();

            Root = new GameObject("root");
            DontDestroyOnLoad(Root);
            Root.SetActive(false);
            Localizer.Load();
            AssetLoader.Read();
            TextureManager.Read();
            TweaksManager.Setup();
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
            FrozenZones.UpdateAll();
            FrozenWaterLOD.UpdateAll();
            MossController.UpdateAll();
            GlobalKeyManager.UpdateSeasonalKey();
            VisEquipController.UpdateAll();
            SeasonalIce.UpdateAll();
            SeasonalIce.UpdateIce();

            SeasonSE.UpdateStatus();
        }

        public void Update()
        {
            float dt = Time.deltaTime;
            SeasonTimer.CheckTransition(dt);
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
    }
}