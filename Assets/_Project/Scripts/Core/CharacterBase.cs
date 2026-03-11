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

    // The main states every fighter needs
    public enum CharacterState
    {
        Idle,
        Moving,
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
    }





    // -- MOVEMENT ACTIONS --

    public virtual void Move(float direction)
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Hit) return;

        // Apply velocity either left or right
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (direction != 0 && currentState == CharacterState.Idle)
        {
            ChangeState(CharacterState.Moving);
        }
        else if (direction == 0 && currentState == CharacterState.Moving)
        {
            ChangeState(CharacterState.Idle);
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






    // -- COMBAT LOGIC --

    public virtual void Attack()
    {
        // Basic attack like a punch. May be added to or replaced later based on abilities
        if (currentState != CharacterState.Attacking && currentState != CharacterState.Dead)
        {
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
        // TODO: Trigger animations based on state changes
        // anim.Play(newState.ToString());
    }
}
