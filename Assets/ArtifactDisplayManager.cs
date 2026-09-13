using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArtifactDisplayManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button closeButton;

    [Header("Next Scene Navigation")]
    public Button goToBedroomButton;
    public string targetSceneName = "Bed";

    void Start()
    {
        infoPanel.SetActive(false);
        
        // Hide the Go To Bedroom button initially
        if (goToBedroomButton != null)
        {
            goToBedroomButton.gameObject.SetActive(false);
            goToBedroomButton.onClick.AddListener(LoadBedScene);
        }

        closeButton.onClick.AddListener(CloseInfo);
    }

    public void ShowArtifactInfo(string artifactName, string description)
    {
        titleText.text = artifactName;
        descriptionText.text = description;
        infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        infoPanel.SetActive(false);

        // Pop up the Go to Bedroom button after reading the info
        if (goToBedroomButton != null)
        {
            goToBedroomButton.gameObject.SetActive(true);
        }
    }

    void LoadBedScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}