using System;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace Seasonality;

public class ConfigurationManagerAttributes
{
    [UsedImplicitly] public int? Order = null!;
    [UsedImplicitly] public bool? Browsable = null!;
    [UsedImplicitly] public string? Category = null!;
    [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;

    public ConfigurationManagerAttributes()
    {
        
    }
    
    public ConfigurationManagerAttributes(Action<ConfigEntryBase>? customDrawer)
    {
        CustomDrawer = customDrawer;
    }
}