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

    private static int SeasonIndex = 0;
    
    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateStatusEffects))]
    static class HudUpdateStatusEffectPatch
    {
        private static void Postfix(Hud __instance, List<StatusEffect> statusEffects)
        {
            if (!__instance) return;
            for (int i = 0; i < statusEffects.Count; ++i)
            {
                StatusEffect? statusEffect = statusEffects[i];
                if (!statusEffect) continue;
                if (!SeasonEffectList.Exists(x => x.name == statusEffect.name)) continue;
                RectTransform? rectTransform = __instance.m_statusEffects[i];
                if (!rectTransform) continue;
                Transform? timeText = rectTransform.Find("TimeText");
                if (!timeText) continue;
                timeText.TryGetComponent(out TMP_Text tmpText);
                if (!tmpText) continue;
                if (_SeasonDuration.Value == 0)
                {
                    tmpText.gameObject.SetActive(false);
                    return;
                }
                int remainingDays = (EnvMan.instance.GetCurrentDay() % _SeasonDuration.Value) + 1;
                float fraction = EnvMan.instance.GetDayFraction(); // value between 0 - 1
                float remainder = remainingDays - fraction;
                int totalMinutes = (int)(remainder * 24 * 60);
                int day = totalMinutes / (24 * 60);
                int hour = (totalMinutes % (24 * 60)) / 60;
                int minute = totalMinutes % 60;
                string time = $"{day:D2}:{hour:D2}:{minute:D2}";
                tmpText.gameObject.SetActive(true);
                tmpText.text = time;

                if (remainder == 0)
                {
                    _Season.Value = (Season)SeasonIndex;
                    if (SeasonIndex >= Enum.GetValues(typeof(Season)).Length)
                    {
                        SeasonIndex = 0;
                    }
                    else
                    {
                        ++SeasonIndex;
                    }
                }
                
                if (_Season.Value != (Season)SeasonIndex)
                {
                    _Season.Value = (Season)SeasonIndex;
                }
                
            }
        }
    }


    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    static class PlayerUpdatePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance.IsPlayer() || !__instance) return;
            if (currentSeason == _Season.Value) return;
            ApplySeasonalEffects(__instance);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    static class PlayerAwakePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance.IsPlayer() || !__instance) return;
            if (!ZNetScene.instance) return;
            ApplySeasonalEffects(__instance);
        }
    }

    private static void ApplySeasonalEffects(Player __instance)
    {
        var toggle = _SeasonalEffectsEnabled.Value;
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
                FallSeasonEffect.displayName = "Autumn";
                FallSeasonEffect.sprite = ValknutIcon;
                FallSeasonEffect.startEffectNames = new[] { "fx_DvergerMage_Support_start" };
                FallSeasonEffect.stopEffectNames = new[] { "fx_DvergerMage_Mistile_die" };
                FallSeasonEffect.startMsg = "Fall is upon us";
                FallSeasonEffect.effectTooltip = toggle is Toggle.On ? "The ground is wet.\nMovement speed reduced by <color=orange>10</color>%" : "The ground is wet";
                // FallSeasonEffect.damageMod = "Fire = Resistant";
                FallSeasonEffect.Modifier = toggle is Toggle.On ? Modifier.Speed : Modifier.None;
                FallSeasonEffect.m_newValue = 0.9f;
                FallSeasonEffect.duration = 0;
                
                StatusEffect? FallEffect = FallSeasonEffect.Init();
                if (FallEffect)
                {
                    SeasonEffect = FallEffect;
                }
                break;
            case Season.Spring:
                SeasonalEffect SpringSeasonEffect = new SeasonalEffect();
                SpringSeasonEffect.effectName = "spring_season";
                SpringSeasonEffect.displayName = "Spring";
                SpringSeasonEffect.sprite = ValknutIcon;
                SpringSeasonEffect.startEffectNames = new[] { "fx_DvergerMage_Support_start" };
                SpringSeasonEffect.stopEffectNames = new[] { "fx_DvergerMage_Mistile_die" };
                SpringSeasonEffect.startMsg = "Spring has finally arrived";
                SpringSeasonEffect.effectTooltip = toggle is Toggle.On 
                    ? "The land is bursting with energy.\nMovement speed increased by <color=orange>10</color>%"
                    : "The land is bursting with energy.";
                // SpringSeasonEffect.damageMod = "Fire = Resistant";
                SpringSeasonEffect.Modifier = toggle is Toggle.On ? Modifier.Speed : Modifier.None;
                SpringSeasonEffect.duration = 0;
                SpringSeasonEffect.m_newValue = 1.1f;

                StatusEffect? SpringEffect = SpringSeasonEffect.Init();
                if (SpringEffect) SeasonEffect = SpringEffect;
                break;
            case Season.Winter:
                SeasonalEffect WinterSeasonEffect = new SeasonalEffect();
                WinterSeasonEffect.effectName = "winter_season";
                WinterSeasonEffect.displayName = "Winter";
                WinterSeasonEffect.sprite = ValknutIcon;
                WinterSeasonEffect.startEffectNames = new[] { "fx_DvergerMage_Support_start" };
                WinterSeasonEffect.stopEffectNames = new[] { "fx_DvergerMage_Mistile_die" };
                WinterSeasonEffect.startMsg = "Winter is coming";
                WinterSeasonEffect.effectTooltip = toggle is Toggle.On 
                    ? "The air is cold.\nHealth regeneration reduced by <color=orange>10</color>%\n<color=orange>Resistant</color> VS <color=orange>Fire</color>"
                    : "The air is cold.";
                WinterSeasonEffect.damageMod = toggle is Toggle.On ? "Fire = Resistant" : "";
                WinterSeasonEffect.Modifier = toggle is Toggle.On ? Modifier.HealthRegen : Modifier.None;
                WinterSeasonEffect.duration = 0;
                WinterSeasonEffect.m_newValue = 0.9f;

                StatusEffect? WinterEffect = WinterSeasonEffect.Init();
                if (WinterEffect) SeasonEffect = WinterEffect;
                break;
            case Season.Summer:
                SeasonalEffect SummerSeasonEffect = new SeasonalEffect();
                SummerSeasonEffect.effectName = "summer_season";
                SummerSeasonEffect.displayName = "Summer";
                SummerSeasonEffect.sprite = ValknutIcon;
                SummerSeasonEffect.startEffectNames = new[] { "fx_DvergerMage_Support_start" };
                SummerSeasonEffect.stopEffectNames = new[] { "fx_DvergerMage_Mistile_die" };
                SummerSeasonEffect.startMsg = "Summer has arrived";
                SummerSeasonEffect.effectTooltip = toggle is Toggle.On 
                    ? "The air is warm.\nIncrease carry weight by <color=orange>100</color>"
                    : "The air is warm.";
                SummerSeasonEffect.Modifier = toggle is Toggle.On ? Modifier.MaxCarryWeight : Modifier.None;
                SummerSeasonEffect.duration = 0;
                SummerSeasonEffect.m_newValue = 100f;

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