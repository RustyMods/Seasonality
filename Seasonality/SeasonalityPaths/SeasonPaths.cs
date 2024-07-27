using System.IO;
using BepInEx;

namespace Seasonality.SeasonalityPaths;

public static class SeasonPaths
{
    public static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
    public static readonly string VegTexturePath = folderPath + Path.DirectorySeparatorChar + "Textures";
    public static readonly string creatureTexPath = folderPath + Path.DirectorySeparatorChar + "Creatures";
    public static readonly string PieceTexPath = folderPath + Path.DirectorySeparatorChar + "Pieces";
    public static readonly string PickableTexturePath = folderPath + Path.DirectorySeparatorChar + "Pickables";
    public static readonly string ArmorTexPath = folderPath + Path.DirectorySeparatorChar + "Armors";
    public static readonly string CustomTexPath = folderPath + Path.DirectorySeparatorChar + "Customs";
    public static readonly string CustomSavePath = folderPath + Path.DirectorySeparatorChar + "Data";
}