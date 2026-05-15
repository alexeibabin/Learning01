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
        public Vector2 GetMoveValue()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return Vector2.zero;

            Vector2 move = Vector2.zero;

            // WASD keys
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                move.y += 1;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                move.y -= 1;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                move.x += 1;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                move.x -= 1;

            return move.normalized;
        }

        public bool GetJumpDown()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        }

        public bool GetJumpHeld()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.isPressed;
        }

        public bool GetJumpUp()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasReleasedThisFrame;
        }
    }
}
