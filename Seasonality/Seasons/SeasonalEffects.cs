using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx;
using HarmonyLib;
using ServerSync;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class SeasonalEffects
{
    private static Season currentSeason = _Season.Value;
    private static int SeasonIndex = (int)_Season.Value; // Get index from config saved value

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Update))]
    static class EnvManPatch
    {
        private static void Postfix()
        {
            if (workingAsType is not WorkingAs.Server || _ModEnabled.Value is SeasonalityPlugin.Toggle.Off) return;
            if (_SeasonControl.Value is SeasonalityPlugin.Toggle.On) return;

            TimeSpan TimeDifference = DateTime.Parse(_LastSavedSeasonChange.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal) 
                                      + TimeSpan.FromDays(_SeasonDurationDays.Value) 
                                      + TimeSpan.FromHours(_SeasonDurationHours.Value) 
                                      + TimeSpan.FromMinutes(_SeasonDurationMinutes.Value) 
                                      - DateTime.UtcNow;
            
            if (TimeDifference < TimeSpan.Zero + TimeSpan.FromSeconds(3))
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
                _LastSavedSeasonChange.Value = DateTime.Now.ToString(CultureInfo.CurrentCulture);
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
            if (_ModEnabled.Value is SeasonalityPlugin.Toggle.Off) return;
            if (Player.m_localPlayer.IsDead()) return;
            
            // Get Status Effect UI
            int index = statusEffects.FindIndex(effect => effect is SeasonEffect && effect.name != "AlwaysCold");
            if (index == -1) return;
            
            RectTransform? rectTransform = __instance.m_statusEffects[index];
            
            if (!rectTransform) return;
            Transform? timeText = rectTransform.Find("TimeText");
            if (!timeText) return;
            if (!timeText.TryGetComponent(out TMP_Text tmpText)) return;
            if (_SeasonControl.Value is SeasonalityPlugin.Toggle.On)
            {
                tmpText.gameObject.SetActive(false);
                return;
            }
            // if (_TimerPositionEnabled.Value is SeasonalityPlugin.Toggle.On) tmpText.transform.localPosition = _TimerPosition.Value;
            if (_SeasonDurationDays.Value == 0 && _SeasonDurationHours.Value == 0 && _SeasonDurationMinutes.Value == 0)
            {
                tmpText.gameObject.SetActive(false);
                return;
            }

            NewTimer(tmpText);
        }
    }

    private static void AugaCompatibility(Hud __instance)
    {
        if (AugaLoaded)
        {
            // _SeasonControl.Value = SeasonalityPlugin.Toggle.On;
            RectTransform? root = __instance.m_statusEffectListRoot;
            if (!root) return;
            int match = 0;
            for (int i = 0; i < root.childCount; ++i)
            {
                Transform child = root.GetChild(i);
                Transform? Background = child.GetChild(0);
                Transform? sprite = Background.GetChild(2);
                if (!sprite.TryGetComponent(out Image image)) continue;
                if (image.sprite.name != "valknutIcon.png") continue;
                match = i;
                break;
            }
        
            if (match == 0) return;
            
            Transform? seasonObj = root.GetChild(match);
            Transform? seasonBk = seasonObj.GetChild(0);
            Transform? countDown = seasonBk.GetChild(1);
            if (!countDown.TryGetComponent(out TMP_Text tmp_Text)) return;
            
            TimeSpan TimeDifference = DateTime.Parse(_LastSavedSeasonChange.Value, CultureInfo.InvariantCulture) + TimeSpan.FromDays(_SeasonDurationDays.Value) + TimeSpan.FromHours(_SeasonDurationHours.Value) + TimeSpan.FromMinutes(_SeasonDurationMinutes.Value) - DateTime.Now;
            int days = TimeDifference.Days;
            int hour = TimeDifference.Hours;
            int minutes = TimeDifference.Minutes;
            int seconds = TimeDifference.Seconds;
        
            // if (workingAsType is WorkingAs.Client)
            // {
            //     hour += _TimerUIFix.Value;
            // }
        
            string time = $"{days:D2}:{hour:D2}:{minutes:D2}:{seconds:D2}";
            if (days == 0) time = $"{hour:D2}:{minutes:D2}:{seconds:D2}";
        
            Transform? parent = tmp_Text.gameObject.transform.parent;
            parent.Find("Background/CountdownBG").gameObject.SetActive(_CounterVisible.Value is SeasonalityPlugin.Toggle.On);
            parent.Find("Background/Countdown").gameObject.SetActive(_CounterVisible.Value is SeasonalityPlugin.Toggle.On);
            tmp_Text.gameObject.SetActive(_CounterVisible.Value is SeasonalityPlugin.Toggle.On);
            tmp_Text.text = time;
        
            if (workingAsType is WorkingAs.Client)
            {
                // If user is a client connected to a server, then do not set seasons
                // Wait for server to change config value
                return;
            }
        
            if (TimeDifference < TimeSpan.Zero)
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
                _LastSavedSeasonChange.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }
            else if (_Season.Value != (Season)SeasonIndex)
            {
                // To switch it back to timer settings if configs changed
                _Season.Value = (Season)SeasonIndex;
            }
            return;
        }
    }

    private static void OldTimer()
    {
        var duration = (_SeasonDurationDays.Value * 24 * 60) + (_SeasonDurationHours.Value * 60) + (_SeasonDurationMinutes.Value);
        int remainingDays = duration - (EnvMan.instance.GetCurrentDay() % duration) + 1;
        float fraction = EnvMan.instance.GetDayFraction(); // value between 0 - 1 - time of day
        float remainder = remainingDays - fraction;

        // Convert to in-game time
        int totalMinutes = (int)(remainder * 24 * 60);
        int hours = remainingDays - 2;
        int minutes = totalMinutes % (24 * 60) / 60;
        int seconds = totalMinutes % 60;
        
        string time = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
    private static void NewTimer(TMP_Text timer)
    {
        TimeSpan TimeDifference = DateTime.Parse(_LastSavedSeasonChange.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal) 
                                  + TimeSpan.FromDays(_SeasonDurationDays.Value) 
                                  + TimeSpan.FromHours(_SeasonDurationHours.Value) 
                                  + TimeSpan.FromMinutes(_SeasonDurationMinutes.Value) 
                                  - DateTime.UtcNow;
        
        int days = TimeDifference.Days;
        int hour = TimeDifference.Hours;
        int minutes = TimeDifference.Minutes;
        int seconds = TimeDifference.Seconds;

        // if (workingAsType is WorkingAs.Client)
        // {
        //     hour += _TimerUIFix.Value;
        // }
        
        string time = $"{days:D2}:{hour:D2}:{minutes:D2}:{seconds:D2}";
        if (days == 0) time = $"{hour:D2}:{minutes:D2}:{seconds:D2}";
        
        timer.gameObject.SetActive(_CounterVisible.Value is SeasonalityPlugin.Toggle.On);
        timer.text = time;
        
        if (workingAsType is WorkingAs.Client)
        {
            // If user is a client connected to a server, then do not set seasons
            // Wait for server to change config value

            return;
        }

        if (TimeDifference < TimeSpan.Zero)
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
            _LastSavedSeasonChange.Value = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
        }
        else if (_Season.Value != (Season)SeasonIndex)
        {
            // To switch it back to timer settings if configs changed
            _Season.Value = (Season)SeasonIndex;
        }
    }

    private static SeasonalityPlugin.Toggle lastToggled = _ModEnabled.Value;
    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    static class PlayerUpdatePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance.IsPlayer() || !__instance) return;
            if (_ModEnabled.Value is SeasonalityPlugin.Toggle.Off)
            {
                // Make sure this code is only called once and stops updating until mod is re-enabled
                if (lastToggled is SeasonalityPlugin.Toggle.Off) return;
                SEMan? SEMan = __instance.GetSEMan();
                if (SEMan == null) return;
                // Make sure to remove status effect when user disables mod
                StatusEffect? currentEffect = SEMan.GetStatusEffects().Find(effect => effect is SeasonEffect);
                if (currentEffect) SEMan.RemoveStatusEffect(currentEffect);
                TerrainPatch.UpdateTerrain();
                Vegetation.ResetBaseVegetation();

                lastToggled = SeasonalityPlugin.Toggle.Off;
                return;
            }

            if (_ModEnabled.Value is SeasonalityPlugin.Toggle.On && lastToggled is SeasonalityPlugin.Toggle.Off)
            {
                // Make sure when mod is re-enabled, that the seasonal effects are re-applied
                ApplySeasonalEffects(__instance);
                SetSeasonalKey();
                TerrainPatch.UpdateTerrain();
                Vegetation.ModifyBaseVegetation();
                lastToggled = SeasonalityPlugin.Toggle.On;
            }
            if (currentSeason == _Season.Value) return;
            // If season has changed, apply new seasonal effect
            TerrainPatch.UpdateTerrain();
            ApplySeasonalEffects(__instance);
            SetSeasonalKey();
            Vegetation.ModifyBaseVegetation();
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
            if (_ModEnabled.Value is SeasonalityPlugin.Toggle.Off) return;

            ApplySeasonalEffects(__instance);
            SetSeasonalKey();
            TerrainPatch.UpdateTerrain();
            Vegetation.ModifyBaseVegetation();
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
        SeasonalityPlugin.Toggle toggle = _SeasonalEffectsEnabled.Value;
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
                FallSeasonEffect.damageMod = toggle is SeasonalityPlugin.Toggle.On ? $"{_FallResistanceElement.Value}={_FallResistanceMod.Value}" : "";
                FallSeasonEffect.Modifier = toggle is SeasonalityPlugin.Toggle.On ? _FallModifier.Value : Modifier.None;
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
                SpringSeasonEffect.damageMod = toggle is SeasonalityPlugin.Toggle.On
                    ? $"{_SpringResistanceElement.Value}={_SpringResistanceMod.Value}"
                    : "";
                SpringSeasonEffect.Modifier = toggle is SeasonalityPlugin.Toggle.On ? _SpringModifier.Value : Modifier.None;
                SpringSeasonEffect.m_newValue = toggle is SeasonalityPlugin.Toggle.On ? _SpringValue.Value : 0;

                StatusEffect? SpringEffect = SpringSeasonEffect.Init();
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
                SummerSeasonEffect.Modifier = toggle is SeasonalityPlugin.Toggle.On ? _SummerModifier.Value : Modifier.None;
                SummerSeasonEffect.m_newValue = _SummerValue.Value;
                SummerSeasonEffect.damageMod = toggle is SeasonalityPlugin.Toggle.On ? $"{ _SummerResistanceElement.Value}={_SummerResistanceMod.Value}" : "";

                StatusEffect? SummerEffect = SummerSeasonEffect.Init();
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