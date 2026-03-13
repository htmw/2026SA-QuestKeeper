using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // Singleton used to allow other scripts to easily access GameManager
    public static GameManager Instance { get; private set; }

    // All possible match states
    public enum MatchStates
    {
        Loading,
        Countdown,
        Playing,
        Paused,
        RoundOver
    }

    public enum AIDifficulty
    {
        Easy,
        Medium,
        Hard
    }
    [Header("Match State")]
    // Stores the games current state (Starts with Loading as default)
    public MatchStates currState = MatchStates.Loading;

    [Header("Fighter References")]
    // Attach Player and Opponenet objects
    public GameObject player;
    public GameObject opponent;

    [Header("Opponent Prefabs")]
    public GameObject easyOpp;
    public GameObject midOpp;
    public GameObject hardOpp;

    private GameObject spawnedOpponent;

    [Header("Player Controller References")]
    public TestPlayerCtrl playerCtrl;

    [Header("AI Difficulty")]
    public AIDifficulty selectedDifficulty = AIDifficulty.Easy;
    [Header("Round Timer")]
    public float roundTime = 30f;
    private float currTime;
    public TextMeshProUGUI timerText;

    [Header("Health")]
    public HealthSystem playerHealth;
    public HealthSystem opponentHealth;

    [Header("Spawn Locations")]
    // Places players and opponents at their assigned spawn locations at the start of each match
    public Transform PlayerSpawnPoint;
    public Transform OpponentSpawnPoint;

    [Header("Countdown Settings")]
    public int countdownStart = 3;

    [Header("UI")]
    public TextMeshProUGUI countdownTxt;

   
    private void Awake()
    {
        // Makes sure only 1 Game Manager exists
        if (Instance != null && Instance != this)
        {
            // Destroys duplicate Game Managers so there's only 1
            Destroy(gameObject);
            return;
        }
        // If no Game Manager exists then this one becomes the singleton instances
        Instance = this;
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Sets Match state to Loading (Players Spawn at assigned locations)
        SetMatchState(MatchStates.Loading);

        // Place both fighters at their assigned positions
        PlaceFighters();
        SpawnOpponent();
        
        if(spawnedOpponent != null)
        {
            opponentHealth = spawnedOpponent.GetComponent<HealthSystem>();
        }

        // Reset Health
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }

        if (opponentHealth != null)
        {
            opponentHealth.ResetHealth();
        }

        currTime = roundTime;

        if(playerCtrl != null)
        {
            playerCtrl.LockMovement();
        }

        StartCoroutine(StartCountdown());

        currTime = roundTime;

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currTime).ToString();
        }      
    }

    private void Update()
    {
        Pause();

        if(currState == MatchStates.Playing)
        {
            Timer();
        }

        CheckHealth();
    }

    void CheckHealth()
    {
        if (currState != MatchStates.Playing)
        {
            return;
        }

        // Lets Game Manager know when to end the round
        if (playerHealth != null && playerHealth.currHealth <= 0)
        {
            Debug.Log("Player Died");
            SetMatchState(MatchStates.RoundOver);
        }

        if(opponentHealth != null && opponentHealth.currHealth <= 0)
        {
            Debug.Log("Opponent Died");
            SetMatchState(MatchStates.RoundOver);
        }

    }
    // Basic Timer Function
    void Timer()
    {
        currTime -= Time.deltaTime;

        if (currTime <= 0)
        {
            currTime = 0;
            Debug.Log("Round Over!");
        }

        if(timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currTime).ToString();
        }


        Debug.Log("Timer: " + Mathf.CeilToInt(currTime));
    }

    void Pause()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if(currState == MatchStates.Playing)
            {
                SetMatchState (MatchStates.Paused);

                if(playerCtrl != null)
                {
                    playerCtrl.LockMovement ();
                }

                Debug.Log("Game Paused");
            }

            else if (currState == MatchStates.Paused)
            {
                SetMatchState(MatchStates.Playing);

                if(playerCtrl != null)
                {
                    playerCtrl.UnlockMovement ();
                }

                Debug.Log("Game Resumed");
            }
        }
    }

    public IEnumerator StartCountdown()
    {
        SetMatchState(MatchStates.Countdown);

        // Count down from 3
        for(int i = countdownStart; i > 0; i--)
        {
            if( countdownTxt  != null)
            {
                countdownTxt.text = i.ToString();
            }

            Debug.Log("Countdown: " + i);
            yield return new WaitForSeconds(1f);
        }

        if (countdownTxt != null)
        {
            countdownTxt.text = "FIGHT!";
        }

        Debug.Log("Countdown: Fight!");
        yield return new WaitForSeconds(1f);

        // Clears text and starts match
        if (countdownTxt != null)
        {
            countdownTxt.text = "";
        }

        SetMatchState(MatchStates.Playing);

        // Unlock movement once countdown ends
        if(playerCtrl != null)
        {
            playerCtrl.UnlockMovement();
        }
    }


    // Function that moves the fighters into the right postions
    private void PlaceFighters()
    {
        // Following two if statements: Only runs if both the player/opponent object and spawn point are assigned
        if (player != null && PlayerSpawnPoint != null)
        {
            player.transform.position = PlayerSpawnPoint.position;
        }

        if (opponent != null && OpponentSpawnPoint != null)
        {
            opponent.transform.position = OpponentSpawnPoint.position;
        }
    }

    // Helper function used to switch the match state
    public void SetMatchState(MatchStates newState)
    {
        currState = newState;
        Debug.Log("Match State changed to: " + currState);
    }

    void SpawnOpponent()
    {
        GameObject spawnPrefab = null;

        switch (selectedDifficulty)
        {
            case AIDifficulty.Easy:
                spawnPrefab = easyOpp;
                break;

            case AIDifficulty.Medium:
                spawnPrefab = midOpp;
                break;

            case AIDifficulty.Hard:
                spawnPrefab = hardOpp;
                break;
        }

        if (spawnPrefab != null && OpponentSpawnPoint != null)

        {
            // Spawns AI Difficulty Opponent
            spawnedOpponent = Instantiate(spawnPrefab, OpponentSpawnPoint.position, Quaternion.identity);

            opponentHealth = spawnedOpponent.GetComponent<HealthSystem>();
            if(playerCtrl != null)
            {
                playerCtrl.opponentHealth = opponentHealth;
            }
            Debug.Log("Spawned AI Difficulty: " + selectedDifficulty);
        }
        {
            
        }
    }
}
