using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string levelToLoad = "InGameScene";

    public void PlayGame()
    {
        Debug.Log("Loading level...");
        SceneManager.LoadScene(levelToLoad);
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting!");
        Application.Quit();
    }

}
