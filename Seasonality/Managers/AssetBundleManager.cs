using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Seasonality;

[PublicAPI]
public static class AssetBundleManager
{
    private static readonly Dictionary<string, AssetBundle> CachedBundles = new();

    public static T? LoadAsset<T>(string assetBundle, string prefab) where T : UnityEngine.Object
    {
        return GetAssetBundle(assetBundle) is not { } bundle ? null : bundle.LoadAsset<T>(prefab);
    }
    
    public static AssetBundle GetAssetBundle(string fileName)
    {
        if (CachedBundles.TryGetValue(fileName, out var assetBundle)) return assetBundle;
        if (AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(b => b.name == fileName) is {} existing)
        {
            CachedBundles[fileName] = existing;
            return existing;
        }
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
        using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
        AssetBundle? bundle = AssetBundle.LoadFromStream(stream);
        CachedBundles[fileName] = bundle;
        return bundle;
    }
}