using HarmonyLib;
using UnityEngine;

namespace Seasonality;

public static class SnowBuildUp
{
    private static void UpdateSnow(WearNTear wnt)
    {
        if (!wnt.m_nview.IsValid() || !wnt.m_nview.IsOwner()) return;
        if (wnt.m_biome is Heightmap.Biome.DeepNorth or Heightmap.Biome.AshLands) return;
        if (!wnt.m_snow) return;
        if (SeasonTime.m_currentSeason is Season.Winter && Configs.m_addSnow.Value is Toggle.On)
        {
            bool haveRoof = wnt.HaveRoof();

            if (haveRoof)
            {
                if (wnt.m_snowBuildup <= 0f) return;
                wnt.m_nview.GetZDO().Set(ZDOVars.s_snow, 0f);
            }
            else
            {
                if (wnt.m_snowBuildup >= 1f) return;
                var value = Mathf.Max(0.26f, Mathf.Clamp01(wnt.m_snowBuildup + Configs.m_snowBuildupSpeed.Value));
                wnt.m_nview.GetZDO().Set(ZDOVars.s_snow, value);
            }
        }
        else
        {
            if (wnt.m_snowBuildup <= 0f) return;
            wnt.m_nview.GetZDO().Set(ZDOVars.s_snow, 0f);
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.UpdateWear))]
    private static class WearNTear_UpdateWear
    {
        private static void Prefix(WearNTear __instance, float time)
        {
            UpdateSnow(__instance);
        }
    }

    // [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
    // private static class Attack_DoMelee_Attack_Patch
    // {
    //     private static void Postfix(Attack __instance)
    //     {
    //         if (Configs.m_addSnow.Value is Toggle.Off) return;
    //         if (EnvMan.instance.GetCurrentBiome() == Heightmap.Biome.DeepNorth) return;
    //         if (!__instance.m_snowShovel || __instance.m_snowShovelRadius <= 0) return;
    //         
    //         Vector3 snowShovelPoint = __instance.GetSnowShovelPoint();
    //         int num5 = Physics.OverlapSphereNonAlloc(snowShovelPoint, __instance.m_snowShovelRadius, Attack.s_pieceColliders, Attack.m_snowShovelRayMask);
    //         for (int index = 0; index < num5; ++index)
    //         {
    //             Collider pieceCollider = Attack.s_pieceColliders[index];
    //             WearNTear componentInParent = pieceCollider.gameObject.GetComponentInParent<WearNTear>();
    //             if (componentInParent != null)
    //             {
    //                 if (componentInParent.m_snowBuildup > 0.0)
    //                 {
    //                     componentInParent.ChangeSnow(-__instance.m_snowShovelStrength);
    //                     Player.m_localPlayer.m_snowShovelEffect.Create(pieceCollider.transform.position, pieceCollider.transform.rotation);
    //                 }
    //             }
    //             else
    //             {
    //                 SnowDestruction component = pieceCollider.gameObject.GetComponent<SnowDestruction>();
    //                 if (component != null)
    //                 {
    //                     component.Damage(__instance.m_snowShovelStrength);
    //                     Player.m_localPlayer.m_snowShovelEffect.Create(pieceCollider.transform.position, pieceCollider.transform.rotation);
    //                 }
    //             }
    //         }
    //     }
    // }
}