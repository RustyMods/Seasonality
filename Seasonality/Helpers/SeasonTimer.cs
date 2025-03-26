using System;
using HarmonyLib;
using UnityEngine;

namespace Seasonality.Helpers;

public static class SeasonTimer
{
    private static float m_seasonTimer;
    public static bool m_sleepOverride;
    public static bool ValidServer;

    private static double GetTimeElapsed()
    {
        if (!ZNet.instance) return 0;
        if (Configs.m_lastSeasonChange.Value > ZNet.instance.GetTimeSeconds()) Configs.m_lastSeasonChange.Value = 0;
        var timeElapsed = ZNet.m_instance.GetTimeSeconds() - Configs.m_lastSeasonChange.Value;
        return timeElapsed < 0 ? 0 : timeElapsed;
    }

    private static void SetLastSeasonChangeTime()
    {
        double time = ZNet.m_instance.GetTimeSeconds();
        Configs.m_lastSeasonChange.Value = time;
    }
    
    private static double GetSeasonLength()
    {
        var vector = Configs.m_durations.GetOrDefault(Configs.m_season.Value, Vector3.zero);
        double days = vector.x * 86400;
        double hours = vector.y * 3600;
        double minutes = vector.z * 60;
        return days + hours + minutes;
    }

    public static double GetTimeDifference()
    {
        double difference = GetSeasonLength() - GetTimeElapsed();
        return difference < 0 ? 0 : difference;
    }

    private static bool ShouldCount() => GetSeasonLength() > 0;

    private static double GetCountdown() => TimeSpan.FromSeconds(GetTimeDifference()).TotalSeconds;
    
    public static Configs.Season GetNextSeason(Configs.Season season)
    {
        int currentSeason = (int)season;
        Configs.Season nextSeason;

        if (Enum.IsDefined(typeof(Configs.Season), currentSeason + 1))
            nextSeason = (Configs.Season)currentSeason + 1;
        else 
            nextSeason = 0;

        return nextSeason;
    }

    private static void CheckSeasonFade(double timer)
    {
        if (Configs.m_seasonFades.Value is Configs.Toggle.Off) return;
        if (!Player.m_localPlayer || !ZNet.instance || FadeToBlack.m_blackScreenImg is null || FadeToBlack.m_blackScreen is null) return;
        if (timer > Configs.m_fadeLength.Value) return;
        if (ZNet.instance.GetTimeSeconds() - FadeToBlack.m_timeLastFade < 50f || FadeToBlack.m_fading) return;
        SeasonalityPlugin._plugin.StartCoroutine(FadeToBlack.TriggerFade());
    }
    
    public static void CheckTransition(float dt)
    {
        if (!ZNet.instance || !EnvMan.instance || ZNet.m_world == null || !ShouldCount()) return;
        m_seasonTimer += dt;
        if (m_seasonTimer < 1f) return;
        m_seasonTimer = 0.0f;
        
        int countdown = (int)GetCountdown();
        if (Configs.m_sleepOverride.Value is Configs.Toggle.On)
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
        Configs.m_season.Value = GetNextSeason(Configs.m_season.Value);
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