using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class CreatureReplacement
{
    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
    static class CreatureTextureReplacement
    {
        private static void Postfix(MonsterAI __instance)
        {
            if (!__instance) return;
            GameObject prefab = __instance.gameObject;
            if (!prefab) return;
            
            string normalizedName = prefab.name.Replace("(Clone)", "").ToLower();
            switch (_Season.Value)
            {
                case Season.Winter:
                    switch (normalizedName)
                    {
                        case "lox":
                            if (_ReplaceLox.Value is Toggle.On)
                            {
                                Texture? LoxWinterCoat = Utils.GetCustomTexture(CreatureDirectories.Lox, Season.Winter);
                                if (LoxWinterCoat) ReplaceCreatureTexture(prefab, LoxWinterCoat);
                            }
                            break;
                        case "leech":
                            if (_ReplaceLeech.Value is Toggle.On)
                            {
                                SkinnedMeshRenderer? leechSkin = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
                                if (!leechSkin) return;
                                GameObject? leech_cave = ZNetScene.instance.GetPrefab("Leech_cave");
                                if (!leech_cave) return;
                                SkinnedMeshRenderer? caveLeechSkin = leech_cave.GetComponentInChildren<SkinnedMeshRenderer>();
                                if (!caveLeechSkin) return;
                                leechSkin.material = caveLeechSkin.material;
                            }
                            break;
                    }

                    break;
            }
        }

        private static void ReplaceCreatureTexture(GameObject prefab, Texture? tex)
        {
            if (!tex) return;
            SkinnedMeshRenderer[]? skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skin in skins)
            {
                if (!skin) return;
                Material? mat = skin.material;
                string[]? properties = mat.GetTexturePropertyNames();
                if (!Utils.FindTexturePropName(properties, "main", out string mainProp)) continue;
                mat.SetTexture(mainProp, tex);
            }
        }
    }
}