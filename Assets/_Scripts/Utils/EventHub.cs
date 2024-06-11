using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface IEvent
{

}

public interface IEventHub
{
    IDisposable Subscribe<T>(Action<T> handler) where T : struct, IEvent;
    void Notify<T>(T message) where T : struct, IEvent;
}

public class EventHub : IEventHub
{
    private readonly Dictionary<Type, object> _subscribers = new Dictionary<Type, object>();

    public IDisposable Subscribe<T>(Action<T> handler) where T : struct, IEvent
    {
        var type = typeof(T);
        ReactiveProperty<T> subscribers;
        if (_subscribers.TryGetValue(type, out var value))
        {
            subscribers = value as ReactiveProperty<T>;
        }
        else
        {
            subscribers = new ReactiveProperty<T>();
            _subscribers.Add(type, subscribers);
        }

        return subscribers.SkipLatestValueOnSubscribe().Subscribe(handler);
    }

    public void Notify<T>(T message) where T : struct, IEvent
    {
        var type = typeof(T);
        if (_subscribers.TryGetValue(type, out var value))
        {
            var subscribers = value as ReactiveProperty<T>;

            try
            {
                subscribers?.SetValueAndForceNotify(message);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}