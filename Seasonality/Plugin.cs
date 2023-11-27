using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Seasonality.Seasons;
using ServerSync;
using UnityEngine;
using UnityEngine.Rendering;
using static Seasonality.Seasons.Environment;
using Environment = Seasonality.Seasons.Environment;


namespace Seasonality
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("randyknapp.mods.auga",BepInDependency.DependencyFlags.SoftDependency)]
    public class SeasonalityPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Seasonality";
        internal const string ModVersion = "1.1.1";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource SeasonalityLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public enum WorkingAs
        {
            Client,
            Server,
            Both
        }

        public static WorkingAs workingAsType;
        public static bool AugaLoaded = false;
        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "1 - Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.auga")) AugaLoaded = true;
            
            workingAsType = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null
                ? WorkingAs.Server
                : WorkingAs.Client;

            CustomTextures.ReadCustomTextures();
            
            InitConfigs();
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }
        
        public enum Season
        {
            Spring = 0,
            Summer = 1,
            Fall = 2,
            Winter = 3
        }

        public enum Element
        {
            Fire,
            Frost,
            Lightning,
            Poison,
            Spirit,
            None
        }

        public enum DamageModifier
        {
             Normal,
             Resistant,
             Weak,
             Immune,
             Ignore,
             VeryResistant,
             VeryWeak,
             None
        }
        #region CustomConfigs
        public static ConfigEntry<Season> _Season = null!;
        public static ConfigEntry<int> _SeasonDuration = null!;
        public static ConfigEntry<int> _WeatherDuration = null!;

        public static ConfigEntry<Toggle> _SeasonalEffectsEnabled = null!;
        
        public static ConfigEntry<string> _SpringName = null!;
        public static ConfigEntry<string> _SpringStartMsg = null!;
        public static ConfigEntry<string> _SpringTooltip = null!;
        public static ConfigEntry<Modifier> _SpringModifier = null!;
        public static ConfigEntry<Element> _SpringResistanceElement = null!;
        public static ConfigEntry<DamageModifier> _SpringResistanceMod = null!;
        public static ConfigEntry<float> _SpringValue = null!;
        
        public static ConfigEntry<string> _FallName = null!;
        public static ConfigEntry<string> _FallStartMsg = null!;
        public static ConfigEntry<string> _FallTooltip = null!;
        public static ConfigEntry<Modifier> _FallModifier = null!;
        public static ConfigEntry<Element> _FallResistanceElement = null!;
        public static ConfigEntry<DamageModifier> _FallResistanceMod = null!;
        public static ConfigEntry<float> _FallValue = null!;
        
        public static ConfigEntry<string> _WinterName = null!;
        public static ConfigEntry<string> _WinterStartMsg = null!;
        public static ConfigEntry<string> _WinterTooltip = null!;
        public static ConfigEntry<Modifier> _WinterModifier = null!;
        public static ConfigEntry<Element> _WinterResistanceElement = null!;
        public static ConfigEntry<DamageModifier> _WinterResistantMod = null!;
        public static ConfigEntry<float> _WinterValue = null!;
        
        public static ConfigEntry<string> _SummerName = null!;
        public static ConfigEntry<string> _SummerStartMsg = null!;
        public static ConfigEntry<string> _SummerTooltip = null!;
        public static ConfigEntry<Modifier> _SummerModifier = null!;
        public static ConfigEntry<Element> _SummerResistanceElement = null!;
        public static ConfigEntry<DamageModifier> _SummerResistanceMod = null!;
        public static ConfigEntry<float> _SummerValue = null!;

        public static ConfigEntry<Environments> _Fall_Meadows_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_Meadows_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_Meadows_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_Meadows_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Fall_BlackForest_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_BlackForest_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_BlackForest_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_BlackForest_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Fall_Swamp_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_Swamp_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_Swamp_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_Swamp_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Fall_Mountains_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_Mountains_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_Mountains_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_Mountains_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Fall_Plains_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_Plains_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_Plains_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_Plains_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Fall_MistLands_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_MistLands_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_MistLands_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_MistLands_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Fall_Ocean_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_Ocean_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_Ocean_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_Ocean_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Fall_AshLands_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_AshLands_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_AshLands_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_AshLands_Weather4 = null!;

        public static ConfigEntry<Environments> _Fall_DeepNorth_Weather1 = null!;
        public static ConfigEntry<Environments> _Fall_DeepNorth_Weather2 = null!;
        public static ConfigEntry<Environments> _Fall_DeepNorth_Weather3 = null!;
        public static ConfigEntry<Environments> _Fall_DeepNorth_Weather4 = null!;

        
        
        public static ConfigEntry<Environments> _Winter_Meadows_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_Meadows_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_Meadows_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_Meadows_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Winter_BlackForest_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_BlackForest_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_BlackForest_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_BlackForest_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Winter_Swamp_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_Swamp_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_Swamp_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_Swamp_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Winter_Mountains_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_Mountains_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_Mountains_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_Mountains_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Winter_Plains_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_Plains_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_Plains_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_Plains_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Winter_MistLands_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_MistLands_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_MistLands_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_MistLands_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Winter_Ocean_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_Ocean_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_Ocean_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_Ocean_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Winter_AshLands_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_AshLands_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_AshLands_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_AshLands_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Winter_DeepNorth_Weather1 = null!;
        public static ConfigEntry<Environments> _Winter_DeepNorth_Weather2 = null!;
        public static ConfigEntry<Environments> _Winter_DeepNorth_Weather3 = null!;
        public static ConfigEntry<Environments> _Winter_DeepNorth_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_Meadows_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_Meadows_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_Meadows_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_Meadows_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_BlackForest_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_BlackForest_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_BlackForest_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_BlackForest_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_Swamp_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_Swamp_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_Swamp_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_Swamp_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_Mountains_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_Mountains_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_Mountains_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_Mountains_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_Plains_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_Plains_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_Plains_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_Plains_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_MistLands_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_MistLands_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_MistLands_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_MistLands_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_Ocean_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_Ocean_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_Ocean_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_Ocean_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_AshLands_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_AshLands_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_AshLands_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_AshLands_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Spring_DeepNorth_Weather1 = null!;
        public static ConfigEntry<Environments> _Spring_DeepNorth_Weather2 = null!;
        public static ConfigEntry<Environments> _Spring_DeepNorth_Weather3 = null!;
        public static ConfigEntry<Environments> _Spring_DeepNorth_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_Meadows_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_Meadows_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_Meadows_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_Meadows_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_BlackForest_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_BlackForest_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_BlackForest_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_BlackForest_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_Swamp_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_Swamp_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_Swamp_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_Swamp_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_Mountains_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_Mountains_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_Mountains_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_Mountains_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_Plains_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_Plains_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_Plains_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_Plains_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_MistLands_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_MistLands_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_MistLands_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_MistLands_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_Ocean_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_Ocean_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_Ocean_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_Ocean_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_AshLands_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_AshLands_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_AshLands_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_AshLands_Weather4 = null!;
        
        public static ConfigEntry<Environments> _Summer_DeepNorth_Weather1 = null!;
        public static ConfigEntry<Environments> _Summer_DeepNorth_Weather2 = null!;
        public static ConfigEntry<Environments> _Summer_DeepNorth_Weather3 = null!;
        public static ConfigEntry<Environments> _Summer_DeepNorth_Weather4 = null!;
        

        public static ConfigEntry<Color> _FallColor1 = null!;
        public static ConfigEntry<Color> _FallColor2 = null!;
        public static ConfigEntry<Color> _FallColor3 = null!;
        public static ConfigEntry<Color> _FallColor4 = null!;
        
        public static ConfigEntry<Color> _SpringColor1 = null!;
        public static ConfigEntry<Color> _SpringColor2 = null!;
        public static ConfigEntry<Color> _SpringColor3 = null!;
        public static ConfigEntry<Color> _SpringColor4 = null!;
        
        public static ConfigEntry<Color> _WinterColor1 = null!;
        public static ConfigEntry<Color> _WinterColor2 = null!;
        public static ConfigEntry<Color> _WinterColor3 = null!;
        public static ConfigEntry<Color> _WinterColor4 = null!;
        
        public static ConfigEntry<Color> _SummerColor1 = null!;
        public static ConfigEntry<Color> _SummerColor2 = null!;
        public static ConfigEntry<Color> _SummerColor3 = null!;
        public static ConfigEntry<Color> _SummerColor4 = null!;

        public static ConfigEntry<SpecialEffects.SpecialEffect> _FallStartEffects = null!;
        public static ConfigEntry<SpecialEffects.SpecialEffect> _FallStopEffects = null!;
        public static ConfigEntry<SpecialEffects.SpecialEffect> _SpringStartEffects = null!;
        public static ConfigEntry<SpecialEffects.SpecialEffect> _SpringStopEffects = null!;
        public static ConfigEntry<SpecialEffects.SpecialEffect> _SummerStartEffects = null!;
        public static ConfigEntry<SpecialEffects.SpecialEffect> _SummerStopEffects = null!;
        public static ConfigEntry<SpecialEffects.SpecialEffect> _WinterStartEffects = null!;
        public static ConfigEntry<SpecialEffects.SpecialEffect> _WinterStopEffects = null!;

        public static ConfigEntry<Toggle> _SeasonControl = null!;
        public static ConfigEntry<Toggle> _ModEnabled = null!;
        public static ConfigEntry<Toggle> _CounterVisible = null!;
        public static ConfigEntry<Toggle> _WeatherControl = null!;
        public static ConfigEntry<Toggle> _StatusEffectVisible = null!;

        public static ConfigEntry<Toggle> _ReplaceLeech = null!;

        #endregion
        private void InitConfigs()
        {
            _SeasonControl = config("1 - General", "3 - Control", Toggle.Off, "If on, season duration is disabled, and user can change season at will");
            _ModEnabled = config("1 - General", "2 - Plugin Enabled", Toggle.On, "If on, mod is enabled");
            
            _Season = config("2 - Utilities", "1 - Current Season", Season.Fall, "Set duration to 0, and select your season, else season is determined by plugin");
            _SeasonDuration = config("2 - Utilities", "2 - Season Duration (Days)", 5, new ConfigDescription("In-game days between season", new AcceptableValueRange<int>(0, 365)));
            _CounterVisible = config("2 - Utilities", "3 - Timer Visible", Toggle.On, "If on, timer under season is visible", false);
            _WeatherDuration = config("2 - Utilities", "4 - Weather Duration (Minutes)", 20, new ConfigDescription("In-game minutes between weather change, if season applies weather", new AcceptableValueRange<int>(0, 200)));
            _WeatherControl = config("2 - Utilities", "Weather Enabled", Toggle.On, "If on, seasons can control the weather");
            _SeasonalEffectsEnabled = config("2 - Utilities", "5 - Player Modifiers Enabled", Toggle.Off, "If on, season effects are enabled");
            _StatusEffectVisible = config("2 - Utilities", "7 - Season Icon Visible", Toggle.On, "If on, season icon is visible", false);
            
            #region Creatures

            _ReplaceLeech = config("7 - Creature Replacement", "Leeches", Toggle.On,
                "If on, winter replaces leeches for leech_cave (white leech)");
            #endregion
            #region SpringConfigs
            _SpringName = config("3 - Spring", "Name", "Spring", "Display name");
            _SpringStartMsg = config("3 - Spring", "Start Message", "Spring has finally arrived", "Start of the season message");
            _SpringTooltip = config("3 - Spring", "Tooltip", "The land is bursting with energy", "Status effect tooltip");
            _SpringModifier = config("3 - Spring", "Modifier", Modifier.None, "Stats modifier");
            _SpringResistanceElement = config("3 - Spring", "Resistance Type", Element.None, "Element");
            _SpringResistanceMod = config("3 - Spring", "Resistance Modifier", DamageModifier.None, "Modifier");
            _SpringValue = config("3 - Spring", "Modifying value", 0.0f, new ConfigDescription("Value applied to modifier", new AcceptableValueRange<float>(-100f, 100f)));
            
            _Spring_Meadows_Weather1 = config("3 - Spring Weather", "1 - Meadows 1", Environments.None, "Environments set by spring season");
            _Spring_Meadows_Weather2 = config("3 - Spring Weather", "1 - Meadows 2", Environments.None, "Environments set by spring season");
            _Spring_Meadows_Weather3 = config("3 - Spring Weather", "1 - Meadows 3", Environments.None, "Environments set by spring season");
            _Spring_Meadows_Weather4 = config("3 - Spring Weather", "1 - Meadows 4", Environments.None, "Environments set by spring season");
            
            _Spring_BlackForest_Weather1 = config("3 - Spring Weather", "2 - BlackForest 1", Environments.None, "Environments set by spring season");
            _Spring_BlackForest_Weather2 = config("3 - Spring Weather", "2 - BlackForest 2", Environments.None, "Environments set by spring season");
            _Spring_BlackForest_Weather3 = config("3 - Spring Weather", "2 - BlackForest 3", Environments.None, "Environments set by spring season");
            _Spring_BlackForest_Weather4 = config("3 - Spring Weather", "2 - BlackForest 4", Environments.None, "Environments set by spring season");

            _Spring_Swamp_Weather1 = config("3 - Spring Weather", "3 - Swamp 1", Environments.None, "Environments set by spring season");
            _Spring_Swamp_Weather2 = config("3 - Spring Weather", "3 - Swamp 2", Environments.None, "Environments set by spring season");
            _Spring_Swamp_Weather3 = config("3 - Spring Weather", "3 - Swamp 3", Environments.None, "Environments set by spring season");
            _Spring_Swamp_Weather4 = config("3 - Spring Weather", "3 - Swamp 4", Environments.None, "Environments set by spring season");

            _Spring_Mountains_Weather1 = config("3 - Spring Weather", "4 - Mountains 1", Environments.None, "Environments set by spring season");
            _Spring_Mountains_Weather2 = config("3 - Spring Weather", "4 - Mountains 2", Environments.None, "Environments set by spring season");
            _Spring_Mountains_Weather3 = config("3 - Spring Weather", "4 - Mountains 3", Environments.None, "Environments set by spring season");
            _Spring_Mountains_Weather4 = config("3 - Spring Weather", "4 - Mountains 4", Environments.None, "Environments set by spring season");

            _Spring_Plains_Weather1 = config("3 - Spring Weather", "5 - Plains 1", Environments.None, "Environments set by spring season");
            _Spring_Plains_Weather2 = config("3 - Spring Weather", "5 - Plains 2", Environments.None, "Environments set by spring season");
            _Spring_Plains_Weather3 = config("3 - Spring Weather", "5 - Plains 3", Environments.None, "Environments set by spring season");
            _Spring_Plains_Weather4 = config("3 - Spring Weather", "5 - Plains 4", Environments.None, "Environments set by spring season");

            _Spring_MistLands_Weather1 = config("3 - Spring Weather", "6 - MistLands 1", Environments.None, "Environments set by spring season");
            _Spring_MistLands_Weather2 = config("3 - Spring Weather", "6 - MistLands 2", Environments.None, "Environments set by spring season");
            _Spring_MistLands_Weather3 = config("3 - Spring Weather", "6 - MistLands 3", Environments.None, "Environments set by spring season");
            _Spring_MistLands_Weather4 = config("3 - Spring Weather", "6 - MistLands 4", Environments.None, "Environments set by spring season");

            _Spring_Ocean_Weather1 = config("3 - Spring Weather", "6 - Ocean 1", Environments.None, "Environments set by spring season");
            _Spring_Ocean_Weather2 = config("3 - Spring Weather", "6 - Ocean 2", Environments.None, "Environments set by spring season");
            _Spring_Ocean_Weather3 = config("3 - Spring Weather", "6 - Ocean 3", Environments.None, "Environments set by spring season");
            _Spring_Ocean_Weather4 = config("3 - Spring Weather", "6 - Ocean 4", Environments.None, "Environments set by spring season");

            _Spring_AshLands_Weather1 = config("3 - Spring Weather", "7 - AshLands 1", Environments.None, "Environments set by spring season");
            _Spring_AshLands_Weather2 = config("3 - Spring Weather", "7 - AshLands 2", Environments.None, "Environments set by spring season");
            _Spring_AshLands_Weather3 = config("3 - Spring Weather", "7 - AshLands 3", Environments.None, "Environments set by spring season");
            _Spring_AshLands_Weather4 = config("3 - Spring Weather", "7 - AshLands 4", Environments.None, "Environments set by spring season");

            _Spring_DeepNorth_Weather1 = config("3 - Spring Weather", "8 - DeepNorth 1", Environments.None, "Environments set by spring season");
            _Spring_DeepNorth_Weather2 = config("3 - Spring Weather", "8 - DeepNorth 2", Environments.None, "Environments set by spring season");
            _Spring_DeepNorth_Weather3 = config("3 - Spring Weather", "8 - DeepNorth 3", Environments.None, "Environments set by spring season");
            _Spring_DeepNorth_Weather4 = config("3 - Spring Weather", "8 - DeepNorth 4", Environments.None, "Environments set by spring season");
            
            _SpringColor1 = config("3 - Spring", "Color 1", SeasonColors.SpringColors[0], "Color tint applied to prefabs");
            _SpringColor2 = config("3 - Spring", "Color 2", SeasonColors.SpringColors[1], "Color tint applied to prefabs");
            _SpringColor3 = config("3 - Spring", "Color 3", SeasonColors.SpringColors[2], "Color tint applied to prefabs");
            _SpringColor4 = config("3 - Spring", "Color 4", SeasonColors.SpringColors[3], "Color tint applied to prefabs");

            _SpringStartEffects = config("3 - Spring", "Start Effect", SpecialEffects.SpecialEffect.None, "Visual effect applied when season starts");
            _SpringStopEffects = config("3 - Spring", "Stop Effect", SpecialEffects.SpecialEffect.None, "Visual effect applied when season ends");
            
            #endregion
            #region FallConfigs
            _FallName = config("4 - Fall", "Name", "Fall", "Display name");
            _FallStartMsg = config("4 - Fall", "Start Message", "Fall is upon us", "Start of the season message");
            _FallTooltip = config("4 - Fall", "Tooltip", "The ground is wet", "Status effect tooltip");
            _FallModifier = config("4 - Fall", "Modifier", Modifier.None, "Stats modifier");
            _FallResistanceElement = config("4 - Fall", "Resistance Type", Element.None, "Element");
            _FallResistanceMod = config("4 - Fall", "Resistance Modifier", DamageModifier.None, "Modifier");
            _FallValue = config("4 - Fall", "Modifying value", 0.0f, new ConfigDescription("Value applied to modifier", new AcceptableValueRange<float>(-100f, 100f)));

            _Fall_Meadows_Weather1 = config("4 - Fall Weather", "1 - Meadows 1", Environments.None, "Environments set by fall season");
            _Fall_Meadows_Weather2 = config("4 - Fall Weather", "1 - Meadows 2", Environments.None, "Environments set by fall season");
            _Fall_Meadows_Weather3 = config("4 - Fall Weather", "1 - Meadows 3", Environments.None, "Environments set by fall season");
            _Fall_Meadows_Weather4 = config("4 - Fall Weather", "1 - Meadows 4", Environments.None, "Environments set by fall season");

            _Fall_BlackForest_Weather1 = config("4 - Fall Weather", "2 - BlackForest 1", Environments.None, "Environments set by Fall season");
            _Fall_BlackForest_Weather2 = config("4 - Fall Weather", "2 - BlackForest 2", Environments.None, "Environments set by Fall season");
            _Fall_BlackForest_Weather3 = config("4 - Fall Weather", "2 - BlackForest 3", Environments.None, "Environments set by Fall season");
            _Fall_BlackForest_Weather4 = config("4 - Fall Weather", "2 - BlackForest 4", Environments.None, "Environments set by Fall season");

            _Fall_Swamp_Weather1 = config("4 - Fall Weather", "3 - Swamp 1", Environments.None, "Environments set by Fall season");
            _Fall_Swamp_Weather2 = config("4 - Fall Weather", "3 - Swamp 2", Environments.None, "Environments set by Fall season");
            _Fall_Swamp_Weather3 = config("4 - Fall Weather", "3 - Swamp 3", Environments.None, "Environments set by Fall season");
            _Fall_Swamp_Weather4 = config("4 - Fall Weather", "3 - Swamp 4", Environments.None, "Environments set by Fall season");

            _Fall_Mountains_Weather1 = config("4 - Fall Weather", "4 - Mountains 1", Environments.None, "Environments set by Fall season");
            _Fall_Mountains_Weather2 = config("4 - Fall Weather", "4 - Mountains 2", Environments.None, "Environments set by Fall season");
            _Fall_Mountains_Weather3 = config("4 - Fall Weather", "4 - Mountains 3", Environments.None, "Environments set by Fall season");
            _Fall_Mountains_Weather4 = config("4 - Fall Weather", "4 - Mountains 4", Environments.None, "Environments set by Fall season");

            _Fall_Plains_Weather1 = config("4 - Fall Weather", "5 - Plains 1", Environments.None, "Environments set by Fall season");
            _Fall_Plains_Weather2 = config("4 - Fall Weather", "5 - Plains 2", Environments.None, "Environments set by Fall season");
            _Fall_Plains_Weather3 = config("4 - Fall Weather", "5 - Plains 3", Environments.None, "Environments set by Fall season");
            _Fall_Plains_Weather4 = config("4 - Fall Weather", "5 - Plains 4", Environments.None, "Environments set by Fall season");

            _Fall_MistLands_Weather1 = config("4 - Fall Weather", "6 - MistLands 1", Environments.None, "Environments set by Fall season");
            _Fall_MistLands_Weather2 = config("4 - Fall Weather", "6 - MistLands 2", Environments.None, "Environments set by Fall season");
            _Fall_MistLands_Weather3 = config("4 - Fall Weather", "6 - MistLands 3", Environments.None, "Environments set by Fall season");
            _Fall_MistLands_Weather4 = config("4 - Fall Weather", "6 - MistLands 4", Environments.None, "Environments set by Fall season");

            _Fall_Ocean_Weather1 = config("4 - Fall Weather", "6 - Ocean 1", Environments.None, "Environments set by Fall season");
            _Fall_Ocean_Weather2 = config("4 - Fall Weather", "6 - Ocean 2", Environments.None, "Environments set by Fall season");
            _Fall_Ocean_Weather3 = config("4 - Fall Weather", "6 - Ocean 3", Environments.None, "Environments set by Fall season");
            _Fall_Ocean_Weather4 = config("4 - Fall Weather", "6 - Ocean 4", Environments.None, "Environments set by Fall season");

            _Fall_AshLands_Weather1 = config("4 - Fall Weather", "7 - AshLands 1", Environments.None, "Environments set by Fall season");
            _Fall_AshLands_Weather2 = config("4 - Fall Weather", "7 - AshLands 2", Environments.None, "Environments set by Fall season");
            _Fall_AshLands_Weather3 = config("4 - Fall Weather", "7 - AshLands 3", Environments.None, "Environments set by Fall season");
            _Fall_AshLands_Weather4 = config("4 - Fall Weather", "7 - AshLands 4", Environments.None, "Environments set by Fall season");

            _Fall_DeepNorth_Weather1 = config("4 - Fall Weather", "8 - DeepNorth 1", Environments.None, "Environments set by Fall season");
            _Fall_DeepNorth_Weather2 = config("4 - Fall Weather", "8 - DeepNorth 2", Environments.None, "Environments set by Fall season");
            _Fall_DeepNorth_Weather3 = config("4 - Fall Weather", "8 - DeepNorth 3", Environments.None, "Environments set by Fall season");
            _Fall_DeepNorth_Weather4 = config("4 - Fall Weather", "8 - DeepNorth 4", Environments.None, "Environments set by Fall season");

            
            _FallColor1 = config("4 - Fall", "Color 1", SeasonColors.FallColors[0], "Color tint applied to prefabs");
            _FallColor2 = config("4 - Fall", "Color 2", SeasonColors.FallColors[1], "Color tint applied to prefabs");
            _FallColor3 = config("4 - Fall", "Color 3", SeasonColors.FallColors[2], "Color tint applied to prefabs");
            _FallColor4 = config("4 - Fall", "Color 4", SeasonColors.FallColors[3], "Color tint applied to prefabs");

            _FallStartEffects = config("4 - Fall", "Start Effect", SpecialEffects.SpecialEffect.None, "Visual effect applied when season starts");
            _FallStopEffects = config("4 - Fall", "Stop Effect", SpecialEffects.SpecialEffect.None, "Visual effect applied when season ends");
            
            #endregion
            #region WinterConfigs
            _WinterName = config("5 - Winter", "Name", "Winter", "Display name");
            _WinterStartMsg = config("5 - Winter", "Start Message", "Winter is coming!", "Start of the season message");
            _WinterTooltip = config("5 - Winter", "Tooltip", "The air is cold", "Status effect tooltip");
            _WinterModifier = config("5 - Winter", "Modifier", Modifier.StaminaRegen, "Stats modifier");
            _WinterResistanceElement = config("5 - Winter", "Resistance Type", Element.Frost, "Element");
            _WinterResistantMod = config("5 - Winter", "Resistance Modifier", DamageModifier.Resistant, "Modifier");
            _WinterValue = config("5 - Winter", "Modifying value", 0.9f, new ConfigDescription("Value applied to modifier", new AcceptableValueRange<float>(-100f, 100f)));

            _Winter_Meadows_Weather1 = config("5 - Winter Weather", "1 - Meadows 1", Environments.Snow, "Environment set by winter season.");
            _Winter_Meadows_Weather2 = config("5 - Winter Weather", "1 - Meadows 2", Environments.None, "Environment set by winter season.");
            _Winter_Meadows_Weather3 = config("5 - Winter Weather", "1 - Meadows 3", Environments.None, "Environment set by winter season.");
            _Winter_Meadows_Weather4 = config("5 - Winter Weather", "1 - Meadows 4", Environments.None, "Environment set by winter season.");
            
            _Winter_BlackForest_Weather1 = config("5 - Winter Weather", "2 - BlackForest 1", Environments.None, "Environments set by Winter season");
            _Winter_BlackForest_Weather2 = config("5 - Winter Weather", "2 - BlackForest 2", Environments.None, "Environments set by Winter season");
            _Winter_BlackForest_Weather3 = config("5 - Winter Weather", "2 - BlackForest 3", Environments.None, "Environments set by Winter season");
            _Winter_BlackForest_Weather4 = config("5 - Winter Weather", "2 - BlackForest 4", Environments.None, "Environments set by Winter season");

            _Winter_Swamp_Weather1 = config("5 - Winter Weather", "3 - Swamp 1", Environments.None, "Environments set by Winter season");
            _Winter_Swamp_Weather2 = config("5 - Winter Weather", "3 - Swamp 2", Environments.None, "Environments set by Winter season");
            _Winter_Swamp_Weather3 = config("5 - Winter Weather", "3 - Swamp 3", Environments.None, "Environments set by Winter season");
            _Winter_Swamp_Weather4 = config("5 - Winter Weather", "3 - Swamp 4", Environments.None, "Environments set by Winter season");

            _Winter_Mountains_Weather1 = config("5 - Winter Weather", "4 - Mountains 1", Environments.None, "Environments set by Winter season");
            _Winter_Mountains_Weather2 = config("5 - Winter Weather", "4 - Mountains 2", Environments.None, "Environments set by Winter season");
            _Winter_Mountains_Weather3 = config("5 - Winter Weather", "4 - Mountains 3", Environments.None, "Environments set by Winter season");
            _Winter_Mountains_Weather4 = config("5 - Winter Weather", "4 - Mountains 4", Environments.None, "Environments set by Winter season");

            _Winter_Plains_Weather1 = config("5 - Winter Weather", "5 - Plains 1", Environments.None, "Environments set by Winter season");
            _Winter_Plains_Weather2 = config("5 - Winter Weather", "5 - Plains 2", Environments.None, "Environments set by Winter season");
            _Winter_Plains_Weather3 = config("5 - Winter Weather", "5 - Plains 3", Environments.None, "Environments set by Winter season");
            _Winter_Plains_Weather4 = config("5 - Winter Weather", "5 - Plains 4", Environments.None, "Environments set by Winter season");

            _Winter_MistLands_Weather1 = config("5 - Winter Weather", "6 - MistLands 1", Environments.None, "Environments set by Winter season");
            _Winter_MistLands_Weather2 = config("5 - Winter Weather", "6 - MistLands 2", Environments.None, "Environments set by Winter season");
            _Winter_MistLands_Weather3 = config("5 - Winter Weather", "6 - MistLands 3", Environments.None, "Environments set by Winter season");
            _Winter_MistLands_Weather4 = config("5 - Winter Weather", "6 - MistLands 4", Environments.None, "Environments set by Winter season");

            _Winter_Ocean_Weather1 = config("5 - Winter Weather", "6 - Ocean 1", Environments.None, "Environments set by Winter season");
            _Winter_Ocean_Weather2 = config("5 - Winter Weather", "6 - Ocean 2", Environments.None, "Environments set by Winter season");
            _Winter_Ocean_Weather3 = config("5 - Winter Weather", "6 - Ocean 3", Environments.None, "Environments set by Winter season");
            _Winter_Ocean_Weather4 = config("5 - Winter Weather", "6 - Ocean 4", Environments.None, "Environments set by Winter season");

            _Winter_AshLands_Weather1 = config("5 - Winter Weather", "7 - AshLands 1", Environments.None, "Environments set by Winter season");
            _Winter_AshLands_Weather2 = config("5 - Winter Weather", "7 - AshLands 2", Environments.None, "Environments set by Winter season");
            _Winter_AshLands_Weather3 = config("5 - Winter Weather", "7 - AshLands 3", Environments.None, "Environments set by Winter season");
            _Winter_AshLands_Weather4 = config("5 - Winter Weather", "7 - AshLands 4", Environments.None, "Environments set by Winter season");

            _Winter_DeepNorth_Weather1 = config("5 - Winter Weather", "8 - DeepNorth 1", Environments.None, "Environments set by Winter season");
            _Winter_DeepNorth_Weather2 = config("5 - Winter Weather", "8 - DeepNorth 2", Environments.None, "Environments set by Winter season");
            _Winter_DeepNorth_Weather3 = config("5 - Winter Weather", "8 - DeepNorth 3", Environments.None, "Environments set by Winter season");
            _Winter_DeepNorth_Weather4 = config("5 - Winter Weather", "8 - DeepNorth 4", Environments.None, "Environments set by Winter season");

            
            _WinterColor1 = config("5 - Winter", "Color 1", SeasonColors.WinterColors[0], "Color tint applied to prefabs");
            _WinterColor2 = config("5 - Winter", "Color 2", SeasonColors.WinterColors[0], "Color tint applied to prefabs");
            _WinterColor3 = config("5 - Winter", "Color 3", SeasonColors.WinterColors[0], "Color tint applied to prefabs");
            _WinterColor4 = config("5 - Winter", "Color 4", SeasonColors.WinterColors[0], "Color tint applied to prefabs");

            _WinterStartEffects = config("5 - Winter", "Start Effect", SpecialEffects.SpecialEffect.None, "Visual effect applied when season starts");
            _WinterStopEffects = config("5 - Winter", "Stop Effect", SpecialEffects.SpecialEffect.None, "Visual effect applied when season ends");
            #endregion
            #region SummerConfigs
            _SummerName = config("6 - Summer", "Name", "Summer", "Display name");
            _SummerStartMsg = config("6 - Summer", "Start Message", "Summer has landed", "Start of the season message");
            _SummerTooltip = config("6 - Summer", "Tooltip", "The air is warm", "Status effect tooltip");
            _SummerModifier = config("6 - Summer", "Modifier", Modifier.MaxCarryWeight, "Stats modifier");
            _SummerResistanceElement = config("6 - Summer", "Resistance Type", Element.None, "Element");
            _SummerResistanceMod = config("6 - Summer", "Resistance Modifier", DamageModifier.None, "Modifier");
            _SummerValue = config("6 - Summer", "Modifying value", -50f, new ConfigDescription("Value applied to modifier", new AcceptableValueRange<float>(-100f, 100f)));
            
            _Summer_Meadows_Weather1 = config("4 - Summer Weather", "1 - Meadows 1", Environments.None, "Environment set by summer season.");
            _Summer_Meadows_Weather2 = config("4 - Summer Weather", "1 - Meadows 2", Environments.None, "Environment set by summer season.");
            _Summer_Meadows_Weather3 = config("4 - Summer Weather", "1 - Meadows 3", Environments.None, "Environment set by summer season.");
            _Summer_Meadows_Weather4 = config("4 - Summer Weather", "1 - Meadows 4", Environments.None, "Environment set by summer season.");

            _Summer_BlackForest_Weather1 = config("4 - Summer Weather", "2 - BlackForest 1", Environments.None, "Environments set by Summer season");
            _Summer_BlackForest_Weather2 = config("4 - Summer Weather", "2 - BlackForest 2", Environments.None, "Environments set by Summer season");
            _Summer_BlackForest_Weather3 = config("4 - Summer Weather", "2 - BlackForest 3", Environments.None, "Environments set by Summer season");
            _Summer_BlackForest_Weather4 = config("4 - Summer Weather", "2 - BlackForest 4", Environments.None, "Environments set by Summer season");

            _Summer_Swamp_Weather1 = config("4 - Summer Weather", "3 - Swamp 1", Environments.None, "Environments set by Summer season");
            _Summer_Swamp_Weather2 = config("4 - Summer Weather", "3 - Swamp 2", Environments.None, "Environments set by Summer season");
            _Summer_Swamp_Weather3 = config("4 - Summer Weather", "3 - Swamp 3", Environments.None, "Environments set by Summer season");
            _Summer_Swamp_Weather4 = config("4 - Summer Weather", "3 - Swamp 4", Environments.None, "Environments set by Summer season");

            _Summer_Mountains_Weather1 = config("4 - Summer Weather", "4 - Mountains 1", Environments.None, "Environments set by Summer season");
            _Summer_Mountains_Weather2 = config("4 - Summer Weather", "4 - Mountains 2", Environments.None, "Environments set by Summer season");
            _Summer_Mountains_Weather3 = config("4 - Summer Weather", "4 - Mountains 3", Environments.None, "Environments set by Summer season");
            _Summer_Mountains_Weather4 = config("4 - Summer Weather", "4 - Mountains 4", Environments.None, "Environments set by Summer season");

            _Summer_Plains_Weather1 = config("4 - Summer Weather", "5 - Plains 1", Environments.None, "Environments set by Summer season");
            _Summer_Plains_Weather2 = config("4 - Summer Weather", "5 - Plains 2", Environments.None, "Environments set by Summer season");
            _Summer_Plains_Weather3 = config("4 - Summer Weather", "5 - Plains 3", Environments.None, "Environments set by Summer season");
            _Summer_Plains_Weather4 = config("4 - Summer Weather", "5 - Plains 4", Environments.None, "Environments set by Summer season");

            _Summer_MistLands_Weather1 = config("4 - Summer Weather", "6 - MistLands 1", Environments.None, "Environments set by Summer season");
            _Summer_MistLands_Weather2 = config("4 - Summer Weather", "6 - MistLands 2", Environments.None, "Environments set by Summer season");
            _Summer_MistLands_Weather3 = config("4 - Summer Weather", "6 - MistLands 3", Environments.None, "Environments set by Summer season");
            _Summer_MistLands_Weather4 = config("4 - Summer Weather", "6 - MistLands 4", Environments.None, "Environments set by Summer season");

            _Summer_Ocean_Weather1 = config("4 - Summer Weather", "6 - Ocean 1", Environments.None, "Environments set by Summer season");
            _Summer_Ocean_Weather2 = config("4 - Summer Weather", "6 - Ocean 2", Environments.None, "Environments set by Summer season");
            _Summer_Ocean_Weather3 = config("4 - Summer Weather", "6 - Ocean 3", Environments.None, "Environments set by Summer season");
            _Summer_Ocean_Weather4 = config("4 - Summer Weather", "6 - Ocean 4", Environments.None, "Environments set by Summer season");
            
            _Summer_AshLands_Weather1 = config("4 - Summer Weather", "7 - AshLands 1", Environments.None, "Environments set by Summer season");
            _Summer_AshLands_Weather2 = config("4 - Summer Weather", "7 - AshLands 2", Environments.None, "Environments set by Summer season");
            _Summer_AshLands_Weather3 = config("4 - Summer Weather", "7 - AshLands 3", Environments.None, "Environments set by Summer season");
            _Summer_AshLands_Weather4 = config("4 - Summer Weather", "7 - AshLands 4", Environments.None, "Environments set by Summer season");
            
            _Summer_DeepNorth_Weather1 = config("4 - Summer Weather", "8 - DeepNorth 1", Environments.None, "Environments set by Summer season");
            _Summer_DeepNorth_Weather2 = config("4 - Summer Weather", "8 - DeepNorth 2", Environments.None, "Environments set by Summer season");
            _Summer_DeepNorth_Weather3 = config("4 - Summer Weather", "8 - DeepNorth 3", Environments.None, "Environments set by Summer season");
            _Summer_DeepNorth_Weather4 = config("4 - Summer Weather", "8 - DeepNorth 4", Environments.None, "Environments set by Summer season");
            
            _SummerColor1 = config("6 - Summer", "Color 1", SeasonColors.SummerColors[0], "Color tint applied to prefabs");
            _SummerColor2 = config("6 - Summer", "Color 2", SeasonColors.SummerColors[1], "Color tint applied to prefabs");
            _SummerColor3 = config("6 - Summer", "Color 3", SeasonColors.SummerColors[2], "Color tint applied to prefabs");
            _SummerColor4 = config("6 - Summer", "Color 4", SeasonColors.SummerColors[3], "Color tint applied to prefabs");
            
            _SummerStartEffects = config("6 - Summer", "Start Effect", SpecialEffects.SpecialEffect.None, "Visual effect applied when season starts");
            _SummerStopEffects = config("6 - Summer", "Stop Effect", SpecialEffects.SpecialEffect.None, "Visual effect applied when season ends");
            #endregion
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                SeasonalityLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                SeasonalityLogger.LogError($"There was an issue loading your {ConfigFileName}");
                SeasonalityLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }
        
        //
        // class AcceptableShortcuts : AcceptableValueBase
        // {
        //     public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
        //     {
        //     }
        //
        //     public override object Clamp(object value) => value;
        //     public override bool IsValid(object value) => true;
        //
        //     public override string ToDescriptionString() =>
        //         "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        // }

        #endregion
    }
}