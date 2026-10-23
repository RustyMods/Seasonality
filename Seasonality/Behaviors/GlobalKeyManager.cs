using System.Linq;

namespace Seasonality;

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
        ZoneSystem.instance.SetGlobalKey($"season_{SeasonTime.GetCurrentSeason().ToString().ToLower()}");
    }
}