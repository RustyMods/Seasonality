using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Seasonality.SeasonalityPaths;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class ConsoleCommands
{
    private static readonly Dictionary<string, Terminal.ConsoleCommand> SeasonCommands = new();
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    static class TerminalInitPatch
    {
        private static void Postfix() 
        {
            Terminal.ConsoleCommand Help = new Terminal.ConsoleCommand("seasonality", "Shows a list of console commands for seasonality", (Terminal.ConsoleEventFailable) (args =>
                    {
                        if (args.Length < 2) return false;
                        
                        List<string> commandList = new List<string>();
                        foreach (KeyValuePair<string, Terminal.ConsoleCommand> command in SeasonCommands)
                        {
                            commandList.Add(command.Value.Command + " - " + command.Value.Description);
                        }

                        commandList.Sort();

                        foreach (string command in commandList)
                        {
                            SeasonalityLogger.LogInfo(command);
                        }
                        return true;
                    }
                ), isCheat: false, optionsFetcher: ((Terminal.ConsoleOptionsFetcher) (() => new List<string>(){"help"})));
            
            SeasonCommands.Clear();
            ConfigCommands();
            SearchCommands();
            // SaveCommands();
        }

        private static void SaveCommands()
        {
            Terminal.ConsoleCommand save = new Terminal.ConsoleCommand("seasonality_save",
                "[prefabName] - Attempts to save texture to file", (Terminal.ConsoleEventFailable)(
                    args =>
                    {
                        if (args.Length < 2) return false;

                        if (!ZNetScene.instance) return false;

                        GameObject prefab = ZNetScene.instance.GetPrefab(args[1]);
                        if (!prefab) return false;

                        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                        {
                            foreach (var material in renderer.materials)
                            {
                                if (!material.HasProperty("_MainTex")) continue;
                                Texture texture = material.GetTexture(MainTex);
                                if (texture == null) continue;
                                if (texture is Texture2D tex)
                                {
                                    try
                                    {
                                        var encode = tex.EncodeToPNG();
                                        if (!Directory.Exists(SeasonalityPaths.SeasonPaths.CustomSavePath))
                                            Directory.CreateDirectory(SeasonPaths.CustomSavePath);
                                        string path = SeasonPaths.CustomSavePath + Path.DirectorySeparatorChar +
                                                      material.name.Replace("(Instance)", string.Empty) + "." +
                                                      texture.name + ".png";
                                        File.WriteAllBytes(path, encode);
                                        SeasonalityLogger.LogInfo("Saved texture to disk: " + path);
                                    }
                                    catch
                                    {
                                        SeasonalityLogger.LogInfo("Failed to save texture: " + texture.name);
                                    }
                                }
                            }
                        }
                        
                        return true;
                    }));

            if (SeasonCommands.ContainsKey("seasonality_save")) return;
            SeasonCommands.Add("seasonality_save", save);
        }

        private static void ConfigCommands()
        {
            Terminal.ConsoleCommand SeasonChange = new Terminal.ConsoleCommand("season",
                "[season name] changes season (ex: summer, fall, winter, spring)",
                (Terminal.ConsoleEventFailable) (args =>
                {

                    if (args.Length < 2) return false;
                    switch (args[1])
                    {
                        case "summer": _Season.Value = Season.Summer; break;
                        case "fall": _Season.Value = Season.Fall; break;
                        case "winter": _Season.Value = Season.Winter; break;
                        case "spring": _Season.Value = Season.Spring; break;
                    }
                    return true;
                }), false, optionsFetcher: ((Terminal.ConsoleOptionsFetcher) (() =>
                {
                    List<string> seasons = new List<string>()
                    {
                        "summer", "fall", "winter", "spring"
                    };
                    return seasons;
                })), onlyAdmin: true);
            if (!SeasonCommands.ContainsKey("season change")) SeasonCommands.Add("season change", SeasonChange);

            Terminal.ConsoleCommand LogCacheTextures = new("seasonality_textures", "", args =>
            {
                SeasonalityLogger.LogInfo("All cached textures:");
                foreach (var kvp in MaterialReplacer.CachedTextures)
                {
                    if (!kvp.Value)
                    {
                        SeasonalityLogger.LogWarning("Failed to get value of " + kvp.Key);
                        continue;
                    }
                    SeasonalityLogger.LogInfo(kvp.Key + ": " + kvp.Value.name);
                }
            });
        }

        private static void SearchCommands()
        {
            Terminal.ConsoleCommand SearchCachedMaterials = new("search_materials", "", (Terminal.ConsoleEventFailable)(
                args =>
                {
                    if (args.Length < 2) return false;
                    SeasonalityLogger.LogInfo("Material search results: ");
                    foreach (KeyValuePair<string, Material> kvp in MaterialReplacer.CachedMaterials)
                    {
                        if (kvp.Key.Contains(args[1]) || kvp.Key.StartsWith(args[1]) || kvp.Key.EndsWith(args[1]))
                        {
                            SeasonalityLogger.LogInfo(kvp.Key + " = " + kvp.Value.name);
                        }
                    }
                    return true;
                }), isSecret: true);

            Terminal.ConsoleCommand SearchCachedTextures = new("search_textures", "", (Terminal.ConsoleEventFailable)(
                args =>
                {
                    if (args.Length < 2) return false;
                    SeasonalityLogger.LogInfo("Texture search results: ");
                    foreach (KeyValuePair<string, Texture> kvp in MaterialReplacer.CachedTextures)
                    {
                        if (kvp.Key.Contains(args[1]) || kvp.Key.StartsWith(args[1]) || kvp.Key.EndsWith(args[1]))
                        {
                            SeasonalityLogger.LogInfo($"{kvp.Key} = {kvp.Value}");
                        }
                    }
                    return true;
                }),isSecret:true);

            Terminal.ConsoleCommand PrintTextureDetails = new("print_texture_details", "", (Terminal.ConsoleEventFailable)(args =>
            {
                if (args.Length < 2) return false;
                SeasonalityLogger.LogInfo("Cached texture details: ");
                foreach (var kvp in MaterialReplacer.CachedTextures)
                {
                    if (kvp.Key.Contains(args[1]))
                    {
                        SeasonalityLogger.LogInfo("****** name : " + kvp.Value.name);
                        SeasonalityLogger.LogInfo("filter mode :" + kvp.Value.filterMode);
                        SeasonalityLogger.LogInfo("aniso level : " + kvp.Value.anisoLevel);
                        SeasonalityLogger.LogInfo("graphic format : " + kvp.Value.graphicsFormat);
                        SeasonalityLogger.LogInfo("mip map count : " + kvp.Value.mipmapCount);
                        SeasonalityLogger.LogInfo("mip map bias : " + kvp.Value.mipMapBias);
                        SeasonalityLogger.LogInfo("wrap mode : " + kvp.Value.wrapMode);
                        SeasonalityLogger.LogInfo("dimensions : " + kvp.Value.dimension);
                        SeasonalityLogger.LogInfo(" ");
                    }
                }
                return true;
            }),isSecret:true);
            
            Terminal.ConsoleCommand SearchCustomMaterials = new("search_custom_materials", "", (Terminal.ConsoleEventFailable)(
                args =>
                {
                    if (args.Length < 2)
                    {
                        foreach (KeyValuePair<string, Material> kvp in MaterialReplacer.CustomMaterials)
                        {
                            SeasonalityLogger.LogInfo(kvp.Key + " = " + kvp.Value);
                        }
                    }
                    else
                    {
                        SeasonalityLogger.LogInfo("Custom material search results:");

                        foreach (var kvp in MaterialReplacer.CustomMaterials)
                        {
                            if (kvp.Key.Contains(args[1]) || kvp.Key.StartsWith(args[1]) || kvp.Key.EndsWith(args[1]))
                            {
                                SeasonalityLogger.LogInfo($"{kvp.Key} = {kvp.Value}");
                            }
                        }
                    }
                    return true;
                }),isSecret:true);

            Terminal.ConsoleCommand SearchShaders = new("search_shaders", "", (Terminal.ConsoleEventFailable)(args =>
            {
                SeasonalityLogger.LogInfo("Shader search results: ");
                if (args.Length < 2)
                {
                    foreach (KeyValuePair<string, Material> kvp in MaterialReplacer.CachedMaterials)
                    {
                        if (!kvp.Value) continue;
                        SeasonalityLogger.LogInfo(kvp.Value.shader.name);
                    }
                };
                foreach (KeyValuePair<string, Material> kvp in MaterialReplacer.CachedMaterials)
                {
                    if (!kvp.Value) continue;
                    Shader? shader = kvp.Value.shader;
                    if (!shader) continue;
                    if (shader.name.Contains(args[1]) || shader.name.StartsWith(args[1]) ||
                        shader.name.EndsWith(args[1]))
                    {
                        SeasonalityLogger.LogInfo($"{kvp.Key} = {kvp.Value} = {shader.name}");
                    }
                }
                
                return true;
            }),isSecret:true);


            Terminal.ConsoleCommand ListWeathers = new("list_weathers", "Lists all available weathers", args =>
            {
                if (!EnvMan.instance) return;
                foreach (var env in EnvMan.instance.m_environments)
                {
                    SeasonalityPlugin.SeasonalityLogger.LogInfo(env.m_name);
                }
            });
        }
    }
}