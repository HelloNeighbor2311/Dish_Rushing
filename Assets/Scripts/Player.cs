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
        moveInput =  GameInput.instance.moveInput;
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
        rb.linearVelocity = GameInput.instance.moveInput*moveSpeed;
    }
}
