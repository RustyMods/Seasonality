using System.Collections.Generic;
using UnityEngine;

namespace Seasonality.Seasons;

public static class SeasonColors
{
    // Default colors
    public static readonly List<Color> FallColors = new()
    {
        new Color(0.8f, 0.5f, 0f, 1f),
        new Color(0.8f, 0.3f, 0f, 1f),
        new Color(0.8f, 0.2f, 0f, 1f),
        new Color(0.9f, 0.5f, 0f, 1f)
    };

    public static readonly List<Color> SpringColors = new()
    {
        new Color(0.5f, 0.8f, 0.2f, 0.7f),
        new Color(1f, 0.8f, 0.2f, 1f),
        new Color(1f, 0.3f, 0.5f, 1f),
        new Color(1f, 0.3f, 0.6f, 1f)
    };

    public static readonly List<Color> SummerColors = new()
    {
        new Color(0.5f, 0.7f, 0.2f, 1f),
        new Color(0.7f, 0.7f, 0.2f, 1f),
        new Color(0.5f, 0.5f, 0f, 1f),
        new Color(0.7f, 0.7f, 0f, 1f)
    };

    public static readonly List<Color> WinterColors = new()
    {
        new Color(0.7f, 0.7f, 0.7f, 1f),
        new Color(0.5f, 0.5f, 0.5f, 1f),
        new Color(0.5f, 0.7f, 0.7f, 1f),
        new Color(0.5f, 0.5f, 0.7f, 1f)
    };
}