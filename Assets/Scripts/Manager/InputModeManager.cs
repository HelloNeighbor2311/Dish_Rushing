using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class InputModeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Action Maps")]
    [SerializeField] private string gameplayMap = "Player";
    [SerializeField] private string uiMap = "UI";
    public static InputModeManager instance;
    public bool IsUIMode { get; private set; }

    private void Awake()
    {
        instance = this;
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput is missing!");
            return;
        }

        EnterGameplayMode();
    }
    public void SwitchMode()
    {
        if (!IsUIMode)
        {
            EnterUIMode();
        }
        else
        {
            EnterGameplayMode();
        }
    }
    public void EnterGameplayMode()
    {
        if (playerInput == null) return;

        playerInput.SwitchCurrentActionMap(gameplayMap);
        IsUIMode = false;
        KeepMouseUIActionsEnabled();
    }

    public void EnterUIMode()
    {
        if (playerInput == null) return;

        playerInput.SwitchCurrentActionMap(uiMap);
        IsUIMode = true;
    }

    private static void KeepMouseUIActionsEnabled()
    {
        // PlayerInput disables the whole UI map when returning to gameplay.
        // The pause icon is still a UI Button, so keep only mouse actions enabled
        // without re-enabling the UI Pause action (which would toggle twice on Esc).
        var uiModule = EventSystem.current != null
            ? EventSystem.current.currentInputModule as InputSystemUIInputModule
            : null;
        if (uiModule == null) return;

        Enable(uiModule.point);
        Enable(uiModule.leftClick);
        Enable(uiModule.middleClick);
        Enable(uiModule.rightClick);
        Enable(uiModule.scrollWheel);
    }

    private static void Enable(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Enable();
    }
}
