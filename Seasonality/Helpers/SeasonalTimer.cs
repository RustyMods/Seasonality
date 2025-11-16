using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Seasonality.Helpers;

public class SeasonalTimer : MonoBehaviour
{
   [HarmonyPatch(typeof(Game), nameof(Game.SleepStop))]
   private static class Game_SleepStop_Patch
   {
      [UsedImplicitly]
      private static void Postfix() => instance?.OnSleepStop();
   }

   [HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
   private static class ZNet_Start_Patch
   {
      [UsedImplicitly]
      private static void Postfix(ZNet __instance)
      {
         SeasonalityPlugin.Record.LogDebug("ZNet.Start");
         lastSeasonChangeFileName = __instance.GetWorldName() + ".Seasonality.bin";
         Read();
         if (!__instance.IsServer()) return;
         instance?.ScheduleNextSeason();
      }
   }

   [HarmonyPatch(typeof(ZNet), nameof(ZNet.SetNetTime))]
   private static class ZNet_SetNetTime_Patch
   {
      [UsedImplicitly]
      private static void Postfix()
      {
         if (Game.instance.m_sleeping) return;
         instance?.ResetSchedule();
      }
   }

   [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
   private static class RegisterSetLastSeasonChangeAndSet
   {
      [UsedImplicitly]
      private static void Prefix(ZNetPeer peer)
      {
         peer.m_rpc.Register<double>(nameof(RPC_SetLastSeasonChange), RPC_SetLastSeasonChange);
      }
   }

   [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
   private static class ZNet_RPC_PeerInfo_Patch
   {
      [UsedImplicitly]
      private static void Postfix(ZNet __instance, ZRpc rpc)
      {
         if (!__instance.IsServer()) return;
         rpc.Invoke(nameof(RPC_SetLastSeasonChange), lastSeasonChange);
      }
   }
   
   public static bool hasValidServer;
   public static bool m_sleepOverride;
   private static readonly string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
   private static readonly string BinFolder = Path.Combine(ConfigFolder, "LastSeasonChangeData");
   public static SeasonalTimer? instance;
   public float m_fadeTimer;
   public static string? lastSeasonChangeFileName;
   public static double lastSeasonChange;
   public void Awake()
   {
      SeasonalityPlugin.Record.LogDebug("Seasonal Timer Awake");
      instance = this;
      if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);
      if (!Directory.Exists(BinFolder)) Directory.CreateDirectory(BinFolder);
   }

   public static void Read()
   {
      if (lastSeasonChangeFileName is null) return;
      string filePath = Path.Combine(BinFolder, lastSeasonChangeFileName);
      if (!File.Exists(filePath)) return;
      try
      {
         using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));
         lastSeasonChange = reader.ReadDouble();
         SeasonalityPlugin.Record.LogDebug(
            $"Reading last season change from, {Path.GetFileNameWithoutExtension(filePath)}: {lastSeasonChange}");
      }
      catch (Exception ex)
      {
         SeasonalityPlugin.Record.LogError(ex.Message);
      }
   }

   public static void Save()
   {
      if (lastSeasonChangeFileName is null) return;
      string filePath = Path.Combine(BinFolder, lastSeasonChangeFileName);
      try
      {
         using BinaryWriter writer = new BinaryWriter(File.Create(filePath));
         writer.Write(lastSeasonChange);
         SeasonalityPlugin.Record.LogDebug(
            $"Saving last season change to, {Path.GetFileNameWithoutExtension(filePath)}, {lastSeasonChange}");
      }
      catch (Exception ex)
      {
         SeasonalityPlugin.Record.LogError(ex.Message);
      }
   }

   public void Update()
   {
      if (Configs.m_sleepOverride.Value is Toggle.On) return;
      
      m_fadeTimer += Time.deltaTime;
      if (m_fadeTimer < 1f) return;
      m_fadeTimer = 0.0f;
      
      double countdown = GetCountdown();
      CheckSeasonFade(countdown);
   }

   public void OnDestroy()
   {
      instance = null;
   }
   
   public void OnSleepStop()
   {
      if (!m_sleepOverride) return;
      SetNextSeason();
      m_sleepOverride = false;
      ScheduleNextSeason();
   }

   private void ScheduleNextSeason()
   {
      if (!ShouldCount())
      {
         SeasonalityPlugin.Record.LogDebug("Season timer set to 0, next season not scheduled");
         return;
      }
      float delay = (float)GetTimeDifference();
      Invoke(nameof(ChangeSeason), delay);
      SeasonalityPlugin.Record.LogDebug($"Next season scheduled in {delay} seconds");
   }

   private void ChangeSeason()
   {
      if (!ZNet.instance.IsServer() && hasValidServer) return;

      if (Configs.m_sleepOverride.Value is Toggle.On)
      {
         m_sleepOverride = true;
      }
      else
      {
         SetNextSeason();
         ScheduleNextSeason();
      }
   }

   private static void SetNextSeason()
   {
      Season currentSeason = Configs.m_season.Value;
      double time = ZNet.m_instance.GetTimeSeconds();
      // Configs.m_lastSeasonChange.Value = time;
      lastSeasonChange = time;
      BroadcastLastSeasonChange();
      Configs.m_season.Value = GetNextSeason(currentSeason);
      Save();
   }

   public static void BroadcastLastSeasonChange()
   {
      foreach (ZRpc? rpc in RpcHandlers.ValidatedPeers)
      {
         rpc.Invoke(nameof(RPC_SetLastSeasonChange), lastSeasonChange);
      }
   }

   public static void RPC_SetLastSeasonChange(ZRpc rpc, double time)
   {
      lastSeasonChange = time;
      Save();
      SeasonalityPlugin.Record.LogDebug($"Received last season change time from server: {time}");
   }
   
   public static void OnConfigChange(object sender, EventArgs e)
   {
      if (instance == null) return;
      instance.ResetSchedule();
   }

   public void ResetSchedule()
   {
      CancelInvoke(nameof(ChangeSeason));
      ScheduleNextSeason();
   }
   
   private static double GetTimeElapsed()
   {
      if (!ZNet.instance) return 0;
      // if (Configs.m_lastSeasonChange.Value > ZNet.instance.GetTimeSeconds()) Configs.m_lastSeasonChange.Value = 0;
      // double timeElapsed = ZNet.m_instance.GetTimeSeconds() - Configs.m_lastSeasonChange.Value;
      double timeElapsed = ZNet.m_instance.GetTimeSeconds() - lastSeasonChange;

      return timeElapsed < 0 ? 0 : timeElapsed;
   }
   
   private static double GetSeasonLength()
   {
      Vector3 vector = Configs.m_durations.GetOrDefault(Configs.m_season.Value, Vector3.zero);
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

   public static Season GetNextSeason(Season season)
   {
      return season switch
      {
         Season.Spring => Season.Summer,
         Season.Summer => Season.Fall,
         Season.Fall => Season.Winter,
         Season.Winter => Season.Spring,
         _ => Season.Summer
      };
   }
   
   private void CheckSeasonFade(double timer)
   {
      if (Configs.m_seasonFades.Value is Toggle.Off) return;
      if (!Player.m_localPlayer || !ZNet.instance || FadeToBlack.m_blackScreenImg is null || FadeToBlack.m_blackScreen is null) return;
      if (timer > Configs.m_fadeLength.Value) return;
      if (ZNet.instance.GetTimeSeconds() - FadeToBlack.m_timeLastFade < 40f || FadeToBlack.m_fading) return;
      StartCoroutine(FadeToBlack.TriggerFade());
   }
}