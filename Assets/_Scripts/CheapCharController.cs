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
    [SerializeField] JumpSettings _jumpSettings;

    CharacterActions ca;
    CharacterController _cc;
    float _verticalVelocity;

    float _coyoteTimeCounter;
    float _jumpBufferCounter;
    float _varJumpTimer;
    float _jumpPeakVelocity;
    float _landingBoostTimer;
    bool _wasGroundedLastFrame;

    void Awake() {
        ca = new CharacterActions();
        _cc = GetComponent<CharacterController>();
        _verticalVelocity = 0f;
    }

    void Update()
    {
        if (ca == null) return;
        Vector2 input = ca.MoveValue;

        Vector3 moveDir = ComputeMoveDirection(input);
        bool isMoving = moveDir.sqrMagnitude > 0.01f;

        if (characterAnimator != null)
        {
            characterAnimator.SetFloat("ForwardOrBackward", isMoving ? input.magnitude * 0.5f : 0f);
            characterAnimator.SetFloat("LeftOrRight", 0f);
        }

        if(isMoving)
        {
            RotateCharacterToward(moveDir);
        }

        ApplyGravity();
        HandleJump();

        float airControlFactor = _jumpSettings?.enableAirControl == true ? _jumpSettings.airControlFactor : 1f;

        // Descent momentum in second half of jump
        if (_jumpSettings?.enableDescentMomentum == true && !_cc.isGrounded && _verticalVelocity < 0f)
        {
            float descentRatio = Mathf.Abs(_verticalVelocity) / Mathf.Abs(_jumpSettings.gravity);
            if (descentRatio > Mathf.Abs(_jumpPeakVelocity) / Mathf.Abs(_jumpSettings.gravity) * 0.5f)
            {
                airControlFactor *= _jumpSettings.descentMomentumMultiplier;
            }
        }

        float baseSpeed = _moveSpeed;

        // Landing speed boost (doesn't stack)
        if (_landingBoostTimer > 0f)
        {
            baseSpeed *= _jumpSettings.landingSpeedBoostMultiplier;
        }

        float hSpeed = _cc.isGrounded ? baseSpeed : baseSpeed * airControlFactor;
        Vector3 movement = moveDir * hSpeed + Vector3.up * _verticalVelocity;
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

    void HandleJump()
    {
        if (_jumpSettings == null) return;

        if (_jumpSettings.enableJumpBuffer)
        {
            if (ca.JumpPressed) _jumpBufferCounter = _jumpSettings.jumpBufferTime;
            else                _jumpBufferCounter -= Time.deltaTime;
        }
        else
        {
            _jumpBufferCounter = ca.JumpPressed ? 1f : 0f;
        }

        float coyoteGate = _jumpSettings.enableCoyoteTime ? _coyoteTimeCounter : (_cc.isGrounded ? 1f : 0f);

        if (_jumpBufferCounter > 0f && coyoteGate > 0f)
        {
            if (_jumpSettings.logJumpEvents) Debug.Log("JUMP EXECUTED");
            _verticalVelocity  = _jumpSettings.JumpVelocity;
            _jumpPeakVelocity  = _jumpSettings.JumpVelocity;
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
            if (_jumpSettings.enableVariableJump)
                _varJumpTimer = _jumpSettings.varJumpTime;
        }

        if (_jumpSettings.enableVariableJump && _varJumpTimer > 0f)
        {
            _varJumpTimer -= Time.deltaTime;
            if (ca.JumpReleased && _verticalVelocity > _jumpSettings.MinJumpVelocity)
            {
                _verticalVelocity = _jumpSettings.MinJumpVelocity;
                _varJumpTimer     = 0f;
            }
        }
    }

    void ApplyGravity()
    {
        if (_jumpSettings == null) return;

        bool isGroundedNow = _cc.isGrounded;

        if (isGroundedNow)
        {
            if (!_wasGroundedLastFrame && _jumpSettings.enableLandingSpeedBoost)
            {
                _landingBoostTimer = _jumpSettings.landingSpeedBoostDuration;
            }

            _verticalVelocity  = -2f;
            _coyoteTimeCounter = _jumpSettings.enableCoyoteTime ? _jumpSettings.coyoteTime : 0f;
            _wasGroundedLastFrame = true;
            return;
        }

        _wasGroundedLastFrame = false;
        _landingBoostTimer -= Time.deltaTime;
        _coyoteTimeCounter -= Time.deltaTime;

        float gravMult = 1f;
        if (_verticalVelocity < 0f)
        {
            gravMult = _jumpSettings.enableFallMultiplier ? _jumpSettings.fallMultiplier : 1f;
        }
        else if (_verticalVelocity > 0f)
        {
            gravMult = _jumpSettings.enableAscentModifier ? _jumpSettings.ascentGravityMult : 1f;
            if (_jumpSettings.enableApexModifier && gravMult == 1f)
            {
                float apexBlend = 1f - Mathf.Clamp01(Mathf.Abs(_verticalVelocity) / _jumpSettings.apexThreshold);
                gravMult = Mathf.Lerp(1f, _jumpSettings.apexGravityMult, apexBlend);
            }
        }

        _verticalVelocity = Mathf.Max(
            _verticalVelocity + _jumpSettings.gravity * gravMult * Time.deltaTime,
            _jumpSettings.terminalVelocity
        );
    }

    private void OnDestroy()
    {
        ca?.Dispose();
    }
}
