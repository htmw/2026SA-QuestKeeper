using UnityEngine;
using UnityEngine.Rendering;

public class MediumAIController : MonoBehaviour
{
   // All possible AI behaviors
   public enum AIStates
    {
        Idle,
        MoveFWD,
        MoveBack,
        Attack,
        Block,
        Jump,
        Kick,
        Grab
    }

    [Header("AI State")]
    // Current State 
    public AIStates currentState = AIStates.Idle;

    [Header("References")]
    // Player Reference
    public CharacterBase player;

    // AI Reference
    private CharacterBase self;

    [Header("Range Settings")]

    // AI chases player if farther than this
    public float chaseRange = 4f;

    // AI's preferred fighting distance
    public float attackRange = 1.5f;

    // AI backs up if too close
    public float tooCloseRange = 2f;

    [Header("Decision Timing")]
    public float decisionInterval = 0.6f;
    private float decisionTimer = 0f;

    // Calculates distance to player
    private float distanceToPlayer;

    [Header("Combat")]
    // How fast the AI reacts to the player attack before blocking
    public float minReactionTime = 0.2f;
    public float maxReactionTime = 0.5f;

    private bool isReactingToAttack = false;

    public float attackCooldown = 1.5f;
    private float attackCooldownTimer = 0f;
    private bool canAttack = true;

    // Health threshold for defensive mode
    public float lowHealthThreshold = 0.3f;

    // Jump cooldown so AI doesn't spam jump
    public float jumpCooldown = 1.5f;
    private float jumpCooldownTimer = 0f;
    private bool canJump = true;

    private void Start()
    {
        // Grab AI
        self = GetComponent<CharacterBase>();

        // Auto-find player prefab via GameManager 
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player.GetComponent<CharacterBase>();
        }

        if (player == null)
        {
            Debug.LogWarning("mediumController: Couldn't not find player!");
        }

        // Auto assign ground layer under CharacterBase section (preventing the stuck in jump state bug for the AI)\
        self.groundLayer = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        // Don't do anything until the match is actually playing
        if (GameManager.Instance == null || GameManager.Instance.currState != GameManager.MatchStates.Playing) return;

        if (player == null || self.currentState == CharacterBase.CharacterState.Dead) return;

        // Recalculates distance every frame
        distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Only makes interval decisions
        decisionTimer += Time.deltaTime;
        if (decisionTimer >= decisionInterval)
        {
            decisionTimer = 0f;
            EvaluateEnvironment();
        }

        if (!canAttack)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0f)
            {
                canAttack = true;
            }
        }

        if (!canJump)
        {
            jumpCooldownTimer -= Time.deltaTime;
            if (jumpCooldownTimer <= 0f)
            {
                canJump = true;
            }
        }

        ExecuteState();
    }

    private void EvaluateEnvironment()
    {
        // Low Health defensive mode [33% block, jump, or attack]
        float healthPercent = (float)self.currentHealth / self.maxHealth;
        if (healthPercent <= lowHealthThreshold)
        {
            float roll = Random.Range(0f, 1f);
            if (roll < 0.33f)
            {
                SetAIState(AIStates.Block);
                self.Block(true);
            }
            else if (roll < 0.66f)
            {
                TriggerJump();
            }
            else
            {
                TriggerAttack();
            }
            return;
        }

        // Player is attacking and AI is in range -> Block
        if (player.currentState == CharacterBase.CharacterState.Attacking && distanceToPlayer <= attackRange && !isReactingToAttack)
        {
            isReactingToAttack = true;
            float reactionTime = Random.Range(minReactionTime, maxReactionTime);
            Invoke("TriggerBlock", reactionTime);
            return;
        }

        // Player stopped attacking -> stop blocking
        if (player.currentState != CharacterBase.CharacterState.Attacking && self.currentState == CharacterBase.CharacterState.Blocking)
        {
            self.Block(false);
            isReactingToAttack = false;
            return;
        }

        // Movement Logic [Left/Right]
        if (distanceToPlayer < tooCloseRange)
        {
            SetAIState(AIStates.MoveBack);
        }
        else if (distanceToPlayer > attackRange)
        {
            SetAIState(AIStates.MoveFWD);
        }
        else
        {
            // In attack range - Decide what to do
            if (player.currentState == CharacterBase.CharacterState.Blocking)
            {
                // Player is blocking - 33% each
                float roll = Random.Range(0f, 1f);
                if (roll < 0.33f)
                {
                    SetAIState(AIStates.Idle); // Wait
                }
                else if (roll < 0.66f)
                {
                    SetAIState(AIStates.Jump); // Jump
                }
                else
                {
                    SetAIState(AIStates.MoveBack); // Move away
                }
            }
            else
            {
                // Player isn't blocking -> 33% punch, kick, or grab
                float roll = Random.Range(0f, 1f);
                if (roll < 0.33f)
                    TriggerAttack();
                else if (roll < 0.66f)
                    TriggerKick();
                else
                    TriggerGrab();
            }
        }
    }

    private void TriggerJump()
    {
        if (!canJump || !self.isGrounded) return;

        SetAIState(AIStates.Jump);
        self.Jump();

        canJump = false;
        jumpCooldownTimer = jumpCooldown;
    }

    private void TriggerAttack()
    {
        if (!canAttack) return;
        SetAIState(AIStates.Attack);
        self.Attack();

        // Start cooldown
        canAttack = false;
        attackCooldownTimer = attackCooldown;
    }

    private void TriggerBlock()
    {
        // Only block if plyaer is still attacking 
        if (player.currentState == CharacterBase.CharacterState.Attacking)
        {
            SetAIState(AIStates.Block);
            self.Block(true);
        }
        isReactingToAttack = false;
    }

    private void TriggerKick()
    {
        if (!canAttack) return;

        SetAIState(AIStates.Kick);
        self.Kick();
        canAttack = false;
        attackCooldownTimer = attackCooldown;
    }

    private void TriggerGrab()
    {
        if (!canAttack) return;

        // Use attackRange
        if (distanceToPlayer > attackRange) return;

        SetAIState(AIStates.Grab);
        self.Grab();

        canAttack = false;
        attackCooldownTimer = attackCooldown;
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case AIStates.MoveFWD:
                float FWD = (player.transform.position.x > transform.position.x) ? 1f : -1f;
                self.Move(FWD);
                break;

            case AIStates.MoveBack:
                float AwayDir = (player.transform.position.x > transform.position.x) ? -1f : 1f;
                self.Move(AwayDir);
                break;

            case AIStates.Idle:
                self.Move(0f);
                break;

            case AIStates.Block:
                self.Move(0f);
                break;

            case AIStates.Attack:
                self.Move(0f);
                break;
            case AIStates.Jump:
                self.Move(0f);
                break;
            case AIStates.Kick:
                self.Move(0f);
                break;
            case AIStates.Grab:
                self.Move(0f);
                break;
        }
    }

    private void SetAIState (AIStates newState)
    {
        if (newState != currentState)
            Debug.Log("AI State -> " + newState);

        currentState = newState;
    }
}