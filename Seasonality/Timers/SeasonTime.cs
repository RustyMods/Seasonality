using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Seasonality;

public static class SeasonTime
{
    public static double m_lastSeasonChange;
    public static string? lastSeasonChangeFileName;
    private static readonly string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    private static readonly string BinFolder = Path.Combine(ConfigFolder, "LastSeasonChangeData");
    public static Season m_currentSeason = Season.Summer;
    public static bool m_sleepOverride => Configs.m_sleepOverride.Value is Toggle.On;

    public static Season GetCurrentSeason()
    {
        return m_currentSeason;
    }

    public static void Read()
    {
        if (lastSeasonChangeFileName is null) return;
        var filePath = Path.Combine(BinFolder, lastSeasonChangeFileName);
        if (!File.Exists(filePath)) return;
        try
        {
            using var reader = new BinaryReader(File.OpenRead(filePath));
            m_lastSeasonChange = reader.ReadDouble();
            SeasonalityPlugin.Record.LogDebug(
                $"Reading last season change from, {Path.GetFileNameWithoutExtension(filePath)}: {m_lastSeasonChange}");
            if (m_lastSeasonChange > ZNet.instance.GetTimeSeconds())
            {
                SeasonalityPlugin.Record.LogDebug(
                    $"Last season change time {m_lastSeasonChange} is larger than world time: {ZNet.instance.GetTimeSeconds()}, setting last season change to world time");
                m_lastSeasonChange = ZNet.instance.GetTimeSeconds();
            }
        }
        catch (Exception ex)
        {
            SeasonalityPlugin.Record.LogError(ex.Message);
        }
    }

    public static void Save()
    {
        if (lastSeasonChangeFileName is null) return;
        var filePath = Path.Combine(BinFolder, lastSeasonChangeFileName);
        try
        {
            using var writer = new BinaryWriter(File.Create(filePath));
            writer.Write(m_lastSeasonChange);
            SeasonalityPlugin.Record.LogDebug(
                $"Saving last season change to, {Path.GetFileNameWithoutExtension(filePath)}, {m_lastSeasonChange}");
        }
        catch (Exception ex)
        {
            SeasonalityPlugin.Record.LogError(ex.Message);
        }
    }

    private static void UpdateSeasonTime(float dt)
    {
        if (m_sleepOverride) return;

        var length = GetSeasonLength();
        if (length <= 0) return;
        
        var time = GetTimeDifference();
        CheckSeasonFade(time);
        if (time <= 0)
        {
            m_lastSeasonChange = ZNet.instance.GetTimeSeconds();
            m_currentSeason = GetNextSeason();
            SeasonalityPlugin.OnSeasonChange();

            if (ZNet.instance.IsServer())
            {
                Configs.m_season.Value = m_currentSeason;
            }
        }
    }

    private static void UpdateSleepTime(Hud __instance)
    {
        if (!m_sleepOverride) return;
        
        var length = GetSeasonLength();
        if (length <= 0) return;
        
        if (!__instance.m_sleepingProgress.activeSelf || __instance.m_loadingScreen.alpha < 0.8f) return;
        var time = GetTimeDifference();
        if (time <= 0)
        {
            m_lastSeasonChange = ZNet.instance.GetTimeSeconds();
            m_currentSeason = GetNextSeason();
            SeasonalityPlugin.OnSeasonChange();

            if (ZNet.instance.IsServer())
            {
                Configs.m_season.Value = m_currentSeason;
            }
        }
    }

    public static void OnSeasonConfigChange(object sender, EventArgs args)
    {
        if (m_currentSeason != Configs.m_season.Value)
        {
            if (ZNet.instance)
            {
                m_lastSeasonChange = ZNet.instance.GetTimeSeconds();
            }
            m_currentSeason = Configs.m_season.Value;
            SeasonalityPlugin.OnSeasonChange();
        }
    }

    private static void CheckSeasonFade(double timer)
    {
        if (Configs.m_seasonFades.Value is Toggle.Off) return;
        if (!Player.m_localPlayer || !ZNet.instance || FadeToBlack.m_blackScreenImg is null ||
            FadeToBlack.m_blackScreen is null) return;
        if (timer > Configs.m_fadeLength.Value) return;
        if (ZNet.instance.GetTimeSeconds() - FadeToBlack.m_timeLastFade < 40f || FadeToBlack.m_fading) return;
        SeasonalityPlugin.instance.StartCoroutine(FadeToBlack.TriggerFade());
    }

    public static Season GetNextSeason()
    {
        return m_currentSeason switch
        {
            Season.Spring => Season.Summer,
            Season.Summer => Season.Fall,
            Season.Fall => Season.Winter,
            Season.Winter => Season.Spring,
            _ => Season.Summer
        };
    }

    public static double GetSeasonLength()
    {
        var vector = Configs.m_durations.GetOrDefault(m_currentSeason, Vector3.zero);
        double days = vector.x * 86400;
        double hours = vector.y * 3600;
        double minutes = vector.z * 60;
        return days + hours + minutes;
    }

    public static double GetTimeDifference()
    {
        var difference = GetSeasonLength() - GetTimeElapsed();
        return difference < 0 ? 0 : difference;
    }

    public static double GetTimeElapsed()
    {
        if (!ZNet.instance) return 0;

        var timeElapsed = ZNet.m_instance.GetTimeSeconds() - m_lastSeasonChange;

        return timeElapsed < 0 ? 0 : timeElapsed;
    }

    private static void RPC_SyncSeason(ZRpc rpc, double lastSeasonChange, int season)
    {
        m_lastSeasonChange = lastSeasonChange;
        if (Enum.IsDefined(typeof(Season), season))
        {
            var _season = (Season)season;
            if (_season != m_currentSeason)
            {
                m_currentSeason = _season;
                SeasonalityPlugin.OnSeasonChange();
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
    private static class ZNet_Start_Patch
    {
        private static void Postfix(ZNet __instance)
        {
            SeasonalityPlugin.Record.LogDebug("ZNet.Start");
            lastSeasonChangeFileName = __instance.GetWorldName() + ".Seasonality.bin";
            Read();
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Save))]
    private static class ZNet_Save_Patch
    {
        private static void Prefix()
        {
            Save();
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
    private static class RPC_PeerInfo_Patch
    {
        private static void Postfix(ZNet __instance, ZRpc rpc, ZPackage pkg)
        {
            if (ZNet.m_isServer) return;
            rpc.Register<double, int>(nameof(RPC_SyncSeason), RPC_SyncSeason);
            rpc.Invoke(nameof(RPC_SyncSeason), m_lastSeasonChange, (int)m_currentSeason);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.SendNetTime))]
    private static class ZNet_SendNetTime_Patch
    {
        private static void Postfix(ZNet __instance)
        {
            for (var i = 0; i < __instance.m_peers.Count; ++i)
            {
                var peer = __instance.m_peers[i];
                if (!peer.IsReady()) continue;
                peer.m_rpc.Invoke(nameof(RPC_SyncSeason), m_lastSeasonChange, (int)m_currentSeason);
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.UpdateNetTime))]
    private static class ZNet_UpdateNetTime_Patch
    {
        private static void Postfix(float dt)
        {
            UpdateSeasonTime(dt);
        }
    }

    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateBlackScreen))]
    private static class Hud_UpdateBlackScreen_Patch
    {
        private static void Postfix(Hud __instance, Player player, float dt)
        {
            UpdateSleepTime(__instance);
        }
    }
}