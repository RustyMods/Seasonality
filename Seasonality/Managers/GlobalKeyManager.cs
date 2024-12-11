using System.Collections.Generic;
using System.Linq;
using Seasonality.Seasons;

namespace Seasonality.Managers;

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
        string? newKey = GetKey(Configurations._Season.Value);
        if (newKey != null) ZoneSystem.instance.SetGlobalKey(newKey);
    }

    private static string? GetKey(SeasonalityPlugin.Season season)
    {
        return season switch
        {
            SeasonalityPlugin.Season.Winter => "season_winter",
            SeasonalityPlugin.Season.Fall => "season_fall",
            SeasonalityPlugin.Season.Summer => "season_summer",
            SeasonalityPlugin.Season.Spring => "season_spring",
            _ => null
        };
    }
}