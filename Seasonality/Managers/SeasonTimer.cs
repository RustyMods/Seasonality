using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Seasonality.Managers;

public static class SeasonTimer
{
    private static readonly string m_filePath = SeasonalityPaths.SeasonPaths.folderPath + Path.DirectorySeparatorChar + "SeasonTimeData.yml";
    private static float m_seasonTimer;
    private static readonly float m_threshold = 3f;
    public static bool m_sleepOverride;
    public static bool m_fading;
    private static double m_lastSeasonChange;
    private static double GetLastSeasonChangeTime()
    {
        if (!File.Exists(m_filePath)) return 0;

        string worldName = ZNet.m_world.m_name;
        try
        {
            var data = File.ReadAllLines(m_filePath);

            foreach (var line in data)
            {
                string[] info = line.Split(':');
                string world = info[0];
                string date = info[1];

                if (world != worldName) continue;

                return double.TryParse(date, out double output) ? output : 0;
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static double GetTimeElapsed()
    {
        if (m_lastSeasonChange == 0)
        {
            m_lastSeasonChange = GetLastSeasonChangeTime();
        }

        var timeElapsed = ZNet.m_instance.GetTimeSeconds() - m_lastSeasonChange;
        return timeElapsed < 0 ? 0 : timeElapsed;
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    private static class Game_Logout_Patch
    {
        private static void Postfix()
        {
            m_lastSeasonChange = 0;
            SeasonManager.m_firstSpawn = true;
        }
    }

    public static void SaveTimeChange()
    {
        if (ZNet.m_world == null || !ZNet.m_instance) return;
        double time = ZNet.m_instance.GetTimeSeconds();
        string worldName = ZNet.m_world.m_name;
        m_lastSeasonChange = time;
        string format = worldName + ":" + time;
        if (!File.Exists(m_filePath))
        {
            File.WriteAllText(m_filePath, format);
        }
        else
        {
            var data = File.ReadAllLines(m_filePath);
            bool updated = false;
            List<string> newData = new();
            foreach (var line in data)
            {
                string[] info = line.Split(':');
                string world = info[0];

                if (world != worldName)
                {
                    newData.Add(line);
                    continue;
                }
                newData.Add(format);
                updated = true;
            }

            if (!updated)
            {
                newData.Add(format);
            }
            File.WriteAllLines(m_filePath, newData);
        }
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

        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"$msg_{GetNextSeason(SeasonalityPlugin._Season.Value).ToString().ToLower()}");
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
    
    private static void CheckSeasonFade(double timer)
    {
        if (SeasonalityPlugin._SeasonFades.Value is SeasonalityPlugin.Toggle.Off) return;
        if (timer - m_threshold > SeasonalityPlugin._FadeLength.Value / 2) return;
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
            if (m_sleepOverride || EnvMan.instance.IsTimeSkipping()) m_lastSeasonChange = ZNet.m_instance.GetTimeSeconds();
            if (countdown > 0 || m_sleepOverride) return;
            if (!m_sleepOverride && !EnvMan.instance.IsTimeSkipping()) m_sleepOverride = true;
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

    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.SkipToMorning))]
    private static class SleepOverride_SeasonChange
    {
        private static void Postfix()
        {
            if (!m_sleepOverride) return;
            m_sleepOverride = false;
            ChangeSeason();
        }
    
        private static void Prefix()
        {
            if (SeasonalityPlugin._SleepOverride.Value is SeasonalityPlugin.Toggle.On) return;
            if ((int)GetCountdown() >= 900) return;
            SeasonalityPlugin._Season.Value = GetNextSeason(SeasonalityPlugin._Season.Value);
        }
    }
    
}