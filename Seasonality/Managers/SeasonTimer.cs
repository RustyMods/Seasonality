using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace Seasonality.Managers;

public static class SeasonTimer
{
    private static float m_seasonTimer;
    public static bool m_sleepOverride;
    public static bool m_fading;

    private static double GetTimeElapsed()
    {
        var timeElapsed = ZNet.m_instance.GetTimeSeconds() - SeasonalityPlugin.m_lastSeasonChange.Value;
        return timeElapsed < 0 ? 0 : timeElapsed;
    }
    public static void SaveTimeChange()
    {
        if (!ZNet.m_instance) return;
        double time = ZNet.m_instance.GetTimeSeconds();
        SeasonalityPlugin.m_lastSeasonChange.Value = time;
    }
    
    private static double GetSeasonLength()
    {
        if (!SeasonalityPlugin._Durations.TryGetValue(SeasonalityPlugin._Season.Value, out Dictionary<SeasonalityPlugin.DurationType, ConfigEntry<int>> configs)) return 0;
        double days = configs[SeasonalityPlugin.DurationType.Day].Value * 86400;
        double hours = configs[SeasonalityPlugin.DurationType.Hours].Value * 3600;
        double minutes = configs[SeasonalityPlugin.DurationType.Minutes].Value * 60;
        return days + hours + minutes;
    }

    public static double GetTimeDifference()
    {
        double difference = GetSeasonLength() - GetTimeElapsed();
        return difference < 0 ? 0 : difference;
    }

    private static bool ShouldCount() => GetSeasonLength() > 0;

    private static double GetCountdown() => TimeSpan.FromSeconds(GetTimeDifference()).TotalSeconds;
    
    private static SeasonalityPlugin.Season GetNextSeason(SeasonalityPlugin.Season season)
    {
        int currentSeason = (int)season;
        SeasonalityPlugin.Season nextSeason;

        if (Enum.IsDefined(typeof(SeasonalityPlugin.Season), currentSeason + 1))
            nextSeason = (SeasonalityPlugin.Season)currentSeason + 1;
        else 
            nextSeason = 0;

        return nextSeason;
    }
    
    private static IEnumerator TriggerFade()
    {
        if (m_fading || HudManager.m_seasonScreen is null || HudManager.m_seasonBlackScreen is null) yield break; // Prevent duplicate coroutines
        m_fading = true;
        try
        {
            float duration = 0f;
            float length = Mathf.Max(SeasonalityPlugin._FadeLength.Value * 50f, 1f); // Avoid zero length
            float alpha = 0f;

            // Fade to black
            while (duration < length)
            {
                alpha += 1f / length;
                HudManager.m_seasonBlackScreen.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
                duration++;
                yield return new WaitForFixedUpdate();
            }

            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_{GetNextSeason(SeasonalityPlugin._Season.Value).ToString().ToLower()}");

            yield return new WaitForSeconds(1);

            // Fade back to normal
            while (duration > 0)
            {
                alpha -= 1f / length;
                HudManager.m_seasonBlackScreen.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
                duration--;
                yield return new WaitForFixedUpdate();
            }
        }
        finally
        {
            m_fading = false; // Ensure flag resets even on failure
        }
    }
    
    private static void CheckSeasonFade(double timer)
    {
        if (SeasonalityPlugin._SeasonFades.Value is SeasonalityPlugin.Toggle.Off) return;
        if (!Player.m_localPlayer || !ZNet.instance) return;
        if (HudManager.m_seasonBlackScreen is null || HudManager.m_seasonScreen is null) return;
        if (timer > SeasonalityPlugin._FadeLength.Value) return;
        if (!m_fading && Player.m_localPlayer) SeasonalityPlugin._plugin.StartCoroutine(TriggerFade());
    }
    
    public static void CheckSeasonTransition(float dt)
    {
        if (!ZNet.instance || !EnvMan.instance || ZNet.m_world == null) return;
        if (!ShouldCount()) return;
        m_seasonTimer += dt;
        if (m_seasonTimer < 1f) return;
        m_seasonTimer = 0.0f;
        
        int countdown = (int)GetCountdown();
        if (SeasonalityPlugin._SleepOverride.Value is SeasonalityPlugin.Toggle.On)
        {
            if (countdown > 0) return;
            if (m_sleepOverride) return;
            m_sleepOverride = true;
        }
        else
        {
            CheckSeasonFade(countdown);
            if (countdown > 0) return;
            ChangeSeason();
        }
    }

    private static void ChangeSeason()
    {
        if (!ZNet.instance.IsServer()) return;
        SeasonalityPlugin._Season.Value = GetNextSeason(SeasonalityPlugin._Season.Value);
    }


    [HarmonyPatch(typeof(Game), nameof(Game.SleepStop))]
    private static class Game_SleepStop_Patch
    {
        private static void Postfix()
        {
            if (!m_sleepOverride) return;
            m_sleepOverride = false;
            ChangeSeason();
            SaveTimeChange();
        }
    }
    
}