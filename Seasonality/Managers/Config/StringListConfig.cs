using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Seasonality;

public class StringListConfig
{
    public static readonly ConfigurationManagerAttributes attributes = new(Draw);
    public readonly List<string> m_names;

    public StringListConfig(List<string> prefabs) => m_names = prefabs;

    public StringListConfig(params string[] prefabs) => m_names = prefabs.ToList();

    public StringListConfig(string config) => m_names = config.Split(',').ToList();

    public override string ToString() => string.Join(",", m_names);

    private static void Draw(ConfigEntryBase cfg)
    {
        bool locked = cfg.Description.Tags
            .Select(a =>
                a.GetType().Name == "ConfigurationManagerAttributes"
                    ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                    : null).FirstOrDefault(v => v != null) ?? false;
        bool wasUpdated = false;
        List<string> prefabs = new();
        GUILayout.BeginVertical();
        foreach (var prefab in new StringListConfig((string)cfg.BoxedValue).m_names)
        {
            GUILayout.BeginHorizontal();
            var prefabName = prefab;
            var nameField = GUILayout.TextField(prefab);
            if (nameField != prefab && !locked)
            {
                wasUpdated = true;
                prefabName = nameField;
            }

            if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
            {
                wasUpdated = true;
            }
            else
            {
                prefabs.Add(prefabName);
            }

            if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
            {
                prefabs.Add("");
                wasUpdated = true;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        if (wasUpdated)
        {
            cfg.BoxedValue = new StringListConfig(prefabs).ToString();
        }
    }
}