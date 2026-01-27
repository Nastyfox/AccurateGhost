using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
        if (inputManagerInstance == null)
        {
            inputManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);

            playerInput = GetComponent<PlayerInput>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private async UniTaskVoid FixedUpdate()
    {
        if(jumpPressed)
        {
            await UniTask.Yield();
            jumpPressed = false;
        }
        if(dashPressed)
        {
            await UniTask.Yield();
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
            if(!OptionsMenu.isGamePaused)
            {
                playerInput.SwitchCurrentActionMap("MenuMode");
                await OptionsMenu.optionsMenuInstance.PauseFromGame();
            }
            else
            {
                playerInput.SwitchCurrentActionMap("PlayMode");
                await OptionsMenu.optionsMenuInstance.ResumeFromPause();
            }
        }
    }
}
