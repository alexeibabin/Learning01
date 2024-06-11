using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class AssetManager : MonoBehaviour
{
    public T Get<T>(string assetName, bool log = true) where T : Object
    {
        var asset = Resources.Load<T>(assetName);

        if (log && asset == null)
        {
            Debug.LogErrorFormat("Failed to load asset - " + assetName);
        }

        return asset;
    }

    public T Get<T>() where T : Object
    {
        return Get<T>(typeof(T).Name);
    }

    public void GetAsync<T>(string assetName, Action<T> callBack)
    {
        callBack?.Invoke(default);
    }
}
