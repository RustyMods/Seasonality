using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Seasons;

namespace Seasonality.Helpers;

public static class SeasonTimer
{
    private static float m_seasonTimer;
    public static bool m_sleepOverride;
    public static bool ValidServer;

    private static double GetTimeElapsed()
    {
        if (!ZNet.instance) return 0;
        if (Configurations.m_lastSeasonChange.Value > ZNet.instance.GetTimeSeconds()) Configurations.m_lastSeasonChange.Value = 0;
        var timeElapsed = ZNet.m_instance.GetTimeSeconds() - Configurations.m_lastSeasonChange.Value;
        return timeElapsed < 0 ? 0 : timeElapsed;
    }

    private static void SetLastSeasonChangeTime()
    {
        double time = ZNet.m_instance.GetTimeSeconds();
        Configurations.m_lastSeasonChange.Value = time;
    }
    
    private static double GetSeasonLength()
    {
        if (!Configurations._Durations.TryGetValue(Configurations._Season.Value, out Dictionary<Configurations.DurationType, ConfigEntry<int>> configs)) return 0;
        double days = configs[Configurations.DurationType.Day].Value * 86400;
        double hours = configs[Configurations.DurationType.Hours].Value * 3600;
        double minutes = configs[Configurations.DurationType.Minutes].Value * 60;
        return days + hours + minutes;
    }

    public static double GetTimeDifference()
    {
        double difference = GetSeasonLength() - GetTimeElapsed();
        return difference < 0 ? 0 : difference;
    }

    private static bool ShouldCount() => GetSeasonLength() > 0;

    private static double GetCountdown() => TimeSpan.FromSeconds(GetTimeDifference()).TotalSeconds;
    
    public static SeasonalityPlugin.Season GetNextSeason(SeasonalityPlugin.Season season)
    {
        int currentSeason = (int)season;
        SeasonalityPlugin.Season nextSeason;

        if (Enum.IsDefined(typeof(SeasonalityPlugin.Season), currentSeason + 1))
            nextSeason = (SeasonalityPlugin.Season)currentSeason + 1;
        else 
            nextSeason = 0;

        return nextSeason;
    }

    private static void CheckSeasonFade(double timer)
    {
        if (Configurations._SeasonFades.Value is SeasonalityPlugin.Toggle.Off) return;
        if (!Player.m_localPlayer || !ZNet.instance || FadeToBlack.m_seasonBlackScreen is null || FadeToBlack.m_seasonScreen is null) return;
        if (timer > Configurations._FadeLength.Value) return;
        if (ZNet.instance.GetTimeSeconds() - FadeToBlack.m_timeLastFade < 50f || FadeToBlack.m_fading) return;
        SeasonalityPlugin._plugin.StartCoroutine(FadeToBlack.TriggerFade());
    }
    
    public static void CheckSeasonTransition(float dt)
    {
        if (!ZNet.instance || !EnvMan.instance || ZNet.m_world == null || !ShouldCount()) return;
        m_seasonTimer += dt;
        if (m_seasonTimer < 1f) return;
        m_seasonTimer = 0.0f;
        
        int countdown = (int)GetCountdown();
        if (Configurations._SleepOverride.Value is SeasonalityPlugin.Toggle.On)
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
        if (!ZNet.instance.IsServer() && ValidServer) return;
        Configurations._Season.Value = GetNextSeason(Configurations._Season.Value);
        SetLastSeasonChangeTime();
    }


    [HarmonyPatch(typeof(Game), nameof(Game.SleepStop))]
    private static class Game_SleepStop_Patch
    {
        private static void Postfix()
        {
            if (!m_sleepOverride) return;
            m_sleepOverride = false;
            ChangeSeason();
            SetLastSeasonChangeTime();
        }
    }
}