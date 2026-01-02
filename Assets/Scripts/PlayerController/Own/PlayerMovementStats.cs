using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementStats", menuName = "ScriptableObjects/PlayerMovementStats", order = 1)]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(0f, 1f)] public float moveThreshold = 0.1f;
    [Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float groundAcceleration = 5f;
    [Range(0.25f, 50f)] public float groundDeceleration = 20f;
    [Range(0.25f, 50f)] public float airAcceleration = 5f;
    [Range(0.25f, 50f)] public float airDeceleration = 5f;
    [Range(0.25f, 50f)] public float wallJumpMoveAcceleration = 5f;
    [Range(0.25f, 50f)] public float wallJumpMoveDeceleration = 5f;

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
    [Header("Wall Jump")]
    public bool resetJumpsOnWallJump = true;

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

    [Header("Wall")]
    [Header("Wall Jump")]
    public Vector2 wallJumpDirection = new Vector2(-20f, 6.5f);
    [Range(0f, 1f)] public float wallJumpPostBufferDuration = 0.125f;
    [Range(0.01f, 5f)] public float wallJumpGravityOnReleaseMultiplier = 1f;
    [Header("Wall Slide")]
    [Range(0.01f, 10f)] public float wallSlideSpeed = 5f;
    [Range(0.25f, 50f)] public float wallSlideDeceleration = 30f;

    [Header("Dash")]
    [Header("Dash Values")]
    [Range(0f, 1f)] public float dashDuration = 0.15f;
    [Range(1f, 100f)] public float dashSpeed = 50f;
    [Range(1f, 100f)] public float afterDashAcceleration = 50f;
    [Range(1f, 100f)] public float afterDashDeceleration = 50f;
    [Range(5f, 50f)] public float maxDashFallSpeed = 50f;
    [Range(0f, 3f)] public float durationBetweenDashes = 0.5f;
    [Range(1, 5)] public int numberOfDashesAllowed = 1;
    [Range(0f, 1f)] public float dashDiagonallyBias = 0.4f;
    public bool resetDashesOnWallJump = true;
    [Header("Dash Cancel")]
    [Range(0.01f, 0.5f)] public float dashDurationForUpwardsCancel = 0.03f;
    [Range(0.01f, 5f)] public float dashGravityOnReleaseMultiplier = 1f;

    public readonly Vector2[] dashDirections = new Vector2[]
    {
        new Vector2(0, 0),    // Nothing
        new Vector2(0, 1),    // Up
        new Vector2(1, 1).normalized,    // Up-Right
        new Vector2(1, 0),    // Right
        new Vector2(1, -1).normalized,   // Down-Right
        new Vector2(0, -1),   // Down
        new Vector2(-1, -1).normalized,  // Down-Left
        new Vector2(-1, 0),   // Left
        new Vector2(-1, 1).normalized    // Up-Left
    };

    public float adjustedWallJumpHeight
    {
        get
        {
            return wallJumpDirection.y * jumpHeightCompensationFactor;
        }
    }

    public float wallJumpGravity
    {
        get
        {
            return -(2f * adjustedWallJumpHeight * jumpHeightCompensationFactor) / Mathf.Pow(timeTillJumpApex, 2);
        }
    }

    public float initialWallJumpVelocity
    {
        get
        {
            return Mathf.Abs(wallJumpGravity) * timeTillJumpApex;
        }
    }

    [Header("Grounded/Collisions")]
    public LayerMask groundLayer;
    [Range(0.01f, 0.5f)] public float groundDetectionRayLength = 0.02f;
    [Range(0.01f, 0.5f)] public float headDetectionRayLength = 0.02f;
    [Range(0.01f, 0.5f)] public float wallDetectionRayLength = 0.125f;
    [Range(0.01f, 1f)] public float wallDetectionRayHeightMultiplier = 0.9f;
    [Range(0f, 1f)] public float headWidth = 0.75f;

    [Header("Debug")]
    public bool DebugGroundCollisionRays = false;
    public bool DebugHeadBumpCollisionRays = false;
    public bool DebugWallCollisionRays = false;

    [Header ("Jump Visualization Tool")]
    public bool showJumpArc = false;
    public bool stopOnCollision = true;
    [Range(5, 100)] public int arcResolution = 20;
    [Range(0, 500)] public int visualizationSteps = 90;
}
