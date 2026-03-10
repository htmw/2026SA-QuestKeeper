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

    // The main states every fighter needs
    public enum  CharacterState
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

    }

    // -- CORE ACTIONS --

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
        if (currentState == CharacterState.Idle || currentState == CharacterState.Moving)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            ChangeState(CharacterState.Jumping);
        }
    }

    public virtual void Attack()
    {
        // Basic attack like a punch. May be added to or replaced later based on abilities
        if (currentState != CharacterState.Attacking && currentState != CharacterState.Dead)
        {
            ChangeState(CharacterState.Attacking);
            Debug.Log(gameObject.name + " performed an attack!");
            // TODO: Set up hitboxes and damage logic for attacks, plus animation triggers
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
