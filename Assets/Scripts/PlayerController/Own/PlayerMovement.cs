using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("References")]
    public PlayerMovementStats movementStats;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private PlayerAnimations playerAnimation;

    public bool IsFacingRight {  get; private set; }

    [SerializeField] private Rigidbody2D playerRb;
    [HideInInspector] public Vector2 velocity;
    [SerializeField] private MovementController movementController;

    private Vector2 moveInput;
    private bool runHeld;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool dashPressed;

    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfAirJumpsUsed;

    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    private float jumpBufferTimer;

    private float jumpCoyoteTimer;

    private bool isWallSliding;
    private bool isWallSlideFalling;

    private bool useWallJumpMoveStats;
    private bool isWallJumping;
    private float wallJumpTime;
    private bool isWallJumpFastFalling;
    private bool isWallJumpFalling;
    private float wallJumpFastFallTime;
    private float wallJumpFastFallReleaseSpeed;
    private int lastWallDirection;

    private float wallJumpPostBufferTimer;

    private float wallJumpApexPoint;
    private float timePastWallJumpApexThreshold;
    private bool isPastWallJumpApexThreshold;

    private bool isDashing;
    private bool isDashOver;
    private bool isAirDashing;
    private float dashTimer;
    private float dashOnGroundTimer;
    private int numberOfDashesUsed;
    private Vector2 dashDirection;
    private bool isDashFastFalling;
    private float dashFastFallTime;
    private float dashFastFallReleaseSpeed;
    private float dashBufferTimer;
    #endregion

    #region Unity Methods

    private void Update()
    {
        moveInput = InputManager.movement;
        runHeld = InputManager.runHeld;
        jumpPressed = InputManager.jumpPressed;
        jumpReleased = InputManager.jumpReleased;
        dashPressed = InputManager.dashPressed;
    }

    private void FixedUpdate()
    {
        CountTimers(Time.fixedDeltaTime);

        CheckFacing();

        LandCheck();
        JumpCheck();
        WallJumpCheck();
        WallSlideCheck();
        DashCheck();

        HorizontalMovement(Time.fixedDeltaTime);
        Jump(Time.fixedDeltaTime);
        WallSlide(Time.fixedDeltaTime);
        WallJump(Time.fixedDeltaTime);
        Dash(Time.fixedDeltaTime);
        Fall(Time.fixedDeltaTime);

        ClampVelocity();
        movementController.Move(velocity * Time.fixedDeltaTime);

        jumpPressed = false;
        jumpReleased = false;
        dashPressed = false;
    }
    #endregion

    #region Movement
    private void HorizontalMovement(float timeStep)
    {
        if(!isDashing)
        {
            float targetVelocityX = 0f;

            float acceleration = movementController.IsGrounded() ? movementStats.groundAcceleration : movementStats.airAcceleration;
            float deceleration = movementController.IsGrounded() ? movementStats.groundDeceleration : movementStats.airDeceleration;

            if (useWallJumpMoveStats)
            {
                acceleration = movementStats.wallJumpMoveAcceleration;
                deceleration = movementStats.wallJumpMoveDeceleration;
            }

            if (Mathf.Abs(moveInput.x) >= movementStats.moveThreshold && Mathf.Sign(moveInput.x) != movementController.GetWallDirection())
            {
                playerSpriteRenderer.flipX = moveInput.x < 0;

                float moveDirection = Mathf.Sign(moveInput.x);
                targetVelocityX = runHeld ? moveDirection * movementStats.maxRunSpeed : moveDirection * movementStats.maxWalkSpeed;

                velocity.x = Mathf.Lerp(velocity.x, targetVelocityX, acceleration * timeStep);

                ParticlesManager.particlesManagerInstance.OffsetParticles(!IsFacingRight ? 1f : -1f);
                ParticlesManager.particlesManagerInstance.ChangeSpeedParticles(Mathf.Abs(velocity.x), ParticleType.Speed);
            }
            else
            {
                velocity.x = Mathf.Lerp(velocity.x, 0, deceleration * timeStep);
            }

            if (Mathf.Abs(velocity.x) >= movementStats.idleAnimationMaximumSpeed && movementController.IsGrounded())
            {
                playerAnimation.PlayAnimation(AnimationType.Move, Mathf.Abs(velocity.x) * movementStats.walkAnimationFactor);
                ParticlesManager.particlesManagerInstance.PlayParticles(ParticleType.Move);
                ParticlesManager.particlesManagerInstance.PlayParticles(ParticleType.Speed);
                AudioManager.audioManagerInstance.PlayWalkSFX(Mathf.Abs(velocity.x));
            }
            else
            {
                playerAnimation.StopAnimation(AnimationType.Move);
                ParticlesManager.particlesManagerInstance.StopParticles(ParticleType.Move);
                ParticlesManager.particlesManagerInstance.StopParticles(ParticleType.Speed);
                AudioManager.audioManagerInstance.StopWalkSFX();
            }

            if(!isWallSliding)
            {
                CameraManager.cameraManagerInstance.SetCameraOffset(velocity.x, 0);
            }
        }
    }

    private void ClampVelocity()
    {
        if (!isDashing)
        {
            velocity.y = Mathf.Clamp(velocity.y, -movementStats.maxFallSpeed, movementStats.maxJumpSpeed);
        }
        else
        {
            velocity.y = Mathf.Clamp(velocity.y, -movementStats.maxDashFallSpeed, movementStats.maxJumpSpeed);
        }
    }
    #endregion

    #region Jump

    private void ResetJumpValues()
    {
        isJumping = false;

        isFalling = false;
        isFastFalling = false;
        fastFallTime = 0f;

        isPastApexThreshold = false;
        //timePastApexThreshold = 0f;
    }

    private void JumpCheck()
    {
        if (jumpPressed)
        {
            if ((isWallSlideFalling && wallJumpPostBufferTimer > 0f) || (isWallSliding || (movementController.IsTouchingWall(IsFacingRight) && !movementController.IsGrounded())))
            {
                return;
            }

            jumpBufferTimer = movementStats.jumpBufferDuration;
        }

        if (jumpReleased)
        {
            if (jumpBufferTimer > 0f)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = velocity.y;
            }

            if (isJumping && velocity.y > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = false;
                    fastFallTime = movementStats.timeForUpwardsCancel;
                    velocity.y = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = velocity.y;
                }
            }
        }

        if (jumpBufferTimer >= 0f && !isJumping && (movementController.IsGrounded() || jumpCoyoteTimer > 0f))
        {
            InitiateJump(0);
        }
        else if (jumpBufferTimer > 0f && (isJumping || isWallJumping || isWallSlideFalling || isAirDashing || isDashFastFalling) && !movementController.IsTouchingWall(IsFacingRight) && numberOfAirJumpsUsed < movementStats.numberOfAirJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);

            isDashFastFalling = false;
        }
        else if (jumpBufferTimer > 0f && isFalling && !isWallSlideFalling && numberOfAirJumpsUsed < movementStats.numberOfAirJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }
    }

    private void InitiateJump(int numberOfAirJumpsToUse)
    {
        isJumping = true;

        playerAnimation.PlayAnimation(AnimationType.Jump, 1f);
        ParticlesManager.particlesManagerInstance.PlayParticles(ParticleType.Jump);
        AudioManager.audioManagerInstance.PlayJumpSFX();

        ResetWallJumpValues();

        numberOfAirJumpsUsed += numberOfAirJumpsToUse;
        jumpBufferTimer = 0f;
        velocity.y = movementStats.initialJumpVelocity;
    }

    private void Jump(float timeStep)
    {
        if (isJumping)
        {
            if (movementController.BumpedHead())
            {
                isFastFalling = true;
            }

            if (velocity.y >= 0f)
            {
                apexPoint = Mathf.InverseLerp(movementStats.initialJumpVelocity, 0f, velocity.y);

                if (apexPoint >= movementStats.apexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }
                    else
                    {
                        timePastApexThreshold += timeStep;
                        if (timePastApexThreshold < movementStats.apexHangDuration)
                        {
                            velocity.y = 0f;
                        }
                        else
                        {
                            velocity.y -= 0.01f;
                        }
                    }
                }
                else if(!isFastFalling)
                {
                    velocity.y += movementStats.gravity * timeStep;
                    isPastApexThreshold = false;
                }
            }
            else if (!isFastFalling)
            {
                velocity.y += movementStats.gravity * movementStats.jumpCutGravityMultiplier * timeStep;
            }
            else
            {
                isFalling = true;
            }
        }

        if (isFastFalling)
        {
            if (fastFallTime >= movementStats.timeForUpwardsCancel)
            {
                velocity.y += movementStats.gravity * movementStats.jumpCutGravityMultiplier * timeStep;
            }
            else
            {
                float t = fastFallTime / movementStats.timeForUpwardsCancel;
                velocity.y = Mathf.Lerp(fastFallReleaseSpeed, 0, t);
            }

            fastFallTime += timeStep;
        }
    }
    #endregion

    #region WallSlide

    private void WallSlideCheck()
    {
        if(movementController.IsTouchingWall(IsFacingRight) && !movementController.IsGrounded() && !isDashing)
        {
            if(velocity.y < 0f)
            {
                ResetJumpValues();
                ResetWallJumpValues();
                ResetDashValues();

                isWallSliding = true;
                isWallSlideFalling = false;

                playerAnimation.PlayAnimation(AnimationType.Wall, 1f);
                ParticlesManager.particlesManagerInstance.PlayParticles(ParticleType.WallSlide);
                AudioManager.audioManagerInstance.PlayWallSlideSFX();
                CameraManager.cameraManagerInstance.SetCameraOffset(0, movementController.GetWallDirection());

                if (movementStats.resetJumpsOnWallJump)
                {
                    numberOfAirJumpsUsed = 0;
                }
            }
        }
        else if(isWallSliding && !movementController.IsTouchingWall(IsFacingRight) && !movementController.IsGrounded())
        {
            isWallSlideFalling = true;
            StopWallSlide();
        }
        else if(isWallSliding)
        {
            StopWallSlide();
        }
    }

    private void WallSlide(float timeStep)
    {
        if(isWallSliding)
        {
            velocity.y = Mathf.Lerp(velocity.y, -movementStats.wallSlideSpeed, movementStats.wallSlideDeceleration * timeStep);
        }
    }

    private void StopWallSlide()
    {
        isWallSliding = false;

        ParticlesManager.particlesManagerInstance.StopParticles(ParticleType.WallSlide);
        AudioManager.audioManagerInstance.StopWallSlideSFX();
    }

    #endregion

    #region WallJump

    private void ResetWallJumpValues()
    {
        isWallJumping = false;
        useWallJumpMoveStats = false;
        wallJumpTime = 0f;

        isWallJumpFalling = false;
        isWallJumpFastFalling = false;
        wallJumpFastFallTime = 0f;

        isWallSlideFalling = false;

        isPastWallJumpApexThreshold = false;
        //timePastWallJumpApexThreshold = 0f;
    }

    private void WallJumpCheck()
    {
        if(ShouldApplyPostWallJumpBuffer())
        {
            wallJumpPostBufferTimer = movementStats.wallJumpPostBufferDuration;
        }

        if(jumpReleased && !isWallSliding && !movementController.IsTouchingWall(IsFacingRight) && !isWallJumping)
        {
            if(velocity.y > 0f)
            {
                if(isPastWallJumpApexThreshold)
                {
                    isPastWallJumpApexThreshold = false;
                    isWallJumpFalling = true;
                    wallJumpFastFallTime = movementStats.timeForUpwardsCancel;

                    velocity.y = 0f;
                }
                else
                {
                    isWallJumpFastFalling = true;
                    wallJumpFastFallReleaseSpeed = velocity.y;
                }
            }
        }

        if(jumpPressed && wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    private void InitiateWallJump()
    {
        isWallJumping = true;
        useWallJumpMoveStats = true;
        wallJumpTime = 0f;

        playerAnimation.PlayAnimation(AnimationType.Jump, 1f);
        ParticlesManager.particlesManagerInstance.PlayParticles(ParticleType.WallJump);
        AudioManager.audioManagerInstance.PlayJumpSFX();

        StopWallSlide();

        ResetJumpValues();

        velocity.y = movementStats.initialWallJumpVelocity;
        velocity.x = Mathf.Abs(movementStats.wallJumpDirection.x) * -lastWallDirection;
    }

    private void WallJump(float timeStep)
    {
        if (isWallJumping)
        {
            wallJumpTime += timeStep;

            if(wallJumpTime >= movementStats.timeTillJumpApex)
            {
                useWallJumpMoveStats = false;
            }

            if (movementController.BumpedHead())
            {
                isWallJumpFastFalling = true;
                useWallJumpMoveStats = false;
            }

            if (velocity.y >= 0f)
            {
                wallJumpApexPoint = Mathf.InverseLerp(movementStats.initialWallJumpVelocity, 0f, velocity.y);

                if (wallJumpApexPoint >= movementStats.apexThreshold)
                {
                    if (!isPastWallJumpApexThreshold)
                    {
                        isPastWallJumpApexThreshold = true;
                        timePastWallJumpApexThreshold = 0f;
                    }
                    else
                    {
                        timePastWallJumpApexThreshold += timeStep;
                        if (timePastWallJumpApexThreshold < movementStats.apexHangDuration)
                        {
                            velocity.y = 0f;
                        }
                        else
                        {
                            velocity.y -= 0.01f;
                        }
                    }
                }
                else if (!isFastFalling)
                {
                    velocity.y += movementStats.wallJumpGravity * timeStep;
                    isPastWallJumpApexThreshold = false;
                }
            }
            else if (!isWallJumpFastFalling)
            {
                velocity.y += movementStats.wallJumpGravity * movementStats.jumpCutGravityMultiplier * timeStep;
            }
            else
            {
                isWallJumpFalling = true;
            }
        }

        if (isWallJumpFastFalling)
        {
            if (wallJumpFastFallTime >= movementStats.timeForUpwardsCancel)
            {
                velocity.y += movementStats.wallJumpGravity * movementStats.jumpCutGravityMultiplier * timeStep;
            }
            else
            {
                float t = wallJumpFastFallTime / movementStats.timeForUpwardsCancel;
                velocity.y = Mathf.Lerp(wallJumpFastFallReleaseSpeed, 0, t);
            }

            wallJumpFastFallTime += timeStep;
        }
    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        if(isWallSliding || movementController.IsTouchingWall(IsFacingRight))
        {
            lastWallDirection = movementController.GetWallDirection();
            return true;
        }
        return false;
    }
    #endregion

    #region Dash

    private void ResetDashValues()
    {
        isDashing = false;
        isAirDashing = false;
        dashTimer = 0f;
        dashOnGroundTimer = -0.01f;

        if(movementStats.resetDashesOnWallJump)
        {
            numberOfDashesUsed = 0;
        }

        isDashFastFalling = false;
        dashFastFallTime = 0f;
        dashFastFallReleaseSpeed = 0f;
        dashDirection = Vector2.zero;
    }

    private void DashCheck()
    {
        if(dashPressed)
        {
            dashBufferTimer = movementStats.dashBufferDuration;
        }

        if (dashBufferTimer > 0f)
        {
            if (movementController.IsGrounded() && dashOnGroundTimer < 0f)
            {
                InitiateDash();
            }
            else if (!movementController.IsGrounded() && numberOfDashesUsed < movementStats.numberOfDashesAllowed)
            {
                isAirDashing = true;
                InitiateDash();
            }
        }
    }

    private void InitiateDash()
    {
        dashDirection = moveInput;
        CheckFacing();

        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(dashDirection.normalized, movementStats.dashDirections[0].normalized);

        for(int i = 0; i < movementStats.dashDirections.Length; i++)
        {
            if(dashDirection == movementStats.dashDirections[i])
            {
                closestDirection = movementStats.dashDirections[i];
                break;
            }

            float distance = Vector2.Distance(dashDirection, movementStats.dashDirections[i]);

            if (Mathf.Abs(movementStats.dashDirections[i].x) > 0f && Mathf.Abs(movementStats.dashDirections[i].y) > 0f)
            {
                distance -= movementStats.dashDiagonallyBias;
            }


            if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = movementStats.dashDirections[i];
            }
        }

        if (closestDirection == Vector2.zero)
        {
            closestDirection = Vector2.right;

            if (!IsFacingRight)
            {
                closestDirection = Vector2.left;
            }
        }

        if(movementController.IsGrounded() && closestDirection.y < 0f && closestDirection.x != 0f)
        {
            closestDirection = new Vector2(Mathf.Sign(closestDirection.x), 0);
        }

        dashDirection = closestDirection;
        numberOfDashesUsed++;
        isDashing = true;
        dashTimer = 0f;
        dashBufferTimer = 0f;
        dashOnGroundTimer = movementStats.durationBetweenDashes;

        ParticlesManager.particlesManagerInstance.PlayParticles(ParticleType.Dash);

        ResetJumpValues();
        ResetWallJumpValues();
        StopWallSlide();
    }

    private void Dash(float timeStep)
    {
        if (isDashing)
        {
            dashTimer += timeStep;

            if (dashTimer >= movementStats.dashDuration)
            {
                if(movementController.IsGrounded())
                {
                    numberOfDashesUsed = 0;
                    isDashOver = true;
                }

                isAirDashing = false;
                isDashing = false;

                if(!isJumping && !isWallJumping)
                {
                    dashFastFallTime = 0f;
                    dashFastFallReleaseSpeed = velocity.y;

                    if(!movementController.IsGrounded())
                    {
                        isDashFastFalling = true;
                    }
                }

                ParticlesManager.particlesManagerInstance.StopParticles(ParticleType.Dash);

                return;
            }

            velocity.x = dashDirection.x * movementStats.dashSpeed;

            if(dashDirection.y != 0f || isAirDashing)
            {
                velocity.y = dashDirection.y * movementStats.dashSpeed;
            }
            else if(!isJumping && dashDirection.y == 0f)
            {
                velocity.y = -0.001f;
            }
        }

        if (isDashFastFalling)
        {
            if (dashFastFallTime >= movementStats.dashDurationForUpwardsCancel)
            {
                velocity.y += movementStats.gravity * movementStats.dashGravityOnReleaseMultiplier * timeStep;
            }
            else
            {
                float t = dashFastFallTime / movementStats.timeForUpwardsCancel;
                velocity.y = Mathf.Lerp(dashFastFallReleaseSpeed, 0, t);
            }

            dashFastFallTime += timeStep;
        }
    }
    #endregion

    #region Land/Fall
    private void Fall(float timeStep)
    {
        if (!isJumping && !isWallSliding && !isWallJumping && !isDashing && !isDashFastFalling && !movementController.IsGrounded())
        {
            isFalling = true;

            velocity.y += movementStats.gravity * timeStep;
        }
    }

    private void LandCheck()
    {
        //if((isFalling || isJumping || isWallJumping || isWallJumpFalling || isWallSliding || isWallSlideFalling || isDashFastFalling || isDashing || isDashOver))
        //{
        //    Debug.Log(" isFalling " + isFalling + " isJumping " + isJumping + " isWallJumping " + isWallJumping + " isWallJumpFalling " + isWallJumpFalling + " isWallSliding " + isWallSliding + " isWallSlideFalling " + isWallSlideFalling + " isDashFastFalling " + isDashFastFalling + " isDashing " + isDashing + " isDashOver " + isDashOver);
        //    Debug.Log("Grounded " + movementController.IsGrounded() + " velocity " + velocity.y);
        //}

        if(movementController.IsGrounded())
        {
            if ((isFalling || isJumping || isWallJumping || isWallJumpFalling || isWallSliding || isWallSlideFalling || isDashFastFalling || isDashing || isDashOver) && velocity.y <= 0.01f)
            {
                ResetJumpValues();
                ResetWallJumpValues();
                ResetDashValues();
                StopWallSlide();

                numberOfAirJumpsUsed = 0;

                //if (isDashFastFalling && isGrounded)
                //{
                //    return;
                //}

                playerAnimation.PlayAnimation(AnimationType.Land, 1f);
                ParticlesManager.particlesManagerInstance.PlayParticles(ParticleType.Land);
                AudioManager.audioManagerInstance.PlayLandSFX();
            }

            if(velocity.y <= 0f)
            {
                velocity.y = movementStats.groundedGravity;
            }
        }
    }
    #endregion

    #region Timers
    private void CountTimers(float timeStep)
    {
        jumpBufferTimer -= timeStep;

        if (movementController.IsGrounded())
        {
            jumpCoyoteTimer = movementStats.jumpCoyoteDuration;
        }
        else
        {
            jumpCoyoteTimer -= timeStep;
        }

        wallJumpPostBufferTimer -= timeStep;

        if (movementController.IsGrounded())
        {
            dashOnGroundTimer -= timeStep;
        }

        dashBufferTimer -= timeStep;
    }
    #endregion

    #region Visualization

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = 0f;

        Vector2 velocity = new Vector2(speed, movementStats.initialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * movementStats.timeTillJumpApex / movementStats.arcResolution;

        for (int i = 0; i < movementStats.visualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if(simulationTime < movementStats.timeTillJumpApex)
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0f, movementStats.gravity) * Mathf.Pow(simulationTime, 2);
            }
            else if(simulationTime < (movementStats.timeTillJumpApex + movementStats.apexHangDuration))
            {
                float apexTime = simulationTime - movementStats.timeTillJumpApex;
                displacement = velocity * movementStats.timeTillJumpApex + 0.5f * new Vector2(0f, movementStats.gravity) * Mathf.Pow(movementStats.timeTillJumpApex, 2);
                displacement += new Vector2(speed, 0f) * apexTime;
            }
            else
            {
                float descendTime = simulationTime - (movementStats.timeTillJumpApex + movementStats.apexHangDuration);
                displacement = velocity * movementStats.timeTillJumpApex + 0.5f * new Vector2(0f, movementStats.gravity) * Mathf.Pow(movementStats.timeTillJumpApex, 2);
                displacement += new Vector2(speed, 0f) * movementStats.apexHangDuration;
                displacement += new Vector2(speed, 0f) * descendTime + 0.5f * new Vector2(0f, movementStats.gravity) * Mathf.Pow(descendTime, 2);
            }

            drawPoint = startPosition + displacement;

            if(movementStats.stopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Linecast(previousPosition, drawPoint, movementStats.groundLayer);
                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (movementStats.showJumpArc && Application.isPlaying)
        {
            DrawJumpArc(playerRb.linearVelocity.x, Color.cyan);
        }
#endif
    }
    #endregion

    #region Helper Methods
    private void CheckFacing()
    {
        IsFacingRight = !playerSpriteRenderer.flipX;
    }
    #endregion
}
