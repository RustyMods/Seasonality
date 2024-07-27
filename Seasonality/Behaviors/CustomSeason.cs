using System.Collections.Generic;
using Seasonality.Seasons;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Behaviors;

public class CustomSeason : MonoBehaviour
{
    private Dictionary<string, Dictionary<string, Texture?>> m_textures = new();
    private readonly Dictionary<Material, Texture> m_textureMap = new();
    private readonly Dictionary<Material, Texture> m_mossMap = new();
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private SeasonalityPlugin.Season m_lastSeason;

    private static readonly List<CustomSeason> Instances = new();

    public static void UpdateInstances()
    {
        foreach (CustomSeason? instance in Instances)
        {
            instance.SetTextures();
        }
    }
    public void Awake()
    {
        if (!TextureManager.RegisteredCustomTextures.TryGetValue(gameObject.name.Replace("(Clone)", string.Empty),
                out Dictionary<string, Dictionary<string, Texture?>> textures)) return;
        m_textures = textures;
        CacheMaterials();
        SetTextures();
        Instances.Add(this);
    }

    public void OnDestroy()
    {
        if (Instances.Contains(this))
            Instances.Remove(this);
    }

    private void CacheMaterials()
    {
        Renderer[]? m_renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in m_renderers)
        {
            foreach (Material? material in renderer.materials)
            {
                if (material.HasProperty(MainTex))
                    m_textureMap[material] = material.GetTexture(MainTex);
                if (material.HasProperty(MossTex))
                    m_mossMap[material] = material.GetTexture(MossTex);
            }
        }
    }

    public void SetTextures()
    {
        if (m_lastSeason == SeasonalityPlugin._Season.Value) return;
        SetPrefabMoss();
        SetPrefabTextures();
        m_lastSeason = SeasonalityPlugin._Season.Value;
    }

    private void SetPrefabTextures()
    {
        UpdateTexture(SeasonalityPlugin._Season.Value.ToString().ToLower());
    }

    private void UpdateTexture(string key)
    {
        foreach (KeyValuePair<Material, Texture> kvp in m_textureMap)
        {
            bool success = false;
            string matName = kvp.Key.name.Replace("(Instance)", string.Empty).Trim();
            if (m_textures.TryGetValue(matName, out Dictionary<string, Texture?> textures))
            {
                if (textures.TryGetValue(key, out Texture? tex))
                {
                    kvp.Key.SetTexture(MainTex, tex);
                    success = true;
                };
            };
            if (!success)
            {
                kvp.Key.SetTexture(MainTex, kvp.Value);
            }
        }
    }

    private void SetPrefabMoss()
    {
        switch (SeasonalityPlugin._Season.Value)
        {
            case SeasonalityPlugin.Season.Winter:
                if (TextureManager.Pillar_Snow == null) break;
                SetMoss(TextureManager.Pillar_Snow);
                break;
            case SeasonalityPlugin.Season.Fall:
                if (!MaterialReplacer.CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) break;
                SetMoss(HeathMoss);
                break;
            default:
                ResetMossTex();
                break;
        }
    }

    private void ResetMossTex()
    {
        foreach(var kvp in m_mossMap) kvp.Key.SetTexture(MossTex, kvp.Value);
    }

    private void SetMoss(Texture texture)
    {
        foreach (KeyValuePair<Material, Texture> kvp in m_mossMap)
        {
            if (kvp.Key.HasProperty(MossTex)) kvp.Key.SetTexture(MossTex, texture);
        }
    }
}