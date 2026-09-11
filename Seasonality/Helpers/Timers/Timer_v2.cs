// using System;
// using System.IO;
// using BepInEx;
// using HarmonyLib;
// using UnityEngine;
//
// namespace Seasonality;
//
// public class Timer_v2 : MonoBehaviour
// {
//     public static bool hasValidServer;
//     public static bool m_sleepOverride;
//     private static readonly string ConfigFolder = Paths.ConfigPath + Path.DirectorySeparatorChar + "Seasonality";
//     private static readonly string BinFolder = Path.Combine(ConfigFolder, "LastSeasonChangeData");
//     public static Timer_v2? instance;
//     public float m_fadeTimer;
//     public static string? lastSeasonChangeFileName;
//     public static double lastSeasonChange;
//     
//     public void Awake()
//     {
//         SeasonalityPlugin.Record.LogDebug("Seasonal Timer Awake");
//         instance = this;
//         if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);
//         if (!Directory.Exists(BinFolder)) Directory.CreateDirectory(BinFolder);
//     }
//     
//     
//     
//     
//     
//     
//     
//     
//     
//     public static void Read()
//     {
//         if (lastSeasonChangeFileName is null) return;
//         string filePath = Path.Combine(BinFolder, lastSeasonChangeFileName);
//         if (!File.Exists(filePath)) return;
//         try
//         {
//             using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));
//             lastSeasonChange = reader.ReadDouble();
//             SeasonalityPlugin.Record.LogDebug(
//                 $"Reading last season change from, {Path.GetFileNameWithoutExtension(filePath)}: {lastSeasonChange}");
//             if (lastSeasonChange > ZNet.instance.GetTimeSeconds())
//             {
//                 SeasonalityPlugin.Record.LogDebug($"Last season change time {lastSeasonChange} is larger than world time: {ZNet.instance.GetTimeSeconds()}, setting last season change to world time");
//                 lastSeasonChange = ZNet.instance.GetTimeSeconds();
//             }
//         }
//         catch (Exception ex)
//         {
//             SeasonalityPlugin.Record.LogError(ex.Message);
//         }
//     }
//     
//     public static void Save()
//     {
//         if (lastSeasonChangeFileName is null) return;
//         string filePath = Path.Combine(BinFolder, lastSeasonChangeFileName);
//         try
//         {
//             using BinaryWriter writer = new BinaryWriter(File.Create(filePath));
//             writer.Write(lastSeasonChange);
//             SeasonalityPlugin.Record.LogDebug(
//                 $"Saving last season change to, {Path.GetFileNameWithoutExtension(filePath)}, {lastSeasonChange}");
//         }
//         catch (Exception ex)
//         {
//             SeasonalityPlugin.Record.LogError(ex.Message);
//         }
//     }
// }