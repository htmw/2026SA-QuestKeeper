using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyNavigation : MonoBehaviour
{

    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject difficultyPanel;

    public void Start()
    {
        ShowMainMenu();
    }

    // PLAY BUTTON → Go to Difficulty Menu
    public void ShowDifficultyMenu()
    {
        mainPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    // BACK BUTTON → Return to Main Menu
    public void ShowMainMenu()
    {
        mainPanel.SetActive(true);
        difficultyPanel.SetActive(false);
    }

    // EASY BUTTON
    public void LoadEasy()
    {
        PlayerPrefs.SetInt("MatchDifficulty", 0);
        SceneManager.LoadScene("Scn_BattleMain");
    }

    // MEDIUM BUTTON
    public void LoadMedium()
    {
        PlayerPrefs.SetInt("MatchDifficulty", 1);
        SceneManager.LoadScene("Scn_BattleMain");
    }

    // HARD BUTTON
    public void LoadHard()
    {
        PlayerPrefs.SetInt("MatchDifficulty", 2);
        SceneManager.LoadScene("Scn_BattleMain");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
