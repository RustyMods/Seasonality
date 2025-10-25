using System.Collections.Generic;
using HarmonyLib;
using Seasonality.Helpers;
using Seasonality.Textures;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Seasonality.Behaviors;

public class RandomColors : MonoBehaviour
{
    public Renderer[]? m_renderers;
    public ParticleSystem? m_particleSystem;
    public ParticleSystem.MinMaxGradient m_originalGradient;
    public static readonly List<RandomColors> m_instances = new();

    public void Awake()
    {
        if (Configs.m_randomColors.Value is Toggle.Off) return;
        if (!new Configs.SerializedNameList(Configs.m_fallObjects.Value).m_names.Contains(name.Replace("(Clone)", string.Empty))) return;
        m_renderers = GetComponentsInChildren<Renderer>(true);
        m_particleSystem = GetComponentInChildren<ParticleSystem>();
        m_instances.Add(this);
    }

    public void Start()
    {
        if (m_renderers != null)
        {
            foreach (var renderer in m_renderers)
            {
                if (renderer.sharedMaterials == null) continue;
                Material[] sharedMaterials = renderer.sharedMaterials;

                for (int index = 0; index < sharedMaterials.Length; ++index)
                {
                    if (sharedMaterials[index] == null) continue;
                    var instanceName = sharedMaterials[index].name.Replace("(Instance)", string.Empty).Trim();
                    if (TextureReplacer.m_fallMaterials.TryGetValue(instanceName, out List<TextureReplacer.MaterialData> data))
                    {
                        sharedMaterials[index] = data[Random.Range(0, data.Count)].m_material;
                    }
                }

                renderer.sharedMaterials = sharedMaterials;
            }
        }

        if (m_particleSystem != null)
        {
            var main = m_particleSystem.main;
            m_originalGradient = main.startColor;
        }
        
        UpdateParticleColors();
    }

    public void OnDestroy()
    {
        m_instances.Remove(this);
    }

    public static void UpdateAll()
    {
        foreach (var instance in m_instances)
        {
            instance.UpdateParticleColors();
        }
    }

    public void UpdateParticleColors()
    {
        if (Configs.m_particlesController.Value is Toggle.Off) return;
        if (m_particleSystem == null) return;
        var main = m_particleSystem.main;
        m_originalGradient = main.startColor;
        switch (Configs.m_season.Value)
        {
            case Season.Fall:
                main.startColor = new ParticleSystem.MinMaxGradient()
                {
                    color = new Color32(205, 92, 92, 255),
                    colorMax = new Color32(218, 165, 32, 255),
                    colorMin = new Color32(233, 116, 81, 255),
                    mode = ParticleSystemGradientMode.RandomColor,
                };
                break;
            case Season.Winter:
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

    private static readonly List<GameObject> m_modifiedPrefabs = new();

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix(ZNetScene __instance)
        {
            foreach (var name in new Configs.SerializedNameList(Configs.m_fallObjects.Value).m_names)
            {
                if (__instance.GetPrefab(name) is { } prefab)
                {
                    prefab.AddComponent<RandomColors>();
                    m_modifiedPrefabs.Add(prefab);
                }
            }

            Configs.m_fallObjects.SettingChanged += (_, _) =>
            {
                foreach (var prefab in m_modifiedPrefabs)
                {
                    if (!prefab.TryGetComponent(out RandomColors component)) continue;
                    Destroy(component);
                }
                m_modifiedPrefabs.Clear();
                foreach (var name in new Configs.SerializedNameList(Configs.m_fallObjects.Value).m_names)
                {
                    if (__instance.GetPrefab(name) is { } prefab)
                    {
                        prefab.AddComponent<RandomColors>();
                        m_modifiedPrefabs.Add(prefab);
                    }
                }
            };
        }
    }
}