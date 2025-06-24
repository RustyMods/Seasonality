using System;
using BepInEx.Configuration;
using Seasonality.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Seasonality.Behaviors;

public class SnowMaterial
{
    private static void CreateConfigs(Material material, string configGroup, Action? onConfigChange = null)
    {
        for (int index = 0; index < material.shader.GetPropertyCount(); ++index)
        {
            string? property = material.shader.GetPropertyName(index);
            string? desc = material.shader.GetPropertyDescription(index);
            ShaderPropertyType type = material.shader.GetPropertyType(index);

            if (type is ShaderPropertyType.Float)
            {
                if (!material.HasProperty(property)) continue;
                var config = SeasonalityPlugin.ConfigManager.config(configGroup, property.Replace("_", string.Empty), material.GetFloat(property), desc);
                config.SettingChanged += (_, _) =>
                {
                    material.SetFloat(property, config.Value);
                    onConfigChange?.Invoke();
                };
                material.SetFloat(property, config.Value);
            }
            else if (type is ShaderPropertyType.Range)
            {
                if (!material.HasProperty(property)) continue;
                Vector2 range = material.shader.GetPropertyRangeLimits(index);
                var config = SeasonalityPlugin.ConfigManager.config(configGroup, property.Replace("_", string.Empty), material.GetFloat(property), new ConfigDescription(desc, new AcceptableValueRange<float>(range.x, range.y)));
                config.SettingChanged += (_, _) =>
                {
                    material.SetFloat(property, config.Value);
                    onConfigChange?.Invoke();
                };
                material.SetFloat(property, config.Value);

            }
            else if (type is ShaderPropertyType.Color)
            {
                if (!material.HasProperty(property)) continue;
                var config = SeasonalityPlugin.ConfigManager.config(configGroup, property.Replace("_", string.Empty), material.GetColor(property), desc);
                config.SettingChanged += (_, _) =>
                {
                    material.SetColor(property, config.Value);
                    onConfigChange?.Invoke();
                };
                SeasonalityPlugin.FrozenWaterMat.SetColor(property, config.Value);
            }
            else if (type is ShaderPropertyType.Vector)
            {
                if (!material.HasProperty(property)) continue;
                var config = SeasonalityPlugin.ConfigManager.config(configGroup, property.Replace("_", string.Empty), material.GetVector(property), desc);
                config.SettingChanged += (_, _) =>
                {
                    material.SetVector(property, config.Value);
                    onConfigChange?.Invoke();
                };
                material.SetVector(property, config.Value);
            }
        }
    }
    public static void Setup()
    {
        
        CreateConfigs(SeasonalityPlugin.FrozenWaterMat, "Frozen Material", () =>
        {
            Configs.m_waterFreezes.Value = Configs.Toggle.Off;
            Configs.m_waterFreezes.Value = Configs.Toggle.On;
        });
                
        // CreateConfigs(SeasonalityPlugin.DistanceFrozenWaterMat, "Distant Frozen Material", () =>
        // {
        //     Configs.m_waterFreezes.Value = Configs.Toggle.Off;
        //     Configs.m_waterFreezes.Value = Configs.Toggle.On;
        // });
    }
}