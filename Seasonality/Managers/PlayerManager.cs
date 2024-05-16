using HarmonyLib;

namespace Seasonality.Managers;

public static class PlayerManager
{
    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    private static class Player_OnSpawned_Patch
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (__instance != Player.m_localPlayer) return;
            SeasonManager.ChangeSeason();
        }
    }
}