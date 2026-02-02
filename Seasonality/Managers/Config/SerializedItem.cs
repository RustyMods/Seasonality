using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Seasonality;

public class SerializedItems
{
    public static ConfigurationManagerAttributes attributes = new(Draw);
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

        int RightColumnWidth = ConfigManager.ConfigManagerRightColumnWidth;
        GUILayout.BeginVertical();
        foreach (var item in originalItems)
        {
            GUILayout.BeginHorizontal();
            var prefab = GUILayout.TextField(item.PrefabName,
                new GUIStyle(GUI.skin.textField) { fixedWidth = RightColumnWidth - 40 - 40 - 21 - 21 });
            var stack = GUILayout.TextField(item.StackSize.ToString(),
                new GUIStyle(GUI.skin.textField) { fixedWidth = 40 });
            var price = GUILayout.TextField(item.Price.ToString(),
                new GUIStyle(GUI.skin.textField) { fixedWidth = 40 });
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
            if (ObjectDB.instance.GetItemPrefab(PrefabName) is { } prefab &&
                prefab.TryGetComponent(out ItemDrop component))
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