using System.Collections.Generic;
using Seasonality.Seasons;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Behaviors;

public class SeasonalBossStone : MonoBehaviour
{
    // public Renderer[] m_renderers = null!;
    private readonly Dictionary<Material, Texture> m_textureMap = new();
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");

    private SeasonalityPlugin.Season m_lastSeason;

    private static readonly List<SeasonalBossStone> Instances = new();

    public static void UpdateInstances()
    {
        foreach (SeasonalBossStone instance in Instances)
        {
            if (instance.m_lastSeason == SeasonalityPlugin._Season.Value) continue;
            instance.SetLocationMoss();
        }
    }

    public void Awake()
    {
        CacheMaterials();
        SetLocationMoss();
        Instances.Add(this);
    }
    public void OnDestroy()
    {
        Instances.Remove(this);
    }

    private void CacheMaterials()
    {
        var m_renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in m_renderers)
        {
            foreach (Material? material in renderer.materials)
            {
                if (material.HasProperty("_MossTex"))
                {
                    m_textureMap[material] = material.GetTexture(MossTex);
                }
            }
        }
    }
    
    public void SetLocationMoss()
    {
        switch (SeasonalityPlugin._Season.Value)
        {
            case SeasonalityPlugin.Season.Spring or SeasonalityPlugin.Season.Summer:
                if (!MaterialReplacer.CachedTextures.TryGetValue("Firetree_oldlog_moss", out Texture StoneMoss)) return;
                ModifyMoss(StoneMoss);
                break;
            case SeasonalityPlugin.Season.Fall:
                if (!MaterialReplacer.CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) return;
                ModifyMoss(HeathMoss);
                break;
            case SeasonalityPlugin.Season.Winter:
                if (TextureManager.Pillar_Snow == null) return;
                ModifyMoss(TextureManager.Pillar_Snow);
                break;
        }
        m_lastSeason = SeasonalityPlugin._Season.Value;

    }

    public void ResetMossTexture()
    {
        foreach (KeyValuePair<Material, Texture> kvp in m_textureMap)
        {
            kvp.Key.SetTexture(MossTex, kvp.Value);
        }
    }

    public void ModifyMoss(Texture texture)
    {
        foreach (KeyValuePair<Material, Texture> kvp in m_textureMap)
        {
            kvp.Key.SetTexture(MossTex, texture);
        }
    }
}

