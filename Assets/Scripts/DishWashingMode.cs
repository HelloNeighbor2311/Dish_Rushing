using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Switches between the restaurant and the washing screen: locks the player while open.
/// Call Open(interactor, station) from a washing station. Close returns to gameplay.
/// Created on demand so existing scenes need no manual wiring; the screen itself is the
/// prefab Resources/DishWashingUI (see DishWashingView).
/// </summary>
public sealed class DishWashingMode : MonoBehaviour
{
    private static DishWashingMode instance;
    public static bool IsOpen => instance != null && instance.open;

    private bool open;
    private Player movement;
    private PlayerInteractor interactor;
    private PlayerInput input;
    private bool movementWasEnabled, interactorWasEnabled, inputWasEnabled;
    private CursorLockMode previousLock;
    private bool previousCursorVisible;
    private DishWashingView view;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() { instance = null; }

    public static void Open(PlayerInteractor player, Dishwasher station)
    {
        if (player == null || station == null || IsOpen) return;
        if (instance == null)
            instance = new GameObject("DishWashingMode").AddComponent<DishWashingMode>();
        instance.Begin(player);
        if (instance.view == null)
            instance.view = Instantiate(Resources.Load<DishWashingView>("DishWashingUI"), instance.transform);
        instance.view.Show(station);
    }

    public static void CloseCurrent()
    {
        if (instance != null) instance.Close();
    }

    private void Begin(PlayerInteractor player)
    {
        interactor = player;
        movement = player.GetComponent<Player>();
        input = player.GetComponent<PlayerInput>();
        movementWasEnabled = movement != null && movement.enabled;
        interactorWasEnabled = player.enabled;
        inputWasEnabled = input != null && input.enabled;
        if (input != null) input.enabled = false;
        if (movement != null) movement.enabled = false;
        player.enabled = false;
        var body = player.GetComponent<Rigidbody2D>();
        if (body != null) body.linearVelocity = Vector2.zero;
        var animator = player.GetComponentInChildren<Animator>();
        if (animator != null) animator.SetBool("isWalking", false);
        previousLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        open = true;
        // Time.timeScale is intentionally untouched: the shift timer continues.
    }

    public void Close()
    {
        if (!open) return;
        open = false;
        if (view != null) view.Hide();
        if (movement != null) movement.enabled = movementWasEnabled;
        if (interactor != null) interactor.enabled = interactorWasEnabled;
        if (input != null) input.enabled = inputWasEnabled;
        Cursor.lockState = previousLock;
        Cursor.visible = previousCursorVisible;
    }

    private void Update()
    {
        if (open && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void OnDisable() { Close(); }
    private void OnDestroy()
    {
        Close();
        if (instance == this) instance = null;
    }
}
