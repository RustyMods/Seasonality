using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Seasonality.Helpers;

public static class FadeToBlack
{
    public static bool m_fading;
    public static double m_timeLastFade;
    public static GameObject? m_blackScreen;
    public static Image? m_blackScreenImg;

    [HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
    private static class Hud_Awake_Patch
    {
        private static void Postfix(Hud __instance)
        {
            if (!__instance) return;
            m_blackScreen = new GameObject("screen");
            RectTransform rect = m_blackScreen.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Screen.width, Screen.height);
            rect.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
            rect.SetParent(__instance.gameObject.transform);
            m_blackScreenImg = m_blackScreen.AddComponent<Image>();
            m_blackScreenImg.color = Color.clear;
            m_blackScreenImg.raycastTarget = false;
        }
    }

    public static IEnumerator TriggerFade()
    {
        if (m_fading || m_blackScreen is null || m_blackScreenImg is null) yield break;
        m_fading = true;
        try
        {
            float duration = 0f;
            float length = Mathf.Max(Configs.m_fadeLength.Value * 50f, 1f); // Avoid zero length
            float alpha = 0f;

            // Fade to black
            while (duration < length)
            {
                alpha += 1f / length;
                m_blackScreenImg.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
                duration++;
                yield return new WaitForFixedUpdate();
            }

            Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                $"$msg_{SeasonTimer.GetNextSeason(Configs.m_season.Value).ToString().ToLower()}");

            yield return new WaitForSeconds(1);

            // Fade back to normal
            while (duration > 0)
            {
                alpha -= 1f / length;
                m_blackScreenImg.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
                duration--;
                yield return new WaitForFixedUpdate();
            }

            m_fading = false;
            m_timeLastFade = ZNet.instance.GetTimeSeconds();
        }
        finally
        {
            m_fading = false; // Ensure flag resets even on failure
            m_timeLastFade = ZNet.instance.GetTimeSeconds();
        }
    }
}