using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace Seasonality.Helpers;

public static class Utils
{
    public static void AddOrSet<T, J, V>(this Dictionary<T, Dictionary<J, V>> dict, T t, J j, V v)
    {
        if (!dict.ContainsKey(t)) dict[t] = new Dictionary<J, V>();
        dict[t][j] = v;
    }

    public static float GetOrDefault<T, J>(this Dictionary<T, Dictionary<J, ConfigEntry<float>>> dict, T t, J j, float d)
    {
        if (!dict.TryGetValue(t, out Dictionary<J, ConfigEntry<float>> s)) return d;
        if (!s.TryGetValue(j, out ConfigEntry<float> result)) return d;
        return result.Value;
    }
    
    public static float GetOrDefault<J>(this Dictionary<Configs.Season, Dictionary<J, ConfigEntry<float>>> dict, J j, float d)
    {
        return dict.GetOrDefault(Configs.m_season.Value, j, d);
    }

    public static void AddOrSetNull<T, J>(this Dictionary<T, Dictionary<J, Texture?>> dict, T TKey, J OKey, Texture? value = null)
    {
        if (!dict.ContainsKey(TKey)) dict[TKey] = new Dictionary<J, Texture?>();
        dict[TKey][OKey] = value;
    }

    public static Vector3 GetOrDefault<T>(this Dictionary<T, ConfigEntry<Vector3>> dict, T season, Vector3 defaultValue)
    {
        return !dict.TryGetValue(season, out ConfigEntry<Vector3> config) ? defaultValue : config.Value;
    }
}