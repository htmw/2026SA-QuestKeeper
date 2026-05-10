using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HardAIAgent : Agent
{
    public bool isTrainingMode = false;

    private CharacterBase fighter;
    private CharacterBase opponentBase;

    private int myPreviousHealth;
    private int oppPreviousHealth;

    public override void Initialize()
    {
        fighter = GetComponent<CharacterBase>();

        if (fighter != null && fighter.opponent != null)
        {
            opponentBase = fighter.opponent.GetComponent<CharacterBase>();
        }
    }

    public override void OnEpisodeBegin()
    {
        if (isTrainingMode)
        {
            GameManager.Instance.ResetForTraining();
        }
        if (fighter != null && fighter.opponent != null)
        {
            opponentBase = fighter.opponent.GetComponent<CharacterBase>();
        }

        if (fighter != null) myPreviousHealth = fighter.currentHealth;
        if (opponentBase != null) oppPreviousHealth = opponentBase.currentHealth;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (fighter == null || opponentBase == null) return;

        // 1. NORMALIZE SPATIAL DATA
        // Neural networks hate raw numbers like "10". Normalize to -1 to 1 range.
        float maxStageWidth = 10f;
        Vector2 relativePos = opponentBase.transform.localPosition - transform.localPosition;
        sensor.AddObservation(relativePos.x / maxStageWidth);
        sensor.AddObservation(relativePos.y / 5f); // Assuming max jump height is ~5

        // 2. SPLIT OBSERVATIONS (Total Count: 16)
        // Make sure Space Size in Inspector is set to 16!

        // Opponent (7)
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Attacking);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Blocking);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Jumping);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Kick);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Duck);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.DuckAttack);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.DuckKick);

        // Me (7)
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Attacking);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Blocking);
        //sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Jumping);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Kick);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Duck);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.DuckAttack);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.DuckKick);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (GameManager.Instance.currState != GameManager.MatchStates.Playing) return;

        int moveInput = actions.DiscreteActions[0];
        int attackInput = actions.DiscreteActions[1];
        //int jumpInput = actions.DiscreteActions[2];
        int postureInput = actions.DiscreteActions[2];

        // 1. Movement
        float moveDir = (moveInput == 1) ? -1f : (moveInput == 2) ? 1f : 0f;
        fighter.Move(moveDir);


        // 2. Posture Logic 
        if (postureInput == 0) { fighter.Block(false); fighter.Duck(false); }
        else if (postureInput == 1) { fighter.Block(true); fighter.Duck(false); }
        else if (postureInput == 2) { fighter.Block(false); fighter.Duck(true); }

        // 3. Attacks
        if (attackInput == 1) fighter.Attack();
        else if (attackInput == 2) fighter.Kick();

        // 4. Jumps
        //if (jumpInput == 1) fighter.Jump();

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;
        discreteActionsOut[2] = 0;
        //discreteActionsOut[3] = 0;

        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.isPressed) discreteActionsOut[0] = 1; // Move Left
        if (Keyboard.current.dKey.isPressed) discreteActionsOut[0] = 2; // Move Right

        if (Keyboard.current.fKey.isPressed) discreteActionsOut[1] = 1; // Punch
        if (Keyboard.current.gKey.isPressed) discreteActionsOut[1] = 2; // Kick

        //if (Keyboard.current.wKey.isPressed) discreteActionsOut[2] = 1; // Jump

        if (Keyboard.current.sKey.isPressed) discreteActionsOut[2] = 1; // Block
        if (Keyboard.current.cKey.isPressed) discreteActionsOut[2] = 2; // Duck
    }

    private void FixedUpdate()
    {
        if (!isTrainingMode || fighter == null || opponentBase == null) return;
        if (GameManager.Instance.currState != GameManager.MatchStates.Playing) return;

        // 1. STABLE TUG-OF-WAR (Scale: 0.1 per point of damage)
        if (fighter.currentHealth < myPreviousHealth)
        {
            float damage = myPreviousHealth - fighter.currentHealth;
            AddReward(-0.3f * damage); // Lose 1.0 points if you take 10 dmg
            myPreviousHealth = fighter.currentHealth;
        }

        if (opponentBase.currentHealth < oppPreviousHealth)
        {
            float damage = oppPreviousHealth - opponentBase.currentHealth;
            AddReward(1.0f * damage); // Slightly higher reward for dealing dmg than taking it
            oppPreviousHealth = opponentBase.currentHealth;
        }

        // 2. THE POSTURE BONUS (Encourages Block/Duck)
        // If the opponent is attacking, reward the AI for being in a defensive state.
        if (opponentBase.currentState == CharacterBase.CharacterState.Attacking ||
            opponentBase.currentState == CharacterBase.CharacterState.Kick)
        {
            if (fighter.currentState == CharacterBase.CharacterState.Blocking ||
                fighter.currentState == CharacterBase.CharacterState.Duck)
            {
                AddReward(0.01f); // "Good job defending" breadcrumb
            }
        }

        // 3. THE "GRAVITY TAX" (Fixing the -11,000 leak)
        if (!fighter.isGrounded)
        {
            // Change from -10.0 to -0.02. 
            // This is still annoying to the AI, but it won't break the math.
            AddReward(-0.06f);
        }
        else
        {
            // Tiny reward for just staying on the floor near the enemy
            float dist = Vector2.Distance(transform.localPosition, opponentBase.transform.localPosition);
            if (dist < 2.0f) AddReward(0.05f);
        }
    }
}
