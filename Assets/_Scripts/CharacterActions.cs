using UnityEngine;

public class CharacterActions
{
    public Vector2 MoveValue => InputManager.GetMoveInput();

    public CharacterActions()
    {
        InputManager.Initialize();
    }

    public void Dispose()
    {
        // InputManager is a singleton; don't dispose it here
    }
}
