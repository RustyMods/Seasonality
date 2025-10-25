using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;
using UnityEngine;

namespace Seasonality.GameplayModifiers;

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
        m_enabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Trader Enabled", Toggle.Off, "If on, trader has extra items for sale depending on season");
        m_summerItems = SeasonalityPlugin.ConfigManager.config("Tweaks", "Summer Items", new SerializedItems(m_defaultSummerItems).ToString(), new ConfigDescription("Set summer items added to trader", null, new Configs.ConfigurationManagerAttributes()
        {
            Category = "Tweaks",
            CustomDrawer = SerializedItems.Draw
        }));
        m_fallItems = SeasonalityPlugin.ConfigManager.config("Tweaks", "Fall Items", new SerializedItems(m_defaultFallItems).ToString(), new ConfigDescription("Set fall items added to trader", null, new Configs.ConfigurationManagerAttributes()
        {
            Category = "Tweaks",
            CustomDrawer = SerializedItems.Draw
        }));
        m_winterItems = SeasonalityPlugin.ConfigManager.config("Tweaks", "Winter Items", new SerializedItems(m_defaultWinterItems).ToString(), new ConfigDescription("Set fall items added to trader", null, new Configs.ConfigurationManagerAttributes()
        {
            Category = "Tweaks",
            CustomDrawer = SerializedItems.Draw
        }));
        m_springItems = SeasonalityPlugin.ConfigManager.config("Tweaks", "Spring Items", new SerializedItems(m_defaultSpringItems).ToString(), new ConfigDescription("Set fall items added to trader", null, new Configs.ConfigurationManagerAttributes()
        {
            Category = "Tweaks",
            CustomDrawer = SerializedItems.Draw
        }));
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

    public class SerializedItems
    {
        public readonly List<SeasonalItem> m_items = new();

        public SerializedItems(List<SeasonalItem> items) => m_items = items;

        public SerializedItems(string config)
        {
            foreach (var part in config.Split(','))
            {
                var item = new SeasonalItem(part);
                if (!item.isValid) continue;
                m_items.Add(item);
            }
        }
        
        public override string ToString() => string.Join(",", m_items.Select(item => item.ToString()));

        public static void Draw(ConfigEntryBase cfg)
        {
            bool locked = cfg.Description.Tags
                .Select(a =>
                    a.GetType().Name == "ConfigurationManagerAttributes"
                        ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                        : null).FirstOrDefault(v => v != null) ?? false;

            var originalItems = new SerializedItems((string)cfg.BoxedValue).m_items;
            if (originalItems.Count <= 0)
            {
                originalItems.Add(new SeasonalItem("", 1, 999));
            }
            List<SeasonalItem> newItems = new();
            bool wasUpdated = false;
            
            int RightColumnWidth =
                (int)(Configs.configManager?.GetType()
                    .GetProperty("RightColumnWidth",
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetGetMethod(true)
                    .Invoke(Configs.configManager, Array.Empty<object>()) ?? 130);
            GUILayout.BeginVertical();
            foreach (var item in originalItems)
            {
                GUILayout.BeginHorizontal();
                var prefab = GUILayout.TextField(item.PrefabName, new GUIStyle(GUI.skin.textField) {fixedWidth = RightColumnWidth - 40 - 40 - 21 - 21});
                var stack = GUILayout.TextField(item.StackSize.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 40 });
                var price = GUILayout.TextField(item.Price.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 40 });
                var prefabName = locked ? item.PrefabName : prefab;
                var stackSize = locked ? item.StackSize : int.TryParse(stack, out int size) ? size : item.StackSize;
                var priceAmount = locked ? item.Price : int.TryParse(price, out int cost) ? cost : item.Price;
                if (prefabName != item.PrefabName || stackSize != item.StackSize || priceAmount != item.Price)
                {
                    wasUpdated = true;
                }
                var remove = GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 });
                var add = GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 });
                if (remove && !locked) wasUpdated = true;
                else newItems.Add(new SeasonalItem(prefabName, stackSize, priceAmount));
                if (add && !locked)
                {
                    newItems.Add(new SeasonalItem("", 1, 999));
                    wasUpdated = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            if (wasUpdated)
            {
                cfg.BoxedValue = new SerializedItems(newItems).ToString();
            }
        }
        
        public class SeasonalItem
        {
            public readonly string PrefabName = "";
            public readonly int StackSize = 1;
            public readonly int Price = 999;
            public readonly bool isValid = true;

            public SeasonalItem(string prefab, int stack, int price)
            {
                PrefabName = prefab;
                StackSize = stack;
                Price = price;
            }

            public SeasonalItem(string config)
            {
                var parts = config.Split(':');
                if (parts.Length < 3)
                {
                    isValid = false;
                    return;
                }
                PrefabName = parts[0];
                StackSize = int.TryParse(parts[1], out int size) ? size : 1;
                Price = int.TryParse(parts[2], out int cost) ? cost : 999;
            }

            public Trader.TradeItem? GetTradeItem()
            {
                if (ObjectDB.instance.GetItemPrefab(PrefabName) is { } prefab && prefab.TryGetComponent(out ItemDrop component))
                {
                    return new Trader.TradeItem()
                    {
                        m_prefab = component,
                        m_stack = StackSize,
                        m_price = Price,
                    };
                }

                return null;
            }

            public override string ToString() => $"{PrefabName}:{StackSize}:{Price}";
        }
    }
}