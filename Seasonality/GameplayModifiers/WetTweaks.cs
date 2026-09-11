using BepInEx.Configuration;
using HarmonyLib;

namespace Seasonality;

public static class WetTweaks
{
    private static ConfigEntry<Toggle> enabled = null!;

    public static void Setup()
    {
        enabled = ConfigManager.config("Winter", "Wet Freezes", Toggle.Off, "If on, player will freeze if wet during winter");
    }
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateEnvStatusEffects))]
    public static class Player_UpdateEnvStatusEffects
    {
        public static void Postfix(Player __instance)
        {
            if (enabled.Value is Toggle.Off) return;
            if (Configs.m_season.Value is not Season.Winter) return;
            if (__instance.GetSEMan().HaveStatusEffect(SEMan.s_statusEffectFreezing)) return;

            HitData.DamageModifiers modifiers = __instance.GetDamageModifiers();
            HitData.DamageModifier frostMod = modifiers.GetModifier(HitData.DamageType.Frost);
            if (frostMod is HitData.DamageModifier.Resistant or HitData.DamageModifier.Immune or HitData.DamageModifier.Ignore) return;
            
            StatusEffect? wet = __instance.GetSEMan().GetStatusEffect(SEMan.s_statusEffectWet);
            if (wet == null) return;
            StatusEffect? freeze = __instance.GetSEMan().AddStatusEffect(SEMan.s_statusEffectFreezing);
            if (freeze == null) return;
            freeze.m_ttl = wet.m_ttl;
            freeze.m_category = "Seasonality";
            __instance.GetSEMan().RemoveStatusEffect(SEMan.s_statusEffectWet, true);
            __instance.GetSEMan().RemoveStatusEffect(SEMan.s_statusEffectCold, true);
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), typeof(int), typeof(bool), typeof(int), typeof(float), typeof(short))]
    private static class SEMan_AddStatusEffect_Patch
    {
        private static bool Prefix(SEMan __instance, int nameHash)
        {
            if (enabled.Value is Toggle.Off) return true;
            if (__instance.m_character is not Player) return true;
            if (nameHash != SEMan.s_statusEffectWet) return true;
            StatusEffect? freeze = __instance.GetStatusEffect(SEMan.s_statusEffectFreezing);
            if (freeze == null) return true;
            if (freeze.m_category != "Seasonality") return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveStatusEffect), typeof(int), typeof(bool))]
    private static class SEMan_RemoveStatusEffect_Patch
    {
        private static bool Prefix(SEMan __instance, int nameHash)
        {
            if (enabled.Value is Toggle.Off) return true;
            if (nameHash != SEMan.s_statusEffectFreezing) return true;
            if (__instance.m_character is not Player) return true;
            StatusEffect? freeze = __instance.GetStatusEffect(nameHash);
            if (freeze == null) return true;
            if (freeze.m_category != "Seasonality") return true;
            bool isNearCampfire = __instance.HaveStatusEffect(SEMan.s_statusEffectCampFire);
            if (isNearCampfire)
            {
                freeze.m_ttl -= 0.25f;
            }
            return false;
        }
    }
}