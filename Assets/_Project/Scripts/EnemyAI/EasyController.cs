using UnityEngine;

[RequireComponent(typeof(CharacterBase))]
public class EasyController : MonoBehaviour
{
    public string difficultyLevel = "Easy";


    [Header("Action Timers")]
    //variables for TC03_02
    public float actionFreeze = 0.8f; 
    private float timer;

    [Header("Movement Settings")]
    public float stoppingDistance = 1.5f;

    private CharacterBase fighter;

    private bool isCurrentlyBlocking = false;
    private bool isCurrentlyDucking = false;
    private bool isBackingUp = false;
    private bool isIdling = false;

    void Start()
    {
        fighter = GetComponent<CharacterBase>();
        Debug.Log("AI status: " + difficultyLevel + " Controller is active");
    }
    
    void Update()
    {
        //work with bugs, ai moving before play started
        if (GameManager.Instance == null || GameManager.Instance.currState != GameManager.MatchStates.Playing) {
            fighter.Move(0);
            return;
        }
       
        //random actions TC03_02
        timer += Time.deltaTime;
        if (timer >= actionFreeze) {
            PerformRandomAction();
            timer = 0;
        }

        //move to player  TC03_03
        if (!isCurrentlyBlocking && !isCurrentlyDucking &&
            fighter.currentState != CharacterBase.CharacterState.Attacking &&
            fighter.currentState != CharacterBase.CharacterState.DuckAttack &&
            fighter.currentState != CharacterBase.CharacterState.DuckKick) {

            if (fighter.opponent != null)
            {
                float distanceX = fighter.opponent.position.x - transform.position.x;
                float direction = Mathf.Sign(distanceX); // 1 if opponent is right, -1 if left

                if (isIdling)
                {
                    // If the flag is checked, do nothing (idle)
                    fighter.Move(0);
                }
                else if (isBackingUp)
                {
                    // If the flag is checked, walk AWAY from the player
                    fighter.Move(-direction);
                }
                else if (Mathf.Abs(distanceX) > stoppingDistance)
                {
                    // If we aren't backing up, and we are too far, walk TOWARDS them
                    fighter.Move(direction);
                }
                else
                {
                    // We are perfectly in range
                    fighter.Move(0);
                }
            }
        }
    }

    void PerformRandomAction() {
        if (fighter.opponent == null)
            return;

        if (isCurrentlyBlocking)
        {
            fighter.Block(false);
            isCurrentlyBlocking = false;
        }

        if (isCurrentlyDucking)
        {
            fighter.Duck(false); // Stand back up!
            isCurrentlyDucking = false;
        }

        isBackingUp = false;
        isIdling = false;

        float distanceToPlayer = Mathf.Abs(fighter.opponent.position.x - transform.position.x);

        if (distanceToPlayer <= stoppingDistance + 0.5f)
        {
            int closeChoice = Random.Range(0, 10); // Roll 0 to 9

            switch (closeChoice)
            {
                case 0:
                case 1: // Standard Punch
                    fighter.Move(0);
                    fighter.Attack();
                    Debug.Log("Easy AI: Standing Punch");
                    break;

                case 2:
                case 3: // Standard Kick 
                    fighter.Move(0);
                    fighter.Kick();
                    Debug.Log("Easy AI: Standing Kick");
                    break;

                case 4: // Defensive Duck
                    fighter.Move(0);
                    fighter.Duck(true);
                    isCurrentlyDucking = true;
                    Debug.Log("Easy AI: Ducking (Evasion)");
                    break;

                case 5: // Duck Punch
                    fighter.Move(0);
                    fighter.Duck(true); 
                    fighter.Attack();  
                    isCurrentlyDucking = true;
                    Debug.Log("Easy AI: Duck Punch");
                    break;

                case 6: // Duck Kick
                    fighter.Move(0);
                    fighter.Duck(true); 
                    fighter.Kick();    
                    isCurrentlyDucking = true;
                    Debug.Log("Easy AI: Duck Kick");
                    break;

                case 7:
                case 8: // Stand and Block
                    fighter.Move(0);
                    fighter.Block(true);
                    isCurrentlyBlocking = true;
                    Debug.Log("Easy AI: Blocking");
                    break;

                case 9: // Tactical step back
                    isBackingUp = true;
                    Debug.Log("Easy AI: Backing Up");
                    break;
            }
        }
        else
        {
            int farChoice = Random.Range(0, 10);

            switch (farChoice)
            {
                case 0: //  Jump forward
                    fighter.Jump();
                    Debug.Log("Easy AI: Jump Approach");
                    break;

                case 1: //  Stand completely still
                    isIdling = true;
                    Debug.Log("Easy AI: Pausing (Idle)");
                    break;

                case 2: //  Step backward
                    isBackingUp = true;
                    Debug.Log("Easy AI: Spacing Backward");
                    break;

                default: //  Do nothing, let Update walk directly to the player
                    Debug.Log("Easy AI: Approaching Player");
                    break;
            }
        }
    }
}
