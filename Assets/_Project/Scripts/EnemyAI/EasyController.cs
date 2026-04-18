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
        if (!isCurrentlyBlocking && fighter.currentState != CharacterBase.CharacterState.Attacking) {

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

        isBackingUp = false;
        isIdling = false;

        float distanceToPlayer = Mathf.Abs(fighter.opponent.position.x - transform.position.x);
        int randomChoice = Random.Range(0, 7);

        switch (randomChoice) {
            case 0: // Attack
                //part for TC03_02/03
                fighter.Move(0); 
                fighter.Attack(); 
                Debug.Log("Easy AI Level Action: Attack");
                break;

            case 1: // Attack
                //part for TC03_02/03
                fighter.Move(0);
                fighter.Attack();
                Debug.Log("Easy AI Level Action: Attack");
                break;

            case 2: // Attack
                //part for TC03_02/03
                fighter.Move(0);
                fighter.Attack();
                Debug.Log("Easy AI Level Action: Attack");
                break;

            case 3: // Block
                fighter.Move(0);
                fighter.Block(true);
                isCurrentlyBlocking = true;
                Debug.Log("Easy AI Level Action: Block");
                break;

            case 4: // Jump
                fighter.Jump(); 
                Debug.Log("Easy AI Level Action: Jump");
                break;

            case 5: // Retreat
                isBackingUp = true;
                Debug.Log("Easy AI Level Action: Backing Up");
                break;
            case 6: // Idle
                fighter.Move(0);
                Debug.Log("Easy AI Level Action: Idle/Waiting");
                break;
        }
    }
}
