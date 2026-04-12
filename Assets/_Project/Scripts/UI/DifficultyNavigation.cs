using UnityEngine;

public class DifficultyNavigation : MonoBehaviour
{
    // PLAY BUTTON → Go to Difficulty Menu
    public void GoToDifficultyMenu()
    {
        SceneManager.LoadScene("DifficultyMenu");
    }

    // EASY BUTTON
    public void LoadEasy()
    {
        GameManager.Instance.SetDifficulty("Easy");
        SceneManager.LoadScene("MainBattle");
    }

    // MEDIUM BUTTON
    public void LoadMedium()
    {
        GameManager.Instance.SetDifficulty("Medium");
        SceneManager.LoadScene("MainBattle");
    }

    // HARD BUTTON
    public void LoadHard()
    {
        GameManager.Instance.SetDifficulty("Hard");
        SceneManager.LoadScene("MainBattle");
    }

    // BACK BUTTON → Return to Main Menu
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
