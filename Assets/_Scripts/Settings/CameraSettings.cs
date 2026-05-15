using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings", menuName = "Settings/Camera")]
public class CameraSettings : ScriptableObject
{
    [Header("Sensitivity")]
    public float sensitivityX = 0.15f;
    public float sensitivityY = 0.15f;

    [Header("Pitch Limits")]
    public float minPitch = -30f;
    public float maxPitch =  60f;

    [Header("Zoom")]
    public int numZoomSteps     = 15;
    public float minZoomDistance = 2f;
    public float maxZoomDistance = 12f;
    public float zoomSmoothTime = 0.12f;

    [Header("Follow")]
    public float pivotHeight      = 1.5f;
    public float focusRadius      = 0.5f;
    public float focusCentering   = 0.5f;

    [Header("Camera Offset")]
    public Vector3 socketOffset = Vector3.zero;
}
