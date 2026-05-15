using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] CameraSettings _settings;

    float _yaw;
    float _pitch;
    float _targetDistance = 6f;
    float _currentDistance = 6f;
    float _distanceVelocity;
    Vector3 _focusPoint;

    void Awake()
    {
        _yaw = transform.eulerAngles.y;
        float rawPitch = transform.eulerAngles.x;
        _pitch = rawPitch > 180f ? rawPitch - 360f : rawPitch;

        _focusPoint = _target != null && _settings != null
            ? _target.position + Vector3.up * _settings.pivotHeight
            : transform.position;
    }

    void OnEnable() => Cursor.lockState = CursorLockMode.Locked;
    void OnDisable() => Cursor.lockState = CursorLockMode.None;

    void Update()
    {
        HandleCursorLock();

        if (Cursor.lockState != CursorLockMode.Locked || _settings == null) return;

        ReadMouseLook();
        ReadZoom();
    }

    void LateUpdate()
    {
        if (_target == null || _settings == null) return;

        UpdateFocusPoint();
        ApplyCameraTransform();
    }

    void HandleCursorLock()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.None;
        else if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.None)
            Cursor.lockState = CursorLockMode.Locked;
    }

    void ReadMouseLook()
    {
        Vector2 delta = Mouse.current.delta.ReadValue();
        _yaw += delta.x * _settings.sensitivityX;
        _pitch = Mathf.Clamp(_pitch - delta.y * _settings.sensitivityY, _settings.minPitch, _settings.maxPitch);
    }

    void ReadZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        _targetDistance = Mathf.Clamp(_targetDistance - scroll * _settings.zoomSpeed, _settings.minZoom, _settings.maxZoom);
        _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _distanceVelocity, _settings.zoomSmoothTime, float.MaxValue, Time.unscaledDeltaTime);
    }

    void UpdateFocusPoint()
    {
        Vector3 targetPoint = _target.position + Vector3.up * _settings.pivotHeight;

        if (_settings.focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, _focusPoint);
            float t = 1f;

            if (distance > 0.01f && _settings.focusCentering > 0f)
                t = Mathf.Pow(1f - _settings.focusCentering, Time.unscaledDeltaTime);

            if (distance > _settings.focusRadius)
                t = Mathf.Min(t, _settings.focusRadius / distance);

            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
        }
        else
        {
            _focusPoint = targetPoint;
        }
    }

    void ApplyCameraTransform()
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 lookDirection = rotation * Vector3.forward;
        Vector3 cameraPosition = _focusPoint - lookDirection * _currentDistance + rotation * _settings.socketOffset;

        transform.SetPositionAndRotation(cameraPosition, rotation);
    }
}
