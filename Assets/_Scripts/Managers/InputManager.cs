using UnityEngine;

public class InputManager
{
    private static PlayerInputActions _actions;

    public static void Initialize()
    {
        if (_actions == null)
        {
            _actions = new PlayerInputActions();
            _actions.Enable();
        }
    }

    public static Vector2 GetMoveInput()
    {
        Initialize();
        return _actions.Player.GetMoveValue();
    }

    public static bool GetJumpDown()
    {
        Initialize();
        return _actions.Player.GetJumpDown();
    }

    public static bool GetJumpHeld()
    {
        Initialize();
        return _actions.Player.GetJumpHeld();
    }

    public static bool GetJumpUp()
    {
        Initialize();
        return _actions.Player.GetJumpUp();
    }

    public static void Dispose()
    {
        _actions?.Dispose();
        _actions = null;
    }
}
