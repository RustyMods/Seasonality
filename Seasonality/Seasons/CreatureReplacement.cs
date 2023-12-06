using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;
using static Seasonality.Seasons.CustomTextures;

namespace Seasonality.Seasons;

public static class CreatureReplacement
{
    [HarmonyPatch(typeof(AnimalAI), nameof(AnimalAI.Awake))]
    static class AnimalTextureReplacement
    {
        private static void Postfix(AnimalAI __instance)
        {
            if (!__instance) return;
            GameObject prefab = __instance.gameObject;
            if (!prefab) return;
            
            ReplaceCreatureTextures(_Season.Value, prefab);
        }
    }
    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
    static class CreatureTextureReplacement
    {
        private static void Postfix(MonsterAI __instance)
        {
            if (!__instance) return;
            if (_ReplaceCreatureTextures.Value is not Toggle.On) return;
            GameObject prefab = __instance.gameObject;
            if (!prefab) return;
            ReplaceCreatureTextures(_Season.Value, prefab);
        }
    }
    private static void ReplaceCreatureTextures(Season season, GameObject prefab)
    {
        string normalizedName = prefab.name.Replace("(Clone)", "").ToLower();
        switch (normalizedName)
        {
            case "lox":
                Texture? CustomLoxCoat = Utils.GetCustomTexture(CreatureDirectories.Lox, season);
                if (CustomLoxCoat) ReplaceCreatureTexture(prefab, CustomLoxCoat);
                break;
            case "troll":
                Texture? CustomTrollCoat = Utils.GetCustomTexture(CreatureDirectories.Troll, season);
                if (CustomTrollCoat) ReplaceCreatureTexture(prefab, CustomTrollCoat);
                break;
            case "hare":
                Texture? CustomHareCoat = Utils.GetCustomTexture(CreatureDirectories.Hare, season);
                if (CustomHareCoat) ReplaceCreatureTexture(prefab, CustomHareCoat);
                break;
            case "leech":
                SkinnedMeshRenderer? leechSkin = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
                if (!leechSkin) return;
                GameObject? leech_cave = ZNetScene.instance.GetPrefab("Leech_cave");
                if (!leech_cave) return;
                SkinnedMeshRenderer? caveLeechSkin = leech_cave.GetComponentInChildren<SkinnedMeshRenderer>();
                if (!caveLeechSkin) return;
                leechSkin.material = caveLeechSkin.material;
                break;
            case "tick":
                Texture? customTickCoat = Utils.GetCustomTexture(CreatureDirectories.Tick, season);
                if (customTickCoat) ReplaceCreatureTexture(prefab, customTickCoat);
                break;
            case "serpent":
                Texture? customSerpentCoat = Utils.GetCustomTexture(CreatureDirectories.Serpent, season);
                if (customSerpentCoat) ReplaceCreatureTexture(prefab, customSerpentCoat);
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