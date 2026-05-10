using UnityEngine;

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
        Duck,
        DuckAttack,
        DuckKick
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
    public float tooCloseRange = 0.8f;

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
    private bool isCurrentlyDucking = false;

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
        if (isCurrentlyDucking)
        {
            self.Duck(false);
            isCurrentlyDucking = false;
        }

        // Low Health defensive mode [50/50 block or jump]
        float healthPercent = (float)self.currentHealth / self.maxHealth;
        if (healthPercent <= lowHealthThreshold)
        {
            float roll = Random.Range(0f, 1f);
            if (roll < 0.4f)
            {
                SetAIState(AIStates.Block);
                self.Block(true);
            }
            else if (roll < 0.7f)
            {
                SetAIState(AIStates.Duck);
                self.Duck(true);
                isCurrentlyDucking = true;
            }
            else
            {
                TriggerJump();
            }

            return;
        }
        // Player is attacking and AI is in range -> Block
        if (player.currentState == CharacterBase.CharacterState.Attacking && distanceToPlayer <= attackRange && !isReactingToAttack)
        {
            isReactingToAttack = true;
            float reactionTime = Random.Range(minReactionTime, maxReactionTime);
            if (Random.value < 0.5f)
            {
                Invoke("TriggerBlock", reactionTime);
            }
            else
            {
                Invoke("TriggerEvasiveDuck", reactionTime);
            }
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
                if (roll < 0.25f) SetAIState(AIStates.Idle); 
                else if (roll < 0.5f) TriggerKick();        
                else SetAIState(AIStates.MoveBack);
            }

            else if (player.currentState == CharacterBase.CharacterState.Duck ||
                     player.currentState == CharacterBase.CharacterState.DuckAttack ||
                     player.currentState == CharacterBase.CharacterState.DuckKick)
            {
                float roll = Random.Range(0f, 1f);
                if (roll < 0.3f) TriggerKick();            
                else if (roll < 0.5f) TriggerDuckAttack(); 
                else if (roll < 0.7f) TriggerDuckKick();   
                else SetAIState(AIStates.MoveBack);              
            }

            else
            {
                float roll = Random.Range(0f, 1f);
                if (roll < 0.4f) TriggerAttack();          
                else if (roll < 0.6f) TriggerKick();        
                else if (roll < 0.75f) TriggerDuckAttack(); 
                else SetAIState(AIStates.MoveBack);
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
        self.Duck(false); 
        self.Attack();

        // Start cooldown
        canAttack = false;
        attackCooldownTimer = attackCooldown;
    }

    private void TriggerKick()
    {
        if (!canAttack) return;
        SetAIState(AIStates.Kick);
        self.Duck(false);
        self.Kick();
        canAttack = false;
        attackCooldownTimer = attackCooldown;
    }

    private void TriggerDuckAttack()
    {
        if (!canAttack) return;
        SetAIState(AIStates.DuckAttack);
        self.Duck(true); 
        self.Attack();   
        isCurrentlyDucking = true;
        canAttack = false;
        attackCooldownTimer = attackCooldown;
    }

    private void TriggerDuckKick()
    {
        if (!canAttack) return;
        SetAIState(AIStates.DuckKick);
        self.Duck(true); 
        self.Kick();     
        isCurrentlyDucking = true;
        canAttack = false;
        attackCooldownTimer = attackCooldown;
    }
    private void TriggerBlock()
    {
        if (player.currentState == CharacterBase.CharacterState.Attacking)
        {
            SetAIState(AIStates.Block);
            self.Block(true);
        }
        isReactingToAttack = false;
    }

    private void TriggerEvasiveDuck()
    {
        if (player.currentState == CharacterBase.CharacterState.Attacking)
        {
            SetAIState(AIStates.Duck);
            self.Duck(true);
            isCurrentlyDucking = true;
        }
        isReactingToAttack = false;
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case AIStates.MoveFWD:
                if (distanceToPlayer <= attackRange && distanceToPlayer >= tooCloseRange)
                {
                    self.Move(0f); 
                }
                else
                {
                    float FWD = (player.transform.position.x > transform.position.x) ? 1f : -1f;
                    self.Move(FWD);
                }
                break;

            case AIStates.MoveBack:
                if (distanceToPlayer <= attackRange && distanceToPlayer >= tooCloseRange)
                {
                    self.Move(0f); 
                }
                else
                {
                    float AwayDir = (player.transform.position.x > transform.position.x) ? -1f : 1f;
                    self.Move(AwayDir);
                }
                break;

            case AIStates.Idle:
            case AIStates.Block:
            case AIStates.Attack:
            case AIStates.Jump:
            case AIStates.Kick:
            case AIStates.Duck:       
            case AIStates.DuckAttack: 
            case AIStates.DuckKick:
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