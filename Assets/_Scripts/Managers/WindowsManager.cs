using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class WindowProperties
{ 
    public readonly Type Type;
    public readonly WindowCanvasType Canvas;
    public readonly WindowPriorityType Priority;
    public readonly WindowLayerType Layer;

    public WindowProperties(Type type, WindowCanvasType canvas, WindowPriorityType priority, WindowLayerType layer)
    {
        Type = type;
        Canvas = canvas;
        Priority = priority;
        Layer = layer;
    }
}

public class WindowsManager : AbstractProvider<Type, BaseWindow>
{
    private static readonly List<WindowProperties> _gameWindows = new List<WindowProperties>()
    {
        new WindowProperties(typeof(LoadingWindow), WindowCanvasType.UI, WindowPriorityType.High, WindowLayerType.Loading),
        new WindowProperties(typeof(MainMenuWindow), WindowCanvasType.UI, WindowPriorityType.Medium, WindowLayerType.Overlay)
    };

    private readonly Dictionary<WindowLayerType, Type> _activeWindows = new Dictionary<WindowLayerType, Type>();

    public void CloseLayer(WindowLayerType layer)
    {
        if (_activeWindows.TryGetValue(layer, out var type))
        {
            Close(type);
        }
    }

    private BaseWindow GetWindow<T>(WindowProperties props) where T : BaseWindow
    {
        if (!TryGet(props.Type, out var window))
        {
            window = Game.AssetManager.Get<T>(@"Windows\" + props.Type.Name);

            if (window != null && Game.CanvasManager.TryGet(props.Canvas, out var canvas))
            {
                window = UnityEngine.Object.Instantiate(window, canvas.transform);
                Register(window);
            }
        }

        if (window != null)
            return window;

        Debug.LogErrorFormat("Window {0} Failed to load.", props.Type);
        return null;
    }

    private void Close(Type type)
    {
        var windowProperties = _gameWindows.FirstOrDefault(w => w.Type == type);
        if (windowProperties == null)
            return;

        if (TryGet(type, out var window))
            window.Deactivate();

        _activeWindows.Remove(windowProperties.Layer);
        Game.EventHub.Notify(new WindowClosedEvent(type));
    }

    public void Open<T>() where T : BaseWindow
    {
        var type = typeof(T);

        if(_activeWindows.Values.Any(w => w == type))
            return;

        var newWindowProperties = _gameWindows.FirstOrDefault(w => w.Type == type);
        if (newWindowProperties == null)
            return;

        if (_activeWindows.TryGetValue(newWindowProperties.Layer, out var activeWindowType))
        {
            var activeWindowProperties = _gameWindows.First(w => w.Type == activeWindowType);
            if (activeWindowProperties.Priority > newWindowProperties.Priority)
            {
                return;
            }

            if (TryGet(activeWindowType, out var activeWindow))
            {
                activeWindow.Deactivate();
                Game.EventHub.Notify(new WindowClosedEvent(activeWindowType));
            }

            _activeWindows.Remove(newWindowProperties.Layer);
        }

        var window = GetWindow<T>(newWindowProperties);
        window.Activate();
        _activeWindows.Add(newWindowProperties.Layer, type);

        Game.EventHub.Notify(new WindowOpenedEvent(type));
    }

    private void Register(BaseWindow window) => Register(window.Type, window);
}
