using System.Collections.Generic;
using HarmonyLib;
using Seasonality.Helpers;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Behaviors;

public class MossController : MonoBehaviour
{
    private readonly Dictionary<Material, Texture> m_textureMap = new();
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");
    
    private static readonly List<MossController> m_instances = new();

    public static void UpdateAll()
    {
        foreach (MossController instance in m_instances)
        {
            instance.SetMoss();
        }
    }

    public void Awake()
    {
        m_instances.Add(this);
    }
    public void OnDestroy()
    {
        m_instances.Remove(this);
    }

    private void Cache()
    {
        var m_renderers = gameObject.GetComponentsInChildren<Renderer>();
        if (m_renderers.Length <= 0) return;
        foreach (Renderer renderer in m_renderers)
        {
            foreach (Material? material in renderer.sharedMaterials)
            {
                if (material == null) continue;
                if (material.HasProperty(MossTex))
                {
                    if (TextureManager.stonemoss.m_tex is { } tex)
                    {
                        m_textureMap[material] = tex;
                        continue;
                    }
                    if (MaterialController.m_mossTextures.TryGetValue("stonemoss", out Texture originalMoss))
                    {
                        m_textureMap[material] = originalMoss;
                    }
                    else
                    {
                        var ogMoss = material.GetTexture(MossTex);
                        m_textureMap[material] = ogMoss;
                        MaterialController.m_mossTextures[ogMoss.name] = ogMoss;
                    }
                }
            }
        }
    }
    
    public void SetMoss()
    {
        switch (Configs.m_season.Value)
        {
            case Configs.Season.Fall:
                foreach (var kvp in m_textureMap)
                {
                    kvp.Key.SetTexture(MossTex, TextureManager.Stonemoss_heath.m_tex);
                }
                break;
            case Configs.Season.Winter:
                foreach (var kvp in m_textureMap)
                {
                    kvp.Key.SetTexture(MossTex, TextureManager.AshOnRocks_d.m_tex);
                }
                break;
            default:
                foreach (var kvp in m_textureMap)
                {
                    kvp.Key.SetTexture(MossTex, kvp.Value);
                }
                break;
        }
    }

    [HarmonyPatch(typeof(BossStone), nameof(BossStone.Start))]
    private static class BossStone_Start_Patch
    {
        private static void Postfix(BossStone __instance)
        {
            if (!__instance) return;
            if (Configs.m_mossController.Value is Configs.Toggle.Off) return;
            if (__instance.GetComponentInParent<LocationMossController>()) return;
            var component = __instance.gameObject.AddComponent<MossController>();
            component.Cache();
            component.SetMoss();
        }
    }
}

