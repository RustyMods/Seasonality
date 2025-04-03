using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Helpers;

namespace Seasonality.GameplayModifiers;

public static class BuildTweaks
{
    private static ConfigEntry<Configs.Toggle> m_enabled = null!;
    private static ConfigEntry<string> m_winterPieces = null!;
    private static ConfigEntry<string> m_springPieces = null!;
    private static ConfigEntry<string> m_fallPieces = null!;
    private static ConfigEntry<string> m_summerPieces = null!;

    private static readonly List<string> m_defaultWinterPieces = new()
    {
        "piece_xmascrown", "piece_xmastree", "piece_xmasgarland", "piece_mistletoe", "piece_gift1", "piece_gift2", "piece_gift3"
    };

    private static readonly List<string> m_defaultSpringPieces = new();
    private static readonly List<string> m_defaultFallPieces = new()
    {
        "piece_jackoturnip"
    };

    private static readonly List<string> m_defaultSummerPieces = new()
    {
        "piece_maypole"
    };

    public static void Setup()
    {
        m_enabled = SeasonalityPlugin.ConfigManager.config("Tweaks", "Build Pieces Enabled", Configs.Toggle.Off,
            "If on, seasonal build pieces enabled");
        m_winterPieces = SeasonalityPlugin.ConfigManager.config("Tweaks", "Winter Pieces",
            new Configs.SerializedNameList(m_defaultWinterPieces).ToString(), new ConfigDescription(
                "Set added winter pieces", null, new Configs.ConfigurationManagerAttributes()
                {
                    Category = "Tweaks",
                    CustomDrawer = Configs.SerializedNameList.Draw
                }));
        m_springPieces = SeasonalityPlugin.ConfigManager.config("Tweaks", "Spring Pieces",
            new Configs.SerializedNameList(m_defaultSpringPieces).ToString(), new ConfigDescription(
                "Set added spring pieces", null, new Configs.ConfigurationManagerAttributes()
                {
                    Category = "Tweaks",
                    CustomDrawer = Configs.SerializedNameList.Draw
                }));
        m_fallPieces = SeasonalityPlugin.ConfigManager.config("Tweaks", "Fall Pieces",
            new Configs.SerializedNameList(m_defaultFallPieces).ToString(), new ConfigDescription(
                "Set added fall pieces", null, new Configs.ConfigurationManagerAttributes()
                {
                    Category = "Tweaks",
                    CustomDrawer = Configs.SerializedNameList.Draw
                }));
        m_summerPieces = SeasonalityPlugin.ConfigManager.config("Tweaks", "Summer Pieces",
            new Configs.SerializedNameList(m_defaultSummerPieces).ToString(), new ConfigDescription(
                "Set added summer pieces", null, new Configs.ConfigurationManagerAttributes()
                {
                    Category = "Tweaks",
                    CustomDrawer = Configs.SerializedNameList.Draw
                }));
    }

    private static ConfigEntry<string> GetConfig() => Configs.m_season.Value switch
    {
        Configs.Season.Summer => m_summerPieces,
        Configs.Season.Spring => m_springPieces,
        Configs.Season.Fall => m_fallPieces,
        Configs.Season.Winter => m_winterPieces,
        _ => m_summerPieces
    };

    [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
    private static class PieceTable_UpdateAvailable
    {
        private static void Postfix(PieceTable __instance, Player player)
        {
            if (m_enabled.Value is Configs.Toggle.Off) return;
            foreach (var name in new Configs.SerializedNameList(GetConfig().Value).m_names)
            {
                if (ZNetScene.instance.GetPrefab(name) is not { } prefab || !prefab.TryGetComponent(out Piece component)) continue;
                if (!player.HaveRequirements(component, Player.RequirementMode.CanAlmostBuild)) continue;
                if (component.m_category == Piece.PieceCategory.All)
                {
                    foreach (var list in __instance.m_availablePieces.Where(list => !list.Contains(component)))
                    {
                        list.Add(component);
                    }
                }  
                else
                {
                    var list = __instance.m_availablePieces[(int)component.m_category];
                    if (!list.Contains(component)) list.Add(component);
                }
            }
        }
    }

}