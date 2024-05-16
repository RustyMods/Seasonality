using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Seasonality.Managers;

public static class HudManager
{
    public static GameObject m_seasonScreen = null!;
    public static Image m_seasonBlackScreen = null!;
    
     [HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
     private static class Hud_Awake_Patch
     {
         private static void Postfix(Hud __instance)
         {
             if (!__instance) return;
             m_seasonScreen = new GameObject("screen");
             RectTransform rect = m_seasonScreen.AddComponent<RectTransform>();
             rect.sizeDelta = new Vector2(Screen.width, Screen.height);
             rect.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
             rect.SetParent(__instance.gameObject.transform);
             m_seasonBlackScreen = m_seasonScreen.AddComponent<Image>();
             m_seasonBlackScreen.color = Color.clear;
             m_seasonBlackScreen.raycastTarget = false;
         }
     }
}