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
        if (!ZoneSystem.instance) return;
        foreach (var key in ZoneSystem.instance.GetGlobalKeys().ToList())
        {
            if (key.StartsWith("season_")) ZoneSystem.instance.RemoveGlobalKey(key);
        }
    }

    private static void SetKey()
    {
        if (!ZoneSystem.instance) return;
        ZoneSystem.instance.SetGlobalKey($"season_{Configs.m_season.Value.ToString().ToLower()}");
    }
}