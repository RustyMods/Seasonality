using BepInEx.Configuration;
using HarmonyLib;

namespace Seasonality;

public static class FishTweaks
{
    private static ConfigEntry<Toggle> m_enabled = null!;
    public static void Setup()
    {
        m_enabled = ConfigManager.config("Tweaks", "Frozen Fish", Toggle.Off, "If on, fish cannot be picked up during winter");
    }

    [HarmonyPatch(typeof(Fish), nameof(Fish.Interact))]
    private static class Fish_Interact_Patch
    {
        private static bool Prefix()
        {
            if (m_enabled.Value is Toggle.Off) return true;
            return SeasonTime.GetCurrentSeason() is not Season.Winter;
        }
    }

    [HarmonyPatch(typeof(Fish), nameof(Fish.GetHoverText))]
    private static class Fish_GetHoverText_Patch
    {
        private static void Postfix(ref string __result)
        {
            if (m_enabled.Value is Toggle.Off) return;
            if (SeasonTime.GetCurrentSeason() is not Season.Winter) return;
            __result += Localization.instance.Localize("\n $cannot_pick");
        }
    }
}