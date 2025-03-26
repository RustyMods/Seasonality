using System.Collections.Generic;
using System.Linq;
using Seasonality.Helpers;

namespace Seasonality.Behaviors;

public static class GlobalKeyManager
{
    public static void UpdateSeasonalKey()
    {
        if (!ZoneSystem.instance) return;
        ClearSeasonalKeys();
        SetKey();
    }

    private static void ClearSeasonalKeys()
    {
        List<string> current = ZoneSystem.instance.GetGlobalKeys();
        foreach (var key in current.Where(key => key.StartsWith("season_")))
        {
            ZoneSystem.instance.RemoveGlobalKey(key);
        }
    }

    private static void SetKey()
    {
        string? newKey = GetKey(Configs.m_season.Value);
        if (newKey != null) ZoneSystem.instance.SetGlobalKey(newKey);
    }

    private static string? GetKey(Configs.Season season)
    {
        return season switch
        {
            Configs.Season.Winter => "season_winter",
            Configs.Season.Fall => "season_fall",
            Configs.Season.Summer => "season_summer",
            Configs.Season.Spring => "season_spring",
            _ => null
        };
    }
}