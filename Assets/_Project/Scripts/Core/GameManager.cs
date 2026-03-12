using System.Collections;
using TMPro;
using UnityEngine;

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

    [Header("Match State")]
    // Stores the games current state (Starts with Loading as default)
    public MatchStates currState = MatchStates.Loading;

    [Header("Fighter References")]
    // Attach Player and Opponenet objects
    public GameObject player;
    public GameObject opponent;

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
        // Scene is starting, set Match State to Loading
        currState = MatchStates.Loading;

        // Place both fighters at their assigned positions
        PlaceFighters();

        // After spawning / loading, switch to Countdown state
        currState = MatchStates.Countdown;

        StartCoroutine(StartCountdown());
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
    }
}
