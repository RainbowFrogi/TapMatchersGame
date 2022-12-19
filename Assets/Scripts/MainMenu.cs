using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        Debug.Log("Quit Game.");
        #else
        Application.Quit();
        #endif
    }
}
