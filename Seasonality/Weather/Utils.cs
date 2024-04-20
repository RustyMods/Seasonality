using System.Text.RegularExpressions;
using UnityEngine;

namespace Seasonality.Weather;

public static class Utils
{
    public enum Environments
    {
        None,
        Clear,
        Twilight_Clear,
        Misty,
        Darklands_dark,
        HeathClear,
        DeepForestMist,
        GDKing,
        Rain,
        LightRain,
        ThunderStorm,
        Eikthyr,
        GoblinKing,
        nofogts,
        SwampRain,
        Bonemass,
        Snow,
        Twilight_Snow,
        Twilight_SnowStorm,
        SnowStorm,
        Moder,
        Ashrain,
        Crypt,
        SunkenCrypt,
        Caves,
        Mistlands_clear,
        Mistlands_rain,
        Mistlands_thunder,
        InfectedMine,
        Queen,
        WarmSnow,
        ClearWarmSnow,
        NightFrost,
        WinterClear
    }
    
    public static string GetEnvironmentName(Environments options)
    {
        return options switch
        {
            Environments.None => "",
            Environments.Clear => "Clear",
            Environments.Misty => "Misty",
            Environments.Darklands_dark => "Darklands_dark",
            Environments.HeathClear => "Heath clear",
            Environments.DeepForestMist => "DeepForest Mist",
            Environments.GDKing => "GDKing",
            Environments.Rain => "Rain",
            Environments.LightRain => "LightRain",
            Environments.ThunderStorm => "ThunderStorm",
            Environments.Eikthyr => "Eikthyr",
            Environments.GoblinKing => "GoblinKing",
            Environments.nofogts => "nofogts",
            Environments.SwampRain => "SwampRain",
            Environments.Bonemass => "Bonemass",
            Environments.Snow => "Snow",
            Environments.Twilight_Clear => "Twilight_Clear",
            Environments.Twilight_Snow => "Twilight_Snow",
            Environments.Twilight_SnowStorm => "Twilight_SnowStorm",
            Environments.SnowStorm => "SnowStorm",
            Environments.Moder => "Moder",
            Environments.Ashrain => "AshRain",
            Environments.Crypt => "Crypt",
            Environments.SunkenCrypt => "SunkenCrypt",
            Environments.Caves => "Caves",
            Environments.Mistlands_clear => "Mistlands_clear",
            Environments.Mistlands_rain => "Mistlands_rain",
            Environments.Mistlands_thunder => "Mistlands_thunder",
            Environments.InfectedMine => "InfectedMine",
            Environments.Queen => "Queen",
            Environments.WarmSnow => "WarmSnow",
            Environments.ClearWarmSnow => "ClearWarmSnow",
            Environments.NightFrost => "NightFrost",
            Environments.WinterClear => "WinterClear",
            _ => ""
        };
    }
    
    public static Color GetColorFromString(string input)
    {
        string format = Regex.Replace(input, "[RGBA()]", "").Replace(" ", "");
        string[] values = format.Split(',');
        return new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
    }
    
    public static string GetEnvironmentTooltip(string environment)
    {
        return (environment) switch
        {
            "Clear" => "$weather_clear_tooltip",
            "Misty" => "$weather_misty_tooltip",
            "Darklands_dark" => "$weather_darkland_dark_tooltip",
            "Heath clear" => "$weather_heath_clear_tooltip",
            "DeepForest Mist" => "$weather_deep_forest_mist_tooltip",
            "GDKing" => "$weather_gd_king_tooltip",
            "Rain" => "$weather_rain_tooltip",
            "LightRain" => "$weather_light_rain_tooltip",
            "ThunderStorm" => "$weather_thunderstorm_tooltip",
            "Eikthyr" => "$weather_eikthyr_tooltip",
            "GoblinKing" => "$weather_goblin_king_tooltip",
            "nofogts" => "$weather_no_fog_thunderstorm_tooltip",
            "SwampRain" => "$weather_swamp_rain_tooltip",
            "Bonemass" => "$weather_bonemass_tooltip",
            "Snow" => "$weather_snow_tooltip",
            "Twilight_Clear" => "$weather_twilight_clear_tooltip",
            "Twilight_Snow" => "$weather_twilight_snow_tooltip",
            "Twilight_SnowStorm" => "$weather_twilight_snowstorm_tooltip",
            "SnowStorm" => "$weather_snowstorm_tooltip",
            "Moder" => "$weather_moder_tooltip",
            "AshRain" => "$weather_ash_rain_tooltip",
            "Crypt" => "$weather_crypt_tooltip",
            "SunkenCrypts" => "$weather_sunken_crypt_tooltip",
            "Caves" => "$weather_caves_tooltip",
            "Mistlands_clear" => "$weather_mistland_clear_tooltip",
            "Mistlands_rain" => "$weather_mistland_rain_tooltip",
            "Mistlands_thunder" => "$weather_mistland_thunder_tooltip",
            "InfectedMine" => "$weather_infected_mine_tooltip",
            "Queen" => "$weather_queen_tooltip",
            "WarmSnow" => "$weather_warm_snow_tooltip",
            "ClearWarmSnow" => "$weather_clear_warm_snow_tooltip",
            "NightFrost" => "$weather_night_frost_tooltip",
            _ => ""
        };
    }
    
    public static string GetEnvironmentDisplayName(string environment)
    {
        return (environment) switch
        {
            "Clear" => "$weather_clear",
            "Misty" => "$weather_misty",
            "Darklands_dark" => "$weather_darklands_dark",
            "Heath clear" => "$weather_heath_clear",
            "GDKing" => "$weather_gd_king",
            "LightRain" => "$weather_light_rain",
            "ThunderStorm" => "$weather_thunderstorm",
            "GoblinKing" => "$weather_goblin_king",
            "nofogts" => "$weather_no_fog_thunderstorm",
            "SwampRain" => "$weather_swamp_rain",
            "Bonemass" => "$weather_bonemass",
            "Twilight_Clear" => "$weather_twilight_clear",
            "Twilight_Snow" => "$weather_twilight_snow",
            "Twilight_SnowStorm" => "$weather_twilight_snowstorm",
            "SnowStorm" => "$weather_snowstorm",
            "Moder" => "$weather_moder",
            "Ashrain" => "$weather_ash_rain",
            "SunkenCrypts" => "$weather_sunken_crypts",
            "Mistlands_clear" => "$weather_mistland_clear",
            "Mistlands_rain" => "$weather_mistland_rain",
            "Mistlands_thunder" => "$weather_mistland_thunder",
            "InfectedMine" => "$weather_infected_mine",
            "Queen" => "$weather_seeker_queen",
            "Snow" => "$weather_snow",
            "Rain" => "$weather_rain",
            "WarmSnow" => "$weather_warm_snow",
            "ClearWarmSnow" => "$weather_clear_warm_snow",
            "NightFrost" => "$weather_night_frost",
            "WinterClear" => "$weather_winter_clear",
            "DeepForest Mist" => "$weather_deep_forest_mist",
            "Eikthyr" => "$enemy_eikthyr",
            "Crypt" => "$weather_crypt",
            "Caves" => "$weather_caves",
            _ => environment
        };
    }
}