using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;

namespace Seasonality.GameplayModifiers;

public static class BeeHiveTweaks
{
    private static ConfigEntry<Configs.Toggle> SummerEnabled = null!;
    private static ConfigEntry<Configs.Toggle> FallEnabled = null!;
    private static ConfigEntry<Configs.Toggle> SpringEnabled = null!;
    private static ConfigEntry<Configs.Toggle> WinterEnabled = null!;
    
    public static void Setup()
    {
        SummerEnabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Beehive Summer", Configs.Toggle.On,
            "If on, beehive interactable during summer");
        FallEnabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Beehive Fall", Configs.Toggle.On,
            "If on, beehive interactable during fall");
        SpringEnabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Beehive Spring", Configs.Toggle.On,
            "If on, beehive interactable during spring");
        WinterEnabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Beehive Winter", Configs.Toggle.On,
            "If on, beehive interactable during winter");
    }

    [HarmonyPatch(typeof(Beehive), nameof(Beehive.Interact))]
    private static class Beehive_Interact_Patch
    {
        private static bool Prefix(Beehive __instance, Humanoid character)
        {
            switch (Configs.m_season.Value)
            {
                case Configs.Season.Summer:
                    if (SummerEnabled.Value is Configs.Toggle.Off)
                    {
                        character.Message(MessageHud.MessageType.Center, __instance.m_areaText);
                        return false;
                    }
                    return true;
                case Configs.Season.Fall:
                    if (FallEnabled.Value is Configs.Toggle.Off)
                    {
                        character.Message(MessageHud.MessageType.Center, __instance.m_areaText);
                        return false;
                    }
                    return true;
                case Configs.Season.Spring:
                    if (SpringEnabled.Value is Configs.Toggle.Off)
                    {
                        character.Message(MessageHud.MessageType.Center, __instance.m_areaText);
                        return false;
                    }
                    return true;
                case Configs.Season.Winter:
                    if (WinterEnabled.Value is Configs.Toggle.Off)
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