using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomNavigation : MonoBehaviour
{
    // Loads Room 1 battle scene
    public void GoToInside()
    {
        SceneManager.LoadScene("Inside");
    }

    // Loads Room 2 battle scene
    public void GoToInside1()
    {
        SceneManager.LoadScene("Inside 1");
    }

    // Loads Room 2 battle scene
    public void GoTostart()
    {
        SceneManager.LoadScene("start");
    }

    // Generic loader so you can reuse one function for any future scene
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}