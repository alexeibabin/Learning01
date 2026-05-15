using UnityEngine;

[CreateAssetMenu(fileName = "JumpSettings", menuName = "Settings/Jump")]
public class JumpSettings : ScriptableObject
{
    [Header("Core Physics")]
    public float gravity         = -27f;
    public float maxJumpHeight   = 2.5f;
    public float terminalVelocity = -30f;

    [Header("Variable Jump Height")]
    public bool  enableVariableJump = true;
    public float minJumpHeight      = 1.0f;
    public float varJumpTime        = 0.2f;

    [Header("Ascent Feel")]
    public bool  enableAscentModifier = true;
    [Range(0f, 1f)]
    public float ascentGravityMult    = 0.7f;

    [Header("Fall Feel")]
    public bool  enableFallMultiplier = true;
    public float fallMultiplier       = 1.8f;

    [Header("Apex Modifier")]
    public bool  enableApexModifier  = true;
    public float apexThreshold       = 25f;
    [Range(0f, 1f)]
    public float apexGravityMult     = 0.35f;

    [Header("Coyote Time")]
    public bool  enableCoyoteTime = true;
    public float coyoteTime       = 0.20f;

    [Header("Jump Buffer")]
    public bool  enableJumpBuffer  = true;
    public float jumpBufferTime    = 0.18f;

    [Header("Air Movement")]
    public bool  enableAirControl   = true;
    [Range(0f, 1f)]
    public float airControlFactor   = 0.85f;

    [Header("Descent Momentum")]
    public bool  enableDescentMomentum = true;
    [Range(1f, 2f)]
    public float descentMomentumMultiplier = 1.3f;

    [Header("Landing Speed Boost")]
    public bool  enableLandingSpeedBoost = true;
    [Range(1f, 2f)]
    public float landingSpeedBoostMultiplier = 1.15f;
    public float landingSpeedBoostDuration = 0.5f;

    [Header("Debug")]
    public bool logJumpEvents = false;

    public float JumpVelocity    => Mathf.Sqrt(-2f * gravity * maxJumpHeight);
    public float MinJumpVelocity => Mathf.Sqrt(-2f * gravity * minJumpHeight);
}
