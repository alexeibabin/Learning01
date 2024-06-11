using System;

public struct WindowOpenedEvent : IEvent
{
    public Type type;

    public WindowOpenedEvent(Type type)
    {
        this.type = type;
    }
}
public struct WindowClosedEvent : IEvent
{
    public Type type;

    public WindowClosedEvent(Type type)
    {
        this.type = type;
    }
}

