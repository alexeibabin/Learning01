using UnityEngine;

public class CharacterActions
{
    public Vector2 MoveValue => InputManager.GetMoveInput();

    public bool JumpPressed  => InputManager.GetJumpDown();
    public bool JumpHeld     => InputManager.GetJumpHeld();
    public bool JumpReleased => InputManager.GetJumpUp();

    public CharacterActions()
    {
        InputManager.Initialize();
    }

    public void Dispose()
    {
        // InputManager is a singleton; don't dispose it here
    }
}
