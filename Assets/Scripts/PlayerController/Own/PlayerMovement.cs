using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.UI.Image;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("References")]
    public PlayerMovementStats movementStats;
    [SerializeField] private Collider2D feetCollider;
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private float moveParticlesOffsetX;

    private Rigidbody2D playerRb;

    private float horizontalVelocity;
    private float currentMaxSpeed;

    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private RaycastHit2D wallHit;
    private RaycastHit2D lastWallHit;
    private bool isGrounded;
    private bool bumpedHead;
    private bool isTouchingWall;

    public float verticalVelocity
    {
        get { return playerRb.linearVelocity.y; }
        set { playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, value); }
    }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfJumpsUsed;

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
    #endregion

    #region Unity Methods
    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();

        currentMaxSpeed = movementStats.maxWalkSpeed;
    }

    private void Update()
    {
        LandCheck();
        JumpTimers();
        JumpCheck();
        WallSlideCheck();
        WallJumpCheck();
        DashCheck();
        MoveParticles();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        Fall();
        WallSlide();
        WallJump();
        Dash();

        ApplyVelocity();
    }
    #endregion

    #region Movement
    private void HorizontalMovement(float acceleration, float deceleration, Vector2 moveInput)
    {
        if(!isDashing)
        {
            float targetVelocity = 0f;

            if (Mathf.Abs(moveInput.x) >= movementStats.moveThreshold)
            {
                playerSpriteRenderer.flipX = moveInput.x < 0;
                float particlesOffsetX = playerSpriteRenderer.flipX ? moveParticlesOffsetX : -moveParticlesOffsetX;
                moveParticles.gameObject.transform.localPosition = new Vector3(particlesOffsetX, moveParticles.gameObject.transform.localPosition.y, moveParticles.gameObject.transform.localPosition.z);

                if (isGrounded && InputManager.runHeld)
                {
                    currentMaxSpeed = movementStats.maxRunSpeed;
                }
                else if (isGrounded && !InputManager.runHeld)
                {
                    currentMaxSpeed = movementStats.maxWalkSpeed;
                }

                targetVelocity = moveInput.x * currentMaxSpeed;
                horizontalVelocity = Mathf.Lerp(horizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                horizontalVelocity = Mathf.Lerp(horizontalVelocity, targetVelocity, deceleration * Time.fixedDeltaTime);
            }

            if (Mathf.Abs(horizontalVelocity) > movementStats.idleAnimationMaximumSpeed)
            {
                playerAnimator.SetBool("IsMoving", true);
                playerAnimator.speed = Mathf.Abs(horizontalVelocity) * movementStats.walkAnimationFactor;
            }
            else if(Mathf.Abs(moveInput.x) < movementStats.moveThreshold)
            {
                horizontalVelocity = 0f;
                playerAnimator.SetBool("IsMoving", false);
                playerAnimator.speed = 1f;
            }
        }
    }

    private void ApplyVelocity()
    {
        if (!isDashing)
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, -movementStats.maxFallSpeed, movementStats.maxJumpSpeed);
        }
        else
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, -movementStats.maxDashFallSpeed, movementStats.maxJumpSpeed);
        }

        playerRb.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);
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
        if (InputManager.jumpPressed)
        {
            if ((isWallSlideFalling && wallJumpPostBufferTimer > 0f) || (isWallSliding || (isTouchingWall && !isGrounded)))
            {
                return;
            }

            jumpBufferTimer = movementStats.jumpBufferDuration;
        }

        if (InputManager.jumpReleased)
        {
            if (jumpBufferTimer > 0f)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = verticalVelocity;
            }

            if (isJumping && verticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = false;
                    fastFallTime = movementStats.timeForUpwardsCancel;
                    verticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = verticalVelocity;
                }
            }
        }

        if (jumpBufferTimer >= 0f && !isJumping && (isGrounded || jumpCoyoteTimer > 0f))
        {
            InitiateJump(1);
        }
        else if (jumpBufferTimer > 0f && (isJumping || isWallJumping || isWallSlideFalling || isAirDashing || isDashFastFalling) && !isTouchingWall && numberOfJumpsUsed < movementStats.numberOfJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);

            isDashFastFalling = false;
        }
        else if (jumpBufferTimer > 0f && isFalling && !isWallSlideFalling && numberOfJumpsUsed < movementStats.numberOfJumpsAllowed - 1)
        {
            isFastFalling = false;
            InitiateJump(2);
        }
    }

    private void InitiateJump(int numberOfJumpsToUse)
    {
        isJumping = true;

        playerAnimator.speed = 1f;
        playerAnimator.SetTrigger("Jump");
        PlayParticles(jumpParticles);

        ResetWallJumpValues();

        numberOfJumpsUsed += numberOfJumpsToUse;
        jumpBufferTimer = 0f;
        verticalVelocity = movementStats.initialJumpVelocity;
    }

    private void Jump()
    {
        if (isJumping)
        {
            if (bumpedHead)
            {
                isFastFalling = true;
            }

            if (verticalVelocity >= 0f)
            {
                apexPoint = Mathf.InverseLerp(movementStats.initialJumpVelocity, 0f, verticalVelocity);

                if (apexPoint >= movementStats.apexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }
                    else
                    {
                        timePastApexThreshold += Time.fixedDeltaTime;
                        if (timePastApexThreshold < movementStats.apexHangDuration)
                        {
                            verticalVelocity = 0f;
                        }
                        else
                        {
                            verticalVelocity -= 0.01f;
                        }
                    }
                }
                else if(!isFastFalling)
                {
                    verticalVelocity += movementStats.gravity * Time.fixedDeltaTime;
                    isPastApexThreshold = false;
                }
            }
            else if (!isFastFalling)
            {
                verticalVelocity += movementStats.gravity * movementStats.jumpCutGravityMultiplier * Time.fixedDeltaTime;
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
                verticalVelocity += movementStats.gravity * movementStats.jumpCutGravityMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                float t = fastFallTime / movementStats.timeForUpwardsCancel;
                verticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0, t);
            }

            fastFallTime += Time.fixedDeltaTime;
        }
    }
    #endregion

    #region WallSlide

    private void WallSlideCheck()
    {
        if(isTouchingWall && !isGrounded && !isDashing)
        {
            if(verticalVelocity < 0f)
            {
                ResetJumpValues();
                ResetWallJumpValues();
                ResetDashValues();

                isWallSliding = true;
                isWallSlideFalling = false;

                if(movementStats.resetJumpsOnWallJump)
                {
                    numberOfJumpsUsed = 0;
                }
            }
        }
        else if(isWallSliding && !isTouchingWall && !isGrounded)
        {
            isWallSlideFalling = true;

            StopWallSlide();
        }
        else
        {
            StopWallSlide();
        }
    }

    private void WallSlide()
    {
        if(isWallSliding)
        {
            verticalVelocity = Mathf.Lerp(verticalVelocity, -movementStats.wallSlideSpeed, movementStats.wallSlideDeceleration * Time.fixedDeltaTime);
        }
    }

    private void StopWallSlide()
    {
        isWallSliding = false;

        if(isWallSliding)
        {
            numberOfJumpsUsed++;
        }
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

        if(InputManager.jumpReleased && !isWallSliding && !isTouchingWall && !isWallJumping)
        {
            if(verticalVelocity > 0f)
            {
                if(isPastWallJumpApexThreshold)
                {
                    isPastWallJumpApexThreshold = false;
                    isWallJumpFalling = true;
                    wallJumpFastFallTime = movementStats.timeForUpwardsCancel;

                    verticalVelocity = 0f;
                }
                else
                {
                    isWallJumpFastFalling = true;
                    wallJumpFastFallReleaseSpeed = verticalVelocity;
                }
            }
        }

        if(InputManager.jumpPressed && wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    private void InitiateWallJump()
    {
        isWallJumping = true;
        useWallJumpMoveStats = true;
        wallJumpTime = 0f;

        StopWallSlide();

        ResetJumpValues();

        verticalVelocity = movementStats.initialWallJumpVelocity;

        float wallDirection = 1f;

        Vector2 hitPoint = lastWallHit.collider.ClosestPoint(bodyCollider.bounds.center);

        if (hitPoint.x < transform.position.x)
        {
            wallDirection = -1f;
        }

        horizontalVelocity = movementStats.wallJumpDirection.x * wallDirection;
    }

    private void WallJump()
    {
        if (isWallJumping)
        {
            wallJumpTime += Time.fixedDeltaTime;

            if(wallJumpTime >= movementStats.timeTillJumpApex)
            {
                useWallJumpMoveStats = false;
            }

            if (bumpedHead)
            {
                isWallJumpFastFalling = true;
                useWallJumpMoveStats = false;
            }

            if (verticalVelocity >= 0f)
            {
                wallJumpApexPoint = Mathf.InverseLerp(movementStats.initialWallJumpVelocity, 0f, verticalVelocity);

                if (wallJumpApexPoint >= movementStats.apexThreshold)
                {
                    if (!isPastWallJumpApexThreshold)
                    {
                        isPastWallJumpApexThreshold = true;
                        timePastWallJumpApexThreshold = 0f;
                    }
                    else
                    {
                        timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                        if (timePastWallJumpApexThreshold < movementStats.apexHangDuration)
                        {
                            verticalVelocity = 0f;
                        }
                        else
                        {
                            verticalVelocity -= 0.01f;
                        }
                    }
                }
                else if (!isFastFalling)
                {
                    verticalVelocity += movementStats.wallJumpGravity * Time.fixedDeltaTime;
                    isPastWallJumpApexThreshold = false;
                }
            }
            else if (!isWallJumpFastFalling)
            {
                verticalVelocity += movementStats.wallJumpGravity * movementStats.jumpCutGravityMultiplier * Time.fixedDeltaTime;
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
                verticalVelocity += movementStats.wallJumpGravity * movementStats.jumpCutGravityMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                float t = wallJumpFastFallTime / movementStats.timeForUpwardsCancel;
                verticalVelocity = Mathf.Lerp(wallJumpFastFallReleaseSpeed, 0, t);
            }

            wallJumpFastFallTime += Time.fixedDeltaTime;
        }
    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        if(!isGrounded && (isWallSliding || isTouchingWall))
        {
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
    }

    private void DashCheck()
    {
        if (InputManager.dashPressed)
        {
            if (isGrounded && dashOnGroundTimer < 0f)
            {
                InitiateDash();
            }
            else if (!isGrounded && numberOfDashesUsed < movementStats.numberOfDashesAllowed)
            {
                isAirDashing = true;
                InitiateDash();

                if (wallJumpPostBufferTimer > 0f)
                {
                    numberOfJumpsUsed = Mathf.Max(0, numberOfJumpsUsed--);
                }
            }
        }
    }

    private void InitiateDash()
    {
        dashDirection = InputManager.movement;

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

            if (playerSpriteRenderer.flipX)
            {
                closestDirection = Vector2.left;
            }
        }

        dashDirection = closestDirection;
        numberOfDashesUsed++;
        isDashing = true;
        dashTimer = 0f;
        dashOnGroundTimer = movementStats.durationBetweenDashes;

        ResetJumpValues();
        ResetWallJumpValues();
        StopWallSlide();
    }

    private void Dash()
    {
        if (isDashing)
        {
            dashTimer += Time.fixedDeltaTime;

            if (dashTimer >= movementStats.dashDuration)
            {
                if(isGrounded)
                {
                    numberOfDashesUsed = 0;
                    isDashOver = true;
                }

                isAirDashing = false;
                isDashing = false;

                if(!isJumping && !isWallJumping)
                {
                    dashFastFallTime = 0f;
                    dashFastFallReleaseSpeed = verticalVelocity;

                    if(!isGrounded)
                    {
                        isDashFastFalling = true;
                    }
                }

                return;
            }

            horizontalVelocity = dashDirection.x * movementStats.dashSpeed;

            if(dashDirection.y != 0f)
            {
                verticalVelocity = dashDirection.y * movementStats.dashSpeed;
            }
        }

        if (isDashFastFalling)
        {
            if (dashFastFallTime >= movementStats.dashDurationForUpwardsCancel)
            {
                verticalVelocity += movementStats.gravity * movementStats.dashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                float t = dashFastFallTime / movementStats.timeForUpwardsCancel;
                verticalVelocity = Mathf.Lerp(dashFastFallReleaseSpeed, 0, t);
            }

            dashFastFallTime += Time.fixedDeltaTime;
        }
    }
    #endregion

    #region CollisionChecks

    private bool IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x, movementStats.groundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, movementStats.groundDetectionRayLength , movementStats.groundLayer);

#if UNITY_EDITOR
        if (movementStats.DebugGroundCollisionRays)
        {
            Color rayColor = groundHit.collider != null ? Color.green : Color.red;
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * (movementStats.groundDetectionRayLength), rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * (movementStats.groundDetectionRayLength), rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - movementStats.groundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.right * boxCastSize.x, rayColor);
        }
#endif

        return groundHit.collider != null;
    }

    private bool BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(bodyCollider.bounds.size.x * movementStats.headWidth, movementStats.headDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, movementStats.headDetectionRayLength, movementStats.groundLayer);

#if UNITY_EDITOR
        if (movementStats.DebugHeadBumpCollisionRays)
        {
            Color rayColor = headHit.collider != null ? Color.green : Color.red;
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * movementStats.headWidth, boxCastOrigin.y - movementStats.headDetectionRayLength), Vector2.up * (movementStats.headDetectionRayLength), rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2 * movementStats.headWidth, boxCastOrigin.y - movementStats.headDetectionRayLength), Vector2.up * (movementStats.headDetectionRayLength), rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * movementStats.headWidth, boxCastOrigin.y - movementStats.headDetectionRayLength), Vector2.right * boxCastSize.x * movementStats.headWidth, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * movementStats.headWidth, boxCastOrigin.y), Vector2.right * boxCastSize.x * movementStats.headWidth, rayColor);
        }
#endif

        return headHit.collider != null;
    }

    private bool isTouchingWallCheck()
    {
        float originX = bodyCollider.bounds.max.x;
        if (playerSpriteRenderer.flipX)
        {
            originX = bodyCollider.bounds.min.x;
        }

        float adjustedHeight = bodyCollider.bounds.size.y * movementStats.wallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originX, bodyCollider.bounds.center.y);
        Vector2 boxCastSize = new Vector2(movementStats.wallDetectionRayLength, adjustedHeight);

        wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, movementStats.wallDetectionRayLength, movementStats.groundLayer);
        lastWallHit = wallHit;

#if UNITY_EDITOR
        if (movementStats.DebugWallCollisionRays)
        {
            Color rayColor = wallHit.collider != null ? Color.green : Color.red;
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2), transform.right * movementStats.wallDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2), transform.right * movementStats.wallDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2), Vector2.up * adjustedHeight, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2), Vector2.up * adjustedHeight, rayColor);
        }
#endif

        return wallHit.collider != null;
    }

    private void CollisionChecks()
    {
        isGrounded = IsGrounded();
        bumpedHead = BumpedHead();
        isTouchingWall = isTouchingWallCheck();

        if (isDashOver)
        {
            isDashOver = false;
            HorizontalMovement(movementStats.afterDashAcceleration, movementStats.afterDashDeceleration, InputManager.movement);
            return;
        }

        if (isGrounded)
        {
            HorizontalMovement(movementStats.groundAcceleration, movementStats.groundDeceleration, InputManager.movement);
        }
        else
        {
            if(useWallJumpMoveStats)
            {
                HorizontalMovement(movementStats.wallJumpMoveAcceleration, movementStats.wallJumpMoveDeceleration, InputManager.movement);
            }
            else
            {
                HorizontalMovement(movementStats.airAcceleration, movementStats.airDeceleration, InputManager.movement);
            }
        }
    }
    #endregion

    #region Land/Fall
    private void Fall()
    {
        if (!isGrounded && !isJumping &&!isWallSliding && !isWallJumping && !isDashing && !isDashFastFalling)
        {
            isFalling = true;

            verticalVelocity += movementStats.gravity * Time.fixedDeltaTime;
        }
    }

    private void LandCheck()
    {
        if ((isFalling || isJumping || isWallJumping || isWallJumpFalling || isWallSliding || isWallSlideFalling || isDashFastFalling) && isGrounded && verticalVelocity <= 0.01f)
        {
            ResetJumpValues();
            ResetWallJumpValues();
            ResetDashValues();
            StopWallSlide();

            numberOfJumpsUsed = 0;

            verticalVelocity = Physics2D.gravity.y;

            if(isDashFastFalling && isGrounded)
            {
                return;
            }

            playerAnimator.speed = 1f;
            playerAnimator.SetTrigger("Land");
            PlayParticles(landParticles);
        }
    }
    #endregion

    #region Timers
    private void JumpTimers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if (isGrounded)
        {
            jumpCoyoteTimer = movementStats.jumpCoyoteDuration;
        }
        else
        {
            jumpCoyoteTimer -= Time.deltaTime;
        }

        if (!ShouldApplyPostWallJumpBuffer())
        {
            wallJumpPostBufferTimer -= Time.deltaTime;
        }

        if (isGrounded)
        {
            dashOnGroundTimer -= Time.deltaTime;
        }
    }
    #endregion

    #region Particles
    private void PlayParticles(ParticleSystem particleSystem)
    {
        particleSystem.gameObject.SetActive(true);
        particleSystem.Play();
    }

    private void StopParticles(ParticleSystem particleSystem)
    {
        particleSystem.Stop();
        particleSystem.gameObject.SetActive(false);
    }

    private void MoveParticles()
    {
        if (isGrounded && Mathf.Abs(horizontalVelocity) >= movementStats.idleAnimationMaximumSpeed)
        {
            Vector3 moveParticlesPosition = moveParticles.gameObject.transform.localPosition;

            if (!moveParticles.isPlaying)
            {
                PlayParticles(moveParticles);
            }
        }
        else
        {
            StopParticles(moveParticles);
        }
    }
    #endregion

    #region Visualization

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
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
}
