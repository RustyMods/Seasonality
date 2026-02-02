using System;
using UnityEngine;

namespace Seasonality;

public class Clone
{
    private GameObject? Source;
    private GameObject? Prefab;
    public readonly string SourceName;
    private readonly string NewName;
    private bool Loaded;
    public event Action<GameObject>? OnCreated;
    public Clone(string prefabName, string newName)
    {
        SourceName = prefabName;
        NewName = newName;
        CloneManager.clones[newName] = this;
    }

    public Clone(GameObject prefab, string newName) : this(prefab.name, newName)
    {
        Source = prefab;
    }

    internal GameObject? Create()
    {
        if (Loaded) return Prefab;
        Source ??= PrefabManager.GetPrefab(SourceName);
        if (Source == null) return null;
        
        Prefab = UnityEngine.Object.Instantiate(Source, CloneManager.GetRootTransform(), false);
        Prefab.name = NewName;
        OnCreated?.Invoke(Prefab);
        PrefabManager.RegisterPrefab(Prefab);
        CloneManager.prefabs[Prefab.name] = Prefab;
        Loaded = true;
        return Prefab;
    }
}