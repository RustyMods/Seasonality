using HarmonyLib;

namespace Seasonality.Seasons;

public static class ConsoleCommands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    static class TerminalInitPatch
    {
        private static void Postfix()
        {
            Terminal.ConsoleCommand ReloadTerrain = new Terminal.ConsoleCommand(
                "seasonality_reload_terrain", "Force reload terrain", args =>
                {
                    TerrainPatch.UpdateTerrain();
                }
            );
        }
    }
}