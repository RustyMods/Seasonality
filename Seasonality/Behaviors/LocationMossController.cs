using System.Collections.Generic;
using HarmonyLib;
using Seasonality.Helpers;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Behaviors;

public class LocationMossController : MonoBehaviour
{
    public static readonly List<LocationMossController> m_instances = new();
    public Renderer[] m_renderers = null!;
    public readonly Dictionary<Material, Texture> m_textures = new();
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");

    public void Awake()
    {
        m_renderers = GetComponentsInChildren<Renderer>();
        m_instances.Add(this);
        if (m_renderers.Length <= 0) return;
        foreach (var renderer in m_renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material.HasProperty(MossTex))
                {
                    var ogMoss = material.GetTexture(MossTex);
                    if (Configs.m_season.Value is Configs.Season.Summer && ogMoss != null)
                    {
                        m_textures[material] = ogMoss;
                    }
                    else if (MaterialController.m_mossTextures.TryGetValue("stonemoss", out Texture stonemoss))
                    {
                        m_textures[material] = stonemoss;
                    }
                }
            }
        }
        UpdateMoss();
    }

    public void OnDestroy()
    {
        m_instances.Remove(this);
    }

    public void UpdateMoss()
    {
        switch (Configs.m_season.Value)
        {
            case Configs.Season.Winter:
                foreach (var material in m_textures.Keys)
                {
                    material.SetTexture(MossTex, TextureManager.AshOnRocks_d.m_tex);
                }
                break;
            case Configs.Season.Fall:
                foreach (var material in m_textures.Keys)
                {
                    material.SetTexture(MossTex, TextureManager.Stonemoss_heath.m_tex);
                }

                break;
            default:
                foreach (var material in m_textures)
                {
                    material.Key.SetTexture(MossTex, material.Value);
                }

                break;
        }
    }

    public static void UpdateAll()
    {
        foreach(var instance in m_instances) instance.UpdateMoss();
    }


    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
    private static class ZoneSystem_SpawnLocation_Patch
    {
        private static void Postfix(ref GameObject __result)
        {
            if (!__result) return;
            __result.AddComponent<LocationMossController>();
        }
    }
}