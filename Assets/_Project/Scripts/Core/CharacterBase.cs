using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterBase : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("State Machine")]
    public CharacterState currentState = CharacterBase.CharacterState.Idle;

    [Header("Jump Settings")]
    public int maxJumps = 2;
    private int jumpsRemaining;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("Combat Setup")]
    public GameObject basicAttackHitbox;
    public float attackDuration = 0.2f;
    private float attackTimer;

    [Header("Movement")]
    public float pushSpeedMultiplier = 0.5f;
    public float backSpeedMultiplier = 0.7f;
    private bool isTouchingFighter = false;
    public Transform opponent;
    public bool isFacingRight = true;

    // The main states every fighter needs
    public enum CharacterState
    {
        Idle,
        Moving,
        MovingBackward,
        Jumping,
        Attacking,
        Blocking,
        Hit,
        Dead
    }

    [Header("Components")]
    protected Rigidbody2D rb;
    protected Animator anim;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        CheckGrounded();

        AttackLogic();

        HandleFacing();
    }





    // -- MOVEMENT ACTIONS --

    public virtual void Move(float direction)
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Hit || currentState == CharacterState.Blocking || currentState == CharacterState.Attacking) return;

        if (direction != 0)
        {
            bool isBackingUp = (isFacingRight && direction < 0) || (!isFacingRight && direction > 0);

            float currentSpeed = isBackingUp ? moveSpeed * backSpeedMultiplier : moveSpeed;

            float actualSpeed = isTouchingFighter ? currentSpeed * pushSpeedMultiplier : currentSpeed;

            // Apply velocity using calculated speed
            rb.linearVelocity = new Vector2(direction * actualSpeed, rb.linearVelocity.y);

            CharacterState targetState = isBackingUp ? CharacterState.MovingBackward : CharacterState.Moving;

            if (currentState == CharacterState.Idle || currentState == CharacterState.Moving || currentState == CharacterState.MovingBackward)
            {
                if (currentState != targetState)
                {
                    ChangeState(targetState);
                }
            }
        }

        else
        {
            rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, 0, 50f * Time.deltaTime), rb.linearVelocity.y);

            bool isPushedBackward = (isFacingRight && rb.linearVelocity.x < -0.1f) || (!isFacingRight && rb.linearVelocity.x > 0.1f);

            if (isPushedBackward && isGrounded && currentState != CharacterState.Blocking && currentState != CharacterState.Hit && currentState != CharacterState.Dead)
            {
                if (currentState != CharacterState.MovingBackward)
                {
                    ChangeState(CharacterState.MovingBackward);
                }
            }

            else if (currentState == CharacterState.Moving || currentState == CharacterState.MovingBackward)
            {
                if (Mathf.Abs(rb.linearVelocity.x) <= 0.1f)
                {
                    ChangeState(CharacterState.Idle);
                }
            }
        }
    }

    public virtual void Jump()
    {
        // Only allow jumping if we aren't currently jumping/attacking/dead
        if (jumpsRemaining > 0 && currentState != CharacterState.Dead && currentState != CharacterState.Attacking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            jumpsRemaining--;
            ChangeState(CharacterState.Jumping);
        }
    }

    protected virtual void CheckGrounded()
    {

        // Draw an invisible cirlce at the groundCheck position to see if it overlaps with any colliders on the ground layer
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            jumpsRemaining = maxJumps;

            if (currentState == CharacterState.Jumping)
            {
                // Reset state machine so we aren't infinitly jumping
                ChangeState(CharacterState.Idle);
            }
        }
    }

    protected virtual void HandleFacing()
    {
        if (currentState == CharacterState.Dead || opponent == null) return;

        if (transform.position.x < opponent.position.x && !isFacingRight)
        {
            Flip();
        }
        else if (transform.position.x > opponent.position.x && isFacingRight)
        {
            Flip();
        }
    }

    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }






    // -- COMBAT LOGIC --

    public virtual void Attack()
    {
        // Basic attack like a punch. May be added to or replaced later based on abilities
        if (currentState != CharacterState.Attacking && currentState != CharacterState.Dead)
        {
            if (isGrounded)
            {
                // If we're on the ground, we want to stop horizontal movement when attacking
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }

            ChangeState(CharacterState.Attacking);
            attackTimer = attackDuration;

            if (basicAttackHitbox != null) basicAttackHitbox.SetActive(true);

            Debug.Log(gameObject.name + " threw a punch!");
        }
    }

    public virtual void Block(bool isBlocking)
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Jumping) return;

        if (isBlocking)
        {
            // Prevent Sliding
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            ChangeState(CharacterState.Blocking);
        }
        else if (currentState == CharacterState.Blocking)
        {
            ChangeState(CharacterState.Idle);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (currentState == CharacterState.Dead) return;

        if (currentState == CharacterState.Blocking)
        {
            Debug.Log(gameObject.name + " blocked the attack!");
            // TODO: Set up possible chip damage logic
            return;
        }

        currentHealth -= damage;
        ChangeState(CharacterState.Hit);

        if (currentHealth <= 0)
        {
            Die();
        }

        if (currentHealth > 0)
        {
            Invoke("RecoverFromHit", 0.3f); // Simulate hit stun duration
        }
    }

    protected virtual void AttackLogic()
    {
        if (currentState == CharacterState.Attacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                if (basicAttackHitbox != null) basicAttackHitbox.SetActive(false);

                ChangeState(isGrounded ? CharacterState.Idle : CharacterState.Jumping);
            }
        }
    }

    protected virtual void RecoverFromHit()
    {
        if (currentState == CharacterState.Hit)
        {
            ChangeState(isGrounded ? CharacterState.Idle : CharacterState.Jumping);
        }
    }






    // -- STATE ACTIONS -- 

    protected virtual void Die()
    {
        ChangeState(CharacterState.Dead);
        Debug.Log(gameObject.name + " has been KO'd!");
        // TODO: Disable hitboxes, trigger death animations
    }

    public virtual void ChangeState(CharacterState newState)
    {
        currentState = newState;
        

        if (anim != null)
        {
            anim.Play(newState.ToString());
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Fighter"))
        {
            isTouchingFighter = true;
        }
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Fighter"))
        {
            isTouchingFighter = false;
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Fighter"))
        {
            if (transform.position.y > collision.transform.position.y + 0.2f)
            {
                float slideDirection = transform.position.x < collision.transform.position.x ? -1 : 1;
                rb.AddForce(new Vector2(slideDirection * 50f, 0));
            }
        }
    }
}
