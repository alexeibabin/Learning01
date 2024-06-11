using UnityEngine;

public abstract class ProviderView<TType, TProduct> : MonoBehaviour
{
    public abstract TType Type { get; }

    protected abstract AbstractProvider<TType, TProduct> Provider { get; }

    private void Awake()
    {
        Provider.Register(Type, GetComponent<TProduct>());
        OnAwake();
    }

    protected virtual void OnAwake()
    {
    }
}