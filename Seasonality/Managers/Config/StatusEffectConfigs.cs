using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Seasonality;

public class StatusEffectConfig
{
    public static readonly ConfigurationManagerAttributes attributes = new(Draw);
    public readonly string m_name;
    public readonly string m_description;
    public readonly float m_value;
    public KeyValuePair<float, float> m_range;

    public StatusEffectConfig(string name, string description, float value, float min, float max)
    {
        m_name = name;
        m_description = description;
        m_value = value;
        m_range = new KeyValuePair<float, float>(min, max);
    }

    public static void Draw(ConfigEntryBase cfg)
    {
        bool locked = cfg.Description.Tags
            .Select(a =>
                a.GetType().Name == "ConfigurationManagerAttributes"
                    ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                    : null).FirstOrDefault(v => v != null) ?? false;
        Vector3 value = (Vector3)cfg.BoxedValue;
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        string? x = GUILayout.TextField(value.x.ToString("0"),
            new GUIStyle(GUI.skin.textField) { fixedWidth = 50 });
        GUILayout.Label("Days", new GUIStyle(GUI.skin.label) { fixedWidth = 50 });
        string? y = GUILayout.TextField(value.y.ToString("0"),
            new GUIStyle(GUI.skin.textField) { fixedWidth = 50 });
        GUILayout.Label("Hours", new GUIStyle(GUI.skin.label) { fixedWidth = 50 });
        string? z = GUILayout.TextField(value.z.ToString("0"),
            new GUIStyle(GUI.skin.textField) { fixedWidth = 50 });
        GUILayout.Label("Minutes", new GUIStyle(GUI.skin.label) { fixedWidth = 50 });
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        if (locked) return;
        var newValue = new Vector3(
            float.TryParse(x, out float days) ? days : 0f,
            float.TryParse(y, out float hours) ? hours : 0,
            float.TryParse(z, out float minutes) ? minutes : 0f);
        if (newValue != value)
        {
            cfg.BoxedValue = newValue;
        }
    }
}