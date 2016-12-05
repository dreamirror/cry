using UnityEngine;
using System.Collections;
using System;

abstract public class IAssetContainerBase : IDisposable
{
    abstract public void Free();

    void IDisposable.Dispose()
    {
        Free();
    }
}

public interface IAssetObject
{
    void OnAlloc();
    void OnFree();
    void InitPrefab();
}

public class AssetContainer<T> : IAssetContainerBase where T : Component, IAssetObject
{
    public T Asset { get; protected set; }
    public string Name { get { return Prefab.name; } }

    public bool IsInit { get { return Asset != null; } }

    public T Component { get { return Prefab.GetComponent<T>(); } }

    public AssetData Data { get; protected set; }
    public GameObject Prefab { get { return Data.Prefab; } }

    public AssetContainer(AssetData data)
    {
        Data = data;
    }

    virtual public T Alloc()
    {
        if (Data == null)
            return null;

        if (Asset == null)
            Asset = Data.Alloc<T>();
        Asset.OnAlloc();
        return Asset;
    }

    override public void Free()
    {
        if (Asset == null)
        {
            Debug.LogWarningFormat("[{0}] Free error", Data.Prefab != null ? Data.Prefab.name : "(unknown)");
            return;
        }

        Data.Free<T>(Asset);
        Asset.OnFree();
        Asset = null;
    }
}

public class CharacterAssetContainer : AssetContainer<Character>
{
    public string SkinName { get; private set; }

    public CharacterAssetContainer(AssetData asset_data, string skin_name)
        : base(asset_data)
    {
        SkinName = skin_name;
    }

    public override Character Alloc()
    {
        Character character = base.Alloc();
        character.CharacterAnimation.SetSkin(SkinName);
        return character;
    }

    public override void Free()
    {
        Asset.CharacterAnimation.FreeSkin();
        base.Free();
    }
}

