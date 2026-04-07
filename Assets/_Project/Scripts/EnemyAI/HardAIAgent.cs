using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HardAIAgent : Agent
{
    public bool isTrainingMode = false;

    private CharacterBase fighter;
    private CharacterBase opponentBase;

    public override void Initialize()
    {
        if (fighter.opponent != null)
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
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (fighter == null || opponentBase == null) return;

        // Spatial Awareness
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(opponentBase.transform.localPosition.x);
        sensor.AddObservation(opponentBase.transform.localPosition.y);

        // Distance
        float distance = Vector2.Distance(transform.localPosition, opponentBase.transform.localPosition);
        sensor.AddObservation(distance);

        // Facing Direction
        sensor.AddObservation(fighter.isFacingRight ? 1f : 0f);
        sensor.AddObservation(opponentBase.isFacingRight ? 1f : 0f);

        // Opponent State
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Attacking ? 1f : 0f);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Blocking ? 1f : 0f);
        sensor.AddObservation(opponentBase.currentState == CharacterBase.CharacterState.Jumping ? 1f : 0f);

        // My State
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Attacking ? 1f : 0f);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Blocking ? 1f : 0f);
        sensor.AddObservation(fighter.currentState == CharacterBase.CharacterState.Jumping ? 1f : 0f);
    }
}
