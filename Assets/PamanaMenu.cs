using UnityEngine;
using UnityEngine.SceneManagement;

public class PamanaMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("enter");
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}