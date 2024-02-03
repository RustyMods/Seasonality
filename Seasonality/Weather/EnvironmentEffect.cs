using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Weather;
public class EnvironmentEffectData
{
    public string name = null!;
    public string m_name = null!;
    public Sprite? m_sprite;
    public string? m_start_msg;
    public string? m_tooltip;
        
    public StatusEffect InitEnvEffect()
    {
        ObjectDB obd = ObjectDB.instance;
        obd.m_StatusEffects.RemoveAll(effect => effect is EnvironmentEffect);

        EnvironmentEffect effect = ScriptableObject.CreateInstance<EnvironmentEffect>();
        effect.data = this;
        effect.name = name;
        effect.m_name = m_name;
        effect.m_icon = _WeatherIconEnabled.Value is Toggle.On ? m_sprite : null;
        effect.m_startMessageType = MessageHud.MessageType.TopLeft;
        effect.m_startMessage = _WeatherStartMessage.Value is Toggle.On ? m_start_msg : "";
        effect.m_tooltip = m_tooltip;
            
        obd.m_StatusEffects.Add(effect);

        return effect;
    }
}
public class EnvironmentEffect : StatusEffect
{
    public EnvironmentEffectData data = null!;
    public override string GetIconText() => WeatherManager.GetEnvironmentCountDown();
}