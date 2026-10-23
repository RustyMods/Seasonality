using System;
using System.Text;
using BepInEx.Configuration;
using UnityEngine;

namespace Seasonality;

public class SE_Season : StatusEffect
{
    public override void Setup(Character character)
    {
        Update();
        base.Setup(character);
    }

    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);
        Update();
    }

    public void Update()
    {
        m_name = Configs.m_displayType.Value is DisplayType.Above
            ? GetSeasonName()
            : "";
        m_icon = Configs.m_displaySeason.Value is Toggle.On
            ? GetIcon()
            : null;
    }

    public override string GetTooltipString()
    {
        var builder = new StringBuilder();
        var localizedTooltip =
            Localization.instance.Localize($"$season_{SeasonTime.GetCurrentSeason().ToString().ToLower()}_tooltip");
        if (!localizedTooltip.Contains("["))
            builder.Append($"$season_{SeasonTime.GetCurrentSeason().ToString().ToLower()}_tooltip\n");
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
        if (!Configs.m_effectConfigs.TryGetValue(SeasonTime.GetCurrentSeason(), out var configs)) return "";
        return configs.TryGetValue(key, out var config) ? FormatTooltip(token, config, key == "Carry Weight") : "";
    }

    private string FormatTooltip(string token, ConfigEntry<float>? config, bool integer)
    {
        var amount = integer ? config?.Value ?? 0f : config?.Value ?? 1f;
        var increase = integer ? amount > 0f : amount > 1f;
        var symbol = increase ? "+" : "";
        var percentage = integer ? "" : "%";
        var value = integer ? amount : amount * 100 - 100;
        return value != 0f ? $"{token}: <color=orange>{symbol}{Math.Round(value, 1)}{percentage}</color>\n" : "";
    }

    public override string GetIconText()
    {
        if (!EnvMan.instance) return "";
        return Configs.m_displayTimer.Value is Toggle.Off
            ? Configs.m_displayType.Value is DisplayType.Above
                ? ""
                : GetSeasonName()
            : Configs.m_displayType.Value is DisplayType.Above
                ? GetSeasonTime()
                : GetSeasonName() + "\n" + GetSeasonTime();
    }

    private static string GetSeasonName()
    {
        return Localization.instance.Localize($"$season_{SeasonTime.GetCurrentSeason().ToString().ToLower()}");
    }

    public override void ModifyStaminaRegen(ref float staminaRegen)
    {
        if (Configs.m_enableModifiers.Value is Toggle.Off) return;
        staminaRegen *= Configs.m_effectConfigs.GetOrDefault(SeasonTime.GetCurrentSeason(), "Stamina Regeneration", 1f);
    }

    public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
    {
        if (Configs.m_enableModifiers.Value is Toggle.Off) return;
        limit += Configs.m_effectConfigs.GetOrDefault("Carry Weight", 0f);
    }

    public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
    {
        if (Configs.m_enableModifiers.Value is Toggle.Off) return;
        value *= Configs.m_effectConfigs.GetOrDefault("Raise Skill", 1f);
    }

    public override void ModifyEitrRegen(ref float eitrRegen)
    {
        if (Configs.m_enableModifiers.Value is Toggle.Off) return;
        eitrRegen *= Configs.m_effectConfigs.GetOrDefault("Eitr Regeneration", 1f);
    }

    public override void ModifyHealthRegen(ref float regenMultiplier)
    {
        if (Configs.m_enableModifiers.Value is Toggle.Off) return;
        regenMultiplier *= Configs.m_effectConfigs.GetOrDefault("Health Regeneration", 1f);
    }

    public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
    {
        if (Configs.m_enableModifiers.Value is Toggle.Off) return;
        hitData.ApplyModifier(Configs.m_effectConfigs.GetOrDefault("Damage", 1f));
    }

    public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
    {
        if (Configs.m_enableModifiers.Value is Toggle.Off) return;
        speed *= Configs.m_effectConfigs.GetOrDefault("Speed", 1f);
    }

    public override void OnDamaged(HitData hit, Character attacker)
    {
        if (FadeToBlack.m_fading && Configs.m_fadeToBlackImmune.Value is Toggle.On) hit.ApplyModifier(0f);
    }

    public static Sprite? GetIcon()
    {
        return SeasonTime.GetCurrentSeason() switch
        {
            Season.Spring => SpriteManager.SpringIcon,
            Season.Summer => SpriteManager.SummerIcon,
            Season.Fall => SpriteManager.FallIcon,
            Season.Winter => SpriteManager.WinterIcon,
            _ => SpriteManager.ValknutIcon
        };
    }

    private static bool ShouldCount()
    {
        return GetTotalSeconds() > 0;
    }

    private static double GetTotalSeconds()
    {
        var vector3 = Configs.m_durations.TryGetValue(SeasonTime.GetCurrentSeason(), out var config)
            ? config.Value
            : Vector3.zero;
        double days = vector3.x * 86400;
        double hours = vector3.y * 3600;
        double minutes = vector3.z * 60;
        return days + hours + minutes;
    }

    private static string GetSeasonTime()
    {
        if (!ShouldCount()) return "";
        var time = SeasonTime.GetTimeDifference();
        var span = TimeSpan.FromSeconds(SeasonTime.GetTimeDifference());
        var days = span.Days;
        var hour = span.Hours;
        var minutes = span.Minutes;
        var seconds = span.Seconds;

        if (SeasonTime.m_sleepOverride && time <= 0) return Localization.instance.Localize("$msg_ready");

        return days > 0 ? $"{days} {Localization.instance.Localize(days > 1 ? "$label_days" : "$label_day")}"
            : hour > 0 ? $"{hour}:{minutes:D2}:{seconds:D2}"
            : minutes > 0 ? $"{minutes}:{seconds:D2}"
            : $"{seconds}";
    }
}