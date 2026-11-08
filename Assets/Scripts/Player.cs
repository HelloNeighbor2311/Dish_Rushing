using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField]private Animator animator;
    
    
    private Vector2 moveInput;
    private float moveSpeed = 5f;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        rb.linearVelocity = moveInput*moveSpeed;
        
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
