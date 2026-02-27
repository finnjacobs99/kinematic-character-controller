using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, PlayerInputActions.IGameplayActions
{
    public PlayerInputActions PlayerInputActions { get; private set; }
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Jump { get; private set; }
    public bool JumpWasPressed { get; private set; }
    public bool Crouch { get; private set; }

    private void OnEnable()
    {
        PlayerInputActions = new PlayerInputActions();
        PlayerInputActions.Enable();

        PlayerInputActions.Gameplay.Enable();
        PlayerInputActions.Gameplay.SetCallbacks(this);
    }

    private void OnDisable()
    {
        PlayerInputActions.Gameplay.Disable();
        PlayerInputActions.Gameplay.RemoveCallbacks(this);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump = context.ReadValueAsButton();

        if (context.started)
        {
            JumpWasPressed = true;
        }
    }

    public bool ConsumeJump()
    {
        if (JumpWasPressed)
        {
            JumpWasPressed = false;
            return true;
        }

        return false;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        Crouch = context.ReadValueAsButton();
    }
}
