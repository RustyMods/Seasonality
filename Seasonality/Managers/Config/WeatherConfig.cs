using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using UnityEngine;

namespace Seasonality;

public class WeatherConfig
{
    public static readonly ConfigurationManagerAttributes attributes = new(Draw);
    
    public struct WeatherData
    {
        public string name;
        public float weight;
    }

    public readonly List<WeatherData> environments;

    public WeatherConfig()
    {
        environments = [];
    }

    public WeatherConfig(List<WeatherData> weathers)
    {
        environments = weathers;
    }

    public WeatherConfig(string weathers)
    {
        string[] entries = Regex.Split(weathers, @",(?!\d+(?:,|$))");
        environments = entries
            .Select(input =>
            {
                var parts = input.Split(':');
                float weight = 1f;

                if (parts.Length > 1)
                {
                    // Normalize decimal comma to dot, then parse invariantly
                    var raw = parts[1].Trim().Replace(',', '.');
                    if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out weight))
                        weight = 1f;
                }

                return new WeatherData
                {
                    name = parts[0].Trim(),
                    weight = weight
                };
            })
            .ToList();
    }

    public void Add(string env, float weight)
    {
        environments.Add(new WeatherData
        {
            name = env,
            weight = weight
        });
    }

    public override string ToString()
    {
        return string.Join(",", environments.Select(e => $"{e.name}:{e.weight}"));
    }

    public static void Draw(ConfigEntryBase cfg)
    {
        var locked = cfg.Description.Tags
            .Select(a =>
                a.GetType().Name == "ConfigurationManagerAttributes"
                    ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                    : null).FirstOrDefault(v => v != null) ?? false;

        List<WeatherData> newWeathers = new();
        var wasUpdated = false;

        var RightColumnWidth = ConfigManager.ConfigManagerRightColumnWidth;
        
        GUILayout.BeginVertical();
        foreach (WeatherData env in new WeatherConfig((string)cfg.BoxedValue).environments)
        {
            GUILayout.BeginHorizontal();

            var weight = env.weight;

            var nameWidth = Mathf.Max(RightColumnWidth - 40 - 21 - 21, 180);
            var newEnvironment = GUILayout.TextField(env.name,
                new GUIStyle(GUI.skin.textField) { fixedWidth = nameWidth });
            var envName = locked ? env.name : newEnvironment;
            wasUpdated = wasUpdated || envName != env.name;

            if (float.TryParse(
                    GUILayout.TextField(weight.ToString(CultureInfo.InvariantCulture),
                        new GUIStyle(GUI.skin.textField) { fixedWidth = 40 }),
                    out var newAmount) && Math.Abs(newAmount - weight) > 0.01f && !locked)
            {
                weight = newAmount;
                wasUpdated = true;
            }

            if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) &&
                !locked)
                wasUpdated = true;
            else
                newWeathers.Add(new WeatherData { name = envName, weight = weight });

            if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) &&
                !locked)
            {
                wasUpdated = true;
                newWeathers.Add(new WeatherData { weight = 1f, name = "" });
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        if (wasUpdated) cfg.BoxedValue = new WeatherConfig(newWeathers).ToString();
    }
}