using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Behaviors;
using Seasonality.Helpers;
using Seasonality.Seasons;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Managers;

public static class SeasonManager
{
    private const string SE_Seasons = "SE_Seasons";
    private static readonly int SE_Season_StableHashCode = SE_Seasons.GetStableHashCode();
    private static float m_seasonEffectTimer;
    private static bool m_firstSpawn = true;
    
    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    private static class Player_OnSpawned_Patch
    {
        private static void Postfix(Player __instance)
        {
            if (!m_firstSpawn || !__instance || __instance != Player.m_localPlayer) return;
            OnSeasonChange();
            m_firstSpawn = false;
        }
    }
    
    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    private static class Game_Logout_Patch
    {
        private static void Postfix()
        {
            MaterialReplacer.ResetMossTextures();
            m_firstSpawn = true;
        }
    }

    public static void UpdateSeasonEffects(float dt)
    {
        if (!Player.m_localPlayer) return;

        m_seasonEffectTimer += dt;
        if (m_seasonEffectTimer < 1f) return;
        m_seasonEffectTimer = 0.0f;
        
        SEMan man = Player.m_localPlayer.GetSEMan();
        if (!man.HaveStatusEffect(SE_Season_StableHashCode)) man.AddStatusEffect(SE_Season_StableHashCode);
        if (!man.HaveStatusEffect(WeatherManager.SE_Weather_StableHashCode) && Configurations._EnableWeather.Value is SeasonalityPlugin.Toggle.On)
            man.AddStatusEffect(WeatherManager.SE_Weather_StableHashCode);
    }
    public static void OnSeasonConfigChange(object sender, EventArgs e) => OnSeasonChange();
    
    private static void OnSeasonChange()
    {
        if (!EnvMan.instance || !ZNet.instance) return;
        --EnvMan.instance.m_environmentPeriod;
        if (!Player.m_localPlayer) return;
        SeasonalClutter.UpdateClutter();
        SeasonalTerrain.UpdateTerrain();
        GlobalKeyManager.UpdateSeasonalKey();
        MaterialReplacer.ModifyCachedMaterials(Configurations._Season.Value);
        FrozenZones.UpdateInstances();
        FrozenWaterLOD.UpdateInstances();
        SeasonalLocation.UpdateInstances();
        SeasonalBossStone.UpdateInstances();
        CustomSeason.UpdateInstances();
        UpdateSE();
    }

    private static void UpdateSE()
    {
        if (!Player.m_localPlayer) return;
        if (Player.m_localPlayer.GetSEMan().GetStatusEffect(SE_Season_StableHashCode) is SE_Season SE)
        {
            SE.Update();
        }
    }
    private static bool ShouldCount() => GetTotalSeconds() > 0;

    private static Sprite? GetIcon()
    {
        return Configurations._Season.Value switch
        {
            SeasonalityPlugin.Season.Spring => SpriteManager.SpringIcon,
            SeasonalityPlugin.Season.Summer => SpriteManager.SummerIcon,
            SeasonalityPlugin.Season.Fall => SpriteManager.FallIcon,
            SeasonalityPlugin.Season.Winter => SpriteManager.WinterIcon,
            _ => SpriteManager.ValknutIcon
        };
    }
    private static double GetTotalSeconds()
    {
        if (!Configurations._Durations.TryGetValue(Configurations._Season.Value, out Dictionary<Configurations.DurationType, ConfigEntry<int>> configs)) return 0;
        double days = configs[Configurations.DurationType.Day].Value * 86400;
        double hours = configs[Configurations.DurationType.Hours].Value * 3600;
        double minutes = configs[Configurations.DurationType.Minutes].Value * 60;
        return days + hours + minutes;
    }
    public static void OnSeasonDisplayConfigChange(object sender, EventArgs e)
    {
        if (!Player.m_localPlayer) return;
        if (Player.m_localPlayer.GetSEMan().GetStatusEffect(SE_Season_StableHashCode) is SE_Season SE)
        {
            SE.Update();
        }

        if (Hud.instance.m_tempStatusEffects.Find(x => x.m_nameHash == SE_Season_StableHashCode) is SE_Season TempSE)
        {
            TempSE.Update();
        }
    }
    private static string GetSeasonTime()
    {
        if (!ShouldCount()) return "";
        TimeSpan span = TimeSpan.FromSeconds(SeasonTimer.GetTimeDifference());
        int days = span.Days;
        int hour = span.Hours;
        int minutes = span.Minutes;
        int seconds = span.Seconds;
    
        if (SeasonTimer.m_sleepOverride) return Localization.instance.Localize("$msg_ready");
        return days > 0 ? $"{days}:{hour:D2}:{minutes:D2}:{seconds:D2}" : hour > 0 ? $"{hour}:{minutes:D2}:{seconds:D2}" : minutes > 0 ? $"{minutes}:{seconds:D2}" : $"{seconds}";
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix()
        {
            if (!ObjectDB.instance || !ZNetScene.instance) return;
            SE_Season effect = ScriptableObject.CreateInstance<SE_Season>();
            effect.name = SE_Seasons;
            effect.m_name = "Seasonality";
            effect.m_icon = GetIcon();
            if (ObjectDB.instance.m_StatusEffects.Contains(effect)) return;
            ObjectDB.instance.m_StatusEffects.Add(effect);
        }
    }
    
    public class SE_Season : StatusEffect
    {
        public override void Setup(Character character)
        {
            Update();
            base.Setup(character);
        }

        public void Update()
        {
            m_name = Configurations._DisplayType.Value is Configurations.DisplayType.Above 
                ? GetSeasonName() 
                : "";
            m_icon = Configurations._DisplaySeason.Value is SeasonalityPlugin.Toggle.On
                ? GetIcon()
                : null;
        }

        public override string GetTooltipString()
        {
            StringBuilder builder = new StringBuilder();
            var localizedTooltip = Localization.instance.Localize($"$season_{Configurations._Season.Value.ToString().ToLower()}_tooltip");
            if (!localizedTooltip.Contains("[")) builder.Append($"$season_{Configurations._Season.Value.ToString().ToLower()}_tooltip\n");
            builder.Append(GetToolTip("Carry Weight", "$se_max_carryweight"));
            builder.Append(GetToolTip("Health Regeneration", "$se_healthregen"));
            builder.Append(GetToolTip("Damage", "$se_damage"));
            builder.Append(GetToolTip("Speed", "$item_movement_modifier"));
            builder.Append(GetToolTip("Eitr Regeneration", "$se_eitrregen"));
            builder.Append(GetToolTip("Raise Skill", "$se_skill_modifier"));
            builder.Append(GetToolTip("Stamina Regeneration", "$se_staminaregen"));

            return Localization.instance.Localize(builder.ToString());
        }

        private string GetToolTip(string key, string token)
        {
            return !GetEffectConfig(key, out ConfigEntry<float>? config) ? "" : FormatTooltip(token, config, key == "Carry Weight");
        }

        private string FormatTooltip(string token, ConfigEntry<float>? config, bool integer)
        {
            var amount = integer ? config?.Value ?? 0f : config?.Value ?? 1f;
            bool increase = integer ? amount > 0f : amount > 1f;
            string symbol = increase ? "+" : "";
            string percentage = integer ? "" : "%";
            float value = integer ? amount : amount * 100 - 100;
            return value != 0f ? $"{token}: <color=orange>{symbol}{Math.Round(value, 1)}{percentage}</color>\n" : "";
        }
        public override string GetIconText()
        {
            if (!EnvMan.instance) return "";
            return Configurations._DisplaySeasonTimer.Value is SeasonalityPlugin.Toggle.Off 
                ? Configurations._DisplayType.Value is Configurations.DisplayType.Above  
                    ? "" 
                    : GetSeasonName()  
                : Configurations._DisplayType.Value is Configurations.DisplayType.Above 
                    ? GetSeasonTime() 
                    : GetSeasonName() + "\n" + GetSeasonTime();
        }

        private static string GetSeasonName() => Localization.instance.Localize($"$season_{Configurations._Season.Value.ToString().ToLower()}");

        private static bool GetEffectConfig(string key, out ConfigEntry<float>? output)
        {
            output = Configurations.effectConfigs.TryGetValue(Configurations._Season.Value,
                out Dictionary<string, ConfigEntry<float>> configs)
                ? configs.TryGetValue(key, out ConfigEntry<float> config) ? config : null
                : null;
            return output != null && Configurations._EnableModifiers.Value is SeasonalityPlugin.Toggle.On;
        }

        public override void ModifyStaminaRegen(ref float staminaRegen)
        {
            if (GetEffectConfig("Stamina Regeneration", out ConfigEntry<float>? config))
                staminaRegen *= config?.Value ?? 1f;
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
        {
            if (GetEffectConfig("Carry Weight", out ConfigEntry<float>? config)) limit += config?.Value ?? 0f;
        }

        public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
        {
            if (GetEffectConfig("Raise Skill", out ConfigEntry<float>? config)) value *= config?.Value ?? 1f;
        }

        public override void ModifyEitrRegen(ref float eitrRegen)
        {
            if (GetEffectConfig("Eitr Regeneration", out ConfigEntry<float>? config)) eitrRegen *= config?.Value ?? 1f;
        }

        public override void ModifyHealthRegen(ref float regenMultiplier)
        {
            if (GetEffectConfig("Health Regeneration", out ConfigEntry<float>? config))
                regenMultiplier *= config?.Value ?? 1f;
        }

        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            if (GetEffectConfig("Damage", out ConfigEntry<float>? config)) hitData.ApplyModifier(config?.Value ?? 1f);
        }

        public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
        {
            if (GetEffectConfig("Speed", out ConfigEntry<float>? config)) speed *= config?.Value ?? 1f;
        }
        
        public override void OnDamaged(HitData hit, Character attacker)
        {
            if (FadeToBlack.m_fading && Configurations._fadeToBlackImmune.Value is SeasonalityPlugin.Toggle.On) hit.ApplyModifier(0f);
        }
    }
}