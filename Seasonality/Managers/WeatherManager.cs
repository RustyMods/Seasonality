using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Seasons;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Managers;

public static class WeatherManager
{
    private const string SE_Weather = "SE_Weatherman";
    public static readonly int SE_Weather_StableHashCode = SE_Weather.GetStableHashCode();
    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.GetAvailableEnvironments))]
    private static class GetAvailableEnvironments_Patch
    {
        private static void Postfix(Heightmap.Biome biome, ref List<EnvEntry> __result)
        {
            if (Configurations._EnableWeather.Value is SeasonalityPlugin.Toggle.Off) return;
            if (biome is Heightmap.Biome.None or Heightmap.Biome.AshLands) return;
            __result = GetEnvironments(biome, __result);
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
    private static class EnvMan_Awake_Patch
    {
        private static void Postfix(EnvMan __instance)
        {
            if (!__instance) return;

            RegisterEnvironments(__instance);
            
            if (Configurations._EnableWeather.Value is SeasonalityPlugin.Toggle.Off) return;
            __instance.m_environmentDuration = Configurations._WeatherDuration.Value * 60;
        }
    }

    private static void RegisterEnvironments(EnvMan __instance)
    {
        EnvSetup? snow = __instance.GetEnv("Snow");
        EnvSetup? snowStorm = __instance.GetEnv("SnowStorm");

        if (snow is null || snowStorm is null) return;
        
        EnvSetup? warmSnow = snow.Clone();
        warmSnow.m_isFreezing = false;
        warmSnow.m_isFreezingAtNight = false;
        warmSnow.m_name = "WarmSnow";

        EnvSetup? warmSnowStorm = snowStorm.Clone();
        warmSnowStorm.m_isFreezing = false;
        warmSnowStorm.m_isFreezingAtNight = false;
        warmSnowStorm.m_name = "WarmSnowStorm";

        EnvSetup? nightFrost = snow.Clone();
        nightFrost.m_isFreezing = false;
        nightFrost.m_name = "NightFrost";
        nightFrost.m_isFreezingAtNight = true;
        
        RegisterEnv(warmSnow);
        RegisterEnv(warmSnowStorm);
        RegisterEnv(nightFrost);
    }

    private static void RegisterEnv(EnvSetup setup)
    {
        if (!EnvMan.instance || EnvMan.instance.m_environments.Contains(setup)) return;
        EnvMan.instance.m_environments.Add(setup);
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
    private static class EnvMan_IsCold_Patch
    {
        private static void Postfix(ref bool __result)
        {
            if (!Player.m_localPlayer) return;
            if (Configurations._WinterAlwaysCold.Value is SeasonalityPlugin.Toggle.Off) return;
            if (Configurations._Season.Value is not SeasonalityPlugin.Season.Winter) return;
            if (Player.m_localPlayer.GetCurrentBiome() is Heightmap.Biome.AshLands) return;
            __result = true;
        }
    }
    public static void OnWeatherDurationChange(object sender, EventArgs e)
    {
        if (!EnvMan.instance) return;
        if (Configurations._EnableWeather.Value is SeasonalityPlugin.Toggle.Off) return;
        EnvMan.instance.m_environmentDuration = Configurations._WeatherDuration.Value * 60;
    }

    private static List<EnvEntry> GetEnvironments(Heightmap.Biome biome, List<EnvEntry> entries)
    {
        if (!EnvMan.instance) return entries;
        List<EnvEntry> output = new();
        if (!Configurations._WeatherConfigs.TryGetValue(Configurations._Season.Value, out Dictionary<Heightmap.Biome, ConfigEntry<string>> configs)) return entries;
        if (!configs.TryGetValue(biome, out ConfigEntry<string> config)) return entries;
        if (config.Value.IsNullOrWhiteSpace()) return entries;
        
        foreach (var weather in new Configurations.SerializedWeather(config.Value).environments)
        {
            if (EnvMan.instance.GetEnv(weather.name) is not { } setup) continue;
            var entry = new EnvEntry()
            {
                m_environment = weather.name,
                m_weight = weather.weight,
                m_env = setup
            };
            output.Add(entry);
        }
        return output.Count > 0 ? output : entries;
    }
    

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix()
        {
            if (!ObjectDB.instance || !ZNetScene.instance) return;
            SE_WeatherMan effect = ScriptableObject.CreateInstance<SE_WeatherMan>();
            effect.name = SE_Weather;
            effect.m_name = "Weatherman";
            effect.m_icon = SpriteManager.ValknutIcon;
            if (ObjectDB.instance.m_StatusEffects.Contains(effect)) return;
            ObjectDB.instance.m_StatusEffects.Add(effect);
        }
    }
    public static void OnDisplayConfigChange(object sender, EventArgs e)
    {
        if (!Player.m_localPlayer) return;
        if (Player.m_localPlayer.GetSEMan().GetStatusEffect(SE_Weather_StableHashCode) is SE_WeatherMan SE)
        {
            SE.Update();
        }
    }

    public class SE_WeatherMan : StatusEffect
    {
        public override void Setup(Character character)
        {
            base.Setup(character);
            Update();
        }

        public void Update()
        {
            m_icon = Configurations._DisplayWeather.Value is SeasonalityPlugin.Toggle.On ? SpriteManager.ValknutIcon : null;
        }
        
        public override string GetTooltipString()
        {
            return m_tooltip + Localization.instance.Localize($"$weather_{EnvMan.instance.m_currentEnv.m_name.ToLower().Replace(" ", "_")}_tooltip");
        }

        public override string GetIconText()
        {
            if (EnvMan.instance == null) return "";
            m_name = Localization.instance.Localize($"$weather_{EnvMan.instance.m_currentEnv.m_name.ToLower().Replace(" ", "_")}");

            if (Configurations._DisplayWeatherTimer.Value is SeasonalityPlugin.Toggle.Off) return "";
            double seed = EnvMan.instance.m_totalSeconds / EnvMan.instance.m_environmentDuration;
            double fraction = 1 - (seed - Math.Truncate(seed));
            double total = fraction * EnvMan.instance.m_environmentDuration;

            int hours = TimeSpan.FromSeconds(total).Hours;
            int minutes = TimeSpan.FromSeconds(total).Minutes;
            int seconds = TimeSpan.FromSeconds(total).Seconds;
    
            if (total < 0) return "";

            return hours > 0 ? $"{hours}:{minutes:D2}:{seconds:D2}" : minutes > 0 ? $"{minutes}:{seconds:D2}" : $"{seconds}";
        }
    }
}