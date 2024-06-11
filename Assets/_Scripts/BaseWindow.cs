using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseWindow : ProviderView<Type, BaseWindow>
{
    [SerializeField] protected GameObject _container = default;

    public Transform Container
    {
        get { return _container.transform; }
    }

    public override Type Type => GetType();

    protected override AbstractProvider<Type, BaseWindow> Provider => Game.WindowsManager;

    public void Activate()
    {
        if (_container != null)
        {
            ActivateContainer(true);
            OnActivated();
        }
        else
        {
            Debug.LogError("Missing container link");
        }
    }

    public void Deactivate()
    {
        if (_container != null)
        {
            ActivateContainer(false);
            OnDeactivated();
        }
        else
        {
            Debug.LogError("Missing container link");
        }
    }

    protected virtual void ActivateContainer(bool activate)
    {
        _container.SetActive(activate);
    }

    protected virtual void OnActivated()
    {
    }

    protected virtual void OnDeactivated()
    {
    }
}
