using System.Collections.Generic;
using UnityEngine;

namespace Seasonality;

public static class CloneManager
{
    private static readonly GameObject root;
    internal static readonly Dictionary<string, GameObject> prefabs;
    internal static readonly Dictionary<string, Clone> clones;
    static CloneManager()
    {
        prefabs = new Dictionary<string, GameObject>();
        clones =  new Dictionary<string, Clone>();
        root = new GameObject($"{SeasonalityPlugin.ModName}_prefab_root");
        Object.DontDestroyOnLoad(root);
        root.SetActive(false);
    }

    public static Transform GetRootTransform()
    {
        return root.transform;
    }
}