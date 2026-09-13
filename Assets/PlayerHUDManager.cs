using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[Serializable]
public class RetrievedUserData
{
    public string playerName;
    public int score;
    public int levelReached;
}

public class PlayerHUDManager : MonoBehaviour
{
    [Header("Firebase Config")]
    [SerializeField] private string databaseUrl = "https://pamana-bb84d-default-rtdb.firebaseio.com/";

    [Header("UI Reference")]
    [SerializeField] private TMP_Text playerInfoText;

    void Start()
    {
        // 1. Instantly display whatever was cached during login
        string cachedId = PlayerPrefs.GetString("CurrentPlayerId", "");
        string cachedName = PlayerPrefs.GetString("CurrentPlayerName", "Guest");
        int cachedScore = PlayerPrefs.GetInt("CurrentPlayerScore", 0);

        if (playerInfoText != null)
        {
            playerInfoText.text = $"Player: {cachedName} | Score: {cachedScore}";
        }

        // 2. Fetch the latest live data from Firebase to guarantee accuracy
        if (!string.IsNullOrEmpty(cachedId))
        {
            StartCoroutine(FetchLivePlayerData(cachedId));
        }
    }

    private IEnumerator FetchLivePlayerData(string playerId)
    {
        string endpoint = $"{databaseUrl}players/{playerId}.json";

        using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success && request.downloadHandler.text != "null")
            {
                RetrievedUserData liveData = JsonUtility.FromJson<RetrievedUserData>(request.downloadHandler.text);

                // Update UI with fresh values from the database
                if (playerInfoText != null)
                {
                    playerInfoText.text = $"Player: {liveData.playerName} | Score: {liveData.score}";
                }

                // Update PlayerPrefs to keep cache synchronized
                PlayerPrefs.SetInt("CurrentPlayerScore", liveData.score);
                PlayerPrefs.SetInt("CurrentPlayerLevel", liveData.levelReached);
                PlayerPrefs.Save();
            }
        }
    }
}