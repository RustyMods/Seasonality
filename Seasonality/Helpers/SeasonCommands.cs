using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Helpers;

public static class SeasonCommands
{
    private static readonly Dictionary<string, SeasonCommand> m_commands = new();

    private static void Setup()
    {
        Terminal.ConsoleCommand main = new("seasonality", "use help to list out commands", (Terminal.ConsoleEventFailable)(args =>
            {
                if (args.Length < 2) return false;
                if (!m_commands.TryGetValue(args[1], out SeasonCommand command)) return false;
                return command.Run(args);
            }), optionsFetcher: () => m_commands.Where(x => !x.Value.IsSecret()).Select(x => x.Key).ToList());
    
        SeasonCommand weathers = new ("weathers", "List of weather names", _ =>
        {
            if (!EnvMan.instance) return false;
            foreach (var env in EnvMan.instance.m_environments)
            {
                SeasonalityPlugin.Record.LogInfo(env.m_name);
            }
            return true;
        });
        
        SeasonCommand materials = new("materials", "search materials", args =>
        {
            if (args.Length < 2) return false;
            foreach (var material in MaterialController.m_allMaterials)
            {
                if (material.Key.ToLower().Contains(args[2].ToLower()))
                {
                    Debug.LogWarning(material.Key);
                }
            }

            foreach (var list in MaterialController.m_fallMaterials)
            {
                if (list.Key.ToLower().Contains(args[2].ToLower()))
                {
                    foreach (var mat in list.Value)
                    {
                        Debug.LogWarning(mat.m_name);
                    }
                }
            }
                
            return true;
        }, isSecret: true);
        
        SeasonCommand registry = new("registered", "search registered materials", args =>
        {
            if (args.Length < 2) return false;
            foreach (var material in MaterialController.m_materials)
            {
                if (material.Key.ToLower().Contains(args[2].ToLower()))
                {
                    Debug.LogWarning(material.Key);
                }
            }
            return true;
        }, isSecret: true);
        
        SeasonCommand print = new("log", "writes to file the current sessions seasonality logs", _ =>
        {
            SeasonalityPlugin.Record.Write();
            return true;
        });
        
        SeasonCommand SearchTextures = new("texture", "search cached textures", args =>
        {
            if (args.Length < 2) return false;
            foreach (var texture in TextureManager.GetAllTextures())
            {
                if (texture.Key.ToLower().Contains(args[2]))
                {
                    Debug.Log(texture.Key);
                }
            }

            return true;
        }, optionsFetcher: () => TextureManager.GetAllTextures().Keys.ToList());
        
        SeasonCommand SetSeason = new("set", "Set current season, admin only", args =>
        {
            if (args.Length < 2) return true;
            if (!Enum.TryParse(args[2], true, out Configs.Season season)) return false;
            Configs.m_season.Value = season;
            return true;
        }, optionsFetcher: () => Enum.GetNames(typeof(Configs.Season)).ToList(), adminOnly: true);
        
        SeasonCommand help = new("help", "list of seasonality commands", _ =>
        {
            foreach (var command in m_commands)
            {
                Debug.Log($"{command.Key} - {command.Value.m_description}");
            }
            return true;
        });
    }


    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
    private static class Terminal_Awake_Patch
    {
        private static void Postfix() => Setup();
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.updateSearch))]
    private static class Terminal_UpdateSearch_Patch
    {
        private static bool Prefix(Terminal __instance, string word)
        {
            if (__instance.m_search == null) return true;
            string[] strArray = __instance.m_input.text.Split(' ');
            if (strArray.Length < 3) return true;
            if (strArray[0] != "seasonality") return true;
            return HandleSearch(__instance, word, strArray);
        }
        
        private static bool HandleSearch(Terminal __instance, string word, string[] strArray)   
        {
            if (!m_commands.TryGetValue(strArray[1], out SeasonCommand command)) return true;
            if (command.HasOptions() && strArray.Length == 3)
            {
                List<string> list = command.FetchOptions();
                List<string> filteredList;
                string currentSearch = strArray[2];
                if (!currentSearch.IsNullOrWhiteSpace())
                {
                    int indexOf = list.IndexOf(currentSearch);
                    filteredList = indexOf != -1 ? list.GetRange(indexOf, list.Count - indexOf) : list;
                    filteredList = filteredList.FindAll(x => x.ToLower().Contains(currentSearch.ToLower()));
                }
                else filteredList = list;
                if (filteredList.Count <= 0) __instance.m_search.text = command.m_description;
                else
                {
                    __instance.m_lastSearch.Clear();
                    __instance.m_lastSearch.AddRange(filteredList);
                    __instance.m_lastSearch.Remove(word);
                    __instance.m_search.text = "";
                    int maxShown = 10;
                    int count = Math.Min(__instance.m_lastSearch.Count, maxShown);
                    for (int index = 0; index < count; ++index)
                    {
                        string text = __instance.m_lastSearch[index];
                        __instance.m_search.text += text + " ";
                    }

                    if (__instance.m_lastSearch.Count <= maxShown) return false;
                    int remainder = __instance.m_lastSearch.Count - maxShown;
                    __instance.m_search.text += $"... {remainder} more.";
                }
            }
            else __instance.m_search.text = command.m_description;
                
            return false;
        }
    }

    private class SeasonCommand
    {
        public readonly string m_description;
        private readonly bool m_isSecret;
        private readonly bool m_adminOnly;
        private readonly Func<Terminal.ConsoleEventArgs, bool> m_command;
        private readonly Func<List<string>>? m_optionFetcher;
        public bool Run(Terminal.ConsoleEventArgs args) => !IsAdmin() || m_command(args);
        private bool IsAdmin()
        {
            if (!ZNet.m_instance) return true;
            if (!m_adminOnly || ZNet.m_instance.LocalPlayerIsAdminOrHost()) return true;
            SeasonalityPlugin.Record.LogWarning("Admin only command");
            return false;
        }
        public bool IsSecret() => m_isSecret;
        public List<string> FetchOptions() => m_optionFetcher == null ? new() :  m_optionFetcher();
        public bool HasOptions() => m_optionFetcher != null;
        
        [Description("Register a custom command with the prefix seasonality")]
        public SeasonCommand(string input, string description, Func<Terminal.ConsoleEventArgs, bool> command, Func<List<string>>? optionsFetcher = null, bool isSecret = false, bool adminOnly = false)
        {
            m_description = description;
            m_command = command;
            m_isSecret = isSecret;
            m_commands[input] = this;
            m_optionFetcher = optionsFetcher;
            m_adminOnly = adminOnly;
        }
    }
}