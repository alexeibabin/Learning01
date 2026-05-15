using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputActions
{
    public PlayerActions Player { get; private set; }

    public PlayerInputActions()
    {
        Player = new PlayerActions();
    }

    public void Enable()
    {
        // Enable is handled per-action in the Player class
    }

    public void Disable()
    {
        // Disable is handled per-action in the Player class
    }

    public void Dispose()
    {
        // Nothing to dispose for keyboard input
    }

    public class PlayerActions
    {
        private Keyboard _keyboard;

        public PlayerActions()
        {
            _keyboard = Keyboard.current;
        }

        public Vector2 GetMoveValue()
        {
            if (_keyboard == null) return Vector2.zero;

            Vector2 move = Vector2.zero;

            // WASD keys
            if (_keyboard.wKey.isPressed || _keyboard.upArrowKey.isPressed)
                move.y += 1;
            if (_keyboard.sKey.isPressed || _keyboard.downArrowKey.isPressed)
                move.y -= 1;
            if (_keyboard.dKey.isPressed || _keyboard.rightArrowKey.isPressed)
                move.x += 1;
            if (_keyboard.aKey.isPressed || _keyboard.leftArrowKey.isPressed)
                move.x -= 1;

            return move.normalized;
        }
    }
}
