using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CheapCharController : MonoBehaviour
{
    [SerializeField] Animator characterAnimator;
    [SerializeField] Transform _camera;
    [SerializeField] float _moveSpeed = 4f;
    [SerializeField] float _rotationSpeed = 720f;

    CharacterActions ca;
    CharacterController _cc;
    float _verticalVelocity;

    void Awake() {
        ca = new CharacterActions();
        _cc = GetComponent<CharacterController>();
        _verticalVelocity = 0f;
    }

    void Update()
    {
        Vector2 input = ca.MoveValue;

        Vector3 moveDir = ComputeMoveDirection(input);
        bool isMoving = moveDir.sqrMagnitude > 0.01f;

        characterAnimator.SetFloat("ForwardOrBackward", isMoving ? input.magnitude * 0.5f : 0f);
        characterAnimator.SetFloat("LeftOrRight", 0f);

        if(isMoving)
        {
            RotateCharacterToward(moveDir);
        }

        ApplyGravity();
        Vector3 movement = moveDir * _moveSpeed + Vector3.up * _verticalVelocity;
        _cc.Move(movement * Time.deltaTime);
    }

    Vector3 ComputeMoveDirection(Vector2 input)
    {
        if(_camera == null) return Vector3.zero;

        Vector3 cameraForward = _camera.forward;
        Vector3 cameraRight = _camera.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDir = (cameraRight * input.x + cameraForward * input.y).normalized;
        return moveDir;
    }

    void RotateCharacterToward(Vector3 direction)
    {
        if(direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if(_cc.isGrounded)
        {
            _verticalVelocity = -0.5f;
        }
        else
        {
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        ca?.Dispose();
    }
}
