using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Seasonality.Helpers;

public static class ShaderFix
{
    public static readonly Dictionary<string, Shader> m_shaders = new();

    public static Shader GetShader(string shaderName, Shader originalShader)
    {
        return !m_shaders.TryGetValue(shaderName, out Shader shader) ? originalShader : shader;
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    private static class ShaderFix_CacheShaders
    {
        private static void Postfix() => CacheShaders();
    }
    
    public static void CacheShaders()
    {
        var assetBundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
        foreach (var bundle in assetBundles)
        {
            IEnumerable<Shader>? bundleShaders;
            try
            {
                bundleShaders = bundle.isStreamedSceneAssetBundle && bundle
                    ? bundle.GetAllAssetNames().Select(bundle.LoadAsset<Shader>).Where(shader => shader != null)
                    : bundle.LoadAllAssets<Shader>();
            }
            catch (Exception)
            {
                continue;
            }

            if (bundleShaders == null) continue;
            foreach (var shader in bundleShaders)
            {
                if (m_shaders.ContainsKey(shader.name)) continue;
                m_shaders[shader.name] = shader;
            }
        }
    }
}