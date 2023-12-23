using HarmonyLib;
using UnityEngine;

namespace Seasonality.Seasons;

public static class AttachToModel
{
    // [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Awake))]
    // static class ChristmasSpecial
    // {
    //     private static void Postfix(ZNetView __instance)
    //     {
    //         if (!__instance) return;
    //         if (!__instance.TryGetComponent(out MonsterAI monsterAI)) return;
    //         if (__instance.name.Replace("(Clone)", "") == "Greyling")
    //         {
    //             AddYuleHat(__instance.gameObject);
    //         }
    //     }
    // }
    
    private static void AddYuleHat(GameObject prefab)
    {
        if (!ZNetScene.instance) return;
        GameObject YuleHat = ZNetScene.instance.GetPrefab("HelmetYule");
        GameObject hat = YuleHat.transform.Find("attach/bronzehelmet").gameObject;

        Transform visual = prefab.transform.Find("Visual");

        GameObject customYule = Object.Instantiate(hat, visual, false);
        customYule.transform.localPosition = new Vector3(0.023f, 1.674f, 0.398f);
        customYule.transform.localRotation = new Quaternion(33.313f, -17.766f, -6.722f, 0f);
        customYule.transform.localScale = new Vector3(0.109525f, 0.109525f, 0.109525f);

        VisEquipment equipment = prefab.AddComponent<VisEquipment>();
        equipment.m_bodyModel = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
        equipment.m_nview = prefab.GetComponent<ZNetView>();
        equipment.m_helmet = global::Utils.FindChild(prefab.transform, "head_end");
        equipment.m_helmetItemInstance = customYule;
        
        SeasonalityPlugin.SeasonalityLogger.LogWarning("Added yule hat to greyling ?");

    }
}