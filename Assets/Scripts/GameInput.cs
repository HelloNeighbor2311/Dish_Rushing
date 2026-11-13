using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameInput : MonoBehaviour
{
    public static GameInput instance;
    public event Action InteractPressed;
    public Vector2 moveInput { get; private set; }

    [SerializeField] private PlayerInput playerInput;

    private void Awake()
    {
        instance = this;
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
    }
   

    private void OnEnable()
    {
        if (playerInput != null)
            playerInput.onActionTriggered += HandlePlayerInput;
    }
     private void OnDisable()
    {
        if (playerInput != null)
            playerInput.onActionTriggered -= HandlePlayerInput;
    }
    private void HandlePlayerInput(InputAction.CallbackContext context)
    {
        if(context.action.actionMap.name != "Player") return;
        switch (context.action.name)
        {
            case "Move":
                if (context.canceled)
                {
                    moveInput = Vector2.zero;
                    return;
                }
                moveInput = context.ReadValue<Vector2>();
                break;
            case "Interact":
                if (context.performed)
                {
                    InteractPressed?.Invoke();
                }
                break;
        }
    }
    // public void OnMove(InputAction.CallbackContext context)
    // {
    //     if (context.canceled)
    //     {
    //         MoveInput = Vector2.zero;
    //         return;
    //     }
    //     MoveInput = context.ReadValue<Vector2>();
    // }

    // public void OnInteract(InputAction.CallbackContext context)
    // {
    //     if (context.performed)
    //     {
            
    //     }
    // }
}
