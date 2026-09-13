using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ArtifactItem : MonoBehaviour
{
    public string artifactName = "Manunggul Jar";
    [TextArea(4, 8)]
    public string historicalDescription = "A secondary burial jar excavated from Palawan, dating back to 890–710 BCE. The boat on the lid symbolizes the journey of the soul into the afterlife.";

    private ArtifactDisplayManager displayManager;

    void Start()
    {
        displayManager = Object.FindFirstObjectByType<ArtifactDisplayManager>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (displayManager != null)
        {
            displayManager.ShowArtifactInfo(artifactName, historicalDescription);
        }
    }
}