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
    public float kickDuration = 0.3f;
    private float kickTimer;

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
        Dead,
        Kick,
        Duck,
        Grab
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

        Debug.Log(gameObject.name + " state: " + currentState); // temp
    }





    // -- MOVEMENT ACTIONS --

    public virtual void Move(float direction)
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Hit || currentState == CharacterState.Blocking || currentState == CharacterState.Attacking || currentState == CharacterState.Duck) return;

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

    public virtual void Kick()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Blocking || currentState == CharacterState.Kick) return;

        if (isGrounded)
        {
            // Stops horizontal movement when kicking
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        ChangeState(CharacterState.Kick);
        kickTimer = kickDuration;

        if (basicAttackHitbox != null) basicAttackHitbox.SetActive(true);

        // Temp Debug to test Kick mechanic until animation is placed in
        Debug.Log(gameObject.name + " kicked!");
    }

    public virtual void Duck(bool isDucking)
    {
        // Can't duck while dead, jumping, or attacking
        if (currentState == CharacterState.Dead || currentState == CharacterState.Jumping || currentState == CharacterState.Attacking) return;

        if (isDucking)
        {
            // Stop movement when ducking
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            ChangeState(CharacterState.Duck);
        }

        else if (currentState == CharacterState.Duck)
        {
            ChangeState(CharacterState.Idle);
        }

        //Temp Debug to test Duck mechanic until animation is placed in
        Debug.Log(gameObject.name + " ducked!");
    }

    public virtual void Grab()
    {
        // Can't grab while dead, blocking, jumping, or already grabbing
        if (currentState == CharacterState.Dead || currentState == CharacterState.Jumping || currentState == CharacterState.Blocking || currentState == CharacterState.Grab) return;

        // Only grab if opponent is close enough
        if (opponent == null) return;
        float grabRange = 1.5f;
        if (Vector2.Distance(transform.position, opponent.position) > grabRange) return;

        // Stop movement while grabbing
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        ChangeState(CharacterState.Grab);
        attackTimer = attackDuration;

        // Deals damage and knocks opponent back
        CharacterBase opponentBase = opponent.GetComponent<CharacterBase>();
        if (opponentBase != null)
        {
            // Apply knockback force
            float knockbackDir = transform.position.x < opponent.position.x ? 1f : -1f;
            opponentBase.TakeGrabDamage(15, knockbackDir * 8f);
        }

        Debug.Log(gameObject.name + " grabbed!");
    }

    public virtual void TakeGrabDamage(int damage, float knockbackForce)
    {
        if (currentState == CharacterState.Dead) return;

        if (currentState == CharacterState.Blocking)
        {
            Debug.Log(gameObject.name + " blocked the grab!");
            return;
        }

        currentHealth -= damage;
        ChangeState(CharacterState.Hit);

        //Apply knockback
        rb.linearVelocity = new Vector2(knockbackForce, rb.linearVelocity.y);

        if (currentHealth <= 0)
        {
            Die();
        }

        if (currentHealth > 0)
        {
            Invoke("RecoverFromHit", 0.5f);
        }

        Debug.Log(gameObject.name + " was grabbed for " + damage + " damage!");
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
            Invoke("RecoverFromHit", 0.5f); // Simulate hit stun duration
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

        if (currentState == CharacterState.Kick)
        {
            kickTimer -= Time.deltaTime;
            if (kickTimer <= 0)
            {
                if (basicAttackHitbox != null) basicAttackHitbox.SetActive(false);
                ChangeState(isGrounded ? CharacterState.Idle : CharacterState.Jumping);
            }
        }

        if (currentState == CharacterState.Grab)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                ChangeState(isGrounded ? CharacterState.Idle : CharacterState.Jumping);
            }
        }

    }

    protected virtual void RecoverFromHit()
    {
        if (currentState == CharacterState.Hit)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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
            if (anim.HasState(0, Animator.StringToHash(newState.ToString())))
            {
                anim.Play(newState.ToString());
            }
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
            if (transform.position.y > collision.transform.position.y + 0.5f)
            {
                float slideDirection = transform.position.x < collision.transform.position.x ? -1 : 1;
                rb.AddForce(new Vector2(slideDirection * 50f, 0));
            }
        }
    }
}
