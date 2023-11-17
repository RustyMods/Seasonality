using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class SeasonalEffects
{
    public static readonly List<SeasonEffect> SeasonEffectList = new();
    private static Season currentSeason = _Season.Value;
    private static int SeasonIndex = (int)_Season.Value; // Get index from config saved value
    private static DateTime LastSeasonChange = DateTime.Now;

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Update))]
    static class EnvManPatch
    {
        private static void Postfix(EnvMan __instance)
        {
            if (workingAsType is not WorkingAs.Server || _ModEnabled.Value is Toggle.Off) return;
            if (_SeasonDuration.Value == 0 || _SeasonLocked.Value is Toggle.On) return;
            
            // Server patch to track season counter and set config file
            int remainingDays = _SeasonDuration.Value - (__instance.GetCurrentDay() % _SeasonDuration.Value) + 1;
            float fraction = __instance.GetDayFraction(); // value between 0 - 1 - time of day
            float remainder = remainingDays - fraction;
            int totalMinutes = (int)(remainder * 24 * 60);

            if (remainingDays < 1 && totalMinutes < 5)
            {
                SeasonalityLogger.LogMessage("Season timer hit zero, sending season config to clients");
                // Throttle the rate at which the server is allowed to change config
                if (LastSeasonChange + TimeSpan.FromSeconds(5) > DateTime.Now) return;
                // Since user can manipulate config value
                // Check to see if the next season is different than the current
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

                LastSeasonChange = DateTime.Now;
            }
            else if (_Season.Value != (Season)SeasonIndex)
            {
                // To switch it back to timer settings if configs changed
                _Season.Value = (Season)SeasonIndex;
            }
        }
    }
    
    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateStatusEffects))]
    static class HudUpdateStatusEffectPatch
    {
        private static void Postfix(Hud __instance, List<StatusEffect> statusEffects)
        {
            if (!__instance) return;
            if (_ModEnabled.Value is Toggle.Off) return;
            StatusEffect? seasonEffect = statusEffects.Find(x => SeasonEffectList.Exists(y => y.name == x.name));
            if (!seasonEffect) return;
            int index = statusEffects.FindIndex(x => x.name == seasonEffect.name);
            RectTransform? rectTransform = __instance.m_statusEffects[index];
            if (!rectTransform) return;
            Transform? timeText = rectTransform.Find("TimeText");
            if (!timeText) return;
            if (!timeText.TryGetComponent(out TMP_Text tmpText)) return;
            if (_SeasonDuration.Value == 0 || _SeasonLocked.Value is Toggle.On)
            {
                tmpText.gameObject.SetActive(false);
                return;
            }
            int remainingDays = _SeasonDuration.Value - (EnvMan.instance.GetCurrentDay() % _SeasonDuration.Value) + 1;
            float fraction = EnvMan.instance.GetDayFraction(); // value between 0 - 1 - time of day
            float remainder = remainingDays - fraction;
            
            // Convert to in-game time
            int totalMinutes = (int)(remainder * 24 * 60);
            int hours = remainingDays - 2;
            int minutes = totalMinutes % (24 * 60) / 60;
            int seconds = totalMinutes % 60;

            string time = $"{hours:D2}:{minutes:D2}:{seconds:D2}";

            tmpText.gameObject.SetActive(_CounterVisible.Value is Toggle.On);
            tmpText.text = time;
            if (workingAsType is WorkingAs.Client)
            {
                // If user is a client connected to a server, then do not set seasons
                // Wait for server to change config value
                return;
            }
            // Use calculated data to set season change if counter hits less than 3
            if (remainingDays < 1 && totalMinutes < 3)
            {
                if (LastSeasonChange + TimeSpan.FromSeconds(5) > DateTime.Now) return;
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

                LastSeasonChange = DateTime.Now;
            }

            else if (_Season.Value != (Season)SeasonIndex)
            {
                // To switch it back to timer settings if configs changed
                _Season.Value = (Season)SeasonIndex;
            }
        }
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
                List<StatusEffect> effectsToRemove = new();
                foreach (StatusEffect effect in SEMan.GetStatusEffects())
                {
                    if (!SeasonEffectList.Exists(x => x.name == effect.name)) continue;
                    effectsToRemove.Add(effect);
                }

                foreach (StatusEffect effect in effectsToRemove)
                {
                    SEMan.RemoveStatusEffect(effect);
                }
                SeasonEffectList.Clear();
                TerrainPatch.UpdateTerrain();
                lastToggled = Toggle.Off;
                return;
            }

            if (_ModEnabled.Value is Toggle.On && lastToggled is Toggle.Off)
            {
                // Make sure when mod is re-enabled, that the seasonal effects are re-applied
                ApplySeasonalEffects(__instance);
                SetSeasonalKey();
                TerrainPatch.UpdateTerrain();
                lastToggled = Toggle.On;
            }
            if (currentSeason == _Season.Value) return;
            // If season has changed, apply new seasonal effect
            TerrainPatch.UpdateTerrain();
            ApplySeasonalEffects(__instance);
            SetSeasonalKey();
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
        foreach (SeasonEffect? effect in SeasonEffectList)
        {
            if (!SEMan.HaveStatusEffect(effect.name)) continue;
            SEMan.RemoveStatusEffect(effect);
        }
        ObjectDB.instance.m_StatusEffects.RemoveAll(effect => effect is SeasonEffect);
        SeasonEffectList.Clear();
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
                FallSeasonEffect.damageMod = _FallResistance.Value;
                FallSeasonEffect.Modifier = toggle is Toggle.On ? _FallModifier.Value : Modifier.None;
                FallSeasonEffect.m_newValue = _FallValue.Value;
                
                StatusEffect? FallEffect = FallSeasonEffect.Init();
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
                SpringSeasonEffect.damageMod = _SpringResistance.Value;
                SpringSeasonEffect.Modifier = toggle is Toggle.On ? _SpringModifier.Value : Modifier.None;
                SpringSeasonEffect.m_newValue = toggle is Toggle.On ? _SpringValue.Value : 0;

                StatusEffect? SpringEffect = SpringSeasonEffect.Init();
                if (SpringEffect) SeasonEffect = SpringEffect;
                break;
            case Season.Winter:
                SeasonalEffect WinterSeasonEffect = new SeasonalEffect();
                WinterSeasonEffect.effectName = "winter_season";
                WinterSeasonEffect.displayName = _WinterName.Value;
                WinterSeasonEffect.sprite = ValknutIcon;
                WinterSeasonEffect.startEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStartEffects.Value) };
                WinterSeasonEffect.stopEffectNames = new[] { SpecialEffects.GetEffectPrefabName(_WinterStopEffects.Value) };
                WinterSeasonEffect.startMsg = _WinterStartMsg.Value;
                WinterSeasonEffect.effectTooltip = _WinterTooltip.Value;
                WinterSeasonEffect.damageMod = toggle is Toggle.On ? _WinterResistance.Value : "";
                WinterSeasonEffect.Modifier = toggle is Toggle.On ? _WinterModifier.Value : Modifier.None;
                WinterSeasonEffect.m_newValue = _WinterValue.Value;

                StatusEffect? WinterEffect = WinterSeasonEffect.Init();
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
                SummerSeasonEffect.Modifier = toggle is Toggle.On ? _SummerModifier.Value : Modifier.None;
                SummerSeasonEffect.m_newValue = _SummerValue.Value;
                SummerSeasonEffect.damageMod = _SummerResistance.Value;

                StatusEffect? SummerEffect = SummerSeasonEffect.Init();
                if (SummerEffect) SeasonEffect = SummerEffect;
                break;
        }

        if (SeasonEffect != null)
        {
            SEMan.AddStatusEffect(SeasonEffect);
        }

        currentSeason = _Season.Value;
    }
}