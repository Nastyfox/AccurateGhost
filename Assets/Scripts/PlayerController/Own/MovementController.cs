using UnityEngine;

public class MovementController : MonoBehaviour
{
    public struct RaycastCorners
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    public const float collisionPadding = 0.015f;

    [Range(2, 100)] public int numOfHorizontalRays = 4;
    [Range(2, 100)] public int numOfVerticalRays = 4;

    private float horizontalRaySpace;
    private float verticalRaySpace;

    public RaycastCorners raycastCorners;

    [SerializeField] private PlayerMovementStats movementStats;

    public bool IsCollidingAbove {  get; private set; }
    public bool IsCollidingBelow {  get; private set; }
    public bool IsCollidingLeft {  get; private set; }
    public bool IsCollidingRight {  get; private set; }

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private BoxCollider2D playerCollider;

    private void Start()
    {
        CalculateRaySpacing();
    }

    public void Move(Vector2 velocity)
    {
        UpdateRaycastCorners();
        ResetCollisionsStates();

        ResolveHorizontalMovement(ref velocity);
        ResolveVerticalMovement(ref velocity);

        playerRb.MovePosition(playerRb.position + velocity);
    }

    private void UpdateRaycastCorners()
    {
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(collisionPadding * -2);

        raycastCorners.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastCorners.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastCorners.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastCorners.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void ResetCollisionsStates()
    {
        IsCollidingAbove = false;
        IsCollidingBelow = false;
        IsCollidingLeft = false;
        IsCollidingRight = false;
    }

    private void CalculateRaySpacing()
    {
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(collisionPadding * -2);

        horizontalRaySpace = bounds.size.y / (numOfHorizontalRays - 1);
        verticalRaySpace = bounds.size.x / (numOfVerticalRays - 1);
    }

    private void ResolveHorizontalMovement(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + collisionPadding;

        for (int i = 0; i < numOfHorizontalRays; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastCorners.bottomLeft : raycastCorners.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpace * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, movementStats.groundLayer);

            if(hit)
            {
                velocity.x = (hit.distance - collisionPadding) * directionX;
                rayLength = hit.distance;

                if (directionX == -1)
                {
                    IsCollidingLeft = true;
                }
                else if (directionX == 1)
                {
                    IsCollidingRight = true;
                }
            }

            #region Debug Visualization
            if(movementStats.DebugShowWallHit)
            {
                Vector2 debugRayOrigin = (directionX == -1) ? raycastCorners.bottomLeft : raycastCorners.bottomRight;
                debugRayOrigin += Vector2.up * (horizontalRaySpace * i);
                float debugRayLength = movementStats.ExtraRayDebugDistance;

                bool didHit = Physics2D.Raycast(debugRayOrigin, Vector2.right * directionX, debugRayLength, movementStats.groundLayer);
                Color rayColor = didHit ? Color.green : Color.red;
                Debug.DrawRay(debugRayOrigin, Vector2.right * directionX * debugRayLength, rayColor);
            }
            #endregion

        }
    }

    private void ResolveVerticalMovement(ref Vector2 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + collisionPadding;

        for (int i = 0; i < numOfVerticalRays; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastCorners.bottomLeft : raycastCorners.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpace * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, movementStats.groundLayer);

            if (hit)
            {
                velocity.y = (hit.distance - collisionPadding) * directionY;
                rayLength = hit.distance;

                if (directionY == -1)
                {
                    IsCollidingBelow = true;
                }
                else if (directionY == 1)
                {
                    IsCollidingAbove = true;
                }
            }

            #region Debug Visualization
            if (movementStats.DebugShowIsGrounded)
            {
                Vector2 debugRayOrigin = raycastCorners.bottomLeft;
                debugRayOrigin += Vector2.right * (verticalRaySpace * i + velocity.x);
                float debugRayLength = movementStats.ExtraRayDebugDistance;

                bool didHit = Physics2D.Raycast(debugRayOrigin, Vector2.down, debugRayLength, movementStats.groundLayer);
                Color rayColor = didHit ? Color.green : Color.red;
                Debug.DrawRay(debugRayOrigin, Vector2.down * debugRayLength, rayColor);
            }

            if(movementStats.DebugShowHeadRays)
            {
                Vector2 debugRayOrigin = raycastCorners.topLeft;
                debugRayOrigin += Vector2.right * (verticalRaySpace * i + velocity.x);
                float debugRayLength = movementStats.ExtraRayDebugDistance;

                bool didHit = Physics2D.Raycast(debugRayOrigin, Vector2.up, debugRayLength, movementStats.groundLayer);
                Color rayColor = didHit ? Color.green : Color.red;
                Debug.DrawRay(debugRayOrigin, Vector2.up * debugRayLength, rayColor);
            }
            #endregion

        }
    }

    #region Helper Methods
    public bool IsGrounded() => IsCollidingBelow;

    public bool BumpedHead() => IsCollidingAbove;

    public bool IsTouchingWall(bool isFacingRight) => (isFacingRight && IsCollidingRight) || (!isFacingRight && IsCollidingLeft);

    public int GetWallDirection()
    {
        if (IsCollidingLeft) return -1;
        if(IsCollidingRight) return 1;
        return 0;
    }
    #endregion
}
