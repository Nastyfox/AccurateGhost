using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;

    public static Vector2 movement;
    public static bool jumpPressed;
    public static bool jumpReleased;
    public static bool runHeld;
    public static bool dashPressed;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if(jumpPressed)
        {
            jumpPressed = false;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        movement = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.started)
        {
            jumpPressed = true;
            jumpReleased = false;
        }
        else if (ctx.canceled)
        {
            jumpPressed = false;
            jumpReleased = true;
        }
    }

    public void OnRun(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.started)
        {
            runHeld = true;
        }
        if (ctx.canceled)
        {
            runHeld = false;
        }
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.started)
        {
            dashPressed = true;
        }
    }
}
