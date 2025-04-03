using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Seasonality.Textures;
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

    public static void AddOrSet(this Dictionary<string, TextureManager.TexturePack> dict, string materialName, TextureManager.ImageData data)
    {
        if (!dict.TryGetValue(materialName, out TextureManager.TexturePack pack))
        {
            var _ = new TextureManager.TexturePack(data);
        }
        else
        {
            pack.Add(data);
        }
    }

    public static bool ParseName(string name, out string materialName, out Configs.Season season, out string property)
    {
        materialName = "";
        property = "";
        season = Configs.Season.Summer;
        var parts = name.Split('@');
        if (parts.Length < 2)
        {
            SeasonalityPlugin.Record.LogWarning($"Invalid file name: {name}, missing @ to distinguish season");
            return false;
        }

        if (!Enum.TryParse(parts[1], true, out season))
        {
            SeasonalityPlugin.Record.LogWarning($"Invalid season: {name} - [{parts[1]}] is invalid");
            return false;
        }
        materialName = parts[0];
        if (materialName.Contains("#"))
        {
            var matParts = materialName.Split('#');
            materialName = matParts[0];
            property = "_" + matParts[1];
        }

        return true;
    }
}