using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField]private Animator animator;

    [Header("Carrying and stamina")]
    [SerializeField, Min(1f)] private float maxStamina = 100f;
    [SerializeField, Min(0f)] private float drainPerBowl = 2f;
    [SerializeField, Min(0f)] private float recoveryPerSecond = 18f;
    [SerializeField, Range(0f, 0.2f)] private float slowdownPerBowl = 0.05f;
    [SerializeField, Min(0.1f)] private float exhaustedDropInterval = 2f;
    private PlayerInteractor interactor;
    private float stamina;
    private float dropCooldown;
    public float StaminaRatio => stamina / Mathf.Max(1f, maxStamina);
    public bool CanLiftBowls => stamina > 0f;
    public bool IsStaminaLow => StaminaRatio <= 0.2f;
    
    
    private Vector2 moveInput;
    private float moveSpeed = 5f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        interactor = GetComponent<PlayerInteractor>();
        stamina = Mathf.Max(1f, maxStamina);
    }
    private void Update()
    {
        moveInput = ReadMovement();
        UpdateStamina(Time.deltaTime);
        if (moveInput == Vector2.zero)
        {
            animator.SetBool("isWalking", false);
            return;
        }
        animator.SetBool("isWalking", true);
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        animator.SetFloat("LastInputX", moveInput.x);
        animator.SetFloat("LastInputY", moveInput.y);
    }
    private void FixedUpdate()
    {
        int bowls = interactor != null ? interactor.CarriedBowls : 0;
        float speedFactor = Mathf.Clamp(1f - slowdownPerBowl * bowls, 0.4f, 1f);
        rb.linearVelocity = ReadMovement() * (moveSpeed * speedFactor);
    }

    private Vector2 ReadMovement()
    {
        if (Time.timeScale == 0f || DishWashingMode.IsOpen ||
            (InputModeManager.instance != null && InputModeManager.instance.IsUIMode) ||
            GameInput.instance == null) return Vector2.zero;
        return Vector2.ClampMagnitude(GameInput.instance.moveInput, 1f);
    }

    private void UpdateStamina(float seconds)
    {
        if (seconds <= 0f) return;
        dropCooldown = Mathf.Max(0f, dropCooldown - seconds);
        int bowls = interactor != null ? interactor.CarriedBowls : 0;
        if (bowls == 0 || moveInput.sqrMagnitude < 0.01f)
        {
            stamina = Mathf.Min(maxStamina, stamina + recoveryPerSecond * seconds);
            return;
        }
        stamina = Mathf.Max(0f, stamina - drainPerBowl * bowls * seconds);
        if (stamina <= 0f && dropCooldown <= 0f && interactor.BreakOneBowl())
        {
            dropCooldown = exhaustedDropInterval;
            if (GameManager.Instance != null) GameManager.Instance.BowlBroken();
        }
    }

    public void RestoreStamina(float amount)
    {
        if (amount > 0f) stamina = Mathf.Min(maxStamina, stamina + amount);
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool("isWalking", false);
    }
}
