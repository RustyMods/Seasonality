using System.Collections.Generic;
using HarmonyLib;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class ConsoleCommands
{
    private static readonly Dictionary<string, Terminal.ConsoleCommand> SeasonCommands = new();

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    static class TerminalInitPatch
    {
        private static void Postfix()
        {
            Terminal.ConsoleCommand Help = new Terminal.ConsoleCommand(
                "seasonality", "Shows a list of console commands for seasonality", (Terminal.ConsoleEventFailable) (args =>
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
            
            ConfigCommands();
        }

        private static void ConfigCommands()
        {
            SeasonCommands.Clear();
            Terminal.ConsoleCommand SeasonChange = new Terminal.ConsoleCommand("season",
                "[season name] changes season (ex: summer, fall, winter, spring)",
                (Terminal.ConsoleEventFailable) (args =>
                {

                    if (args.Length < 2) return false;
                    _SeasonControl.Value = Toggle.On;
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

            Terminal.ConsoleCommand SeasonControl = new("season_control",
                "[toggle] Enable/Disable automatic seasonal change", (Terminal.ConsoleEventFailable)(
                    args =>
                    {
                        if (args.Length < 2) return false;
                        switch (args[1])
                        {
                            case "on": _SeasonControl.Value = Toggle.On;
                                break;
                            case "off": _SeasonControl.Value = Toggle.Off;
                                break;
                        }
                        return true;
                    }), false, optionsFetcher: ((Terminal.ConsoleOptionsFetcher) (() =>
                {
                    return new List<string>() { "on", "off" };
                })), onlyAdmin: true);
            
            if (!SeasonCommands.ContainsKey("season control")) SeasonCommands.Add("season control", SeasonControl);

            Terminal.ConsoleCommand SeasonTimer = new("season_duration",
                "[days] [hours] [minutes] Sets the duration between seasons", (Terminal.ConsoleEventFailable)(
                    args =>
                    {
                        if (args.Length == 4 &&
                            int.TryParse(args[1], out int days) &&
                            int.TryParse(args[2], out int hours) &&
                            int.TryParse(args[3], out int minutes))
                        {
                            _SeasonDurationDays.Value = days;
                            _SeasonDurationHours.Value = hours;
                            _SeasonDurationMinutes.Value = minutes;
                            return true;
                        }
                        return false;
                    }), false){OnlyAdmin = true};
            if (!SeasonCommands.ContainsKey("season duration")) SeasonCommands.Add("season duration", SeasonTimer);
        }
    }
}