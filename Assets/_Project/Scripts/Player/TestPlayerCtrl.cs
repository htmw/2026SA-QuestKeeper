using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class TestPlayerCtrl : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;


    [Header("Ground Check")]
    public bool isGrounded = true;

    private Rigidbody2D rb;
    private bool moveLocked = true; // Player movement locked
    private float moveInput = 0f;


    [Header("Combat System")]
    public int punchDamage = 10;
    public float punchRange = 2f;
    public HealthSystem opponentHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Stops all inputs while movement is locked
        if (moveLocked)
        {
            moveInput = 0f;
            return;
        }
        CombatSystem();
        PlayerControls();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // Basic Movement Controls (A/D or Left/Right Arrow Keys to move left or right)
    private void PlayerControls()
    {
        moveInput = 0f;

        bool leftPressed = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;

        bool rightPressed = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;

        if (leftPressed && !rightPressed)
        {
            moveInput = -1f;
        }

        else if (rightPressed && !leftPressed)
        {
            moveInput = 1f;
        }

        // Jump Controls
        if(Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            Debug.Log("Jump");
        }
    }

      private void CombatSystem()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Punch Thrown");

            if (opponentHealth == null)
            {
               
                Debug.Log("No opponent health assigned");
                return;
            }
            float distanceToOpp = Vector2.Distance(transform.position, opponentHealth.transform.position);

            if (distanceToOpp <= punchRange)
            {
                opponentHealth.TakeDamage(punchDamage);
                Debug.Log("Punch Connected");
            }

            else
            {
                Debug.Log("Punch missed");
            }
        }
    }

    // Disables Player Movement (Player & Opponent)
    public void LockMovement()
    {
        moveLocked = true;
        moveInput = 0f;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("Movement Locked");
    }

    // Allows Players to move (Player & Opponent)
    public void UnlockMovement()
    {
        moveLocked = false;
        Debug.Log("Movement Unlocked! Use A/D or the Left/Right Arrow Keys to Move and Space to Jump");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
