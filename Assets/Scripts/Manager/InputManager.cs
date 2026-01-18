using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager inputManagerInstance;

    public static PlayerInput playerInput;

    public static Vector2 movement;
    public static bool jumpPressed;
    public static bool jumpReleased;
    public static bool runHeld;
    public static bool dashPressed;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (inputManagerInstance == null)
        {
            inputManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if(jumpPressed)
        {
            jumpPressed = false;
        }
        if(dashPressed)
        {
            dashPressed = false;
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

    public async void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            playerInput.SwitchCurrentActionMap("PauseMode");
            await OptionsMenu.optionsMenuInstance.SetPauseMenu();
        }
    }

    public async void OnResume(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            playerInput.SwitchCurrentActionMap("PlayMode");
            await OptionsMenu.optionsMenuInstance.ResumeFromPauseMenu();
        }
    }
}
