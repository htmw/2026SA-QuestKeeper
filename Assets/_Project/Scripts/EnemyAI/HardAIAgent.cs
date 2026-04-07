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
    }

    public override void OnEpisodeBegin()
    {
        if (isTrainingMode)
        {
            GameManager.Instance.ResetForTraining();
        }
        if (fighter != null) myPreviousHealth = fighter.currentHealth;
        if (opponentBase != null) oppPreviousHealth = opponentBase.currentHealth;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        
        
        if (fighter == null || opponentBase == null) return;

        if (opponentBase == null)
        {
            if (fighter.opponent != null)
            {
                opponentBase = fighter.opponent.GetComponent<CharacterBase>();
            }
            else
            {
                return; // Can't collect observations without opponent
            }
        }

        // Spatial Awareness
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(opponentBase.transform.localPosition.x);
        sensor.AddObservation(opponentBase.transform.localPosition.y);

        // Distance
        float distance = Vector2.Distance(transform.localPosition, opponentBase.transform.localPosition);
        sensor.AddObservation(distance);

        // Opponent State
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Attacking ? 1f : 0f);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Blocking ? 1f : 0f);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Jumping ? 1f : 0f);

        // My State
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Attacking ? 1f : 0f);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Blocking ? 1f : 0f);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Jumping ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (GameManager.Instance.currState != GameManager.MatchStates.Playing) return;

        // Read choices from the 4 branches
        int moveInput = actions.DiscreteActions[0];
        int attackInput = actions.DiscreteActions[1];
        int jumpInput = actions.DiscreteActions[2];
        int blockInput = actions.DiscreteActions[3];

        // Movement
        float moveDirection = 0f;
        if (moveInput == 1)
        {
            moveDirection = -1f;
        }
        else if (moveInput == 2)
        {
            moveDirection = 1f;
        }
        fighter.Move(moveDirection);

        // Attack
        if (attackInput == 1)
        {
            fighter.Attack();
        }

        // Jump
        if (jumpInput == 1)
        {
            fighter.Jump();
        }

        // Block
        fighter.Block(blockInput == 1);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;
        discreteActionsOut[2] = 0;
        discreteActionsOut[3] = 0;

        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.isPressed) discreteActionsOut[0] = 1; // Left
        if (Keyboard.current.dKey.isPressed) discreteActionsOut[0] = 2; // Right
        if (Keyboard.current.fKey.isPressed) discreteActionsOut[1] = 1; // Attack
        if (Keyboard.current.wKey.isPressed) discreteActionsOut[2] = 1; // Jump
        if (Keyboard.current.sKey.isPressed) discreteActionsOut[3] = 1; // Block
    }

    private void Update()
    {
        if (!isTrainingMode || fighter == null || opponentBase == null) return;
        if (GameManager.Instance.currState != GameManager.MatchStates.Playing) return;

        // Micro Penalty for wasting time
        AddReward(-0.0005f);

        // Penalty for taking damage
        if (fighter.currentHealth < myPreviousHealth)
        {
            float damageTaken = myPreviousHealth - fighter.currentHealth;
            AddReward(-0.01f * damageTaken);
            myPreviousHealth = fighter.currentHealth;
        }

        // Reward for dealing damage
        if (opponentBase.currentHealth < oppPreviousHealth)
        {
            float damageDealt = oppPreviousHealth - opponentBase.currentHealth;
            AddReward(0.01f * damageDealt);
            oppPreviousHealth = opponentBase.currentHealth;
        }
    }
}
