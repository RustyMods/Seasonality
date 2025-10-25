using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;
namespace Seasonality.GameplayModifiers;

public static class FishTweaks
{
    private static ConfigEntry<Toggle> m_enabled = null!;
    public static void Setup()
    {
        m_enabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Frozen Fish", Toggle.Off, "If on, fish cannot be picked up during winter");
    }

    [HarmonyPatch(typeof(Fish), nameof(Fish.Interact))]
    private static class Fish_Interact_Patch
    {
        private static bool Prefix()
        {
            if (m_enabled.Value is Toggle.Off) return true;
            return Configs.m_season.Value is not Season.Winter;
        }
    }

    [HarmonyPatch(typeof(Fish), nameof(Fish.GetHoverText))]
    private static class Fish_GetHoverText_Patch
    {
        private static void Postfix(ref string __result)
        {
            if (m_enabled.Value is Toggle.Off) return;
            if (Configs.m_season.Value is not Season.Winter) return;
            __result += Localization.instance.Localize("\n $cannot_pick");
        }
    }
}