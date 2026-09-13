using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BedManager : MonoBehaviour
{
    [Header("Bed Clickable")]
    public Button bedButton;

    [Header("Question Panel")]
    public GameObject sleepConfirmPanel;
    public Button yesButton;
    public Button noButton;

    [Header("Dream Transition Panel")]
    public GameObject enterDreamPanel;
    public Button enterDreamButton;
    public string dreamSceneName = "Inside"; // Change to your dream/next level scene name

    void Start()
    {
        // Initial visibility
        sleepConfirmPanel.SetActive(false);
        enterDreamPanel.SetActive(false);

        // Listeners
        bedButton.onClick.AddListener(OnBedClicked);
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        
        if (enterDreamButton != null)
        {
            enterDreamButton.onClick.AddListener(LoadDreamScene);
        }
    }

    void OnBedClicked()
    {
        sleepConfirmPanel.SetActive(true);
    }

    void OnYesClicked()
    {
        sleepConfirmPanel.SetActive(false);
        enterDreamPanel.SetActive(true);
    }

    void OnNoClicked()
    {
        sleepConfirmPanel.SetActive(false);
    }

    void LoadDreamScene()
    {
        if (!string.IsNullOrEmpty(dreamSceneName))
        {
            SceneManager.LoadScene(dreamSceneName);
        }
    }
}