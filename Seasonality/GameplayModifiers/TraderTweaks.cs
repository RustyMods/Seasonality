using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace Seasonality;

public static class TraderTweaks
{
    private static ConfigEntry<Toggle> m_enabled = null!;
    private static ConfigEntry<string> m_summerItems = null!;
    private static ConfigEntry<string> m_fallItems = null!;
    private static ConfigEntry<string> m_winterItems = null!;
    private static ConfigEntry<string> m_springItems = null!;

    private static readonly List<SerializedItems.SeasonalItem> m_defaultSummerItems = new()
    {
        new SerializedItems.SeasonalItem("HelmetMidsummerCrown", 1, 300),
    };
    private static readonly List<SerializedItems.SeasonalItem> m_defaultFallItems = new()
    {
        new SerializedItems.SeasonalItem("HelmetPointyHat", 1, 300),
    };

    private static readonly List<SerializedItems.SeasonalItem> m_defaultWinterItems = new()
    {
        
    };

    private static readonly List<SerializedItems.SeasonalItem> m_defaultSpringItems = new()
    {
        
    };

    public static void Setup()
    {
        m_enabled = ConfigManager.config("Tweaks", "Trader Enabled", Toggle.Off, "If on, trader has extra items for sale depending on season");
        m_summerItems = ConfigManager.config("Tweaks", "Summer Items", new SerializedItems(m_defaultSummerItems).ToString(), new ConfigDescription("Set summer items added to trader", null, SerializedItems.attributes));
        m_fallItems = ConfigManager.config("Tweaks", "Fall Items", new SerializedItems(m_defaultFallItems).ToString(), new ConfigDescription("Set fall items added to trader", null, SerializedItems.attributes));
        m_winterItems = ConfigManager.config("Tweaks", "Winter Items", new SerializedItems(m_defaultWinterItems).ToString(), new ConfigDescription("Set fall items added to trader", null, SerializedItems.attributes));
        m_springItems = ConfigManager.config("Tweaks", "Spring Items", new SerializedItems(m_defaultSpringItems).ToString(), new ConfigDescription("Set fall items added to trader", null, SerializedItems.attributes));
    }

    private static ConfigEntry<string> GetConfig() => Configs.m_season.Value switch
    {
        Season.Summer => m_summerItems,
        Season.Fall => m_fallItems,
        Season.Winter => m_winterItems,
        Season.Spring => m_springItems,
        _ => m_summerItems,
    };

    [HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
    private static class Trader_GetAvailableItems_Patch
    {
        private static void Postfix(ref List<Trader.TradeItem> __result)
        {
            if (m_enabled.Value is Toggle.Off) return;
            foreach (var item in new SerializedItems(GetConfig().Value).m_items)
            {
                if (item.GetTradeItem() is not { } tradeItem) continue;
                __result.Add(tradeItem);
            }
        }
    }

    
}