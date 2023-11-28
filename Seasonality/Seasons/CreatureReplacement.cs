using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class CreatureReplacement
{
    // [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
    // static class SpawnSystemPatch
    // {
    //     private static bool Prefix(SpawnSystem __instance, SpawnSystem.SpawnData critter, Vector3 spawnPoint, bool eventSpawner)
    //     {
    //         string critterName = critter.m_prefab.name;
    //         switch (_Season.Value)
    //         {
    //             case Season.Winter:
    //                 switch (critterName)
    //                 {
    //                     case "Leech":
    //                         if (_ReplaceLeech.Value is Toggle.Off) return true;
    //                         __instance.Spawn(CloneData(critter, "Leech_cave"), spawnPoint, eventSpawner);
    //                         return false;
    //                 }
    //                 break;
    //         }
    //         return true;
    //     }
    //     private static SpawnSystem.SpawnData CloneData(SpawnSystem.SpawnData critter, string prefabName)
    //     {
    //         SpawnSystem.SpawnData data = new SpawnSystem.SpawnData()
    //         {
    //             m_prefab = ZNetScene.instance.GetPrefab(prefabName),
    //             m_name = critter.m_name,
    //             m_enabled = true,
    //             m_biome = critter.m_biome,
    //             m_biomeArea = critter.m_biomeArea,
    //             m_maxSpawned = critter.m_maxSpawned,
    //             m_spawnInterval = critter.m_spawnInterval,
    //             m_spawnChance = critter.m_spawnChance,
    //             m_spawnDistance = critter.m_spawnDistance,
    //             m_spawnRadiusMin = critter.m_spawnRadiusMin,
    //             m_spawnRadiusMax = critter.m_spawnRadiusMax,
    //             m_requiredGlobalKey = critter.m_requiredGlobalKey,
    //             m_requiredEnvironments = critter.m_requiredEnvironments,
    //             m_groupSizeMin = critter.m_groupSizeMin,
    //             m_groupSizeMax = critter.m_groupSizeMax,
    //             m_groupRadius = critter.m_groupRadius,
    //             m_spawnAtNight = critter.m_spawnAtNight,
    //             m_spawnAtDay = critter.m_spawnAtDay,
    //             m_minAltitude = critter.m_minAltitude,
    //             m_maxAltitude = critter.m_maxAltitude,
    //             m_minTilt = critter.m_minTilt,
    //             m_maxTilt = critter.m_maxTilt,
    //             m_inForest = critter.m_inForest,
    //             m_outsideForest = critter.m_outsideForest,
    //             m_minOceanDepth = critter.m_minOceanDepth,
    //             m_maxOceanDepth = critter.m_maxOceanDepth,
    //             m_huntPlayer = critter.m_huntPlayer,
    //             m_groundOffset = critter.m_groundOffset,
    //             m_maxLevel = critter.m_maxLevel,
    //             m_minLevel = critter.m_minLevel,
    //             m_levelUpMinCenterDistance = critter.m_levelUpMinCenterDistance,
    //             m_overrideLevelupChance = critter.m_overrideLevelupChance,
    //             m_foldout = critter.m_foldout
    //         };
    //
    //         return data;
    //     }
    // }

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
                            if (_ReplaceLox.Value is Toggle.On) ReplaceCreatureTexture(prefab, CustomTextures.Lox_Winter);
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