using BepInEx.Configuration;
using Seasonality.Helpers;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Behaviors;

public static class FrozenManager
{
    private static readonly Material FrozenWaterMat1 = SeasonalityPlugin.NewIce.LoadAsset<Material>("FrozenWater_mat3");
    // private static readonly Material FrozenWaterMat2 = SeasonalityPlugin.NewIce.LoadAsset<Material>("FrozenWater_mat6");

    // public static readonly Shader SnowPieces = SeasonalityPlugin.NewIce.LoadAsset<Shader>("SnowPieces");

    private enum FrozenMaterials
    {
        Option1,
        Option2
    }

    // private static ConfigEntry<FrozenMaterials> Options = null!;
    // private static ConfigEntry<Configs.Toggle> SnowShader = null!;

    public static void Setup()
    {
        // ShaderConfigs.CreateConfigs(FrozenWaterMat2, "Ice Shader", () =>
        // {
        //     if (Configs.m_waterFreezes.Value == Configs.Toggle.Off) return;
        //     Configs.m_waterFreezes.Value = Configs.Toggle.Off;
        //     Configs.m_waterFreezes.Value = Configs.Toggle.On;
        // });

        // Options = SeasonalityPlugin.ConfigManager.config("Ice Shader", "Option", FrozenMaterials.Option1, "Select ice shader");
        // Options.SettingChanged += (sender, args) =>
        // {
        //     if (Configs.m_waterFreezes.Value == Configs.Toggle.Off) return;
        //     Configs.m_waterFreezes.Value = Configs.Toggle.Off;
        //     Configs.m_waterFreezes.Value = Configs.Toggle.On;
        // };
        // SnowShader = SeasonalityPlugin.ConfigManager.config("1 - Settings", "Apply Snow", Configs.Toggle.On,
        //     "If on, snow shader is applied to pieces");
        // SnowShader.SettingChanged += (sender, args) =>
        // {
        //     foreach (var data in TextureReplacer.m_mats.Values)
        //     {
        //         data.ToggleSnowShader(SnowShader.Value == Configs.Toggle.On);
        //     }
        // };
        
        
    }


    public static Material GetSelectedMaterial()
    {
        return FrozenWaterMat1;
        // switch (Options.Value)
        // {
        //     case FrozenMaterials.Option1:
        //         return FrozenWaterMat1;
        //     case FrozenMaterials.Option2:
        //         return FrozenWaterMat2;
        //     default: 
        //         return FrozenWaterMat1;
        // }
    }
}