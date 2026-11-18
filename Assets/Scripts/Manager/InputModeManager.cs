using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private void Start()
    {
        GameInput.instance.PausePressed += OnPausePressed;
    }

    private void OnPausePressed()
    {
        SwitchMode();
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
    }

    public void EnterUIMode()
    {
        if (playerInput == null) return;

        playerInput.SwitchCurrentActionMap(uiMap);
        IsUIMode = true;
    }
}