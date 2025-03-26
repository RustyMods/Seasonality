using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Seasonality.Helpers;
using Seasonality.Textures;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Seasonality.Behaviors;

public class RandomColors : MonoBehaviour
{
    private static readonly List<Data> m_prefabs = new()
    {
        new Data("Beech1", "beech_leaf", "leaf"),
        new Data("Birch1", "birch_leaf"),
        new Data("Birch2", "birch_leaf"),
        new Data("Birch1_aut", "birch_leaf_aut"),
        new Data("Birch2_aut", "birch_leaf_aut"),
        new Data("Beech_small1", "beech_leaf_small"),
        new Data("Beech_small2", "beech_leaf_small"),
        new Data("Oak1", "oak_leaf", "leaf"),
        new Data("YggaShoot1", "Shoot_Leaf_mat"),
        new Data("YggaShoot2", "Shoot_Leaf_mat"),
        new Data("YggaShoot3", "Shoot_Leaf_mat"),
        new Data("YggaShoot_small1", "Shoot_Leaf_mat")
    };

    private static readonly List<Data> m_vfx = new()
    {
        new Data("vfx_beech_cut", "beech_particle"),
        new Data("vfx_birch1_cut", "birch_particle"),
        new Data("vfx_birch2_cut", "birch_particle"),
        new Data("vfx_birch1_aut_cut", "birch_particle"),
        new Data("vfx_birch2_aut_cut", "birch_particle"),
        new Data("vfx_oak_cut", "oak_particle"),
        new Data("vfx_beech_small1_destroy", "beech_particle"),
        new Data("vfx_beech_small2_destroy", "beech_particle"),
        new Data("vfx_yggashoot_cut", "shoot_leaf_particle"),
        new Data("vfx_yggashoot_small1_destroy", "shoot_leaf_particle")
    };

    private class Data
    {
        public readonly string m_prefabName;
        public readonly List<string> m_materialNames;
        
        public Data(string prefabName, params string[] materialNames)
        {
            m_prefabName = prefabName;
            m_materialNames = materialNames.ToList();
        }
    }

    public Renderer[]? m_renderers;
    public ParticleSystem? m_particleSystem;
    public ParticleSystem.MinMaxGradient m_originalGradient;
    public static readonly List<RandomColors> m_instances = new();

    public void Awake()
    {
        if (Configs.m_randomColors.Value is Configs.Toggle.Off) return;
        m_renderers = GetComponentsInChildren<Renderer>(true);
        m_particleSystem = GetComponentInChildren<ParticleSystem>();
        if (m_renderers != null)
        {
            foreach (var renderer in m_renderers)
            {
                List<Material> newMats = new();
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    var instanceName = material.name.Replace("(Instance)", string.Empty).Trim();
                    if (MaterialController.m_fallMaterials.TryGetValue(instanceName, out List<MaterialController.MaterialData> data))
                    {
                        var mat = data[Random.Range(0, data.Count)];
                        newMats.Add(mat.m_material);
                    }
                    else
                    {
                        newMats.Add(material);
                    }
                }

                renderer.materials = newMats.ToArray();
                renderer.sharedMaterials = newMats.ToArray();
            }
        }

        if (m_particleSystem != null)
        {
            var main = m_particleSystem.main;
            m_originalGradient = main.startColor;
        }

        m_instances.Add(this);
        UpdateParticleColors();
    }

    public void OnDestroy()
    {
        m_instances.Remove(this);
    }

    public static void UpdateAll()
    {
        foreach(var instance in m_instances) instance.UpdateParticleColors();
    }

    public void UpdateParticleColors()
    {
        if (Configs.m_particlesController.Value is Configs.Toggle.Off) return;
        if (m_particleSystem == null) return;
        var main = m_particleSystem.main;
        m_originalGradient = main.startColor;
        switch (Configs.m_season.Value)
        {
            case Configs.Season.Fall:
                main.startColor = new ParticleSystem.MinMaxGradient()
                {
                    color = new Color32(205, 92, 92, 255),
                    colorMax = new Color32(218, 165, 32, 255),
                    colorMin = new Color32(233, 116, 81, 255),
                    mode = ParticleSystemGradientMode.RandomColor,
                };
                break;
            case Configs.Season.Winter:
                main.startColor = new ParticleSystem.MinMaxGradient()
                {
                    color = Color.white,
                    mode = ParticleSystemGradientMode.Color
                };
                break;
            default:
                main.startColor = m_originalGradient;
                return;
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix(ZNetScene __instance)
        {
            foreach (var data in m_prefabs)
            {
                if (__instance.GetPrefab(data.m_prefabName) is { } prefab)
                {
                    prefab.AddComponent<RandomColors>();
                }
            }

            foreach (var data in m_vfx)
            {
                if (__instance.GetPrefab(data.m_prefabName) is { } prefab)
                {
                    prefab.AddComponent<RandomColors>();
                }
            }
        }
    }
}