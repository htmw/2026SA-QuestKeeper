using UnityEngine;

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
    protected bool canAttack = true;
    protected bool isDuckingInput;

    [Header("Movement")]
    public float pushSpeedMultiplier = 0.5f;
    public float backSpeedMultiplier = 0.7f;
    private bool isTouchingFighter = false;
    public Transform opponent;
    public bool isFacingRight = true;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackWhiff;
    public AudioClip hitImpact;
    public AudioClip jumpWhoosh;
    public AudioClip blockHit;

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
        Grab,
        DuckAttack,
        DuckKick
    }

    public enum AttackType
    {
        Punch,
        Kick
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
        if (currentState == CharacterState.Dead || currentState == CharacterState.Hit ||
            currentState == CharacterState.Blocking || currentState == CharacterState.Attacking ||
            currentState == CharacterState.Duck || currentState == CharacterState.DuckAttack ||
            currentState == CharacterState.DuckKick) return;

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

            if (isPushedBackward && isGrounded && currentState != CharacterState.Blocking &&
                currentState != CharacterState.Hit && currentState != CharacterState.Dead &&
                currentState != CharacterState.Duck)
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
            if (audioSource != null && jumpWhoosh != null)
            {
                audioSource.PlayOneShot(jumpWhoosh);
            }
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

        if (!canAttack || currentState == CharacterState.Dead || currentState == CharacterState.Blocking) return;
        canAttack = false;

        if (currentState == CharacterState.Duck)
        {
            ChangeState(CharacterState.DuckAttack);
            attackTimer = attackDuration;

            if (basicAttackHitbox != null) basicAttackHitbox.SetActive(true);
            if (audioSource != null && attackWhiff != null) audioSource.PlayOneShot(attackWhiff);

            Debug.Log(gameObject.name + " threw a duck punch!");
            return;
        }

        if (currentState != CharacterState.Attacking && currentState != CharacterState.Dead && currentState != CharacterState.Blocking)
        {
            if (isGrounded) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            ChangeState(CharacterState.Attacking);
            attackTimer = attackDuration;

            if (basicAttackHitbox != null) basicAttackHitbox.SetActive(true);
            if (audioSource != null && attackWhiff != null) audioSource.PlayOneShot(attackWhiff);
            Debug.Log(gameObject.name + " threw a punch!");
        }
    }

    public virtual void Block(bool isBlocking)
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Jumping || currentState == CharacterState.Attacking) return;

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
        if (!canAttack || currentState == CharacterState.Dead || currentState == CharacterState.Blocking) return;

        canAttack = false;

        if (currentState == CharacterState.Duck)
        {
            ChangeState(CharacterState.DuckKick);
            kickTimer = kickDuration;

            if (basicAttackHitbox != null) basicAttackHitbox.SetActive(true);
            Debug.Log(gameObject.name + " threw a duck kick!");
            return;
        }

        if (currentState == CharacterState.Dead || currentState == CharacterState.Blocking || currentState == CharacterState.Kick) return;

        if (isGrounded) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        ChangeState(CharacterState.Kick);
        kickTimer = kickDuration;
        if (basicAttackHitbox != null) basicAttackHitbox.SetActive(true);
        Debug.Log(gameObject.name + " kicked!");
    }

    public virtual void Duck(bool isDucking)
    {
        isDuckingInput = isDucking;

        if (currentState == CharacterState.Dead || currentState == CharacterState.Jumping ||
        currentState == CharacterState.Attacking || currentState == CharacterState.DuckAttack ||
        currentState == CharacterState.DuckKick || currentState == CharacterState.Kick) return;
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
        // Deals damage & knocks opponent back
        CharacterBase opponentBase = opponent.GetComponent<CharacterBase>();
        if (opponentBase != null)
        {
            // Apply knockback force
            float knockbackDir = transform.position.x < opponent.position.x ? 1f : -1f;
            // Grab attack deals 12 damage
            opponentBase.TakeGrabDamage(12, knockbackDir * 8f);
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


    public virtual void TakeDamage(int damage, AttackType attackType = AttackType.Punch)
    {
        if (basicAttackHitbox != null) basicAttackHitbox.SetActive(false);

        if (currentState == CharacterState.Dead) return;

        if (currentState == CharacterState.Duck && attackType == AttackType.Punch)
        {
            return;
        }

        if (currentState == CharacterState.Blocking)
        {
            Debug.Log(gameObject.name + " blocked the attack!");
            currentHealth -= (damage - 8);
            if (audioSource != null && blockHit != null) audioSource.PlayOneShot(blockHit);
            return;
        }

        ResetCombat();
        if (basicAttackHitbox != null) basicAttackHitbox.SetActive(false);

        currentHealth -= damage;
        ChangeState(CharacterState.Hit);
        if (audioSource != null && hitImpact != null)
        {
            audioSource.PlayOneShot(hitImpact);
        }

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
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                canAttack = true;
                if (basicAttackHitbox != null) basicAttackHitbox.SetActive(false);

                if (currentState == CharacterState.Attacking || currentState == CharacterState.DuckAttack)
                {
                    CharacterState nextState = isDuckingInput ? CharacterState.Duck : CharacterState.Idle;
                    ChangeState(nextState);
                }
            }
        }

        if (kickTimer > 0)
        {
            kickTimer -= Time.deltaTime;
            if (kickTimer <= 0)
            {
                canAttack = true;
                if (basicAttackHitbox != null) basicAttackHitbox.SetActive(false);

                if (currentState == CharacterState.Kick || currentState == CharacterState.DuckKick)
                {
                    CharacterState nextState = isDuckingInput ? CharacterState.Duck : CharacterState.Idle;
                    ChangeState(nextState);
                }
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

    public void ResetCombat()
    {
        canAttack = true;
        attackTimer = 0;
        kickTimer = 0;
        isTouchingFighter = false; 
    }
}
