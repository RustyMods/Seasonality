using System.Collections.Generic;
using HarmonyLib;
using Seasonality.Helpers;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Behaviors;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(VisEquipment))]
public class VisEquipController : MonoBehaviour
{
    public static readonly List<VisEquipController> m_instances = new();
    public static VisEquipment m_visEquipment = null!;
    private static readonly int ChestTex = Shader.PropertyToID("_ChestTex");
    private static readonly int LegsTex = Shader.PropertyToID("_LegsTex");

    public void Awake()
    {
        m_visEquipment = GetComponent<VisEquipment>();
        m_instances.Add(this);
    }

    public void OnDestroy()
    {
        m_instances.Remove(this);
    }

    public void UpdatePlayerMaterial()
    {
        UpdateChestTexture();
        UpdateLegTexture();
    }

    public void UpdateChestTexture()
    {
        if (m_visEquipment == null || m_visEquipment.m_bodyModel == null) return;
        if (ObjectDB.m_instance.GetItemPrefab(m_visEquipment.m_currentChestItemHash) is not { } chestItem) return;
        if (!chestItem.TryGetComponent(out ItemDrop component)) return;
        if (component.m_itemData.m_shared.m_armorMaterial is not { } material) return;
            
        var materialName = material.name.Replace("(Instance)", string.Empty).Trim();
        if (!TextureReplacer.m_materials.TryGetValue(materialName, out TextureReplacer.MaterialData data)) return;
        if (!data.m_specialTextures.TryGetValue("_ChestTex", out Dictionary<Season, Texture?> textures)) return;
            
        if (textures.TryGetValue(Configs.m_season.Value, out Texture? texture))
        {
            m_visEquipment.m_bodyModel.material.SetTexture(ChestTex, texture);
        }
        else
        {
            if (!data.m_originalTextures.TryGetValue("_ChestTex", out Texture originalChestTex)) return;
            m_visEquipment.m_bodyModel.material.SetTexture(ChestTex, originalChestTex);
        }
    }

    public void UpdateLegTexture()
    {
        if (m_visEquipment == null || m_visEquipment.m_bodyModel == null) return;
        if (ObjectDB.m_instance.GetItemPrefab(m_visEquipment.m_currentLegItemHash) is not { } legsItem) return;
        if (!legsItem.TryGetComponent(out ItemDrop component)) return;
        if (component.m_itemData.m_shared.m_armorMaterial is not { } material) return;
        var materialName = material.name.Replace("(Instance)", string.Empty).Trim();
        if (!TextureReplacer.m_materials.TryGetValue(materialName, out TextureReplacer.MaterialData data)) return;
        if (!data.m_specialTextures.TryGetValue("_LegsTex", out Dictionary<Season, Texture?> textures)) return;
            
        if (textures.TryGetValue(Configs.m_season.Value, out Texture? texture))
        {
            m_visEquipment.m_bodyModel.material.SetTexture(LegsTex, texture);
        }
        else
        {
            if (!data.m_originalTextures.TryGetValue("_LegsTex", out Texture originalChestTex)) return;
            m_visEquipment.m_bodyModel.material.SetTexture(LegsTex, originalChestTex);
        }
    }

    public static void UpdateAll()
    {
        foreach (var instance in m_instances)
        {
            instance.UpdatePlayerMaterial();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    private static class Player_Awake_Patch
    {
        private static void Postfix(Player __instance)
        {
            __instance.gameObject.AddComponent<VisEquipController>();
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetChestEquipped))]
    private static class VisEquipment_SetChestEquipped_Patch
    {
        private static void Postfix(VisEquipment __instance, bool __result)
        {
            if (!__result || !__instance.TryGetComponent(out VisEquipController controller)) return;
            controller.UpdateChestTexture();
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetLegEquipped))]
    private static class VisEquipment_SetLegsEquipped_Patch
    {
        private static void Postfix(VisEquipment __instance, bool __result)
        {
            if (!__result || !__instance.TryGetComponent(out VisEquipController controller)) return;
            controller.UpdateLegTexture();
        }
    }
}