using UnityEngine;
using UnityEngine.InputSystem;

public class JumpInputMonitor : MonoBehaviour
{
    int frameCount = 0;

    void Update()
    {
        frameCount++;
        var kb = Keyboard.current;
        if (kb == null)
        {
            Debug.Log("Keyboard is null!");
            return;
        }

        var spacePressed = kb.spaceKey.wasPressedThisFrame;
        var spaceHeld = kb.spaceKey.isPressed;

        if (frameCount % 60 == 0) // Log every 60 frames (1 second at 60fps)
            Debug.Log($"Frame {frameCount}: space pressed={spacePressed}, held={spaceHeld}");

        if (spacePressed)
            Debug.Log($"SPACE PRESSED at frame {frameCount}!");
    }
}
