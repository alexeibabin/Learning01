using UnityEngine;

public class Script
{
    public static object Main()
    {
        var ca = new CharacterActions();
        var results = new System.Collections.Generic.List<string>();

        results.Add($"JumpPressed: {ca.JumpPressed}");
        results.Add($"JumpHeld: {ca.JumpHeld}");
        results.Add($"JumpReleased: {ca.JumpReleased}");

        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        results.Add($"Keyboard.current: {(keyboard != null ? "valid" : "null")}");
        if (keyboard != null)
        {
            results.Add($"SpaceKey pressed: {keyboard.spaceKey.isPressed}");
            results.Add($"SpaceKey wasPressedThisFrame: {keyboard.spaceKey.wasPressedThisFrame}");
        }

        return string.Join("\n", results);
    }
}
