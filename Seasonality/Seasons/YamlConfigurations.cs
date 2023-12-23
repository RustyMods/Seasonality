using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Seasonality.Seasons;

public static class YamlConfigurations
{
    [Serializable]
    [CanBeNull]
    public class ConfigurationData
    {
        public string name = null!;
        public Dictionary<Modifier, float> modifiers = new Dictionary<Modifier, float>()
        {
            { Modifier.Attack, 1f },
            { Modifier.HealthRegen , 1f },
            { Modifier.StaminaRegen , 1f },
            { Modifier.RaiseSkills , 1f },
            { Modifier.Speed , 1f },
            { Modifier.Noise , 1f },
            { Modifier.MaxCarryWeight , 0f },
            { Modifier.Stealth , 1f },
            { Modifier.RunStaminaDrain , 1f },
            { Modifier.DamageReduction , 0f },
            { Modifier.FallDamage , 1f },
            { Modifier.EitrRegen , 1f }
        };
        public List<HitData.DamageModPair> resistances = new List<HitData.DamageModPair>()
        {
            new () {m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal},
        };
        public string startMessage = null!;
        public string tooltip = null!;
        public List<string> meadowWeather = new();
        public List<string> blackForestWeather = new();
        public List<string> swampWeather = new();
        public List<string> mountainWeather = new();
        public List<string> plainWeather = new();
        public List<string> mistLandWeather = new();
        public List<string> ashLandWeather = new();
        public List<string> deepNorthWeather = new();
        public List<string> oceanWeather = new();
        public List<string> colors = new();
    }
    
    private static readonly string SpringFilePath =
        Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality" + Path.DirectorySeparatorChar +
        "spring_configurations.yml";
    private static readonly string SummerFilePath =
        Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality" + Path.DirectorySeparatorChar +
        "summer_configurations.yml";
    private static readonly string FallFilePath =
        Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality" + Path.DirectorySeparatorChar +
        "fall_configurations.yml";
    private static readonly string WinterFilePath =
        Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality" + Path.DirectorySeparatorChar +
        "winter_configurations.yml";
    
    private static readonly List<string> filePaths = new()
    {
        SpringFilePath, SummerFilePath, FallFilePath, WinterFilePath
    };

    private static readonly ConfigurationData SpringDefaultConfigurations = new ConfigurationData()
    {
        name = "Spring",
        modifiers = new Dictionary<Modifier, float>()
        {
            { Modifier.Attack, 1f },
            { Modifier.HealthRegen , 1f },
            { Modifier.StaminaRegen , 1f },
            { Modifier.RaiseSkills , 1.1f },
            { Modifier.Speed , 0.9f },
            { Modifier.Noise , 1f },
            { Modifier.MaxCarryWeight , 0f },
            { Modifier.Stealth , 1f },
            { Modifier.RunStaminaDrain , 1f },
            { Modifier.DamageReduction , 0f },
            { Modifier.FallDamage , 1f },
            { Modifier.EitrRegen , 1.1f }
        },
        resistances = new List<HitData.DamageModPair>()
        {
            new () {m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Resistant},
        },
        startMessage = "Spring has finally arrived",
        tooltip = "The land is bursting with energy",
        colors = new List<string>(){"#80CC33B2","#FFCC33FF","#FF4C80FF","#FF4C99FF"}
    };

    private static readonly ConfigurationData FallDefaultConfigurations = new()
    {
        name = "Autumn",
        modifiers = new Dictionary<Modifier, float>()
        {
            { Modifier.Attack, 1f },
            { Modifier.HealthRegen , 1f },
            { Modifier.StaminaRegen , 1f },
            { Modifier.RaiseSkills , 1f },
            { Modifier.Speed , 0.9f },
            { Modifier.Noise , 2f },
            { Modifier.MaxCarryWeight , 0f },
            { Modifier.Stealth , 1f },
            { Modifier.RunStaminaDrain , 1f },
            { Modifier.DamageReduction , 0f },
            { Modifier.FallDamage , 1f },
            { Modifier.EitrRegen , 1f }
        },
        resistances = new List<HitData.DamageModPair>()
        {
            new () {m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal},
        },
        startMessage = "Fall is upon us",
        tooltip = "The ground is wet",
        colors = new List<string>(){"#CC8000FF","#CC4C00FF","#CC3300FF","#E68000FF"}
    };

    private static readonly ConfigurationData SummerDefaultConfigurations = new()
    {
        name = "Summer",
        modifiers = new Dictionary<Modifier, float>()
        {
            { Modifier.Attack, 1f },
            { Modifier.HealthRegen , 1f },
            { Modifier.StaminaRegen , 1f },
            { Modifier.RaiseSkills , 1f },
            { Modifier.Speed , 1.1f },
            { Modifier.Noise , 1f },
            { Modifier.MaxCarryWeight , 50f },
            { Modifier.Stealth , 1f },
            { Modifier.RunStaminaDrain , 1f },
            { Modifier.DamageReduction , 0f },
            { Modifier.FallDamage , 1f },
            { Modifier.EitrRegen , 1f }
        },
        resistances = new List<HitData.DamageModPair>()
        {
            new () {m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Weak},
            new () {m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal},
        },
        startMessage = "Summer has landed",
        tooltip = "The air is warm",
        colors = new List<string>(){"#80B233FF","#B2B233FF","#808000FF","#B2B200FF"}
    };

    private static readonly ConfigurationData WinterDefaultConfigurations = new()
    {
        name = "Winter",
        modifiers = new Dictionary<Modifier, float>()
        {
            { Modifier.Attack, 1f },
            { Modifier.HealthRegen , 0.9f },
            { Modifier.StaminaRegen , 0.9f },
            { Modifier.RaiseSkills , 1f },
            { Modifier.Speed , 0.9f },
            { Modifier.Noise , 1f },
            { Modifier.MaxCarryWeight , 0f },
            { Modifier.Stealth , 1f },
            { Modifier.RunStaminaDrain , 1f },
            { Modifier.DamageReduction , 0f },
            { Modifier.FallDamage , 0.9f },
            { Modifier.EitrRegen , 1f }
        },
        resistances = new List<HitData.DamageModPair>()
        {
            new () {m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant},
            new () {m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Normal},
            new () {m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Normal},
        },
        startMessage = "Winter is coming!",
        tooltip = "The air is cold",
        colors = new List<string>(){"#B2B2B2FF","#B2B2B2FF","#B2B2B2FF","#B2B2B2FF"},
        meadowWeather = new List<string>(){"WarmSnow"},
        blackForestWeather =  new List<string>(){"WarmSnow"},
        swampWeather = new List<string>(){"WarmSnow"},
        plainWeather = new List<string>(){"WarmSnow"},
        mistLandWeather = new List<string>(){"WarmSnow"},
        oceanWeather = new List<string>(){"WarmSnow"},
    };
    public static ConfigurationData springData = SpringDefaultConfigurations;
    public static ConfigurationData summerData = SummerDefaultConfigurations;
    public static ConfigurationData fallData = FallDefaultConfigurations;
    public static ConfigurationData winterData = WinterDefaultConfigurations;

    private static void WriteTutorial()
    {
        List<string> tutorial = new()
        {
            "# Seasonality YAML configurations",
            "Use these files to create a more in-depth seasonal experience",
            "",
            "## DISCLAIMER",
            "Formatting the configurations this way allows for greater control, but also greater chance of creating errors if you do not set values with expected inputs",
            "Use this file as reference to find correct syntax",
            "",
            "## RESISTANCES",
            "```yml",
            "- VeryWeak",
            "- Weak",
            "- Normal",
            "- Resistant",
            "- VeryResistant",
            "- Immune",
            "- Ignore",
            "```",
            "Any resistances set as 'Normal' will be ignored",
            "",
            "## ENVIRONMENTS",
            "```yml"
        };
        foreach (Environment.Environments value in Enum.GetValues(typeof(Environment.Environments)))
        {
            string env = value.ToString();
            tutorial.Add($"- {env}");
        }

        List<string> nextTutorial = new()
        {
            "```",
            "- Any brackets ( [ ] ) you see in the yml file can use different types of syntax",
            "- Either add strings within brackets",
            "- example:",
            "```yml",
            " meadowWeather: ['WarmSnow','Snow','Clear']",
            "```",
            "- Or replace with a list",
            "- example:",
            "```yml",
            "meadowWeather:",
            "- WarmSnow",
            "- Snow",
            "- Clear",
            "```"
        };
        tutorial.AddRange(nextTutorial);
        string filePath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality" + Path.DirectorySeparatorChar +
                          "YML_README.md";
        if (!File.Exists(filePath))
        {
            File.WriteAllLines(filePath, tutorial);
        }
    }

    public static void ReadYamlFile()
    {
        WriteTutorial();
        
        ISerializer serializer = new SerializerBuilder().Build();
        if (!File.Exists(SpringFilePath))
        {
            string data = serializer.Serialize(SpringDefaultConfigurations);
            File.WriteAllText(SpringFilePath, data);
        }
        if (!File.Exists(SummerFilePath))
        {
            string data = serializer.Serialize(SummerDefaultConfigurations);
            File.WriteAllText(SummerFilePath, data);
        }
        if (!File.Exists(FallFilePath))
        {
            string data = serializer.Serialize(FallDefaultConfigurations);
            File.WriteAllText(FallFilePath, data);
        }
        if (!File.Exists(WinterFilePath))
        {
            string data = serializer.Serialize(WinterDefaultConfigurations);
            File.WriteAllText(WinterFilePath, data);
        }

        IDeserializer deserializer = new DeserializerBuilder().Build();

        foreach (string path in filePaths)
        {
            string input = File.ReadAllText(path);
            try
            {
                ConfigurationData data = deserializer.Deserialize<ConfigurationData>(input);
                if (path == SpringFilePath) springData = data;
                if (path == SummerFilePath) summerData = data;
                if (path == FallFilePath) fallData = data;
                if (path == WinterFilePath) winterData = data;
            }
            catch (Exception)
            {
                SeasonalityPlugin.SeasonalityLogger.LogFatal("YML configuration error: ");
                SeasonalityPlugin.SeasonalityLogger.LogWarning(path);
                SeasonalityPlugin._YamlConfigurations.Value = SeasonalityPlugin.Toggle.Off;
                break;
            }
        }
    }
}