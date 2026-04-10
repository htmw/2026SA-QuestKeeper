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
        Jump
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

        ExecuteState();
    }

    private void EvaluateEnvironment()
    {
        if (distanceToPlayer > attackRange)
        {
            SetAIState(AIStates.MoveFWD);
        }
        else if (distanceToPlayer < tooCloseRange)
        {
            SetAIState(AIStates.MoveBack);
        }
        else
        {
            // In attack range, remains idle for now
            SetAIState(AIStates.Idle);
        }
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
        }
    }

    private void SetAIState (AIStates newState)
    {
        if (newState != currentState)
            Debug.Log("AI State -> " + newState);

        currentState = newState;
    }
}