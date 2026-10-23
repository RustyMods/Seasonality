using System;
namespace Seasonality;

public class SE_WeatherMan : StatusEffect
{
    public override void Setup(Character character)
    {
        base.Setup(character);
        Update();
    }

    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);
        Update();
    }

    public void Update()
    {
        m_icon = Configs.m_displayWeather.Value is Toggle.On ? SpriteManager.ValknutIcon : null;
    }
        
    public override string GetTooltipString()
    {
        var localized = Localization.instance.Localize($"$weather_{EnvMan.instance.m_currentEnv.m_name.ToLower().Replace(" ", "_")}_tooltip");
        if (localized.Contains("[")) localized = "";
        return m_tooltip + localized;
    }

    public override string GetIconText()
    {
        if (EnvMan.instance == null) return "";
        var env = EnvMan.instance.m_currentEnv.m_name;
        var localized = Localization.instance.Localize($"$weather_{EnvMan.instance.m_currentEnv.m_name.ToLower().Replace(" ", "_")}");
        m_name = localized.Contains("[") ? env : localized;

        if (Configs.m_displayWeatherTimer.Value is Toggle.Off) return "";
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