using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HardAIAgent : Agent
{
    public bool isTrainingMode = false;
    
    private CharacterBase fighter;
    private Vector2 startingPosition;

    public override void Initialize()
    {
        fighter = GetComponent<CharacterBase>();
        startingPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        if (isTrainingMode)
        {
            // Reset the fighter's position and health
            transform.position = startingPosition;
            fighter.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            fighter.currentHealth = fighter.maxHealth;
            fighter.ChangeState(CharacterBase.CharacterState.Idle);
        }
    }
}
