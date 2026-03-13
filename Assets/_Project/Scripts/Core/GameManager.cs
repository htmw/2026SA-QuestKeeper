using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Opponent Prefabs")]
    public GameObject easyOpp;
    public GameObject midOpp;
    public GameObject hardOpp;

    private GameObject spawnedOpponent;

    [Header("Player Controller References")]
    public PlayerController playerCtrl;

    [Header("AI Difficulty")]
    public AIDifficulty selectedDifficulty = AIDifficulty.Easy;
    [Header("Round Timer")]
    public float roundTime = 30f;
    private float currTime;
    public TextMeshProUGUI timerText;

    [Header("Spawn Locations")]
    // Places players and opponents at their assigned spawn locations at the start of each match
    public Transform PlayerSpawnPoint;
    public Transform OpponentSpawnPoint;

    [Header("Countdown Settings")]
    public int countdownStart = 3;

    [Header("UI")]
    public UnityEngine.UI.Image countdownImage;
    public Sprite[] countdownSprites;
    public UnityEngine.UI.Image playerHealthBar;
    public UnityEngine.UI.Image opponentHealthBar;
    public GameObject pauseGamePanel;
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameTxt;


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

        //if(spawnedOpponent != null)
        //{
        //opponentHealth = spawnedOpponent.GetComponent<HealthSystem>();
        //}

        // Reset Health
        //if (playerHealth != null)
        //{
        //playerHealth.ResetHealth();
        //}

        //if (opponentHealth != null)
        //{
        //opponentHealth.ResetHealth();
        //}

        currTime = roundTime;

        if (playerCtrl != null)
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

        if (currState == MatchStates.Playing)
        {
            Timer();
        }

        UpdateHealthUI();
        CheckEndGame();
        //CheckHealth();
    }

    //void CheckHealth()
    //{
    //if (currState != MatchStates.Playing)
    //{
    //return;
    //}

    // Lets Game Manager know when to end the round
    //if (playerHealth != null && playerHealth.currHealth <= 0)
    //{
    //Debug.Log("Player Died");
    //SetMatchState(MatchStates.RoundOver);
    //}

    //if(opponentHealth != null && opponentHealth.currHealth <= 0)
    //{
    //Debug.Log("Opponent Died");
    //SetMatchState(MatchStates.RoundOver);
    //}

    //}

    // Updated Health function that uses the stats from the CharacterBase and updates the UI
    void UpdateHealthUI()
    {
        if (player != null && playerHealthBar != null)
        {
            CharacterBase playerBase = player.GetComponent<CharacterBase>();
            if (playerBase != null)
            {
                playerHealthBar.fillAmount = (float)playerBase.currentHealth / playerBase.maxHealth;
            }
        }

        if (spawnedOpponent != null && opponentHealthBar != null)
        {
            CharacterBase opponentBase = spawnedOpponent.GetComponent<CharacterBase>();
            if (opponentBase != null)
            {
                opponentHealthBar.fillAmount = (float)opponentBase.currentHealth / opponentBase.maxHealth;
            }
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

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currTime).ToString();
        }
    }

    void Pause()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currState == MatchStates.Playing)
            {
                SetMatchState(MatchStates.Paused);

                if (playerCtrl != null)
                {
                    playerCtrl.LockMovement();
                    pauseGamePanel.SetActive(true);
                }

                Debug.Log("Game Paused");
            }

            else if (currState == MatchStates.Paused)
            {
                SetMatchState(MatchStates.Playing);

                if (playerCtrl != null)
                {
                    playerCtrl.UnlockMovement();
                    pauseGamePanel.SetActive(false);
                }

                Debug.Log("Game Resumed");
            }
        }
    }

    public IEnumerator StartCountdown()
    {
        SetMatchState(MatchStates.Countdown);
        if (countdownImage != null) countdownImage.enabled = true;

        // Count down from 3
        for (int i = countdownStart; i > 0; i--)
        {
            if (countdownImage != null && countdownSprites.Length > 0)
            {
                int spriteIndex = countdownStart - i;
                countdownImage.sprite = countdownSprites[spriteIndex];
            }

            Debug.Log("Countdown: " + i);
            yield return new WaitForSeconds(1f);
        }

        if (countdownImage != null && countdownSprites.Length > 3)
        {
            countdownImage.sprite = countdownSprites[3];
        }

        Debug.Log("Countdown: Fight!");
        yield return new WaitForSeconds(1f);

        // Clears text and starts match
        if (countdownImage != null)
        {
            countdownImage.enabled = false;
        }

        SetMatchState(MatchStates.Playing);

        // Unlock movement once countdown ends
        if (playerCtrl != null)
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
            case AIDifficulty.Easy: spawnPrefab = easyOpp; break;
            case AIDifficulty.Medium: spawnPrefab = midOpp; break;
            case AIDifficulty.Hard: spawnPrefab = hardOpp; break;
        }

        if (spawnPrefab != null && OpponentSpawnPoint != null)

        {
            // Spawns AI Difficulty Opponent
            spawnedOpponent = Instantiate(spawnPrefab, OpponentSpawnPoint.position, Quaternion.identity);

            // Fixes AutoFacing reference issues on spawned opponent
            if (player != null)
            {
                // Make sure opponent has correct character reference
                CharacterBase playerBase = player.GetComponent<CharacterBase>();
                CharacterBase aiBase = spawnedOpponent.GetComponent<CharacterBase>();

                if (playerBase != null && aiBase != null)
                {
                    playerBase.opponent = spawnedOpponent.transform;
                    aiBase.opponent = player.transform;
                }
                Debug.Log("Spawned AI Difficulty: " + selectedDifficulty);
            }

            // Camera can reference the spawned opponents position
            Main_Battle_Camera camScript = Camera.main.GetComponent<Main_Battle_Camera>();
            if (camScript != null)
            {
                camScript.opponent = spawnedOpponent.transform;
            }
        }
    }


    // END GAME LOGIC


    public void RetryMatch()
    {
        // Reloads the exact scene we are currently in
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("Scn_MainMenu");
    }

    void CheckEndGame()
    {
        if (currState != MatchStates.Playing)
        {
            return;
        }

        CharacterBase playerBase = player != null ? player.GetComponent<CharacterBase>() : null;
        CharacterBase opponentBase = spawnedOpponent != null ? spawnedOpponent.GetComponent<CharacterBase>() : null;

        // Player Loses
        if (playerBase != null && playerBase.currentHealth <= 0)
        {
            SetMatchState(MatchStates.RoundOver);

            if (playerCtrl != null)
            {
                playerCtrl.LockMovement();
            }

            if (endGameTxt != null)
            {
                endGameTxt.text = "DEFEAT";
            }

            if (endGamePanel != null)
            {
                endGamePanel.SetActive(true);
            }
            return;
        }

        // Opponent Loses
        if (opponentBase != null && opponentBase.currentHealth <= 0)
        {
            SetMatchState(MatchStates.RoundOver);

            if (playerCtrl != null)
            {
                playerCtrl.LockMovement();
            }

            if (endGameTxt != null)
            {
                endGameTxt.text = "VICTORY!";
            }

            if (endGamePanel != null)
            {
                endGamePanel.SetActive(true);
            }
            return;
        }
    }
}
