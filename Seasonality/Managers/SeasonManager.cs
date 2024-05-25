using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Seasons;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Managers;

public static class SeasonManager
{
    public static bool m_fading;
    private static readonly int se_seasons = "SE_Seasons".GetStableHashCode();
    private static IEnumerator TriggerFade()
    {
        float duration = 0f;
        float length = SeasonalityPlugin._FadeLength.Value * 50f;
        float alpha = 0f;
        m_fading = true;
        
        while (duration < length)
        {
            alpha += 1f / length;
            HudManager.m_seasonBlackScreen.color = new Color(0f, 0f, 0f, alpha);
            ++duration;
            yield return new WaitForFixedUpdate(); // 50 times per second
        }

        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_{GetNextSeason().ToString().ToLower()}");
        yield return new WaitForSeconds(1);
        while (duration > 0)
        {
            alpha -= 1f / length;
            HudManager.m_seasonBlackScreen.color = new Color(0f, 0f, 0f, alpha);
            --duration;
            yield return new WaitForFixedUpdate();
        }

        m_fading = false;
    }

    private static float m_seasonEffectTimer;
    
    public static void UpdateSeasonEffects(float dt)
    {
        if (!Player.m_localPlayer) return;

        m_seasonEffectTimer += dt;
        if (m_seasonEffectTimer < 1f) return;
        m_seasonEffectTimer = 0.0f;
        
        var man = Player.m_localPlayer.GetSEMan();
        if (!man.HaveStatusEffect(se_seasons))
        {
            man.AddStatusEffect(se_seasons);
        }
        if (!man.HaveStatusEffect("SE_Weatherman".GetStableHashCode()))
        {
            if (SeasonalityPlugin._EnableWeather.Value is SeasonalityPlugin.Toggle.Off) return;
            man.AddStatusEffect("SE_Weatherman".GetStableHashCode());
        }
    }
    
    public static void OnSeasonConfigChange(object sender, EventArgs e)
    {
        if (sender is not ConfigEntry<SeasonalityPlugin.Season> config) return;
        ChangeSeason();
    }
    
    public static void ChangeSeason()
    {
        ClutterManager.UpdateClutter();
        TerrainManager.UpdateTerrain();
        GlobalKeyManager.UpdateSeasonalKey();
        MaterialReplacer.ModifyCachedMaterials(SeasonalityPlugin._Season.Value);
        --EnvMan.instance.m_environmentPeriod;
    }

    private static bool ShouldCount() => GetTotalSeconds() > 0;

    private static SeasonalityPlugin.Season GetNextSeason()
    {
        // int currentSeason = (int)SeasonalityPlugin._Season.Value;
        // if (Enum.IsDefined(typeof(SeasonalityPlugin.Season), currentSeason + 1))
        // {
        //     return (SeasonalityPlugin.Season)currentSeason + 1;
        // }
        // return 0;
        
        return SeasonalityPlugin._Season.Value switch
        {
            SeasonalityPlugin.Season.Spring => SeasonalityPlugin.Season.Summer,
            SeasonalityPlugin.Season.Summer => SeasonalityPlugin.Season.Fall,
            SeasonalityPlugin.Season.Fall => SeasonalityPlugin.Season.Winter,
            SeasonalityPlugin.Season.Winter => SeasonalityPlugin.Season.Spring,
            _ => SeasonalityPlugin.Season.Spring
        };
    }

    private static float m_seasonTimer;
    private static readonly float m_threshold = 0.5f;
    private static bool m_sleepOverride;
    public static void UpdateSeason(float dt)
    {
        if (!ZNet.instance || !EnvMan.instance) return;
        if (!ShouldCount()) return;

        m_seasonTimer += dt;
        if (m_seasonTimer < m_threshold) return;
        m_seasonTimer = 0.0f;
        double timer = GetSeasonFraction();

        if (SeasonalityPlugin._SleepOverride.Value is SeasonalityPlugin.Toggle.On)
        {
            if (timer > m_threshold) return;
            if (!m_sleepOverride) m_sleepOverride = true;
            return;
        }
        
        CheckSeasonFade(timer);
        ChangeSeason(timer);
    }

    private static void ChangeSeason(double timer)
    {
        if (timer > m_threshold || !ZNet.instance.IsServer()) return;
        SeasonalityPlugin._Season.Value = GetNextSeason();
    }

    private static void CheckSeasonFade(double timer)
    {
        if (SeasonalityPlugin._SeasonFades.Value is SeasonalityPlugin.Toggle.Off) return;
        if (timer - m_threshold > SeasonalityPlugin._FadeLength.Value) return;
        if (!m_fading && Player.m_localPlayer) SeasonalityPlugin._plugin.StartCoroutine(TriggerFade());
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.SkipToMorning))]
    private static class SleepOverride_SeasonChange
    {
        private static void Postfix()
        {
            if (m_sleepOverride && ZNet.instance.IsServer())
            {
                SeasonalityPlugin._Season.Value = GetNextSeason();
                m_sleepOverride = false;
            }
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix() => RegisterSeasonSE();
    }
    
    private static Sprite? GetIcon()
    {
        return SeasonalityPlugin._Season.Value switch
        {
            SeasonalityPlugin.Season.Spring => SpriteManager.SpringIcon,
            SeasonalityPlugin.Season.Summer => SpriteManager.SummerIcon,
            SeasonalityPlugin.Season.Fall => SpriteManager.FallIcon,
            SeasonalityPlugin.Season.Winter => SpriteManager.WinterIcon,
            _ => SpriteManager.ValknutIcon
        };
    }

    private static void RegisterSeasonSE()
    {
        if (!ObjectDB.instance || !ZNetScene.instance) return;
        SE_Season effect = ScriptableObject.CreateInstance<SE_Season>();
        effect.name = "SE_Seasons";
        effect.m_name = "Seasonality";
        effect.m_icon = GetIcon();
        if (ObjectDB.instance.m_StatusEffects.Contains(effect)) return;
        ObjectDB.instance.m_StatusEffects.Add(effect);
    }

    private static double GetTotalSeconds()
    {
        if (!SeasonalityPlugin._Durations.TryGetValue(SeasonalityPlugin._Season.Value, out Dictionary<SeasonalityPlugin.DurationType, ConfigEntry<int>> configs)) return 0;
        double days = configs[SeasonalityPlugin.DurationType.Day].Value * 86400;
        double hours = configs[SeasonalityPlugin.DurationType.Hours].Value * 3600;
        double minutes = configs[SeasonalityPlugin.DurationType.Minutes].Value * 60;
        return days + hours + minutes;
    }
    
    private static double GetSeasonFraction() => GetTotalSeconds() - (EnvMan.instance.m_totalSeconds % GetTotalSeconds());
    

    public static void OnSeasonDisplayConfigChange(object sender, EventArgs e)
    {
        if (sender is not ConfigEntry<SeasonalityPlugin.Toggle> config) return;
        if (config.Value is SeasonalityPlugin.Toggle.Off) return;
        Player.m_localPlayer.GetSEMan().RemoveStatusEffect(se_seasons);
        Player.m_localPlayer.GetSEMan().AddStatusEffect(se_seasons);
    }
    
    private static string GetSeasonTime()
    {
        if (!ShouldCount()) return "";
        TimeSpan span = TimeSpan.FromSeconds(GetSeasonFraction());
        int days = span.Days;
        int hour = span.Hours;
        int minutes = span.Minutes;
        int seconds = span.Seconds;

        if (m_sleepOverride) return Localization.instance.Localize("$msg_ready");
            
        return days > 0 ? $"{days}:{hour:D2}:{minutes:D2}:{seconds:D2}" : hour > 0 ? $"{hour}:{minutes:D2}:{seconds:D2}" : minutes > 0 ? $"{minutes}:{seconds:D2}" : $"{seconds}";
    }

    public class SE_Season : StatusEffect
    {
        public override void OnDamaged(HitData hit, Character attacker)
        {
            if (m_fading) hit.ApplyModifier(0f);
        }

        public override string GetTooltipString()
        {
            return m_tooltip + Localization.instance.Localize($"$season_{SeasonalityPlugin._Season.Value.ToString().ToLower()}_tooltip");
        }

        public override string GetIconText()
        {
            if (!EnvMan.instance) return "";
            m_name = Localization.instance.Localize($"$season_{SeasonalityPlugin._Season.Value.ToString().ToLower()}");
            m_icon = SeasonalityPlugin._DisplaySeason.Value is SeasonalityPlugin.Toggle.On
                ? GetIcon()
                : null;
            return SeasonalityPlugin._DisplaySeasonTimer.Value is SeasonalityPlugin.Toggle.Off ? "" : GetSeasonTime();
        }
    }
}