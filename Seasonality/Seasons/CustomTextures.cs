using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.HDConfigurations;

namespace Seasonality.Seasons;

public static class CustomTextures
{
    public static readonly Sprite? ValknutIcon = RegisterSprite("valknutIcon.png");
    public static readonly Texture? MistLands_Moss = RegisterTexture("mistlands_moss.png");
    public static readonly Texture? Mistlands_Rock_Plant = RegisterTexture("MistlandsVegetation_d.png");

    public static readonly Dictionary<VegDirectories, Dictionary<string, Texture?>> CustomRegisteredTextures = new();
    public static readonly Dictionary<CreatureDirectories, Dictionary<string, Texture?>> CustomRegisteredCreatureTex = new();
    public static readonly Dictionary<PieceDirectories, Dictionary<string, Texture?>> CustomRegisteredPieceTextures = new();
    public static readonly Dictionary<PickableDirectories, Dictionary<string, Texture?>> CustomRegisteredPickableTex = new();
    public static readonly Dictionary<ArmorDirectories, Dictionary<string, Texture?>> CustomRegisteredArmorTex = new();
    private static Texture? RegisterTexture(string fileName, string folderName = "assets")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{ModName}.{folderName}.{fileName}";

        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);

        return texture.LoadImage(buffer) ? texture : null;
    }
    private static Sprite? RegisterSprite(string fileName, string folderName = "icons")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{ModName}.{folderName}.{fileName}";
        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        
        Sprite? sprite = texture.LoadImage(buffer) ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
        if (sprite != null) sprite.name = fileName;
        return sprite;
    }
    private static Texture? RegisterCustomTexture(string filePath, TextureFormat format, FilterMode filter, int aniso = 1, int mipMapBias = 0, TextureWrapMode wrap = TextureWrapMode.Repeat, bool isBark = false)
    {
        if (!File.Exists(filePath)) return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(4, 4, format, true, false)
        {
            filterMode = filter,
        };
        if (isBark)
        {
            texture.anisoLevel = aniso;
            texture.mipMapBias = mipMapBias;
            texture.wrapMode = wrap;
        }

        if (texture.LoadImage(fileData))
        {
            // if (isBark)
            // {
            //     texture.name = filePath;
            //     SeasonalityLogger.LogInfo("*** name : " + texture.name);
            //     SeasonalityLogger.LogInfo("format : " + texture.format);
            //     SeasonalityLogger.LogInfo("mip map bias : " + texture.mipMapBias);
            //     SeasonalityLogger.LogInfo("mip map count : " + texture.mipmapCount);
            //     SeasonalityLogger.LogInfo("aniso level : " +  texture.anisoLevel);
            //     SeasonalityLogger.LogInfo("texel size : " + texture.texelSize);
            //     SeasonalityLogger.LogInfo("filter mode : " + texture.filterMode);
            // }
            texture.name = filePath;
            return texture;
        }
        return null;
    }
    
    private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    private static readonly string VegTexturePath = folderPath + Path.DirectorySeparatorChar + "Textures";
    private static readonly string creatureTexPath = folderPath + Path.DirectorySeparatorChar + "Creatures";
    private static readonly string PieceTexPath = folderPath + Path.DirectorySeparatorChar + "Pieces";
    private static readonly string PickableTexturePath = folderPath + Path.DirectorySeparatorChar + "Pickables";
    private static readonly string ArmorTexPath = folderPath + Path.DirectorySeparatorChar + "Armors";

    public enum PickableDirectories
    {
        None,
        Mushroom,
        MushroomYellow,
        MushroomBlue,
        JotunPuff,
        Magecap,
        Raspberry,
        Blueberry,
        Dandelion,
        Barley,
        Flax,
        Carrot,
        Turnip,
        Onion,
        CarrotSeed,
        TurnipSeed,
        OnionSeed,
        Branches,
        Flint,
        Rock,
        BoneRemains,
        SurtlingCore,
        BlackCore,
        Thistle
    }
    public enum VegDirectories
    {
        Beech,
        BeechSmall,
        Birch,
        Bushes,
        Oak,
        Pine,
        PineParticles,
        FirSmall,
        FirDead,
        FirDeadParticles,
        Fir,
        FirParticles,
        YggaShoot,
        SwampTrees,
        PlainsBush,
        Shrub,
        ShrubParticles,
        Rock,
        MeadowGrass,
        MeadowGrassShort,
        PlainsGrass,
        BlackForestGrass,
        BlackForestGrassAlt,
        SwampGrass,
        MistlandsGrass,
        PlainsFlowers,
        Ormbunke,
        Vass,
        Vines,
        WaterLilies,
        RockPlant,
        Clutter,
        CloudberryBush,
        YggdrasilSkyTree,
        None
    }
    public enum PieceDirectories
    {
        None,
        Straw,
        DarkWood,
        GoblinVillage
    }
    public enum CreatureDirectories
    {
        None,
        Gull,
        Serpent,
        Deer,
        Boar,
        Piggy,
        Neck,
        Greydwarf,
        GreydwarfShaman,
        Troll,
        Skeleton,
        Leech,
        Wraith,
        Blob,
        Oozer,
        Wolf,
        WolfCub,
        Lox,
        LoxCalf,
        Deathsquito,
        Goblin,
        GoblinBrute,
        GoblinShaman,
        Hare,
        Seeker,
        SeekerSoldier,
        SeekerBrood,
        Gjall,
        Tick,
    }
    public enum ArmorDirectories
    {
        None,
        Rags,
        Leather,
        Troll,
        Bronze,
        Iron,
        Wolf,
        Padded,
        Carapace,
        Mage
    }

    public static bool HDPackLoaded;
    private static void ReadVegDirectories()
    {
        foreach (VegDirectories directory in Enum.GetValues(typeof(VegDirectories)))
        {
            if (directory is VegDirectories.None) continue;
            string type = directory.ToString();
            if (!Directory.Exists(VegTexturePath + Path.DirectorySeparatorChar + type))
            {
                Directory.CreateDirectory(VegTexturePath + Path.DirectorySeparatorChar + type);
            };

            switch (directory)
            {
                case VegDirectories.PlainsGrass or VegDirectories.SwampGrass or VegDirectories.BlackForestGrass or VegDirectories.BlackForestGrassAlt:
                    Dictionary<string, Texture?> compressedMap = RegisterCustomTextures(type, VegTexturePath, 
                        HDPackLoaded ? HdConfigurations.GrassSettings.TextureFormat : TextureFormat.DXT1, 
                        HDPackLoaded ? HdConfigurations.GrassSettings.TextureFilter : FilterMode.Point);
                    if (compressedMap.Count == 0) continue;
                    CustomRegisteredTextures.Add(directory, compressedMap);
                    break;
                default:
                    Dictionary<string, Texture?> map = HDPackLoaded 
                        ? RegisterCustomTextures(type, VegTexturePath, HdConfigurations.defaultSettings.TextureFormat, HdConfigurations.defaultSettings.TextureFilter) 
                        : RegisterCustomTextures(type, VegTexturePath);
                    if (map.Count == 0) continue;
                    CustomRegisteredTextures.Add(directory, map);
                    break;
            }
        }
    }
    private static void ReadCreatureDirectories()
    {
        foreach (CreatureDirectories creatureDir in Enum.GetValues(typeof(CreatureDirectories)))
        {
            if (creatureDir is CreatureDirectories.None) continue;
            string type = creatureDir.ToString();
            if (!Directory.Exists(creatureTexPath + Path.DirectorySeparatorChar + type))
            {
                Directory.CreateDirectory(creatureTexPath + Path.DirectorySeparatorChar + type);
            }

            Dictionary<string, Texture?> map = HDPackLoaded 
                ? RegisterCustomTextures(type, creatureTexPath, HdConfigurations.CreatureSettings.TextureFormat, HdConfigurations.CreatureSettings.TextureFilter) 
                : RegisterCustomTextures(type, creatureTexPath, TextureFormat.BC7, FilterMode.Point);
            if (map.Count == 0) continue;
            CustomRegisteredCreatureTex.Add(creatureDir, map);
        }
    }
    private static void ReadPieceDirectories()
    {
        foreach (PieceDirectories pieceDir in Enum.GetValues(typeof(PieceDirectories)))
        {
            if (pieceDir is PieceDirectories.None) continue;
            string type = pieceDir.ToString();
            if (!Directory.Exists(PieceTexPath + Path.DirectorySeparatorChar + type))
            {
                Directory.CreateDirectory(PieceTexPath + Path.DirectorySeparatorChar + type);
            }

            Dictionary<string, Texture?> map = HDPackLoaded 
                ? RegisterCustomTextures(type, PieceTexPath, HdConfigurations.PieceSettings.TextureFormat, HdConfigurations.PieceSettings.TextureFilter) 
                : RegisterCustomTextures(type, PieceTexPath, TextureFormat.BC7, FilterMode.Point);
            if (map.Count == 0) continue;
            CustomRegisteredPieceTextures.Add(pieceDir, map);
        }
    }
    private static void ReadPickableDirectories()
    {
        foreach (PickableDirectories pickableDir in Enum.GetValues(typeof(PickableDirectories)))
        {
            if (pickableDir is PickableDirectories.None) continue;
            string type = pickableDir.ToString();
            if (!Directory.Exists(PickableTexturePath + Path.DirectorySeparatorChar + type))
            {
                Directory.CreateDirectory(PickableTexturePath + Path.DirectorySeparatorChar + type);
            }

            Dictionary<string, Texture?> map = HDPackLoaded 
                ? RegisterCustomTextures(type, PickableTexturePath, HdConfigurations.PickableSettings.TextureFormat, HdConfigurations.PickableSettings.TextureFilter) :
                RegisterCustomTextures(type, PickableTexturePath, TextureFormat.BC7, FilterMode.Point);
            if (map.Count == 0) continue;
            CustomRegisteredPickableTex.Add(pickableDir, map);
        }
    }
    private static void ReadArmorDirectories()
    {
        foreach (ArmorDirectories armorDir in Enum.GetValues(typeof(ArmorDirectories)))
        {
            if (armorDir is ArmorDirectories.None) continue;
            string type = armorDir.ToString();
            if (!Directory.Exists(ArmorTexPath + Path.DirectorySeparatorChar + type))
            {
                Directory.CreateDirectory(ArmorTexPath + Path.DirectorySeparatorChar + type);
            }
            Dictionary<string, Texture?> map = HDPackLoaded 
                ? RegisterCustomTextures(type, ArmorTexPath, HdConfigurations.ArmorSettings.TextureFormat, HdConfigurations.ArmorSettings.TextureFilter) 
                : RegisterCustomTextures(type, ArmorTexPath, TextureFormat.BC7, FilterMode.Point);
            if (map.Count == 0) continue;
            CustomRegisteredArmorTex.Add(armorDir, map);
        }
    }
    public static void ReadCustomTextures()
    {
        // Create directories if they are missing
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        if (!Directory.Exists(VegTexturePath)) Directory.CreateDirectory(VegTexturePath);

        if (File.Exists(folderPath + Path.DirectorySeparatorChar + "WillyBachHD.md"))
        {
            SeasonalityLogger.LogInfo("Willybach HD loaded");
            HDPackLoaded = true;
            InitHDConfigurations();
        }

        ReadVegDirectories();
        ReadCreatureDirectories();
        ReadPieceDirectories();
        ReadPickableDirectories();
        ReadArmorDirectories();
    }
    private static void GetTexture(Dictionary<string, Texture?> textureMap, Season season, string path, string type, TextureFormat textureFormat, FilterMode filterMode)
    {
        string filePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + ".png");
        string message = type + "/" + season.ToString().ToLower() + ".png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}"; // Beech/spring.png
        if (!File.Exists(filePath)) return;
        Texture? tex = RegisterCustomTexture(filePath, textureFormat, filterMode);
        if (!tex)
        {
            SeasonalityLogger.LogDebug($"Failed: {message}");
            return;
        }
        textureMap.Add(season.ToString(), tex);
        SeasonalityLogger.LogDebug($"Registered: {message}");
    }
    private static void GetBark(Dictionary<string, Texture?> textureMap, Season season, string path, string type, TextureFormat textureFormat, FilterMode filterMode)
    {
        string BarkFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_bark.png");
        string barkMessage = type + "/" + season.ToString().ToLower() + "_bark.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";

        if (!File.Exists(BarkFilePath)) return;
        Texture? tex = RegisterCustomTexture(BarkFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat, true);
        if (!tex)
        {
            SeasonalityLogger.LogDebug($"Failed: {barkMessage}");
            return;
        }
        textureMap.Add(season + "_bark", tex);
        SeasonalityLogger.LogDebug($"Registered: {barkMessage}");
    }
    private static void GetNormal(Dictionary<string, Texture?> textureMap, Season season, string path, string type, TextureFormat textureFormat, FilterMode filterMode)
    {
        string NormalFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_normal.png");
        string NormalMessage = type + "/" + season.ToString().ToLower() + "_normal.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";

        if (!File.Exists(NormalFilePath)) return;
        Texture? tex = RegisterCustomTexture(NormalFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat, true);
        if (!tex)
        {
            SeasonalityLogger.LogDebug($"Failed: {NormalMessage}");
            return;
        }
        textureMap.Add(season + "_normal", tex);
        SeasonalityLogger.LogDebug($"Registered: {NormalMessage}");
    }
    private static void GetWorn(Dictionary<string, Texture?> textureMap, Season season, string path, string type, TextureFormat textureFormat, FilterMode filterMode)
    {
        string WornFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_worn.png");
        string WornMessage = type + "/" + season.ToString().ToLower() + "_worn.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";

        if (File.Exists(WornFilePath))
        {
            Texture? tex = RegisterCustomTexture(WornFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat, true);
            if (!tex)
            {
                SeasonalityLogger.LogDebug($"Failed: {WornMessage}");
                return;
            }
            textureMap.Add(season + "_worn", tex);
            SeasonalityLogger.LogDebug($"Registered: {WornMessage}");
        }
    }
    private static void GetCorner(Dictionary<string, Texture?> textureMap, Season season, string path, string type, TextureFormat textureFormat, FilterMode filterMode)
    {
        string CornerFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_corner.png");
        string CornerMessage = type + "/" + season.ToString().ToLower() + "_corner.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";
            
        if (File.Exists(CornerFilePath))
        {
            Texture? tex = RegisterCustomTexture(CornerFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat, true);
            if (!tex)
            {
                SeasonalityLogger.LogDebug($"Failed: {CornerMessage}");
                return;
            }
            textureMap.Add(season + "_corner", tex);
            SeasonalityLogger.LogDebug($"Registered: {CornerMessage}");
        }
    }
    private static void GetCornerWorn(Dictionary<string, Texture?> textureMap, Season season, string path, string type, TextureFormat textureFormat, FilterMode filterMode)
    {
        string CornerWornFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_corner_worn.png");
        string CornerWornMessage = type + "/" + season.ToString().ToLower() + "_corner_worn.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";
            
        if (File.Exists(CornerWornFilePath))
        {
            Texture? tex = RegisterCustomTexture(CornerWornFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat, true);
            if (!tex)
            {
                SeasonalityLogger.LogDebug($"Failed: {CornerWornMessage}");
                return;
            }
            textureMap.Add(season + "_corner_worn", tex);
            SeasonalityLogger.LogDebug($"Registered: {CornerWornMessage}");
        }
    }
    private static void GetLegs(Dictionary<string, Texture?> textureMap, Season season, string path, string type, TextureFormat textureFormat, FilterMode filterMode)
    {
        string LegFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_legs.png");
        string LegMessage = type + "/" + season.ToString().ToLower() + "_legs.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";
            
        if (File.Exists(LegFilePath))
        {
            Texture? tex = RegisterCustomTexture(LegFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat);
            if (!tex)
            {
                SeasonalityLogger.LogDebug($"Failed: {LegMessage}");
                return;
            }
            textureMap.Add(season + "_legs", tex);
            SeasonalityLogger.LogDebug($"Registered: {LegMessage}");
        }
    }
    private static void getChest(Dictionary<string, Texture?> textureMap, Season season, string path, string type, TextureFormat textureFormat, FilterMode filterMode)
    {
        string ChestFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_chest.png");
        string ChestMessage = type + "/" + season.ToString().ToLower() + "_chest.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";
            
        if (File.Exists(ChestFilePath))
        {
            Texture? tex = RegisterCustomTexture(ChestFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat);
            if (!tex)
            {
                SeasonalityLogger.LogDebug($"Failed: {ChestMessage}");
                return;
            }
            textureMap.Add(season + "_chest", tex);
            SeasonalityLogger.LogDebug($"Registered: {ChestMessage}");
        }
    }
    private static void GetCape(Dictionary<string, Texture?> textureMap, Season season, string path, string type,
        TextureFormat textureFormat, FilterMode filterMode)
    {
        string CapeFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_cape.png");
        string CapeMessage = type + "/" + season.ToString().ToLower() + "_cape.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";
            
        if (File.Exists(CapeFilePath))
        {
            Texture? tex = RegisterCustomTexture(CapeFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat);
            if (!tex)
            {
                SeasonalityLogger.LogDebug($"Failed: {CapeMessage}");
                return;
            }
            textureMap.Add(season + "_cape", tex);
            SeasonalityLogger.LogDebug($"Registered: {CapeMessage}");
        }
    }
    private static void GetHelmet(Dictionary<string, Texture?> textureMap, Season season, string path, string type,
        TextureFormat textureFormat, FilterMode filterMode)
    {
        string HelmetFilePath = path + Path.DirectorySeparatorChar + type + Path.DirectorySeparatorChar + (season.ToString().ToLower() + "_helmet.png");
        string HelmetMessage = type + "/" + season.ToString().ToLower() + "_helmet.png" + $" compressed as {textureFormat.ToString()}, filter {filterMode.ToString()}";
            
        if (File.Exists(HelmetFilePath))
        {
            Texture? tex = RegisterCustomTexture(HelmetFilePath, textureFormat, filterMode, aniso: 1, mipMapBias: 0, wrap: TextureWrapMode.Repeat);
            if (!tex)
            {
                SeasonalityLogger.LogDebug($"Failed: {HelmetMessage}");
                return;
            }
            textureMap.Add(season + "_helmet", tex);
            SeasonalityLogger.LogDebug($"Registered: {HelmetMessage}");
        }
    }
    private static Dictionary<string, Texture?> RegisterCustomTextures(string type, string path, TextureFormat textureFormat = TextureFormat.DXT5, FilterMode filterMode = FilterMode.Trilinear)
    {
        Dictionary<string, Texture?> textureMap = new();

        foreach (Season season in Enum.GetValues(typeof(Season)))
        {
            GetTexture(textureMap, season, path, type, textureFormat, filterMode);
            GetBark(textureMap, season, path, type, textureFormat, filterMode);
            GetNormal(textureMap, season, path, type, textureFormat, filterMode);
            GetWorn(textureMap, season, path, type, textureFormat, filterMode);
            GetCorner(textureMap, season, path, type, textureFormat, filterMode);
            GetCornerWorn(textureMap, season, path, type, textureFormat, filterMode);
            getChest(textureMap, season, path, type, textureFormat, filterMode);
            GetLegs(textureMap, season, path, type, textureFormat, filterMode);
            GetCape(textureMap, season, path, type, textureFormat, filterMode);
            GetHelmet(textureMap, season, path, type, textureFormat, filterMode);
        }

        return textureMap;
    }
}