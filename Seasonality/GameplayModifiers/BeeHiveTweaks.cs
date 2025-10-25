using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;

namespace Seasonality.GameplayModifiers;

public static class BeeHiveTweaks
{
    private static ConfigEntry<Toggle> SummerEnabled = null!;
    private static ConfigEntry<Toggle> FallEnabled = null!;
    private static ConfigEntry<Toggle> SpringEnabled = null!;
    private static ConfigEntry<Toggle> WinterEnabled = null!;
    
    public static void Setup()
    {
        SummerEnabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Beehive Summer", Toggle.On,
            "If on, beehive interactable during summer");
        FallEnabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Beehive Fall", Toggle.On,
            "If on, beehive interactable during fall");
        SpringEnabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Beehive Spring", Toggle.On,
            "If on, beehive interactable during spring");
        WinterEnabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Beehive Winter", Toggle.On,
            "If on, beehive interactable during winter");
    }

    [HarmonyPatch(typeof(Beehive), nameof(Beehive.Interact))]
    private static class Beehive_Interact_Patch
    {
        private static bool Prefix(Beehive __instance, Humanoid character)
        {
            switch (Configs.m_season.Value)
            {
                case Season.Summer:
                    if (SummerEnabled.Value is Toggle.Off)
                    {
                        character.Message(MessageHud.MessageType.Center, __instance.m_areaText);
                        return false;
                    }
                    return true;
                case Season.Fall:
                    if (FallEnabled.Value is Toggle.Off)
                    {
                        character.Message(MessageHud.MessageType.Center, __instance.m_areaText);
                        return false;
                    }
                    return true;
                case Season.Spring:
                    if (SpringEnabled.Value is Toggle.Off)
                    {
                        character.Message(MessageHud.MessageType.Center, __instance.m_areaText);
                        return false;
                    }
                    return true;
                case Season.Winter:
                    if (WinterEnabled.Value is Toggle.Off)
                    {
                        character.Message(MessageHud.MessageType.Center, __instance.m_areaText);
                        return false;
                    }
                    return true;
                default:
                    return true;
            }
        }
    }
}