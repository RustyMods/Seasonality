using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using TMPro;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;
using static Seasonality.Seasons.YamlConfigurations;

namespace Seasonality.Seasons;

public static class SeasonalEffects
{
    private static Season currentSeason = _Season.Value;
    private static int SeasonIndex = (int)_Season.Value; // Get index from config saved value
    private static DateTime LastSeasonChange = DateTime.UtcNow;

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Update))]
    static class EnvManPatch
    {
        private static void Postfix()
        {
            if (_ModEnabled.Value is Toggle.Off) return;
            if (_SeasonControl.Value is Toggle.On) return;
            if (_SeasonDurationDays.Value == 0 && _SeasonDurationHours.Value == 0 && _SeasonDurationMinutes.Value == 0) return;
            // To throttle seasonal changes to a minimum of 1 minute
            if (LastSeasonChange > DateTime.UtcNow + TimeSpan.FromSeconds(30)) return;
            
            if (workingAsType is WorkingAs.Server)
            {
                TimeSpan TimeDifference = GetTimeDifference(); 
            
                if (TimeDifference <= TimeSpan.Zero + TimeSpan.FromSeconds(3))
                {
                    if (_Season.Value == (Season)SeasonIndex)
                    {
                        SeasonIndex = (SeasonIndex + 1) % Enum.GetValues(typeof(Season)).Length;
                        _Season.Value = (Season)SeasonIndex;
                    }
                    else
                    {
                        _Season.Value = (Season)SeasonIndex;
                        SeasonIndex = (SeasonIndex + 1) % Enum.GetValues(typeof(Season)).Length;
                    }
                    _LastSavedSeasonChange.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                    LastSeasonChange = DateTime.UtcNow;
                }
                else if (_Season.Value != (Season)SeasonIndex)
                {
                    // To switch it back to timer settings if configs changed
                    _Season.Value = (Season)SeasonIndex;
                    LastSeasonChange = DateTime.UtcNow;
                }
            }
            else
            {
                if (!Player.m_localPlayer) return;
                if (Player.m_localPlayer.IsDead()) return;
                
                if (workingAsType is WorkingAs.Client)
                {
                    // If user is a client connected to a server, then do not set seasons
                    // Wait for server to change config value
                    return;
                }

                if (GetTimeDifference() <= TimeSpan.Zero)
                {
                    if (_Season.Value == (Season)SeasonIndex)
                    {
                        SeasonIndex = (SeasonIndex + 1) % Enum.GetValues(typeof(Season)).Length;
                        _Season.Value = (Season)SeasonIndex;
                    }
                    else
                    {
                        _Season.Value = (Season)SeasonIndex;
                        SeasonIndex = (SeasonIndex + 1) % Enum.GetValues(typeof(Season)).Length;
                    }
                    _LastSavedSeasonChange.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                    LastSeasonChange = DateTime.UtcNow;
                }
                else if (_Season.Value != (Season)SeasonIndex)
                {
                    // To switch it back to timer settings if configs changed
                    _Season.Value = (Season)SeasonIndex;
                    LastSeasonChange = DateTime.UtcNow;
                }
            }
        }
    }

    private static void OldTimer(TMP_Text timer)
    {
        // Valheim days are 30min long 
        
        int duration = (_SeasonDurationDays.Value * 24 * 60) + (_SeasonDurationHours.Value * 60) + (_SeasonDurationMinutes.Value);
        int remainingDays = duration - (EnvMan.instance.GetCurrentDay() % duration) + 1;
        float fraction = EnvMan.instance.GetDayFraction(); // value between 0 - 1 - time of day
        float remainder = remainingDays - fraction;

        // Convert to in-game time
        int totalSeconds = (int)(remainder * EnvMan.instance.m_dayLengthSec);

        string time = $"{totalSeconds}";
        
        timer.gameObject.SetActive(_CounterVisible.Value is SeasonalityPlugin.Toggle.On);
        timer.text = time;
        
        if (workingAsType is WorkingAs.Client)
        {
            // If user is a client connected to a server, then do not set seasons
            // Wait for server to change config value

            return;
        }

        if (totalSeconds < 3)
        {
            if (_Season.Value == (Season)SeasonIndex)
            {
                SeasonIndex = (SeasonIndex + 1) % Enum.GetValues(typeof(Season)).Length;
                _Season.Value = (Season)SeasonIndex;
            }
            else
            {
                _Season.Value = (Season)SeasonIndex;
                SeasonIndex = (SeasonIndex + 1) % Enum.GetValues(typeof(Season)).Length;
            }
            _LastSavedSeasonChange.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        }
        else if (_Season.Value != (Season)SeasonIndex)
        {
            // To switch it back to timer settings if configs changed
            _Season.Value = (Season)SeasonIndex;
        }
    }

    public static TimeSpan GetTimeDifference()
    {
        return DateTime.Parse(_LastSavedSeasonChange.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                                      + TimeSpan.FromDays(_SeasonDurationDays.Value)
                                      + TimeSpan.FromHours(_SeasonDurationHours.Value)
                                      + TimeSpan.FromMinutes(_SeasonDurationMinutes.Value)
                                      - DateTime.UtcNow;
    }

    private static Toggle lastToggled = _ModEnabled.Value;
    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    static class PlayerUpdatePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance.IsPlayer() || !__instance) return;
            if (_ModEnabled.Value is Toggle.Off)
            {
                // Make sure this code is only called once and stops updating until mod is re-enabled
                if (lastToggled is Toggle.Off) return;
                SEMan? SEMan = __instance.GetSEMan();
                if (SEMan == null) return;
                // Make sure to remove status effect when user disables mod
                StatusEffect? currentEffect = SEMan.GetStatusEffects().Find(effect => effect is SeasonEffect);
                if (currentEffect) SEMan.RemoveStatusEffect(currentEffect);
                TerrainPatch.UpdateTerrain();
                // Set season to summer as it uses mostly default values
                _SeasonControl.Value = Toggle.On;
                _Season.Value = Season.Summer;
                MaterialReplacer.ModifyCachedMaterials();

                lastToggled = Toggle.Off;
                return;
            }

            if (_ModEnabled.Value is Toggle.On && lastToggled is Toggle.Off)
            {
                // Make sure when mod is re-enabled, that the seasonal effects are re-applied
                ApplySeasonalEffects(__instance);
                SetSeasonalKey();
                TerrainPatch.UpdateTerrain();
                MaterialReplacer.ModifyCachedMaterials();
                lastToggled = Toggle.On;
            }
            if (currentSeason == _Season.Value) return;
            // If season has changed, apply new seasonal effect
            TerrainPatch.UpdateTerrain();
            ApplySeasonalEffects(__instance);
            SetSeasonalKey();
            MaterialReplacer.ModifyCachedMaterials();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    static class PlayerAwakePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance.IsPlayer() || !__instance) return;
            if (!ZNetScene.instance) return;
            if (workingAsType is WorkingAs.Client)
            {
                if (ZNet.instance.IsServer())
                {
                    workingAsType = WorkingAs.Both;
                }
            }
            if (_ModEnabled.Value is Toggle.Off) return;

            ApplySeasonalEffects(__instance);
            SetSeasonalKey();
            TerrainPatch.UpdateTerrain();
            MaterialReplacer.ModifyCachedMaterials();
        }
    }

    private static void SetSeasonalKey()
    {
        if (!ZoneSystem.instance) return;
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
        Toggle toggle = _SeasonalEffectsEnabled.Value;
        SEMan? SEMan = __instance.GetSEMan();
        if (SEMan == null) return;
        // Remove all seasonal effects
        List<StatusEffect> activeEffects = SEMan.GetStatusEffects();
        List<StatusEffect> statusEffects = activeEffects.FindAll(effect => effect is SeasonEffect);
        foreach (StatusEffect effect in statusEffects)
        {
            SEMan.RemoveStatusEffect(effect);
        }
        ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is SeasonEffect);
        // Apply new seasonal effect
        StatusEffect? SeasonEffect = null!;
        switch (_Season.Value)
        {
            case Season.Fall:
                SeasonalEffect FallSeasonEffect = new SeasonalEffect();
                FallSeasonEffect.effectName = "fall_season";
                FallSeasonEffect.displayName = _FallName.Value;
                FallSeasonEffect.sprite = ValknutIcon;
                FallSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_FallStartEffects.Value) };
                FallSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_FallStopEffects.Value) };
                FallSeasonEffect.startMsg = _FallStartMsg.Value;
                FallSeasonEffect.effectTooltip = _FallTooltip.Value;
                FallSeasonEffect.damageMod = toggle is Toggle.On ? $"{_FallResistanceElement.Value}={_FallResistanceMod.Value}" : "";
                FallSeasonEffect.Modifier = toggle is Toggle.On ? _FallModifier.Value : Modifier.None;
                FallSeasonEffect.m_newValue = _FallValue.Value;
                // Yaml
                SeasonalEffect YmlFallSeasonEffect = new SeasonalEffect();
                YmlFallSeasonEffect.effectName = "fall_season";
                YmlFallSeasonEffect.displayName = fallData.name;
                YmlFallSeasonEffect.sprite = ValknutIcon;
                YmlFallSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_FallStartEffects.Value) };
                YmlFallSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_FallStopEffects.Value) };
                YmlFallSeasonEffect.startMsg = fallData.startMessage;
                YmlFallSeasonEffect.effectTooltip = fallData.tooltip;

                string formattedFallResistances = "";
                for (int index = 0; index < fallData.resistances.Count; index++)
                {
                    HitData.DamageModPair data = fallData.resistances[index];
                    if (data.m_modifier == HitData.DamageModifier.Normal) continue;
                    string formattedData = $"{data.m_type}={data.m_modifier}";
                    formattedFallResistances += formattedData + ",";
                }

                YmlFallSeasonEffect.Modifiers = fallData.modifiers;
                YmlFallSeasonEffect.useModifiers = toggle is Toggle.On;
                YmlFallSeasonEffect.damageMod = toggle is Toggle.On ? formattedFallResistances : "";
                
                StatusEffect? FallEffect = _YamlConfigurations.Value is Toggle.On ? YmlFallSeasonEffect.Init() : FallSeasonEffect.Init();
                if (FallEffect) SeasonEffect = FallEffect;
                break;
            case Season.Spring:
                SeasonalEffect SpringSeasonEffect = new SeasonalEffect();
                SpringSeasonEffect.effectName = "spring_season";
                SpringSeasonEffect.displayName = _SpringName.Value;
                SpringSeasonEffect.sprite = ValknutIcon;
                SpringSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SpringStartEffects.Value) };
                SpringSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SpringStopEffects.Value) };
                SpringSeasonEffect.startMsg = _SpringStartMsg.Value;
                SpringSeasonEffect.effectTooltip = _SpringTooltip.Value;
                SpringSeasonEffect.damageMod = toggle is SeasonalityPlugin.Toggle.On
                    ? $"{_SpringResistanceElement.Value}={_SpringResistanceMod.Value}"
                    : "";
                SpringSeasonEffect.Modifier = toggle is SeasonalityPlugin.Toggle.On ? _SpringModifier.Value : Modifier.None;
                SpringSeasonEffect.m_newValue = toggle is SeasonalityPlugin.Toggle.On ? _SpringValue.Value : 0;
                // Yaml
                SeasonalEffect YmlSpringSeasonEffect = new SeasonalEffect();
                YmlSpringSeasonEffect.effectName = "spring_season";
                YmlSpringSeasonEffect.displayName = springData.name;
                YmlSpringSeasonEffect.sprite = ValknutIcon;
                YmlSpringSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SpringStartEffects.Value) };
                YmlSpringSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SpringStopEffects.Value) };
                YmlSpringSeasonEffect.startMsg = springData.startMessage;
                YmlSpringSeasonEffect.effectTooltip = springData.tooltip;

                string formattedSpringResistances = "";
                for (int index = 0; index < springData.resistances.Count; index++)
                {
                    HitData.DamageModPair data = springData.resistances[index];
                    if (data.m_modifier == HitData.DamageModifier.Normal) continue;
                    string formattedData = $"{data.m_type}={data.m_modifier}";
                    formattedSpringResistances += formattedData + ",";
                }

                YmlSpringSeasonEffect.Modifiers = springData.modifiers;
                YmlSpringSeasonEffect.useModifiers = toggle is Toggle.On;
                YmlSpringSeasonEffect.damageMod = toggle is Toggle.On ? formattedSpringResistances : "";
                
                StatusEffect? SpringEffect = _YamlConfigurations.Value is Toggle.On ? YmlSpringSeasonEffect.Init() : SpringSeasonEffect.Init();
                if (SpringEffect) SeasonEffect = SpringEffect;
                break;
            case Season.Winter:
                if (_WinterAlwaysCold.Value is SeasonalityPlugin.Toggle.On)
                {
                    StatusEffect? AlwaysCold = InitWinterAlwaysColdEffect();
                    if (AlwaysCold) SEMan.AddStatusEffect(AlwaysCold);
                }
                SeasonalEffect WinterSeasonEffect = new SeasonalEffect();
                WinterSeasonEffect.effectName = "winter_season";
                WinterSeasonEffect.displayName = _WinterName.Value;
                WinterSeasonEffect.sprite = ValknutIcon;
                WinterSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStartEffects.Value) };
                WinterSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStopEffects.Value) };
                WinterSeasonEffect.startMsg = _WinterStartMsg.Value;
                WinterSeasonEffect.effectTooltip = _WinterTooltip.Value;
                WinterSeasonEffect.damageMod = toggle is SeasonalityPlugin.Toggle.On ? $"{_WinterResistanceElement.Value}={_WinterResistantMod.Value}" : "";
                WinterSeasonEffect.Modifier = toggle is SeasonalityPlugin.Toggle.On ? _WinterModifier.Value : Modifier.None;
                WinterSeasonEffect.m_newValue = _WinterValue.Value;
                // Yaml
                SeasonalEffect YmlWinterSeasonEffect = new SeasonalEffect();
                YmlWinterSeasonEffect.effectName = "winter_season";
                YmlWinterSeasonEffect.displayName = winterData.name;
                YmlWinterSeasonEffect.sprite = ValknutIcon;
                YmlWinterSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStartEffects.Value) };
                YmlWinterSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStopEffects.Value) };
                YmlWinterSeasonEffect.startMsg = winterData.startMessage;
                YmlWinterSeasonEffect.effectTooltip = winterData.tooltip;

                string formattedWinterResistances = "";
                for (int index = 0; index < winterData.resistances.Count; index++)
                {
                    HitData.DamageModPair data = winterData.resistances[index];
                    if (data.m_modifier == HitData.DamageModifier.Normal) continue;
                    string formattedData = $"{data.m_type}={data.m_modifier}";
                    formattedWinterResistances += formattedData + ",";
                }

                YmlWinterSeasonEffect.Modifiers = winterData.modifiers;
                YmlWinterSeasonEffect.useModifiers = toggle is Toggle.On;
                YmlWinterSeasonEffect.damageMod = toggle is Toggle.On ? formattedWinterResistances : "";

                StatusEffect? WinterEffect = _YamlConfigurations.Value is Toggle.On ? YmlWinterSeasonEffect.Init() : WinterSeasonEffect.Init();
                if (WinterEffect) SeasonEffect = WinterEffect;
                break;
            case Season.Summer:
                SeasonalEffect SummerSeasonEffect = new SeasonalEffect();
                SummerSeasonEffect.effectName = "summer_season";
                SummerSeasonEffect.displayName = _SummerName.Value;
                SummerSeasonEffect.sprite = ValknutIcon;
                SummerSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SummerStartEffects.Value) };
                SummerSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SummerStopEffects.Value) };
                SummerSeasonEffect.startMsg = _SummerStartMsg.Value;
                SummerSeasonEffect.effectTooltip = _SummerTooltip.Value;
                SummerSeasonEffect.Modifier = toggle is SeasonalityPlugin.Toggle.On ? _SummerModifier.Value : Modifier.None;
                SummerSeasonEffect.m_newValue = _SummerValue.Value;
                SummerSeasonEffect.damageMod = toggle is SeasonalityPlugin.Toggle.On ? $"{ _SummerResistanceElement.Value}={_SummerResistanceMod.Value}" : "";
                // Yaml
                SeasonalEffect YmlSummerSeasonEffect = new SeasonalEffect();
                YmlSummerSeasonEffect.effectName = "summer_season";
                YmlSummerSeasonEffect.displayName = summerData.name;
                YmlSummerSeasonEffect.sprite = ValknutIcon;
                YmlSummerSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SummerStartEffects.Value) };
                YmlSummerSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_SummerStopEffects.Value) };
                YmlSummerSeasonEffect.startMsg = summerData.startMessage;
                YmlSummerSeasonEffect.effectTooltip = summerData.tooltip;

                string formattedSummerResistances = "";
                for (int index = 0; index < summerData.resistances.Count; index++)
                {
                    HitData.DamageModPair data = summerData.resistances[index];
                    if (data.m_modifier == HitData.DamageModifier.Normal) continue;
                    string formattedData = $"{data.m_type}={data.m_modifier}";
                    formattedSummerResistances += formattedData + ",";
                }

                YmlSummerSeasonEffect.Modifiers = summerData.modifiers;
                YmlSummerSeasonEffect.useModifiers = toggle is Toggle.On;
                YmlSummerSeasonEffect.damageMod = toggle is Toggle.On ? formattedSummerResistances : "";
                
                StatusEffect? SummerEffect = _YamlConfigurations.Value is Toggle.On ? YmlSummerSeasonEffect.Init() : SummerSeasonEffect.Init();
                if (SummerEffect) SeasonEffect = SummerEffect;
                break;
        }

        if (SeasonEffect != null)
        {
            SeasonalCompendium.customTooltip = $"<color=orange>{SeasonEffect.m_name}</color>\n{SeasonEffect.m_tooltip}";
            SEMan.AddStatusEffect(SeasonEffect);
        }
        
        currentSeason = _Season.Value;
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

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
    static class EnvManColdPatch
    {
        private static void Postfix(EnvMan __instance, ref bool __result)
        {
            EnvSetup currentEnvironment = __instance.GetCurrentEnvironment();
            bool vanillaResult = currentEnvironment != null && (currentEnvironment.m_isCold || currentEnvironment.m_isColdAtNight && !__instance.IsDay());
            
            if (_Season.Value is Season.Summer) __result = _SummerNeverCold.Value is SeasonalityPlugin.Toggle.Off && vanillaResult; // If on, result = false
            if (!Player.m_localPlayer) return;
            List<StatusEffect>? statusEffects = Player.m_localPlayer.GetSEMan().GetStatusEffects();
            if (statusEffects.Exists(x => x.name == "AlwaysCold")) __result = false;
        }
    }
}