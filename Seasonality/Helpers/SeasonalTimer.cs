using System;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Seasonality.Helpers;

public class SeasonalTimer : MonoBehaviour
{
   public static bool hasValidServer;
   public static bool m_sleepOverride;
   public float m_fadeTimer;

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
         if (!__instance.GetComponent<SeasonalTimer>()) __instance.gameObject.AddComponent<SeasonalTimer>();
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

   public static SeasonalTimer? instance;

   public void Awake()
   {
      SeasonalityPlugin.Record.LogDebug("Seasonal Timer Awake");
      
      instance = this;
      
      ScheduleNextSeason();
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
      Configs.m_lastSeasonChange.Value = time;
      Configs.m_season.Value = GetNextSeason(currentSeason);
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
      if (Configs.m_lastSeasonChange.Value > ZNet.instance.GetTimeSeconds()) Configs.m_lastSeasonChange.Value = 0;
      double timeElapsed = ZNet.m_instance.GetTimeSeconds() - Configs.m_lastSeasonChange.Value;
      return timeElapsed < 0 ? 0 : timeElapsed;
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