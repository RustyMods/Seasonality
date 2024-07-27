using System.Collections.Generic;
using Seasonality.Seasons;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Behaviors;

public class SeasonalLocation : MonoBehaviour
{
    public Renderer[] m_renderers = null!;
    private readonly Dictionary<Material, Texture> m_textureMap = new();
    private static readonly int MossTex = Shader.PropertyToID("_MossTex");

    private SeasonalityPlugin.Season m_lastSeason;

    private static readonly List<SeasonalLocation> Instances = new();

    public static void UpdateInstances()
    {
        foreach (var instance in Instances)
        {
            if (instance.m_lastSeason == SeasonalityPlugin._Season.Value) continue;
            instance.SetLocationMoss();
        }
    }

    public void Awake()
    {
        m_renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in m_renderers)
        {
            foreach (Material? material in renderer.materials)
            {
                if (material.HasProperty("_MossTex"))
                {
                    m_textureMap[material] = material.GetTexture(MossTex);
                }
            }
        }
        SetLocationMoss();
        Instances.Add(this);
    }

    public void SetLocationMoss()
    {
        switch (SeasonalityPlugin._Season.Value)
        {
            case SeasonalityPlugin.Season.Spring:
                ResetMossTexture();
                m_lastSeason = SeasonalityPlugin.Season.Spring;
                break;
            case SeasonalityPlugin.Season.Summer:
                ResetMossTexture();
                m_lastSeason = SeasonalityPlugin.Season.Summer;
                break;
            case SeasonalityPlugin.Season.Fall:
                if (!MaterialReplacer.CachedTextures.TryGetValue("rock_heath_moss", out Texture HeathMoss)) return;
                ModifyMoss(HeathMoss);
                m_lastSeason = SeasonalityPlugin.Season.Fall;
                break;
            case SeasonalityPlugin.Season.Winter:
                if (TextureManager.Pillar_Snow == null) return;
                ModifyMoss(TextureManager.Pillar_Snow);
                m_lastSeason = SeasonalityPlugin.Season.Winter;
                break;
        }
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