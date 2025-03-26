using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Behaviors;

public static class SeasonSE
{
    private const string SE_Seasons = "SE_Seasons";
    private static readonly int SE_Season_StableHashCode = SE_Seasons.GetStableHashCode();
    private static float m_timer;
    public static void CheckOrSet(float dt)
    {
        m_timer += dt;
        if (m_timer <= 10f) return;
        m_timer = 0.0f;
        if (!Player.m_localPlayer) return;
        if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(SE_Season_StableHashCode)) return;
        Player.m_localPlayer.GetSEMan().AddStatusEffect(SE_Season_StableHashCode);
    }
    
    public static void UpdateStatus()
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
        return Configs.m_season.Value switch
        {
            Configs.Season.Spring => SpriteManager.SpringIcon,
            Configs.Season.Summer => SpriteManager.SummerIcon,
            Configs.Season.Fall => SpriteManager.FallIcon,
            Configs.Season.Winter => SpriteManager.WinterIcon,
            _ => SpriteManager.ValknutIcon
        };
    }
    private static double GetTotalSeconds()
    {
        Vector3 vector3 = Configs.m_durations.TryGetValue(Configs.m_season.Value, out ConfigEntry<Vector3> config)
            ? config.Value
            : Vector3.zero;
        double days = vector3.x * 86400;
        double hours = vector3.y * 3600;
        double minutes = vector3.z * 60;
        return days + hours + minutes;
    }
    public static void OnSeasonDisplayConfigChange(object sender, EventArgs e)
    {
        if (!Player.m_localPlayer || !Hud.instance) return;
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
            m_name = Configs.m_displayType.Value is Configs.DisplayType.Above 
                ? GetSeasonName() 
                : "";
            m_icon = Configs.m_displaySeason.Value is Configs.Toggle.On
                ? GetIcon()
                : null;
        }

        public override string GetTooltipString()
        {
            StringBuilder builder = new StringBuilder();
            var localizedTooltip = Localization.instance.Localize($"$season_{Configs.m_season.Value.ToString().ToLower()}_tooltip");
            if (!localizedTooltip.Contains("[")) builder.Append($"$season_{Configs.m_season.Value.ToString().ToLower()}_tooltip\n");
            builder.Append("Modifiers Enabled: " + $"{Configs.m_enableModifiers.Value.ToString()}\n");
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
            if (!Configs.m_effectConfigs.TryGetValue(Configs.m_season.Value, out Dictionary<string, ConfigEntry<float>> configs)) return "";
            return configs.TryGetValue(key, out ConfigEntry<float> config) ? FormatTooltip(token, config, key == "Carry Weight") : "";
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
            return Configs.m_displayTimer.Value is Configs.Toggle.Off 
                ? Configs.m_displayType.Value is Configs.DisplayType.Above  
                    ? "" 
                    : GetSeasonName()  
                : Configs.m_displayType.Value is Configs.DisplayType.Above 
                    ? GetSeasonTime() 
                    : GetSeasonName() + "\n" + GetSeasonTime();
        }

        private static string GetSeasonName() => Localization.instance.Localize($"$season_{Configs.m_season.Value.ToString().ToLower()}");

        public override void ModifyStaminaRegen(ref float staminaRegen)
        {
            if (Configs.m_enableModifiers.Value is Configs.Toggle.Off) return;
            staminaRegen *= Configs.m_effectConfigs.GetOrDefault(Configs.m_season.Value, "Stamina Regeneration", 1f);
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
        {
            if (Configs.m_enableModifiers.Value is Configs.Toggle.Off) return;
            limit += Configs.m_effectConfigs.GetOrDefault("Carry Weight", 0f);
        }

        public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
        {
            if (Configs.m_enableModifiers.Value is Configs.Toggle.Off) return;
            value *= Configs.m_effectConfigs.GetOrDefault("Raise Skill", 1f);
        }

        public override void ModifyEitrRegen(ref float eitrRegen)
        {
            if (Configs.m_enableModifiers.Value is Configs.Toggle.Off) return;
            eitrRegen *= Configs.m_effectConfigs.GetOrDefault("Eitr Regeneration", 1f);
        }

        public override void ModifyHealthRegen(ref float regenMultiplier)
        {
            if (Configs.m_enableModifiers.Value is Configs.Toggle.Off) return;
            regenMultiplier *= Configs.m_effectConfigs.GetOrDefault("Health Regeneration", 1f);
        }

        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            if (Configs.m_enableModifiers.Value is Configs.Toggle.Off) return;
            hitData.ApplyModifier(Configs.m_effectConfigs.GetOrDefault("Damage", 1f));
        }

        public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
        {
            if (Configs.m_enableModifiers.Value is Configs.Toggle.Off) return;
            speed *= Configs.m_effectConfigs.GetOrDefault("Speed", 1f);
        }
        
        public override void OnDamaged(HitData hit, Character attacker)
        {
            if (FadeToBlack.m_fading && Configs.m_fadeToBlackImmune.Value is Configs.Toggle.On) hit.ApplyModifier(0f);
        }
    }
}