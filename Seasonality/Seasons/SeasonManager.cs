using System.Collections.Generic;
using HarmonyLib;
using Seasonality.SeasonStatusEffect;
using Seasonality.SeasonUtility;
using Seasonality.Textures;
using Seasonality.Weather;
using ServerSync;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Configurations.YamlConfigurations;
using static Seasonality.Textures.SpriteManager;

namespace Seasonality.Seasons;

public static class SeasonalEffects
{
    private static Season currentSeason = _Season.Value;

    private static readonly CustomSyncedValue<string> SyncedSeasons = new(SeasonalityPlugin.ConfigSync, "SyncedSeason", "");

    private static readonly CustomSyncedValue<bool> SyncedReadyToChange = new(SeasonalityPlugin.ConfigSync, "SyncedSleepTime", false);
    
    [HarmonyPatch(typeof(Game), nameof(Game.SleepStop))]
    private static class GameSleepStartPatch
    {
        private static void Prefix(Game __instance)
        {
            if (!__instance) return;
            if (_ModEnabled.Value is Toggle.Off) return;
            if (_SleepSeasons.Value is Toggle.Off) return;
            if (_SeasonControl.Value is Toggle.On) return;
            if (SyncedReadyToChange.Value is false) return;
            UpdateSeasonIndex();
            WakeUpMessageRan = false;
            SyncedReadyToChange.Value = false;
        }
    }

    private static bool WakeUpMessageRan = false;

    [HarmonyPatch(typeof(Player), nameof(Player.Message))]
    private static class PlayerMessagePatch
    {
        private static void Prefix(Player __instance, ref string msg)
        {
            if (!__instance) return;
            if (WakeUpMessageRan) return;
            if (msg != "$msg_goodmorning") return;
            string seasonMessage = "";
            switch (_Season.Value)
            {
                case Season.Fall:
                    seasonMessage = _FallStartMsg.Value;
                    break;
                case Season.Spring:
                    seasonMessage = _SpringStartMsg.Value;
                    break;
                case Season.Summer:
                    seasonMessage = _SummerStartMsg.Value;
                    break;
                case Season.Winter:
                    seasonMessage = _WinterStartMsg.Value;
                    break;
            }

            msg += "\n\n" + seasonMessage;
            WakeUpMessageRan = true;
        }
    }

    private static void UpdateSeasonIndex()
    {
        switch (_Season.Value)
        {
            case Season.Spring:
                _Season.Value = Season.Summer;
                break;
            case Season.Summer:
                _Season.Value = Season.Fall;
                break;
            case Season.Fall:
                _Season.Value = Season.Winter;
                break;
            case Season.Winter:
                _Season.Value = Season.Spring;
                break;
        }

        _LastInGameSavedSeasonChange.Value = EnvMan.instance.m_totalSeconds;
    }
    public static void CheckInGameTimer()
    {
        if (_ModEnabled.Value is Toggle.Off) return;
        if (_SeasonControl.Value is Toggle.On) return;
        if (_SeasonDurationDays.Value == 0 && _SeasonDurationHours.Value == 0 && _SeasonDurationMinutes.Value == 0) return;
        
        if (!EnvMan.instance) return;

        if (workingAsType is WorkingAs.Server or WorkingAs.Both)
        {
            if (!(GetInGameTimeDifference() <= 3)) return;
            
            if (_SleepSeasons.Value is Toggle.On)
            {
                if (SyncedReadyToChange.Value != true) SyncedReadyToChange.Value = true;
            }
            else
            {
                UpdateSeasonIndex();
            }
        }
        else
        {
            if (!Player.m_localPlayer) return;
            if (Player.m_localPlayer.IsDead()) return;
            
            if (workingAsType is WorkingAs.Client && SyncedSeasons.Value != "")
            {
                // If user is a client connected to a server, then do not set seasons
                // Wait for server to change config value
                return;
            }

            if (!(GetInGameTimeDifference() <= 0)) return;
            
            if (_SleepSeasons.Value is Toggle.On)
            {
                if (SyncedReadyToChange.Value != true) SyncedReadyToChange.Value = true;
            }
            else
            {
                UpdateSeasonIndex();
            }
        }
    }

    public static double GetInGameTimeDifference()
    {
        double daysInSeconds = _SeasonDurationDays.Value * 86400;
        double hoursInSeconds = _SeasonDurationHours.Value * 3600;
        double minutesInSeconds = _SeasonDurationMinutes.Value * 60;
        double TotalSeconds = daysInSeconds + hoursInSeconds + minutesInSeconds;

        switch (_Season.Value)
        {
            case Season.Spring:
                TotalSeconds += _SpringDurationTweak.Value * 60;
                break;
            case Season.Summer:
                TotalSeconds += _SummerDurationTweak.Value * 60;
                break;
            case Season.Fall:
                TotalSeconds += _FallDurationTweak.Value * 60;
                break;
            case Season.Winter:
                TotalSeconds += _WinterDurationTweak.Value * 60;
                break;
        }

        return (_LastInGameSavedSeasonChange.Value + TotalSeconds) - EnvMan.instance.m_totalSeconds;
    }

    private static Toggle lastToggled = _ModEnabled.Value;
    public static void UpdateSeasonEffects()
    {
        if (!Player.m_localPlayer) return;
        if (_ModEnabled.Value is Toggle.Off)
        {
            // Make sure this code is only called once and stops updating until mod is re-enabled
            if (lastToggled is Toggle.Off) return;
            DisableSeasonEffects();
            lastToggled = Toggle.Off;
            return;
        }
        if (_ModEnabled.Value is Toggle.On && lastToggled is Toggle.Off)
        {
            // Make sure when mod is re-enabled, that the seasonal effects are re-applied
            ApplySeasons();
            lastToggled = Toggle.On;
        }
        if (currentSeason == _Season.Value) return;
        // If season has changed, apply new seasonal effect
        ApplySeasons();
        SeasonalityLogger.LogDebug("Season changed to " + _Season.Value);
    }

    private static void DisableSeasonEffects()
    {
        SeasonalityLogger.LogDebug("Disabling seasonal effects");
        SEMan? SEMan = Player.m_localPlayer.GetSEMan();
        if (SEMan == null) return;
        // Make sure to remove status effect when user disables mod
        StatusEffect? currentEffect = SEMan.GetStatusEffects().Find(effect => effect is SeasonEffect);
        if (currentEffect) SEMan.RemoveStatusEffect(currentEffect);
        TerrainPatch.UpdateTerrain();
        // Set season to summer as it uses mostly default values
        _SeasonControl.Value = Toggle.On;
        _Season.Value = Season.Summer;
        MaterialReplacer.ModifyCachedMaterials(_Season.Value);
        WaterMaterial.ModifyWater(_Season.Value);
    }

    private static bool enteredCustomBiome;
    public static void CheckBiomeSeason()
    {
        if (_CheckCustomBiomes.Value is Toggle.Off) return;
        if (!Player.m_localPlayer) return;
        Heightmap.Biome biome = Player.m_localPlayer.GetCurrentBiome();
        if (isBiomeDefined(biome))
        {
            if (!enteredCustomBiome) return;
            ApplySeasons();
            enteredCustomBiome = false;
            return;
        }
        if (enteredCustomBiome) return;
        if (_Season.Value is Season.Summer) return;
        SeasonalityLogger.LogDebug("Biome is not defined, setting season to summer");
        TerrainPatch.SetDefaultTerrainSettings();
        MaterialReplacer.ModifyCachedMaterials(Season.Summer);
        WaterMaterial.ModifyWater(Season.Summer);
        enteredCustomBiome = true;
    }

    private static bool isBiomeDefined(Heightmap.Biome biome)
    {
        return _AffectedBiomes.Value.HasFlagFast(biome);
    }
    private static void ApplySeasons()
    {
        SeasonalityLogger.LogDebug("Applying seasons");
        TerrainPatch.UpdateTerrain();
        ApplySeasonalEffects(Player.m_localPlayer);
        SetSeasonalKey();
        MaterialReplacer.ModifyCachedMaterials(_Season.Value);
        if (_WinterFreezesWater.Value is Toggle.On) WaterMaterial.ModifyWater(_Season.Value);
        currentSeason = _Season.Value;
    }
    public static void UpdateAlwaysColdEffect(Player instance)
    {
        
        SEMan SEMan = instance.GetSEMan();
        bool isColdPlus = SEMan.HaveStatusEffect("AlwaysCold");
        bool isBurning = SEMan.HaveStatusEffect("Burning");
        bool isWarm = SEMan.HaveStatusEffect("CampFire");
        
        if (!isColdPlus)
        {
            if (_WinterAlwaysCold.Value is Toggle.Off || _Season.Value is not Season.Winter) return;
            if (isBurning || isWarm) return;
            StatusEffect? AlwaysCold = InitWinterAlwaysColdEffect();
            if (AlwaysCold) SEMan.AddStatusEffect(AlwaysCold);
        }
        else if (isColdPlus && (isBurning || isWarm))
        {
            SEMan.RemoveStatusEffect("AlwaysCold".GetStableHashCode());
        }
    }

    // private static bool SeasonsLoaded = false;

    [HarmonyPatch(typeof(Player),nameof(Player.OnSpawned))]
    private static class InitSeasonalEffects
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (__instance != Player.m_localPlayer) return;
            ApplyInitialSeasons();
        }
    }
    
    private static void ApplyInitialSeasons()
    {
        if (!Player.m_localPlayer) return;
        // if (SeasonsLoaded) return;
        if (!ZNetScene.instance) return;
        if (workingAsType is WorkingAs.Client)
        {
            // Check if client is also a server
            if (ZNet.instance.IsServer()) workingAsType = WorkingAs.Both;
        }
        if (_ModEnabled.Value is Toggle.Off) return;
        // Apply seasons
        SeasonalityLogger.LogDebug("Applying initial seasons");
        ApplySeasonalEffects(Player.m_localPlayer);
        SetSeasonalKey();
        TerrainPatch.UpdateTerrain();
        MaterialReplacer.ModifyCachedMaterials(_Season.Value);
        if (_WinterFreezesWater.Value is Toggle.On) WaterMaterial.ModifyWater(_Season.Value);
        SetServerSyncedYmlData();
        if (!EnvMan.instance) return;
        Environment.RegisterServerEnvironments(EnvMan.instance);
    }
    private static void SetSeasonalKey()
    {
        if (!ZoneSystem.instance) return;
        SeasonalityLogger.LogDebug("Setting global keys");
        List<string>? currentKeys = ZoneSystem.instance.GetGlobalKeys();
        List<string> SeasonalKeys = new() { "season_summer", "season_fall", "season_winter", "season_spring" };
        // Remove all seasonal keys
        foreach (string key in SeasonalKeys)
        {
            if (!currentKeys.Exists(x => x == key)) continue;
            ZoneSystem.instance.RemoveGlobalKey(key);
        }
        // Get seasonal key
        string? newKey = _Season.Value switch
        {
            Season.Winter => "season_winter",
            Season.Fall => "season_fall",
            Season.Summer => "season_summer",
            Season.Spring => "season_spring",
            _ => null,
        };
        // Set seasonal key
        if (newKey != null) ZoneSystem.instance.SetGlobalKey(newKey);
    }
    private static void ApplySeasonalEffects(Player __instance)
    {
        SEMan? SEMan = __instance.GetSEMan();
        if (SEMan == null) return;
        SeasonalityLogger.LogDebug("Applying seasonal effects");
        // Remove all seasonal effects
        List<StatusEffect> activeEffects = SEMan.GetStatusEffects();
        List<StatusEffect> statusEffects = activeEffects.FindAll(effect => effect is SeasonEffect);
        foreach (StatusEffect effect in statusEffects)
        {
            SEMan.RemoveStatusEffect(effect);
        }
        ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is SeasonEffect);
        // Apply new seasonal effect
        StatusEffect? SeasonEffect = _YamlConfigurations.Value is Toggle.On ? GetYMLSeasonEffect() : GetConfigSeasonEffect();

        if (_Season.Value is Season.Winter && _WinterAlwaysCold.Value is Toggle.On)
        {
            StatusEffect? AlwaysCold = InitWinterAlwaysColdEffect();
            if (AlwaysCold) SEMan.AddStatusEffect(AlwaysCold);
        }

        if (SeasonEffect)
        {
            SeasonalCompendium.customTooltip = $"<color=orange>{SeasonEffect.m_name}</color>\n{SeasonEffect.m_tooltip}";
            SEMan.AddStatusEffect(SeasonEffect);
        }
        
        currentSeason = _Season.Value;
    }
    private static StatusEffect? GetConfigSeasonEffect()
    {
        switch (_Season.Value)
        {
            case Season.Fall:
                SeasonalEffect FallSeasonEffect = new SeasonalEffect
                {
                    effectName = "fall_season",
                    displayName = _FallName.Value,
                    sprite = ValknutIcon,
                    startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_FallStartEffects.Value) },
                    stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_FallStopEffects.Value) },
                    startMsg = _FallStartMsg.Value,
                    effectTooltip = _FallTooltip.Value,
                    damageMod = _SeasonalEffectsEnabled.Value is Toggle.On ? $"{_FallResistanceElement.Value}={_FallResistanceMod.Value}" : "",
                    Modifier = _SeasonalEffectsEnabled.Value is Toggle.On ? _FallModifier.Value : Modifier.None,
                    m_newValue = _FallValue.Value
                };
                StatusEffect? FallEffect = FallSeasonEffect.Init();
                if (FallEffect) return FallEffect;
                break;
            case Season.Spring:
                SeasonalEffect SpringSeasonEffect = new SeasonalEffect
                {
                    effectName = "spring_season",
                    displayName = _SpringName.Value,
                    sprite = ValknutIcon,
                    startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SpringStartEffects.Value) },
                    stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SpringStopEffects.Value) },
                    startMsg = _SpringStartMsg.Value,
                    effectTooltip = _SpringTooltip.Value,
                    damageMod = _SeasonalEffectsEnabled.Value is Toggle.On
                        ? $"{_SpringResistanceElement.Value}={_SpringResistanceMod.Value}"
                        : "",
                    Modifier = _SeasonalEffectsEnabled.Value is Toggle.On ? _SpringModifier.Value : Modifier.None,
                    m_newValue = _SeasonalEffectsEnabled.Value is Toggle.On ? _SpringValue.Value : 0
                };
                StatusEffect? SpringEffect = SpringSeasonEffect.Init();
                if (SpringEffect) return SpringEffect;
                break;
            case Season.Winter:
                SeasonalEffect WinterSeasonEffect = new SeasonalEffect
                {
                    effectName = "winter_season",
                    displayName = _WinterName.Value,
                    sprite = ValknutIcon,
                    startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStartEffects.Value) },
                    stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStopEffects.Value) },
                    startMsg = _WinterStartMsg.Value,
                    effectTooltip = _WinterTooltip.Value,
                    damageMod = _SeasonalEffectsEnabled.Value is Toggle.On ? $"{_WinterResistanceElement.Value}={_WinterResistantMod.Value}" : "",
                    Modifier = _SeasonalEffectsEnabled.Value is Toggle.On ? _WinterModifier.Value : Modifier.None,
                    m_newValue = _WinterValue.Value
                };

                StatusEffect? WinterEffect =  WinterSeasonEffect.Init();
                if (WinterEffect) return WinterEffect;
                break;
            case Season.Summer:
                SeasonalEffect SummerSeasonEffect = new SeasonalEffect
                {
                    effectName = "summer_season",
                    displayName = _SummerName.Value,
                    sprite = ValknutIcon,
                    startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SummerStartEffects.Value) },
                    stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SummerStopEffects.Value) },
                    startMsg = _SummerStartMsg.Value,
                    effectTooltip = _SummerTooltip.Value,
                    Modifier = _SeasonalEffectsEnabled.Value is Toggle.On ? _SummerModifier.Value : Modifier.None,
                    m_newValue = _SummerValue.Value,
                    damageMod = _SeasonalEffectsEnabled.Value is Toggle.On ? $"{ _SummerResistanceElement.Value}={_SummerResistanceMod.Value}" : ""
                };

                StatusEffect? SummerEffect = SummerSeasonEffect.Init();
                if (SummerEffect) return SummerEffect;
                break;
        }
        return null;
    }
    private static StatusEffect? GetYMLSeasonEffect()
    {
        switch (_Season.Value)
        {
            case Season.Fall:
                SeasonalEffect YmlFallSeasonEffect = new SeasonalEffect
                {
                    effectName = "fall_season",
                    displayName = fallData.name,
                    sprite = ValknutIcon,
                    startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_FallStartEffects.Value) },
                    stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_FallStopEffects.Value) },
                    startMsg = fallData.startMessage,
                    effectTooltip = fallData.tooltip
                };

                string formattedFallResistances = "";
                for (int index = 0; index < fallData.resistances.Count; index++)
                {
                    HitData.DamageModPair data = fallData.resistances[index];
                    if (data.m_modifier == HitData.DamageModifier.Normal) continue;
                    string formattedData = $"{data.m_type}={data.m_modifier}";
                    formattedFallResistances += formattedData + ",";
                }

                YmlFallSeasonEffect.Modifiers = fallData.modifiers;
                YmlFallSeasonEffect.useModifiers = _SeasonalEffectsEnabled.Value is Toggle.On;
                YmlFallSeasonEffect.damageMod = _SeasonalEffectsEnabled.Value is Toggle.On ? formattedFallResistances : "";
                
                StatusEffect? FallEffect = YmlFallSeasonEffect.Init();
                if (FallEffect) return FallEffect;
                break;
            case Season.Spring:
                SeasonalEffect YmlSpringSeasonEffect = new SeasonalEffect
                {
                    effectName = "spring_season",
                    displayName = springData.name,
                    sprite = ValknutIcon,
                    startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SpringStartEffects.Value) },
                    stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SpringStopEffects.Value) },
                    startMsg = springData.startMessage,
                    effectTooltip = springData.tooltip
                };

                string formattedSpringResistances = "";
                for (int index = 0; index < springData.resistances.Count; index++)
                {
                    HitData.DamageModPair data = springData.resistances[index];
                    if (data.m_modifier == HitData.DamageModifier.Normal) continue;
                    string formattedData = $"{data.m_type}={data.m_modifier}";
                    formattedSpringResistances += formattedData + ",";
                }

                YmlSpringSeasonEffect.Modifiers = springData.modifiers;
                YmlSpringSeasonEffect.useModifiers = _SeasonalEffectsEnabled.Value is Toggle.On;
                YmlSpringSeasonEffect.damageMod = _SeasonalEffectsEnabled.Value is Toggle.On ? formattedSpringResistances : "";
                
                StatusEffect? SpringEffect = YmlSpringSeasonEffect.Init();
                if (SpringEffect) return SpringEffect;
                break;
            case Season.Winter:
                SeasonalEffect YmlWinterSeasonEffect = new SeasonalEffect
                {
                    effectName = "winter_season",
                    displayName = winterData.name,
                    sprite = ValknutIcon,
                    startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStartEffects.Value) },
                    stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStopEffects.Value) },
                    startMsg = winterData.startMessage,
                    effectTooltip = winterData.tooltip
                };

                string formattedWinterResistances = "";
                for (int index = 0; index < winterData.resistances.Count; index++)
                {
                    HitData.DamageModPair data = winterData.resistances[index];
                    if (data.m_modifier == HitData.DamageModifier.Normal) continue;
                    string formattedData = $"{data.m_type}={data.m_modifier}";
                    formattedWinterResistances += formattedData + ",";
                }

                YmlWinterSeasonEffect.Modifiers = winterData.modifiers;
                YmlWinterSeasonEffect.useModifiers = _SeasonalEffectsEnabled.Value is Toggle.On;
                YmlWinterSeasonEffect.damageMod = _SeasonalEffectsEnabled.Value is Toggle.On ? formattedWinterResistances : "";

                StatusEffect? WinterEffect = YmlWinterSeasonEffect.Init();
                if (WinterEffect) return WinterEffect;
                break;
            case Season.Summer:
                SeasonalEffect YmlSummerSeasonEffect = new SeasonalEffect
                {
                    effectName = "summer_season",
                    displayName = summerData.name,
                    sprite = ValknutIcon,
                    startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SummerStartEffects.Value) },
                    stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SummerStopEffects.Value) },
                    startMsg = summerData.startMessage,
                    effectTooltip = summerData.tooltip
                };

                string formattedSummerResistances = "";
                for (int index = 0; index < summerData.resistances.Count; index++)
                {
                    HitData.DamageModPair data = summerData.resistances[index];
                    if (data.m_modifier == HitData.DamageModifier.Normal) continue;
                    string formattedData = $"{data.m_type}={data.m_modifier}";
                    formattedSummerResistances += formattedData + ",";
                }

                YmlSummerSeasonEffect.Modifiers = summerData.modifiers;
                YmlSummerSeasonEffect.useModifiers = _SeasonalEffectsEnabled.Value is Toggle.On;
                YmlSummerSeasonEffect.damageMod = _SeasonalEffectsEnabled.Value is Toggle.On ? formattedSummerResistances : "";
                
                StatusEffect? SummerEffect = YmlSummerSeasonEffect.Init();
                if (SummerEffect) return SummerEffect;
                break;
        }
        return null;
    }
    private static StatusEffect? InitWinterAlwaysColdEffect()
    {
        StatusEffect? coldEffect = ObjectDB.instance.GetStatusEffect("Cold".GetStableHashCode());
        if (!coldEffect) return null;
        SeasonEffect clonedColdEffect = ScriptableObject.CreateInstance<SeasonEffect>();
        clonedColdEffect.name = "AlwaysCold";
        clonedColdEffect.m_name = "Cold+";
        clonedColdEffect.m_icon = coldEffect.m_icon;
        clonedColdEffect.data = new SeasonalEffect()
        {
            effectName = "AlwaysCold",
            displayName = "Cold",
            sprite = coldEffect.m_icon,
            Modifiers = new Dictionary<Modifier, float>()
            {
                { Modifier.Attack, 1f },
                { Modifier.HealthRegen, 0.5f },
                { Modifier.StaminaRegen, 0.75f },
                { Modifier.RaiseSkills, 1f },
                { Modifier.Speed, 1f },
                { Modifier.Noise, 1f },
                { Modifier.MaxCarryWeight, 0f },
                { Modifier.Stealth, 1f },
                { Modifier.RunStaminaDrain, 1f },
                { Modifier.DamageReduction, 0f },
                { Modifier.FallDamage, 1f },
                { Modifier.EitrRegen, 0.75f }
            }
        };
        clonedColdEffect.m_tooltip = coldEffect.m_tooltip + "\nHeath regen: <color=orange>-50%</color>\nStamina regen: <color=orange>-25%</color>\nEitr regen: <color=orange>-25%</color>";
        
        ObjectDB.instance.m_StatusEffects.Add(clonedColdEffect);
        return clonedColdEffect;
    }

    private static bool PlayerDied;
    
    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    private static class OnRespawnPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (!PlayerDied) return;
            SeasonalityLogger.LogDebug("Player respawning, applying seasonal effects");
            ApplySeasonalEffects(__instance);
            PlayerDied = false;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    private static class OnDestroyPatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (!__instance.m_nview.IsOwner()) return;
            PlayerDied = true;
        } 
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
    static class EnvManColdPatch
    {
        private static void Postfix(EnvMan __instance, ref bool __result)
        {
            EnvSetup currentEnvironment = __instance.GetCurrentEnvironment();
            bool vanillaResult = currentEnvironment != null && (currentEnvironment.m_isCold || currentEnvironment.m_isColdAtNight && !__instance.IsDay());
            
            if (_Season.Value is Season.Summer) __result = _SummerNeverCold.Value is Toggle.Off && vanillaResult; // If on, result = false
            if (!Player.m_localPlayer) return;
            if (Player.m_localPlayer.GetSEMan().HaveStatusEffect("AlwaysCold")) __result = false;
        }
    }
}