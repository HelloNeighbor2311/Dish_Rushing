using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header("Stamina")]
    [SerializeField, Min(1f)] private float maxStamina = 100f;
    [SerializeField, Min(0f)] private float drainPerDishPerSecond = 8f;
    [SerializeField, Min(0f)] private float restRecoveryPerSecond = 20f;
    [SerializeField, Range(0.1f, 1f)] private float speedMultiplierPerDish = 0.85f;
    [SerializeField] private Image staminaFill;
    [SerializeField] private TMP_Text brokenDishText;

    private PlayerInteractor interactor;
    private float stamina;
    private int brokenDishes;
    public bool HasStamina => stamina > 0f;
    
    
    private Vector2 moveInput;
    private float moveSpeed = 5f;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        interactor = GetComponent<PlayerInteractor>();
        stamina = maxStamina;
        UpdateStaminaUI();
    }

    private void Update()
    {
        int carried = interactor != null ? interactor.CarriedDishes : 0;
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving && carried > 0)
        {
            float previous = stamina;
            stamina = Mathf.Max(0f, stamina - drainPerDishPerSecond * carried * Time.deltaTime);
            if (previous > 0f && stamina <= 0f)
            {
                interactor.RemoveDish();
                carried--;
                brokenDishes++;
                if (brokenDishText != null)
                    brokenDishText.text = $"Bát Vỡ: {brokenDishes}";
            }
        }
        else if (!isMoving)
        {
            stamina = Mathf.Min(maxStamina, stamina + restRecoveryPerSecond * Time.deltaTime);
        }

        rb.linearVelocity = Vector2.ClampMagnitude(moveInput, 1f) *
            (moveSpeed * Mathf.Pow(speedMultiplierPerDish, carried));
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaFill == null) return;

        float fraction = stamina / maxStamina;
        staminaFill.rectTransform.anchorMax = new Vector2(fraction, 1f);
        staminaFill.color = fraction <= 0.2f ? new Color32(240, 90, 60, 255)
            : fraction <= 0.4f ? new Color32(255, 170, 65, 255)
            : new Color32(50, 205, 50, 255);
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true);
        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }



}
