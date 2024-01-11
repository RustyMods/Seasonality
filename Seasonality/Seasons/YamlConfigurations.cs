using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using JetBrains.Annotations;
using ServerSync;
using YamlDotNet.Serialization;

namespace Seasonality.Seasons;

public static class YamlConfigurations
{
    private static readonly CustomSyncedValue<string> SyncedSpringData = new(SeasonalityPlugin.ConfigSync, "SpringData", "");
    private static readonly CustomSyncedValue<string> SyncedSummerData = new(SeasonalityPlugin.ConfigSync, "SummerData", "");
    private static readonly CustomSyncedValue<string> SyncedFallData = new(SeasonalityPlugin.ConfigSync, "FallData", "");
    private static readonly CustomSyncedValue<string> SyncedWinterData = new(SeasonalityPlugin.ConfigSync, "WinterData", "");
    
    [Serializable]
    [CanBeNull]
    public class ConfigurationData
    {
        public string name = null!;
        public Dictionary<Modifier, float> modifiers = new ()
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
        public List<HitData.DamageModPair> resistances = new ()
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
    }

    private static readonly string SeasonalityFolderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    private static readonly string SpringFilePath = SeasonalityFolderPath + Path.DirectorySeparatorChar + "spring_configurations.yml";
    private static readonly string SummerFilePath = SeasonalityFolderPath + Path.DirectorySeparatorChar + "summer_configurations.yml";
    private static readonly string FallFilePath = SeasonalityFolderPath + Path.DirectorySeparatorChar + "fall_configurations.yml";
    private static readonly string WinterFilePath = SeasonalityFolderPath + Path.DirectorySeparatorChar + "winter_configurations.yml";

    private static readonly ConfigurationData SpringDefaultConfigurations = new()
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
        string filePath = SeasonalityFolderPath + Path.DirectorySeparatorChar + "YML_README.md";
        if (!File.Exists(filePath)) File.WriteAllLines(filePath, tutorial);
    }
    public static void InitYamlConfigurations()
    {
        WriteTutorial();
        
        WriteDefaultConfigurations();
        
        // Set local yml data
        springData = ReadFile(SpringFilePath, SpringDefaultConfigurations);
        summerData = ReadFile(SummerFilePath, SummerDefaultConfigurations);
        fallData = ReadFile(FallFilePath, FallDefaultConfigurations);
        winterData = ReadFile(WinterFilePath, WinterDefaultConfigurations);
        
        // Set server data
        if (SeasonalityPlugin.workingAsType is SeasonalityPlugin.WorkingAs.Server)
        {
            SyncedSpringData.Value = ReadFileRaw(SpringFilePath);
            SyncedSummerData.Value = ReadFileRaw(SummerFilePath);
            SyncedFallData.Value = ReadFileRaw(FallFilePath);
            SyncedWinterData.Value = ReadFileRaw(WinterFilePath);
        }
    }
    public static void SetServerSyncedYmlData()
    {
        if (SeasonalityPlugin.workingAsType != SeasonalityPlugin.WorkingAs.Client) return;
        springData = ReadSyncedData(SyncedSpringData.Value, SpringDefaultConfigurations);
        summerData = ReadSyncedData(SyncedSummerData.Value, SummerDefaultConfigurations);
        fallData = ReadSyncedData(SyncedFallData.Value, FallDefaultConfigurations);
        winterData = ReadSyncedData(SyncedWinterData.Value, WinterDefaultConfigurations);
    }
    private static ConfigurationData ReadSyncedData(string serverData, ConfigurationData defaultData)
    {
        if (serverData == "") return defaultData;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            ConfigurationData data = deserializer.Deserialize<ConfigurationData>(serverData);
            return data;
        }
        catch (Exception)
        {
            SeasonalityPlugin.SeasonalityLogger.LogFatal("Server YML configuration error: ");
            SeasonalityPlugin._YamlConfigurations.Value = SeasonalityPlugin.Toggle.Off;
        }

        return defaultData;
    }
    private static string ReadFileRaw(string filePath) => File.ReadAllText(filePath);
    private static ConfigurationData ReadFile(string filePath, ConfigurationData defaultData)
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();

        try
        {
            ConfigurationData data = deserializer.Deserialize<ConfigurationData>(File.ReadAllText(filePath));
            return data;
        }
        catch (Exception)
        {
            SeasonalityPlugin.SeasonalityLogger.LogFatal("YML configuration error: ");
            SeasonalityPlugin.SeasonalityLogger.LogWarning(filePath);
            SeasonalityPlugin._YamlConfigurations.Value = SeasonalityPlugin.Toggle.Off;
        }

        return defaultData;
    }
    private static void WriteDefaultConfigurations()
    {
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
    }
}