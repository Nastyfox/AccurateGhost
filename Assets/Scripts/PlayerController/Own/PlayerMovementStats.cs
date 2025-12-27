using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementStats", menuName = "ScriptableObjects/PlayerMovementStats", order = 1)]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float groundAcceleration = 5f;
    [Range(0.25f, 50f)] public float groundDeceleration = 20f;
    [Range(0.25f, 50f)] public float airAcceleration = 5f;
    [Range(0.25f, 50f)] public float airDeceleration = 5f;

    [Header("Run")]
    [Range(1f, 100f)] public float maxRunSpeed = 20f;

    [Header("Jump")]
    [Header("Jump Values")]
    [Range(2f, 10f)] public float jumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.054f;
    [Range(0.1f, 0.75f)] public float timeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float jumpCutGravityMultiplier = 2f;
    [Range(5f, 50f)] public float maxFallSpeed = 26f;
    [Range(5f, 50f)] public float maxJumpSpeed = 50f;
    [Range(1, 5)] public int numberOfJumpsAllowed = 1;
    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float timeForUpwardsCancel = 0.027f;
    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float apexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float apexHangDuration = 0.075f;
    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float jumpBufferDuration = 0.125f;
    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float jumpCoyoteDuration = 0.1f;

    [Header("Grounded/Collisions")]
    public LayerMask groundLayer;
    public float groundDetectionRayLength = 0.02f;
    public float headDetectionRayLength = 0.02f;
    [Range(0f, 1f)] public float headWidth = 0.75f;

    [Header("Debug")]
    public bool DebugGroundCollisionRays = false;
    public bool DebugHeadBumpCollisionRays = false;

    [Header ("Jump Visualization Tool")]
    public bool showJumpArc = false;
    public bool stopOnCollision = true;
    [Range(5, 100)] public int arcResolution = 20;
    [Range(0, 500)] public int visualizationSteps = 90;

    public float adjustedJumpHeight
    {
        get
        {
            return jumpHeight * jumpHeightCompensationFactor;
        }
    }

    public float gravity
    {
        get
        {
            return -(2f * adjustedJumpHeight * jumpHeightCompensationFactor) / Mathf.Pow(timeTillJumpApex, 2);
        }
    }

    public float initialJumpVelocity
    {
        get
        {
            return Mathf.Abs(gravity) * timeTillJumpApex;
        }
    }
}
